using System;
using System.Collections.Generic;
using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

/// <summary>Controls scenario 2: passenger call, complaint, parent dialogue and outcome branches.</summary>
public partial class PassengerScenarioController : Component
{
	public class PassengerSeatVariant
	{
		public Node passengerNode;
		[ParameterFile(Filter = ".png|.texture")]
		public string monitorSeatSpritePath = "";
		public Player dialogueCamera;
	}

	private enum PassengerAnimationState
	{
		TurnToPlayer,
		Aggressive,
		Confused,
		Displeased,
		PositiveNod,
		ReturnToIdle
	}

	private sealed class PassengerAnimationSet
	{
		public Node TurnToPlayer;
		public Node Aggressive;
		public Node Confused;
		public Node Displeased;
		public Node PositiveNod;
		public Node ReturnToIdle;
		public Node Work;
	}

	public enum ScenarioStep
	{
		WaitingForBriefing,
		WaitingForScenarioTwo,
		FreeRoam,
		WaitingForPassenger,
		Opening,
		Complaint,
		ComplaintChoice,
		PassengerReaction,
		InternalPrompt,
		AwaitingDisplayChecks,
		AwaitingParentInteraction,
		ParentOpening,
		ParentResponse,
		ParentChoice,
		ParentReminder,
		ParentChoiceGrace,
		ParentBranchReaction,
		AwaitingActivityPickup,
		AwaitingActivityHandoff,
		ComplainantBranchReaction,
		ParentTimeoutReaction,
		Stage3ConductorAppeal,
		Stage3PassengerResponse,
		Stage3SeatCheckPrompt,
		AwaitingSeatCheck,
		SeatCheckCompleted,
		SeatDecisionChoice,
		SeatDecisionReaction,
		SeatInvalidDiscovery,
		SeatInvalidReaction,
		SeatDecisionTimeoutComplainant,
		SeatDecisionTimeoutParent,
		Finished
	}

	[Parameter(Group = "Scenario 2 Timers")]
	public float adaptationSeconds = 30.0f;
	[Parameter(Group = "Scenario 2 Timers")]
	public float passengerSearchSeconds = 60.0f;
	[Parameter(Group = "Scenario 2 Timers")]
	public float parentAnswerSeconds = 30.0f;
	[Parameter(Group = "Scenario 2 Timers")]
	public float parentAnswerGraceSeconds = 10.0f;

	[Parameter(Group = "Scenario 2 Nodes")]
	public Node normalMonitorSpriteNode;
	[Parameter(Title = "Passenger Seat Variants", Group = "Scenario 2 Random Passenger Seats")]
	public List<PassengerSeatVariant> passengerSeatVariants = new List<PassengerSeatVariant>
	{
		new PassengerSeatVariant(),
		new PassengerSeatVariant(),
		new PassengerSeatVariant(),
		new PassengerSeatVariant(),
		new PassengerSeatVariant()
	};
	[Parameter(Group = "Scenario 2 Nodes")]
	public Node parentPassengerNode;
	[Parameter(Group = "Scenario 2 Nodes")]
	public Node activityItemNode;
	[Parameter(Group = "Scenario 2 Nodes")]
	public string activityItemNodeName = "Книга";
	[Parameter(Group = "Scenario 2 Call Monitor")]
	public Node callMonitorInteractionNode;
	[Parameter(Group = "Scenario 2 Call Monitor")]
	public string callMonitorInteractionNodeName = "измененное табло";
	[Parameter(Group = "Scenario 2 Call Monitor")]
	public Player callMonitorCamera;
	[Parameter(Group = "Scenario 2 Call Monitor")]
	public string callMonitorCameraName = "see_monitor";
	[ParameterSlider(Min = 0.2f, Max = 3.0f, Group = "Scenario 2 Call Monitor")]
	public float callMonitorTransitionSeconds = 0.8f;
	[Parameter(Group = "Scenario 3 Timers")]
	public float seatDecisionSeconds = 30.0f;
	[Parameter(Group = "Scenario 3 Bad Situation")]
	public Node badSituationPassengerNodeOne;
	[Parameter(Group = "Scenario 3 Bad Situation")]
	public Node badSituationPassengerNodeTwo;
	[Parameter(Group = "Scenario 3 Bad Situation")]
	public Player badSituationCamera;
	[Parameter(Group = "Scenario 3 Bad Situation")]
	public string badSituationCameraName = "bad_situation";
	[Parameter(Group = "Scenario 2 Dialogue Cameras")]
	public Player parentDialogueCamera;
	[Parameter(Group = "Scenario 2 Dialogue Cameras")]
	public Player bothPassengersDialogueCamera;
	[ParameterSlider(Min = 0.2f, Max = 3.0f, Group = "Scenario 2 Dialogue Cameras")]
	public float dialogueCameraTransitionSeconds = 0.8f;
	[ParameterSlider(Min = 0.2f, Max = 2.0f, Group = "Scenario 2 Activity Item")]
	public float activityPickupAnimationSeconds = 0.8f;
	public static PassengerScenarioController Active { get; private set; }
	public static bool IsSeatMonitorViewVisible { get; private set; }
	public ScenarioStep CurrentStep { get; private set; } = ScenarioStep.WaitingForBriefing;
	public float RemainingSeconds { get; private set; }
	public bool IsStarted { get; private set; }
	public bool DialogueActive { get; private set; }
	public bool PassengerApproachTimedOut { get; private set; }
	public bool OnboardActivitiesAvailable { get; private set; }
	public bool ActivitiesFactChecked { get; private set; }
	public bool CanOpenTerminal => IsStarted && (CurrentStep == ScenarioStep.AwaitingDisplayChecks
		|| CurrentStep == ScenarioStep.AwaitingSeatCheck
		|| CurrentStep == ScenarioStep.SeatCheckCompleted);
	public bool RelocationSeatAvailable { get; private set; }
	public bool SeatAvailabilityFactChecked { get; private set; }
	public int SelectedPassengerSeatIndex { get; private set; } = -1;
	public Node SelectedPassengerNode { get; private set; }
	public int InitialMood { get; private set; }
	public int InitialParentMood { get; private set; }
	public int MoodPassenger1 { get; private set; }
	public int MoodPassenger2 { get; private set; }
	public int SafetyDelta { get; private set; }
	public int LoyaltyDelta { get; private set; }
	public List<string> DecisionHistory { get; } = new List<string>();
	public bool DiplomatWithoutPropsEligible { get; private set; }
	public string CurrentSpeaker { get; private set; } = "";
	public string CurrentText { get; private set; } = "";
	public bool HasChoices => CurrentStep == ScenarioStep.ComplaintChoice
		|| CurrentStep == ScenarioStep.ParentChoice
		|| CurrentStep == ScenarioStep.ParentChoiceGrace
		|| CurrentStep == ScenarioStep.SeatDecisionChoice;

	public event Action PassengerCallRequested;
	public event Action<ScenarioStep> StepChanged;
	public event Action Changed;
	public event Action<float> TimerChanged;
	public event Action<string, string> SecondScenarioResultRequested;

	private readonly System.Random random = new System.Random();
	private GameFlowController gameFlow;
	private bool subscribedToFlow;
	private bool duplicate;
	private bool monitorWarningShown;
	private bool terminalOpen;
	private int lastDisplayedSecond = -1;
	private EInteractable parentInteractable;
	private EInteractable subscribedParentInteractable;
	private EInteractable activityItemInteractable;
	private EInteractable subscribedActivityItemInteractable;
	private EInteractable seatMonitorInteractable;
	private EInteractable subscribedSeatMonitorInteractable;
	private bool activityPickupAnimating;
	private float activityPickupElapsed;
	private Vec3 activityPickupStartPosition;
	private Gui gui;
	private WidgetSprite callMonitorSeatSprite;
	private WidgetLabel seatMonitorExitHint;
	private PlayerDummy seatMonitorTransitionCamera;
	private Player playerBeforeSeatMonitor;
	private bool seatMonitorViewActive;
	private bool seatMonitorTransitionEntering;
	private bool seatMonitorTransitionLeaving;
	private float seatMonitorTransitionElapsed;
	private Vec3 seatMonitorTransitionStartPosition;
	private vec3 seatMonitorTransitionStartDirection;
	private vec3 seatMonitorTransitionStartUp;
	private float seatMonitorTransitionStartFov;
	private float savedPhysicsScale = 1.0f;
	private Input.MOUSE_HANDLE savedMonitorMouseHandle;
	private bool savedMonitorMouseGrab;
	private bool savedMonitorCursorHide;
	private bool savedSeatMonitorCameraControlled;
	private string selectedParentChoiceId = "";
	private string selectedSeatChoiceId = "";
	private Player playerBeforeBadSituation;
	private bool badSituationActive;
	private bool savedBadSituationCameraControlled;
	private float savedBadSituationPhysicsScale = 1.0f;
	private PlayerDummy dialogueTransitionCamera;
	private Player playerBeforeDialogueCamera;
	private Player currentDialogueTarget;
	private Player pendingDialogueTarget;
	private bool dialogueCameraActive;
	private bool dialogueCameraTransitioning;
	private bool dialogueCameraReturning;
	private float dialogueCameraTransitionElapsed;
	private Vec3 dialogueCameraTransitionStartPosition;
	private vec3 dialogueCameraTransitionStartDirection;
	private vec3 dialogueCameraTransitionStartUp;
	private float dialogueCameraTransitionStartFov;
	private float savedDialoguePhysicsScale = 1.0f;
	private bool savedComplainantCameraControlled;
	private bool savedParentCameraControlled;
	private bool savedBothCameraControlled;
	private Player selectedComplainantDialogueCamera;
	private string selectedPassengerSeatSpritePath = "";
	private readonly Dictionary<Node, PassengerAnimationSet> passengerAnimationSets = new Dictionary<Node, PassengerAnimationSet>();
	private ObjectGui callMonitorSurfaceGui;
	private WidgetSprite callMonitorSurfaceSprite;
	private string appliedCallMonitorTexturePath = "";

	private void Init()
	{
		if (Active != null && Active != this)
		{
			duplicate = true;
			return;
		}

		Active = this;
		IsSeatMonitorViewVisible = false;
		CreateSeatMonitorView();
		ResolveNodes();
		PreparePassengerAnimations();
		BindParentInteraction();
		BindActivityItemInteraction();
		BindSeatMonitorInteraction();
		SetPassengerVisible(false);
		SetParentPassengerVisible(false);
		SetActivityItemVisible(false);
		SetBadSituationPassengersVisible(false);
		SetCallMonitor(false);
	}

