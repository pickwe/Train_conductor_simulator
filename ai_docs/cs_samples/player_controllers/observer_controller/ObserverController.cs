// Editor-style camera controller with multiple movement modes.
// Supports: spectator (free-fly), rail (zoom along view direction), panning (strafe),
// and focusing (zoom to clicked object). Features gear-based speed presets (1/2/3 keys)
// with Alt modifier for mode switching. Includes a toggleable HUD for position/speed display.

using System;
using System.Collections.Generic;
using Unigine;
using System.Linq;

#region Math
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Multi-mode camera controller with state machine for movement behaviors.
public partial class ObserverController : Component
{
	// ═══════════════════════════════════════════════════════════════════════════════
	// INPUT SETTINGS - Keyboard, mouse, and modifier bindings for camera control
	// ═══════════════════════════════════════════════════════════════════════════════

	// Key to show/hide the on-screen HUD with position and speed info
	[ShowInEditor, Parameter(Group = "Input", Title = "Toggle Camera Menu", Tooltip = "Key to toggle the visibility of the camera menu.")]
	Input.KEY toggleCameraMenu = Input.KEY.F3;

	// Key to zoom camera to clicked object (raycast to find target)
	[ShowInEditor, Parameter(Group = "Input", Title = "Focusing Key", Tooltip = "Key to focus the camera on the object, zooming in for a closer view.")]
	Input.KEY focusKey = Input.KEY.F;

	// Modifier that switches mouse buttons to Rail/Panning modes instead of Spectator
	[ShowInEditor, Parameter(Group = "Input", Title = "Camera Control Modifier", Tooltip = "Modifier key for switching between different camera control modes.")]
	Input.MODIFIER altCameraModKey = Input.MODIFIER.ANY_ALT;

	// Modifier for speed boost (doubles current gear velocity)
	[ShowInEditor, Parameter(Group = "Input", Title = "Acceleration Key", Tooltip = "Key to activate acceleration mode, typically used for faster movement.")]
	Input.KEY accelerationKey = Input.KEY.ANY_SHIFT;

	// Button for free-fly spectator mode (no Alt modifier)
	[ShowInEditor, Parameter(Group = "Input", Title = "Spectator Mode Mouse Button", Tooltip = "Mouse button to toggle spectator mode.")]
	Input.MOUSE_BUTTON spectatorModeMouseButton = Input.MOUSE_BUTTON.RIGHT;

	// Button for rail mode - zoom along view direction (with Alt modifier)
	[ShowInEditor, Parameter(Group = "Input", Title = "Rail Mode Mouse Button", Tooltip = "Mouse button to activate rail mode.")]
	Input.MOUSE_BUTTON railModeMouseButton = Input.MOUSE_BUTTON.RIGHT;

	// Button for panning mode - strafe perpendicular to view (with Alt modifier)
	[ShowInEditor, Parameter(Group = "Input", Title = "Panning Mode Mouse Button", Tooltip = "Mouse button to initiate panning mode.")]
	Input.MOUSE_BUTTON panningModeMouseButton = Input.MOUSE_BUTTON.MIDDLE;

	// Sensitivity multiplier for Rail and Panning mouse movements
	[ShowInEditor, Parameter(Group = "Input", Title = "Panning/Rail Scale", Tooltip = "Scale factor for panning and rail modes, controlling the movement speed.")]
	float panningRailScale = 0.01f;

	// ═══════════════════════════════════════════════════════════════════════════════
	// VELOCITY SETTINGS - Speed presets (gears) for different navigation needs
	// Switch gears with number keys 1/2/3 for slow/medium/fast movement
	// ═══════════════════════════════════════════════════════════════════════════════

	// First gear (slow) - for precise positioning
	[ShowInEditor, Parameter(Group = "Velocity", Title = "First Gear Key", Tooltip = "Key to activate the first gear (low speed).")]
	Input.KEY firstGearKey = Input.KEY.DIGIT_1;

	[ShowInEditor, Parameter(Group = "Velocity", Title = "First Gear Speed", Tooltip = "Velocity for the first gear.")]
	float firstGearVelocity = 5.0f;

	// Second gear (medium) - for general navigation
	[ShowInEditor, Parameter(Group = "Velocity", Title = "Second Gear Key", Tooltip = "Key to activate the second gear (medium speed).")]
	Input.KEY secondGearKey = Input.KEY.DIGIT_2;

