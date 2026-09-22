// Controls the animated cat character (ObjectMeshSkinnedLegacy) with two-layer animation
// blending between idle and run states. Rotation uses exponential smoothing for
// natural turning, and run animation speed matches actual movement speed.

using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Skinned mesh implementation of the persecutor (animated cat character).
// Handles skeletal animation blending between idle and run states.
public partial class PersecutorSkinned : Component, PersecutorBase
{
	[ShowInEditor, ParameterFile]
	public string persecutorRunAnim = "";	// Running animation file
	[ShowInEditor, ParameterFile]
	public string persecutorIdleAnim = "";	// Idle animation file
	[ShowInEditor]
	public float moveRate = 10.0f;			// Position interpolation speed
	[ShowInEditor]
	public float turnRate = 15.0f;			// Rotation interpolation speed
	[ShowInEditor]
	public float animationSpeed = 15.0f;

	private ObjectMeshSkinnedLegacy persecutor = null;

	// Animation settings
	private float idleAnimationWeight = 0.0f;
	private float runAnimationTime = 0.0f;

	// Node is cast to ObjectMeshSkinnedLegacy and animation layers are configured.
	private void Init()
	{
		persecutor = node as ObjectMeshSkinnedLegacy;
		if (persecutor == null)
		{
			Log.Error("PersecutorSkinned.Init(): node is not ObjectMeshSkinnedLegacy!\n");
			return;
		}

		// Set up two animation layers for blending: idle (layer 0) and run (layer 1).
		// Layers allow smooth transitions between animation states.
		persecutor.NumLayers = 2;
		persecutor.SetLayerAnimationFilePath(0, persecutorIdleAnim);
		persecutor.SetLayerAnimationFilePath(1, persecutorRunAnim);
	}

	// World position of the skinned mesh is returned.
	public Vec3 GetPosition()
	{
		return persecutor != null ? persecutor.WorldPosition : Vec3.ZERO;
	}

	// World rotation of the skinned mesh is returned.
	public quat GetRotation()
	{
		return persecutor != null ? persecutor.GetWorldRotation() : quat.IDENTITY;
	}

	// World position of the skinned mesh is set directly.
	public void SetPosition(Vec3 newPosition)
	{
		if (persecutor != null)
			persecutor.WorldPosition = newPosition;
	}

	// Rotation is smoothly interpolated toward the target direction.
	public void SetRotation(Vec3 targetDirection)
	{
		if (persecutor == null)
			return;

		// Build rotation from direction vector. Model faces -X, so apply 90 degree correction.
		quat targetRotation = MathLib.RotationFromDir(new vec3(targetDirection), vec3.UP) * new quat(vec3.UP, -90.0f);

		// Exponential smoothing for rotation: creates natural turning motion
		// Higher turnRate = snappier turns, lower = more gradual
		persecutor.SetWorldRotation(MathLib.Slerp(persecutor.GetWorldRotation(), targetRotation,
			1.0f - MathLib.Exp(-turnRate * Game.IFps)));
	}

	// Animation layers are blended based on movement state and speed.
	public void SetAnimation(float persecutorSpeed, bool reachedTarget)
	{
		if (persecutor == null)
			return;

		// Blend between idle and run animations based on movement state.
		// Uses exponential moving average for smooth weight transitions.
		idleAnimationWeight = MathLib.Lerp(idleAnimationWeight, reachedTarget ? 1.0f : 0.0f,
			MathLib.Saturate(moveRate * Game.IFps));
		persecutor.SetLayer(0, true, idleAnimationWeight);			// Idle layer weight
		persecutor.SetLayer(1, true, 1.0f - idleAnimationWeight);	// Run layer weight (inverse)

		// Advance run animation based on movement speed (speed-matched footsteps)
		runAnimationTime += persecutorSpeed * animationSpeed * Game.IFps;
		persecutor.SetLayerFrame(0, 0);					// Idle stays at frame 0
		persecutor.SetLayerFrame(1, runAnimationTime);	// Run advances with movement
	}
}
