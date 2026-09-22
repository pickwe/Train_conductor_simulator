// Captures the rendered frame and saves it to a DDS file when a button is clicked.
// Uses async texture transfer to avoid blocking the main thread while copying
// GPU texture data to CPU-side image. The screenshot is captured at the end of
// the render frame to ensure all post-processing effects are included.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unigine;

// Captures and saves screenshots of the rendered scene on button press.
public partial class Screenshot : Component
{
	// Flag set by button click, processed in render callback
	private bool grab = false;
	// Reference to main window for size and render events
	private EngineWindow window = null;
	// Connection to end-of-render event for screenshot capture
	private EventConnection renderEndConnection = null;

	// Creates UI button and hooks into render end event for capture.
	private void Init()
	{
		var descWindowCreator = ComponentSystem.FindComponentInWorld<DescriptionWindowCreator>();
		var sampleDescriptionWindow = descWindowCreator.getWindow();
		var parameters = sampleDescriptionWindow.getParameterGroupBox();

		// Show where screenshot will be saved
		var infoLabel = new WidgetLabel("Screenshot will be saved in <b>data/screenshot.dds</b>");
		infoLabel.FontRich = 1;
		parameters.AddChild(infoLabel, Gui.ALIGN_EXPAND);

		// Button triggers screenshot capture on next frame
		var screenshot_button = new WidgetButton("Take Screenshot");
		parameters.AddChild(screenshot_button, Gui.ALIGN_EXPAND);
		screenshot_button.EventClicked.Connect(() => { grab = true; });

		// Hook into end of render to capture fully composited frame
		window = WindowManager.MainWindow;
		renderEndConnection = window.EventFuncEndRender.Connect(OnRender);
	}

	// Disconnects render event on shutdown.
	private void Shutdown()
	{
		renderEndConnection.Disconnect();
	}

	// Captures screen content and saves to file when grab flag is set.
	private void OnRender()
	{
		if (!grab)
		{
			return;
		}

		grab = false;

		// Get temporary texture matching window size
		Texture tmp_texture = Render.GetTemporaryTexture2D(window.ClientSize.x, window.ClientSize.y, Texture.FORMAT_RGBA8, 0);

		// Copy current color buffer to our texture
		tmp_texture.Copy2D();

		// Async transfer avoids blocking while GPU->CPU copy completes
		Render.AsyncTransferTextureToImage(null, (Image image) => {
			// Handle potential Y-flip from graphics API differences
			if (!Render.IsFlipped)
			{
				image.FlipY();
			}

			image.Save("screenshot.dds");
			Log.MessageLine("Screenshot saved to \"data/screenshot.dds\"");
		}, tmp_texture);

		Render.ReleaseTemporaryTexture(tmp_texture);
	}
}
