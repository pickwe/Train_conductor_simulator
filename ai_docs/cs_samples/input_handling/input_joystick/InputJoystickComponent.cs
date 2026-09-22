// Manages joystick input detection and force feedback effects. Tracks connected
// joysticks, reads button/axis/POV states, and provides force feedback playback
// for devices that support it (wheels, flight sticks, etc.).

using System;
using System.Collections.Generic;
using Unigine;

// Tracks joystick connections and provides force feedback control.
public partial class InputJoystickComponent : Component
{
	// Number of currently tracked joysticks
	public int JoysticksCount { get { return Joysticks.Count; } }

	/// <summary>
	/// Holds force feedback effect data for joysticks.
	/// </summary>
	public class FfbEffectData
	{
		public Input.JOYSTICK_FORCE_FEEDBACK_EFFECT EffectType =
			Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_CONSTANT;

		public float Force = 0;
		public bool IsPlaying = false;
		public float? Magnitude = null;
		public uint? Frequency = null;
		public ulong? Duration = null;
	}

	/// <summary>
	/// Stores available force feedback effects for each joystick instance.
	/// </summary>
	public class FfbData
	{
		public FfbData() { }

		public FfbData(InputJoystick joystick)
		{
			// Check which force feedback effects are supported by the joystick.
			for (Input.JOYSTICK_FORCE_FEEDBACK_EFFECT i = 0;
			     i < Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.NUM_JOYSTICK_FORCE_FEEDBACKS; ++i)
			{
				if (joystick.IsForceFeedbackEffectSupported(i))
				{
					AvailableFfbEffects.Add(i);
				}
			}
		}

		public readonly List<Input.JOYSTICK_FORCE_FEEDBACK_EFFECT> AvailableFfbEffects = [];
	}

	/// <summary>
	/// Stores joystick details including button, axis, and force feedback data.
	/// </summary>
	public class JoystickInfo
	{
		public string Name;
		public int Number;
		public int NumButtons;
		public int NumAxes;
		public int NumPovs;
		public bool IsAvailable;
		public float Filter;
		public List<string> ButtonsName;
		public List<string> AxesName;
		public List<string> PovsName;
		public int? LastPressedButton;
		public List<float> Axes;
		public List<Input.JOYSTICK_POV> Povs;
		public InputJoystick Joystick;
		public FfbData FfbData;
	}

	private List<InputJoystick> Joysticks { get; set; } = null;
	public List<JoystickInfo> JoysticksInfo { get; private set; } = null;

	/// <summary>
	/// Plays a specified force feedback effect on a given joystick.
	/// Ensure the joystick is available and supports the effect type.
	/// <param name="data">Values of force desired force feedback effect</param>
	/// <param name="joystickId">Id of joystick, on which effect will be played</param>
	/// </summary>
	public void PlayFfbEffect(FfbEffectData data, int joystickId)
	{
		// To start FFB effect, we need a joystick to start it on
		var joystick = Joysticks[joystickId];

		if (!joystick.IsAvailable || !joystick.IsForceFeedbackEffectSupported(data.EffectType))
			return;

		// Apply the correct force feedback effect based on type.
		// Every force feedback has different arguments, but in this example we use single class to pass all data
		// So here we check if required values is present, but if you call this function directly, you don't need to check each value before call
		// For example: joystick.PlayForceFeedbackEffectConstant(0.1, -0.3);
		switch (data.EffectType)
		{
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_CONSTANT:
				if (!data.Magnitude.HasValue)
					return;
				joystick.PlayForceFeedbackEffectConstant(data.Force);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_DAMPER:
				joystick.PlayForceFeedbackEffectDamper(data.Force);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_FRICTION:
				joystick.PlayForceFeedbackEffectFriction(data.Force);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_INERTIA:
				joystick.PlayForceFeedbackEffectInertia(data.Force);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_RAMP:
				if (!data.Magnitude.HasValue || !data.Duration.HasValue)
					return;
				joystick.PlayForceFeedbackEffectRamp(data.Force, data.Duration.Value);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_SPRING:
				joystick.PlayForceFeedbackEffectSpring(data.Force);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_SAWTOOTHDOWNWAVE:
				if (!data.Frequency.HasValue)
					return;
				joystick.PlayForceFeedbackEffectSawtoothDownWave(data.Force, data.Frequency.Value);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_SAWTOOTHUPWAVE:
				if (!data.Frequency.HasValue)
					return;
				joystick.PlayForceFeedbackEffectSawtoothUpWave(data.Force, data.Frequency.Value);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_SINEWAVE:
				if (!data.Frequency.HasValue)
					return;
				joystick.PlayForceFeedbackEffectSineWave(data.Force, data.Frequency.Value);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_SQUAREWAVE:
				if (!data.Frequency.HasValue)
					return;
				joystick.PlayForceFeedbackEffectSquareWave(data.Force, data.Frequency.Value);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.JOYSTICK_FORCE_FEEDBACK_TRIANGLEWAVE:
				if (!data.Frequency.HasValue)
					return;
				joystick.PlayForceFeedbackEffectTriangleWave(data.Force, data.Frequency.Value);
				break;
			case Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.NUM_JOYSTICK_FORCE_FEEDBACKS:
			default:
				throw new ArgumentException(null, nameof(data));
		}
	}

