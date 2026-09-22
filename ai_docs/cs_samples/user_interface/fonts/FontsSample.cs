// Demonstrates multi-script text rendering (HarfBuzz shaping) across GUI widgets, scene
// ObjectText nodes and a world-space ObjectGui, using six languages (EN/RU/AR/ZH/TH/HI).
// RTL and complex scripts rely on the font fallback chain set with SetGlobalFontFallback();
// the Noto/Hind .ttf files must be present in the data folder or glyphs render as "tofu".

using System.Collections.Generic;
using Unigine;

// UI controller for the multi-script font demonstration.
public partial class FontsSample : Component
{
	// One language entry: ISO tag plus plain, paragraph (for wrapping) and rich (markup) variants.
	private struct LangSample
	{
		public string code;
		public string plain;
		public string paragraph;
		public string rich;
		public LangSample(string code, string plain, string paragraph, string rich)
		{
			this.code = code;
			this.plain = plain;
			this.paragraph = paragraph;
			this.rich = rich;
		}
	}

	private struct TextSample
	{
		public string label;
		public string text;
		public TextSample(string label, string text)
		{
			this.label = label;
			this.text = text;
		}
	}

	// ObjectText nodes, one per language (SAMPLES order), plus the world-space ObjectGui.
	[ShowInEditor, Parameter(Title = "Text EN (ObjectText)")]
	public Node TextEn = null;
	[ShowInEditor, Parameter(Title = "Text RU (ObjectText)")]
	public Node TextRu = null;
	[ShowInEditor, Parameter(Title = "Text AR (ObjectText)")]
	public Node TextAr = null;
	[ShowInEditor, Parameter(Title = "Text ZH (ObjectText)")]
	public Node TextZh = null;
	[ShowInEditor, Parameter(Title = "Text TH (ObjectText)")]
	public Node TextTh = null;
	[ShowInEditor, Parameter(Title = "Text HI (ObjectText)")]
	public Node TextHi = null;
	[ShowInEditor, Parameter(Title = "World GUI object (ObjectGui)")]
	public Node GuiObject = null;

	[ShowInEditor, ParameterFile(Filter = ".ttf")]
	public string FontLatin = "";
	[ShowInEditor, ParameterFile(Filter = ".ttf")]
	public string FontArabic = "";
	[ShowInEditor, ParameterFile(Filter = ".ttf")]
	public string FontChinese = "";
	[ShowInEditor, ParameterFile(Filter = ".ttf")]
	public string FontThai = "";
	[ShowInEditor, ParameterFile(Filter = ".ttf")]
	public string FontHindi = "";

	// Order is fixed and shared with FontForLang() below.
	private static readonly LangSample[] SAMPLES = {
		new LangSample("EN", "Hello, world!",
			"The quick brown fox jumps over the lazy dog near the river bank "
			+ "where the trees grow tall and the wind whispers through the leaves.",
			"Hello <b>bold</b> <i>italic</i> <font color=#ff0000>red</font> <font color=#00cc00>green</font> world!"),
		new LangSample("RU", "Привет, мир!",
			"Съешь же ещё этих мягких французских булок, да выпей чаю — "
			+ "холодного или горячего, как тебе угодно, у самовара на старой даче.",
			"Привет <b>жирный</b> <i>курсив</i> <font color=#ff0000>красный</font> <font color=#00cc00>зелёный</font> мир!"),
		new LangSample("AR", "مرحبا بالعالم",
			"في عام 2024 بدأت العديد من الشركات التقنية باستخدام أدوات "
			+ "تعتمد على الذكاء الاصطناعي لتحسين تجربة المستخدم بشكل ملحوظ.",
			"مرحبا <b>بالعالم</b> <font color=#ff0000>عربي</font> <font color=#00cc00>نص</font> RTL"),
		new LangSample("ZH", "你好，世界!",
			"敏捷的棕色狐狸跳过懒狗。在长江两岸的城市里，许多公司正在使用"
			+ "人工智能技术来改善用户在现代应用程序中的体验。",
			"你好 <b>粗体</b> <font color=#ff0000>红色</font> <font color=#00cc00>绿色</font> 世界!"),
		new LangSample("TH", "สวัสดีชาวโลก",
			"สุนัขจิ้งจอกสีน้ำตาลที่ว่องไวกระโดดข้ามสุนัขขี้เกียจ "
			+ "ในสวนที่สวยงามใกล้แม่น้ำซึ่งต้นไม้สูงและลมพัดเบาๆ",
			"สวัสดี <b>หนา</b> <font color=#ff0000>แดง</font> <font color=#00cc00>เขียว</font> ชาวโลก"),
		new LangSample("HI", "नमस्ते दुनिया",
			"तेज़ भूरी लोमड़ी आलसी कुत्ते के ऊपर कूदती है। नदी के पास "
			+ "के पेड़ बहुत लंबे हैं और शाम भर हवा धीरे-धीरे बहती रहती है।",
			"नमस्ते <b>मोटा</b> <font color=#ff0000>लाल</font> <font color=#00cc00>हरा</font> दुनिया"),
	};
	private const int NUM_SAMPLES = 6;

