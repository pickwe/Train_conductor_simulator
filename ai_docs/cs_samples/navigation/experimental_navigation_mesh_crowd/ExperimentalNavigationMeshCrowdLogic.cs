// Interactive crowd demo on an experimental navigation mesh. A configurable
// number of agents is spawned at runtime and driven by ExperimentalSeekerCrowd
// through the ExperimentalNavigationAvoidance solver. Scenarios: two teams
// crossing through the bottleneck gates, agents swapping places across a
// circle, and random wandering. The sample exposes the solver settings, a VIP
// agent priority, the team interaction masks, the agent density heatmap, and
// a teleport button demonstrating the separation of overlapping agents.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec3 = Unigine.dvec3;
#else
using Scalar = System.Single;
using Vec3 = Unigine.vec3;
#endif
#endregion

using System.Collections.Generic;
using Unigine;

public partial class ExperimentalNavigationMeshCrowdLogic : Component
{
	public Node navigation = null;
	[ParameterFile(Filter = ".node")]
	public AssetLink agentRed = null;
	[ParameterFile(Filter = ".node")]
	public AssetLink agentBlue = null;
	public Node spawnA = null;
	public Node spawnB = null;
	public int numAgents = 100;

	private enum SCENARIO
	{
		BOTTLENECK = 0,
		CIRCLE,
		WANDER,
	}

	private const int TEAM_RED = 0;
	private const int TEAM_BLUE = 1;

	private class Agent
	{
		public ExperimentalSeeker seeker;
		public Node node;
		public Node target;
		public int team = TEAM_RED;
		// own random avoidance priority, rolled at spawn
		public float avoidancePriority = 0.0f;
		// the two ends the agent walks between in the bottleneck and circle
		// scenarios
		public Vec3 pointA = Vec3.ZERO;
		public Vec3 pointB = Vec3.ZERO;
		public bool toB = true;
	}

	// distance to the target at which the goal counts as reached
	private const float REACH_DISTANCE = 1.0f;

	// spacing of the spawn grid cells
	private const float SPAWN_SPACING = 1.2f;

	// mask bits of the teams: the avoidance interaction masks when the teams
	// ignore each other, and the obstacle masks of the team body obstacles
	// preconfigured on the obstacle nodes of the agent assets in the editor
	private static readonly int[] TEAM_MASKS = { 1, 2 };

	// avoidance priority of the VIP agent, well above the random 0..5 priorities
	// of the crowd
	private const float VIP_PRIORITY = 20.0f;

	private ExperimentalNavigationMesh navigationMesh;
	private ExperimentalNavigationMeshFilter filter;
	private ExperimentalNavigationPathFetch reachFetch;
	private ExperimentalSeekerCrowd crowd;

	private List<Agent> agents = new List<Agent>();

	// runtime-adjustable settings
	private int scenario = (int)SCENARIO.WANDER;
	private int agentsCount = 50;
	private float circleRadius = 10.0f;
	private bool vipEnabled = false;
	private bool teamsIgnoreEachOther = false;
	private bool useObstacles = false;
	private bool densityEnabled = false;
	private float densityCellSize = 1.0f;
	private bool showDebug = false;
	private bool showNavigationMesh = false;

	private int goalsReached = 0;

	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	private EventConnections widgetConnections = new EventConnections();

