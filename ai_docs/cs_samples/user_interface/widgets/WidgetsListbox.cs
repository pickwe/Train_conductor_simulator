// Demonstrates WidgetListBox - a scrollable list for selecting items.
// Logs selection changes to the on-screen console.

using Unigine;

// Scrollable list widget for item selection.
public partial class WidgetsListbox : Component
{
	public int x = 300;
	public int y = 300;
	public int fontSize = 16;

	private WidgetListBox listBox = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create list and populate with items
		listBox = new WidgetListBox(gui);
		listBox.SetPosition(x, y);
		listBox.AddItem("item 0");
		listBox.AddItem("item 1");
		listBox.AddItem("item 2");
		listBox.FontSize = fontSize;
		listBox.FontOutline = 1;
		listBox.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Listbox: {listBox.GetCurrentItemText()}"));

		gui.AddChild(listBox, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes listbox from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(listBox);
		Unigine.Console.Onscreen = false;
	}
}
