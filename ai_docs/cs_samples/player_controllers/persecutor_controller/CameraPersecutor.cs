// Third-person follow camera that tracks a moving target. Maintains distance
// and allows orbiting around target with optional fixed angles mode. Camera
// automatically follows target movement while preserving user's viewing angle.

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

using Unigine;

// Follow camera that tracks a moving target with orbit controls.
public partial class CameraPersecutor : Component
{
	// Input handler for camera controls
	public CameraControls controls = null;
	// Rotation speed in degrees per second
	public float angularSpeed = 90.0f;
	// Zoom speed in units per second
	public float zoomSpeed = 3.0f;
	// Minimum distance from target
	public float minDistance = 5.0f;
	// Maximum distance from target
	public float maxDistance = 10.0f;
	// Vertical angle limits in degrees
	public float minVerticalAngle = -89.9f;
	public float maxVerticalAngle = 89.9f;
	// When true, angles are fixed relative to world; when false, angles update as target moves
	public bool useFixedAngles = false;
	// Node to follow
	public Node target = null;

	// Reference to attached PlayerDummy camera
	private PlayerDummy camera = null;

	// Current follow angles in degrees
	private float horizontalAngle = 0.0f;
	private float verticalAngle = 0.0f;
	// Current distance from target
	private float distance = 0.0f;

	// Saved mouse handle state for restoration
	private Input.MOUSE_HANDLE init_mouse_handle;

	// Camera is positioned at initial follow distance from target.
	private void Init()
	{
		camera = node as PlayerDummy;
		if (!camera)
			return;

		if (!target)
			return;

		init_mouse_handle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;

		// Get initial camera direction toward target
		vec3 direction = new vec3(target.WorldPosition - camera.WorldPosition);
		direction.Normalize();

		SetAngles(direction);

		// Set initial camera position at midpoint distance
		distance = minDistance + (maxDistance - minDistance) * 0.5f;
		camera.SetWorldDirection(direction, vec3.UP);
		camera.WorldPosition = target.WorldPosition - direction * distance;
	}

	// Camera follows target and responds to orbit input.
	private void Update()
	{
		if (!camera || !target || controls == null)
			return;

		// Get current direction to target
		vec3 direction = new vec3(target.WorldPosition - camera.WorldPosition);

		// Update distance based on input and current separation
		distance = direction.Length;
		distance -= controls.ZoomIn * zoomSpeed * Game.IFps;
		distance += controls.ZoomOut * zoomSpeed * Game.IFps;
		distance = MathLib.Clamp(distance, minDistance, maxDistance);

		// Recalculate angles from target direction (unless using fixed angles)
		if (!useFixedAngles)
			SetAngles(direction.Normalized);

		// Apply user input to angles
		verticalAngle -= controls.TurnUp * angularSpeed * Game.IFps;
		verticalAngle += controls.TurnDown * angularSpeed * Game.IFps;
		verticalAngle = MathLib.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

		horizontalAngle += controls.TurnRight * angularSpeed * Game.IFps;
		horizontalAngle -= controls.TurnLeft * angularSpeed * Game.IFps;

		// Calculate new camera direction from angles
		direction = vec3.FORWARD * MathLib.RotateZ(horizontalAngle);
		direction = direction * MathLib.Rotate(MathLib.Cross(direction, vec3.UP), verticalAngle);

		// Position camera at follow distance from target
		camera.SetWorldDirection(direction, vec3.UP);
		camera.WorldPosition = target.WorldPosition - direction * distance;
	}

	// Calculates horizontal and vertical angles from direction vector.
	private void SetAngles(vec3 currentDirection)
	{
		// Get projection of direction on XY plane
		vec3 horizontalDirection = currentDirection;
		horizontalDirection.z = 0;
		horizontalDirection.Normalize();

		// Get current vertical angle of camera
		verticalAngle = MathLib.Angle(currentDirection, horizontalDirection);
		verticalAngle *= -MathLib.Sign(currentDirection.z);
		verticalAngle = MathLib.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

		// Get current horizontal angle of camera
		horizontalAngle = MathLib.Angle(horizontalDirection, vec3.FORWARD);
		horizontalAngle *= MathLib.Sign(currentDirection.x);
	}

	// Mouse handle mode is restored on shutdown.
	private void Shutdown()
	{
		Input.MouseHandle = init_mouse_handle;
	}
}