	[MethodInit(Order = 2)]
	private void Init()
	{
		Visualizer.Enabled = true;
		Unigine.Console.Run("experimental_navigation_show_mesh 0");
		Unigine.Console.Run("experimental_navigation_show_mesh_mode 1");
		Unigine.Console.Run("experimental_navigation_show_mesh_depth_test 1");
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		navigationMesh = navigation as ExperimentalNavigationMesh;

		// the filter starts with the parameters the navigation mesh was baked
		// with, so the mesh matches the filter by default
		filter = new ExperimentalNavigationMeshFilter();
		if (navigationMesh)
		{
			filter.NavigationMask = navigationMesh.NavigationMask;

			var bakeSettings = navigationMesh.BakeSettings;
			filter.AgentRadius = bakeSettings.AgentRadius;
			filter.AgentHeight = bakeSettings.AgentHeight;
		}

		// used to validate the spawn grid cells by reachability
		reachFetch = new ExperimentalNavigationPathFetch();
		reachFetch.Filter = filter;
		reachFetch.NavigationMesh = navigationMesh;

		// the crowd controller component must be added in the world,
		// the sample only picks it up
		crowd = GetComponent<ExperimentalSeekerCrowd>(node);
		if (crowd == null)
		{
			var crowds = ComponentSystem.FindComponentsInWorld<ExperimentalSeekerCrowd>(true);
			if (crowds.Length > 0)
				crowd = crowds[0];
		}
		if (crowd == null)
			Log.Warning("ExperimentalNavigationMeshCrowdLogic: no ExperimentalSeekerCrowd component in the world\n");

		// the body obstacles are respected by the route planning only: the solver
		// already represents the agents natively, their obstacles would double them
		if (crowd != null && crowd.Avoidance != null)
			crowd.Avoidance.ObstacleMask = 0;

		agentsCount = numAgents;
		RestartScenario();

		CreateUi();
	}

	private void Update()
	{
		if (!navigationMesh || crowd == null)
		{
			sampleDescriptionWindow.setStatus("Assign the experimental navigation mesh node and add the ExperimentalSeekerCrowd component to the world.");
			return;
		}

		// retarget the agents that reached their goals; the positions are taken
		// from the seeker node: the component may sit on a child of the asset
		foreach (var agent in agents)
		{
			if (agent.seeker == null || !agent.target)
				continue;

			Vec3 delta = agent.target.WorldPosition - agent.seeker.node.WorldPosition;
			delta.z = 0.0f;
			if (delta.Length2 < REACH_DISTANCE * REACH_DISTANCE)
			{
				goalsReached += 1;
				RetargetAgent(agent);
			}
		}

		// highlight the VIP agent
		if (vipEnabled && agents.Count > 0 && agents[0].seeker != null)
			Visualizer.RenderCircle(0.7f, MathLib.Translate(agents[0].seeker.node.WorldPosition + new Vec3(0.0f, 0.0f, 0.1f)), new vec4(1.0f, 1.0f, 0.0f, 1.0f), Game.IFps);

		if (showNavigationMesh)
			navigationMesh.RenderVisualizer();

		RenderDensity();
		UpdateStatus();
	}

	private void Shutdown()
	{
		ClearAgents();
		widgetConnections.DisconnectAll();
		Visualizer.Enabled = false;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
		sampleDescriptionWindow.shutdown();

		// the native navigation objects are deleted with the component on the
		// main thread, the way the smart pointer members do it in C++
		reachFetch?.Dispose();
		reachFetch = null;
		filter?.Dispose();
		filter = null;
	}

