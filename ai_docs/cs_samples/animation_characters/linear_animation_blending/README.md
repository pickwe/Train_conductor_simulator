# Linear Animation Blending [Animation Graph]

This sample demonstrates linear interpolation between two animations in an *animation graph*.

Two animations are played at the same time, and the poses they produce are mixed into the resulting one. The closer the weight is to either end, the more of the corresponding animation is left in the result, so the character goes from standing still to walking and back.

Use the *Weight* slider in the sample window to change the proportion. The slider sets a parameter of the graph, where the animations and the blending are configured.

Interpolating skeletal animations allows you to create smooth and natural transitions between different character or object movements.