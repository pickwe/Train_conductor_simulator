// Projectile that moves forward and detects collisions with world objects.
// Spawns visual effects on creation and impact, deals damage to Robo targets,
// and self-destructs on collision with environment or enemies.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

using Unigine;

// Moves forward and damages targets on collision.
public partial class Projectile : Component
{
	// Movement speed in units per second
	public float moveSpeed = 5.0f;

	// Effect spawned when projectile is created
	[ParameterFile(Filter = ".node")]
	public string bulletSpawnFx;

	// Effect spawned on impact
	[ParameterFile(Filter = ".node")]
	public string bulletHitFx;

	// Stores intersection data for collision detection
	private WorldIntersectionNormal intersection = null;

	// Spawn effect is created at projectile position.
	private void Init()
	{
		// Spawn muzzle flash effect
		if (bulletSpawnFx != "")
		{
			Node bulletSpawn = World.LoadNode(bulletSpawnFx);
			bulletSpawn.WorldPosition = node.WorldPosition;
		}

		intersection = new WorldIntersectionNormal();
	}

	// Projectile moves forward and checks for collisions each frame.
	private void Update()
	{
		Vec3 oldPosition = node.WorldPosition;

		// Move projectile forward along its Y axis
		vec3 direction = node.GetWorldDirection(MathLib.AXIS.Y);
		node.WorldPosition += direction * moveSpeed * Game.IFps;

		// Check for collision between old and new position
		WorldIntersectionNormal intersection = new WorldIntersectionNormal();
		Unigine.Object hitObj = World.GetIntersection(oldPosition, node.WorldPosition, 0xFFFFFF, intersection);
		if (hitObj)
		{
			// Ignore collision with turret or other bullets
			if (hitObj.Name == "turret" || hitObj.Name == "bullet")
				return;

			// Deal damage to Robo if hit
			Robo robo = hitObj.GetComponent<Robo>();
			if (robo != null)
				robo.Hit(20);

			// Spawn hit effect at impact point
			if (bulletHitFx != "")
			{
				Node bulletHit = World.LoadNode(bulletHitFx);
				bulletHit.WorldPosition = intersection.Point;
				bulletHit.SetWorldDirection(intersection.Normal, vec3.UP, MathLib.AXIS.Y);
			}
			node.DeleteLater();
		}
	}

}
