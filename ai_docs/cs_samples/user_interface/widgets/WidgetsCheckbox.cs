// Demonstrates WidgetCheckBox - a toggleable checkbox with label text.
// Logs checked state changes to the on-screen console.

using Unigine;

// Checkbox widget with state change event handling.
public partial class WidgetsCheckbox : Component
{
	// Checkbox position and appearance settings
	public int x = 450;
	public int y = 50;
	public string text = "Check Me";
	public int fontSize = 16;

	private WidgetCheckBox checkBox = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create checkbox with text label and outline for readability
		checkBox = new WidgetCheckBox(gui, text);
		checkBox.SetPosition(x, y);
		checkBox.FontSize = fontSize;
		checkBox.FontOutline = 1;
		checkBox.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Checkbox: {checkBox.Checked}"));

		gui.AddChild(checkBox, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes checkbox from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(checkBox);
		Unigine.Console.Onscreen = false;
	}
}
