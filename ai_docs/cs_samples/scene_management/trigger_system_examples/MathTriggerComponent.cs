// Custom trigger implementation using pure math bounds checking.
// Unlike physics-based triggers, this component manually tests a list of
// registered objects against a bounding volume (sphere or box) each frame.
// Fires EventEnter when an object first enters the bounds, and EventLeave
// when it exits. Useful for lightweight triggers without physics simulation,
// or when you need explicit control over which objects are monitored.

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

// Lightweight trigger using math bounds checking on a registered object list.
public partial class MathTriggerComponent : Component
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

	// Objects to check against the trigger bounds each frame
	private List<Node> objects = new List<Node>();
	public int NumObjects { get { return objects.Count; } }

	// Tracks which objects are currently inside (for enter/leave detection)
	private List<Node> entered = new List<Node>();

	// Bounding volumes for overlap testing
	private WorldBoundBox boundBox;
	private WorldBoundSphere boundSphere;

	// Fired when an object enters the trigger for the first time
	private EventInvoker<Node> eventEnter = new EventInvoker<Node>();
	public Event<Node> EventEnter {  get { return eventEnter; } }

	// Fired when an object leaves the trigger
	private EventInvoker<Node> eventLeave = new EventInvoker<Node>();
	public Event<Node> EventLeave {  get { return eventLeave; } }

	// Initializes bounding volumes at the component's position.
	void Init()
	{
		Vec3 translation = node.WorldTransform.Translate;
		Vec3 coordinatesForBoundBoxMin = new Vec3(-boundBoxSize * 0.5f) + translation;
		Vec3 coordinatesForBoundBoxMax = new Vec3(boundBoxSize * 0.5f) + translation;

		boundBox.Set(new vec3(coordinatesForBoundBoxMin), new vec3(coordinatesForBoundBoxMax));
		boundSphere.Set(new vec3(translation), boundSphereSize * 0.5f);
	}
	
	// Updates bounds position and checks all registered objects for enter/leave.
	void Update()
	{
		ReplaceBounds();

		if(debug)
		{
			VisualizeBounds();
		}

		CheckEnter();
		CheckLeave();
	}

	// Registers a node to be checked against this trigger.
	public void AddObject(Node obj)
	{
		objects.Add(obj);
	}

	// Registers multiple nodes at once.
	public void AddObjects(ICollection<Node> inputObjects)
	{
		objects.AddRange(inputObjects);
	}

	public Node GetObjectByIndex(int index)
	{
		return objects[index];
	}

	// Removes a node from the watch list.
	public void RemoveObject(Node obj)
	{
		objects.Remove(obj);
	}

	public void RemoveObjectByIndex(int index)
	{
		objects.RemoveAt(index);
	}

	public void ClearObjects()
	{
		objects.Clear();
	}

	// Checks for newly entered objects and fires EventEnter.
	private void CheckEnter()
	{
		foreach(Node obj in objects)
		{
			if (entered.Contains(obj))
				continue;

			bool isInside = isSphere ? CheckSphere(obj) : CheckBox(obj);
			if(isInside)
			{
				entered.Add(obj);
				eventEnter.Run(obj);
			}
		}
	}

	// Checks for objects that have left and fires EventLeave.
	private void CheckLeave()
	{
		foreach (Node obj in objects)
		{
			if (!entered.Contains(obj))
				continue;

			// Handle deleted objects without firing leave event
			if(obj.IsDeleted)
			{
				entered.Remove(obj);
				continue;
			}

			bool isInside = isSphere ? CheckSphere(obj) : CheckBox(obj);
			if (!isInside)
			{
				entered.Remove(obj);
				eventLeave.Run(obj);
			}
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

	// Tests if object's position is inside the spherical bounds.
	private bool CheckSphere(Node obj)
	{
		return boundSphere.InsideValid(new vec3(obj.Transform.Translate));
	}

	// Tests if object's position is inside the box bounds.
	private bool CheckBox(Node obj)
	{
		return boundBox.InsideValid(new vec3(obj.Transform.Translate));
	}
}

