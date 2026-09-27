using System;
using System.Collections.Generic;
using Unigine;

public partial class MenuManager : Component
{
	[ParameterFile(Filter = ".world")] public string mainWorldPath = "";
	[ParameterAsset(Filter = ".ui")] public AssetLink menuUiFile = new AssetLink("ui/menu.ui");
	[ParameterFile(Filter = ".ogg|.wav|.mp3")] public string menuMusicPath = "";
	[ParameterFile(Filter = ".ogg|.wav|.mp3")] public string gameplayMusicPath = "";
	public float musicGain = 0.7f;
	public float environmentGain = 0.7f;
	public string playerName = "Иванов Иван Иванович";
	public string workExperience = "6 лет 4 месяца";
	public string currentRoute = "17";
	[Parameter(Group = "Testing")]
	public bool skipLoginForTesting = true;
	[Parameter(Group = "Testing")]
	public bool skipOpeningDialogueForTesting = true;
	[Parameter(Group = "Testing")]
	public bool startLocationDirectlyForTesting = true;
	[Parameter(Group = "Testing")]
	public bool scenarioOneCompletedForTesting = false;
	public int employeeExperience = 12480;
	[ParameterSlider(Min = 0, Max = 6)] public int unlockedAchievements = 5;
	[ParameterSlider(Min = 0, Max = 100)] public int passengerLoyalty = 86;
	[ParameterSlider(Min = 0, Max = 100)] public int safetyRating = 94;
	[ParameterSlider(Min = 0, Max = 100)] public int regulationCompliance = 91;
	public Input.KEY pauseKey = Input.KEY.ESC;
	public Input.KEY profileKey = Input.KEY.P;

	private enum Screen { Main, Game, Pause, Login, Settings, Profile }
	private enum LoginTarget { Start, Profile }
	private Gui gui;
	private UserInterface ui;
	private WidgetSprite background, settingsBackground, loginBackground, logo, loginInputBackground, passwordInputBackground, confirmInputBackground;
	private WidgetSprite musicVolumeBackground, musicVolumeFill, sfxVolumeBackground, sfxVolumeFill;
	private WidgetSprite[] progressBackgrounds, progressFills;
	private WidgetVBox mainScreen, pauseScreen, loginScreen, settingsScreen, profileScreen;
	private WidgetVBox profileSummaryPanel, profileMetricsPanel, achievementsPanel, rankingPanel;
	private WidgetHBox achievementsRow, achievementItem1, achievementItem2, achievementItem3;
	private WidgetHBox achievementItem4, achievementItem5, achievementItem6;
	private WidgetScrollBox achievementsScroll;
	private WidgetLabel mainStatus, loginStatus, volumeLabel, sfxVolumeLabel, profileName, profileExperience, profileXp, profileEmployeeId, achievementsEmpty;
	private WidgetLabel settingsTitle, musicTitle, effectsTitle;
	private WidgetLabel loyaltyLabel, safetyLabel, complianceLabel;
	private WidgetEditLine loginEdit, passwordEdit, confirmPasswordEdit;
	private WidgetSlider volumeSlider, sfxVolumeSlider;
	private readonly List<WidgetSprite> mainButtonSprites = new List<WidgetSprite>();
	private readonly List<WidgetSprite> allButtonSprites = new List<WidgetSprite>();
	private readonly List<Image> loadedImages = new List<Image>();
	private readonly List<WidgetLayout> layouts = new List<WidgetLayout>();
	private int lastGuiWidth, lastGuiHeight;
	private int lastMainTextureSignature;
	private Screen state = Screen.Main, returnState = Screen.Main;
	private LoginTarget loginTarget;
	private AmbientSource menuMusic, gameplayMusic;
	private GameFlowController subscribedGameFlow;
	private bool authenticated, changingVolume, pauseWasDown, profileWasDown, loadRequested, duplicate;
	private float savedGameScale = 1.0f, savedPhysicsScale = 1.0f;
	private Input.MOUSE_HANDLE savedMouseHandle;
	private bool savedMouseGrab, savedCursorHide;
	private static bool gameplayStarted;
	public static bool BriefingRequestedForCurrentRun { get; private set; }
	public static bool ScenarioOneCompletedForTesting => instance != null && instance.scenarioOneCompletedForTesting;
	private static MenuManager instance;
	public static bool SkipOpeningDialogueForTesting => instance != null && instance.skipOpeningDialogueForTesting;
	private const float ReferenceWidth = 1600.0f;
	private const float ReferenceHeight = 900.0f;
	private const int EnvironmentSoundChannel = 0;
	private const int EnvironmentSoundMask = 1 << EnvironmentSoundChannel;
	private const int MenuMusicChannel = 1;
	private const int MenuMusicMask = 1 << MenuMusicChannel;

	private class WidgetLayout
	{
		public Widget Widget;
		public int Width, Height, FontSize;
		public int SpaceX, SpaceY, PaddingLeft, PaddingRight, PaddingTop, PaddingBottom;
	}

	private void Init()
	{
		if (instance != null && instance != this) { duplicate = true; return; }
		instance = this;
		authenticated = skipLoginForTesting;
		node.Lifetime = Node.LIFETIME.ENGINE;
		gui = Gui.GetCurrent();
		savedMouseHandle = Input.MouseHandle;
		savedMouseGrab = Input.MouseGrab;
		savedCursorHide = Input.MouseCursorHide;
		if (!LoadUi()) return;
		if (gameplayStarted) ShowGame();
		else if (startLocationDirectlyForTesting) StartGame();
		else ShowMain();
	}

	private void Update()
	{
		if (duplicate || ui == null) return;
		UpdateGameFlowSubscription();
		RefreshMainTextureLayout();
		ResizeInterface();
		PositionInputSprites();
		UpdateSettingsProgressBars();
		UpdateProfileProgressBars();
		UpdateVolumeScroll();
		UpdateAchievementsScroll();
		ResizeBackground();
		if (loadRequested && !World.IsLoadWorldRequested)
		{
			loadRequested = false;
			UpdatePlayerSoundMask();
			ApplyVolumeSettings();
		}
		UpdateKeys();
		if (state == Screen.Game && !BriefingWindow.IsIntroductionVisible && !ConductorTerminal.IsVisible
			&& !PassengerScenarioController.IsSeatMonitorViewVisible &&
			(Game.Scale == 0.0f || Physics.Scale == 0.0f)) ResumeScales();
		if (!PassengerScenarioController.IsSeatMonitorViewVisible)
		{
			if (state == Screen.Game && !BriefingWindow.IsVisible && !DialogueWindow.IsVisible
				&& !ConductorTerminal.IsVisible && !ScenarioResults.IsVisible)
				GrabMouse();
			else
				ReleaseMouse();
		}
	}

