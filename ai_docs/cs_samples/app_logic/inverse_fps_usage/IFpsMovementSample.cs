// Provides UI control for adjusting maximum frame rate. Used together with
// IFpsMovementController to demonstrate the difference between frame-rate
// dependent and independent movement at various FPS limits.

using Unigine;

// Creates UI slider for controlling maximum render FPS.
public partial class IFpsMovementSample : Component
{
	// UI window reference
	private SampleDescriptionWindow window = null;
	// FPS control slider
	private WidgetSlider maxFpsSlider = null;

	// UI window with FPS slider is created.
	private void Init()
	{
		window = new SampleDescriptionWindow();
		window.createWindow();
		WidgetGroupBox parameters = window.getParameterGroupBox();
		// Add slider to control maximum render frame rate
		maxFpsSlider = window.addIntParameter("Max render fps:", "Max render fps:", Render.MaxFPS, 15, 150, (int value) =>
		{
			Render.MaxFPS = value;
		});
	}

	// UI window is released on shutdown.
	private void Shutdown()
	{
		window.shutdown();
	}
}
