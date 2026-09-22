# Additive Animation Blending [Animation Graph]

This sample demonstrates additive blending of two animations in an *animation graph*.

Unlike ordinary blending, where two animations are mixed and each of them gets only a part of the result, additive blending takes the difference between an animation and its reference pose and adds it on top of the base one. The base animation keeps playing in full, while the additive one only modifies it.

Use the *Weight* slider in the sample window to change how much of the additive animation is applied. The slider sets a parameter of the graph, where the animations and the blending are configured.

Additive blending is a technique for combining skeletal model animations, enabling smooth transitions and the seamless merging of character or object movements.