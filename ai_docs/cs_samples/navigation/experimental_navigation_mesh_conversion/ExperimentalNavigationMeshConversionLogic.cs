// Demonstrates the interoperation between the classic NavigationMesh and the
// experimental navigation system: both nodes are baked from the scene by the
// new baker sharing a common settings block, the classic mesh data can be
// converted into the experimental format and back, and the data can be
// exchanged through a .mesh file.
//
// The conversion operations are self-contained functions that depend only on
// the engine API, so any of them can be copied into a project.

using Unigine;

public partial class ExperimentalNavigationMeshConversionLogic : Component
{
	public Node navigationMesh = null;
	public Node experimentalNavigationMesh = null;

	// default directory of the .mesh file dialogs, relative to the data path
	private const string DEFAULT_DIR = "csharp_component_samples/navigation/experimental_navigation_mesh_conversion/";

	// the classic node is baked into its own separate asset, so the original
	// asset shared with the other samples is left intact
	private const string CLASSIC_BAKED_MESH_FILE = "csharp_component_samples/navigation/experimental_navigation_mesh_conversion/navigation_mesh_baked.mesh";

	private NavigationMesh classicMesh;
	private ExperimentalNavigationMesh experimentalMesh;

	// shared bake settings, pushed to both nodes right before a bake:
	// writing them earlier would invalidate the data loaded from the assets
	private float agentRadius;
	private float agentHeight;
	private float maxSlopeAngle;
	private float maxStepHeight;
	private float cellSize;
	private float cellHeight;
	private int tileResolution;
	private int partitioning;
	private float regionMinSize;
	private float regionMergeSize;
	private float maxEdgeError;
	private float detailSampleDistance;
	private float detailSampleError;
	private bool showBakeBounds = true;

	private ExperimentalNavigationBakeQuery bakeQuery;
	private string lastAction = "";

	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	private Manipulators widgetManipulator;
	private WidgetEditLine savePathEditline;
	private WidgetEditLine loadPathEditline;
	private WidgetDialogFile saveDialog;
	private WidgetDialogFile loadDialog;
	private EventConnections widgetConnections = new EventConnections();

	//////////////////////////////////////////////////////////////////////////
	// Conversion operations: no UI dependencies, can be copied into a project.
	//////////////////////////////////////////////////////////////////////////

	// Bakes the experimental navigation mesh from the scene geometry.
	private static ExperimentalNavigationBakeQuery BakeExperimentalFromScene(ExperimentalNavigationMesh experimental)
	{
		return ExperimentalBakeNavigation.BakeAsync(experimental);
	}

	// Bakes the classic navigation mesh from the scene geometry with the new
	// baker. The node is repointed to its own asset and cleared first, so the
	// original asset shared with the other samples stays intact.
	private static ExperimentalNavigationBakeQuery BakeClassicWithNewBaker(NavigationMesh classic)
	{
		classic.SetMeshPath(CLASSIC_BAKED_MESH_FILE);
		using Mesh emptyMesh = new Mesh();
		classic.SetMesh(emptyMesh);
		return ExperimentalBakeNavigation.BakeAsync(classic);
	}

	// Converts the classic navigation mesh data into the experimental format:
	// the polygon mesh of the classic node is spawned as temporary geometry
	// aligned with the experimental node and baked with a dedicated bake mask,
	// keeping the rest of the scene out of the bake. The agent radius is zeroed
	// for this bake since the classic mesh is already shrunk by it.
	private static bool ConvertClassicToExperimental(NavigationMesh classic, ExperimentalNavigationMesh experimental)
	{
		using Mesh polygonMesh = new Mesh();
		if (classic.GetMesh(polygonMesh) == 0)
			return false;

		ObjectMeshStatic geometry = new ObjectMeshStatic();
		geometry.SetMeshProceduralMode(ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC, 0);
		geometry.ApplyCopyMeshProceduralForce(polygonMesh);
		geometry.Immovable = false;
		geometry.WorldTransform = experimental.WorldTransform;

		int tempBakeMask = 1 << 30;
		for (int i = 0; i < geometry.NumSurfaces; i += 1)
		{
			geometry.SetExperimentalNavigation(true, i);
			geometry.SetExperimentalNavigationBakeMask(tempBakeMask, i);
		}

		// make the freshly created geometry visible to the baker
		World.UpdateSpatial();

		int oldBakeMask = experimental.BakeSettings.BakeMask;
		float oldAgentRadius = experimental.BakeSettings.AgentRadius;
		experimental.BakeSettings.BakeMask = tempBakeMask;
		experimental.BakeSettings.AgentRadius = 0.0f;

		bool baked = ExperimentalBakeNavigation.BakeForce(experimental);

		experimental.BakeSettings.BakeMask = oldBakeMask;
		experimental.BakeSettings.AgentRadius = oldAgentRadius;
		geometry.DeleteLater();

		return baked;
	}

