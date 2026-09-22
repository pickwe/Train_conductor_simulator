// Custom spectator camera controller with keyboard/mouse look and optional collision.
// Uses sphere-based collision detection for camera clipping prevention.
// Features configurable WASD movement, sprint mode, and arrow key rotation.
// Provides public API for external camera angle manipulation and collision contact queries.

using System.Collections.Generic;
using System.Diagnostics;
using Unigine;


#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif

// Free-fly spectator camera with collision detection and customizable controls.
public partial class SpectatorController : Component
{
	// ═══════════════════════════════════════════════════════════════════════════════
	// INPUT SETTINGS - Keyboard bindings, mouse sensitivity, and control toggle
	// ═══════════════════════════════════════════════════════════════════════════════

	// Master toggle for accepting player input
	[ShowInEditor, Parameter(Group="Input", Title="Controlled", Tooltip="Toggle spectator inputs")]
	public bool isControlled = true; 

	[ShowInEditor, Parameter(Group="Input", Title="Forward Key", Tooltip="Move forward key")] 
	public string forwardKey = "W";

	[ShowInEditor, Parameter(Group="Input", Title="Left Key", Tooltip="Move left key")] 
	public string leftKey = "A";

	[ShowInEditor, Parameter(Group="Input", Title="Backward Key", Tooltip="Move backward key")] 
	public string backwardKey = "S";

	[ShowInEditor, Parameter(Group="Input", Title="Right Key", Tooltip="Move right key")] 
	public string rightKey = "D";

	[ShowInEditor, Parameter(Group="Input", Title="Down key", Tooltip="Move down key")]  
	public string downKey = "Q";

	[ShowInEditor, Parameter(Group="Input", Title="Up key", Tooltip="Move up key")] 
	public string upKey = "E";

	[ShowInEditor, Parameter(Group="Input", Title="Turn up key", Tooltip="Turn up")] 
	public string turnUpKey = "UP";

	[ShowInEditor, Parameter(Group="Input", Title="Turn down key", Tooltip="Turn down")] 
	public string turnDownKey = "DOWN";

	[ShowInEditor, Parameter(Group="Input", Title="Turn left key", Tooltip="Turn left")] 
	public string turnLeftKey = "LEFT";	

	[ShowInEditor, Parameter(Group="Input", Title="Turn right key", Tooltip="Turn right")] 
	public string turnRightKey = "RIGHT";

	[ShowInEditor, Parameter(Group="Input", Title="Sprint Key", Tooltip="Sprint mode activation key")] 
	public string accelerateKey = "ANY_SHIFT";

	// Mouse look sensitivity multiplier
	[ShowInEditor, Parameter(Group="Input", Title="Mouse Sensitivity", Tooltip="Mouse sensitivity multiplier")]
	public float mouseSensitivity = 0.4f;

	// ═══════════════════════════════════════════════════════════════════════════════
	// MOVEMENT SETTINGS - Speed, acceleration, and damping for smooth camera motion
	// ═══════════════════════════════════════════════════════════════════════════════

	// Normal movement speed in units per second
	[ShowInEditor, Parameter(Group="Movement", Title="Velocity", Tooltip="Movement velocity")]
	public float velocity = 2.0f;

	[ShowInEditor, Parameter(Group="Movement", Title="Sprint velocity", Tooltip="Sprint movement velocity")]
	public float sprintVelocity = 4.0f;

	[ShowInEditor, Parameter(Group="Movement", Title="Acceleration", Tooltip="Movement acceleration")]
	public float acceleration = 4.0f;

	// Deceleration rate when no input (higher = faster stop)
	[ShowInEditor, Parameter(Group="Movement", Title="Damping", Tooltip="Movement damping")]
	public float damping = 8.0f;

	// ═══════════════════════════════════════════════════════════════════════════════
	// COLLISION SETTINGS - Sphere-based collision detection to prevent camera clipping
	// ═══════════════════════════════════════════════════════════════════════════════