	private void Update()
	{
		if (duplicate) return;
		FindAndSubscribeToGameFlow();
		ResolveNodes();
		BindParentInteraction();
		BindActivityItemInteraction();
		BindSeatMonitorInteraction();
		UpdateSeatMonitorInteractionAvailability();
		UpdateSeatMonitorView();
		UpdateDialogueCameraView();
		UpdateActivityPickupAnimation();

		if (CurrentStep != ScenarioStep.FreeRoam
			&& CurrentStep != ScenarioStep.WaitingForPassenger
			&& CurrentStep != ScenarioStep.ParentChoice
			&& CurrentStep != ScenarioStep.ParentChoiceGrace
			&& CurrentStep != ScenarioStep.SeatDecisionChoice)
			return;

		RemainingSeconds = Math.Max(0.0f, RemainingSeconds - Game.IFps);
		int displayedSecond = (int)Math.Ceiling(RemainingSeconds);
		if (displayedSecond != lastDisplayedSecond)
		{
			lastDisplayedSecond = displayedSecond;
			TimerChanged?.Invoke(RemainingSeconds);
		}

		if (RemainingSeconds > 0.0f)
			return;

		if (CurrentStep == ScenarioStep.FreeRoam)
			BeginPassengerCall();
		else if (CurrentStep == ScenarioStep.WaitingForPassenger && !PassengerApproachTimedOut)
		{
			PassengerApproachTimedOut = true;
			ApplyOutcome(0, -2, -1, 0);
			Changed?.Invoke();
		}
		else if (CurrentStep == ScenarioStep.ParentChoice)
			HandleFirstParentAnswerTimeout();
		else if (CurrentStep == ScenarioStep.ParentChoiceGrace)
			HandleFinalParentAnswerTimeout();
		else if (CurrentStep == ScenarioStep.SeatDecisionChoice)
			HandleSeatDecisionTimeout();
	}

	/// <summary>The route briefing belongs to scenario 1; scenario 2 waits for GameFlow.</summary>
	public void NotifyBriefingClosed()
	{
		if (CurrentStep == ScenarioStep.WaitingForBriefing)
			SetStep(ScenarioStep.WaitingForScenarioTwo);
	}

	public void NotifyPassengerInteracted()
	{
		if (CurrentStep != ScenarioStep.WaitingForPassenger)
			return;

		RemainingSeconds = 0.0f;
		SetCallMonitor(false);
		StoryStateController state = ComponentSystem.FindComponentInWorld<StoryStateController>();
		state?.SetFlag(StoryStateController.StoryFlag.PassengerFound, true);
		ShowLine(ScenarioStep.Opening, "Проводник",
			"Добрый день. Вы вызывали проводника? Подскажите, пожалуйста, чем я могу помочь?");
	}