	// Mixed-direction (bidirectional) strings: digits, Latin and RTL runs in one line.
	private static readonly TextSample[] BIDI_SAMPLES = {
		new TextSample("AR+EN+Num", "المستخدم John Smith لديه 3 رسائل جديدة"),
		new TextSample("EN+AR",     "The document title is \"تقرير سنوي\" and it has 42 pages"),
		new TextSample("AR+ZH",     "النسخة الصينية: 你好世界 متاحة الآن"),
		new TextSample("Nested",    "Start مرحبا Hello again عودة end"),
	};
	private const int NUM_BIDI = 4;

	// Accent color for section headers in the test panel.
	private const string HEADER_COLOR = "#7fbfff";

	// Column widths for the test-panel tables.
	private const int COL_TAG = 50, COL_LBL = 230, COL_BTN = 150, COL_EDIT = 230, COL_ET = 230, COL_ET_H = 32;
	private const int COL_PAD = 6, COL_PADY = 3;

	private SampleDescriptionWindow descriptionWindow = new SampleDescriptionWindow();
	private WidgetWindow featuresWindow;
	// Every panel widget, kept so font size can be applied to all of them.
	private List<Widget> widgets = new List<Widget>();

	// Resolved ObjectGui node and its GUI, plus the content root for rebuilds.
	private ObjectGui objectGui;
	private Gui objectGuiGui;
	private WidgetVBox objectGuiRoot;

	private int fontSize = 16;
	private Gui.CursorMode cursorMode = Gui.CursorMode.CURSOR_MODE_AUTO;

	// The parameter window, the test panel and the scene nodes are set up here.
	void Init()
	{
		InstallFontFallback();

		descriptionWindow.createWindow(Gui.ALIGN_LEFT | Gui.ALIGN_TOP);

		// Global text direction: Auto picks LTR/RTL from the first strong character of each string,
		// while LTR/RTL force a single block direction for every widget at once.
		descriptionWindow.addSwitchParameter("Text direction",
			"Auto detects direction per string; LTR/RTL force one direction globally.",
			0, new string[] { "Auto", "LTR", "RTL" }, (int index) => SetTextDirection(index));

		// Caret movement across bidirectional text: visual follows on-screen position,
		// logical follows character order in memory.
		descriptionWindow.addSwitchParameter("Cursor mode",
			"How the caret steps through bidirectional text.",
			0, new string[] { "Auto", "Visual", "Logical" }, (int index) => SetCursorMode(index));

		descriptionWindow.addIntParameter("GUI font size", "Font size of the feature-panel widgets.",
			fontSize, 12, 32, (int v) => SetFontSizeAll(v));

		descriptionWindow.addParameterSpacer();

		descriptionWindow.addBoolParameter("Show widget font features", "Show the multi-script widget font features window.",
			true, (bool v) => { if (featuresWindow) featuresWindow.Hidden = !v; });

		BuildFeaturePanel();
		SetupObjectTexts();
		SetupObjectGui();
	}

