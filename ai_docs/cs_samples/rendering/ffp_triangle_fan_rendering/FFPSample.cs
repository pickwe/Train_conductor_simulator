// Demonstrates Fixed Function Pipeline (FFP) rendering for drawing 2D primitives
// directly to screen. Renders an animated multi-colored polygon fan that rotates
// over time, showing how to use Ffp for custom procedural graphics.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Animated polygon fan drawn using Fixed Function Pipeline.
public partial class FFPSample : Component
{
	// Event connections for render callback
	EventConnections connection = new EventConnections();

	// Render callback is registered after GUI plugins finish.
	void Init()
	{
		// Connect to render event that fires after plugin GUI is drawn
		Engine.EventEndPluginsGui.Connect(connection, Render);
	}

	// Draws animated triangle fan with rotating colored vertices.
	void Render()
	{
		// Get current screen dimensions for orthographic projection
		int width = 0;
		int height = 0;

		float time = Game.Time;

		EngineWindow main_window = WindowManager.MainWindow;
		if (main_window != null)
		{
			width = main_window.ClientRenderSize.x;
			height = main_window.ClientRenderSize.y;
		}

		// Fan radius is half the screen height to fit nicely
		float radius = height / 2.0f;

		// Enable FFP in solid fill mode and set up 2D orthographic projection
		Ffp.Enable(Ffp.MODE_SOLID);
		Ffp.SetOrtho(width, height);

		// Start defining triangle primitives
		Ffp.BeginTriangles();

		// RGB vertex colors in ARGB format (red, green, blue cycling)
		uint[] colors = { 0xffff0000, 0xff00ff00, 0xff0000ff };

		// Generate vertices arranged in a circle, subtracting time for rotation animation
		int num_vertices = 16;
		for (int i = 0; i < num_vertices; i++)
		{
			float angle = MathLib.PI2 * i / (num_vertices - 1) - time;
			float x = width / 2 + (float)Math.Sin(angle) * radius;
			float y = height / 2 + (float)Math.Cos(angle) * radius;
			Ffp.AddVertex(x, y);
			Ffp.SetColor(colors[i % 3]);
		}

		// Build triangle fan by connecting each edge vertex to center (index 0)
		for (int i = 1; i < num_vertices; i++)
		{
			Ffp.AddIndex(0);
			Ffp.AddIndex(i);
			Ffp.AddIndex(i - 1);
		}

		// Finish triangle batch and submit for rendering
		Ffp.EndTriangles();

		Ffp.Disable();
	}

	// Render callback is disconnected on shutdown.
	void Shutdown()
	{
		connection.DisconnectAll();
	}
}
