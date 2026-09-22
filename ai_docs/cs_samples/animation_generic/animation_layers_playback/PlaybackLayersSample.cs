using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// This component demonstrates composing one animation out of several reusable sequences.
// A sub-sequence channel holds them as clips, and clip times decide how they are combined:
// - Combined: the clips start together, so position, rotation and scale play at once
// - Sequential: the clips follow each other, so position, then rotation, then scale play
// The same sequences serve both players: each player retargets their binds to its own cube.
public partial class PlaybackLayersSample : Component
{
	// Length of a single piece, in seconds: the keys of every piece end at this time
	private const float PIECE_DURATION = 6.0f;

	// The cubes the shared pieces are retargeted to, identified by these ids and names
	private const int COMBINED_BOX_ID = 123;
	private const string COMBINED_BOX_NAME = "box_node_combined";
	private const int SEQUENTIAL_BOX_ID = 456;
	private const string SEQUENTIAL_BOX_NAME = "box_node_sequential";

	// A player does not own a sequence built in code, so the sample keeps them all alive itself
	private AnimationSequence positionSequence;
	private AnimationSequence rotationSequence;
	private AnimationSequence scaleSequence;
	private AnimationSequence combinedSequence;
	private AnimationSequence sequentialSequence;

	private AnimationSequencePlayer playerCombined;
	private AnimationSequencePlayer playerSequential;

	// Animations are created and both players are started.
	private void Init()
	{
		CreateAnimations();

		// Create objects for animation. The channels animate local transform parameters,
		// so a parent node places its cube in the scene without disturbing the animation.
		Node parentCombined = new NodeDummy();
		Node parentSequential = new NodeDummy();

		Node boxNodeCombined = Primitives.CreateBox(vec3.ONE);
		boxNodeCombined.Name = COMBINED_BOX_NAME;
		boxNodeCombined.ID = COMBINED_BOX_ID;
		boxNodeCombined.Parent = parentCombined;
		parentCombined.WorldPosition = new Vec3(-2.0f, 0.0f, 1.75f);

		Node boxNodeSequential = Primitives.CreateBox(vec3.ONE);
		boxNodeSequential.Name = SEQUENTIAL_BOX_NAME;
		boxNodeSequential.ID = SEQUENTIAL_BOX_ID;
		boxNodeSequential.Parent = parentSequential;
		parentSequential.WorldPosition = new Vec3(2.0f, 0.0f, 1.75f);

		playerCombined.Play();
		playerSequential.Play();
	}

	// Both players are stopped on component destruction.
	private void Shutdown()
	{
		playerCombined.Stop();
		playerSequential.Stop();
	}

	// Three single-parameter sequences are created and combined in two different ways.
	private void CreateAnimations()
	{
		// Piece 1: Z position bounce
		positionSequence = new AnimationSequence();

		AnimationChannelScalar positionChannel = new AnimationChannelScalar("node.position_z");
		positionChannel.SetBind(new AnimationBindNode());
		positionChannel.AddValue(0.0f, 0.0f, AnimationCurve.KEY_TYPE.SMOOTH);
		positionChannel.AddValue(3.0f, 2.0f, AnimationCurve.KEY_TYPE.SMOOTH);
		positionChannel.AddValue(6.0f, 0.0f, AnimationCurve.KEY_TYPE.SMOOTH);
		positionSequence.AddChannel(positionChannel);

		// Piece 2: Z-axis rotation
		rotationSequence = new AnimationSequence();

		AnimationChannelQuat rotationChannel = new AnimationChannelQuat(AnimationChannelQuat.MODE.QUAT, "node.rotation");
		rotationChannel.SetBind(new AnimationBindNode());
		rotationChannel.AddQuatValue(0.0f, new quat(0.0f, 0.0f, 0.0f));
		rotationChannel.AddQuatValue(3.0f, new quat(0.0f, 0.0f, 180.0f));
		rotationChannel.AddQuatValue(6.0f, new quat(0.0f, 0.0f, 360.0f));
		rotationSequence.AddChannel(rotationChannel);

		// Piece 3: Scale pulse
		scaleSequence = new AnimationSequence();

		AnimationChannelFVec3 scaleChannel = new AnimationChannelFVec3("node.scale");
		scaleChannel.SetBind(new AnimationBindNode());
		scaleChannel.AddValue(0.0f, new vec3(1.0f, 1.0f, 1.0f), AnimationCurve.KEY_TYPE.SMOOTH);
		scaleChannel.AddValue(3.0f, new vec3(1.5f, 1.5f, 0.66f), AnimationCurve.KEY_TYPE.SMOOTH);
		scaleChannel.AddValue(6.0f, new vec3(1.0f, 1.0f, 1.0f), AnimationCurve.KEY_TYPE.SMOOTH);
		scaleSequence.AddChannel(scaleChannel);

		// Combined: the three clips cover the same time range, so they play in parallel
		{
			combinedSequence = new AnimationSequence();

			AnimationChannelSubSequence clips = new AnimationChannelSubSequence();
			clips.AddEmbeddedSubSequence(0.0f, PIECE_DURATION, positionSequence);
			clips.AddEmbeddedSubSequence(0.0f, PIECE_DURATION, rotationSequence);
			clips.AddEmbeddedSubSequence(0.0f, PIECE_DURATION, scaleSequence);
			combinedSequence.AddChannel(clips);

			playerCombined = new AnimationSequencePlayer(combinedSequence);
			playerCombined.Loop = true;

			// This player drives the left cube
			RetargetPlayer(playerCombined, COMBINED_BOX_ID, COMBINED_BOX_NAME);
		}

		// Sequential: the clips follow each other, so the pieces play one after another.
		// A finished clip stops writing its parameter, and the cube keeps the value it left.
		{
			sequentialSequence = new AnimationSequence();

			AnimationChannelSubSequence clips = new AnimationChannelSubSequence();
			clips.AddEmbeddedSubSequence(0.0f, PIECE_DURATION, positionSequence);
			clips.AddEmbeddedSubSequence(PIECE_DURATION, PIECE_DURATION, rotationSequence);
			clips.AddEmbeddedSubSequence(PIECE_DURATION * 2.0f, PIECE_DURATION, scaleSequence);
			sequentialSequence.AddChannel(clips);

			playerSequential = new AnimationSequencePlayer(sequentialSequence);
			playerSequential.Loop = true;

			// The right cube is the target of this player
			RetargetPlayer(playerSequential, SEQUENTIAL_BOX_ID, SEQUENTIAL_BOX_NAME);
		}
	}

	// A player enumerates every bind of its sequences, including the ones inside sub-sequences,
	// so the shared pieces are pointed at this player's own node without being changed themselves.
	private static void RetargetPlayer(AnimationSequencePlayer player, int nodeID, string nodeName)
	{
		AnimationBindNode bind = new AnimationBindNode();
		bind.NumTargets = 1;
		bind.SetTargetNodeDescription(0, nodeID, nodeName);

		for (int i = 0; i < player.BindCount; i += 1)
			player.SetBind(i, bind);
	}
}