	// Enable/disable collision detection with world geometry
	[ShowInEditor, Parameter(Group="Collision", Title="Collision", Tooltip="Toggle spectator collision")]
	public bool isCollided = true;

	[ShowInEditor, ParameterMask(Group="Collision", MaskType = ParameterMaskAttribute.TYPE.COLLISION, Title="Collision Mask", Tooltip="A bit mask of collisions")]
	public int mask = 1;

	// Size of collision sphere (larger = more buffer from geometry)
	[ShowInEditor, Parameter(Group="Collision", Title="Collision radius", Tooltip="The radius of the collision sphere")]
	public float collisionRadius = 0.5f;

	// ═══════════════════════════════════════════════════════════════════════════════
	// ROTATION SETTINGS - Keyboard turn speed and pitch angle limits
	// ═══════════════════════════════════════════════════════════════════════════════

	// Degrees per second for arrow key rotation
	[ShowInEditor, Parameter(Group="Rotation", Title="Turning speed", Tooltip="Velocity of the spectator's turning action")]
	public float turning = 90f;

	[ShowInEditor, Parameter(Group="Rotation", Title="Min theta angle", Tooltip="Minimun pitch angle")]
	public float minThetaAngle = -89.9f;

	// Maximum look-up angle (clamped to prevent camera flip)
	[ShowInEditor, Parameter(Group="Rotation", Title="Max theta angle", Tooltip="Maximum pitch angle")]
	public float maxThetaAngle = 89.9f;

	// Fixed timestep for physics simulation (60 Hz)
	private const float PLAYER_SPECTATOR_IFPS = 1.0f / 60.0f;
	// Maximum collision resolution iterations per frame
	private const int PLAYER_SPECTATOR_COLLISIONS = 4;

	// Current world position of the spectator camera
	private Vec3 _position = Vec3.ZERO;
	// Current forward direction vector (normalized)
	private Unigine.vec3 _direction = Unigine.vec3.ZERO;

	// Horizontal rotation angle in degrees (yaw)
	private float _phiAngle = 0;
	// Vertical rotation angle in degrees (pitch)
	private float _thetaAngle = 0;

	// Previous frame values for change detection (optimization)
	private Vec3 _lastPosition = 0;
	private Unigine.vec3 _lastDirection = 0;
	private Unigine.vec3 _lastUp = 0;
	// Cached transform for external modification detection
	private Mat4 transform;

	// Collision contacts from current frame (reused each update)
	private List<ShapeContact> _contacts = [];

	// These properties provide convenient access to the specific key bindings 
	// for movement and camera control in the application. 
	// Each property retrieves the corresponding Input.KEY value using a key name string.
	// This design allows key mappings to be configured via string identifiers, 
	// making the input system more flexible and customizable.
	private Input.KEY ForwardKey => Input.GetKeyByName(forwardKey);
	private Input.KEY BackwardKey => Input.GetKeyByName(backwardKey);
	private Input.KEY RightKey => Input.GetKeyByName(rightKey);
	private Input.KEY LeftKey => Input.GetKeyByName(leftKey);
	private Input.KEY UpKey => Input.GetKeyByName(upKey);
	private Input.KEY DownKey => Input.GetKeyByName(downKey);
	private Input.KEY TurnUpKey => Input.GetKeyByName(turnUpKey);
	private Input.KEY TurnDownKey => Input.GetKeyByName(turnDownKey);
	private Input.KEY TurnLeftKey => Input.GetKeyByName(turnLeftKey);
	private Input.KEY TurnRightKey => Input.GetKeyByName(turnRightKey);
	private Input.KEY AccelerateKey => Input.GetKeyByName(accelerateKey);

	// Collision detection sphere (created at Init)
	private ShapeSphere _shapeSphere = null;
	// Reference to the Player component on this node
	private Player _player = null;