	/// <summary>
	/// Stops the specified force feedback effect if supported by the joystick.
	/// <param name="effectType">Type of effect to be stopped</param>
	/// <param name="joystick">Joystick on which effect will be stopped</param>
	/// </summary>
	public void StopFfbEffect(Input.JOYSTICK_FORCE_FEEDBACK_EFFECT effectType, InputJoystick joystick)
	{
		if (joystick.IsAvailable && joystick.IsForceFeedbackEffectSupported(effectType))
			joystick.StopForceFeedbackEffect(effectType);
	}

	// Handles joystick connection by adding to tracked list.
	private void OnJoystickConnected(int number)
	{
		InputJoystick joystick = Input.GetJoystick(number);
		if (!joystick)
			return;
		AddJoystick(joystick);
	}

	// Handles joystick disconnection by clearing its state.
	private void OnJoystickDisconnected(int number)
	{
		InputJoystick joystick = Input.GetJoystick(number);
		if (!joystick)
			return;
		ClearJoystick(joystick, number);
	}

	// Connection events are registered and initial joysticks are detected.
	private void Init()
	{
		// Register event listeners for joystick connections.
		Input.EventJoyConnected.Connect(OnJoystickConnected);
		Input.EventJoyDisconnected.Connect(OnJoystickDisconnected);

		Joysticks = new List<InputJoystick>();
		JoysticksInfo = new List<JoystickInfo>();
		// Initialize available joysticks.
		for (int i = 0; i < Input.NumJoysticks; i++)
		{
			// Get joystick
			InputJoystick joystick = Input.GetJoystick(i);
			if (!joystick)
				break;

			AddJoystick(joystick);
		}

		InputJoystickUI.filterChanged += OnFilterChanged;
	}

	/// <summary>
	/// Updates joystick input states each frame.
	/// </summary>
	private void Update()
	{
		// Refresh joystick status and button/axis states.
		for (int i = 0; i < Joysticks.Count; i++)
		{
			JoysticksInfo[i].IsAvailable = Joysticks[i].IsAvailable;
			if (!JoysticksInfo[i].IsAvailable)
				continue;

			JoysticksInfo[i].Filter = Joysticks[i].Filter;

			// Detect last pressed button.
			for (int j = 0; j < Joysticks[i].NumButtons; j++)
			{
				if (!Joysticks[i].IsButtonPressed((uint)j))
					continue;

				JoysticksInfo[i].LastPressedButton = j;
				break;
			}

			// Update axes
			for (int j = 0; j < Joysticks[i].NumAxes; j++)
				JoysticksInfo[i].Axes[j] = Joysticks[i].GetAxis((uint)j);

			// Update POVs
			for (int j = 0; j < Joysticks[i].NumPovs; j++)
				JoysticksInfo[i].Povs[j] = Joysticks[i].GetPov((uint)j);
		}
	}

