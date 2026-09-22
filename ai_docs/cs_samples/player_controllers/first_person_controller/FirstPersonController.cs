// Comprehensive first-person character controller with physics-based movement.
// Features: WASD movement, mouse look, jumping, crouching, running, auto-stepping (stairs),
// slope handling, collision detection with capsule shape, and rigid body interaction.
// Supports keyboard and gamepad input with extensive customization options.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

using System.Collections.Generic;
using Unigine;

// Physics-based first-person controller with capsule collision and movement states.
public partial class FirstPersonController : Component
{
	#region Editor parameters
	// ═══════════════════════════════════════════════════════════════════════════
	// INPUT SETTINGS - Keyboard and mouse bindings for player control
	// ═══════════════════════════════════════════════════════════════════════════

	[ShowInEditor]
	[Parameter(Group = "Input", Tooltip = "Move forward key")]
	private Input.KEY forwardKey = Input.KEY.W;

	[ShowInEditor]
	[Parameter(Group = "Input", Tooltip = "Move backward key")]
	private Input.KEY backwardKey = Input.KEY.S;

	[ShowInEditor]
	[Parameter(Group = "Input", Tooltip = "Move right key")]
	private Input.KEY rightKey = Input.KEY.D;

	[ShowInEditor]
	[Parameter(Group = "Input", Tooltip = "Move left key")]
	private Input.KEY leftKey = Input.KEY.A;

	[ShowInEditor]
	[Parameter(Group = "Input", Tooltip = "Run mode activation key")]
	private Input.KEY runKey = Input.KEY.ANY_SHIFT;

	[ShowInEditor]
	[Parameter(Group = "Input", Tooltip = "Jump key")]
	private Input.KEY jumpKey = Input.KEY.SPACE;

	[ShowInEditor]
	[Parameter(Group = "Input", Tooltip = "Crouch mode activation key")]
	private Input.KEY crouchKey = Input.KEY.ANY_CTRL;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Input", Tooltip = "Mouse sensitivity multiplier")]
	private float mouseSensitivity = 1.0f;

	// ═══════════════════════════════════════════════════════════════════════════
	// GAMEPAD SETTINGS - Controller stick assignments and button mappings
	// ═══════════════════════════════════════════════════════════════════════════

	public enum GamepadStickSide
	{
		LEFT = 0,
		RIGHT
	}

	[ShowInEditor]
	[Parameter(Group = "Gamepad Input", Tooltip = "Horizontal movement stick")]
	private GamepadStickSide moveStick = GamepadStickSide.LEFT;

	[ShowInEditor]
	[Parameter(Group = "Gamepad Input", Tooltip = "Camera rotation stick")]
	private GamepadStickSide cameraStick = GamepadStickSide.RIGHT;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Gamepad Input", Tooltip = "Sensitivity of the camera stick")]
	private float cameraStickSensitivity = 0.8f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Gamepad Input", Tooltip = "Deadzone deviation, set for both sticks")]
	private float sticksDeadZone = 0.3f;

	[ShowInEditor]
	[Parameter(Group = "Gamepad Input", Tooltip = "Run mode activation button")]
	private Input.GAMEPAD_BUTTON runButton = Input.GAMEPAD_BUTTON.SHOULDER_RIGHT;

	[ShowInEditor]
	[Parameter(Group = "Gamepad Input", Tooltip = "Jump button")]
	private Input.GAMEPAD_BUTTON jumpButton = Input.GAMEPAD_BUTTON.A;

	[ShowInEditor]
	[Parameter(Group = "Gamepad Input", Tooltip = "Crouch mode activation button")]
	private Input.GAMEPAD_BUTTON crouchButton = Input.GAMEPAD_BUTTON.SHOULDER_LEFT;

	// ═══════════════════════════════════════════════════════════════════════════
	// BODY SETTINGS - Capsule collider dimensions and physics masks
	// The player uses a capsule shape for collision detection (cylinder + hemispheres)
	// ═══════════════════════════════════════════════════════════════════════════

	[ShowInEditor]
	[Parameter(Group = "Body", Tooltip = "Use the body and shape of the current node.\nIf this option is enabled, the node should have\na dummy body and a capsule shape assigned.\n" +
		"If the custom body or shape has invalid settings,\na default body and capsule shape are created\nand used instead.")]
	private bool useObjectBody = false;

	[ShowInEditor]
	[ParameterSlider(Group = "Body", Tooltip = "Radius of the capsule shape")]
	[ParameterCondition(nameof(useObjectBody), 0)]
	private float capsuleRadius = 0.3f;

	[ShowInEditor]
	[ParameterSlider(Group = "Body", Tooltip = "Height of the capsule shape (cylindrical part only)")]
	[ParameterCondition(nameof(useObjectBody), 0)]
	private float capsuleHeight = 1.15f;

	[ShowInEditor]
	[ParameterMask(Group = "Body", MaskType = ParameterMaskAttribute.TYPE.PHYSICS_INTERSECTION, Tooltip = "Mask used for selective detection of physics intersections\n(with other physical objects having bodies and collider shapes,\nor ray intersections with collider geometry)")]
	[ParameterCondition(nameof(useObjectBody), 0)]
	private int physicsIntersectionMask = 1;

	[ShowInEditor]
	[ParameterMask(Group = "Body", MaskType = ParameterMaskAttribute.TYPE.COLLISION, Tooltip = "Mask used for selective collision detection")]
	[ParameterCondition(nameof(useObjectBody), 0)]
	private int collisionMask = 1;

	[ShowInEditor]
	[ParameterMask(Group = "Body", MaskType = ParameterMaskAttribute.TYPE.COLLISION, Tooltip = "Mask used for exclusion of collision detection")]
	[ParameterCondition(nameof(useObjectBody), 0)]
	private int exclusionMask = 0;

	// ═══════════════════════════════════════════════════════════════════════════
	// CAMERA SETTINGS - First-person camera configuration and viewing parameters
	// Camera can be auto-created, externally assigned, or disabled for custom use
	// ═══════════════════════════════════════════════════════════════════════════

	public enum CameraMode
	{
		NONE = 0,              // No camera - use your own camera system
		CREATE_AUTOMATICALLY,  // Auto-create PlayerDummy as main camera
		USE_EXTERNAL           // Use externally assigned PlayerDummy
	}

	[ShowInEditor]
	[Parameter(Group = "Camera", Tooltip = "The mode of assigning the camera to the player:\n\nNONE — camera is not created. The user implements its own camera,\nthis component is used for the movement logic only.\n\n" + 
		"CREATE_AUTOMATICALLY — camera is created automatically\nat initialization of the component and set as the main camera.\nThis camera uses the values set for the parameters in this section.\n\n" + 
		"USE_EXTERNAL — a Player Dummy assigned via the Editor.")]
	private CameraMode cameraMode = CameraMode.CREATE_AUTOMATICALLY;

