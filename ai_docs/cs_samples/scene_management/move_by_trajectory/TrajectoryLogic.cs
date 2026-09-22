// Demo controller that manages three airplanes using different trajectory systems.
// Demonstrates all three approaches to path-following simultaneously:
// - airplane1: SimpleTrajectoryMovement (linear interpolation)
// - airplane2: SplineTrajectoryMovement (real-time Catmull-Rom spline)
// - airplane3: SavedPathTrajectory (pre-baked path file with WorldTransformPath)
// Also provides camera switching between overview and each airplane's viewpoint.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Orchestrates the trajectory sample demo with shared controls.
public partial class TrajectoryLogic : Component
{
	// Airplane using SimpleTrajectoryMovement (linear waypoint interpolation)
	[ShowInEditor]
	private Node airplane1 = null;
	// Airplane using SplineTrajectoryMovement (smooth Catmull-Rom curve)
	[ShowInEditor]
	private Node airplane2 = null;
	// Airplane using SavedPathTrajectory (WorldTransformPath from file)
	[ShowInEditor]
	private Node airplane3 = null;

	// Checkbox widget for toggling path visualization
	private WidgetCheckBox enableVisualizePath;

	// Camera indices for cycling through viewpoints
	private enum Players
	{
		MAIN = 0,    // Overview camera
		ONE,         // Follow airplane1
		TWO,         // Follow airplane2
		THREE,       // Follow airplane3
		TOTAL_PLAYERS
	}

	// All cameras in the scene marked as main players
	private List<Player> mainPlayers = new List<Player>();
	// Currently active camera
	Players currentPlayer = Players.MAIN;

	// UI window for shared velocity control and camera switching
	SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();

	// Gathers cameras and initializes UI controls.
	void Init()
	{
		Game.GetMainPlayers(mainPlayers);
		Game.Player = mainPlayers[(int)currentPlayer];

		InitGui();
		Visualizer.Enabled = true;
	}

	// Cleans up UI on shutdown.
	void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
		Visualizer.Enabled = false;
	}

	// Creates velocity slider, camera switch button, and path visualization checkbox.
	private void InitGui()
	{
		sampleDescriptionWindow.createWindow();

		sampleDescriptionWindow.addFloatParameter("Velocity", "Velicity", 5.0f, 1.0f, 50.0f, (float v) => {
			GetComponent<SimpleTrajectoryMovement>(airplane1).Velocity = v;
			GetComponent<SplineTrajectoryMovement>(airplane2).Velocity = v;
			GetComponent<SavedPathTrajectory>(airplane3).Velocity = v;
		});

		WidgetGroupBox parameters = sampleDescriptionWindow.getParameterGroupBox();

		var changeCameraButton = new WidgetButton();
		parameters.AddChild(changeCameraButton, Gui.ALIGN_LEFT);
		changeCameraButton.Text = "Switch Camera";
		changeCameraButton.EventClicked.Connect(SwitchTrajectoryCallback);

		var gridbox = new WidgetGridBox();
		parameters.AddChild(gridbox, Gui.ALIGN_LEFT);
		enableVisualizePath = new WidgetCheckBox();
		enableVisualizePath.Checked = false;
		enableVisualizePath.EventClicked.Connect(EnableVisualizeCallback);
		var visualizeLabel = new WidgetLabel("Visualize Path");
		gridbox.AddChild(visualizeLabel);
		gridbox.AddChild(enableVisualizePath);
	}

	// Cycles through cameras: overview -> airplane1 -> airplane2 -> airplane3 -> overview...
	private void SwitchTrajectoryCallback()
	{
		currentPlayer = (Players)(((int)currentPlayer + 1) % (int)Players.TOTAL_PLAYERS);
		Game.Player = mainPlayers[(int)currentPlayer];
	}

	// Toggles debug path visualization on all three trajectory components.
	private void EnableVisualizeCallback()
	{
		bool enabled = enableVisualizePath.Checked;
		GetComponent<SimpleTrajectoryMovement>(airplane1).Debug = enabled;
		GetComponent<SplineTrajectoryMovement>(airplane2).Debug = enabled;
		GetComponent<SavedPathTrajectory>(airplane3).Debug = enabled;
	}
}
