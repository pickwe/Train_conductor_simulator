// UI controller for the camera zoom demonstration.
// Provides a slider to adjust zoom level and buttons to focus on different targets.
// Displays real-time FOV, mouse sensitivity, and render distance scale values.

using System.Globalization;
using Unigine;
using static Unigine.Input;

// Demo UI for ZoomController with zoom slider and target focus buttons.
public partial class ZoomSample : Component
{
	// Target nodes that can be focused on via buttons
	[ShowInEditor]
	[Parameter(Title = "Target one node")]
	private Node targetOneNode = null;

	[ShowInEditor]
	[Parameter(Title = "Target two node")]
	private Node targetTwoNode = null;

	[ShowInEditor]
	[Parameter(Title = "Target three node")]
	private Node targetThreeNode = null;

	// Reference to the ZoomController component
	[ShowInEditor]
	[Parameter(Title = "Zoom controller")]
	private ZoomController zoom = null;

	// UI elements
	private SampleDescriptionWindow window = null;
	private WidgetLabel fov_label = null;
	private WidgetLabel mouse_sensivity_label = null;
	private WidgetLabel render_scale_label = null;
	private WidgetSlider sliderZoom = null;

	[ShowInEditor]
	[Parameter(Title = "Player Camera")]
	private Player player = null;

	// Saved mouse handle mode for cleanup
	private Input.MOUSE_HANDLE mouse_handle = Input.MOUSE_HANDLE.USER;

	// Creates UI with zoom slider, info labels, reset button, and target focus buttons.
	private void Init()
	{
		InitComponents();

		// Switch to soft mouse for UI interaction
		mouse_handle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		window = new SampleDescriptionWindow();
		window.createWindow();

		Widget parameters = window.getParameterGroupBox();
		Widget grid = parameters.GetChild(0);

		// Zoom slider - updates zoom and refreshes all info labels
		sliderZoom = window.addFloatParameter("Zoom:", "Zoom", 1, 1, 100.0f, (float val) =>
		{
			zoom.UpdateZoomFactor(val);
			fov_label.Text = "FOV:" + player.Fov.ToString("0.00", CultureInfo.InvariantCulture) + " deg";
			mouse_sensivity_label.Text = "Mouse sensivity:" + ControlsApp.MouseSensitivity.ToString("0.000", CultureInfo.InvariantCulture) + " deg";
			render_scale_label.Text = "Render distance scale:" + Render.DistanceScale.ToString("0.00", CultureInfo.InvariantCulture) + " deg";
		});


		fov_label = new WidgetLabel("FOV:" + player.Fov.ToString("0.00", CultureInfo.InvariantCulture) + " deg");
		fov_label.Width = 100;
		parameters.AddChild(fov_label, Gui.ALIGN_LEFT);

		mouse_sensivity_label = new WidgetLabel("Mouse sensivity:" + ControlsApp.MouseSensitivity.ToString("0.000", CultureInfo.InvariantCulture) + " deg");
		mouse_sensivity_label.Width = 100;
		parameters.AddChild(mouse_sensivity_label, Gui.ALIGN_LEFT);

		render_scale_label = new WidgetLabel("Render distance scale:" + Render.DistanceScale.ToString("0.00", CultureInfo.InvariantCulture) + " deg");
		render_scale_label.Width = 100;
		parameters.AddChild(render_scale_label, Gui.ALIGN_LEFT);


		var resetButton = new WidgetButton("Reset");
		parameters.AddChild(resetButton, Gui.ALIGN_LEFT);
		resetButton.EventClicked.Connect(() =>
		{
			sliderZoom.Value = 1 * 100;
		});

		WidgetHBox hbox = new WidgetHBox();
		hbox.SetSpace(2, 1);
		parameters.AddChild(hbox, Gui.ALIGN_LEFT);

		var focusLabel = new WidgetLabel("Focus on:");
		hbox.AddChild(focusLabel);

		var targetOne = new WidgetButton("Target 1");
		hbox.AddChild(targetOne);
		targetOne.EventClicked.Connect(() =>
		{
			zoom.FocusOnTarget(targetOneNode);
		});

		var targetTwo = new WidgetButton("Target 2");
		hbox.AddChild(targetTwo);
		targetTwo.EventClicked.Connect(() =>
		{
			zoom.FocusOnTarget(targetTwoNode);
		});

		var targetThree = new WidgetButton("Target 3");
		hbox.AddChild(targetThree);
		targetThree.EventClicked.Connect(() =>
		{
			zoom.FocusOnTarget(targetThreeNode);
		});
	}

	// Restores mouse handle and cleans up UI on shutdown.
	private void Shutdown()
	{
		Input.MouseHandle = mouse_handle;
		window.shutdown();
	}

	// Validates that all required references are assigned in the editor.
	void InitComponents()
	{
		if (!zoom)
		{
			Log.Error("ZoomSample.Init.InitComponents zoom controller is not assigned !\n");
		}

		if (!player)
		{
			Log.Error("ZoomSample.Init.InitComponents player is not assigned!\n");
		}
		if (!targetOneNode || !targetTwoNode || !targetThreeNode)
		{
			Log.Error("ZoomSample.Init.InitComponents targets are not assigned!\n");
		}
	}
}
