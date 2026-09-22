// User interface for displaying joystick input state and force feedback controls.
// Shows connected joysticks, button/axis/POV values, and provides UI for testing
// various force feedback effects like constant, ramp, sine wave, spring, etc.

using System;
using System.Collections.Generic;
using System.Linq;
using Unigine;
using static InputJoystickComponent;
using FfbEffectType = Unigine.Input.JOYSTICK_FORCE_FEEDBACK_EFFECT;

// Displays joystick state and provides force feedback effect controls.
public partial class InputJoystickUI : Component
{
	// Reference to joystick input component
	public InputJoystickComponent joystickComponent = null;

	// Event fired when filter slider value changes
	static public event Action<int, float> filterChanged;

	/// <summary>
	/// Wraps a widget with a label inside a horizontal layout for consistent UI.
	/// </summary>
	static Widget WrapWidget(string name, Widget toWrap, Widget parent)
	{
		var hbox = new WidgetHBox();

		var nameLabel = new WidgetLabel(name) { FontSize = 16, FontOutline = 1 };
		nameLabel.FontSize = 16;
		nameLabel.FontOutline = 1;
		hbox.SetSpace(5, 0);
		parent.AddChild(hbox, Gui.ALIGN_LEFT);
		hbox.AddChild(nameLabel);
		hbox.AddChild(toWrap);
		return hbox;
	}

	/// <summary>
	/// Handles UI elements displaying joystick information.
	/// Updates UI components dynamically based on joystick state.
	/// </summary>
	private class InfoWidgets
	{
		/// <summary>
		/// Updates the force feedback effect button state for ramp effect.
		/// </summary>
		public void Update()
		{
			rampEffectButtonController?.Update();
		}

		/// <summary>
		/// Initializes UI elements for displaying joystick information.
		/// </summary>
		public InfoWidgets()
		{
			infoVBox = new WidgetVBox();

			var hbox = new WidgetHBox { Width = 325, FontSize = 16, FontOutline = 1 };
			hbox.SetSpace(5, 0);
			infoVBox.AddChild(hbox, Gui.ALIGN_LEFT);

			infoContainer = new WidgetVBox(5, 5) { FontSize = 16, FontOutline = 1 };
			hbox.AddChild(infoContainer);

			nameLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
			WrapWidget("Name:", nameLabel, infoContainer);

			numberLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
			WrapWidget("Number:", numberLabel, infoContainer);

			buttonsCountLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
			WrapWidget("Buttons Count:", buttonsCountLabel, infoContainer);

			axesCountLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
			WrapWidget("Axes Count", axesCountLabel, infoContainer);

			povsCountLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
			WrapWidget("Povs Count:", povsCountLabel, infoContainer);

			availableLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
			WrapWidget("Available:", availableLabel, infoContainer);

			{
				// Wrap slider
				var filterSliderContainer = new WidgetHBox() { Width = 100 };
				filterSliderContainer.SetSpace(5, 0);
				filterSliderContainer.SetPadding(0, 0, 8, 0);
				infoContainer.AddChild(filterSliderContainer, Gui.ALIGN_LEFT);
				filterSliderContainer.AddChild(new WidgetLabel("Filter:") { FontSize = 16, FontOutline = 1 });
				filterSlider = new WidgetSlider() { Width = 100 };
				filterSliderContainer.AddChild(filterSlider);
				var filterSliderValueLabel = new WidgetLabel("0.00") { FontSize = 16, FontOutline = 1 };
				filterSliderContainer.AddChild(filterSliderValueLabel);
				filterSlider.AddAttach(filterSliderValueLabel, "%.2f", 100);
			}

			lastPressedButtonLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
			WrapWidget("Pressed Button:", lastPressedButtonLabel, infoContainer);

			ffbStatusLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
			WrapWidget("Ffb Status:", ffbStatusLabel, infoContainer);

			clearWidgets = [];
			axesLabels = [];
			povsLabels = [];
		}

		public WidgetVBox infoContainer = null;
		public WidgetVBox infoVBox = null;
		public WidgetLabel nameLabel = null;
		public WidgetLabel numberLabel = null;
		public WidgetLabel buttonsCountLabel = null;
		public WidgetLabel axesCountLabel = null;
		public WidgetLabel povsCountLabel = null;
		public WidgetLabel availableLabel = null;
		public WidgetSlider filterSlider = null;
		public WidgetLabel lastPressedButtonLabel = null;
		public List<WidgetLabel> axesLabels = null;
		public List<WidgetLabel> povsLabels = null;
		public WidgetLabel ffbStatusLabel = null;

