#region Math Types
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

using System.Collections.Generic;
using Unigine;

/// <summary>
/// Compact first-person controller with a kinematic capsule, stable collision
/// resolution, buffered variable-height jumping, interaction and a crosshair.
	/// Attach it to any node; the camera and capsule are created automatically.
/// The node origin is treated as the bottom point of the player capsule.
/// </summary>
public partial class FP_Controller : Component
{
	#region Editor Parameters

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Walking speed in meters per second")]
	private float walkSpeed = 3.5f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Running speed while Shift is held")]
	private float runSpeed = 6.5f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Ground acceleration and braking")]
	private float groundAcceleration = 35.0f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Air steering acceleration")]
	private float airAcceleration = 8.0f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Initial upward jump speed")]
	private float jumpSpeed = 6.0f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Time after leaving an edge during which jumping is still allowed")]
	private float coyoteTime = 0.12f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Time an early jump press is remembered before landing")]
	private float jumpBufferTime = 0.12f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Upward velocity multiplier when Space is released")]
	private float jumpReleaseMultiplier = 0.45f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Maximum walkable slope angle")]
	private float maxGroundAngle = 55.0f;

	[ShowInEditor, Parameter(Group = "Movement", Tooltip = "Small downward speed that keeps the capsule attached to the ground")]
	private float groundSnapSpeed = 1.0f;

	[ShowInEditor, Parameter(Group = "Body", Tooltip = "Capsule radius")]
	private float capsuleRadius = 0.32f;

	[ShowInEditor, Parameter(Group = "Body", Tooltip = "Height of the cylindrical part of the capsule")]
	private float capsuleHeight = 1.15f;

	[ShowInEditor, ParameterMask(Group = "Body", MaskType = ParameterMaskAttribute.TYPE.COLLISION)]
	private int collisionMask = 1;

	[ShowInEditor, ParameterMask(Group = "Body", MaskType = ParameterMaskAttribute.TYPE.PHYSICS_INTERSECTION)]
	private int physicsIntersectionMask = 1;

	[ShowInEditor, Parameter(Group = "Camera", Tooltip = "Camera height above the player origin")]
	private float eyeHeight = 1.65f;

	[ShowInEditor, Parameter(Group = "Camera", Tooltip = "Mouse sensitivity")]
	private float mouseSensitivity = 0.12f;

	[ShowInEditor, Parameter(Group = "Camera")]
	private float fieldOfView = 70.0f;

	[ShowInEditor, Parameter(Group = "Camera Bob", Tooltip = "Enable subtle camera movement while walking")]
	private bool useHeadBob = true;

	[ShowInEditor, Parameter(Group = "Camera Bob", Tooltip = "Number of walking cycles per second")]
	private float headBobFrequency = 1.7f;

	[ShowInEditor, Parameter(Group = "Camera Bob", Tooltip = "Horizontal camera movement in meters")]
	private float headBobHorizontal = 0.011f;

	[ShowInEditor, Parameter(Group = "Camera Bob", Tooltip = "Vertical camera movement in meters")]
	private float headBobVertical = 0.030f;

	[ShowInEditor, Parameter(Group = "Camera Bob", Tooltip = "How smoothly the bob starts and stops")]
	private float headBobSmoothing = 12.0f;

	[ShowInEditor, Parameter(Group = "Interaction", Tooltip = "Interaction distance from the camera")]
	private float interactionDistance = 2.5f;