	// Respawns the agents according to the current scenario.
	private void RestartScenario()
	{
		ClearAgents();
		goalsReached = 0;

		if (!navigationMesh || crowd == null)
			return;

		Vec3 baseA = spawnA ? spawnA.WorldPosition : navigationMesh.WorldPosition;
		Vec3 baseB = spawnB ? spawnB.WorldPosition : navigationMesh.WorldPosition;

		int count = MathLib.Max(agentsCount, 2);

		switch ((SCENARIO)scenario)
		{
			// two teams facing each other across the gates. The spawn grids are
			// fit into the walkable area around the bases, the slots of the two
			// grids are paired: each agent walks back and forth between its own
			// slot and the paired slot on the opposite side
			case SCENARIO.BOTTLENECK:
			{
				List<Vec3> gridA = BuildSpawnGrid(baseA, count / 2);
				List<Vec3> gridB = BuildSpawnGrid(baseB, count / 2);
				int pairs = MathLib.Min(gridA.Count, gridB.Count);

				for (int i = 0; i < pairs; i += 1)
				{
					if (SpawnAgent(TEAM_RED, gridA[i], gridB[i]))
					{
						Agent agent = agents[agents.Count - 1];
						agent.pointA = agent.seeker.node.WorldPosition;
						agent.pointB = gridB[i];
					}
					if (SpawnAgent(TEAM_BLUE, gridB[i], gridA[i]))
					{
						Agent agent = agents[agents.Count - 1];
						agent.pointA = agent.seeker.node.WorldPosition;
						agent.pointB = gridA[i];
					}
				}
				break;
			}

			// agents evenly placed on a circle walk to the diametrically opposite
			// points, meeting each other in the center; the circle is centered on
			// the navigation mesh (the middle of the arena)
			case SCENARIO.CIRCLE:
			{
				Vec3 center = navigationMesh.WorldPosition;
				if (spawnA)
					center.z = spawnA.WorldPosition.z;

				for (int i = 0; i < count; i += 1)
				{
					float angle = MathLib.PI2 * i / count;
					Vec3 offset = new Vec3(MathLib.Cos(angle), MathLib.Sin(angle), 0.0f) * circleRadius;

					// both ends of the diameter must be on the mesh and reachable
					Vec3 startPoint, endPoint;
					if (!IsSpawnCellValid(center, center + offset, out startPoint)
						|| !IsSpawnCellValid(center, center - offset, out endPoint))
						continue;

					if (!SpawnAgent(i % 2, startPoint, endPoint))
						continue;

					Agent agent = agents[agents.Count - 1];
					agent.pointA = agent.seeker.node.WorldPosition;
					agent.pointB = endPoint;
				}
				break;
			}

			// agents wander between random reachable points
			case SCENARIO.WANDER:
			{
				for (int i = 0; i < count; i += 1)
				{
					Vec3 position;
					if (!navigationMesh.FindRandomPoint(filter, out position))
						position = baseA;
					SpawnAgent(i % 2, position, RandomReachablePoint(position, 30.0f));
				}
				break;
			}
		}

		ApplyTeamMasks();
		ApplyObstacleMasks();
		ApplyVip();
	}

	private void ClearAgents()
	{
		// unregister the agents before deleting their nodes
		if (crowd != null)
			crowd.ClearAgents();

		foreach (var agent in agents)
		{
			if (agent.node)
				agent.node.DeleteLater();
			if (agent.target)
				agent.target.DeleteLater();
		}
		agents.Clear();
	}

	// Checks that a spawn cell lies on the mesh close to the requested position
	// and is directly reachable from the base: a complete path not much longer
	// than the straight line rejects the cells behind the walls and the gates.
	private bool IsSpawnCellValid(Vec3 basePoint, Vec3 cell, out Vec3 retSnapped)
	{
		if (!navigationMesh.FindNearestPoint(filter, cell, out retSnapped))
			return false;

		Vec3 delta = retSnapped - cell;
		delta.z = 0.0f;
		if (delta.Length2 > (0.6f * SPAWN_SPACING) * (0.6f * SPAWN_SPACING))
			return false;

		if (!reachFetch.FetchForce(basePoint, retSnapped))
			return false;
		using var path = reachFetch.TakePath();
		if (path == null || path.Status != ExperimentalNavigationPath.STATUS.COMPLETE)
			return false;

		float straight = (float)(retSnapped - basePoint).Length;
		return path.Length <= straight * 1.5f + 3.0f;
	}