	// Sets horizontal rotation (yaw) and updates the view direction accordingly.
	public void SetPhiAngle(float phiAngle)
	{
		phiAngle -= _phiAngle;
		_direction = new quat(_player.Up, phiAngle) * _direction;
		_phiAngle += phiAngle;

		FlushTransform();
	}

	// Sets vertical rotation (pitch) with clamping to prevent gimbal lock.
	public void SetThetaAngle(float thetaAngle)
	{
		thetaAngle = MathLib.Clamp(thetaAngle, minThetaAngle, maxThetaAngle) - _thetaAngle;
		_direction = new quat(MathLib.Cross(_player.Up, _direction), thetaAngle) * _direction;
		_thetaAngle += thetaAngle;

		FlushTransform();
	}

	// Sets view direction directly and recalculates phi/theta angles from it.
	public void SetViewDirection(vec3 direction)
	{
		_direction = MathLib.Normalize(direction);

		vec3 tangent = vec3.ZERO;
		vec3 binormal = vec3.ZERO;

		MathLib.OrthoBasis(_player.Up,out tangent,out binormal);

		_phiAngle = MathLib.Atan2(MathLib.Dot(_direction, tangent), MathLib.Dot(_direction, binormal)) * MathLib.RAD2DEG;
		_thetaAngle = MathLib.Acos(MathLib.Clamp(MathLib.Dot(_direction, _player.Up), -1.0f, 1.0f)) * MathLib.RAD2DEG - 90.0f;
		_thetaAngle = MathLib.Clamp(_thetaAngle,minThetaAngle,maxThetaAngle);

		FlushTransform();
	}

	// Returns current horizontal rotation angle (yaw) in degrees.
	public float GetPhiAngle() =>_phiAngle;
	// Returns current vertical rotation angle (pitch) in degrees.
	public float GetThetaAngle() => _thetaAngle;
	// Returns current normalized view direction vector.
	public Unigine.vec3 GetViewdirection() => _direction;

	// This block exposes collision contact information for external physics handling.
	// Useful for custom physics responses or debugging collision issues.

	// Returns total number of collision contacts this frame.
	public int GetNumContacts() => _contacts.Count;

	// Returns full contact data structure for the specified contact index.
	public ShapeContact GetContact(int num)
	{
		Debug.Assert(num >= 0 && num < GetNumContacts(), "SpectatorController.GetContact(): bad contact number");
		return _contacts[num];
	}

	// Returns penetration depth (how far into the surface).
	public float GetContactDepth(int num)
	{
		Debug.Assert(num >= 0 && num < GetNumContacts(), "SpectatorController.GetContact(): bad contact number");
		return _contacts[num].Depth;
	}

	// Returns surface normal at contact point (points away from surface).
	public Unigine.vec3 GetContactNormal(int num)
	{
		Debug.Assert(num >= 0 && num < GetNumContacts(), "SpectatorController.GetContact(): bad contact number");
		return _contacts[num].Normal;
	}

	// Returns the Object that was collided with.
	public Unigine.Object GetContactObject(int num)
	{
		Debug.Assert(num >= 0 && num < GetNumContacts(), "SpectatorController.GetContact(): bad contact number");
		return _contacts[num].Object;
	}

	// Returns world-space position of the contact point.
	public Vec3 GetContactPoint(int num)
	{
		Debug.Assert(num >= 0 && num < GetNumContacts(), "SpectatorController.GetContact(): bad contact number");
		return _contacts[num].Point;
	}

	// Returns the collision Shape that was hit.
	public Shape GetContactShape(int num)
	{
		Debug.Assert(num >= 0 && num < GetNumContacts(), "SpectatorController.GetContact(): bad contact number");
		return _contacts[num].Shape0;
	}

	// Returns surface index within the Object's mesh.
	public int GetContactSurface(int num)
	{
		Debug.Assert(num >= 0 && num < GetNumContacts(), "SpectatorController.GetContact(): bad contact number");
		return _contacts[num].Surface;
	}

