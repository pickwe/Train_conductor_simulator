using System.Collections.Generic;
using Unigine;

/// <summary>In-game conductor terminal opened with Q; carrier stock is set by the scenario once.</summary>
public partial class ConductorTerminal : Component
{
	public static bool IsVisible { get; private set; }

	private Gui gui;
	private WidgetSprite panel;
	private WidgetLabel heading, body;
	private readonly List<WidgetButton> buttons = new List<WidgetButton>();
	private PetCarrierScenarioController scenario;
	private bool visible;
	private Input.MOUSE_HANDLE previousMouseHandle;
	private bool previousMouseGrab, previousCursorHide;
	private float previousGameScale, previousPhysicsScale;

	private void Init()
	{
		gui = Gui.GetCurrent();
		panel = new WidgetSprite(gui, "") { Color = new vec4(0.035f, 0.055f, 0.075f, 0.97f), Hidden = true };
		gui.AddChild(panel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		heading = new WidgetLabel(gui, "ТЕРМИНАЛ ПРОВОДНИКА") { FontSize = 26, FontColor = vec4.WHITE, Hidden = true };
		gui.AddChild(heading, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		body = new WidgetLabel(gui, "") { FontSize = 19, FontColor = vec4.WHITE, FontWrap = 1, Hidden = true };
		gui.AddChild(body, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
	}

	private void Update()
	{
		if (scenario == null) scenario = ComponentSystem.FindComponentInWorld<PetCarrierScenarioController>();
		if (scenario != null && Input.IsKeyDown(Input.KEY.Q))
		{
			if (visible) Close(); else if (scenario.CanOpenTerminal) Open();
		}
		if (visible)
		{
			KeepGamePaused();
			ReleaseCursor();
			Layout();
		}
	}

	private void Open()
	{
		visible = true;
		IsVisible = true;
		scenario.SetTerminalOpen(true);
		previousGameScale = Game.Scale > 0.0f ? Game.Scale : 1.0f;
		previousPhysicsScale = Physics.Scale > 0.0f ? Physics.Scale : 1.0f;
		previousMouseHandle = Input.MouseHandle;
		previousMouseGrab = Input.MouseGrab;
		previousCursorHide = Input.MouseCursorHide;
		KeepGamePaused();
		ReleaseCursor();
		ShowHome();
	}

	private void KeepGamePaused()
	{
		Game.Scale = 0.0f;
		Physics.Scale = 0.0f;
	}

	private void ReleaseCursor()
	{
		Input.MouseHandle = Input.MOUSE_HANDLE.USER;
		Input.MouseGrab = false;
		Input.MouseCursorHide = false;
		Input.MouseCursorNeedUpdate = true;
	}

	private void ShowHome()
	{
		heading.Text = "ТЕРМИНАЛ ПРОВОДНИКА";
		body.Text = "Рейс 018 • Москва — Питер\nСледующая: Тверь\n\nВыберите раздел.";
		ShowWidgets();
		ClearButtons();
		AddButton("Маршрут и расписание", () => ShowInfo("Маршрут и расписание", "Рейс 018 • Москва — Питер\nСледующая станция: Тверь."));
		AddButton("Товары и услуги", ShowGoods);
		AddButton("Схема мест", () => ShowInfo("Схема мест", "Схема мест недоступна в этом сценарии."));
		AddButton("Оплата", () => ShowInfo("Оплата", "Информация об оплате."));
		AddButton("Связь с начальником", () => ShowInfo("Связь с начальником", "Связь будет доступна при вызове начальника поезда."));
		Layout();
	}

	private void ShowGoods()
	{
		bool available = scenario.RevealCarrierAvailability();
		heading.Text = "ТОВАРЫ И УСЛУГИ";
		body.Text = available
			? "Переноска для животных есть в продаже.\nМожно предложить пассажиру оформить покупку."
			: "Переносок для животных в продаже нет.";
		ShowWidgets();
		ClearButtons();
		AddButton("Назад", ShowHome);
		AddButton("Закрыть терминал (Q)", Close);
		Layout();
	}

	private void ShowInfo(string title, string message)
	{
		heading.Text = title.ToUpperInvariant();
		body.Text = message;
		ShowWidgets();
		ClearButtons();
		AddButton("Назад", ShowHome);
		AddButton("Закрыть терминал (Q)", Close);
		Layout();
	}

	private void ShowWidgets()
	{
		panel.Hidden = heading.Hidden = body.Hidden = false;
	}

	private void AddButton(string label, System.Action action)
	{
		WidgetButton button = new WidgetButton(gui, label) { FontSize = 18 };
		button.EventClicked.Connect(() => action.Invoke());
		gui.AddChild(button, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		buttons.Add(button);
	}

	private void Layout()
	{
		int width = MathLib.Min(760, gui.Width - 48);
		int height = MathLib.Min(560, gui.Height - 48);
		int left = (gui.Width - width) / 2;
		int top = (gui.Height - height) / 2;
		panel.SetPosition(left, top); panel.Width = width; panel.Height = height;
		heading.SetPosition(left + 28, top + 24); heading.Width = width - 56;
		body.SetPosition(left + 28, top + 78); body.Width = width - 56;
		for (int i = 0; i < buttons.Count; i++)
		{
			buttons[i].SetPosition(left + 28, top + 170 + i * 56);
			buttons[i].Width = width - 56;
			buttons[i].Height = 48;
		}
	}

	private void ClearButtons()
	{
		foreach (WidgetButton button in buttons) button.DeleteLater();
		buttons.Clear();
	}

	private void Close()
	{
		if (!visible) return;
		visible = false;
		IsVisible = false;
		panel.Hidden = heading.Hidden = body.Hidden = true;
		ClearButtons();
		Game.Scale = previousGameScale > 0.0f ? previousGameScale : 1.0f;
		Physics.Scale = previousPhysicsScale > 0.0f ? previousPhysicsScale : 1.0f;
		Input.MouseHandle = previousMouseHandle;
		Input.MouseGrab = previousMouseGrab;
		Input.MouseCursorHide = previousCursorHide;
		Input.MouseCursorNeedUpdate = true;
		scenario?.ShowTerminalClosed();
	}

	private void Shutdown()
	{
		if (visible)
		{
			IsVisible = false;
			Game.Scale = previousGameScale > 0.0f ? previousGameScale : 1.0f;
			Physics.Scale = previousPhysicsScale > 0.0f ? previousPhysicsScale : 1.0f;
			Input.MouseHandle = previousMouseHandle;
			Input.MouseGrab = previousMouseGrab;
			Input.MouseCursorHide = previousCursorHide;
			Input.MouseCursorNeedUpdate = true;
		}
		ClearButtons();
		panel?.DeleteLater(); heading?.DeleteLater(); body?.DeleteLater();
	}
}
