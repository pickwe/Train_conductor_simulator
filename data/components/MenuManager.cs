using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Unigine;

public partial class MenuManager : Component
{
	[ParameterFile(Filter = ".world")] public string mainWorldPath = "";
	[ParameterAsset(Filter = ".ui")] public AssetLink menuUiFile = new AssetLink("ui/menu.ui");
	[ParameterFile(Filter = ".ogg|.wav|.mp3")] public string menuMusicPath = "";
	[ParameterFile(Filter = ".ogg|.wav|.mp3")] public string gameplayMusicPath = "";
	public float musicGain = 0.7f;
	public float environmentGain = 0.7f;
	public string serverBaseUrl = "http://localhost:5416";
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
	private WidgetHBox achievementsRow;
	private WidgetHBox[] achievementItems, rankRows;
	private WidgetScrollBox achievementsScroll;
	private WidgetLabel mainStatus, loginStatus, volumeLabel, sfxVolumeLabel, profileName, profileExperience, profileXp, profileEmployeeId, achievementsEmpty;
	private WidgetLabel settingsTitle, musicTitle, effectsTitle;
	private WidgetLabel loyaltyLabel, safetyLabel, complianceLabel;
	private WidgetLabel[] rankPlaces, rankNames, rankLogins, rankExperienceLabels;
	private WidgetEditLine loginEdit, passwordEdit, confirmPasswordEdit;
	private WidgetSlider volumeSlider, sfxVolumeSlider;
	private readonly List<WidgetSprite> mainButtonSprites = new List<WidgetSprite>();
	private readonly List<WidgetSprite> allButtonSprites = new List<WidgetSprite>();
	private readonly List<Image> loadedImages = new List<Image>();
	private readonly List<WidgetLayout> layouts = new List<WidgetLayout>();
	private readonly HashSet<string> earnedAchievements = new HashSet<string>();
	private readonly HashSet<string> completedScenarios = new HashSet<string>();
	private readonly Queue<string> pendingAchievements = new Queue<string>();
	private readonly Queue<ScenarioCompletionRequest> pendingScenarioCompletions = new Queue<ScenarioCompletionRequest>();
	private int lastGuiWidth, lastGuiHeight;
	private int lastMainTextureSignature;
	private int leaderboardMinExperience, leaderboardMaxExperience;
	private Screen state = Screen.Main, returnState = Screen.Main;
	private LoginTarget loginTarget;
	private AmbientSource menuMusic, gameplayMusic;
	private GameFlowController subscribedGameFlow;
	private StoryStateController subscribedStoryState;
	private bool authenticated, changingVolume, pauseWasDown, profileWasDown, loadRequested, duplicate;
	private bool statisticsLoaded;
	private string authenticationToken = "", authenticatedLogin = "";
	private Task<AuthResponse> authenticationTask;
	private Task<AuthResponse> statisticsTask;
	private Task<AuthResponse> outcomeTask;
	private Task<AuthResponse> achievementTask;
	private Task<AuthResponse> scenarioCompletionTask;
	private Task<AuthResponse> leaderboardTask;
	private OutcomeRequest activeOutcome;
	private string activeAchievement = "";
	private ScenarioCompletionRequest activeScenarioCompletion;
	private readonly Queue<OutcomeRequest> pendingOutcomes = new Queue<OutcomeRequest>();
	private LeaderboardEntry[] leaderboardEntries = Array.Empty<LeaderboardEntry>();
	private string currentRunId = "";
	private bool leaderboardRefreshRequested;
	private float savedGameScale = 1.0f, savedPhysicsScale = 1.0f;
	private Input.MOUSE_HANDLE savedMouseHandle;
	private bool savedMouseGrab, savedCursorHide;
	private static bool gameplayStarted;
	public static bool BriefingRequestedForCurrentRun { get; private set; }
	public static bool ScenarioOneCompletedForTesting => instance != null && instance.scenarioOneCompletedForTesting;
	public static bool IsGameplayScreen => instance == null || instance.state == Screen.Game;
	private static MenuManager instance;
	public static bool SkipOpeningDialogueForTesting => instance != null && instance.skipOpeningDialogueForTesting;
	private const float ReferenceWidth = 1600.0f;
	private const float ReferenceHeight = 900.0f;
	private const int EnvironmentSoundChannel = 0;
	private const int EnvironmentSoundMask = 1 << EnvironmentSoundChannel;
	private const int MenuMusicChannel = 1;
	private const int MenuMusicMask = 1 << MenuMusicChannel;
	private const float MenuMusicVolumeScale = 0.3f;
	private static readonly HttpClient AuthHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10.0) };
	public static string AuthenticationToken => instance?.authenticationToken ?? "";
	private static readonly string[] AchievementIds = {
		"ACH_PET_01", "ACH_PET_02", "ACH_PET_03", "ACH_PET_04", "ACH_PET_05", "ACH_PET_06",
		"ACH_NOISE_01", "ACH_NOISE_02", "ACH_NOISE_03", "ACH_NOISE_04", "ACH_NOISE_05", "ACH_NOISE_06", "ACH_NOISE_07"
	};

	private sealed class RegisterRequest
	{
		[JsonPropertyName("login")] public string Login { get; set; }
		[JsonPropertyName("password")] public string Password { get; set; }
		[JsonPropertyName("name")] public string Name { get; set; }
	}

	private sealed class LoginRequest
	{
		[JsonPropertyName("login")] public string Login { get; set; }
		[JsonPropertyName("password")] public string Password { get; set; }
	}

	private sealed class AuthResponse
	{
		public bool Success;
		public HttpStatusCode StatusCode;
		public string Body = "";
		public string Error = "";
	}

	private sealed class StatsResponse
	{
		[JsonPropertyName("name")] public string Name { get; set; }
		[JsonPropertyName("login")] public string Login { get; set; }
		[JsonPropertyName("safety")] public int Safety { get; set; }
		[JsonPropertyName("loyality")] public int Loyalty { get; set; }
		[JsonPropertyName("experience")] public int Experience { get; set; }
		[JsonPropertyName("achievements")] public string[] Achievements { get; set; }
	}

	private sealed class OutcomeRequest
	{
		[JsonPropertyName("reason")] public string Reason { get; set; }
		[JsonPropertyName("safety")] public int Safety { get; set; }
		[JsonPropertyName("loyality")] public int Loyalty { get; set; }
	}

	private sealed class AchievementRequest
	{
		[JsonPropertyName("id")] public string Id { get; set; }
	}

	private sealed class ScenarioCompletionRequest
	{
		[JsonPropertyName("runId")] public string RunId { get; set; }
		[JsonPropertyName("scenarioId")] public string ScenarioId { get; set; }
	}

	private sealed class LeaderboardEntry
	{
		[JsonPropertyName("name")] public string Name { get; set; }
		[JsonPropertyName("login")] public string Login { get; set; }
		[JsonPropertyName("experience")] public int Experience { get; set; }
		[JsonPropertyName("achievements")] public int Achievements { get; set; }
		[JsonPropertyName("safety")] public int Safety { get; set; }
		[JsonPropertyName("loyality")] public int Loyalty { get; set; }
	}

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
		ProcessAuthenticationResult();
		ProcessStatisticsResult();
		UpdateGameFlowSubscription();
		UpdateStoryStateSubscription();
		ProcessOutcomeQueue();
		ProcessAchievementQueue();
		ProcessScenarioCompletionQueue();
		ProcessLeaderboardResult();
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
		if (state == Screen.Game && !BriefingRequestedForCurrentRun && !BriefingWindow.IsIntroductionVisible && !ConductorTerminal.IsVisible
			&& !PassengerScenarioController.IsSeatMonitorViewVisible &&
			(Game.Scale == 0.0f || Physics.Scale == 0.0f)) ResumeScales();
		if (!PassengerScenarioController.IsSeatMonitorViewVisible)
		{
			if (state == Screen.Game && !BriefingRequestedForCurrentRun && !BriefingWindow.IsVisible && !DialogueWindow.IsVisible
				&& !ConductorTerminal.IsVisible && !ScenarioResults.IsVisible)
				GrabMouse();
			else
				ReleaseMouse();
		}
	}

	private void Shutdown()
	{
		if (duplicate) return;
		authenticationTask = null;
		statisticsTask = null;
		outcomeTask = null;
		achievementTask = null;
		scenarioCompletionTask = null;
		leaderboardTask = null;
		pendingOutcomes.Clear();
		pendingAchievements.Clear();
		pendingScenarioCompletions.Clear();
		if (subscribedGameFlow != null)
			subscribedGameFlow.ReturnToMenuRequested -= ReturnToMainMenuAfterRun;
		if (subscribedStoryState != null)
		{
			subscribedStoryState.MetricChanged -= OnStoryMetricChanged;
			subscribedStoryState.OutcomeApplied -= OnStoryOutcomeApplied;
		}
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
		achievementItems = new WidgetHBox[AchievementIds.Length];
		for (int i = 0; i < AchievementIds.Length; i++)
			achievementItems[i] = W<WidgetHBox>("achievement_" + AchievementIds[i]);
		rankRows = new WidgetHBox[] { W<WidgetHBox>("rank_row_1"), W<WidgetHBox>("rank_row_2"), W<WidgetHBox>("rank_row_3") };
		rankPlaces = new WidgetLabel[] { W<WidgetLabel>("rank_place_1"), W<WidgetLabel>("rank_place_2"), W<WidgetLabel>("rank_place_3") };
		rankNames = new WidgetLabel[] { W<WidgetLabel>("rank_name_1"), W<WidgetLabel>("rank_name_2"), W<WidgetLabel>("rank_name_3") };
		rankLogins = new WidgetLabel[] { W<WidgetLabel>("rank_login_1"), W<WidgetLabel>("rank_login_2"), W<WidgetLabel>("rank_login_3") };
		rankExperienceLabels = new WidgetLabel[] { W<WidgetLabel>("rank_xp_1"), W<WidgetLabel>("rank_xp_2"), W<WidgetLabel>("rank_xp_3") };
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
			achievementsScroll == null || achievementsRow == null || ContainsNull(achievementItems) || ContainsNull(rankRows) ||
			ContainsNull(rankPlaces) || ContainsNull(rankNames) || ContainsNull(rankLogins) || ContainsNull(rankExperienceLabels) ||
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
	private static bool ContainsNull<T>(T[] widgets) where T : Widget
	{
		if (widgets == null) return true;
		foreach (T widget in widgets)
			if (widget == null) return true;
		return false;
	}
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
		ResizeLoginButtons(scale);
		ResizeProfileMetrics(scale);
		ResizeMainButtons();
		mainScreen.Arrange(); pauseScreen.Arrange(); loginScreen.Arrange(); settingsScreen.Arrange(); profileScreen.Arrange();
		PositionScreens();
	}

	private void ResizeProfileMetrics(float scale)
	{
		Widget loyaltyRow = loyaltyLabel.Parent?.Parent;
		Widget safetyRow = safetyLabel.Parent?.Parent;
		Widget complianceRow = complianceLabel.Parent?.Parent;
		if (loyaltyRow == null || safetyRow == null || complianceRow == null
			|| profileMetricsPanel.NumChildren == 0) return;

		complianceRow.Hidden = true;
		Widget title = profileMetricsPanel.GetChild(0);
		// Place the remaining rows explicitly inside the panel, below its title.
		profileMetricsPanel.SetPadding(0, 0, 0, 0);
		title.Flags = loyaltyRow.Flags = safetyRow.Flags = Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED;
		int margin = Math.Max(1, (int)Math.Round(18 * scale));
		int gap = Math.Max(1, (int)Math.Round(16 * scale));
		title.SetPosition(Math.Max(0, (profileMetricsPanel.Width - title.Width) / 2), margin);
		int contentTop = margin + title.Height + Math.Max(1, (int)Math.Round(12 * scale));
		int rowsHeight = loyaltyRow.Height + gap + safetyRow.Height;
		int firstRowY = contentTop + Math.Max(0,
			(profileMetricsPanel.Height - margin - contentTop - rowsHeight) / 2);
		loyaltyRow.SetPosition(Math.Max(0, (profileMetricsPanel.Width - loyaltyRow.Width) / 2), firstRowY);
		safetyRow.SetPosition(Math.Max(0, (profileMetricsPanel.Width - safetyRow.Width) / 2),
			firstRowY + loyaltyRow.Height + gap);
	}

	private void ResizeBackButtons(float scale)
	{
		WidgetSprite settingsBack = W<WidgetSprite>("settings_back_button");
		WidgetSprite profileBack = W<WidgetSprite>("profile_back_button");
		if (settingsBack != null) { settingsBack.Width = Math.Max(1, (int)(320 * scale)); settingsBack.Height = Math.Max(1, (int)(67 * scale)); }
		if (profileBack != null) { profileBack.Width = Math.Max(1, (int)(260 * scale)); profileBack.Height = Math.Max(1, (int)(55 * scale)); }
	}

	private void ResizeLoginButtons(float scale)
	{
		WidgetSprite login = W<WidgetSprite>("login_button");
		WidgetSprite back = W<WidgetSprite>("login_back_button");
		if (login == null || back == null) return;
		int textureWidth = login.GetLayerWidth(0);
		int frameHeight = login.GetLayerHeight(0) / 2;
		if (textureWidth <= 0 || frameHeight <= 0) return;

		int buttonWidth = Math.Min(loginScreen.Width, Math.Max(1, (int)Math.Round(textureWidth * scale * 1.1f)));
		int buttonHeight = Math.Max(1, (int)Math.Round((float)buttonWidth * frameHeight / textureWidth));
		login.Width = back.Width = buttonWidth;
		login.Height = back.Height = buttonHeight;
		ResizeLoginButtonRow(login.Parent, login, buttonWidth, buttonHeight);
		Widget backSlot = back.Parent;
		if (backSlot != null)
			ResizeLoginButtonRow(backSlot.Parent, backSlot, buttonWidth, buttonHeight);
	}

	private void ResizeLoginButtonRow(Widget row, Widget slot, int buttonWidth, int buttonHeight)
	{
		if (row == null || slot == null || row.NumChildren != 3) return;
		int rowWidth = loginScreen.Width;
		int sideWidth = Math.Max(0, (rowWidth - buttonWidth) / 2);
		row.Width = rowWidth;
		row.Height = buttonHeight;
		slot.Width = buttonWidth;
		slot.Height = buttonHeight;
		row.GetChild(0).Width = sideWidth;
		row.GetChild(2).Width = Math.Max(0, rowWidth - buttonWidth - sideWidth);
		row.GetChild(0).Height = row.GetChild(2).Height = buttonHeight;
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
		SetExperienceProgress(progressBackgrounds[0], progressFills[0], employeeExperience);
		SetProgress(progressBackgrounds[1], progressFills[1], passengerLoyalty, 100);
		SetProgress(progressBackgrounds[2], progressFills[2], safetyRating, 100);
		SetProgress(progressBackgrounds[3], progressFills[3], regulationCompliance, 100);
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
	private void SetExperienceProgress(WidgetSprite backgroundSprite, WidgetSprite fillSprite, int experience)
	{
		float progress;
		if (leaderboardMaxExperience <= leaderboardMinExperience)
			progress = leaderboardEntries.Length == 0 ? 0.0f : 1.0f;
		else
			progress = (experience - leaderboardMinExperience) / (float)(leaderboardMaxExperience - leaderboardMinExperience);
		SetProgress(backgroundSprite, fillSprite, (int)Math.Round(Math.Max(0.0f, Math.Min(1.0f, progress)) * 1000.0f), 1000);
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
		int achievementCount = earnedAchievements.Count;
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
		string employeeId = string.IsNullOrWhiteSpace(authenticatedLogin)
			? (string.IsNullOrWhiteSpace(loginEdit.Text) ? "DEMO" : loginEdit.Text.Trim())
			: authenticatedLogin;
		profileEmployeeId.Text = "ID сотрудника: " + employeeId;
		loyaltyLabel.Text = loyalty + "%";
		safetyLabel.Text = safety + "%";
		complianceLabel.Text = compliance + "%";
		UpdateProfileProgressBars();
		int achievementCount = earnedAchievements.Count;
		for (int i = 0; i < achievementItems.Length; i++)
			achievementItems[i].Hidden = !earnedAchievements.Contains(AchievementIds[i]);
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
		currentRunId = Guid.NewGuid().ToString("N");
		completedScenarios.Clear();
		gameplayStarted = true; loadRequested = true; BriefingRequestedForCurrentRun = !skipOpeningDialogueForTesting; ShowGame();
		if (BriefingRequestedForCurrentRun) PauseScales();
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
		if (authenticationTask != null || statisticsTask != null) return;

		string login = loginEdit.Text.Trim();
		string password = passwordEdit.Text;
		if (string.IsNullOrWhiteSpace(login)) { loginStatus.Text = "Введите табельный номер"; return; }
		if (string.IsNullOrEmpty(password)) { loginStatus.Text = "Введите PIN-код"; return; }
		if (password != confirmPasswordEdit.Text) { loginStatus.Text = "PIN-коды не совпадают"; return; }
		if (string.IsNullOrWhiteSpace(serverBaseUrl)) { loginStatus.Text = "Не указан адрес сервера"; return; }

		string name = string.IsNullOrWhiteSpace(playerName) ? login : playerName.Trim();
		loginStatus.Text = "Подключение к серверу...";
		authenticationTask = RegisterOrLoginAsync(login, password, name);
	}

	private async Task<AuthResponse> RegisterOrLoginAsync(string login, string password, string name)
	{
		AuthResponse registration = await PostJsonAsync("/register", new RegisterRequest
		{
			Login = login,
			Password = password,
			Name = name
		});

		if (registration.Success || registration.StatusCode != HttpStatusCode.Forbidden)
			return registration;

		return await PostJsonAsync("/login", new LoginRequest
		{
			Login = login,
			Password = password
		});
	}

	private async Task<AuthResponse> PostJsonAsync(string path, object payload)
	{
		try
		{
			string url = serverBaseUrl.TrimEnd('/') + path;
			string json = JsonSerializer.Serialize(payload);
			using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
			using HttpResponseMessage response = await AuthHttpClient.PostAsync(url, content);
			return new AuthResponse
			{
				Success = response.IsSuccessStatusCode,
				StatusCode = response.StatusCode,
				Body = await response.Content.ReadAsStringAsync()
			};
		}
		catch (Exception exception)
		{
			return new AuthResponse { Error = exception.Message };
		}
	}

	private async Task<AuthResponse> GetStatisticsAsync(string token)
	{
		try
		{
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, serverBaseUrl.TrimEnd('/') + "/stats");
			request.Headers.Add("token", token);
			using HttpResponseMessage response = await AuthHttpClient.SendAsync(request);
			return new AuthResponse
			{
				Success = response.IsSuccessStatusCode,
				StatusCode = response.StatusCode,
				Body = await response.Content.ReadAsStringAsync()
			};
		}
		catch (Exception exception)
		{
			return new AuthResponse { Error = exception.Message };
		}
	}

	private async Task<AuthResponse> PostOutcomeAsync(OutcomeRequest outcome)
	{
		try
		{
			string json = JsonSerializer.Serialize(outcome);
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serverBaseUrl.TrimEnd('/') + "/actOutcome");
			request.Headers.Add("token", authenticationToken);
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");
			using HttpResponseMessage response = await AuthHttpClient.SendAsync(request);
			return new AuthResponse
			{
				Success = response.IsSuccessStatusCode,
				StatusCode = response.StatusCode,
				Body = await response.Content.ReadAsStringAsync()
			};
		}
		catch (Exception exception)
		{
			return new AuthResponse { Error = exception.Message };
		}
	}

	private async Task<AuthResponse> PostAuthorizedJsonAsync(string path, object payload)
	{
		try
		{
			string json = JsonSerializer.Serialize(payload);
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serverBaseUrl.TrimEnd('/') + path);
			request.Headers.Add("token", authenticationToken);
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");
			using HttpResponseMessage response = await AuthHttpClient.SendAsync(request);
			return new AuthResponse
			{
				Success = response.IsSuccessStatusCode,
				StatusCode = response.StatusCode,
				Body = await response.Content.ReadAsStringAsync()
			};
		}
		catch (Exception exception)
		{
			return new AuthResponse { Error = exception.Message };
		}
	}

	private async Task<AuthResponse> GetLeaderboardAsync()
	{
		try
		{
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, serverBaseUrl.TrimEnd('/') + "/leaderboard");
			request.Headers.Add("token", authenticationToken);
			using HttpResponseMessage response = await AuthHttpClient.SendAsync(request);
			return new AuthResponse
			{
				Success = response.IsSuccessStatusCode,
				StatusCode = response.StatusCode,
				Body = await response.Content.ReadAsStringAsync()
			};
		}
		catch (Exception exception)
		{
			return new AuthResponse { Error = exception.Message };
		}
	}

	private void ProcessAuthenticationResult()
	{
		if (authenticationTask == null || !authenticationTask.IsCompleted) return;

		AuthResponse response = authenticationTask.GetAwaiter().GetResult();
		authenticationTask = null;
		if (state != Screen.Login) return;

		if (!response.Success)
		{
			if (!string.IsNullOrWhiteSpace(response.Error))
			{
				loginStatus.Text = "Сервер недоступен";
				Log.Error("MenuManager: authentication request failed: {0}\n", response.Error);
			}
			else if (response.StatusCode == HttpStatusCode.Forbidden)
				loginStatus.Text = "Пользователь уже существует, но PIN-код неверен";
			else if (response.StatusCode == HttpStatusCode.BadRequest)
				loginStatus.Text = "Сервер отклонил введённые данные";
			else
				loginStatus.Text = "Ошибка сервера: " + (int)response.StatusCode;
			return;
		}

		authenticationToken = response.Body.Trim();
		if (string.IsNullOrEmpty(authenticationToken))
		{
			loginStatus.Text = "Сервер не вернул токен доступа";
			return;
		}

		loginStatus.Text = "Загрузка статистики...";
		statisticsTask = GetStatisticsAsync(authenticationToken);
	}

	private void ProcessStatisticsResult()
	{
		if (statisticsTask == null || !statisticsTask.IsCompleted) return;

		AuthResponse response = statisticsTask.GetAwaiter().GetResult();
		statisticsTask = null;
		if (state != Screen.Login) return;

		if (!response.Success)
		{
			loginStatus.Text = string.IsNullOrWhiteSpace(response.Error)
				? "Не удалось загрузить статистику: " + (int)response.StatusCode
				: "Сервер статистики недоступен";
			Log.Error("MenuManager: statistics request failed: {0}\n",
				string.IsNullOrWhiteSpace(response.Error) ? response.Body : response.Error);
			return;
		}

		try
		{
			StatsResponse stats = JsonSerializer.Deserialize<StatsResponse>(response.Body);
			if (stats == null) throw new JsonException("Empty statistics response.");
			playerName = string.IsNullOrWhiteSpace(stats.Name) ? playerName : stats.Name;
			authenticatedLogin = stats.Login ?? "";
			employeeExperience = Math.Max(0, stats.Experience);
			safetyRating = Math.Max(0, Math.Min(100, stats.Safety));
			passengerLoyalty = Math.Max(0, Math.Min(100, stats.Loyalty));
			earnedAchievements.Clear();
			if (stats.Achievements != null)
				foreach (string id in stats.Achievements)
					if (IsKnownAchievement(id)) earnedAchievements.Add(id);
			statisticsLoaded = true;
			subscribedStoryState?.LoadPersistedMetrics(safetyRating, passengerLoyalty);
			UpdateProfileData();
		}
		catch (Exception exception)
		{
			loginStatus.Text = "Сервер вернул неверную статистику";
			Log.Error("MenuManager: can't parse statistics: {0}\n", exception.Message);
			return;
		}

		authenticated = true;
		RequestLeaderboardRefresh();
		loginStatus.Text = "";
		if (loginTarget == LoginTarget.Start) StartGame();
		else { Screen origin = returnState; ShowProfile(); returnState = origin; }
	}
	private void ShowSettings() { returnState = state; SetState(Screen.Settings); }
	private void ShowProfile() { if (state == Screen.Profile) return; returnState = state; RequestLeaderboardRefresh(); UpdateProfileData(); SetState(Screen.Profile); if (returnState == Screen.Game) PauseScales(); }
	private void BackFromLogin() { if (returnState == Screen.Pause) ShowPause(); else ShowMain(); }
	private void BackFromSettings() { if (returnState == Screen.Pause) ShowPause(); else ShowMain(); }
	private void BackFromProfile() { if (returnState == Screen.Game) ShowGame(); else if (returnState == Screen.Pause) ShowPause(); else ShowMain(); }

	private void ChangeMenuVolume()
	{
		if (volumeSlider == null || changingVolume) return;
		changingVolume = true; int value = SnapVolume(volumeSlider.Value); volumeSlider.Value = value;
		musicGain = value / 100.0f;
		Sound.SetSourceVolume(MenuMusicChannel, musicGain * MenuMusicVolumeScale);
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

	private void UpdateStoryStateSubscription()
	{
		StoryStateController current = ComponentSystem.FindComponentInWorld<StoryStateController>();
		if (current == subscribedStoryState) return;

		if (subscribedStoryState != null)
		{
			subscribedStoryState.MetricChanged -= OnStoryMetricChanged;
			subscribedStoryState.OutcomeApplied -= OnStoryOutcomeApplied;
		}

		subscribedStoryState = current;
		if (subscribedStoryState == null) return;
		subscribedStoryState.MetricChanged += OnStoryMetricChanged;
		subscribedStoryState.OutcomeApplied += OnStoryOutcomeApplied;
		if (statisticsLoaded)
			subscribedStoryState.LoadPersistedMetrics(safetyRating, passengerLoyalty);
	}

	private void OnStoryMetricChanged(string metric, int oldValue, int newValue)
	{
		if (metric == "safety") safetyRating = newValue;
		else if (metric == "loyalty") passengerLoyalty = newValue;
		else return;
		UpdateProfileData();
		foreach (WidgetHBox row in rankRows) row.Hidden = true;
	}

	private void OnStoryOutcomeApplied(string reason, int safety, int loyalty)
	{
		if (!authenticated || string.IsNullOrEmpty(authenticationToken)) return;
		pendingOutcomes.Enqueue(new OutcomeRequest
		{
			Reason = string.IsNullOrWhiteSpace(reason) ? "gameplay" : reason,
			Safety = safety,
			Loyalty = loyalty
		});
	}

	public static bool UnlockAchievement(string id)
	{
		if (instance == null || !IsKnownAchievement(id) || !instance.earnedAchievements.Add(id)) return false;
		instance.employeeExperience += IsHiddenAchievement(id) ? 1500 : 1000;
		instance.UpdateProfileData();
		if (instance.authenticated && !string.IsNullOrEmpty(instance.authenticationToken))
			instance.pendingAchievements.Enqueue(id);
		return true;
	}

	public static void CompleteScenario(string scenarioId)
	{
		if (instance == null || string.IsNullOrEmpty(instance.currentRunId) ||
			(scenarioId != "pet" && scenarioId != "noise") || !instance.completedScenarios.Add(scenarioId)) return;
		instance.employeeExperience += 500;
		instance.UpdateProfileData();
		if (instance.authenticated && !string.IsNullOrEmpty(instance.authenticationToken))
			instance.pendingScenarioCompletions.Enqueue(new ScenarioCompletionRequest
			{
				RunId = instance.currentRunId,
				ScenarioId = scenarioId
			});
	}

	private static bool IsKnownAchievement(string id)
	{
		foreach (string knownId in AchievementIds)
			if (knownId == id) return true;
		return false;
	}

	private static bool IsHiddenAchievement(string id)
	{
		return id == "ACH_PET_05" || id == "ACH_PET_06" || id == "ACH_NOISE_05" || id == "ACH_NOISE_07";
	}

	private void ProcessOutcomeQueue()
	{
		if (outcomeTask != null && outcomeTask.IsCompleted)
		{
			AuthResponse response = outcomeTask.GetAwaiter().GetResult();
			if (!response.Success)
				Log.Error("MenuManager: failed to save outcome '{0}': {1}\n", activeOutcome?.Reason ?? "",
					string.IsNullOrWhiteSpace(response.Error) ? ((int)response.StatusCode).ToString() : response.Error);
			outcomeTask = null;
			activeOutcome = null;
		}

		if (outcomeTask != null || pendingOutcomes.Count == 0 || string.IsNullOrEmpty(authenticationToken)) return;
		activeOutcome = pendingOutcomes.Dequeue();
		outcomeTask = PostOutcomeAsync(activeOutcome);
	}

	private void ProcessAchievementQueue()
	{
		if (achievementTask != null && achievementTask.IsCompleted)
		{
			AuthResponse response = achievementTask.GetAwaiter().GetResult();
			if (!response.Success)
				Log.Error("MenuManager: failed to save achievement '{0}': {1}\n", activeAchievement,
					string.IsNullOrWhiteSpace(response.Error) ? ((int)response.StatusCode).ToString() : response.Error);
			else leaderboardRefreshRequested = true;
			achievementTask = null;
			activeAchievement = "";
		}

		if (achievementTask != null || pendingAchievements.Count == 0 || string.IsNullOrEmpty(authenticationToken)) return;
		activeAchievement = pendingAchievements.Dequeue();
		achievementTask = PostAuthorizedJsonAsync("/achievement", new AchievementRequest { Id = activeAchievement });
	}

	private void ProcessScenarioCompletionQueue()
	{
		if (scenarioCompletionTask != null && scenarioCompletionTask.IsCompleted)
		{
			AuthResponse response = scenarioCompletionTask.GetAwaiter().GetResult();
			if (!response.Success)
				Log.Error("MenuManager: failed to save scenario completion '{0}': {1}\n", activeScenarioCompletion?.ScenarioId ?? "",
					string.IsNullOrWhiteSpace(response.Error) ? ((int)response.StatusCode).ToString() : response.Error);
			else leaderboardRefreshRequested = true;
			scenarioCompletionTask = null;
			activeScenarioCompletion = null;
		}

		if (scenarioCompletionTask != null || pendingScenarioCompletions.Count == 0 || string.IsNullOrEmpty(authenticationToken)) return;
		activeScenarioCompletion = pendingScenarioCompletions.Dequeue();
		scenarioCompletionTask = PostAuthorizedJsonAsync("/scenarioComplete", activeScenarioCompletion);
	}

	private void RequestLeaderboardRefresh()
	{
		if (!authenticated || string.IsNullOrEmpty(authenticationToken)) return;
		if (leaderboardTask != null) { leaderboardRefreshRequested = true; return; }
		leaderboardRefreshRequested = false;
		leaderboardTask = GetLeaderboardAsync();
	}

	private void ProcessLeaderboardResult()
	{
		if (leaderboardTask != null && leaderboardTask.IsCompleted)
		{
			AuthResponse response = leaderboardTask.GetAwaiter().GetResult();
			leaderboardTask = null;
			if (!response.Success)
				Log.Error("MenuManager: failed to load leaderboard: {0}\n",
					string.IsNullOrWhiteSpace(response.Error) ? ((int)response.StatusCode).ToString() : response.Error);
			else
			{
				try
				{
					leaderboardEntries = JsonSerializer.Deserialize<LeaderboardEntry[]>(response.Body) ?? Array.Empty<LeaderboardEntry>();
					UpdateLeaderboardData();
				}
				catch (Exception exception)
				{
					Log.Error("MenuManager: can't parse leaderboard: {0}\n", exception.Message);
				}
			}
		}

		if (leaderboardRefreshRequested && leaderboardTask == null)
			RequestLeaderboardRefresh();
	}

	private void UpdateLeaderboardData()
	{
		leaderboardMinExperience = 0;
		leaderboardMaxExperience = 0;
		if (leaderboardEntries.Length > 0)
		{
			leaderboardMinExperience = leaderboardEntries[0].Experience;
			leaderboardMaxExperience = leaderboardEntries[0].Experience;
			foreach (LeaderboardEntry entry in leaderboardEntries)
			{
				leaderboardMinExperience = Math.Min(leaderboardMinExperience, entry.Experience);
				leaderboardMaxExperience = Math.Max(leaderboardMaxExperience, entry.Experience);
			}
		}

		for (int i = 0; i < rankRows.Length; i++)
		{
			bool hasEntry = i < leaderboardEntries.Length;
			rankRows[i].Hidden = !hasEntry;
			if (!hasEntry) continue;
			LeaderboardEntry entry = leaderboardEntries[i];
			rankPlaces[i].Text = (i + 1).ToString();
			rankNames[i].Text = entry.Name ?? "";
			rankLogins[i].Text = entry.Login ?? "";
			rankExperienceLabels[i].Text = entry.Experience.ToString("N0").Replace(',', ' ');
			SetExperienceProgress(progressBackgrounds[4 + i], progressFills[4 + i], entry.Experience);
		}
		UpdateProfileProgressBars();
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
			foreach (WidgetHBox item in achievementItems) item.Hidden = true;
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
		Sound.SetSourceVolume(MenuMusicChannel, musicGain * MenuMusicVolumeScale);
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
