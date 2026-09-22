// User interface for displaying gamepad input state. Shows connected gamepads,
// button presses, analog stick values, trigger states, and touchpad visualization.
// Provides controls for filter adjustment and vibration testing.

using System;
using System.Collections.Generic;
using Unigine;

// Displays gamepad state and provides controls for filter and vibration.
public partial class InputGamepadUI : Component
{
	// Reference to gamepad input component
	public InputGamePadComponent gamepadComponent = null;

	// Main UI layout file
	[ParameterFile(Filter = ".ui")]
	public string uiFile = null;

	// Per-gamepad info panel template
	[ParameterFile(Filter = ".ui")]
	public string uiInfoFile = null;

	// Event fired when filter slider value changes
	static public event Action<int, float> filterChanged;
	// Event fired when vibration test is requested
	static public event Action<int, float, float, float> setVibration;

	// Container for all UI widgets displaying a single gamepad's information
	private class InfoWidgets
	{
		public WidgetVBox infoVBox = null;
		public WidgetLabel nameLabel = null;
		public WidgetLabel numberLabel = null;
		public WidgetLabel playerIndexLabel = null;
		public WidgetLabel deviceTypeLabel = null;
		public WidgetLabel modelTypeLabel = null;
		public WidgetLabel availableLabel = null;
		public WidgetSlider filterSlider = null;
		public WidgetLabel lastButtonDownLabel = null;
		public WidgetLabel lastButtonPressedLabel = null;
		public WidgetLabel lastButtonUpLabel = null;
		public WidgetLabel leftAxesXLabel = null;
		public WidgetLabel leftAxesYLabel = null;
		public WidgetLabel rightAxesXLabel = null;
		public WidgetLabel rightAxesYLabel = null;
		public WidgetLabel leftLastDeltaXLabel = null;
		public WidgetLabel leftLastDeltaYLabel = null;
		public WidgetLabel rightLastDeltaXLabel = null;
		public WidgetLabel rightLastDeltaYLabel = null;
		public WidgetLabel leftTriggerLabel = null;
		public WidgetLabel rightTriggerLabel = null;
		public WidgetLabel lastLeftTriggerDeltaLabel = null;
		public WidgetLabel lastRightTriggerDeltaLabel = null;
		public WidgetSlider lowFrequencySlider = null;
		public WidgetSlider highFrequencySlider = null;
		public WidgetSlider durationSlider = null;
		public WidgetButton vibrationButton = null;
		public WidgetCanvas touchpadCanvas = null;
	}

	private UserInterface ui = null;
	private List<UserInterface> infosUi = null;
	private List<InfoWidgets> infoWidgets = null;

	private WidgetHBox mainHBox = null;
	private WidgetVBox backgroundVBox = null;

	private WidgetLabel errorMessageLabel = null;
	private WidgetVBox generalMessageInfo = null;
	private WidgetLabel countGamepadsLabel = null;
	private WidgetLabel countActiveGamepadsLabel = null;

	private WidgetHBox gamepadsInfoBox = null;
	private int lastGamepadsCount = 0;

	// UI is created and touch callback is registered.
	[MethodInit(Order = -1)]
	private void Init()
	{
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;

		infosUi = new List<UserInterface>();
		infoWidgets = new List<InfoWidgets>();

		Gui gui = Gui.GetCurrent();

		ui = new UserInterface(gui, uiFile);

		InitWidget(nameof(mainHBox), out mainHBox, ui);
		InitWidget(nameof(backgroundVBox), out backgroundVBox, ui);
		InitWidget(nameof(errorMessageLabel), out errorMessageLabel, ui);
		InitWidget(nameof(generalMessageInfo), out generalMessageInfo, ui);
		InitWidget(nameof(countGamepadsLabel), out countGamepadsLabel, ui);
		InitWidget(nameof(countActiveGamepadsLabel), out countActiveGamepadsLabel, ui);
		InitWidget(nameof(gamepadsInfoBox), out gamepadsInfoBox, ui);

		gui.AddChild(mainHBox);
		backgroundVBox.BackgroundColor = new vec4(0.0f, 0.0f, 0.0f, 0.5f);

		gamepadComponent.onTouch += DrawTouchInfo;
	}
	
