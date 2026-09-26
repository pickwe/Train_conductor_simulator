using System;
using System.Collections.Generic;
using Unigine;

/// <summary>Owns the randomized passenger and carrier facts and the first scenario's score.</summary>
public partial class PetCarrierScenarioController : Component
{
	public enum PassengerProfile { Calm, Anxious, Confrontational }
	public enum PassengerEmotion { None, Confused, Aggressive, PositiveNod, CarrierNod }
	public enum Stage { Opening, Explanation, MoodReaction, TerminalPrompt, WaitingForTerminal, AwaitingPassengerInteraction, Solution, SolutionTimeoutPrompt, AlternativeSolutionReaction, RefusalSolutionReaction, CarrierPurchaseReaction, AwaitingCarrierPickup, AwaitingPassengerHandoff, Refusal, RefusalReaction, Finished }

	public float solutionSeconds = 30.0f;
	public float passengerReplySeconds = 15.0f;
	[Parameter(Group = "Scenario")]
	public bool timersEnabled = true;
	public PassengerProfile Profile { get; private set; }
	public bool CarrierAvailable { get; private set; }
	public bool CarrierFactChecked { get; private set; }
	public bool ID02Selected { get; private set; }
	public bool IsStarted => started;
	public bool CanOpenTerminal => started && (CurrentStage == Stage.WaitingForTerminal || CurrentStage == Stage.Solution || CurrentStage == Stage.Refusal);
	public Stage CurrentStage { get; private set; }
	public int Mood { get; private set; }
	public int InitialMood { get; private set; }
	public int Safety { get; private set; }
	public int Loyalty { get; private set; }
	public List<string> DecisionHistory { get; } = new List<string>();
	public bool EndingBonusGranted { get; private set; }
	public float RemainingSeconds { get; private set; }
	public string CurrentText { get; private set; } = "";
	public string CurrentSpeaker { get; private set; } = "Проводник";
	public bool HasChoices { get; private set; }
	public bool PassengerConversationActive { get; private set; }
	public PassengerEmotion CurrentPassengerEmotion { get; private set; } = PassengerEmotion.None;

	public event Action Changed;
	public event Action<float> TimerChanged;
	public event Action<bool> CarrierFactRevealed;
	public event Action FinalDialogueRead;
	public event Action<bool> PassengerConversationChanged;
	public event Action<PassengerEmotion> PassengerEmotionRequested;

	private bool started;
	private bool finalDialogueAcknowledged;
	private bool terminalOpen;
	private GameFlowController gameFlow;
	private bool subscribedToFlow;
	private string passengerOpeningLine = "";
	private bool pendingEndingBonus;
	private bool solutionReplyTimerStarted;
	private readonly System.Random random = new System.Random();

	private void Init() { }

	private void Update()
	{
		if (gameFlow == null)
		{
			gameFlow = ComponentSystem.FindComponentInWorld<GameFlowController>();
			if (gameFlow != null && !subscribedToFlow)
			{
				gameFlow.ScenarioOneStarted += StartScenario;
				subscribedToFlow = true;
				if (gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioOne) StartScenario();
			}
		}
		if (!started || !timersEnabled || terminalOpen || CurrentStage == Stage.Finished || RemainingSeconds <= 0.0f) return;
		RemainingSeconds = Math.Max(0.0f, RemainingSeconds - Game.IFps);
		if (RemainingSeconds > 0.0f)
		{
			TimerChanged?.Invoke(RemainingSeconds);
			return;
		}
		if (CurrentStage == Stage.Solution) Choose("timeout");
		else if (CurrentStage == Stage.SolutionTimeoutPrompt) FinishSolutionTimeout();
		else if (CurrentStage == Stage.Refusal) ChooseRefusal("ID-10");
	}

