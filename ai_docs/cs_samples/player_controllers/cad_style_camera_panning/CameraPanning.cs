// CAD-style panning camera with object-based movement. Left mouse drags along
// the plane of clicked objects, right mouse rotates view, scroll wheel zooms.
// Useful for architectural visualization and modeling applications.

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

// Pan camera with object-plane dragging, rotation, and zoom.
public partial class CameraPanning : Component
{
	// Default pan speed when no object is grabbed
	public float defaultLinearSpeed = 0.02f;
	// Mouse rotation sensitivity
	public float mouseSensitivity = 0.08f;
	// Scroll wheel zoom sensitivity
	public float mouseWheelSensitivity = 0.5f;

	// Reference to attached PlayerDummy camera
	private PlayerDummy camera = null;

	// Current rotation angles in degrees
	private float horizontalAngle = 0.0f;
	private float verticalAngle = 0.0f;

	// Object grabbing state for plane-based panning
	private Vec3? grabbingPoint = Vec3.ZERO;
	private Vec3 grabbingCameraPos = Vec3.ZERO;
	private vec3 planeNormal = vec3.UP;
	private WorldIntersection intersection = null;

	// Mouse state for rotation mode
	private ivec2 savedMousePos = ivec2.ZERO;
	private Input.MOUSE_HANDLE init_mouse_handle;

	// Camera angles and intersection helper are initialized.
	private void Init()
	{
		camera = node as PlayerDummy;
		if (!camera)
			return;

		init_mouse_handle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;

		intersection = new WorldIntersection();

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

	// Mouse input is processed for panning, rotation, and zoom.
	private void Update()
	{
		if (!camera)
			return;

		// Left mouse down - try to grab object for plane-based panning
		if (Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT))
		{
			ivec2 mouse_coord = Input.MousePosition;
			camera.GetDirectionFromMainWindow(out var p0, out var p1, mouse_coord.x, mouse_coord.y);
			Node obj = World.GetIntersection(p0, p1, 0xFFFFFF, intersection);

			grabbingPoint = null;
			if (obj != null)
			{
				// Save grab point for camera movement
				grabbingPoint = intersection.Point;
				grabbingCameraPos = camera.WorldPosition;
				planeNormal = camera.ViewDirection;
			}
		}

		// Right mouse down - start rotation mode
		if (Input.IsMouseButtonDown(Input.MOUSE_BUTTON.RIGHT))
		{
			savedMousePos = Input.MousePosition;
			Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
			Input.MouseGrab = true;
		}

		// Left mouse held - pan camera
		if (Input.IsMouseButtonPressed(Input.MOUSE_BUTTON.LEFT))
		{
			if (grabbingPoint != null)
			{
				// Move camera based on grabbed object plane
				ivec2 mouse_coord = Input.MousePosition;
				camera.GetDirectionFromMainWindow(out var p0, out var p1, mouse_coord.x, mouse_coord.y);
				MathLib.LinePlaneIntersection(new vec3(p0), new vec3(p1), new vec3(grabbingPoint.Value), new vec3(planeNormal), out vec3 currentPoint);
				camera.WorldTranslate(grabbingPoint.Value - currentPoint);
			}
			else
				camera.Translate(new Vec3(-Input.MouseDeltaPosition.x * defaultLinearSpeed, Input.MouseDeltaPosition.y * defaultLinearSpeed, 0));
		}
		// Right mouse held - rotate camera
		else if (Input.IsMouseButtonPressed(Input.MOUSE_BUTTON.RIGHT))
		{
			// Update vertical angle with clamping
			verticalAngle += Input.MouseDeltaPosition.y * mouseSensitivity;
			verticalAngle = MathLib.Clamp(verticalAngle, -89.9f, 89.9f);

			// Update horizontal angle with wrapping
			horizontalAngle += Input.MouseDeltaPosition.x * mouseSensitivity;
			if (horizontalAngle < -180 || 180 < horizontalAngle)
				horizontalAngle = MathLib.Clamp(-horizontalAngle, -180.0f, 180.0f);

			// Calculate new camera direction from angles
			vec3 cameraDirection = vec3.FORWARD * MathLib.RotateZ(horizontalAngle);
			cameraDirection = cameraDirection * MathLib.Rotate(MathLib.Cross(cameraDirection, vec3.UP), verticalAngle);

			// Apply rotation to camera
			camera.SetWorldDirection(cameraDirection, vec3.UP);
		}

		// Right mouse up - exit rotation mode
		if (Input.IsMouseButtonUp(Input.MOUSE_BUTTON.RIGHT))
		{
			Input.MouseHandle = Input.MOUSE_HANDLE.USER;
			Input.MouseGrab = false;
			Input.MousePosition = savedMousePos;
		}

		// Scroll wheel - zoom in/out
		if (Input.MouseWheel != 0)
			camera.Translate(new Vec3(0, 0, -Input.MouseWheel * mouseWheelSensitivity));
	}

	// Mouse handle mode is restored on shutdown.
	private void Shutdown()
	{
		Input.MouseHandle = init_mouse_handle;
	}
}
