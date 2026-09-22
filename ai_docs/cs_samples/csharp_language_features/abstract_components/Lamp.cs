// Concrete implementation of Toggleable that controls material emission.
// When toggled on, sets emission_color on all surfaces to make object glow.
// When toggled off, sets emission to zero (dark). Demonstrates material
// parameter manipulation and the ParameterColor attribute for editor UI.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Glowing lamp component that can be toggled on/off via Toggler clicks.
public partial class Lamp : Toggleable
{
	// Emission color when lamp is on - color picker shown in editor
	[ParameterColor]
	public vec4 emission_color = vec4.WHITE;

	// Called when lamp is toggled on - applies emission color.
	protected override bool On()
	{
		Log.MessageLine("Lamp::On()");
		return SetEmissionColor(emission_color);
	}

	// Called when lamp is toggled off - removes emission (dark).
	protected override bool Off()
	{
		Log.MessageLine("Lamp::Off()");
		return SetEmissionColor(vec4.ZERO);
	}

	// Applies emission color to all surfaces of the object's material.
	private bool SetEmissionColor(vec4 emission_color)
	{
		Object obj = (Object)node;

		if (obj == null)
			return false;

		// Apply to all surfaces (multi-surface meshes)
		for (var surface = 0; surface < obj.NumSurfaces; surface += 1)
			obj.SetMaterialParameterFloat4("emission_color", emission_color, surface);

		return true;
	}

	// Sets initial emission based on editor toggle state.
	private void Init()
	{
		SetEmissionColor(Toggled ? emission_color : vec4.ZERO);
	}
}
