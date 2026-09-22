// Manages gamepad input detection and state tracking. Monitors connected gamepads,
// tracks button states, analog stick axes, triggers, and touchpad input. Provides
// filter and vibration control through UI event callbacks.

using System;
using System.Collections.Generic;
using Unigine;

// Tracks gamepad connections and input state for all connected controllers.
public partial class InputGamePadComponent : Component
{
	// Total number of gamepads detected
	public int CountGamePads { get; private set; } = 0;
	// Number of currently active gamepads
	public int CountActiveGamePads { get; private set; } = 0;

	// Container for individual gamepad state information
	public class GamepadInfo
	{
		public string name;
		public int number;
		public int playerIndex;
		public Input.DEVICE deviceType;
		public InputGamePad.MODEL_TYPE modelType;
		public bool isAvailable;
		public float filter;
		public Input.GAMEPAD_BUTTON? lastButtonDown;
		public Input.GAMEPAD_BUTTON? lastButtonPressed;
		public Input.GAMEPAD_BUTTON? lastButtonUp;
		public vec2 axesLeft;
		public vec2 axesLeftLastDelta;
		public vec2 axesRight;
		public vec2 axesRightLastDelta;
		public float triggerLeft;
		public float triggerLeftLastDelta;
		public float triggerRight;
		public float triggerRightLastDelta;
	}

	// List of currently active gamepad instances
	private List<InputGamePad> activeGamepads = null;

	// Public access to gamepad state information
	public List<GamepadInfo> GamepadsInfo { get; private set; } = null;

	// Cached array of all gamepad button types
	private Array buttons = null;

	// Callback invoked when touchpad input is detected
	public Action<InputGamePad, int> onTouch;

	// Gamepad collections and UI event handlers are initialized.
	private void Init()
	{
		// Get all available gamepad buttons for iteration
		buttons = Enum.GetValues(typeof(Input.GAMEPAD_BUTTON));

		// Initialize collections for active gamepads and their states
		activeGamepads = new List<InputGamePad>();
		GamepadsInfo = new List<GamepadInfo>();

		InputGamepadUI.filterChanged += OnFilterChanged;
		InputGamepadUI.setVibration += OnSetVibration;
	}

	// Gamepad states are updated and input is polled each frame.
	[MethodUpdate(Order=1)]
	private void Update()
	{
		// Rebuild gamepad list when count changes
		if (CountActiveGamePads != Input.NumGamePads)
		{
			activeGamepads.Clear();
			GamepadsInfo.Clear();

			CountActiveGamePads = Input.NumGamePads;
			for (int i = 0; i < CountActiveGamePads; i++)
			{
				InputGamePad gamepad = Input.GetGamePad(i);
				activeGamepads.Add(gamepad);

				GamepadInfo info = new GamepadInfo()
				{
					number = gamepad.Number,
					isAvailable = gamepad.IsAvailable
				};
				GamepadsInfo.Add(info);
			}
		}

		// Update information about gamepads
		for (int i = 0; i < CountActiveGamePads; i++)
		{
			GamepadsInfo[i].name = activeGamepads[i].Name;

			GamepadsInfo[i].playerIndex = activeGamepads[i].PlayerIndex;
			GamepadsInfo[i].deviceType = activeGamepads[i].DeviceType;
			GamepadsInfo[i].modelType = activeGamepads[i].ModelType;

			GamepadsInfo[i].isAvailable = activeGamepads[i].IsAvailable;

			if (!GamepadsInfo[i].isAvailable)
				continue;

			GamepadsInfo[i].filter = activeGamepads[i].Filter;

			foreach (var button in buttons)
			{
				Input.GAMEPAD_BUTTON currentButton = (Input.GAMEPAD_BUTTON)button;
				if (currentButton == Input.GAMEPAD_BUTTON.NUM_GAMEPAD_BUTTONS)
					continue;

				// Update buttons
				if (activeGamepads[i].IsButtonDown(currentButton))
					GamepadsInfo[i].lastButtonDown = currentButton;

				if (activeGamepads[i].IsButtonPressed(currentButton))
					GamepadsInfo[i].lastButtonPressed = currentButton;

				if (activeGamepads[i].IsButtonUp(currentButton))
					GamepadsInfo[i].lastButtonUp = currentButton;
			}

			// Update axes and deltas
			GamepadsInfo[i].axesLeft = activeGamepads[i].AxesLeft;
			if (activeGamepads[i].AxesLeftDelta.Length2 > 0.0f)
				GamepadsInfo[i].axesLeftLastDelta = activeGamepads[i].AxesLeftDelta;

			GamepadsInfo[i].axesRight = activeGamepads[i].AxesRight;
			if (activeGamepads[i].AxesRightDelta.Length2 > 0.0f)
				GamepadsInfo[i].axesRightLastDelta = activeGamepads[i].AxesRightDelta;

			// Update triggers and deltas
			GamepadsInfo[i].triggerLeft = activeGamepads[i].TriggerLeft;
			if (activeGamepads[i].TriggerLeftDelta > 0.0f)
				GamepadsInfo[i].triggerLeftLastDelta = activeGamepads[i].TriggerLeftDelta;

			GamepadsInfo[i].triggerRight = activeGamepads[i].TriggerRight;
			if (activeGamepads[i].TriggerRightDelta > 0.0f)
				GamepadsInfo[i].triggerRightLastDelta = activeGamepads[i].TriggerRightDelta;

			if (activeGamepads[i].NumTouches > 0)
			{
				onTouch.Invoke(activeGamepads[i], i);
			}
		}
	}

	// UI event handlers are unsubscribed on shutdown.
	private void Shutdown()
	{
		InputGamepadUI.filterChanged -= OnFilterChanged;
		InputGamepadUI.setVibration -= OnSetVibration;
	}

	// Applies filter value to specified gamepad.
	private void OnFilterChanged(int number, float value)
	{
		if (0 <= number && number < activeGamepads.Count)
			activeGamepads[number].Filter = value;

		Log.MessageLine($"{value}");
	}

	// Triggers vibration on specified gamepad with given parameters.
	private void OnSetVibration(int number, float lowFrequency, float highFrequency, float duration)
	{
		if (0 <= number && number < activeGamepads.Count)
			activeGamepads[number].SetVibration(lowFrequency, highFrequency, duration);
	}

}
