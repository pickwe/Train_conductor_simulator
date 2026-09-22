// Trigger implementation using World.GetIntersection() spatial queries.
// Unlike MathTriggerComponent which manually tracks a registered object list,
// this version uses the engine's spatial acceleration structures to find ALL
// nodes within the bounds automatically. More efficient for detecting any object
// in the world, but gives less control over which specific objects to monitor.
// Fires EventEnter/EventLeave when objects enter or leave the trigger volume.

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
using System.Collections.Generic;

// Trigger using World.GetIntersection() to detect any objects in bounds.
public partial class IntersectionTriggerComponent : Component
{
	// Diameter of the spherical trigger volume
	[ShowInEditor]
	private float boundSphereSize = 5.0f;
	// Size of the box trigger volume (uniform on all axes)
	[ShowInEditor]
	private float boundBoxSize = 5.0f;
	// When true uses sphere; when false uses box
	[ShowInEditor]
	private bool isSphere = false;
	// When true, visualizes the trigger bounds
	[ShowInEditor]
	private bool debug = false;
	// Intersection mask for filtering which objects are detected
	[ShowInEditor][ParameterMask]
	private int materialBallIntersectionMask = 0x7;
	public int MaterialBallIntersectionMask { get { return materialBallIntersectionMask; } }

	// Tracks which objects are currently inside (for enter/leave detection)
	private List<Node> entered = new List<Node>();
	// Refreshed each frame with current intersection query results
	private List<Node> inside = new List<Node>();

	// Bounding volumes for intersection queries
	private WorldBoundBox boundBox;
	private WorldBoundSphere boundSphere;

	// Fired when an object first enters the trigger
	private EventInvoker<Node> eventEnter = new EventInvoker<Node>();
	public Event<Node> EventEnter { get { return eventEnter; } }

	// Fired when an object leaves the trigger
	private EventInvoker<Node> eventLeave = new EventInvoker<Node>();
	public Event<Node> EventLeave { get { return eventLeave; } }

	// Initializes bounding volumes at the component's position.
	void Init()
	{
		Vec3 translation = node.WorldTransform.Translate;
		Vec3 coordinatesForBoundBoxMin = new Vec3(-boundBoxSize * 0.5f) + translation;
		Vec3 coordinatesForBoundBoxMax = new Vec3(boundBoxSize * 0.5f) + translation;
		boundBox.Set(new vec3(coordinatesForBoundBoxMin), new vec3(coordinatesForBoundBoxMax));
		boundSphere.Set(new vec3(translation), boundSphereSize * 0.5f);
	}
	
	// Queries the world for all nodes in bounds, then checks enter/leave.
	void Update()
	{
		ReplaceBounds();

		if(debug)
		{
			VisualizeBounds();
		}

		// Use engine's spatial query to find all nodes in bounds
		GetInsideNodes();
		CheckEnter();
		CheckLeave();
	}

	// Fires EventEnter for any objects newly detected inside.
	private void CheckEnter()
	{
		foreach(Node obj in inside)
		{
			if(!entered.Contains(obj))
			{
				entered.Add(obj);
				eventEnter.Run(obj);
			}
		}
	}

	// Fires EventLeave for objects that were inside but no longer detected.
	private void CheckLeave()
	{
		List<Node> toRemove = new List<Node>();

		foreach (Node obj in entered)
		{
			if (obj.IsDeleted)
				toRemove.Add(obj);

			if (!inside.Contains(obj))
			{
				toRemove.Add(obj);
				eventLeave.Run(obj);
			}
		}

		// Remove after iteration to avoid modifying collection during loop
		foreach(Node obj in toRemove)
		{
			entered.Remove(obj);
		}
	}

	// Updates bounds position to follow this node if it moves.
	private void ReplaceBounds()
	{
		Vec3 translation = node.WorldTransform.Translate;
		Vec3 coordinatesForBoundBoxMin = new Vec3(-boundBoxSize * 0.5f) + translation;
		Vec3 coordinatesForBoundBoxMax = new Vec3(boundBoxSize * 0.5f) + translation;
		boundBox.Set(new vec3(coordinatesForBoundBoxMin), new vec3(coordinatesForBoundBoxMax));
		boundSphere.Set(new vec3(translation), boundSphereSize * 0.5f);
	}

	// Renders the trigger volume for debugging.
	private void VisualizeBounds()
	{
		if (isSphere)
			Visualizer.RenderSphere(boundSphereSize * 0.5f, node.WorldTransform, vec4.RED);
		else
			Visualizer.RenderBoundBox(new BoundBox(new vec3(boundBox.minimum), new vec3(boundBox.maximum)), Mat4.IDENTITY, vec4.RED);
	}

	// Uses engine's spatial query to get all nodes inside the bounds.
	private void GetInsideNodes()
	{
		if (isSphere)
			World.GetIntersection(boundSphere, inside);
		else
			World.GetIntersection(boundBox, inside);
	}
}
