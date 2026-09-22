// Implements spring-based motion with fixed timestep physics simulation.
// Uses accumulator pattern for frame-rate independent physics and interpolates
// between physics steps for smooth rendering. Spring follows Hooke's law.

using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Spring-based motion using damped harmonic oscillator physics.
// Creates natural, organic-feeling movement with overshoot and settle behavior.
// Base class for SpringEasy (simple) and SpringRegular (configurable) variants.
// Uses Hooke's law: F = -kx - cv (stiffness and damping forces).
public abstract partial class SpringMotion : MotionMode
{
	[ShowInEditor]
	public float physicsFPS = 60.0f;	// Fixed physics timestep for stability
	[ShowInEditor]
	public float mass = 0.75f;			// Affects oscillation frequency

	protected float physicsIFps = 1.0f / 60.0f;

	protected bool finished = false;

	// Spring parameters: stiffness (k) and damping (c) coefficients
	protected float stiffness = 1.0f;	// Higher = faster snap to target
	protected float damping = 1.0f;		// Higher = less oscillation

	// Fixed timestep accumulator for physics simulation
	protected float accumulator = 0.0f;

	// Spring state variables
	protected Vec3 position;
	protected Vec3 velocity;

	// Target position interpolation (for smooth target movement)
	protected Vec3 previousInputPosition;
	protected Vec3 currentInputPosition;
	// Output position interpolation (for smooth rendering between physics steps)
	protected Vec3 previousOutputPosition;
	protected Vec3 currentOutputPosition;

	protected const float DISTANCE_EPSILON = 0.5f;	// Threshold for "caught" detection

	// Base class and spring settings are initialized.
	protected override void Init()
	{
		base.Init();
		physicsIFps = 1.0f / physicsFPS;
		RefreshSpringSettings();
	}

	// Persecutor position is updated using spring physics and animation is applied.
	protected void Update()
	{
		if (!ready)
			return;

		Vec3 currentPosition = persecutor.GetPosition();
		Vec3 previousPosition = currentPosition;
		Vec3 targetPosition = targetNode.WorldPosition;
		// Align target to persecutor's plane
		targetPosition.z = currentPosition.z;

		// Calculate persecutor's new position and direction
		currentPosition = SimulatePhysics(currentPosition, targetPosition, out finished, out Vec3 targetDirection);

		// Apply calculations to persecutor
		if (!finished)
			persecutor.SetRotation(targetDirection);

		persecutor.SetPosition(currentPosition);

		float realSpeed = (float)MathLib.Length(currentPosition - previousPosition) / Game.IFps;
		persecutor.SetAnimation(realSpeed, finished);
	}

	// Called when component is activated. Spring state is reset to current position.
	protected override void OnEnable()
	{
		if (!ready)
			return;

		// Refresh persecutor's position to continue movement from its last point
		currentOutputPosition = persecutor.GetPosition();
		previousOutputPosition = currentOutputPosition;
		position = currentOutputPosition;
		velocity = Vec3.ZERO;
	}

	// Fixed timestep spring physics is simulated with accumulator pattern.
	protected virtual Vec3 SimulatePhysics(Vec3 source, Vec3 destination, out bool reachedTarget, out Vec3 direction)
	{
		direction = Vec3.ZERO;

		Vec3 offset = destination - source;
		float distance = (float)MathLib.Length(offset);
		if (distance > 0)
			direction = offset / distance;

		reachedTarget = distance < DISTANCE_EPSILON;

		float ifps = Game.IFps;
		// Clamp delta time to avoid "spiral of death" on frame spikes
		ifps = MathLib.Min(ifps, 0.25f);

		// Fixed timestep accumulator pattern for deterministic physics
		accumulator += ifps;
		if (accumulator >= physicsIFps)
		{
			// Store previous and current target for input interpolation
			previousInputPosition = currentInputPosition;
			currentInputPosition = destination;

			// Run multiple physics steps if needed (catch-up)
			int steps = (int)MathLib.Floor(accumulator / physicsIFps);
			for (int i = 0; i < steps; i++)
			{
				// Interpolate target position for sub-frame accuracy
				float alphaIn = (float)(i + 1) / steps;

				previousOutputPosition = currentOutputPosition;
				IntegrateSpring(MathLib.Lerp(previousInputPosition, currentInputPosition, alphaIn), physicsIFps);
				currentOutputPosition = position;
			}
			accumulator -= steps * physicsIFps;
		}

		// Interpolate between physics frames for smooth rendering
		float alphaOut = accumulator / physicsIFps;
		return MathLib.Lerp(previousOutputPosition, currentOutputPosition, alphaOut);
	}

	// Single spring physics step is integrated using Hooke's law and damping.
	protected void IntegrateSpring(Vec3 targetPosition, float ifps)
	{
		// Hooke's law: F_spring = -k * x (displacement from target)
		Vec3 forceSpring = (targetPosition - position) * stiffness;
		// Damping force: F_damping = -c * v (opposes velocity)
		Vec3 forceDamping = -velocity * damping;
		// Newton's second law: a = F/m
		Vec3 acceleration = (forceSpring + forceDamping) / mass;

		// Semi-implicit Euler integration (symplectic): more stable than explicit Euler
		velocity += acceleration * ifps;
		position += velocity * ifps;
	}

	// Derived classes must implement to set stiffness/damping values
	protected abstract void RefreshSpringSettings();
}
