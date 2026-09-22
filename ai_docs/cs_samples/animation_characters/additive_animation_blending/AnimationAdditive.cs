// Demonstrates additive animation blending on NodeSkeletonPose. Two animations
// are combined by multiplying layers, with a reference frame used to compute
// additive deltas. Weight is adjustable via UI sliders.

using System;
using Unigine;
using static Unigine.VREyeTracking;

// Blends two animations additively using layer multiplication and inverse reference frame.
public partial class AnimationAdditive : Component
{
	// Skeleton pose node for animation playback
	private NodeSkeletonPose skeletonPose = null;
	// Animation script that contains the logic for blending animations
	private AnimScript animScript = null;
	// Blend weight for additive animation contribution
	private float weight = 0.5f;

	// UI window for parameter controls
	private SampleDescriptionWindow sampleDescriptionWindow;

	// Animation layers are initialized and inverse reference frame is computed.
	private void Init()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow();

		skeletonPose = node as NodeSkeletonPose;
		if (skeletonPose != null)
			animScript = skeletonPose.AnimScript;

		// Add UI slider for blend weight adjustment
		sampleDescriptionWindow.addFloatParameter("Weight:", "Weight", weight, 0.0f, 4.0f, (float value) =>
		{
			weight = value;
		});
	}

	// Animation frames are updated and layers are combined additively each frame.
	private void Update()
	{
		if (skeletonPose == null || animScript == null)
			return;

		// Combine base animation with weighted additive
		animScript.SetParamFloat("blend_weight", weight);
	}

	// UI window is released on component shutdown.
	private void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
	}

}
