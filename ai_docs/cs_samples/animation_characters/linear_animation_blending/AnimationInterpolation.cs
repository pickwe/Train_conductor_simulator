// Demonstrates linear interpolation between two poses on NodeSkeletonPose.
// Two animation are blended using BlendPoses with adjustable weight, creating
// smooth transitions between different animation poses.

using Unigine;

// Blends two animations using linear interpolation with adjustable weight.
public partial class AnimationInterpolation : Component
{
	// Skeleton pose node for animation playback
	private NodeSkeletonPose skeletonPose = null;
	// Animation script that contains the logic for blending animations
	private AnimScript animScript = null;
	// Interpolation weight between animations (0 = idle, 1 = walk)
	private float weight = 0.5f;

	// UI window for parameter controls
	private SampleDescriptionWindow sampleDescriptionWindow;

	private void Init()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow();

		// Get skeleton pose node and animation script
		skeletonPose = node as NodeSkeletonPose;
		if (skeletonPose != null)
			animScript = skeletonPose.AnimScript;

		// Add UI slider for blend weight adjustment
		sampleDescriptionWindow.addFloatParameter("Weight:", "Weight", weight, 0.0f, 1.0f, (float value) =>
		{
			weight = value;
		});
	}

	private void Update()
	{
		if (skeletonPose == null || animScript == null)
			return;

		// Linearly interpolate between poses based on weight
		animScript.SetParamFloat("blend_weight", weight);
	}

	// UI window is released on component shutdown.
	private void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
	}
}
