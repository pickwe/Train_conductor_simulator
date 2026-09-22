// Top-down RTS-style camera controller with orbit, zoom, and panning functionality.
// Supports mouse wheel zoom with automatic pitch adjustment (closer = steeper angle),
// edge scrolling for viewport panning, and middle-mouse drag for precise positioning.
// Q/E keys rotate camera around pivot point. Press F to focus on selected objects.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unigine;
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

// RTS-style camera that orbits around a pivot point with zoom and pan controls.
public partial class CameraTopDown : Component
{
	// Horizontal rotation angle around vertical axis (Q/E keys)
	public float phi = 180.0f;
	// Vertical tilt angle (pitch) - auto-adjusts with zoom distance
	public float theta = 40.0f;
	// Min/max pitch angle limits (x = min, y = max)
	public vec2 thetaMinMax = new vec2(10.0f, 70.0f);

	// Distance from camera to pivot point
	public float distance = 25.0f;
	// Min/max zoom distance limits
	public vec2 distanceMinMax = new vec2(5.0f, 45.0f);

	// Interpolation speed for smooth camera transitions
	public float zoomSpeed = 5.0f;

	private Player camera;
	private WorldIntersection intersection = new WorldIntersection();
	// Vectors for middle-mouse panning - tracks world position under cursor
	private vec3 previousMouseToIntersectionPointVector;
	private vec3 currentMouseToIntersectionPointVector;
	// True when middle-mouse drag started on a valid surface
	private bool isPreviousHooked;

	// Current and target values for smooth interpolation
	private float currentPhi = 180.0f;
	private float targetPhi = 180.0f;

	private float currentTheta = 40.0f;
	private float targetTheta = 40.0f;
	private float maxTheta = 70.0f;
	private float minTheta = 10.0f;

	private Scalar currentDistance = 25.0f;
	private Scalar targetDistance = 25.0f;
	private Scalar maxDistance = 45.0f;
	private Scalar minDistance = 5.0f;

	private float interpolationFactor = 1.0f;

	// Pivot point that camera orbits around (ground level)
	private Vec3 currentCameraPivotPosition;
	private Vec3 targetCameraPivotPosition;

	// Pitch angle change per unit of distance (for auto-tilt on zoom)
	private float degreesPerUnit = 1.0f;

	private Input.MOUSE_HANDLE init_mouse_handle;

	// Reference to selection system for focus-on-selection feature
	CameraSelection selection;

	// Initializes camera parameters and creates selection system.
	private void Init()
	{
		// Copy editor values to runtime state
		targetPhi = phi;
		currentPhi = targetPhi;
		targetTheta = theta;
		currentTheta = targetTheta;
		minTheta = thetaMinMax.x;
		maxTheta = thetaMinMax.y;

		targetDistance = distance;
		currentDistance = targetDistance;
		minDistance = distanceMinMax.x;
		maxDistance = distanceMinMax.y;

		interpolationFactor = zoomSpeed;

		// Calculate pitch-per-distance ratio for smooth tilt during zoom
		if (maxDistance != minDistance)
			degreesPerUnit =  (maxTheta - minTheta) / (float)(maxDistance - minDistance);
		else
			degreesPerUnit = 0.0f;

		camera = node as Player;
		if(camera == null)
		{
			Log.Error("CameraTopDown::init(): camera is not valid\n");
			return;
		}

		// Set initial pivot at camera position, fixed ground height
		targetCameraPivotPosition = node.WorldPosition;
		targetCameraPivotPosition.z = 2.0f;
		currentCameraPivotPosition = targetCameraPivotPosition;

		init_mouse_handle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;

		// Calculate initial view direction from spherical coordinates (phi, theta)
		vec3 cameraViewDirection = new quat(vec3.UP, currentPhi) * vec3.FORWARD;
		cameraViewDirection = new quat(MathLib.Cross(vec3.UP, cameraViewDirection), -currentTheta) * cameraViewDirection * -1;
		cameraViewDirection.Normalize();

		camera.ViewDirection = cameraViewDirection;
		camera.WorldPosition = currentCameraPivotPosition - cameraViewDirection * currentDistance;

		// Create selection system on a dummy node
		NodeDummy logic = new NodeDummy();
		selection = ComponentSystem.AddComponent<CameraSelection>(logic);
	}
	
