// Converts a hierarchy of control point nodes into a smooth .path file.
// Uses Catmull-Rom centripetal spline interpolation for positions and SQUAD
// (Spherical Quadrangle Interpolation) for rotations to generate a smooth
// looping trajectory. The quality parameter controls subdivision density,
// and frame times are based on arc length for consistent playback speed.

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

// Generates smooth path files from control point hierarchies for use with WorldTransformPath.
public partial class PathTrajectorySaver : Component
{
	// Output path file location
	[ShowInEditor]
	private string pathFile = null;
	// Parent node whose children define control points for the spline
	[ShowInEditor]
	private Node pathNode = null;
	// Number of interpolated samples between each control point pair
	[ShowInEditor]
	private int quality = 25;
	// When true, generates the path file automatically on Init
	[ShowInEditor]
	private bool autosave = true;

	// Triggers path generation on component initialization.
	void Init()
	{
		if (autosave)
			Save();
	}

	// Interpolates between control points and saves the result as a .path file.
	private void Save()
	{
		Path path = new Path();
		path.Clear();

		// Child nodes define the control points (looping - last connects to first)
		// Frame time accumulates arc length for velocity-independent playback
		int points_count = pathNode.NumChildren;
		double frame_time = 0;
		for (int j = 0; j < points_count; j++)
		{
			int j_prev = (j - 1 < 0) ? (points_count - 1) : j - 1;
			int j_cur = j;
			int j_next = (j + 1) % points_count;
			int j_next_next = (j + 2) % points_count;

			// Get current control position and rotation
			Vec3 p0 = pathNode.GetChild(j_prev).WorldPosition;
			Vec3 p1 = pathNode.GetChild(j_cur).WorldPosition;
			Vec3 p2 = pathNode.GetChild(j_next).WorldPosition;
			Vec3 p3 = pathNode.GetChild(j_next_next).WorldPosition;

			quat q0 = pathNode.GetChild(j_prev).GetWorldRotation();
			quat q1 = pathNode.GetChild(j_cur).GetWorldRotation();
			quat q2 = pathNode.GetChild(j_next).GetWorldRotation();
			quat q3 = pathNode.GetChild(j_next_next).GetWorldRotation();

			// Calculate curve
			Vec3 start = Utils.CatmullRomCentripetal(p0, p1, p2, p3, 0);
			for (int i = 1; i < quality; i++)
			{
				path.AddFrame();

				float time = (float)i / (quality - 1);

				// Calculate segment position and rotation
				Vec3 end = Utils.CatmullRomCentripetal(p0, p1, p2, p3, time);
				quat rot = Utils.Squad(q0, q1, q2, q3, time);

				// Calculate current frame time based on arc length
				// Using distance ensures constant velocity when played back at speed=1
				double len = MathLib.Length(start - end);
				frame_time += len;

				path.SetFramePosition(path.NumFrames - 1, end);
				path.SetFrameRotation(path.NumFrames - 1, rot);
				path.SetFrameTime(path.NumFrames - 1, (float)frame_time);

				start = end;
			}
		}

		// Save to file
		path.Save(pathFile);
	}
}
