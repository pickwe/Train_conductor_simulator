// Waypoint-based AI movement controller for units in top-down games.
// Moves the unit along a path defined by child nodes of pathNode.
// Unit rotates smoothly towards next waypoint using torque-based steering.
// Loops through waypoints continuously when reaching the end of the path.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;
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

// Simple AI path-following controller with smooth steering.
public partial class CameraUnitPathControl : Component
{
	// ═══════════════════════════════════════════════════════════════════════════════
	// EDITOR PARAMETERS - Movement speed, steering, and path configuration
	// ═══════════════════════════════════════════════════════════════════════════════

	// Forward movement speed in units per second
	public float speed = 13;
	// Rotation speed multiplier for steering
	public float torque = 1;
	// Parent node containing waypoint children (order matters)
	public Node pathNode;
	// Distance threshold to consider waypoint reached
	public float epsD = 0.1f;

	// ═══════════════════════════════════════════════════════════════════════════════
	// RUNTIME STATE - Path data and current navigation progress
	// ═══════════════════════════════════════════════════════════════════════════════

	// Cached transforms of all waypoints
	private List<Mat4> path = new List<Mat4>(0);
	// Index of current target waypoint
	private int currentPathIndex = 0;
	// Direction along path (1 = forward, -1 = backward)
	private int dir = 1;
	// Accumulated rotation acceleration for smooth turning
	private float rotAcceleration;

	// ═══════════════════════════════════════════════════════════════════════════════
	// LIFECYCLE METHODS - Path initialization and movement update
	// ═══════════════════════════════════════════════════════════════════════════════

	// Builds path from children of pathNode, starts at closest waypoint.
	private void Init()
	{
		float minDistance = 1e+9f;
		if (pathNode != null)
		{
			// Collect all child node transforms as waypoints
			for (int i = 0; i < pathNode.NumChildren; i++)
			{
				var p = pathNode.GetChild(i).Transform;
				// Find closest waypoint to start from
				float distance = (float)MathLib.Length2(node.WorldPosition - p.Translate);
				if (distance < minDistance)
				{
					currentPathIndex = i;
					minDistance = distance;
				}
				path.Add(p);
			}
		}
	}

	// Moves unit towards current waypoint with smooth steering.
	private void Update()
	{
		if (path.Count == 0)
			return;

		var currentPosition = node.WorldPosition;
		var currentRot = node.GetWorldRotation();
		var route = path[currentPathIndex];

		float ifps = Game.IFps;
		if (ifps == 0)
			return;

		// Calculate direction to target waypoint
		vec3 ddir = new vec3(route.Translate - currentPosition);
		ddir.Normalize();

		// Steering: compare current heading (X axis) with desired direction
		// Positive dot = target is to the right, negative = to the left
		float d = MathLib.Dot(currentRot.Mat3.AxisX, ddir);
		if (MathLib.IsNaN(d))
			d = 0;
		// Smoothly accelerate/decelerate rotation towards target
		rotAcceleration += (d > rotAcceleration ? ifps : -ifps);

		// Apply rotation and forward movement
		currentRot = currentRot * new quat(vec3.UP, torque * ifps * -rotAcceleration);
		currentPosition = currentPosition + new vec3(currentRot.Mat3.AxisY * ifps * speed);

		node.WorldPosition = currentPosition;
		node.SetWorldRotation(currentRot);

		// Check if waypoint reached
		float distanceToTarget = (float)MathLib.Length(route.Translate - currentPosition);
		if(distanceToTarget < epsD)
		{
			// Advance to next waypoint (wraps around at ends)
			currentPathIndex += dir;

			if (currentPathIndex == path.Count)
				currentPathIndex = 0;

			if(currentPathIndex == -1)
				currentPathIndex = path.Count - 1;
		}
	}
}
