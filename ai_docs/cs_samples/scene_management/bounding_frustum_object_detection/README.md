# Bounding Frustum Object Detection

This sample demonstrates how to find an intersection between the bound frustum and geometry.

The *BoundFrustumIntersection.cs* component shows how to use *World.GetIntersection()* to detect static mesh objects located inside a perspective frustum defined in world coordinates. The frustum is highlighted using the *Visualizer*, and intersected object names are printed to the onscreen console overlay.