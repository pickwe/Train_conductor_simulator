// Automatically destroys a node after a specified lifetime. Useful for
// temporary objects like particle effects, projectiles, or debris that
// should be cleaned up after a set duration.

using Unigine;

// Destroys the attached node after lifetime expires.
public partial class LifetimeController : Component
{
	// Duration in seconds before node is destroyed
	public float lifetime = 1.0f;

	// Game time when component was initialized
	private float starttime = 0.0f;

	// Start time is recorded for lifetime tracking.
	private void Init()
	{
		starttime = Game.Time;
	}

	// Node is deleted when lifetime expires.
	private void Update()
	{
		if (Game.Time - starttime > lifetime)
			node.DeleteLater();
	}

	// Node is deleted on component shutdown.
	private void Shutdown()
	{
		node.DeleteLater();
	}
}
