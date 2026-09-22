// Interactive demo of hierarchical navigation on a large streamed terrain.
// The world holds two experimental navigation meshes: a fully resident low
// resolution mesh for the global plan and a dynamic high resolution mesh
// whose tiles are baked at runtime around the invokers. A group of animated
// characters walks to the point clicked on the terrain: each agent plans the
// global route on the low resolution mesh, splits it into waypoints, and
// refines every leg on the high resolution mesh streamed around it by its
// own invoker (see ExperimentalSeeker).
// Boxes and spheres with dynamic cut area volumes can be spawned and dragged
// with the manipulator, imitating a construction site: moving a volume
// automatically rebakes the touched tiles of both meshes, so the agents
// route around the buildings.

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

public partial class ExperimentalNavigationMeshTerrainLogic : Component
{
	// the navigation mesh nodes; found in the world by the streaming mode
	// when not assigned
	public Node navigationHigh = null;
	public Node navigationLow = null;
	// the assets of the agent and the building blocks; the sample defaults
	// are used when empty
	[ParameterFile(Filter = ".node")]
	public AssetLink agentAsset = null;
	[ParameterFile(Filter = ".node")]
	public AssetLink boxAsset = null;
	[ParameterFile(Filter = ".node")]
	public AssetLink sphereAsset = null;
	public int numAgents = 8;
	// where the agents are (re)spawned; the spot in front of the camera is
	// used when empty
	public Node spawnPoint = null;
	// the intersection mask of the walkable ground: the click-to-move ray
	// looks for the target point on the surfaces matching it, and the spawned
	// nodes are dropped onto them
	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	public int targetIntersectionMask = 1;

	// the sample assets used when the file parameters are left empty
	private const string DEFAULT_AGENT_ASSET = "csharp_component_samples/navigation/experimental_navigation_mesh_character/character.node";
	private const string DEFAULT_BOX_ASSET = "csharp_component_samples/navigation/experimental_navigation_mesh_terrain/box.node";
	private const string DEFAULT_SPHERE_ASSET = "csharp_component_samples/navigation/experimental_navigation_mesh_terrain/sphere.node";

	// spacing of the agent spawn grid
	private const float SPAWN_SPACING = 1.5f;

	// the mouse travel (in pixels) above which a right button press counts as a
	// camera look, not a click
	private const float CLICK_DRAG_THRESHOLD = 8.0f;

	private ExperimentalNavigationMesh highMesh;
	private ExperimentalNavigationMesh lowMesh;
	private ExperimentalSeekerCrowd crowd;
	private Manipulators manipulators;

	private List<ExperimentalSeeker> agents = new List<ExperimentalSeeker>();
	private List<Node> agentNodes = new List<Node>();
	private List<Node> blocks = new List<Node>();

	// runtime-adjustable settings
	private int agentsCount = 8;
	private float agentSpeed = 1.34f;
	private bool showDebug = false;
	private bool carveWhileDragging = true;
	private bool showLocalNavigationMesh = false;
	private bool showGlobalNavigationMesh = false;

	private Vec3 lastTarget = Vec3.ZERO;
	private bool hasLastTarget = false;

	// the mouse travel of the current right button press, see HandleClick()
	private float clickMouseTravel = 0.0f;

	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	private EventConnections widgetConnections = new EventConnections();

	private static string AssetOrDefault(AssetLink asset, string defaultPath)
	{
		return (asset != null && !asset.IsNull) ? asset.Path : defaultPath;
	}