	private void Shutdown()
	{
		if (duplicate) return;
		if (subscribedGameFlow != null)
			subscribedGameFlow.ReturnToMenuRequested -= ReturnToMainMenuAfterRun;
		if (instance == this) instance = null;
		Input.MouseHandle = savedMouseHandle;
		Input.MouseGrab = savedMouseGrab;
		Input.MouseCursorHide = savedCursorHide;
		Input.MouseCursorNeedUpdate = true;
		StopMusic(ref menuMusic); StopMusic(ref gameplayMusic);
		foreach (WidgetSprite button in mainButtonSprites) Remove(button);
		Remove(loginInputBackground); Remove(passwordInputBackground); Remove(confirmInputBackground);
		Remove(background); Remove(settingsBackground); Remove(loginBackground); Remove(logo); Remove(mainScreen); Remove(pauseScreen); Remove(loginScreen); Remove(settingsScreen); Remove(profileScreen);
		if (ui != null) ui.DeleteLater();
		ui = null;
	}

	private bool LoadUi()
	{
		if (menuUiFile == null || menuUiFile.IsNull || string.IsNullOrWhiteSpace(menuUiFile.Path))
		{
			Log.Error("MenuManager: menuUiFile is not assigned.\n"); return false;
		}
		ui = new UserInterface(gui, menuUiFile.Path);
		if (!ui) { Log.Error("MenuManager: can't load {0}.\n", menuUiFile.Path); return false; }
		ui.Lifetime = Widget.LIFETIME.ENGINE;

		background = W<WidgetSprite>("menu_background");
		settingsBackground = W<WidgetSprite>("settings_background");
		loginBackground = W<WidgetSprite>("login_background");
		logo = W<WidgetSprite>("menu_logo");
		loginInputBackground = W<WidgetSprite>("login_input_background");
		passwordInputBackground = W<WidgetSprite>("password_input_background");
		confirmInputBackground = W<WidgetSprite>("confirm_input_background");
		musicVolumeBackground = W<WidgetSprite>("music_volume_background"); musicVolumeFill = W<WidgetSprite>("music_volume_fill");
		sfxVolumeBackground = W<WidgetSprite>("sfx_volume_background"); sfxVolumeFill = W<WidgetSprite>("sfx_volume_fill");
		progressBackgrounds = new WidgetSprite[] {
			W<WidgetSprite>("profile_xp_background"), W<WidgetSprite>("loyalty_background"),
			W<WidgetSprite>("safety_background"), W<WidgetSprite>("compliance_background"),
			W<WidgetSprite>("rank_xp_1_background"), W<WidgetSprite>("rank_xp_2_background"), W<WidgetSprite>("rank_xp_3_background")
		};
		progressFills = new WidgetSprite[] {
			W<WidgetSprite>("profile_xp_fill"), W<WidgetSprite>("loyalty_fill"),
			W<WidgetSprite>("safety_fill"), W<WidgetSprite>("compliance_fill"),
			W<WidgetSprite>("rank_xp_1_fill"), W<WidgetSprite>("rank_xp_2_fill"), W<WidgetSprite>("rank_xp_3_fill")
		};
		mainScreen = W<WidgetVBox>("main_screen"); pauseScreen = W<WidgetVBox>("pause_screen");
		loginScreen = W<WidgetVBox>("login_screen"); settingsScreen = W<WidgetVBox>("settings_screen");
		profileScreen = W<WidgetVBox>("profile_screen");
		profileSummaryPanel = W<WidgetVBox>("profile_summary_panel"); profileMetricsPanel = W<WidgetVBox>("profile_metrics_panel");
		achievementsPanel = W<WidgetVBox>("achievements_panel");
		rankingPanel = W<WidgetVBox>("ranking_panel"); achievementsRow = W<WidgetHBox>("achievements_row");
		achievementsScroll = W<WidgetScrollBox>("achievements_scroll");
		achievementItem1 = W<WidgetHBox>("achievement_item_1"); achievementItem2 = W<WidgetHBox>("achievement_item_2");
		achievementItem3 = W<WidgetHBox>("achievement_item_3");
		achievementItem4 = W<WidgetHBox>("achievement_item_4"); achievementItem5 = W<WidgetHBox>("achievement_item_5");
		achievementItem6 = W<WidgetHBox>("achievement_item_6");
		mainStatus = W<WidgetLabel>("main_status"); loginStatus = W<WidgetLabel>("login_status");
		volumeLabel = W<WidgetLabel>("volume_label"); sfxVolumeLabel = W<WidgetLabel>("sfx_volume_label");
		settingsTitle = W<WidgetLabel>("settings_title"); musicTitle = W<WidgetLabel>("music_title"); effectsTitle = W<WidgetLabel>("effects_title");
		profileName = W<WidgetLabel>("profile_name");
		profileExperience = W<WidgetLabel>("profile_experience"); profileXp = W<WidgetLabel>("profile_xp");
		profileEmployeeId = W<WidgetLabel>("profile_employee_id"); achievementsEmpty = W<WidgetLabel>("achievements_empty");
		loyaltyLabel = W<WidgetLabel>("loyalty_label");
		safetyLabel = W<WidgetLabel>("safety_label"); complianceLabel = W<WidgetLabel>("compliance_label");
		loginEdit = W<WidgetEditLine>("login_edit"); passwordEdit = W<WidgetEditLine>("password_edit");
		confirmPasswordEdit = W<WidgetEditLine>("confirm_password_edit");
		volumeSlider = W<WidgetSlider>("volume_slider"); sfxVolumeSlider = W<WidgetSlider>("sfx_volume_slider");

		WidgetSprite start = W<WidgetSprite>("start_button"), settings = W<WidgetSprite>("settings_button");
		WidgetSprite profile = W<WidgetSprite>("profile_button"), exit = W<WidgetSprite>("exit_button");
		WidgetSprite resume = W<WidgetSprite>("resume_button"), pauseSettings = W<WidgetSprite>("pause_settings_button");
		WidgetSprite pauseProfile = W<WidgetSprite>("pause_profile_button");
		WidgetSprite menu = W<WidgetSprite>("menu_button"), login = W<WidgetSprite>("login_button");
		WidgetSprite loginBack = W<WidgetSprite>("login_back_button"), settingsBack = W<WidgetSprite>("settings_back_button");
		WidgetSprite profileBack = W<WidgetSprite>("profile_back_button");
		if (!LoadSpriteImage(settingsBackground, "ui/settings_background.png") ||
			!LoadSpriteImage(loginBackground, "ui/background_login.png") ||
			!LoadSpriteImage(start, "ui/Start.png") || !LoadSpriteImage(settings, "ui/Options.png") ||
			!LoadSpriteImage(profile, "ui/Profile.png") || !LoadSpriteImage(exit, "ui/Exit.png") ||
			!LoadSpriteImage(resume, "ui/Continue.png") || !LoadSpriteImage(pauseSettings, "ui/Options.png") ||
			!LoadSpriteImage(pauseProfile, "ui/Profile.png") ||
			!LoadSpriteImage(menu, "ui/Exit.png") ||
			!LoadSpriteImage(login, "ui/Enter.png") ||
			!LoadSpriteImage(loginBack, "ui/Back.png") ||
			!LoadSpriteImage(settingsBack, "ui/Back.png") ||
			!LoadSpriteImage(profileBack, "ui/Back.png") ||
			!LoadSpriteImage(loginInputBackground, "ui/Login.png") || !LoadSpriteImage(passwordInputBackground, "ui/Password.png") ||
			!LoadSpriteImage(confirmInputBackground, "ui/Password.png") ||
			!LoadSpriteImage(musicVolumeBackground, "ui/ProgressBackground.png") || !LoadSpriteImage(musicVolumeFill, "ui/ProgressFill.png") ||
			!LoadSpriteImage(sfxVolumeBackground, "ui/ProgressBackground.png") || !LoadSpriteImage(sfxVolumeFill, "ui/ProgressFill.png") ||
			!LoadProgressImages())
		{
			Log.Error("MenuManager: failed to load one or more menu PNG files.\n");
			return false;
		}

		if (background == null || settingsBackground == null || loginBackground == null || logo == null || loginInputBackground == null || passwordInputBackground == null || confirmInputBackground == null ||
			musicVolumeBackground == null || musicVolumeFill == null || sfxVolumeBackground == null || sfxVolumeFill == null ||
			mainScreen == null || pauseScreen == null || loginScreen == null || settingsScreen == null ||
			profileScreen == null || profileSummaryPanel == null || profileMetricsPanel == null || achievementsPanel == null || rankingPanel == null ||
			achievementsScroll == null || achievementsRow == null || achievementItem1 == null || achievementItem2 == null || achievementItem3 == null ||
			achievementItem4 == null || achievementItem5 == null || achievementItem6 == null ||
			mainStatus == null || loginStatus == null || volumeLabel == null || sfxVolumeLabel == null || settingsTitle == null ||
			musicTitle == null || effectsTitle == null || profileName == null ||
			profileExperience == null || profileXp == null || profileEmployeeId == null || achievementsEmpty == null || loyaltyLabel == null ||
			safetyLabel == null || complianceLabel == null || loginEdit == null || passwordEdit == null ||
			volumeSlider == null || sfxVolumeSlider == null ||
			start == null || settings == null || profile == null || exit == null || resume == null || pauseSettings == null || pauseProfile == null ||
			menu == null || login == null || loginBack == null || settingsBack == null || profileBack == null || confirmPasswordEdit == null)
		{
			Log.Error("MenuManager: menu.ui is missing required exported widgets.\n"); return false;
		}

		start.EventClicked.Connect(RequestStart); settings.EventClicked.Connect(ShowSettings);
		profile.EventClicked.Connect(RequestProfile); exit.EventClicked.Connect(() => Engine.Quit());
		resume.EventClicked.Connect(ShowGame); pauseSettings.EventClicked.Connect(ShowSettings); pauseProfile.EventClicked.Connect(RequestProfile);
		menu.EventClicked.Connect(() => Engine.Quit()); login.EventClicked.Connect(Login);
		loginBack.EventClicked.Connect(BackFromLogin); settingsBack.EventClicked.Connect(BackFromSettings);
		profileBack.EventClicked.Connect(BackFromProfile); volumeSlider.EventChanged.Connect(ChangeMenuVolume);
		sfxVolumeSlider.EventChanged.Connect(ChangeEnvironmentVolume);
		WidgetSprite[] buttons = { start, settings, profile, exit, resume, pauseSettings, pauseProfile, menu, login, loginBack, settingsBack, profileBack };
		foreach (WidgetSprite button in buttons) BindButtonSprite(button);
		loginBack.Width = 376; loginBack.Height = 79;
		settingsBack.Width = 320;
		settingsBack.Height = 67;
		allButtonSprites.AddRange(buttons);
		loginEdit.FontSize = 24; passwordEdit.FontSize = 24; confirmPasswordEdit.FontSize = 24;
		loginInputBackground.IntersectionEnabled = false;
		passwordInputBackground.IntersectionEnabled = false;
		confirmInputBackground.IntersectionEnabled = false;
		W<WidgetLabel>("login_button_label").IntersectionEnabled = false;
		W<WidgetLabel>("login_button_label").Hidden = true;
		W<WidgetLabel>("login_back_button_label").Hidden = true;
		W<WidgetLabel>("login_back_button_label").IntersectionEnabled = false;
		if (!LoadSpriteImage(logo, "ui/Name_Menu.png"))
		{
			Log.Error("MenuManager: failed to load ui/Name_Menu.png.\n");
			return false;
		}
		if (!ConfigurePanel(profileScreen, "ui/ProfileOuterPanel.png", 0.03f, 0.03f, 0.04f, 0.04f) ||
			!ConfigurePanel(profileSummaryPanel, "ui/ProfileEmployeePanel.png", 0.04f, 0.04f, 0.20f, 0.12f) ||
			!ConfigurePanel(profileMetricsPanel, "ui/ProfileMetricsPanel.png", 0.04f, 0.04f, 0.20f, 0.12f) ||
			!ConfigurePanel(achievementsPanel, "ui/ProfileAchievementsPanel.png", 0.02f, 0.02f, 0.19f, 0.19f) ||
			!ConfigurePanel(rankingPanel, "ui/ProfileRankingPanel.png", 0.02f, 0.02f, 0.20f, 0.16f))
		{
			Log.Error("MenuManager: failed to load profile panel PNG files.\n");
			return false;
		}
		loginEdit.Background = 0;
		passwordEdit.Background = 0;
		confirmPasswordEdit.Background = 0;
		settingsTitle.FontRich = 1;
		musicTitle.FontRich = 1;
		effectsTitle.FontRich = 1;
		volumeLabel.FontRich = 1;
		sfxVolumeLabel.FontRich = 1;
		volumeSlider.BackgroundColor = new vec4(1.0f, 1.0f, 1.0f, 0.0f);
		volumeSlider.ButtonColor = new vec4(1.0f, 1.0f, 1.0f, 0.0f);
		sfxVolumeSlider.BackgroundColor = new vec4(1.0f, 1.0f, 1.0f, 0.0f);
		sfxVolumeSlider.ButtonColor = new vec4(1.0f, 1.0f, 1.0f, 0.0f);
		musicVolumeBackground.IntersectionEnabled = false;
		musicVolumeFill.IntersectionEnabled = false;
		sfxVolumeBackground.IntersectionEnabled = false;
		sfxVolumeFill.IntersectionEnabled = false;
		foreach (WidgetSprite sprite in progressBackgrounds) sprite.IntersectionEnabled = false;
		foreach (WidgetSprite sprite in progressFills) sprite.IntersectionEnabled = false;
		loginEdit.BorderColor = new vec4(0.0f, 0.0f, 0.0f, 0.0f);
		passwordEdit.BorderColor = new vec4(0.0f, 0.0f, 0.0f, 0.0f);
		confirmPasswordEdit.BorderColor = new vec4(0.0f, 0.0f, 0.0f, 0.0f);
		mainButtonSprites.Add(start); mainButtonSprites.Add(settings); mainButtonSprites.Add(profile); mainButtonSprites.Add(exit);

		UpdateProfileData();
		mainScreen.Background = 0;
		mainScreen.IntersectionEnabled = false;
		mainScreen.Hidden = true;
		volumeSlider.Value = SnapVolume((int)(musicGain * 100.0f)); ChangeMenuVolume();
		sfxVolumeSlider.Value = SnapVolume((int)(environmentGain * 100.0f)); ChangeEnvironmentVolume();
		gui.AddChild(background, Gui.ALIGN_OVERLAP | Gui.ALIGN_BACKGROUND);
		gui.AddChild(settingsBackground, Gui.ALIGN_OVERLAP | Gui.ALIGN_BACKGROUND);
		gui.AddChild(loginBackground, Gui.ALIGN_OVERLAP | Gui.ALIGN_BACKGROUND);
		gui.AddChild(logo, Gui.ALIGN_OVERLAP);
		gui.AddChild(loginInputBackground, Gui.ALIGN_OVERLAP);
		gui.AddChild(passwordInputBackground, Gui.ALIGN_OVERLAP);
		gui.AddChild(confirmInputBackground, Gui.ALIGN_OVERLAP);
		foreach (WidgetSprite button in mainButtonSprites) gui.AddChild(button, Gui.ALIGN_OVERLAP);
		gui.AddChild(mainScreen, Gui.ALIGN_OVERLAP);
		gui.AddChild(pauseScreen, Gui.ALIGN_OVERLAP);
		gui.AddChild(loginScreen, Gui.ALIGN_OVERLAP);
		gui.AddChild(settingsScreen, Gui.ALIGN_OVERLAP);
		gui.AddChild(profileScreen, Gui.ALIGN_OVERLAP);
		CaptureLayouts();
		ResizeInterface();
		return true;
	}

