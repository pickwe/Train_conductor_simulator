// Interactive showcase of spatial queries on an experimental navigation mesh.
// The query point (and the target point for the boundary raycast) are moved
// via manipulators, the query mode and its parameters are selected in the
// sample window.
//
// The queries themselves are wrapped into the self-contained Query* functions
// below that depend only on the engine API, so any of them can be copied into
// a project together with its result struct. The component is only responsible
// for the sample UI and the visualization of the query results.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec3 = Unigine.dvec3;
#else
using Scalar = System.Single;
using Vec3 = Unigine.vec3;
using WorldBoundBox = Unigine.BoundBox;
#endif
#endregion

using System;
using System.Collections.Generic;
using Unigine;

public partial class ExperimentalNavigationMeshQueriesLogic : Component
{
	public Node queryPoint = null;
	public Node raycastTarget = null;
	public Node navigation = null;
	[ParameterColor]
	public vec4 queryColor = vec4.WHITE;

	private enum QUERY_MODE
	{
		NEAREST_POINT = 0,
		REACHABLE_POINTS,
		RANDOM_REACHABLE_POINTS,
		RANDOM_POINTS,
		NEAREST_BOUNDARY,
		BOUNDARY_RAYCAST,
		POLYGONS_IN_BOX,
	}

	// period of re-rolling the random point batches when auto refresh is enabled
	private const float RANDOM_REFRESH_INTERVAL = 0.5f;

	private ExperimentalNavigationMesh navigationMesh;
	private ExperimentalNavigationMeshFilter filter;

	// runtime-adjustable settings, pushed to the filter by ApplySettings();
	// initial values are taken from the engine defaults and the bake settings
	// of the navigation mesh in InitNavigation()
	private int queryMode = (int)QUERY_MODE.NEAREST_POINT;
	private float maxCost = 15.0f;
	private int numRandomPoints = 100;
	private bool autoRefresh = true;
	private float boundarySearchRadius = 5.0f;
	private float boxSize = 5.0f;
	private float agentRadius = 0.0f;
	private float agentHeight = 0.0f;
	private float snapSize = 0.0f;

	// sample visualization
	private bool showNavigationMesh = true;

	// cached batch for the random point modes; the batch is re-rolled when
	// the query point moves, a setting changes, or the refresh timer expires
	private List<Vec3> randomPoints = new List<Vec3>();
	private Vec3 lastQueryPosition = Vec3.ZERO;
	private float refreshTimer = 0.0f;
	private bool queriesDirty = true;

	// cached navigation mesh geometry used to highlight the mesh surface,
	// invalidated when the mesh tiles change
	private List<Vec3> meshVertices = new List<Vec3>();
	private List<int> meshIndices = new List<int>();
	private List<int> meshAreas = new List<int>();
	private bool meshGeometryDirty = true;
	private EventConnections meshConnections = new EventConnections();

	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	private Manipulators widgetManipulator;

	// per-mode parameter grids, only the grid of the current mode is shown
	private List<WidgetGridBox> modeGrids = new List<WidgetGridBox>();
	// refreshers for the widgets that show the same setting in several modes
	private List<Action> uiSync = new List<Action>();
	private EventConnections widgetConnections = new EventConnections();

	//////////////////////////////////////////////////////////////////////////
	// Navigation mesh queries.
	//
	// Self-contained input -> result wrappers around the query API of
	// ExperimentalNavigationMesh with no UI or visualization dependencies:
	// any of them can be copied into a project together with its result struct.
	//////////////////////////////////////////////////////////////////////////

	// Snaps a point onto the navigation mesh and reports the polygon it landed on.
	private struct NearestPointResult
	{
		public bool found;
		public Vec3 point;
		public long polygon;
	}

	private static NearestPointResult QueryNearestPoint(ExperimentalNavigationMesh mesh,
		ExperimentalNavigationMeshFilter filter, Vec3 position)
	{
		NearestPointResult result = new NearestPointResult();
		result.found = mesh.FindNearestPoint(filter, position, out result.point);
		if (result.found)
			result.polygon = mesh.FindNearestPolygon(filter, result.point);
		return result;
	}