		public List<Widget> clearWidgets = null;

		private readonly Dictionary<FfbEffectType, FfbEffectData> effectData = [];
		private FfbDurationEffectButtonController rampEffectButtonController;

		private class FfbDurationEffectButtonController(FfbEffectData effect, WidgetButton button)
		{
			private float currentTime;

			public void Update()
			{
				if (currentTime >= effect.Duration || !effect.IsPlaying)
					return;

				var iFps = Game.IFps;
				currentTime += iFps;

				if (!(currentTime >= (float)effect.Duration / 1_000_000))
					return;

				button.Toggled = false;
				effect.IsPlaying = false;
			}
		}


		public void Set(JoystickInfo info, InputJoystickComponent joystickComponent)
		{
			availableLabel.Text = info.IsAvailable.ToString();
			filterSlider.Value = (int)(info.Filter * 100.0f);

			nameLabel.Text = info.Name;
			numberLabel.Text = info.Number.ToString();
			buttonsCountLabel.Text = info.NumButtons.ToString();
			axesCountLabel.Text = info.NumAxes.ToString();
			povsCountLabel.Text = info.NumPovs.ToString();
			ffbStatusLabel.Text = info.FfbData.AvailableFfbEffects.Count > 0 ? "Supported" : "Unsupported";

			filterSlider.Data = info.Number.ToString();

			int? lastPressedButton = info.LastPressedButton;
			if (lastPressedButton != null)
				lastPressedButtonLabel.Text = info.ButtonsName[lastPressedButton.Value];

			if (axesLabels.Count == info.NumAxes && povsLabels.Count == info.NumPovs)
			{
				for (int i = 0; i < info.NumAxes; i++)
				{
					float value = info.Axes[i];
					axesLabels[i].Text = value.ToString("0.000");
				}
				for (int i = 0; i < info.NumPovs; i++)
				{
					Input.JOYSTICK_POV value = info.Povs[i];
					povsLabels[i].Text = value.ToString();
				}
				return;
			}

			axesLabels.Clear();
			povsLabels.Clear();

			Gui gui = Gui.GetCurrent();

			foreach (Widget w in clearWidgets)
			{
				infoContainer.RemoveChild(w);
				w.DeleteLater();
			}

			clearWidgets.Clear();

			for (int i = 0; i < info.NumAxes; i++)
			{
				WidgetHBox hbox = new WidgetHBox(gui);
				hbox.SetSpace(5, 0);
				hbox.SetPadding(0, 0, 5, 0);
				clearWidgets.Add(hbox);
				infoContainer.AddChild(hbox, Gui.ALIGN_LEFT);

				WidgetLabel nameLabel = new WidgetLabel(gui);
				nameLabel.Text = info.AxesName[i] + ":";
				nameLabel.FontSize = 16;
				nameLabel.FontOutline = 1;
				hbox.AddChild(nameLabel);

				WidgetLabel axisValue = new WidgetLabel(gui, "0.0");
				axisValue.FontSize = 16;
				axisValue.FontOutline = 1;
				float value = info.Axes[i];
				axisValue.Text = value.ToString("0.000");
				hbox.AddChild(axisValue);

				axesLabels.Add(axisValue);
			}

			for (int i = 0; i < info.NumPovs; i++)
			{
				WidgetHBox hbox = new WidgetHBox(gui);
				hbox.SetSpace(5, 0);
				hbox.SetPadding(0, 0, i == 0 ? 16 : 5, 0);
				clearWidgets.Add(hbox);
				infoContainer.AddChild(hbox, Gui.ALIGN_LEFT);

				WidgetLabel nameLabel = new WidgetLabel(gui);
				nameLabel.Text = info.PovsName[i] + ":";
				nameLabel.FontSize = 16;
				nameLabel.FontOutline = 1;
				hbox.AddChild(nameLabel);

				WidgetLabel povValue = new WidgetLabel(gui, "NOT_PRESSED");
				povValue.FontSize = 16;
				povValue.FontOutline = 1;
				Input.JOYSTICK_POV value = info.Povs[i];
				povValue.Text = value.ToString();
				hbox.AddChild(povValue);

				povsLabels.Add(povValue);
			}



			effectData.Clear();
			foreach (var effectType in info.FfbData.AvailableFfbEffects)
			{
				var data = new FfbEffectData();
				data.EffectType = effectType;
				effectData.Add(effectType, data);
			}

			SetupFfbUi(info, infoContainer, joystickComponent);
		}


