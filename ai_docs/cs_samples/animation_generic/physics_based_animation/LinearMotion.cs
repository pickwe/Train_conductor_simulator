// Implements constant-speed linear motion toward target. The simplest motion
// type: moves at fixed speed regardless of distance, snapping to destination
// when within one step distance.

using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Linear motion: constant speed movement towards target.
// Simplest easing function - no acceleration or deceleration.
// Movement stops abruptly when target is reached.
public partial class LinearMotion : MotionMode
{
	[ShowInEditor]
	public float speed = 2.0f;	// Units per second

	// Persecutor position is updated using constant-speed linear interpolation.
	private void Update()
	{
		if (!ready)
			return;

		Vec3 currentPosition = persecutor.GetPosition();
		Vec3 targetPosition = targetNode.WorldPosition;
		// Align target to persecutor's plane
		targetPosition.z = currentPosition.z;

		float step = speed * Game.IFps;
		currentPosition = MoveTowards(currentPosition, targetPosition, step, out bool finished, out Vec3 targetDirection);

		// Apply calculations to persecutor
		if (!finished)
			persecutor.SetRotation(targetDirection);

		persecutor.SetPosition(currentPosition);
		persecutor.SetAnimation(speed, finished);
	}

	// New position is calculated by moving at constant speed toward target.
	// "finished" tells the movement has stopped, "direction" points from source to destination.
	private static Vec3 MoveTowards(Vec3 source, Vec3 destination, float step, out bool finished, out Vec3 direction)
	{
		direction = Vec3.ZERO;

		// Linear interpolation: move at constant speed toward target
		Vec3 offset = destination - source;
		float distance = (float)MathLib.Length(offset);

		// Snap to destination if within one step distance
		if (distance <= step)
		{
			finished = true;
			if (distance > 0)
				direction = offset / distance;
			return destination;
		}

		// Move exactly 'step' units toward target
		finished = false;
		direction = offset / distance;
		return source + direction * step;
	}
}
