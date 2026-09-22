# Persecutor Controller

This sample demonstrates a camera following a moving target.

The *CameraPersecutor.cs* component implements a third-person follow camera that smoothly tracks a moving target defined in the *Target* field. The camera adjusts its distance, pitch, and yaw to maintain the desired view of the target, using the *PlayerDummy* node as its base.

Input is handled via the *CameraControls.cs* component and allows orbiting around the target as well as zooming in and out. The behavior is configurable through parameters such as *Angular Speed, Zoom Speed, Min/Max Distance, Min/Max Vertical Angle*, and the *Use Fixed Angles* toggle. The *Target* field can be manually assigned and defines the object the camera will follow.

The target movement is defined by the *CameraPersecutorTarget.cs* component, which moves the object along a circular path over time to demonstrate dynamic tracking.