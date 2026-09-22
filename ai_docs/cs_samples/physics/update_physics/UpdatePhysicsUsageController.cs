// Demonstrates difference between Update and UpdatePhysics methods. Applies
// force to rigid body using either Update or UpdatePhysics, showing how
// physics timing affects movement consistency at different frame rates.

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

// Compares Update vs UpdatePhysics for physics control.
public partial class UpdatePhysicsUsageController : Component
{
	// Toggle between Update and UpdatePhysics modes
	[ShowInEditor]
	[Parameter(Title = "Use update function")]
	private bool useUdpate = false;

	// Force magnitude applied to body
	[ShowInEditor]
	[Parameter(Title = "Linear force applied to body")]
	private float linearForce = 5.0f;

	// Rigid body reference
	private BodyRigid rigidBody;
	// Current force direction
	private float currentForce = 0.0f;

	// Rigid body reference is obtained.
	void Init()
	{
		rigidBody = node.ObjectBodyRigid;

		if (!rigidBody)
		{
			Log.Error("PhysicsIFpsController.Init() can't find rigid body on the node!\n");
		}
		currentForce = linearForce;
	}

	// NOTE: In the scene, two instances of this component are attached to different cubes.
	// Red cube has useUpdate=true (uses Update), green cube has useUpdate=false (uses UpdatePhysics).
	// This allows side-by-side comparison of both approaches with identical movement code.

	// Update runs every render frame (variable rate depending on FPS).
	// Using physics here causes inconsistent behavior at different frame rates.
	void Update()
	{
		// Visualize velocity vector for comparison
		Visualizer.Enabled = true;
		Visualizer.RenderVector(rigidBody.Position, rigidBody.Position + new Vec3(rigidBody.LinearVelocity), vec4.RED, 0.5f);

		// BAD: Physics in Update causes frame-rate dependent behavior
		if (useUdpate)
		{
			Movement();
		}
	}

	// UpdatePhysics runs at fixed physics timestep (independent of render FPS).
	// This ensures consistent physics behavior regardless of frame rate.
	void UpdatePhysics()
	{
		// GOOD: Physics in UpdatePhysics is frame-rate independent
		if (!useUdpate)
		{
			Movement();
		}
	}

	// Applies force and reverses direction at boundaries.
	private void Movement()
	{
		rigidBody.AddForce(vec3.RIGHT * currentForce);

		if (node.WorldPosition.x > 5)
			currentForce= -linearForce;
		if (node.WorldPosition.x < -5)
			currentForce = linearForce;
	}
}
