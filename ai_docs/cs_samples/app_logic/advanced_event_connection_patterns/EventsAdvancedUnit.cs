// Demonstrates event subscription with various connection methods. Shows how to
// connect to events using methods with extra arguments, lambdas, and delegates.
// Rotates the node in response to events fired by EventsAdvancedSample.

using Unigine;

// Subscribes to rotation events and applies rotation to its node.
public partial class EventsAdvancedUnit : Component
{
	// Reference to the event source component
	public EventsAdvancedSample input_manager;

	// Connection handles for manual disconnection
	private EventConnection rotate_x_connection;
	private EventConnection rotate_y_connection;
	private EventConnection rotate_z_connection;
	private EventConnection rotate_connection;

	// Collection for automatic connection management
	private readonly EventConnections connections = new();

	// Delegate for multi-parameter event
	private EventDelegate<float, float, float> rotate_delegate;

	// Event connections are established using different subscription patterns.
	private void Init()
	{
		if (input_manager is null)
			return;

		rotate_delegate += rotate;

		// Connect to method with additional arguments bound
		rotate_x_connection = input_manager.EventRotateX.Connect(rotateNode, 0.0f, 0.0f, node);
		rotate_y_connection = input_manager.EventRotateY.Connect(connections, rotateNodeY, node);

		// Connect to lambda expression
		rotate_z_connection = input_manager.EventRotateZ.Connect(connections,
		angle => {
			rotateNode(0, 0, angle, node);
		});

		// Connect to delegate (discards last argument from 4-param event)
		rotate_connection = input_manager.EventRotate.Connect(rotate_delegate);
	}

	// All event connections are disconnected on shutdown.
	private void Shutdown()
	{
		rotate_x_connection.Disconnect();
		rotate_y_connection.Disconnect();
		rotate_z_connection.Disconnect();
		rotate_connection.Disconnect();
	}

	// Instance method that forwards rotation to static helper.
	private void rotate(float angleX, float angleY, float angleZ)
	{
		rotateNode(angleX, angleY, angleZ, node);
	}

	// Applies rotation to node and logs the operation.
	private static void rotateNode(float angleX, float angleY, float angleZ, Node node)
	{
		string axisString = "";
		if (angleX != 0.0f) axisString += 'X';
		if (angleY != 0.0f) axisString += 'Y';
		if (angleZ != 0.0f) axisString += 'Z';
		axisString = axisString.PadRight(3);

		Log.MessageLine($"Rotate {axisString} ({angleX:F1} {angleY:F1} {angleZ:F1})!");
		node?.Rotate(angleX, angleY, angleZ);
	}

	// Helper method for Y-axis only rotation.
	private static void rotateNodeY(float angle, Node node)
	{
		rotateNode(0, angle, 0, node);
	}
}
