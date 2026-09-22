// Spring variant using intuitive parameters (duration, dampingRatio) instead of
// raw physics values. Converts to stiffness/damping using damped harmonic oscillator
// settling time formulas for predictable animation timing.

using Unigine;

// Simplified spring configuration using intuitive parameters.
// Instead of raw stiffness/damping values, uses:
// - duration: time for spring to settle
// - dampingRatio: oscillation amount (0.01-1, lower = more bouncy)
// Stiffness and damping are computed automatically from these values.
public partial class SpringEasy : SpringMotion
{
	[ShowInEditor]
	public float duration = 6.0f;
	// Damping ratio [0.01, 1]
	[ShowInEditor]
	public float dampingRatio = 0.6f;
	// A spring never gets to 0 amplitude, it gets infinitely smaller.
	// This fraction represents the perceived 0 point.
	[ShowInEditor]
	public float fractionOfAmplitude = 1500.0f;

	// Stiffness and damping are calculated from intuitive parameters using harmonic oscillator formulas.
	protected override void RefreshSpringSettings()
	{
		// Convert intuitive parameters (duration, dampingRatio) to physics parameters.
		// Derived from the damped harmonic oscillator settling time formula.
		float fractionLog = MathLib.Log(fractionOfAmplitude);
		stiffness = MathLib.Pow(fractionLog, 2.0f) / (MathLib.Pow(duration, 2.0f) * MathLib.Pow(dampingRatio, 2.0f));
		// Critical damping coefficient: 2 * sqrt(k) * zeta
		damping = 2.0f * MathLib.Sqrt(stiffness) * dampingRatio;
	}
}