	[ShowInEditor]
	[Parameter(Group = "Camera", Tooltip = "Drag the camera node from the World Nodes hierarchy here")]
	[ParameterCondition(nameof(cameraMode), (int)CameraMode.USE_EXTERNAL)]
	private PlayerDummy camera = null;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Max = 180.0f, Group = "Camera", Tooltip = "Field of View")]
	[ParameterCondition(nameof(cameraMode), (int)CameraMode.CREATE_AUTOMATICALLY)]
	private float fov = 60.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Camera", Tooltip = "Distance to the near clipping plane of the player's viewing frustum, in units.")]
	[ParameterCondition(nameof(cameraMode), (int)CameraMode.CREATE_AUTOMATICALLY)]
	private float nearClipping = 0.01f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Camera", Tooltip = "Distance to the far clipping plane of the player's viewing frustum, in units.")]
	[ParameterCondition(nameof(cameraMode), (int)CameraMode.CREATE_AUTOMATICALLY)]
	private float farClipping = 1000.0f;

	[ShowInEditor]
	[Parameter(Group = "Camera", Tooltip = "Distance from the camera to the player's position.")]
	[ParameterCondition(nameof(cameraMode), (int)CameraMode.CREATE_AUTOMATICALLY)]
	private vec3 cameraPositionOffset = new vec3(0.0f, 0.0f, 1.65f);

	[ShowInEditor]
	[ParameterSlider(Min = -89.9f, Max = 89.9f, Group = "Camera", Tooltip = "The minimum vertical angle of the camera, i.e. maximum possible angle to look down.")]
	[ParameterCondition(nameof(cameraMode), (int)CameraMode.CREATE_AUTOMATICALLY, (int)CameraMode.USE_EXTERNAL)]
	private float minVerticalAngle = -89.9f;

	[ShowInEditor]
	[ParameterSlider(Min = -89.9f, Max = 89.9f, Group = "Camera", Tooltip = "The maximum vertical angle of the camera, i.e. maximum possible angle to look up.")]
	[ParameterCondition(nameof(cameraMode), (int)CameraMode.CREATE_AUTOMATICALLY, (int)CameraMode.USE_EXTERNAL)]
	private float maxVerticalAngle = 89.9f;

	// ═══════════════════════════════════════════════════════════════════════════
	// MOVEMENT SETTINGS - Speed, acceleration, damping for walk/run/crouch/jump
	// Movement uses physics-based velocity with separate ground and air parameters
	// ═══════════════════════════════════════════════════════════════════════════

	[ShowInEditor]
	[Parameter(Group = "Movement", Tooltip = "Enable jumping and all relevant parameters in the player logic")]
	private bool useJump = true;

	[ShowInEditor]
	[Parameter(Group = "Movement", Tooltip = "Enable crouching and all relevant parameters in the player logic")]
	private bool useCrouch = true;

	[ShowInEditor]
	[Parameter(Group = "Movement", Tooltip = "Enable running and all relevant parameters in the player logic")]
	private bool useRun = true;

	[ShowInEditor]
	[Parameter(Group = "Movement", Tooltip = "Set running as a default state of the player's movement.\nIf this state is enabled, the walk state is enabled by using the Run Key.")]
	[ParameterCondition(nameof(useRun), 1)]
	private bool useRunDefault = false;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Crouching speed of the player")]
	[ParameterCondition(nameof(useCrouch), 1)]
	private float crouchSpeed = 1.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Walking speed of the player")]
	private float walkSpeed = 3.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Running speed of the player")]
	[ParameterCondition(nameof(useRun), 1)]
	private float runSpeed = 6.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Horizontal acceleration applied when the player is on the ground")]
	private float groundAcceleration = 25.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Horizontal acceleration applied when the player is in the air")]
	private float airAcceleration = 10.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Damping of the horizontal speed when the player is on the ground")]
	private float groundDamping = 25.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Damping of the horizontal speed when the player is in the air")]
	private float airDamping = 0.05f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Jump power of the player in the Run, Walk and Idle modes")]
	[ParameterCondition(nameof(useJump), 1)]
	private float jumpPower = 6.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Jump power of the player in the Crouch mode")]
	[ParameterCondition(nameof(useJump), 1)]
	[ParameterCondition(nameof(useCrouch), 1)]
	private float crouchJumpPower = 3.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "The height of the camera when the player is crouching.\nThe minimum value equals to capsule radius x 2,\nthe maximum height is equal to the capsule height.")]
	[ParameterCondition(nameof(useCrouch), 1)]
	private float crouchHeight = 1.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Movement", Tooltip = "Time required for transition between the crouching and standing states")]
	[ParameterCondition(nameof(useCrouch), 1)]
	private float crouchTransitionTime = 0.3f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Max = 90.0f, Group = "Movement", Tooltip = "Maximum surface angle up to which the player is considered to be standing on the ground")]
	private float maxGroundAngle = 60.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Max = 90.0f, Group = "Movement", Tooltip = "Maximum surface angle up to which the player is considered to be touching the ceiling")]
	private float maxCeilingAngle = 60.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Max = 0.1f, Group = "Movement", Tooltip = "Offset of the ray that checks the surface it would potentially move to")]
	private float checkMoveRayOffset = 0.01f;

	[ShowInEditor]
	[ParameterMask(Group = "Movement", MaskType = ParameterMaskAttribute.TYPE.INTERSECTION, Tooltip = "Mask used to sort out intersections with the surface")]
	private int checkMoveMask = 1;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Max = 90.0f, Group = "Movement", Tooltip = "Horizontal angle between the direction ray and contact point of the capsule with the wall,\nwithin which the player doesn't slide along the wall.")]
	private float wallStopSlidingAngle = 15.0f;

	// ═══════════════════════════════════════════════════════════════════════════
	// AUTO-STEPPING SETTINGS - Automatic stair/obstacle climbing configuration
	// Allows smooth traversal of stairs without explicit jumping
	// Uses 3-phase approach: move up -> move forward -> settle down
	// ═══════════════════════════════════════════════════════════════════════════

	[ShowInEditor]
	[Parameter(Group = "Auto Stepping", Tooltip = "Automatic climbing upon obstacles up to a certain height (stairs, for example)")]
	private bool useAutoStepping = true;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Auto Stepping", Tooltip = "Minimum height of an obstacle that can be climbed upon.\nAllows avoiding false positives on flat surfaces.")]
	[ParameterCondition(nameof(useAutoStepping), 1)]
	private float minStepHeight = 0.05f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Auto Stepping", Tooltip = "Maximum height of an obstacle that can be climbed upon")]
	[ParameterCondition(nameof(useAutoStepping), 1)]
	private float maxStepHeight = 0.3f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Auto Stepping", Tooltip = "Maximum stair angle. If the character steps on a surface with a slope,\nthe angle of which is greater than the Max Stair Angle value,\nthis last step is canceled.")]
	[ParameterCondition(nameof(useAutoStepping), 1)]
	private float maxStairAngle = 30.0f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Group = "Auto Stepping", Tooltip = "Offset of the ray that checks the level of the potential stair for Auto Stepping.\nThis offset is vertical and in the direction of the potential movement.")]
	[ParameterCondition(nameof(useAutoStepping), 1)]
	private float checkStairRayOffset = 0.1f;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Max = 90.0f, Group = "Auto Stepping", Tooltip = "Horizontal angle that defines the area for checking the contacts\nof the capsule with other objects in the movement direction.\n" +
		"Allows avoiding false Auto Stepping near walls.")]
	[ParameterCondition(nameof(useAutoStepping), 1)]
	private float stairsDetectionAngle = 45.0f;

	[ShowInEditor]
	[ParameterMask(Group = "Auto Stepping", MaskType = ParameterMaskAttribute.TYPE.INTERSECTION, Tooltip = "The mask for checking intersections with the stairs for Auto Stepping.\n" +
		"If you have stairs with a complex geometry and want to simplify it for the Auto Stepping process,\ndisable this mask for the complex geometry and enable for the simplified model.")]
	private int stairDetectionMask = 1;

	// ═══════════════════════════════════════════════════════════════════════════
	// PHYSICS INTERACTION - Player can push rigid bodies in the scene
	// Uses a cylinder shape slightly larger than capsule to detect contacts
	// ═══════════════════════════════════════════════════════════════════════════

	[ShowInEditor]
	[Parameter(Group = "Objects Interaction", Tooltip = "Toggles physical interaction with rigid bodies on anf off")]
	private bool useObjectsInteraction = true;

	[ShowInEditor]
	[ParameterSlider(Min = 0.0f, Max = 1.0f, Group = "Objects Interaction", Tooltip = "Multiplier for the impulse applied to the Rigid Body the player collides with")]
	[ParameterCondition(nameof(useObjectsInteraction), 1)]
	private float impulseMultiplier = 0.05f;

	// ═══════════════════════════════════════════════════════════════════════════
	// ADVANCED SETTINGS - Simulation quality and performance tuning
	// Higher values = more accurate collision resolution but more CPU cost
	// ═══════════════════════════════════════════════════════════════════════════

	[ShowInEditor]
	[ParameterSlider(Min = 15, Max = 240, Group = "Advanced Settings", Tooltip = "Minimum update framerate for the player.\n" +
	"If the current FPS is less than this value, the player is updated several times per frame.")]
	private int playerFps = 60;

	[ShowInEditor]
	[ParameterSlider(Min = 1, Max = 128, Group = "Advanced Settings", Tooltip = "Number of iterations for resolving collisions")]
	private int collisionIterations = 4;

	[ShowInEditor]
	[ParameterSlider(Min = 4, Max = 1000, Group = "Advanced Settings", Tooltip = "Maximum number of contacts processed for a collision")]
	private int contactsBufferSize = 16;

	[ShowInEditor]
	[ParameterSlider(Min = 4, Max = 1000, Group = "Advanced Settings", Tooltip = "Maximum number of contacts up to which the Collision Iterations value is applied at processing contacts.\n" +
		"If the number of contacts exceeds this value, then only one iteration is used to avoid performance drop.")]
	private int heavyContactsCount = 100;

