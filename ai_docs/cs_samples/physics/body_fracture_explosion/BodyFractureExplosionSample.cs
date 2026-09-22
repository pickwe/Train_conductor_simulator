// Provides UI for triggering body fracture explosions. Creates a button
// that triggers the explosion effect on fracturable physics objects.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// UI button to trigger explosion effect.
public partial class BodyFractureExplosionSample : Component
{
	// Explosion component reference
	public BodyFractureExplosion explosion = null;
	// Sample UI window
	public SampleDescriptionWindow sampleDescriptionWindow = new();

	// Explosion button is created.
	void Init()
	{
		sampleDescriptionWindow.createWindow();
		var btn = new WidgetButton("Explode!");
		btn.EventClicked.Connect(() => explosion?.Explode());
		sampleDescriptionWindow.getParameterGroupBox().AddChild(btn);

		Visualizer.Enabled = true;
	}

	// UI is cleaned up on shutdown.
	void Shutdown()
	{
		Visualizer.Enabled = false;

		sampleDescriptionWindow.shutdown();
	}

}
