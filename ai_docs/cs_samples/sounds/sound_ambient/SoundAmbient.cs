// Demonstrates AmbientSource for non-positional background audio.
// Ambient sounds play at constant volume regardless of listener position,
// ideal for music, ambient loops, or UI sounds. Shows switching between
// buffered (preloaded) and streaming (disk-read) playback modes.

using System;
using Unigine;

// Sample for non-positional ambient audio with gain, pitch, and streaming options.
public partial class SoundAmbient : Component
{
	// Path to the audio file to play
	[ParameterFile]
	public string soundFile = "";

	// Engine's ambient sound source - plays without 3D positioning
	private AmbientSource ambientSource = null;
	// Stream mode: false = preload entire file, true = read from disk during playback
	private bool isStream = false;

	private SampleDescriptionWindow window = null;

	// Creates ambient source and UI controls for playback manipulation.
	private void Init()
	{
		ambientSource = new AmbientSource(soundFile);
		if (!ambientSource)
			return;

		ambientSource.Loop = 1;
		ambientSource.Gain = 1.0f;
		ambientSource.Play();

		window = new SampleDescriptionWindow();
		window.createWindow();

		Widget parameters = window.getParameterGroupBox();

		WidgetButton button_play = new WidgetButton("Play");
		WidgetButton button_stop = new WidgetButton("Stop");
		button_play.EventClicked.Connect(() =>
		{
			ambientSource.Play();
		});
		button_stop.EventClicked.Connect(() =>
		{
			ambientSource.Stop();
		});
		WidgetHBox buttons = new WidgetHBox();
		buttons.SetSpace(10, 0);
		buttons.AddChild(button_play);
		buttons.AddChild(button_stop);
		parameters.AddChild(buttons, Gui.ALIGN_LEFT);

		window.addBoolParameter("Loop:", "Loop", Convert.ToBoolean(ambientSource.Loop), (bool active) =>
		{
			ambientSource.Loop = Convert.ToInt32(active);
		});
		window.addBoolParameter("Stream:", "Stream", isStream, (bool active) =>
		{
			ChangeSourceType();
		});

		window.addFloatParameter("Gain:", "Gain", ambientSource.Gain, 0.0f, 1.0f, (float val) =>
		{
			ambientSource.Gain = val;
		});
		window.addFloatParameter("Pitch:", "Pitch", ambientSource.Pitch, 0.1f, 5.0f, (float val) =>
		{
			ambientSource.Pitch = val;
		});
	}

	private void Update()
	{
	}

	// Cleans up ambient source and UI.
	private void Shutdown()
	{
		ambientSource.DeleteLater();
		window.shutdown();
	}

	// Recreates ambient source with opposite streaming mode while preserving settings.
	private void ChangeSourceType()
	{
		// Save current settings before destroying the source
		int isLoop = ambientSource.Loop;
		bool isPlaying = ambientSource.IsPlaying;
		float gain = ambientSource.Gain;
		float pitch = ambientSource.Pitch;

		ambientSource.DeleteLater();
		ambientSource = null;

		// Toggle streaming mode and recreate source
		isStream = !isStream;
		if (isStream)
			ambientSource = new AmbientSource(soundFile, 1);  // 1 = streaming
		else
			ambientSource = new AmbientSource(soundFile, 0);  // 0 = buffered

		// Restore saved settings to the new source
		ambientSource.Loop = isLoop;
		ambientSource.Gain = gain;
		ambientSource.Pitch = pitch;

		if (isPlaying)
			ambientSource.Play();
		else
			ambientSource.Stop();
	}

}