	// The test panel is released; the scene nodes belong to the world and are left untouched.
	void Shutdown()
	{
		widgets.Clear();
		if (featuresWindow)
			featuresWindow.DeleteLater();
		descriptionWindow.shutdown();
	}

	// =========================================================================
	// Controls
	// =========================================================================

	// Lists per-script fonts tried in order when the primary font has no glyph for a character.
	private void InstallFontFallback()
	{
		string[] paths = { FontHindi, FontArabic, FontChinese, FontThai, FontLatin };
		List<string> fonts = new List<string>();
		foreach (string p in paths)
			if (!string.IsNullOrEmpty(p))
				fonts.Add(p);
		Gui.SetGlobalFontFallback(fonts.ToArray());
	}

	private void SetTextDirection(int index)
	{
		// Combo order matches the TextDirection enum (Auto = 0, LTR = 1, RTL = 2).
		Gui.TextDirection dir = (Gui.TextDirection)index;
		Gui.GetCurrent().GlobalTextDirection = dir;
		for (int i = 0; i < NUM_SAMPLES; i++)
		{
			ObjectText text = GetTextObject(i);
			if (text)
				text.TextDirection = dir;
		}
		if (objectGuiGui != null)
			objectGuiGui.GlobalTextDirection = dir;
	}

	private void SetCursorMode(int index)
	{
		cursorMode = (Gui.CursorMode)index;
		Gui.GetCurrent().GlobalCursorMode = cursorMode;
		if (objectGuiGui != null)
			objectGuiGui.GlobalCursorMode = cursorMode;
	}

	private void SetFontSizeAll(int size)
	{
		fontSize = size;
		for (int i = 0; i < widgets.Count; i++)
			if (widgets[i])
				widgets[i].FontSize = fontSize;
	}

	// Test panel (screen-space GUI). Keep() tracks widgets so the font size can be applied to
	// all of them at once.
	private T Keep<T>(T w) where T : Widget
	{
		widgets.Add(w);
		return w;
	}

	// =========================================================================
	// Test panel (screen-space GUI)
	// =========================================================================

	// Creates a new tab holding a content column (with a subtle hint line) and returns that
	// column so a section can be built into it.
	private WidgetVBox BeginTab(WidgetTabBox tabs, string name, string hint)
	{
		tabs.AddTab(name);

		WidgetScrollBox scroll = new WidgetScrollBox();
		scroll.Background = 0;
		scroll.Border = 0;
		scroll.Width = System.Math.Min(900, Gui.GetCurrent().Width - 120);
		scroll.Height = System.Math.Min(420, Gui.GetCurrent().Height - 160);
		scroll.HScrollEnabled = true;
		scroll.VScrollEnabled = true;
		scroll.HScrollHidden = WidgetScrollBox.SCROLL_RENDER_MODE.AUTO_HIDE;
		scroll.VScrollHidden = WidgetScrollBox.SCROLL_RENDER_MODE.AUTO_HIDE;
		tabs.AddChild(scroll, Gui.ALIGN_EXPAND);

		WidgetVBox content = new WidgetVBox(0, 4);
		content.SetPadding(10, 10, 10, 10);
		scroll.AddChild(content, Gui.ALIGN_EXPAND);

		WidgetLabel hintLabel = Keep(new WidgetLabel($"<font color={HEADER_COLOR}><i>{hint}</i></font>"));
		hintLabel.FontRich = 1;
		hintLabel.FontSize = fontSize;
		content.AddChild(hintLabel);

		return content;
	}

