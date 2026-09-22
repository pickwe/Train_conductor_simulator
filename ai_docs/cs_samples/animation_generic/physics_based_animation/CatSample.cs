// Main controller for the cat animation sample. Manages two modes: Demo mode
// (showcases different easing algorithms) and Game mode (interactive catch game).
// Toggle buttons allow switching between modes via the sample description window.

using Unigine;

// Main sample component demonstrating physics-based animation with easing functions.
// Shows a cat chasing a laser pointer using different motion interpolation methods:
// Linear, EaseIn, EaseOut, EaseOutBounce, and Spring-based motion.
// Two modes available: Demo (automatic showcase) and Game (interactive laser control).
public partial class CatSample : Component
{
	[ShowInEditor]
	public Node demoNode = null;	// Root node for demo mode
	[ShowInEditor]
	public Node gameNode = null;	// Root node for game mode

	private CatDemo demoManager = null;
	private CatGame gameManager = null;

	private SampleDescriptionWindow descriptionWindow = new SampleDescriptionWindow();
	private WidgetLabel titleLabel;
	private WidgetButton demoButton;
	private WidgetButton gameButton;

	private Input.MOUSE_HANDLE mouseHandle;

	// Sample is initialized: mouse handle is configured and mode controllers are obtained.
	private void Init()
	{
		// Keep mouse cursor visible (SOFT mode shows cursor, USER mode hides it)
		mouseHandle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		// Get references to the demo and game mode controllers
		gameManager = ComponentSystem.GetComponent<CatGame>(gameNode);
		if (gameManager == null)
			Log.Error("CatSample.Init(): cannot find CatGame component!\n");

		demoManager = ComponentSystem.GetComponent<CatDemo>(demoNode);
		if (demoManager == null)
			Log.Error("CatSample.Init(): cannot find CatDemo component!\n");

		InitGui();

		// Start with both modes disabled, then activate demo mode
		if (demoManager != null)
			demoManager.Enabled = false;
		if (gameManager != null)
			gameManager.Enabled = false;

		demoButton.Toggled = true;
	}

	// GUI is created: description window, mode toggle buttons, and child GUIs are initialized.
	private void InitGui()
	{
		titleLabel = new WidgetLabel("Move the pointer away from the cat");
		titleLabel.TextAlign = Gui.ALIGN_CENTER;
		titleLabel.FontSize = 40;
		titleLabel.FontOutline = 1;
		Gui.GetCurrent().AddChild(titleLabel, Gui.ALIGN_EXPAND);

		descriptionWindow.createWindow();

		WidgetHBox buttonBox = new WidgetHBox(5);
		descriptionWindow.getParameterGroupBox().AddChild(buttonBox, Gui.ALIGN_BOTTOM);

		gameButton = new WidgetButton("Start Game");
		gameButton.Toggleable = true;
		gameButton.EventClicked.Connect(() => SwitchToGame());
		buttonBox.AddChild(gameButton, Gui.ALIGN_LEFT);
		if (gameManager != null)
			gameManager.InitGui(descriptionWindow.MainWindow);

		demoButton = new WidgetButton("Animations Demo");
		demoButton.Toggleable = true;
		demoButton.EventClicked.Connect(() => SwitchToDemo());
		buttonBox.AddChild(demoButton, Gui.ALIGN_LEFT);
		if (demoManager != null)
			demoManager.InitGui(descriptionWindow.MainWindow);
	}

	// Demo mode is disabled and game mode is activated.
	private void SwitchToGame()
	{
		if (demoManager == null || gameManager == null)
			return;

		demoManager.Enabled = false;
		gameManager.Enabled = true;

		// The event is muted to prevent a recursive callback
		// when programmatically setting the toggle state
		demoButton.EventClicked.Enabled = false;
		demoButton.Toggled = false;
		demoButton.EventClicked.Enabled = true;
	}

	// Game mode is disabled and demo mode is activated.
	private void SwitchToDemo()
	{
		if (demoManager == null || gameManager == null)
			return;

		gameManager.Enabled = false;
		demoManager.Enabled = true;

		// Mute event to avoid triggering SwitchToGame() when untoggling
		gameButton.EventClicked.Enabled = false;
		gameButton.Toggled = false;
		gameButton.EventClicked.Enabled = true;
	}

	// Mouse handle is restored and GUI resources are released.
	private void Shutdown()
	{
		Input.MouseHandle = mouseHandle;
		ShutdownGui();
	}

	// GUI widgets are released via DeleteLater for safe deferred destruction.
	private void ShutdownGui()
	{
		if (titleLabel)
			titleLabel.DeleteLater();
		if (demoButton)
			demoButton.DeleteLater();
		if (gameButton)
			gameButton.DeleteLater();
		descriptionWindow.shutdown();
	}
}
