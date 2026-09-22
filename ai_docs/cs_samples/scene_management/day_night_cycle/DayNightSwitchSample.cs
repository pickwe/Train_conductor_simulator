// Demonstrates a day/night cycle system with UI controls. Provides sliders for
// time, zenith threshold, and morning/evening bounds. Shows how to connect
// multiple components (SunController, DayNightSwitcher) and visualize sun direction.

using System;
using System.Globalization;
using Unigine;
using System.Reflection.Metadata;


#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Creates UI for controlling day/night cycle parameters and visualizes sun direction.
public partial class DayNightSwitchSample : Component
{
	// Reference to sun controller component
	[ShowInEditor]
	[Parameter(Title = "Sun Controller")]
	private SunController sun = null;

	// Reference to day/night switcher component
	[ShowInEditor]
	[Parameter(Title = "Day night swithcer")]
	private DayNightSwitcher switcher = null;

	// UI elements
	private SampleDescriptionWindow window = null;
	private WidgetLabel timeLabel = null;
	private WidgetSlider timeSlider = null;

	// UI window and sliders for day/night control are created.
	void Init()
	{
		InitComponents();
		const int maxMinutes = 24 * 60;
		const int noonInMinutes = 12 * 60;

		window = new SampleDescriptionWindow();
		window.createWindow();


		WidgetGroupBox parameters = window.getParameterGroupBox();
		Widget grid = parameters.GetChild(0);

		//========== Global time=========//
		var label = new WidgetLabel("Global time:");
		label.Width = 100;
		grid.AddChild(label, Gui.ALIGN_LEFT);

		timeSlider = new WidgetSlider();
		timeSlider.MinValue = 0;
		timeSlider.MaxValue = maxMinutes;

		timeSlider.Width = 200;
		timeSlider.ButtonWidth = 20;
		timeSlider.ButtonHeight = 20;
		grid.AddChild(timeSlider, Gui.ALIGN_LEFT);

		timeLabel = new WidgetLabel("00:00");
		timeLabel.Width = 20;
		grid.AddChild(timeLabel, Gui.ALIGN_LEFT);

		timeSlider.EventChanged.Connect(() =>
		{
			int time = timeSlider.Value;
			String timeString = GetTimeString(time);
			timeLabel.Text = timeString;
			sun.SetTime(time * 60);
		});
		timeSlider.Value = noonInMinutes;


		//========== Threshold Slider=========//
		window.addFloatParameter("Zenith angle threshold:", "Zenith angle threshold", (float)switcher.SunZenitThreshold, 70.0f, 120.0f, (float val) =>
		{
			float floatValue = val / 100.0f;
		});


		//========== Morning Slider=========//
		label = new WidgetLabel("Morning time bound:");
		label.Width = 100;
		grid.AddChild(label, Gui.ALIGN_LEFT);

		var morningSlider = new WidgetSlider();
		morningSlider.MinValue = 0;
		morningSlider.MaxValue = maxMinutes;

		morningSlider.Width = 200;
		morningSlider.ButtonWidth = 20;
		morningSlider.ButtonHeight = 20;
		grid.AddChild(morningSlider, Gui.ALIGN_LEFT);

		var morningTimeLabel = new WidgetLabel("00:00");
		morningTimeLabel.Width = 20;
		grid.AddChild(morningTimeLabel, Gui.ALIGN_LEFT);

		morningSlider.EventChanged.Connect(() =>
		{
			int time = morningSlider.Value;
			String timeString = GetTimeString(time);
			morningTimeLabel.Text = timeString;
			int hours = time / 60;
			int minutes = time % 60;
			switcher.SetMorningControlTime(new ivec2(hours, minutes));
		});
		morningSlider.Value = switcher.GetControlMorningTime();


		//========== Evening Slider=========//
		label = new WidgetLabel("Evening time bound:");
		label.Width = 100;
		grid.AddChild(label, Gui.ALIGN_LEFT);

		var eveningSlider = new WidgetSlider();
		eveningSlider.MinValue = 0;
		eveningSlider.MaxValue = maxMinutes;

		eveningSlider.Width = 200;
		eveningSlider.ButtonWidth = 20;
		eveningSlider.ButtonHeight = 20;
		grid.AddChild(eveningSlider, Gui.ALIGN_LEFT);

		var eveningTimeLabel = new WidgetLabel("00:00");
		eveningTimeLabel.Width = 20;
		grid.AddChild(eveningTimeLabel, Gui.ALIGN_LEFT);

		eveningSlider.EventChanged.Connect(() =>
		{
			int time = eveningSlider.Value;
			String timeString = GetTimeString(time);
			eveningTimeLabel.Text = timeString;
			int hours = time / 60;
			int minutes = time % 60;
			switcher.SetEveningControlTime(new ivec2(hours, minutes));
		});
		eveningSlider.Value = switcher.GetControlEveningTime();


		//========== Control type Switch =========//
		WidgetComboBox controlType = new WidgetComboBox();
		parameters.AddChild(controlType, Gui.ALIGN_LEFT);
		controlType.AddItem("Zenith angle threshold");
		controlType.AddItem("Time");
		controlType.EventChanged.Connect(() =>
		{
			int item = controlType.CurrentItem;
			var type = (DayNightSwitcher.CONTROL_TYPE)(item);
			switcher.SetControlType(type);
		});

		//========== Timescale slider =========//

		window.addIntParameter("Timescale:", "Timescale", (int)sun.Timescale, 0, 60 * 60 * 12, (int val) =>
		{
			sun.Timescale = val;
		});

		//========== Continuous rotation checkbox =========//
		WidgetCheckBox continuousCheckBox = new WidgetCheckBox("Continuous sun rotation");
		parameters.AddChild(continuousCheckBox, Gui.ALIGN_LEFT);
		continuousCheckBox.EventChanged.Connect(() =>
		{
			sun.IsContinuous = continuousCheckBox.Checked;
		});

		continuousCheckBox.Checked = sun.IsContinuous;


		sun.EventOnTimeChanged.Connect((double time) =>
		{
			// Updating slider to current time coming from sun controller
			sun.EventOnTimeChanged.Enabled = false;
			timeSlider.Value = ((int)time / 60);
			sun.EventOnTimeChanged.Enabled = true;

		});
	}

	// Sun direction vectors are rendered for visualization.
	void Update()
	{
		// Render reference up vector and sun direction
		Visualizer.RenderVector(new Vec3(0, 0, 2), new Vec3(0, 0, 7), vec4.RED,0.5f);
		Visualizer.RenderVector(new Vec3(0, 0, 2), new Vec3(0, 0, 2) + /*Vec3 */sun.node.GetWorldDirection(MathLib.AXIS.Z) * 5, vec4.BLUE, 0.5f);
	}

	// UI window is released on component shutdown.
	private void Shutdown()
	{
		window.shutdown();
	}

	// Converts minutes to formatted time string (HH:MM).
	public string GetTimeString(int minutes)
	{
		int hours = minutes / 60;
		int leftMinutes = minutes % 60;
		String str = "";
		str += hours.ToString("00", CultureInfo.InvariantCulture);
		str += ":";
		str += leftMinutes.ToString("00", CultureInfo.InvariantCulture);
		return str;
	}

	// Validates required component references and enables visualizer.
	private void InitComponents()
	{
		if (!sun)
		{
			Log.Error("DayNightSwitchSample.InitComponents SunController is not assigned!\n");
		}

		if (!switcher)
		{
			Log.Error("DayNightSwitchSample.InitComponents DayNightSwithcer is not assigned!\n");
		}

		Visualizer.Enabled = true;
	}
}