	// A language table is a 5-column grid: every column is sized to its widest cell, so the
	// header and all language rows line up regardless of how wide each script renders.
	private void AddLangTable(WidgetVBox root, bool wrap, bool rich)
	{
		WidgetGridBox grid = Keep(new WidgetGridBox(5, COL_PAD, COL_PADY));

		AddHeader(grid, "Lang", COL_TAG);
		AddHeader(grid, "Label", COL_LBL);
		AddHeader(grid, "Button", COL_BTN);
		AddHeader(grid, "EditLine", COL_EDIT);
		AddHeader(grid, "EditText", COL_ET);

		for (int i = 0; i < NUM_SAMPLES; i++)
			AddLangRow(grid, SAMPLES[i], wrap, rich);

		root.AddChild(grid);
	}

	private void AddHeader(WidgetGridBox grid, string text, int width)
	{
		WidgetLabel label = Keep(new WidgetLabel($"<b>{text}</b>"));
		label.FontRich = 1;
		label.FontSize = fontSize;
		label.Width = width;
		grid.AddChild(label, Gui.ALIGN_LEFT);
	}

	// One language as five grid cells (tag, Label, Button, EditLine, EditText). wrap enables
	// word wrapping, rich enables markup parsing.
	private void AddLangRow(WidgetGridBox grid, LangSample s, bool wrap, bool rich)
	{
		string text = rich ? s.rich : (wrap ? s.paragraph : s.plain);

		WidgetLabel code = Keep(new WidgetLabel(s.code));
		code.FontSize = fontSize;
		code.Width = COL_TAG;
		grid.AddChild(code, Gui.ALIGN_LEFT);

		WidgetLabel label = Keep(new WidgetLabel(text));
		label.FontSize = fontSize;
		label.Width = COL_LBL;
		label.FontRich = rich ? 1 : 0;
		label.FontWrap = wrap ? 1 : 0;
		grid.AddChild(label, Gui.ALIGN_LEFT);

		WidgetButton button = Keep(new WidgetButton(rich ? s.rich : s.plain));
		button.FontSize = fontSize;
		button.FontRich = rich ? 1 : 0;
		button.Width = COL_BTN;
		grid.AddChild(button, Gui.ALIGN_LEFT);

		WidgetEditLine editLine = Keep(new WidgetEditLine(rich ? s.rich : s.plain));
		editLine.Background = 1;
		editLine.FontSize = fontSize;
		editLine.Width = COL_EDIT;
		editLine.FontRich = rich ? 1 : 0;
		grid.AddChild(editLine, Gui.ALIGN_LEFT);

		WidgetEditText editText = Keep(new WidgetEditText(text));
		editText.Background = 1;
		editText.FontSize = fontSize;
		editText.Width = COL_ET;
		editText.Height = COL_ET_H;
		editText.FontRich = rich ? 1 : 0;
		grid.AddChild(editText, Gui.ALIGN_LEFT);
	}

	private void AddBidiSection(WidgetVBox root)
	{
		WidgetGridBox grid = Keep(new WidgetGridBox(3, COL_PAD, COL_PADY));
		for (int i = 0; i < NUM_BIDI; i++)
		{
			WidgetLabel tag = Keep(new WidgetLabel(BIDI_SAMPLES[i].label));
			tag.FontSize = fontSize;
			tag.Width = 120;
			grid.AddChild(tag, Gui.ALIGN_LEFT);
			WidgetLabel label = Keep(new WidgetLabel(BIDI_SAMPLES[i].text));
			label.FontSize = fontSize;
			label.Width = 340;
			grid.AddChild(label, Gui.ALIGN_LEFT);
			WidgetEditLine edit = Keep(new WidgetEditLine(BIDI_SAMPLES[i].text));
			edit.Background = 1;
			edit.FontSize = fontSize;
			edit.Width = 340;
			grid.AddChild(edit, Gui.ALIGN_LEFT);
		}
		root.AddChild(grid);
	}

