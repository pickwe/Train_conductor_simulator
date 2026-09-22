# Camera First Person

This sample demonstrates a first-person spectator-style camera with free movement.
The *CameraSpectator.cs* component provides free-flight first-person controls by modifying the position and orientation of a *PlayerDummy* node in real time. The movement and rotation are handled via the *CameraControls.cs* component that defines directional input.

The camera moves in six directions (forward, backward, left, right, up, down) and rotates using pitch and yaw angles. The *Speed* and *Angular Speed* parameters can be adjusted in the component properties to control the camera's movement and rotation responsiveness.