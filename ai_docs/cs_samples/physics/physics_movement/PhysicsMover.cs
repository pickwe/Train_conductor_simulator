// Physics-based rigid body movement controller. Applies forces and torques
// based on WASD input with jump and brake support, using physics simulation
// for realistic momentum and collision response.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Moves rigid body using forces, torques, and impulses from keyboard input.
public partial class PhysicsMover : Component
{
	// Forward/backward movement force
	public float Force = 10.0f;
	// Turning torque strength
	public float Torque = 1.0f;
	// Vertical impulse for jumping
	public float JumpImpulse = 3.0f;
	// Maximum linear velocity limit
	public float MaxSpeed = 30.0f;
	// Maximum angular velocity limit
	public float MaxRotationSpeed = 10.0f;
	// Damping applied when braking
	public float BrakesStrength = 5.0f;

	// Rigid body to control
	private BodyRigid body = null;
	// Accumulated input direction from WASD keys
	private vec2 inputDirection = new();
	// Brake flag (Shift key)
	private bool brake = false;
	// Jump flag (Space key)
	private bool jump = false;

	// Body reference is obtained on init.
	private void Init()
	{
		Console.Onscreen = true;
		body = node.ObjectBodyRigid;
	}

	// Input is read each frame (accumulates until physics tick).
	private void Update()
	{
		body.MaxLinearVelocity = MaxSpeed;
		body.MaxAngularVelocity = MaxRotationSpeed;

		if (!Console.Active)
		{
			// W/S for forward/backward, A/D for turning
			inputDirection.y = (Input.IsKeyPressed(Input.KEY.W) ? 1.0f : 0.0f) - (Input.IsKeyPressed(Input.KEY.S) ? 1.0f : 0.0f);
			inputDirection.x = (Input.IsKeyPressed(Input.KEY.A) ? 1.0f : 0.0f) - (Input.IsKeyPressed(Input.KEY.D) ? 1.0f : 0.0f);
			brake |= Input.IsKeyPressed(Input.KEY.ANY_SHIFT);
			jump |= Input.IsKeyDown(Input.KEY.SPACE);
		}
	}

	// Physics forces are applied at fixed physics timestep.
	private void UpdatePhysics()
	{
		vec3 forward = node.GetWorldDirection(MathLib.AXIS.Y);
		vec3 up = node.GetWorldDirection(MathLib.AXIS.Z);

		// Ground contact check for jump and brake restrictions
		bool onGround = body.NumContacts != 0;

		// Apply movement force and steering torque
		body.AddForce(forward * inputDirection.y * Force);
		// Torque direction depends on movement direction for car-like steering
		body.AddTorque(up * inputDirection.x * MathLib.Sign(inputDirection.y) * Torque);

		// Braking increases damping to slow down
		body.LinearDamping = brake && onGround ? BrakesStrength : 0.0f;
		body.AngularDamping = brake ? BrakesStrength : 0.0f;

		// Jump only when grounded
		if (jump && onGround)
		{
			body.AddLinearImpulse(up * JumpImpulse);
		}

		// Reset input flags after physics update
		brake = false;
		jump = false;
	}
}