	private void AddAlignmentSection(WidgetVBox root)
	{
		// Each alignment is shown inside a fixed-width tinted box so the text container bounds
		// are visible and the left/center/right placement is obvious.
		string[] alignNames = { "Left", "Center", "Right" };
		int[] alignValues = { Gui.ALIGN_LEFT, Gui.ALIGN_CENTER, Gui.ALIGN_RIGHT };
		vec4[] alignColors = {
			new vec4(1.0f, 0.4f, 0.4f, 0.14f),
			new vec4(0.4f, 1.0f, 0.4f, 0.14f),
			new vec4(0.4f, 0.6f, 1.0f, 0.14f),
		};
		int cellWidth = 210;

		WidgetGridBox grid = Keep(new WidgetGridBox(4, COL_PAD, COL_PADY));

		AddHeader(grid, "Lang", COL_TAG);
		for (int c = 0; c < alignNames.Length; c++)
			AddHeader(grid, alignNames[c], cellWidth);

		for (int i = 0; i < NUM_SAMPLES; i++)
		{
			WidgetLabel tag = Keep(new WidgetLabel(SAMPLES[i].code));
			tag.FontSize = fontSize;
			tag.Width = COL_TAG;
			grid.AddChild(tag, Gui.ALIGN_LEFT);
			for (int c = 0; c < alignNames.Length; c++)
			{
				WidgetVBox box = new WidgetVBox();
				box.Width = cellWidth;
				box.Background = 1;
				box.BackgroundColor = alignColors[c];
				WidgetLabel label = Keep(new WidgetLabel(SAMPLES[i].plain));
				label.FontSize = fontSize;
				label.Width = cellWidth;
				label.TextAlign = alignValues[c];
				box.AddChild(label, Gui.ALIGN_EXPAND);
				grid.AddChild(box, Gui.ALIGN_LEFT);
			}
		}
		root.AddChild(grid);
	}

	private void AddOutlineSection(WidgetVBox root)
	{
		WidgetGridBox grid = Keep(new WidgetGridBox(4, COL_PAD, COL_PADY));
		for (int i = 0; i < NUM_SAMPLES; i++)
		{
			WidgetLabel tag = Keep(new WidgetLabel(SAMPLES[i].code));
			tag.FontSize = fontSize;
			tag.Width = COL_TAG;
			grid.AddChild(tag, Gui.ALIGN_LEFT);
			WidgetLabel plain = Keep(new WidgetLabel(SAMPLES[i].plain));
			plain.FontSize = fontSize;
			plain.Width = 210;
			grid.AddChild(plain, Gui.ALIGN_LEFT);
			WidgetLabel outline = Keep(new WidgetLabel(SAMPLES[i].plain));
			outline.FontSize = fontSize;
			outline.FontOutline = 1;
			outline.Width = 210;
			grid.AddChild(outline, Gui.ALIGN_LEFT);
			WidgetLabel big = Keep(new WidgetLabel(SAMPLES[i].plain));
			big.FontSize = fontSize + 8;
			big.FontOutline = 1;
			big.Width = 240;
			grid.AddChild(big, Gui.ALIGN_LEFT);
		}
		root.AddChild(grid);
	}

	private void AddColorSection(WidgetVBox root)
	{
		string[] tags = { "AR", "ZH", "HI", "TH", "EN", "RU" };
		string[] texts = { "مرحبا بالعالم", "你好，世界！", "नमस्ते दुनिया", "สวัสดีชาวโลก", "Hello, world!", "Привет, мир!" };
		vec4[] colors = {
			new vec4(1.0f, 0.2f, 0.2f, 1.0f),
			new vec4(0.1f, 0.8f, 0.1f, 1.0f),
			new vec4(0.2f, 0.5f, 1.0f, 1.0f),
			new vec4(1.0f, 0.8f, 0.0f, 1.0f),
			new vec4(0.0f, 0.8f, 0.8f, 1.0f),
			new vec4(1.0f, 0.5f, 0.0f, 1.0f),
		};
		WidgetGridBox grid = Keep(new WidgetGridBox(3, COL_PAD, COL_PADY));
		for (int i = 0; i < tags.Length; i++)
		{
			WidgetLabel tag = Keep(new WidgetLabel(tags[i]));
			tag.FontSize = fontSize;
			tag.Width = COL_TAG;
			grid.AddChild(tag, Gui.ALIGN_LEFT);
			WidgetLabel label = Keep(new WidgetLabel(texts[i]));
			label.FontSize = fontSize;
			label.Width = 250;
			label.FontColor = colors[i];
			grid.AddChild(label, Gui.ALIGN_LEFT);
			WidgetEditLine edit = Keep(new WidgetEditLine(texts[i]));
			edit.Background = 1;
			edit.FontSize = fontSize;
			edit.Width = 250;
			edit.FontColor = colors[i];
			grid.AddChild(edit, Gui.ALIGN_LEFT);
		}
		root.AddChild(grid);
	}

