// Sample setup for falling spheres body fracture demonstration.
// Enables the visualizer for physics debug rendering.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Enables visualizer for body fracture falling spheres demo.
public partial class BodyFractureFallingSpheresSample : Component
{
	// Visualizer is enabled for physics debugging.
	void Init()
	{
		Visualizer.Enabled = true;
	}

	// Visualizer is disabled on shutdown.
	void Shutdown()
	{
		Visualizer.Enabled = false;
	}
}
