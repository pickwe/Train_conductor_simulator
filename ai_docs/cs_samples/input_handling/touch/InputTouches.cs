// Tracks multi-touch input positions. Monitors up to NUM_TOUCHES simultaneous
// touch points, storing their screen coordinates. Uses -1,-1 to indicate
// inactive touch points.

using System.Collections.Generic;
using Unigine;

// Monitors multi-touch input positions.
public partial class InputTouches : Component
{
	// Array of touch positions (-1,-1 when inactive)
	public ivec2[] TouchesPositions { get; private set; } = new ivec2[Input.NUM_TOUCHES];

	// All touch positions are initialized to inactive state.
	private void Init()
	{
		for (int i = 0; i < Input.NUM_TOUCHES; i++)
			TouchesPositions[i] = -ivec2.ONE;
	}

	// Touch states are updated each frame.
	private void Update()
	{
		for (int i = 0; i < Input.NUM_TOUCHES; i++)
		{
			// Update position when touch is active
			if (Input.IsTouchPressed(i))
				TouchesPositions[i] = Input.GetTouchPosition(i);

			// Reset to inactive when touch is released
			if (Input.IsTouchUp(i))
				TouchesPositions[i] = -ivec2.ONE;
		}
	}
}
