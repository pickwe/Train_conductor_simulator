// Player-controlled robot character with arrow key movement and collision sliding.
// Features health tracking, damage handling, and death effect spawning. Demonstrates
// basic character controller with wall sliding collision response.

using System;
using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif
#endregion

// Player character with movement, health, and collision handling.
public partial class Robo : Component
{
	// Movement speed in units per second
	public float moveSpeed = 5.0f;
	// Rotation speed in degrees per second
	public float turnSpeed = 90.0f;
	// Current health points
	public int health = 50;

	// Effect spawned when robot dies
	[ParameterFile(Filter = ".node")]
	public string deathFx = "";

	// Reference to UI window for health display
	private SampleDescriptionWindow sampleDescriptionWindow;

	// UI window reference is acquired for status display.
	private void Init()
	{
		var description = ComponentSystem.FindComponentInWorld<DescriptionWindowCreator>();
		if (description)
			sampleDescriptionWindow = description.getWindow();
	}

	// Input is processed for rotation and movement with collision sliding.
	private void Update()
	{
		// Handle rotation input (left/right arrows)
		float rotation = Convert.ToSingle(Input.IsKeyPressed(Input.KEY.LEFT))
			- Convert.ToSingle(Input.IsKeyPressed(Input.KEY.RIGHT));

		node.Rotate(0, 0, rotation * turnSpeed * Game.IFps);

		vec3 dir = node.GetWorldDirection(MathLib.AXIS.Y);

		// Handle movement input (up/down arrows)
		float movement = Convert.ToSingle(Input.IsKeyPressed(Input.KEY.UP))
			- Convert.ToSingle(Input.IsKeyPressed(Input.KEY.DOWN));

		vec3 move = movement * dir;
		if (move.Length2 < MathLib.EPSILON) { return; }
		move.Normalize();

		vec3 shift = move * moveSpeed * Game.IFps;

		Vec3 pos = node.WorldPosition;
		Vec3 start = pos;

		// Check for collision in movement direction
		WorldIntersectionNormal intersectionNormal = new();
		Unigine.Object obj = World.GetIntersection(start, start + shift, 1, intersectionNormal);

		// No collision - move directly
		if (obj == null)
		{
			node.WorldPosition = pos + shift;
			return;
		}

		// Calculate wall slide direction (remove component toward wall)
		vec3 n = intersectionNormal.Normal;
		n.z = 0.0f;
		n.Normalize();
		vec3 slide = move - n * MathLib.Dot(move, n);
		float slideFactor = slide.Length;
		if (slideFactor < MathLib.EPSILON) { return; }
		slide.Normalize();

		// Apply slide movement if no collision in slide direction
		shift = slide * moveSpeed * slideFactor * Game.IFps;

		Unigine.Object obj2 = World.GetIntersection(start, start + shift, 1);
		if (obj2 == null)
		{
			node.WorldPosition = pos + shift;
			return;
		}
	}

	// Reduces health by damage amount and destroys robot if health depleted.
	public void Hit(int damage)
	{
		health -= damage;
		var status = $"Health: {health}";
		if (sampleDescriptionWindow != null)
			sampleDescriptionWindow.setStatus(status);

		// Destroy robot when health reaches zero
		if (health <= 0)
			node.DeleteLater();
	}

	// Death effect is spawned when robot is destroyed.
	private void Shutdown()
	{
		if (deathFx != "")
		{
			Node deathFxNode = World.LoadNode(deathFx);
			deathFxNode.WorldPosition = node.WorldPosition;
		}
	}
}