	// Samples the points reachable from a point within the route cost limit.
	private struct ReachablePointsResult
	{
		public List<Vec3> points;
		public List<long> polygons;
	}

	private static ReachablePointsResult QueryReachablePoints(ExperimentalNavigationMesh mesh,
		ExperimentalNavigationMeshFilter filter, Vec3 position, float maxCost)
	{
		ReachablePointsResult result;
		result.points = new List<Vec3>();
		result.polygons = new List<long>();
		mesh.FindReachablePoints(filter, position, maxCost, result.points, result.polygons, 0);
		return result;
	}

	// Generates a batch of random points over the whole navigation mesh.
	private static List<Vec3> QueryRandomPoints(ExperimentalNavigationMesh mesh,
		ExperimentalNavigationMeshFilter filter, int count)
	{
		List<Vec3> points = new List<Vec3>();
		for (int i = 0; i < count; i += 1)
		{
			Vec3 point;
			if (mesh.FindRandomPoint(filter, out point))
				points.Add(point);
		}
		return points;
	}

	// Generates a batch of random points reachable from a point within the route
	// cost limit.
	private static List<Vec3> QueryRandomReachablePoints(ExperimentalNavigationMesh mesh,
		ExperimentalNavigationMeshFilter filter, Vec3 position, float maxCost, int count)
	{
		List<Vec3> points = new List<Vec3>();
		for (int i = 0; i < count; i += 1)
		{
			Vec3 point;
			if (mesh.FindRandomReachablePoint(filter, position, maxCost, out point, 0))
				points.Add(point);
		}
		return points;
	}

	// Finds the nearest navigation mesh boundary within the search radius.
	private struct NearestBoundaryResult
	{
		public bool found;
		public Vec3 point;
		public vec3 normal;
		public float distance;
	}

	private static NearestBoundaryResult QueryNearestBoundary(ExperimentalNavigationMesh mesh,
		ExperimentalNavigationMeshFilter filter, Vec3 position, float maxRadius)
	{
		NearestBoundaryResult result = new NearestBoundaryResult();
		result.found = mesh.GetNearestBoundary(filter, position, maxRadius, out result.point, out result.normal, out result.distance, 0);
		return result;
	}

	// Casts a segment along the navigation mesh surface and reports where it
	// crosses the mesh boundary. When nothing is hit, startOnMesh tells whether
	// the whole segment stays on the mesh or the start point is off the mesh.
	private struct BoundaryRaycastResult
	{
		public bool hit;
		public bool startOnMesh;
		public Vec3 point;
		public vec3 normal;
	}

	private static BoundaryRaycastResult QueryBoundaryRaycast(ExperimentalNavigationMesh mesh,
		ExperimentalNavigationMeshFilter filter, Vec3 from, Vec3 to)
	{
		BoundaryRaycastResult result = new BoundaryRaycastResult();
		result.hit = mesh.GetBoundaryIntersection(filter, from, to, out result.point, out result.normal, 0);
		result.startOnMesh = result.hit || mesh.IsPolygonValid(mesh.FindNearestPolygon(filter, from));
		return result;
	}

	// Collects the navigation mesh polygons overlapping a box around a point.
	private static List<long> QueryPolygonsInBox(ExperimentalNavigationMesh mesh,
		ExperimentalNavigationMeshFilter filter, Vec3 center, float boxSize)
	{
		Vec3 halfSize = new Vec3(boxSize * 0.5f);
		List<long> polygons = new List<long>();
		mesh.GetPolygons(filter, new WorldBoundBox(center - halfSize, center + halfSize), polygons);
		return polygons;
	}

