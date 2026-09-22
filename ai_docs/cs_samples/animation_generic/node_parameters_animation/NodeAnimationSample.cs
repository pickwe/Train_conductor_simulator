using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// This component demonstrates node transform animation using the Animation system.
// AnimationBindNode points channels at a scene node, animating its position, rotation and scale.
// Different channel types are shown: Scalar (single axis), Quat (rotation),
// and FVec3 (3D vector). Animation curves can use different interpolation types.
public partial class NodeAnimationSample : Component
{
	// The node the animation channels are bound to, identified by this id and name
	private const int BOX_ID = 123;
	private const string BOX_NAME = "box";

	// A player does not own a sequence built in code, so the sample keeps it alive itself
	private AnimationSequence sequence;
	private AnimationSequencePlayer player;
	private Node box;

	private SampleDescriptionWindow descriptionWindow = new SampleDescriptionWindow();
	private WidgetEditLine positionField;
	private WidgetEditLine rotationField;
	private WidgetEditLine scaleFieldX;
	private WidgetEditLine scaleFieldY;
	private WidgetEditLine scaleFieldZ;

	private void Init()
	{
		// Create animation sequence and player
		CreateAnimations();

		// A box primitive is created as the animation target
		box = Primitives.CreateBox(vec3.ONE);
		box.Name = BOX_NAME;
		box.ID = BOX_ID;
		box.WorldPosition = new Vec3(0.0f, 0.0f, 1.15f);

		player.Play();

		// GUI is initialized manually
		InitGui();
	}

	private void Update()
	{
		// GUI state is updated manually each frame
		positionField.Text = box.WorldPosition.z.ToString("0.00");
		rotationField.Text = box.GetWorldRotation().GetAngle(vec3.UP).ToString("0.00");
		scaleFieldX.Text = box.WorldScale.x.ToString("0.00");
		scaleFieldY.Text = box.WorldScale.y.ToString("0.00");
		scaleFieldZ.Text = box.WorldScale.z.ToString("0.00");
	}

	private void Shutdown()
	{
		// GUI resources are released manually before component destruction
		descriptionWindow.shutdown();
		player.Stop();
	}

	// Animation sequence with transform channels for position, rotation, and scale is created
	private void CreateAnimations()
	{
		sequence = new AnimationSequence();

		// Bind to target node by ID and name (both used for identification).
		// SetBind() copies the bind, so one bind describes the target of all three channels.
		AnimationBindNode bind = new AnimationBindNode();
		bind.NumTargets = 1;
		bind.SetTargetNodeDescription(0, BOX_ID, BOX_NAME);

		// AnimationChannelScalar animates a single float value (here: Z position only)
		AnimationChannelScalar positionChannel = new AnimationChannelScalar("node.world_position_z");
		positionChannel.SetBind(bind);

		// AnimationCurveScalar stores keyframes with time and value pairs
		AnimationCurveScalar positionCurve = new AnimationCurveScalar();
		positionCurve.AddKey(0.0f, 1.5f + 0.0f);
		positionCurve.AddKey(4.0f, 1.5f + 2.0f);
		positionCurve.AddKey(8.0f, 1.5f + 0.0f);

		// KEY_TYPE.SMOOTH uses Bezier curve with symmetric tangents
		positionCurve.SetTypeOfAllKeys(AnimationCurve.KEY_TYPE.SMOOTH);

		positionChannel.Curve = positionCurve;
		sequence.AddChannel(positionChannel);

		// AnimationChannelQuat animates rotation using quaternion interpolation (slerp)
		// MODE.QUAT uses AnimationCurveQuat directly; MODE.ANGLES_XYZ/ANGLES_ZYX use three separate
		// AnimationCurveFloat curves for each angle, composing a quaternion in XYZ or ZYX order
		AnimationChannelQuat rotationChannel = new AnimationChannelQuat(AnimationChannelQuat.MODE.QUAT, "node.world_rotation");
		rotationChannel.SetBind(bind);
		rotationChannel.AddQuatValue(0.0f, new quat(0.0f, 0.0f, 0.0f));
		rotationChannel.AddQuatValue(4.0f, new quat(0.0f, 0.0f, 180.0f));
		rotationChannel.AddQuatValue(8.0f, new quat(0.0f, 0.0f, 360.0f));
		sequence.AddChannel(rotationChannel);

		// AnimationChannelFVec3 animates a 3D vector (here: scale)
		AnimationChannelFVec3 scaleChannel = new AnimationChannelFVec3("node.world_scale");
		scaleChannel.SetBind(bind);
		scaleChannel.AddValue(0.0f, new vec3(1.0f, 1.0f, 1.0f), AnimationCurve.KEY_TYPE.SMOOTH);
		scaleChannel.AddValue(4.0f, new vec3(1.5f, 1.5f, 0.66f), AnimationCurve.KEY_TYPE.SMOOTH);
		scaleChannel.AddValue(8.0f, new vec3(1.0f, 1.0f, 1.0f), AnimationCurve.KEY_TYPE.SMOOTH);
		sequence.AddChannel(scaleChannel);

		// Create player for the sequence
		player = new AnimationSequencePlayer(sequence);
		player.Loop = true;
	}

	// ========================================================================================

	private void InitGui()
	{
		descriptionWindow.createWindow();

		WidgetGroupBox stateBox = new WidgetGroupBox("State", 9, 3);
		descriptionWindow.MainWindow.AddChild(stateBox);

		WidgetGridBox grid = new WidgetGridBox(2);
		stateBox.AddChild(grid, Gui.ALIGN_LEFT);

		positionField = AddStateField(grid, "node.position.z");
		rotationField = AddStateField(grid, "node.rotation.z");
		scaleFieldX = AddStateField(grid, "node.scale.x");
		scaleFieldY = AddStateField(grid, "node.scale.y");
		scaleFieldZ = AddStateField(grid, "node.scale.z");
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
