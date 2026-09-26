using System;
using System.Collections.Generic;
using Unigine;

/// <summary>
/// Coordinates the opening passenger scenario. UI, sound, and interaction
/// components can subscribe to these events and report their results here.
/// </summary>
public partial class PassengerScenarioController : Component
{
	public enum ScenarioStep
	{
		WaitingForBriefing,
		FreeRoam,
		WaitingForPassenger,
		PassengerDialogue,
		AwaitingDisplayChecks,
		AnswerChoice,
		Stage2,
		Stage3
	}

	public float freeRoamSeconds = 30.0f;
	public float answerSeconds = 25.0f;
	public string passengerQuestionId = "passenger_schedule_question";
	public string answerThatLeadsToStage2 = "compare_board_with_schedule";
	public List<AnswerOutcomeRule> answerRules = new List<AnswerOutcomeRule>();

	public struct AnswerOutcomeRule
	{
		public string answerId;
		public int safetyDelta;
		public int loyaltyDelta;
		public int regulationDelta;
		public int observationDelta;
		public List<StoryStateController.StoryFlag> flagsToSet;
		public List<StoryStateController.StoryFlag> flagsToClear;
	}

	public ScenarioStep CurrentStep { get; private set; } = ScenarioStep.WaitingForBriefing;
	public float RemainingSeconds { get; private set; }
	public bool CarDisplayChecked { get; private set; }
	public bool CabDisplayChecked { get; private set; }

	public event Action BriefingRequested;
	public event Action PassengerCallRequested;
	public event Action<string> PassengerDialogueRequested;
	public event Action<float> AnswerChoiceRequested;
	public event Action<ScenarioStep> StepChanged;
	public event Action<string> AnswerSubmitted;

	private bool briefingRequestPending;
	private string pendingAnswerId = "";
	private void Init()
	{
		CurrentStep = ScenarioStep.WaitingForBriefing;
		CarDisplayChecked = false;
		CabDisplayChecked = false;
		briefingRequestPending = true;
	}

	private void Update()
	{
		// Defer the briefing signal until all scene components have initialized
		// and had a chance to subscribe to it.
		if (briefingRequestPending)
		{
			briefingRequestPending = false;
			BriefingRequested?.Invoke();
		}

		if (CurrentStep != ScenarioStep.FreeRoam && CurrentStep != ScenarioStep.AnswerChoice)
			return;

		RemainingSeconds = Math.Max(0.0f, RemainingSeconds - Game.IFps);
		if (RemainingSeconds > 0.0f)
			return;

		if (CurrentStep == ScenarioStep.FreeRoam)
		{
			SetStep(ScenarioStep.WaitingForPassenger);
			PassengerCallRequested?.Invoke();
		}
		else
		{
			RecordAnswer("wait_timeout");
		}
	}

	/// <summary>Call when the separate route briefing has been closed.</summary>
	public void NotifyBriefingClosed()
	{
		if (CurrentStep != ScenarioStep.WaitingForBriefing)
			return;

		RemainingSeconds = Math.Max(0.0f, freeRoamSeconds);
		SetStep(ScenarioStep.FreeRoam);
	}

	/// <summary>Call when the player presses E on the passenger.</summary>
	public void NotifyPassengerInteracted()
	{
		if (CurrentStep != ScenarioStep.WaitingForPassenger)
			return;

		SetStep(ScenarioStep.PassengerDialogue);
		PassengerDialogueRequested?.Invoke(passengerQuestionId);
	}

	/// <summary>Called by the dialogue UI after the final configured dialogue line.</summary>
	public void NotifyPassengerDialogueRead()
	{
		if (CurrentStep != ScenarioStep.PassengerDialogue) return;
		RemainingSeconds = Math.Max(0.0f, answerSeconds);
		SetStep(ScenarioStep.AnswerChoice);
		AnswerChoiceRequested?.Invoke(RemainingSeconds);
	}

