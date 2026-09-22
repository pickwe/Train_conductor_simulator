# Node Spawning And Deletion

This sample demonstrates how to dynamically create and delete node instances at runtime in UNIGINE.

A specified **.node** asset is instantiated repeatedly at timed intervals, with each new instance positioned in a grid pattern. Once a certain number of nodes are active, the oldest ones are automatically removed (first-in, first-out deletion) to maintain a fixed, memory-safe maximum count in the scene.

**Use Cases:**

-Object pooling and controlled instancing
-Spawning projectiles, enemies, or temporary effects.