	// Collects the triangles of the navigation mesh geometry (obtained via
	// ExperimentalNavigationMesh.GetTriangles) whose centroids fall inside a box
	// around a point; returns the numbers of the matching triangles.
	private static List<int> CollectTrianglesInBox(List<Vec3> vertices, List<int> indices,
		Vec3 center, float boxSize)
	{
		Vec3 boxMin = center - new Vec3(boxSize * 0.5f);
		Vec3 boxMax = center + new Vec3(boxSize * 0.5f);

		List<int> triangles = new List<int>();
		int numTriangles = indices.Count / 3;
		for (int i = 0; i < numTriangles; i += 1)
		{
			Vec3 centroid = (vertices[indices[i * 3 + 0]]
				+ vertices[indices[i * 3 + 1]]
				+ vertices[indices[i * 3 + 2]]) / (Scalar)3;

			if (centroid.x < boxMin.x || centroid.x > boxMax.x
				|| centroid.y < boxMin.y || centroid.y > boxMax.y
				|| centroid.z < boxMin.z || centroid.z > boxMax.z)
				continue;

			triangles.Add(i);
		}
		return triangles;
	}

	//////////////////////////////////////////////////////////////////////////
	// Sample logic.
	//////////////////////////////////////////////////////////////////////////

	[MethodInit(Order = 2)]
	private void Init()
	{
		Visualizer.Enabled = true;
		Unigine.Console.Run("experimental_navigation_show_mesh 0");
		Unigine.Console.Run("experimental_navigation_show_mesh_mode 1");
		Unigine.Console.Run("experimental_navigation_show_mesh_depth_test 1");
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		InitNavigation();

		// manipulators move the query points, scaling is disabled
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
		if (widgetManipulator != null)
			Game.Player.Controlled = !widgetManipulator.Active;

		if (!navigationMesh || !queryPoint)
		{
			sampleDescriptionWindow.setStatus("No navigation mesh or query point assigned.");
			return;
		}

		if (showNavigationMesh)
			navigationMesh.RenderVisualizer();

		// green marker at the exact position the queries are made from,
		// drawn on top so it is always visible
		Vec3 queryPosition = queryPoint.WorldPosition;
		Visualizer.RenderSolidBox(new vec3(0.5f, 0.5f, 0.04f), MathLib.Translate(queryPosition), vec4.GREEN, Game.IFps, false);

		// re-roll the random point batches when the query point moves
		// or periodically while auto refresh is enabled
		if ((queryPosition - lastQueryPosition).Length2 > 1e-8f)
			queriesDirty = true;
		lastQueryPosition = queryPosition;

		refreshTimer += Game.IFps;
		if (autoRefresh && refreshTimer >= RANDOM_REFRESH_INTERVAL)
			queriesDirty = true;

		string status = "";
		switch ((QUERY_MODE)queryMode)
		{
			case QUERY_MODE.NEAREST_POINT: status = UpdateNearestPoint(); break;
			case QUERY_MODE.REACHABLE_POINTS: status = UpdateReachablePoints(); break;
			case QUERY_MODE.RANDOM_REACHABLE_POINTS: status = UpdateRandomPoints(true); break;
			case QUERY_MODE.RANDOM_POINTS: status = UpdateRandomPoints(false); break;
			case QUERY_MODE.NEAREST_BOUNDARY: status = UpdateNearestBoundary(); break;
			case QUERY_MODE.BOUNDARY_RAYCAST: status = UpdateBoundaryRaycast(); break;
			case QUERY_MODE.POLYGONS_IN_BOX: status = UpdatePolygonsInBox(); break;
		}

		string header =
			$"Mesh polygons: {navigationMesh.NumPolygons}\n" +
			$"Walkable area: {navigationMesh.WalkableArea:0.0} m2\n" +
			$"Tiles loaded: {navigationMesh.NumLoadedTiles}/{navigationMesh.NumTiles}\n\n";

		sampleDescriptionWindow.setStatus(header + status);
	}

	private void Shutdown()
	{
		widgetConnections.DisconnectAll();
		meshConnections.DisconnectAll();
		Visualizer.Enabled = false;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
		sampleDescriptionWindow.shutdown();

		// the filter is deleted with the component on the main thread, the way
		// the smart pointer member does it in C++
		filter?.Dispose();
		filter = null;
	}