	private T W<T>(string name) where T : Widget { return ui.GetWidgetByName(name) as T; }
	private bool LoadSpriteImage(WidgetSprite sprite, string path)
	{
		if (sprite == null) return false;
		Image image = new Image();
		if (!image.Load(path)) return false;
		sprite.SetImage(image);
		loadedImages.Add(image);
		return true;
	}
	private bool LoadProgressImages()
	{
		if (progressBackgrounds == null || progressFills == null || progressBackgrounds.Length != progressFills.Length) return false;
		for (int i = 0; i < progressBackgrounds.Length; i++)
		{
			if (!LoadSpriteImage(progressBackgrounds[i], "ui/ProgressBackground.png") ||
				!LoadSpriteImage(progressFills[i], "ui/ProgressFill.png")) return false;
		}
		return true;
	}
	private bool ConfigurePanel(WidgetVBox panel, string path, float left, float right, float top, float bottom)
	{
		Image image = new Image();
		if (!image.Load(path)) return false;
		panel.SetBackgroundImage(image);
		panel.Background = 1;
		panel.Background9Sliced = true;
		panel.SetBackground9SliceOffsets(left, right, top, bottom);
		panel.Background9SliceScale = 1.0f;
		loadedImages.Add(image);
		return true;
	}
	private void BindButtonSprite(WidgetSprite sprite)
	{
		int textureWidth = sprite.GetLayerWidth(0);
		int textureHeight = sprite.GetLayerHeight(0);
		if (textureWidth > 0 && textureHeight > 0)
		{
			sprite.Width = textureWidth;
			sprite.Height = Math.Max(1, textureHeight / 2);
		}
		sprite.SetLayerTexCoord(0, new vec4(0.0f, 0.0f, 1.0f, 0.5f));
		sprite.EventEnter.Connect(ButtonEnter); sprite.EventLeave.Connect(ButtonLeave);
	}
	private void ButtonEnter(Widget widget)
	{
		WidgetSprite sprite = widget as WidgetSprite;
		if (sprite != null)
			sprite.SetLayerTexCoord(0, new vec4(0.0f, 0.5f, 1.0f, 1.0f));
	}
	private void ButtonLeave(Widget widget)
	{
		WidgetSprite sprite = widget as WidgetSprite;
		if (sprite != null)
			sprite.SetLayerTexCoord(0, new vec4(0.0f, 0.0f, 1.0f, 0.5f));
	}
	private void Remove(Widget widget) { if (gui != null && widget != null && widget.IsValidPtr) gui.RemoveChild(widget); }