	[ShowInEditor, ParameterMask(Group = "Interaction", MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	private int interactionMask = ~0;

	[ShowInEditor, Parameter(Group = "HUD", Tooltip = "Show a crosshair in the screen center")]
	private bool showCrosshair = true;

	[ShowInEditor, ParameterFile, Parameter(Group = "HUD", Tooltip = "Optional crosshair texture; an empty value shows a simple dot")]
	private string crosshairImage = "";

	[ShowInEditor, Parameter(Group = "HUD", Tooltip = "Crosshair sprite size in pixels")]
	private ivec2 crosshairSize = new ivec2(24, 24);

	#endregion

	#region Public State

	public bool IsGrounded { get; private set; }
	public Vec3 Velocity { get; private set; } = Vec3.ZERO;
	public Unigine.Object CurrentInteractable { get; private set; }

	/// <summary>Fired when E is pressed and an object is under the crosshair.</summary>
	public Event<Unigine.Object> EventInteract { get { return eventInteract; } }

	#endregion

	private const float fixedStep = 1.0f / 120.0f;
	private const int collisionIterations = 4;

	private readonly List<ShapeContact> contacts = new List<ShapeContact>();
	private readonly EventInvoker<Unigine.Object> eventInteract = new EventInvoker<Unigine.Object>();
	private readonly Node[] interactionExclude = new Node[1];

	private BodyDummy body;
	private ShapeCapsule capsule;
	private ObjectDummy physicsObject;
	private PlayerDummy camera;
	private Player previousPlayer;
	private Widget crosshair;
	private Input.MOUSE_HANDLE previousMouseHandle;
	private Mat4 playerTransform = Mat4.IDENTITY;

	private float yaw;
	private float verticalAngle = 90.0f;
	private float coyoteTimer;
	private float jumpBufferTimer;
	private float groundCos;
	private float headBobPhase;
	private Vec3 headBobOffset = Vec3.ZERO;
	private bool initialized;
	private bool wasMouseGrabbed;

	private void Init()
	{
		Unigine.Object playerObject = node as Unigine.Object;
		if (!playerObject)
		{
			// A physics body can only belong to an Object. Most convenient player
			// roots are NodeDummy, so create an invisible runtime physics owner.
			physicsObject = new ObjectDummy();
			physicsObject.Name = "FP_Controller_RuntimeBody";
			physicsObject.WorldTransform = node.WorldTransform;
			playerObject = physicsObject;
		}

		capsuleRadius = MathLib.Max(capsuleRadius, 0.05f);
		capsuleHeight = MathLib.Max(capsuleHeight, 0.05f);
		groundCos = MathLib.Cos(MathLib.Clamp(maxGroundAngle, 0.0f, 89.0f) * MathLib.DEG2RAD);

		playerTransform = playerObject.WorldTransform;
		Vec3 position = playerTransform.Translate;
		yaw = playerObject.GetWorldRotation().GetAngle(vec3.UP);
		playerTransform.SetRotate(Vec3.UP, yaw);
		playerTransform.SetColumn3(3, position);

		body = new BodyDummy(playerObject);
		body.Transform = playerTransform;
		capsule = new ShapeCapsule(body, capsuleRadius, capsuleHeight);
		body.SetShapeTransform(body.NumShapes - 1,
			MathLib.Translate(vec3.UP * (capsuleRadius + capsuleHeight * 0.5f)));
		// A zero mask disables every collision. Use all layers as a safe fallback
		// so a freshly added component cannot silently turn into noclip mode.
		capsule.CollisionMask = collisionMask == 0 ? ~0 : collisionMask;
		capsule.PhysicsIntersectionMask = physicsIntersectionMask == 0 ? ~0 : physicsIntersectionMask;

		previousPlayer = Game.Player;
		if (previousPlayer)
			previousPlayer.MainPlayer = false;

		camera = new PlayerDummy();
		camera.Parent = node;
		camera.Fov = fieldOfView;
		camera.ZNear = 0.02f;
		camera.ZFar = 1000.0f;
		camera.MainPlayer = true;
		Game.Player = camera;

		interactionExclude[0] = node;
		previousMouseHandle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
		CreateCrosshair();

		initialized = true;
		UpdateCamera();
	}

	private void Update()
	{
		if (!initialized)
			return;

		float frameTime = Game.IFps * Physics.Scale;
		if (frameTime <= 0.0f)
		{
			CurrentInteractable = null;
			InteractableHoverOutline.Draw(null, null);
			return;
		}

		UpdateLook();
		UpdateJumpInput(frameTime);
		UpdateHorizontalVelocity(frameTime);

		float timeLeft = frameTime;
		while (timeLeft > 0.0f)
		{
			float step = MathLib.Min(timeLeft, fixedStep);
			timeLeft -= step;
			Simulate(step);
		}

		node.WorldTransform = playerTransform;
		body.Transform = playerTransform;
		UpdateHeadBob(frameTime);
		UpdateCamera();
		UpdateInteraction();
	}

	private void UpdateLook()
	{
		if (!Input.MouseGrab)
		{
			wasMouseGrabbed = false;
			return;
		}

		// Ignore the first delta after a menu releases the cursor. It can include
		// the cursor's movement across the UI and would otherwise rotate the view.
		if (!wasMouseGrabbed)
		{
			wasMouseGrabbed = true;
			return;
		}

		// Raw deltas continue past screen edges, so the player can turn freely
		// even when the cursor is hidden after leaving a menu.
		yaw -= Input.MouseDeltaRaw.x * mouseSensitivity;
		verticalAngle -= Input.MouseDeltaRaw.y * mouseSensitivity;
		verticalAngle = MathLib.Clamp(verticalAngle, 0.1f, 179.9f);

		Vec3 position = playerTransform.Translate;
		playerTransform.SetRotate(Vec3.UP, yaw);
		playerTransform.SetColumn3(3, position);
	}

	private void UpdateHorizontalVelocity(float frameTime)
	{
		vec2 input = vec2.ZERO;
		if (Input.MouseGrab)
		{
			if (Input.IsKeyPressed(Input.KEY.W)) input.y += 1.0f;
			if (Input.IsKeyPressed(Input.KEY.S)) input.y -= 1.0f;
			if (Input.IsKeyPressed(Input.KEY.D)) input.x += 1.0f;
			if (Input.IsKeyPressed(Input.KEY.A)) input.x -= 1.0f;
		}

		if (input.Length2 > 1.0f)
			input.Normalize();

		float speed = Input.IsKeyPressed(Input.KEY.ANY_SHIFT) ? runSpeed : walkSpeed;
		Vec3 desired = new Vec3(playerTransform.AxisX) * input.x * speed
			+ new Vec3(playerTransform.AxisY) * input.y * speed;
		desired.z = 0.0f;

		Vec3 horizontal = new Vec3(Velocity.x, Velocity.y, 0.0f);
		float acceleration = IsGrounded ? groundAcceleration : airAcceleration;
		Vec3 difference = desired - horizontal;
		float maxChange = acceleration * frameTime;

		if (difference.Length > maxChange && difference.Length > MathLib.EPSILON)
			horizontal += difference.Normalized * maxChange;
		else
			horizontal = desired;

		Velocity = new Vec3(horizontal.x, horizontal.y, Velocity.z);
	}

	private void UpdateJumpInput(float frameTime)
	{
		coyoteTimer = IsGrounded ? coyoteTime : MathLib.Max(0.0f, coyoteTimer - frameTime);
		jumpBufferTimer = MathLib.Max(0.0f, jumpBufferTimer - frameTime);

		if (Input.MouseGrab && Input.IsKeyDown(Input.KEY.SPACE))
			jumpBufferTimer = jumpBufferTime;

		if (jumpBufferTimer > 0.0f && (IsGrounded || coyoteTimer > 0.0f))
		{
			Velocity = new Vec3(Velocity.x, Velocity.y, jumpSpeed);
			IsGrounded = false;
			coyoteTimer = 0.0f;
			jumpBufferTimer = 0.0f;
		}

		if (Input.IsKeyUp(Input.KEY.SPACE) && Velocity.z > 0.0f)
			Velocity = new Vec3(Velocity.x, Velocity.y, Velocity.z * jumpReleaseMultiplier);
	}

	private void Simulate(float step)
	{
		if (!IsGrounded || Velocity.z > 0.0f)
			Velocity += new Vec3(Physics.Gravity) * step;
		else
			Velocity = new Vec3(Velocity.x, Velocity.y, -groundSnapSpeed);

		playerTransform.SetColumn3(3, playerTransform.Translate + Velocity * step);
		body.Transform = playerTransform;
		IsGrounded = false;

		for (int iteration = 0; iteration < collisionIterations; iteration++)
		{
			capsule.GetCollision(contacts);
			if (contacts.Count == 0)
				break;

			Vec3 correction = Vec3.ZERO;
			for (int i = 0; i < contacts.Count; i++)
			{
				ShapeContact contact = contacts[i];
				correction += new Vec3(contact.Normal) * contact.Depth;

				float upDot = MathLib.Dot(contact.Normal, vec3.UP);
				if (upDot >= groundCos && Velocity.z <= 0.0f)
					IsGrounded = true;

				float intoSurface = MathLib.Dot(new vec3(Velocity), contact.Normal);
				if (intoSurface < 0.0f)
					Velocity -= new Vec3(contact.Normal) * intoSurface;
			}

			correction /= contacts.Count;
			playerTransform.SetColumn3(3, playerTransform.Translate + correction);
			body.Transform = playerTransform;
		}

		if (IsGrounded && Velocity.z < 0.0f)
			Velocity = new Vec3(Velocity.x, Velocity.y, -groundSnapSpeed);
	}

	private void UpdateCamera()
	{
		camera.WorldPosition = playerTransform * (new Vec3(0.0f, 0.0f, eyeHeight) + headBobOffset);

		vec3 direction = vec3.FORWARD * MathLib.RotateZ(-yaw);
		direction = direction * MathLib.Rotate(MathLib.Cross(direction, vec3.UP), 90.0f - verticalAngle);
		direction.Normalize();
		camera.SetWorldDirection(direction, vec3.UP);
	}

	private void UpdateHeadBob(float frameTime)
	{
		Vec3 horizontalVelocity = new Vec3(Velocity.x, Velocity.y, 0.0f);
		float horizontalSpeed = (float)horizontalVelocity.Length;
		bool isWalking = useHeadBob && IsGrounded && horizontalSpeed > 0.1f;

		Vec3 targetOffset = Vec3.ZERO;
		if (isWalking)
		{
			float speedRatio = MathLib.Clamp(horizontalSpeed / MathLib.Max(walkSpeed, MathLib.EPSILON), 0.0f, 1.35f);
			headBobPhase += headBobFrequency * speedRatio * MathLib.PI2 * frameTime;

			targetOffset.x = MathLib.Sin(headBobPhase) * headBobHorizontal * speedRatio;
			targetOffset.z = MathLib.Sin(headBobPhase * 2.0f) * headBobVertical * speedRatio;
		}

		float blend = MathLib.Saturate(headBobSmoothing * frameTime);
		headBobOffset = MathLib.Lerp(headBobOffset, targetOffset, blend);
	}

	private void UpdateInteraction()
	{
		EInteractable interactable = null;
		if (Input.MouseGrab)
		{
			Vec3 rayStart = camera.WorldPosition;
			Vec3 rayEnd = rayStart + new Vec3(camera.GetWorldDirection()) * interactionDistance;
			Unigine.Object hit = World.GetIntersection(rayStart, rayEnd, interactionMask, interactionExclude);
			if (hit != null)
				interactable = ComponentSystem.GetComponentInParent<EInteractable>(hit);
			if (interactable != null && interactable.IsAvailable)
				CurrentInteractable = hit;
			else
				CurrentInteractable = null;
		}
		else
		{
			CurrentInteractable = null;
		}

		InteractableHoverOutline.Draw(
			CurrentInteractable != null && interactable != null ? interactable.InteractionNode : null,
			CurrentInteractable);
		if (Input.MouseGrab && Input.IsKeyDown(Input.KEY.E) && CurrentInteractable && interactable != null)
		{
			interactable.Interact();
			eventInteract.Run(CurrentInteractable);
		}
	}

	private void CreateCrosshair()
	{
		if (!showCrosshair)
			return;

		if (crosshairImage != "")
		{
			WidgetSprite sprite = new WidgetSprite(crosshairImage);
			sprite.Width = crosshairSize.x;
			sprite.Height = crosshairSize.y;
			crosshair = sprite;
		}
		else
		{
			WidgetLabel dot = new WidgetLabel("•");
			dot.FontSize = crosshairSize.y;
			dot.FontOutline = 1;
			crosshair = dot;
		}

		Gui.GetCurrent().AddChild(crosshair, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);
	}

	private void Shutdown()
	{
		Input.MouseHandle = previousMouseHandle;
		InteractableHoverOutline.Shutdown();
		crosshair?.DeleteLater();
		crosshair = null;

		if (camera)
			camera.DeleteLater();

		if (physicsObject)
			physicsObject.DeleteLater();

		if (previousPlayer)
		{
			previousPlayer.MainPlayer = true;
			Game.Player = previousPlayer;
		}
	}
}
