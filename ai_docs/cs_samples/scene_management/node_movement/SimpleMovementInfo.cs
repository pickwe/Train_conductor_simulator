// UI controller for the SimpleMovement demo that synchronizes parameters
// across all three movement variants. Provides sliders for velocity and
// angular velocity, plus a reset button to restore initial positions.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

#if UNIGINE_DOUBLE
using Mat4 = Unigine.dmat4;
#else
using Mat4 = Unigine.mat4;
#endif

// Manages shared parameters and reset functionality for movement samples.
public partial class SimpleMovementInfo : Component
{
	// References to each movement variant's component
	[ShowInEditor, Parameter(Title = "Simple Movement Component")]
	private SimpleMovement simpleMovement = null;

	[ShowInEditor, Parameter(Title = "Simple Movement2 Component")]
	private SimpleMovement2 simpleMovement2 = null;

	[ShowInEditor, Parameter(Title = "Simple Movement3 Component")]
	private SimpleMovement3 simpleMovement3 = null;

	// UI window for parameter controls
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();

	// Cached starting transforms for reset functionality
	private Mat4 simpleMovementStartTransform;
	private Mat4 simpleMovement2StartTransform;
	private Mat4 simpleMovement3StartTransform;

	// Sets up UI controls and saves initial node positions.
	void Init()
	{
		init_components();

		sampleDescriptionWindow.createWindow();

		sampleDescriptionWindow.addFloatParameter("Velocity", "Velocity", 3.0f, 1.0f, 50.0f,
			v =>
			{
				simpleMovement.Velocity = v;
				simpleMovement2.Velocity = v;
				simpleMovement3.Velocity = v;
			});

		sampleDescriptionWindow.addFloatParameter("Angular Velocity", "Angular Velocity", 50.0f, 1.0f, 100.0f,
			v =>
			{
				simpleMovement.AngularVelocity = v;
				simpleMovement2.AngularVelocity = v;
				simpleMovement3.AngularVelocity = v;
			});

		var parameters = sampleDescriptionWindow.getParameterGroupBox();
		var reset_button = new WidgetButton("Reset Position");

		reset_button.EventClicked.Connect((w, p) =>
		{
			simpleMovement.node.Transform = simpleMovementStartTransform;
			simpleMovement2.node.Transform = simpleMovement2StartTransform;
			simpleMovement3.node.Transform = simpleMovement3StartTransform;
		});

		parameters?.AddChild(reset_button, Gui.ALIGN_LEFT | Gui.ALIGN_EXPAND);
	}

	// Cleans up UI on shutdown.
	void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
	}

	// Validates component references and caches their starting transforms.
	private void init_components()
	{
		if (simpleMovement == null)
		{
			Log.Error("SimpleMovementInfo::init: cannot find SimpleMovement component!\n");
		}
		else
		{
			simpleMovementStartTransform = simpleMovement.node.Transform;
		}

		if (simpleMovement2 == null)
		{
			Log.Error("SimpleMovementInfo::init: cannot find SimpleMovement2 component!\n");
		}
		else
		{
			simpleMovement2StartTransform = simpleMovement2.node.Transform;
		}

		if (simpleMovement3 == null)
		{
			Log.Error("SimpleMovementInfo::init: cannot find SimpleMovement3 component!\n");
		}
		else
		{
			simpleMovement3StartTransform = simpleMovement3.node.Transform;
		}
	}
}