	private void AddPerWidgetFontSection(WidgetVBox root)
	{
		WidgetGridBox grid = Keep(new WidgetGridBox(3, COL_PAD, COL_PADY));
		AddFontRow(grid, "Arabic font", "مرحبا بالعالم", FontArabic);
		AddFontRow(grid, "Chinese font", "你好，世界！测试", FontChinese);
		AddFontRow(grid, "Hindi font", "नमस्ते दुनिया", FontHindi);
		AddFontRow(grid, "Thai font", "สวัสดีชาวโลก", FontThai);
		AddFontRow(grid, "Latin on AR", "مرحبا بالعالم", FontLatin);
		root.AddChild(grid);
	}

	private void AddFontRow(WidgetGridBox grid, string labelText, string text, string font)
	{
		WidgetLabel tag = Keep(new WidgetLabel(labelText));
		tag.FontSize = fontSize;
		tag.Width = 200;
		grid.AddChild(tag, Gui.ALIGN_LEFT);
		WidgetLabel label = Keep(new WidgetLabel(text));
		label.FontSize = fontSize;
		label.Width = 280;
		label.SetFont(font);
		grid.AddChild(label, Gui.ALIGN_LEFT);
		WidgetEditLine edit = Keep(new WidgetEditLine(text));
		edit.Background = 1;
		edit.FontSize = fontSize;
		edit.Width = 280;
		edit.SetFont(font);
		grid.AddChild(edit, Gui.ALIGN_LEFT);
	}

	// Builds the floating, resizable window with one feature per tab.
	private void BuildFeaturePanel()
	{
		featuresWindow = new WidgetWindow("Multi-script font features");
		featuresWindow.Sizeable = true;

		WidgetTabBox tabs = new WidgetTabBox(4, 4);
		featuresWindow.AddChild(tabs, Gui.ALIGN_EXPAND);

		AddLangTable(BeginTab(tabs, "Plain", "Plain text in Label, Button, EditLine and EditText."), false, false);
		AddLangTable(BeginTab(tabs, "Wrapped", "Word-wrapped paragraphs."), true, false);
		AddLangTable(BeginTab(tabs, "Rich", "Rich markup: bold, italic and color."), false, true);
		AddBidiSection(BeginTab(tabs, "Bidirectional", "Mixed LTR/RTL runs with digits and punctuation."));
		AddAlignmentSection(BeginTab(tabs, "Alignment", "Left / center / right alignment."));
		AddOutlineSection(BeginTab(tabs, "Outline", "No outline / outline / large outline."));
		AddColorSection(BeginTab(tabs, "Color", "Per-widget SetFontColor."));
		AddPerWidgetFontSection(BeginTab(tabs, "Per-widget font", "SetFont overrides the global fallback chain."));

		tabs.CurrentTab = 0;
		featuresWindow.Arrange();
		WindowManager.MainWindow.AddChild(featuresWindow, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);
	}

	// =========================================================================
	// Scene nodes (ObjectText / ObjectGui placed in the world)
	// =========================================================================

	// Returns the ObjectText assigned for language i (SAMPLES order), or null if unassigned.
	private ObjectText GetTextObject(int i)
	{
		Node[] nodes = { TextEn, TextRu, TextAr, TextZh, TextTh, TextHi };
		if (i < 0 || i >= NUM_SAMPLES || !nodes[i])
			return null;
		ObjectText text = nodes[i] as ObjectText;
		if (text == null)
			Log.Warning($"FontsSample: text node for {SAMPLES[i].code} is not an ObjectText.\n");
		return text;
	}

