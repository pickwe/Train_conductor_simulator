// Demo controller for the coroutines sample showcasing async-like animation.
// Provides UI buttons to trigger animations and checkboxes to toggle node/component
// enabled states. The Animator component handles actual coroutine execution,
// demonstrating how coroutines pause when their node or component is disabled.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// UI controller that triggers coroutine-based animations on the Animator component.
public partial class CoroutinesSample : Component
{
	// The cube with Animator component that runs coroutine animations
	[ShowInEditor] private Animator animatedCube = null;
	// Target positions for move animations
	[ShowInEditor] private Node leftTargetNode = null;
	[ShowInEditor] private Node rightTargetNode = null;

	private SampleDescriptionWindow window = null;

	// Creates UI with animation controls and node/component enable toggles.
	private void Init()
	{
		InitComponents();

		window = new SampleDescriptionWindow();
		window.createWindow();

		Widget parameters = window.getParameterGroupBox();

		var nodeEnabledCheckBox = new WidgetCheckBox("Node Enabled");
		parameters.AddChild(nodeEnabledCheckBox, Gui.ALIGN_LEFT);
		nodeEnabledCheckBox.EventChanged.Connect(() =>
		{
			animatedCube.node.Enabled = nodeEnabledCheckBox.Checked;
		});
		nodeEnabledCheckBox.Checked = true;

		var componentEnabledCheckBox = new WidgetCheckBox("Component Enabled");
		parameters.AddChild(componentEnabledCheckBox, Gui.ALIGN_LEFT);
		componentEnabledCheckBox.EventChanged.Connect(() =>
		{
			animatedCube.Enabled = componentEnabledCheckBox.Checked;
		});
		componentEnabledCheckBox.Checked = true;

		var animateToLeftTargetButton = new WidgetButton("Animate to the Left Target");
		parameters.AddChild(animateToLeftTargetButton, Gui.ALIGN_LEFT);
		animateToLeftTargetButton.EventClicked.Connect(() =>
		{
			animatedCube.RunAnimation(leftTargetNode);
		});

		var animateToRightTargetButton = new WidgetButton("Animate to the Right Target");
		parameters.AddChild(animateToRightTargetButton, Gui.ALIGN_LEFT);
		animateToRightTargetButton.EventClicked.Connect(() =>
		{
			animatedCube.RunAnimation(rightTargetNode);
		});

		var stopAnimationButton = new WidgetButton("Stop Animation");
		parameters.AddChild(stopAnimationButton, Gui.ALIGN_LEFT);
		stopAnimationButton.EventClicked.Connect(() =>
		{
			animatedCube.StopAnimation();
		});
	}

	// Cleans up UI resources.
	private void Shutdown()
	{
		window.shutdown();
	}

	// Validates that required component references are assigned in editor.
	void InitComponents()
	{
		if (!animatedCube)
		{
			Log.Error("CoroutinesSample.Init.InitComponents animatedCube is not assigned!\n");
		}

		if (!leftTargetNode || !rightTargetNode)
		{
			Log.Error("CoroutinesSample.Init.InitComponents targets are not assigned!\n");
		}
	}
}
