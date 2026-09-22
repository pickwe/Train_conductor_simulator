// Demonstrates UnigineScript calling C# functions for array manipulation.
// Registers C# functions as externals that UnigineScript can invoke to manipulate
// its array types. Functions allow setting/getting values, generating test data,
// and enumerating contents of ArrayVector (indexed) and ArrayMap (associative).

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Partial class for registering C# functions as UnigineScript externals.
public partial class InterpreterRegistrator
{
	private InterpreterRegistrator.InterpreterRegistratorAction my_array_action
		= new InterpreterRegistrator.InterpreterRegistratorAction(() =>
		{
			// Export functions
			Interpreter.AddExternFunction("my_array_vector_set", new Interpreter.Function3(MyArray.my_array_vector_set), "[]");
			Interpreter.AddExternFunction("my_array_vector_get", new Interpreter.Function2v(MyArray.my_array_vector_get), "[]");
			Interpreter.AddExternFunction("my_array_map_set", new Interpreter.Function3(MyArray.my_array_map_set), "[]");
			Interpreter.AddExternFunction("my_array_map_get", new Interpreter.Function2v(MyArray.my_array_map_get), "[]");
			Interpreter.AddExternFunction("my_array_vector_generate", new Interpreter.Function1(MyArray.my_array_vector_generate), "[]");
			Interpreter.AddExternFunction("my_array_map_generate", new Interpreter.Function1(MyArray.my_array_map_generate), "[]");
			Interpreter.AddExternFunction("my_array_vector_enumerate", new Interpreter.Function1(MyArray.my_array_vector_enumerate), "[]");
		}
	);
}

// Static class containing array manipulation functions callable from UnigineScript.
public class MyArray
{
	// Prefix for log messages indicating C# origin
	private const string sourse_str = "From [C++]:";

	// Sets a value in ArrayVector at specified index.
	public static void my_array_vector_set(Variable id, Variable index, Variable val)
	{
		ArrayVector vector = ArrayVector.Get(Interpreter.Get(), id);
		vector.Set(index.Int, val);
	}

	// Gets a value from ArrayVector at specified index.
	public static Variable my_array_vector_get(Variable id, Variable index)
	{
		ArrayVector vector = ArrayVector.Get(Interpreter.Get(), id);
		return vector.Get(index.Int);
	}

	// Sets a value in ArrayMap for specified key.
	public static void my_array_map_set(Variable id, Variable key, Variable val)
	{
		ArrayMap map = ArrayMap.Get(Interpreter.Get(), id);
		map.Set(key, val);
	}

	// Gets a value from ArrayMap for specified key.
	public static Variable my_array_map_get(Variable id, Variable key)
	{
		ArrayMap map = ArrayMap.Get(Interpreter.Get(), id);
		return map.Get(key);
	}

	// Generates test data in ArrayVector: squares 1-9 plus string "128".
	public static void my_array_vector_generate(Variable id)
	{
		ArrayVector vector = ArrayVector.Get(Interpreter.Get(), id);
		vector.Clear();
		for (int i = 0; i < 4; i++)
		{
			vector.Append(new Variable(i * i));
		}
		vector.Remove(0);
		vector.Append(new Variable("128"));
	}

	// Generates test data in ArrayMap: key-value pairs of squares plus 128:"128".
	public static void my_array_map_generate(Variable id)
	{
		ArrayMap map = ArrayMap.Get(Interpreter.Get(), id);
		map.Clear();
		for (int i = 0; i < 4; i++)
		{
			map.Append(new Variable(i * i), new Variable(i * i));
		}
		map.Remove(new Variable(0));
		map.Append(new Variable(128), new Variable("128"));
	}

	// Logs all elements in ArrayVector with their type info.
	public static void my_array_vector_enumerate(Variable id)
	{
		ArrayVector vector = ArrayVector.Get(Interpreter.Get(), id);
		for (int i = 0; i < vector.Size; i++)
		{
			Log.Message("{0} {1}: {2}\n", sourse_str, i, vector.Get(i).TypeInfo);
		}
	}
}

// Sample component that configures on-screen console for viewing interop results.
public partial class ScriptArrays : Component
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

