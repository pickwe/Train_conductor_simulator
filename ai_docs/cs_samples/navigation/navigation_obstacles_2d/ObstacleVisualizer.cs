// Visualizes navigation obstacles for debugging purposes. Renders the
// obstacle boundaries that block pathfinding routes each frame.

using Unigine;

// Renders obstacle visualization for debugging.
public partial class ObstacleVisualizer : Component
{
	// Reference to the obstacle node
	private Obstacle obstacle = null;

	// Obstacle reference is obtained and visualizer is enabled.
	private void Init()
	{
		obstacle = node as Obstacle;

		if (obstacle)
			Visualizer.Enabled = true;
	}

	// Obstacle is rendered each frame.
	private void Update()
	{
		if (obstacle)
			obstacle.RenderVisualizer();
	}

	// Visualizer is disabled on shutdown.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
	}
}
