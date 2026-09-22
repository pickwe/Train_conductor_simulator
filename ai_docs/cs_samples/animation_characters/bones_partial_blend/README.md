# Bones: Partial Blend [Animation Graph]

This sample demonstrates partial blending between two animations in an *animation graph*, where only the selected bones are affected.

Two animations are played at the same time: the character keeps walking while the punching animation is blended into the upper part of the body only. The set of bones that receive the blended pose is defined by a mask assigned to the blending node of the graph.

Use the *Weight* slider in the sample window to change how much of the punching animation is applied to the masked bones. The slider sets a parameter of the graph, where the animations, the blending, and the mask are configured.

Partial blending lets a character perform different actions with different parts of its body at the same time, without preparing a separate animation for every combination.