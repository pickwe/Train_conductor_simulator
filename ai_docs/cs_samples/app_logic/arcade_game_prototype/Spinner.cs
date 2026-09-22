// Rotating turret that spawns projectiles at configurable intervals. Rotates
// continuously and spawns bullets from designated spawn points. Used as an
// enemy hazard in the arcade demo.

using System.Collections.Generic;
using Unigine;

// Rotates and spawns projectiles from multiple spawn points.
public partial class Spinner : Component
{
	// Rotation speed in degrees per second
	[Parameter(Title = "Angular Speed", Tooltip = "Rotation speed, in degrees per second", Group = "General")]
	public float rotationSpeed = 50.0f;

	// Node file to spawn as projectile
	[ParameterFile(Filter = ".node")]
	public string bulletAsset = "";

	// Points where projectiles are spawned
	public List<Node> spawnPoints = null;

	// Time between projectile spawns in seconds
	public float spawnDelay = 2.0f;

	// Countdown timer until next spawn
	private float spawnTimer = 0.0f;

	// Spawn timer is initialized to delay value.
	private void Init()
	{
		spawnTimer = spawnDelay;
	}

	// Turret rotates and spawns projectiles when timer expires.
	private void Update()
	{
		// Rotate turret continuously
		node.Rotate(0, 0, rotationSpeed * Game.IFps);

		if (spawnPoints == null)
			return;

		// Check spawn timer
		spawnTimer -= Game.IFps;
		if (spawnTimer < 0)
		{
			spawnTimer = spawnDelay;

			// Spawn projectile at each spawn point
			foreach (Node point in spawnPoints)
			{
				Node spawnedBullet = World.LoadNode(bulletAsset);
				spawnedBullet.WorldTransform = point.WorldTransform;
			}
		}
	}
}
