// Demonstrates WidgetScroll - a scrollbar for scrolling content.
// Logs scroll position changes to the on-screen console.

using Unigine;

// Standalone scrollbar widget with value change events.
public partial class WidgetsScroll : Component
{
	public int x = 500;
	public int y = 150;

	private WidgetScroll scroll = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create horizontal scrollbar (Orientation = 0)
		scroll = new WidgetScroll(gui);
		scroll.SetPosition(x, y);
		scroll.Orientation = 0;
		scroll.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Scroll: {scroll.Value}"));

		gui.AddChild(scroll, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes scroll from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(scroll);
		Unigine.Console.Onscreen = false;
	}
}
