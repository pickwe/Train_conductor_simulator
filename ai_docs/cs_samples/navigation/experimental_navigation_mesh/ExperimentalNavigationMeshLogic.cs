// Demonstrates pathfinding on an experimental navigation mesh using
// ExperimentalNavigationPathFetch. Start and end points can be moved via
// manipulators, pathfinding settings can be tweaked in the sample window.
// A complete path is drawn with the route color, a partial path is drawn up
// to the last reachable point with the remaining segment shown as a red line.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec3 = Unigine.dvec3;
#else
using Scalar = System.Single;
using Vec3 = Unigine.vec3;
#endif
#endregion

using Unigine;

public partial class ExperimentalNavigationMeshLogic : Component
{
	public Node pathStart = null;
	public Node pathEnd = null;
	public Node navigation = null;
	[ParameterColor]
	public vec4 routeColor = vec4.WHITE;

	private ExperimentalNavigationMesh navigationMesh;
	private ExperimentalNavigationMeshFilter filter;
	private ExperimentalNavigationPathFetch routeFetch;
	private ExperimentalNavigationPath route;

	// runtime-adjustable settings, pushed to the filter and the path fetch
	// by ApplySettings(); initial values are taken from the engine defaults
	// and the bake settings of the navigation mesh in InitNavigation()
	private ExperimentalNavigationPath.STRAIGHT_MODE straightMode;
	private float heuristicScale;
	private float maxCost;
	private int maxSearchNodes;
	private float agentRadius;
	private float agentHeight;
	private float maxSlopeAngle;
	private float maxStepHeight;
	private float snapSize;
	private bool considerObstacles;

	// sample visualization
	private bool showNavigationMesh = true;

	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	private Manipulators widgetManipulator;

	[MethodInit(Order = 2)]
	private void Init()
	{
		Visualizer.Enabled = true;
		Unigine.Console.Run("experimental_navigation_show_mesh 0");
		Unigine.Console.Run("experimental_navigation_show_mesh_mode 1");
		Unigine.Console.Run("experimental_navigation_show_mesh_depth_test 1");
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		InitNavigation();

		// manipulators move the start and end points, scaling is disabled
		widgetManipulator = GetComponent<Manipulators>(node);
		if (widgetManipulator != null)
		{
			widgetManipulator.XAxisScale = false;
			widgetManipulator.YAxisScale = false;
			widgetManipulator.ZAxisScale = false;
		}

		CreateUi();
	}

	private void Update()
	{
		// disable player controls while dragging manipulators
		Game.Player.Controlled = !widgetManipulator.Active;

		if (showNavigationMesh && navigationMesh)
			navigationMesh.RenderVisualizer();

		UpdateRoute();
		UpdateStatus();
	}

	private void Shutdown()
	{
		Visualizer.Enabled = false;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
		sampleDescriptionWindow.shutdown();

		// the native navigation objects are deleted with the component on the
		// main thread, the way the smart pointer members do it in C++
		route?.Dispose();
		route = null;
		routeFetch?.Dispose();
		routeFetch = null;
		filter?.Dispose();
		filter = null;
	}

	// Creates the navigation mesh filter and the path fetch restricted to the
	// navigation mesh assigned to the component.
	private void InitNavigation()
	{
		navigationMesh = navigation as ExperimentalNavigationMesh;

		filter = new ExperimentalNavigationMeshFilter();
		routeFetch = new ExperimentalNavigationPathFetch();
		routeFetch.NavigationMesh = navigationMesh;

		// start with the parameters the navigation mesh was baked with,
		// so the mesh matches the filter by default
		if (navigationMesh)
		{
			filter.NavigationMask = navigationMesh.NavigationMask;

			var bakeSettings = navigationMesh.BakeSettings;
			filter.AgentRadius = bakeSettings.AgentRadius;
			filter.AgentHeight = bakeSettings.AgentHeight;
			filter.MaxSlopeAngle = bakeSettings.MaxSlopeAngle;
			filter.MaxStepHeight = bakeSettings.MaxStepHeight;
		}

		straightMode = ExperimentalNavigationPath.STRAIGHT_MODE.ALL_CROSSINGS;
		heuristicScale = routeFetch.HeuristicScale;
		maxCost = routeFetch.MaxCost;
		maxSearchNodes = routeFetch.MaxSearchNodes;
		agentRadius = filter.AgentRadius;
		agentHeight = filter.AgentHeight;
		maxSlopeAngle = filter.MaxSlopeAngle;
		maxStepHeight = filter.MaxStepHeight;
		snapSize = filter.SnapSize.z;
		considerObstacles = filter.ObstacleMask != 0;

		ApplySettings();
	}

	// Pushes the current settings to the filter and the path fetch.
	private void ApplySettings()
	{
		filter.AgentRadius = agentRadius;
		filter.AgentHeight = agentHeight;
		filter.MaxSlopeAngle = maxSlopeAngle;
		filter.MaxStepHeight = maxStepHeight;
		filter.SnapSize = new vec3(snapSize);
		filter.ObstacleMask = considerObstacles ? ~0 : 0;

		routeFetch.Filter = filter;
		routeFetch.StraightMode = straightMode;
		routeFetch.HeuristicScale = heuristicScale;
		routeFetch.MaxCost = maxCost;
		routeFetch.MaxSearchNodes = maxSearchNodes;
	}

