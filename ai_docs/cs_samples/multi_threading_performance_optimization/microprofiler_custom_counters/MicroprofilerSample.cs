// Demonstrates microprofiler integration. Displays the microprofiler URL
// for remote profiling access and configures background update mode
// to keep rendering when the window loses focus.

using Unigine;

// Shows microprofiler URL and configures background rendering.
public partial class MicroprofilerSample : Component
{
	// Saved background update mode to restore on shutdown
	private Engine.BACKGROUND_UPDATE previous_bg_update = Engine.BACKGROUND_UPDATE.BACKGROUND_UPDATE_DISABLED;
	// Sample description window for UI
	private SampleDescriptionWindow sampleDescriptionWindow = new();

	// Microprofiler availability is checked and URL is displayed.
	void Init()
	{	
		string description;

		// Check if microprofiler is available (requires development binaries)
		if (Profiler.MicroprofileUrl == "")
		{
			WindowManager.DialogError("Warning", "Microprofiler is not available!");
			description = "<font color=\"#de4a14\"><p>Microprofiler is not compiled.</p><p>Use development-release binaries.</p></font>";
		}
		else
		{
			// Display URL for remote profiler access
			description = "<p>Microprofiler url <font color=\"#de4a14\">" + Profiler.MicroprofileUrl + "</font></p>";
		}

		// Create UI with rich text label
		sampleDescriptionWindow.createWindow();

		WidgetLabel label = new(description)
		{
			FontRich = 1,
			FontWrap = 1,
			Width = 300
		};
		sampleDescriptionWindow.getParameterGroupBox().AddChild(label, Gui.ALIGN_LEFT);

		// Enable background rendering to keep profiler active when unfocused
		previous_bg_update = Engine.BackgroundUpdate;
		Engine.BackgroundUpdate = Engine.BACKGROUND_UPDATE.BACKGROUND_UPDATE_RENDER_NON_MINIMIZED;		
	}

	// Background update mode is restored and UI is cleaned up on shutdown.
	void Shutdown()
	{
		Engine.BackgroundUpdate = previous_bg_update;
		sampleDescriptionWindow.shutdown();
	}
}
