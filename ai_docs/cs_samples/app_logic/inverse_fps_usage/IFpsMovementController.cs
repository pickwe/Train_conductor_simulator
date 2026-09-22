// Demonstrates frame-rate independent movement using Game.IFps. Compares
// movement with and without delta time multiplication, showing how IFps
// ensures consistent speed regardless of frame rate.

using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Moves node back and forth, optionally using frame-rate independent timing.
public partial class IFpsMovementController : Component
{
	// Toggle between frame-rate dependent and independent movement
	[ShowInEditor]
	[Parameter(Title = "Use IFps")]
	private bool useIFps = false;

	// Base movement speed (units per second when using IFps)
	[ShowInEditor]
	[Parameter(Title = "Movement speed")]
	private float movementSpeed = 1.0f;

	// Current movement direction
	Vec3 current_dir = Vec3.RIGHT;

	// Node is translated each frame, with speed optionally scaled by delta time.
	void Update()
	{
		// Apply movement with or without frame-rate independence
		if (useIFps)
		{
			node.Translate(current_dir * movementSpeed * Game.IFps);
		}
		else
		{
			node.Translate(current_dir * movementSpeed);
		}

		// Reverse direction at boundaries
		if (node.WorldPosition.x > 5)
			current_dir = Vec3.LEFT;
		if (node.WorldPosition.x < -5)
			current_dir = Vec3.RIGHT;
	}
}