	// Applies the experimental navigation mesh data to the classic node in
	// memory through the extracted polygon mesh.
	private static bool ApplyExperimentalToClassic(ExperimentalNavigationMesh experimental, NavigationMesh classic)
	{
		using Mesh polygonMesh = new Mesh();
		if (!experimental.GetMesh(polygonMesh))
			return false;

		return classic.SetMesh(polygonMesh) != 0;
	}

	// Saves the polygon mesh of the experimental navigation mesh to a .mesh
	// file; the mesh is stored in the local space of the node.
	private static bool SaveExperimentalMesh(ExperimentalNavigationMesh experimental, string path)
	{
		using Mesh polygonMesh = new Mesh();
		if (!experimental.GetMesh(polygonMesh))
			return false;
		return polygonMesh.Save(path) != 0;
	}

	// Loads a .mesh file into the classic navigation mesh.
	private static bool LoadClassicMesh(NavigationMesh classic, string path)
	{
		return classic.LoadMesh(path);
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

		// manipulators move the navigation mesh nodes, scaling is disabled
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

		if (!classicMesh || !experimentalMesh)
		{
			sampleDescriptionWindow.setStatus("Assign both the classic and the experimental navigation mesh nodes.");
			return;
		}

		// both navigation meshes are always visualized, so the results of the
		// operations can be compared side by side
		classicMesh.RenderVisualizer();
		experimentalMesh.RenderVisualizer();

		// bake bounds of each node, colored to match its mesh visualization
		if (showBakeBounds)
		{
			Visualizer.RenderBox(classicMesh.BakeSettings.BakeSize, classicMesh.WorldTransform, vec4.GREEN, Game.IFps);
			Visualizer.RenderBox(experimentalMesh.BakeSettings.BakeSize, experimentalMesh.WorldTransform, vec4.BLUE, Game.IFps);
		}

		// pick up the result of an asynchronous bake
		if (bakeQuery != null && bakeQuery.IsReady)
		{
			if (bakeQuery.Status == ExperimentalNavigationBakeQuery.STATUS.READY)
				lastAction = $"Bake finished in {bakeQuery.ElapsedTime:0.0} s, tiles built: {bakeQuery.NumBuiltTiles}.";
			else
				lastAction = $"Bake failed, failed tiles: {bakeQuery.NumFailedTiles}, failed meshes: {bakeQuery.NumFailedMeshes}.";
			bakeQuery.Dispose();
			bakeQuery = null;
		}

		UpdateStatus();
	}

	private void Shutdown()
	{
		if (saveDialog)
			saveDialog.DeleteLater();
		if (loadDialog)
			loadDialog.DeleteLater();
		widgetConnections.DisconnectAll();
		Visualizer.Enabled = false;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
		sampleDescriptionWindow.shutdown();

		// the pending bake query is released with the component on the main thread
		bakeQuery?.Dispose();
		bakeQuery = null;
	}

	// Reads the initial values of the shared settings block from the nodes.
	private void InitNavigation()
	{
		classicMesh = navigationMesh as NavigationMesh;
		experimentalMesh = experimentalNavigationMesh as ExperimentalNavigationMesh;

		var settings = experimentalMesh ? experimentalMesh.BakeSettings : null;
		if (settings == null)
			return;

		agentRadius = settings.AgentRadius;
		agentHeight = settings.AgentHeight;
		maxSlopeAngle = settings.MaxSlopeAngle;
		maxStepHeight = settings.MaxStepHeight;
		cellSize = settings.CellSize;
		cellHeight = settings.CellHeight;
		tileResolution = settings.TileResolution;
		partitioning = (int)settings.Partitioning;
		regionMinSize = settings.RegionMinSize;
		regionMergeSize = settings.RegionMergeSize;
		maxEdgeError = settings.MaxEdgeError;
		detailSampleDistance = settings.DetailSampleDistance;
		detailSampleError = settings.DetailSampleError;
	}