		private static void CreateFfbEffectParameterSlider(string name, float initialValue, float min, float max,
			Widget parent,
			Action<float> onChange)
		{
			const int sliderMin = 0;
			const int sliderMax = 1000;

			onChange(initialValue);

			var label = new WidgetLabel(name) { FontSize = 16, FontOutline = 1, Width = 75 };
			parent.AddChild(label, Gui.ALIGN_LEFT);

			var slider = new WidgetSlider { Width = 200, MinValue = sliderMin, MaxValue = sliderMax };
			slider.Value = (int)MapRange(initialValue, min, max, sliderMin, sliderMax);
			parent.AddChild(slider, Gui.ALIGN_CENTER);

			var input = new WidgetEditLine(initialValue.ToString("F2"))
			{
				FontSize = 16, FontOutline = 1, Width = 75, Validator = Gui.VALIDATOR_FLOAT
			};
			parent.AddChild(input, Gui.ALIGN_LEFT);

			input.EventChanged.Connect(() =>
			{
				float value = float.TryParse(input.Text, out var parsed) ? parsed : 0.0f;

				if (value < min || value > max)
				{
					value = Math.Clamp(value, min, max);
					input.EventChanged.Enabled = false;
					input.Text = value.ToString("F2");
					input.EventChanged.Enabled = true;
				}

				slider.EventChanged.Enabled = false;
				slider.Value = (int)MapRange(value, min, max, sliderMin, sliderMax);
				slider.EventChanged.Enabled = true;
				onChange(value);
			});

			slider.EventChanged.Connect(() =>
			{
				var value = MapRange(slider.Value, sliderMin, sliderMax, min, max);
				input.EventChanged.Enabled = false;
				input.Text = value.ToString("F2");
				input.EventChanged.Enabled = true;
				onChange(value);
			});

			return;

			static float MapRange(float input, float inMin, float inMax, float outMin, float outMax)
			{
				if (Math.Abs(inMin - inMax) < MathLib.EPSILON)
					return outMin;

				float lowerBound = Math.Min(inMin, inMax);
				float upperBound = Math.Max(inMin, inMax);
				input = Math.Clamp(input, lowerBound, upperBound);

				float normalized = (input - inMin) / (inMax - inMin);

				return normalized * (outMax - outMin) + outMin;
			}
		}

		private void CreateFfbEffectUi(string name, InputJoystickComponent joystickComponent, Widget parent,
			JoystickInfo joystickInfo, FfbEffectType effectType, bool needMagnitude, bool needFrequency,
			bool needDuration)
		{
			var group = new WidgetGroupBox(name) { FontSize = 16, FontOutline = 1 };
			group.SetSpace(30, 0);
			parent.AddChild(group);
			var grid = new WidgetGridBox(3) { FontSize = 16, FontOutline = 1 };
			grid.SetSpace(30, 10);
			group.AddChild(grid);

			var playButton = new WidgetButton("Play Effect") { Toggleable = true };

			CreateFfbEffectParameterSlider("Force", 0, 0, 1, grid, value =>
			{
				effectData[effectType].Force = value;
				if (playButton.Toggled)
				{
					joystickComponent.PlayFfbEffect(effectData[effectType], joystickInfo.Number);
				}
			});

			if (needMagnitude)
			{
				CreateFfbEffectParameterSlider("Magnitude", 0, -1, 1, grid, value =>
				{
					effectData[effectType].Magnitude = value;
					if (playButton.Toggled)
					{
						joystickComponent.PlayFfbEffect(effectData[effectType], joystickInfo.Number);
					}

					if (needDuration)
						rampEffectButtonController =
							new FfbDurationEffectButtonController(effectData[effectType], playButton);
				});
			}

			if (needFrequency)
			{
				CreateFfbEffectParameterSlider("Frequency", 0, 0, 10, grid, value =>
				{
					effectData[effectType].Frequency = Convert.ToUInt32(value);
					if (playButton.Toggled)
					{
						joystickComponent.PlayFfbEffect(effectData[effectType], joystickInfo.Number);
					}

					if (needDuration)
						rampEffectButtonController =
							new FfbDurationEffectButtonController(effectData[effectType], playButton);
				});
			}

			if (needDuration)
			{
				CreateFfbEffectParameterSlider("Duration", 0, 0, 10, grid, value =>
				{
					effectData[effectType].Duration = (ulong)(value * 1_000_000);
					rampEffectButtonController =
						new FfbDurationEffectButtonController(effectData[effectType], playButton);
					if (playButton.Toggled)
					{
						joystickComponent.PlayFfbEffect(effectData[effectType], joystickInfo.Number);
					}
				});
			}


			group.AddChild(playButton, Gui.ALIGN_EXPAND);

			playButton.EventClicked.Connect(() =>
			{
				if (playButton.Toggled)
				{
					joystickComponent.PlayFfbEffect(effectData[effectType], joystickInfo.Number);
					effectData[effectType].IsPlaying = true;
				}
				else if (!playButton.Toggled)
				{
					joystickComponent.StopFfbEffect(effectType, joystickInfo.Joystick);
					effectData[effectType].IsPlaying = false;
				}

				if (needDuration)
					rampEffectButtonController =
						new FfbDurationEffectButtonController(effectData[effectType], playButton);
			});
		}

