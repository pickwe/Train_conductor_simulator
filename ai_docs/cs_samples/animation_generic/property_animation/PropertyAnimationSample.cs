using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// This component demonstrates property parameter animation using the Animation system.
// AnimationBindPropertyParameter allows animating any property parameter attached to a node.
// Here, a "speed" property parameter is animated, which then drives object rotation in Update().
// This pattern is useful for data-driven animations where logic reads animated values.
public partial class PropertyAnimationSample : Component
{
	// The node the animation channel is bound to, identified by this id and name
	private const int BOX_ID = 123;
	private const string BOX_NAME = "box";
	// The animated property and the name of its animated parameter
	private const string PROPERTY_NAME = "speed_prop";
	private const string PARAMETER_NAME = "speed";

	private Node box;

	// A player does not own a sequence built in code, so the sample keeps it alive itself
	private AnimationSequence sequence;
	private AnimationSequencePlayer player;

	private SampleDescriptionWindow descriptionWindow = new SampleDescriptionWindow();
	private WidgetEditLine speedField;

	private void Init()
	{
		// Animation sequence and player are created
		CreateAnimations();

		// A box primitive is created as the animation target
		box = Primitives.CreateBox(new vec3(1.0f, 1.0f, 1.0f));
		box.ID = BOX_ID;
		box.Name = BOX_NAME;
		box.WorldPosition = new Vec3(0.0f, 0.0f, 1.15f);

		// Without the property there is nothing to animate, so the asset has to be there
		Property property = Properties.FindPropertyByPath(GetWorldRootPath() + "properties/speed_prop.prop");
		if (property != null)
			box.AddProperty(property);
		else
			Log.Error($"PropertyAnimationSample.Init(): cannot find the \"{PROPERTY_NAME}\" property asset\n");

		player.Play();

		// GUI is initialized manually
		InitGui();
	}

	private void Update()
	{
		// The animated property value is read and used to drive rotation.
		// The animation system updates the property; game logic reads and applies it.
		float speed = GetAnimatedSpeed();

		box.WorldRotate(0.0f, 0.0f, speed * Game.IFps);

		// GUI state is updated manually each frame
		speedField.Text = speed.ToString("0.00");
	}

	private void Shutdown()
	{
		// GUI resources are released manually before component destruction
		descriptionWindow.shutdown();
		player.Stop();
	}

	// Current value of the animated property parameter, 0 while the property is not resolved
	private float GetAnimatedSpeed()
	{
		Property property = box.GetProperty();
		if (property == null)
			return 0.0f;

		PropertyParameter parameter = property.GetParameterPtr(PARAMETER_NAME);
		return parameter != null ? parameter.ValueFloat : 0.0f;
	}

	// Animation that modifies a property parameter value over time is created
	private void CreateAnimations()
	{
		sequence = new AnimationSequence();

		// AnimationBindPropertyParameter points a channel at a specific parameter within a property.
		// ACCESS.FROM_NODE finds the property on a target node; targets are addressed by index.
		AnimationBindPropertyParameter bind = new AnimationBindPropertyParameter();
		bind.Access = AnimationBindPropertyParameter.ACCESS.FROM_NODE;
		bind.SetNodePropertyDescription(PROPERTY_NAME, 0);	// Property name and index
		bind.NumTargets = 1;
		bind.SetTargetNodeDescription(0, BOX_ID, BOX_NAME);	// Target node
		bind.ParameterPath = PARAMETER_NAME;					// Parameter name within property

		// "property_parameter.value_float" is the animated parameter of float property parameters
		AnimationChannelFloat speedChannel = new AnimationChannelFloat("property_parameter.value_float");
		speedChannel.SetBind(bind);
		speedChannel.AddValue(0.0f, 0.0f);		// Start at 0
		speedChannel.AddValue(3.0f, 120.0f);	// Accelerate to 120
		speedChannel.AddValue(9.0f, -120.0f);	// Reverse to -120
		speedChannel.AddValue(12.0f, 0.0f);		// Return to 0

		// The sequence copies the channel, so it has to be fully set up by now
		sequence.AddChannel(speedChannel);

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

		speedField = AddStateField(grid, "speed_prop.speed");
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
