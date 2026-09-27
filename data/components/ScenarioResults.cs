using System.Collections.Generic;
using System.Text;
using Unigine;

/// <summary>Shows scenario one feedback and achievement results after the final line finishes typing.</summary>
public partial class ScenarioResults : Component
{
	public static bool IsVisible { get; private set; }

	[ParameterSlider(Min = 0.1f, Max = 3.0f, Group = "Results")]
	public float fadeInSeconds = 0.65f;

	private Gui gui;
	private WidgetSprite panel;
	private WidgetScrollBox resultsScroll;
	private WidgetLabel heading, body;
	private WidgetButton continueButton;
	private PetCarrierScenarioController scenario;
	private PassengerScenarioController secondScenario;
	private GameFlowController gameFlow;
	private bool subscribed, secondSubscribed, visible;
	private bool showingSecondScenarioResult;
	private Input.MOUSE_HANDLE previousMouseHandle;
	private bool previousMouseGrab, previousCursorHide, mouseStateSaved;
	private float fade;

	private sealed class AchievementResult
	{
		public string Id;
		public string Title;
		public string Description;
		public string EarnedText;
		public bool Hidden;

		public AchievementResult(string id, string title, string description, string earnedText, bool hidden = false)
		{
			Id = id;
			Title = title;
			Description = description;
			EarnedText = earnedText;
			Hidden = hidden;
		}
	}