	// Preferred font path for language i (SAMPLES order), taken from the File parameters.
	private string FontForLang(int i)
	{
		switch (i)
		{
			case 2: return FontArabic;
			case 3: return FontChinese;
			case 4: return FontThai;
			case 5: return FontHindi;
			default: return FontLatin;  // EN, RU
		}
	}

	// Fills each assigned ObjectText node with its language sample and matching font.
	private void SetupObjectTexts()
	{
		Gui.TextDirection dir = Gui.GetCurrent().GlobalTextDirection;
		for (int i = 0; i < NUM_SAMPLES; i++)
		{
			ObjectText text = GetTextObject(i);
			if (text == null)
				continue;
			text.FontName = FontForLang(i);
			text.TextDirection = dir;
			text.Text = SAMPLES[i].paragraph;
		}
	}

	// Resolves the ObjectGui node and populates it.
	private void SetupObjectGui()
	{
		if (!GuiObject)
			return;
		objectGui = GuiObject as ObjectGui;
		if (objectGui == null)
		{
			Log.Warning($"FontsSample: \"World GUI object\" ({GuiObject.Name}) is not an ObjectGui.\n");
			return;
		}
		objectGuiGui = objectGui.GetGui();
		BuildObjectGuiContent();
	}

	// (Re)builds the widget tree shown on the ObjectGui surface.
	private void BuildObjectGuiContent()
	{
		if (objectGuiGui == null)
			return;

		// Replace any previously built content so font-size changes take effect.
		if (objectGuiRoot)
		{
			objectGuiGui.RemoveChild(objectGuiRoot);
			objectGuiRoot.DeleteLater();
		}

		// Scale the layout to the ObjectGui surface resolution (designed for 1024 wide) so the
		// text fills the physical panel instead of rendering tiny.
		float sc = objectGuiGui.Width > 0 ? objectGuiGui.Width / 1024.0f : 1.0f;
		int titleFontSize = (int)(54 * sc);
		int rowFontSize = (int)(44 * sc);
		int pad = (int)(20 * sc);

		objectGuiRoot = new WidgetVBox(0, (int)(10 * sc));
		objectGuiRoot.SetPadding(pad, pad, pad, pad);

		WidgetLabel title = new WidgetLabel($"<font color={HEADER_COLOR}><b>World GUI — multi-script</b></font>");
		title.FontRich = 1;
		title.FontSize = titleFontSize;
		objectGuiRoot.AddChild(title);

		WidgetGridBox grid = new WidgetGridBox(3, (int)(12 * sc), (int)(8 * sc));
		for (int i = 0; i < NUM_SAMPLES; i++)
		{
			WidgetLabel code = new WidgetLabel(SAMPLES[i].code);
			code.FontSize = rowFontSize;
			code.Width = (int)(90 * sc);
			grid.AddChild(code, Gui.ALIGN_LEFT);
			WidgetLabel label = new WidgetLabel(SAMPLES[i].plain);
			label.FontSize = rowFontSize;
			label.Width = (int)(400 * sc);
			grid.AddChild(label, Gui.ALIGN_LEFT);
			WidgetEditLine edit = new WidgetEditLine(SAMPLES[i].plain);
			edit.Background = 1;
			edit.FontSize = rowFontSize;
			edit.Width = (int)(400 * sc);
			grid.AddChild(edit, Gui.ALIGN_LEFT);
		}
		objectGuiRoot.AddChild(grid);

		objectGuiGui.AddChild(objectGuiRoot, Gui.ALIGN_EXPAND);
		objectGuiGui.GlobalTextDirection = Gui.GetCurrent().GlobalTextDirection;
		objectGuiGui.GlobalCursorMode = cursorMode;
	}
}
