// Demonstrates XML tree creation and traversal using UNIGINE's Xml class.
// Creates a nested XML structure with child nodes and attributes, then
// recursively prints the tree structure to the console with indentation.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Creates and prints an XML tree structure to demonstrate Xml API usage.
public partial class XmlSample : Component
{
	// Stores original console display time for restoration
	float onscreenTime;

	// XML tree is created and printed to onscreen console.
	void Init()
	{
		// Enable onscreen console and extend display time
		Unigine.Console.Onscreen = true;
		onscreenTime = Unigine.Console.OnscreenTime;
		Unigine.Console.OnscreenTime = 100.0f;

		Log.Message("\n");

		// Create nested XML structure
		Xml xml = xml_create();

		// Print tree with recursive indentation
		xml_print(xml, 0);
	}

	// Console settings are restored on shutdown.
	void Shutdown()
	{
		Unigine.Console.Onscreen = false;
		Unigine.Console.OnscreenTime = onscreenTime;
	}

	// Builds a nested XML tree with attributes and data.
	private Xml xml_create()
	{
		// Create root node
		Xml xml = new Xml("node");
		// Add nested child nodes with attributes
		Xml xml_0 = xml.AddChild("child", "arg=\"0\"");
		Xml xml_1 = xml_0.AddChild("child", "arg=\"1\"");
		Xml xml_2 = xml_1.AddChild("child", "arg=\"2\"");
		// Set data on deepest child
		xml_2.Data = "data";
		// Prevent automatic cleanup when returning from function
		xml.APIInterfaceOwner = false;
		return xml;
	}

	// Recursively prints XML node with indentation based on depth.
	private static void xml_print(Xml xml, int offset)
	{
		// Print indentation
		for (int i = 0; i < offset; i++)
		{
			Log.Message(" ");
		}

		// Print node name and all attributes
		Log.Message("{0}: ", xml.Name);
		for (int i = 0; i < xml.NumArgs; i++)
		{
			Log.Message("{0}={1} ", xml.GetArgName(i), xml.GetArgValue(i));
		}
		Log.Message(": {0}\n", xml.Data);

		// Recursively print children with increased indentation
		for (int i = 0; i < xml.NumChildren; i++)
		{
			xml_print(xml.GetChild(i), offset + 1);
		}
	}
}