	[ShowInEditor, Parameter(Group = "Velocity", Title = "Second Gear Speed", Tooltip = "Velocity for the second gear.")]
	float secondGearVelocity = 50.0f;

	// Third gear (fast) - for large scene traversal
	[ShowInEditor, Parameter(Group = "Velocity", Title = "Third Gear Key", Tooltip = "Key to activate the third gear (high speed).")]
	Input.KEY thirdGearKey = Input.KEY.DIGIT_3;

	[ShowInEditor, Parameter(Group = "Velocity", Title = "Third Gear Speed", Tooltip = "Velocity for the third gear.")]
	float thirdGearVelocity = 500.0f;

	// Speed multiplier when acceleration key is held
	[ShowInEditor, Parameter(Group = "Velocity", Title = "Acceleration Multiplier", Tooltip = "Multiplier for acceleration when the assigned key is held.")]
	float accelerationMultiplier = 2.0f;

	// ═══════════════════════════════════════════════════════════════════════════════
	// RUNTIME STATE - Internal variables for controller operation
	// ═══════════════════════════════════════════════════════════════════════════════

	// Default values for UI text fields when empty
	float _defaultVelocity = 5.0f;
	float _defaultPositionValue = 0f;

	// Focusing mode flags
	bool _tryEndFocusing = false;    // Signals focusing animation complete
	bool _editText = false;          // True while user is typing in UI text field

	// HUD widget references for runtime updates
	WidgetHBox _menuLayout = null;                    // Main menu container
	WidgetCheckBox _firstGearCheckbox = null;         // Gear 1 radio button
	WidgetCheckBox _secondGearCheckbox = null;        // Gear 2 radio button
	WidgetCheckBox _thirdGearCheckBox = null;         // Gear 3 radio button
	List<WidgetEditLine> _widgetEditLines = [];       // All editable text fields (for focus management)
	WidgetEditLine _currentGearVelocityTextField = null;  // Velocity input field
	WidgetEditLine _widgetCameraPositionX = null;     // Position X input
	WidgetEditLine _widgetCameraPositionY = null;     // Position Y input
	WidgetEditLine _widgetCameraPositionZ = null;     // Position Z input

	// Raycast result for focus targeting
	WorldIntersection _intersection = new WorldIntersection();

	// Target position for focus animation (lerps camera to this point)
	Vec3 _targetPoint;

	// Reference to the underlying PlayerSpectator for movement control
	PlayerSpectator _playerSpectator = null;

	// Saved mouse grab state for cleanup on shutdown
	bool _enterMouseGrabMode = false;

	// ═══════════════════════════════════════════════════════════════════════════════
	// STATE MACHINE TYPES - Defines movement modes and transition logic
	// ═══════════════════════════════════════════════════════════════════════════════

	// Available camera movement modes
	public enum PlayerMovementState
	{
		IDLE,       // No active movement - waiting for input
		SPECTATOR,  // Free-fly mode (WASD + mouse look)
		RAIL,       // Zoom along view direction (Alt + RMB drag)
		FOCUSING,   // Animated zoom to clicked object
		PANNING     // Strafe perpendicular to view (Alt + MMB drag)
	}

	// Current active movement mode
	PlayerMovementState _playerState = PlayerMovementState.IDLE;

	// Defines a condition that triggers transition to a new state
	private readonly struct StateTransition(Func<bool> condition, PlayerMovementState tragetState)
	{
		public Func<bool> Condition { get; } = condition;      // Returns true when transition should occur
		public PlayerMovementState TargetState { get; } = tragetState;  // State to transition to
	}

	// Complete definition of a movement state with lifecycle callbacks
	private readonly struct MovementState(IEnumerable<StateTransition> transitions, Action onEnter = null, Action onExit = null, Action onUpdate = null)
	{
		public Action OnEnter { get; } = onEnter;    // Called once when entering this state
		public Action OnExit { get; } = onExit;      // Called once when leaving this state
		public Action OnUpdate { get; } = onUpdate;  // Called every frame while in this state

		public List<StateTransition> Transitions { get; } = transitions.ToList();  // Possible exits from this state
	}

	// Maps each movement state to its definition (transitions + callbacks)
	private Dictionary<PlayerMovementState, MovementState> _stateMap = null;

