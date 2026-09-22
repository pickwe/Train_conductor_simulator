// Interactive demo with multiple ExperimentalSeeker agents chasing a target
// on an experimental navigation mesh. Each seeker computes a route with
// ExperimentalNavigationPathFetch and follows it using
// ExperimentalNavigationMeshCorridor. When a seeker reaches the target, the
// target relocates to a random position from a predefined set. Every seeker
// can be configured individually on its own tab of the sample window.

using System.Collections.Generic;
using Unigine;

public partial class ExperimentalNavigationMeshDemoLogic : Component
{
	[ParameterColor]
	public vec4 pathColor = vec4.WHITE;
	[ParameterColor]
	public vec4 selectedPathColor = new vec4(1.0f, 1.0f, 0.0f, 1.0f);

	// sample visualization
	private bool showNavigationMesh = true;

	private List<ExperimentalNavigationMesh> navigationMeshes = new List<ExperimentalNavigationMesh>();
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	private Manipulators widgetManipulator;
	private EventConnections widgetConnections = new EventConnections();

	private WidgetTabBox seekersTabbox;
	private List<ExperimentalSeeker> tabSeekers = new List<ExperimentalSeeker>();

	[MethodInit(Order = 2)]
	private void Init()
	{
		Visualizer.Enabled = true;
		Unigine.Console.Run("experimental_navigation_show_mesh 0");
		Unigine.Console.Run("experimental_navigation_show_mesh_mode 1");
		Unigine.Console.Run("experimental_navigation_show_mesh_depth_test 1");
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		// manipulators move the target and obstacles, scaling is disabled
		widgetManipulator = GetComponent<Manipulators>(node);
		if (widgetManipulator != null)
		{
			widgetManipulator.XAxisScale = false;
			widgetManipulator.YAxisScale = false;
			widgetManipulator.ZAxisScale = false;
		}

		// every seeker draws its debug visualizer by default; the crowd controller
		// does not collect agents itself: register all the seekers in it if one is
		// present in the world
		var worldSeekers = ComponentSystem.FindComponentsInWorld<ExperimentalSeeker>(true);
		var crowds = ComponentSystem.FindComponentsInWorld<ExperimentalSeekerCrowd>(true);

		foreach (var seeker in worldSeekers)
		{
			seeker.debugVisualizerEnabled = true;
			if (crowds.Length > 0)
				crowds[0].AddAgent(seeker);
		}

		// the navigation meshes of the world, for the visualization toggle
		List<Node> meshNodes = new List<Node>();
		World.GetNodesByType((int)Node.TYPE.EXPERIMENTAL_NAVIGATION_MESH, meshNodes);
		foreach (Node meshNode in meshNodes)
		{
			var mesh = meshNode as ExperimentalNavigationMesh;
			if (mesh)
				navigationMeshes.Add(mesh);
		}

		CreateUi();
	}

	private void Update()
	{
		// disable player controls while dragging manipulators
		Game.Player.Controlled = !widgetManipulator.Active;

		if (showNavigationMesh)
		{
			foreach (var mesh in navigationMeshes)
				mesh.RenderVisualizer();
		}
	}

	private void Shutdown()
	{
		Visualizer.Enabled = false;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
		sampleDescriptionWindow.shutdown();
	}

	private void CreateUi()
	{
		sampleDescriptionWindow.createWindow();

		CreateSeekersUi();

		// visualization shared by all seekers
		var group = new WidgetGroupBox("Visualization", 8, 8);
		sampleDescriptionWindow.MainWindow.AddChild(group, Gui.ALIGN_LEFT);

		SampleWidgets.AddBoolParameter(widgetConnections, group,
			"Show Navigation Mesh",
			"Draws the navigation mesh colored by the areas of its polygons.",
			showNavigationMesh,
			(bool enabled) =>
			{
				showNavigationMesh = enabled;
			});
	}

	// Creates a tab box with a tab per seeker holding its individual settings.
	private void CreateSeekersUi()
	{
		var worldSeekers = ComponentSystem.FindComponentsInWorld<ExperimentalSeeker>(true);
		if (worldSeekers.Length == 0)
			return;

		var group = new WidgetGroupBox("Seekers", 8, 8);
		sampleDescriptionWindow.MainWindow.AddChild(group, Gui.ALIGN_LEFT);

		seekersTabbox = new WidgetTabBox(4, 4);
		group.AddChild(seekersTabbox, Gui.ALIGN_EXPAND);

		for (int i = 0; i < worldSeekers.Length; i += 1)
		{
			var seeker = worldSeekers[i];

			string tabName = seeker.node.Name;
			if (string.IsNullOrEmpty(tabName))
				tabName = $"Seeker {i}";

			seekersTabbox.CurrentTab = seekersTabbox.AddTab(tabName);
			tabSeekers.Add(seeker);

			var grid = new WidgetGridBox(3, 5, 5);
			seekersTabbox.AddChild(grid, Gui.ALIGN_EXPAND);
			CreateSeekerTab(grid, seeker);
		}

		seekersTabbox.EventChanged.Connect(widgetConnections, () =>
		{
			HighlightSelectedSeeker();
		});

		if (seekersTabbox.NumTabs > 0)
			seekersTabbox.CurrentTab = 0;
		HighlightSelectedSeeker();
	}

