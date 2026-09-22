// Moves a node in a circular path for demonstrating the CameraPersecutor.
// The node orbits around the origin at configurable radius and speed,
// providing a moving target for the follow camera to track.

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

// Moves node in a circular path as a target for follow camera.
public partial class CameraPersecutorTarget : Component
{
	// Radius of circular path
	public float radius = 5.0f;
	// Angular speed of movement
	public float speed = 0.2f;

	// Node position is updated along circular path.
	private void Update()
	{
		float x = radius * MathLib.Cos(Game.Time * speed);
		float y = radius * MathLib.Sin(Game.Time * speed);

		node.WorldPosition = new Vec3(x, y, 0);
	}
}
