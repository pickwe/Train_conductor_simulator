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

	private Gui gui;
	private WidgetSprite panel, terminalHintPanel;
	private WidgetLabel speaker, text, status, terminalHint;
	private readonly List<WidgetButton> buttons = new List<WidgetButton>();
	private readonly List<int> buttonHeights = new List<int>();
	private PetCarrierScenarioController scenario;
	private bool subscribed, mouseStateSaved, choicesBuilt;
	private string fullText = "";
	private float visibleCharacterCount;
	private float lineHoldElapsed;
	private int visibleCharacters;
	private bool waitingAfterTyping;
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
			FontSize = 18,
			FontColor = new vec4(1.0f, 1.0f, 1.0f, 1.0f),
			TextAlign = Gui.ALIGN_CENTER,
			Hidden = true
		};
		gui.AddChild(terminalHint, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
	}

	private void Update()
	{
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
		if (scenario != null && scenario.CurrentStage == PetCarrierScenarioController.Stage.WaitingForTerminal)
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
			if (lineHoldElapsed >= MathLib.Max(1.0f, lineHoldSeconds))
			{
				waitingAfterTyping = false;
				OnCurrentTextFinished();
			}
		}
	}

	private void Refresh()
	{
		if (scenario == null) return;
		bool keepFinalLine = scenario.CurrentStage == PetCarrierScenarioController.Stage.Finished
			&& speaker != null
			&& speaker.Text == scenario.CurrentSpeaker
			&& fullText == (scenario.CurrentText ?? "");

		if (scenario.CurrentStage == PetCarrierScenarioController.Stage.WaitingForTerminal)
		{
			panel.Hidden = true;
			speaker.Hidden = true;
			text.Hidden = true;
			status.Hidden = true;
			terminalHintPanel.Hidden = false;
			terminalHint.Hidden = false;
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

	private void BuildChoices()
	{
		if (choicesBuilt || scenario == null || !scenario.HasChoices)
			return;

		choicesBuilt = true;
		string[] ids = scenario.GetChoiceIds();
		for (int i = 0; i < ids.Length; i++)
		{
			string choiceId = ids[i];
			string label = scenario.GetChoiceLabel(choiceId);
			WidgetButton button = new WidgetButton(gui, label)
			{
				FontSize = 16,
				FontWrap = 1
			};
			button.EventClicked.Connect(() => scenario.Choose(choiceId));
			gui.AddChild(button, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
			buttons.Add(button);
			buttonHeights.Add(MathLib.Max(76, MathLib.Min(142, 28 + ((label.Length + 45) / 46) * 19)));
		}
		Layout();
	}

	private void OnCurrentTextFinished()
	{
		if (scenario == null) return;
		BuildChoices();
		scenario.NotifyTextFinished();
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
		int totalButtonsHeight = buttonGap * MathLib.Max(0, buttons.Count - 1);
		foreach (int buttonHeight in buttonHeights)
			totalButtonsHeight += buttonHeight;
		int firstButtonY = MathLib.Max(18, buttonsBottom - totalButtonsHeight);

		for (int i = 0; i < buttons.Count; i++)
		{
			int buttonY = firstButtonY;
			for (int previous = 0; previous < i; previous++)
				buttonY += buttonHeights[previous] + buttonGap;
			buttons[i].SetPosition(right, buttonY);
			buttons[i].Width = buttonWidth;
			buttons[i].Height = buttonHeights[i];
		}
	}

	private void LayoutTerminalHint()
	{
		if (terminalHintPanel == null || gui == null) return;
		int width = MathLib.Min(460, gui.Width - 48);
		int height = 54;
		int left = gui.Width - width - 24;
		terminalHintPanel.SetPosition(left, 24);
		terminalHintPanel.Width = width;
		terminalHintPanel.Height = height;
		terminalHint.SetPosition(left + 8, 38);
		terminalHint.Width = width - 16;
	}

	private void ClearButtons()
	{
		foreach (WidgetButton button in buttons)
			button.DeleteLater();
		buttons.Clear();
		buttonHeights.Clear();
	}

	private void Shutdown()
	{
		IsVisible = false;
		if (scenario != null && subscribed)
		{
			scenario.Changed -= Refresh;
			scenario.TimerChanged -= OnTimerChanged;
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
