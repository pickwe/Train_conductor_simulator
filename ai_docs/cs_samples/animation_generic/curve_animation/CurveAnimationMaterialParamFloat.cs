using System.Collections;
using System.Collections.Generic;
using Unigine;

// Animates material float parameters using curves for dynamic visual effects.
public partial class CurveAnimationMaterialParamFloat : Component
{
	// Configuration for a single animated material parameter
	[System.Serializable]
	public class MaterialParamCurve
	{
		// Curve defining parameter value over time
		[ShowInEditor, Parameter(Title = "Curve")]
		public Curve2d curve;

		// Name of the material parameter to animate
		[ShowInEditor, Parameter(Title = "Param Name")]
		public string name;

		// Surface index to apply parameter to
		[ShowInEditor, Parameter(Title = "Surface")]
		public int surface = 0;
	}

	// Array of material parameters to animate
	[ShowInEditor, Parameter(Title = "Parameters")]
	public MaterialParamCurve[] parameters = new MaterialParamCurve[0];

	// Playback speed multiplier
	[ShowInEditor, Parameter(Title = "Speed")]
	public float speed = 1.0f;

	// Accumulated animation time
	private float time = 0.0f;

	// Material parameters are updated from curve values each frame.
	void Update()
	{
		time += Game.IFps * speed;

		var obj = node as Object;
		if (obj == null)
			return;

		// Apply each curve value to its corresponding material parameter
		for (int i = 0; i < parameters.Length; i++)
		{
			var p = parameters[i];

			obj.SetMaterialParameterFloat(p.name, p.curve.Evaluate(time), p.surface);
		}
	}
}