	// Builds a spawn grid fit into the walkable area: the cells grow ring by
	// ring around the base and only the valid ones are kept, so the grid never
	// leaks off the mesh or to the other side of the walls.
	private List<Vec3> BuildSpawnGrid(Vec3 basePoint, int count)
	{
		const int maxRing = 24;

		List<Vec3> cells = new List<Vec3>();
		Vec3 snapped;

		if (IsSpawnCellValid(basePoint, basePoint, out snapped))
			cells.Add(snapped);

		for (int ring = 1; ring <= maxRing && cells.Count < count; ring += 1)
		{
			for (int y = -ring; y <= ring && cells.Count < count; y += 1)
			{
				for (int x = -ring; x <= ring && cells.Count < count; x += 1)
				{
					// only the cells of the current ring
					if (MathLib.Max(MathLib.Abs(x), MathLib.Abs(y)) != ring)
						continue;

					Vec3 cell = basePoint + new Vec3(x * SPAWN_SPACING, y * SPAWN_SPACING, 0.0f);
					if (IsSpawnCellValid(basePoint, cell, out snapped))
						cells.Add(snapped);
				}
			}
		}

		return cells;
	}

	// Spawns a single agent of the team at the position and registers it in the
	// crowd. The seeker properties are assigned before the component init runs.
	private bool SpawnAgent(int team, Vec3 position, Vec3 targetPosition)
	{
		string assetPath = team == TEAM_RED ? agentRed.Path : agentBlue.Path;
		Node agentNode = World.LoadNode(assetPath);
		if (!agentNode)
		{
			Log.Warning($"ExperimentalNavigationMeshCrowdLogic: cannot load the agent asset \"{assetPath}\"\n");
			return false;
		}

		ExperimentalSeeker seeker = GetComponent<ExperimentalSeeker>(agentNode);
		if (seeker == null)
			seeker = GetComponentInChildren<ExperimentalSeeker>(agentNode);
		if (seeker == null)
		{
			Log.Warning($"ExperimentalNavigationMeshCrowdLogic: the agent asset \"{assetPath}\" has no ExperimentalSeeker component\n");
			agentNode.DeleteLater();
			return false;
		}

		// an agent must never spawn off the mesh: its route fetch would fail and
		// it would stand still forever; fall back to a random reachable point
		Vec3 snapped = position;
		if (!navigationMesh.FindNearestPoint(filter, position, out snapped))
			snapped = RandomReachablePoint(position, 10.0f);
		agentNode.WorldPosition = snapped;

		Node targetNode = new NodeDummy();
		targetNode.WorldPosition = targetPosition;

		seeker.navigation = navigation;
		seeker.target = targetNode;
		seeker.debugVisualizerEnabled = showDebug;

		// unequal agents break the symmetric head-on deadlocks (e.g. in the middle
		// of the circle scenario): each agent gets a random avoidance priority and
		// a randomized walk speed
		seeker.movementSpeed = seeker.movementSpeed * Game.GetRandomFloat(0.8f, 1.2f);

		crowd.AddAgent(seeker);

		Agent agent = new Agent();
		agent.seeker = seeker;
		agent.node = agentNode;
		agent.target = targetNode;
		agent.team = team;
		agent.avoidancePriority = Game.GetRandomFloat(0.0f, 5.0f);
		agents.Add(agent);
		return true;
	}

	// Assigns the next goal to an agent that reached its target.
	private void RetargetAgent(Agent agent)
	{
		switch ((SCENARIO)scenario)
		{
			case SCENARIO.BOTTLENECK:
			case SCENARIO.CIRCLE:
				agent.toB = !agent.toB;
				agent.target.WorldPosition = agent.toB ? agent.pointB : agent.pointA;
				break;

			case SCENARIO.WANDER:
			{
				// retry a few times so the next point is not right next to the agent
				Vec3 position = agent.seeker.node.WorldPosition;
				Vec3 next = position;
				for (int attempt = 0; attempt < 8; attempt += 1)
				{
					next = RandomReachablePoint(position, 30.0f);
					if ((next - position).Length2 > 25.0f)
						break;
				}
				agent.target.WorldPosition = next;
				break;
			}
		}
	}

	private Vec3 RandomReachablePoint(Vec3 around, float maxCost)
	{
		Vec3 point;
		if (navigationMesh.FindRandomReachablePoint(filter, around, maxCost, out point, 0))
			return point;
		if (navigationMesh.FindRandomPoint(filter, out point))
			return point;
		return around;
	}