	// Creates the navigation mesh filter used by all queries.
	private void InitNavigation()
	{
		navigationMesh = navigation as ExperimentalNavigationMesh;

		filter = new ExperimentalNavigationMeshFilter();

		// start with the parameters the navigation mesh was baked with,
		// so the mesh matches the filter by default
		if (navigationMesh)
		{
			filter.NavigationMask = navigationMesh.NavigationMask;

			var bakeSettings = navigationMesh.BakeSettings;
			filter.AgentRadius = bakeSettings.AgentRadius;
			filter.AgentHeight = bakeSettings.AgentHeight;

			// the cached mesh geometry becomes stale when the tiles change
			navigationMesh.EventTilesChanged.Connect(meshConnections,
				(WorldBoundBox bounds) => { meshGeometryDirty = true; });
		}

		agentRadius = filter.AgentRadius;
		agentHeight = filter.AgentHeight;
		snapSize = filter.SnapSize.z;

		ApplySettings();
	}

	// Pushes the current settings to the filter.
	private void ApplySettings()
	{
		filter.AgentRadius = agentRadius;
		filter.AgentHeight = agentHeight;
		filter.SnapSize = new vec3(snapSize);

		queriesDirty = true;
	}

	private string UpdateNearestPoint()
	{
		Vec3 position = queryPoint.WorldPosition;

		NearestPointResult result = QueryNearestPoint(navigationMesh, filter, position);
		if (!result.found)
			return "Nearest point: not found, move the query point closer to the mesh or increase the snap size\n";

		Visualizer.RenderLine3D(position, result.point, queryColor, Game.IFps);
		Visualizer.RenderPoint3D(result.point, 0.3f, queryColor, false, 0.0f, false);

		string status =
			$"Nearest point: {result.point.x:0.00} {result.point.y:0.00} {result.point.z:0.00}\n" +
			$"Distance: {(result.point - position).Length:0.00}\n";

		status += "\n";
		status += GetPolygonInfo("Point polygon", result.polygon);
		return status;
	}

	// The reachable points are colored by the area of the polygon they belong to.
	private string UpdateReachablePoints()
	{
		Vec3 position = queryPoint.WorldPosition;

		ReachablePointsResult result = QueryReachablePoints(navigationMesh, filter, position, maxCost);

		for (int i = 0; i < result.points.Count; i += 1)
		{
			vec4 color = queryColor;
			if (i < result.polygons.Count)
			{
				color = ExperimentalNavigation.GetAreaColor(navigationMesh.GetPolygonAreaIndex(result.polygons[i]));
				color.w = 1.0f;
			}
			Visualizer.RenderPoint3D(result.points[i], 0.15f, color);
		}

		// the reference circle shows the theoretical maximum: the reachable
		// points stay inside it since the route cost is measured along the mesh
		// surface around obstacles and is scaled by the area costs
		vec4 circleColor = queryColor;
		circleColor.w = 0.4f;
		Visualizer.RenderCircle(maxCost, MathLib.Translate(position), circleColor, Game.IFps);

		return
			$"Reachable points: {result.points.Count}\n" +
			$"Cost limit: {maxCost:0.0}\n";
	}

	private string UpdateRandomPoints(bool reachableOnly)
	{
		Vec3 position = queryPoint.WorldPosition;

		if (queriesDirty)
		{
			randomPoints = reachableOnly
				? QueryRandomReachablePoints(navigationMesh, filter, position, maxCost, numRandomPoints)
				: QueryRandomPoints(navigationMesh, filter, numRandomPoints);
			queriesDirty = false;
			refreshTimer = 0.0f;
		}

		foreach (Vec3 point in randomPoints)
			Visualizer.RenderPoint3D(point, 0.15f, queryColor);

		string status =
			$"Points requested: {numRandomPoints}\n" +
			$"Points generated: {randomPoints.Count}\n";

		if (reachableOnly)
		{
			vec4 circleColor = queryColor;
			circleColor.w = 0.4f;
			Visualizer.RenderCircle(maxCost, MathLib.Translate(position), circleColor, Game.IFps);
			status += $"Cost limit: {maxCost:0.0}\n";
		}

		return status;
	}

