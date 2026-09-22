// Demonstrates interactive mesh cluster manipulation. Left-click on cluster
// meshes to remove them, or click on ground to add new instances. Displays
// the current mesh count in the UI.

using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Interactive mesh cluster add/remove with mouse clicks.
public partial class ClusterSample : Component
{
	// Target mesh cluster node
	[Parameter(Title = "Cluster Node")]
	public ObjectMeshCluster cluster = null;

	// Intersection mask for ray casting
	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	public int intersectionMask = 1;

	// Intersection result data
	private WorldIntersection intersection = new WorldIntersection();

	// Sample UI window
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();

	// Z-offset for new mesh instances
	private const float OFFSET_Z = 0.5f;

	// Cluster reference is validated and UI is created.
	private void Init()
	{
		if (!cluster)
			Log.Error("ClusterSample.Init(): cannot get Cluster property\n");
		InitGui();
	}
	
	// Mouse clicks add or remove meshes from cluster.
	private void Update()
	{
		if (Unigine.Console.Active)
			return;

		// Handle left-click to add or remove mesh
		if (Unigine.Input.IsMouseButtonDown(Unigine.Input.MOUSE_BUTTON.LEFT))
		{
			// select a mesh or empty space using the mouse
			ivec2 mouse = Unigine.Input.MousePosition;
			Vec3 p0 = Game.Player.WorldPosition;
			Vec3 p1 = p0 + Game.Player.GetDirectionFromMainWindow(mouse.x, mouse.y) * 100;

			// check for an intersection with the cluster or ground
			Unigine.Object obj = World.GetIntersection(p0, p1, intersectionMask, intersection);
			if (obj)
			{
				// if obj is ObjectMeshCluster then remove a mesh
				if (obj == cluster)
				{
					int num = intersection.Instance;
					cluster.RemoveMeshTransform(num);
				}
				else 
				{
					// create transformation matrix for the new mesh
					Vec3 point = intersection.Point;
					point.z = OFFSET_Z;

					// add a single mesh in local space
					int new_index = cluster.AddMeshTransform();
					cluster.SetMeshTransform(new_index, new mat4(cluster.IWorldTransform * MathLib.Translate(point)));
					// add multiple meshes in global space
					// Mat4[] tr = {MathLib.Translate(point)};
					// cluster.AppendMeshes(tr);
				}

				// Spatial tree must be updated after any cluster modification.
				// This rebuilds the internal acceleration structure used for:
				// - Frustum culling (visibility determination)
				// - Intersection queries (raycasting against instances)
				// - LOD calculations based on camera distance
				cluster.UpdateSpatialTree();
			}
			UpdateGui();
		}
	}

	// UI is cleaned up on shutdown.
	private void Shutdown()
	{
		ShutdownGui();
	}

	// Creates UI displaying mesh count.
	private void InitGui()
	{
		sampleDescriptionWindow.createWindow();
		sampleDescriptionWindow.setStatus($"Number of meshes in the cluster: {cluster.NumMeshes}");
	}

	// Updates mesh count display.
	private void UpdateGui()
	{
		string status = $"Number of meshes in the cluster: {cluster.NumMeshes}";
		sampleDescriptionWindow.setStatus(status);
	}

	// Cleans up UI window.
	private void ShutdownGui()
	{
		sampleDescriptionWindow.shutdown();
	}
}