	// Teleports all agents into a small disk, demonstrating how the crowd
	// separates overlapping agents.
	private void TeleportAll()
	{
		Vec3 center = spawnA ? spawnA.WorldPosition : navigationMesh.WorldPosition;
		foreach (var agent in agents)
		{
			if (agent.seeker == null)
				continue;
			vec3 offset = new vec3(Game.GetRandomFloat(-1.5f, 1.5f), Game.GetRandomFloat(-1.5f, 1.5f), 0.0f);
			agent.seeker.Teleport(center + new Vec3(offset));
		}
	}

	// When the teams ignore each other, each team gets its own interaction mask
	// bit, so the cross-team mask overlap is empty.
	private void ApplyTeamMasks()
	{
		foreach (var agent in agents)
		{
			if (agent.seeker == null)
				continue;
			agent.seeker.AvoidanceInteractionMask = teamsIgnoreEachOther ? TEAM_MASKS[agent.team] : ~0;
		}
	}

	// Assigns the obstacle mask each agent respects: none while the body obstacles
	// are disabled, the obstacles of both teams by default, only the own team ones
	// when the teams ignore each other. The own obstacle of an agent is excluded
	// by the seeker itself.
	private void ApplyObstacleMasks()
	{
		foreach (var agent in agents)
		{
			if (agent.seeker == null)
				continue;

			int mask = 0;
			if (useObstacles)
				mask = teamsIgnoreEachOther ? TEAM_MASKS[agent.team] : (TEAM_MASKS[TEAM_RED] | TEAM_MASKS[TEAM_BLUE]);

			agent.seeker.obstacleMask = mask;
			agent.seeker.ApplySettings();
		}
	}

	// The VIP priority is applied to the first agent, others keep the random
	// priorities rolled at spawn.
	private void ApplyVip()
	{
		foreach (var agent in agents)
		{
			if (agent.seeker != null)
				agent.seeker.AvoidancePriority = agent.avoidancePriority;
		}

		if (vipEnabled && agents.Count > 0 && agents[0].seeker != null)
			agents[0].seeker.AvoidancePriority = VIP_PRIORITY;
	}

	// Draws the agent density heatmap over the navigation mesh bake area.
	private void RenderDensity()
	{
		if (!densityEnabled)
			return;

		var avoidance = crowd.Avoidance;
		if (avoidance == null)
			return;

		vec3 bakeSize = navigationMesh.BakeSettings.BakeSize;
		Vec3 center = navigationMesh.WorldPosition;
		Scalar groundZ = spawnA ? spawnA.WorldPosition.z : center.z;

		float cell = MathLib.Max(densityCellSize, 0.5f);
		int halfX = MathLib.Min((int)(bakeSize.x * 0.5f / cell), 40);
		int halfY = MathLib.Min((int)(bakeSize.y * 0.5f / cell), 40);

		for (int y = -halfY; y <= halfY; y += 1)
		{
			for (int x = -halfX; x <= halfX; x += 1)
			{
				Vec3 point = new Vec3(center.x + x * cell, center.y + y * cell, groundZ);
				float density = avoidance.GetAgentDensity(point);
				if (density < 0.05f)
					continue;

				vec4 color = MathLib.Lerp(new vec4(0.0f, 1.0f, 0.0f, 0.35f), new vec4(1.0f, 0.0f, 0.0f, 0.5f), MathLib.Saturate(density * 0.25f));
				Visualizer.RenderSolidBox(new vec3(cell * 0.95f, cell * 0.95f, 0.02f), MathLib.Translate(point + new Vec3(0.0f, 0.0f, 0.05f)), color, Game.IFps);
			}
		}
	}

