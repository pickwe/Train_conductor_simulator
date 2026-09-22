// Drives four SensorZoneVisualizers mounted on one object (forward / back / left /
// right) to visualize the field of view of four sensors. The parameter panel shows
// one tab per direction; sliders for near/far distance, horizontal and vertical
// angles and color rebuild that zone's mesh online.

using Unigine;

public partial class SensorZoneVisualizerSample : Component
{
	private const int NUM_SENSORS = 4;
	// minimal gap kept between a min/max slider pair so they never meet
	private const float MIN_GAP = 0.01f;

	// Clockwise from forward (top-down view): forward, right, back, left.
	[ShowInEditor, Parameter(Title = "Forward sensor")]
	public Node SensorForward = null;
	[ShowInEditor, Parameter(Title = "Right sensor")]
	public Node SensorRight = null;
	[ShowInEditor, Parameter(Title = "Back sensor")]
	public Node SensorBack = null;
	[ShowInEditor, Parameter(Title = "Left sensor")]
	public Node SensorLeft = null;

	// One editable sensor zone per direction. Angles/color are mirrored here so the
	// paired min/max sliders can rebuild the vec2 ranges on change.
	private class SensorUI
	{
		public SensorZoneVisualizer visualizer = null;
		public vec2 horizontalAngles;
		public vec2 verticalAngles;
		public vec3 color;
		// kept so a min/max slider can snap itself back when it would cross its pair
		public WidgetSlider nearSlider;
		public WidgetSlider farSlider;
		public WidgetSlider horizontalMin;
		public WidgetSlider horizontalMax;
		public WidgetSlider verticalMin;
		public WidgetSlider verticalMax;
	}
	private SensorUI[] sensors = new SensorUI[NUM_SENSORS];

	private SampleDescriptionWindow descriptionWindow = new SampleDescriptionWindow();
	private WidgetTabBox tabBox;

	// The shared color dialog: only one is open at a time, reopened per swatch.
	// While it is open the picked color is applied live (polled in Update()).
	private WidgetDialogColor colorDialog;
	private EventConnections colorDialogConnections = new EventConnections();
	private WidgetSprite activeSwatch;
	private System.Action<vec4> activeColorChange;
	private vec4 activeColorOriginal;
	private vec4 activeColorLast;

	// Init order 2 so it runs after the SensorZoneVisualizers (order 1) and their
	// material color is already available via GetColor().
	[MethodInit(Order = 2)]
	void Init()
	{
		for (int i = 0; i < NUM_SENSORS; i++)
			sensors[i] = new SensorUI();

		// Each direction's visualizer lives on its own node, referenced explicitly.
		sensors[0].visualizer = GetVisualizer(SensorForward);
		sensors[1].visualizer = GetVisualizer(SensorRight);
		sensors[2].visualizer = GetVisualizer(SensorBack);
		sensors[3].visualizer = GetVisualizer(SensorLeft);

		for (int i = 0; i < NUM_SENSORS; i++)
		{
			SensorZoneVisualizer visualizer = sensors[i].visualizer;
			if (visualizer == null)
			{
				Log.Warning($"SensorZoneVisualizerSample.Init: sensor zone node {i} is not assigned\n");
				continue;
			}

			// start from the current values: angles from the property, color from the
			// visualizer's material (its current "Emission Color")
			sensors[i].horizontalAngles = visualizer.HorizontalAngles;
			sensors[i].verticalAngles = visualizer.VerticalAngles;
			vec4 materialColor = visualizer.GetColor();
			sensors[i].color = new vec3(materialColor.x, materialColor.y, materialColor.z);
		}

		BuildUI();
	}

	void Update()
	{
		// The color dialog has no "changed" event, so while it is open we poll its
		// color and apply it immediately whenever it differs from the last applied one.
		if (colorDialog == null || activeColorChange == null)
			return;

		vec4 color = colorDialog.Color;
		if (color != activeColorLast)
		{
			activeColorLast = color;
			activeColorChange(color);
			if (activeSwatch)
				activeSwatch.Color = color;
		}
	}

	void Shutdown()
	{
		CloseColorDialog();
		if (tabBox)
			tabBox.DeleteLater();
		descriptionWindow.shutdown();
	}

	private SensorZoneVisualizer GetVisualizer(Node sensorNode)
	{
		return sensorNode ? ComponentSystem.GetComponent<SensorZoneVisualizer>(sensorNode) : null;
	}

	private void CloseColorDialog()
	{
		colorDialogConnections.DisconnectAll();
		activeColorChange = null;
		activeSwatch = null;
		if (colorDialog != null)
		{
			colorDialog.DeleteLater();
			colorDialog = null;
		}
	}

