// Demonstrates WidgetCanvas for drawing 2D vector graphics (lines, polygons, text).
// Creates colored shapes using parametric generation, then animates the entire canvas
// with a 3D perspective transform for a tumbling visual effect. Shows how to overlay
// custom graphics on the screen independent of 3D scene rendering.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Animated 2D canvas with procedurally generated shapes and text.
public partial class Canvas : Component
{
	private WidgetCanvas canvas;

	// Creates various geometric shapes and text on the canvas.
	void Init()
	{
		canvas = new WidgetCanvas();

		// Create overlapping triangular line shapes (3, 4, 5 sides) in different colors
		canvas.SetLineColor(create_line(canvas, 0, 200.0f, 200.0f, 100.0f, 3, 360.0f), new vec4(0.0f, 0.0f, 1.0f, 1.0f));
		canvas.SetLineColor(create_line(canvas, 0, 200.0f, 200.0f, 100.0f, 4, 360.0f), new vec4(0.0f, 1.0f, 0.0f, 1.0f));
		canvas.SetLineColor(create_line(canvas, 0, 200.0f, 200.0f, 100.0f, 5, 360.0f), new vec4(1.0f, 0.0f, 0.0f, 1.0f));

		// Create a spiral line pattern (16 segments over 9 full rotations)
		canvas.SetLineColor(create_line(canvas, 0, 800.0f, 400.0f, 100.0f, 16, 360.0f * 9.0f), new vec4(1.0f, 1.0f, 1.0f, 1.0f));

		// Hexagon with inscribed triangle (order determines draw depth)
		canvas.SetPolygonColor(create_polygon(canvas, 0, 600.0f, 200.0f, 100.0f, 6, 360.0f), new vec4(1.0f, 0.0f, 0.0f, 1.0f));
		canvas.SetPolygonColor(create_polygon(canvas, 1, 600.0f, 200.0f, 100.0f, 3, 360.0f), new vec4(0.0f, 0.0f, 1.0f, 1.0f));

		// Octagon with inscribed square
		canvas.SetPolygonColor(create_polygon(canvas, 0, 400.0f, 400.0f, 100.0f, 8, 360.0f), new vec4(0.0f, 1.0f, 0.0f, 1.0f));
		canvas.SetPolygonColor(create_polygon(canvas, 1, 400.0f, 400.0f, 100.0f, 4, 360.0f), new vec4(1.0f, 0.0f, 0.0f, 1.0f));

		create_text(canvas, 0, 200.0f - 64.0f, 200.0f - 30.0f, "This is C# canvas text");

		Gui.GetCurrent().AddChild(canvas, Gui.ALIGN_OVERLAP | Gui.ALIGN_BACKGROUND);

		// Allow rendering when window is in background
		Engine.BackgroundUpdate = Engine.BACKGROUND_UPDATE.BACKGROUND_UPDATE_RENDER_NON_MINIMIZED;
	}

	// Applies animated 3D perspective rotation to the entire canvas.
	void Update()
	{
		Gui gui = Gui.GetCurrent();

		float fov = 2.0f;
		float time = Game.Time;
		float x = gui.Width / 2.0f;
		float y = gui.Height / 2.0f;
		// Compose transform: translate to center, apply perspective, rotate on X/Y axes, translate back
		canvas.Transform = MathLib.Translate(new vec3(x, y, 0.0f)) * MathLib.Perspective(fov, 1.0f, 0.01f, 100.0f) * MathLib.RotateY((float)Math.Sin(time)) * MathLib.RotateX((float)Math.Cos(time * 0.5f)) * MathLib.Translate(new vec3(-x, -y, -1.0f / (float)Math.Tan(fov * MathLib.DEG2RAD * 0.5f)));
	}

	void Shutdown()
	{
		canvas.DeleteLater();
	}

	// Creates a line strip arranged in a circular pattern.
	// num: number of line segments, angle: total arc (360 = full circle, more = spiral)
	private int create_line(WidgetCanvas canvas, int order, float x, float y, float radius, int num, float angle)
	{
		int line = canvas.AddLine(order);
		for (int i = 0; i <= num; i++)
		{
			// Generate points around a circle/spiral
			float s = (float)Math.Sin(angle / num * MathLib.DEG2RAD * i) * radius + x;
			float c = (float)Math.Cos(angle / num * MathLib.DEG2RAD * i) * radius + y;
			canvas.AddLinePoint(line, new vec3(s, c, 0.0f));
		}
		return line;
	}

	// Creates a filled polygon with vertices arranged in a regular pattern.
	// num: number of sides, angle: angular span (360 = closed polygon)
	private int create_polygon(WidgetCanvas canvas, int order, float x, float y, float radius, int num, float angle)
	{
		int polygon = canvas.AddPolygon(order);
		for (int i = 0; i < num; i++)
		{
			float s = (float)Math.Sin(angle / num * MathLib.DEG2RAD * i) * radius + x;
			float c = (float)Math.Cos(angle / num * MathLib.DEG2RAD * i) * radius + y;
			canvas.AddPolygonPoint(polygon, new vec3(s, c, 0.0f));
		}
		return polygon;
	}

	// Creates a text element at the specified position.
	private int create_text(WidgetCanvas canvas, int order, float x, float y, string str)
	{
		int text = canvas.AddText(order);
		canvas.SetTextPosition(text, new vec2(x, y));
		canvas.SetTextText(text, str);
		return text;
	}
}
