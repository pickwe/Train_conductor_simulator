# Navigation Sectors 3D Demo

This sample demonstrates how to implement 3D pathfinding logic using **Navigation Sector** and *PathRoute* class via the C# API. Robots autonomously fly and collect coins, which are dynamically placed at random locations within the navigation sector volume.

The main logic is implemented in the **PathRoute3DWithTarget** component. A *PathRoute* object is created to calculate a valid 3D path from the robot's current position to the target using *PathRoute.Create3D()*. Once a valid path is generated, the robot rotates toward the next point in the path and moves forward. If the path becomes invalid - for example, if the target ends up in an unreachable area, then the system selects a new target location and recalculates the route.

Target positions are chosen at random inside the volume of a *Navigation Sector*, using *Inside3D()* for validation. The route is automatically updated as the robot approaches the target. If the route is successfully resolved, the path is drawn on screen using *RenderVisualizer()*.

To help visualize active navigation areas, the **NavigationSectorVisualizer** component renders the geometry of all sectors during runtime.

This setup is well-suited for implementing autonomous navigation in 3D volume, where both the moving objects and their targets can change position during runtime within a defined navigable area.