	// Computes the route between the start and end points and visualizes it.
	// A partial route is drawn up to the last reachable point with the
	// unreachable remainder shown as a red line.
	private void UpdateRoute()
	{
		if (!pathStart || !pathEnd)
			return;

		Vec3 start = pathStart.WorldPosition;
		Vec3 end = pathEnd.WorldPosition;

		routeFetch.FetchForce(start, end);
		route?.Dispose();
		route = routeFetch.TakePath();

		var status = route != null ? route.Status : ExperimentalNavigationPath.STATUS.NO_PATH;
		if (status == ExperimentalNavigationPath.STATUS.COMPLETE)
		{
			route.RenderVisualizer(routeColor);
		}
		else if (status == ExperimentalNavigationPath.STATUS.PARTIAL && route.NumPoints > 0)
		{
			route.RenderVisualizer(routeColor);
			Visualizer.RenderLine3D(route.GetPoint(route.NumPoints - 1), end, vec4.RED, Game.IFps);
		}
		else
			Visualizer.RenderLine3D(start, end, vec4.RED, Game.IFps);
	}

	private void CreateUi()
	{
		sampleDescriptionWindow.createWindow();

		sampleDescriptionWindow.addSwitchParameter(
			"Straight mode",
			"Defines which points are included in the resulting path: only corners, corners with area crossings, or all polygon crossings.",
			(int)straightMode,
			new string[] { "Corners", "Area crossings", "All crossings" },
			(int mode) =>
			{
				straightMode = (ExperimentalNavigationPath.STRAIGHT_MODE)mode;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addFloatParameter(
			"Heuristic scale",
			"The weight of the A* heuristic. Values close to 1 give the optimal path, higher values speed up the search at the cost of path optimality.",
			heuristicScale,
			0.0f,
			5.0f,
			(float value) =>
			{
				heuristicScale = value;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addFloatParameter(
			"Max cost",
			"The maximum allowed cost of the route. When the limit is reached, the path is built partially up to the last reachable point.",
			MathLib.Min(maxCost, 500.0f),
			0.0f,
			500.0f,
			(float value) =>
			{
				maxCost = value;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addIntParameter(
			"Max search nodes",
			"The maximum number of nodes visited during the path search. Low values may cause the search to stop before the target is reached.",
			MathLib.Min(maxSearchNodes, 16384),
			0,
			16384,
			(int value) =>
			{
				maxSearchNodes = value;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addParameterSpacer();

		sampleDescriptionWindow.addFloatParameter(
			"Agent radius",
			"The agent radius used to select a suitable navigation mesh and keep the route away from boundaries.",
			agentRadius,
			0.0f,
			5.0f,
			(float value) =>
			{
				agentRadius = value;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addFloatParameter(
			"Agent height",
			"The agent height used to select a suitable navigation mesh.",
			agentHeight,
			0.0f,
			10.0f,
			(float value) =>
			{
				agentHeight = value;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addFloatParameter(
			"Max slope angle",
			"The maximum slope angle (in degrees) the agent can walk on.",
			maxSlopeAngle,
			0.0f,
			90.0f,
			(float value) =>
			{
				maxSlopeAngle = value;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addFloatParameter(
			"Max step height",
			"The maximum height of an obstacle the agent can step over.",
			maxStepHeight,
			0.0f,
			2.0f,
			(float value) =>
			{
				maxStepHeight = value;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addFloatParameter(
			"Snap size",
			"The search extents used to snap the start and end points onto the navigation mesh.",
			snapSize,
			0.0f,
			10.0f,
			(float value) =>
			{
				snapSize = value;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addBoolParameter(
			"Consider obstacles",
			"Toggles whether obstacles block the route.",
			considerObstacles,
			(bool enabled) =>
			{
				considerObstacles = enabled;
				ApplySettings();
			}
		);

		sampleDescriptionWindow.addParameterSpacer();

		sampleDescriptionWindow.addBoolParameter(
			"Show Navigation Mesh",
			"Draws the navigation mesh colored by the areas of its polygons.",
			showNavigationMesh,
			(bool enabled) =>
			{
				showNavigationMesh = enabled;
			}
		);
	}

	private void UpdateStatus()
	{
		string status = "Route status: no path\n";
		if (route != null)
		{
			status = $"Route status: {ExperimentalNavigationUtils.GetRouteStatusName(route.Status)}\n" +
				$"Route length: {route.Length:0.00}\n" +
				$"Route cost: {route.Cost:0.00}\n" +
				$"Route points: {route.NumPoints}\n";

			if (route.FailureReason != ExperimentalNavigationPath.FAILURE.NONE)
				status += $"Failure reason: {ExperimentalNavigationUtils.GetRouteFailureName(route.FailureReason)}\n";
		}

		sampleDescriptionWindow.setStatus(status);
	}
}
