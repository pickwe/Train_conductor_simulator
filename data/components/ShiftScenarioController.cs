using System;
using Unigine;

/// <summary>
/// Coordinates the opening passenger scenario. UI, sound, and interaction
/// components can subscribe to these events and report their results here.
/// </summary>
public partial class ShiftScenarioController : Component
{
	public enum ScenarioStep
	{
		WaitingForBriefing,
		FreeRoam,
		WaitingForPassenger,
		PassengerDialogue,
		AnswerChoice,
		Stage2,
		Stage3
	}

	public float freeRoamSeconds = 30.0f;
	public float answerSeconds = 25.0f;
	public string passengerQuestionId = "passenger_schedule_question";
	public string answerThatLeadsToStage2 = "check_schedule_without_board";

	public ScenarioStep CurrentStep { get; private set; } = ScenarioStep.WaitingForBriefing;
	public float RemainingSeconds { get; private set; }

	public event Action BriefingRequested;
	public event Action PassengerCallRequested;
	public event Action<string> PassengerDialogueRequested;
	public event Action<float> AnswerChoiceRequested;
	public event Action<ScenarioStep> StepChanged;

	private bool briefingRequestPending;

	private void Init()
	{
		CurrentStep = ScenarioStep.WaitingForBriefing;
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
			// No answer before the deadline follows the direct-to-stage-3 path.
			SetStep(ScenarioStep.Stage3);
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

	/// <summary>Call after the player finishes reading the passenger's line.</summary>
	public void NotifyPassengerDialogueRead()
	{
		if (CurrentStep != ScenarioStep.PassengerDialogue)
			return;

		RemainingSeconds = Math.Max(0.0f, answerSeconds);
		SetStep(ScenarioStep.AnswerChoice);
		AnswerChoiceRequested?.Invoke(RemainingSeconds);
	}

	/// <summary>Call with the stable ID of the selected answer.</summary>
	public void SubmitAnswer(string answerId)
	{
		if (CurrentStep != ScenarioStep.AnswerChoice || string.IsNullOrWhiteSpace(answerId))
			return;

		RemainingSeconds = 0.0f;
		SetStep(string.Equals(answerId, answerThatLeadsToStage2, StringComparison.Ordinal)
			? ScenarioStep.Stage2
			: ScenarioStep.Stage3);
	}

	private void SetStep(ScenarioStep step)
	{
		CurrentStep = step;
		StepChanged?.Invoke(step);
	}
}