	public void StartScenario()
	{
		if (started) return;
		started = true;
		switch (random.Next(0, 3))
		{
			case 0:
				Profile = PassengerProfile.Calm;
				Mood = 2;
				passengerOpeningLine = "Нет, переноски с собой нет. Но он спокойно сидит и никому не мешает.";
				break;
			case 1:
				Profile = PassengerProfile.Anxious;
				Mood = 0;
				passengerOpeningLine = "Переноски нет. Он боится закрытого пространства. Разве нельзя держать его на руках?";
				break;
			default:
				Profile = PassengerProfile.Confrontational;
				Mood = -3;
				passengerOpeningLine = "Нет. Мы всегда так ездили, и раньше никто ничего не говорил.";
				break;
		}
		InitialMood = Mood;
		DecisionHistory.Clear();
		ID02Selected = false;
		EndingBonusGranted = false;
		finalDialogueAcknowledged = false;
		solutionReplyTimerStarted = false;
		CarrierAvailable = random.Next(0, 2) == 0;
		Safety = 0;
		Loyalty = 0;
		CurrentStage = Stage.Opening;
		CurrentSpeaker = "Проводник";
		CurrentText = "Добрый день. Я вижу, что питомец находится вне переноски. Подскажите, пожалуйста, переноска у Вас есть?";
		HasChoices = false;
		BeginPassengerConversation();
		Changed?.Invoke();
	}

	private string GetPassengerOpeningLine()
	{
		return passengerOpeningLine;
	}

	public string[] GetChoiceIds()
	{
		if (CurrentStage == Stage.Opening || CurrentStage == Stage.MoodReaction || CurrentStage == Stage.TerminalPrompt || CurrentStage == Stage.WaitingForTerminal) return Array.Empty<string>();
		if (CurrentStage == Stage.Explanation) return new[] { "ID-01", "ID-02", "ID-03" };
		if ((CurrentStage == Stage.Solution || CurrentStage == Stage.SolutionTimeoutPrompt) && CarrierAvailable)
			return DecisionHistory.Contains("ID-04") ? new[] { "ID-05", "ID-06" } : new[] { "ID-04", "ID-05", "ID-06" };
		if (CurrentStage == Stage.Solution || CurrentStage == Stage.SolutionTimeoutPrompt) return new[] { "ID-07", "ID-08", "ID-09" };
		if (CurrentStage == Stage.Refusal) return new[] { "ID-10", "ID-11", "ID-12" };
		return Array.Empty<string>();
	}