#if DEBUG
	public struct DebugCamera
	{
		[Parameter(Tooltip = "Enable debug camera — a third-person camera that observes the player.\nControls: cursor arrows and +/- for zooming.")]
		public bool enabled;

		[Parameter(Tooltip = "Debug camera with fixed angles — is positioned behind the player and in the player's direction")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool useFixedAngles;

		[HideInEditor] public PlayerDummy camera;
		[HideInEditor] public float angularSpeed;
		[HideInEditor] public float zoomSpeed;
		[HideInEditor] public float maxDistance;
		[HideInEditor] public float horizontalAngle;
		[HideInEditor] public float verticalAngle;
		[HideInEditor] public float distance;
	}

	[ShowInEditor]
	[Parameter(Group = "Debug")]
	private DebugCamera debugCamera;

	public struct DebugVisualizer
	{
		[Parameter(Tooltip = "Enable visualizer")]
		public bool enabled;

		[Parameter(Tooltip = "Visualisation of polygons")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool triangles;

		[Parameter(Tooltip = "Visualisation of shapes")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool shapes;

		[Parameter(Tooltip = "Visualisation of the player's capsule")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool playerShape;

		[Parameter(Tooltip = "Arrow that shows the player's direction, i.e. its Y axis")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool playerDirection;

		[Parameter(Tooltip = "Visualisation of the player's camera frustum and direction")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool camera;

		[Parameter(Tooltip = "Visualisation of the basis of the surface the player is standing on")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool slopeBasis;

		[Parameter(Tooltip = "Visualisation of the current horizontal speed vector applied to the player")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool appliedHorizontalVelocity;

		[Parameter(Tooltip = "Visualisation of the current vertical speed vector applied to the player")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool appliedVerticalVelocity;

		[Parameter(Tooltip = "Visualisation of contacts during the upward phase of Auto Stepping")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool upPassContacts;

		[Parameter(Tooltip = "Visualisation of contacts during the horizontal movement to any side")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool sidePassContacts;

		[Parameter(Tooltip = "Visualisation of contacts during the downward phase of Auto Stepping")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool downPassContacts;

		[Parameter(Tooltip = "Visualisation of the ray that checks the surface to perform potential movement")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool checkMoveRay;

		[Parameter(Tooltip = "Visualisation of the ray that checks obstacles during Auto Stepping")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool stairDetectionRay;
	}

	[ShowInEditor]
	[Parameter(Group = "Debug")]
#pragma warning disable CS0649
	private DebugVisualizer debugVisualizer;
#pragma warning restore CS0649

	public struct DebugProfiler
	{
		[Parameter(Tooltip = "Enable Profiler")]
		public bool enabled;

		[Parameter(Tooltip = "Scalar value of the horizontal velocity applied to the player")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool appliedHorizontalSpeed;

		[Parameter(Tooltip = "Scalar value of the vertical velocity applied to the player")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool appliedVerticalSpeed;

		[Parameter(Tooltip = "Contacts during the upward phase of Auto Stepping")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool upPassContact;

		[Parameter(Tooltip = "Contacts during the horizontal movement to any side")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool sidePassContact;

		[Parameter(Tooltip = "Contacts during the downward phase of Auto Stepping")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool downPassContact;

		[Parameter(Tooltip = "Changes in the state of touching the ground")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool isGround;

		[Parameter(Tooltip = "Changes in the state of touching the ceiling")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool isCeiling;

		[Parameter(Tooltip = "Changes in the state of crouching")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool isCrouch;

		[Parameter(Tooltip = "Average player's speed, which is calculated based on the coordinates changes")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool averageSpeed;

		[Parameter(Tooltip = "Application of Auto Stepping")]
		[ParameterCondition(nameof(enabled), 1)]
		public bool autoStepping;
	}

	[ShowInEditor]
	[Parameter(Group = "Debug")]
#pragma warning disable CS0649
	private DebugProfiler debugProfiler;
#pragma warning restore CS0649

	public struct DebugColors
	{
		[ParameterColor] public vec4 playerShape;
		[ParameterColor] public vec4 playerDirection;
		[ParameterColor] public vec4 cameraColor;
		[ParameterColor] public vec4 appliedHorizontalVelocity;
		[ParameterColor] public vec4 appliedVerticalVelocity;
		[ParameterColor] public vec4 upPassContacts;
		[ParameterColor] public vec4 sidePassContacts;
		[ParameterColor] public vec4 downPassContacts;
		[ParameterColor] public vec4 isGround;
		[ParameterColor] public vec4 isCeiling;
		[ParameterColor] public vec4 isCrouch;
		[ParameterColor] public vec4 averageSpeed;
		[ParameterColor] public vec4 autoStepping;

		[HideInEditor] public float[] arrayAppliedHorizontalVelocity;
		[HideInEditor] public float[] arrayAppliedVerticalVelocity;
		[HideInEditor] public float[] arrayUpPassContacts;
		[HideInEditor] public float[] arraySidePassContacts;
		[HideInEditor] public float[] arrayDownPassContacts;
		[HideInEditor] public float[] arrayIsGround;
		[HideInEditor] public float[] arrayIsCeiling;
		[HideInEditor] public float[] arrayIsCrouch;
		[HideInEditor] public float[] arrayAverageSpeed;
		[HideInEditor] public float[] arrayAutoStepping;

		public DebugColors(bool useDefault)
		{
			playerShape = new vec4(0.0f, 0.0f, 1.0f, 0.098f);
			playerDirection = vec4.YELLOW;
			cameraColor = vec4.CYAN;
			appliedHorizontalVelocity = vec4.BLACK;
			appliedVerticalVelocity = vec4.GREY;
			upPassContacts = vec4.CYAN;
			sidePassContacts = vec4.MAGENTA;
			downPassContacts = vec4.RED;
			isGround = vec4.RED;
			isCeiling = vec4.GREEN;
			isCrouch = vec4.BLUE;
			averageSpeed = vec4.YELLOW;
			autoStepping = new vec4(0.66f, 0.33f, 0.0f, 1.0f);

			arrayAppliedHorizontalVelocity = null;
			arrayAppliedVerticalVelocity = null;
			arrayUpPassContacts = null;
			arraySidePassContacts = null;
			arrayDownPassContacts = null;
			arrayIsGround = null;
			arrayIsCeiling = null;
			arrayIsCrouch = null;
			arrayAverageSpeed = null;
			arrayAutoStepping = null;
		}
	}

	[ShowInEditor]
	[Parameter(Group = "Debug")]
	private DebugColors debugColors = new DebugColors(true);
#endif

	#endregion editor parameters

	// ═══════════════════════════════════════════════════════════════════════════════
	// PUBLIC PROPERTIES - Read-only state accessible from external scripts
	// Use these to query player state for animations, effects, game logic, etc.
	// ═══════════════════════════════════════════════════════════════════════════════
	#region Player properties

	// True after Init() completes successfully
	public bool IsInitialized { get; private set; } = false;

	// External camera offset (for weapon sway, head bob, etc.)
	public vec3 AdditionalCameraOffset { get; set; } = vec3.ZERO;

	// External camera rotation (for recoil, look-at effects, etc.)
	public quat AdditionalCameraRotation { get; set; } = quat.IDENTITY;

	// True when standing on a surface within maxGroundAngle
	public bool IsGround { get; private set; } = false;
	// True when touching ceiling within maxCeilingAngle (stops upward velocity)
	public bool IsCeiling { get; private set; } = false;
	// True when in crouch state or transitioning to/from crouch
	public bool IsCrouch { get; private set; } = false;
	// True when velocity is below frozen threshold (prevents slope sliding)
	public bool IsHorizontalFrozen { get; private set; } = false;

	// Normal vector of the surface player is standing on (UP when in air)
	public Vec3 SlopeNormal { get; private set; } = Vec3.UP;
	// Right axis in slope-local coordinate system
	public Vec3 SlopeAxisX { get; private set; } = Vec3.RIGHT;
	// Forward axis in slope-local coordinate system
	public Vec3 SlopeAxisY { get; private set; } = Vec3.FORWARD;

	// Current horizontal velocity vector (in slope basis when grounded)
	public Vec3 HorizontalVelocity { get; private set; } = Vec3.ZERO;
	// Current vertical velocity (positive = up, negative = down/falling)
	public float VerticalVelocity { get; private set; } = 0.0f;

	#endregion Player properties

	// ═══════════════════════════════════════════════════════════════════════════════
	// PRIVATE RUNTIME STATE - Internal variables for physics simulation
	// ═══════════════════════════════════════════════════════════════════════════════

	// Direction vectors for input processing (Y = forward, X = right)
	static private readonly vec2 forward = new vec2(0, 1);
	static private readonly vec2 right = new vec2(1, 0);

	// Small offsets for collision detection accuracy
	private const float skinWidthOffset = 0.05f;          // Prevents clipping through geometry
	private const float autoSteppingSpeedThreshold = 0.1f; // Min speed to trigger auto-step
	private const float largeEpsilon = 0.001f;            // Tolerance for slope comparisons

	// Fixed timestep for physics simulation (1/playerFps)
	private float playerIFps = 1.0f;

	// Connected gamepad reference (null if no gamepad)
	private InputGamePad gamePad = null;

	// Physics objects for collision detection
	private BodyDummy body = null;           // Kinematic body (no physics forces)
	private ShapeCapsule shape = null;       // Main collision shape (player hitbox)
	private ShapeCylinder interactionShape = null; // Larger shape for pushing objects

	// Collision detection state
	private List<ShapeContact> contacts = new List<ShapeContact>();
	private bool isAvailableSideMove = false;  // Can move in current direction
	private bool isAvailableStair = false;     // Stair detected ahead
	private bool hasBottomContacts = false;    // Touching something below
	private bool isHeavyContacts = false;      // Too many contacts (skip iterations)

	// Camera rotation state
	private float cameraVerticalAngle = 0.0f;   // Pitch (look up/down)
	private float cameraHorizontalAngle = 0.0f; // Yaw (look left/right)
	private vec3 cameraCrouchOffset = vec3.ZERO; // Camera height offset when crouching

	// Input direction this frame
	private vec2 horizontalMoveDirection = vec2.ZERO; // WASD input normalized
	private float verticalMoveDirection = 0.0f;       // Jump impulse

	// Speed limit when airborne (preserves momentum from ground)
	private float maxAirSpeed = 0.0f;

	// Auto-stepping state
	private bool usedAutoStepping = false;  // True if step-up occurred this frame
	private Scalar lastStepHeight = 0.0f;   // Height of last detected step

	// Pre-calculated cosines for angle comparisons (faster than computing each frame)
	private float cosGroundAngle = 0.5f;         // cos(maxGroundAngle)
	private float cosCeilingAngle = 0.5f;        // cos(maxCeilingAngle)
	private float cosStairAngle = 0.5f;          // cos(maxStairAngle)
	private float cosStairsDetectionAngle = 0.5f; // cos(stairsDetectionAngle)
	private float cosWallStopSlidingAngle = 0.96f; // cos(wallStopSlidingAngle)

	// Cached transform for physics calculations (avoids modifying node directly)
	private Mat4 worldTransform = Mat4.IDENTITY;

	// Reusable intersection result object
	WorldIntersectionNormal normalIntersection = new WorldIntersectionNormal();

	// ═══════════════════════════════════════════════════════════════════════════════
	// CROUCH STATE MACHINE - Handles smooth crouch transitions
	// ═══════════════════════════════════════════════════════════════════════════════

	public enum CrouchPhase
	{
		STAND,     // Full height, not crouching
		MOVE_DOWN, // Transitioning to crouch (shrinking capsule)
		CROUCH,    // Fully crouched
		MOVE_UP    // Transitioning to stand (growing capsule)
	}

	// State for smooth crouch interpolation
	private struct CrouchState
	{
		public CrouchPhase phase;       // Current crouch phase
		public float currentTime;       // Time remaining in transition
		public Scalar currentHeight;    // Current capsule total height
		public Scalar startHeight;      // Height at transition start
		public Scalar endHeight;        // Target height for transition
	}

	private CrouchState crouchState;

	// Saved mouse mode for cleanup on shutdown
	private Input.MOUSE_HANDLE mouse_handle;

	#region Debug

#if DEBUG
	private float maxAppliedHorizontalSpeed = 0.0f;
	private float maxAppliedVerticalSpeed = 20.0f;
	private int maxPassContacts = 200;
	private float maxFlagValue = 1.05f;

	private float[] speedsBuffer = new float[10];
	private Vec3 lastPlayerPosition = Vec3.ZERO;

	private bool autoSteppingApplied = false;
#endif

	#endregion Debug

	// ═══════════════════════════════════════════════════════════════════════════════
	// LIFECYCLE METHODS - Component initialization and frame updates
	// ═══════════════════════════════════════════════════════════════════════════════

	// Initializes physics body, capsule shape, camera, and gamepad input.
	// Sets up all pre-calculated values and validates editor parameters.
	private void Init()
	{
		// Lock mouse cursor for FPS controls
		mouse_handle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;

		// Verify node type - must be an Object for physics body attachment
		Object obj = node as Object;
		if (!obj)
		{
			Log.Error("FirstPersonController: can't cast node to Object\n");
			return;
		}

		// Fix player transformation
		// Player can only have vertical position, Y axis is used for forward direction
		Vec3 axisY = new Vec3(obj.WorldTransform.AxisY);
		axisY.z = 0;
		axisY = (axisY.Length2 > MathLib.EPSILON ? axisY.Normalized : Vec3.FORWARD);
		obj.WorldTransform = MathLib.SetTo(node.WorldPosition, node.WorldPosition + axisY, vec3.UP, MathLib.AXIS.Y);

		// Set dummy body
		if (useObjectBody)
		{
			body = obj.Body as BodyDummy;
			if (!body)
				Log.Warning("FirstPersonController: object doesn't contain BodyDummy, it was created automatically\n");
		}

		if (!body)
		{
			body = new BodyDummy(obj);
			body.Transform = obj.WorldTransform;
		}

		// Set capsule shape
		if (useObjectBody)
		{
			if (body.NumShapes > 0)
			{
				for (int i = 0; i < body.NumShapes; i++)
					if (!shape)
						shape = body.GetShape(i) as ShapeCapsule;

				if (!shape)
					Log.Warning("FirstPersonController: body doesn't contain ShapeCapsule, it was created automatically\n");
			}
			else
				Log.Warning("FirstPersonController: body doesn't contain shapes, it was created automatically\n");
		}

		if (!shape)
		{
			shape = new ShapeCapsule(body, capsuleRadius, capsuleHeight);
			body.SetShapeTransform(body.NumShapes - 1, MathLib.Translate(vec3.UP * (capsuleRadius + 0.5f * capsuleHeight)));

			shape.PhysicsIntersectionMask = physicsIntersectionMask;
			shape.CollisionMask = collisionMask;
			shape.ExclusionMask = exclusionMask;
		}

		capsuleHeight = shape.Height;
		crouchHeight = MathLib.Clamp(crouchHeight, 2.0f * shape.Radius, shape.Height + 2.0f * shape.Radius);
		crouchState.currentHeight = shape.Height + 2.0f * shape.Radius;

		// Set camera
		if (cameraMode == CameraMode.USE_EXTERNAL && !camera)
			Log.Warning("FirstPersonController: camera is null, it was created automatically\n");

		if (!camera || cameraMode == CameraMode.CREATE_AUTOMATICALLY)
		{
			camera = new PlayerDummy();
			camera.Parent = obj;
			camera.Fov = fov;
			camera.ZNear = nearClipping;
			camera.ZFar = farClipping;
			camera.WorldPosition = obj.WorldTransform * new Vec3(cameraPositionOffset);
			camera.SetWorldDirection(new vec3(axisY), vec3.UP);
			camera.MainPlayer = true;
		}

		if (camera && cameraMode != CameraMode.NONE)
		{
			cameraVerticalAngle = MathLib.Angle(vec3.DOWN, camera.GetWorldDirection());
			cameraVerticalAngle = MathLib.Clamp(cameraVerticalAngle, minVerticalAngle + 90.0f, maxVerticalAngle + 90.0f);

			cameraHorizontalAngle = node.GetWorldRotation().GetAngle(vec3.UP);
			cameraPositionOffset = new vec3(node.IWorldTransform * camera.WorldPosition);

			vec3 cameraDirection = vec3.FORWARD * MathLib.RotateZ(-cameraHorizontalAngle);
			cameraDirection = cameraDirection * MathLib.Rotate(MathLib.Cross(cameraDirection, vec3.UP), 90.0f - cameraVerticalAngle);
			cameraDirection.Normalize();
			camera.SetWorldDirection(cameraDirection, vec3.UP);
		}

		// Create cylinder shape for interacting with objects
		if (useObjectsInteraction)
		{
			interactionShape = new ShapeCylinder();
			interactionShape.Enabled = false;
		}

		// Set auxiliary parameters
		playerIFps = 1.0f / playerFps;
		crouchTransitionTime = MathLib.Max(crouchTransitionTime, MathLib.EPSILON);

		cosGroundAngle = MathLib.Cos(maxGroundAngle * MathLib.DEG2RAD);
		cosCeilingAngle = MathLib.Cos(maxCeilingAngle * MathLib.DEG2RAD);
		cosStairAngle = MathLib.Cos(maxStairAngle * MathLib.DEG2RAD);
		cosStairsDetectionAngle = MathLib.Cos(stairsDetectionAngle * MathLib.DEG2RAD + MathLib.PI05);
		cosWallStopSlidingAngle = MathLib.Cos(wallStopSlidingAngle * MathLib.DEG2RAD);

#if DEBUG
		maxPassContacts = contactsBufferSize;
#endif

		worldTransform = obj.WorldTransform;

		for (int i = 0; i < Input.NumGamePads; i++)
		{
			Unigine.InputGamePad game_pad = Input.GetGamePad(i);
			if (game_pad.IsAvailable)
			{
				gamePad = game_pad;
				break;
			}
		}

		IsInitialized = true;
	}

	// Restores mouse handle mode on shutdown.
	private void Shutdown()
	{
		Input.MouseHandle = mouse_handle;
	}

	// Main update loop - processes input, physics, collisions each frame.
	// Uses adaptive timestep to maintain consistent physics at variable framerates.
	private void Update()
	{
		if (!IsInitialized)
			return;

		worldTransform = node.WorldTransform;

		// Scale frame time by physics scale for slow-motion support
		float ifps = Game.IFps * Physics.Scale;

		// Step 1: Read input and determine desired movement direction
		UpdateMoveDirections(ifps);

		// Step 2: Raycast to check if movement is possible (slope/stair detection)
		CheckMoveAndStair();

		// Block movement if facing an impassable slope while grounded
		if (!isAvailableSideMove)
		{
			if (hasBottomContacts)
			{
				HorizontalVelocity = Vec3.ZERO;
				horizontalMoveDirection = vec2.ZERO;
			}
		}

		// Step 3: Physics simulation with fixed timestep subdivision
		// Multiple iterations per frame when FPS < playerFps for stability
		float updateTime = ifps;
		hasBottomContacts = false;
		while (updateTime > 0.0f)
		{
			float adaptiveTimeStep = MathLib.Min(updateTime, playerIFps);
			updateTime -= adaptiveTimeStep;

			// Apply acceleration, gravity, damping
			UpdateVelocity(adaptiveTimeStep, adaptiveTimeStep / ifps);
			// Move player by velocity
			worldTransform = MathLib.Translate((HorizontalVelocity + Vec3.UP * VerticalVelocity) * adaptiveTimeStep) * worldTransform;
			// Resolve collisions and detect ground/ceiling
			UpdateCollisions(adaptiveTimeStep);
		}

		// Step 4: Handle crouch state transitions
		UpdateCrouch(ifps);

		// Step 5: Apply final transform to node and physics body
		node.WorldTransform = worldTransform;
		body.Transform = node.WorldTransform;
		if (IsCrouch)
			shape.Transform = worldTransform * MathLib.Translate(Vec3.UP * (shape.Radius + shape.Height * 0.5f));

		// Step 6: Update camera position and rotation
		UpdateCamera();
	}

	// Physics callback - applies impulses to rigid bodies the player contacts.
	// Uses a cylinder shape slightly larger than capsule for better detection.
	private void UpdatePhysics()
	{
		if (!IsInitialized)
			return;

		if (useObjectsInteraction)
		{
			// Enable interaction cylinder temporarily for collision detection
			// Cylinder is slightly larger than capsule to catch edge contacts
			interactionShape.Enabled = true;
			interactionShape.Radius = shape.Radius + checkMoveRayOffset + skinWidthOffset;
			interactionShape.Height = shape.Height + 2.0f * (shape.Radius + checkMoveRayOffset - skinWidthOffset);
			interactionShape.Transform = MathLib.Translate(worldTransform.Translate + Vec3.UP * (0.5f * interactionShape.Height + skinWidthOffset));

			// Get all contacts with physics objects
			interactionShape.GetCollision(contacts);
			int contactsCount = MathLib.Min(contactsBufferSize, contacts.Count);

			// Calculate impulse magnitude based on player speed
			Scalar speed = HorizontalVelocity.Length + MathLib.Abs(VerticalVelocity);
			speed = MathLib.Max(speed, 1.0f);

			// Apply push impulse to each rigid body we're touching
			for (int i = 0; i < contactsCount; i++)
			{
				ShapeContact c = contacts[i];
				if (c.Object && c.Object.BodyRigid)
				{
					// Wake up frozen bodies so they react
					c.Object.BodyRigid.Frozen = false;

					// Push object away from player based on contact normal
					c.Object.BodyRigid.AddWorldImpulse(c.Point, -c.Normal * c.Object.BodyRigid.Mass * (float)speed * impulseMultiplier * 0.5f);
				}
			}

			interactionShape.Enabled = false;
		}
	}

	// ═══════════════════════════════════════════════════════════════════════════════
	// MOVEMENT METHODS - Input processing and velocity calculations
	// ═══════════════════════════════════════════════════════════════════════════════

	// Reads keyboard/gamepad input and sets movement direction vectors.
	// Output: horizontalMoveDirection (normalized WASD), verticalMoveDirection (jump impulse)
	private void UpdateMoveDirections(float ifps)
	{
		// Clear previous frame's input
		horizontalMoveDirection = vec2.ZERO;
		verticalMoveDirection = 0.0f;

		// Don't process input if mouse isn't captured (e.g., menu open)
		if (!Input.MouseGrab)
			return;

		// Accumulate keyboard WASD input
		if (Input.IsKeyPressed(forwardKey))
			horizontalMoveDirection += forward;

		if (Input.IsKeyPressed(backwardKey))
			horizontalMoveDirection -= forward;

		if (Input.IsKeyPressed(rightKey))
			horizontalMoveDirection += right;

		if (Input.IsKeyPressed(leftKey))
			horizontalMoveDirection -= right;

		// Normalize to prevent faster diagonal movement
		if (horizontalMoveDirection.Length2 > 0)
			horizontalMoveDirection.Normalize();

		// Override with gamepad stick if it has stronger input
		if (gamePad != null)
		{
			vec2 moveValue = (moveStick == GamepadStickSide.LEFT ? gamePad.AxesLeft : gamePad.AxesRight);
			if (moveValue.Length > sticksDeadZone && moveValue.Length2 > horizontalMoveDirection.Length2)
				horizontalMoveDirection = moveValue;
		}

		// Jump only when grounded and jump key just pressed
		if (useJump && IsGround && (Input.IsKeyDown(jumpKey) || gamePad && gamePad.IsButtonDown(jumpButton)))
			verticalMoveDirection = (IsCrouch ? crouchJumpPower : jumpPower) / ifps;
	}

	// Raycasts ahead to check if movement is possible and detect stairs.
	// Sets isAvailableSideMove (can walk forward) and isAvailableStair (step detected).
	private void CheckMoveAndStair()
	{
		isAvailableSideMove = false;
		isAvailableStair = false;

		// Convert local input direction to world space
		vec3 horizontalDirection = worldTransform.GetRotate() * new vec3(horizontalMoveDirection);

		if (horizontalMoveDirection.Length2 > 0)
			horizontalMoveDirection.Normalize();

		// === Check if forward movement is possible ===
		// Cast ray downward from slightly ahead of player to detect ground slope
		if (horizontalMoveDirection.Length2 > 0.0f)
		{
			// Ray starts above and casts down to detect floor ahead
			Vec3 p2 = worldTransform.Translate + horizontalDirection * (shape.Radius + checkMoveRayOffset) + Vec3.DOWN * checkMoveRayOffset;
			Vec3 p1 = p2 + Vec3.UP * (MathLib.Max(shape.Radius, maxStepHeight) + checkMoveRayOffset);

			var hitObj = World.GetIntersection(p1, p2, checkMoveMask, normalIntersection);
			if (hitObj)
			{
				// Allow movement if surface is within walkable angle
				if (MathLib.Dot(vec3.UP, normalIntersection.Normal) > cosGroundAngle)
					isAvailableSideMove = true;

				// Also allow if transitioning between different slopes
				Scalar cos = MathLib.Dot(SlopeNormal, new Vec3(normalIntersection.Normal));
				if (cos < largeEpsilon)
					isAvailableSideMove = true;
			}
			else
			{
				// No surface = moving into air (jumping off edge)
				isAvailableSideMove = true;
			}

#if DEBUG
			if (debugVisualizer.enabled && debugVisualizer.checkMoveRay)
			{
				if (isAvailableSideMove)
					Visualizer.RenderVector(p1, p2, vec4.GREEN);
				else
					Visualizer.RenderVector(p1, p2, vec4.RED);
			}
#endif
		}

		// === Check for stairs ahead ===
		// Ray detects horizontal surfaces within step height range
		if (useAutoStepping && horizontalMoveDirection.Length2 > 0.0f)
		{
			// Ray from minStepHeight to maxStepHeight ahead of player
			Vec3 p2 = worldTransform.Translate + horizontalDirection * (shape.Radius + checkStairRayOffset) + Vec3.UP * minStepHeight;
			Vec3 p1 = p2 + vec3.UP * (maxStepHeight - minStepHeight + checkStairRayOffset);

			var hitObj = World.GetIntersection(p1, p2, stairDetectionMask, normalIntersection);
			if (hitObj)
			{
				// Surface is flat enough to be a stair
				if (MathLib.Dot(vec3.UP, normalIntersection.Normal) > cosStairAngle)
					isAvailableStair = true;
			}

#if DEBUG
			if (debugVisualizer.enabled && debugVisualizer.stairDetectionRay)
			{
				if (isAvailableStair)
					Visualizer.RenderVector(p1, p2, vec4.GREEN);
				else
					Visualizer.RenderVector(p1, p2, vec4.RED);
			}
#endif
		}
	}

	// Calculates velocity based on input, applies acceleration, gravity, damping.
	// Handles slope-relative movement so player moves along surface, not through it.
	// ifps = timestep, updatePart = fraction of total frame time (for mouse smoothing)
	private void UpdateVelocity(float ifps, float updatePart)
	{
		// === Build slope-local coordinate system ===
		// Movement on slopes follows the surface rather than world axes
		Scalar cosAngle = MathLib.Dot(new Vec3(worldTransform.AxisY), SlopeNormal);
		if (MathLib.Equals(MathLib.Abs(cosAngle), 1.0f))
		{
			// Player facing directly up/down slope - use X axis as reference
			SlopeAxisY = MathLib.Cross(new Vec3(worldTransform.AxisX) * MathLib.Sign(cosAngle), SlopeNormal).Normalized;
			SlopeAxisX = MathLib.Cross(SlopeAxisY, SlopeNormal).Normalized;
		}
		else
		{
			// Normal case - derive from player's forward direction
			SlopeAxisX = MathLib.Cross(new Vec3(worldTransform.AxisY), SlopeNormal).Normalized;
			SlopeAxisY = MathLib.Cross(SlopeNormal, SlopeAxisX).Normalized;
		}

		// Get decomposition of velocity for instant change on ground
		Vec3 horizontalVelocityDecomposition = Vec3.ZERO;
		if (IsGround)
		{
			horizontalVelocityDecomposition.x = MathLib.Dot(SlopeAxisX, HorizontalVelocity);
			horizontalVelocityDecomposition.y = MathLib.Dot(SlopeAxisY, HorizontalVelocity);
			horizontalVelocityDecomposition.z = MathLib.Dot(SlopeNormal, HorizontalVelocity);
		}

		// Player rotation
		if (Input.MouseGrab)
		{
			worldTransform *= new Mat4(MathLib.Rotate(new quat(vec3.UP, -Input.MouseDeltaPosition.x * mouseSensitivity * 0.1f * updatePart)));

			float delta = -Input.MouseDeltaPosition.x * mouseSensitivity * 0.1f;
			if (gamePad)
			{
				vec2 rotateValue = (cameraStick == GamepadStickSide.LEFT ? gamePad.AxesLeft : gamePad.AxesRight);
				if (rotateValue.Length > sticksDeadZone && MathLib.Abs(rotateValue.x * cameraStickSensitivity) > MathLib.Abs(delta))
					delta = -rotateValue.x * cameraStickSensitivity;
			}

			cameraHorizontalAngle += delta * updatePart;
			if (cameraHorizontalAngle < -180.0f || 180.0f < cameraHorizontalAngle)
				cameraHorizontalAngle -= MathLib.Sign(cameraHorizontalAngle) * 360.0f;

			Vec3 position = worldTransform.Translate;
			worldTransform.SetRotate(Vec3.UP, cameraHorizontalAngle);
			worldTransform.SetColumn3(3, position);
		}

		// On the ground change velocity without inertia
		if (IsGround)
		{
			// Again check vectors for collinearity and, depending on this, update the slope basis
			cosAngle = MathLib.Dot(new Vec3(worldTransform.AxisY), SlopeNormal);
			if (MathLib.Equals(MathLib.Abs(cosAngle), 1.0f))
			{
				SlopeAxisY = MathLib.Cross(new Vec3(worldTransform.AxisX) * MathLib.Sign(cosAngle), SlopeNormal).Normalized;
				SlopeAxisX = MathLib.Cross(SlopeAxisY, SlopeNormal).Normalized;
			}
			else
			{
				SlopeAxisX = MathLib.Cross(new Vec3(worldTransform.AxisY), SlopeNormal).Normalized;
				SlopeAxisY = MathLib.Cross(SlopeNormal, SlopeAxisX).Normalized;
			}

			// Restore velocity in new basis
			HorizontalVelocity = SlopeAxisX * horizontalVelocityDecomposition.x +
								 SlopeAxisY * horizontalVelocityDecomposition.y +
								 SlopeNormal * horizontalVelocityDecomposition.z;
		}

		// Add horizontal velocity in slope basis
		float acceleration = (IsGround ? groundAcceleration : airAcceleration);
		HorizontalVelocity += SlopeAxisX * horizontalMoveDirection.x * acceleration * ifps;
		HorizontalVelocity += SlopeAxisY * horizontalMoveDirection.y * acceleration * ifps;

		// Update vertical velocity
		VerticalVelocity += verticalMoveDirection * ifps;
		if (!IsGround)
			VerticalVelocity += Physics.Gravity.z * ifps;

		// Get current max speed
		float maxSpeed = maxAirSpeed;
		if (IsGround)
		{
			maxSpeed = (useRun && useRunDefault) ? runSpeed : walkSpeed;

			if (useRun && (Input.IsKeyPressed(runKey) || gamePad && gamePad.IsButtonPressed(runButton)))
				maxSpeed = useRunDefault ? walkSpeed : runSpeed;

			if (IsGround && IsCrouch)
				maxSpeed = crouchSpeed;

			maxAirSpeed = maxSpeed;
		}

		// Apply damping to horizontal velocity when it exceeds target speed
		// or target speed too low (not pressed horizontal movement keys)
		vec2 targetSpeed = horizontalMoveDirection * maxSpeed;
		if (targetSpeed.Length < MathLib.EPSILON || targetSpeed.Length < HorizontalVelocity.Length)
			HorizontalVelocity *= MathLib.Exp((IsGround ? -groundDamping : -airDamping) * ifps);

		// Clamp horizontal velocity if it greater than current max speed
		if (HorizontalVelocity.Length > maxSpeed)
			HorizontalVelocity = HorizontalVelocity.Normalized * maxSpeed;

		// Check frozen state for horizontal velocity
		// IsGround needed in case of slipping from the edges
		// Contacts will be pushed player in all directions, and not just up
		IsHorizontalFrozen = IsGround && (HorizontalVelocity.Length < Physics.FrozenLinearVelocity);
	}

	// ═══════════════════════════════════════════════════════════════════════════════
	// COLLISION METHODS - Capsule collision detection and resolution
	// ═══════════════════════════════════════════════════════════════════════════════

	// Main collision resolution loop. Iterates multiple times to resolve penetrations.
	// Implements auto-stepping: TryMoveUp -> MoveSide -> TryMoveDown
	private void UpdateCollisions(float ifps)
	{
		// Reset ground/ceiling state each frame
		IsGround = false;
		IsCeiling = false;
		SlopeNormal = Vec3.UP;

		isHeavyContacts = false;

		// Multiple iterations handle complex geometry (corners, wedges)
		for (int j = 0; j < collisionIterations; j++)
		{
			// === Auto-stepping Phase 1: Try to move up ===
			if (useAutoStepping)
			{
#if DEBUG
				autoSteppingApplied = false;
#endif
				if (isAvailableStair)
					TryMoveUp(ifps);
			}

			// === Main collision: horizontal and vertical resolution ===
			MoveSide(ifps);

			// === Auto-stepping Phase 2: Settle down onto stair ===
			if (useAutoStepping && usedAutoStepping && isAvailableStair)
			{
#if DEBUG
				autoSteppingApplied = true;
#endif
				TryMoveDown(ifps);
			}

			// Skip remaining iterations if too many contacts (performance)
			if (isHeavyContacts)
				break;
		}
	}

	// ═══════════════════════════════════════════════════════════════════════════════
	// AUTO-STEPPING METHODS - Smooth stair/ledge climbing
	// ═══════════════════════════════════════════════════════════════════════════════

	// Phase 1 of auto-stepping: Elevate player to clear the obstacle.
	// Finds highest contact point in movement direction and tries moving up.
	// If new position is collision-free, marks usedAutoStepping = true.
	private void TryMoveUp(float ifps)
	{
		usedAutoStepping = false;
		lastStepHeight = 0.0f;

		// Only step when moving forward, not frozen, and falling (not jumping)
		if (horizontalMoveDirection.Length2 > 0.0f && !IsHorizontalFrozen && VerticalVelocity < 0.0f)
		{
			// Sync physics shape with current position
			body.Transform = worldTransform;
			if (IsCrouch)
				shape.Transform = worldTransform * MathLib.Translate(Vec3.UP * (shape.Radius + shape.Height * 0.5f));

			// Get all contacts at current position
			shape.GetCollision(contacts);
			if (contacts.Count == 0)
				return;

			if (contacts.Count > heavyContactsCount)
				isHeavyContacts = true;

			int contactsCount = MathLib.Min(contactsBufferSize, contacts.Count);

			// Ensure minimum velocity for step detection
			Vec2 velocityXY = new Vec2(HorizontalVelocity);
			if (velocityXY.Length2 < autoSteppingSpeedThreshold)
			{
				velocityXY = new Vec2(worldTransform.GetRotate() * new Vec3(horizontalMoveDirection));
				velocityXY.Normalize();
				HorizontalVelocity = new Vec3(velocityXY * walkSpeed);
			}

			// Find highest contact point in movement direction
			for (int i = 0; i < contactsCount; i++)
			{
				ShapeContact c = contacts[i];

				Vec2 normalXY = new Vec2(c.Normal);

				// Skip contacts behind us (walls beside player)
				if (MathLib.Dot(normalXY, velocityXY) > cosStairsDetectionAngle)
					continue;

				// Track height of obstacle relative to player origin
				Scalar step = MathLib.Dot(c.Point - worldTransform.Translate, Vec3.UP);
				if (lastStepHeight < step)
					lastStepHeight = step;
			}

			// Try stepping up if height is within valid range
			if (minStepHeight < lastStepHeight && lastStepHeight < maxStepHeight)
			{
				// Move up by step height
				worldTransform.SetColumn3(3, worldTransform.Translate + vec3.UP * lastStepHeight);

				// Verify new position is clear
				body.Transform = worldTransform;
				if (IsCrouch)
					shape.Transform = worldTransform * MathLib.Translate(Vec3.UP * (shape.Radius + shape.Height * 0.5f));

				shape.GetCollision(contacts);
				if (contacts.Count == 0)
					usedAutoStepping = true;
				else
					// Revert if elevated position has collisions
					worldTransform.SetColumn3(3, worldTransform.Translate + vec3.DOWN * lastStepHeight);
			}

#if DEBUG
			if (debugVisualizer.enabled && debugVisualizer.upPassContacts)
			{
				foreach (var c in contacts)
					Visualizer.RenderVector(c.Point, c.Point + c.Normal, debugColors.upPassContacts);
			}

			if (debugProfiler.enabled && debugProfiler.upPassContact)
				Profiler.SetValue("Up Pass Contacts ", "", contacts.Count, maxPassContacts, debugColors.arrayUpPassContacts);
#endif
		}
	}

	// Main collision resolution: pushes player out of geometry, detects ground/ceiling.
	// Processes all contacts and accumulates position offset to resolve penetrations.
	private void MoveSide(float ifps)
	{
		// Sync physics shape with current position
		body.Transform = worldTransform;
		if (IsCrouch)
			shape.Transform = worldTransform * MathLib.Translate(Vec3.UP * (shape.Radius + shape.Height * 0.5f));

		// Get all contacts at current position
		shape.GetCollision(contacts);
		if (contacts.Count == 0)
			return;

		if (contacts.Count > heavyContactsCount)
			isHeavyContacts = true;

		int contactsCount = MathLib.Min(contactsBufferSize, contacts.Count);

		// Accumulated push-out offset (averaged over all contacts)
		var positionOffset = vec3.ZERO;

		// Track steepest slope under player for movement direction
		float maxCosAngle = 1.0f;

		float inum = 1.0f / contactsCount;
		for (int i = 0; i < contactsCount; i++)
		{
			var c = contacts[i];

			// When horizontal velocity is frozen, we can move player only in vertical direction
			// This helps to avoid sliding on slopes
			// In other cases, move player in all directions
			// And use epsilon offset with depth for accuracy ground detection
			if (IsHorizontalFrozen)
			{
				// When frozen, only push vertically (prevents slope sliding)
				float depth = MathLib.Dot(vec3.UP, c.Normal) * (c.Depth - MathLib.EPSILON);
				positionOffset += vec3.UP * depth * inum;
			}
			else
			{
				// Push along contact normal
				positionOffset += c.Normal * (c.Depth - MathLib.EPSILON) * inum;

				// Cancel velocity component pushing into surface
				Scalar normalSpeed = MathLib.Dot(new Vec3(c.Normal), HorizontalVelocity);
				HorizontalVelocity -= c.Normal * normalSpeed;
			}

			// === Wall stop: prevent sliding when facing wall directly ===
			if ((c.Object && c.Object.BodyRigid == null) && shape.BottomCap.z < c.Point.z && c.Point.z < shape.TopCap.z)
			{
				float cos = MathLib.Dot(worldTransform.GetRotate() * new vec3(horizontalMoveDirection), -c.Normal);
				if (cos > cosWallStopSlidingAngle)
					HorizontalVelocity = Vec3.ZERO;
			}

			// === Ground detection: contact on bottom hemisphere within angle limit ===
			if (MathLib.Dot(c.Point - shape.BottomCap, Vec3.UP) < 0.0f)
			{
				hasBottomContacts = true;
				if (MathLib.Dot(c.Normal, vec3.UP) > cosGroundAngle)
				{
					// Apply small downward velocity to keep grounded
					VerticalVelocity = Physics.Gravity.z * ifps;
					IsGround = true;
				}

				// Track steepest slope for movement direction calculation
				float cosAngle = MathLib.Dot(vec3.UP, c.Normal);
				if (MathLib.Equals(cosAngle, 0.0f, 0.01f) == false && cosAngle < maxCosAngle)
				{
					SlopeNormal = new Vec3(contacts[i].Normal);
					maxCosAngle = cosAngle;
				}
			}

			// === Ceiling detection: contact on top hemisphere within angle limit ===
			if (MathLib.Dot(contacts[i].Normal, vec3.DOWN) > cosCeilingAngle && MathLib.Dot(contacts[i].Point - shape.TopCap, Vec3.DOWN) < 0.0f)
			{
				IsCeiling = true;
				// Stop upward movement when hitting ceiling
				VerticalVelocity = 0.0f;
			}
		}

		// Apply accumulated push-out
		worldTransform.SetColumn3(3, worldTransform.Translate + positionOffset);

#if DEBUG
		if (debugVisualizer.enabled && debugVisualizer.sidePassContacts)
		{
			foreach (var c in contacts)
				Visualizer.RenderVector(c.Point, c.Point + c.Normal, debugColors.sidePassContacts);
		}

		if (debugProfiler.enabled && debugProfiler.sidePassContact)
			Profiler.SetValue("Side Pass Contacts ", "", contacts.Count, maxPassContacts, debugColors.arraySidePassContacts);
#endif
	}

	// Phase 2 of auto-stepping: Lower player onto the stepped surface.
	// After TryMoveUp elevated player, this settles them down onto the actual stair.
	// Prevents floating above stairs by finding ground contact below.
	private void TryMoveDown(float ifps)
	{
		// Clamp step height to avoid jitter on large stairs
		// When step > radius, capsule bottom may clip through geometry
		if (lastStepHeight > shape.Radius)
			lastStepHeight = shape.Radius - Physics.PenetrationTolerance;

		// Drop player down by the step height we rose in TryMoveUp
		worldTransform.SetColumn3(3, worldTransform.Translate - vec3.UP * lastStepHeight);

		// Sync physics shape with lowered position
		body.Transform = worldTransform;
		if (IsCrouch)
			shape.Transform = worldTransform * MathLib.Translate(Vec3.UP * (shape.Radius + shape.Height * 0.5f));

		// Get all contacts at lowered position
		shape.GetCollision(contacts);
		if (contacts.Count == 0)
			return;

		if (contacts.Count > heavyContactsCount)
			isHeavyContacts = true;

		int contactsCount = MathLib.Min(contactsBufferSize, contacts.Count);

		// Push player up to resolve any penetration (settle onto stair surface)
		float inumContacts = 1.0f / MathLib.ToFloat(contactsCount);
		for (int i = 0; i < contactsCount; i++)
		{
			ShapeContact c = contacts[i];

			// Calculate vertical push-out from contact
			float depth = MathLib.Dot(vec3.UP, c.Normal) * c.Depth;
			worldTransform.SetColumn3(3, worldTransform.Translate + vec3.UP * depth * inumContacts);

			// Detect ground contact (bottom hemisphere, walkable angle)
			if (MathLib.Dot(c.Normal, vec3.UP) > cosGroundAngle && MathLib.Dot(c.Point - shape.BottomCap, Vec3.UP) < 0.0f)
			{
				IsGround = true;
				// Small downward velocity keeps player grounded next frame
				VerticalVelocity = Physics.Gravity.z * ifps;
			}
		}

#if DEBUG
		if (debugVisualizer.enabled && debugVisualizer.downPassContacts)
		{
			foreach (var c in contacts)
				Visualizer.RenderVector(c.Point, c.Point + c.Normal, debugColors.downPassContacts);
		}

		if (debugProfiler.enabled && debugProfiler.downPassContact)
			Profiler.SetValue("Down Pass Contacts ", "", contacts.Count, maxPassContacts, debugColors.arrayDownPassContacts);
#endif
	}

	// ═══════════════════════════════════════════════════════════════════════════════
	// CROUCH SYSTEM - Smooth crouch/stand transitions with collision checking
	// ═══════════════════════════════════════════════════════════════════════════════

	// Handles crouch state machine: STAND <-> MOVE_DOWN <-> CROUCH <-> MOVE_UP.
	// Smoothly interpolates capsule height over crouchTransitionTime seconds.
	// Checks for overhead obstacles before allowing stand-up.
	private void UpdateCrouch(float ifps)
	{
		if (!useCrouch)
			return;

		// Check if crouch key/button is currently held
		bool isKey = (Input.IsKeyPressed(crouchKey) || gamePad && gamePad.IsButtonPressed(crouchButton));

		// === State machine: determine next transition based on current phase ===
		switch (crouchState.phase)
		{
			case CrouchPhase.STAND:
				if (isKey)
				{
					// Begin crouch: transition from full height to crouch height
					crouchState.phase = CrouchPhase.MOVE_DOWN;
					SwapInterpolationDirection(capsuleHeight + 2.0f * shape.Radius, crouchHeight);
					IsCrouch = true;
				}
				break;

			case CrouchPhase.MOVE_DOWN:
			case CrouchPhase.CROUCH:
				// Key released while crouched - try to stand up
				if (!isKey)
				{
					bool canStandUp = true;

					// === Overhead clearance check ===
					// Temporarily expand capsule to standing height and check for collisions
					// Use slightly thinner radius to avoid false positives from walls
					float radius = shape.Radius;
					shape.Radius = radius - skinWidthOffset;
					UpdatePlayerHeight(capsuleHeight + 2.0f * skinWidthOffset);

					shape.GetCollision(contacts);

					// Any contact above crouch height blocks standing up
					Scalar topPoint = worldTransform.Translate.z + crouchHeight;
					for (int i = 0; i < contacts.Count; i++)
						if (contacts[i].Point.z > topPoint)
						{
							canStandUp = false;
							break;
						}

					// Restore current capsule dimensions
					shape.Radius = radius;
					UpdatePlayerHeight(crouchState.currentHeight - 2.0f * shape.Radius);

					if (canStandUp)
					{
						// Begin stand-up: transition from crouch height to full height
						crouchState.phase = CrouchPhase.MOVE_UP;
						SwapInterpolationDirection(crouchHeight, capsuleHeight + 2.0f * shape.Radius);
					}
				}
				break;

			case CrouchPhase.MOVE_UP:
				// Hit ceiling or key pressed again - abort stand-up, go back to crouch
				if (IsCeiling || isKey)
				{
					crouchState.phase = CrouchPhase.MOVE_DOWN;
					SwapInterpolationDirection(capsuleHeight + 2.0f * shape.Radius, crouchHeight);
				}
				break;

			default: break;
		}

		// === Smooth height interpolation ===
		// Gradually change capsule height over crouchTransitionTime seconds
		if (crouchState.currentTime > 0.0f)
		{
			// Calculate interpolation progress (0 = start, 1 = end)
			float t = 1.0f;
			if (MathLib.Equals(crouchTransitionTime, MathLib.EPSILON) == false)
				t = MathLib.Saturate(1.0f - crouchState.currentTime / crouchTransitionTime);

			// Interpolate height and update capsule shape
			crouchState.currentHeight = MathLib.Lerp(crouchState.startHeight, crouchState.endHeight, t);
			UpdatePlayerHeight(crouchState.currentHeight - 2.0f * shape.Radius);

			crouchState.currentTime -= ifps;

			// === Transition complete - finalize state ===
			if (crouchState.currentTime <= 0.0f)
			{
				crouchState.currentTime = 0.0f;
				crouchState.currentHeight = crouchState.endHeight;

				switch (crouchState.phase)
				{
					case CrouchPhase.MOVE_DOWN:
						// Finished crouching - enter stable crouch state
						UpdatePlayerHeight(crouchState.currentHeight - 2.0f * shape.Radius);
						crouchState.phase = CrouchPhase.CROUCH;
						break;

					case CrouchPhase.MOVE_UP:
						// Finished standing - enter stable stand state
						UpdatePlayerHeight(crouchState.currentHeight - 2.0f * shape.Radius);
						crouchState.phase = CrouchPhase.STAND;
						IsCrouch = false;
						break;

					default: break;
				}
			}
		}
	}

	// ═══════════════════════════════════════════════════════════════════════════════
	// CAMERA METHODS - First-person camera positioning and rotation
	// ═══════════════════════════════════════════════════════════════════════════════

	// Updates camera vertical angle (pitch) from mouse/gamepad input.
	// Applies position offset, crouch offset, and any additional offsets.
	// Horizontal rotation is handled in UpdateVelocity (rotates whole player).
	private void UpdateCamera()
	{
		if (!camera || cameraMode == CameraMode.NONE)
			return;

		// === Process vertical look input (pitch) ===
		if (Input.MouseGrab)
		{
			// Mouse Y movement controls pitch
			float delta = -Input.MouseDeltaPosition.y * mouseSensitivity * 0.1f;

			// Override with gamepad stick if it has stronger input
			if (gamePad)
			{
				vec2 rotateValue = (cameraStick == GamepadStickSide.LEFT ? gamePad.AxesLeft : gamePad.AxesRight);
				if (rotateValue.Length > sticksDeadZone && MathLib.Abs(rotateValue.y * cameraStickSensitivity) > MathLib.Abs(delta))
					delta = rotateValue.y * cameraStickSensitivity;
			}

			// Accumulate and clamp vertical angle within configured limits
			cameraVerticalAngle += delta;
			cameraVerticalAngle = MathLib.Clamp(cameraVerticalAngle, minVerticalAngle + 90.0f, maxVerticalAngle + 90.0f);
		}

		// === Calculate camera world position ===
		// Combines: base offset + crouch offset + any external offsets (e.g., head bob)
		camera.WorldPosition = worldTransform * (new Vec3(cameraPositionOffset) + cameraCrouchOffset + AdditionalCameraOffset);

		// === Calculate camera view direction from angles ===
		// Start with forward, rotate by horizontal angle (yaw), then vertical angle (pitch)
		vec3 cameraDirection = vec3.FORWARD * MathLib.RotateZ(-cameraHorizontalAngle);
		cameraDirection = cameraDirection * MathLib.Rotate(MathLib.Cross(cameraDirection, vec3.UP), 90.0f - cameraVerticalAngle);
		// Apply any external rotation (e.g., recoil, camera shake)
		cameraDirection = AdditionalCameraRotation * cameraDirection;
		cameraDirection.Normalize();
		camera.SetWorldDirection(cameraDirection, vec3.UP);
	}

	// Reverses crouch interpolation direction (e.g., when aborting stand-up).
	// Preserves progress by computing remaining time from current position.
	private void SwapInterpolationDirection(Scalar startHeight, Scalar endHeight)
	{
		// Remaining time = total time minus what was already spent going the other way
		crouchState.currentTime = MathLib.Max(MathLib.EPSILON, crouchTransitionTime - crouchState.currentTime);
		crouchState.startHeight = startHeight;
		crouchState.endHeight = endHeight;
	}

	// Updates capsule shape height and adjusts camera offset for crouch.
	// Also repositions shape transform to keep bottom at ground level.
	private void UpdatePlayerHeight(Scalar height)
	{
		shape.Height = (float)height;
		// Offset camera down when crouched (negative offset)
		cameraCrouchOffset = vec3.UP * (float)(height - capsuleHeight);
		// Reposition capsule so bottom hemisphere stays at player origin
		shape.Transform = worldTransform * MathLib.Translate(Vec3.UP * (shape.Radius + height * 0.5f));
	}


	#region Debug

#if DEBUG
	[MethodInit]
	private void InitDebug()
	{
		if (!IsInitialized)
			return;

		// Debug camera
		if (debugCamera.enabled)
		{
			debugCamera.camera = new PlayerDummy(); ;
			debugCamera.angularSpeed = 90.0f;
			debugCamera.zoomSpeed = 3.0f;
			debugCamera.maxDistance = 10.0f;
			debugCamera.horizontalAngle = 0.0f;
			debugCamera.verticalAngle = 0.0f;
			debugCamera.distance = debugCamera.maxDistance * 0.5f;

			Game.Player = debugCamera.camera;

			debugCamera.camera.SetWorldDirection(vec3.FORWARD, vec3.UP);
			debugCamera.camera.WorldPosition = (worldTransform.Translate + vec3.UP * (shape.Radius + shape.Height * 0.5f)) - vec3.FORWARD * debugCamera.distance;
		}

		// Use visualizer
		Visualizer.Enabled = debugVisualizer.enabled;
		if (debugVisualizer.enabled)
		{
			Render.ShowTriangles = (debugVisualizer.triangles ? 1 : 0);

			int showShapes = (debugVisualizer.shapes ? 1 : 0);
			Unigine.Console.Run($"physics_show_shapes {showShapes}");
		}

		// Use profiler
		int showProfiler = (debugProfiler.enabled ? 1 : 0);
		Unigine.Console.Run($"show_profiler {showProfiler}");
		if (debugProfiler.enabled)
		{
			// Applied horizontal speed
			maxAppliedHorizontalSpeed = MathLib.Max(crouchSpeed, walkSpeed);
			maxAppliedHorizontalSpeed = MathLib.Max(maxAppliedHorizontalSpeed, runSpeed);
			maxAppliedHorizontalSpeed *= 1.1f;

			debugColors.arrayAppliedHorizontalVelocity = new float[4]
			{
				debugColors.appliedHorizontalVelocity.x,
				debugColors.appliedHorizontalVelocity.y,
				debugColors.appliedHorizontalVelocity.z,
				1.0f
			};

			// Applied vertical speed
			debugColors.arrayAppliedVerticalVelocity = new float[4]
			{
				debugColors.appliedVerticalVelocity.x,
				debugColors.appliedVerticalVelocity.y,
				debugColors.appliedVerticalVelocity.z,
				1.0f
			};

			// Up pass contacts
			debugColors.arrayUpPassContacts = new float[4]
			{
				debugColors.upPassContacts.x,
				debugColors.upPassContacts.y,
				debugColors.upPassContacts.z,
				1.0f
			};

			// Side pass contacts
			debugColors.arraySidePassContacts = new float[4]
			{
				debugColors.sidePassContacts.x,
				debugColors.sidePassContacts.y,
				debugColors.sidePassContacts.z,
				1.0f
			};

			// Down pass contacts
			debugColors.arrayDownPassContacts = new float[4]
			{
				debugColors.downPassContacts.x,
				debugColors.downPassContacts.y,
				debugColors.downPassContacts.z,
				1.0f
			};

			// Is ground
			debugColors.arrayIsGround = new float[4]
			{
				debugColors.isGround.x,
				debugColors.isGround.y,
				debugColors.isGround.z,
				1.0f
			};

			// Is ceiling
			debugColors.arrayIsCeiling = new float[4]
			{
				debugColors.isCeiling.x,
				debugColors.isCeiling.y,
				debugColors.isCeiling.z,
				1.0f
			};

			// Is crouch
			debugColors.arrayIsCrouch = new float[4]
			{
				debugColors.isCrouch.x,
				debugColors.isCrouch.y,
				debugColors.isCrouch.z,
				1.0f
			};

			// Average speed
			debugColors.arrayAverageSpeed = new float[4]
			{
				debugColors.averageSpeed.x,
				debugColors.averageSpeed.y,
				debugColors.averageSpeed.z,
				1.0f
			};

			// Profile auto stepping
			debugColors.arrayAutoStepping = new float[4]
			{
				debugColors.autoStepping.x,
				debugColors.autoStepping.y,
				debugColors.autoStepping.z,
				1.0f
			};

		}

		lastPlayerPosition = worldTransform.Translate;
	}

	[MethodUpdate]
	private void UpdateDebug()
	{
		if (!IsInitialized)
			return;

		// Debug camera
		if (debugCamera.enabled)
		{
			if (!debugCamera.useFixedAngles)
			{
				if (Input.IsKeyPressed(Input.KEY.UP))
					debugCamera.verticalAngle += debugCamera.angularSpeed * Game.IFps;
				if (Input.IsKeyPressed(Input.KEY.DOWN))
					debugCamera.verticalAngle -= debugCamera.angularSpeed * Game.IFps;

				debugCamera.verticalAngle = MathLib.Clamp(debugCamera.verticalAngle, -89.9f, 89.9f);

				if (Input.IsKeyPressed(Input.KEY.RIGHT))
					debugCamera.horizontalAngle -= debugCamera.angularSpeed * Game.IFps;
				if (Input.IsKeyPressed(Input.KEY.LEFT))
					debugCamera.horizontalAngle += debugCamera.angularSpeed * Game.IFps;

				if (debugCamera.horizontalAngle < -180.0f || 180.0f < debugCamera.horizontalAngle)
					debugCamera.horizontalAngle -= MathLib.Sign(debugCamera.horizontalAngle) * 360.0f;
			}

			if (Input.IsKeyPressed(Input.KEY.EQUALS))
				debugCamera.distance -= debugCamera.zoomSpeed * Game.IFps;
			if (Input.IsKeyPressed(Input.KEY.MINUS))
				debugCamera.distance += debugCamera.zoomSpeed * Game.IFps;

			debugCamera.distance = MathLib.Clamp(debugCamera.distance, 0.0f, debugCamera.maxDistance);

			vec3 cameraDirection = debugCamera.camera.GetDirection();
			if (debugCamera.useFixedAngles && camera)
			{
				if (MathLib.Dot(camera.GetDirection(), vec3.DOWN) < 1.0f)
					cameraDirection = camera.GetWorldDirection();
			}
			else
			{
				cameraDirection = vec3.FORWARD * MathLib.RotateZ(debugCamera.horizontalAngle);
				cameraDirection = cameraDirection * MathLib.Rotate(MathLib.Cross(cameraDirection, vec3.UP), debugCamera.verticalAngle);
			}

			debugCamera.camera.SetWorldDirection(cameraDirection, vec3.UP);
			debugCamera.camera.WorldPosition = (worldTransform.Translate + vec3.UP * (shape.Radius + shape.Height * 0.5f)) - cameraDirection * debugCamera.distance;
		}

		// Use visualizer
		if (debugVisualizer.enabled)
		{
			if (debugVisualizer.playerShape)
				shape.RenderVisualizer(debugColors.playerShape);

			if (debugVisualizer.playerDirection)
			{
				Vec3 p0 = worldTransform.Translate + Vec3.UP * (shape.Radius + shape.Height * 0.5f);
				Vec3 p1 = p0 + new vec3(worldTransform.AxisY);
				Visualizer.RenderVector(p0, p1, debugColors.playerDirection);
			}

			if (debugVisualizer.camera && camera)
			{
				Vec3 p0 = camera.WorldPosition;
				Vec3 p1 = p0 + camera.GetWorldDirection();
				Visualizer.RenderVector(p0, p1, debugColors.cameraColor);

				camera.RenderVisualizer();
			}

			if (debugVisualizer.slopeBasis)
			{
				Vec3 p0 = worldTransform.Translate;
				Visualizer.RenderVector(p0, p0 + SlopeAxisX, vec4.RED);
				Visualizer.RenderVector(p0, p0 + SlopeAxisY, vec4.GREEN);
				Visualizer.RenderVector(p0, p0 + SlopeNormal, vec4.BLUE);
			}

			if (debugVisualizer.appliedHorizontalVelocity)
			{
				Vec3 p0 = worldTransform.Translate + Vec3.UP * (shape.Radius + shape.Height * 0.5f);
				Vec3 p1 = p0 + HorizontalVelocity;
				Visualizer.RenderVector(p0, p1, debugColors.appliedHorizontalVelocity);
			}

			if (debugVisualizer.appliedVerticalVelocity)
			{
				Vec3 p0 = worldTransform.Translate + Vec3.UP * (shape.Radius + shape.Height * 0.5f);
				Vec3 p1 = p0 + vec3.UP * VerticalVelocity;
				Visualizer.RenderVector(p0, p1, debugColors.appliedVerticalVelocity);
			}
		}

		// Use profiler
		if (debugProfiler.enabled)
		{
			if (debugProfiler.appliedHorizontalSpeed)
				Profiler.SetValue("Applied Horizontal Speed", "m/s", (float)HorizontalVelocity.Length, maxAppliedHorizontalSpeed, debugColors.arrayAppliedHorizontalVelocity);

			if (debugProfiler.appliedVerticalSpeed)
				Profiler.SetValue("|Applied Vertical Speed|", "m/s", MathLib.Abs(VerticalVelocity), maxAppliedVerticalSpeed, debugColors.arrayAppliedVerticalVelocity);

			if (debugProfiler.isGround)
				Profiler.SetValue("Is Ground", "", (IsGround ? 1.0f : 0.0f), maxFlagValue, debugColors.arrayIsGround);

			if (debugProfiler.isCeiling)
				Profiler.SetValue("Is Ceiling", "", (IsCeiling ? 1.0f : 0.0f), maxFlagValue, debugColors.arrayIsCeiling);

			if (debugProfiler.isCrouch)
				Profiler.SetValue("Is Crouch", "", (IsCrouch ? 1.0f : 0.0f), maxFlagValue, debugColors.arrayIsCrouch);

			if (debugProfiler.averageSpeed)
			{
				for (int i = 0; i < speedsBuffer.Length - 1; i++)
					speedsBuffer[i] = speedsBuffer[i + 1];

				speedsBuffer[speedsBuffer.Length - 1] = (float)(worldTransform.Translate - lastPlayerPosition).Length / Game.IFps;
				lastPlayerPosition = worldTransform.Translate;

				float avgSpeed = 0.0f;
				for (int i = 0; i < speedsBuffer.Length; i++)
					avgSpeed += speedsBuffer[i];

				avgSpeed /= (float)speedsBuffer.Length;

				Profiler.SetValue("Avg Speed", "m/s", avgSpeed, maxAppliedHorizontalSpeed * 1.75f, debugColors.arrayAverageSpeed);
			}

			if (debugProfiler.autoStepping)
				Profiler.SetValue("Auto Stepping", "", (autoSteppingApplied ? 1.0f : 0.0f), maxFlagValue, debugColors.arrayAutoStepping);
		}
	}
#endif

	#endregion Debug

}
