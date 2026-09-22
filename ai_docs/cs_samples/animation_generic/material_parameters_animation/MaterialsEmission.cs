// Toggles a material's emission state on and off at regular intervals.
// Emission makes objects appear to glow by adding self-illumination that isn't
// affected by scene lighting. This creates a blinking/pulsing light effect
// useful for indicators, warning lights, or glowing objects.

using Unigine;

// Periodically toggles material emission on/off for blinking effect.
public partial class MaterialsEmission : Component
{
	// Interval between emission state toggles in seconds
	public float time = 5.0f;

	// Cached reference to the object's material
	private Material material = null;
	// Tracks elapsed time for state switching
	private float currentTime = 0.0f;
	// Direction of time accumulation for ping-pong timing
	private float timeSign = 1.0f;

	// Gets material reference from the object this component is attached to.
	private void Init()
	{
		Unigine.Object obj = node as Unigine.Object;
		if (!obj)
			return;

		if (obj.NumSurfaces != 0)
			material = obj.GetMaterial(0);
	}

	// Toggles emission state when timer threshold is reached.
	private void Update()
	{
		if (!material)
			return;

		currentTime += Game.IFps * timeSign;
		if (currentTime < 0 || time < currentTime)
		{
			// Flip emission state: 0 becomes 1, 1 becomes 0
			material.SetState("emission", 1 - material.GetState("emission"));

			timeSign = -timeSign;
		}
	}
}
