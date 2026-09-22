// Demonstrates C# calling UnigineScript functions via Engine.RunWorldFunction().
// Shows invoking USC functions by name with Variable arguments and return values.
// Also registers a wrapper function so USC can trigger C#-to-USC calls indirectly.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Helper class with wrapper function for calling USC functions from C#.
public class MyCallback
{
	// Prefix for log messages indicating C# origin
	public const string sourse_str = "From [C++]:";

	// Calls a UnigineScript function by name with one argument, returns result.
	public static Variable runWorldFunction(Variable name, Variable v)
	{
		Log.Message("{0} runWorldFunction({1},{2}): called\n", sourse_str, name.TypeName, v.TypeName);
		return Engine.RunWorldFunction(name, v);
	}
}

// Partial class for registering C# functions as UnigineScript externals.
public partial class InterpreterRegistrator
{
	private InterpreterRegistrator.InterpreterRegistratorAction my_callback_action
		= new InterpreterRegistrator.InterpreterRegistratorAction(() =>
		{
			// Export wrapper function so USC can also invoke USC functions via C#
			Interpreter.AddExternFunction("runWorldFunction", new Interpreter.Function2v(MyCallback.runWorldFunction));
		}
	);
}

// Sample component demonstrating C# calling USC functions each frame.
public partial class ScriptCallback : Component
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

	// Calls USC "counter" function each frame, demonstrates coroutine-like yield.
	void Update()
	{
		// Call USC function "counter" - it yields values 0,1,2,3 then returns -1
		Variable ret = Engine.RunWorldFunction(new Variable("counter"));
		if (ret.Int != -1)
			Log.Message("{0} counter is: {1}\n", MyCallback.sourse_str, ret.Int);
		// Also demonstrates calling built-in USC functions
		if (ret.Int == 3)
			Log.Message("\n{0} world path is: \"{1}\"\n", MyCallback.sourse_str, Engine.RunWorldFunction(new Variable("engine.world.getPath")).String);

	}

	// Restores original console settings.
	void Shutdown()
	{
		Unigine.Console.Onscreen = false;
		Unigine.Console.OnscreenHeight = 30;
		Unigine.Console.OnscreenTime = onscreenTime;
	}
}
