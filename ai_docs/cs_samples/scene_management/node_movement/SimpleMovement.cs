// Demonstrates three different approaches to implementing WASD keyboard movement.
// All three variants produce the same visual result but use different UNIGINE APIs:
// - SimpleMovement: Uses Translate/Rotate (relative movement in local space)
// - SimpleMovement2: Uses Position/Rotation properties (absolute values with manual transform)
// - SimpleMovement3: Uses Transform matrix (combines position and rotation in one operation)
// Each approach has trade-offs for code clarity, performance, and flexibility.

using System.Collections;
using System.Collections.Generic;
using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif

// Variant 1: Movement using Node.Translate() and Node.Rotate() methods.
// These methods apply incremental changes in the node's local coordinate space,
// making them the simplest approach for basic movement.
public partial class SimpleMovement : Component
{
	// Movement speed in units per second
	[ShowInEditor, Parameter(Title = "Velocity")]
	public float Velocity = 5.0f;

	// Rotation speed in degrees per second
	[ShowInEditor, Parameter(Title = "Angular velocity")]
	public float AngularVelocity = 50.0f;

	// Processes WASD input and applies movement each frame.
	void Update()
	{
		// Don't move while console is open
		if (Unigine.Console.Active)
			return;

		// W moves forward (+1), S moves backward (-1), both cancel out (0)
		int moveSign = (Input.IsKeyPressed(Input.KEY.W) ? 1 : 0) - (Input.IsKeyPressed(Input.KEY.S) ? 1 : 0);
		if (moveSign != 0)
		{
			Move(new Vec3(0.0f, (float)moveSign, 0.0f));
		}

		// A turns left (+1), D turns right (-1)
		int rotateSign = (Input.IsKeyPressed(Input.KEY.A) ? 1 : 0) - (Input.IsKeyPressed(Input.KEY.D) ? 1 : 0);
		if (moveSign != 0)
		{
			// Invert rotation when moving backwards for natural controls
			rotateSign *= moveSign;
		}
		if (rotateSign != 0)
		{
			Turn(new vec3(0.0f, 0.0f, (float)rotateSign));
		}
	}

	// Moves the node in its local Y direction (forward).
	public void Move(Vec3 dir)
	{
		Vec3 delta_movement = dir * Velocity * Game.IFps;
		node.Translate(delta_movement);
	}

	// Rotates the node around its local Z axis (yaw).
	public void Turn(vec3 dir)
	{
		vec3 delta_rotation = dir * AngularVelocity * Game.IFps;
		node.Rotate(delta_rotation);
	}
}
