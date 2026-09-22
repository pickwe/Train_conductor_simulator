// UI for adjusting physics movement parameters at runtime. Provides sliders
// to control force, torque, jump impulse, speed limits, and brake strength.

using System.Runtime.InteropServices;
using Unigine;

// Runtime parameter editor for PhysicsMover component.
public partial class PhysicsMovementSample : Component
{
	// Target mover component to configure
	[ShowInEditor]
	[Parameter(Title = "Physics Mover")]
	private PhysicsMover physicsMover = null;

	// Event connections for cleanup
	private EventConnections buttonConnections = new();
	// Sample UI window
	private SampleDescriptionWindow window = null;

	// UI with parameter sliders is created.
	void Init()
	{
		if (!physicsMover)
		{
			Log.Error("PhysicsMovementSample.Init PhysicsMover component is not assigned !\n");
			return;
		}

		window = new SampleDescriptionWindow();
		window.createWindow();

		window.addFloatParameter("Force", null, physicsMover.Force, 1.0f, 20.0f, v =>
		{
			physicsMover.Force = v;
		});

		window.addFloatParameter("Torque", null, physicsMover.Torque, 1.0f, 5.0f, v =>
		{
			physicsMover.Torque = v;
		});

		window.addFloatParameter("Jump Impulse", null, physicsMover.JumpImpulse, 1.0f, 50.0f, v =>
		{
			physicsMover.JumpImpulse = v;
		});

		window.addFloatParameter("Max Speed", null, physicsMover.MaxSpeed, 1.0f, 50.0f, v =>
		{
			physicsMover.MaxSpeed = v;
		});

		window.addFloatParameter("Max Rotation Speed", null, physicsMover.MaxRotationSpeed, 1.0f, 50.0f, v =>
		{
			physicsMover.MaxRotationSpeed = v;
		});

		window.addFloatParameter("Brakes Strength", null, physicsMover.BrakesStrength, 1.0f, 50.0f, v =>
		{
			physicsMover.BrakesStrength = v;
		});
	}

	// UI is cleaned up on shutdown.
	void Shutdown()
	{
		buttonConnections.DisconnectAll();
		window.shutdown();
	}
}
