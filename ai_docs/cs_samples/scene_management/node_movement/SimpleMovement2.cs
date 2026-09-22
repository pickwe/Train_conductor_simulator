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

// Variant 2: Movement using Node.Position and Node.SetRotation() properties.
// This approach manually transforms the local direction by current rotation,
// giving you more control but requiring explicit coordinate conversion.
public partial class SimpleMovement2 : Component
{
	// Movement speed in units per second
	[ShowInEditor, Parameter(Title = "Velocity")]
	public float Velocity = 5.0f;

	// Rotation speed in degrees per second
	[ShowInEditor, Parameter(Title = "Angular velocity (deg/s)")]
	public float AngularVelocity = 50.0f;

	// Processes WASD input and applies movement each frame.
	void Update()
	{
		if (Console.Active)
			return;

		// W/S for forward/backward movement
		int moveSign = (Input.IsKeyPressed(Input.KEY.W) ? 1 : 0) - (Input.IsKeyPressed(Input.KEY.S) ? 1 : 0);
		if (moveSign != 0)
		{
			Move(new Vec3(0.0f, (float)moveSign, 0.0f));
		}

		// A/D for left/right rotation
		int rotateSign = (Input.IsKeyPressed(Input.KEY.A) ? 1 : 0) - (Input.IsKeyPressed(Input.KEY.D) ? 1 : 0);
		if (moveSign != 0)
		{
			// Invert rotation when moving backwards
			rotateSign *= moveSign;
		}
		if (rotateSign != 0)
		{
			Turn(new vec3(0.0f, 0.0f, (float)rotateSign));
		}
	}

	// Transforms local direction to world space using current rotation, then applies.
	private void Move(Vec3 dir)
	{
		Vec3 deltaMovement = node.GetRotation() * (dir * Velocity * Game.IFps);
		Vec3 oldPosition = node.Position;
		node.Position = oldPosition + deltaMovement;
	}

	// Multiplies current rotation by incremental rotation quaternion.
	private void Turn(vec3 axis)
	{
		float deltaRotation = AngularVelocity * Game.IFps;
		quat rot = node.GetRotation();
		node.SetRotation(rot * new quat(axis, deltaRotation));
	}
}
