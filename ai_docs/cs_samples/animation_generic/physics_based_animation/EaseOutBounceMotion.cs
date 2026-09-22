// Extends SpringMotion with bounce behavior when crossing the target. When the
// chaser crosses an imaginary "wall" near the target (within bounceRadius), its
// velocity is reversed to create an elastic bounce-back effect.

using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Extends spring motion with bounce effect when approaching the target.
// When the chaser enters the bounce radius and is moving toward the target,
// velocity is reversed to create a "bounce off wall" effect.
// Useful for playful or exaggerated motion aesthetics.
public partial class EaseOutBounceMotion : SpringRegular
{
	[ShowInEditor]
	public float bounceRadius = 3.0f;

	// Medium dynamic variables for position
	private Vec3 newPosition;
	// Bounce normal
	private Vec3 wallNormal;

	// Spring physics with bounce detection is executed. Velocity is reversed when crossing the wall plane.
	// "reachedTarget" tells the movement has stopped, "direction" points from source to destination.
	protected override Vec3 SimulatePhysics(Vec3 source, Vec3 destination, out bool reachedTarget, out Vec3 direction)
	{
		direction = Vec3.ZERO;

		// Find distance and direction to destination
		Vec3 offset = destination - source;
		float distance = (float)MathLib.Length(offset);
		if (distance > 0)
			direction = offset / distance;

		reachedTarget = distance < DISTANCE_EPSILON;

		float ifps = Game.IFps;
		// Avoid "spiral of death"
		ifps = MathLib.Min(ifps, 0.25f);

		accumulator += ifps;
		// Check if we've reached next physical frame
		if (accumulator >= physicsIFps)
		{
			// Update input
			previousInputPosition = currentInputPosition;
			currentInputPosition = destination;

			// Target moved
			if (previousInputPosition != currentInputPosition)
			{
				// Calculate target normal
				wallNormal = MathLib.Normalize(previousInputPosition - currentInputPosition);
				if (MathLib.Dot(MathLib.Normalize(position - currentInputPosition), wallNormal) < 0)
					wallNormal = -wallNormal;
			}

			// Update physics
			int steps = (int)MathLib.Floor(accumulator / physicsIFps);
			for (int i = 0; i < steps; i++)
			{
				// Interpolate input for each physical frame: (0, 1]
				float alphaIn = (float)(i + 1) / steps;
				Vec3 targetPosition = MathLib.Lerp(previousInputPosition, currentInputPosition, alphaIn);

				previousOutputPosition = currentOutputPosition;
				IntegrateSpring(targetPosition, physicsIFps);

				// Bounce detection: when chaser crosses the "wall" plane near the target
				Vec3 directionFromTarget = MathLib.Normalize(position - targetPosition);
				if (MathLib.Dot(directionFromTarget, wallNormal) < 0.0
					&& MathLib.Length2(position - targetPosition) < bounceRadius * bounceRadius)
					velocity = -velocity;	// Reverse velocity to create bounce effect

				// Recalculate position after applying the bounce effect
				newPosition += velocity * physicsIFps;
				position = newPosition;

				currentOutputPosition = position;
			}
			// Reset counter
			accumulator -= steps * physicsIFps;
		}

		// Interpolate output position
		float alphaOut = accumulator / physicsIFps;
		return MathLib.Lerp(previousOutputPosition, currentOutputPosition, alphaOut);
	}

	// Called when component is activated. Parent spring state and bounce position are reset.
	protected override void OnEnable()
	{
		base.OnEnable();
		newPosition = position;
	}
}
