// Demonstrates loading a User Interface from an XML .ui file via C# API.
// Shows how to get widgets by name via GetWidgetByName() and subscribe to events
// for custom handling of user actions (menu selection, text input).

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Sample component for loading UI from XML file and handling widget events.
public partial class UserInterfaceSample : Component
{
	// Path to the .ui file containing XML UI definition
	public AssetLink ui_file;

	// Loaded UI interface and main window widget
	private UserInterface ui;
	private Widget window;

	// Loads UI from file, finds widgets by name, and subscribes to events.
	void Init()
	{
		if (ui_file.Path.Length < 1)
		{
			Log.Warning("UserInterfaceSample::init(): ui_file is not assigned.\n");
			return;
		}

		Gui gui = Gui.GetCurrent();

		// Load UI definition from XML file
		ui = new UserInterface(gui, ui_file.Path);
		if (!ui)
			Log.Error("UserInterfaceSample::init(): can't created UserInterface.\n");

		// Find widgets by name and connect event handlers
		ui.GetWidgetByName("edittext").EventChanged.Connect(EdittextChanged);
		ui.GetWidgetByName("menubox_0").EventClicked.Connect(Menubox0Clicked);

		// Get main window widget and add to GUI
		window = ui.GetWidget(ui.FindWidget("window"));
		window.Arrange();
		gui.AddChild(window, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);

		Console.Onscreen = true;
	}

	// Cleans up UI resources.
	void Shutdown()
	{
		if (window)
			window.DeleteLater();
		if (ui)
			ui.DeleteLater();

		Console.Onscreen = false;
	}

	// Handles text changes in the EditText widget.
	private void EdittextChanged (Widget widget)
	{
		WidgetEditText edittext = widget as WidgetEditText;
		Log.Message("EditText changed: {0}\n", edittext.Text);
	}

	// Handles menu item selection - quits application on third item.
	private void Menubox0Clicked(Widget widget)
	{
		WidgetMenuBox menubox = widget as WidgetMenuBox;
		Log.Message("MenuBox clicked: {0}\n", menubox.CurrentItemText);
		if (menubox.CurrentItem == 2)
			Unigine.Console.Run("quit");
	}

}