	public string GetChoiceLabel(string id)
	{
		if (id == "ID-01")
			return Mood >= 2
				? "Понимаю, что питомец ведёт себя спокойно. Однако во время поездки его необходимо разместить в переноске — это требуется для безопасности питомца и окружающих."
				: Mood >= -1
					? "Понимаю Ваши сомнения. Даже спокойный питомец во время поездки должен находиться в переноске, поскольку обстановка и его поведение могут неожиданно измениться."
					: "Вижу, что Вы не согласны с требованием. Я не хочу вступать с Вами в спор, но для безопасной поездки питомца необходимо разместить в переноске.";
		if (id == "ID-02")
			return Mood >= 2
				? "Перевозка питомца без переноски не допускается. Вам необходимо выполнить установленное требование, после чего Вы сможете продолжить поездку."
				: Mood >= -1
					? "Вы обязаны разместить питомца в переноске. Спокойное поведение животного не является основанием для исключения."
					: "Ваше несогласие не отменяет требования. Без переноски разрешить дальнейшую перевозку питомца я не могу.";
		if (id == "ID-03")
			return Mood >= 2
				? "Поскольку питомец небольшой и спокойно остаётся у Вас на коленях, можно продолжить поездку без переноски."
				: Mood >= -1
					? "Требование переноски относится прежде всего к крупным или беспокойным животным. Для небольшого спокойного питомца можно сделать исключение."
					: "Учитывая небольшой размер питомца, разрешу продолжить поездку без переноски, если он останется рядом с Вами.";
		if (id == "ID-04")
		{
			if (ID02Selected)
				return Mood >= 1
					? "Прошу прощения, если моя формулировка прозвучала слишком резко. На борту есть подходящая переноска, которую можно приобрести. Я помогу оформить покупку и разместить питомца."
					: Mood >= -2
						? "Понимаю, что мой тон вызвал у Вас недовольство. Приношу извинения за форму обращения. На борту есть подходящая переноска, и я готов помочь с её приобретением."
						: "Вы правы, я был невежлив. Прошу прощения. Требование безопасности остаётся в силе, но я помогу решить вопрос: на борту можно приобрести подходящую переноску.";
			return Mood >= 2
				? "На борту есть подходящая переноска, которую можно приобрести у персонала. Я помогу оформить покупку и безопасно разместить питомца."
				: Mood >= -1
					? "Я проверил наличие: подходящую переноску можно приобрести на борту. Если хотите, я помогу с оформлением и размещением питомца."
					: "Понимаю, что Вы не планировали дополнительных трат. Тем не менее подходящая переноска есть на борту, и я готов помочь оформить её приобретение.";
		}
		if (id == "ID-05")
			return Mood >= 2
				? "На борту есть подходящая переноска, которую можно приобрести у персонала. Для продолжения поездки питомца необходимо разместить внутри. Сообщите, будете ли Вы оформлять покупку сейчас."
				: Mood >= -1
					? "На борту можно приобрести подходящую переноску. Для продолжения поездки питомца необходимо разместить в ней. Какое решение Вы предлагаете?"
					: "Требование остаётся прежним: питомца необходимо разместить в переноске. Если не готовы покупать её, сообщите, какое решение предлагаете.";
		if (id == "ID-06")
			return Mood >= 2
				? "До приобретения переноски можно временно разместить питомца в закрытой дорожной сумке."
				: Mood >= -1
					? "Если у Вас есть закрывающаяся сумка, её можно временно использовать вместо переноски."
					: "Чтобы не продолжать спор, временно разместите питомца в любой закрывающейся сумке.";
		if (id == "ID-07")
			return Mood >= 2
				? "К сожалению, переносок в продаже нет. Я приглашу начальника поезда, чтобы определить дальнейшие действия."
				: Mood >= -1
					? "Я проверил наличие: переносок на борту нет. Поскольку самостоятельно разрешить перевозку в таком виде я не могу, приглашу начальника поезда."
					: "Понимаю, что отсутствие переноски усложняет ситуацию. Я не буду обещать недоступную услугу и приглашу начальника поезда для решения вопроса.";
		if (id == "ID-08")
			return Mood >= 2
				? "Переносок на борту нет. Вы должны были заранее подготовиться к поездке с животным. Оставлять питомца без переноски нельзя, поэтому я вызываю начальника поезда."
				: Mood >= -1
					? "Переносок на борту нет, и решать эту проблему Вы должны были до посадки. Питомец не может оставаться без переноски, я вызываю начальника поезда."
					: "Отсутствие переноски — Ваша ответственность. На борту их нет, а оставлять питомца свободно запрещено. Дальше этим вопросом будет заниматься начальник поезда.";
		if (id == "ID-09")
			return Mood >= 2
				? "Я помогу разместить Вас в менее загруженной части вагона, где питомец сможет находиться рядом."
				: Mood >= -1
					? "Поскольку переноски нет, предложу Вам место подальше от остальных пассажиров."
					: "Чтобы избежать дальнейшего конфликта, размещу Вас с питомцем в другой части вагона.";
		if (id == "ID-10")
			return Mood >= 0
				? "Понимаю, что Вы не хотите приобретать переноску. Я не могу разрешить перевозку в таком виде, поэтому приглашу начальника поезда."
				: Mood >= -2
					? "Понимаю, что Вы не согласны. Чтобы не продолжать спор, я передам решение вопроса начальнику поезда."
					: "Вижу, что разговор стал напряжённым. Я прекращаю обсуждение и приглашаю начальника поезда для дальнейшего решения.";
		if (id == "ID-11")
			return Mood >= 2
				? "Вы отказались от предложенного решения, хотя подходящая переноска имеется на борту. Оставлять питомца без неё нельзя. Я фиксирую Ваш отказ и вызываю начальника поезда."
				: Mood >= -1
					? "Без переноски продолжать поездку с питомцем нельзя. Возможность приобрести её Вам предоставили, но Вы отказались. Теперь вопрос будет решать начальник поезда."
					: "Вы сами отказались выполнять требование и приобретать доступную переноску. Я фиксирую отказ. Дальше с Вами будет разбираться начальник поезда.";
		if (id == "ID-12")
			return Mood >= 0
				? "Я зафиксирую отсутствие переноски и передам информацию начальнику поезда. Пока можете оставаться на месте."
				: Mood >= -2
					? "Чтобы не продолжать спор, я зарегистрирую ситуацию и передам её начальнику поезда."
					: "Я фиксирую Ваш отказ, но до получения дальнейших указаний вмешиваться больше не буду.";
		switch (id)
		{
			default: return id;
		}
	}

