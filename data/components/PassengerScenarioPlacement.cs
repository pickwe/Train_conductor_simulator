using Unigine;

/// <summary>Places the player at the initial spawn and moves them for later scenarios.</summary>
public partial class PassengerScenarioPlacement : Component
{
	[Parameter(Group = "Scenario 1")]
	public Node scenarioOneMarker;

	[Parameter(Group = "Scenario 2")]
	public Node scenarioTwoMarker;

	private GameFlowController gameFlow;
	private FP_Controller playerController;
	private bool pendingScenarioTwoPlacement;
	private bool scenarioOnePlaced;
	private bool scenarioOneMarkerWarningShown;

	private void Init()
	{
		TryPlaceScenarioOneImmediately();
	}

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

		TryPlaceScenarioOneImmediately();

		// Scenario 2 still changes the player's location after its introduction.
		if (pendingScenarioTwoPlacement)
		{
			pendingScenarioTwoPlacement = false;
			TeleportPlayer(scenarioTwoMarker, "scenario 2");
		}
	}

	private void OnPhaseChanged(GameFlowController.Phase phase)
	{
		if (phase == GameFlowController.Phase.ScenarioTwo)
			pendingScenarioTwoPlacement = true;
	}

	private void TryPlaceScenarioOneImmediately()
	{
		if (scenarioOnePlaced)
			return;
		if (scenarioOneMarker == null)
		{
			if (!scenarioOneMarkerWarningShown)
			{
				scenarioOneMarkerWarningShown = true;
				Log.Warning("PassengerScenarioPlacement: assign the player marker for scenario 1 in the Editor.\n");
			}
			return;
		}
		if (playerController == null)
			playerController = ComponentSystem.FindComponentInWorld<FP_Controller>();
		if (playerController == null || !playerController.IsInitialized)
			return;

		TeleportPlayer(scenarioOneMarker, "scenario 1");
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
