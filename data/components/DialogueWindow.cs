using System.Collections.Generic;
using Unigine;

/// <summary>Bottom-screen dialogue bar with typewriter text and choices above it on the right.</summary>
public partial class DialogueWindow : Component
{
	public static bool IsVisible { get; private set; }

	[ParameterSlider(Min = 5.0f, Max = 100.0f, Group = "Dialogue")]
	public float typingCharactersPerSecond = 24.0f;
	[ParameterSlider(Min = 1.0f, Max = 5.0f, Group = "Dialogue")]
	public float lineHoldSeconds = 1.0f;
	[ParameterSlider(Min = 1.0f, Max = 10.0f, Group = "Dialogue")]
	public float automaticLineHoldSeconds = 3.0f;

	private Gui gui;
	private WidgetSprite panel, terminalHintPanel;
	private WidgetLabel speaker, text, status, terminalHint;
	private readonly List<WidgetSprite> choiceBackgrounds = new List<WidgetSprite>();
	private readonly List<WidgetLabel> choiceLabels = new List<WidgetLabel>();
	private readonly List<int> choiceHeights = new List<int>();
	private PetCarrierScenarioController scenario;
	private PassengerScenarioController secondScenario;
	private bool subscribed, mouseStateSaved, choicesBuilt;
	private bool secondSubscribed, showingSecondScenario;
	private string fullText = "";
	private float visibleCharacterCount;
	private float lineHoldElapsed;
	private int visibleCharacters;
	private bool waitingAfterTyping;
	private bool hiddenByMenu;
	private bool terminalHintRequested;
	private bool panelWasHiddenByState, speakerWasHiddenByState, textWasHiddenByState, statusWasHiddenByState;
	private bool terminalHintPanelWasHiddenByState, terminalHintWasHiddenByState;
	private Input.MOUSE_HANDLE previousMouseHandle;
	private bool previousMouseGrab, previousCursorHide;
	private const float ReferenceWidth = 1600.0f;
	private const float ReferenceHeight = 900.0f;
	private int lastGuiWidth = -1, lastGuiHeight = -1;

