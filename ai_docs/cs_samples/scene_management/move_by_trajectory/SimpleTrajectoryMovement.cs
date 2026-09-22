// Moves a node along a path using simple linear interpolation between waypoints.
// Unlike SplineTrajectoryMovement which uses smooth spline curves, this version
// uses straight-line Lerp for positions and Slerp for rotations. This creates
// visible corners at waypoints but is simpler to understand and cheaper to
// compute. Useful for learning path-following basics or when sharp turns are acceptable.

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
using System;

// Linear path-following with constant velocity between waypoints.
public partial class SimpleTrajectoryMovement : Component
{
	// Parent node whose children define the waypoints
	[ShowInEditor]
	private Node pathNode = null;
	// Movement speed in units per second
	[ShowInEditor]
	private float velocity = 10.0f;
	public float Velocity { get { return velocity; } set { velocity = value; } }
	// When true, draws straight lines between waypoints
	[ShowInEditor]
	private bool debug;
	public bool Debug { get { return debug; } set { debug = value; } }

	// Cached waypoint positions from child nodes
	private List<Vec3> pointsPos = new List<Vec3>();
	// Cached waypoint rotations from child nodes
	private List<quat> pointsRot = new List<quat>();
	// Starting point for current segment interpolation
	private Vec3 prevPoint = Vec3.ZERO;
	// Starting rotation for current segment interpolation
	private quat prevRot = quat.IDENTITY;
	// Index of the target waypoint we're moving toward
	private int pointsIndex = 0;
	// Normalized progress within current segment (0 to 1)
	private float time = 0.0f;

	// Caches waypoint data from child nodes.
	void Init()
	{
		int num_childs = pathNode.NumChildren;
		for (int i = 0; i < num_childs; i++)
		{
			Node nc = pathNode.GetChild(i);
			pointsPos.Add(nc.WorldPosition);
			pointsRot.Add(nc.GetWorldRotation());
		}
		prevPoint = node.WorldPosition;
	}

	// Linearly interpolates position and rotation toward next waypoint.
	void Update()
	{
		UpdateTime();

		// Lerp for position, Slerp for rotation (spherical linear interpolation)
		node.WorldPosition = MathLib.Lerp(prevPoint, pointsPos[pointsIndex], time);
		node.SetWorldRotation(MathLib.Slerp(prevRot, pointsRot[pointsIndex], time));

		if (debug)
			VisualizePath();
	}

	// Draws straight lines connecting all waypoints.
	private void VisualizePath()
	{
		for (int i = 0; i < pointsPos.Count; i++)
		{
			int next = (i + 1) % pointsPos.Count;
			Visualizer.RenderLine3D(pointsPos[i], pointsPos[next], vec4.WHITE);
		}
	}

	// Advances time based on segment length for constant velocity, handles transitions.
	private void UpdateTime()
	{
		// Calculate distance to target for velocity-adjusted timing
		float len = (float)MathLib.Length(pointsPos[pointsIndex] - prevPoint);

		// Scale time by inverse distance for constant world-space velocity
		time += velocity / len * Game.IFps;
		if (time >= 1.0f)
		{
			// Reached waypoint - save it as new starting point
			prevPoint = pointsPos[pointsIndex];
			prevRot = pointsRot[pointsIndex];

			// Advance to next waypoint, wrapping for loop
			pointsIndex = (pointsIndex + Convert.ToInt32(time)) % pointsPos.Count;
			time = MathLib.Frac(time);
		}
	}
}