	private string UpdateNearestBoundary()
	{
		Vec3 position = queryPoint.WorldPosition;

		vec4 circleColor = queryColor;
		circleColor.w = 0.4f;
		Visualizer.RenderCircle(boundarySearchRadius, MathLib.Translate(position), circleColor, Game.IFps);

		NearestBoundaryResult result = QueryNearestBoundary(navigationMesh, filter, position, boundarySearchRadius);
		if (!result.found)
			return "Nearest boundary: not found within the search radius\n";

		Visualizer.RenderLine3D(position, result.point, queryColor, Game.IFps);
		Visualizer.RenderPoint3D(result.point, 0.3f, vec4.RED, false, 0.0f, false);
		Visualizer.RenderVector(result.point, result.point + new Vec3(result.normal), vec4.BLUE, 0.25f, false, Game.IFps);

		return
			$"Nearest boundary: {result.point.x:0.00} {result.point.y:0.00} {result.point.z:0.00}\n" +
			$"Distance: {result.distance:0.00}\n" +
			$"Normal: {result.normal.x:0.00} {result.normal.y:0.00} {result.normal.z:0.00}\n";
	}

	private string UpdateBoundaryRaycast()
	{
		if (!raycastTarget)
			return "Assign the raycast target node to use this mode.\n";

		Vec3 from = queryPoint.WorldPosition;
		Vec3 to = raycastTarget.WorldPosition;

		BoundaryRaycastResult result = QueryBoundaryRaycast(navigationMesh, filter, from, to);
		if (result.hit)
		{
			// walkable part of the segment with the query color,
			// the part beyond the boundary in red
			Visualizer.RenderLine3D(from, result.point, queryColor, Game.IFps);
			Visualizer.RenderLine3D(result.point, to, vec4.RED, Game.IFps);
			Visualizer.RenderPoint3D(result.point, 0.3f, vec4.RED, false, 0.0f, false);
			Visualizer.RenderVector(result.point, result.point + new Vec3(result.normal), vec4.BLUE, 0.25f, false, Game.IFps);

			return
				"Boundary hit: yes\n" +
				$"Hit point: {result.point.x:0.00} {result.point.y:0.00} {result.point.z:0.00}\n" +
				$"Hit distance: {(result.point - from).Length:0.00}\n" +
				$"Normal: {result.normal.x:0.00} {result.normal.y:0.00} {result.normal.z:0.00}\n";
		}

		if (result.startOnMesh)
		{
			Visualizer.RenderLine3D(from, to, queryColor, Game.IFps);
			return "Boundary hit: no, the segment does not cross the mesh boundary\n";
		}

		Visualizer.RenderLine3D(from, to, vec4.RED, Game.IFps);
		return "Boundary hit: no, the start point is outside the navigation mesh\n";
	}

	// The polygons are queried by box overlap, the mesh surface inside the box is
	// highlighted with the cached mesh triangles colored by their areas.
	private string UpdatePolygonsInBox()
	{
		Vec3 position = queryPoint.WorldPosition;

		List<long> polygons = QueryPolygonsInBox(navigationMesh, filter, position, boxSize);

		if (meshGeometryDirty)
		{
			meshVertices.Clear();
			meshIndices.Clear();
			meshAreas.Clear();
			navigationMesh.GetTriangles(meshVertices, meshIndices, meshAreas);
			meshGeometryDirty = false;
		}

		// sample-grade visualization: the whole triangle list is filtered every
		// frame, which is fine for a sample-sized mesh
		List<int> triangles = CollectTrianglesInBox(meshVertices, meshIndices, position, boxSize);
		Vec3 lift = new Vec3(0.0f, 0.0f, 0.03f);
		foreach (int triangle in triangles)
		{
			vec4 color = vec4.GREEN;
			if (triangle < meshAreas.Count)
				color = ExperimentalNavigation.GetAreaColor(meshAreas[triangle]);
			color.w = 0.45f;

			Visualizer.RenderTriangle3D(
				meshVertices[meshIndices[triangle * 3 + 0]] + lift,
				meshVertices[meshIndices[triangle * 3 + 1]] + lift,
				meshVertices[meshIndices[triangle * 3 + 2]] + lift,
				color, Game.IFps);
		}

		Visualizer.RenderBoundBox(new BoundBox(new vec3(-boxSize * 0.5f), new vec3(boxSize * 0.5f)), MathLib.Translate(position), queryColor, Game.IFps);

		// the counts may differ: polygons are queried by box overlap while the
		// triangles are highlighted by their centroids being inside the box
		string status =
			$"Polygons in box: {polygons.Count}\n" +
			$"Highlighted triangles: {triangles.Count}\n" +
			$"Box size: {boxSize:0.0}\n";

		// only the polygon nearest to the query point is detailed,
		// there is no aggregate info for the whole box
		status += "\n";
		status += GetPolygonInfo("Nearest polygon to the query point", navigationMesh.FindNearestPolygon(filter, position));
		return status;
	}