	public void NotifySeatDecisionPassengerInteracted()
	{
		if (CurrentStep != ScenarioStep.AwaitingSeatCheck
			&& CurrentStep != ScenarioStep.SeatCheckCompleted)
			return;

		DialogueActive = true;
		CurrentSpeaker = "Проводник";
		CurrentText = "";
		SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.TurnToPlayer);
		RemainingSeconds = Math.Max(0.0f, seatDecisionSeconds);
		lastDisplayedSecond = -1;
		SetStep(ScenarioStep.SeatDecisionChoice);
		Changed?.Invoke();
	}

	public void NotifyParentPassengerInteracted()
	{
		if (CurrentStep == ScenarioStep.AwaitingActivityHandoff)
		{
			if (parentInteractable != null)
				parentInteractable.IsAvailable = false;
			ShowLine(ScenarioStep.ComplainantBranchReaction, "Пассажир ябеда",
				"Спасибо. Теперь стало значительно спокойнее.");
			return;
		}

		if (CurrentStep != ScenarioStep.AwaitingParentInteraction)
			return;

		if (parentInteractable != null)
			parentInteractable.IsAvailable = false;
		ShowLine(ScenarioStep.ParentOpening, "Проводник",
			"Добрый день. Один из пассажиров сообщил, что ему мешает шум. Я хотел бы спокойно обсудить с Вами, как можно сделать обстановку комфортнее для всех.");
	}

	public string[] GetChoiceIds()
	{
		if (CurrentStep == ScenarioStep.ComplaintChoice)
			return new[] { "ID-01", "ID-02", "ID-03" };
		if (CurrentStep == ScenarioStep.ParentChoice || CurrentStep == ScenarioStep.ParentChoiceGrace)
			return OnboardActivitiesAvailable
				? new[] { "ID-04", "ID-05", "ID-06" }
				: new[] { "ID-07", "ID-08", "ID-09" };
		if (CurrentStep == ScenarioStep.SeatDecisionChoice)
		{
			if (!SeatAvailabilityFactChecked)
				return new[] { "ID-12" };
			return RelocationSeatAvailable
				? new[] { "ID-10", "ID-11" }
				: new[] { "ID-13", "ID-14", "ID-15" };
		}
		return Array.Empty<string>();
	}

	public string GetChoiceLabel(string choiceId)
	{
			switch (choiceId)
		{
			case "ID-01":
				return InitialMood >= 2
					? "Понимаю, что Вы хотите отдохнуть. Я проверю обстановку и постараюсь предложить решение, не создавая конфликта с другими пассажирами."
					: InitialMood <= -2
						? "Я вижу, что ситуация Вас сильно беспокоит. Давайте решим вопрос спокойно: я проверю обстановку и предложу возможные варианты."
						: "Понимаю, что шум доставляет Вам неудобство. Я уточню ситуацию и посмотрю, как можно сделать поездку комфортнее.";
			case "ID-02":
				return InitialMood >= 2
					? "Дети имеют право находиться в вагоне, но я проверю уровень шума и поговорю с сопровождающими их взрослыми."
					: InitialMood <= -2
						? "Ваше недовольство понятно, но требовать полной тишины в вагоне нельзя. Я проверю ситуацию и поговорю со взрослыми."
						: "В вагоне находятся и другие пассажиры. Тем не менее я проверю ситуацию и напомню взрослым о необходимости соблюдать порядок.";
			case "ID-03":
				return InitialMood >= 2
					? "Сейчас я попрошу родителей полностью прекратить шум. В вагоне комфорт-класса дети не должны мешать отдыху других пассажиров."
					: InitialMood <= -2
						? "Я сейчас же заставлю родителей успокоить детей. В этом вагоне они обязаны соблюдать полную тишину."
						: "Я потребую, чтобы родители обеспечили полную тишину. В комфорт-классе шуметь нельзя.";
			case "ID-04":
				return GetParentMoodText(
					"У нас есть подходящая детская литература и настольные игры. Если хотите, я помогу выбрать вариант с учётом возраста детей. Возможно, это позволит им спокойно провести время.",
					"Я не хочу ограничивать детей или обвинять Вас. На борту есть детская литература и настольные игры. Могу предложить подходящий вариант и попросить Вас по возможности обратить внимание на уровень шума.",
					"Я понимаю, что дети имеют право находиться в вагоне. Речь не идёт о полном запрете на шум. Я предлагаю спокойное решение: подобрать детскую литературу или настольную игру и постараться немного снизить громкость.");
			case "ID-05":
				return GetParentMoodText(
					"Возьмите для детей детскую литературу или настольную игру и проследите, чтобы они вели себя тише.",
					"Вы должны лучше контролировать детей. На борту есть детская литература и настольные игры — воспользуйтесь ими, чтобы прекратить шум.",
					"Шум создают Ваши дети, поэтому решить проблему должны Вы. Возьмите предложенные материалы и обеспечьте порядок.");
			case "ID-06":
				return GetParentMoodText(
					"Для пассажиров с детьми в комфорт-классе предусмотрены планшеты. Я принесу один, чтобы дети могли спокойно посмотреть развлекательные материалы.",
					"В комфорт-классе детям предоставляются бесплатные игровые наборы. Сейчас я принесу такой набор.",
					"Я выдам детям планшет или бесплатный набор. После этого они не должны больше мешать другим пассажирам.");
			case "ID-07":
				return GetParentMoodText(
					"К сожалению, подходящих детских материалов сейчас нет. Пожалуйста, по возможности помогите детям выбрать более спокойное занятие.",
					"Я не обвиняю Вас или детей. Подходящих материалов на борту сейчас нет, поэтому прошу Вас по возможности обратить внимание на уровень шума.",
					"Я понимаю, что дети не могут всё время находиться в полной тишине. Материалов сейчас нет, поэтому предлагаю спокойно договориться и постараться немного снизить громкость.");
			case "ID-08":
				return GetParentMoodText(
					"Материалов на борту нет, поэтому самостоятельно займите детей и проследите, чтобы они не мешали остальным пассажирам.",
					"Никаких игр сейчас нет. Вы отвечаете за поведение детей, поэтому обеспечьте порядок своими силами.",
					"Отсутствие материалов не освобождает Вас от ответственности. Успокойте детей и прекратите мешать другим пассажирам.");
			case "ID-09":
				return GetParentMoodText(
					"Ваши дети мешают пассажирам отдыхать. Если Вы не обеспечите тишину, мне придётся принимать дополнительные меры.",
					"На Ваших детей поступила жалоба. Немедленно прекратите шум и не мешайте остальным пассажирам.",
					"Из-за Ваших детей весь вагон не может отдохнуть. Если Вы не наведёте порядок, я приглашу начальника поезда.");
			case "ID-10":
				return GetPassengerOneMoodText(
					"Я проверил наличие свободных мест. Могу предложить Вам место в более спокойной части вагона того же класса обслуживания. Если Вам будет удобно, я помогу с пересадкой.",
					"В вагоне есть проверенное свободное место того же класса обслуживания. Там должно быть спокойнее. Если Вы согласны, я помогу Вам пересесть.",
					"Чтобы решить вопрос без дальнейшего конфликта, я проверил наличие мест. Вам доступно свободное место в более спокойной части вагона того же класса. Пересадка возможна только с Вашего согласия.");
			case "ID-11":
				return GetPassengerOneMoodText(
					"Если Вас по-прежнему не устраивает уровень шума, можете пересесть. Свободное место того же класса я уже проверил.",
					"Предыдущие меры Вас не устроили. Остаётся пересадка на проверенное место того же класса. Решайте, будете пересаживаться или нет.",
					"Раз ни одно из предложенных решений Вас не устраивает, пересядьте на свободное место того же класса. Других вариантов я Вам предложить не могу.");
			case "ID-12":
				return GetPassengerOneMoodText(
					"В более спокойной части вагона сейчас свободно одно место. Можете занять его, дополнительная проверка не требуется.",
					"Пересядьте на свободное место в другом купе. Если позже появится его пассажир, тогда решим вопрос.",
					"Сейчас в другой части вагона никто не сидит. Займите любое свободное место, а с оформлением разберёмся позднее.");
			case "ID-13":
				return GetPassengerOneMoodText(
					"К сожалению, свободных мест того же класса сейчас нет. Я продолжу контролировать ситуацию и повторно обращусь к родителям, если уровень шума не снизится. Если урегулировать вопрос не получится, я приглашу начальника поезда.",
					"Я проверил наличие мест, но свободных мест того же класса сейчас нет. Я не могу обещать Вам пересадку, однако продолжу контролировать ситуацию и при необходимости приглашу начальника поезда.",
					"Свободных мест того же класса сейчас нет, поэтому выполнить пересадку невозможно. Я понимаю Ваше недовольство. Я продолжу контролировать ситуацию, а если конфликт не удастся урегулировать, приглашу начальника поезда.");
			case "ID-14":
				return GetPassengerOneMoodText(
					"Свободных мест того же класса нет. Я уже проверил все доступные варианты. Оставайтесь на своём месте, а если шум продолжится, вопрос будет решать начальник поезда.",
					"Пересаживать Вас некуда. Свободных мест того же класса нет. Других решений с моей стороны не осталось, поэтому при продолжении жалобы я вызову начальника поезда.",
					"Свободных мест нет, поэтому требовать переселения бесполезно. Оставайтесь на своём месте. Если Вас это не устраивает, дальше разговаривать будете с начальником поезда.");
			case "ID-15":
				return GetPassengerOneMoodText(
					"Свободных мест Вашего класса нет, но в другом классе сейчас есть незанятое место. Можете временно перейти туда, дополнительное согласование не требуется.",
					"В этом классе мест нет. Перейдите на свободное место в другом классе, а вопрос с оформлением решим позднее.",
					"Здесь пересадить Вас некуда. Займите любое свободное место в другом классе, даже если оно не соответствует Вашему билету.");
			default:
				return "";
		}
	}

	public void Choose(string choiceId)
	{
		if (!HasChoices) return;
		if (CurrentStep == ScenarioStep.ParentChoice || CurrentStep == ScenarioStep.ParentChoiceGrace)
		{
			ChooseParentAnswer(choiceId);
			return;
		}
		if (CurrentStep == ScenarioStep.SeatDecisionChoice)
		{
			ChooseSeatDecision(choiceId);
			return;
		}

		int safety = 0;
		int loyalty = 0;
		int mood1 = 0;
		int mood2 = 0;
		switch (choiceId)
		{
			case "ID-01": safety = 2; loyalty = 2; mood1 = 1; break;
			case "ID-02": safety = 2; loyalty = -3; mood1 = -1; break;
			case "ID-03": safety = -4; loyalty = 2; mood1 = 1; mood2 = -2; break;
			default: return;
		}
		DecisionHistory.Add(choiceId);

		SafetyDelta += safety;
		LoyaltyDelta += loyalty;
		MoodPassenger1 = Math.Max(-3, Math.Min(3, MoodPassenger1 + mood1));
		MoodPassenger2 = Math.Max(-3, Math.Min(3, MoodPassenger2 + mood2));
		StoryStateController state = ComponentSystem.FindComponentInWorld<StoryStateController>();
		if (state != null)
		{
			state.ChangeMetric(StoryStateController.Metric.Safety, safety);
			state.ChangeMetric(StoryStateController.Metric.Loyalty, loyalty);
		}

		ShowLine(ScenarioStep.PassengerReaction, "Пассажир", GetPassengerReaction());
	}

	public void NotifyTextFinished()
	{
		switch (CurrentStep)
		{
			case ScenarioStep.Opening:
				ShowLine(ScenarioStep.Complaint, "Пассажир", GetPassengerComplaint());
				break;
			case ScenarioStep.Complaint:
				SetStep(ScenarioStep.ComplaintChoice);
				Changed?.Invoke();
				break;
			case ScenarioStep.PassengerReaction:
				ShowLine(ScenarioStep.InternalPrompt, "Проводник (мысленно)",
					"Перед тем как попытаться решить проблему, необходимо удостовериться в наличии детской литературы или настольных игр.");
				break;
			case ScenarioStep.InternalPrompt:
				DialogueActive = false;
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.ReturnToIdle);
				SetStep(ScenarioStep.AwaitingDisplayChecks);
				Changed?.Invoke();
				break;
			case ScenarioStep.ParentOpening:
				ShowLine(ScenarioStep.ParentResponse, "Пассажир (родитель)", GetParentOpeningResponse());
				break;
			case ScenarioStep.ParentResponse:
				BeginParentChoice(parentAnswerSeconds, ScenarioStep.ParentChoice);
				break;
			case ScenarioStep.ParentReminder:
				BeginParentChoice(parentAnswerGraceSeconds, ScenarioStep.ParentChoiceGrace);
				break;
			case ScenarioStep.ParentBranchReaction:
				ContinueParentBranchAfterParentReaction();
				break;
			case ScenarioStep.ComplainantBranchReaction:
				CompleteParentBranchAfterComplainantReaction();
				break;
			case ScenarioStep.Stage3ConductorAppeal:
				ShowLine(ScenarioStep.Stage3PassengerResponse, "Пассажир ябеда", GetStage3PassengerResponse());
				break;
			case ScenarioStep.Stage3PassengerResponse:
				ShowLine(ScenarioStep.Stage3SeatCheckPrompt, "Проводник пассажиру ябеде",
					"Могу проверить наличие свободных мест в другой части вагона, где Вы сможете отдохнуть.");
				break;
			case ScenarioStep.Stage3SeatCheckPrompt:
				DialogueActive = false;
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.ReturnToIdle);
				SetStep(ScenarioStep.AwaitingSeatCheck);
				Changed?.Invoke();
				break;
			case ScenarioStep.SeatDecisionReaction:
				CompleteSeatDecisionAfterReaction();
				break;
			case ScenarioStep.SeatInvalidDiscovery:
				ShowLine(ScenarioStep.SeatInvalidReaction, "Пассажир ябеда", GetInvalidSeatFinalReaction());
				break;
			case ScenarioStep.SeatInvalidReaction:
				CompleteInvalidSeatFinal();
				break;
			case ScenarioStep.SeatDecisionTimeoutComplainant:
				ShowLine(ScenarioStep.SeatDecisionTimeoutParent, "Пассажир (родитель)",
					"Не нужно разговаривать с нами таким тоном. Дети ничего плохого не делают.");
				break;
			case ScenarioStep.SeatDecisionTimeoutParent:
				FinishSecondScenario(
					"Негативный финал: открытый конфликт",
					"Вы не приняли решение после повторного возражения родителей. Пассажиры начали выяснять отношения напрямую, и ситуация переросла в открытый конфликт.");
				break;
			case ScenarioStep.ParentTimeoutReaction:
				FinishSecondScenario(
					"Негативный финал: ответ не дан вовремя",
					"Вы выслушали жалобу пассажира, но не выбрали способ урегулирования ситуации в установленное время. " +
					"В результате шум продолжился, родители не получили корректной просьбы, а пассажир потерял доверие к персоналу и мог самостоятельно вступить в конфликт с семьёй. " +
					"Следовало своевременно обратиться к сопровождающим, предложить доступную детскую литературу или настольную игру либо проверить возможность пересадки.");
				break;
		}
	}

	private void FindAndSubscribeToGameFlow()
	{
		if (gameFlow != null) return;
		gameFlow = ComponentSystem.FindComponentInWorld<GameFlowController>();
		if (gameFlow == null) return;
		gameFlow.ScenarioTwoStarted += StartScenarioTwo;
		subscribedToFlow = true;
		if (gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioTwo)
			StartScenarioTwo();
	}

	private void StartScenarioTwo()
	{
		if (IsStarted) return;
		IsStarted = true;
		int profile = random.Next(0, 3);
		InitialMood = profile == 0 ? 2 : profile == 1 ? 0 : -3;
		int parentProfile = random.Next(0, 3);
		InitialParentMood = parentProfile == 0 ? 2 : parentProfile == 1 ? 0 : -3;
		MoodPassenger1 = InitialMood;
		MoodPassenger2 = InitialParentMood;
		PassengerApproachTimedOut = false;
		DecisionHistory.Clear();
		DiplomatWithoutPropsEligible = false;
		OnboardActivitiesAvailable = random.Next(0, 2) == 0;
		ActivitiesFactChecked = false;
		RelocationSeatAvailable = random.Next(0, 2) == 0;
		SeatAvailabilityFactChecked = false;
		terminalOpen = false;
		SafetyDelta = LoyaltyDelta = 0;
		ResolveNodes();
		SelectRandomPassengerSeat();
		PreparePassengerAnimations();
		SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.TurnToPlayer, false);
		SetPassengerAnimation(parentPassengerNode, PassengerAnimationState.TurnToPlayer, false);
		SetPassengerVisible(true);
		SetParentPassengerVisible(true);
		SetActivityItemVisible(OnboardActivitiesAvailable);
		SetBadSituationPassengersVisible(false);
		SetCallMonitor(false);
		RemainingSeconds = Math.Max(0.0f, adaptationSeconds);
		lastDisplayedSecond = -1;
		SetStep(ScenarioStep.FreeRoam);
		Changed?.Invoke();
	}

	private void BeginPassengerCall()
	{
		ResolveNodes();
		SetPassengerVisible(true);
		SetCallMonitor(true);
		RemainingSeconds = Math.Max(0.0f, passengerSearchSeconds);
		lastDisplayedSecond = -1;
		SetStep(ScenarioStep.WaitingForPassenger);
		PassengerCallRequested?.Invoke();
		Changed?.Invoke();
	}

	private string GetPassengerComplaint()
	{
		if (InitialMood >= 2)
			return "Да, спасибо. Я хотел бы отдохнуть, но дети рядом довольно громко разговаривают. Можно что-нибудь сделать?";
		if (InitialMood <= -2)
			return "Здесь вообще кто-нибудь следит за порядком? Дети шумят на весь вагон, а родители никак не реагируют.";
		return "Я специально выбрал комфорт-класс, чтобы отдохнуть, но из-за шума рядом это невозможно. Помогите, пожалуйста.";
	}

	private string GetPassengerReaction()
	{
		if (MoodPassenger1 >= 2)
			return "Спасибо. Буду признателен, если получится решить вопрос спокойно.";
		if (MoodPassenger1 <= -2)
			return "Я уже достаточно ждал. Надеюсь, Вы действительно что-нибудь предпримете.";
		return "Хорошо, только, пожалуйста, не оставляйте ситуацию без внимания.";
	}

	private string GetParentOpeningResponse()
	{
		if (MoodPassenger2 >= 2)
			return "Да, конечно. Я Вас слушаю. Дети, наверное, немного увлеклись.";
		if (MoodPassenger2 <= -2)
			return "Почему Вы обращаетесь именно к нам? Мои дети никому не мешают.";
		return "А что именно случилось? Дети ведут себя как обычно.";
	}

	private string GetParentMoodText(string positive, string neutral, string negative)
	{
		if (MoodPassenger2 >= 2) return positive;
		if (MoodPassenger2 <= -2) return negative;
		return neutral;
	}

	private string GetPassengerOneMoodText(string positive, string neutral, string negative)
	{
		if (MoodPassenger1 >= 2) return positive;
		if (MoodPassenger1 <= -2) return negative;
		return neutral;
	}

	private void BeginParentChoice(float seconds, ScenarioStep step)
	{
		RemainingSeconds = Math.Max(0.0f, seconds);
		lastDisplayedSecond = -1;
		SetStep(step);
		Changed?.Invoke();
	}

	private void ChooseParentAnswer(string choiceId)
	{
		int safety;
		int loyalty;
		int mood1;
		int mood2;
		switch (choiceId)
		{
			case "ID-04":
			case "ID-07":
				safety = 5; loyalty = 3; mood1 = 1; mood2 = 1;
				break;
			case "ID-05":
			case "ID-08":
				safety = 5; loyalty = -3; mood1 = -1; mood2 = -2;
				break;
			case "ID-06":
				safety = -3; loyalty = 1; mood1 = 0; mood2 = 0;
				break;
			case "ID-09":
				safety = -5; loyalty = -2; mood1 = -1; mood2 = -2;
				break;
			default:
				return;
		}

		RemainingSeconds = 0.0f;
		selectedParentChoiceId = choiceId;
		DecisionHistory.Add(choiceId);
		ApplyOutcome(safety, loyalty, mood1, mood2);
		if (choiceId == "ID-06")
		{
			FinishSecondScenario(
				"Негативный финал: обещана недоступная услуга",
				"Вы пообещали бесплатную услугу, которая не гарантирована для комфорт-класса. " +
				"Перед предложением необходимо проверить наличие и условия предоставления детского набора.");
			return;
		}

		ShowLine(ScenarioStep.ParentBranchReaction, "Пассажир (родитель)", GetParentBranchReaction(choiceId));
	}

	private void HandleFirstParentAnswerTimeout()
	{
		ApplyOutcome(-2, -2, -1, -1);
		ShowLine(ScenarioStep.ParentReminder, "Пассажир (родитель)",
			"Подскажите, пожалуйста, в чем проблема?");
	}

	private void HandleFinalParentAnswerTimeout()
	{
		ApplyOutcome(-8, -2, -1, -1);
		ShowLine(ScenarioStep.ParentTimeoutReaction, "Пассажир 1",
			"Понятно. Значит, решать ситуацию никто не будет.");
	}

	private void ChooseSeatDecision(string choiceId)
	{
		int safety;
		int loyalty;
		int mood1;
		switch (choiceId)
		{
			case "ID-10":
			case "ID-13":
				safety = 5; loyalty = 2; mood1 = 1;
				break;
			case "ID-11":
			case "ID-14":
				safety = 5; loyalty = -3; mood1 = -1;
				break;
			case "ID-12":
			case "ID-15":
				safety = -5; loyalty = 2; mood1 = 1;
				break;
			default:
				return;
		}

		RemainingSeconds = 0.0f;
		selectedSeatChoiceId = choiceId;
		DecisionHistory.Add(choiceId);
		ApplyOutcome(safety, loyalty, mood1, 0);
		string reaction = choiceId == "ID-12"
			? "Хорошо, если место свободно, я пересяду."
			: choiceId == "ID-15"
				? "Хорошо. Если там свободно, я перейду."
				: GetSeatDecisionReaction(choiceId);
		ShowLine(ScenarioStep.SeatDecisionReaction, "Пассажир ябеда", reaction);
	}

	private string GetSeatDecisionReaction(string choiceId)
	{
		switch (choiceId)
		{
			case "ID-10":
				return GetPassengerOneMoodText(
					"Да, этот вариант мне подходит. Спасибо, что проверили место и сохранили мой класс обслуживания.",
					"Хорошо, я согласен пересесть. Надеюсь, там действительно будет спокойнее.",
					"Ладно, я пересяду. По крайней мере, это конкретное решение.");
			case "ID-11":
				return GetPassengerOneMoodText(
					"Я согласен пересесть, но предложить это можно было в более корректной форме.",
					"Хорошо, я пересяду. Но Ваш тон только усилил моё недовольство.",
					"Я пересяду только потому, что хочу отдохнуть. На Ваше обращение я ещё пожалуюсь.");
			case "ID-13":
				return GetPassengerOneMoodText(
					"Понятно. Спасибо, что проверили и честно сообщили результат. Пожалуйста, продолжайте следить за ситуацией.",
					"Жаль, что пересесть нельзя. Хорошо, тогда продолжайте контролировать уровень шума.",
					"Я недоволен, но понимаю, что свободных мест нет. Если ничего не изменится, приглашайте начальника поезда.");
			case "ID-14":
				return GetPassengerOneMoodText(
					"Я понял, что мест нет. Но сообщить об этом можно было спокойнее.",
					"Хорошо, зовите начальника поезда, если сами не можете нормально объяснить ситуацию.",
					"Немедленно приглашайте начальника поезда. На Ваш тон я тоже буду жаловаться.");
			default:
				return "";
		}
	}

	private void CompleteSeatDecisionAfterReaction()
	{
		switch (selectedSeatChoiceId)
		{
			case "ID-10":
				FinishSecondScenario(
					"Финал 3А",
					"Вы убедились, что свободное место действительно доступно, сохранили пассажиру тот же класс обслуживания и предложили пересадку как добровольное решение. Пассажир получил возможность отдохнуть, а семья с детьми не подвергалась обвинениям или необоснованному перемещению. Ситуация урегулирована корректно и без дополнительного конфликта.");
				break;
			case "ID-11":
				FinishSecondScenario(
					"Финал 3Б",
					"Вы предложили пассажиру проверенное свободное место того же класса и не стали обвинять или пересаживать семью с детьми. С точки зрения порядка действий решение правильное. Однако резкая формулировка усилила недовольство пассажира и снизила его лояльность. Даже при наличии подходящего решения важно предлагать его как помощь, а не как способ избавиться от жалобы.");
				break;
			case "ID-12":
				StartBadSituationScene();
				ShowLine(ScenarioStep.SeatInvalidDiscovery, "Ситуация",
					"После пересадки выясняется, что место закреплено за другим пассажиром.");
				break;
			case "ID-13":
				FinishSecondScenario(
					"Финал 4А",
					"Вы проверили наличие мест и честно сообщили пассажиру, что пересадка в пределах того же класса невозможна. Вы не стали обещать недоступное место, предложили продолжить контроль ситуации и предусмотрели обращение к начальнику поезда, если урегулировать конфликт не удастся. Пассажир не получил желаемую пересадку, но увидел, что его жалоба не была проигнорирована.");
				break;
			case "ID-14":
				FinishSecondScenario(
					"Финал 4Б",
					"Вы правильно проверили наличие мест, не стали обещать невозможную пересадку и предусмотрели вызов начальника поезда. Требования безопасности и порядок эскалации соблюдены. Однако резкая формулировка создала у пассажира ощущение, что его жалобу считают необоснованной, поэтому его лояльность снизилась.");
				break;
			case "ID-15":
				StartBadSituationScene();
				ShowLine(ScenarioStep.SeatInvalidDiscovery, "Ситуация",
					"После пересадки выясняется, что место закреплено за другим пассажиром.");
				break;
		}
	}

	private string GetInvalidSeatFinalReaction()
	{
		return selectedSeatChoiceId == "ID-12"
			? "Вы сказали, что место свободно. Теперь меня снова просят пересесть. Почему Вы не проверили это заранее?"
			: "Вы сами разрешили мне пересесть. Почему теперь я должен возвращаться обратно?";
	}

	private void CompleteInvalidSeatFinal()
	{
		if (selectedSeatChoiceId == "ID-12")
		{
			FinishSecondScenario(
				"Финал 3В",
				"Вы предложили пассажиру пересадку, не проверив статус места. Это временно повысило его лояльность, поскольку он ожидал быстрого решения, но затем создало новый конфликт с пассажиром, которому место было назначено. Перед предложением пересадки необходимо убедиться, что место свободно, относится к тому же классу обслуживания и может быть предоставлено этому пассажиру.");
			return;
		}

		FinishSecondScenario(
			"Финал 4В",
			"Вы попытались решить жалобу пересадкой и предложили пассажиру место другого класса без проверки и согласования. Это создало новую спорную ситуацию и не обеспечило устойчивого решения. Если мест того же класса нет, необходимо честно сообщить об этом, продолжить контроль уровня шума и при необходимости пригласить начальника поезда.");
	}

	private void HandleSeatDecisionTimeout()
	{
		ApplyOutcome(-10, -6, -2, -2);
		ShowLine(ScenarioStep.SeatDecisionTimeoutComplainant, "Пассажир ябеда",
			"Если персонал не собирается ничего делать, я сам попрошу Вас вести себя тише.");
	}

	private string GetParentBranchReaction(string choiceId)
	{
		switch (choiceId)
		{
			case "ID-04":
				return "Хорошо, давайте посмотрим. Думаю, детям будет интересно.";
			case "ID-05":
			case "ID-08":
				return GetParentMoodText(
					"Я постараюсь, но Ваш тон совершенно неуместен.",
					"Я понял просьбу. Но обвинять меня при этом не нужно.",
					"Мои дети ничего не нарушают. Зовите начальника поезда, если у Вас есть претензии.");
			case "ID-07":
				if (MoodPassenger2 >= 2 && MoodPassenger1 >= 2)
					return "Хорошо, я постараюсь занять детей и прослежу, чтобы они вели себя спокойнее.";
				if (MoodPassenger2 >= 2 && MoodPassenger1 <= 1)
					return "Хорошо, мы постараемся вести себя спокойнее.";
				return "Постараюсь, но дети не могут всё время сидеть совершенно тихо.";
			case "ID-09":
				return "Вы не имеете права обвинять моих детей перед всем вагоном. Приглашайте начальника поезда.";
			default:
				return "";
		}
	}

	private void ContinueParentBranchAfterParentReaction()
	{
		switch (selectedParentChoiceId)
		{
			case "ID-04":
				DialogueActive = false;
				SetPassengerAnimation(parentPassengerNode, PassengerAnimationState.ReturnToIdle);
				SetStep(ScenarioStep.AwaitingActivityPickup);
				if (activityItemInteractable != null)
					activityItemInteractable.IsAvailable = true;
				else
					Log.Warning("PassengerScenarioController: assign activityItemNode with EInteractable for the ID-04 pickup step.\n");
				Changed?.Invoke();
				break;
			case "ID-05":
			case "ID-08":
				ShowLine(ScenarioStep.ComplainantBranchReaction, "Пассажир ябеда",
					selectedParentChoiceId == "ID-05"
						? "Теперь Вы только усилили конфликт. Отдохнуть всё равно невозможно."
						: "Вы только спровоцировали спор. Шум никуда не исчез.");
				break;
			case "ID-07":
				ShowLine(ScenarioStep.ComplainantBranchReaction, "Пассажир ябеда", GetId07ComplainantReaction());
				break;
			case "ID-09":
				ShowLine(ScenarioStep.ComplainantBranchReaction, "Пассажир ябеда",
					"Теперь здесь ещё и конфликт. Спокойнее точно не стало.");
				break;
			default:
				CompleteParentBranchToStage3();
				break;
		}
	}

	private string GetId07ComplainantReaction()
	{
		if (MoodPassenger2 >= 2 && MoodPassenger1 >= 2)
			return "Теперь Вы только усилили конфликт. Отдохнуть всё равно невозможно.";
		if (MoodPassenger2 >= 2 && MoodPassenger1 <= 1)
			return "Они стали вести себя тише, но мне всё равно не удаётся отдохнуть.";
		return "Шум всё ещё мешает. Мне нужно другое решение.";
	}

	private void CompleteParentBranchAfterComplainantReaction()
	{
		switch (selectedParentChoiceId)
		{
			case "ID-04":
				ApplyOutcome(5, 2, 0, 0);
				FinishSecondScenario(
					"Финал 1А",
					"Вы корректно объяснили ситуацию и предложили реально доступное решение. " +
					"Родитель согласился принять детскую литературу или настольную игру, поэтому шум удалось снизить без вмешательства начальника поезда.\n\n" +
					"Начисляется бонус успешного разрешения: безопасность +5, лояльность +2.");
				break;
			case "ID-09":
				FinishSecondScenario(
					"Негативный финал: публичный конфликт",
					"Предложение пересадки допустимо только после проверки свободных мест. " +
					"Нельзя заранее обещать или требовать пересадку пассажира.");
				break;
			default:
				CompleteParentBranchToStage3();
				break;
		}
	}

	private void CompleteParentBranchToStage3()
	{
		if (DecisionHistory.Count >= 2
			&& DecisionHistory[0] == "ID-01"
			&& selectedParentChoiceId == "ID-07"
			&& MoodPassenger1 >= 2
			&& MoodPassenger2 >= 2)
			DiplomatWithoutPropsEligible = true;
		ShowLine(ScenarioStep.Stage3ConductorAppeal, "Проводник пассажиру ябеде", GetStage3ConductorAppeal());
	}

	private string GetStage3ConductorAppeal()
	{
		if (MoodPassenger1 >= 2)
			return "Спасибо, что подождали. Я предпринял доступные меры, но понимаю, что шум всё ещё может Вам мешать. Я хотел бы предложить дополнительный вариант решения.";
		if (MoodPassenger1 <= -2)
			return "Я понимаю, что ситуация по-прежнему вызывает у Вас недовольство. Я готов предложить ещё один способ решения, чтобы не продолжать конфликт.";
		return "Я вижу, что принятых мер оказалось недостаточно. Давайте рассмотрим ещё один вариант, который может помочь Вам отдохнуть.";
	}

	private string GetStage3PassengerResponse()
	{
		if (MoodPassenger1 >= 2)
			return "Хорошо, я Вас слушаю. Что Вы можете предложить?";
		if (MoodPassenger1 <= -2)
			return "Говорите. Потому что до сих пор вопрос так и не решён.";
		return "Хорошо. Надеюсь, на этот раз решение действительно поможет.";
	}

	private void UpdateDialogueCameraView()
	{
		if (badSituationActive || seatMonitorViewActive)
			return;

		Player desiredTarget = DialogueActive ? GetDialogueCameraForCurrentStep() : null;
		if (desiredTarget != null)
		{
			if (!dialogueCameraActive)
				BeginDialogueCameraSequence(desiredTarget);
			else if (desiredTarget != currentDialogueTarget && desiredTarget != pendingDialogueTarget)
				StartDialogueCameraTransition(Game.Player, desiredTarget, false);
		}
		else if (dialogueCameraActive && !dialogueCameraReturning)
		{
			if (playerBeforeDialogueCamera != null)
				StartDialogueCameraTransition(Game.Player, playerBeforeDialogueCamera, true);
			else
				RestoreDialogueCameraImmediately();
		}

		if (dialogueCameraActive)
		{
			Physics.Scale = 0.0f;
			if (dialogueCameraTransitioning)
				AdvanceDialogueCameraTransition();
		}
	}

	private Player GetDialogueCameraForCurrentStep()
	{
		switch (CurrentStep)
		{
			case ScenarioStep.ParentOpening:
			case ScenarioStep.ParentResponse:
			case ScenarioStep.ParentChoice:
			case ScenarioStep.ParentReminder:
			case ScenarioStep.ParentChoiceGrace:
			case ScenarioStep.ParentBranchReaction:
				return parentDialogueCamera;

			case ScenarioStep.ParentTimeoutReaction:
			case ScenarioStep.SeatDecisionTimeoutComplainant:
			case ScenarioStep.SeatDecisionTimeoutParent:
				return bothPassengersDialogueCamera ?? selectedComplainantDialogueCamera;

			case ScenarioStep.SeatInvalidDiscovery:
			case ScenarioStep.SeatInvalidReaction:
				return null;

			default:
				return selectedComplainantDialogueCamera;
		}
	}

	private void BeginDialogueCameraSequence(Player target)
	{
		if (target == null || Game.Player == null)
			return;

		dialogueCameraActive = true;
		playerBeforeDialogueCamera = Game.Player;
		savedDialoguePhysicsScale = Physics.Scale > 0.0f ? Physics.Scale : 1.0f;
		savedComplainantCameraControlled = selectedComplainantDialogueCamera != null && selectedComplainantDialogueCamera.Controlled;
		savedParentCameraControlled = parentDialogueCamera != null && parentDialogueCamera.Controlled;
		savedBothCameraControlled = bothPassengersDialogueCamera != null && bothPassengersDialogueCamera.Controlled;
		if (selectedComplainantDialogueCamera != null) selectedComplainantDialogueCamera.Controlled = false;
		if (parentDialogueCamera != null) parentDialogueCamera.Controlled = false;
		if (bothPassengersDialogueCamera != null) bothPassengersDialogueCamera.Controlled = false;
		Physics.Scale = 0.0f;
		StartDialogueCameraTransition(playerBeforeDialogueCamera, target, false);
	}

	private void StartDialogueCameraTransition(Player source, Player target, bool returning)
	{
		if (source == null || target == null)
			return;

		if (dialogueTransitionCamera == null)
		{
			dialogueTransitionCamera = new PlayerDummy();
			dialogueTransitionCamera.Controlled = false;
		}

		dialogueCameraTransitionStartPosition = source.WorldPosition;
		dialogueCameraTransitionStartDirection = source.GetWorldDirection(MathLib.AXIS.NZ);
		dialogueCameraTransitionStartUp = source.GetWorldDirection(MathLib.AXIS.Y);
		dialogueCameraTransitionStartFov = source.Fov;
		dialogueTransitionCamera.WorldPosition = dialogueCameraTransitionStartPosition;
		dialogueTransitionCamera.SetWorldDirection(dialogueCameraTransitionStartDirection, dialogueCameraTransitionStartUp);
		dialogueTransitionCamera.Fov = dialogueCameraTransitionStartFov;
		if (Game.Player != null) Game.Player.MainPlayer = false;
		dialogueTransitionCamera.MainPlayer = true;
		Game.Player = dialogueTransitionCamera;
		pendingDialogueTarget = target;
		dialogueCameraTransitionElapsed = 0.0f;
		dialogueCameraTransitioning = true;
		dialogueCameraReturning = returning;
	}

	private void AdvanceDialogueCameraTransition()
	{
		if (pendingDialogueTarget == null || dialogueTransitionCamera == null)
		{
			RestoreDialogueCameraImmediately();
			return;
		}

		dialogueCameraTransitionElapsed += Game.IFps;
		float duration = MathLib.Max(0.2f, dialogueCameraTransitionSeconds);
		float t = MathLib.Min(1.0f, dialogueCameraTransitionElapsed / duration);
		float eased = t * t * (3.0f - 2.0f * t);
		Vec3 targetPosition = pendingDialogueTarget.WorldPosition;
		vec3 targetDirection = pendingDialogueTarget.GetWorldDirection(MathLib.AXIS.NZ);
		vec3 targetUp = pendingDialogueTarget.GetWorldDirection(MathLib.AXIS.Y);
		dialogueTransitionCamera.WorldPosition = MathLib.Lerp(dialogueCameraTransitionStartPosition, targetPosition, eased);
		dialogueTransitionCamera.SetWorldDirection(
			MathLib.Lerp(dialogueCameraTransitionStartDirection, targetDirection, eased).Normalized,
			MathLib.Lerp(dialogueCameraTransitionStartUp, targetUp, eased).Normalized);
		dialogueTransitionCamera.Fov = MathLib.Lerp(dialogueCameraTransitionStartFov, pendingDialogueTarget.Fov, eased);
		if (t < 1.0f)
			return;

		dialogueTransitionCamera.MainPlayer = false;
		pendingDialogueTarget.MainPlayer = true;
		Game.Player = pendingDialogueTarget;
		dialogueCameraTransitioning = false;
		currentDialogueTarget = pendingDialogueTarget;
		pendingDialogueTarget = null;
		if (dialogueCameraReturning)
			RestoreDialogueCameraState();
	}

	private void RestoreDialogueCameraImmediately()
	{
		if (playerBeforeDialogueCamera != null)
		{
			if (Game.Player != null) Game.Player.MainPlayer = false;
			playerBeforeDialogueCamera.MainPlayer = true;
			Game.Player = playerBeforeDialogueCamera;
		}
		RestoreDialogueCameraState();
	}

	private void RestoreDialogueCameraState()
	{
		if (selectedComplainantDialogueCamera != null) selectedComplainantDialogueCamera.Controlled = savedComplainantCameraControlled;
		if (parentDialogueCamera != null) parentDialogueCamera.Controlled = savedParentCameraControlled;
		if (bothPassengersDialogueCamera != null) bothPassengersDialogueCamera.Controlled = savedBothCameraControlled;
		Physics.Scale = savedDialoguePhysicsScale > 0.0f ? savedDialoguePhysicsScale : 1.0f;
		dialogueCameraActive = false;
		dialogueCameraTransitioning = false;
		dialogueCameraReturning = false;
		currentDialogueTarget = null;
		pendingDialogueTarget = null;
		playerBeforeDialogueCamera = null;
	}

	private void StartBadSituationScene()
	{
		if (badSituationActive)
			return;

		ResolveNodes();
		Player returnPlayer = dialogueCameraActive && playerBeforeDialogueCamera != null
			? playerBeforeDialogueCamera
			: Game.Player;
		float returnPhysicsScale = dialogueCameraActive
			? savedDialoguePhysicsScale
			: (Physics.Scale > 0.0f ? Physics.Scale : 1.0f);
		if (dialogueCameraActive)
		{
			if (selectedComplainantDialogueCamera != null) selectedComplainantDialogueCamera.Controlled = savedComplainantCameraControlled;
			if (parentDialogueCamera != null) parentDialogueCamera.Controlled = savedParentCameraControlled;
			if (bothPassengersDialogueCamera != null) bothPassengersDialogueCamera.Controlled = savedBothCameraControlled;
			dialogueCameraActive = false;
			dialogueCameraTransitioning = false;
			dialogueCameraReturning = false;
			currentDialogueTarget = null;
			pendingDialogueTarget = null;
			playerBeforeDialogueCamera = null;
		}
		badSituationActive = true;
		playerBeforeBadSituation = returnPlayer;
		savedBadSituationPhysicsScale = returnPhysicsScale;
		SetBadSituationPassengersVisible(true);
		if (badSituationCamera == null || Game.Player == null)
		{
			Log.Warning("PassengerScenarioController: assign badSituationCamera or add a Player named 'bad_situation'.\n");
			return;
		}

		savedBadSituationCameraControlled = badSituationCamera.Controlled;
		Physics.Scale = 0.0f;
		badSituationCamera.Controlled = false;
		playerBeforeBadSituation.MainPlayer = false;
		badSituationCamera.MainPlayer = true;
		Game.Player = badSituationCamera;
	}

	public void NotifySecondScenarioResultsClosed()
	{
		RestoreBadSituationScene();
	}

	private void RestoreBadSituationScene()
	{
		if (!badSituationActive)
			return;

		if (playerBeforeBadSituation != null)
		{
			if (badSituationCamera != null)
				badSituationCamera.MainPlayer = false;
			playerBeforeBadSituation.MainPlayer = true;
			Game.Player = playerBeforeBadSituation;
		}
		if (badSituationCamera != null)
			badSituationCamera.Controlled = savedBadSituationCameraControlled;
		Physics.Scale = savedBadSituationPhysicsScale > 0.0f ? savedBadSituationPhysicsScale : 1.0f;
		badSituationActive = false;
		playerBeforeBadSituation = null;
		SetBadSituationPassengersVisible(false);
	}

	private void FinishSecondScenario(string finalTitle, string feedback)
	{
		DialogueActive = false;
		SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.ReturnToIdle);
		SetPassengerAnimation(parentPassengerNode, PassengerAnimationState.ReturnToIdle);
		SetStep(ScenarioStep.Finished);
		Changed?.Invoke();
		gameFlow?.NotifyScenarioTwoFinished();
		SecondScenarioResultRequested?.Invoke(finalTitle, feedback);
	}

	private void ApplyOutcome(int safety, int loyalty, int mood1, int mood2)
	{
		SafetyDelta += safety;
		LoyaltyDelta += loyalty;
		MoodPassenger1 = Math.Max(-3, Math.Min(3, MoodPassenger1 + mood1));
		MoodPassenger2 = Math.Max(-3, Math.Min(3, MoodPassenger2 + mood2));
		StoryStateController state = ComponentSystem.FindComponentInWorld<StoryStateController>();
		if (state == null) return;
		state.ChangeMetric(StoryStateController.Metric.Safety, safety);
		state.ChangeMetric(StoryStateController.Metric.Loyalty, loyalty);
	}

	private void ShowLine(ScenarioStep step, string speaker, string text)
	{
		DialogueActive = true;
		CurrentSpeaker = speaker;
		CurrentText = text;
		ApplyDialogueAnimation(step);
		SetStep(step);
		Changed?.Invoke();
	}

	private void ApplyDialogueAnimation(ScenarioStep step)
	{
		switch (step)
		{
			case ScenarioStep.Opening:
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.TurnToPlayer);
				break;
			case ScenarioStep.Complaint:
				SetPassengerAnimation(SelectedPassengerNode, GetComplaintAnimation());
				break;
			case ScenarioStep.PassengerReaction:
				SetPassengerAnimation(SelectedPassengerNode, GetComplainantReactionAnimation());
				break;
			case ScenarioStep.InternalPrompt:
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.ReturnToIdle);
				break;
			case ScenarioStep.ParentOpening:
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.ReturnToIdle);
				SetPassengerAnimation(parentPassengerNode, PassengerAnimationState.TurnToPlayer);
				break;
			case ScenarioStep.ParentResponse:
				SetPassengerAnimation(parentPassengerNode, GetMoodAnimation(MoodPassenger2));
				break;
			case ScenarioStep.ParentReminder:
				SetPassengerAnimation(parentPassengerNode, PassengerAnimationState.Displeased);
				break;
			case ScenarioStep.ParentBranchReaction:
				SetPassengerAnimation(parentPassengerNode, GetParentBranchAnimation());
				break;
			case ScenarioStep.ComplainantBranchReaction:
				SetPassengerAnimation(parentPassengerNode, PassengerAnimationState.ReturnToIdle);
				SetPassengerAnimation(SelectedPassengerNode, GetComplainantBranchAnimation());
				break;
			case ScenarioStep.ParentTimeoutReaction:
				SetPassengerAnimation(parentPassengerNode, PassengerAnimationState.Displeased);
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.Aggressive);
				break;
			case ScenarioStep.Stage3ConductorAppeal:
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.TurnToPlayer);
				break;
			case ScenarioStep.Stage3PassengerResponse:
				SetPassengerAnimation(SelectedPassengerNode, GetMoodAnimation(MoodPassenger1));
				break;
			case ScenarioStep.SeatDecisionReaction:
				SetPassengerAnimation(SelectedPassengerNode, GetSeatDecisionAnimation());
				break;
			case ScenarioStep.SeatInvalidDiscovery:
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.Confused);
				break;
			case ScenarioStep.SeatInvalidReaction:
			case ScenarioStep.SeatDecisionTimeoutComplainant:
				SetPassengerAnimation(SelectedPassengerNode, PassengerAnimationState.Aggressive);
				break;
			case ScenarioStep.SeatDecisionTimeoutParent:
				SetPassengerAnimation(parentPassengerNode, PassengerAnimationState.Aggressive);
				break;
		}
	}

	private PassengerAnimationState GetComplaintAnimation()
	{
		if (InitialMood >= 2) return PassengerAnimationState.Confused;
		if (InitialMood <= -2) return PassengerAnimationState.Aggressive;
		return PassengerAnimationState.Displeased;
	}

	private PassengerAnimationState GetComplainantReactionAnimation()
	{
		if (MoodPassenger1 >= 2) return PassengerAnimationState.PositiveNod;
		if (MoodPassenger1 <= -2) return PassengerAnimationState.Aggressive;
		if (DecisionHistory.Count > 0 && DecisionHistory[DecisionHistory.Count - 1] == "ID-02")
			return PassengerAnimationState.Displeased;
		return PassengerAnimationState.Confused;
	}

	private PassengerAnimationState GetParentBranchAnimation()
	{
		if (selectedParentChoiceId == "ID-04" || selectedParentChoiceId == "ID-07")
			return MoodPassenger2 >= 0 ? PassengerAnimationState.PositiveNod : PassengerAnimationState.Confused;
		if (selectedParentChoiceId == "ID-09" || MoodPassenger2 <= -2)
			return PassengerAnimationState.Aggressive;
		return PassengerAnimationState.Displeased;
	}

	private PassengerAnimationState GetComplainantBranchAnimation()
	{
		if (selectedParentChoiceId == "ID-04")
			return PassengerAnimationState.PositiveNod;
		if (selectedParentChoiceId == "ID-05" || selectedParentChoiceId == "ID-08" || selectedParentChoiceId == "ID-09")
			return MoodPassenger1 <= -2 ? PassengerAnimationState.Aggressive : PassengerAnimationState.Displeased;
		return GetMoodAnimation(MoodPassenger1);
	}

	private PassengerAnimationState GetSeatDecisionAnimation()
	{
		if (selectedSeatChoiceId == "ID-11" || selectedSeatChoiceId == "ID-14")
			return MoodPassenger1 <= -2 ? PassengerAnimationState.Aggressive : PassengerAnimationState.Displeased;
		return PassengerAnimationState.PositiveNod;
	}

	private static PassengerAnimationState GetMoodAnimation(int mood)
	{
		if (mood >= 2) return PassengerAnimationState.PositiveNod;
		if (mood <= -2) return PassengerAnimationState.Aggressive;
		return PassengerAnimationState.Confused;
	}

	private void PreparePassengerAnimations()
	{
		if (passengerSeatVariants != null)
		{
			for (int i = 0; i < passengerSeatVariants.Count; i++)
			{
				PassengerSeatVariant variant = passengerSeatVariants[i];
				if (variant != null)
					RegisterPassengerAnimationSet(variant.passengerNode);
			}
		}
		RegisterPassengerAnimationSet(parentPassengerNode);
	}

	private void RegisterPassengerAnimationSet(Node passengerRoot)
	{
		if (passengerRoot == null || passengerAnimationSets.ContainsKey(passengerRoot))
			return;

		PassengerAnimationSet set = new PassengerAnimationSet
		{
			TurnToPlayer = FindAnimationNode(passengerRoot, "TurnToPlayer_1", "TurnToPlayer"),
			Aggressive = passengerRoot.FindNode("Agressive_1", 1),
			Confused = passengerRoot.FindNode("Confused_1", 1),
			Displeased = passengerRoot.FindNode("Displeased_1", 1),
			PositiveNod = FindAnimationNode(passengerRoot, "PositiveNod_1", "PositiveNod"),
			ReturnToIdle = FindAnimationNode(passengerRoot, "ReturnToIdle_1", "ReturnToIdle"),
			Work = passengerRoot.FindNode("Work", 1)
		};
		passengerAnimationSets.Add(passengerRoot, set);

		bool hasStandardSet = set.TurnToPlayer != null && set.Aggressive != null && set.Confused != null
			&& set.Displeased != null && set.PositiveNod != null && set.ReturnToIdle != null;
		bool hasPassenger17Set = set.TurnToPlayer != null && set.PositiveNod != null
			&& set.ReturnToIdle != null && set.Work != null;
		if (!hasStandardSet && !hasPassenger17Set)
		{
			Log.Warning("PassengerScenarioController: unsupported passenger animation set. Expected the standard six models or TurnToPlayer, PositiveNod, ReturnToIdle and Work.\n");
		}

		SetPassengerAnimation(passengerRoot, PassengerAnimationState.TurnToPlayer, false);
	}

	private void SetPassengerAnimation(Node passengerRoot, PassengerAnimationState state, bool restart = true)
	{
		if (passengerRoot == null)
			return;
		if (!passengerAnimationSets.ContainsKey(passengerRoot))
			RegisterPassengerAnimationSet(passengerRoot);

		PassengerAnimationSet set;
		if (!passengerAnimationSets.TryGetValue(passengerRoot, out set))
			return;

		Node target = GetAnimationModel(set, state);
		if (target == null)
			return;

		Node[] models =
		{
			set.TurnToPlayer,
			set.Aggressive,
			set.Confused,
			set.Displeased,
			set.PositiveNod,
			set.ReturnToIdle,
			set.Work
		};
		for (int i = 0; i < models.Length; i++)
		{
			Node model = models[i];
			if (model == null) continue;
			StopAnimationHierarchy(model);
			model.Enabled = model == target;
		}

		if (restart)
			RestartAnimationHierarchy(target);
		else
			ResetAnimationHierarchy(target);
	}

	private static Node GetAnimationModel(PassengerAnimationSet set, PassengerAnimationState state)
	{
		switch (state)
		{
			case PassengerAnimationState.TurnToPlayer: return set.TurnToPlayer ?? set.Work;
			case PassengerAnimationState.Aggressive: return set.Aggressive ?? set.Work;
			case PassengerAnimationState.Confused: return set.Confused ?? set.Work;
			case PassengerAnimationState.Displeased: return set.Displeased ?? set.Work;
			case PassengerAnimationState.PositiveNod: return set.PositiveNod ?? set.Work;
			case PassengerAnimationState.ReturnToIdle: return set.ReturnToIdle ?? set.Work;
			default: return set.TurnToPlayer ?? set.Work;
		}
	}

	private static Node FindAnimationNode(Node passengerRoot, string standardName, string alternateName)
	{
		Node result = passengerRoot.FindNode(standardName, 1);
		return result ?? passengerRoot.FindNode(alternateName, 1);
	}

	private static void RestartAnimationHierarchy(Node current)
	{
		if (current == null) return;
		ObjectMeshSkinnedLegacy skinned = current as ObjectMeshSkinnedLegacy;
		if (skinned != null)
		{
			skinned.Loop = false;
			skinned.Time = 0.0f;
			skinned.Play();
		}
		for (int i = 0; i < current.NumChildren; i++)
			RestartAnimationHierarchy(current.GetChild(i));
	}

	private static void ResetAnimationHierarchy(Node current)
	{
		if (current == null) return;
		ObjectMeshSkinnedLegacy skinned = current as ObjectMeshSkinnedLegacy;
		if (skinned != null)
		{
			skinned.Stop();
			skinned.Loop = false;
			skinned.Time = 0.0f;
		}
		for (int i = 0; i < current.NumChildren; i++)
			ResetAnimationHierarchy(current.GetChild(i));
	}

	private static void StopAnimationHierarchy(Node current)
	{
		if (current == null) return;
		ObjectMeshSkinnedLegacy skinned = current as ObjectMeshSkinnedLegacy;
		if (skinned != null)
			skinned.Stop();
		for (int i = 0; i < current.NumChildren; i++)
			StopAnimationHierarchy(current.GetChild(i));
	}

	private void ResolveNodes()
	{
		if (normalMonitorSpriteNode == null)
			normalMonitorSpriteNode = World.GetNodeByName("Монитор табло1.001");
		if (parentPassengerNode == null)
			parentPassengerNode = World.GetNodeByName("Пассажир (родитель)");
		if (activityItemNode == null && !string.IsNullOrWhiteSpace(activityItemNodeName))
			activityItemNode = World.GetNodeByName(activityItemNodeName);
		if (callMonitorInteractionNode == null && !string.IsNullOrWhiteSpace(callMonitorInteractionNodeName))
			callMonitorInteractionNode = World.GetNodeByName(callMonitorInteractionNodeName);
		if (callMonitorCamera == null && !string.IsNullOrWhiteSpace(callMonitorCameraName))
			callMonitorCamera = World.GetNodeByName(callMonitorCameraName) as Player;
		if (badSituationCamera == null && !string.IsNullOrWhiteSpace(badSituationCameraName))
			badSituationCamera = World.GetNodeByName(badSituationCameraName) as Player;
		if (parentInteractable == null && parentPassengerNode != null)
			parentInteractable = ComponentSystem.GetComponentInChildren<EInteractable>(parentPassengerNode);
		if (activityItemInteractable == null && activityItemNode != null)
			activityItemInteractable = ComponentSystem.GetComponentInChildren<EInteractable>(activityItemNode);
		if (seatMonitorInteractable == null && callMonitorInteractionNode != null)
		{
			seatMonitorInteractable = ComponentSystem.GetComponentInChildren<EInteractable>(callMonitorInteractionNode);
			if (seatMonitorInteractable == null)
				seatMonitorInteractable = ComponentSystem.GetComponentInParent<EInteractable>(callMonitorInteractionNode);
		}
	}

	private void SelectRandomPassengerSeat()
	{
		SelectedPassengerSeatIndex = -1;
		SelectedPassengerNode = null;
		selectedComplainantDialogueCamera = null;
		selectedPassengerSeatSpritePath = "";
		List<int> configuredVariants = new List<int>();
		if (passengerSeatVariants != null)
		{
			for (int i = 0; i < passengerSeatVariants.Count; i++)
			{
				PassengerSeatVariant variant = passengerSeatVariants[i];
				if (variant != null && variant.passengerNode != null)
					configuredVariants.Add(i);
			}
		}

		if (configuredVariants.Count == 0)
		{
			Log.Warning("PassengerScenarioController: configure five Passenger Seat Variants in the editor.\n");
			SetPassengerVisible(false);
			SetCallMonitor(false);
			return;
		}
		if (configuredVariants.Count != 5)
			Log.Warning("PassengerScenarioController: expected 5 configured Passenger Seat Variants, found {0}.\n", configuredVariants.Count);

		SelectedPassengerSeatIndex = configuredVariants[random.Next(0, configuredVariants.Count)];
		PassengerSeatVariant selected = passengerSeatVariants[SelectedPassengerSeatIndex];
		SelectedPassengerNode = selected.passengerNode;
		selectedComplainantDialogueCamera = selected.dialogueCamera;
		selectedPassengerSeatSpritePath = selected.monitorSeatSpritePath ?? "";
		SetPassengerVisible(false);
		SetCallMonitor(false);
	}

	public bool IsSelectedPassengerNode(Node passengerNode)
	{
		return passengerNode != null && passengerNode == SelectedPassengerNode;
	}

	private void SetPassengerVisible(bool visible)
	{
		// Seat randomization controls interaction, monitor sprite and dialogue camera only.
		// Other passenger mannequins must retain the visibility configured in the scene.
		if (visible && SelectedPassengerNode != null)
			SelectedPassengerNode.Enabled = true;
	}

	private void SetParentPassengerVisible(bool visible)
	{
		if (parentPassengerNode != null)
			parentPassengerNode.Enabled = visible;
	}

	private void SetActivityItemVisible(bool visible)
	{
		if (activityItemNode != null)
			activityItemNode.Enabled = visible;
	}

	private void SetBadSituationPassengersVisible(bool visible)
	{
		if (badSituationPassengerNodeOne != null)
			badSituationPassengerNodeOne.Enabled = visible;
		if (badSituationPassengerNodeTwo != null)
			badSituationPassengerNodeTwo.Enabled = visible;
	}

	public bool RevealOnboardActivitiesAvailability()
	{
		ActivitiesFactChecked = true;
		return OnboardActivitiesAvailable;
	}

	public bool RevealRelocationSeatAvailability()
	{
		SeatAvailabilityFactChecked = true;
		return RelocationSeatAvailable;
	}

	public void SetTerminalOpen(bool value)
	{
		terminalOpen = value;
	}

	public void ShowTerminalClosed()
	{
		terminalOpen = false;
		if (CurrentStep == ScenarioStep.AwaitingDisplayChecks && ActivitiesFactChecked)
		{
			SetStep(ScenarioStep.AwaitingParentInteraction);
			if (parentInteractable != null)
				parentInteractable.IsAvailable = true;
			Changed?.Invoke();
			return;
		}

		if (CurrentStep == ScenarioStep.AwaitingSeatCheck && SeatAvailabilityFactChecked)
		{
			SetStep(ScenarioStep.SeatCheckCompleted);
			Changed?.Invoke();
		}
	}

	private void BindParentInteraction()
	{
		if (subscribedParentInteractable == parentInteractable)
			return;
		if (subscribedParentInteractable != null)
			subscribedParentInteractable.Interacted -= NotifyParentPassengerInteracted;
		subscribedParentInteractable = parentInteractable;
		if (subscribedParentInteractable != null)
		{
			subscribedParentInteractable.Interacted += NotifyParentPassengerInteracted;
			subscribedParentInteractable.IsAvailable = CurrentStep == ScenarioStep.AwaitingParentInteraction
				|| CurrentStep == ScenarioStep.AwaitingActivityHandoff;
		}
	}

	private void BindActivityItemInteraction()
	{
		if (subscribedActivityItemInteractable == activityItemInteractable)
			return;
		if (subscribedActivityItemInteractable != null)
			subscribedActivityItemInteractable.Interacted -= NotifyActivityItemInteracted;
		subscribedActivityItemInteractable = activityItemInteractable;
		if (subscribedActivityItemInteractable != null)
		{
			subscribedActivityItemInteractable.Interacted += NotifyActivityItemInteracted;
			subscribedActivityItemInteractable.IsAvailable = CurrentStep == ScenarioStep.AwaitingActivityPickup;
		}
	}

	private void BindSeatMonitorInteraction()
	{
		if (subscribedSeatMonitorInteractable == seatMonitorInteractable)
			return;
		if (subscribedSeatMonitorInteractable != null)
			subscribedSeatMonitorInteractable.Interacted -= OpenSeatMonitorView;
		subscribedSeatMonitorInteractable = seatMonitorInteractable;
		if (subscribedSeatMonitorInteractable != null)
			subscribedSeatMonitorInteractable.Interacted += OpenSeatMonitorView;
	}

	private bool CanInspectSeatMonitor()
	{
		return CurrentStep == ScenarioStep.WaitingForPassenger;
	}

	private void UpdateSeatMonitorInteractionAvailability()
	{
		bool canInspect = CanInspectSeatMonitor();
		if (callMonitorInteractionNode != null)
			callMonitorInteractionNode.Enabled = canInspect;
		if (seatMonitorInteractable != null)
			seatMonitorInteractable.IsAvailable = canInspect && !seatMonitorViewActive;
	}

	private void CreateSeatMonitorView()
	{
		gui = Gui.GetCurrent();
		callMonitorSeatSprite = new WidgetSprite(gui)
		{
			Color = vec4.WHITE,
			Hidden = true,
			IntersectionEnabled = false
		};
		seatMonitorExitHint = new WidgetLabel(gui, "Чтобы выйти из режима просмотра нажмите Esc")
		{
			FontSize = 18,
			FontColor = vec4.WHITE,
			FontOutline = 1,
			Hidden = true,
			IntersectionEnabled = false
		};
		gui.AddChild(callMonitorSeatSprite, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		gui.AddChild(seatMonitorExitHint, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
	}

	private void OpenSeatMonitorView()
	{
		if (!CanInspectSeatMonitor() || seatMonitorViewActive || callMonitorCamera == null || Game.Player == null)
			return;

		seatMonitorViewActive = true;
		IsSeatMonitorViewVisible = true;
		playerBeforeSeatMonitor = Game.Player;
		savedSeatMonitorCameraControlled = callMonitorCamera.Controlled;
		callMonitorCamera.Controlled = false;
		savedPhysicsScale = Physics.Scale > 0.0f ? Physics.Scale : 1.0f;
		savedMonitorMouseHandle = Input.MouseHandle;
		savedMonitorMouseGrab = Input.MouseGrab;
		savedMonitorCursorHide = Input.MouseCursorHide;
		Physics.Scale = 0.0f;
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;
		Input.MouseGrab = false;
		Input.MouseCursorHide = true;
		Input.MouseCursorNeedUpdate = true;
		if (seatMonitorInteractable != null)
			seatMonitorInteractable.IsAvailable = false;

		StartSeatMonitorTransition(playerBeforeSeatMonitor, true);
	}

	private void StartSeatMonitorTransition(Player source, bool entering)
	{
		if (seatMonitorTransitionCamera == null)
		{
			seatMonitorTransitionCamera = new PlayerDummy();
			seatMonitorTransitionCamera.Controlled = false;
		}
		seatMonitorTransitionStartPosition = source.WorldPosition;
		seatMonitorTransitionStartDirection = source.GetWorldDirection(MathLib.AXIS.NZ);
		seatMonitorTransitionStartUp = source.GetWorldDirection(MathLib.AXIS.Y);
		seatMonitorTransitionStartFov = source.Fov;
		seatMonitorTransitionCamera.WorldPosition = seatMonitorTransitionStartPosition;
		seatMonitorTransitionCamera.SetWorldDirection(seatMonitorTransitionStartDirection, seatMonitorTransitionStartUp);
		seatMonitorTransitionCamera.Fov = seatMonitorTransitionStartFov;
		if (Game.Player != null)
			Game.Player.MainPlayer = false;
		seatMonitorTransitionCamera.MainPlayer = true;
		Game.Player = seatMonitorTransitionCamera;
		seatMonitorTransitionElapsed = 0.0f;
		seatMonitorTransitionEntering = entering;
		seatMonitorTransitionLeaving = !entering;
	}

	private void UpdateSeatMonitorView()
	{
		if (!seatMonitorViewActive)
			return;

		LayoutSeatMonitorView();
		if (seatMonitorTransitionEntering || seatMonitorTransitionLeaving)
		{
			UpdateSeatMonitorTransition();
			return;
		}

		Physics.Scale = 0.0f;
		Input.MouseGrab = false;
		Input.MouseCursorHide = true;
		if (Input.IsKeyDown(Input.KEY.ESC))
			BeginLeavingSeatMonitorView();
	}

	private void UpdateSeatMonitorTransition()
	{
		Player target = seatMonitorTransitionEntering ? callMonitorCamera : playerBeforeSeatMonitor;
		if (target == null)
		{
			CloseSeatMonitorViewImmediately();
			return;
		}

		seatMonitorTransitionElapsed += Game.IFps;
		float duration = MathLib.Max(0.2f, callMonitorTransitionSeconds);
		float t = MathLib.Min(1.0f, seatMonitorTransitionElapsed / duration);
		float eased = t * t * (3.0f - 2.0f * t);
		Vec3 targetPosition = target.WorldPosition;
		vec3 targetDirection = target.GetWorldDirection(MathLib.AXIS.NZ);
		vec3 targetUp = target.GetWorldDirection(MathLib.AXIS.Y);
		vec3 direction = MathLib.Lerp(seatMonitorTransitionStartDirection, targetDirection, eased).Normalized;
		vec3 up = MathLib.Lerp(seatMonitorTransitionStartUp, targetUp, eased).Normalized;
		seatMonitorTransitionCamera.WorldPosition = MathLib.Lerp(seatMonitorTransitionStartPosition, targetPosition, eased);
		seatMonitorTransitionCamera.SetWorldDirection(direction, up);
		seatMonitorTransitionCamera.Fov = MathLib.Lerp(seatMonitorTransitionStartFov, target.Fov, eased);

		if (t < 1.0f)
			return;

		seatMonitorTransitionCamera.MainPlayer = false;
		target.MainPlayer = true;
		Game.Player = target;
		if (seatMonitorTransitionEntering)
		{
			seatMonitorTransitionEntering = false;
			callMonitorSeatSprite.Texture = selectedPassengerSeatSpritePath;
			callMonitorSeatSprite.Hidden = string.IsNullOrWhiteSpace(selectedPassengerSeatSpritePath);
			seatMonitorExitHint.Hidden = false;
		}
		else
		{
			seatMonitorTransitionLeaving = false;
			FinishLeavingSeatMonitorView();
		}
	}

	private void BeginLeavingSeatMonitorView()
	{
		callMonitorSeatSprite.Hidden = true;
		seatMonitorExitHint.Hidden = true;
		StartSeatMonitorTransition(callMonitorCamera, false);
	}

	private void FinishLeavingSeatMonitorView()
	{
		seatMonitorViewActive = false;
		IsSeatMonitorViewVisible = false;
		Physics.Scale = savedPhysicsScale > 0.0f ? savedPhysicsScale : 1.0f;
		Input.MouseHandle = savedMonitorMouseHandle;
		Input.MouseGrab = savedMonitorMouseGrab;
		Input.MouseCursorHide = savedMonitorCursorHide;
		Input.MouseCursorNeedUpdate = true;
		if (callMonitorCamera != null)
			callMonitorCamera.Controlled = savedSeatMonitorCameraControlled;
		UpdateSeatMonitorInteractionAvailability();
	}

	private void CloseSeatMonitorViewImmediately()
	{
		seatMonitorTransitionEntering = false;
		seatMonitorTransitionLeaving = false;
		if (playerBeforeSeatMonitor != null)
		{
			playerBeforeSeatMonitor.MainPlayer = true;
			Game.Player = playerBeforeSeatMonitor;
		}
		callMonitorSeatSprite.Hidden = true;
		seatMonitorExitHint.Hidden = true;
		FinishLeavingSeatMonitorView();
	}

	private void LayoutSeatMonitorView()
	{
		if (gui == null) return;
		const int screenMargin = 48;
		int availableWidth = Math.Max(1, gui.Width - screenMargin * 2);
		int availableHeight = Math.Max(1, gui.Height - screenMargin * 2);
		int textureWidth = callMonitorSeatSprite.GetLayerWidth(0);
		int textureHeight = callMonitorSeatSprite.GetLayerHeight(0);
		if (textureWidth > 0 && textureHeight > 0)
		{
			float scale = Math.Min(
				(float)availableWidth / textureWidth,
				(float)availableHeight / textureHeight);
			int imageWidth = Math.Max(1, (int)Math.Round(textureWidth * scale));
			int imageHeight = Math.Max(1, (int)Math.Round(textureHeight * scale));
			callMonitorSeatSprite.Width = imageWidth;
			callMonitorSeatSprite.Height = imageHeight;
			callMonitorSeatSprite.SetPosition(
				(gui.Width - imageWidth) / 2,
				(gui.Height - imageHeight) / 2);
		}
		seatMonitorExitHint.Width = MathLib.Min(470, gui.Width - 40);
		seatMonitorExitHint.SetPosition(gui.Width - seatMonitorExitHint.Width - 24, 22);
	}

	private void NotifyActivityItemInteracted()
	{
		if (CurrentStep != ScenarioStep.AwaitingActivityPickup)
			return;
		if (activityPickupAnimating || activityItemNode == null)
			return;

		if (activityItemInteractable != null)
			activityItemInteractable.IsAvailable = false;
		activityPickupStartPosition = activityItemNode.WorldPosition;
		activityPickupElapsed = 0.0f;
		activityPickupAnimating = true;
	}

	private void UpdateActivityPickupAnimation()
	{
		if (!activityPickupAnimating || activityItemNode == null || Game.Player == null)
			return;

		activityPickupElapsed += Game.IFps;
		float duration = MathLib.Max(0.2f, activityPickupAnimationSeconds);
		float t = MathLib.Min(1.0f, activityPickupElapsed / duration);
		float eased = t * t * (3.0f - 2.0f * t);
		Node player = Game.Player;
		Vec3 forward = player.GetWorldDirection(MathLib.AXIS.NZ);
		Vec3 right = player.GetWorldDirection(MathLib.AXIS.X);
		Vec3 down = player.GetWorldDirection(MathLib.AXIS.NY);
		Vec3 target = player.WorldPosition + forward * 0.55f + right * 0.35f + down * 0.45f;
		activityItemNode.WorldPosition = activityPickupStartPosition
			+ (target - activityPickupStartPosition) * eased;

		if (t < 1.0f)
			return;

		activityPickupAnimating = false;
		SetActivityItemVisible(false);
		SetStep(ScenarioStep.AwaitingActivityHandoff);
		if (parentInteractable != null)
			parentInteractable.IsAvailable = true;
		Changed?.Invoke();
	}

	private void SetCallMonitor(bool callVisible)
	{
		if (callVisible && string.IsNullOrWhiteSpace(selectedPassengerSeatSpritePath) && !monitorWarningShown)
		{
			monitorWarningShown = true;
			Log.Warning("PassengerScenarioController: assign a .png monitor seat sprite to every Passenger Seat Variant.\n");
		}
		if (callVisible && callMonitorInteractionNode != null)
			callMonitorInteractionNode.Enabled = true;
		if (callVisible && callMonitorSurfaceGui == null)
			CreateCallMonitorSurface();
		if (callVisible && !string.IsNullOrWhiteSpace(selectedPassengerSeatSpritePath))
			UpdateCallMonitorSurfaceTexture();
		if (callMonitorSurfaceGui != null)
			callMonitorSurfaceGui.Enabled = callVisible;
		if (!callVisible && callMonitorSeatSprite != null)
			callMonitorSeatSprite.Hidden = true;
		if (normalMonitorSpriteNode != null)
			normalMonitorSpriteNode.Enabled = !callVisible;
	}

	private void CreateCallMonitorSurface()
	{
		if (callMonitorSurfaceGui != null || callMonitorInteractionNode == null)
			return;

		Unigine.Object monitorObject = callMonitorInteractionNode as Unigine.Object;
		if (monitorObject == null)
			return;

		BoundBox bounds = monitorObject.BoundBox;
		vec3 boundsSize = bounds.maximum - bounds.minimum;
		vec3 boundsCenter = (bounds.minimum + bounds.maximum) * 0.5f;
		float physicalWidth = Math.Max(0.1f, boundsSize.z * 0.92f);
		float physicalHeight = Math.Max(0.1f, boundsSize.y * 0.92f);

		callMonitorSurfaceGui = new ObjectGui(physicalWidth, physicalHeight)
		{
			Name = "Passenger Call Monitor Image",
			Parent = callMonitorInteractionNode,
			Background = true,
			DepthTest = true,
			PolygonOffset = 4.0f,
			Enabled = false
		};
		callMonitorSurfaceGui.Transform = MathLib.Translate(new vec3(
			bounds.maximum.x + 0.01f,
			boundsCenter.y,
			boundsCenter.z)) * MathLib.RotateY(90.0f);

		const int guiWidth = 1920;
		int guiHeight = Math.Max(1, (int)Math.Round(guiWidth * physicalHeight / physicalWidth));
		callMonitorSurfaceGui.SetScreenSize(guiWidth, guiHeight);
		Gui monitorGui = callMonitorSurfaceGui.GetGui();
		callMonitorSurfaceSprite = new WidgetSprite(monitorGui)
		{
			Color = vec4.WHITE,
			WrapRepeat = 0,
			IntersectionEnabled = false
		};
		monitorGui.AddChild(callMonitorSurfaceSprite, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
	}

	private void UpdateCallMonitorSurfaceTexture()
	{
		if (callMonitorSurfaceGui == null || callMonitorSurfaceSprite == null)
			return;
		if (appliedCallMonitorTexturePath == selectedPassengerSeatSpritePath)
			return;

		callMonitorSurfaceSprite.Texture = selectedPassengerSeatSpritePath;
		callMonitorSurfaceSprite.WrapRepeat = 0;
		Gui monitorGui = callMonitorSurfaceGui.GetGui();
		const int imageMargin = 48;
		int availableWidth = Math.Max(1, monitorGui.Width - imageMargin * 2);
		int availableHeight = Math.Max(1, monitorGui.Height - imageMargin * 2);
		int textureWidth = callMonitorSurfaceSprite.GetLayerWidth(0);
		int textureHeight = callMonitorSurfaceSprite.GetLayerHeight(0);
		if (textureWidth > 0 && textureHeight > 0)
		{
			float scale = Math.Min(
				(float)availableWidth / textureWidth,
				(float)availableHeight / textureHeight);
			int imageWidth = Math.Max(1, (int)Math.Round(textureWidth * scale));
			int imageHeight = Math.Max(1, (int)Math.Round(textureHeight * scale));
			callMonitorSurfaceSprite.Width = imageWidth;
			callMonitorSurfaceSprite.Height = imageHeight;
			callMonitorSurfaceSprite.SetPosition(
				(monitorGui.Width - imageWidth) / 2,
				(monitorGui.Height - imageHeight) / 2);
		}
		appliedCallMonitorTexturePath = selectedPassengerSeatSpritePath;
	}

	private void SetStep(ScenarioStep step)
	{
		CurrentStep = step;
		StepChanged?.Invoke(step);
	}

	private void Shutdown()
	{
		if (duplicate) return;
		if (dialogueCameraActive)
			RestoreDialogueCameraImmediately();
		RestoreBadSituationScene();
		if (seatMonitorViewActive)
			CloseSeatMonitorViewImmediately();
		if (gameFlow != null && subscribedToFlow)
			gameFlow.ScenarioTwoStarted -= StartScenarioTwo;
		if (subscribedParentInteractable != null)
		{
			subscribedParentInteractable.Interacted -= NotifyParentPassengerInteracted;
			subscribedParentInteractable.IsAvailable = false;
		}
		if (subscribedActivityItemInteractable != null)
		{
			subscribedActivityItemInteractable.Interacted -= NotifyActivityItemInteracted;
			subscribedActivityItemInteractable.IsAvailable = false;
		}
		if (subscribedSeatMonitorInteractable != null)
		{
			subscribedSeatMonitorInteractable.Interacted -= OpenSeatMonitorView;
			subscribedSeatMonitorInteractable.IsAvailable = false;
		}
		SetCallMonitor(false);
		SetPassengerVisible(false);
		SetParentPassengerVisible(false);
		SetActivityItemVisible(false);
		seatMonitorTransitionCamera?.DeleteLater();
		dialogueTransitionCamera?.DeleteLater();
		callMonitorSeatSprite?.DeleteLater();
		seatMonitorExitHint?.DeleteLater();
		callMonitorSurfaceSprite?.DeleteLater();
		callMonitorSurfaceGui?.DeleteLater();
		if (Active == this) Active = null;
	}
}
