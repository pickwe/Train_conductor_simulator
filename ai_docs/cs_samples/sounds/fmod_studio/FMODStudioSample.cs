// Demonstrates FMOD Studio integration for event-based audio in UNIGINE.
// FMOD Studio provides higher-level audio authoring with banks, events,
// parameters, and VCAs (Volume Control Amplifiers). Shows ambient sounds,
// engine simulation with RPM parameters, and Doppler effect demonstration.

using Unigine;
using Unigine.Plugins.FMOD;
using System;


#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
#endif

// Sample showcasing FMOD Studio: events, parameters, banks, VCAs, and Doppler effect.
public partial class FMODStudioSample : Component
{
	// Tracks whether FMOD plugin loaded successfully
	private bool pluginInitialized = false;

	// Timer for Doppler object reset cycle
	private float timer = 0.0f;

	// Visual markers: static car and moving Doppler source
	private ObjectMeshDynamic carSphere;
	private ObjectMeshDynamic dopplerSphere;

	// FMOD Studio event instances - high-level sound objects with parameters
	private EventInstance engineEvent = null;
	private EventInstance dopplerEngineEvent = null;
	private EventInstance forestEvent = null;
	// VCA controls volume for groups of events (like a mixer bus)
	private VCA envVCA = null;

	// Doppler object movement parameters
	private Vec3 velocity;
	private Vec3 startPoint = new Vec3(-5, 80, 0);

	// UI elements for controlling event parameters
	private SampleDescriptionWindow sampleDescriptionWindow = null;
	private WidgetSlider engineSlider;
	private WidgetSlider windForestSlider;
	private WidgetSlider rainForestSlider;
	private WidgetSlider coverForestSlider;
	private WidgetSlider envVCASlider;
	private WidgetSlider dopplerRPMSlider;
	private WidgetSlider dopplerVelocitySlider;
	private WidgetCheckBox showDopplerBoxCheckBox;

	// Loads FMOD plugin, sound banks, creates events, and sets up UI.
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

		// Create two spheres: one static (car) and one moving (doppler)
		carSphere = Primitives.CreateSphere(2.0f);
		carSphere.SetMaterialParameterFloat4("albedo_color", new vec4(0.4f, 0.0f, 0.0f, 1.0f), 0);

		dopplerSphere = Primitives.CreateSphere(1.0f);
		dopplerSphere.SetMaterialParameterFloat4("albedo_color", new vec4(0.0f, 4.0f, 0.0f, 1.0f), 0);
		dopplerSphere.WorldPosition = startPoint;

		// Initialize FMOD Studio and load sound banks
		FMODStudio studio = FMOD.Studio;
		studio.UseStudioLiveUpdateFlag();
		studio.InitStudio();
		studio.LoadBank(FileSystem.GetAbsolutePath(FileSystem.ResolvePartialVirtualPath("fmod_studio/fmod_banks/Master.bank")));
		studio.LoadBank(FileSystem.GetAbsolutePath(FileSystem.ResolvePartialVirtualPath("fmod_studio/fmod_banks/Master.strings.bank")));
		studio.LoadBank(FileSystem.GetAbsolutePath(FileSystem.ResolvePartialVirtualPath("fmod_studio/fmod_banks/Vehicles.bank")));
		studio.LoadBank(FileSystem.GetAbsolutePath(FileSystem.ResolvePartialVirtualPath("fmod_studio/fmod_banks/SFX.bank")));

		// Play ambient forest and engine events
		forestEvent = studio.GetEvent("event:/Ambience/Forest");
		forestEvent.Play();

		engineEvent = studio.GetEvent("event:/Vehicles/Car Engine");
		engineEvent.Play();

		// Doppler effect setup
		dopplerEngineEvent = studio.GetEvent("event:/Vehicles/Car Engine");
		dopplerEngineEvent.SetParent(dopplerSphere);
		dopplerEngineEvent.SetParameter("RPM", 4000);

