// Click handler that toggles Toggleable objects under the mouse cursor.
// Demonstrates polymorphism: any object with a Toggleable component (Fan, Lamp,
// or custom implementations) can be toggled with a single click. Uses ray
// intersection with configurable mask to find clicked objects.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Player component that handles click-to-toggle interaction with Toggleable objects.
public partial class Toggler : Component
{
	// Mask to filter which objects can be toggled (set on object's intersection mask)
	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	public int interaction_intersection_mask = 1 << 16;

	// Casts ray from camera on click and toggles any Toggleable under cursor.
	private void Update()
	{
		if (!Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT))
			return;

		ivec2 mouse = Input.MousePosition;
		// This component must be attached to a Player (camera)
		Player player = (Player) node;

		if (player == null)
			return;

		// Get world-space direction from camera through mouse position
		vec3 direction = player.GetDirectionFromMainWindow(mouse.x, mouse.y);

		// Cast ray from camera position to far plane
		vec3 p0 = new vec3(player.WorldPosition);
		vec3 p1 = p0 + direction * player.ZFar;

		// Find first object hit that matches our interaction mask
		Object obj = World.GetIntersection(p0, p1, interaction_intersection_mask);

		if (obj)
		{
			// Polymorphic call - works with any Toggleable subclass
			var toggleable = obj.GetComponent<Toggleable>();
			if (toggleable)
			{
				toggleable.Toggle();
			}
		}
	}

	// Enables on-screen console to show toggle messages.
	private void Init()
	{
		consoleOnScreen = Console.Onscreen;
		Console.Onscreen = true;
	}

	// Restores original console setting.
	private void Shutdown()
	{
		Console.Onscreen = consoleOnScreen;
	}

	// Saved console state for restoration
	private bool consoleOnScreen = false;
}
