// Orbit camera that rotates around a target node at configurable distance.
// Supports zoom in/out with distance limits and vertical angle constraints.
// Camera always looks at the target while orbiting around it.

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

// Orbits around a target node with zoom and angle constraints.
public partial class CameraOrbit : Component
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
	// Node to orbit around
	public Node target = null;

	// Reference to attached PlayerDummy camera
	private PlayerDummy camera = null;

	// Current orbit angles in degrees
	private float horizontalAngle = 0.0f;
	private float verticalAngle = 0.0f;
	// Current distance from target
	private float distance = 0.0f;

	// Saved mouse handle state for restoration
	private Input.MOUSE_HANDLE init_mouse_handle;

	// Camera is positioned at initial orbit distance around target.
	private void Init()
	{
		camera = node as PlayerDummy;
		if (!camera)
			return;

		if (!target)
			return;

		init_mouse_handle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;

		// Get camera direction toward target
		vec3 direction = new vec3(target.WorldPosition - camera.WorldPosition);
		direction.Normalize();

		// Get projection of direction on XY plane
		vec3 horizontalDirection = direction;
		horizontalDirection.z = 0;
		horizontalDirection.Normalize();

		// Get current vertical angle of camera
		verticalAngle = MathLib.Angle(direction, horizontalDirection);
		verticalAngle *= -MathLib.Sign(direction.z);
		verticalAngle = MathLib.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

		// Get current horizontal angle of camera
		horizontalAngle = MathLib.Angle(horizontalDirection, vec3.FORWARD);
		horizontalAngle *= MathLib.Sign(direction.x);

		// Set initial camera position at midpoint distance
		distance = minDistance + (maxDistance - minDistance) * 0.5f;
		camera.SetWorldDirection(direction, vec3.UP);
		camera.WorldPosition = target.WorldPosition - direction * distance;
	}

	// Camera orbits around target based on input and zoom.
	private void Update()
	{
		if (!camera || !target || controls == null)
			return;

		// Update vertical angle with clamping
		verticalAngle -= controls.TurnUp * angularSpeed * Game.IFps;
		verticalAngle += controls.TurnDown * angularSpeed * Game.IFps;
		verticalAngle = MathLib.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

		// Update horizontal angle with wrapping
		horizontalAngle += controls.TurnRight * angularSpeed * Game.IFps;
		horizontalAngle -= controls.TurnLeft * angularSpeed * Game.IFps;

		if (horizontalAngle < -180 || 180 < horizontalAngle)
			horizontalAngle = MathLib.Clamp(-horizontalAngle, -180.0f, 180.0f);

		// Calculate new camera direction from angles
		vec3 cameraDirection = vec3.FORWARD * MathLib.RotateZ(horizontalAngle);
		cameraDirection = cameraDirection * MathLib.Rotate(MathLib.Cross(cameraDirection, vec3.UP), verticalAngle);

		// Update distance with clamping
		distance -= controls.ZoomIn * zoomSpeed * Game.IFps;
		distance += controls.ZoomOut * zoomSpeed * Game.IFps;
		distance = MathLib.Clamp(distance, minDistance, maxDistance);

		// Position camera at orbit distance from target
		camera.SetWorldDirection(cameraDirection, vec3.UP);
		camera.WorldPosition = target.WorldPosition - cameraDirection * distance;
	}

	// Mouse handle mode is restored on shutdown.
	private void Shutdown()
	{
		Input.MouseHandle = init_mouse_handle;
	}
}
