// Demonstrates exporting C# variables to UnigineScript via Interpreter.AddExternVariable().
// Shows how to expose various types (int, float, double, string, vec3, vec4) as global
// variables that UnigineScript can access directly by name.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Partial class for registering C# variables as UnigineScript globals.
public partial class InterpreterRegistrator
{
	private InterpreterRegistrator.InterpreterRegistratorAction my_variable_action
		= new InterpreterRegistrator.InterpreterRegistratorAction(() =>
		{
			// Export variables of various types accessible from UnigineScript
			Interpreter.AddExternVariable("my_variable_int", 13);
			Interpreter.AddExternVariable("my_variable_float", 13.17f);
			Interpreter.AddExternVariable("my_variable_double", 13.17);
			Interpreter.AddExternVariable("my_variable_string", "13.17s");
			Interpreter.AddExternVariable("my_variable_vec3", new Variable(new vec3(13.0f, 17.0f, 137.0f)));
			Interpreter.AddExternVariable("my_variable_vec4", new Variable(new vec4(13.0f, 17.0f, 137.0f, 173.0f)));
		}
	);
}

// Sample component that configures on-screen console for viewing interop results.
public partial class ScriptVariables : Component
{
	// Saved console time setting for restoration
	private float onscreenTime;

	// Configures on-screen console for better visibility of log messages.
	void Init()
	{
		Unigine.Console.Onscreen = true;
		Unigine.Console.OnscreenFontSize = 15;
		Unigine.Console.OnscreenHeight = 100;
		onscreenTime = Unigine.Console.OnscreenTime;
		Unigine.Console.OnscreenTime = 1000;
	}

	// Restores original console settings.
	void Shutdown()
	{
		Unigine.Console.Onscreen = false;
		Unigine.Console.OnscreenHeight = 30;
		Unigine.Console.OnscreenTime = onscreenTime;
	}
}
