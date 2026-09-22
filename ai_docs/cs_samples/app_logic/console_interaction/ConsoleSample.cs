// Demonstrates custom console commands in UNIGINE. Shows how to register
// console commands with callbacks and control nodes via command-line input.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unigine;
using static Unigine.Console;

// Registers custom console commands and handles their callbacks.
public partial class ConsoleSample : Component
{
	// Node that can be controlled via console commands
	public Node controllable_node;

	// Console is activated and custom commands are registered.
	void Init()
	{
		// Show console
		Unigine.Console.Active = true;

		// Register a simple console command with callback
		Unigine.Console.AddCommand("my_console_command", "my_console_command decription", command_callback);

		// Execute console command to demonstrate usage
		Unigine.Console.Run("my_console_command \"Hello from C++\"");

		// Register command for controlling node position
		Unigine.Console.AddCommand("control_node", "With this command you can control node, pass desired position through the arguments",
		 move_node_callback);

		if (!controllable_node)
		{
			Log.Message("ConsoleSample.Init(): No node was provided!\n");
		}

		Log.Message("To move the node, you can use control_node command and 3 arguments to specify node position\n");
	}

	// Console commands are removed and console is hidden.
	void Shutdown()
	{
		Unigine.Console.RemoveCommand("my_console_command");
		Unigine.Console.RemoveCommand("control_node");
		Unigine.Console.Active = false;
	}

	// Callback that prints command arguments to the log.
	private void command_callback(int argc, string[] argv)
	{
		Log.Message("my_console_command(): called\n");
		for (int i = 0; i < argc; i++)
			Log.Message("{0}: {1}\n", i, argv[i]);
	}

	// Callback that moves the controllable node to specified position.
	private void move_node_callback(int argc, string[] argv)
	{
		if (!controllable_node)
			return;

		vec3 node_position = new vec3();
		if (argc != 4)
		{
			Log.Warning("control_node was called incorrectly, you need to pass 3 coordinates to move the node\n");
			return;
		}

		// Parse coordinate arguments from strings to floats
		var parse_arg = (int index) =>
		{
			string a_value = argv[index];
			bool is_number = float.TryParse(a_value, NumberStyles.Any, CultureInfo.InvariantCulture, out float res);
			float value = is_number ? res : 0.0f;
			return value;
		};

		node_position.x = parse_arg(1);
		node_position.y = parse_arg(2);
		node_position.z = parse_arg(3);

		controllable_node.WorldPosition = node_position;
	}
};