	// Processes input for camera rotation, zoom, panning, and edge scrolling.
	private void Update()
	{
		if (!camera)
			return;

		if(!Unigine.Console.Active)
		{
			// Middle-mouse button down: raycast to find ground point for panning
			if(Input.IsMouseButtonDown(Input.MOUSE_BUTTON.MIDDLE))
			{
				ivec2 mouse = Input.MousePosition;
				vec3 rayDir = camera.GetDirectionFromMainWindow(mouse.x, mouse.y);
				Unigine.Object obj = World.GetIntersection(camera.WorldPosition, camera.WorldPosition + rayDir * 10000, ~0, intersection);
				if (obj != null)
				{
					isPreviousHooked = true;
					previousMouseToIntersectionPointVector = new vec3(intersection.Point - camera.WorldPosition);
				}
				else
				{
					isPreviousHooked = false;
				}
			}

			// Mouse wheel: zoom in/out with automatic pitch adjustment
			int mouseAxis = Input.MouseWheel;
			if(mouseAxis != 0)
			{
				targetDistance = MathLib.Clamp(targetDistance - mouseAxis, minDistance, maxDistance);
				targetTheta = MathLib.Clamp(targetTheta - mouseAxis, minTheta, maxTheta);
			}

			// Q/E keys: rotate camera around pivot
			if(Input.IsKeyPressed(Input.KEY.Q))
			{
				targetPhi -= 50.0f * Game.IFps;
			}
			if (Input.IsKeyPressed(Input.KEY.E))
			{
				targetPhi += 50.0f * Game.IFps;
			}

			// F key: focus on selected objects, zoom to fit their bounding sphere
			if(Input.IsKeyPressed(Input.KEY.F) && selection.Selection)
			{
				targetDistance = selection.BoundRadius;
				targetCameraPivotPosition = selection.Center;
				targetCameraPivotPosition.z = 2.0f;

				targetDistance = MathLib.Clamp(targetDistance, minDistance, maxDistance);
				targetTheta = MathLib.Clamp(minTheta + (float)(targetDistance - minDistance) * degreesPerUnit, minTheta, maxTheta);
			}

			// Middle-mouse drag: pan camera by tracking ground point under cursor
			if(Input.IsMouseButtonPressed(Input.MOUSE_BUTTON.MIDDLE) && isPreviousHooked)
			{
				ivec2 mouse = Input.MousePosition;
				currentMouseToIntersectionPointVector = camera.GetDirectionFromMainWindow(mouse.x, mouse.y);
				// Scale ray direction to match previous ground plane distance
				currentMouseToIntersectionPointVector *= previousMouseToIntersectionPointVector.z / currentMouseToIntersectionPointVector.z;

				vec3 displacement = currentMouseToIntersectionPointVector - previousMouseToIntersectionPointVector;

				targetCameraPivotPosition -= displacement;
				currentCameraPivotPosition -= displacement;
				previousMouseToIntersectionPointVector = currentMouseToIntersectionPointVector;
			}
			else
			{
				// Edge scrolling: move camera when cursor is near window edges
				vec3 forward = new quat(vec3.UP, targetPhi) * vec3.FORWARD * -1;
				forward.Normalize();
				vec3 right = new quat(vec3.UP, targetPhi) * vec3.RIGHT * -1;
				right.Normalize();
				ivec2 mouse = Input.MousePosition;
				ivec2 windowPos = WindowManager.MainWindow.Position;
				ivec2 windowSize = WindowManager.MainWindow.RenderSize;

				// 10-pixel edge zones trigger scrolling
				if (mouse.x < windowPos.x + 10)
					targetCameraPivotPosition -= right * 10.0f * Game.IFps;

				if (mouse.y < windowPos.y + 10)
					targetCameraPivotPosition += forward * 10.0f * Game.IFps;

				if (mouse.x > windowPos.x + windowSize.x - 10)
					targetCameraPivotPosition += right * 10.0f * Game.IFps;

				if (mouse.y > windowPos.y + windowSize.y - 10)
					targetCameraPivotPosition -= forward * 10.0f * Game.IFps;

			}
		}

		// Smoothly interpolate all camera parameters towards targets
		currentPhi = MathLib.Lerp(currentPhi, targetPhi, interpolationFactor * Game.IFps);
		currentTheta = MathLib.Lerp(currentTheta, targetTheta, interpolationFactor * Game.IFps);
		currentDistance = MathLib.Lerp(currentDistance, targetDistance, interpolationFactor * Game.IFps);
		currentCameraPivotPosition = MathLib.Lerp(currentCameraPivotPosition, targetCameraPivotPosition, interpolationFactor * Game.IFps);
	}

	// Applies camera transform after all updates complete (ensures smooth visuals).
	private void PostUpdate()
	{
		// Convert spherical coordinates (phi, theta) to view direction
		vec3 cameraViewDirection = new quat(vec3.UP, currentPhi) * vec3.FORWARD;
		cameraViewDirection = new quat(MathLib.Cross(vec3.UP, cameraViewDirection), -currentTheta) * cameraViewDirection * -1;
		cameraViewDirection.Normalize();

		camera.ViewDirection = cameraViewDirection;
		// Position camera at distance from pivot along the inverse view direction
		camera.WorldPosition = currentCameraPivotPosition - cameraViewDirection * currentDistance;
	}

	private void Shutdown()
	{
		Input.MouseHandle = init_mouse_handle;
	}

	// Instantly moves camera pivot to position (no interpolation).
	public void SetPosition(vec3 pos)
	{
		targetCameraPivotPosition = pos;
		targetCameraPivotPosition.z = 2.0f;
		currentCameraPivotPosition = targetCameraPivotPosition;
	}

	// Sets target position for smooth camera pan.
	public void SetTargetPosition(vec3 pos)
	{
		targetCameraPivotPosition = pos;
		targetCameraPivotPosition.z = 2.0f;
	}

	// Instantly sets zoom distance with matching pitch angle (no interpolation).
	public void SetDistance(float dist)
	{
		targetDistance = dist;

		targetDistance = MathLib.Clamp(targetDistance, minDistance, maxDistance);
		targetTheta = MathLib.Clamp(minTheta + (float)(targetDistance - minDistance) * degreesPerUnit, minTheta, maxTheta);

		currentDistance = targetDistance;
		currentTheta = targetTheta;
	}

	// Sets target zoom distance for smooth zoom transition.
	public void SetTargetDistance(float dist)
	{
		targetDistance = dist;

		targetDistance = MathLib.Clamp(targetDistance, minDistance, maxDistance);
		targetTheta = MathLib.Clamp(minTheta + (float)(targetDistance - minDistance) * degreesPerUnit, minTheta, maxTheta);
	}


}
