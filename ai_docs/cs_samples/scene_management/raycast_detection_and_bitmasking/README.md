# Raycast Detection And Bitmasking

This sample demonstrates how to find an intersection of a ray with geometry.

The *LaserRayIntersection.cs* component casts a ray from the laser origin in its forward direction and uses *World.GetIntersection()* to detect intersections with geometry. The method provides both the intersection point and the surface normal. Intersected object names are printed to the onscreen console overlay.

If a hit is detected, the laser beam is resized to visually represent the exact distance between its origin and the intersection point. The hit effect object is shown at the intersection point, oriented in accordance with the surface normal. If no intersection is found, the laser beam keeps its default length and the effect remains hidden.