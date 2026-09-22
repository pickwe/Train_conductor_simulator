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

// Variant 3: Movement using Node.Transform matrix directly.
// Combines position and rotation into a single 4x4 matrix operation.
// More efficient when updating both simultaneously, and useful for
// complex transformations involving scale or hierarchies.
public partial class SimpleMovement3 : Component
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
			Turn(new Vec3(0.0f, 0.0f, (float)rotateSign));
		}
	}

	// Creates translation matrix and multiplies with current transform.
	private void Move(Vec3 dir)
	{
		Mat4 transform = node.Transform;
		Vec3 deltaMovement = dir * Velocity * Game.IFps;
		Mat4 deltaTransform = MathLib.Translate(deltaMovement);
		node.Transform = transform * deltaTransform;
	}

	// Creates rotation matrix and multiplies with current transform.
	private void Turn(Vec3 axis)
	{
		Mat4 transform = node.Transform;
		float deltaRotation = AngularVelocity * Game.IFps;
		Mat4 deltaTransform = MathLib.Rotate(axis, deltaRotation);
		node.Transform = transform * deltaTransform;
	}
}
