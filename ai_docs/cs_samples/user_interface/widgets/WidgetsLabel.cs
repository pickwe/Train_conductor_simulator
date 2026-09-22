// Demonstrates WidgetLabel - a static text display widget.
// Labels are non-interactive and typically used for displaying information.

using Unigine;

// Static text label for displaying non-editable text.
public partial class WidgetsLabel : Component
{
	// Label position and appearance
	public int x = 800;
	public int y = 150;
	public string text = "Label";
	public int fontSize = 16;

	private WidgetLabel label = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create label with outline for readability
		label = new WidgetLabel(gui, text);
		label.SetPosition(x, y);
		label.FontSize = fontSize;
		label.FontOutline = 1;

		gui.AddChild(label, Gui.ALIGN_OVERLAP);
	}

	// Removes label from GUI.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(label);
	}
}
