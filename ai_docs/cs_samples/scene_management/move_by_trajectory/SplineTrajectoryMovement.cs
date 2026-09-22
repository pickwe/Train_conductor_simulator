// Moves a node along a smooth Catmull-Rom spline defined by control points.
// Unlike SavedPathTrajectory which uses pre-baked paths, this component
// calculates spline positions in real-time using centripetal Catmull-Rom
// interpolation for positions and SQUAD for smooth rotation blending.
// Pre-calculates segment lengths to maintain constant velocity regardless
// of control point spacing. Best for runtime-modifiable paths.

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

using System.Collections.Generic;
using Unigine;
using System.Linq;
using static Unigine.Animations;

// Real-time Catmull-Rom spline movement with constant velocity compensation.
public partial class SplineTrajectoryMovement : Component
{
	// Parent node whose children define control points
	[ShowInEditor]
	private Node pathNode = null;
	// Movement speed in units per second
	[ShowInEditor]
	private float velocity = 10.0f;
	public float Velocity { get { return velocity; } set { velocity = value; } }
	// Subdivision quality for length calculation
	[ShowInEditor]
	private int quality = 25;
	// When true, draws the spline path
	[ShowInEditor]
	private bool debug;
	public bool Debug { get { return debug; } set { debug = value; } }

	// Pre-calculated arc lengths for each segment subdivision (for constant velocity)
	// Each segment is subdivided into 'quality' parts, storing cumulative lengths
	// to convert uniform time to arc-length parameterization
	private List<List<float>> lengths = new List<List<float>>();
	// Cached control point positions
	private List<Vec3> pointsPos = new List<Vec3>();
	// Cached control point rotations
	private List<quat> pointsRot = new List<quat>();
	// Current segment index in the spline
	private int pointsIndex = 0;
	// Normalized time within current segment (0 to 1)
	private float time = 0.0f;

	// Caches control points and pre-calculates arc lengths.
	void Init()
	{
		// Save positions and rotations from child nodes
		int numChilds = pathNode.NumChildren;
		for (int i = 0; i < numChilds; i++)
		{
			Node nc = pathNode.GetChild(i);
			pointsPos.Add(nc.WorldPosition);
			pointsRot.Add(nc.GetWorldRotation());
		}

		int pointsCount = pointsPos.Count;
		for (int j = 0; j < pointsCount; j++)
		{
			int j_prev = (j - 1 < 0) ? (pointsCount - 1) : j - 1;
			int j_cur = j;
			int j_next = (j + 1) % pointsCount;
			int j_next_next = (j + 2) % pointsCount;

			Vec3 p0 = pointsPos[j_prev];
			Vec3 p1 = pointsPos[j_cur];
			Vec3 p2 = pointsPos[j_next];
			Vec3 p3 = pointsPos[j_next_next];

			lengths.Add(Utils.GetLengthCatmullRomCentripetal(p0, p1, p2, p3, quality));
		}
	}

	// Interpolates position and rotation along the spline each frame.
	void Update()
	{
		// Adjust time step based on local arc length for constant velocity
		// Spline segments have varying arc lengths per unit time, so we scale
		// the time delta by inverse local length to maintain constant world-space speed
		float speed = velocity / (lengths[pointsIndex][(int)(time * (quality - 1))] * quality);
		UpdateTime(speed);

		// Get four control points needed for Catmull-Rom interpolation
		Vec3[] p = GetCurrentPoints();
		quat[] q = GetCurrentQuats();

		// Calculate smooth position and rotation at current time
		// Catmull-Rom centripetal: smooth curve passing through p[1] and p[2]
		// SQUAD: Spherical Quadrangle interpolation for smooth rotation blending
		Vec3 pos = Utils.CatmullRomCentripetal(p[0], p[1], p[2], p[3], time);
		quat rot = Utils.Squad(q[0], q[1], q[2], q[3], time);

		node.WorldPosition = pos;
		node.SetWorldRotation(rot, true);

		if (debug)
			VisualizePath();
	}

	// Draws the spline curve using line segments.
	private void VisualizePath()
	{
		int points_count = pointsPos.Count;
		for (int j = 0; j < points_count; j++)
		{
			int j_prev = (j - 1 < 0) ? (points_count - 1) : j - 1;
			int j_cur = j;
			int j_next = (j + 1) % points_count;
			int j_next_next = (j + 2) % points_count;

			Vec3 p0 = pointsPos[j_prev];
			Vec3 p1 = pointsPos[j_cur];
			Vec3 p2 = pointsPos[j_next];
			Vec3 p3 = pointsPos[j_next_next];

			// Draw curve
			Vec3 start = Utils.CatmullRomCentripetal(p0, p1, p2, p3, 0);
			int quality = 10;
			for (int i = 1; i < quality; i++)
			{
				Vec3 end = Utils.CatmullRomCentripetal(p0, p1, p2, p3, (float)i / (quality - 1));
				Visualizer.RenderLine3D(start, end, vec4.WHITE);
				start = end;
			}
		}
	}

	// Advances time and handles segment transitions with looping.
	private void UpdateTime(float speed)
	{
		time += speed * Game.IFps;
		if (time >= 1.0f)
		{
			// Move to next segment, wrapping around for loop
			pointsIndex = (pointsIndex + (int)time) % pointsPos.Count;
			time = MathLib.Frac(time);
		}
	}

	// Gets the four control points around the current segment for interpolation.
	// Catmull-Rom requires 4 points: previous, current, next, and next-next
	// The curve interpolates between current (p1) and next (p2), using
	// previous (p0) and next-next (p3) to determine tangent directions
	private Vec3[] GetCurrentPoints()
	{
		int points_count = pointsPos.Count;
		int i_prev = (pointsIndex - 1 < 0) ? (points_count - 1) : pointsIndex - 1;
		int i_cur = pointsIndex;
		int i_next = (pointsIndex + 1) % points_count;
		int i_next_next = (pointsIndex + 2) % points_count;

		Vec3[] result = new Vec3[4];
		result[0] = pointsPos[i_prev];
		result[1] = pointsPos[i_cur];
		result[2] = pointsPos[i_next];
		result[3] = pointsPos[i_next_next];
		return result;
	}

	// Gets the four control rotations around the current segment for SQUAD interpolation.
	private quat[] GetCurrentQuats()
	{
		int points_count = pointsPos.Count;
		int i_prev = (pointsIndex - 1 < 0) ? (points_count - 1) : pointsIndex - 1;
		int i_cur = pointsIndex;
		int i_next = (pointsIndex + 1) % points_count;
		int i_next_next = (pointsIndex + 2) % points_count;

		quat[] result = new quat[4];
		result[0] = pointsRot[i_prev];
		result[1] = pointsRot[i_cur];
		result[2] = pointsRot[i_next];
		result[3] = pointsRot[i_next_next];
		return result;
	}
}
