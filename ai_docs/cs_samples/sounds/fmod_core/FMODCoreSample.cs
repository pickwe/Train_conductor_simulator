// Demonstrates FMOD Core API integration for audio playback in UNIGINE.
// Shows both 2D (non-positional) and 3D (spatialized) sound playback,
// along with DSP effects like distortion and timeline control.
// FMOD Core provides low-level audio control compared to FMOD Studio.

using System;
using Unigine;
using Unigine.Plugins.FMOD;

// Sample showcasing FMOD Core API features: 2D/3D sounds, DSP effects, and playback control.
public partial class FMODCoreSample : Component
{
	// Tracks whether FMOD plugin loaded successfully
	private bool pluginInitialized = false;

	// Visual marker for 3D sound position in the world
	private ObjectMeshDynamic carSphere = null;

	// Sound objects - one for 2D playback, one for 3D spatialized audio
	private Unigine.Plugins.FMOD.Sound musicSound = null;
	private Unigine.Plugins.FMOD.Sound musicSound3D = null;
	// Channels control playback state (play, pause, volume, position)
	private Channel musicChannel = null;
	private Channel musicChannel3D = null;

	// UI elements for audio control
	private SampleDescriptionWindow sampleDescriptionWindow = null;
	private WidgetSlider musicPositionSlider;
	private WidgetSlider distortionSlider;
	private WidgetSlider volumeSlider;

	// Loads FMOD plugin, creates sounds, and sets up UI controls.
	void Init()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow(Gui.ALIGN_LEFT, 500);

		// Dynamically load FMOD plugin if not already loaded
		if (Engine.FindPlugin("UnigineFMOD") == -1)
			Engine.AddPlugin("UnigineFMOD");
		if (!FMOD.CheckPlugin())
		{
			WidgetGroupBox parametersGroupbox = sampleDescriptionWindow.getParameterGroupBox();

			var infoLabel = new WidgetLabel();
			infoLabel.FontWrap = 1;
			infoLabel.Text = "Cannot find FMOD plugin. Please check UnigineFMOD and fmod.dll, fmodL.dll, fmodstudio.dll, fmodstudioL.dll (You can download these files from official site) in bin directory.";

			parametersGroupbox.AddChild(infoLabel);

			return;
		}

		pluginInitialized = true;

		// Initialize FMOD Core with 1024 channels in NORMAL mode
		FMODCore core = FMOD.Core;
		core.InitCore(1024, FMODEnums.INIT_FLAGS.NORMAL);

		// Load a 2D sound file
		musicSound = core.CreateSound(
			FileSystem.GetAbsolutePath(FileSystem.ResolvePartialVirtualPath("fmod_core/sounds/soundtrack.oga")),
			FMODEnums.FMOD_MODE._2D);

		// Load the same file as a 3D sound
		musicSound3D = core.CreateSound(
			FileSystem.GetAbsolutePath(FileSystem.ResolvePartialVirtualPath("fmod_core/sounds/soundtrack.oga")),
			FMODEnums.FMOD_MODE._3D);

		// Create a sphere to visualize the 3D sound source
		carSphere = Primitives.CreateSphere(1.0f);
		carSphere.SetMaterialParameterFloat4("albedo_color", new vec4(0.4f, 0.0f, 0.0f, 1.0f), 0);

