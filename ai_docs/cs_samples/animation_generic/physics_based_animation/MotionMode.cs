// Base class for motion algorithms. Stores references to target (laser pointer)
// and persecutor (cat character). Derived classes implement different easing
// behaviors: linear, ease-in, ease-out, spring, bounce.

using Unigine;

// Base class for motion interpolation strategies.
// Each derived class implements a different easing/physics approach:
// - LinearMotion: constant speed movement
// - EaseInMotion: gradual acceleration
// - EaseOutMotion: gradual deceleration
// - EaseOutBounceMotion: bounce effect at destination
// - SpringMotion family: spring physics simulation
public abstract partial class MotionMode : Component
{
	[ShowInEditor]
	public Node targetNode = null;		// The laser pointer to chase
	[ShowInEditor]
	public Node persecutorNode = null;	// The cat that does the chasing

	protected PersecutorBase persecutor = null;

	// Both references are filled from the world, a missing one leaves nothing to move
	protected bool ready = false;

	// References to target and persecutor nodes are validated and stored.
	protected virtual void Init()
	{
		// Validate references to target (laser) and persecutor (cat) nodes.
		// These are set by the CatDemo component during runtime.
		if (targetNode == null)
			Log.Error("MotionMode.Init(): cannot get targetNode property\n");

		// Get the PersecutorBase interface from the persecutor node.
		// This allows different persecutor implementations (skinned mesh, simple mesh, etc.)
		persecutor = ComponentSystem.GetComponent<PersecutorBase>(persecutorNode);
		if (persecutor == null)
			Log.Error("MotionMode.Init(): cannot get Persecutor component from persecutorNode\n");

		ready = targetNode != null && persecutor != null;
	}
}