	public void NotifyDisplayCheckedDuringInspection(bool isCabDisplay)
	{
		if (isCabDisplay)
		{
			if (CurrentStep != ScenarioStep.Stage2 || CabDisplayChecked) return;
			CabDisplayChecked = true;
			SetInformationVerifiedIfBothDisplaysChecked();
			return;
		}

		if (!CanInspectDisplay(false) || CarDisplayChecked) return;
		CarDisplayChecked = true;
		StoryStateController state = ComponentSystem.FindComponentInWorld<StoryStateController>();
		if (state != null)
		{
			state.SetFlag(StoryStateController.StoryFlag.DisplayFaultDetected, true);
			SetInformationVerifiedIfBothDisplaysChecked(state);
		}

		if (CurrentStep == ScenarioStep.AwaitingDisplayChecks &&
			string.Equals(pendingAnswerId, answerThatLeadsToStage2, StringComparison.Ordinal))
			SetStep(ScenarioStep.Stage2);
	}

	public bool CanInspectDisplay(bool isCabDisplay)
	{
		if (isCabDisplay)
			return CurrentStep == ScenarioStep.Stage2;

		return CurrentStep == ScenarioStep.FreeRoam ||
			CurrentStep == ScenarioStep.WaitingForPassenger ||
			CurrentStep == ScenarioStep.AwaitingDisplayChecks ||
			CurrentStep == ScenarioStep.Stage2;
	}

	/// <summary>Call with the stable ID of the selected answer.</summary>
	public void SubmitAnswer(string answerId)
	{
		if (CurrentStep != ScenarioStep.AnswerChoice || string.IsNullOrWhiteSpace(answerId))
			return;

		RemainingSeconds = 0.0f;
		RecordAnswer(answerId);
	}

	private void RecordAnswer(string answerId)
	{
		pendingAnswerId = answerId;
		ApplyAnswerOutcome(answerId);
		AnswerSubmitted?.Invoke(answerId);
		if (string.Equals(answerId, answerThatLeadsToStage2, StringComparison.Ordinal))
		{
			SetStep(CarDisplayChecked ? ScenarioStep.Stage2 : ScenarioStep.AwaitingDisplayChecks);
		}
		else
		{
			SetStep(ScenarioStep.Stage3);
		}
	}

	private void SetInformationVerifiedIfBothDisplaysChecked(StoryStateController state = null)
	{
		if (!CarDisplayChecked || !CabDisplayChecked) return;
		if (state == null)
			state = ComponentSystem.FindComponentInWorld<StoryStateController>();
		if (state != null)
			state.SetFlag(StoryStateController.StoryFlag.InformationVerified, true);
	}

	private void SetStep(ScenarioStep step)
	{
		CurrentStep = step;
		StepChanged?.Invoke(step);
	}

	private void ApplyAnswerOutcome(string answerId)
	{
		if (answerRules == null || answerRules.Count == 0)
			return;

		AnswerOutcomeRule? matchingRule = null;
		foreach (AnswerOutcomeRule rule in answerRules)
		{
			if (string.Equals(rule.answerId, answerId, StringComparison.Ordinal))
			{
				matchingRule = rule;
				break;
			}
		}

		if (!matchingRule.HasValue)
			return;

		StoryStateController state = ComponentSystem.FindComponentInWorld<StoryStateController>();
		if (state == null)
		{
			Log.Warning("PassengerScenarioController: no StoryStateController found in the current world.\n");
			return;
		}

		AnswerOutcomeRule outcome = matchingRule.Value;
		if (outcome.safetyDelta != 0) state.ChangeMetric(StoryStateController.Metric.Safety, outcome.safetyDelta);
		if (outcome.loyaltyDelta != 0) state.ChangeMetric(StoryStateController.Metric.Loyalty, outcome.loyaltyDelta);
		if (outcome.regulationDelta != 0) state.ChangeMetric(StoryStateController.Metric.Regulation, outcome.regulationDelta);
		if (outcome.observationDelta != 0) state.ChangeMetric(StoryStateController.Metric.Observation, outcome.observationDelta);

		if (outcome.flagsToSet != null)
			foreach (StoryStateController.StoryFlag flag in outcome.flagsToSet) state.SetFlag(flag, true);
		if (outcome.flagsToClear != null)
			foreach (StoryStateController.StoryFlag flag in outcome.flagsToClear) state.SetFlag(flag, false);
	}
}
