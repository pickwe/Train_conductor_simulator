// Implements exponential ease-out motion where the object decelerates as it
// approaches the target. Uses exp(-speed * dt) for asymptotic approach that
// covers most distance quickly then slows down smoothly near the destination.

using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Ease-out motion: gradual deceleration towards target.
// Starts fast and slows down as it approaches the destination.
// Uses exponential interpolation (lerp with exp decay) for smooth stopping.
public partial class EaseOutMotion : MotionMode
{
	[ShowInEditor]
	public float speed = 2.0f;

	// Max distance to the target to catch
	private const float DISTANCE_EPSILON = 0.5f;

	// Persecutor position is updated using exponential ease-out interpolation.
	private void Update()
	{
		if (!ready)
			return;

		Vec3 currentPosition = persecutor.GetPosition();
		Vec3 previousPosition = currentPosition;
		Vec3 targetPosition = targetNode.WorldPosition;
		// Align target to persecutor's plane
		targetPosition.z = currentPosition.z;

		// Calculate persecutor's new position and direction
		currentPosition = MoveTowards(currentPosition, targetPosition, speed, out bool finished, out Vec3 targetDirection);

		// Apply calculations to persecutor
		if (!finished)
			persecutor.SetRotation(targetDirection);

		persecutor.SetPosition(currentPosition);

		float realSpeed = (float)MathLib.Length(currentPosition - previousPosition) / Game.IFps;
		persecutor.SetAnimation(realSpeed, finished);
	}

	// Exponential interpolation toward target is calculated. Distance decreases asymptotically.
	// "finished" tells the movement has stopped, "direction" points from source to destination.
	private static Vec3 MoveTowards(Vec3 source, Vec3 destination, float speed, out bool finished, out Vec3 direction)
	{
		direction = Vec3.ZERO;

		Vec3 offset = destination - source;
		float distance = (float)MathLib.Length(offset);
		if (distance > 0)
			direction = offset / distance;

		finished = distance < DISTANCE_EPSILON;

		// Exponential ease-out: covers most distance quickly, then slows down.
		// The exp(-speed * dt) term creates asymptotic approach to target.
		return MathLib.Lerp(source, destination, 1.0f - MathLib.Exp(-speed * Game.IFps));
	}
}