	// Pushes the shared settings to both nodes right before a bake;
	// the bake size stays per-node.
	private void ApplyBakeSettings()
	{
		ExperimentalNavigationBakeSettings[] allSettings = {
			classicMesh ? classicMesh.BakeSettings : null,
			experimentalMesh ? experimentalMesh.BakeSettings : null,
		};

		foreach (var settings in allSettings)
		{
			if (settings == null)
				continue;
			settings.AgentRadius = agentRadius;
			settings.AgentHeight = agentHeight;
			settings.MaxSlopeAngle = maxSlopeAngle;
			settings.MaxStepHeight = maxStepHeight;
			settings.CellSize = cellSize;
			settings.CellHeight = cellHeight;
			settings.TileResolution = tileResolution;
			settings.Partitioning = (ExperimentalNavigationBakeSettings.PARTITIONING)partitioning;
			settings.RegionMinSize = regionMinSize;
			settings.RegionMergeSize = regionMergeSize;
			settings.MaxEdgeError = maxEdgeError;
			settings.DetailSampleDistance = detailSampleDistance;
			settings.DetailSampleError = detailSampleError;
		}
	}

	private void OnBakeExperimental()
	{
		if (!experimentalMesh)
			return;
		if (ExperimentalBakeNavigation.IsBaking)
		{
			lastAction = "The baker is busy, wait for the current bake to finish.";
			return;
		}

		ApplyBakeSettings();
		bakeQuery = BakeExperimentalFromScene(experimentalMesh);
		lastAction = "Baking the experimental navigation mesh from the scene...";
	}

	private void OnConvertFromClassic()
	{
		if (!classicMesh || !experimentalMesh)
			return;
		if (ExperimentalBakeNavigation.IsBaking)
		{
			lastAction = "The baker is busy, wait for the current bake to finish.";
			return;
		}

		ApplyBakeSettings();
		if (ConvertClassicToExperimental(classicMesh, experimentalMesh))
			lastAction = "The classic navigation mesh is converted into the experimental format.";
		else
			lastAction = "Conversion failed: the classic navigation mesh has no data or the bake failed.";
	}

	private void OnBakeClassic()
	{
		if (!classicMesh)
			return;
		if (ExperimentalBakeNavigation.IsBaking)
		{
			lastAction = "The baker is busy, wait for the current bake to finish.";
			return;
		}

		ApplyBakeSettings();
		bakeQuery = BakeClassicWithNewBaker(classicMesh);
		lastAction = "Baking the classic navigation mesh from the scene with the new baker...";
	}

	private void OnApplyFromExperimental()
	{
		if (!classicMesh || !experimentalMesh)
			return;

		if (ApplyExperimentalToClassic(experimentalMesh, classicMesh))
			lastAction = "The experimental navigation mesh is applied to the classic node.";
		else
			lastAction = "Applying failed: the experimental navigation mesh has no data.";
	}

