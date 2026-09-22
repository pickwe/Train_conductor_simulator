// Demonstrates WidgetSlider - a draggable slider for selecting values.
// Logs value changes to the on-screen console.

using Unigine;

// Slider widget for continuous value selection.
public partial class WidgetsSlider : Component
{
	// Slider position and dimensions
	public int x = 600;
	public int y = 150;
	public int width = 100;
	public int height = 50;
	public int buttonWidth = 30;

	private WidgetSlider slider = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create slider with specified dimensions
		slider = new WidgetSlider(gui);
		slider.Width = width;
		slider.Height = height;
		slider.ButtonWidth = buttonWidth;
		slider.SetPosition(x, y);
		slider.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Slider: {slider.Value}"));

		gui.AddChild(slider, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes slider from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(slider);
		Unigine.Console.Onscreen = false;
	}
}
