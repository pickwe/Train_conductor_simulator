# Abstract Components

This sample demonstrates how to use abstract classes in the C# Component System to implement common behavior across different components.

At the core of the sample is the abstract **Toggleable** component, which defines a shared structure for enabling and disabling functionality. It contains the **Toggled** property that automatically calls the *On()* or *Off()* methods when changed. These methods are abstract and must be implemented in each derived class. The *Toggle()* method is used to switch the state manually, applying the corresponding behavior and updating the internal state.

Two specific components, **Lamp** and **Fan**, inherit from **Toggleable** and implement their own versions of the abstract methods. Lamp controls a light source by toggling its emission material state, while Fan continuously rotates the object when active.

The **Toggler** component performs interaction by casting a ray from the camera when the left mouse button is pressed. If it hits an object with a **Toggleable** component attached, it toggles that component's state.

This setup is useful for scenarios where different types of objects need to respond to a common interaction pattern. Using abstract classes makes it easy to implement consistent logic across objects while still allowing each one of them to behave differently.