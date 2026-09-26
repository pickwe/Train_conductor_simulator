using System;
using System.Collections.Generic;
using Unigine;

/// <summary>Owns the randomized passenger and carrier facts and the first scenario's score.</summary>
public partial class PetCarrierScenarioController : Component
{
	public enum PassengerProfile { Calm, Anxious, Confrontational }
	public enum Stage { Opening, Explanation, MoodReaction, TerminalPrompt, WaitingForTerminal, Solution, Refusal, RefusalReaction, Finished }

	public float solutionSeconds = 12.0f;
	public float passengerReplySeconds = 8.0f;
	[Parameter(Group = "Scenario")]
	public bool timersEnabled = false;
	public PassengerProfile Profile { get; private set; }
	public bool CarrierAvailable { get; private set; }
	public bool CarrierFactChecked { get; private set; }
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

	public event Action Changed;
	public event Action<float> TimerChanged;
	public event Action<bool> CarrierFactRevealed;
	public event Action FinalDialogueRead;

	private bool started;
	private bool finalDialogueAcknowledged;
	private bool terminalOpen;
	private GameFlowController gameFlow;
	private bool subscribedToFlow;
	private string passengerOpeningLine = "";
	private bool pendingEndingBonus;
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
		else if (CurrentStage == Stage.Refusal) ChooseRefusal("ID-10");
	}

	public void StartScenario()
	{
		if (started) return;
		started = true;
		// switch (random.Next(0, 3))
		// {
		// 	case 0:
		// 		Profile = PassengerProfile.Calm;
		// 		Mood = 1;
		// 		passengerOpeningLine = "Он у меня спокойный и никому не мешает. Зачем ему переноска?";
		// 		break;
		// 	case 1:
		// 		Profile = PassengerProfile.Anxious;
		// 		Mood = 0;
		// 		passengerOpeningLine = "Он очень боится закрытого пространства. Нельзя ли ехать без переноски?";
		// 		break;
		// 	default:
		// 		Profile = PassengerProfile.Confrontational;
		// 		Mood = -1;
		// 		passengerOpeningLine = "Мы всегда так ездили, и никто ничего не говорил. Почему именно сейчас нельзя?";
		// 		break;
		// }
		Profile = PassengerProfile.Confrontational;
		Mood = -2;
		InitialMood = Mood;
		DecisionHistory.Clear();
		EndingBonusGranted = false;
		finalDialogueAcknowledged = false;
		passengerOpeningLine = "Мы всегда так ездили, и никто ничего не говорил. Почему именно сейчас нельзя?";
		// CarrierAvailable = random.Next(0, 2) == 1;
		CarrierAvailable = true;
		Safety = 0;
		Loyalty = 0;
		CurrentStage = Stage.Opening;
		CurrentSpeaker = "Проводник";
		CurrentText = "Добрый день. Я вижу, что питомец находится вне переноски. Подскажите, пожалуйста, переноска у Вас есть?";
		HasChoices = true;
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
		if (CurrentStage == Stage.Solution && CarrierAvailable) return new[] { "ID-04", "ID-05", "ID-06" };
		if (CurrentStage == Stage.Solution) return new[] { "ID-07", "ID-08", "ID-09" };
		if (CurrentStage == Stage.Refusal) return new[] { "ID-10", "ID-11", "ID-12" };
		return Array.Empty<string>();
	}

	public string GetChoiceLabel(string id)
	{
		switch (id)
		{
			case "ID-01": return "Понимаю, что питомец ведёт себя спокойно. Однако во время поездки его необходимо разместить в переноске. Это требуется для безопасного нахождения питомца и окружающих на борту.";
			case "ID-02": return "Понимаю Вашу ситуацию. Если питомец спокойно находится на руках и не выходит в проход, допускается продолжить поездку без переноски под Вашим постоянным контролем.";
			case "ID-03": return "Понимаю. Переноска обязательна прежде всего для крупных или беспокойных животных. Если питомец небольшой и остаётся у Вас на коленях, можно продолжить поездку в таком виде.";
			case "ID-04": return "На борту есть подходящая переноска, её можно приобрести у персонала. Я помогу Вам оформить покупку. После этого, пожалуйста, разместите питомца внутри.";
			case "ID-05": return "На борту можно приобрести переноску. Если Вы не планируете покупку, питомец может остаться у Вас на руках при условии, что он не будет мешать другим пассажирам.";
			case "ID-06": return "На борту есть переноска, но до оформления покупки можно разместить питомца в закрытой сумке или укутать его так, чтобы он не мог свободно перемещаться по вагону.";
			case "ID-07": return "К сожалению, на этом рейсе переноски в продаже нет. Оставлять питомца без безопасного размещения нельзя, поэтому я приглашу начальника поезда для дальнейшего решения ситуации.";
			case "ID-08": return "К сожалению, переносок в продаже нет. В таком случае Вы можете продолжить поездку, если будете постоянно держать питомца рядом и возьмёте ответственность за его поведение.";
			case "ID-09": return "Переносок в продаже нет, поэтому я помогу разместить Вас в незагруженной части вагона. Там питомец сможет находиться рядом с Вами, не мешая другим пассажирам.";
			case "ID-10": return "Понимаю, что Вы не согласны. Однако разрешить перевозку питомца в таком виде я не могу, поэтому приглашаю начальника поезда для урегулирования ситуации.";
			case "ID-11": return "Понимаю, что Вы не хотите приобретать переноску. Если Вы гарантируете, что питомец останется рядом с Вами, я разрешу продолжить поездку под Вашу ответственность.";
			case "ID-12": return "Я фиксирую, что переноска отсутствует, и передаю информацию начальнику поезда. До получения дальнейших указаний оставайтесь на месте с питомцем.";
			default: return id;
		}
	}

	public void Choose(string id)
	{
		if (CurrentStage == Stage.Explanation) ChooseExplanation(id);
		else if (CurrentStage == Stage.Solution) ChooseSolution(id);
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
				CurrentStage = Stage.Solution;
				CurrentSpeaker = "Проводник";
				CurrentText = "";
				RemainingSeconds = timersEnabled ? Math.Max(0.0f, solutionSeconds) : 0.0f;
				HasChoices = true;
			}
		}
		Changed?.Invoke();
	}

	public void SetTerminalOpen(bool value) => terminalOpen = value;

	private void ChooseExplanation(string id)
	{
		if (id == "ID-01") { AddSafety(2); AddLoyalty(2); Mood += 1; }
		else if (id == "ID-02") { AddSafety(-5); AddLoyalty(2); Mood += 1; }
		else if (id == "ID-03") { AddSafety(-8); AddLoyalty(3); Mood += 2; }
		else return;
		DecisionHistory.Add(id);
		Mood = Math.Max(-3, Math.Min(3, Mood));
		CurrentSpeaker = "Пассажир";
		if (id == "ID-02" || id == "ID-03")
		{
			CurrentText = "Хорошо, я буду постоянно держать его рядом и не позволю выходить в проход.";
			FinishWithoutBonus();
			return;
		}

		CurrentText = Mood >= 2
			? "Понятно. Переноски у меня с собой нет. Что можно сделать?"
			: Mood >= 0
				? "Я понимаю, но питомец не любит переноски. Есть другой безопасный вариант?"
				: "Я всё равно не понимаю, кому он мешает. Что Вы предлагаете?";
		CurrentStage = Stage.MoodReaction;
		RemainingSeconds = 0.0f;
		HasChoices = false;
		Changed?.Invoke();
	}

	private void ChooseSolution(string id)
	{
		if (id == "timeout") { CurrentSpeaker = "Пассажир"; CurrentText = "Я не могу больше ждать. Я возвращаюсь на своё место."; FinishWithoutBonus(); return; }
		if (!CarrierFactChecked) return;
		if (CarrierAvailable)
		{
			if (id == "ID-04") { AddSafety(5); AddLoyalty(3); Mood += 1; }
			else if (id == "ID-05") { AddSafety(-5); AddLoyalty(2); Mood += 1; }
			else if (id == "ID-06") { AddSafety(-5); AddLoyalty(1); Mood += 1; }
			else return;
		}
		else
		{
			if (id == "ID-07") { AddSafety(10); AddLoyalty(5); Mood += 1; }
			else if (id == "ID-08") { AddSafety(-6); AddLoyalty(3); Mood += 1; }
			else if (id == "ID-09") { AddSafety(-7); AddLoyalty(2); Mood += 1; }
			else return;
		}
		DecisionHistory.Add(id);
		Mood = Math.Max(-3, Math.Min(3, Mood));
		CurrentSpeaker = "Пассажир";
		switch (id)
		{
			case "ID-04":
				if (Mood >= 2)
				{
					CurrentText = "Хорошо, давайте приобретём переноску. Спасибо, что всё объяснили.";
					FinishWithBonus();
				}
				else
				{
					CurrentText = Mood >= 0
						? "Я понимаю, но покупать переноску не хочу. Она мне больше не понадобится."
						: "Ничего я покупать не буду. Мой питомец никому не мешает.";
					CurrentStage = Stage.Refusal;
					RemainingSeconds = timersEnabled ? Math.Max(0.0f, passengerReplySeconds) : 0.0f;
					HasChoices = true;
					Changed?.Invoke();
				}
				return;
			case "ID-05":
				CurrentText = "Если переноска необязательна, я лучше буду держать питомца на руках. Покупать её не хочу.";
				FinishWithoutBonus();
				return;
			case "ID-06":
				CurrentText = "Хорошо, у меня есть большая сумка. Я могу временно поместить его туда.";
				FinishWithoutBonus();
				return;
			case "ID-07":
				CurrentText = Mood >= 2
					? "Хорошо, приглашайте. До его прихода я буду держать питомца рядом."
					: Mood >= 0
						? "Хорошо. А что начальник поезда сможет предложить в этой ситуации?"
						: "Зовите. Я всё равно считаю, что питомец никому не мешает.";
				FinishWithBonus();
				return;
			case "ID-08":
				CurrentText = "Хорошо, я буду держать его рядом и прослежу, чтобы он никому не мешал.";
				FinishWithoutBonus();
				return;
			case "ID-09":
				CurrentText = "Если можно пересесть в другое место, давайте так и сделаем.";
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
					? "Хорошо. Надеюсь, мы сможем спокойно решить этот вопрос."
					: "Зовите. Я сам хочу обсудить это с начальником поезда.";
			pendingEndingBonus = true;
		}
		else if (id == "ID-11")
		{
			AddSafety(-10);
			AddLoyalty(3);
			Mood = Math.Max(-3, Math.Min(3, Mood + 2));
			CurrentSpeaker = "Пассажир";
			CurrentText = "Хорошо, я гарантирую, что питомец будет находиться рядом со мной.";
			pendingEndingBonus = false;
		}
		else if (id == "ID-12")
		{
			AddSafety(-8);
			AddLoyalty(1);
			CurrentSpeaker = "Пассажир";
			CurrentText = "Хорошо, тогда я останусь на своём месте и подожду дальнейшего решения.";
			pendingEndingBonus = false;
		}
		else return;
		DecisionHistory.Add(id);

		CurrentStage = Stage.RefusalReaction;
		RemainingSeconds = 0.0f;
		HasChoices = false;
		Changed?.Invoke();
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
		FinalDialogueRead?.Invoke();
		if (gameFlow != null) gameFlow.NotifyScenarioOneFinished();
	}

	private void AddSafety(int delta) => Safety = Math.Min(12, Safety + delta);
	private void AddLoyalty(int delta) => Loyalty = Math.Min(7, Loyalty + delta);

	private void Shutdown()
	{
		if (gameFlow != null && subscribedToFlow) gameFlow.ScenarioOneStarted -= StartScenario;
	}

}