		private void SetupFfbUi(JoystickInfo joystickInfo, Widget parent, InputJoystickComponent joystickComponent)
		{
			if (joystickInfo.FfbData.AvailableFfbEffects.Count == 0)
				return;

			var scroll = new WidgetScrollBox() { Width = 325, Height = 200 };
			scroll.HScrollEnabled = false;
			parent.AddChild(scroll, Gui.ALIGN_EXPAND);

			foreach (var effectType in joystickInfo.FfbData.AvailableFfbEffects)
			{
				switch (effectType)
				{
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_CONSTANT:
						CreateFfbEffectUi("Constant", joystickComponent, scroll, joystickInfo, effectType, true, false,
							false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_RAMP:
						CreateFfbEffectUi("Ramp", joystickComponent, scroll, joystickInfo, effectType, true, false,
							true);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_SINEWAVE:
						CreateFfbEffectUi("Sine Wave", joystickComponent, scroll, joystickInfo, effectType, true, true,
							false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_SQUAREWAVE:
						CreateFfbEffectUi("Square Wave", joystickComponent, scroll, joystickInfo, effectType, true,
							true, false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_TRIANGLEWAVE:
						CreateFfbEffectUi("Triangle Wave", joystickComponent, scroll, joystickInfo, effectType, true,
							true, false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_SAWTOOTHUPWAVE:
						CreateFfbEffectUi("Saw Tooth Up", joystickComponent, scroll, joystickInfo, effectType, true,
							true, false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_SAWTOOTHDOWNWAVE:
						CreateFfbEffectUi("Saw Tooth Down", joystickComponent, scroll, joystickInfo, effectType, true,
							true, false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_SPRING:
						CreateFfbEffectUi("Spring", joystickComponent, scroll, joystickInfo, effectType, false, false,
							false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_FRICTION:
						CreateFfbEffectUi("Friction", joystickComponent, scroll, joystickInfo, effectType, false, false,
							false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_DAMPER:
						CreateFfbEffectUi("Damper", joystickComponent, scroll, joystickInfo, effectType, false, false,
							false);
						break;
					case FfbEffectType.JOYSTICK_FORCE_FEEDBACK_INERTIA:
						CreateFfbEffectUi("Inertia", joystickComponent, scroll, joystickInfo, effectType, false, false,
							false);
						break;
					case FfbEffectType.NUM_JOYSTICK_FORCE_FEEDBACKS:
					default:
						throw new ArgumentOutOfRangeException(nameof(joystickInfo));
				}
			}
		}
	}

	private List<InfoWidgets> infoWidgets = null;

	private WidgetHBox mainHBox = null;
	private WidgetVBox backgroundVBox = null;

	private WidgetLabel errorMessageLabel = null;
	private WidgetVBox generalMessageInfo = null;
	private WidgetLabel countActiveJoysticksLabel = null;
	private Widget countActiveJoysticksContainer = null;

	private WidgetHBox joysticksInfoBox = null;

	/// <summary>
	/// Initializes the joystick UI elements and sets up event handlers.
	/// </summary>
	[MethodInit(Order = 1)]
	private void Init()
	{
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;

		Gui gui = Gui.GetCurrent();

		mainHBox = new WidgetHBox();

		backgroundVBox = new WidgetVBox { FontSize = 16, FontOutline = 1, Background = 1, Width = 400 };
		backgroundVBox.SetPadding(8, 8, 8, 8);
		mainHBox.AddChild(backgroundVBox, Gui.ALIGN_CENTER);

		var commonInfoContainer = new WidgetHBox();
		backgroundVBox.AddChild(commonInfoContainer);

		errorMessageLabel = new WidgetLabel("ACTIVE JOYSTICKS NOT FOUND") { FontSize = 16, FontOutline = 1, Hidden = true };
		commonInfoContainer.AddChild(errorMessageLabel);

		generalMessageInfo = new WidgetVBox();
		commonInfoContainer.AddChild(generalMessageInfo);

		countActiveJoysticksLabel = new WidgetLabel() { FontSize = 16, FontOutline = 1 };
		countActiveJoysticksContainer = WrapWidget("Active Joysticks:", countActiveJoysticksLabel, generalMessageInfo);

		joysticksInfoBox = new WidgetHBox();
		joysticksInfoBox.SetSpace(30, 0);
		backgroundVBox.AddChild(joysticksInfoBox);

		gui.AddChild(mainHBox, Gui.ALIGN_CENTER);
		backgroundVBox.BackgroundColor = new vec4(0.0f, 0.0f, 0.0f, 0.5f);

		if (joystickComponent == null || joystickComponent.JoysticksCount == 0)
			return;

		errorMessageLabel.Hidden = true;
		generalMessageInfo.Hidden = false;

		countActiveJoysticksLabel.Text = joystickComponent.JoysticksCount.ToString();

		infoWidgets = new List<InfoWidgets>();
		for (int i = 0; i < joystickComponent.JoysticksCount; i++)
		{
			CreateInterface(joystickComponent.JoysticksInfo[i]);
		}
	}

	/// <summary>
	/// Updates joystick status and UI elements each frame.
	/// </summary>
	private void Update()
	{
		if (joystickComponent == null || joystickComponent.JoysticksCount == 0 || !HasActiveJoysticks())
		{
			if (infoWidgets != null)
			{
				foreach (var infoWidget in infoWidgets)
				{
					infoWidget.infoContainer.Hidden = true;
				}
			}
			countActiveJoysticksContainer.Hidden = true;
			errorMessageLabel.Hidden = false;
			return;
		}

		foreach (var infoWidget in infoWidgets)
		{
			infoWidget.infoContainer.Hidden = false;
		}
		countActiveJoysticksContainer.Hidden = false;
		errorMessageLabel.Hidden = true;

		infoWidgets ??= [];
		for (int i = 0; i < joystickComponent.JoysticksCount; i++)
		{
			JoystickInfo info = joystickComponent.JoysticksInfo[i];
			if (i >= infoWidgets.Count)
			{
				CreateInterface(info);
				continue;
			}


			infoWidgets[i].Set(info, joystickComponent);
		}

		foreach (var info in infoWidgets)
		{
			info.Update();
		}

		return;

		bool HasActiveJoysticks()
		{
			return joystickComponent.JoysticksInfo.Any(info => info != null && info.Joystick.IsAvailable);
		}
	}

	/// <summary>
	/// Cleans up UI elements and removes event listeners on shutdown.
	/// </summary>
	private void Shutdown()
	{
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;

		if (mainHBox)
		{
			if (infoWidgets != null)
			{
				foreach (InfoWidgets iw in infoWidgets)
				{
					mainHBox.RemoveChild(iw.infoContainer);
				}
			}

			Gui.GetCurrent().RemoveChild(mainHBox);
		}
	}

	/// <summary>
	/// Handles filter value changes and invokes corresponding events.
	/// </summary>
	private void ChangeFilter(Widget sender)
	{
		int number = 0;
		bool res = int.TryParse(sender.Data, out number);

		WidgetSlider slider = sender as WidgetSlider;
		if (res && slider)
			filterChanged?.Invoke(number, slider.Value / 100.0f);
	}

	private void CreateInterface(JoystickInfo joystickInfo)
	{
		Gui gui = Gui.GetCurrent();

		InfoWidgets info = new InfoWidgets();
		infoWidgets.Add(info);

		joysticksInfoBox.AddChild(info.infoVBox);
		info.filterSlider.EventChanged.Connect(ChangeFilter);
		info.Set(joystickInfo, joystickComponent);
	}
}
