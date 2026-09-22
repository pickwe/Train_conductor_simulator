// Provides UI for clutter generation and conversion to cluster. Allows
// generating random clutter instances and converting them to optimized
// mesh cluster nodes via button controls.

using Unigine;

// UI for clutter generation and cluster conversion.
public partial class ClutterSample : Component
{
	// Node containing ClutterConverter component
	public Node converterNode = null;

	// Reference to converter component
	private ClutterConverter clutter_converter = null;

	// Sample UI window
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();

	// Converter reference is obtained and UI is created.
	private void Init()
	{
		clutter_converter = GetComponent<ClutterConverter>(converterNode);
		if (!clutter_converter)
			Log.Error("ClutterSample.Init(): cannot find ClutterConverter component!\n");
		InitGui();
	}

	// UI is cleaned up on shutdown.
	private void Shutdown()
	{
		ShutdownGui();
	}

	// Buttons for generate and convert actions are created.
	private void InitGui()
	{
		sampleDescriptionWindow.createWindow();
		var parameters = sampleDescriptionWindow.getParameterGroupBox();

		WidgetHBox hbox = new WidgetHBox(5, 0);
		parameters.AddChild(hbox, Gui.ALIGN_CENTER);

		WidgetButton button = new WidgetButton("Generate Clutter");
		button.EventClicked.Connect(GenerateButtonCallback);
		hbox.AddChild(button, Gui.ALIGN_LEFT);

		button = new WidgetButton("Convert to Cluster");
		button.EventClicked.Connect(ConvertButtonCallback);
		hbox.AddChild(button, Gui.ALIGN_LEFT);
	}

	// Cleans up UI window.
	private void ShutdownGui()
	{
		sampleDescriptionWindow.shutdown();
	}

	// Triggers clutter regeneration with new random seed.
	private void GenerateButtonCallback()
	{
		clutter_converter.generateClutter();
	}

	// Triggers conversion of clutter to cluster node.
	private void ConvertButtonCallback()
	{
		clutter_converter.ConvertToCluster();
	}
}
