// Camera zoom controller that adjusts FOV, render distance scale, and mouse sensitivity.
// When zooming in, FOV decreases for magnification while distance scale increases to
// maintain detail quality. Mouse sensitivity is also reduced to allow precise aiming.
// Works with any Player-derived class (PlayerSpectator, PlayerActor, etc.).

using Unigine;
#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Zoom functionality for any Player type with FOV and sensitivity adjustments.
public partial class ZoomController : Component
{
	// Default values stored at Init for reset capability
	private float defaultFOV = 60.0f;
	private float defaultDistanceScale = 1.0f;
	private float defaultSensivity = 1.0f;
	private float defaultPlayerTurning = 90.0f;

	private Player player;

	// Captures default values for FOV, distance scale, sensitivity, and turning speed.
	private void Init()
	{
		player = node as Player;
		if (!player)
		{
			Log.Error("ZoomSample::init cannot cast node to player!\n");
		}

		// Store defaults for restoration on reset or shutdown
		defaultFOV = player.Fov;
		defaultDistanceScale = Render.DistanceScale;
		defaultSensivity = ControlsApp.MouseSensitivity;

		// Get turning speed based on player type (not all Player classes have Turning)
		if (node.Type == Node.TYPE.PLAYER_SPECTATOR)
		{
			PlayerSpectator playerSpectator = player as PlayerSpectator;
			defaultPlayerTurning = playerSpectator.Turning;
		}
		if (node.Type == Node.TYPE.PLAYER_ACTOR)
		{
			PlayerActor playerActor = player as PlayerActor;
			defaultPlayerTurning = playerActor.Turning;
		}
	}

	// Restores render settings to prevent zoom affecting other scenes.
	private void Shutdown()
	{
		Render.DistanceScale = defaultDistanceScale;
		ControlsApp.MouseSensitivity = defaultSensivity;
	}

	// Rotates camera to look at specified target node.
	public void FocusOnTarget(Node target)
	{
		Vec3 dir = target.WorldPosition - node.WorldPosition;
		dir.Normalize();
		player.ViewDirection = (vec3)dir;
	}

	// Applies zoom by adjusting FOV, distance scale, sensitivity, and turning speed.
	// zoomFactor of 2 = 2x magnification (half FOV, double distance scale, etc.)
	public void UpdateZoomFactor(float zoomFactor)
	{
		player.Fov = defaultFOV / zoomFactor;
		Render.DistanceScale = defaultDistanceScale * zoomFactor;
		ControlsApp.MouseSensitivity = defaultSensivity / zoomFactor;

		if (node.Type == Node.TYPE.PLAYER_SPECTATOR || node.Type == Node.TYPE.PLAYER_ACTOR)
		{
			UpdateTurning(zoomFactor);
		}
	}

	// Adjusts turning speed for player types that support it.
	// Lower turning at high zoom prevents jerky camera movement.
	private void UpdateTurning(float zoomFactor)
	{
		// Turning determines speed at which the Player is rotated. It should be lowered and heightened depending on zoom factor
		// ZoomController has been made for the base Player class so it could work with any Player derived class But not every Player has a Turning property
		// Because of that we should regulate Turning depeping on player node type.
		// There is no work around this situation and since changing behavior depending on class type is bad practice this functionality has been hidden from public interface

		if (node.Type == Node.TYPE.PLAYER_SPECTATOR)
		{
			PlayerSpectator playerSpectator = player as PlayerSpectator;
			playerSpectator.Turning = defaultPlayerTurning / zoomFactor;
		}
		if (node.Type == Node.TYPE.PLAYER_ACTOR)
		{
			PlayerActor player_actor = player as PlayerActor;
			player_actor.Turning = defaultPlayerTurning / zoomFactor;
		}
	}

	// Resets zoom to default (1x).
	public void Reset()
	{
		UpdateZoomFactor(1);
	}
}
