// Demonstrates world space rotation using node.WorldRotate(). Rotates the node
// around global world axes at a configurable angular velocity. Compare with
// TransformRotate to see the difference between world and local rotation.

using Unigine;

// Rotates node in world space at specified angular velocity.
public partial class TransformWorldRotate : Component
{
	// Rotation speed in degrees per second (pitch, roll, yaw)
	public vec3 angularVelocity = new vec3(0.0f, 0.0f, 45.0f);

	// Node is rotated in world space each frame.
	private void Update()
	{
		vec3 offset = angularVelocity * Game.IFps;
		node.WorldRotate(offset.x, offset.y, offset.z);
	}
}