		// Initial movement direction
		velocity = -Vec3.FORWARD;

		// Load VCA for master environmental volume
		envVCA = studio.GetVCA("vca:/Environment");

		Visualizer.Enabled = true;
		InitDescriptionWindow();
	}
	
	// Moves Doppler object and updates visualizer labels each frame.
	void Update()
	{
		if (!pluginInitialized)
			return;

		float t = Game.Time;
		float dt = Game.IFps;

		// Doppler effect demo: object flies past listener, pitch shifts
		if (showDopplerBoxCheckBox.Checked)
		{
			dopplerSphere.Enabled = true;

			// Reset position after 2.5 seconds
			if (timer >= 2.5f)
			{
				dopplerSphere.WorldPosition = startPoint;
				timer = 0.0f;
			}
			timer += dt;

			// Restart Doppler sound if not playing
			if (!dopplerEngineEvent.IsPlaying && !dopplerEngineEvent.IsStarting)
			{
				dopplerEngineEvent.Play();
			}

			// Move the Doppler object and update sound velocity
			dopplerSphere.WorldPosition = dopplerSphere.WorldPosition + velocity;
			Visualizer.RenderMessage3D(dopplerSphere.WorldPosition, vec3.ZERO, "Doppler", vec4.WHITE, 0, 20);
		}
		else
		{
			dopplerEngineEvent.Stop();
			dopplerSphere.Enabled = false;
		}

		// Show label above the car
		Visualizer.RenderMessage3D(carSphere.WorldPosition, vec3.ZERO, "Car", vec4.WHITE, 0, 20);
	}

	// Releases all FMOD Studio events, VCAs, and unloads the plugin.
	void Shutdown()
	{
		// Release event instances (stops playback and frees resources)
		if (engineEvent)
		{
			engineEvent.Release();
			engineEvent = null;
		}

		if (dopplerEngineEvent)
		{
			dopplerEngineEvent.Release();
			dopplerEngineEvent = null;
		}

		if (forestEvent)
		{
			forestEvent.Release();
			forestEvent = null;
		}

		if (envVCA)
		{
			envVCA.Release();
			envVCA = null;
		}

		if (pluginInitialized)
		{
			carSphere.DeleteLater();
			dopplerSphere.DeleteLater();
		}

		Unigine.Console.Run("plugin_unload UnigineFMOD");
		pluginInitialized = false;

		Visualizer.Enabled = false;
		sampleDescriptionWindow.shutdown();
	}

	// Creates tabbed UI for ambience, engine, Doppler, and VCA controls.
	void InitDescriptionWindow()
	{
		// Tabs organize different FMOD Studio features
		WidgetGroupBox parametersGroupbox = sampleDescriptionWindow.getParameterGroupBox();
		WidgetTabBox tab = new WidgetTabBox(4, 4);
		parametersGroupbox.AddChild(tab, Gui.ALIGN_EXPAND);

		// Ambience tab - forest event with wind, rain, and cover parameters
		{
			tab.AddTab("Ambience");
			windForestSlider = new WidgetSlider();
			var wind_label = new WidgetLabel("Wind");
			tab.AddChild(wind_label, Gui.ALIGN_EXPAND);
			tab.AddChild(windForestSlider, Gui.ALIGN_EXPAND);
			windForestSlider.EventChanged.Connect(WindForestSliderChanged);

			rainForestSlider = new WidgetSlider();
			var forest_label = new WidgetLabel("Rain");
			tab.AddChild(forest_label, Gui.ALIGN_EXPAND);
			tab.AddChild(rainForestSlider, Gui.ALIGN_EXPAND);
			rainForestSlider.EventChanged.Connect(RainForestSliderChanged);

			coverForestSlider = new WidgetSlider();
			var cover_label = new WidgetLabel("Cover");
			tab.AddChild(cover_label, Gui.ALIGN_EXPAND);
			tab.AddChild(coverForestSlider, Gui.ALIGN_EXPAND);
			coverForestSlider.EventChanged.Connect(CoverForestSliderChanged);
		}

		// Engine tab - car engine with RPM parameter affecting sound
		{
			tab.AddTab("Engine");
			engineSlider = new WidgetSlider();
			engineSlider.MinValue = 0;
			engineSlider.MaxValue = 8000;
			var label = new WidgetLabel("RPM");
			tab.AddChild(label, Gui.ALIGN_EXPAND);
			tab.AddChild(engineSlider, Gui.ALIGN_EXPAND);
			engineSlider.EventChanged.Connect(EngineSliderChanged);
		}

		// Doppler tab - moving sound source demonstrates pitch shift effect
		{
			tab.AddTab("Doppler");
			showDopplerBoxCheckBox = new WidgetCheckBox();
			showDopplerBoxCheckBox.Checked = false;
			var label = new WidgetLabel("Show Doppler Effect");
			tab.AddChild(label, Gui.ALIGN_EXPAND);
			tab.AddChild(showDopplerBoxCheckBox, Gui.ALIGN_EXPAND);

			dopplerRPMSlider = new WidgetSlider();
			dopplerRPMSlider.MaxValue = 8000;
			dopplerRPMSlider.Value = 4000;
			tab.AddChild(new WidgetLabel("RPM"), Gui.ALIGN_EXPAND);
			tab.AddChild(dopplerRPMSlider, Gui.ALIGN_EXPAND);

			dopplerVelocitySlider = new WidgetSlider();
			tab.AddChild(new WidgetLabel("Velocity"), Gui.ALIGN_EXPAND);
			tab.AddChild(dopplerVelocitySlider, Gui.ALIGN_EXPAND);

			dopplerRPMSlider.EventChanged.Connect(DopplerRPMSliderChanged);
			dopplerVelocitySlider.EventChanged.Connect(DopplerVelocitySliderChanged);
			dopplerVelocitySlider.Value = 5;
		}

		// VCA tab - master volume control for environment sounds group
		{
			tab.AddTab("VCA");
			envVCASlider = new WidgetSlider();
			envVCASlider.Value = 100;
			var label = new WidgetLabel("Sounds Volume");
			tab.AddChild(label, Gui.ALIGN_EXPAND);
			tab.AddChild(envVCASlider, Gui.ALIGN_EXPAND);
			envVCASlider.EventChanged.Connect(EnvVCASliderChanged);
		}

		parametersGroupbox.Arrange();
	}

	// Sets master volume for all environment sounds via VCA.
	void EnvVCASliderChanged()
	{
		envVCA.Volume = envVCASlider.Value * 0.01f;
	}

	// Adjusts engine RPM parameter - affects playback based on FMOD Studio design.
	void EngineSliderChanged()
	{
		engineEvent.SetParameter("RPM", Convert.ToSingle(engineSlider.Value));
	}

	// Controls wind layer intensity in the forest ambience event.
	void WindForestSliderChanged()
	{
		forestEvent.SetParameter("Wind", windForestSlider.Value * 0.01f);
	}

	// Controls rain layer intensity in the forest ambience event.
	void RainForestSliderChanged()
	{
		forestEvent.SetParameter("Rain", rainForestSlider.Value * 0.01f);
	}

	// Controls cover parameter - affects muffling/indoor effect in forest ambience.
	void CoverForestSliderChanged()
	{
		forestEvent.SetParameter("Cover", coverForestSlider.Value * 0.01f);
	}

	// Sets RPM for the Doppler sound source (affects engine pitch).
	void DopplerRPMSliderChanged()
	{
		dopplerEngineEvent.SetParameter("RPM", Convert.ToSingle(dopplerRPMSlider.Value));
	}

	// Adjusts how fast the Doppler object moves (affects pitch shift intensity).
	void DopplerVelocitySliderChanged()
	{
		velocity.y = -dopplerVelocitySlider.Value * 0.1f;
	}
}
