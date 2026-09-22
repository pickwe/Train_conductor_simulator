// Demonstrates WidgetMenuBar with dropdown WidgetMenuBox menus.
// Shows File/Edit/Help menu structure with click event handling.

using Unigine;

// Menu bar with dropdown menu boxes.
public partial class WidgetsMenu : Component
{
	public int x = 450;
	public int y = 300;
	public int fontSize = 16;

	// Highlight color for selected menu items
	[ParameterColor]
	public vec4 selectionColor = vec4.ZERO;

	private WidgetMenuBar menuBar = null;

	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Create menu bar with three top-level items
		menuBar = new WidgetMenuBar(gui);
		menuBar.SelectionColor = selectionColor;
		menuBar.AddItem("File");
		menuBar.AddItem("Edit");
		menuBar.AddItem("Help");
		menuBar.SetPosition(x, y);
		menuBar.FontSize = fontSize;
		menuBar.FontOutline = 1;

		// File menu dropdown
		WidgetMenuBox fileMenuBox = new WidgetMenuBox(gui);
		fileMenuBox.FontSize = fontSize;
		fileMenuBox.FontOutline = 1;
		fileMenuBox.AddItem("File 0");
		fileMenuBox.AddItem("File 1");
		fileMenuBox.AddItem("File 2");
		fileMenuBox.EventClicked.Connect(() => Unigine.Console.OnscreenMessageLine($"Menubar: {fileMenuBox.CurrentItemText}"));
		menuBar.SetItemMenu(0, fileMenuBox);

		// Edit menu dropdown
		WidgetMenuBox editMenuBox = new WidgetMenuBox(gui);
		editMenuBox.FontSize = fontSize;
		editMenuBox.FontOutline = 1;
		editMenuBox.AddItem("Edit 0");
		editMenuBox.AddItem("Edit 1");
		editMenuBox.AddItem("Edit 2");
		editMenuBox.EventClicked.Connect(() => Unigine.Console.OnscreenMessageLine($"Menubar: {editMenuBox.CurrentItemText}"));
		menuBar.SetItemMenu(1, editMenuBox);

		// Help menu dropdown
		WidgetMenuBox helpMenuBox = new WidgetMenuBox(gui);
		helpMenuBox.FontSize = fontSize;
		helpMenuBox.FontOutline = 1;
		helpMenuBox.AddItem("Help 0");
		helpMenuBox.AddItem("Help 1");
		helpMenuBox.AddItem("Help 2");
		helpMenuBox.EventClicked.Connect(() => Unigine.Console.OnscreenMessageLine($"Menubar: {helpMenuBox.CurrentItemText}"));
		menuBar.SetItemMenu(2, helpMenuBox);

		gui.AddChild(menuBar, Gui.ALIGN_OVERLAP);

		Unigine.Console.Onscreen = true;
	}

	// Removes menu bar from GUI and disables on-screen console.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(menuBar);
		Unigine.Console.Onscreen = false;
	}
}
