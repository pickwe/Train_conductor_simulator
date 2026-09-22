// Spawns nodes at regular time intervals at this component's position.
// Demonstrates time-based spawning using Game.IFps for frame-independent
// timing. Useful for creating projectile emitters, particle spawners,
// or any system that needs periodic instantiation of objects.

using Unigine;

// Timer-based node spawner that creates instances at configurable intervals.
public partial class NodeSpawnerTimer : Component
{
	// Time in seconds between each spawn
	public float spawnRate = 5.0f;
	// Path to the .node file to instantiate
	[ParameterFile(Filter=".node")]
	public string nodeToSpawn = null;
	// Exposes current timer value for visualization purposes
	public float timer { get => timeBufferSec; }
	// Accumulates elapsed time until reaching spawnRate threshold
	private float timeBufferSec = 0.1f;

	// Accumulates time and spawns a new node when interval is reached.
	void Update()
	{
		timeBufferSec += Game.IFps;
		if (timeBufferSec >= spawnRate)
		{
			// Subtract instead of reset to handle frame time overflow correctly
			timeBufferSec -= spawnRate;
			// Load and position new node at spawner's location
			Node spawnedNode = World.LoadNode(nodeToSpawn);
			spawnedNode.WorldTransform = node.WorldTransform;
		}
	}
}
