// Basic WidgetWindow example showing a draggable, resizable window.
// Contains an editable text field and a button, both with event handling
// that logs user interactions to the on-screen console.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Creates a simple window with text input and button.
public partial class Window : Component
{
	private WidgetWindow window;

	// Creates window with edit field and button, wired to log events.
	void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create centered, resizable window
		window = new WidgetWindow(gui, "Hello from C#", 4, 4);
		window.Flags = Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER;
		window.Width = 320;
		window.Sizeable = true;

		// Single-line text input with change event
		var editline = new WidgetEditLine(gui, "Edit me");
		editline.Flags = Gui.ALIGN_EXPAND;
		window.AddChild(editline);
		editline.EventChanged.Connect(widget => {
			WidgetEditLine el = widget as WidgetEditLine;
			Log.Message("EditLine changed: {0}\n", el.Text);
		});
		editline.FontSize = 16;

		// Button with click event
		var button = new WidgetButton(gui, "Press me");
		button.Flags = Gui.ALIGN_EXPAND;
		window.AddChild(button);
		button.EventClicked.Connect(() => Log.Message("Button pressed\n"));
		button.FontSize = 18;

		// Finalize layout and show window
		window.Arrange();
		Gui.GetCurrent().AddChild(window);

		// Enable on-screen console to see event logs
		Console.Onscreen = true;
	}

	void Shutdown()
	{
		Console.Onscreen = false;

		window.DeleteLater();
	}
}