	// Formats a titled block with the polygon id, its area (with the cost from
	// the area registry) and flags including their registered names.
	private string GetPolygonInfo(string title, long polygon)
	{
		if (!navigationMesh.IsPolygonValid(polygon))
			return $"{title}: none\n";

		int areaIndex = navigationMesh.GetPolygonAreaIndex(polygon);
		int flags = navigationMesh.GetPolygonFlags(polygon);

		string flagNames = "";
		for (int i = 0; i < ExperimentalNavigation.NumFlags; i += 1)
		{
			if ((flags & (1 << i)) == 0)
				continue;
			string name = ExperimentalNavigation.GetFlagName(i);
			if (!string.IsNullOrEmpty(name))
			{
				if (flagNames.Length > 0)
					flagNames += ", ";
				flagNames += name;
			}
		}

		string flagsText = $"0x{flags:X}";
		if (flagNames.Length > 0)
			flagsText += $" [{flagNames}]";

		return
			$"{title}: {polygon}\n" +
			$"  Area: {areaIndex} \"{ExperimentalNavigation.GetAreaName(areaIndex)}\" (cost {ExperimentalNavigation.GetAreaCost(areaIndex):0.00})\n" +
			$"  Flags: {flagsText}\n";
	}

	//////////////////////////////////////////////////////////////////////////
	// Sample UI.
	//////////////////////////////////////////////////////////////////////////

