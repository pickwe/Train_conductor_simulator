// Sample setup for shooting gallery body fracture demonstration.
// Enables visualizer and grabs mouse for FPS-style aiming.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Enables visualizer and mouse grab for shooting gallery demo.
public partial class BodyFractureShootingGallerySample : Component
{
	// Saved mouse handle state
	private Input.MOUSE_HANDLE mouse_handle;

	// Visualizer and mouse grab are enabled.
	void Init()
	{
		Visualizer.Enabled = true;

		mouse_handle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
	}
	
	// Visualizer and mouse state are restored on shutdown.
	void Shutdown()
	{
		Visualizer.Enabled = false;
		Input.MouseHandle = mouse_handle;
	}
}
