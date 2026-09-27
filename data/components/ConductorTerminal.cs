using System.Collections.Generic;
using Unigine;

/// <summary>In-game conductor terminal opened with Q; carrier stock is set by the scenario once.</summary>
public partial class ConductorTerminal : Component
{
	[ParameterFile(Filter = ".png"), Parameter(Title = "Seats Available Sprite", Group = "Scenario 3 Seat Map")]
	public string seatsAvailableSpritePath = "";
	[ParameterFile(Filter = ".png"), Parameter(Title = "No Seats Sprite", Group = "Scenario 3 Seat Map")]
	public string noSeatsSpritePath = "";

	public static bool IsVisible { get; private set; }

	[ParameterFile(Filter = ".png"), Parameter(Group = "Terminal Maps")]
	public string routeMapSprite = "data/ui/terminl map.png";
	[ParameterFile(Filter = ".png"), Parameter(Group = "Terminal Maps")]
	public string seatMapSprite = "data/ui/terminl map.png";

	private enum Page { Home, Route, Goods, Seats, Connection }
	private class GoodsRow
	{
		public WidgetLabel Title, Description, Price;
	}

	private Gui gui;
	private WidgetSprite panel, pageMap, backButton;
	private WidgetLabel heading, body, exitHint;
	private readonly List<Widget> pageWidgets = new List<Widget>();
	private readonly List<WidgetSprite> homeButtons = new List<WidgetSprite>();
	private readonly List<GoodsRow> goodsRows = new List<GoodsRow>();
	private PetCarrierScenarioController scenario;
	private PassengerScenarioController secondScenario;
	private Page page = Page.Home;
	private bool visible;
	private bool openedForSecondScenario;
	private bool showingSeatMap;
	private Input.MOUSE_HANDLE previousMouseHandle;
	private bool previousMouseGrab, previousCursorHide;
	private float previousGameScale, previousPhysicsScale;

