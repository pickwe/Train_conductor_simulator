# Navigation Sectors 2D

This sample demonstrates how to calculate and visualize 2D navigation paths using the **Navigation Sector** objects and *PathRoute* class. Unlike navigation meshes, sectors allow defining modular navigable areas that can be enabled, disabled, or moved dynamically at runtime.

This 2D version is well-suited for top-down navigation, grid-based layouts, or layered 2D gameplay. For more complex 3D navigation scenarios, see the *Navigation Sectors 3D* sample.

The main logic is implemented in the **PathRoute2D** component, which creates a *PathRoute* class instance and uses *Create2D()* to compute a path through the active navigation sectors. If the route is successfully resolved, the path is drawn on screen using *RenderVisualizer()*. The radius parameter is set manually to ensure the generated path accounts for the agent's size, avoiding collisions with nearby geometry.

To help visualize active navigation areas, the **NavigationSectorVisualizer** component renders the geometry of all sectors during runtime.

Sectors are especially useful when navigation space needs to change at runtime - for example, if parts of the environment become inaccessible. Pathfinding requires sectors to be properly connected (their edges must overlap).