	public void Choose(string id)
	{
		if (CurrentStage == Stage.Explanation) ChooseExplanation(id);
		else if (CurrentStage == Stage.Solution || CurrentStage == Stage.SolutionTimeoutPrompt) ChooseSolution(id);
		else if (CurrentStage == Stage.Refusal) ChooseRefusal(id);
	}

	public void NotifyTextFinished()
	{
		if (CurrentStage == Stage.Finished)
		{
			NotifyFinalDialogueRead();
			return;
		}
		if (CurrentStage == Stage.Opening)
		{
			CurrentStage = Stage.Explanation;
			CurrentSpeaker = "Пассажир";
			CurrentText = GetPassengerOpeningLine();
			HasChoices = true;
			RequestPassengerEmotion(Profile == PassengerProfile.Confrontational
				? PassengerEmotion.Aggressive
				: PassengerEmotion.Confused);
		}
		else if (CurrentStage == Stage.MoodReaction)
		{
			CurrentStage = Stage.TerminalPrompt;
			CurrentSpeaker = "Проводник";
			CurrentText = "Переноску можно приобрести на борту, но для начала мне необходимо проверить ее наличие.";
		}
		else if (CurrentStage == Stage.TerminalPrompt)
		{
			CurrentStage = Stage.WaitingForTerminal;
			CurrentSpeaker = "";
			CurrentText = "";
			EndPassengerConversation();
		}
		else if (CurrentStage == Stage.AlternativeSolutionReaction)
		{
			CurrentStage = Stage.Refusal;
			CurrentSpeaker = "";
			CurrentText = "";
			RemainingSeconds = 0.0f;
			HasChoices = true;
		}
		else if (CurrentStage == Stage.SolutionTimeoutPrompt)
		{
			if (!solutionReplyTimerStarted)
			{
				solutionReplyTimerStarted = true;
				RemainingSeconds = timersEnabled ? Math.Max(0.0f, passengerReplySeconds) : 0.0f;
				TimerChanged?.Invoke(RemainingSeconds);
			}
			return;
		}
		else if (CurrentStage == Stage.RefusalSolutionReaction)
		{
			CurrentStage = Stage.Refusal;
			CurrentSpeaker = "Проводник";
			CurrentText = "Пассажир отказался от переноски. Выберите дальнейшее действие.";
			HasChoices = true;
		}
		else if (CurrentStage == Stage.RefusalReaction)
		{
			if (pendingEndingBonus)
			{
				AddSafety(5);
				AddLoyalty(2);
				EndingBonusGranted = true;
			}
			FinishState();
			NotifyFinalDialogueRead();
			return;
		}
		else if (CurrentStage == Stage.CarrierPurchaseReaction)
		{
			CurrentStage = Stage.AwaitingCarrierPickup;
			CurrentSpeaker = "";
			CurrentText = "";
			EndPassengerConversation();
			Changed?.Invoke();
			return;
		}
		else return;

		Changed?.Invoke();
	}

	/// <summary>Called by the conductor terminal when Goods and Services is opened.</summary>
	public bool RevealCarrierAvailability()
	{
		CarrierFactChecked = true;
		CarrierFactRevealed?.Invoke(CarrierAvailable);
		return CarrierAvailable;
	}

	public void ShowTerminalClosed()
	{
		terminalOpen = false;
		if (CurrentStage == Stage.WaitingForTerminal)
		{
			if (CarrierFactChecked)
			{
				CurrentStage = Stage.AwaitingPassengerInteraction;
				CurrentSpeaker = "Проводник";
				CurrentText = "";
				HasChoices = false;
			}
		}
		Changed?.Invoke();
	}

	public void NotifyCarrierPickedUp()
	{
		if (CurrentStage != Stage.AwaitingCarrierPickup) return;
		CurrentStage = Stage.AwaitingPassengerHandoff;
		CurrentSpeaker = "";
		CurrentText = "";
		HasChoices = false;
		Changed?.Invoke();
	}

