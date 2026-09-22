// Abstract base class for objects that can be toggled on/off.
// Demonstrates C# abstract classes in UNIGINE's component system.
// Derived classes implement On() and Off() for specific toggle behavior.
// Used with Toggler component for click-based interaction.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Base class providing toggle state management and abstract on/off methods.
public abstract partial class Toggleable : Component
{
	// Current toggle state - can be set in editor for initial value
	[ShowInEditor]
	private bool isToggled = false;

	// Property with smart setter that only calls On/Off when state actually changes
	public bool Toggled
	{
		get => isToggled;
		set
		{
			if (value != isToggled)
			{
				// Call appropriate method and only update state if it succeeded
				bool ok = value ? On() : Off();
				isToggled = isToggled ^ ok;  // XOR flips state only if ok is true
			}
		}
	}

	// Convenience method to flip toggle state - returns new state
	public bool Toggle() => isToggled = isToggled ^ (isToggled ? Off() : On());

	// Derived classes implement what happens when toggled on
	protected abstract bool On();
	// Derived classes implement what happens when toggled off
	protected abstract bool Off();
}
