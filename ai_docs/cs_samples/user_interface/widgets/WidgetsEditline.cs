// Demonstrates WidgetEditLine - a single-line text input field.
// Logs text changes to the on-screen console.

using Unigine;

// Single-line text input with change event handling.
public partial class WidgetsEditline : Component
{
	// Position, size, and default text settings
	public int x = 750;
	public int y = 50;
	public int width = 150;
	public int height = 30;
	public string text = "Enter text...";
	public int fontSize = 16;

	private WidgetEditLine editLine = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create single-line text field with outline for readability
		editLine = new WidgetEditLine(gui, text);
		editLine.SetPosition(x, y);
		editLine.Width = width;
		editLine.Height = height;
		editLine.FontSize = fontSize;
		editLine.FontOutline = 1;
		editLine.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Editline text: {editLine.Text}"));

		gui.AddChild(editLine, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes editline from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(editLine);
		Unigine.Console.Onscreen = false;
	}
}