	// Speed presets selectable via number keys
	public enum VelocityGear
	{
		GEAR_FIRST = 1,   // Slow - precise positioning
		GEAR_SECOND,      // Medium - general navigation
		GEAR_THIRD        // Fast - large scene traversal
	}

	// Currently active speed preset
	VelocityGear _velocityGear = VelocityGear.GEAR_FIRST;

	// ═══════════════════════════════════════════════════════════════════════════════
	// INPUT CONDITION PROPERTIES - Evaluate input for state transitions
	// ═══════════════════════════════════════════════════════════════════════════════

	// F key pressed - start focusing on clicked object
	bool TryFocusing => Input.IsKeyDown(focusKey);

	// RMB without Alt - enter free-fly spectator mode
	bool TryEnterSpectatorMode => !Input.IsModifierEnabled(altCameraModKey) && Input.IsMouseButtonPressed(spectatorModeMouseButton);

	// Alt + RMB - enter rail mode (zoom along view axis)
	bool TryEnterRailMode => Input.IsModifierEnabled(altCameraModKey) && Input.IsMouseButtonPressed(railModeMouseButton);

	// Alt + MMB - enter panning mode (strafe perpendicular to view)
	bool TryEnterPannigMode => Input.IsModifierEnabled(altCameraModKey) && Input.IsMouseButtonPressed(panningModeMouseButton);

	// RMB released - exit rail mode
	bool TryExitRailMode => !Input.IsMouseButtonPressed(railModeMouseButton);

	// MMB released - exit panning mode
	bool TryExitPanningMode => !Input.IsMouseButtonPressed(panningModeMouseButton);

	// Sets up state machine, initializes gear, and creates HUD widgets.
	void Init()
	{
		// Get reference to PlayerSpectator for movement control
		_playerSpectator = Game.Player as PlayerSpectator;

		// Save mouse state for cleanup
		_enterMouseGrabMode = Input.MouseGrab;

		// Initialize state machine with all states and transitions
		CreateStateTable();
		// Apply initial velocity settings
		UpdateGear(_velocityGear);
		// Build on-screen HUD
		GenerateMenu();
	}

	// Main update loop - processes state machine and UI updates.
	void Update()
	{
		// Skip all input while console is open
		if (Unigine.Console.Active)
			return;

		// While editing a text field, only handle Enter key submission
		if (_editText)
		{
			UpdateEditFieldSubmission();
			return;
		}

		// Check for gear switch key presses (1/2/3)
		UpdateVelocityGear();
		// Run state machine (check transitions, call update callbacks)
		UpdateStates();
		// Refresh position display in HUD
		UpdateMenuParams();
	}

	// Cleans up HUD and restores mouse state.
	void Shutdown()
	{
		_menuLayout.DeleteLater();
		Input.MouseGrab = _enterMouseGrabMode;
	}

	// This method sets up the state transitions and associated actions for each movement state of the player.
	// Each state (e.g., IDLE, SPECTATOR, FOCUSING, etc.) is mapped to a set of conditions for state transitions, 
	// as well as initialization, update, and end functions for when the state is entered or exited.
	private void CreateStateTable()
	{
		_stateMap = new Dictionary<PlayerMovementState, MovementState>
		{
			// IDLE: Default state - waits for user input to enter a movement mode
			[PlayerMovementState.IDLE] = new MovementState
			(
				transitions:
				[
					new (() => TryFocusing, PlayerMovementState.FOCUSING),
					new (() => TryEnterSpectatorMode, PlayerMovementState.SPECTATOR),
					new (() => TryEnterRailMode, PlayerMovementState.RAIL),
					new (() => TryEnterPannigMode, PlayerMovementState.PANNING)
				]
			),

			// FOCUSING: Animated zoom to clicked object
			// Can be interrupted by entering other modes
			[PlayerMovementState.FOCUSING] = new MovementState
			(
				transitions:
				[
					new (() => _tryEndFocusing, PlayerMovementState.IDLE),
					new (() => TryEnterSpectatorMode, PlayerMovementState.SPECTATOR),
					new (() => TryEnterPannigMode, PlayerMovementState.PANNING),
					new (() => TryEnterRailMode, PlayerMovementState.RAIL)
				],
				onEnter: InitFocusing,
				onExit: EndFocusing,
				onUpdate: UpdateFocusing
			),

			// SPECTATOR: Free-fly camera with WASD + mouse look
			// Exits when RMB released
			[PlayerMovementState.SPECTATOR] = new MovementState
			(
				transitions:
				[
					new (() => !TryEnterSpectatorMode, PlayerMovementState.IDLE)
				],
				onEnter: InitSpectator,
				onExit: EndSpectator,
				onUpdate: UpdateSpectator
			),

			// RAIL: Zoom along view direction via mouse drag
			// Exits when RMB released
			[PlayerMovementState.RAIL] = new MovementState
			(
				transitions:
				[
					new (() => TryExitRailMode, PlayerMovementState.IDLE)
				],
				onUpdate: UpdateRail
			),

			// PANNING: Strafe camera via mouse drag
			// Exits when MMB released
			[PlayerMovementState.PANNING] = new MovementState
			(
				transitions:
				[
					new (() => TryExitPanningMode, PlayerMovementState.IDLE)
				],
				onEnter: InitPanning,
				onExit: EndPanning,
				onUpdate: UpdatePanning
			)
		};
	}