	private void CaptureLayouts()
	{
		layouts.Clear();
		for (int i = 0; i < ui.NumWidgets; i++)
		{
			Widget widget = ui.GetWidget(i);
			if (widget == null || widget == background || widget == loginInputBackground || widget == passwordInputBackground || widget == confirmInputBackground ||
				allButtonSprites.Contains(widget as WidgetSprite)) continue;
			WidgetLayout layout = new WidgetLayout { Widget = widget, Width = widget.Width, Height = widget.Height, FontSize = widget.FontSize };
			WidgetVBox box = widget as WidgetVBox;
			if (box != null)
			{
				layout.SpaceX = box.SpaceX; layout.SpaceY = box.SpaceY;
				layout.PaddingLeft = box.PaddingLeft; layout.PaddingRight = box.PaddingRight;
				layout.PaddingTop = box.PaddingTop; layout.PaddingBottom = box.PaddingBottom;
			}
			layouts.Add(layout);
		}
	}

	private void ResizeInterface()
	{
		if (gui.Width == lastGuiWidth && gui.Height == lastGuiHeight) return;
		lastGuiWidth = gui.Width; lastGuiHeight = gui.Height;
		float scale = Math.Min(gui.Width / ReferenceWidth, gui.Height / ReferenceHeight);
		scale = Math.Max(0.2f, Math.Min(1.5f, scale));
		foreach (WidgetLayout layout in layouts)
		{
			if (layout.Width > 0) layout.Widget.Width = Math.Max(1, (int)(layout.Width * scale));
			if (layout.Height > 0) layout.Widget.Height = Math.Max(1, (int)(layout.Height * scale));
			if (layout.FontSize > 0) layout.Widget.FontSize = Math.Max(10, (int)(layout.FontSize * scale));
			WidgetVBox box = layout.Widget as WidgetVBox;
			if (box != null)
			{
				box.SetSpace((int)(layout.SpaceX * scale), (int)(layout.SpaceY * scale));
				box.SetPadding((int)(layout.PaddingLeft * scale), (int)(layout.PaddingRight * scale),
					(int)(layout.PaddingTop * scale), (int)(layout.PaddingBottom * scale));
			}
		}
		UpdateAchievementsRowWidth(scale);
		ResizeBackButtons(scale);
		ResizeMainButtons();
		mainScreen.Arrange(); pauseScreen.Arrange(); loginScreen.Arrange(); settingsScreen.Arrange(); profileScreen.Arrange();
		PositionScreens();
	}

