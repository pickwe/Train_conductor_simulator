// Demonstrates object lifetime management for nodes and widgets. Shows how
// LIFETIME.WORLD objects persist across world reloads while LIFETIME.ENGINE
// objects persist until engine shutdown. Provides UI to switch lifetime modes.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Creates persistent node and widget with configurable lifetime settings.
public partial class LifetimeSample : Component
{
	// Path to node file to spawn
	[ParameterFile(Filter = ".node")]
	public string nodeToSpawn = null;

	// UI window reference
	private SampleDescriptionWindow sampleDescriptionWindow = new();
	// Static references persist across component instances
	private static Node spawnedNode = null;
	private static Widget spawnedWidget = null;

	// Widget and node are created if not already existing, UI controls lifetime.
	void Init()
	{
		// Create widget with WORLD lifetime (persists across world reloads)
		if (spawnedWidget == null || !spawnedWidget.IsValidPtr)
		{
			WidgetButton button = new("WIDGET FROM LIFETIME SAMPLE!");
			button.FontSize = 18;
			button.Lifetime = Widget.LIFETIME.WORLD;
			spawnedWidget = button;
			Gui.GetCurrent().AddChild(spawnedWidget, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);

			Log.MessageLine("New widget created!");
		}

		// Load node with WORLD lifetime (persists across world reloads)
		if (spawnedNode == null || !spawnedNode.IsValidPtr)
		{
			spawnedNode = World.LoadNode(nodeToSpawn);
			spawnedNode.WorldTransform = node.WorldTransform;
			spawnedNode.Lifetime = Node.LIFETIME.WORLD;

			Log.MessageLine("New node created!");
		}

		sampleDescriptionWindow.createWindow(Gui.ALIGN_RIGHT);

		// Determine current widget lifetime for UI
		int widget_cb_current = 0;
		switch (spawnedWidget.Lifetime)
		{
			case Widget.LIFETIME.WORLD:
				widget_cb_current = 0;
				break;
			case Widget.LIFETIME.WINDOW:
				widget_cb_current = 1;
				break;
			default: break;
		}

		// Add widget lifetime toggle (World = persists, Window = deleted on world unload)
		sampleDescriptionWindow.addSwitchParameter("Widget lifetime", "Widget lifetime", widget_cb_current, ["World", "Window"],
			(int v) =>
			{
				spawnedWidget.Lifetime = v == 0 ? Widget.LIFETIME.WORLD : Widget.LIFETIME.WINDOW;
			});

		// Determine current node lifetime for UI
		int node_cb_current = 0;
		switch (spawnedNode.Lifetime)
		{
			case Node.LIFETIME.WORLD:
				node_cb_current = 0;
				break;
			case Node.LIFETIME.ENGINE:
				node_cb_current = 1;
				break;
			default:
				break;
		}

		// Add node lifetime toggle (World = persists, Engine = deleted on engine shutdown)
		sampleDescriptionWindow.addSwitchParameter("Node lifetime", "Node lifetime", node_cb_current, ["World", "Engine"],
			 (int v) =>
			 {
				 spawnedNode.Lifetime = v == 0 ? Node.LIFETIME.WORLD : Node.LIFETIME.ENGINE;
			 });
	}

	// UI window is released on shutdown.
	void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
	}
}
