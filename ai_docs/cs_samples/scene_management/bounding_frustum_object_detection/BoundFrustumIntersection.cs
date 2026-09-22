// Demonstrates spatial intersection queries using a view frustum volume.
// Creates a frustum (like a camera's view cone) with configurable FOV, aspect ratio,
// and near/far planes, then finds all objects inside it. Useful for visibility culling,
// AI line-of-sight checks, spotlight coverage detection, or custom camera queries.

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

// Finds all static mesh objects inside a frustum-shaped volume.
public partial class BoundFrustumIntersection : Component
{
	// Field of view angle in degrees (horizontal)
	public float fov = 100.0f;
	// Width-to-height ratio of the frustum
	public float aspect = 1.0f;
	// Distance to near clipping plane
	public float zNear = 0.001f;
	// Distance to far clipping plane
	public float zFar = 10.0f;
	// Frustum origin position in world space
	public vec3 position = vec3.ZERO;
	// Frustum orientation as Euler angles
	public vec3 rotation = vec3.ZERO;

	// Combined projection and transform matrix
	private mat4 frustumTransform = mat4.IDENTITY;
	// The frustum volume used for intersection queries
	private WorldBoundFrustum boundFrustum;
	// Reusable list to receive intersection results
	private List<Node> nodes = null;
	// UI window showing intersection status
	private SampleDescriptionWindow sampleDescriptionWindow;

	// Builds the frustum matrix and initializes intersection testing.
	private void Init()
	{
		// Build projection matrix from camera-like parameters
		frustumTransform = MathLib.Perspective(fov, aspect, zNear, zFar);
		// Rotate to align with UNIGINE coordinate system
		frustumTransform = frustumTransform * MathLib.RotateX(-90);
		frustumTransform = frustumTransform * MathLib.Translate(-position);
		frustumTransform = frustumTransform * MathLib.Rotate(new quat(rotation.x, rotation.y, rotation.z));

		// Create bound frustum
		boundFrustum = new WorldBoundFrustum(frustumTransform, Mat4.IDENTITY);

		// Create collection for intersecting nodes
		nodes = new List<Node>();

		Visualizer.Enabled = true;
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow();
	}

	// Visualizes the frustum and updates the list of objects inside it.
	private void Update()
	{
		// Draw the frustum volume for visual debugging
		Visualizer.RenderFrustum(frustumTransform, Mat4.IDENTITY, new vec4(0.0f, 1.0f, 0.0f, 1.0f));

		// Query world for all static meshes intersecting our frustum
		var status = "Inside bound frustum:";
		bool res = World.GetIntersection(boundFrustum, Node.TYPE.OBJECT_MESH_STATIC, nodes);
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