	[MethodInit(Order = 2)]
	private void Init()
	{
		Visualizer.Enabled = true;
		Unigine.Console.Run("experimental_navigation_show_mesh 0");
		Unigine.Console.Run("experimental_navigation_show_mesh_mode 1");
		Unigine.Console.Run("experimental_navigation_show_mesh_depth_test 1");
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		// the meshes are either assigned or recognized by the streaming mode: the
		// high resolution mesh streams (dynamic bake), the low resolution one is
		// fully resident
		highMesh = navigationHigh as ExperimentalNavigationMesh;
		lowMesh = navigationLow as ExperimentalNavigationMesh;
		if (!highMesh || !lowMesh)
		{
			List<Node> nodes = new List<Node>();
			World.GetNodesByType((int)Node.TYPE.EXPERIMENTAL_NAVIGATION_MESH, nodes);
			foreach (Node meshNode in nodes)
			{
				var mesh = meshNode as ExperimentalNavigationMesh;
				if (!mesh)
					continue;
				bool streamed = mesh.StreamingEnabled || mesh.BakeMode == ExperimentalNavigationMesh.BAKE_MODE.DYNAMIC;
				if (streamed && !highMesh)
					highMesh = mesh;
				if (!streamed && !lowMesh)
					lowMesh = mesh;
			}
		}
		if (!highMesh)
			Log.Warning("ExperimentalNavigationMeshTerrainLogic: no streamed high resolution navigation mesh in the world\n");
		if (!lowMesh)
			Log.Warning("ExperimentalNavigationMeshTerrainLogic: no resident low resolution navigation mesh in the world\n");

		// the crowd controller and the manipulators sit on the same logic node.
		// The crowd is optional: without it the agents drive themselves and do
		// not avoid each other
		crowd = GetComponent<ExperimentalSeekerCrowd>(node);
		if (crowd == null)
			Log.Message("ExperimentalNavigationMeshTerrainLogic: no ExperimentalSeekerCrowd component on the logic node, the agents drive themselves\n");

		// the manipulator is configured in the world, the sample only picks it up
		manipulators = GetComponent<Manipulators>(node);

		agentsCount = numAgents;
		SpawnAgents();

		CreateUi();
	}

	private void Update()
	{
		if (!highMesh || !lowMesh)
		{
			sampleDescriptionWindow.setStatus("The world must contain two experimental navigation meshes: a streamed high resolution one and a resident low resolution one.");
			return;
		}

		// disable player controls while dragging a block: the manipulator hotkeys
		// share the keys with the camera
		if (manipulators != null)
			Game.Player.Controlled = !manipulators.Active;

		HandleClick();

		if (showLocalNavigationMesh)
			highMesh.RenderVisualizer();
		if (showGlobalNavigationMesh)
			lowMesh.RenderVisualizer();

		// the target is done once every agent has arrived: the marker goes out
		// and freshly respawned agents do not inherit the stale point
		if (hasLastTarget && agents.Count > 0)
		{
			bool allReached = true;
			foreach (ExperimentalSeeker agent in agents)
				allReached = allReached && agent.IsTargetReached;
			if (allReached)
				hasLastTarget = false;
		}

		// mark the commanded point
		if (hasLastTarget)
			Visualizer.RenderCircle(0.75f, MathLib.Translate(lastTarget + new Vec3(0.0f, 0.0f, 0.2f)), new vec4(1.0f, 0.85f, 0.0f, 1.0f), Game.IFps);

		UpdateStatus();
	}

	private void Shutdown()
	{
		ClearAgents();
		ClearBlocks();
		widgetConnections.DisconnectAll();
		Visualizer.Enabled = false;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
		sampleDescriptionWindow.shutdown();
	}

	// A right click on the terrain sends all agents to the clicked point. The
	// right button also drives the camera look, so only a short press without a
	// mouse travel counts as a click; a click on a spawned block is ignored.
	private void HandleClick()
	{
		if (Input.IsMouseButtonDown(Input.MOUSE_BUTTON.RIGHT))
			clickMouseTravel = 0.0f;
		if (Input.IsMouseButtonPressed(Input.MOUSE_BUTTON.RIGHT))
			clickMouseTravel += new vec2(Input.MouseDeltaPosition).Length;

		if (!Input.IsMouseButtonUp(Input.MOUSE_BUTTON.RIGHT))
			return;
		if (clickMouseTravel > CLICK_DRAG_THRESHOLD)
			return;
		if (Gui.GetCurrent().GetUnderCursorWidget())
			return;

		Player player = Game.Player;
		ivec2 mouse = Input.MousePosition;
		Vec3 p0 = player.WorldPosition;
		Vec3 p1 = p0 + new Vec3(player.GetDirectionFromMainWindow(mouse.x, mouse.y)) * (Scalar)4000.0;

		using WorldIntersection intersection = new WorldIntersection();
		Unigine.Object hit = World.GetIntersection(p0, p1, targetIntersectionMask, intersection);
		if (!hit || IsBlock(hit))
			return;

		lastTarget = intersection.Point;
		hasLastTarget = true;

		// everyone walks to the same point: the arrival is loose enough
		// (targetReachDistance) for the agents not to fight for the exact spot
		foreach (ExperimentalSeeker agent in agents)
			agent.SetTargetPosition(lastTarget);
	}