	private void BuildUI()
	{
		descriptionWindow.createWindow();

		// One tab per sensor direction, placed inside the "Parameters" group.
		tabBox = new WidgetTabBox(4, 4);
		descriptionWindow.getParameterGroupBox().AddChild(tabBox, Gui.ALIGN_EXPAND);

		for (int i = 0; i < NUM_SENSORS; i++)
			BuildTab(i);

		// start on the forward sensor
		tabBox.CurrentTab = 0;
	}

	private void BuildTab(int index)
	{
		string[] names = { "Forward", "Right", "Back", "Left" };
		tabBox.AddTab(names[index]);

		SensorZoneVisualizer visualizer = sensors[index].visualizer;
		if (visualizer == null)
		{
			// keep the tab present but inform that its node is missing
			tabBox.AddChild(new WidgetLabel("No sensor zone node assigned"), Gui.ALIGN_LEFT);
			return;
		}

		WidgetGridBox grid = new WidgetGridBox(3, 4, 4);
		tabBox.AddChild(grid, Gui.ALIGN_EXPAND);

		// Near / far distance (radii of the spherical caps). Near can't move above
		// far; far can't move below near.
		sensors[index].nearSlider = AddFloatSlider(grid, "Near distance", "Near radius of the zone",
			visualizer.Near, 0.1f, 10.0f, (float value) => {
				if (value > sensors[index].visualizer.Far - MIN_GAP)
				{
					value = sensors[index].visualizer.Far - MIN_GAP;
					MuteSetValue(sensors[index].nearSlider, value);
				}
				sensors[index].visualizer.Near = value;
				sensors[index].visualizer.Refresh();
			});

		sensors[index].farSlider = AddFloatSlider(grid, "Far distance", "Far radius of the zone",
			visualizer.Far, 0.2f, 20.0f, (float value) => {
				if (value < sensors[index].visualizer.Near + MIN_GAP)
				{
					value = sensors[index].visualizer.Near + MIN_GAP;
					MuteSetValue(sensors[index].farSlider, value);
				}
				sensors[index].visualizer.Far = value;
				sensors[index].visualizer.Refresh();
			});

		// Horizontal angles (yaw). Each border stays in its own half relative to the
		// forward direction: left in [-180, 0], right in [0, 180], so they never cross.
		sensors[index].horizontalMin = AddFloatSlider(grid, "Left border", "Minimum horizontal angle, degrees",
			sensors[index].horizontalAngles.x, -180.0f, 0.0f, (float value) => {
				if (value > sensors[index].horizontalAngles.y - MIN_GAP)
				{
					value = sensors[index].horizontalAngles.y - MIN_GAP;
					MuteSetValue(sensors[index].horizontalMin, value);
				}
				sensors[index].horizontalAngles.x = value;
				sensors[index].visualizer.HorizontalAngles = sensors[index].horizontalAngles;
				sensors[index].visualizer.Refresh();
			});

		sensors[index].horizontalMax = AddFloatSlider(grid, "Right border", "Maximum horizontal angle, degrees",
			sensors[index].horizontalAngles.y, 0.0f, 180.0f, (float value) => {
				if (value < sensors[index].horizontalAngles.x + MIN_GAP)
				{
					value = sensors[index].horizontalAngles.x + MIN_GAP;
					MuteSetValue(sensors[index].horizontalMax, value);
				}
				sensors[index].horizontalAngles.y = value;
				sensors[index].visualizer.HorizontalAngles = sensors[index].horizontalAngles;
				sensors[index].visualizer.Refresh();
			});

		// Vertical angles (pitch). Same half-space split: lower in [-180, 0], upper in
		// [0, 180], built from the forward direction without crossing into each other.
		sensors[index].verticalMin = AddFloatSlider(grid, "Lower border", "Minimum vertical angle, degrees",
			sensors[index].verticalAngles.x, -180.0f, 0.0f, (float value) => {
				if (value > sensors[index].verticalAngles.y - MIN_GAP)
				{
					value = sensors[index].verticalAngles.y - MIN_GAP;
					MuteSetValue(sensors[index].verticalMin, value);
				}
				sensors[index].verticalAngles.x = value;
				sensors[index].visualizer.VerticalAngles = sensors[index].verticalAngles;
				sensors[index].visualizer.Refresh();
			});

		sensors[index].verticalMax = AddFloatSlider(grid, "Upper border", "Maximum vertical angle, degrees",
			sensors[index].verticalAngles.y, 0.0f, 180.0f, (float value) => {
				if (value < sensors[index].verticalAngles.x + MIN_GAP)
				{
					value = sensors[index].verticalAngles.x + MIN_GAP;
					MuteSetValue(sensors[index].verticalMax, value);
				}
				sensors[index].verticalAngles.y = value;
				sensors[index].visualizer.VerticalAngles = sensors[index].verticalAngles;
				sensors[index].visualizer.Refresh();
			});

		// Color drives the material "Emission Color" (visible only when the zone
		// material exposes that parameter). The swatch opens a color dialog.
		AddColorPicker(grid, "Color", new vec4(sensors[index].color, 1.0f),
			(vec4 color) => {
				sensors[index].color = new vec3(color.x, color.y, color.z);
				sensors[index].visualizer.SetColor(new vec4(sensors[index].color, 1.0f));
			});
	}

