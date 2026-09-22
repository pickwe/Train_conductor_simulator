// Demo controller for ObjectFrame bounding box visualization system.
// Provides a screenshot button that captures the current view along with
// JSON metadata about all visible ObjectFrame components (position, rotation,
// screen coordinates). Useful for generating training data for ML/CV systems.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Emit;
using Unigine;
using static System.Net.Mime.MediaTypeNames;

// UI controller for capturing screenshots with ObjectFrame metadata.
public partial class ObjectFrameSample : Component
{
	SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	EngineWindow window;
	// Label showing save status messages
	WidgetLabel label;
	// All ObjectFrame components in the scene
	ObjectFrame[] frames;
	// Flag to trigger screenshot capture at end of render
	bool grabFlag = false;
	// Timestamp format for unique filenames
	const string timeFormat = "yyyy-MM-dd HH_mm_ss";

	// Creates UI and hooks into render end event for screenshot capture.
	void Init()
	{
		sampleDescriptionWindow.createWindow();

		WidgetGroupBox paramBox = sampleDescriptionWindow.getParameterGroupBox();
		paramBox.Text = "Controls";
		WidgetButton button = new WidgetButton("Snap Screenshot");
		button.EventClicked.Connect(() =>
		{
			// Hide frames before capture to get clean image
			ObjectFrame.setObjectFramesEnabled(false);
			grabFlag = true;
		});
		paramBox.AddChild(button, Gui.ALIGN_EXPAND);

		label = new WidgetLabel("");
		paramBox.AddChild(label, Gui.ALIGN_LEFT);

		// Subscribe to end render event for screenshot timing
		window = WindowManager.MainWindow;
		if (window)
		{
			window.EventFuncEndRender.Connect(snapScreenshot);
		}

		// Find all ObjectFrame components for metadata export
		frames = ComponentSystem.FindComponentsInWorld<ObjectFrame>();
	}

	// Cleans up UI resources.
	void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
	}

	// Captures screenshot and saves JSON metadata for visible objects.
	void snapScreenshot()
	{
		if (!grabFlag)
			return;
		grabFlag = false;

		Texture temporaryTexture = Render.GetTemporaryTexture2D(window.ClientRenderSize.x,
			window.ClientRenderSize.y, Texture.FORMAT_RGBA8);
		temporaryTexture.Copy2D();
		ObjectFrame.setObjectFramesEnabled(true);

		string timestamp = DateTime.Now.ToString(timeFormat, CultureInfo.InvariantCulture);

		Json json = new Json();
		Json array = json.AddChild("entities");
		array.SetArray();
		foreach (ObjectFrame frame in frames)
		{
			if (!frame.isVisible() || !frame.Enabled)
				continue;
			Json info = frame.getJsonMeta();
			array.AddChild(info);
		}

		var worldDir = System.IO.Path.GetDirectoryName(World.Path);
		var frameDataDir = System.IO.Path.Combine(worldDir, "frame_data" + "/" + timestamp + ".json");
		json.Save(frameDataDir);

		Render.AsyncTransferTextureToImage(null, ((Unigine.Image image) =>
		{
			if (!Render.IsFlipped)
				image.FlipY();

			Log.Message($"Saving \"{timestamp}.png\"\n");
			var screenshotsDir = System.IO.Path.Combine(worldDir, "screenshots" + "/" + timestamp + ".png");
			image.Save(screenshotsDir);

			string msg = string.Format("Saved screenshot \"{0}.png\"", timestamp);
			if (label)
				label.Text = msg;
		}),
		temporaryTexture);
		Render.ReleaseTemporaryTexture(temporaryTexture);
	}
}