		Visualizer.Enabled = true;
		InitDescriptionWindow();
	}
	
	// Updates playback progress slider and handles track looping.
	void Update()
	{
		if (!pluginInitialized)
			return;

		// Display label above 3D sound source position
		var len = musicSound.GetLength(FMODEnums.TIME_UNIT.MS);
		Visualizer.RenderMessage3D(carSphere.WorldPosition, vec3.ZERO, "Music 3D", vec4.WHITE, 0, 25);

		// Sync slider with current playback position
		uint pos;
		if (musicChannel)
		{
			musicChannel.GetPositionTimeLine(out pos, FMODEnums.TIME_UNIT.MS);
			int progress = Convert.ToInt32(pos / Convert.ToSingle(len) * 100);
			if (progress >= 100)
			{
				musicChannel.SetPositionTimeLine(0, FMODEnums.TIME_UNIT.MS);
				StopMusic();
				progress = 0;
			}
			musicPositionSlider.Value = progress;
		}
	}

	// Releases all FMOD resources and unloads the plugin.
	void Shutdown()
	{
		// Release sound objects (source audio data)
		if (musicSound)
		{
			musicSound.Release();
			musicSound = null;
		}

		if (musicSound3D)
		{
			musicSound3D.Release();
			musicSound3D = null;
		}

		if (musicChannel)
		{
			musicChannel.Release();
			musicChannel = null;
		}

		if (musicChannel3D)
		{
			musicChannel3D.Release();
			musicChannel3D = null;
		}
		if (pluginInitialized)
		{
			carSphere.DeleteLater();
		}

		Unigine.Console.Run("plugin_unload UnigineFMOD");
		pluginInitialized = false;

		Visualizer.Enabled = false;
		sampleDescriptionWindow.shutdown();
	}

	// Creates tabbed UI with playback controls, effects, and 3D sound controls.
	void InitDescriptionWindow()
	{
		// Tabbed interface separates 2D and 3D sound controls
		WidgetGroupBox parametersGroupbox = sampleDescriptionWindow.getParameterGroupBox();
		WidgetTabBox tab = new WidgetTabBox(4, 4);
		parametersGroupbox.AddChild(tab, Gui.ALIGN_EXPAND);

		// Tab music
		{
			tab.AddTab("Music");

			// Playback controls
			var playButton = new WidgetButton("Play");
			var stopButton = new WidgetButton("Stop");
			var pauseButton = new WidgetButton("Pause/Resume");
			var plusButton = new WidgetButton("+ 10 sec");
			var minusButton = new WidgetButton("- 10 sec");

			var hbox = new WidgetHBox();
			playButton.EventClicked.Connect(PlayMusic);
			stopButton.EventClicked.Connect(StopMusic);
			pauseButton.EventClicked.Connect(TogglePauseMusic);

			minusButton.EventClicked.Connect(MinusMS);
			plusButton.EventClicked.Connect(PlusMS);

			musicPositionSlider = new WidgetSlider();

			distortionSlider = new WidgetSlider();
			distortionSlider.EventChanged.Connect(DistortionChanged);

			volumeSlider = new WidgetSlider();
			volumeSlider.EventChanged.Connect(VolumeChanged);
			volumeSlider.Value = 100;
			hbox.AddChild(minusButton);
			hbox.AddChild(plusButton);

			tab.AddChild(new WidgetLabel("Time Line"), Gui.ALIGN_EXPAND);
			tab.AddChild(musicPositionSlider, Gui.ALIGN_EXPAND);


			tab.AddChild(hbox, Gui.ALIGN_EXPAND);
			tab.AddChild(playButton, Gui.ALIGN_EXPAND);
			tab.AddChild(stopButton, Gui.ALIGN_EXPAND);
			tab.AddChild(pauseButton, Gui.ALIGN_EXPAND);

			tab.AddChild(new WidgetLabel("Distortion Mix"), Gui.ALIGN_EXPAND);
			tab.AddChild(distortionSlider, Gui.ALIGN_EXPAND);

			tab.AddChild(new WidgetLabel("Volume"), Gui.ALIGN_EXPAND);
			tab.AddChild(volumeSlider, Gui.ALIGN_EXPAND);
		}

		// Tab music 3D
		{
			tab.AddTab("Music 3D");
			var playButton = new WidgetButton("Play");
			var stopButton = new WidgetButton("Stop");
			var pauseButton = new WidgetButton("Pause/Resume");

			var hbox = new WidgetHBox();
			playButton.EventClicked.Connect(PlayMusic3D);
			stopButton.EventClicked.Connect(StopMusic3D);
			pauseButton.EventClicked.Connect(TogglePauseMusic3D);

			tab.AddChild(playButton, Gui.ALIGN_EXPAND);
			tab.AddChild(stopButton, Gui.ALIGN_EXPAND);
			tab.AddChild(pauseButton, Gui.ALIGN_EXPAND);
		}

		parametersGroupbox.Arrange();
	}

	// Applies distortion DSP effect based on slider value.
	void DistortionChanged()
	{
		if (!musicChannel)
		{
			return;
		}

		// DSP at index 0 is our distortion effect - parameter 0 controls wet/dry mix
		musicChannel.GetDSP(0).SetParameterFloat(0, distortionSlider.Value * 0.01f);
	}

	// Adjusts channel volume from slider (0-100 mapped to 0.0-1.0).
	void VolumeChanged()
	{
		if (!musicChannel)
		{
			return;
		}

		// Adjust volume on the music channel
		musicChannel.Volume = volumeSlider.Value * 0.01f;
	}

	// Seeks forward 10 seconds in the audio timeline, wrapping if at end.
	void PlusMS()
	{
		if (!musicChannel)
		{
			return;
		}

		// Get current position and add 10 seconds (10000ms)
		uint currTimeLine;
		uint len = musicSound.GetLength(FMODEnums.TIME_UNIT.MS);
		musicChannel.GetPositionTimeLine(out currTimeLine, FMODEnums.TIME_UNIT.MS);
		if (currTimeLine + 10000 >= len)
		{
			musicChannel.SetPositionTimeLine(0, FMODEnums.TIME_UNIT.MS);
		}
		else
		{
			musicChannel.SetPositionTimeLine(currTimeLine + 10000, FMODEnums.TIME_UNIT.MS);
		}
	}

	// Seeks backward 10 seconds in the audio timeline, clamping at start.
	void MinusMS()
	{
		if (!musicChannel)
		{
			return;
		}

		uint currTimeLine;
		musicChannel.GetPositionTimeLine(out currTimeLine, FMODEnums.TIME_UNIT.MS);
		if (currTimeLine < 10000)
		{
			musicChannel.SetPositionTimeLine(0, FMODEnums.TIME_UNIT.MS);
		}
		else
		{
			musicChannel.SetPositionTimeLine(currTimeLine - 10000, FMODEnums.TIME_UNIT.MS);
		}
	}

	// Starts 2D music playback with distortion effect applied.
	void PlayMusic()
	{
		// Only play 2D if 3D isn't active (they share the same source)
		if (!musicChannel3D || !musicChannel3D.IsPlaying)
		{
			StopMusic();

			// Play returns a channel for controlling this playback instance
			musicChannel = musicSound.Play();

			if (!musicChannel)
			{
				return;
			}

			// Add distortion DSP and apply current slider value
			musicChannel.AddDSP(0, DSPType.TYPE.DISTORTION).SetParameterFloat(0, distortionSlider.Value * 0.01f);
			musicChannel.Volume = volumeSlider.Value * 0.01f;
		}
	}

	// Stops 2D music and releases the channel.
	void StopMusic()
	{
		if (!musicChannel)
		{
			return;
		}

		musicChannel.Stop();
		musicChannel = null;
	}

	// Toggles pause state for 2D music channel.
	void TogglePauseMusic()
	{
		if (!musicChannel)
		{
			return;
		}

		musicChannel.Paused = !musicChannel.Paused;
	}

	// Starts 3D spatialized music at the car sphere's world position.
	void PlayMusic3D()
	{
		// Only play 3D if 2D isn't active
		if (!musicChannel || !musicChannel.IsPlaying)
		{
			StopMusic3D();

			// 3D sound position determines volume and panning based on listener
			musicChannel3D = musicSound3D.Play();

			if (!musicChannel3D)
			{
				return;
			}

			musicChannel3D.SetPosition(carSphere.WorldPosition);
		}
	}

	// Stops 3D music and releases the channel.
	void StopMusic3D()
	{
		if (!musicChannel3D)
		{
			return;
		}

		musicChannel3D.Stop();
		musicChannel3D = null;
	}

	// Toggles pause state for 3D music channel.
	void TogglePauseMusic3D()
	{
		if (!musicChannel3D)
		{
			return;
		}

		musicChannel3D.Paused = !musicChannel3D.Paused;
	}
}
