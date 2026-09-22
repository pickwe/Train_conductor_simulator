# Navigation Sectors 3D

This sample demonstrates how to calculate and visualize 3D navigation paths using the **Navigation Sector** objects and *PathRoute* class via the C# API. Unlike navigation meshes, sectors allow defining modular navigable areas that can be enabled, disabled, or moved dynamically at runtime.

This setup is useful for multilevel structures or modular environments where the layout changes dynamically. For simpler 2D navigation scenarios, see the *Navigation Sectors 2D* sample.

The main logic is implemented in the **PathRoute3D** component, which creates a *PathRoute* class instance and uses *Create3D()* to compute a path through the active navigation sectors. If the route is successfully resolved, the path is drawn on the screen using *RenderVisualizer()*. The radius parameter is set manually to ensure the generated path accounts for the agent's size, avoiding collisions with nearby geometry.

To help visualize active navigation areas, the **NavigationSectorVisualizer** component renders the geometry of all sectors during runtime.

Sectors are especially useful when navigation space needs to change at runtime - for example, if parts of the environment become inaccessible. Pathfinding requires sectors to be properly connected (their edges must overlap).