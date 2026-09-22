# Navigation Obstacles 2D

This sample demonstrates how to use dynamic **Obstacles** in combination with a **Navigation Mesh** to influence 2D pathfinding in runtime. When an obstacle overlaps the navigation mesh, it temporarily modifies the traversable area, forcing the pathfinding algorithm to recalculate a valid route around it.

This example is useful for prototyping interactive environments, where navigation must adapt to moving objects, barriers, or other gameplay elements affecting traversal.

The main logic is implemented in the **PathRoute2D** component, which initializes a *PathRoute* class instance and uses *PathRoute.Create2D()* to compute the path from a start node to a finish node. If the route is successfully resolved, the path is drawn on screen using *RenderVisualizer()*. The radius parameter is set manually to ensure the generated path accounts for the navigation agent's size, avoiding collisions with nearby geometry.

To demonstrate dynamic behavior, several **Obstacles** are moved using the **Rotator** component, which continuously rotates them during runtime. This setup allows you to observe how the path is recalculated when obstacles shift position and alter the available navigation space.