	private void UpdateStatus()
	{
		string[] scenarioNames = { "Bottleneck", "Circle", "Wander" };

		// agents without a complete route signal a broken spawn or an
		// unreachable target
		int noRoute = 0;
		foreach (var agent in agents)
		{
			if (agent.seeker != null && agent.seeker.RouteStatus != ExperimentalNavigationPath.STATUS.COMPLETE)
				noRoute += 1;
		}

		sampleDescriptionWindow.setStatus(
			$"Scenario: {scenarioNames[scenario]}\n" +
			$"Agents: {agents.Count}\n" +
			$"Agents without a route: {noRoute}\n" +
			$"Goals reached: {goalsReached}\n");
	}

	private void CreateUi()
	{
		sampleDescriptionWindow.createWindow(Gui.ALIGN_LEFT, 600);
		var window = sampleDescriptionWindow.MainWindow;

		// all the parameter groups live in one scroll box to keep the window compact
		var scrollBox = new WidgetScrollBox();
		scrollBox.Height = 420;
		window.AddChild(scrollBox, Gui.ALIGN_EXPAND);

		var scrollContent = new WidgetVBox(0, 4);
		scrollBox.AddChild(scrollContent, Gui.ALIGN_EXPAND);

		// scenario control
		{
			var group = new WidgetGroupBox("Scenario", 8, 8);
			scrollContent.AddChild(group, Gui.ALIGN_EXPAND);

			var grid = new WidgetGridBox(3, 5, 5);
			group.AddChild(grid, Gui.ALIGN_EXPAND);

			SampleWidgets.AddSwitchParameter(widgetConnections, grid, "Scenario",
				"Bottleneck: two teams cross through the gates. Circle: agents swap places across a circle. Wander: agents walk between random points.",
				scenario, new string[] { "Bottleneck", "Circle", "Wander" },
				(int value) =>
				{
					scenario = value;
					RestartScenario();
				});

			SampleWidgets.AddIntParameter(widgetConnections, grid, "Agents",
				"The number of agents, applied on restart.",
				agentsCount, 10, 300,
				(int value) =>
				{
					agentsCount = value;
				});

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Circle radius",
				"The radius of the circle scenario, applied on restart.",
				circleRadius, 2.0f, 20.0f,
				(float value) =>
				{
					circleRadius = value;
				});

			SampleWidgets.AddButton(widgetConnections, group, "Restart scenario",
				"Respawns the agents with the current settings.",
				() => { RestartScenario(); });

			SampleWidgets.AddButton(widgetConnections, group, "Teleport all to one point",
				"Teleports all agents into a small disk: the crowd pushes the overlapping agents apart.",
				() => { TeleportAll(); });
		}

		// avoidance solver settings, pushed to the reusable crowd component
		if (crowd != null)
		{
			var group = new WidgetGroupBox("Avoidance solver", 8, 8);
			scrollContent.AddChild(group, Gui.ALIGN_EXPAND);

			var grid = new WidgetGridBox(3, 5, 5);
			group.AddChild(grid, Gui.ALIGN_EXPAND);

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Neighbor range",
				"The range within which the agents react to each other. The solver recommends at least 2 * (max_speed * prediction_time + agent_radius).",
				crowd.neighborRange, 1.0f, 20.0f,
				(float value) => { crowd.neighborRange = value; crowd.ApplySettings(); });

