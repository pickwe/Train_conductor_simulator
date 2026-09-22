// Demonstrates world space translation using node.WorldTranslate(). Moves the
// node along global world axes, oscillating back and forth. Compare with
// TransformTranslate to see the difference between world and local movement.

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

// Translates node in world space with oscillating motion.
public partial class TransformWorldTranslate : Component
{
	// Movement speed in units per second along world axes
	public dvec3 linearVelocity = dvec3.FORWARD;

	// Timer for oscillation tracking
	private float time = 0.0f;
	// Direction multiplier (1 or -1)
	private float timeSign = 1.0f;

	// Node oscillates back and forth in world space.
	private void Update()
	{
		// Reverse direction at bounds
		if (time < -3.0f || time > 3.0f)
			timeSign *= -1.0f;

		time += Game.IFps * timeSign;

		// Translate in world space
		node.WorldTranslate(new Vec3(linearVelocity * Game.IFps * timeSign));
	}
}