	// Evaluates all transitions for current state, switches if condition met.
	// If no transition, calls the current state's onUpdate callback.
	private void UpdateStates()
	{
		var state = _stateMap[_playerState];

		// Check transitions in order - first match wins
		foreach (var transition in state.Transitions)
		{
			if (transition.Condition())
			{
				SwitchState(transition.TargetState);
				return;
			}
		}
		// No transition triggered - run update logic for current state
		state.OnUpdate?.Invoke();
	}

	// Transitions from current state to newState, calling exit/enter callbacks.
	private void SwitchState(PlayerMovementState newState)
	{
		if (_playerState == newState)
			return;

		// Call exit callback for old state
		_stateMap[_playerState].OnExit?.Invoke();
		_playerState = newState;
		// Call enter callback for new state
		_stateMap[_playerState].OnEnter?.Invoke();
	}

	// Checks for gear switch key presses (1/2/3) and updates UI checkboxes.
	// Checkbox change events trigger the actual gear switch.
	void UpdateVelocityGear()
	{
		if (Input.IsKeyDown(firstGearKey))
		{
			_firstGearCheckbox.Checked = true;
		}

		if (Input.IsKeyDown(secondGearKey))
		{
			_secondGearCheckbox.Checked = true;
		}

		if (Input.IsKeyDown(thirdGearKey))
		{
			_thirdGearCheckBox.Checked = true;
		}
	}

	// Applies a gear preset to the PlayerSpectator velocity settings.
	// MinVelocity = base speed, MaxVelocity = accelerated speed (with Shift)
	void UpdateGear(VelocityGear newGear)
	{
		_velocityGear = newGear;
		_playerSpectator.MinVelocity = GetVelocity();
		_playerSpectator.MaxVelocity = GetVelocityAcceleration();
	}

	// Returns the base velocity for the currently selected gear.
	float GetVelocity()
	{
		return _velocityGear switch
		{
			VelocityGear.GEAR_FIRST => firstGearVelocity,
			VelocityGear.GEAR_SECOND => secondGearVelocity,
			VelocityGear.GEAR_THIRD => thirdGearVelocity,
			_ => 0.0f
		};
	}

	// Returns accelerated velocity (base * multiplier) for Shift-boosted movement.
	float GetVelocityAcceleration()
	{
		return GetVelocity() * accelerationMultiplier;
	}

	// Updates velocity for a specific gear (called from UI text field).
	void SetVelocity(VelocityGear targetGear, float velocity)
	{
		switch (targetGear)
		{
			case VelocityGear.GEAR_FIRST:
				firstGearVelocity = velocity;
				break;
			case VelocityGear.GEAR_SECOND:
				secondGearVelocity = velocity;
				break;
			case VelocityGear.GEAR_THIRD:
				thirdGearVelocity = velocity;
				break;
		}
		// Reapply to PlayerSpectator
		UpdateGear(targetGear);
	}

	// --- SPECTATOR MODE: Free-fly camera with WASD + mouse look ---

	// Enables PlayerSpectator built-in controls.
	void InitSpectator()
	{
		_playerSpectator.Controlled = true;
		ControlsApp.MouseEnabled = true;
	}

