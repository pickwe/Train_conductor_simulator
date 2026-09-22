// Demonstrates WidgetButton - a clickable button with customizable size and text.
// Logs click events to the on-screen console.

using Unigine;

// Simple button widget with click event handling.
public partial class WidgetsButton : Component
{
	// Button position and dimensions (configurable in editor)
	public int x = 250;
	public int y = 50;
	public int width = 100;
	public int height = 50;
	public string text = "Press Me";
	public int fontSize = 16;

	private WidgetButton button = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create button with specified properties
		button = new WidgetButton(gui, text);
		button.SetPosition(x, y);
		button.Width = width;
		button.Height = height;
		button.FontSize = fontSize;
		button.EventClicked.Connect(() => Unigine.Console.OnscreenMessageLine("Button Clicked!"));

		gui.AddChild(button, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes button from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(button);
		Unigine.Console.Onscreen = false;
	}
}