	// Sets a slider value without re-firing its EventChanged (the clamp path).
	private static void MuteSetValue(WidgetSlider slider, float value)
	{
		slider.EventChanged.Enabled = false;
		slider.Value = (int)(value * 100);
		slider.EventChanged.Enabled = true;
	}

	// Builds a labelled float slider (label + slider + value readout) in the grid.
	// The integer slider keeps two decimals of precision via a x100 scale.
	private WidgetSlider AddFloatSlider(WidgetGridBox grid, string name, string tooltip,
		float value, float minValue, float maxValue, System.Action<float> onChange)
	{
		WidgetLabel label = new WidgetLabel(name);
		label.Width = 100;
		label.SetToolTip(tooltip);
		grid.AddChild(label, Gui.ALIGN_LEFT);

		WidgetSlider slider = new WidgetSlider();
		slider.MinValue = (int)(minValue * 100);
		slider.MaxValue = (int)(maxValue * 100);
		slider.Value = (int)(value * 100);
		slider.Width = 200;
		slider.ButtonWidth = 20;
		slider.ButtonHeight = 20;
		slider.SetToolTip(tooltip);
		grid.AddChild(slider, Gui.ALIGN_LEFT);

		WidgetLabel valueLabel = new WidgetLabel(value.ToString("0.00"));
		valueLabel.Width = 40;
		valueLabel.SetToolTip(tooltip);
		grid.AddChild(valueLabel);

		// onChange may clamp by re-setting the slider, so read the final value back
		// for the readout label.
		slider.EventChanged.Connect(() => {
			onChange(slider.Value / 100.0f);
			valueLabel.Text = (slider.Value / 100.0f).ToString("0.00");
		});

		return slider;
	}

	// A color swatch (label + clickable sprite). Clicking it opens a WidgetDialogColor;
	// on OK the picked color is applied to the swatch and passed to onChange.
	private void AddColorPicker(WidgetGridBox grid, string name, vec4 initColor, System.Action<vec4> onChange)
	{
		WidgetLabel label = new WidgetLabel(name);
		label.Width = 100;
		grid.AddChild(label, Gui.ALIGN_LEFT);

		WidgetSprite swatch = new WidgetSprite();
		// match the slider width so the color row lines up with the parameter rows
		swatch.Width = 200;
		swatch.Texture = "core/textures/common/white.texture";
		swatch.Color = initColor;
		grid.AddChild(swatch, Gui.ALIGN_LEFT);
		// the third grid column is unused for this row
		grid.AddChild(new WidgetLabel(""));

		swatch.EventClicked.Connect(() => {
			// only one dialog at a time: drop the previous one and its connections
			CloseColorDialog();

			colorDialog = new WidgetDialogColor(Gui.GetCurrent(), "Select color");
			colorDialog.Color = swatch.Color;

			// remember what this dialog edits so Update() can apply changes live and
			// Cancel can revert to the color the swatch had before opening
			activeSwatch = swatch;
			activeColorChange = onChange;
			activeColorOriginal = swatch.Color;
			activeColorLast = swatch.Color;

			// the color is already applied live; OK just closes the dialog
			colorDialog.GetOkButton().EventClicked.Connect(colorDialogConnections, () => { CloseColorDialog(); });
			// Cancel restores the original color, then closes
			colorDialog.GetCancelButton().EventClicked.Connect(colorDialogConnections, () => {
				if (activeColorChange != null)
					activeColorChange(activeColorOriginal);
				if (activeSwatch)
					activeSwatch.Color = activeColorOriginal;
				CloseColorDialog();
			});

			Gui.GetCurrent().AddChild(colorDialog, Gui.ALIGN_OVERLAP);
			colorDialog.SetPermanentFocus();
		});
	}
}