	// Hides cursor during spectator mode.
	void UpdateSpectator()
	{
		Input.MouseCursorHide = true;
	}

	// Disables PlayerSpectator controls when exiting.
	void EndSpectator()
	{
		_playerSpectator.Controlled = false;
	}

	// --- RAIL MODE: Zoom along view direction via mouse drag ---

	// Moves camera forward/backward based on combined mouse delta.
	// Shift key toggles between base and accelerated speed.
	void UpdateRail()
	{
		Input.MouseCursorHide = true;

		ivec2 mouseDelta = Input.MouseDeltaPosition;

		// Normalize to prevent speed variance based on drag distance
		if (mouseDelta != ivec2.ZERO)
			MathLib.Normalize(mouseDelta);

		// Shift inverts speed logic (base when held, accelerated when not)
		float currentAcceleration = Input.IsKeyPressed(accelerationKey) ? GetVelocity() : GetVelocityAcceleration();

		// Combined X+Y delta controls zoom (negative = zoom out on drag down/right)
		float delta = -(mouseDelta.x + mouseDelta.y) * currentAcceleration * ControlsApp.MouseSensitivity * panningRailScale;

		// Move along local Z axis (view direction)
		_playerSpectator.Translate(new Vec3(0, 0, delta));
	}

	// --- PANNING MODE: Strafe camera via mouse drag ---

	// Grabs mouse for smooth delta tracking.
	void InitPanning()
	{
		Input.MouseGrab = true;
		ControlsApp.MouseEnabled = true;
	}

	// Moves camera perpendicular to view based on mouse delta.
	void UpdatePanning()
	{
		ivec2 mouse_delta = Input.MouseDeltaPosition;

		float current_acceleration = Input.IsKeyPressed(accelerationKey) ? GetVelocity() : GetVelocityAcceleration();

		// X delta = strafe left/right, Y delta = strafe up/down
		_playerSpectator.Translate(new Vec3(-mouse_delta.x, mouse_delta.y, 0) * current_acceleration * ControlsApp.MouseSensitivity * panningRailScale);

		Input.MouseCursorHide = true;
	}

	// Releases mouse grab when exiting panning.
	void EndPanning()
	{
		Input.MouseGrab = false;
	}

	// --- FOCUSING MODE: Animated zoom to clicked object ---

	// Raycasts from cursor position to find target object.
	// Calculates target position at 2x object radius distance.
	void InitFocusing()
	{
		// Get ray from camera through cursor position
		_playerSpectator.GetDirectionFromMainWindow(out Vec3 startPoint, out Vec3 endPoint, Input.MousePosition.x, Input.MousePosition.y);

		Vec3 objPosition;
		double focusRadius;

		// Raycast to find clicked object
		Node obj = World.GetIntersection(startPoint, endPoint, ~0, _intersection);

		if (!obj)
		{
			// No object hit - immediately exit focusing
			_tryEndFocusing = true;
			return;
		}

		// Default: use object's world position and bounding sphere
		objPosition = obj.WorldPosition;
		focusRadius = obj.WorldBoundSphere.Radius;

		// Special handling for ObjectMeshCluster instances
		// These are batched meshes where individual instances have separate transforms
		if (obj as ObjectMeshCluster != null)
		{
			ObjectMeshCluster cluster = obj as ObjectMeshCluster;
			int instanceIndex = _intersection.Instance;

			Mesh mesh = cluster.GetMeshCurrentRAM();
			Mat4 transform = cluster.GetMeshTransform(instanceIndex);

			// Get world position of the specific instance
			objPosition = cluster.WorldTransform * transform.Translate;

			// Calculate radius accounting for both instance and cluster scale
			focusRadius = mesh.BoundSphere.Radius * cluster.Scale.Maximum * transform.Scale.Maximum;
		}

		// Position camera at 2x radius distance from object center (along view direction)
		_targetPoint = objPosition - (Vec3)(_playerSpectator.GetWorldDirection() * focusRadius * 2);
	}

	// Smoothly interpolates camera position towards target.
	void UpdateFocusing()
	{
		// Lerp camera position towards target
		_playerSpectator.WorldPosition = MathLib.Lerp(_playerSpectator.WorldPosition, _targetPoint, Game.IFps * 5);

		// Check if close enough to target (squared distance for performance)
		if (MathLib.Distance2(_playerSpectator.WorldPosition, _targetPoint) < 0.01f)
			_tryEndFocusing = true;
	}