	// Creates both file dialogs hidden and pointed at the default directory.
	// The work is done in the OK button callbacks, closing just hides the dialog.
	private void CreateFileDialogs()
	{
		// saving the experimental navigation mesh
		saveDialog = new WidgetDialogFile("Save navigation mesh");
		saveDialog.Filter = ".mesh";
		saveDialog.Path = Engine.DataPath + DEFAULT_DIR;

		saveDialog.GetOkButton().EventClicked.Connect(widgetConnections, () =>
		{
			string path = saveDialog.File;
			if (!path.EndsWith(".mesh"))
			{
				path += ".mesh";
				saveDialog.File = path;
			}
			if (savePathEditline)
				savePathEditline.Text = path;

			var split = path.Split('/');
			var filename = split[split.Length - 1];

			if (experimentalMesh && SaveExperimentalMesh(experimentalMesh, path))
				lastAction = $"The experimental navigation mesh is saved to \"{filename}\".";
			else
				lastAction = "Saving failed: the experimental navigation mesh has no data or the path is not writable.";

			saveDialog.RemoveFocus();
			saveDialog.Hidden = true;
		});

		void CloseSave()
		{
			saveDialog.RemoveFocus();
			saveDialog.Hidden = true;
		}
		saveDialog.GetCancelButton().EventClicked.Connect(widgetConnections, CloseSave);
		saveDialog.GetCloseButton().EventClicked.Connect(widgetConnections, CloseSave);

		WindowManager.MainWindow.AddChild(saveDialog, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);
		saveDialog.Hidden = true;

		// loading a .mesh file into the classic navigation mesh
		loadDialog = new WidgetDialogFile("Load navigation mesh");
		loadDialog.Filter = ".mesh";
		loadDialog.Path = Engine.DataPath + DEFAULT_DIR;

		loadDialog.GetOkButton().EventClicked.Connect(widgetConnections, () =>
		{
			string path = loadDialog.File;
			if (loadPathEditline)
				loadPathEditline.Text = path;

			var split = path.Split('/');
			var filename = split[split.Length - 1];

			if (!FileSystem.IsFileExist(path))
				lastAction = $"Loading failed: \"{filename}\" is not a valid asset.";
			else if (classicMesh && LoadClassicMesh(classicMesh, path))
				lastAction = $"The classic navigation mesh is loaded from \"{filename}\".";
			else
				lastAction = $"Loading failed: cannot read \"{filename}\".";

			loadDialog.RemoveFocus();
			loadDialog.Hidden = true;
		});

		void CloseLoad()
		{
			loadDialog.RemoveFocus();
			loadDialog.Hidden = true;
		}
		loadDialog.GetCancelButton().EventClicked.Connect(widgetConnections, CloseLoad);
		loadDialog.GetCloseButton().EventClicked.Connect(widgetConnections, CloseLoad);

		WindowManager.MainWindow.AddChild(loadDialog, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);
		loadDialog.Hidden = true;
	}

	private void UpdateStatus()
	{
		string status = "";

		if (experimentalMesh)
		{
			string rebake = experimentalMesh.IsNeedBake ? ", needs re-bake" : "";
			status += $"Experimental mesh: {experimentalMesh.NumPolygons} polygons, walkable area {experimentalMesh.WalkableArea:0.0} m2{rebake}\n";
		}

		if (bakeQuery != null)
		{
			status += $"Baking: {bakeQuery.Progress * 100.0f:0}%, tiles built: {bakeQuery.NumBuiltTiles}, pending: {bakeQuery.NumPendingTiles}\n";
		}

		if (lastAction.Length > 0)
		{
			status += "\n";
			status += lastAction;
			status += "\n";
		}

		sampleDescriptionWindow.setStatus(status);
	}

	//////////////////////////////////////////////////////////////////////////
	// Sample UI.
	//////////////////////////////////////////////////////////////////////////