	private bool IsBlock(Node hitNode)
	{
		for (Node current = hitNode; current; current = current.Parent)
		{
			if (blocks.Contains(current))
				return true;
		}
		return false;
	}

	// Drops the point onto the ground with a vertical ray.
	private bool GroundPoint(Vec3 around, ref Vec3 retPoint)
	{
		using WorldIntersection intersection = new WorldIntersection();
		Unigine.Object hit = World.GetIntersection(around + new Vec3(0.0f, 0.0f, 200.0f), around - new Vec3(0.0f, 0.0f, 500.0f), targetIntersectionMask, intersection);
		if (!hit)
			return false;

		retPoint = intersection.Point;
		return true;
	}

	// The spot on the ground the camera looks at: a straight ray along the view
	// direction down to the terrain.
	private bool PointInFront(ref Vec3 retPoint)
	{
		Player player = Game.Player;
		Vec3 p0 = player.WorldPosition;
		Vec3 p1 = p0 + new Vec3(player.ViewDirection) * (Scalar)4000.0;

		using WorldIntersection intersection = new WorldIntersection();
		Unigine.Object hit = World.GetIntersection(p0, p1, targetIntersectionMask, intersection);
		if (!hit)
			return false;

		retPoint = intersection.Point;
		return true;
	}

	// Spawns the agents on a grid in front of the camera. The agent asset carries
	// an ExperimentalNavigationMeshInvoker, so the high resolution tiles are baked
	// around each agent wherever it goes; the routes are planned hierarchically
	// through the low resolution mesh.
	private void SpawnAgents()
	{
		ClearAgents();

		if (!highMesh || !lowMesh)
			return;

		// the agents spawn around the spawn point when it is assigned, otherwise
		// in front of the camera
		Vec3 center = Vec3.ZERO;
		if (spawnPoint)
		{
			center = spawnPoint.WorldPosition;
			GroundPoint(center, ref center);
		}
		else if (!PointInFront(ref center))
			return;

		string assetPath = AssetOrDefault(agentAsset, DEFAULT_AGENT_ASSET);

		int side = (int)MathLib.Ceil(MathLib.Sqrt((float)agentsCount));
		for (int i = 0; i < agentsCount; i++)
		{
			Vec3 cell = center + new Vec3((i % side - side / 2) * SPAWN_SPACING, (i / side - side / 2) * SPAWN_SPACING, 0.0f);
			Vec3 position = cell;
			GroundPoint(cell, ref position);

			Node agentNode = World.LoadNode(assetPath);
			if (!agentNode)
			{
				Log.Warning($"ExperimentalNavigationMeshTerrainLogic: cannot load the agent asset \"{assetPath}\"\n");
				return;
			}

			ExperimentalSeeker seeker = GetComponent<ExperimentalSeeker>(agentNode);
			if (seeker == null)
				seeker = GetComponentInChildren<ExperimentalSeeker>(agentNode);
			if (seeker == null)
			{
				Log.Warning($"ExperimentalNavigationMeshTerrainLogic: the agent asset \"{assetPath}\" has no ExperimentalSeeker component\n");
				agentNode.DeleteLater();
				return;
			}

			agentNode.WorldPosition = position;

			// the asset may carry the references of its authoring world: rewire
			// the seeker to the meshes of this one; the properties are assigned
			// before the component init runs
			seeker.navigation = highMesh;
			seeker.globalNavigation = lowMesh;
			seeker.movementSpeed = agentSpeed;
			seeker.debugVisualizerEnabled = showDebug;

			if (crowd != null)
				crowd.AddAgent(seeker);

			agents.Add(seeker);
			agentNodes.Add(agentNode);

			if (hasLastTarget)
				seeker.SetTargetPosition(lastTarget);
		}
	}

