// Provides UI for adjusting render FPS to demonstrate physics timing.
// Allows changing max render FPS to show Update vs UpdatePhysics differences.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unigine;

// UI to adjust max FPS for physics timing demonstration.
public partial class UpdatePhysicsUsageSample : Component
{
	// Sample UI window
	private SampleDescriptionWindow window = null;
	// FPS slider control
	private WidgetSlider maxFpsSlider = null;

	// FPS control slider is created.
	private void Init()
	{
		window = new SampleDescriptionWindow();
		window.createWindow();
		maxFpsSlider = window.addIntParameter("Max render fps:", "Max render fps:", Render.MaxFPS, 15, 150, (int value) =>
		{
			Render.MaxFPS = value;
		});
	}

	// UI is cleaned up on shutdown.
	private void Shutdown()
	{
		window.shutdown();
	}
}