	private void Init()
	{
		gui = Gui.GetCurrent();
		panel = new WidgetSprite(gui, "") { Color = new vec4(0.05f, 0.07f, 0.09f, 0.96f), Hidden = true };
		heading = new WidgetLabel(gui, "ИТОГИ СЦЕНАРИЯ")
		{
			FontSize = 25,
			FontColor = new vec4(1.0f, 0.88f, 0.48f, 0.0f),
			FontOutline = 1,
			Hidden = true
		};
		body = new WidgetLabel(gui, "")
		{
			FontSize = 18,
			FontColor = new vec4(1.0f, 1.0f, 1.0f, 0.0f),
			FontWrap = 1,
			FontOutline = 1,
			Hidden = true
		};
		resultsScroll = new WidgetScrollBox(gui)
		{
			Hidden = true,
			HScrollEnabled = false,
			VScrollHidden = WidgetScrollBox.SCROLL_RENDER_MODE.AUTO_HIDE
		};
		continueButton = new WidgetButton(gui, "Продолжить") { FontSize = 18, Hidden = true };
		continueButton.EventClicked.Connect(Continue);
		gui.AddChild(panel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		gui.AddChild(heading, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		resultsScroll.AddChild(body);
		gui.AddChild(resultsScroll, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		gui.AddChild(continueButton, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
	}

	private void Update()
	{
		if (scenario == null)
		{
			scenario = ComponentSystem.FindComponentInWorld<PetCarrierScenarioController>();
			if (scenario != null)
			{
				scenario.FinalDialogueRead += Show;
				subscribed = true;
			}
		}
		if (gameFlow == null)
			gameFlow = ComponentSystem.FindComponentInWorld<GameFlowController>();
		if (secondScenario == null || (PassengerScenarioController.Active != null && secondScenario != PassengerScenarioController.Active))
		{
			if (secondScenario != null && secondSubscribed)
				secondScenario.SecondScenarioResultRequested -= ShowSecondScenarioResult;
			secondScenario = PassengerScenarioController.Active ?? ComponentSystem.FindComponentInWorld<PassengerScenarioController>();
			secondSubscribed = false;
			if (secondScenario != null && secondScenario == PassengerScenarioController.Active)
			{
				secondScenario.SecondScenarioResultRequested += ShowSecondScenarioResult;
				secondSubscribed = true;
			}
		}
		if (!visible) return;

		fade = MathLib.Min(1.0f, fade + Game.IFps / MathLib.Max(0.1f, fadeInSeconds));
		vec4 headingColor = heading.FontColor;
		headingColor.w = fade;
		heading.FontColor = headingColor;
		vec4 bodyColor = body.FontColor;
		bodyColor.w = fade;
		body.FontColor = bodyColor;
		vec4 panelColor = panel.Color;
		panelColor.w = 0.96f * fade;
		panel.Color = panelColor;
		continueButton.Enabled = fade >= 1.0f;
	}

	private void Show()
	{
		if (visible || scenario == null) return;
		showingSecondScenarioResult = false;
		heading.Text = "ИТОГИ СЦЕНАРИЯ 1";
		continueButton.Text = "Продолжить";
		ShowResults(BuildResultsText());
	}

	private void ShowSecondScenarioResult(string finalTitle, string feedback)
	{
		if (visible) return;
		showingSecondScenarioResult = true;
		heading.Text = "ОБЩИЙ ИТОГ ДВУХ СЦЕНАРИЕВ";
		continueButton.Text = "Завершить";
		ShowResults(BuildCombinedResultsText(finalTitle, feedback));
	}

	private string BuildCombinedResultsText(string secondFinalTitle, string secondFeedback)
	{
		StringBuilder text = new StringBuilder();
		text.AppendLine("СЦЕНАРИЙ 1 — ПЕРЕВОЗКА ПИТОМЦА");
		text.AppendLine(GetFeedback());
		text.AppendFormat("Изменения: безопасность {0:+#;-#;0}, лояльность {1:+#;-#;0}.", scenario.Safety, scenario.Loyalty);
		text.AppendLine();
		text.AppendLine();
		text.AppendLine("СЦЕНАРИЙ 2 — ЖАЛОБА НА ШУМ");
		text.AppendLine(secondFinalTitle);
		text.AppendLine(secondFeedback);
		text.AppendFormat("Изменения: безопасность {0:+#;-#;0}, лояльность {1:+#;-#;0}.",
			secondScenario.SafetyDelta, secondScenario.LoyaltyDelta);
		text.AppendLine();
		text.AppendLine();

		StoryStateController state = ComponentSystem.FindComponentInWorld<StoryStateController>();
		text.AppendLine("ОБЩИЙ РЕЗУЛЬТАТ");
		text.AppendFormat("За два сценария: безопасность {0:+#;-#;0}, лояльность {1:+#;-#;0}.",
			scenario.Safety + secondScenario.SafetyDelta,
			scenario.Loyalty + secondScenario.LoyaltyDelta);
		text.AppendLine();
		if (state != null)
			text.AppendFormat("Текущие показатели смены: безопасность {0}, лояльность {1}.", state.Safety, state.Loyalty);
		text.AppendLine();
		text.AppendLine();
		text.AppendLine("ДОСТИЖЕНИЯ ЗА ОБА СЦЕНАРИЯ:");

		List<AchievementResult> earned = GetEarnedAchievements();
		earned.AddRange(GetSecondScenarioAchievements());
		AppendAchievements(text, earned);
		return text.ToString();
	}

	private static void AppendAchievements(StringBuilder text, List<AchievementResult> earned)
	{
		if (earned.Count == 0)
		{
			text.Append("В этот раз достижения не получены.");
			return;
		}

		foreach (AchievementResult achievement in earned)
		{
			text.AppendFormat("★ {0}{1} — «{2}»", achievement.Hidden ? "Скрытая ачивка: " : "", achievement.Id, achievement.Title);
			text.AppendLine();
			text.AppendLine("Описание: " + achievement.Description);
			text.AppendLine((achievement.Hidden ? "Скрытая ачивка получена! " : "Ачивка получена! ") + achievement.EarnedText);
		}
	}

	private void ShowResults(string resultsText)
	{
		DialogueWindow dialogue = ComponentSystem.FindComponentInWorld<DialogueWindow>();
		dialogue?.HideForResults();
		previousMouseHandle = Input.MouseHandle;
		previousMouseGrab = Input.MouseGrab;
		previousCursorHide = Input.MouseCursorHide;
		mouseStateSaved = true;
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;
		Input.MouseGrab = false;
		Input.MouseCursorHide = false;
		Input.MouseCursorNeedUpdate = true;
		visible = true;
		IsVisible = true;
		fade = 0.0f;
		body.Text = resultsText;
		panel.Hidden = heading.Hidden = body.Hidden = resultsScroll.Hidden = continueButton.Hidden = false;
		resultsScroll.VScrollValue = 0;
		Layout();
	}

	private string BuildResultsText()
	{
		StringBuilder text = new StringBuilder();
		text.AppendLine(GetFeedback());
		text.AppendLine();
		if (scenario.EndingBonusGranted)
		{
			text.AppendLine("Бонус за успешное разрешение: безопасность +5, лояльность +2.");
			text.AppendLine();
		}
		text.AppendFormat("Итог сценария: безопасность {0}, лояльность {1}.", scenario.Safety, scenario.Loyalty);
		text.AppendLine();
		text.AppendLine();
		text.AppendLine("Достижения:");

		List<AchievementResult> earned = GetEarnedAchievements();
		if (earned.Count == 0)
			text.Append("В этот раз достижения не получены.");
		else
			foreach (AchievementResult achievement in earned)
			{
				text.AppendFormat("★ {0}{1} — «{2}»", achievement.Hidden ? "Скрытая ачивка: " : "", achievement.Id, achievement.Title);
				text.AppendLine();
				text.AppendLine("Описание: " + achievement.Description);
				text.AppendLine((achievement.Hidden ? "Скрытая ачивка получена! " : "Ачивка получена! ") + achievement.EarnedText);
			}
		return text.ToString();
	}

	private string GetFeedback()
	{
		if (HasDecision("ID-10"))
			return "Вы предложили безопасное решение, но пассажир отказался. Вы не стали спорить или предоставлять исключение, а передали ситуацию начальнику поезда. Это правильное действие после прямого отказа.";

		if (HasDecision("ID-11"))
			return "Вы не разрешили продолжить поездку с питомцем без переноски, зафиксировали отказ пассажира и своевременно вызвали начальника поезда. С точки зрения безопасности решение правильное. Однако обвинительная и резкая формулировка усилила конфликт и снизила его лояльность. Требование следовало повторить нейтрально, не перекладывая разговор в форму личного упрёка.";

		if (HasDecision("ID-12"))
			return "Фиксация нарушения не устраняет текущий риск. Вы сообщили о ситуации, но фактически разрешили пассажиру продолжить поездку в прежнем виде.";

		if (HasDecision("ID-07"))
			return "Вы не стали грубить пассажиру и не предложили небезопасную замену переноске. Ситуация была своевременно передана начальнику поезда. Это правильное решение для обеспечения безопасности.";

		if (HasDecision("ID-08"))
			return "Вы правильно сообщили, что переноски на борту нет, и не разрешили оставить питомца свободно. Ситуация передана начальнику поезда. Однако обвинительная формулировка ухудшила настроение пассажира.";

		if (HasDecision("ID-09"))
			return "Пересадка может уменьшить неудобство для окружающих, но не обеспечивает безопасность питомца. Проблема была перемещена в другую часть вагона, а не решена.";

		if (HasDecision("ID-02-APOLOGY") && scenario.EndingBonusGranted)
			return "Вы признали, что первоначальная формулировка прозвучала резко, извинились и повторно предложили доступную переноску. Пассажир согласился, питомец размещён безопасно.";

		if (scenario.EndingBonusGranted)
			return "Вы спокойно объяснили требование и предложили пассажиру безопасное решение. Пассажир согласился, а питомец размещён в переноске.";

		if (HasPath("ID-01", "ID-04", "ID-10"))
			return "Вы предложили переноску и после отказа передали ситуацию начальнику поезда. Это сохранило безопасный порядок действий, хотя пассажир не воспользовался переноской.";

		if (HasPath("ID-01", "ID-04"))
			return "Вы предложили переноску, но безопасное размещение питомца не было подтверждено. Ситуация завершилась без успешного разрешения.";

		if (HasPath("ID-01", "ID-06"))
			return "Большая сумка не гарантирует вентиляцию, надёжное закрытие и защиту питомца. Предложенное решение не устраняет риск.";

		if (HasPath("ID-02") || HasPath("ID-03"))
			return "Вы разрешили продолжить поездку без переноски. Контроль поведения питомца не устраняет риск для него и окружающих.";

		return "Ситуация завершилась без подтверждённого безопасного размещения питомца. Передача вопроса начальнику поезда позволяет принять решение с учётом обстоятельств.";
	}

	private List<AchievementResult> GetEarnedAchievements()
	{
		List<AchievementResult> earned = new List<AchievementResult>();
		if (scenario.CarrierAvailable
			&& HasChoicePath("ID-01", "ID-04")
			&& HasDecision("CARRIER_DELIVERED")
			&& scenario.Mood >= 2
			&& scenario.EndingBonusGranted)
		{
			earned.Add(new AchievementResult(
				"ACH_PET_01",
				"Мягкие лапы — твёрдые правила",
				"Спокойно объяснить требование и добиться безопасного размещения питомца без конфликта.",
				"Вы сохранили лояльность пассажира и обеспечили безопасное размещение питомца."));
		}
		if (scenario.CarrierAvailable
			&& HasChoicePath("ID-02", "ID-04")
			&& HasDecision("ID-02-APOLOGY")
			&& HasDecision("CARRIER_DELIVERED")
			&& scenario.EndingBonusGranted)
		{
			earned.Add(new AchievementResult(
				"ACH_PET_02",
				"Ошибка признана",
				"Исправить собственную грубость, признать неудачную формулировку и всё-таки убедить пассажира приобрести переноску.",
				"Вы признали ошибку в общении, восстановили диалог и нашли безопасное решение."));
		}
		if (scenario.CarrierAvailable
			&& HasChoicePath("ID-01", "ID-05", "ID-10")
			&& scenario.EndingBonusGranted)
		{
			earned.Add(new AchievementResult(
				"ACH_PET_03",
				"Три шага до порядка",
				"Пройти все три этапа: объяснить требование, предложить переноску и корректно вызвать начальника поезда после отказа.",
				"Объяснение, решение, эскалация — Вы правильно прошли все три этапа ситуации."));
		}
		if (!scenario.CarrierAvailable
			&& HasChoicePath("ID-01", "ID-07")
			&& !HasDecision("SOLUTION_TIMEOUT")
			&& scenario.EndingBonusGranted)
		{
			earned.Add(new AchievementResult(
				"ACH_PET_04",
				"Правила без витрины",
				"Правильно действовать, когда переноски нет в продаже, и не обещать пассажиру недоступную услугу.",
				"Вы честно сообщили об отсутствии переноски и выбрали правильный порядок действий."));
		}
		if (scenario.InitialMood == -3
			&& scenario.CarrierAvailable
			&& HasChoicePath("ID-01", "ID-05", "ID-10")
			&& scenario.EndingBonusGranted)
		{
			earned.Add(new AchievementResult(
				"ACH_PET_05",
				"Холодная голова",
				"Сохранить профессиональный тон при общении с изначально конфликтным пассажиром.",
				"Пассажир был настроен на конфликт, но Вы сохранили спокойствие и действовали профессионально.",
				true));
		}
		if (scenario.CarrierAvailable
			&& HasChoicePath("ID-02", "ID-05", "ID-11")
			&& !scenario.EndingBonusGranted)
		{
			earned.Add(new AchievementResult(
				"ACH_PET_06",
				"Ледяной протокол",
				"Соблюсти требования безопасности, выбирая только правильные, но грубые ответы.",
				"Требования безопасности соблюдены, но холодный тон стоил Вам лояльности пассажира.",
				true));
		}
		return earned;
	}

	private List<AchievementResult> GetSecondScenarioAchievements()
	{
		List<AchievementResult> earned = new List<AchievementResult>();
		if (secondScenario == null)
			return earned;

		bool politeMaterials = HasSecondPath("ID-01", "ID-04")
			&& secondScenario.MoodPassenger1 >= 2
			&& secondScenario.MoodPassenger2 >= 2;
		if (politeMaterials)
		{
			earned.Add(new AchievementResult(
				"ACH_NOISE_01",
				"Тише без приказов",
				"Снизить уровень шума с помощью детских материалов, не обвиняя родителей и детей.",
				"Вы помогли детям найти спокойное занятие и сохранили комфорт обеих сторон."));
		}

		if (secondScenario.DiplomatWithoutPropsEligible)
		{
			earned.Add(new AchievementResult(
				"ACH_NOISE_02",
				"Дипломат без реквизита",
				"Урегулировать ситуацию вежливым разговором, когда детских материалов на борту нет.",
				"Даже без дополнительных материалов Вы нашли решение с помощью спокойного диалога."));
		}

		bool politeParentBranch = HasSecondPrefix("ID-01", "ID-04") || HasSecondPrefix("ID-01", "ID-07");
		if (politeParentBranch && HasSecondLastDecision("ID-10"))
		{
			earned.Add(new AchievementResult(
				"ACH_NOISE_03",
				"Тихий переезд",
				"Предложить пожаловавшемуся пассажиру проверенное место того же класса и не перекладывать проблему на семью.",
				"Вы предложили корректную пересадку и решили проблему без обвинений и принуждения."));
		}

		if (politeParentBranch && HasSecondLastDecision("ID-13"))
		{
			earned.Add(new AchievementResult(
				"ACH_NOISE_04",
				"Мест нет — решение есть",
				"Не давать ложных обещаний, когда свободных мест нет, и предложить правильный порядок дальнейших действий.",
				"Вы честно сообщили об отсутствии мест и не оставили жалобу пассажира без внимания."));
		}

		bool safeFinal = HasSecondLastDecision("ID-10") || HasSecondLastDecision("ID-13");
		if (secondScenario.InitialMood == -3
			&& secondScenario.InitialParentMood == -3
			&& politeParentBranch
			&& safeFinal)
		{
			earned.Add(new AchievementResult(
				"ACH_NOISE_05",
				"На два фронта",
				"Успешно завершить ситуацию, когда и пассажир с жалобой, и родители изначально настроены конфликтно.",
				"Вы удержали под контролем конфликт сразу двух пассажиров и не допустили открытого столкновения.",
				true));
		}

		if (HasAnySecondPath(
			new[] { "ID-01", "ID-04", "ID-10" },
			new[] { "ID-01", "ID-04", "ID-13" },
			new[] { "ID-01", "ID-07", "ID-10" },
			new[] { "ID-01", "ID-07", "ID-13" }))
		{
			earned.Add(new AchievementResult(
				"ACH_NOISE_06",
				"Без крайнего виноватого",
				"Завершить сценарий, ни разу не обвинив детей, родителей или пассажира с жалобой.",
				"Вы решили проблему, не назначая виноватых и сохранив уважение к обеим сторонам."));
		}

		if (HasAnySecondPath(
			new[] { "ID-02", "ID-05", "ID-11" },
			new[] { "ID-02", "ID-05", "ID-14" },
			new[] { "ID-02", "ID-08", "ID-11" },
			new[] { "ID-02", "ID-08", "ID-14" }))
		{
			earned.Add(new AchievementResult(
				"ACH_NOISE_07",
				"Всё правильно. Всё грубо.",
				"Выбирать только безопасные, но грубые действия на всех трёх этапах.",
				"Все решения соответствовали требованиям, но Ваш тон превратил помощь в конфликт.",
				true));
		}

		return earned;
	}

	private bool HasSecondPath(params string[] expected)
	{
		if (secondScenario == null || secondScenario.DecisionHistory.Count != expected.Length)
			return false;
		for (int i = 0; i < expected.Length; i++)
			if (secondScenario.DecisionHistory[i] != expected[i]) return false;
		return true;
	}

	private bool HasSecondPrefix(params string[] expected)
	{
		if (secondScenario == null || secondScenario.DecisionHistory.Count < expected.Length)
			return false;
		for (int i = 0; i < expected.Length; i++)
			if (secondScenario.DecisionHistory[i] != expected[i]) return false;
		return true;
	}

	private bool HasSecondLastDecision(string id)
	{
		return secondScenario != null
			&& secondScenario.DecisionHistory.Count > 0
			&& secondScenario.DecisionHistory[secondScenario.DecisionHistory.Count - 1] == id;
	}

	private bool HasAnySecondPath(params string[][] paths)
	{
		for (int i = 0; i < paths.Length; i++)
			if (HasSecondPath(paths[i])) return true;
		return false;
	}

	private bool HasChoicePath(params string[] expected)
	{
		int choiceIndex = 0;
		for (int i = 0; i < scenario.DecisionHistory.Count; i++)
		{
			string entry = scenario.DecisionHistory[i];
			if (entry == null || entry.Length != 5 || !entry.StartsWith("ID-"))
				continue;
			if (choiceIndex >= expected.Length || entry != expected[choiceIndex])
				return false;
			choiceIndex++;
		}
		return choiceIndex == expected.Length;
	}

	private bool HasPath(params string[] expected)
	{
		if (scenario.DecisionHistory.Count < expected.Length) return false;
		for (int i = 0; i < expected.Length; i++)
			if (scenario.DecisionHistory[i] != expected[i]) return false;
		return true;
	}

	private bool HasDecision(string id)
	{
		return scenario.DecisionHistory.Contains(id);
	}

	private void Layout()
	{
		int width = MathLib.Min(900, gui.Width - 48);
		int height = MathLib.Min(700, gui.Height - 48);
		int left = (gui.Width - width) / 2;
		int top = (gui.Height - height) / 2;
		panel.SetPosition(left, top);
		panel.Width = width;
		panel.Height = height;
		heading.SetPosition(left + 30, top + 24);
		resultsScroll.SetPosition(left + 30, top + 72);
		resultsScroll.Width = width - 60;
		resultsScroll.Height = height - 150;
		body.SetPosition(0, 0);
		body.Width = resultsScroll.Width - 24;
		continueButton.SetPosition(left + width - 190, top + height - 60);
		continueButton.Width = 160;
		continueButton.Height = 38;
	}

	private void Continue()
	{
		if (fade < 1.0f || gameFlow == null) return;
		panel.Hidden = heading.Hidden = body.Hidden = resultsScroll.Hidden = continueButton.Hidden = true;
		visible = false;
		IsVisible = false;
		RestoreMouse();
		if (showingSecondScenarioResult)
		{
			showingSecondScenarioResult = false;
			secondScenario?.NotifySecondScenarioResultsClosed();
			gameFlow.ShowRunSummary();
			gameFlow.FinishRun();
		}
		else
			gameFlow.StartScenarioTwoIntroduction();
	}

	private void RestoreMouse()
	{
		if (!mouseStateSaved) return;
		Input.MouseHandle = previousMouseHandle;
		Input.MouseGrab = previousMouseGrab;
		Input.MouseCursorHide = previousCursorHide;
		Input.MouseCursorNeedUpdate = true;
		mouseStateSaved = false;
	}

	private void Shutdown()
	{
		IsVisible = false;
		if (scenario != null && subscribed) scenario.FinalDialogueRead -= Show;
		if (secondScenario != null && secondSubscribed)
			secondScenario.SecondScenarioResultRequested -= ShowSecondScenarioResult;
		RestoreMouse();
		panel?.DeleteLater();
		heading?.DeleteLater();
		resultsScroll?.DeleteLater();
		continueButton?.DeleteLater();
	}
}