	private void CreateUi()
	{
		sampleDescriptionWindow.createWindow();
		var window = sampleDescriptionWindow.MainWindow;

		// query group: the mode selector followed by the parameter grids of all
		// modes, only the grid of the current mode is visible
		var queryGroup = new WidgetGroupBox("Query", 8, 8);
		window.AddChild(queryGroup, Gui.ALIGN_LEFT);

		var modeGrid = new WidgetGridBox(3, 5, 5);
		queryGroup.AddChild(modeGrid, Gui.ALIGN_EXPAND);
		SampleWidgets.AddSwitchParameter(widgetConnections, modeGrid,
			"Query mode",
			"The spatial query performed at the query point and visualized.",
			queryMode,
			new string[] { "Nearest point", "Reachable points", "Random reachable points", "Random points", "Nearest boundary", "Boundary raycast", "Polygons in box" },
			(int mode) =>
			{
				queryMode = mode;
				queriesDirty = true;
				UpdateModeUi();
			});

		for (int i = 0; i <= (int)QUERY_MODE.POLYGONS_IN_BOX; i += 1)
		{
			var grid = new WidgetGridBox(3, 5, 5);
			queryGroup.AddChild(grid, Gui.ALIGN_EXPAND);
			modeGrids.Add(grid);
		}

		// the parameters shared by several modes get a widget in the grid of each
		// mode using them; the widgets are refreshed from the variables on a mode
		// switch by UpdateModeUi()
		string maxCostTooltip = "The route cost limit of the query. Equals the distance while all area costs are 1.";
		string countTooltip = "The number of points generated by the query.";
		string refreshTooltip = "Re-rolls the random point batch every half a second. The batch is always re-rolled when the query point moves or a setting changes.";

		foreach (int mode in new int[] { (int)QUERY_MODE.REACHABLE_POINTS, (int)QUERY_MODE.RANDOM_REACHABLE_POINTS })
		{
			var slider = SampleWidgets.AddFloatParameter(widgetConnections, modeGrids[mode],
				"Max cost", maxCostTooltip, maxCost, 0.0f, 100.0f,
				(float value) =>
				{
					maxCost = value;
					queriesDirty = true;
				});
			uiSync.Add(() => { slider.Value = (int)(maxCost * 100.0f); });
		}

		foreach (int mode in new int[] { (int)QUERY_MODE.RANDOM_REACHABLE_POINTS, (int)QUERY_MODE.RANDOM_POINTS })
		{
			var slider = SampleWidgets.AddIntParameter(widgetConnections, modeGrids[mode],
				"Random points", countTooltip, numRandomPoints, 1, 500,
				(int value) =>
				{
					numRandomPoints = value;
					queriesDirty = true;
				});
			uiSync.Add(() => { slider.Value = numRandomPoints; });

			var checkbox = SampleWidgets.AddBoolParameter(widgetConnections, modeGrids[mode],
				"Auto refresh", refreshTooltip, autoRefresh,
				(bool enabled) =>
				{
					autoRefresh = enabled;
				});
			uiSync.Add(() => { checkbox.Checked = autoRefresh; });
		}

		SampleWidgets.AddFloatParameter(widgetConnections, modeGrids[(int)QUERY_MODE.NEAREST_BOUNDARY],
			"Boundary radius",
			"The search radius for the nearest boundary query.",
			boundarySearchRadius, 0.1f, 20.0f,
			(float value) =>
			{
				boundarySearchRadius = value;
			});

		SampleWidgets.AddFloatParameter(widgetConnections, modeGrids[(int)QUERY_MODE.POLYGONS_IN_BOX],
			"Box size",
			"The size of the box for the polygons in box query.",
			boxSize, 0.1f, 20.0f,
			(float value) =>
			{
				boxSize = value;
			});

		// filter group: the settings shared by all query modes, always visible
		var filterGroup = new WidgetGroupBox("Filter", 8, 8);
		window.AddChild(filterGroup, Gui.ALIGN_LEFT);

		var filterGrid = new WidgetGridBox(3, 5, 5);
		filterGroup.AddChild(filterGrid, Gui.ALIGN_EXPAND);

		SampleWidgets.AddFloatParameter(widgetConnections, filterGrid,
			"Agent radius",
			"The agent radius used to select a suitable navigation mesh.",
			agentRadius, 0.0f, 5.0f,
			(float value) =>
			{
				agentRadius = value;
				ApplySettings();
			});

		SampleWidgets.AddFloatParameter(widgetConnections, filterGrid,
			"Agent height",
			"The agent height used to select a suitable navigation mesh.",
			agentHeight, 0.0f, 10.0f,
			(float value) =>
			{
				agentHeight = value;
				ApplySettings();
			});

		SampleWidgets.AddFloatParameter(widgetConnections, filterGrid,
			"Snap size",
			"The search extents used to snap the query points onto the navigation mesh.",
			snapSize, 0.0f, 10.0f,
			(float value) =>
			{
				snapSize = value;
				ApplySettings();
			});

		// visualization group: shared by all query modes, always visible
		var visualizationGroup = new WidgetGroupBox("Visualization", 8, 8);
		window.AddChild(visualizationGroup, Gui.ALIGN_LEFT);

		SampleWidgets.AddBoolParameter(widgetConnections, visualizationGroup,
			"Show Navigation Mesh",
			"Draws the navigation mesh colored by the areas of its polygons.",
			showNavigationMesh,
			(bool enabled) =>
			{
				showNavigationMesh = enabled;
			});

		UpdateModeUi();
	}

	// Shows only the parameter grid of the current query mode and refreshes the
	// widgets duplicated between the modes.
	private void UpdateModeUi()
	{
		foreach (var sync in uiSync)
			sync();

		for (int i = 0; i < modeGrids.Count; i += 1)
			modeGrids[i].Hidden = i != queryMode;

		sampleDescriptionWindow.MainWindow.Arrange();
	}
}