	private void ClearAgents()
	{
		// unregister the agents before deleting their nodes
		if (crowd != null)
			crowd.ClearAgents();

		foreach (Node agentNode in agentNodes)
		{
			if (agentNode)
				agentNode.DeleteLater();
		}
		agents.Clear();
		agentNodes.Clear();
	}

	private void ApplyAgentSettings()
	{
		foreach (ExperimentalSeeker agent in agents)
		{
			agent.movementSpeed = agentSpeed;
			agent.debugVisualizerEnabled = showDebug;
		}
	}

	// Spawns a building block in front of the camera. The block asset carries a
	// dynamic cut area volume: wherever the block is dragged, the volume cuts the
	// walkable area out of both meshes, and the touched tiles are rebaked by the
	// engine automatically.
	private void SpawnBlock(string assetPath)
	{
		Vec3 position = Vec3.ZERO;
		if (!PointInFront(ref position))
			return;

		Node block = World.LoadNode(assetPath);
		if (!block)
		{
			Log.Warning($"ExperimentalNavigationMeshTerrainLogic: cannot load the block asset \"{assetPath}\"\n");
			return;
		}

		block.WorldPosition = position;
		blocks.Add(block);
	}

	private void ClearBlocks()
	{
		// deleting a block invalidates the area its volume covered
		foreach (Node block in blocks)
		{
			if (block)
				block.DeleteLater();
		}
		blocks.Clear();
	}

	// Applies the carve mode to the volumes of all blocks: while dragging, the
	// walkable area follows the block continuously; in the stationary mode the
	// rebake happens once the block is released and settles. Called only when
	// the mode is switched in the UI: a freshly spawned block keeps the settings
	// of its asset.
	private void ApplyCarveMode()
	{
		foreach (Node block in blocks)
		{
			if (!block)
				continue;

			List<Node> hierarchy = new List<Node>();
			hierarchy.Add(block);
			block.GetHierarchy(hierarchy);
			foreach (Node child in hierarchy)
			{
				var volume = child as ExperimentalNavigationMeshAreaVolume;
				if (volume)
					volume.DynamicApplyOnlyWhenStationary = !carveWhileDragging;
			}
		}
	}

	private void UpdateStatus()
	{
		string status = "";

		status += $"High resolution mesh: {highMesh.NumLoadedTiles} / {highMesh.NumTiles} tiles loaded, {highMesh.NumPolygons} polygons\n";
		status += $"Low resolution mesh: {lowMesh.NumPolygons} polygons\n";
		string baking = ExperimentalBakeNavigation.IsBaking ? ", baking..." : "";
		string memoryLimit = ExperimentalNavigation.IsStreamingMemoryLimitReached ? ", streaming memory limit reached!" : "";
		status += $"Navigation tasks: {ExperimentalNavigation.NumActiveTasks}{baking}{memoryLimit}\n";
		status += $"Agents: {agents.Count}, blocks: {blocks.Count}\n";

		// the routes of the lead agent show how the hierarchical plan advances;
		// the failure reasons point at the misconfiguration when a route cannot
		// be built (e.g. no invoker streaming the tiles around the agent)
		if (agents.Count > 0)
		{
			ExperimentalSeeker lead = agents[0];
			if (!lead.HasTarget())
				status += "Lead agent: no target, right-click the terrain to send the agents\n";
			else
			{
				string globalLine = ExperimentalNavigationUtils.GetRouteStatusName(lead.GlobalRouteStatus);
				if (lead.GlobalRouteStatus != ExperimentalNavigationPath.STATUS.COMPLETE)
					globalLine += $" ({ExperimentalNavigationUtils.GetRouteFailureName(lead.GlobalRouteFailure)})";

				string localLine = ExperimentalNavigationUtils.GetRouteStatusName(lead.RouteStatus);
				if (lead.RouteStatus != ExperimentalNavigationPath.STATUS.COMPLETE)
					localLine += $" ({ExperimentalNavigationUtils.GetRouteFailureName(lead.RouteFailure)})";

				status += $"Lead agent: global route {globalLine}, leg {lead.CurrentWaypointIndex + 1} / {lead.NumWaypoints}, local route {localLine}\n";
			}

			// the locomotion state of the lead character, e.g. to spot an agent
			// stuck in a turn or a stop
			if (agentNodes.Count > 0)
			{
				var character = GetComponentInChildren<ExperimentalSeekerCharacter>(agentNodes[0]);
				if (character != null)
					status += $"Lead agent locomotion: {character.GetControlStateName()} / {character.GetLocomotionStateName()}\n";
			}
		}

		sampleDescriptionWindow.setStatus(status);
	}

