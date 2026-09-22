// Smoothly animates a material's albedo color between two values over time.
// Uses linear interpolation to create a gradual color transition that ping-pongs
// back and forth. Perfect for breathing effects, status indicators, or any
// visual feedback that requires smooth color changes.

using Unigine;

// Animates albedo color smoothly between two colors in a ping-pong pattern.
public partial class MaterialsAlbedoColor : Component
{
	// Starting color (shown at time = 0)
	[ParameterColor]
	public vec4 beginColor = vec4.ZERO;

	// Target color (reached at time = duration)
	[ParameterColor]
	public vec4 endColor = vec4.ONE;

	// Duration of one color transition in seconds
	public float time = 5.0f;

	// Cached reference to the object's material
	private Material material = null;
	// Current position in the animation timeline
	private float currentTime = 0.0f;
	// Animation direction (1 = toward endColor, -1 = toward beginColor)
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

	// Interpolates color each frame based on animation progress.
	private void Update()
	{
		if (!material)
			return;

		// Advance animation time, reverse direction at boundaries
		currentTime += Game.IFps * timeSign;
		if (currentTime < 0 || time < currentTime)
			timeSign = -timeSign;

		// Calculate interpolation factor (0 to 1) and blend between colors
		material.SetParameterFloat4("albedo_color", MathLib.Lerp(beginColor, endColor, MathLib.Saturate(currentTime / time)));
	}
}
