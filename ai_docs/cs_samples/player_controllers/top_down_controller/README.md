# Top-Down Controller

This sample demonstrates how to set up and control a top-down camera controller for real-time strategy or tactics-style gameplay. It implements core features typical for such games, including camera movement, zooming, rotation, edge scrolling, unit selection, and focus tracking.
The logic is implemented across four components: **CameraTopDown**, **CameraSelection, CameraUnitSelection**, and **CameraUnitPathControl**:

-The camera controls are implemented in the **CameraTopDown** component, which handles moving, adjusting zoom levels, and interpreting user input for panning and rotating the view. The camera also supports edge scrolling - when the mouse cursor moves near the edge of the screen, the camera starts moving in that direction. When one or more units are selected, the camera can optionally shift focus to the selection and follow it continuously while a key is held.
-Selection logic is managed by the **CameraSelection** component. It enables rectangular selection of multiple units using a screen-space box and calculates the combined bounding sphere of all selected objects for proper camera focusing.
-The **CameraUnitSelection** component is responsible for visual feedback during selection. It highlights selected units by rendering a visual circle beneath them.
-One of the units in the scene includes the **CameraUnitPathControl** component, which drives its movement along a predefined path that includes multiple waypoints. The unit traverses the path in a loop, demonstrating how moving elements can be integrated into a top-down control system.

This setup provides a flexible base for implementing top-down camera control and unit interaction, useful for strategy games, tactical simulations, or scene overview tools.