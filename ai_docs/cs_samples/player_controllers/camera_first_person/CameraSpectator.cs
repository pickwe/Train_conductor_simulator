// First-person spectator camera with free movement in all directions. Uses
// CameraControls for input abstraction, supports 6DOF movement (forward/back,
// left/right, up/down) and mouse look with vertical angle clamping.

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

// Free-flying spectator camera with 6DOF movement and mouse look.
public partial class CameraSpectator : Component
{
	// Movement speed in units per second
	public float speed = 3.0f;
	// Rotation speed in degrees per second
	public float angularSpeed = 90.0f;

	// Input handler for camera controls
	public CameraControls controls = null;

	// Reference to attached PlayerDummy camera
	private PlayerDummy camera = null;

	// Current rotation angles in degrees
	private float horizontalAngle = 0.0f;
	private float verticalAngle = 0.0f;

	// Camera angles are initialized from current direction.
	private void Init()
	{
		camera = node as PlayerDummy;
		if (!camera)
			return;

		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;

		vec3 direction = camera.GetWorldDirection();

		// Get projection of direction on XY plane
		vec3 horizontalDirection = direction;
		horizontalDirection.z = 0;
		horizontalDirection.Normalize();

		// Get current vertical angle of camera
		verticalAngle = MathLib.Angle(direction, horizontalDirection);
		verticalAngle *= -MathLib.Sign(direction.z);

		// Get current horizontal angle of camera
		horizontalAngle = MathLib.Angle(horizontalDirection, vec3.FORWARD);
		horizontalAngle *= MathLib.Sign(direction.x);
	}

	// Camera position and rotation are updated based on input.
	private void Update()
	{
		if (!camera || controls == null)
			return;

		vec3 forward = camera.GetWorldDirection();
		vec3 right = camera.GetWorldDirection(MathLib.AXIS.X);
		vec3 up = camera.GetWorldDirection(MathLib.AXIS.Y);

		// Calculate movement direction from input
		vec3 targetVelocityDirection = vec3.ZERO;

		targetVelocityDirection += forward * controls.Forward;
		targetVelocityDirection -= forward * controls.Backward;

		targetVelocityDirection += right * controls.Right;
		targetVelocityDirection -= right * controls.Left;

		targetVelocityDirection += up * controls.Up;
		targetVelocityDirection -= up * controls.Down;

		// Apply movement with acceleration modifier
		float currentSpeed = speed * controls.Acceleration;

		if (targetVelocityDirection.Length2 > 0)
			targetVelocityDirection.Normalize();

		camera.WorldTranslate(new Vec3(targetVelocityDirection) * currentSpeed * Game.IFps);

		// Update vertical angle with clamping to prevent gimbal lock
		verticalAngle -= controls.TurnUp * angularSpeed * Game.IFps;
		verticalAngle += controls.TurnDown * angularSpeed * Game.IFps;
		verticalAngle = MathLib.Clamp(verticalAngle, -89.9f, 89.9f);

		// Update horizontal angle with wrapping
		horizontalAngle += controls.TurnRight * angularSpeed * Game.IFps;
		horizontalAngle -= controls.TurnLeft * angularSpeed * Game.IFps;

		if (horizontalAngle < -180 || 180 < horizontalAngle)
			horizontalAngle = MathLib.Clamp(-horizontalAngle, -180.0f, 180.0f);

		// Calculate new camera direction from angles
		vec3 cameraDirection = vec3.FORWARD * MathLib.RotateZ(horizontalAngle);
		cameraDirection = cameraDirection * MathLib.Rotate(MathLib.Cross(cameraDirection, vec3.UP), verticalAngle);

		// Apply rotation to camera
		camera.SetWorldDirection(cameraDirection, vec3.UP);
	}
}
