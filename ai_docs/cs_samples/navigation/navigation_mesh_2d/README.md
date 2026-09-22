# Navigation Mesh 2D

This sample demonstrates how to calculate and visualize 2D navigation paths using the **Navigation Mesh** object and *PathRoute* class via the C# API. It shows how to build a route between two points on a navigation mesh and renders the result for debugging or visualization purposes.

This setup is useful for prototyping AI navigation, testing route validity, and analyzing the structure of navigable areas in 2D gameplay scenarios.

The main logic is implemented in the **PathRoute2D** component, which initializes a *PathRoute* class instance and uses *PathRoute.Create2D()* to compute the path from a start node to a finish node. If the route is successfully resolved, the path is drawn on screen using *RenderVisualizer()*. The radius parameter is set manually to ensure the generated path accounts for the navigation agent's size, avoiding collisions with nearby geometry.

The sample includes a second component, **NavigationMeshVisualizer**, that renders the **Navigation Mesh** during runtime to help visualize navigable areas.