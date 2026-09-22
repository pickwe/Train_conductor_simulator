// Provides UI controls for the Visualizer demonstration sample.
// Creates checkboxes to toggle visualizer on/off, enable depth testing,
// and control which 2D primitives are displayed. Works together with
// VisualizerUsage component which does the actual rendering.

using Unigine;

// UI controller for visualizer sample with toggles for various render options.
public partial class VisualizerSample : Component
{
	// Reference to the component that renders visualizer primitives
	[ShowInEditor]
	[Parameter(Title = "Visualizer usage")]
	private VisualizerUsage visualizer_usage = null;
	// UI window containing all control checkboxes
	private SampleDescriptionWindow window = null;

	// Creates UI window with checkboxes for visualizer settings.
	void Init()
	{
		window = new SampleDescriptionWindow();
		window.createWindow();
		var parameters = window.getParameterGroupBox();

		// Master toggle for visualizer rendering
		WidgetCheckBox visualizer_check_box = new WidgetCheckBox("Enable visualizer");
		parameters.AddChild(visualizer_check_box, Gui.ALIGN_LEFT);
		visualizer_check_box.EventChanged.Connect(() =>
		{
			Visualizer.Enabled = visualizer_check_box.Checked;
		});
		visualizer_check_box.Checked = true;

		// Depth test toggle - when enabled, primitives are occluded by scene geometry
		WidgetCheckBox depth_test_check_box = new WidgetCheckBox("Enable depth test");
		parameters.AddChild(depth_test_check_box, Gui.ALIGN_LEFT);
		depth_test_check_box.EventChanged.Connect(() =>
		{
			if (depth_test_check_box.Checked)
			{
				Visualizer.Mode = Visualizer.MODE.ENABLED_DEPTH_TEST_ENABLED;
			}
			else
			{
				Visualizer.Mode = Visualizer.MODE.ENABLED_DEPTH_TEST_DISABLED;
			}
		});
		depth_test_check_box.Checked = true;

		// 2D primitive toggles - control which screen-space shapes are drawn
		WidgetCheckBox point2D_check_box = new WidgetCheckBox("Point2D");
		parameters.AddChild(point2D_check_box, Gui.ALIGN_LEFT);
		point2D_check_box.EventChanged.Connect(() =>
		{
			visualizer_usage.renderPoint2D = point2D_check_box.Checked;
		});
		point2D_check_box.Checked = visualizer_usage.renderPoint2D;

		WidgetCheckBox line2D_check_box = new WidgetCheckBox("Line2D");
		parameters.AddChild(line2D_check_box, Gui.ALIGN_LEFT);
		line2D_check_box.EventChanged.Connect(() =>
		{
			visualizer_usage.renderLine2D = line2D_check_box.Checked;
		});
		line2D_check_box.Checked = visualizer_usage.renderPoint2D;

		WidgetCheckBox triangle2D_check_box = new WidgetCheckBox("Triangle2D");
		parameters.AddChild(triangle2D_check_box, Gui.ALIGN_LEFT);
		triangle2D_check_box.EventChanged.Connect(() =>
		{
			visualizer_usage.renderTriangle2D = triangle2D_check_box.Checked;
		});
		triangle2D_check_box.Checked = visualizer_usage.renderTriangle2D;

		WidgetCheckBox quad2D_check_box = new WidgetCheckBox("Quad2D");
		parameters.AddChild(quad2D_check_box, Gui.ALIGN_LEFT);
		quad2D_check_box.EventChanged.Connect(() =>
		{
			visualizer_usage.renderQuad2D = quad2D_check_box.Checked;
		});
		quad2D_check_box.Checked = visualizer_usage.renderQuad2D;

		WidgetCheckBox rectangle_check_box = new WidgetCheckBox("Rectangle");
		parameters.AddChild(rectangle_check_box, Gui.ALIGN_LEFT);
		rectangle_check_box.EventChanged.Connect(() =>
		{
			visualizer_usage.renderRectangle = rectangle_check_box.Checked;
		});
		rectangle_check_box.Checked = visualizer_usage.renderRectangle;

		WidgetCheckBox message2D_check_box = new WidgetCheckBox("Message2D");
		parameters.AddChild(message2D_check_box, Gui.ALIGN_LEFT);
		message2D_check_box.EventChanged.Connect(() =>
		{
			visualizer_usage.renderMessage2D = message2D_check_box.Checked;
		});
		message2D_check_box.Checked = visualizer_usage.renderMessage2D;
	}

	// Cleans up UI window on component shutdown.
	void Shutdown()
	{
		window.shutdown();
	}
}