	public void NotifyPassengerInteraction()
	{
		if (CurrentStage == Stage.AwaitingPassengerInteraction)
		{
			BeginPassengerConversation();
			CurrentStage = Stage.Solution;
			CurrentSpeaker = "Пассажир";
			CurrentText = "Так что вы мне предлагаете?";
			RemainingSeconds = timersEnabled ? Math.Max(0.0f, solutionSeconds) : 0.0f;
			HasChoices = true;
			RequestPassengerEmotion(PassengerEmotion.Confused);
		}
		else if (CurrentStage == Stage.AwaitingPassengerHandoff)
		{
			BeginPassengerConversation();
			CurrentSpeaker = "Пассажир";
			CurrentText = "Спасибо большое! Теперь питомец будет в безопасности.";
			DecisionHistory.Add("CARRIER_DELIVERED");
			RequestPassengerEmotion(PassengerEmotion.CarrierNod);
			FinishWithBonus();
		}
		else return;
		Changed?.Invoke();
	}

	public void SetTerminalOpen(bool value) => terminalOpen = value;

	private void ChooseExplanation(string id)
	{
		if (id == "ID-01") { AddSafety(2); AddLoyalty(2); Mood += 1; }
		else if (id == "ID-02") { AddSafety(2); AddLoyalty(-3); Mood -= 1; }
		else if (id == "ID-03") { AddSafety(-8); AddLoyalty(3); Mood += 2; }
		else return;
		ID02Selected = id == "ID-02";
		DecisionHistory.Add(id);
		Mood = Math.Max(-3, Math.Min(3, Mood));
		CurrentSpeaker = "Пассажир";
		if (id == "ID-03")
		{
			CurrentText = Mood >= 2
				? "Спасибо. Он действительно маленький и будет спокойно сидеть рядом."
				: Mood >= 0
					? "Хорошо. Я прослежу, чтобы он никому не мешал."
					: Mood >= -2
						? "Я же говорил, что для такого маленького питомца переноска не нужна."
						: "Наконец-то Вы поняли, что никакой проблемы здесь нет.";
			RequestPassengerEmotion(Mood >= 0 ? PassengerEmotion.PositiveNod : PassengerEmotion.Aggressive);
			FinishWithoutBonus();
			return;
		}

		if (id == "ID-02")
			CurrentText = Mood >= 2
				? "Можно было объяснить это спокойнее. Хорошо, и что Вы предлагаете?"
				: Mood >= 0
					? "Не нужно разговаривать со мной приказным тоном. Переноску у меня всё равно нет."
					: "Вы не имеете права так со мной разговаривать. Я буду жаловаться, но сначала скажите, что мне теперь делать.";
		else
			CurrentText = Mood >= 2
				? "Понятно. Я не подумал, что он может испугаться. Но переноски у меня нет. Что можно сделать?"
				: Mood >= 0
					? "Я понимаю, но питомец не любит переноски. Есть какой-нибудь другой вариант?"
					: Mood >= -2
						? "Я всё равно не согласен. Но раз Вы настаиваете, что конкретно Вы предлагаете?"
						: "Я уже говорил, что убирать его не собираюсь. Докажите сначала, что у Вас вообще есть правила.";
		RequestPassengerEmotion(id == "ID-02" || Mood < 0
			? PassengerEmotion.Aggressive
			: PassengerEmotion.Confused);
		CurrentStage = Stage.MoodReaction;
		RemainingSeconds = 0.0f;
		HasChoices = false;
		Changed?.Invoke();
	}

