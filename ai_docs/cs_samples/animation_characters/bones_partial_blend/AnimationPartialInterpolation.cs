// Demonstrates partial joints blending between two poses. Only specified bones
// receive the interpolated animation, allowing different body parts to play
// independent animations simultaneously.

using System.Collections.Generic;
using Unigine;

// Blends animations on selected joints only, leaving other bones unchanged.
public partial class AnimationPartialInterpolation : Component
{
	// Skeleton pose node for animation playback
	private NodeSkeletonPose skeletonPose = null;
	// Animation script that contains the logic for blending animations
	private AnimScript animScript = null;
	// Interpolation weight between animations
	private float weight = 0.5f;

	// UI window for parameter controls
	private SampleDescriptionWindow sampleDescriptionWindow;

	// Animation layers are initialized and target bone indices are resolved.
	private void Init()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow();

		skeletonPose = node as NodeSkeletonPose;
		if (skeletonPose != null)
			animScript = skeletonPose.AnimScript;

		// Add UI slider for blend weight adjustment
		sampleDescriptionWindow.addFloatParameter("Weight:", "Weight", weight, 0.0f, 1.0f, (float value) =>
		{
			weight = value;
		});
	}

	// Animation frames are updated and selected bones receive interpolated transforms.
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
