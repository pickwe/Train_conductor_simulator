// Visualization helper for NodeSpawnerTimer that shows a color-changing
// sphere indicating time until next spawn. Color transitions from red (just
// spawned) to green (about to spawn) using MathLib.Lerp for smooth interpolation.

using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Unigine;

// Adds visual feedback and UI controls to the NodeSpawnerTimer component.
public partial class NodeSpawnerTimerSample : Component
{
	// Reference to the spawner component to visualize and control
	public NodeSpawnerTimer spawner = null;

	// Sets up the spawn rate slider and enables visualization.
	private void Init()
	{
		Visualizer.Enabled = true;

		spawner.spawnRate = 5.0f;
		var descriptionWindowCreator = FindComponentInWorld<DescriptionWindowCreator>();
		var sampleDescriptionWindow = descriptionWindowCreator.getWindow();
		sampleDescriptionWindow.addFloatParameter("Spawn Frequency", "spawn frequency", 
			spawner.spawnRate, 0.1f, 20.0f, v => { spawner.spawnRate = v; });
	}

	// Draws a sphere that changes color based on spawn timer progress.
	private void Update()
	{
		// Interpolate from red to green based on how close we are to spawning
		vec4 color = MathLib.Lerp(vec4.RED, vec4.GREEN, MathLib.InverseLerp(0, spawner.spawnRate, spawner.timer));
		color.w = 0.5f;  // Semi-transparent
		Visualizer.RenderSolidSphere(1.0f, spawner.node.WorldTransform, color);
	}

	// Cleans up visualizer on shutdown.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
	}
}
