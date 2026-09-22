// Demonstrates spatial intersection queries using an axis-aligned bounding box (AABB).
// Creates a box volume defined by min/max world coordinates and continuously checks
// which objects are inside it. Useful for area triggers, selection boxes, spatial
// queries, or any gameplay mechanic that needs to know what's inside a rectangular region.

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
using static Unigine.VREyeTracking;

// Finds all static mesh objects inside an axis-aligned bounding box.
public partial class BoundBoxIntersection : Component
{
	// Minimum corner of the bounding box in world space
	public dvec3 minPoint = dvec3.ZERO;
	// Maximum corner of the bounding box in world space
	public dvec3 maxPoint = dvec3.ZERO;

	// The bounding box used for intersection queries
	private WorldBoundBox boundBox;
	// Reusable list to receive intersection results
	private List<Node> nodes = null;
	// UI window showing intersection status
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();

	// Creates the bounding box from min/max points and initializes UI.
	private void Init()
	{
		boundBox = new WorldBoundBox(new Vec3(minPoint), new Vec3(maxPoint));

		// Create collection for intersecting nodes
		nodes = new List<Node>();

		Visualizer.Enabled = true;
		sampleDescriptionWindow.createWindow();
	}

	// Visualizes the box and updates the list of objects inside it.
	private void Update()
	{
		// Draw the bounding box so we can see its extent
		Visualizer.RenderBoundBox(new BoundBox(new vec3(boundBox.minimum), new vec3(boundBox.maximum)), Mat4.IDENTITY, new vec4(0.0f, 1.0f, 0.0f, 1.0f));

		// Query world for all static meshes intersecting our box
		var status = "Inside bound box:";
		bool res = World.GetIntersection(boundBox, Node.TYPE.OBJECT_MESH_STATIC, nodes);
		if (res)
		{
			// Show nodes names
			foreach (Node n in nodes)
				status += $" {n.Name}";
		}
		else
			status += " empty";

		sampleDescriptionWindow.setStatus(status);
	}

	// Cleans up visualizer and UI on shutdown.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
		sampleDescriptionWindow.shutdown();
	}
}
