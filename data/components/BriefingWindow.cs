using System;
using System.Diagnostics;
using Unigine;

/// <summary>Shows the route briefing when a shift world starts.</summary>
public partial class BriefingWindow : Component
{
	public string title = "Брифинг смены";
	public string briefingText = "Маршрут: 17\n\nПроверьте готовность вагона и следите за пассажирами.\nВо время смены соблюдайте расписание и правила безопасности.";
	[Parameter(Group = "Вступление сценария 1")]
	public string scenarioOneTitle = "Сценарий 1. Неисправность вагона";
	[Parameter(Group = "Вступление сценария 1")]
	public string scenarioOneText = "Во время рейса обратите внимание на состояние вагона и проверьте замеченные неисправности.";
	[Parameter(Group = "Вступление сценария 2")]
	public string scenarioTwoTitle = "Сценарий 2. Помощь пассажиру";
	[Parameter(Group = "Вступление сценария 2")]
	public string scenarioTwoText = "На маршруте возникнет ситуация, в которой важно выслушать пассажира и принять решение.";
	public float introductionFadeDuration = 0.65f;

	private Gui gui;
	private WidgetSprite dimBackground, panel;
	private WidgetLabel heading, body;
	private WidgetLabel introHeading, introBody, introPrompt;
	private WidgetButton closeButton;
	private PassengerScenarioController scenario;
	private GameFlowController gameFlow;
	private Input.MOUSE_HANDLE previousMouseHandle;
	private bool previousMouseGrab, previousCursorHide;
	private bool visible;
	private bool introductionVisible, waitingForKeyRelease, suppressPauseUntilEscRelease;
	private float introFade;
	private float savedGameScale, savedPhysicsScale;
	private readonly Stopwatch introductionClock = new Stopwatch();
	private Array keyboardKeys, mouseButtons;
	private bool subscribedToFlow;
	public static bool IsVisible { get; private set; }
	public static bool IsIntroductionVisible { get; private set; }
	public static bool SuppressPauseUntilEscRelease { get; private set; }

	private void Init()
	{
		gui = Gui.GetCurrent();
		keyboardKeys = Enum.GetValues(typeof(Input.KEY));
		mouseButtons = Enum.GetValues(typeof(Input.MOUSE_BUTTON));
		BuildWindow();
	}

	private void Update()
	{
		if (scenario == null)
			scenario = ComponentSystem.FindComponentInWorld<PassengerScenarioController>();
		if (gameFlow == null)
			gameFlow = ComponentSystem.FindComponentInWorld<GameFlowController>();
		if (gameFlow != null && !subscribedToFlow)
		{
			gameFlow.ScenarioOneIntroductionRequested += ShowScenarioOneIntroduction;
			gameFlow.ScenarioTwoIntroductionRequested += ShowScenarioTwoIntroduction;
			subscribedToFlow = true;
			if (gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioOneIntroduction)
				ShowScenarioOneIntroduction();
			else if (gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioTwoIntroduction)
				ShowScenarioTwoIntroduction();
		}

		if (!visible && MenuManager.BriefingRequestedForCurrentRun && scenario != null &&
			scenario.CurrentStep == PassengerScenarioController.ScenarioStep.WaitingForBriefing)
			Show();

		if (visible || introductionVisible)
			LayoutWindow();
		if (introductionVisible)
			UpdateIntroduction();
		if (suppressPauseUntilEscRelease && Input.IsKeyUp(Input.KEY.ESC))
		{
			suppressPauseUntilEscRelease = false;
			SuppressPauseUntilEscRelease = false;
		}
	}