	private void ResizeBackButtons(float scale)
	{
		WidgetSprite loginBack = W<WidgetSprite>("login_back_button");
		WidgetSprite settingsBack = W<WidgetSprite>("settings_back_button");
		WidgetSprite profileBack = W<WidgetSprite>("profile_back_button");
		if (loginBack != null) { loginBack.Width = Math.Max(1, (int)(376 * scale)); loginBack.Height = Math.Max(1, (int)(79 * scale)); }
		if (settingsBack != null) { settingsBack.Width = Math.Max(1, (int)(320 * scale)); settingsBack.Height = Math.Max(1, (int)(67 * scale)); }
		if (profileBack != null) { profileBack.Width = Math.Max(1, (int)(260 * scale)); profileBack.Height = Math.Max(1, (int)(55 * scale)); }
	}

	private void RefreshMainTextureLayout()
	{
		int signature = 17;
		bool ready = mainButtonSprites.Count > 0;
		foreach (WidgetSprite button in mainButtonSprites)
		{
			int width = button.GetLayerWidth(0);
			int height = button.GetLayerHeight(0);
			ready &= width > 0 && height > 0;
			signature = signature * 31 + width;
			signature = signature * 31 + height;
		}

		if (!ready || signature == lastMainTextureSignature) return;
		lastMainTextureSignature = signature;
		lastGuiWidth = -1;
		lastGuiHeight = -1;
	}

	private void ResizeMainButtons()
	{
		if (mainButtonSprites.Count == 0) return;
		int buttonWidth = mainButtonSprites[0].Width;
		int buttonHeight = mainButtonSprites[0].Height;
		foreach (WidgetSprite button in mainButtonSprites)
		{
			button.Width = buttonWidth;
			button.Height = buttonHeight;
		}
		mainStatus.Width = buttonWidth;
		mainStatus.Height = Math.Max(1, (int)(gui.Height * 0.035f));
		mainScreen.Width = buttonWidth;
		mainScreen.Height = mainStatus.Height;
		mainScreen.SetSpace(0, 0);
		mainScreen.SetPadding(0, 0, 0, 0);
	}

