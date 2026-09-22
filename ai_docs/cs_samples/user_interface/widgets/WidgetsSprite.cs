// Demonstrates WidgetSprite - displays an image texture on the GUI.
// Sprites are typically used for backgrounds, icons, or custom graphics.

using Unigine;

// Image display widget for rendering textures on the GUI.
public partial class WidgetsSprite : Component
{
	// Sprite position and size
	public int x = 275;
	public int y = 450;
	public int width = 100;
	public int height = 50;

	// Path to sprite image file
	[ParameterFile]
	public string spriteImage = "";

	private WidgetSprite sprite = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create sprite from image file
		sprite = new WidgetSprite(gui, spriteImage);
		sprite.Width = width;
		sprite.Height = height;
		sprite.SetPosition(x, y);

		gui.AddChild(sprite, Gui.ALIGN_OVERLAP);
	}

	// Removes sprite from GUI.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(sprite);
	}
}
