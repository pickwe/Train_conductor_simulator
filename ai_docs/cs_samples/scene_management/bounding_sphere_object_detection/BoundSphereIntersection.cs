// Demonstrates spatial intersection queries using a spherical volume.
// Creates a sphere at a given center point with specified radius and continuously
// checks which objects intersect it. Useful for proximity detection, explosion
// radii, audio source ranges, or any gameplay mechanic requiring distance-based checks.

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

// Finds all static mesh objects inside a spherical radius.
public partial class BoundSphereIntersection : Component
{
	// Center point of the sphere in world space
	public dvec3 center = dvec3.ZERO;
	// Radius of the detection sphere
	public float radius = 1.0f;

	// The bounding sphere used for intersection queries
	private WorldBoundSphere boundSphere;
	// Reusable list to receive intersection results
	private List<Node> nodes = null;
	// UI window showing intersection status
	private SampleDescriptionWindow sampleDescriptionWindow;

	// Creates the bounding sphere and initializes UI.
	private void Init()
	{
		boundSphere = new WorldBoundSphere(new Vec3(center), radius);

		// Create collection for intersecting nodes
		nodes = new List<Node>();

		Visualizer.Enabled = true;
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow();
	}

	// Visualizes the sphere and updates the list of objects inside it.
	private void Update()
	{
		// Draw the sphere so we can see its extent
		Visualizer.RenderBoundSphere(new BoundSphere(new vec3(boundSphere.Center), (float)boundSphere.Radius), Mat4.IDENTITY, new vec4(0.0f, 1.0f, 0.0f, 1.0f));

		// Query world for all static meshes intersecting our sphere
		var status = "Inside bound sphere:";
		bool res = World.GetIntersection(boundSphere, Node.TYPE.OBJECT_MESH_STATIC, nodes);
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
