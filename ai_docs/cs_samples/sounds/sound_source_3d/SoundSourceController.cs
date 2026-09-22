// Demonstrates SoundSource for 3D positional audio in UNIGINE.
// Unlike AmbientSource, SoundSource has a position in the world and its
// volume/panning depends on listener distance and direction. Uses min/max
// distance to control attenuation falloff from full volume to silence.

using System;
using Unigine;

// Sample for 3D positional sound with distance attenuation and streaming options.
public partial class SoundSourceController : Component
{
	// Path to the audio file to play
	[ParameterFile]
	public string soundFile = "";

	// 3D positioned sound source with distance-based attenuation
	private SoundSource sound = null;

	SampleDescriptionWindow window = null;

	// Creates 3D sound source with distance attenuation and UI controls.
	private void Init()
	{
		sound = new SoundSource(soundFile);
		if (!sound)
			return;

		// Distance attenuation: full volume within 5 units, silent at 50 units
		sound.MinDistance = 5.0f;
		sound.MaxDistance = 50.0f;
		sound.Loop = 1;
		sound.Play();

		Visualizer.Enabled = true;

		window = new SampleDescriptionWindow();
		window.createWindow();

		Widget parameters = window.getParameterGroupBox();

		WidgetButton button_play = new WidgetButton("Play");
		WidgetButton button_stop = new WidgetButton("Stop");
		button_play.EventClicked.Connect(() =>
		{
			sound.Play();
		});
		button_stop.EventClicked.Connect(() =>
		{
			sound.Stop();
		});
		WidgetHBox buttons = new WidgetHBox();
		buttons.SetSpace(10, 0);
		buttons.AddChild(button_play);
		buttons.AddChild(button_stop);
		parameters.AddChild(buttons, Gui.ALIGN_LEFT);

		window.addBoolParameter("Loop:", "Loop", Convert.ToBoolean(sound.Loop), (bool active) =>
		{
			sound.Loop = Convert.ToInt32(active);
		});
		window.addBoolParameter("Stream:", "Stream", sound.Stream, (bool active) =>
		{
			sound.Stream = active;
		});

		window.addFloatParameter("Gain:", "Gain", sound.Gain, 0.0f, 1.0f, (float val) =>
		{
			sound.Gain = val;
		});
		window.addFloatParameter("Pitch:", "Pitch", sound.Pitch, 0.1f, 5.0f, (float val) =>
		{
			sound.Pitch = val;
		});
	}

	// Renders the sound source's range visualization each frame.
	private void Update()
	{
		// Visualizer shows min (full volume) and max (silent) distance spheres
		sound.RenderVisualizer();
	}

	// Cleans up visualization and UI.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
		window.shutdown();
	}

}