	private void CreateUi()
	{
		sampleDescriptionWindow.createWindow(Gui.ALIGN_LEFT, 600);
		var window = sampleDescriptionWindow.MainWindow;

		// agents
		{
			var group = new WidgetGroupBox("Agents", 8, 8);
			window.AddChild(group, Gui.ALIGN_LEFT);

			var grid = new WidgetGridBox(3);
			group.AddChild(grid, Gui.ALIGN_EXPAND);

			SampleWidgets.AddIntParameter(widgetConnections, grid, "Agents",
				"The number of agents, applied on respawn.",
				agentsCount, 1, 50,
				(int value) =>
				{
					agentsCount = value;
				});

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Movement speed",
				"The walking speed of the agents (in m/s): the locomotion animations are played at the matching rate.",
				agentSpeed, 0.7f, 3.0f,
				(float value) =>
				{
					agentSpeed = value;
					ApplyAgentSettings();
				});

			SampleWidgets.AddButton(widgetConnections, group, "Respawn agents",
				"Respawns the agents on a grid at the spawn point (or in front of the camera).",
				() => { SpawnAgents(); });
		}

		// construction
		{
			var group = new WidgetGroupBox("Construction", 8, 8);
			window.AddChild(group, Gui.ALIGN_LEFT);

			// the block actions share one row
			var buttons = new WidgetHBox(4, 4);
			group.AddChild(buttons, Gui.ALIGN_EXPAND);

			SampleWidgets.AddButton(widgetConnections, buttons, "Spawn box",
				"Spawns a box in front of the camera. Drag it with the manipulator: the cut volume of the box carves the walkable area out of both meshes.",
				() => { SpawnBlock(AssetOrDefault(boxAsset, DEFAULT_BOX_ASSET)); });

			SampleWidgets.AddButton(widgetConnections, buttons, "Spawn sphere",
				"Spawns a sphere in front of the camera. Drag it with the manipulator: the cut volume of the sphere carves the walkable area out of both meshes.",
				() => { SpawnBlock(AssetOrDefault(sphereAsset, DEFAULT_SPHERE_ASSET)); });

			SampleWidgets.AddButton(widgetConnections, buttons, "Clear blocks",
				"Deletes all spawned blocks; the areas they covered are rebaked.",
				() => { ClearBlocks(); });

			SampleWidgets.AddBoolParameter(widgetConnections, group, "Carve while dragging",
				"When enabled, the walkable area follows the dragged block continuously; otherwise the touched tiles are rebaked once the block is released. Switching the mode overrides the volume settings of all spawned blocks.",
				carveWhileDragging,
				(bool enabled) =>
				{
					carveWhileDragging = enabled;
					ApplyCarveMode();
				});
		}

		// visualization
		{
			var group = new WidgetGroupBox("Visualization", 8, 8);
			window.AddChild(group, Gui.ALIGN_LEFT);

			SampleWidgets.AddBoolParameter(widgetConnections, group, "Show Seeker Debug",
				"Draws the local route corridor and the waypoints of the global route of every agent.",
				showDebug,
				(bool enabled) =>
				{
					showDebug = enabled;
					ApplyAgentSettings();
				});

			SampleWidgets.AddBoolParameter(widgetConnections, group, "Show Local Navigation Mesh",
				"Draws the streamed high resolution mesh: the loaded tiles follow the agents.",
				showLocalNavigationMesh,
				(bool enabled) =>
				{
					showLocalNavigationMesh = enabled;
				});

			SampleWidgets.AddBoolParameter(widgetConnections, group, "Show Global Navigation Mesh",
				"Draws the fully resident low resolution mesh the global routes are planned on.",
				showGlobalNavigationMesh,
				(bool enabled) =>
				{
					showGlobalNavigationMesh = enabled;
				});
		}

		window.Arrange();
	}
}
