// Demonstrates UNIGINE's coroutine system for async-like animation control.
// Coroutines allow writing sequential animation code that executes across
// multiple frames. Shows starting/stopping coroutines by reference or name,
// WaitForSeconds delays, yielding to other coroutines, and infinite loops.

using System.Collections;
using System.Numerics;
using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Component demonstrating coroutine-based animation with rotation, movement, and blinking.
public partial class Animator : Component
{
	// Materials for alternating blink effect
	public Material materialOne;
	public Material materialTwo;

	// Starts the animation sequence toward target, stopping any previous animation.
	public void RunAnimation(Node targetNode)
	{
		// Cancel existing animations to prevent overlapping behavior
		StopAllCoroutines();

		// StartCoroutine begins execution - Animate() IEnumerator is stepped each frame
		StartCoroutine(Animate(targetNode));
	}

	// Immediately stops all running coroutines on this component.
	public void StopAnimation()
	{
		StopAllCoroutines();
	}

	// Coroutine methods return IEnumerator.
	// Use `yield break` to stop the current coroutine.
	// Use `yield return {yield instruction}` to pause/resume coroutine execution at specific points.
	private IEnumerator Animate(Node targetNode)
	{
		if (targetNode == null)
		{
			yield break; // Exit if the target node is null.
		}

		// Start coroutine by IEnumerator and store a reference to it.
		Coroutine coroutineRotateCube = StartCoroutine(Rotate(90f));

		// Start the Blink coroutine by method name. Arguments are passed as an array of objects
		StartCoroutine("Blink", [0.5f, 0.5f]);

		// Run the Move coroutine and wait for it to complete before continuing.
		yield return StartCoroutine(Move(targetNode.WorldPosition, 5f));

		// Stop the Blink coroutine by method name.
		// This only works for coroutines started using the method name approach.
		StopCoroutines("Blink");

		// Stop coroutine by `Coroutine` instance
		StopCoroutine(coroutineRotateCube);
	}

	// Infinite loop coroutine that alternates materials with configurable delays.
	private IEnumerator Blink(float delayOne, float delayTwo)
	{
		ObjectMeshStatic meshStatic = (ObjectMeshStatic)node;
		while (true)
		{
			meshStatic.SetMaterial(materialOne, 0);

			// Execution of the current coroutine will be paused here and then resume after delayOne seconds in
			// the next engine update
			yield return new WaitForSeconds(delayOne);

			meshStatic.SetMaterial(materialTwo, 0);
			// Pause execution for delayTwo seconds.
			// Using the Instance method instead of creating a new object avoids allocations by reusing a cached instruction.
			yield return WaitForSeconds.Instance(delayTwo);
		}
	}

	// Infinite loop coroutine that continuously rotates around Z axis.
	private IEnumerator Rotate(float speed)
	{
		while (true)
		{
			// Continuously rotate the node around the Z-axis based on the given speed and frame delta time.
			node.Rotate(0, 0, speed * Engine.IFps);
			// yield return null waits one frame before continuing
			yield return null;
		}
	}

	// Finite coroutine that moves from current position to target over duration.
	private IEnumerator Move(Vec3 endPosition, float duration)
	{
		Vec3 startPosition = node.WorldPosition;
		float t = 0;
		while (t < duration)
		{
			t += Engine.IFps;
			// Lerp creates smooth interpolation from start to end
			node.WorldPosition = MathLib.Lerp(startPosition, endPosition, t / duration);
			// Wait for the next update
			yield return null;
		}
	}
}
