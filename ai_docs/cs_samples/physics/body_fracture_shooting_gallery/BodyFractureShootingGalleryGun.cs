// Shooting gallery gun that spawns projectiles with physics impulse.
// Instantiates node prefabs on click and applies directional force to
// launch them toward the target with a crosshair overlay.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Unigine;

// Spawns projectiles with impulse on mouse click.
public partial class BodyFractureShootingGalleryGun : Component
{
	// Launch force applied to projectile
	public float Force = 50.0f;

	// Node file to spawn as projectile
	[ParameterFile(Filter = ".node")]
	public string InstanceNode = "";

	// Crosshair sprite image path
	[ParameterFile]
	public string CrosshairImage = "";
	// Crosshair display size
	public ivec2 CrosshairSize = new(25, 25);

	// Crosshair UI sprite
	private WidgetSprite crosshair = null;

	// Crosshair is created if image is specified.
	void Init()
	{
		if (CrosshairImage != "")
		{
			crosshair = new(CrosshairImage) { Width = CrosshairSize.x, Height = CrosshairSize.y };
			Gui.GetCurrent().AddChild(crosshair, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);
		}
	}

	// Projectile is spawned and launched on left-click.
	void Update()
	{
		if (Console.Active && InstanceNode != "")
			return;

		if (Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT))
		{
			var inst = World.LoadNode(InstanceNode);
			inst.WorldPosition = node.WorldPosition;

			var body = inst.ObjectBodyRigid;
			if (!body)
				return;

			body.AddLinearImpulse(node.GetWorldDirection() * Force);
		}
	}

	// Crosshair is cleaned up on shutdown.
	void Shutdown()
	{
		crosshair?.DeleteLater();
	}
}
