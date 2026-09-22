// Demonstrates physics joint breaking events. Creates a hinge-connected bridge
// with weight blocks that break joints when force limits are exceeded, changing
// materials on broken sections via event callbacks.

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

// Breakable bridge with joint broken event material changes.
public partial class JointCallbacks : Component
{
	// Number of bridge segments
	public int bridge_sections = 14;
	// Material for broken joint sections
	public Material broken_materal;
	// Material for intact joint sections
	public Material joint_materal;
	// Mesh file for bridge sections
	[ParameterFile(Filter = ".mesh")]
	public string mesh_file = "";

	// Collection of created objects
	private List<Node> objects = new List<Node>();
	// Event connections for joint callbacks
	private EventConnections joint_connections = new EventConnections();

	// Spacing between bridge sections
	private float space = 1.1f;

	// Bridge with hinge joints and weights is built.
	void Init()
	{
		// Parameters validation
		if (mesh_file.Length <= 0)
			Log.Error("JointCallbacks.Init(): Mesh File parameter is empty!\n");

		if (!broken_materal)
			Log.Error("JointCallbacks.Init(): Broken Matreial parameter is empty!\n");

		if (!joint_materal)
			Log.Error("JointCallbacks.Init(): Joint Matreial parameter is empty!\n");

		// General physics settings
		Physics.FrozenLinearVelocity = 0.1f;
		Physics.FrozenAngularVelocity = 0.1f;
		Physics.NumIterations = 4;

		// Create object from mesh file
		ObjectMeshStatic orig_object = new ObjectMeshStatic(mesh_file);

		// Create heavy weights that will fall and break the bridge
		BodyRigid body = new BodyRigid(orig_object);
		ShapeBox shape = new ShapeBox(body, new vec3(1));
		shape.Density = 80.0f;
		for (int i = 0; i < 3; i++)
		{
			ObjectMeshStatic mesh = orig_object.Clone() as ObjectMeshStatic;
			// Position weights above bridge, spaced horizontally
			mesh.WorldTransform = MathLib.Translate(new Vec3(3.0f * (i - 1), 0.0f, 12.0f));
			objects.Add(mesh);
		}

		// Detach body from original to reuse mesh for bridge sections
		orig_object.Body = null;
		body.DeleteLater();

		// Create bridge: chain of boxes connected by hinge joints
		orig_object.SetMaterial(joint_materal, "*");
		Body b0 = null, b1;
		for (int i = 0; i < bridge_sections; i++)
		{
			ObjectMeshStatic mesh = orig_object.Clone() as ObjectMeshStatic;
			// Center bridge around origin
			float pos = space * (i - (bridge_sections - 1) / 2.0f);
			mesh.WorldTransform = MathLib.Translate(new Vec3(pos, 0.0f, 8.0f));

			// Anchor endpoints with BodyDummy (static), middle sections are dynamic
			if (i == 0 || i == bridge_sections - 1)
				b1 = new BodyDummy(mesh);
			else
				b1 = new BodyRigid(mesh);
			shape = new ShapeBox(b1, new vec3(1));
			objects.Add(mesh);

			// Connect adjacent sections with breakable hinge joint
			if (b0 != null)
			{
				JointHinge joint = new JointHinge(b0, b1, new Vec3(pos - space, 0.0f, 8.0f), new vec3(1.0f, 0.0f, 0.0f));
				joint.AngularDamping = 8.0f;
				joint.NumIterations = 2;
				joint.LinearRestitution = 0.02f;
				joint.AngularRestitution = 0.02f;
				// Force/torque limits that trigger joint breaking
				joint.MaxForce = 1000.0f;
				joint.MaxTorque = 16000.0f;
				// Subscribe to breaking event to change material on broken sections
				joint.EventBroken.Connect(joint_connections, j => {
					joint.Body0.Object.SetMaterial(broken_materal, "*");
					joint.Body1.Object.SetMaterial(broken_materal, "*");
				});
			}

			b0 = b1;
		}

		orig_object.DeleteLater();
	}
	
	// Event connections are cleaned up on shutdown.
	void Shutdown()
	{
		// Remove all connections
		joint_connections.DisconnectAll();
		objects.Clear();
	}
}