	// Resets focusing flag for next use.
	void EndFocusing()
	{
		_tryEndFocusing = false;
	}

	// Creates the on-screen HUD with speed/gear controls and position display.
	// Layout: [Speed: [value] [1] [2] [3]] | [X: [val] Y: [val] Z: [val]]
	void GenerateMenu()
	{
		// Main horizontal container anchored to top of screen
		_menuLayout = new WidgetHBox(0, 4)
		{
			Background = 1,
		};
		Gui.GetCurrent().AddChild(_menuLayout, Gui.ALIGN_TOP);

		// === Speed/Gear section ===
		WidgetHBox gearsLayout = new (5, 0);
		_menuLayout.AddChild(gearsLayout,Gui.ALIGN_LEFT);

		WidgetLabel labelName = new WidgetLabel("Player speed:");
		gearsLayout.AddChild(labelName, Gui.ALIGN_LEFT);

		// Editable velocity field - updates current gear when focus lost
		_currentGearVelocityTextField = new WidgetEditLine(GetVelocity().ToString("F5"))
		{
			Validator = 3,  // Numeric input only
			Width = 100
		};
		gearsLayout.AddChild(_currentGearVelocityTextField, Gui.ALIGN_LEFT);
		_widgetEditLines.Add(_currentGearVelocityTextField);
		_currentGearVelocityTextField.EventFocusIn.Connect(() => _editText = true);
		_currentGearVelocityTextField.EventFocusOut.Connect(() =>
		{
			_editText = false;
			// Apply new velocity (or default if empty)
			SetVelocity
			(
				_velocityGear,
				_currentGearVelocityTextField.Text.Length == 0 ? _defaultVelocity : float.Parse(_currentGearVelocityTextField.Text)
			);
		});

		// Gear radio buttons (mutually exclusive via AddAttach)
		_firstGearCheckbox = new WidgetCheckBox("1")
		{
			Checked = true
		};
		gearsLayout.AddChild(_firstGearCheckbox, Gui.ALIGN_LEFT);
		_firstGearCheckbox.EventChanged.Connect(() =>
		{
			if (_firstGearCheckbox.Checked)
				ChangeGearTextField(VelocityGear.GEAR_FIRST);
		});

		_secondGearCheckbox = new WidgetCheckBox("2");
		_firstGearCheckbox.AddAttach(_secondGearCheckbox);  // Links as radio group
		gearsLayout.AddChild(_secondGearCheckbox, Gui.ALIGN_LEFT);
		_secondGearCheckbox.EventChanged.Connect(() =>
		{
			if (_secondGearCheckbox.Checked)
				ChangeGearTextField(VelocityGear.GEAR_SECOND);
		});

		_thirdGearCheckBox = new WidgetCheckBox("3");
		_firstGearCheckbox.AddAttach(_thirdGearCheckBox);
		gearsLayout.AddChild(_thirdGearCheckBox, Gui.ALIGN_LEFT);
		_thirdGearCheckBox.EventChanged.Connect(() =>
		{
			if (_thirdGearCheckBox.Checked)
				ChangeGearTextField(VelocityGear.GEAR_THIRD);
		});

		// Visual separator between gear and position sections
		WidgetSpacer gearSpacer = new WidgetSpacer()
		{
			Orientation = 0
		};
		_menuLayout.AddChild(gearSpacer,Gui.ALIGN_LEFT);

		// === Position display section ===
		WidgetHBox positionLayout = new WidgetHBox(5,0);
		_menuLayout.AddChild(positionLayout, Gui.ALIGN_LEFT);

		// X position - editable, updates camera on key press
		labelName = new WidgetLabel("X:");
		positionLayout.AddChild(labelName, Gui.ALIGN_LEFT);

		_widgetCameraPositionX = new WidgetEditLine(_playerSpectator.WorldPosition.x.ToString("F5"))
		{
			Validator = 3,
			Width = 100
		};
		positionLayout.AddChild(_widgetCameraPositionX, Gui.ALIGN_LEFT);
		_widgetEditLines.Add(_widgetCameraPositionX);
		_widgetCameraPositionX.EventFocusIn.Connect(() =>  _editText = true);
		_widgetCameraPositionX.EventKeyPressed.Connect(() =>
		{
			float value = _widgetCameraPositionX.Text.Length == 0 ? _defaultPositionValue : float.Parse(_widgetCameraPositionX.Text);
			Vec3 position = _playerSpectator.WorldPosition;
			_playerSpectator.WorldPosition = new Vec3(value, position.y, position.z);
		});
		_widgetCameraPositionX.EventFocusOut.Connect(() =>
		{
			_editText = false;
			if (_widgetCameraPositionX.Text.Length == 0)
				_widgetCameraPositionX.Text = _defaultPositionValue.ToString("F5");
		});

		// Y position
		labelName = new WidgetLabel("Y:");
		positionLayout.AddChild(labelName, Gui.ALIGN_LEFT);

		_widgetCameraPositionY = new WidgetEditLine(_playerSpectator.WorldPosition.y.ToString("F5"))
		{
			Validator = 3,
			Width = 100
		};
		positionLayout.AddChild(_widgetCameraPositionY, Gui.ALIGN_LEFT);
		_widgetEditLines.Add(_widgetCameraPositionY);
		_widgetCameraPositionY.EventFocusIn.Connect(() => _editText = true);
		_widgetCameraPositionY.EventKeyPressed.Connect(() =>
		{
			float value = _widgetCameraPositionY.Text.Length == 0 ? _defaultPositionValue : float.Parse(_widgetCameraPositionY.Text);
			Vec3 position = _playerSpectator.WorldPosition;
			_playerSpectator.WorldPosition = new Vec3(position.x, value, position.z);
		});
		_widgetCameraPositionY.EventFocusOut.Connect(() =>
		{
			_editText = false;
			if (_widgetCameraPositionY.Text.Length == 0)
			_widgetCameraPositionY.Text = _defaultPositionValue.ToString("F5");
		});

		// Z position
		labelName = new WidgetLabel("Z:");
		positionLayout.AddChild(labelName, Gui.ALIGN_LEFT);

		_widgetCameraPositionZ = new WidgetEditLine(_playerSpectator.WorldPosition.z.ToString("F5"))
		{
			Validator = 3,
			Width = 100
		};
		positionLayout.AddChild(_widgetCameraPositionZ, Gui.ALIGN_LEFT);
		_widgetEditLines.Add(_widgetCameraPositionZ);
		_widgetCameraPositionZ.EventFocusIn.Connect(() => _editText = true);
		_widgetCameraPositionZ.EventKeyPressed.Connect(() =>
		{
			float value = _widgetCameraPositionZ.Text.Length == 0 ? _defaultPositionValue : float.Parse(_widgetCameraPositionZ.Text);
			Vec3 position = _playerSpectator.WorldPosition;
			_playerSpectator.WorldPosition = new Vec3(position.x, position.y, value);
		});
		_widgetCameraPositionZ.EventFocusOut.Connect(() =>
		{
			_editText = false;
			if (_widgetCameraPositionY.Text.Length == 0)
				_widgetCameraPositionY.Text = _defaultPositionValue.ToString("F5");
		});
	}

	// Updates position display and handles menu toggle key.
	void UpdateMenuParams()
	{
		// F3 toggles menu visibility
		if (Input.IsKeyDown(toggleCameraMenu))
			_menuLayout.Hidden = !_menuLayout.Hidden;

		// Update position fields with current camera position
		_widgetCameraPositionX.Text = _playerSpectator.WorldPosition.x.ToString("F5");
		_widgetCameraPositionY.Text = _playerSpectator.WorldPosition.y.ToString("F5");
		_widgetCameraPositionZ.Text = _playerSpectator.WorldPosition.z.ToString("F5");
	}

	// Handles Enter key to submit text field edits.
	void UpdateEditFieldSubmission()
	{
		// Enter key removes focus from all edit fields (triggers their FocusOut events)
		if (_widgetEditLines.Count != 0 && Input.IsKeyDown(Input.KEY.ENTER))
		{
			foreach (var widget in _widgetEditLines)
				widget.RemoveFocus();
		}
	}

	// Updates gear and syncs velocity display when gear radio button changes.
	void ChangeGearTextField(VelocityGear newGear)
	{
		UpdateGear(newGear);
		_currentGearVelocityTextField.Text = GetVelocity().ToString();
	}
}