	private void PositionScreens()
	{
		int sideMargin = Math.Max(1, (int)(gui.Width * 0.02f));
		int bottomMargin = Math.Max(1, (int)(gui.Height * 0.04f));
		int buttonX = Math.Min(gui.Width - mainButtonSprites[0].Width - sideMargin,
			Math.Max(sideMargin, (int)(gui.Width * 0.68f)));
		int buttonY = Math.Max(0, gui.Height - GetMainButtonsHeight() - bottomMargin);
		foreach (WidgetSprite button in mainButtonSprites)
		{
			button.SetPosition(buttonX, buttonY);
			buttonY += button.Height + Math.Max(1, (int)(gui.Height * 0.008f));
		}
		mainScreen.SetPosition(buttonX, Math.Min(gui.Height - mainStatus.Height, buttonY));
		int logoMargin = Math.Max(16, (int)(gui.Width * 0.025f));
		logo.SetPosition(Math.Max(0, gui.Width - logo.Width - logoMargin), Math.Max(0, (int)(gui.Height * 0.035f)));

		CenterScreen(pauseScreen, sideMargin);
		CenterScreen(loginScreen, sideMargin);
		CenterScreen(settingsScreen, sideMargin);
		CenterScreen(profileScreen, sideMargin);
	}
	private void PositionInputSprites()
	{
		if (loginInputBackground == null || passwordInputBackground == null || confirmInputBackground == null) return;
		loginInputBackground.Width = loginEdit.Width; loginInputBackground.Height = loginEdit.Height;
		passwordInputBackground.Width = passwordEdit.Width; passwordInputBackground.Height = passwordEdit.Height;
		confirmInputBackground.Width = confirmPasswordEdit.Width; confirmInputBackground.Height = confirmPasswordEdit.Height;
		loginInputBackground.SetPosition(loginEdit.ScreenPositionX, loginEdit.ScreenPositionY);
		passwordInputBackground.SetPosition(passwordEdit.ScreenPositionX, passwordEdit.ScreenPositionY);
		confirmInputBackground.SetPosition(confirmPasswordEdit.ScreenPositionX, confirmPasswordEdit.ScreenPositionY);
	}
	private void UpdateSettingsProgressBars()
	{
		if (volumeSlider != null && musicVolumeBackground != null && musicVolumeFill != null)
			SetProgress(musicVolumeBackground, musicVolumeFill, volumeSlider.Value, 100);
		if (sfxVolumeSlider != null && sfxVolumeBackground != null && sfxVolumeFill != null)
			SetProgress(sfxVolumeBackground, sfxVolumeFill, sfxVolumeSlider.Value, 100);
	}
	private void UpdateProfileProgressBars()
	{
		if (progressBackgrounds == null || progressFills == null || progressBackgrounds.Length != 7) return;
		SetProgress(progressBackgrounds[0], progressFills[0], employeeExperience, 20000);
		SetProgress(progressBackgrounds[1], progressFills[1], passengerLoyalty, 100);
		SetProgress(progressBackgrounds[2], progressFills[2], safetyRating, 100);
		SetProgress(progressBackgrounds[3], progressFills[3], regulationCompliance, 100);
		SetProgress(progressBackgrounds[4], progressFills[4], 18920, 20000);
		SetProgress(progressBackgrounds[5], progressFills[5], 15670, 20000);
		SetProgress(progressBackgrounds[6], progressFills[6], 12480, 20000);
	}
	private void SetProgress(WidgetSprite backgroundSprite, WidgetSprite fillSprite, int value, int maximum)
	{
		if (backgroundSprite == null || fillSprite == null || maximum <= 0) return;
		float progress = Math.Max(0.0f, Math.Min(1.0f, value / (float)maximum));
		fillSprite.Hidden = progress <= 0.0f;
		fillSprite.Width = Math.Max(1, (int)Math.Round(backgroundSprite.Width * progress));
		fillSprite.Height = backgroundSprite.Height;
		fillSprite.SetLayerTexCoord(0, new vec4(0.0f, 0.0f, progress, 1.0f));
	}
	private void UpdateVolumeScroll()
	{
		if (state != Screen.Settings) return;
		UpdateVolumeScroll(musicVolumeBackground, volumeSlider, ChangeMenuVolume);
		UpdateVolumeScroll(sfxVolumeBackground, sfxVolumeSlider, ChangeEnvironmentVolume);
	}
	private void UpdateVolumeScroll(WidgetSprite track, WidgetSlider slider, Action change)
	{
		if (track == null || slider == null) return;
		int mouseX = gui.MouseX;
		int mouseY = gui.MouseY;
		bool hovered = mouseX >= track.ScreenPositionX && mouseX < track.ScreenPositionX + track.Width &&
			mouseY >= track.ScreenPositionY && mouseY < track.ScreenPositionY + track.Height;
		int wheel = Input.MouseWheel;
		if (!hovered || wheel == 0) return;
		slider.Value = Math.Max(0, Math.Min(100, slider.Value + wheel * 10));
		change();
	}
	private void UpdateAchievementsScroll()
	{
		if (state != Screen.Profile || achievementsScroll.Hidden) return;
		int maximum = Math.Max(0, achievementsScroll.HScrollObjectSize - achievementsScroll.HScrollFrameSize);
		if (achievementsScroll.HScrollValue > maximum) achievementsScroll.HScrollValue = maximum;
		int mouseX = gui.MouseX;
		int mouseY = gui.MouseY;
		bool hovered = mouseX >= achievementsScroll.ScreenPositionX && mouseX < achievementsScroll.ScreenPositionX + achievementsScroll.Width &&
			mouseY >= achievementsScroll.ScreenPositionY && mouseY < achievementsScroll.ScreenPositionY + achievementsScroll.Height;
		int wheel = Input.MouseWheel;
		if (!hovered || wheel == 0) return;
		achievementsScroll.HScrollValue = Math.Max(0, Math.Min(maximum, achievementsScroll.HScrollValue - wheel * 860));
	}

	private void UpdateAchievementsRowWidth(float scale)
	{
		int achievementCount = Math.Max(0, Math.Min(6, unlockedAchievements));
		int itemWidth = Math.Max(1, (int)(320 * scale));
		int spacing = Math.Max(0, (int)(8 * scale));
		achievementsRow.Width = Math.Max(1, achievementCount * itemWidth + Math.Max(0, achievementCount - 1) * spacing);
	}

	private int GetMainButtonsHeight()
	{
		int height = 0;
		foreach (WidgetSprite button in mainButtonSprites) height += button.Height;
		return height + Math.Max(0, mainButtonSprites.Count - 1) * Math.Max(1, (int)(gui.Height * 0.008f));
	}

	private void CenterScreen(Widget widget, int sideMargin)
	{
		int x = Math.Max(sideMargin, (gui.Width - widget.Width) / 2);
		int y = Math.Max(0, (gui.Height - widget.Height) / 2);
		widget.SetPosition(x, y);
	}
	private void UpdateProfileData()
	{
		int loyalty = Math.Max(0, Math.Min(100, passengerLoyalty));
		int safety = Math.Max(0, Math.Min(100, safetyRating));
		int compliance = Math.Max(0, Math.Min(100, regulationCompliance));
		profileName.Text = playerName;
		profileExperience.Text = "Стаж: " + workExperience;
		profileXp.Text = "Опыт: " + employeeExperience.ToString("N0").Replace(',', ' ');
		string employeeId = string.IsNullOrWhiteSpace(loginEdit.Text) ? "DEMO" : loginEdit.Text.Trim();
		profileEmployeeId.Text = "ID сотрудника: " + employeeId;
		loyaltyLabel.Text = loyalty + "%";
		safetyLabel.Text = safety + "%";
		complianceLabel.Text = compliance + "%";
		UpdateProfileProgressBars();
		int achievementCount = Math.Max(0, Math.Min(6, unlockedAchievements));
		achievementItem1.Hidden = achievementCount < 1;
		achievementItem2.Hidden = achievementCount < 2;
		achievementItem3.Hidden = achievementCount < 3;
		achievementItem4.Hidden = achievementCount < 4;
		achievementItem5.Hidden = achievementCount < 5;
		achievementItem6.Hidden = achievementCount < 6;
		float scale = Math.Max(0.2f, Math.Min(1.5f, Math.Min(gui.Width / ReferenceWidth, gui.Height / ReferenceHeight)));
		UpdateAchievementsRowWidth(scale);
		achievementsRow.Hidden = achievementCount == 0;
		achievementsEmpty.Hidden = achievementCount != 0;
	}

