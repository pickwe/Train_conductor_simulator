// Demonstrates advanced event system with multiple event types and parameters.
// Shows how to create custom events with different signatures and fire them
// from keyboard input. Works with EventsAdvancedUnit to show event subscription.

using Unigine;

// Fires rotation events in response to keyboard input.
public partial class EventsAdvancedSample : Component
{
	// Events for single-axis rotation
	public Event<float> EventRotateX { get { return rotate_x_event; } }
	public Event<float> EventRotateY { get { return rotate_y_event; } }
	public Event<float> EventRotateZ { get { return rotate_z_event; } }
	// Event for combined rotation with sender reference
	public Event<float, float, float, EventsAdvancedSample> EventRotate { get { return rotate_event; } }

	// Event invokers for firing events
	private readonly EventInvoker<float> rotate_x_event = new();
	private readonly EventInvoker<float> rotate_y_event = new();
	private readonly EventInvoker<float> rotate_z_event = new();
	private readonly EventInvoker<float, float, float, EventsAdvancedSample> rotate_event = new();

	// Speed values passed to rotation events
	public vec3 rotation_speed = new(3.0f, 3.0f, 3.0f);

	// Onscreen console is enabled for log output.
	private void Init()
	{
		Console.Onscreen = true;
	}

	// Keyboard input is checked and rotation events are fired accordingly.
	private void Update()
	{
		if (Console.Active)
			return;

		// Fire X rotation event on T key
		if (Input.IsKeyPressed(Input.KEY.T))
		{
			Log.MessageLine("Run rotate X with 1 arg");
			rotate_x_event.Run(rotation_speed.x);
		}
		// Fire Y rotation event on Y key
		if (Input.IsKeyPressed(Input.KEY.Y))
		{
			Log.MessageLine("Run rotate Y with 1 arg");
			rotate_y_event.Run(rotation_speed.y);
		}
		// Fire Z rotation event on U key
		if (Input.IsKeyPressed(Input.KEY.U))
		{
			Log.MessageLine("Run rotate Z with 1 arg");
			rotate_z_event.Run(rotation_speed.z);
		}
		// Fire combined rotation event on I key
		if (Input.IsKeyPressed(Input.KEY.I))
		{
			Log.MessageLine("Run rotate XYZ with 4 args");
			rotate_event.Run(rotation_speed.x, rotation_speed.y, rotation_speed.z, this);
		}
	}

	// Onscreen console is disabled on component shutdown.
	private void Shutdown()
	{
		Console.Onscreen = false;
	}
}
