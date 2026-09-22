// Demonstrates WidgetComboBox - a dropdown selection widget.
// Logs selected item changes to the on-screen console.

using Unigine;

// Dropdown combo box with selection event handling.
public partial class WidgetsCombobox : Component
{
	public int x = 600;
	public int y = 50;
	public int fontSize = 16;

	private WidgetComboBox comboBox = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create dropdown and populate with items
		comboBox = new WidgetComboBox(gui);
		comboBox.SetPosition(x, y);
		comboBox.FontSize = fontSize;

		comboBox.AddItem("item 0");
		comboBox.AddItem("item 1");
		comboBox.AddItem("item 2");

		comboBox.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Combobox: {comboBox.GetCurrentItemText()}"));

		gui.AddChild(comboBox, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes combobox from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(comboBox);
		Unigine.Console.Onscreen = false;
	}
}
