// Implements a simple game where player must keep laser pointer away from the cat.
// Difficulty increases over time by ramping up spring stiffness and damping, making
// the cat faster and more responsive. Game ends when cat catches the laser.

using Unigine;

// Interactive game mode where the player controls a laser pointer.
// The cat uses spring physics to chase the laser with increasing
// difficulty over time (stiffness and damping increase).
// Game ends when the cat catches the laser.
public partial class CatGame : Component
{
	[ShowInEditor]
	public Node laserNode = null;
	[ShowInEditor]
	public Node catMode = null;

	private float timer = 0.0f;
	private bool caught = false;

	private SpringRegular catSpring = null;
	private Laser laser = null;

	private WidgetGroupBox gameBox;
	private WidgetLabel gameLabel;

	// References to Laser and SpringRegular components are obtained from property nodes.
	[MethodInit(Order = -1)]
	private void Init()
	{
		laser = ComponentSystem.GetComponent<Laser>(laserNode);
		if (laser == null)
			Log.Error("CatGame.Init(): cannot find Laser component!\n");

		catSpring = ComponentSystem.GetComponent<SpringRegular>(catMode);
		if (catSpring == null)
			Log.Error("CatGame.Init(): cannot find SpringRegular component!\n");
	}

	// Game mode GUI elements are created and added to the sample window.
	public void InitGui(WidgetWindow window)
	{
		gameBox = new WidgetGroupBox("Game", 8, 4);
		gameLabel = new WidgetLabel($"Don't let the cat catch the laser pointer!\nTime: {timer:0.00}");
		gameLabel.FontVSpacing = 4;
		gameBox.AddChild(gameLabel, Gui.ALIGN_LEFT);
		window.AddChild(gameBox, Gui.ALIGN_LEFT);
	}

	// Called when game mode is activated. Game state is reset and cat motion is enabled.
	protected override void OnEnable()
	{
		// The GUI is built by CatSample after this component is initialized
		if (gameBox == null)
			return;

		caught = false;
		timer = 0.0f;
		if (catSpring != null)
			catSpring.Enabled = true;
		gameBox.Hidden = false;
	}

	// Called when game mode is deactivated. Cat motion is disabled and laser control is restored.
	protected override void OnDisable()
	{
		if (catSpring != null)
			catSpring.Enabled = false;
		if (laser != null)
			laser.Enabled = true;

		if (gameBox == null)
			return;

		gameBox.Hidden = true;
	}

	// Game state is updated: either active gameplay or game over screen is processed.
	// Updated after the laser and the cat's movement components.
	[MethodUpdate(Order = 1)]
	private void Update()
	{
		if (catSpring == null || gameLabel == null)
			return;

		if (caught)
			UpdateGameOver();
		else
			UpdateGame();
	}

	// Active gameplay is processed: difficulty increases and catch condition is checked.
	private void UpdateGame()
	{
		timer += Game.IFps;

		// Increase difficulty over time: cat becomes faster and more responsive.
		// Stiffness controls how quickly the spring snaps to target.
		// Damping controls oscillation reduction (higher = less bouncy).
		catSpring.Stiffness += Game.IFps * 2.0f;
		catSpring.Damping += Game.IFps * 0.25f;

		// Check if cat has caught the laser (spring finished oscillating at target)
		caught = catSpring.Finished;
		if (caught && laser != null)
			laser.Enabled = false;

		gameLabel.Text = $"Don't let the cat catch the laser pointer!\nTime: {timer:0.00}";
	}

	// Game over state is processed. Restart is available via Enter key.
	private void UpdateGameOver()
	{
		// Game is restarted when Enter key is pressed
		if (Input.IsKeyDown(Input.KEY.ENTER))
		{
			caught = false;
			timer = 0.0f;
			catSpring.RefreshSpring();
			if (laser != null)
				laser.Enabled = true;
		}

		gameLabel.Text = $"GAME OVER! Press Enter to restart\nTime: {timer:0.00}";
	}

	private void Shutdown()
	{
		ShutdownGui();
	}

	// GUI widgets are released via DeleteLater for safe deferred destruction.
	private void ShutdownGui()
	{
		if (gameLabel)
			gameLabel.DeleteLater();
		if (gameBox)
			gameBox.DeleteLater();
	}
}