	// UI is updated with current gamepad states each frame.
	private void Update()
	{
		if (gamepadComponent == null)
			return;

		// Rebuild UI when gamepad count changes
		if (lastGamepadsCount != gamepadComponent.CountActiveGamePads)
		{
			UpdateUI();
			lastGamepadsCount = gamepadComponent.CountActiveGamePads;
		}

		// Show information about gamepads
		for (int i = 0; i < gamepadComponent.CountActiveGamePads; i++)
		{
			infoWidgets[i].nameLabel.Text = gamepadComponent.GamepadsInfo[i].name;

			infoWidgets[i].deviceTypeLabel.Text = gamepadComponent.GamepadsInfo[i].deviceType.ToString();
			infoWidgets[i].modelTypeLabel.Text = gamepadComponent.GamepadsInfo[i].modelType.ToString();
			infoWidgets[i].playerIndexLabel.Text = gamepadComponent.GamepadsInfo[i].playerIndex.ToString();
			infoWidgets[i].availableLabel.Text = gamepadComponent.GamepadsInfo[i].isAvailable.ToString();

			infoWidgets[i].lastButtonDownLabel.Text = gamepadComponent.GamepadsInfo[i].lastButtonDown?.ToString();
			infoWidgets[i].lastButtonPressedLabel.Text = gamepadComponent.GamepadsInfo[i].lastButtonPressed?.ToString();
			infoWidgets[i].lastButtonUpLabel.Text = gamepadComponent.GamepadsInfo[i].lastButtonUp?.ToString();

			infoWidgets[i].leftAxesXLabel.Text = gamepadComponent.GamepadsInfo[i].axesLeft.x.ToString("0.000");
			infoWidgets[i].leftAxesYLabel.Text = gamepadComponent.GamepadsInfo[i].axesLeft.y.ToString("0.000");
			infoWidgets[i].rightAxesXLabel.Text = gamepadComponent.GamepadsInfo[i].axesRight.x.ToString("0.000");
			infoWidgets[i].rightAxesYLabel.Text = gamepadComponent.GamepadsInfo[i].axesRight.y.ToString("0.000");

			infoWidgets[i].leftLastDeltaXLabel.Text = gamepadComponent.GamepadsInfo[i].axesLeftLastDelta.x.ToString("0.000");
			infoWidgets[i].leftLastDeltaYLabel.Text = gamepadComponent.GamepadsInfo[i].axesLeftLastDelta.y.ToString("0.000");
			infoWidgets[i].rightLastDeltaXLabel.Text = gamepadComponent.GamepadsInfo[i].axesRightLastDelta.x.ToString("0.000");
			infoWidgets[i].rightLastDeltaYLabel.Text = gamepadComponent.GamepadsInfo[i].axesRightLastDelta.y.ToString("0.000");

			infoWidgets[i].leftTriggerLabel.Text = gamepadComponent.GamepadsInfo[i].triggerLeft.ToString("0.000");
			infoWidgets[i].rightTriggerLabel.Text = gamepadComponent.GamepadsInfo[i].triggerRight.ToString("0.000");
			infoWidgets[i].lastLeftTriggerDeltaLabel.Text = gamepadComponent.GamepadsInfo[i].triggerLeftLastDelta.ToString("0.000");
			infoWidgets[i].lastRightTriggerDeltaLabel.Text = gamepadComponent.GamepadsInfo[i].triggerRightLastDelta.ToString("0.000");
			
			infoWidgets[i].touchpadCanvas.Clear();
		}
	}

	// UI widgets are cleaned up on shutdown.
	private void Shutdown()
	{
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;

		if (mainHBox)
		{
			if (infosUi != null)
				for (int i = 0; i < infosUi.Count; i++)
					mainHBox.RemoveChild(infosUi[i].GetWidget(0));

			Gui.GetCurrent().RemoveChild(mainHBox);
		}
	}

