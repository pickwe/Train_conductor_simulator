// Demonstrates physics body event callbacks. Builds a tower of rigid bodies
// that change materials based on frozen/position state and visualize contact
// points when collisions occur.

using System.Collections;
using System.Collections.Generic;
using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
#endif

// Tower of rigid bodies with frozen/position/contact event visualization.
public partial class BodyCallbacks : Component
{
	// Spacing between tower blocks
	public float space = 1.2f;
	// Number of levels in tower
	public int tower_level = 10;
	// Material applied when body is frozen
	public Material frozen_material;
	// Material applied when body moves
	public Material position_material;
	// Mesh file for tower blocks
	[ParameterFile(Filter = ".mesh")]
	public string mesh_file = "";

	// Collection of created tower objects
	private List<Node> objects = new List<Node>();
	// Event connections for body callbacks
	private EventConnections body_connections = new EventConnections();
	// Saved visualizer state
	private bool visualizer_state;

	// Tower is built with body event callbacks registered.
	void Init()
	{
		visualizer_state = Visualizer.Enabled;
		Visualizer.Enabled = true;

		// Parameters validation
		if (mesh_file.Length <= 0)
			Log.Error("BodyCallbacks.Init(): Mesh File parameter is empty!\n");

		if (!frozen_material)
			Log.Error("BodyCallbacks.Init(): Frozen Matreial parameter is empty!\n");

		if (!position_material)
			Log.Error("BodyCallbacks.Init(): Position Matreial parameter is empty!\n");

		// General physics settings
		Physics.FrozenLinearVelocity = 0.1f;
		Physics.FrozenAngularVelocity = 0.1f;
		Physics.NumIterations = 4;

		// Create object and physical body with a box shape
		ObjectMeshStatic obj = new ObjectMeshStatic(mesh_file);
		BodyRigid body = new BodyRigid(obj);
		ShapeBox shape = new ShapeBox(body, new vec3(1));
		obj.SetMaterial(position_material, "*");

		// Create tower
		for (int i = 0; i < tower_level; i++)
		{
			for (int j = 0; j < tower_level - i; j++)
			{
				// Clone created earlier object and calculate its position in a tower
				ObjectMeshStatic mesh = obj.Clone() as ObjectMeshStatic;
				mesh.WorldTransform = MathLib.Translate(new Vec3(0.0f, j - 0.5f * (tower_level - i) + 0.5f, i + 0.5f) * space);
				
				// Add Frozen, Position and Contact callbacks to new object's body
				body = mesh.BodyRigid;
				body.EventFrozen.Connect(body_connections, b => b.Object.SetMaterial(frozen_material, "*"));
				body.EventPosition.Connect(body_connections, b => b.Object.SetMaterial(position_material, "*"));
				body.EventContactEnter.Connect(body_connections, (b, num) => b.RenderContacts());
				objects.Add(mesh);
			}
		}
		obj.DeleteLater();
	}
	
	// Event connections are cleaned up and visualizer restored.
	void Shutdown()
	{
		// Remove all connections
		body_connections.DisconnectAll();
		objects.Clear();
		// Restore visualizer state
		Visualizer.Enabled = visualizer_state;
	}
}
