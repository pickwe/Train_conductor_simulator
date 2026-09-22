using Unigine;

// This component demonstrates singleton animation channels for global engine parameters.
// Unlike channels that target specific nodes/materials, a singleton channel needs no bind:
// it animates a global system like Physics (gravity) or Render (background color).
// Important: animation players have "engine lifetime" - they persist across world changes.
// Active players must be stopped when switching worlds to prevent unwanted effects.
public partial class SingletonAnimationSample : Component
{
	// A player does not own a sequence built in code, so the sample keeps it alive itself
	private AnimationSequence sequence;
	private AnimationSequencePlayer player;

	private SampleDescriptionWindow descriptionWindow = new SampleDescriptionWindow();
	private WidgetEditLine backgroundColorField;
	private WidgetEditLine gravityField;

	private void Init()
	{
		// Create the sequence and its player
		CreateAnimations();

		player.Play();

		InitGui();
	}

	private void Update()
	{
		backgroundColorField.Text = Render.BackgroundColor.w.ToString("0.00");
		gravityField.Text = Physics.Gravity.z.ToString("0.0");
	}

	private void Shutdown()
	{
		descriptionWindow.shutdown();

		// Animation sequences and players have "engine lifetime" - they persist
		// from creation until engine shutdown and are preserved between different worlds.
		// Active players must be stopped when switching worlds to prevent continued playback.
		player.Stop();

		// Render fade color is restored to transparent black
		Render.BackgroundColor = new vec4(0.0f, 0.0f, 0.0f, 0.0f);
	}

	// Create animation sequence with singleton channels for global engine parameters
	private void CreateAnimations()
	{
		// Create new sequence
		sequence = new AnimationSequence();

		// Channel for Physics.Gravity Z component (global engine parameter)
		// AddValue(time_sec, value) adds a keyframe: time in seconds and the value at that time
		AnimationChannelFloat gravityChannel = new AnimationChannelFloat("physics.gravity_z");
		gravityChannel.AddValue(0.0f, -9.8f);	// Normal gravity
		gravityChannel.AddValue(3.0f, 2.5f);	// Reverse (objects float up)
		gravityChannel.AddValue(4.0f, -1.0f);
		gravityChannel.AddValue(5.0f, -4.5f);
		gravityChannel.AddValue(6.0f, -9.8f);	// Normal gravity
		// A channel left without a bind targets global engine state
		sequence.AddChannel(gravityChannel);

		// Channel for Render.BackgroundColor alpha component (global engine parameter)
		// Alpha approaching 1 creates a fade-to-white effect
		AnimationChannelFloat colorChannel = new AnimationChannelFloat("render.background_color_w");
		colorChannel.AddValue(0.0f, 0.0f);
		colorChannel.AddValue(3.0f, 1.0f);	// Full white fade
		colorChannel.AddValue(4.0f, 1.0f);
		colorChannel.AddValue(5.0f, 0.5f);
		colorChannel.AddValue(6.0f, 0.0f);
		sequence.AddChannel(colorChannel);

		// Sequences can be serialized to disk for reuse, the ".seq" extension is required
		string sequenceDirectory = GetWorldRootPath() + "sequences";
		Dir.Mkdir(FileSystem.GetAbsolutePath(sequenceDirectory));
		string sequencePath = sequenceDirectory + "/singletons.seq";
		sequence.Path = sequencePath;

		if (sequence.Save() == false)
			Log.Warning($"SingletonAnimationSample: can't save the sequence to \"{sequencePath}\"\n");

		// A saved sequence can be played right from its file (serialization roundtrip)
		player = new AnimationSequencePlayer(sequencePath);

		// The file may be unavailable, then the in-memory sequence is played instead
		if (player.HasSequence() == false)
			player = new AnimationSequencePlayer(sequence);

		player.Loop = true;
	}

	// Directory the sample's world lies in, with a trailing slash
	private static string GetWorldRootPath()
	{
		string worldPath = World.Path;
		return worldPath.Substring(0, worldPath.LastIndexOf('/') + 1);
	}

	// ========================================================================================

	private void InitGui()
	{
		descriptionWindow.createWindow();

		WidgetGroupBox stateBox = new WidgetGroupBox("State", 9, 3);
		descriptionWindow.MainWindow.AddChild(stateBox);

		WidgetGridBox grid = new WidgetGridBox(2);
		stateBox.AddChild(grid, Gui.ALIGN_LEFT);

		backgroundColorField = AddStateField(grid, "render.background_color.a");
		gravityField = AddStateField(grid, "physics.gravity.z");
	}

	// A read-only row "parameter name | current value" in the state grid
	private static WidgetEditLine AddStateField(WidgetGridBox grid, string name)
	{
		WidgetHBox nameBox = new WidgetHBox();
		nameBox.AddChild(new WidgetLabel(name));
		nameBox.AddChild(new WidgetHBox(6));
		grid.AddChild(nameBox, Gui.ALIGN_LEFT);

		WidgetEditLine valueField = new WidgetEditLine();
		valueField.Editable = false;
		valueField.FontVOffset = -2;
		valueField.FontColor = new vec4(0.9f, 0.9f, 0.9f, 1.0f);
		valueField.Width = 50;
		grid.AddChild(valueField, Gui.ALIGN_LEFT);

		return valueField;
	}
}