	// Rebuilds UI panels for current gamepad count.
	private void UpdateUI()
	{
		countActiveGamepadsLabel.Text = gamepadComponent.CountActiveGamePads.ToString();

		if (gamepadComponent.CountActiveGamePads != 0)
		{
			errorMessageLabel.Hidden = true;
			generalMessageInfo.Hidden = false;
		}
		else
		{
			errorMessageLabel.Hidden = false;
			generalMessageInfo.Hidden = true;
		}

		countGamepadsLabel.Text = gamepadComponent.CountGamePads.ToString();

		// Remove current UI with gamepads information
		for (int i = 0; i < infosUi.Count; i++)
		{
			mainHBox.RemoveChild(infosUi[i].GetWidget(0));
			infosUi[i].DeleteLater();
		}
		infosUi.Clear();

		infoWidgets.Clear();
		for (int i = 0; i < gamepadComponent.CountActiveGamePads; i++)
		{
			UserInterface uiInfo = new UserInterface(Gui.GetCurrent(), uiInfoFile);
			infosUi.Add(uiInfo);

			InfoWidgets info = new InfoWidgets();
			infoWidgets.Add(info);

			InitWidget(nameof(info.infoVBox), out info.infoVBox, uiInfo);
			InitWidget(nameof(info.nameLabel), out info.nameLabel, uiInfo);
			InitWidget(nameof(info.numberLabel), out info.numberLabel, uiInfo);
			InitWidget(nameof(info.playerIndexLabel), out info.playerIndexLabel, uiInfo);
			InitWidget(nameof(info.deviceTypeLabel), out info.deviceTypeLabel, uiInfo);
			InitWidget(nameof(info.modelTypeLabel), out info.modelTypeLabel, uiInfo);
			InitWidget(nameof(info.availableLabel), out info.availableLabel, uiInfo);
			InitWidget(nameof(info.filterSlider), out info.filterSlider, uiInfo);
			InitWidget(nameof(info.lastButtonDownLabel), out info.lastButtonDownLabel, uiInfo);
			InitWidget(nameof(info.lastButtonPressedLabel), out info.lastButtonPressedLabel, uiInfo);
			InitWidget(nameof(info.lastButtonUpLabel), out info.lastButtonUpLabel, uiInfo);
			InitWidget(nameof(info.leftAxesXLabel), out info.leftAxesXLabel, uiInfo);
			InitWidget(nameof(info.leftAxesYLabel), out info.leftAxesYLabel, uiInfo);
			InitWidget(nameof(info.rightAxesXLabel), out info.rightAxesXLabel, uiInfo);
			InitWidget(nameof(info.rightAxesYLabel), out info.rightAxesYLabel, uiInfo);
			InitWidget(nameof(info.leftLastDeltaXLabel), out info.leftLastDeltaXLabel, uiInfo);
			InitWidget(nameof(info.leftLastDeltaYLabel), out info.leftLastDeltaYLabel, uiInfo);
			InitWidget(nameof(info.rightLastDeltaXLabel), out info.rightLastDeltaXLabel, uiInfo);
			InitWidget(nameof(info.rightLastDeltaYLabel), out info.rightLastDeltaYLabel, uiInfo);
			InitWidget(nameof(info.leftTriggerLabel), out info.leftTriggerLabel, uiInfo);
			InitWidget(nameof(info.rightTriggerLabel), out info.rightTriggerLabel, uiInfo);
			InitWidget(nameof(info.lastLeftTriggerDeltaLabel), out info.lastLeftTriggerDeltaLabel, uiInfo);
			InitWidget(nameof(info.lastRightTriggerDeltaLabel), out info.lastRightTriggerDeltaLabel, uiInfo);
			InitWidget(nameof(info.lowFrequencySlider), out info.lowFrequencySlider, uiInfo);
			InitWidget(nameof(info.highFrequencySlider), out info.highFrequencySlider, uiInfo);
			InitWidget(nameof(info.durationSlider), out info.durationSlider, uiInfo);
			InitWidget(nameof(info.vibrationButton), out info.vibrationButton, uiInfo);
			InitWidget(nameof(info.touchpadCanvas), out info.touchpadCanvas, uiInfo);

			gamepadsInfoBox.AddChild(info.infoVBox);

			infoWidgets[i].numberLabel.Text = gamepadComponent.GamepadsInfo[i].number.ToString();

			info.filterSlider.Data = i.ToString();
			info.filterSlider.EventChanged.Connect(ChangeFilter);

			info.vibrationButton.Data = i.ToString();
			info.vibrationButton.EventClicked.Connect(ClickVibrate);
		}
	}

