// Demonstrates 2D pathfinding using navigation mesh. Calculates a route
// between start and finish points on a flat navigation surface and
// optionally visualizes the computed path.

using Unigine;

// Calculates 2D path between two points on navigation mesh.
public partial class PathRoute2D : Component
{
	// Starting point node for pathfinding
	public Node startPoint = null;
	// Destination point node for pathfinding
	public Node finishPoint = null;

	// Whether to render the calculated path
	public bool visualizeRoute = false;

	// Color for route visualization
	[ParameterColor]
	public vec4 routeColor = vec4.ZERO;

	// PathRoute object for path calculation
	private PathRoute route = null;

	// Route is created and visualizer is enabled.
	private void Init()
	{
		if (startPoint && finishPoint)
		{
			// Create route for path calculation
			route = new PathRoute();

			// Set point radius inside navigation mesh
			route.Radius = 0.5f;

			// Enable visualizer for route rendering
			Visualizer.Enabled = true;
		}
	}
	
	// Path is recalculated and visualized each frame.
	private void Update()
	{
		if (startPoint && finishPoint)
		{
			// Calculate 2D path from start to finish
			route.Create2D(startPoint.WorldPosition, finishPoint.WorldPosition);
			if (route.IsReached)
			{
				// Render path if visualization is enabled
				if (visualizeRoute)
					route.RenderVisualizer(routeColor);
			}
			else
				Log.Message($"{node.Name} PathRoute not reached yet\n");
		}
	}

	// Visualizer is disabled on shutdown.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
	}
}