			SampleWidgets.AddIntParameter(widgetConnections, grid, "Max neighbors",
				"The maximum number of neighbors an agent takes into account.",
				crowd.maxNeighbors, 1, 32,
				(int value) => { crowd.maxNeighbors = value; crowd.ApplySettings(); });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Prediction time",
				"How far ahead (in seconds) the agents predict the collisions.",
				crowd.predictionTime, 0.0f, 2.0f,
				(float value) => { crowd.predictionTime = value; crowd.ApplySettings(); });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Desired velocity",
				"The weight of following the desired velocity.",
				crowd.weightDesiredVelocity, 0.0f, 5.0f,
				(float value) => { crowd.weightDesiredVelocity = value; crowd.ApplySettings(); });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Current velocity",
				"The weight of keeping the current velocity.",
				crowd.weightCurrentVelocity, 0.0f, 5.0f,
				(float value) => { crowd.weightCurrentVelocity = value; crowd.ApplySettings(); });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Side preference",
				"The weight of preferring one side when avoiding.",
				crowd.weightSide, 0.0f, 5.0f,
				(float value) => { crowd.weightSide = value; crowd.ApplySettings(); });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Time to impact",
				"The weight of avoiding the imminent collisions.",
				crowd.weightTimeToImpact, 0.0f, 5.0f,
				(float value) => { crowd.weightTimeToImpact = value; crowd.ApplySettings(); });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Separation",
				"The weight of keeping distance from the neighbors.",
				crowd.weightSeparation, 0.0f, 5.0f,
				(float value) => { crowd.weightSeparation = value; crowd.ApplySettings(); });
		}

		// per-agent solver overrides
		{
			var group = new WidgetGroupBox("Agents", 8, 8);
			scrollContent.AddChild(group, Gui.ALIGN_EXPAND);

			SampleWidgets.AddBoolParameter(widgetConnections, group, "VIP agent",
				"Gives the first agent a fixed high avoidance priority (the regular agents get a random one from 0 to 5 at spawn): the crowd yields to it. The VIP agent is highlighted with a circle.",
				vipEnabled,
				(bool enabled) =>
				{
					vipEnabled = enabled;
					ApplyVip();
				});

			SampleWidgets.AddBoolParameter(widgetConnections, group, "Teams ignore each other",
				"Gives each team its own interaction mask, so the agents avoid only their teammates and respect only their body obstacles.",
				teamsIgnoreEachOther,
				(bool enabled) =>
				{
					teamsIgnoreEachOther = enabled;
					ApplyTeamMasks();
					ApplyObstacleMasks();
				});

			SampleWidgets.AddBoolParameter(widgetConnections, group, "Use obstacles",
				"Makes the agents respect the body obstacles of the others: a standing group obstructs the walkable area and the routes are rebuilt around it. The own obstacle of an agent never blocks it.",
				useObstacles,
				(bool enabled) =>
				{
					useObstacles = enabled;
					ApplyObstacleMasks();
				});
		}

		// visualization
		{
			var group = new WidgetGroupBox("Visualization", 8, 8);
			scrollContent.AddChild(group, Gui.ALIGN_EXPAND);

			var grid = new WidgetGridBox(3, 5, 5);
			group.AddChild(grid, Gui.ALIGN_EXPAND);

			SampleWidgets.AddBoolParameter(widgetConnections, grid, "Show Seeker Debug",
				"Draws the route corridor and the velocities of every agent.",
				showDebug,
				(bool enabled) =>
				{
					showDebug = enabled;
					foreach (var agent in agents)
					{
						if (agent.seeker != null)
							agent.seeker.debugVisualizerEnabled = enabled;
					}
				});

			SampleWidgets.AddBoolParameter(widgetConnections, grid, "Show Navigation Mesh",
				"Draws the navigation mesh colored by the areas of its polygons.",
				showNavigationMesh,
				(bool enabled) =>
				{
					showNavigationMesh = enabled;
				});

			SampleWidgets.AddBoolParameter(widgetConnections, grid, "Density heatmap",
				"Visualizes the agent density computed by the avoidance solver.",
				densityEnabled,
				(bool enabled) =>
				{
					densityEnabled = enabled;
					if (crowd != null && crowd.Avoidance != null)
					{
						crowd.Avoidance.DensityEnabled = enabled;
						crowd.Avoidance.DensityCellSize = densityCellSize;
					}
				});

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Density cell",
				"The cell size of the density grid.",
				densityCellSize, 0.5f, 4.0f,
				(float value) =>
				{
					densityCellSize = value;
					if (crowd != null && crowd.Avoidance != null)
						crowd.Avoidance.DensityCellSize = value;
				});
		}

		window.Arrange();
	}
}