	private void RequestStart() { if (authenticated) StartGame(); else ShowLogin(LoginTarget.Start); }
	private void RequestProfile() { if (authenticated) ShowProfile(); else ShowLogin(LoginTarget.Profile); }
	private void StartGame()
	{
		mainStatus.Text = "";
		mainScreen.Hidden = true;
		if (string.IsNullOrWhiteSpace(mainWorldPath))
		{
			mainStatus.Text = "Укажите mainWorldPath";
			mainScreen.Hidden = false;
			return;
		}
		gameplayStarted = true; loadRequested = true; BriefingRequestedForCurrentRun = !skipOpeningDialogueForTesting; ResumeScales(); ShowGame();
		if (!World.LoadWorld(mainWorldPath))
		{
			loadRequested = false; gameplayStarted = false; BriefingRequestedForCurrentRun = false;
			mainStatus.Text = "Не удалось загрузить мир"; ShowMain();
			mainScreen.Hidden = false;
		}
	}

	public static void CompleteRunBriefingRequest()
	{
		BriefingRequestedForCurrentRun = false;
	}

	private void ShowLogin(LoginTarget target)
	{
		loginTarget = target; returnState = state; loginStatus.Text = ""; loginEdit.Text = ""; passwordEdit.Text = ""; confirmPasswordEdit.Text = "";
		SetState(Screen.Login);
	}
	private void Login()
	{
		if (passwordEdit.Text != confirmPasswordEdit.Text) { loginStatus.Text = "PIN confirmation does not match"; return; }
		if (loginEdit.Text != "DEMO" || passwordEdit.Text != "2026") { loginStatus.Text = "Неверный табельный номер или PIN-код"; return; }
		authenticated = true; loginStatus.Text = "";
		if (loginTarget == LoginTarget.Start) StartGame();
		else { Screen origin = returnState; ShowProfile(); returnState = origin; }
	}
	private void ShowSettings() { returnState = state; SetState(Screen.Settings); }
	private void ShowProfile() { if (state == Screen.Profile) return; returnState = state; UpdateProfileData(); SetState(Screen.Profile); if (returnState == Screen.Game) PauseScales(); }
	private void BackFromLogin() { if (returnState == Screen.Pause) ShowPause(); else ShowMain(); }
	private void BackFromSettings() { if (returnState == Screen.Pause) ShowPause(); else ShowMain(); }
	private void BackFromProfile() { if (returnState == Screen.Game) ShowGame(); else if (returnState == Screen.Pause) ShowPause(); else ShowMain(); }

	private void ChangeMenuVolume()
	{
		if (volumeSlider == null || changingVolume) return;
		changingVolume = true; int value = SnapVolume(volumeSlider.Value); volumeSlider.Value = value;
		musicGain = value / 100.0f;
		Sound.SetSourceVolume(MenuMusicChannel, musicGain);
		if (menuMusic != null) menuMusic.Gain = 1.0f;
		volumeLabel.Text = "<b>Мелодия меню: " + value + "</b>";
		UpdateSettingsProgressBars(); changingVolume = false;
	}
	private void ChangeEnvironmentVolume()
	{
		if (sfxVolumeSlider == null || changingVolume) return;
		changingVolume = true; int value = SnapVolume(sfxVolumeSlider.Value); sfxVolumeSlider.Value = value;
		environmentGain = value / 100.0f;
		Sound.SetSourceVolume(EnvironmentSoundChannel, environmentGain);
		if (gameplayMusic != null) gameplayMusic.Gain = 1.0f;
		sfxVolumeLabel.Text = "<b>Звук окружения: " + value + "</b>";
		UpdateSettingsProgressBars(); changingVolume = false;
	}
	private int SnapVolume(int value) { return Math.Max(0, Math.Min(100, ((value + 5) / 10) * 10)); }

	private void ShowMain() { gameplayStarted = false; loadRequested = false; SetState(Screen.Main); PauseScales(); }
	private void ShowGame() { SetState(Screen.Game); ResumeScales(); }
	private void ShowPause() { SetState(Screen.Pause); PauseScales(); }

	private void UpdateGameFlowSubscription()
	{
		GameFlowController current = ComponentSystem.FindComponentInWorld<GameFlowController>();
		if (current == subscribedGameFlow) return;

		if (subscribedGameFlow != null)
			subscribedGameFlow.ReturnToMenuRequested -= ReturnToMainMenuAfterRun;

		subscribedGameFlow = current;
		if (subscribedGameFlow != null)
			subscribedGameFlow.ReturnToMenuRequested += ReturnToMainMenuAfterRun;
	}

	private void ReturnToMainMenuAfterRun()
	{
		gameplayStarted = false;
		BriefingRequestedForCurrentRun = false;
		ShowMain();

		// Reload the gameplay world so every scenario component and score is reset
		// before the player starts a new run from the persistent main menu.
		if (string.IsNullOrWhiteSpace(mainWorldPath) || !World.LoadWorld(mainWorldPath))
		{
			Log.Error("MenuManager: failed to reload the gameplay world after completing the run.\n");
			return;
		}

		loadRequested = true;
	}

