using System.Collections.Generic;
using System.Text;
using Unigine;

/// <summary>Shows scenario one feedback and achievement results after the final line finishes typing.</summary>
public partial class ScenarioResults : Component
{
	[ParameterSlider(Min = 0.1f, Max = 3.0f, Group = "Results")]
	public float fadeInSeconds = 0.65f;

	private Gui gui;
	private WidgetSprite panel;
	private WidgetLabel heading, body;
	private WidgetButton continueButton;
	private PetCarrierScenarioController scenario;
	private GameFlowController gameFlow;
	private bool subscribed, visible;
	private float fade;

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
		continueButton = new WidgetButton(gui, "Продолжить") { FontSize = 18, Hidden = true };
		continueButton.EventClicked.Connect(Continue);
		gui.AddChild(panel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		gui.AddChild(heading, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		gui.AddChild(body, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
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
		visible = true;
		fade = 0.0f;
		body.Text = BuildResultsText();
		panel.Hidden = heading.Hidden = body.Hidden = continueButton.Hidden = false;
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

		List<string> earned = GetEarnedAchievements();
		if (earned.Count == 0)
			text.Append("В этот раз достижения не получены.");
		else
			foreach (string achievement in earned)
			{
				text.AppendLine("★ " + achievement);
				text.AppendLine("Достижение получено!");
			}
		return text.ToString();
	}

	private string GetFeedback()
	{
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

	private List<string> GetEarnedAchievements()
	{
		List<string> earned = new List<string>();
		if (HasPath("ID-01", "ID-04") && scenario.Mood >= 2 && scenario.EndingBonusGranted)
			earned.Add("Мягкие лапы — твёрдые правила");
		if (HasPath("ID-01", "ID-04", "ID-10") && scenario.Mood <= 1)
			earned.Add("Три шага до порядка");
		if (scenario.InitialMood == -1 && HasPath("ID-01", "ID-04", "ID-10") && scenario.Mood == 1)
			earned.Add("Холодная голова");
		if (HasPath("ID-01", "ID-07"))
			earned.Add("Без права на удобное исключение");
		return earned;
	}

	private bool HasPath(params string[] expected)
	{
		if (scenario.DecisionHistory.Count < expected.Length) return false;
		for (int i = 0; i < expected.Length; i++)
			if (scenario.DecisionHistory[i] != expected[i]) return false;
		return true;
	}

	private void Layout()
	{
		int width = MathLib.Min(900, gui.Width - 48);
		int height = MathLib.Min(500, gui.Height - 48);
		int left = (gui.Width - width) / 2;
		int top = (gui.Height - height) / 2;
		panel.SetPosition(left, top);
		panel.Width = width;
		panel.Height = height;
		heading.SetPosition(left + 30, top + 24);
		body.SetPosition(left + 30, top + 72);
		body.Width = width - 60;
		continueButton.SetPosition(left + width - 190, top + height - 60);
		continueButton.Width = 160;
		continueButton.Height = 38;
	}

	private void Continue()
	{
		if (fade < 1.0f || gameFlow == null) return;
		panel.Hidden = heading.Hidden = body.Hidden = continueButton.Hidden = true;
		visible = false;
		gameFlow.StartScenarioTwoIntroduction();
	}

	private void Shutdown()
	{
		if (scenario != null && subscribed) scenario.FinalDialogueRead -= Show;
		panel?.DeleteLater();
		heading?.DeleteLater();
		body?.DeleteLater();
		continueButton?.DeleteLater();
	}
}