	// --- Input Processing ---

	// Reads keyboard/mouse input and converts to movement impulse.
	// Uses fixed timestep sub-stepping for consistent physics behavior.
	public void UpdateControls()
	{
		Unigine.vec3 up = _player.Up;

		Unigine.vec3 impulse = Unigine.vec3.ZERO;

		Unigine.vec3 tangent = Unigine.vec3.ZERO;
		Unigine.vec3 binormal = Unigine.vec3.ZERO;
		MathLib.OrthoBasis(_player.Up, out tangent, out binormal);

		if (isControlled && !Unigine.Console.Active)
		{
			if (Input.MouseCursorHide)
			{
				_phiAngle += Input.MouseDeltaPosition.x * mouseSensitivity;
				_thetaAngle += Input.MouseDeltaPosition.y * mouseSensitivity;
			}

			_thetaAngle += turning * Game.IFps * (MathLib.ToInt(Input.IsKeyPressed(TurnDownKey)) - MathLib.ToInt(Input.IsKeyPressed(TurnUpKey)));
			_thetaAngle = MathLib.Clamp(_thetaAngle, minThetaAngle, maxThetaAngle);

			_phiAngle += turning * Game.IFps * (MathLib.ToInt(Input.IsKeyPressed(TurnRightKey)) - MathLib.ToInt(Input.IsKeyPressed(TurnLeftKey)));

			Unigine.vec3 x = (new quat (up, -_phiAngle) * new quat(tangent, -_thetaAngle)) * binormal;
			Unigine.vec3 y = MathLib.Normalize(MathLib.Cross(up, x));
			Unigine.vec3 z = MathLib.Normalize(MathLib.Cross(x,y));

			_direction = x;

			impulse += x * (MathLib.ToInt(Input.IsKeyPressed(ForwardKey)) - MathLib.ToInt(Input.IsKeyPressed(BackwardKey)));
			impulse += y * (MathLib.ToInt(Input.IsKeyPressed(LeftKey)) - MathLib.ToInt(Input.IsKeyPressed(RightKey)));
			impulse += z * (MathLib.ToInt(Input.IsKeyPressed(UpKey)) - MathLib.ToInt(Input.IsKeyPressed(DownKey)));
			
			if (impulse != vec3.ZERO)
				impulse.Normalize();

			impulse *= Input.IsKeyPressed(AccelerateKey) ? sprintVelocity : velocity;
		}
		
		float time = Game.IFps;

		float targetVelocity = MathLib.Length(impulse);

		Vec3 playerVelocity = _player.Velocity;
		
		// Use do-while to ensure at least one update is processed,
		// even when the remaining time is very small (e.g., at high FPS).
		do
		{
			// Clamp the simulation step to a maximum fixed time interval (PLAYER_SPECTATOR_IFPS).
    		// This prevents instability or large jumps in movement and collisions when frame time is high (e.g., during frame drops). 
			float ifps = MathLib.Min(time, PLAYER_SPECTATOR_IFPS); 
			time -= ifps;
			UpdateMovement(impulse, ifps, targetVelocity);
		} while (time > MathLib.EPSILON);

	}

	// --- Transform Management ---

	// Applies position and direction to the node's world transform.
	// Only updates if values have changed (optimization).
	public void FlushTransform()
	{
		Unigine.vec3 up = _player.Up;
		
		if (_lastPosition != _position || _lastDirection != _direction || _lastUp != up)
		{
			node.WorldTransform = MathLib.SetTo(_position,_position + (Vec3)_direction, up);
			OnTransformChanged(); // update all internal params

			_lastPosition = _position;
			_lastDirection = _direction;
			_lastUp = up;
		}

	}