	private void SetState(Screen next)
	{
		Screen previous = state;
		state = next; bool game = state == Screen.Game || loadRequested;
		background.Hidden = game || state == Screen.Settings || state == Screen.Login;
		settingsBackground.Hidden = state != Screen.Settings;
		loginBackground.Hidden = state != Screen.Login;
		logo.Hidden = state != Screen.Main;
		foreach (WidgetSprite button in mainButtonSprites) button.Hidden = state != Screen.Main;
		mainScreen.Hidden = state != Screen.Main || string.IsNullOrWhiteSpace(mainStatus.Text); pauseScreen.Hidden = state != Screen.Pause;
		loginScreen.Hidden = state != Screen.Login; settingsScreen.Hidden = state != Screen.Settings;
		profileScreen.Hidden = state != Screen.Profile;
		if (state != Screen.Profile)
		{
			achievementsPanel.Hidden = true;
			achievementsScroll.Hidden = true;
			achievementsRow.Hidden = true;
			achievementItem1.Hidden = true;
			achievementItem2.Hidden = true;
			achievementItem3.Hidden = true;
			achievementItem4.Hidden = true;
			achievementItem5.Hidden = true;
			achievementItem6.Hidden = true;
			achievementsEmpty.Hidden = true;
		}
		else
		{
			achievementsPanel.Hidden = false;
			achievementsScroll.Hidden = false;
			UpdateProfileData();
		}
		loginInputBackground.Hidden = state != Screen.Login;
		passwordInputBackground.Hidden = state != Screen.Login;
		confirmInputBackground.Hidden = state != Screen.Login;
		UpdateMusic(previous);
	}

	private void UpdateMusic(Screen previous)
	{
		ApplyVolumeSettings();
		if (state == Screen.Game)
		{
			StopMusic(ref menuMusic);
			PlayMusic(ref gameplayMusic, gameplayMusicPath, EnvironmentSoundMask);
		}
		else
		{
			StopMusic(ref gameplayMusic);
			if (previous == Screen.Game || menuMusic == null || !menuMusic.IsPlaying)
				RestartMusic(ref menuMusic, menuMusicPath, MenuMusicMask);
		}
	}

	private void UpdatePlayerSoundMask()
	{
		if (Game.Player != null)
			Game.Player.SourceMask |= EnvironmentSoundMask | MenuMusicMask;
	}

	private void ApplyVolumeSettings()
	{
		Sound.SetSourceVolume(MenuMusicChannel, musicGain);
		Sound.SetSourceVolume(EnvironmentSoundChannel, environmentGain);
	}

	private void GrabMouse() { Input.MouseHandle = Input.MOUSE_HANDLE.USER; Input.MouseGrab = true; Input.MouseCursorHide = true; Input.MouseCursorNeedUpdate = true; }
	private void ReleaseMouse() { Input.MouseHandle = Input.MOUSE_HANDLE.USER; Input.MouseGrab = false; Input.MouseCursorHide = false; Input.MouseCursorNeedUpdate = true; }
	private void PauseScales()
	{
		if (Game.Scale > 0.0f) savedGameScale = Game.Scale; if (Physics.Scale > 0.0f) savedPhysicsScale = Physics.Scale;
		Game.Scale = 0.0f; Physics.Scale = 0.0f;
	}
	private void ResumeScales() { Game.Scale = savedGameScale > 0.0f ? savedGameScale : 1.0f; Physics.Scale = savedPhysicsScale > 0.0f ? savedPhysicsScale : 1.0f; }

	private void UpdateKeys()
	{
		if (PassengerScenarioController.IsSeatMonitorViewVisible)
		{
			pauseWasDown = Input.IsKeyDown(pauseKey);
			profileWasDown = Input.IsKeyDown(profileKey);
			return;
		}
		if (ConductorTerminal.IsVisible)
		{
			pauseWasDown = Input.IsKeyDown(pauseKey);
			profileWasDown = Input.IsKeyDown(profileKey);
			return;
		}
		if (BriefingWindow.IsVisible)
		{
			pauseWasDown = Input.IsKeyDown(pauseKey);
			profileWasDown = Input.IsKeyDown(profileKey);
			return;
		}
		if (BriefingWindow.SuppressPauseUntilEscRelease)
		{
			pauseWasDown = Input.IsKeyDown(pauseKey);
			profileWasDown = Input.IsKeyDown(profileKey);
			return;
		}
		bool pause = Input.IsKeyDown(pauseKey);
		if (pause && !pauseWasDown)
		{
			if (state == Screen.Game) ShowPause(); else if (state == Screen.Pause) ShowGame();
			else if (state == Screen.Profile) BackFromProfile(); else if (state == Screen.Login) BackFromLogin();
			else if (state == Screen.Settings) BackFromSettings();
		}
		pauseWasDown = pause;
		bool profile = Input.IsKeyDown(profileKey);
		if (profile && !profileWasDown && (state == Screen.Game || state == Screen.Pause)) RequestProfile();
		profileWasDown = profile;
	}

	private void ResizeBackground()
	{
		ResizeBackground(background);
		ResizeBackground(settingsBackground);
		ResizeBackground(loginBackground);
	}
	private void ResizeBackground(WidgetSprite sprite)
	{
		int tw = sprite.GetLayerWidth(0), th = sprite.GetLayerHeight(0);
		if (tw <= 0 || th <= 0) { sprite.Width = gui.Width; sprite.Height = gui.Height; sprite.SetPosition(0, 0); return; }
		float scale = Math.Max((float)gui.Width / tw, (float)gui.Height / th);
		int width = (int)(tw * scale), height = (int)(th * scale);
		sprite.Width = width; sprite.Height = height; sprite.SetPosition((gui.Width - width) / 2, (gui.Height - height) / 2);
	}

	private void PlayMusic(ref AmbientSource source, string path, int sourceMask)
	{
		if (string.IsNullOrWhiteSpace(path)) return;
		if (source == null) { source = new AmbientSource(path, 1); source.Loop = 1; }
		UpdatePlayerSoundMask();
		ApplyVolumeSettings();
		source.SourceMask = sourceMask;
		source.Gain = 1.0f; if (!source.IsPlaying) source.Play();
	}
	private void RestartMusic(ref AmbientSource source, string path, int sourceMask)
	{
		if (string.IsNullOrWhiteSpace(path)) return;
		if (source == null) { source = new AmbientSource(path, 1); source.Loop = 1; }
		UpdatePlayerSoundMask();
		ApplyVolumeSettings();
		source.SourceMask = sourceMask;
		if (source.IsPlaying)
		{
			source.Stop();
			Sound.RenderWorld(1);
		}
		source.Time = 0.0f;
		source.Gain = 1.0f;
		source.Play();
	}
	private void StopMusic(ref AmbientSource source) { if (source != null && source.IsPlaying) source.Stop(); }
}
