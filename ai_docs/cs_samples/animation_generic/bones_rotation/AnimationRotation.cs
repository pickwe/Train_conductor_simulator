// Demonstrates joint rotation with additive animation blending. An idle animation
// plays continuously while left/right shoot animations are additively blended.
// A horizontal joint bone is rotated programmatically each frame.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

using Unigine;

// Combines idle animation with alternating shoot animations and programmatic bone rotation.
public partial class AnimationRotation : Component
{
	// Skeleton pose node for animation playback
	private NodeSkeletonPose skeletonPose = null;
	// Bone index for horizontal rotation joint
	private const int horizontalJointBone = 1;

	// Accumulated world transform for rotating bone
	private mat4 boneTransform = mat4.IDENTITY;

	private void Init()
	{
		skeletonPose = node as NodeSkeletonPose;
		if (!skeletonPose)
			return;

		// Store initial bone transform for rotation accumulation
		boneTransform = skeletonPose.GetJointTransform(horizontalJointBone);
	}

	// Idle animation is updated, shoot animation is blended, and bone is rotated each frame.
	private void PostUpdate()
	{
		if (!skeletonPose)
			return;

		// Rotate horizontal joint bone continuously around Z axis
		boneTransform = boneTransform * MathLib.RotateZ(45.0f * Game.IFps);
		skeletonPose.SetJointTransform(horizontalJointBone, boneTransform);
		skeletonPose.ForceApplyPose();
	}
}
