// Demonstrates WidgetIcon - an image-based button that can be toggled.
// Logs toggle state changes to the on-screen console.

using Unigine;

// Clickable icon button with toggle functionality.
public partial class WidgetsIcon : Component
{
	// Icon position and size
	public int x = 500;
	public int y = 450;
	public int width = 32;
	public int height = 32;

	// Path to icon image file
	[ParameterFile]
	public string iconImage = "";

	private WidgetIcon icon = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create toggleable icon button from image
		icon = new WidgetIcon(gui, iconImage, width, height);
		icon.Toggleable = true;
		icon.SetPosition(x, y);
		icon.EventClicked.Connect(() => Unigine.Console.OnscreenMessageLine($"Icon: {icon.Toggled}"));

		gui.AddChild(icon, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes icon from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(icon);
		Unigine.Console.Onscreen = false;
	}
}
