// Stress test for async node loading. Spawns multiple nodes on background
// thread and integrates them into the spatial tree on the main thread.
// Demonstrates thread-safe node loading pattern.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

using System.Threading;
using Unigine;

// Stress tests async node loading with background thread spawning.
public partial class AsyncQueueStressSample : Component
{
	// Path to node file to spawn
	[ShowInEditor][ParameterFile]
	private string nodeToSpawn = null;

	// Thread-safe counter for loaded nodes
	private int numNodesLoaded;

	// UI label displaying node count
	private WidgetLabel numNodesLoadedLabel;

	// Sample description window
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();

	// Profiler and UI are initialized with node spawn controls.
	void Init()
	{
		// Enable profiler for performance monitoring
		Profiler.Enabled = true;

		numNodesLoaded = 0;

		// Create UI window
		sampleDescriptionWindow.createWindow(Gui.ALIGN_RIGHT);

		WidgetGroupBox parameters = sampleDescriptionWindow.getParameterGroupBox();

		// Node count spinbox
		var numNodesHBox = new WidgetHBox(5);
		parameters.AddChild(numNodesHBox, Gui.ALIGN_EXPAND);

		var multithreadLabel = new WidgetLabel("Num nodes");
		numNodesHBox.AddChild(multithreadLabel);

		var spinboxHBox = new WidgetHBox();
		var editline = new WidgetEditLine();
		editline.Validator = Gui.VALIDATOR_INT;
		var spinbox = new WidgetSpinBox();
		editline.AddAttach(spinbox);
		spinbox.MinValue = 1;
		spinbox.MaxValue = 10000;
		spinbox.Value = 100;
		spinboxHBox.AddChild(editline);
		spinboxHBox.AddChild(spinbox);
		numNodesHBox.AddChild(spinboxHBox, Gui.ALIGN_RIGHT);

		// Button triggers async node loading
		var requestLoadNodesButton = new WidgetButton("Request Load Nodes Async");
		parameters.AddChild(requestLoadNodesButton, Gui.ALIGN_EXPAND);

		numNodesLoadedLabel = new WidgetLabel();
		parameters.AddChild(numNodesLoadedLabel, Gui.ALIGN_EXPAND);

		// Start loading on background thread when clicked
		requestLoadNodesButton.EventClicked.Connect(() =>
		{
			AsyncQueue.RunAsync(AsyncQueue.ASYNC_THREAD.BACKGROUND, () => { LoadNodes(spinbox.Value); });
		});
	}

	// Node count label is updated and colored red if over threshold.
	void Update()
	{
		numNodesLoadedLabel.Text = "Num nodes loaded " + numNodesLoaded.ToString();
		// Warn if too many nodes loaded
		if (numNodesLoaded > 2000)
			numNodesLoadedLabel.FontColor = vec4.RED;
	}

	// Profiler is disabled and UI is cleaned up on shutdown.
	void Shutdown()
	{
		Profiler.Enabled = false;

		sampleDescriptionWindow.shutdown();
	}

	// Loads nodes on background thread and integrates them on main thread.
	private void LoadNodes(int num)
	{
		for (int i = 0; i < num; ++i)
		{
			// Load node on background thread (not added to spatial tree yet)
			Node loadedNode = World.LoadNode(nodeToSpawn, false);

			// Randomize position in 3D space
			Vec3 position = new Vec3();
			position.x = Game.GetRandomFloat(-100.0f, 100.0f);
			position.y = Game.GetRandomFloat(-100.0f, 100.0f);
			position.z = Game.GetRandomFloat(0.0f, 50.0f);
			loadedNode.WorldPosition = position;

			// Thread-safe increment of counter
			Interlocked.Add(ref numNodesLoaded, 1);

			// Schedule spatial tree integration on main thread
			AsyncQueue.RunAsync(AsyncQueue.ASYNC_THREAD.MAIN, () =>
			{
				// Add node and children to spatial tree
				loadedNode.UpdateEnabled();
			});
	}
}
}
