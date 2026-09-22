// Demonstrates SoundReverb for adding spatial reverb effects to sounds.
// Creates a reverb zone where sounds passing through get echo/reflection
// effects. The reverb parameters (density, diffusion, decay, reflections)
// can be adjusted to simulate different acoustic environments.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

using Unigine;

// Sample for creating and controlling a SoundReverb zone with adjustable parameters.
public partial class SoundReverbController : Component
{
	// Sound source that will be affected by the reverb zone
	public SoundSource soundSource = null;

	// Reverb zone object - sounds within its bounds get reverb applied
	private SoundReverb reverb = null;
	// Master control affecting multiple reverb parameters at once
	private float reverbPower = 0.5f;

	SampleDescriptionWindow window = null;

	// Creates reverb zone with specified size and threshold for blending.
	private void Init()
	{
		// Create 20x20x20 reverb zone at origin
		reverb = new SoundReverb(new vec3(20.0f, 20.0f, 20.0f));
		reverb.WorldTransform = Mat4.IDENTITY;
		// Threshold defines the inner zone where reverb is at full strength
		reverb.Threshold = new vec3(10.0f, 10.0f, 10.0f);

		UpdateReverbSettings();

		Visualizer.Enabled = true;

		window = new SampleDescriptionWindow();
		window.createWindow();

		window.addFloatParameter("Gain:", "Gain", reverbPower, 0.0f, 1.0f, (float val) =>
		{
			reverbPower = val;
			UpdateReverbSettings();
		});
	}

	// Renders visualizers for both reverb zone and sound source bounds.
	private void Update()
	{
		if (!reverb || !soundSource)
			return;

		// Show reverb zone and sound source bounds for debugging
		reverb.RenderVisualizer();
		soundSource.RenderVisualizer();
	}

	// Cleans up UI and visualization.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
		window.shutdown();
	}

	// Calculates reverb parameters from single power value for easy control.
	private void UpdateReverbSettings()
	{
		// Low power = dry sound, high power = wet/cavernous reverb
		reverb.Density = MathLib.Clamp(1.0f - reverbPower, 0.0f, 1.0f);
		reverb.Diffusion = MathLib.Clamp(1.0f - reverbPower, 0.0f, 1.0f);
		// Decay time: how long reflections last (0.1s to 20s based on power)
		reverb.DecayTime = MathLib.Clamp(0.1f + 19.9f * reverbPower, 0.1f, 20.0f);
		// Early reflections gain - first bounces
		reverb.ReflectionGain = MathLib.Clamp(3.16f * reverbPower, 0.0f, 2.16f);
		// Late reverb gain - diffuse tail of the reverb
		reverb.LateReverbGain = MathLib.Clamp(10.0f * reverbPower, 0.0f, 10.0f);
	}

}
