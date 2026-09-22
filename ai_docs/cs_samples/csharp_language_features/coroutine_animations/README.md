# Coroutine Animations

This sample demonstrates how to implement application logic to be executed across multiple frames using coroutines, their execution can be suspended either by the Engine or manually, and then resumed. Here coroutines are used to create non-blocking, time-based targeted animations on a node. Smooth movement, continuous rotation, and material blinking effects are implemented using coroutine control flow (**StartCoroutine, yield return, StopCoroutine**). The sample also shows how to manage multiple coroutines simultaneously and stop them selectively.

You can control coroutine-based animations using a simple GUI which provides a practical way to explore and understand coroutine-driven behavior.