	// Syncs internal state when node transform is modified.
	// Recalculates phi/theta angles from the current world transform.
	private void OnTransformChanged()
	{	
		Unigine.vec3 up = _player.Up;

		Unigine.vec3 tangent = Unigine.vec3.ZERO; 
		Unigine.vec3 binormal = Unigine.vec3.ZERO;
		MathLib.OrthoBasis(up,out tangent, out binormal);

		_position = node.WorldTransform.GetColumn3(3);

		_direction = MathLib.Normalize(new Unigine.vec3(-node.WorldTransform.GetColumn3(2)));

		_phiAngle = MathLib.Atan2(MathLib.Dot(_direction, tangent), MathLib.Dot(_direction, binormal)) * MathLib.RAD2DEG;
		_thetaAngle = MathLib.Acos(MathLib.Clamp(MathLib.Dot(_direction, _player.Up), -1.0f, 1.0f)) * MathLib.RAD2DEG - 90.0f;
		_thetaAngle = MathLib.Clamp(_thetaAngle,minThetaAngle,maxThetaAngle);
		_direction = new quat(up, -_phiAngle) * new quat(tangent, -_thetaAngle) * binormal;
		
		_lastPosition = _position;
		_lastDirection = _direction;
		_lastUp = up;
	}

	// --- Movement & Collision ---

	// Applies velocity with acceleration/damping and resolves collisions.
	// Iteratively pushes camera out of geometry using contact normals.
	private void UpdateMovement(Unigine.vec3 impulse, float ifps, float targetVelocity)
	{
		float oldVelocity = MathLib.Length(_player.Velocity);

		_player.Velocity += impulse * acceleration * ifps;

		float currentVelocity = MathLib.Length(_player.Velocity);
		if (targetVelocity < MathLib.EPSILON || currentVelocity > targetVelocity)
			_player.Velocity *= MathLib.Exp(-damping * ifps);
		
		currentVelocity = MathLib.Length(_player.Velocity);
		if(currentVelocity > oldVelocity && currentVelocity > targetVelocity)
			_player.Velocity *= targetVelocity / currentVelocity;
		
		if(currentVelocity < MathLib.EPSILON)
			_player.Velocity = Unigine.vec3.ZERO;

		_position += _player.Velocity * ifps;

		_contacts.Clear();

		if (_player.Enabled && isCollided)
		{
			for (int i = 0; i < PLAYER_SPECTATOR_COLLISIONS; i++)
			{
				_shapeSphere.Center = _position;
				_shapeSphere.GetCollision(_contacts, ifps); 
				if (_contacts.Count == 0)
					break;
				
				// Calculate the inverse of the number of contacts to evenly distribute the total push-out
				// This prevents applying the full depth for every contact, which would overcompensate the position
				float inversContacts = 1.0f / MathLib.ToFloat(_contacts.Count); 
				for (int j = 0; j < _contacts.Count; j++)
				{
					ShapeContact contact = _contacts[j];

					// Push the player out along the contact normal, scaled by penetration depth and evenly divided by contact count
					_position += new Vec3(contact.Normal * (contact.Depth * inversContacts));

					// Remove the velocity component that's directed into the contact surface
    				// This prevents the player from continuing to move into the object
					_player.Velocity -= contact.Normal * MathLib.Dot(_player.Velocity, contact.Normal);  
				}
			}
		}

		_shapeSphere.Center = _position;
	}

	// Creates collision sphere and caches Player reference.
	private void Init()
	{
		_shapeSphere = new ShapeSphere(collisionRadius);
		_shapeSphere.Continuous = false;

		_player = node as Player;
		
		OnTransformChanged();
	}

	// Main update loop: detects external transform changes, processes input, applies transform.
	private void Update()
	{
		if (!transform.Equals(node.Transform)) // if somebody change our position outside -> update all internal params.
			OnTransformChanged();
			
		UpdateControls();
		FlushTransform();

		transform = node.Transform;
	}

	// Cleans up collision sphere to prevent memory leaks.
	private void Shutdown()
	{
		_shapeSphere.DeleteLater();
	}
}