	/// <summary>
	/// Cleans up resources when shutting down.
	/// </summary>
	private void Shutdown()
	{
		InputJoystickUI.filterChanged -= OnFilterChanged;

		// Disable all force feedback effects
		for (Input.JOYSTICK_FORCE_FEEDBACK_EFFECT i = 0;
		     i < Input.JOYSTICK_FORCE_FEEDBACK_EFFECT.NUM_JOYSTICK_FORCE_FEEDBACKS;
		     ++i)
		{
			foreach (var joystick in Joysticks)
			{
				if (joystick && joystick.IsAvailable && joystick.IsForceFeedbackEffectSupported(i))
					joystick.StopForceFeedbackEffect(i);
			}
		}
	}

	// Applies filter value to specified joystick.
	private void OnFilterChanged(int number, float value)
	{
		if (0 <= number && number < Joysticks.Count)
			Joysticks[number].Filter = value;
	}

	// Adds joystick to tracked list and initializes its state info.
	private void AddJoystick(InputJoystick joystick)
	{
		JoystickInfo info = null;

		if (joystick.Number >= 0 && joystick.Number < Joysticks.Count)
		{
			info = JoysticksInfo[joystick.Number];
		}
		else
		{
			// Add joystick info and set static fields
			info = new JoystickInfo();
			JoysticksInfo.Add(info);
			Joysticks.Add(joystick);
		}

		info.Joystick = joystick;
		info.Name = joystick.Name;
		info.Number = joystick.Number;
		info.IsAvailable = joystick.IsAvailable;
		info.NumButtons = joystick.NumButtons;
		info.NumAxes = joystick.NumAxes;
		info.NumPovs = joystick.NumPovs;

		// Set buttons names
		info.ButtonsName = new List<string>();
		for (uint j = 0; j < joystick.NumButtons; j++)
			info.ButtonsName.Add(joystick.GetButtonName(j));

		// Set axes names and axes values
		info.AxesName = new List<string>();
		info.Axes = new List<float>();
		for (uint j = 0; j < joystick.NumAxes; j++)
		{
			info.AxesName.Add(joystick.GetAxisName(j));
			info.Axes.Add(0.0f);
		}

		// Set POVs names and POVs values
		info.PovsName = new List<string>();
		info.Povs = new List<Input.JOYSTICK_POV>();
		for (uint j = 0; j < joystick.NumPovs; j++)
		{
			info.PovsName.Add(joystick.GetPovName(j));
			info.Povs.Add(Input.JOYSTICK_POV.NOT_PRESSED);
		}

		info.FfbData = new FfbData(joystick);
	}

	// Clears joystick state when disconnected.
	private void ClearJoystick(InputJoystick joystick, int number)
	{
		if (number < 0 && number >= Joysticks.Count)
			return;

		JoystickInfo info = JoysticksInfo[number];

		info.Name = joystick.Name;
		info.Number = number;
		info.IsAvailable = joystick.IsAvailable;
		info.NumButtons = joystick.NumButtons;
		info.NumAxes = joystick.NumAxes;
		info.NumPovs = joystick.NumPovs;
		info.LastPressedButton = null;

		// Set buttons names
		info.ButtonsName = new List<string>();
		for (uint j = 0; j < joystick.NumButtons; j++)
			info.ButtonsName.Add(joystick.GetButtonName(j));

		// Set axes names and axes values
		info.AxesName = new List<string>();
		info.Axes = new List<float>();
		for (uint j = 0; j < joystick.NumAxes; j++)
		{
			info.AxesName.Add(joystick.GetAxisName(j));
			info.Axes.Add(0.0f);
		}

		// Set POVs names and POVs values
		info.PovsName = new List<string>();
		info.Povs = new List<Input.JOYSTICK_POV>();
		for (uint j = 0; j < joystick.NumPovs; j++)
		{
			info.PovsName.Add(joystick.GetPovName(j));
			info.Povs.Add(Input.JOYSTICK_POV.NOT_PRESSED);
		}
	}
}
