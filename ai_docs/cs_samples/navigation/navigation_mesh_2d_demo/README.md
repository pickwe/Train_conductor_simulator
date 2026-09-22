# Navigation Mesh 2D Demo

This sample demonstrates how to implement dynamic 2D pathfinding using a **Navigation Mesh** object, with autonomous robots navigating toward randomly positioned targets. Each robot uses a *PathRoute* class instance to calculate a valid route within the navigation mesh and moves along it in real time.

This setup is useful for prototyping simple AI behavior such as patrolling or target chasing, where agents continuously search for and move toward dynamic goals.

The main logic is implemented in the **PathRoute2DWithTarget** component. On initialization, it creates a coin from a **Node Reference** and places it at a random valid location inside the Navigation Mesh. A *PathRoute* instance is initialized with a defined agent radius to ensure the generated path accounts for the agent's size, avoiding collisions with nearby geometry.

At runtime, the component continuously checks whether the robot is near its current target. If so, the target is moved to a new valid position, and a new path is generated using *Create2D()*. If the route is valid, the path is drawn on screen using *RenderVisualizer()* and the robot moves forward along it and smoothly adjusts its orientation using quaternion interpolation.

The sample includes a second component, **NavigationMeshVisualizer**, that renders the actual mesh structure during runtime to help visualize navigable areas.