	private void ChooseSolution(string id)
	{
		if (id == "timeout")
		{
			AddSafety(-3);
			AddLoyalty(-2);
			Mood = Math.Max(-3, Math.Min(3, Mood - 1));
			DecisionHistory.Add("SOLUTION_TIMEOUT");
			CurrentSpeaker = "Пассажир";
			CurrentText = "Подскажите, пожалуйста, какое решение Вы предлагаете?";
			RequestPassengerEmotion(PassengerEmotion.Confused);
			CurrentStage = Stage.SolutionTimeoutPrompt;
			solutionReplyTimerStarted = false;
			RemainingSeconds = 0.0f;
			HasChoices = true;
			Changed?.Invoke();
			return;
		}
		if (!CarrierFactChecked) return;
		int moodBeforeChoice = Mood;
		if (CarrierAvailable)
		{
			if (id == "ID-04")
			{
				AddSafety(5);
				AddLoyalty(3);
				if (ID02Selected) Mood += 2;
			}
			else if (id == "ID-05") { AddSafety(5); AddLoyalty(-1); Mood -= 1; }
			else if (id == "ID-06") { AddSafety(-6); AddLoyalty(1); }
			else return;
		}
		else
		{
			if (id == "ID-07") { AddSafety(10); AddLoyalty(5); Mood += 1; }
			else if (id == "ID-08") { AddSafety(-6); AddLoyalty(3); Mood += 1; }
			else if (id == "ID-09") { AddSafety(-7); AddLoyalty(2); Mood += 1; }
			else return;
		}
		RemainingSeconds = 0.0f;
		DecisionHistory.Add(id);
		Mood = Math.Max(-3, Math.Min(3, Mood));
		CurrentSpeaker = "Пассажир";
			switch (id)
		{
			case "ID-04":
				if (ID02Selected)
				{
					DecisionHistory.Add("ID-02-APOLOGY");
					CurrentText = Mood >= 2
						? "Хорошо, давайте приобретём переноску. Спасибо, что объяснили всё спокойнее."
						: Mood >= 0
							? "Я всё ещё недоволен Вашим первоначальным тоном, но требование понимаю. Давайте оформим переноску."
							: "Ваш тон мне не понравился, и я это так не оставлю. Но требование я понял — оформляйте переноску.";
					CurrentStage = Stage.CarrierPurchaseReaction;
					RequestPassengerEmotion(PassengerEmotion.PositiveNod);
					RemainingSeconds = 0.0f;
					HasChoices = false;
				}
				else if (Mood >= 2)
				{
					CurrentText = "Хорошо, давайте приобретём переноску. Спасибо, что всё объяснили.";
					CurrentStage = Stage.CarrierPurchaseReaction;
					RequestPassengerEmotion(PassengerEmotion.PositiveNod);
					RemainingSeconds = 0.0f;
					HasChoices = false;
				}
				else
				{
					CurrentText = Mood >= 0
						? "Я понимаю требование, но покупать переноску не хочу."
						: Mood >= -2
							? "Ничего я покупать не буду. Питомец никому не мешает."
							: "Это навязывание покупки. Зовите начальника поезда.";
					CurrentStage = Stage.AlternativeSolutionReaction;
					RequestPassengerEmotion(PassengerEmotion.Aggressive);
					RemainingSeconds = 0.0f;
					HasChoices = false;
				}
				Changed?.Invoke();
				return;
			case "ID-05":
				CurrentText = Mood >= 2
					? "Я понимаю требование, но пока не готов приобретать переноску. Какие ещё варианты Вы можете предложить?"
					: Mood >= -1
						? "Покупать переноску сейчас я не буду. Предложите другое решение."
						: "Ничего покупать не буду. Зовите начальника поезда.";
				CurrentStage = Stage.RefusalSolutionReaction;
				RequestPassengerEmotion(PassengerEmotion.Aggressive);
				RemainingSeconds = 0.0f;
				HasChoices = false;
				Changed?.Invoke();
				return;
			case "ID-06":
				CurrentText = "Хорошо, у меня есть большая сумка. Я могу временно поместить его туда.";
				RequestPassengerEmotion(PassengerEmotion.PositiveNod);
				FinishWithoutBonus();
				return;
			case "ID-07":
				CurrentText = moodBeforeChoice >= 2
					? "Хорошо, приглашайте. Я подожду."
					: moodBeforeChoice >= 0
						? "Хорошо. А что начальник сможет предложить в этой ситуации?"
						: moodBeforeChoice >= -2
							? "Я недоволен, но приглашайте. Я сам хочу с ним поговорить."
							: "Зовите. Заодно я расскажу, как здесь обращаются с пассажирами.";
				RequestPassengerEmotion(moodBeforeChoice >= 0 ? PassengerEmotion.PositiveNod : PassengerEmotion.Aggressive);
				FinishWithBonus();
				return;
			case "ID-08":
				CurrentText = moodBeforeChoice >= 2
					? "Можно было сказать это спокойнее. Хорошо, приглашайте начальника поезда."
					: moodBeforeChoice >= -1
						? "Я понял требование, но разговаривать со мной в таком тоне не нужно. Зовите начальника поезда."
						: "Вы могли объяснить это нормально. Вызывайте начальника, с Вами я больше разговаривать не собираюсь.";
				RequestPassengerEmotion(PassengerEmotion.Aggressive);
				FinishWithoutBonus();
				return;
			case "ID-09":
				CurrentText = "Если можно пересесть в другое место, давайте так и сделаем.";
				RequestPassengerEmotion(PassengerEmotion.PositiveNod);
				FinishWithoutBonus();
				return;
		}
	}