	private void Init()
	{
		gui = Gui.GetCurrent();
		panel = new WidgetSprite(gui, "data/ui/terminal_background.png") { Hidden = true };
		gui.AddChild(panel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		heading = new WidgetLabel(gui, "ТЕРМИНАЛ ПРОВОДНИКА") { FontSize = 26, FontColor = vec4.WHITE, Hidden = true };
		gui.AddChild(heading, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		body = new WidgetLabel(gui, "") { FontSize = 19, FontColor = vec4.WHITE, FontWrap = 1, Hidden = true };
		gui.AddChild(body, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		exitHint = new WidgetLabel(gui, "Чтобы выйти из терминала, нажмите Q")
		{
			FontSize = 16,
			FontColor = new vec4(0.78f, 0.86f, 0.96f, 1.0f),
			TextAlign = Gui.ALIGN_RIGHT,
			Hidden = true
		};
		gui.AddChild(exitHint, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
	}

	private void Update()
	{
		if (scenario == null) scenario = ComponentSystem.FindComponentInWorld<PetCarrierScenarioController>();
		if (secondScenario == null || (PassengerScenarioController.Active != null && secondScenario != PassengerScenarioController.Active))
			secondScenario = PassengerScenarioController.Active ?? ComponentSystem.FindComponentInWorld<PassengerScenarioController>();
		bool canOpenFirstScenario = scenario != null && scenario.CanOpenTerminal;
		bool canOpenSecondScenario = secondScenario != null && secondScenario.CanOpenTerminal;
		if (Input.IsKeyDown(Input.KEY.Q))
		{
			if (visible) Close();
			else if (canOpenFirstScenario || canOpenSecondScenario) Open(canOpenSecondScenario);
		}
		if (visible)
		{
			KeepGamePaused();
			ReleaseCursor();
			Layout();
		}
	}

	private void Open(bool forSecondScenario)
	{
		openedForSecondScenario = forSecondScenario;
		visible = true;
		IsVisible = true;
		if (openedForSecondScenario) secondScenario.SetTerminalOpen(true);
		else scenario.SetTerminalOpen(true);
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
		page = Page.Home;
		heading.Text = "ТЕРМИНАЛ ПРОВОДНИКА";
		body.Text = "Рейс 018 • Москва — Санкт-Петербург\nСледующая станция: Тверь\n\nВыберите раздел.";
		body.Hidden = false;
		ShowWidgets();
		ClearPageWidgets();
		AddHomeButton("data/ui/terminal_route.png", ShowRoute);
		AddHomeButton("data/ui/terminal_sell.png", ShowGoods);
		AddHomeButton("data/ui/terminal_map of place.png", ShowSeats);
		AddHomeButton("data/ui/terminal_connect.png", ShowConnection);
		Layout();
	}

	private void ShowRoute()
	{
		page = Page.Route;
		heading.Text = "Маршрут и расписание";
		body.Hidden = true;
		ShowWidgets();
		ClearPageWidgets();
		string texture = string.IsNullOrWhiteSpace(routeMapSprite) ? "data/ui/terminl map.png" : routeMapSprite;
		pageMap = new WidgetSprite(gui, texture);
		AddPageWidget(pageMap);
		AddBackButton();
		Layout();
	}

	private void ShowSeats()
	{
		string texture = string.IsNullOrWhiteSpace(seatMapSprite) ? "data/ui/terminl map.png" : seatMapSprite;
		if (openedForSecondScenario && secondScenario != null)
		{
			if (secondScenario.CurrentStep != PassengerScenarioController.ScenarioStep.AwaitingSeatCheck
				&& secondScenario.CurrentStep != PassengerScenarioController.ScenarioStep.SeatCheckCompleted)
			{
				ShowConnection();
				return;
			}
			bool available = secondScenario.RevealRelocationSeatAvailability();
			string selectedSpritePath = available ? seatsAvailableSpritePath : noSeatsSpritePath;
			texture = string.IsNullOrWhiteSpace(selectedSpritePath) ? "" : selectedSpritePath;
		}
		page = Page.Seats;
		heading.Text = "Схема мест";
		body.Hidden = true;
		ShowWidgets();
		ClearPageWidgets();
		pageMap = new WidgetSprite(gui, texture);
		if (openedForSecondScenario && string.IsNullOrWhiteSpace(texture))
			pageMap.Color = new vec4(0.75f, 0.02f, 0.02f, 1.0f);
		AddPageWidget(pageMap);
		AddBackButton();
		Layout();
	}

	private void ShowGoods()
	{
		page = Page.Goods;
		bool carrierAvailable = openedForSecondScenario
			? secondScenario != null && secondScenario.RevealOnboardActivitiesAvailability()
			: scenario != null && scenario.RevealCarrierAvailability();
		heading.Text = "Товары и услуги";
		body.Hidden = true;
		ShowWidgets();
		ClearPageWidgets();
		AddGoodsRow("Переноска для животных", carrierAvailable ? "Для перевозки питомца" : "Для перевозки питомца • нет в наличии", "1 490 ₽");
		AddGoodsRow("Чай / кофе", "Горячие напитки в пути", "150 ₽");
		AddGoodsRow("Набор путешественника", "Маска для сна, беруши, салфетки", "390 ₽");
		AddGoodsRow("Плед", "Дополнительный плед на время поездки", "300 ₽");
		AddGoodsRow("Детский набор", "Раскраска и дорожные мелочи", "450 ₽");
		AddBackButton();
		Layout();
	}

	private void ShowConnection()
	{
		page = Page.Connection;
		heading.Text = "Связь с начальником";
		body.Text = "Связь будет доступна при вызове начальника поезда.";
		body.Hidden = false;
		ShowWidgets();
		ClearPageWidgets();
		AddBackButton();
		Layout();
	}

	private void AddHomeButton(string texture, System.Action action)
	{
		WidgetSprite sprite = CreateButtonSprite(texture, action);
		homeButtons.Add(sprite);
		AddPageWidget(sprite);
	}

	private void AddBackButton()
	{
		backButton = CreateButtonSprite("data/ui/terminal back.png", ShowHome);
		AddPageWidget(backButton);
	}

	private WidgetSprite CreateButtonSprite(string texture, System.Action action)
	{
		WidgetSprite sprite = new WidgetSprite(gui, texture);
		sprite.SetLayerTexCoord(0, new vec4(0.0f, 0.0f, 1.0f, 0.5f));
		sprite.EventEnter.Connect((Widget widget) => sprite.SetLayerTexCoord(0, new vec4(0.0f, 0.5f, 1.0f, 1.0f)));
		sprite.EventLeave.Connect((Widget widget) => sprite.SetLayerTexCoord(0, new vec4(0.0f, 0.0f, 1.0f, 0.5f)));
		sprite.EventClicked.Connect(() => action.Invoke());
		gui.AddChild(sprite, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		return sprite;
	}

	private void AddGoodsRow(string title, string description, string price)
	{
		WidgetSprite background = new WidgetSprite(gui, "")
		{
			Color = new vec4(0.035f, 0.060f, 0.095f, 0.86f)
		};
		AddPageWidget(background);
		GoodsRow row = new GoodsRow
		{
			Title = new WidgetLabel(gui, title) { FontSize = 19, FontColor = vec4.WHITE },
			Description = new WidgetLabel(gui, description) { FontSize = 15, FontColor = new vec4(0.68f, 0.78f, 0.91f, 1.0f) },
			Price = new WidgetLabel(gui, price) { FontSize = 20, FontColor = vec4.WHITE, TextAlign = Gui.ALIGN_RIGHT }
		};
		AddPageWidget(row.Title);
		AddPageWidget(row.Description);
		AddPageWidget(row.Price);
		goodsRows.Add(row);
	}

	private void AddPageWidget(Widget widget)
	{
		gui.AddChild(widget, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		pageWidgets.Add(widget);
	}

	private void ShowWidgets()
	{
		panel.Hidden = heading.Hidden = false;
		exitHint.Hidden = false;
	}

	private void Layout()
	{
		int width = MathLib.Min(760, gui.Width - 48);
		int height = MathLib.Min(560, gui.Height - 48);
		int left = (gui.Width - width) / 2;
		int top = (gui.Height - height) / 2;
		panel.SetPosition(left, top); panel.Width = width; panel.Height = height;
		heading.SetPosition(left + 28, top + 24); heading.Width = width - 56;
		int hintWidth = MathLib.Min(380, gui.Width - 24);
		exitHint.SetPosition(gui.Width - hintWidth - 12, 12); exitHint.Width = hintWidth; exitHint.Height = 28;

		if (page == Page.Home)
		{
			body.SetPosition(left + 28, top + 78); body.Width = width - 56;
			int buttonHeight = MathLib.Min(78, (width - 56) * 105 / 930);
			int buttonTop = top + 174;
			int buttonGap = 5;
			for (int i = 0; i < homeButtons.Count; i++)
			{
				homeButtons[i].SetPosition(left + 28, buttonTop + i * (buttonHeight + buttonGap));
				homeButtons[i].Width = width - 56;
				homeButtons[i].Height = buttonHeight;
			}
		}
		else if (page == Page.Goods)
		{
			int rowTop = top + 82;
			int rowHeight = 66;
			int rowGap = 5;
			int rowWidth = width - 56;
			int priceWidth = MathLib.Min(150, rowWidth / 3);
			for (int i = 0; i < goodsRows.Count; i++)
			{
				int y = rowTop + i * (rowHeight + rowGap);
				int x = left + 28;
				Widget background = pageWidgets[i * 4];
				background.SetPosition(x, y); background.Width = rowWidth; background.Height = rowHeight;
				GoodsRow row = goodsRows[i];
				row.Title.SetPosition(x + 14, y + 7); row.Title.Width = rowWidth - priceWidth - 34; row.Title.Height = 26;
				row.Description.SetPosition(x + 14, y + 35); row.Description.Width = rowWidth - priceWidth - 34; row.Description.Height = 22;
				row.Price.SetPosition(x + rowWidth - priceWidth - 14, y + 19); row.Price.Width = priceWidth; row.Price.Height = 28;
			}
			LayoutBackButton(left, top, width, height);
		}
		else if (page == Page.Route || page == Page.Seats)
		{
			int mapWidth = MathLib.Min(width - 56, (height - 160) * 1025 / 598);
			int mapHeight = mapWidth * 598 / 1025;
			pageMap.SetPosition(left + (width - mapWidth) / 2, top + 88);
			pageMap.Width = mapWidth; pageMap.Height = mapHeight;
			LayoutBackButton(left, top, width, height);
		}
		else
		{
			body.SetPosition(left + 28, top + 88); body.Width = width - 56; body.Height = 160;
			LayoutBackButton(left, top, width, height);
		}
	}

	private void LayoutBackButton(int left, int top, int width, int height)
	{
		int buttonWidth = MathLib.Min(190, width - 56);
		int buttonHeight = buttonWidth * 100 / 379;
		backButton.SetPosition(left + 28, top + height - buttonHeight - 12);
		backButton.Width = buttonWidth;
		backButton.Height = buttonHeight;
	}

	private void ClearPageWidgets()
	{
		foreach (Widget widget in pageWidgets) widget.DeleteLater();
		pageWidgets.Clear();
		homeButtons.Clear();
		goodsRows.Clear();
		pageMap = null;
		backButton = null;
	}

	private void Close()
	{
		if (!visible) return;
		visible = false;
		IsVisible = false;
		panel.Hidden = heading.Hidden = body.Hidden = exitHint.Hidden = true;
		ClearPageWidgets();
		Game.Scale = previousGameScale > 0.0f ? previousGameScale : 1.0f;
		Physics.Scale = previousPhysicsScale > 0.0f ? previousPhysicsScale : 1.0f;
		Input.MouseHandle = previousMouseHandle;
		Input.MouseGrab = previousMouseGrab;
		Input.MouseCursorHide = previousCursorHide;
		Input.MouseCursorNeedUpdate = true;
		if (openedForSecondScenario) secondScenario?.ShowTerminalClosed();
		else scenario?.ShowTerminalClosed();
		openedForSecondScenario = false;
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
		ClearPageWidgets();
		panel?.DeleteLater(); heading?.DeleteLater(); body?.DeleteLater(); exitHint?.DeleteLater();
	}
}
