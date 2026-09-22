// Demonstrates radio button behavior using attached WidgetCheckBox widgets.
// AddAttach() links checkboxes so only one can be selected at a time.

using Unigine;

// Radio button group using checkbox attachment for mutual exclusion.
public partial class WidgetsRadioButtons : Component
{
	// Position and layout settings
	[ShowInEditor]
	vec2 widgetPosition = new vec2(600, 450);

	[ShowInEditor]
	int fontSize = 16;

	[ShowInEditor]
	int horizontaLayoutSpace = 4;

	[ShowInEditor]
	int verticalLayoutSpace = 4;

	// Button labels
	[ShowInEditor]
	string firstButtontext = "Check Me";

	[ShowInEditor]
	string secondButtontext = "Or Me";

	// Layout container and checkbox widgets
	WidgetVBox _verticalLayout = null;
	WidgetCheckBox _firstCheckBox = null;
	WidgetCheckBox _secondCheckBox = null;

	// Saved console state for cleanup
	bool _consoleOnScreenState = false;

	void Init()
	{
		var gui = Gui.GetCurrent();

		// Create vertical layout container
		_verticalLayout = new WidgetVBox(horizontaLayoutSpace, verticalLayoutSpace)
		{
			PositionX = (int)widgetPosition.x,
			PositionY = (int)widgetPosition.y,
			Background = 1
		};

		gui.AddChild(_verticalLayout, Gui.ALIGN_OVERLAP);

		// First radio button - selected by default
		_firstCheckBox = new WidgetCheckBox(firstButtontext)
		{
			Checked = true,
			FontSize = fontSize
		};

		_verticalLayout.AddChild(_firstCheckBox, Gui.ALIGN_LEFT);

		_firstCheckBox.EventClicked.Connect(() =>
		{
			if (_firstCheckBox.Checked)
				Console.OnscreenMessageLine("Radio buttons: first option");
		});

		// Second radio button
		_secondCheckBox = new WidgetCheckBox(secondButtontext)
		{
			FontSize = fontSize
		};

		_verticalLayout.AddChild(_secondCheckBox, Gui.ALIGN_LEFT);
		// AddAttach creates radio button behavior - only one can be checked
		_firstCheckBox.AddAttach(_secondCheckBox);

		_secondCheckBox.EventClicked.Connect(() =>
		{
			if (_secondCheckBox.Checked)
				Console.OnscreenMessageLine("Radio buttons: second option");
		});

		_consoleOnScreenState = Console.Onscreen;
		Console.Onscreen = true;
	}

	// Restores console state and cleans up layout container.
	void Shutdown()
	{
		Console.Onscreen = _consoleOnScreenState;
		_verticalLayout.DeleteLater();
	}
}
