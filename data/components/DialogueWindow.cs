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
	private bool panelWasHiddenByState, speakerWasHiddenByState, textWasHiddenByState, statusWasHiddenByState;
	private bool terminalHintPanelWasHiddenByState, terminalHintWasHiddenByState;
	private Input.MOUSE_HANDLE previousMouseHandle;
	private bool previousMouseGrab, previousCursorHide;

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
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
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

		terminalHintPanel.Hidden = true;
		terminalHint.Hidden = true;

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
			terminalHintPanel.Hidden = true;
			terminalHint.Hidden = true;
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.WaitingForPassenger)
		{
			HideDialogueWidgets();
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
			terminalHint.Text = secondScenario.PassengerApproachTimedOut
				? "Время поиска истекло. Найдите вызвавшего пассажира"
				: string.Format("Вызов с пассажирского места. Найдите пассажира: {0:0} сек.", secondScenario.RemainingSeconds);
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingDisplayChecks)
		{
			HideDialogueWidgets();
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
			terminalHint.Text = "Нажмите Q, чтобы открыть монитор проводника";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingParentInteraction)
		{
			HideDialogueWidgets();
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
			terminalHint.Text = "Подойдите к пассажиру-родителю и нажмите E";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingActivityPickup)
		{
			HideDialogueWidgets();
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
			terminalHint.Text = "Подойдите к книге или настольной игре и нажмите E";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingActivityHandoff)
		{
			HideDialogueWidgets();
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
			terminalHint.Text = "Вернитесь к пассажиру-родителю и нажмите E";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.AwaitingSeatCheck)
		{
			HideDialogueWidgets();
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
			terminalHint.Text = "Проверьте схему мест через Q или вернитесь к пассажиру-ябеде и нажмите E";
			LayoutTerminalHint();
			return;
		}

		if (secondScenario.CurrentStep == PassengerScenarioController.ScenarioStep.SeatCheckCompleted)
		{
			HideDialogueWidgets();
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
			terminalHint.Text = "Вернитесь к пассажиру-ябеде и нажмите E. Схему мест можно проверить повторно через Q";
			LayoutTerminalHint();
			return;
		}

		if (!secondScenario.DialogueActive)
		{
			HideDialogueWidgets();
			terminalHintPanel.Hidden = true;
			terminalHint.Hidden = true;
			return;
		}

		terminalHintPanel.Hidden = true;
		terminalHint.Hidden = true;
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

	private void Layout()
	{
		int width = gui.Width;
		int height = gui.Height;
		int horizontalMargin = MathLib.Max(18, width / 40);
		int panelHeight = MathLib.Max(138, height / 5);
		int panelTop = height - panelHeight - 18;
		int panelWidth = width - horizontalMargin * 2;

		panel.SetPosition(horizontalMargin, panelTop);
		panel.Width = panelWidth;
		panel.Height = panelHeight;

		speaker.SetPosition(horizontalMargin + 28, panelTop + 15);
		text.SetPosition(horizontalMargin + 28, panelTop + 48);
		text.Width = panelWidth - 56;
		status.SetPosition(horizontalMargin + 28, panelTop + panelHeight - 30);
		status.Width = panelWidth - 56;

		int buttonWidth = MathLib.Min(420, MathLib.Max(280, width / 3));
		int buttonGap = 9;
		int right = width - horizontalMargin - buttonWidth;
		int buttonsBottom = panelTop - 14;
		int totalChoicesHeight = buttonGap * MathLib.Max(0, choiceBackgrounds.Count - 1);
		foreach (int choiceHeight in choiceHeights)
			totalChoicesHeight += choiceHeight;
		int firstChoiceY = MathLib.Max(18, buttonsBottom - totalChoicesHeight);

		for (int i = 0; i < choiceBackgrounds.Count; i++)
		{
			int choiceY = firstChoiceY;
			for (int previous = 0; previous < i; previous++)
				choiceY += choiceHeights[previous] + buttonGap;
			choiceBackgrounds[i].SetPosition(right, choiceY);
			choiceBackgrounds[i].Width = buttonWidth;
			choiceBackgrounds[i].Height = choiceHeights[i];
			choiceLabels[i].SetPosition(right + 14, choiceY + 8);
			choiceLabels[i].Width = buttonWidth - 28;
			choiceLabels[i].Height = choiceHeights[i] - 16;
		}
	}

	private void LayoutTerminalHint()
	{
		if (terminalHintPanel == null || gui == null) return;
		int width = MathLib.Min(680, gui.Width - 48);
		int height = 72;
		int left = gui.Width - width - 24;
		terminalHintPanel.SetPosition(left, 24);
		terminalHintPanel.Width = width;
		terminalHintPanel.Height = height;
		terminalHint.SetPosition(left + 12, 34);
		terminalHint.Width = width - 24;
		terminalHint.Height = height - 20;
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
		if (terminalHintPanel != null) terminalHintPanel.Hidden = true;
		if (terminalHint != null) terminalHint.Hidden = true;
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