	private void CreateSeekerTab(WidgetGridBox grid, ExperimentalSeeker seeker)
	{
		SampleWidgets.AddFloatParameter(widgetConnections, grid,
			"Movement speed",
			"The movement speed of the agent.",
			seeker.movementSpeed,
			0.0f,
			20.0f,
			(float value) =>
			{
				seeker.movementSpeed = value;
			}
		);

		SampleWidgets.AddSwitchParameter(widgetConnections, grid,
			"Straight mode",
			"Defines which points are included in the resulting path: only corners, corners with area crossings, or all polygon crossings.",
			seeker.straightMode,
			new string[] { "Corners", "Area crossings", "All crossings" },
			(int mode) =>
			{
				seeker.straightMode = mode;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddFloatParameter(widgetConnections, grid,
			"Heuristic scale",
			"The weight of the A* heuristic. Values close to 1 give the optimal path, higher values speed up the search at the cost of path optimality.",
			seeker.heuristicScale,
			0.0f,
			5.0f,
			(float value) =>
			{
				seeker.heuristicScale = value;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddFloatParameter(widgetConnections, grid,
			"Max cost",
			"The maximum allowed cost of the route. When the limit is reached, the path is built partially up to the last reachable point.",
			MathLib.Min(seeker.maxCost, 500.0f),
			0.0f,
			500.0f,
			(float value) =>
			{
				seeker.maxCost = value;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddIntParameter(widgetConnections, grid,
			"Max search nodes",
			"The maximum number of nodes visited during the path search. Low values may cause the search to stop before the target is reached.",
			MathLib.Min(seeker.maxSearchNodes, 16384),
			0,
			16384,
			(int value) =>
			{
				seeker.maxSearchNodes = value;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddFloatParameter(widgetConnections, grid,
			"Agent radius",
			"The agent radius used to select a suitable navigation mesh and keep the route away from boundaries.",
			seeker.agentRadius,
			0.0f,
			1.0f,
			(float value) =>
			{
				seeker.agentRadius = value;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddFloatParameter(widgetConnections, grid,
			"Agent height",
			"The agent height used to select a suitable navigation mesh.",
			seeker.agentHeight,
			0.0f,
			4.0f,
			(float value) =>
			{
				seeker.agentHeight = value;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddFloatParameter(widgetConnections, grid,
			"Max slope angle",
			"The maximum slope angle (in degrees) the agent can walk on.",
			seeker.maxSlopeAngle,
			0.0f,
			90.0f,
			(float value) =>
			{
				seeker.maxSlopeAngle = value;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddFloatParameter(widgetConnections, grid,
			"Max step height",
			"The maximum height of an obstacle the agent can step over.",
			seeker.maxStepHeight,
			0.0f,
			2.0f,
			(float value) =>
			{
				seeker.maxStepHeight = value;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddFloatParameter(widgetConnections, grid,
			"Snap size",
			"The search extents used to snap the route points onto the navigation mesh.",
			seeker.snapSize,
			0.0f,
			10.0f,
			(float value) =>
			{
				seeker.snapSize = value;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddBoolParameter(widgetConnections, grid,
			"Consider obstacles",
			"Toggles whether obstacles block the route.",
			seeker.obstacleMask != 0,
			(bool enabled) =>
			{
				seeker.obstacleMask = enabled ? ~0 : 0;
				seeker.ApplySettings();
			}
		);

		SampleWidgets.AddBoolParameter(widgetConnections, grid,
			"Show Seeker Debug",
			"Draws the route corridor and the velocities of this agent.",
			seeker.debugVisualizerEnabled,
			(bool enabled) =>
			{
				seeker.debugVisualizerEnabled = enabled;
			}
		);
	}

	// The route of the seeker selected in the tab box is drawn in green.
	private void HighlightSelectedSeeker()
	{
		int current = seekersTabbox ? seekersTabbox.CurrentTab : -1;
		for (int i = 0; i < tabSeekers.Count; i += 1)
		{
			if (i == current)
				tabSeekers[i].pathColor = selectedPathColor;
			else
				tabSeekers[i].pathColor = pathColor;
		}
	}
}
