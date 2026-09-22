// UI controller for the SpectatorController demo.
// Provides sliders for adjusting mouse sensitivity, turning speed, movement velocity,
// and sprint velocity, plus toggles for input control and collision detection.

using Unigine;
using System.Globalization;

// Parameter panel for configuring SpectatorController settings at runtime.
public partial class SpectatorControllerSample : Component
{
	// UI window manager for parameter sliders and toggles
	private SampleDescriptionWindow _sampleDescriptionWindow = new();
	// Original mouse mode to restore on shutdown
	private Input.MOUSE_HANDLE _mouseHandler = Input.MOUSE_HANDLE.USER;

	// Toggle states mirrored from controller
	private bool _isControlled = false;
	private bool _isCollided = false;

	// Mouse sensitivity: min/max bounds and current slider value
	private float _minMouseSensetivity = 0.1f;
	private float _maxMouseSensetivity = 1.0f;
	private float _currentMouseSensetivity = 0.0f;

	// Arrow key rotation speed: min/max bounds and current slider value
	private float _minTurning = 15.0f;
	private float _maxTurning = 120.0f;
	private float _currentTurning = 0.0f;

	// Base movement velocity: min/max bounds and current slider value
	private float _minVelocity = 1.0f;
	private float _maxVelocity = 4.0f;
	private float _currentVelocity = 0.0f;

	// Sprint velocity: min/max bounds and current slider value
	private float _minSprintVelocity = 5.0f;
	private float _maxSprintVelocity = 10.0f;
	private float _currentSprintVelocity = 0.0f;

	// Reference to the SpectatorController being configured
	private SpectatorController _controller;

	// Creates UI window with sliders and toggles bound to controller properties.
	private void Init()
	{
		_mouseHandler = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;

		_controller = ComponentSystem.GetComponent<SpectatorController>(Game.Player);

		_isControlled = _controller.isControlled;
		_isCollided = _controller.isCollided;
		_currentMouseSensetivity = _controller.mouseSensitivity;
		_currentTurning = _controller.turning;
		_currentVelocity = _controller.velocity;
		_currentSprintVelocity = _controller.sprintVelocity;

		_sampleDescriptionWindow.createWindow();

		_sampleDescriptionWindow.addFloatParameter(
			"Mouse sensetivity",
			"Сontrols the mouse sensetivity",
			_currentMouseSensetivity,
			_minMouseSensetivity,
			_maxMouseSensetivity,
			(float v) =>
			{
				_currentMouseSensetivity = v;
				_controller.mouseSensitivity = _currentMouseSensetivity;
			});

		_sampleDescriptionWindow.addFloatParameter(
		"Turning speed",
		"Controls the turning speed using the arrow keys.",
		_currentTurning,
		_minTurning,
		_maxTurning,
		(float v) =>
		{
			_currentTurning = v;
			_controller.turning = _currentTurning;
		});

		_sampleDescriptionWindow.addFloatParameter(
		"Velocity",
		"Controls the base velocity",
		_currentVelocity,
		_minVelocity,
		_maxVelocity,
		(float v) =>
		{
			_currentVelocity = v;
			_controller.velocity = _currentVelocity;
		});

		_sampleDescriptionWindow.addFloatParameter(
			"Sprint velocity",
			"Controls the sprint velocity",
			_currentSprintVelocity,
			_minSprintVelocity,
			_maxSprintVelocity,
			(float v) =>
			{
				_currentSprintVelocity = v;
				_controller.sprintVelocity = _currentSprintVelocity;
			});

		_sampleDescriptionWindow.addBoolParameter(
			"Set camera receive inputs",
			"Controls whether the camera receives input.",
			_isControlled,
			(bool k) =>
			{
				_isControlled = k;
				_controller.isControlled = _isControlled;
			});

		_sampleDescriptionWindow.addBoolParameter(
			"Set camera collision",
			"Controls whether the camera collision enabled",
			_isControlled,
			(bool k) =>
			{
				_isCollided = k;
				_controller.isCollided = _isCollided;
			});
	}

	// Destroys UI window and restores original mouse handling mode.
	private void Shutdown()
	{
		_sampleDescriptionWindow.shutdown();
		Input.MouseHandle = _mouseHandler;
	}
}