	// Finds and initializes a widget by name from the UI file.
	private void InitWidget<T>(string name, out T widget, UserInterface ui) where T : Widget
	{
		widget = null;

		if (!ui)
			return;

		int id = ui.FindWidget(name);
		if (id != -1)
			widget = ui.GetWidget(id) as T;
	}

	// Handles filter slider changes and fires filter event.
	private void ChangeFilter(Widget sender)
	{
		int number = 0;
		bool res = int.TryParse(sender.Data, out number);

		WidgetSlider slider = sender as WidgetSlider;
		if (res && slider)
			filterChanged?.Invoke(number, slider.Value / 100.0f);
	}

	// Handles vibration button click and fires vibration event.
	private void ClickVibrate(Widget widget)
	{
		bool res = int.TryParse(widget.Data, out var number);
		if (res)
			setVibration?.Invoke(
				number,
				infoWidgets[number].lowFrequencySlider.Value / 100.0f,
				infoWidgets[number].highFrequencySlider.Value / 100.0f,
				infoWidgets[number].durationSlider.Value
				);
	}

	// Draws touchpad input visualization on canvas.
	private void DrawTouchInfo(InputGamePad gamepad, int gamepadNum)
	{
		if (infoWidgets.Count == 0) return;
		
		const int numberOfColors = 10;
		vec4[] colors = new vec4[numberOfColors];
		colors[0] = new vec4(1.0f, 0.0f, 0.0f, 1.0f);
		colors[1] = new vec4(0.0f, 1.0f, 0.0f, 1.0f);
		colors[2] = new vec4(0.0f, 0.0f, 1.0f, 1.0f);
		colors[3] = new vec4(1.0f, 1.0f, 0.0f, 1.0f);
		colors[4] = new vec4(0.0f, 1.0f, 1.0f, 1.0f);
		colors[5] = new vec4(1.0f, 0.0f, 1.0f, 1.0f);
		colors[6] = new vec4(1.0f, 0.0f, 1.0f, 1.0f);
		colors[7] = new vec4(0.5f, 0.0f, 0.0f, 1.0f);
		colors[8] = new vec4(0.0f, 0.5f, 0.0f, 1.0f);
		colors[9] = new vec4(0.0f, 0.0f, 0.5f, 1.0f);

		var drawCircle = (vec2 pos, WidgetCanvas canvas, vec4 color, float pressure) =>
		{
			int polygon = canvas.AddPolygon();
			canvas.SetPolygonColor(polygon, color);
			const int num = 10;
			const float radius = 10;
			for (int i = 0; i < num; ++i)
			{
				float s = Unigine.MathLib.Sin(Unigine.MathLib.PI2 * i / num) * radius * pressure + pos.x * canvas.Width;
				float c = Unigine.MathLib.Cos(Unigine.MathLib.PI2 * i / num) * radius + pressure * pos.y * canvas.Height;
				canvas.AddPolygonPoint(polygon, new vec3(s, c, 0.0f));
			}
		};

		for (int i = 0; i < gamepad.NumTouches; ++i)
		{
			for (int j = 0; j < gamepad.GetNumTouchFingers(i); ++j)
			{
				if (!(gamepad.GetTouchPressure(i, j) > MathLib.EPSILON)) continue;

				vec4 color = colors[i * gamepad.NumTouches + j];
				drawCircle.Invoke(gamepad.GetTouchPosition(i, j), infoWidgets[gamepadNum].touchpadCanvas, color,
					gamepad.GetTouchPressure(i, j));
			}
		}
	}
}