	private void ChooseRefusal(string id)
	{
		if (id == "ID-10")
		{
			AddSafety(5);
			AddLoyalty(2);
			Mood = Math.Max(-3, Math.Min(3, Mood + 1));
			CurrentSpeaker = "Пассажир";
			CurrentText = Mood >= 2
				? "Хорошо, приглашайте. Я подожду."
				: Mood >= 0
					? "Хорошо. Надеюсь, мы сможем нормально решить этот вопрос."
					: Mood == -1
						? "Приглашайте. Я всё равно не согласен с этим требованием."
						: "Зовите начальника, я буду подавать жалобу.";
			RequestPassengerEmotion(Mood >= 0 ? PassengerEmotion.PositiveNod : PassengerEmotion.Aggressive);
			pendingEndingBonus = true;
		}
		else if (id == "ID-11")
		{
			AddSafety(5);
			AddLoyalty(-3);
			Mood = Math.Max(-3, Math.Min(3, Mood - 1));
			CurrentSpeaker = "Пассажир";
			CurrentText = Mood >= 2
				? "Можно было объяснить без такого тона. Хорошо, приглашайте начальника поезда."
				: Mood >= -1
					? "Я понял. Зовите начальника, раз Вы не можете разговаривать нормально."
					: "Зовите кого хотите. С Вами я больше разговаривать не собираюсь.";
			RequestPassengerEmotion(PassengerEmotion.Aggressive);
			pendingEndingBonus = false;
		}
		else if (id == "ID-12")
		{
			AddSafety(-8);
			AddLoyalty(1);
			CurrentSpeaker = "Пассажир";
			CurrentText = "Хорошо, тогда я останусь на своём месте и подожду дальнейшего решения.";
			RequestPassengerEmotion(PassengerEmotion.PositiveNod);
			pendingEndingBonus = false;
		}
		else return;
		DecisionHistory.Add(id);

		CurrentStage = Stage.RefusalReaction;
		RemainingSeconds = 0.0f;
		HasChoices = false;
		Changed?.Invoke();
	}

	private void FinishSolutionTimeout()
	{
		AddSafety(-7);
		AddLoyalty(-2);
		DecisionHistory.Add("SOLUTION_REPLY_TIMEOUT");
		CurrentSpeaker = "Пассажир";
		CurrentText = "Я не могу больше ждать. Я возвращаюсь на своё место.";
		RequestPassengerEmotion(PassengerEmotion.Aggressive);
		FinishWithoutBonus();
	}

	private void FinishWithBonus()
	{
		AddSafety(5); AddLoyalty(2);
		EndingBonusGranted = true;
		FinishState();
	}

	private void FinishWithoutBonus() => FinishState();

	private void FinishState()
	{
		CurrentStage = Stage.Finished;
		RemainingSeconds = 0.0f;
		HasChoices = false;
		Changed?.Invoke();
	}

	private void NotifyFinalDialogueRead()
	{
		if (finalDialogueAcknowledged) return;
		finalDialogueAcknowledged = true;
		EndPassengerConversation();
		FinalDialogueRead?.Invoke();
		if (gameFlow != null) gameFlow.NotifyScenarioOneFinished();
	}

	private void BeginPassengerConversation()
	{
		if (PassengerConversationActive) return;
		PassengerConversationActive = true;
		CurrentPassengerEmotion = PassengerEmotion.None;
		PassengerConversationChanged?.Invoke(true);
	}

	private void EndPassengerConversation()
	{
		if (!PassengerConversationActive) return;
		PassengerConversationActive = false;
		CurrentPassengerEmotion = PassengerEmotion.None;
		PassengerConversationChanged?.Invoke(false);
	}

	private void RequestPassengerEmotion(PassengerEmotion emotion)
	{
		CurrentPassengerEmotion = emotion;
		PassengerEmotionRequested?.Invoke(emotion);
	}

	private void AddSafety(int delta) => Safety = Math.Min(12, Safety + delta);
	private void AddLoyalty(int delta) => Loyalty = Math.Min(7, Loyalty + delta);

	private void Shutdown()
	{
		if (gameFlow != null && subscribedToFlow) gameFlow.ScenarioOneStarted -= StartScenario;
	}

}
