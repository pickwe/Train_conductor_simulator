// Concrete implementation of Toggleable that rotates a node like a fan.
// When toggled on, smoothly accelerates to target speed using Lerp.
// When toggled off, smoothly decelerates to zero. Demonstrates
// how abstract component classes enable polymorphic click handling.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Rotating fan component that can be toggled on/off via Toggler clicks.
public partial class Fan : Toggleable
{
	// Target rotation speed in degrees per second when enabled
	public float rotation_speed = 120;

	// Speed we're lerping toward (0 or rotation_speed)
	private float target_speed = 0;
	// Current interpolated speed for smooth acceleration/deceleration
	private float actual_speed = 0;

	// Called when fan is toggled on - sets target speed to start spinning.
	protected override bool On()
	{
		Log.MessageLine("Fan::On()");
		target_speed = rotation_speed;
		return true;
	}

	// Called when fan is toggled off - sets target to zero to stop.
	protected override bool Off()
	{
		Log.MessageLine("Fan::Off()");
		target_speed = 0;
		return true;
	}

	// Sets initial speed based on editor toggle state.
	private void Init()
	{
		target_speed = Toggled ? rotation_speed : 0;
	}

	// Smoothly interpolates rotation speed and applies rotation each frame.
	private void Update()
	{
		// Lerp creates smooth acceleration/deceleration curve
		actual_speed = MathLib.Lerp(actual_speed, target_speed, Game.IFps);
		// Rotate around Z axis (fan blade spin)
		node.Rotate(0, 0, actual_speed * Game.IFps);
	}
}
