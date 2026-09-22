// Demonstrates local space rotation using node.Rotate(). Rotates the node
// around its own local axes at a configurable angular velocity. Compare with
// TransformWorldRotate to see the difference between local and world rotation.

using Unigine;

// Rotates node in local space at specified angular velocity.
public partial class TransformRotate : Component
{
	// Rotation speed in degrees per second (pitch, roll, yaw)
	public vec3 angularVelocity = new vec3(0.0f, 0.0f, 45.0f);

	// Node is rotated in local space each frame.
	private void Update()
	{
		node.Rotate(angularVelocity * Game.IFps);
	}
}
