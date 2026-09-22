// Moves the laser pointer node to follow mouse cursor position in world space.
// Casts a ray from camera through mouse position, finds world intersection, and
// places the laser on that point (constrained to XY plane).

using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Laser pointer that follows mouse cursor position.
// Projects a ray from the camera through the mouse position
// and places the node at the world intersection point.
public partial class Laser : Component
{
	private WorldIntersection intersection = new WorldIntersection();

	// Laser position is updated to follow mouse cursor in world space.
	[MethodUpdate(Order = -1)]
	private void Update()
	{
		// Convert mouse screen coordinates to a 3D direction from the camera
		Player player = Game.Player;
		if (player == null)
			return;

		ivec2 mouseCoordinates = Input.MousePosition;
		Vec3 mouseDirection = new Vec3(player.GetDirectionFromMainWindow(mouseCoordinates.x, mouseCoordinates.y));

		// Cast a ray from camera through mouse position to find world intersection
		Vec3 cameraPosition = player.WorldPosition;
		if (World.GetIntersection(cameraPosition, cameraPosition + mouseDirection * player.ZFar, ~0, intersection))
		{
			Vec3 point = intersection.Point;
			// Keep laser on the same horizontal plane (XY plane movement only)
			point.z = node.WorldPosition.z;
			node.WorldPosition = point;
		}
	}
}
