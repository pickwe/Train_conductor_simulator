// Implements ease-in motion with constant acceleration. Object starts slow and
// gradually speeds up as it moves toward target. Uses fixed timestep physics
// with accumulator pattern for frame-rate independent movement.

using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Ease-in motion: gradual acceleration from rest.
// Starts slow and speeds up as it approaches the target.
// Uses fixed timestep physics simulation with render interpolation
// for frame-rate independent behavior.
public partial class EaseInMotion : MotionMode
{
	[ShowInEditor]
	public float physicsFPS = 60.0f;		// Fixed physics timestep
	[ShowInEditor]
	public float acceleration = 3.0f;		// Acceleration rate

	private float physicsIFps = 1.0f / 60.0f;
	// Dynamic variables
	private float velocity = 0.0f;
	// Starting drift
	private float driftVelocity = 4.0f;
	// Persecutor has reached the target
	private bool finished = false;

	// Counter for the next physical frame
	private float accumulator = 0.0f;

	// Target position
	private Vec3 previousInputPosition;
	private Vec3 currentInputPosition;
	// Spring positions in physical frame
	private Vec3 previousStepPosition;
	private Vec3 currentStepPosition;

	// Base class is initialized and physics timestep is configured.
	protected override void Init()
	{
		base.Init();
		physicsIFps = 1.0f / physicsFPS;
	}

	// Persecutor position is updated using ease-in physics and animation is applied.
	private void Update()
	{
		if (!ready)
			return;

		Vec3 position = persecutor.GetPosition();
		Vec3 targetPosition = targetNode.WorldPosition;
		// Align target to persecutor's plane
		targetPosition.z = position.z;

		// Calculate persecutor's new position and direction
		position = SimulatePhysics(position, targetPosition, out Vec3 targetDirection);

		// Apply calculations to persecutor
		if (!finished)
			persecutor.SetRotation(targetDirection);

		persecutor.SetPosition(position);

		float speed = MathLib.Max(driftVelocity, velocity);
		persecutor.SetAnimation(speed, finished);
	}

	// Called when component is activated. Position and velocity are reset.
	protected override void OnEnable()
	{
		if (!ready)
			return;

		currentStepPosition = persecutor.GetPosition();
		velocity = 0.0f;
		previousStepPosition = currentStepPosition;
	}

	// Fixed timestep physics simulation is executed with accumulator pattern.
	// "direction" points from source to destination.
	private Vec3 SimulatePhysics(Vec3 source, Vec3 destination, out Vec3 direction)
	{
		direction = Vec3.ZERO;

		// Find the distance and direction to the destination point
		Vec3 offset = destination - source;
		float distance = (float)MathLib.Length(offset);
		if (distance > 0)
			direction = offset / distance;

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

			// Update physics
			int steps = (int)MathLib.Floor(accumulator / physicsIFps);
			for (int i = 0; i < steps; i++)
			{
				// Interpolate input for each physical frame: (0, 1]
				float alphaIn = (float)(i + 1) / steps;

				previousStepPosition = currentStepPosition;
				currentStepPosition = MoveTowards(currentStepPosition,
					MathLib.Lerp(previousInputPosition, currentInputPosition, alphaIn), physicsIFps);
			}
			// Reset counter
			accumulator -= steps * physicsIFps;
		}

		// Linearly interpolated position is returned for smooth rendering
		return MathLib.Lerp(previousStepPosition, currentStepPosition, accumulator / physicsIFps);
	}

	// Single physics step with constant acceleration is performed.
	private Vec3 MoveTowards(Vec3 source, Vec3 destination, float ifps)
	{
		// Constant acceleration: velocity increases linearly with time
		velocity += acceleration * ifps;
		float step = velocity * ifps;

		Vec3 offset = destination - source;
		float distance = (float)MathLib.Length(offset);

		// Snap to destination if close enough
		if (distance <= step)
		{
			finished = true;
			velocity = 0.0f;
			return destination;
		}

		finished = false;
		return source + offset / distance * step;
	}
}
