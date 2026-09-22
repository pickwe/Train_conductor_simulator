// Demonstrates dynamic node creation and deletion. Spawns node instances
// at timed intervals in a grid pattern, automatically removing older
// instances when the maximum count is exceeded.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

using System.Collections.Generic;
using Unigine;

// Spawns and removes nodes on a timer in grid pattern.
public partial class NodeSpawner : Component
{
	// Path to node file to spawn
	[ParameterFile(Filter = ".node")]
	public string instancePath = "";

	// Collection of spawned node instances
	private List<Node> instances = null;

	// Interval between spawns
	private const float spawnTimer = 0.5f;
	// Current countdown timer
	private float currentTime = 0.0f;

	// Maximum number of instances
	private const int count = 20;
	// Current grid position index
	private int currentIndex = 0;

	// Instance list is initialized.
	private void Init()
	{
		instances = new List<Node>();
		currentTime = spawnTimer;
	}

	// Nodes are spawned and old ones removed on timer.
	private void Update()
	{
		// Countdown spawn timer
		currentTime -= Game.IFps;
		if (currentTime < 0)
		{
			// Create new instance and add to collection
			Node newNode = World.LoadNode(instancePath);
			instances.Add(newNode);

			// Set world position of node based on current index
			float x = 4.0f - 2.0f * (currentIndex / 4);
			float y = -4.0f + 2.0f * (currentIndex % 4);
			newNode.WorldPosition = new Vec3(x, y, 0.0f);

			// Remove oldest node when half capacity reached
			if (instances.Count > count / 2)
			{
				// Delete first node in collection
				instances[0].DeleteLater();
				instances.RemoveAt(0);
			}

			// Advance grid position, wrap around at max count
			currentIndex++;
			if (currentIndex == count)
				currentIndex = 0;

			// Reset timer for next spawn
			currentTime = spawnTimer;
		}
	}
}