	private void CreateUi()
	{
		sampleDescriptionWindow.createWindow(Gui.ALIGN_LEFT, 600);
		var window = sampleDescriptionWindow.MainWindow;

		// shared bake settings driving both bakers; the values are pushed to the
		// nodes right before a bake, so tweaking them does not invalidate the
		// currently loaded data
		{
			var group = new WidgetGroupBox("Bake settings", 8, 8);
			window.AddChild(group, Gui.ALIGN_LEFT);

			var grid = new WidgetGridBox(3, 5, 5);
			group.AddChild(grid, Gui.ALIGN_EXPAND);

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Agent radius",
				"The agent radius: the walkable surface is shrunk from the boundaries by this distance.",
				agentRadius, 0.0f, 2.0f,
				(float value) => { agentRadius = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Agent height",
				"The agent height: areas with a lower ceiling are excluded from the walkable surface.",
				agentHeight, 0.0f, 5.0f,
				(float value) => { agentHeight = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Max slope angle",
				"The maximum slope angle (in degrees) considered walkable.",
				maxSlopeAngle, 0.0f, 90.0f,
				(float value) => { maxSlopeAngle = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Max step height",
				"The maximum height of a ledge the agent can step over.",
				maxStepHeight, 0.0f, 2.0f,
				(float value) => { maxStepHeight = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Cell size",
				"The size of the voxelization cell in the horizontal plane. Smaller cells give a more precise mesh at the cost of the bake time.",
				cellSize, 0.01f, 1.0f,
				(float value) => { cellSize = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Cell height",
				"The height of the voxelization cell.",
				cellHeight, 0.01f, 1.0f,
				(float value) => { cellHeight = value; });

			SampleWidgets.AddIntParameter(widgetConnections, grid, "Tile resolution",
				"The size of a navigation mesh tile in cells.",
				tileResolution, 16, 256,
				(int value) => { tileResolution = value; });

			SampleWidgets.AddSwitchParameter(widgetConnections, grid, "Partitioning",
				"The region partitioning algorithm: watershed gives the best quality, monotone is the fastest, chunky is a tradeoff for tiled meshes.",
				partitioning, new string[] { "Watershed", "Monotone", "Chunky" },
				(int value) => { partitioning = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Region min size",
				"Isolated walkable regions smaller than this size are removed.",
				regionMinSize, 0.0f, 50.0f,
				(float value) => { regionMinSize = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Region merge size",
				"Walkable regions smaller than this size are merged with the neighbors when possible.",
				regionMergeSize, 0.0f, 100.0f,
				(float value) => { regionMergeSize = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Max edge error",
				"The maximum deviation of the simplified mesh boundary from the raw contour.",
				maxEdgeError, 0.0f, 5.0f,
				(float value) => { maxEdgeError = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Detail distance",
				"The sampling distance of the detail elevation mesh.",
				detailSampleDistance, 0.0f, 16.0f,
				(float value) => { detailSampleDistance = value; });

			SampleWidgets.AddFloatParameter(widgetConnections, grid, "Detail error",
				"The maximum elevation error of the detail mesh.",
				detailSampleError, 0.0f, 5.0f,
				(float value) => { detailSampleError = value; });

			SampleWidgets.AddBoolParameter(widgetConnections, grid, "Show bake bounds",
				"Draws the bake bounds of each navigation mesh: green for the classic node, blue for the experimental one.",
				showBakeBounds,
				(bool enabled) => { showBakeBounds = enabled; });
		}

		// operations on the navigation meshes, one tab per mesh
		var operationsTabbox = new WidgetTabBox(4, 4);
		window.AddChild(operationsTabbox, Gui.ALIGN_EXPAND);

		// operations on the experimental navigation mesh
		{
			operationsTabbox.CurrentTab = operationsTabbox.AddTab("Experimental Navigation");

			var tab = new WidgetVBox(4, 4);
			operationsTabbox.AddChild(tab, Gui.ALIGN_EXPAND);

			SampleWidgets.AddButton(widgetConnections, tab, "Bake from scene",
				"Bakes the experimental navigation mesh from the scene geometry asynchronously.",
				() => { OnBakeExperimental(); });

			SampleWidgets.AddButton(widgetConnections, tab, "Convert from classic",
				"Bakes the experimental navigation mesh over the polygon mesh of the classic node, converting its data into the experimental format.",
				() => { OnConvertFromClassic(); });

			var grid = new WidgetGridBox(3, 5, 5);
			tab.AddChild(grid, Gui.ALIGN_EXPAND);
			savePathEditline = SampleWidgets.AddStringFieldWithButton(widgetConnections, grid, "Mesh file",
				"The path of the .mesh file the experimental navigation mesh is saved to.",
				DEFAULT_DIR,
				"Save", "Opens the file dialog to choose where to save the polygon mesh extracted from the experimental navigation mesh.",
				() =>
				{
					saveDialog.Hidden = false;
					saveDialog.SetPermanentFocus();
				});
		}

		// operations on the classic navigation mesh
		{
			operationsTabbox.CurrentTab = operationsTabbox.AddTab("Classic Navigation");

			var tab = new WidgetVBox(4, 4);
			operationsTabbox.AddChild(tab, Gui.ALIGN_EXPAND);

			SampleWidgets.AddButton(widgetConnections, tab, "Bake with new baker",
				"Bakes the classic navigation mesh from the scene geometry with the new baker.",
				() => { OnBakeClassic(); });

			SampleWidgets.AddButton(widgetConnections, tab, "Apply from experimental",
				"Sets the polygon mesh extracted from the experimental navigation mesh to the classic node directly in memory.",
				() => { OnApplyFromExperimental(); });

			var grid = new WidgetGridBox(3, 5, 5);
			tab.AddChild(grid, Gui.ALIGN_EXPAND);
			loadPathEditline = SampleWidgets.AddStringFieldWithButton(widgetConnections, grid, "Mesh file",
				"The path of the .mesh file loaded into the classic navigation mesh.",
				DEFAULT_DIR,
				"Load", "Opens the file dialog to choose a .mesh file and load it into the classic navigation mesh.",
				() =>
				{
					loadDialog.Hidden = false;
					loadDialog.SetPermanentFocus();
				});
		}

		operationsTabbox.CurrentTab = 0;

		CreateFileDialogs();

		window.Arrange();
	}
}
