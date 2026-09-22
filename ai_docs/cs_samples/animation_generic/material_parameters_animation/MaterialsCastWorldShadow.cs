// Toggles whether a material casts shadows in the world at regular intervals.
// When CastWorldShadow is disabled, the object becomes invisible to shadow
// calculations, which can be useful for performance optimization or visual
// effects where an object shouldn't cast shadows.

using Unigine;

// Periodically toggles shadow casting on/off for the material.
public partial class MaterialsCastWorldShadow : Component
{
	// Interval between shadow toggle in seconds
	public float time = 5.0f;

	// Cached reference to the object's material
	private Material material = null;
	// Tracks elapsed time for shadow state switching
	private float currentTime = 0.0f;
	// Direction of time accumulation for ping-pong timing
	private float timeSign = 1.0f;

	// Gets material reference from the attached object.
	private void Init()
	{
		Unigine.Object obj = node as Unigine.Object;
		if (!obj)
			return;

		if (obj.NumSurfaces != 0)
			material = obj.GetMaterial(0);
	}

	// Toggles shadow casting when timer threshold is reached.
	private void Update()
	{
		if (!material)
			return;

		currentTime += Game.IFps * timeSign;
		if (currentTime < 0 || time < currentTime)
		{
			// Flip shadow casting state on/off
			material.CastWorldShadow = !material.CastWorldShadow;

			timeSign = -timeSign;
		}
	}
}
