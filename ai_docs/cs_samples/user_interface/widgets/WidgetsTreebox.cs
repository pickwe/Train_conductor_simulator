// Demonstrates WidgetTreeBox - a hierarchical list with expandable nodes.
// Shows parent-child relationships using AddItemChild() to build tree structure.

using Unigine;

// Hierarchical tree view with expandable/collapsible parent nodes.
public partial class WidgetsTreebox : Component
{
	public int x = 775;
	public int y = 300;
	public int fontSize = 16;

	private WidgetTreeBox treeBox = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create tree view widget
		treeBox = new WidgetTreeBox(gui);
		treeBox.SetPosition(x, y);
		treeBox.FontSize = fontSize;
		treeBox.FontOutline = 1;

		// First parent node (index 0) with three children (indices 1-3)
		treeBox.AddItem("parent 0");
		treeBox.AddItem("child 0");
		treeBox.AddItem("child 1");
		treeBox.AddItem("child 2");
		treeBox.AddItemChild(0, 1);  // Make item 1 a child of item 0
		treeBox.AddItemChild(0, 2);
		treeBox.AddItemChild(0, 3);

		// Second parent node (index 4) with three children (indices 5-7)
		treeBox.AddItem("parent 1");
		treeBox.AddItem("child 0");
		treeBox.AddItem("child 1");
		treeBox.AddItem("child 2");
		treeBox.AddItemChild(4, 5);
		treeBox.AddItemChild(4, 6);
		treeBox.AddItemChild(4, 7);

		treeBox.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Treebox: {treeBox.CurrentItemText}"));

		gui.AddChild(treeBox, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes treebox from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(treeBox);
		Unigine.Console.Onscreen = false;
	}
}
