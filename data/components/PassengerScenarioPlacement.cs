using Unigine;

/// <summary>Moves the player to an editor-placed marker when a scenario begins.</summary>
public partial class PassengerScenarioPlacement : Component
{
	[Parameter(Group = "Scenario 1")]
	public Node scenarioOneMarker;

	[Parameter(Group = "Scenario 2")]
	public Node scenarioTwoMarker;

	private GameFlowController gameFlow;
	private FP_Controller playerController;
	private bool pendingScenarioOnePlacement;
	private bool pendingScenarioTwoPlacement;
	private bool scenarioOnePlaced;

	private void Update()
	{
		if (gameFlow == null)
		{
			gameFlow = ComponentSystem.FindComponentInWorld<GameFlowController>();
			if (gameFlow != null)
				gameFlow.PhaseChanged += OnPhaseChanged;
		}

		if (playerController == null)
			playerController = ComponentSystem.FindComponentInWorld<FP_Controller>();

		// Apply after the phase-change callbacks have finished for this frame.
		if (pendingScenarioOnePlacement)
		{
			pendingScenarioOnePlacement = false;
			TeleportPlayer(scenarioOneMarker, "scenario 1");
		}

		if (pendingScenarioTwoPlacement)
		{
			pendingScenarioTwoPlacement = false;
			TeleportPlayer(scenarioTwoMarker, "scenario 2");
		}
	}

	private void OnPhaseChanged(GameFlowController.Phase phase)
	{
		// Put the player at the scenario start before its intro is shown.
		if (phase == GameFlowController.Phase.ScenarioOneIntroduction && !scenarioOnePlaced)
			pendingScenarioOnePlacement = true;
		else if (phase == GameFlowController.Phase.ScenarioOne && !scenarioOnePlaced)
			pendingScenarioOnePlacement = true;
		else if (phase == GameFlowController.Phase.ScenarioTwo)
			pendingScenarioTwoPlacement = true;
	}

	private void TeleportPlayer(Node marker, string scenarioLabel)
	{
		if (marker == null)
		{
			Log.Warning("PassengerScenarioPlacement: assign the player marker for {0} in the Editor.\n", scenarioLabel);
			return;
		}

		if (playerController == null)
		{
			Log.Warning("PassengerScenarioPlacement: FP_Controller was not found; cannot teleport the player.\n");
			return;
		}

		playerController.TeleportToMarker(marker);
		if (scenarioLabel == "scenario 1")
			scenarioOnePlaced = true;
	}

	private void Shutdown()
	{
		if (gameFlow != null)
			gameFlow.PhaseChanged -= OnPhaseChanged;
	}
}
