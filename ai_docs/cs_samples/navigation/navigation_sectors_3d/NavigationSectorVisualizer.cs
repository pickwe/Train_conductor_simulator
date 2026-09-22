// Visualizes a navigation sector for debugging purposes. Renders the
// 3D navigable volume defined by the navigation sector each frame.

using Unigine;

// Renders navigation sector visualization for debugging.
public partial class NavigationSectorVisualizer : Component
{
	// Reference to the navigation sector node
	private NavigationSector navigationSector = null;

	// Navigation sector reference is obtained and visualizer is enabled.
	private void Init()
	{
		navigationSector = node as NavigationSector;

		if (navigationSector)
			Visualizer.Enabled = true;
	}

	// Navigation sector is rendered each frame.
	private void Update()
	{
		if (navigationSector)
			navigationSector.RenderVisualizer();
	}

	// Visualizer is disabled on shutdown.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
	}
}
