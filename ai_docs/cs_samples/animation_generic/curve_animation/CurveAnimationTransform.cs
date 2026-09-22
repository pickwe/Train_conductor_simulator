// Demonstrates curve-based animation of node transforms. Position, rotation, and
// scale are animated independently using Curve2d objects for each axis. The curves
// are evaluated each frame based on elapsed time multiplied by speed.

using System.Collections;
using System.Collections.Generic;
using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4= Unigine.mat4;

#endif

// Animates node transform using curves for position, rotation, and scale.
public partial class CurveAnimationTransform : Component
{
	// Container for XYZ curve triplet
	[System.Serializable]
	public class Curves
	{
		// Curve for X axis animation
		[ShowInEditor, Parameter(Title = "X")]
		public Curve2d x;

		// Curve for Y axis animation
		[ShowInEditor, Parameter(Title = "Y")]
		public Curve2d y;

		// Curve for Z axis animation
		[ShowInEditor, Parameter(Title = "Z")]
		public Curve2d z;
	}

	// Curves controlling position animation per axis
	[ShowInEditor, Parameter(Title = "Position Curves")]
	public Curves position = new Curves();

	// Curves controlling rotation animation in degrees per axis
	[ShowInEditor, Parameter(Title = "Rotation Curves (deg)")]
	public Curves rotation = new Curves();

	// Curves controlling scale animation per axis
	[ShowInEditor, Parameter(Title = "Scale Curves")]
	public Curves scale_prop = new Curves();

	// Playback speed multiplier for all curves
	[ShowInEditor, Parameter(Title = "Speed")]
	public float speed = 1.0f;

	// Accumulated animation time
	private float time = 0.0f;

	// Curves are evaluated and transform is updated each frame.
	private void Update()
	{
		time += Game.IFps * speed;

		// Evaluate position curves
		float x = position.x.Evaluate(time);
		float y = position.y.Evaluate(time);
		float z = position.z.Evaluate(time);

		// Evaluate rotation curves
		float angX = rotation.x.Evaluate(time);
		float angY = rotation.y.Evaluate(time);
		float angZ = rotation.z.Evaluate(time);

		// Evaluate scale curves with fallback to 1.0 if no keys defined
		float scaleX = scale_prop.x.NumKeys > 1 ? scale_prop.x.Evaluate(time) : 1.0f;
		float scaleY = scale_prop.y.NumKeys > 1 ? scale_prop.y.Evaluate(time) : 1.0f;
		float scaleZ = scale_prop.z.NumKeys > 1 ? scale_prop.z.Evaluate(time) : 1.0f;

		// Compose final transform from translation, rotation, and scale
		node.Transform = new Mat4(
			MathLib.Translate(new Vec3(x, y, z))
			* MathLib.Rotate(new quat(angX, angY, angZ))
			* MathLib.Scale(new Vec3(scaleX, scaleY, scaleZ))
			);
	}
}
