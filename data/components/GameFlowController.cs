using System;
using Unigine;

/// <summary>
/// Coordinates the shift sequence. Presentation and scenario components own
/// all UI, dialogue, results text, and menu transitions.
/// </summary>
public partial class GameFlowController : Component
{
	public enum Phase
	{
		WaitingForBriefing,
		ScenarioOneIntroduction,
		ScenarioOne,
		ScenarioOneReview,
		ScenarioTwoIntroduction,
		ScenarioTwo,
		ScenarioTwoReview,
		RunSummary,
		Finished
	}

	public Phase CurrentPhase { get; private set; } = Phase.WaitingForBriefing;

	public event Action<Phase> PhaseChanged;
	public event Action ScenarioOneIntroductionRequested;
	public event Action ScenarioOneStarted;
	public event Action ScenarioOneReviewRequested;
	public event Action ScenarioTwoIntroductionRequested;
	public event Action ScenarioTwoStarted;
	public event Action ScenarioTwoReviewRequested;
	public event Action RunSummaryRequested;
	public event Action ReturnToMenuRequested;

	/// <summary>Called by the route briefing component after it closes.</summary>
	public void NotifyBriefingClosed()
	{
		if (CurrentPhase != Phase.WaitingForBriefing) return;
		if (MenuManager.ScenarioOneCompletedForTesting)
		{
			SetPhase(Phase.ScenarioTwoIntroduction);
			ScenarioTwoIntroductionRequested?.Invoke();
			return;
		}
		SetPhase(Phase.ScenarioOneIntroduction);
		ScenarioOneIntroductionRequested?.Invoke();
	}

	/// <summary>Called by the scenario 1 introduction component when the player continues.</summary>
	public void StartScenarioOne()
	{
		if (CurrentPhase != Phase.ScenarioOneIntroduction) return;
		SetPhase(Phase.ScenarioOne);

		// Scenario 1 owns its own branch logic and randomized passenger facts.
		PetCarrierScenarioController scenario = ComponentSystem.FindComponentInWorld<PetCarrierScenarioController>();
		if (scenario != null)
			scenario.StartScenario();
		else
			Log.Warning("GameFlowController: no PetCarrierScenarioController found in the current world.\n");

		ScenarioOneStarted?.Invoke();
	}

	/// <summary>Called by scenario 1 when its final dialogue line is complete.</summary>
	public void NotifyScenarioOneFinished()
	{
		if (CurrentPhase != Phase.ScenarioOne) return;
		SetPhase(Phase.ScenarioOneReview);
		ScenarioOneReviewRequested?.Invoke();
	}

	/// <summary>Called by the scenario 1 review component when the player continues.</summary>
	public void StartScenarioTwoIntroduction()
	{
		if (CurrentPhase != Phase.ScenarioOneReview) return;
		SetPhase(Phase.ScenarioTwoIntroduction);
		ScenarioTwoIntroductionRequested?.Invoke();
	}

	/// <summary>Called by the scenario 2 introduction component when the player continues.</summary>
	public void StartScenarioTwo()
	{
		if (CurrentPhase != Phase.ScenarioTwoIntroduction) return;
		SetPhase(Phase.ScenarioTwo);
		ScenarioTwoStarted?.Invoke();
	}

	/// <summary>Called by scenario 2 when its final dialogue line is complete.</summary>
	public void NotifyScenarioTwoFinished()
	{
		if (CurrentPhase != Phase.ScenarioTwo) return;
		SetPhase(Phase.ScenarioTwoReview);
		ScenarioTwoReviewRequested?.Invoke();
	}

	/// <summary>Called by the scenario 2 review component when the player continues.</summary>
	public void ShowRunSummary()
	{
		if (CurrentPhase != Phase.ScenarioTwoReview) return;
		SetPhase(Phase.RunSummary);
		RunSummaryRequested?.Invoke();
	}

	/// <summary>Called by the shift summary component when the player chooses to leave.</summary>
	public void FinishRun()
	{
		if (CurrentPhase != Phase.RunSummary) return;
		SetPhase(Phase.Finished);
		ReturnToMenuRequested?.Invoke();
	}

	private void SetPhase(Phase phase)
	{
		CurrentPhase = phase;
		PhaseChanged?.Invoke(phase);
	}
}
