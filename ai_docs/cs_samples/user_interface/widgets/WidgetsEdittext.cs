// Demonstrates WidgetEditText - a multi-line text editor field.
// Logs text changes to the on-screen console.

using Unigine;

// Multi-line text area with change event handling.
public partial class WidgetsEdittext : Component
{
	// Position, size, and default text settings
	public int x = 250;
	public int y = 150;
	public int width = 150;
	public int height = 100;
	public string text = "Enter text...";
	public int fontSize = 16;

	private WidgetEditText editText = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create multi-line text area with outline for readability
		editText = new WidgetEditText(gui, text);
		editText.SetPosition(x, y);
		editText.Width = width;
		editText.Height = height;
		editText.FontSize = fontSize;
		editText.FontOutline = 1;
		editText.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Edittext: {editText.Text}"));

		gui.AddChild(editText, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes edittext from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(editText);
		Unigine.Console.Onscreen = false;
	}
}
