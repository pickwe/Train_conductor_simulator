// Demonstrates WidgetSpinBox - numeric input with up/down buttons.
// Shows how to attach spinbox to an editline for combined text/button input.

using Unigine;

// Numeric spinner with increment/decrement buttons attached to text input.
public partial class WidgetsSpinbox : Component
{
	public int x = 625;
	public int y = 300;

	private WidgetSpinBox spinBox = null;
	private WidgetEditLine spinBoxLine = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create editline for numeric display
		spinBoxLine = new WidgetEditLine(gui, "0");
		spinBoxLine.SetPosition(x, y);
		spinBoxLine.FontOutline = 1;

		gui.AddChild(spinBoxLine, Gui.ALIGN_OVERLAP);

		// Create spinbox (up/down buttons) and attach to editline
		spinBox = new WidgetSpinBox(gui);
		spinBox.Order = spinBoxLine.Order + 1;
		spinBoxLine.AddAttach(spinBox);

		spinBox.EventChanged.Connect(() => Unigine.Console.OnscreenMessageLine($"Spinbox: {spinBox.Value}"));

		gui.AddChild(spinBox, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes spinbox and attached editline from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(spinBox);
		Gui.GetCurrent().RemoveChild(spinBoxLine);
		Unigine.Console.Onscreen = false;
	}
}
