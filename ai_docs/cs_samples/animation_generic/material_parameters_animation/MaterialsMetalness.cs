// Animates a material's metalness parameter between two values over time.
// Metalness controls how metallic a surface appears: 0 = dielectric (plastic, wood),
// 1 = full metal (steel, gold). This creates a visual transformation effect
// as the surface transitions between non-metallic and metallic appearance.

using Unigine;

// Smoothly transitions metalness between two values in a ping-pong pattern.
public partial class MaterialsMetalness : Component
{
	// Starting metalness value (0 = non-metallic)
	public float beginMetalness = 0.0f;
	// Target metalness value (1 = fully metallic)
	public float endMetalness = 1.0f;
	// Duration of one transition cycle in seconds
	public float time = 5.0f;

	// Cached reference to the object's material
	private Material material = null;
	// Current position in the animation timeline
	private float currentTime = 0.0f;
	// Animation direction (1 = toward end, -1 = toward begin)
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

	// Interpolates metalness each frame based on animation progress.
	private void Update()
	{
		if (!material)
			return;

		// Advance animation time, reverse direction at boundaries
		currentTime += Game.IFps * timeSign;
		if (currentTime < 0 || time < currentTime)
			timeSign = -timeSign;

		// Calculate interpolation factor and set metalness parameter
		material.SetParameterFloat("metalness", MathLib.Lerp(beginMetalness, endMetalness, MathLib.Saturate(currentTime / time)));
	}
}