	private void BuildWindow()
	{
		dimBackground = new WidgetSprite(gui, "");
		dimBackground.Color = new vec4(0.0f, 0.0f, 0.0f, 0.68f);
		dimBackground.IntersectionEnabled = false;
		gui.AddChild(dimBackground, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		panel = new WidgetSprite(gui, "");
		panel.Color = new vec4(0.10f, 0.11f, 0.14f, 1.0f);
		gui.AddChild(panel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		heading = new WidgetLabel(gui, title);
		heading.FontSize = 30;
		heading.FontColor = new vec4(1.0f, 1.0f, 1.0f, 1.0f);
		gui.AddChild(heading, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		body = new WidgetLabel(gui, briefingText);
		body.FontSize = 20;
		body.FontColor = new vec4(0.9f, 0.9f, 0.9f, 1.0f);
		gui.AddChild(body, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		closeButton = new WidgetButton(gui, "Закрыть  ×");
		closeButton.FontSize = 18;
		closeButton.EventClicked.Connect(Close);
		gui.AddChild(closeButton, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		introHeading = new WidgetLabel(gui, "");
		introHeading.FontSize = 38;
		introHeading.FontColor = new vec4(1.0f, 1.0f, 1.0f, 1.0f);
		introHeading.TextAlign = Gui.ALIGN_CENTER;
		gui.AddChild(introHeading, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		introBody = new WidgetLabel(gui, "");
		introBody.FontSize = 22;
		introBody.FontColor = new vec4(0.91f, 0.92f, 0.94f, 1.0f);
		introBody.FontWrap = 1;
		introBody.TextAlign = Gui.ALIGN_CENTER;
		gui.AddChild(introBody, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		introPrompt = new WidgetLabel(gui, "Нажмите любую клавишу или кнопку мыши, чтобы продолжить");
		introPrompt.FontSize = 18;
		introPrompt.FontColor = new vec4(0.80f, 0.82f, 0.86f, 1.0f);
		introPrompt.TextAlign = Gui.ALIGN_CENTER;
		gui.AddChild(introPrompt, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		SetWidgetsHidden(true);
		SetIntroductionWidgetsHidden(true);
	}

	private void Show()
	{
		previousMouseHandle = Input.MouseHandle;
		previousMouseGrab = Input.MouseGrab;
		previousCursorHide = Input.MouseCursorHide;
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;
		Input.MouseGrab = false;
		Input.MouseCursorHide = false;
		Input.MouseCursorNeedUpdate = true;
		SetWidgetsHidden(false);
		visible = true;
		IsVisible = true;
		LayoutWindow();
	}

	private void Close()
	{
		if (!visible)
			return;

		SetWidgetsHidden(true);
		visible = false;
		IsVisible = false;
		Input.MouseHandle = previousMouseHandle;
		Input.MouseGrab = previousMouseGrab;
		Input.MouseCursorHide = previousCursorHide;
		Input.MouseCursorNeedUpdate = true;
		if (gameFlow != null)
			gameFlow.NotifyBriefingClosed();
		else
			scenario?.NotifyBriefingClosed();
		MenuManager.CompleteRunBriefingRequest();
	}

	private void ShowScenarioOneIntroduction()
	{
		ShowIntroduction(scenarioOneTitle, scenarioOneText);
	}

	private void ShowScenarioTwoIntroduction()
	{
		ShowIntroduction(scenarioTwoTitle, scenarioTwoText);
	}

	private void ShowIntroduction(string introTitle, string introText)
	{
		if (introductionVisible) return;
		introHeading.Text = introTitle;
		introBody.Text = introText;
		introFade = 0.0f;
		introductionClock.Restart();
		waitingForKeyRelease = true;
		savedGameScale = Game.Scale > 0.0f ? Game.Scale : 1.0f;
		savedPhysicsScale = Physics.Scale > 0.0f ? Physics.Scale : 1.0f;
		Game.Scale = 0.0f;
		Physics.Scale = 0.0f;
		previousMouseHandle = Input.MouseHandle;
		previousMouseGrab = Input.MouseGrab;
		previousCursorHide = Input.MouseCursorHide;
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;
		Input.MouseGrab = false;
		Input.MouseCursorHide = false;
		Input.MouseCursorNeedUpdate = true;
		SetIntroductionWidgetsHidden(false);
		introductionVisible = true;
		IsVisible = true;
		IsIntroductionVisible = true;
		LayoutWindow();
	}

	private void UpdateIntroduction()
	{
		float duration = Math.Max(0.01f, introductionFadeDuration);
		introFade = Math.Min(1.0f, (float)introductionClock.Elapsed.TotalSeconds / duration);
		float alpha = introFade;
		dimBackground.Color = new vec4(0.0f, 0.0f, 0.0f, 0.78f * alpha);
		introHeading.FontColor = new vec4(1.0f, 1.0f, 1.0f, alpha);
		introBody.FontColor = new vec4(0.91f, 0.92f, 0.94f, alpha);
		introPrompt.FontColor = new vec4(0.80f, 0.82f, 0.86f, alpha);
		if (waitingForKeyRelease)
		{
			if (!AnyInputDown()) waitingForKeyRelease = false;
			return;
		}
		if (introFade >= 1.0f && AnyInputDown())
			CloseIntroduction();
	}

	private bool AnyInputDown()
	{
		foreach (object value in keyboardKeys)
		{
			Input.KEY key = (Input.KEY)value;
			if (key >= Input.KEY.ANY_SHIFT) continue;
			if (Input.IsKeyDown(key)) return true;
		}
		foreach (object value in mouseButtons)
		{
			Input.MOUSE_BUTTON button = (Input.MOUSE_BUTTON)value;
			if (button == Input.MOUSE_BUTTON.MOUSE_NUM_BUTTONS) continue;
			if (Input.IsMouseButtonDown(button)) return true;
		}
		return false;
	}

	private void CloseIntroduction()
	{
		if (!introductionVisible) return;
		bool escDown = Input.IsKeyDown(Input.KEY.ESC);
		SetIntroductionWidgetsHidden(true);
		introductionVisible = false;
		IsIntroductionVisible = false;
		IsVisible = visible;
		Game.Scale = savedGameScale;
		Physics.Scale = savedPhysicsScale;
		Input.MouseHandle = previousMouseHandle;
		Input.MouseGrab = previousMouseGrab;
		Input.MouseCursorHide = previousCursorHide;
		Input.MouseCursorNeedUpdate = true;
		if (escDown)
		{
			suppressPauseUntilEscRelease = true;
			SuppressPauseUntilEscRelease = true;
		}
		if (gameFlow != null)
		{
			if (gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioOneIntroduction)
				gameFlow.StartScenarioOne();
			else if (gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioTwoIntroduction)
				gameFlow.StartScenarioTwo();
		}
	}

	private void LayoutWindow()
	{
		int width = gui.Size.x;
		int height = gui.Size.y;
		int panelWidth = MathLib.Min(900, width - 48);
		int panelHeight = MathLib.Min(520, height - 48);
		int left = (width - panelWidth) / 2;
		int top = (height - panelHeight) / 2;

		dimBackground.SetPosition(0, 0);
		dimBackground.Width = width;
		dimBackground.Height = height;
		panel.SetPosition(left, top);
		panel.Width = panelWidth;
		panel.Height = panelHeight;
		heading.SetPosition(left + 36, top + 30);
		body.SetPosition(left + 36, top + 100);
		body.Width = panelWidth - 72;
		closeButton.SetPosition(left + panelWidth - 168, top + 24);
		introHeading.SetPosition(left + 36, top + 70);
		introHeading.Width = panelWidth - 72;
		introBody.SetPosition(left + 72, top + 175);
		introBody.Width = panelWidth - 144;
		introPrompt.SetPosition(left + 36, top + panelHeight - 72);
		introPrompt.Width = panelWidth - 72;
	}

	private void SetWidgetsHidden(bool hidden)
	{
		if (dimBackground != null) dimBackground.Hidden = hidden;
		if (panel != null) panel.Hidden = hidden;
		if (heading != null) heading.Hidden = hidden;
		if (body != null) body.Hidden = hidden;
		if (closeButton != null) closeButton.Hidden = hidden;
	}

	private void SetIntroductionWidgetsHidden(bool hidden)
	{
		if (dimBackground != null) dimBackground.Hidden = hidden;
		if (introHeading != null) introHeading.Hidden = hidden;
		if (introBody != null) introBody.Hidden = hidden;
		if (introPrompt != null) introPrompt.Hidden = hidden;
	}

	private void Shutdown()
	{
		if (visible)
		{
			IsVisible = false;
			Input.MouseHandle = previousMouseHandle;
			Input.MouseGrab = previousMouseGrab;
			Input.MouseCursorHide = previousCursorHide;
			Input.MouseCursorNeedUpdate = true;
		}
		if (introductionVisible)
		{
			Game.Scale = savedGameScale;
			Physics.Scale = savedPhysicsScale;
			IsIntroductionVisible = false;
			IsVisible = false;
		}
		if (gameFlow != null && subscribedToFlow)
		{
			gameFlow.ScenarioOneIntroductionRequested -= ShowScenarioOneIntroduction;
			gameFlow.ScenarioTwoIntroductionRequested -= ShowScenarioTwoIntroduction;
		}
		dimBackground?.DeleteLater();
		panel?.DeleteLater();
		heading?.DeleteLater();
		body?.DeleteLater();
		closeButton?.DeleteLater();
		introHeading?.DeleteLater();
		introBody?.DeleteLater();
		introPrompt?.DeleteLater();
	}
}
