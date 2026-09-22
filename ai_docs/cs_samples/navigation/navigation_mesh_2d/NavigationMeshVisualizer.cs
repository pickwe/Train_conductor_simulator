// Visualizes a navigation mesh for debugging purposes. Renders the
// walkable surface areas defined by the navigation mesh each frame.

using Unigine;

// Renders navigation mesh visualization for debugging.
public partial class NavigationMeshVisualizer : Component
{
	// Reference to the navigation mesh node
	private NavigationMesh navigationMesh = null;

	// Navigation mesh reference is obtained and visualizer is enabled.
	private void Init()
	{
		navigationMesh = node as NavigationMesh;

		if (navigationMesh)
			Visualizer.Enabled = true;
	}

	// Navigation mesh is rendered each frame.
	private void Update()
	{
		if (navigationMesh)
			navigationMesh.RenderVisualizer();
	}

	// Visualizer is disabled on shutdown.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
	}
}