	private void Init()
	{
		gui = Gui.GetCurrent();
		panel = new WidgetSprite(gui, "")
		{
			Color = new vec4(0.015f, 0.018f, 0.025f, 0.82f),
			Hidden = true
		};
		gui.AddChild(panel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		speaker = new WidgetLabel(gui, "")
		{
			FontSize = 19,
			FontColor = new vec4(1.0f, 1.0f, 1.0f, 1.0f),
			Hidden = true
		};
		gui.AddChild(speaker, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		text = new WidgetLabel(gui, "")
		{
			FontSize = 21,
			FontColor = new vec4(0.96f, 0.96f, 0.96f, 1.0f),
			FontWrap = 1,
			Hidden = true
		};
		gui.AddChild(text, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		status = new WidgetLabel(gui, "")
		{
			FontSize = 15,
			FontColor = new vec4(0.72f, 0.74f, 0.78f, 1.0f),
			Hidden = true
		};
		gui.AddChild(status, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		terminalHintPanel = new WidgetSprite(gui, "")
		{
			Color = new vec4(0.015f, 0.018f, 0.025f, 0.82f),
			Hidden = true
		};
		gui.AddChild(terminalHintPanel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		terminalHint = new WidgetLabel(gui, "Нажмите Q, чтобы открыть монитор проводника")
		{
			FontSize = 17,
			FontColor = new vec4(1.0f, 1.0f, 1.0f, 1.0f),
			FontWrap = 1,
			TextAlign = Gui.ALIGN_CENTER,
			Hidden = true
		};
		gui.AddChild(terminalHint, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
	}

	private void Update()
	{
		SetHiddenByMenu(!MenuManager.IsGameplayScreen);
		if (hiddenByMenu)
			return;
		ResizeInterface();

		if (scenario == null)
		{
			scenario = ComponentSystem.FindComponentInWorld<PetCarrierScenarioController>();
			if (scenario != null)
			{
				scenario.Changed += Refresh;
				scenario.TimerChanged += OnTimerChanged;
				subscribed = true;
				if (scenario.IsStarted) Refresh();
			}
		}
		if (secondScenario == null || (PassengerScenarioController.Active != null && secondScenario != PassengerScenarioController.Active))
		{
			if (secondScenario != null && secondSubscribed)
			{
				secondScenario.Changed -= RefreshSecondScenario;
				secondScenario.TimerChanged -= OnSecondScenarioTimerChanged;
				secondSubscribed = false;
			}
			secondScenario = PassengerScenarioController.Active ?? ComponentSystem.FindComponentInWorld<PassengerScenarioController>();
			if (secondScenario != null && secondScenario == PassengerScenarioController.Active)
			{
				secondScenario.Changed += RefreshSecondScenario;
				secondScenario.TimerChanged += OnSecondScenarioTimerChanged;
				secondSubscribed = true;
				if (secondScenario.IsStarted) RefreshSecondScenario();
			}
		}
		if (scenario != null && (scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingInitialPassengerInteraction
			|| scenario.CurrentStage == PetCarrierScenarioController.Stage.WaitingForTerminal
			|| scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingPassengerInteraction
			|| scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingCarrierPickup
			|| scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingPassengerHandoff))
			LayoutTerminalHint();
		if (secondScenario != null && secondScenario.IsStarted &&
			(secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.WaitingForPassenger
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingDisplayChecks
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingParentInteraction
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingActivityPickup
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingActivityHandoff
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingSeatCheck
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.SeatCheckCompleted))
			LayoutTerminalHint();

		ApplyTerminalHintVisibility();
		if (!IsVisible)
			return;

		if (visibleCharacters < fullText.Length)
		{
			visibleCharacterCount += MathLib.Max(0.0f, typingCharactersPerSecond) * Game.IFps;
			int nextVisibleCharacters = MathLib.Min(fullText.Length, (int)visibleCharacterCount);
			if (nextVisibleCharacters != visibleCharacters)
			{
				visibleCharacters = nextVisibleCharacters;
				text.Text = fullText.Substring(0, visibleCharacters);
			}
		}

		if (visibleCharacters >= fullText.Length)
		{
			if (!waitingAfterTyping)
				waitingAfterTyping = true;
			lineHoldElapsed += Game.IFps;
			bool hasChoices = showingSecondScenario
				? secondScenario != null && secondScenario.HasChoices
				: scenario != null && scenario.HasChoices;
			float requiredHoldSeconds = !hasChoices
				? automaticLineHoldSeconds
				: lineHoldSeconds;
			if (lineHoldElapsed >= MathLib.Max(1.0f, requiredHoldSeconds))
			{
				waitingAfterTyping = false;
				OnCurrentTextFinished();
			}
		}
	}

	private void Refresh()
	{
		if (scenario == null) return;
		if (secondScenario != null && secondScenario.IsStarted) return;
		showingSecondScenario = false;
		bool keepFinalLine = scenario.CurrentStage == PetCarrierScenarioController.Stage.Finished
			&& speaker != null
			&& speaker.Text == scenario.CurrentSpeaker
			&& fullText == (scenario.CurrentText ?? "");

		if (scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingInitialPassengerInteraction
			|| scenario.CurrentStage == PetCarrierScenarioController.Stage.WaitingForTerminal
			|| scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingPassengerInteraction
			|| scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingCarrierPickup
			|| scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingPassengerHandoff)
		{
			panel.Hidden = true;
			speaker.Hidden = true;
			text.Hidden = true;
			status.Hidden = true;
			SetTerminalHintVisible(true);
			if (scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingInitialPassengerInteraction
				|| scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingPassengerInteraction)
				terminalHint.Text = "Подойдите к пассажиру и нажмите E";
			else if (scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingCarrierPickup)
				terminalHint.Text = "Подойдите к переноске и нажмите E";
			else if (scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingPassengerHandoff)
				terminalHint.Text = "Вернитесь к пассажиру и нажмите E";
			else
				terminalHint.Text = "Нажмите Q, чтобы открыть монитор проводника";
			ClearButtons();
			IsVisible = false;
			RestoreMouse();
			LayoutTerminalHint();
			return;
		}

		SetTerminalHintVisible(false);

		if (!mouseStateSaved)
		{
			previousMouseHandle = Input.MouseHandle;
			previousMouseGrab = Input.MouseGrab;
			previousCursorHide = Input.MouseCursorHide;
			mouseStateSaved = true;
		}

		if (scenario.CurrentStage == PetCarrierScenarioController.Stage.Finished)
			RestoreMouse();
		else
		{
			Input.MouseHandle = Input.MOUSE_HANDLE.USER;
			Input.MouseGrab = false;
			Input.MouseCursorHide = false;
			Input.MouseCursorNeedUpdate = true;
		}

		panel.Hidden = false;
		speaker.Hidden = false;
		text.Hidden = false;
		status.Hidden = false;
		IsVisible = true;
		if (keepFinalLine)
		{
			choicesBuilt = false;
			ClearButtons();
			UpdateStatus();
			Layout();
			return;
		}

		speaker.Text = scenario.CurrentSpeaker;
		fullText = scenario.CurrentText ?? "";
		visibleCharacterCount = 0.0f;
		lineHoldElapsed = 0.0f;
		visibleCharacters = 0;
		waitingAfterTyping = false;
		text.Text = "";
		choicesBuilt = false;
		ClearButtons();
		UpdateStatus();
		Layout();
		if (fullText.Length == 0)
			OnCurrentTextFinished();
	}

	private void RefreshSecondScenario()
	{
		if (secondScenario == null || !secondScenario.IsStarted) return;
		showingSecondScenario = true;
		bool keepCurrentLine = IsVisible
			&& speaker.Text == secondScenario.CurrentSpeaker
			&& fullText == (secondScenario.CurrentText ?? "");

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.FreeRoam
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.WaitingForScenarioTwo)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(false);
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.WaitingForPassenger)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(true);
			terminalHint.Text = secondScenario.PassengerApproachTimedOut
				? "Время поиска истекло. Найдите вызвавшего пассажира"
				: string.Format("Вызов с пассажирского места. Найдите пассажира: {0:0} сек.", secondScenario.RemainingSeconds);
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingDisplayChecks)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(true);
			terminalHint.Text = "Нажмите Q, чтобы открыть монитор проводника";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingParentInteraction)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(true);
			terminalHint.Text = "Подойдите к пассажиру-родителю и нажмите E";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingActivityPickup)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(true);
			terminalHint.Text = "Подойдите к книге или настольной игре и нажмите E";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingActivityHandoff)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(true);
			terminalHint.Text = "Вернитесь к пассажиру-родителю и нажмите E";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingSeatCheck)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(true);
			terminalHint.Text = "Проверьте схему мест через Q или вернитесь к первому пассажиру и нажмите E";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.SeatCheckCompleted)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(true);
			terminalHint.Text = "Вернитесь к первому пассажиру и нажмите E. Схему мест можно проверить повторно через Q";
			LayoutTerminalHint();
			return;
		}

		if (!secondScenario.DialogueActive)
		{
			HideDialogueWidgets();
			SetTerminalHintVisible(false);
			return;
		}

		SetTerminalHintVisible(false);
		if (!mouseStateSaved)
		{
			previousMouseHandle = Input.MouseHandle;
			previousMouseGrab = Input.MouseGrab;
			previousCursorHide = Input.MouseCursorHide;
			mouseStateSaved = true;
		}
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;
		Input.MouseGrab = false;
		Input.MouseCursorHide = false;
		Input.MouseCursorNeedUpdate = true;

		panel.Hidden = speaker.Hidden = text.Hidden = status.Hidden = false;
		IsVisible = true;
		if (keepCurrentLine)
		{
			visibleCharacterCount = fullText.Length;
			visibleCharacters = fullText.Length;
			text.Text = fullText;
			lineHoldElapsed = 0.0f;
			waitingAfterTyping = true;
			choicesBuilt = false;
			ClearButtons();
			UpdateStatus();
			BuildChoices();
			Layout();
			return;
		}
		speaker.Text = secondScenario.CurrentSpeaker;
		fullText = secondScenario.CurrentText ?? "";
		visibleCharacterCount = 0.0f;
		lineHoldElapsed = 0.0f;
		visibleCharacters = 0;
		waitingAfterTyping = false;
		text.Text = "";
		choicesBuilt = false;
		ClearButtons();
		UpdateStatus();
		Layout();
		if (fullText.Length == 0)
			OnCurrentTextFinished();
	}

	private void HideDialogueWidgets()
	{
		panel.Hidden = true;
		speaker.Hidden = true;
		text.Hidden = true;
		status.Hidden = true;
		ClearButtons();
		IsVisible = false;
		RestoreMouse();
	}

	private void SetHiddenByMenu(bool hidden)
	{
		if (hiddenByMenu == hidden)
			return;

		hiddenByMenu = hidden;
		if (hidden)
		{
			panelWasHiddenByState = panel == null || panel.Hidden;
			speakerWasHiddenByState = speaker == null || speaker.Hidden;
			textWasHiddenByState = text == null || text.Hidden;
			statusWasHiddenByState = status == null || status.Hidden;
			terminalHintPanelWasHiddenByState = terminalHintPanel == null || terminalHintPanel.Hidden;
			terminalHintWasHiddenByState = terminalHint == null || terminalHint.Hidden;

			if (panel != null) panel.Hidden = true;
			if (speaker != null) speaker.Hidden = true;
			if (text != null) text.Hidden = true;
			if (status != null) status.Hidden = true;
			if (terminalHintPanel != null) terminalHintPanel.Hidden = true;
			if (terminalHint != null) terminalHint.Hidden = true;
			foreach (WidgetSprite background in choiceBackgrounds)
				background.Hidden = true;
			foreach (WidgetLabel label in choiceLabels)
				label.Hidden = true;
			return;
		}

		if (panel != null) panel.Hidden = panelWasHiddenByState;
		if (speaker != null) speaker.Hidden = speakerWasHiddenByState;
		if (text != null) text.Hidden = textWasHiddenByState;
		if (status != null) status.Hidden = statusWasHiddenByState;
		if (terminalHintPanel != null) terminalHintPanel.Hidden = terminalHintPanelWasHiddenByState;
		if (terminalHint != null) terminalHint.Hidden = terminalHintWasHiddenByState;
		foreach (WidgetSprite background in choiceBackgrounds)
			background.Hidden = false;
		foreach (WidgetLabel label in choiceLabels)
			label.Hidden = false;
	}

	private void BuildChoices()
	{
		bool hasChoices = showingSecondScenario
			? secondScenario != null && secondScenario.HasChoices
			: scenario != null && scenario.HasChoices;
		if (choicesBuilt || !hasChoices)
			return;

		choicesBuilt = true;
		string[] ids = showingSecondScenario ? secondScenario.GetChoiceIds() : scenario.GetChoiceIds();
		for (int i = 0; i < ids.Length; i++)
		{
			string choiceId = ids[i];
			string label = showingSecondScenario ? secondScenario.GetChoiceLabel(choiceId) : scenario.GetChoiceLabel(choiceId);
			WidgetSprite choiceBackground = new WidgetSprite(gui, "")
			{
				Color = new vec4(0.015f, 0.018f, 0.025f, 0.78f)
			};
			choiceBackground.EventClicked.Connect(() =>
			{
				if (showingSecondScenario) secondScenario?.Choose(choiceId);
				else scenario?.Choose(choiceId);
			});
			gui.AddChild(choiceBackground, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
			choiceBackgrounds.Add(choiceBackground);

			WidgetLabel choiceLabel = new WidgetLabel(gui, label)
			{
				FontSize = 16,
				FontColor = new vec4(0.96f, 0.96f, 0.96f, 1.0f),
				FontWrap = 1
			};
			choiceLabel.EventClicked.Connect(() =>
			{
				if (showingSecondScenario) secondScenario?.Choose(choiceId);
				else scenario?.Choose(choiceId);
			});
			gui.AddChild(choiceLabel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
			choiceLabels.Add(choiceLabel);
			choiceHeights.Add(MathLib.Max(76, MathLib.Min(142, 28 + ((label.Length + 45) / 46) * 19)));
		}
		Layout();
	}

	private void OnCurrentTextFinished()
	{
		if (showingSecondScenario && secondScenario == null) return;
		if (!showingSecondScenario && scenario == null) return;
		BuildChoices();
		if (showingSecondScenario) secondScenario.NotifyTextFinished();
		else scenario.NotifyTextFinished();
	}

	private void OnSecondScenarioTimerChanged(float seconds)
	{
		if (secondScenario == null || !secondScenario.IsStarted) return;
		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.WaitingForPassenger)
		{
			terminalHint.Text = string.Format("Вызов с пассажирского места. Найдите пассажира: {0:0} сек.", seconds);
			LayoutTerminalHint();
		}
		else if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.ParentChoice
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.ParentChoiceGrace
			|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.SeatDecisionChoice)
		{
			UpdateStatus();
		}
	}

	private void OnTimerChanged(float seconds)
	{
		if (status == null || scenario == null) return;
		status.Text = string.Format("K: {0}   Безопасность: {1}   Лояльность: {2}   Время: {3:0} сек.",
			scenario.Mood, scenario.Safety, scenario.Loyalty, seconds);
		if (!scenario.HasChoices && scenario.CurrentStage == PetCarrierScenarioController.Stage.Solution)
			status.Text += "   Q — терминал проводника";
	}

	private void UpdateStatus()
	{
		if (showingSecondScenario && secondScenario != null)
		{
			status.Text = string.Format("K1: {0}   K2: {1}   Безопасность: {2:+#;-#;0}   Лояльность: {3:+#;-#;0}",
				secondScenario.MoodPassenger1, secondScenario.MoodPassenger2,
				secondScenario.SafetyDelta, secondScenario.LoyaltyDelta);
			if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.ParentChoice
				|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.ParentChoiceGrace
				|| secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.SeatDecisionChoice)
				status.Text += string.Format("   Время: {0:0} сек.", secondScenario.RemainingSeconds);
			return;
		}
		if (scenario.CurrentStage == PetCarrierScenarioController.Stage.Finished)
			status.Text = string.Format("Безопасность: {0}   Лояльность: {1}", scenario.Safety, scenario.Loyalty);
		else if (scenario.RemainingSeconds > 0.0f)
			status.Text = string.Format("K: {0}   Безопасность: {1}   Лояльность: {2}   Время: {3:0} сек.",
				scenario.Mood, scenario.Safety, scenario.Loyalty, scenario.RemainingSeconds);
		else
			status.Text = string.Format("K: {0}   Безопасность: {1}   Лояльность: {2}",
				scenario.Mood, scenario.Safety, scenario.Loyalty);

		if (!scenario.HasChoices && scenario.CurrentStage == PetCarrierScenarioController.Stage.Solution)
			status.Text += "   Q — терминал проводника";
	}

	private void ResizeInterface()
	{
		if (gui == null || gui.Width <= 0 || gui.Height <= 0)
			return;
		if (gui.Width == lastGuiWidth && gui.Height == lastGuiHeight)
			return;

		lastGuiWidth = gui.Width;
		lastGuiHeight = gui.Height;
		Layout();
		LayoutTerminalHint();
	}

	private float GetInterfaceScale()
	{
		return System.Math.Min(System.Math.Max(1, gui.Width) / ReferenceWidth,
			System.Math.Max(1, gui.Height) / ReferenceHeight);
	}

	private static int ScaleSize(int size, float scale)
	{
		return System.Math.Max(1, (int)System.Math.Round(size * scale));
	}

	private void Layout()
	{
		int width = System.Math.Max(1, gui.Width);
		int height = System.Math.Max(1, gui.Height);
		float scale = GetInterfaceScale();
		int horizontalMargin = ScaleSize(40, scale);
		int panelHeight = ScaleSize(180, scale);
		int panelTop = System.Math.Max(0, height - panelHeight - ScaleSize(18, scale));
		int panelWidth = System.Math.Max(1, width - horizontalMargin * 2);
		int textPadding = ScaleSize(28, scale);

		speaker.FontSize = ScaleSize(19, scale);
		text.FontSize = ScaleSize(21, scale);
		status.FontSize = ScaleSize(15, scale);

		panel.SetPosition(horizontalMargin, panelTop);
		panel.Width = panelWidth;
		panel.Height = panelHeight;

		speaker.SetPosition(horizontalMargin + textPadding, panelTop + ScaleSize(15, scale));
		text.SetPosition(horizontalMargin + textPadding, panelTop + ScaleSize(48, scale));
		text.Width = System.Math.Max(1, panelWidth - textPadding * 2);
		status.SetPosition(horizontalMargin + textPadding, panelTop + panelHeight - ScaleSize(30, scale));
		status.Width = text.Width;

		int buttonWidth = System.Math.Min(ScaleSize(420, scale), panelWidth);
		int buttonGap = ScaleSize(9, scale);
		int right = width - horizontalMargin - buttonWidth;
		int buttonsBottom = panelTop - ScaleSize(14, scale);
		int totalChoicesHeight = buttonGap * System.Math.Max(0, choiceBackgrounds.Count - 1);
		foreach (int choiceHeight in choiceHeights)
			totalChoicesHeight += ScaleSize(choiceHeight, scale);
		int choiceY = System.Math.Max(ScaleSize(18, scale), buttonsBottom - totalChoicesHeight);
		int choicePaddingX = ScaleSize(14, scale);
		int choicePaddingY = ScaleSize(8, scale);

		for (int i = 0; i < choiceBackgrounds.Count; i++)
		{
			int choiceHeight = ScaleSize(choiceHeights[i], scale);
			choiceLabels[i].FontSize = ScaleSize(16, scale);
			choiceBackgrounds[i].SetPosition(right, choiceY);
			choiceBackgrounds[i].Width = buttonWidth;
			choiceBackgrounds[i].Height = choiceHeight;
			choiceLabels[i].SetPosition(right + choicePaddingX, choiceY + choicePaddingY);
			choiceLabels[i].Width = System.Math.Max(1, buttonWidth - choicePaddingX * 2);
			choiceLabels[i].Height = System.Math.Max(1, choiceHeight - choicePaddingY * 2);
			choiceY += choiceHeight + buttonGap;
		}
	}

	private void SetTerminalHintVisible(bool visible)
	{
		terminalHintRequested = visible;
		ApplyTerminalHintVisibility();
	}

	private void ApplyTerminalHintVisibility()
	{
		bool hidden = !terminalHintRequested || hiddenByMenu || ConductorTerminal.IsVisible;
		if (terminalHintPanel != null) terminalHintPanel.Hidden = hidden;
		if (terminalHint != null) terminalHint.Hidden = hidden;
	}

	private void LayoutTerminalHint()
	{
		if (terminalHintPanel == null || gui == null) return;
		float scale = GetInterfaceScale();
		int margin = ScaleSize(24, scale);
		int paddingX = ScaleSize(12, scale);
		int paddingY = ScaleSize(10, scale);
		terminalHint.FontSize = ScaleSize(17, scale);
		// Measure the unwrapped text, then let the label calculate any required wrapping.
		terminalHint.FontWrap = 0;
		ivec2 textSize = terminalHint.GetTextUnitSize(terminalHint.Text);
		int maxTextWidth = System.Math.Max(1,
			System.Math.Min(ScaleSize(680, scale), gui.Width - margin * 2) - paddingX * 2);
		terminalHint.Width = System.Math.Max(1, System.Math.Min(textSize.x, maxTextWidth));
		terminalHint.Height = 0;
		terminalHint.FontWrap = 1;
		terminalHint.Arrange();
		int width = terminalHint.Width + paddingX * 2;
		int height = System.Math.Max(textSize.y, terminalHint.Height) + paddingY * 2;
		int left = System.Math.Max(0, gui.Width - width - margin);
		terminalHintPanel.SetPosition(left, margin);
		terminalHintPanel.Width = width;
		terminalHintPanel.Height = height;
		terminalHint.SetPosition(left + paddingX, margin + paddingY);
	}

	private void ClearButtons()
	{
		foreach (WidgetSprite background in choiceBackgrounds)
			background.DeleteLater();
		foreach (WidgetLabel label in choiceLabels)
			label.DeleteLater();
		choiceBackgrounds.Clear();
		choiceLabels.Clear();
		choiceHeights.Clear();
	}

	public void HideForResults()
	{
		if (panel != null) panel.Hidden = true;
		if (speaker != null) speaker.Hidden = true;
		if (text != null) text.Hidden = true;
		if (status != null) status.Hidden = true;
		SetTerminalHintVisible(false);
		ClearButtons();
		IsVisible = false;
		RestoreMouse();
	}

	private void Shutdown()
	{
		IsVisible = false;
		if (scenario != null && subscribed)
		{
			scenario.Changed -= Refresh;
			scenario.TimerChanged -= OnTimerChanged;
		}
		if (secondScenario != null && secondSubscribed)
		{
			secondScenario.Changed -= RefreshSecondScenario;
			secondScenario.TimerChanged -= OnSecondScenarioTimerChanged;
		}
		ClearButtons();
		if (mouseStateSaved) RestoreMouse();
		panel?.DeleteLater();
		speaker?.DeleteLater();
		text?.DeleteLater();
		status?.DeleteLater();
		terminalHintPanel?.DeleteLater();
		terminalHint?.DeleteLater();
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
}
