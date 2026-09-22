// Demonstrates how to swap between two albedo textures on a material at runtime.
// The component loads two textures from file paths and alternates between them
// on a timer, creating a texture-switching animation effect. Useful for things
// like blinking displays, animated surfaces, or visual state changes.

using Unigine;

// Alternates between two albedo textures on a timed interval.
public partial class MaterialsAlbedoTexture : Component
{
	// Path to the first texture file (shown initially)
	[ParameterFile]
	public string firstTextureImage;

	// Path to the second texture file (swapped to after timer expires)
	[ParameterFile]
	public string secondTextureImage;

	// How long each texture is displayed before switching
	public float time = 5.0f;

	// Cached reference to the object's material
	private Material material = null;
	// Tracks elapsed time for texture switching
	private float currentTime = 0.0f;
	// Direction of time accumulation (1 = forward, -1 = backward for ping-pong)
	private float timeSign = 1.0f;

	// Runtime texture objects loaded from file paths
	private Texture firstTexture = null;
	private Texture secondTexture = null;

	// Which of the two textures is applied right now (read by MaterialsParametersAnimationSample)
	public bool IsFirstTextureApplied => timeSign > 0;

	// Loads both textures and applies the first one to the material's albedo slot.
	private void Init()
	{
		Unigine.Object obj = node as Unigine.Object;
		if (!obj)
			return;

		if (obj.NumSurfaces != 0)
			material = obj.GetMaterial(0);

		if (!material)
			return;

		// Load texture files into GPU-ready texture objects
		firstTexture = new Texture();
		firstTexture.Load(firstTextureImage);

		secondTexture = new Texture();
		secondTexture.Load(secondTextureImage);

		// Start with the first texture visible
		material.SetTexture("albedo", firstTexture);
	}

	// Swaps the albedo texture when the timer reaches the threshold.
	private void Update()
	{
		if (!material)
			return;

		// Accumulate time in current direction
		currentTime += Game.IFps * timeSign;
		if (currentTime < 0 || time < currentTime)
		{
			// Swap to the other texture based on direction
			if (timeSign > 0)
				material.SetTexture("albedo", secondTexture);
			else
				material.SetTexture("albedo", firstTexture);

			// Reverse direction for ping-pong effect
			timeSign = -timeSign;
		}
	}
}
