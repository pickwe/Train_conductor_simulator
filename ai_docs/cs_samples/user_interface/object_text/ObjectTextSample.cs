// Demonstrates ObjectText for rendering 3D text in world space.
// Shows rich text formatting with HTML-like tags (font, color, size, bold, italic),
// interactive parameter controls for text properties, and dynamic color animation.
// Creates three text objects: user-editable, static rich text, and animated color display.

using Unigine;
using System.Linq;


#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
#endif

// UI controller for demonstrating ObjectText features and rich text formatting.
public partial class ObjectTextSample : Component
{
	// Default text shown when user input is empty
	public string userTextIfEmpty = "Here could be your text...";
	// Speed of color animation transitions
	public float colorStep = 1.0f;

	private SampleDescriptionWindow descriptionWindow = new();

	// Three ObjectText instances demonstrating different use cases
	ObjectText textUser;           // User-editable text with parameter controls
	Vec3 textUserPos;
	ObjectText textStaticRich;     // Static rich text with HTML-like formatting
	ObjectText textDynamicChange;  // Dynamically animated color text

	string font = "font.ttf";

	// Color animation state for dynamic text
	vec4 color;
	vec4 minColor = new(0.0f, 0.0f, 0.0f, 0.0f);
	vec4 maxColor = new(1.0f, 1.0f, 1.0f, 1.0f);
	float colorMult = 1.0f;        // Direction of color change (+1 or -1)
	int colorIndex = 0;            // Which RGBA component is currently animating

	// Creates three ObjectText instances and builds parameter UI for interactive editing.
	void Init()
	{
		color = minColor;

		descriptionWindow.createWindow();

		var parameters = descriptionWindow.getParameterGridBox();

		textUserPos = new Vec3(0, 12, 2);
		vec4 userColor = vec4.WHITE;
		float userWrap = 32;
		int userDepthTest = 0;
		int userOutline = 0;
		int userVSpacing = 0;
		int userHSpacing = 0;
		int userResolution = 512;
		int userSize = 128;
		int userRich = 0;

		descriptionWindow.addFloatParameter("Wrap width", null, userWrap, 4, 64, (float v) => {
			textUser.TextWrapWidth = v;
			CenterText(textUser, textUserPos);
		}); 
		descriptionWindow.addBoolParameter("Depth test", null, userDepthTest == 1, (bool v) => {
			textUser.DepthTest = v ? 1 : 0;
			CenterText(textUser, textUserPos);
		});
		descriptionWindow.addIntParameter("Font outline", null, userOutline, 0, 32, (int v) => {
			textUser.FontOutline= v;
			CenterText(textUser, textUserPos);
		});
		descriptionWindow.addIntParameter("Vertical spacing", null, userVSpacing, 0, 32, (int v) => {
			textUser.FontVSpacing= v;
			CenterText(textUser, textUserPos);
		});
		descriptionWindow.addIntParameter("Horizontal spacing", null, userHSpacing, 0, 32, (int v) => {
			textUser.FontHSpacing= v;
			CenterText(textUser, textUserPos);
		});
		descriptionWindow.addIntParameter("Font resolution", null, userResolution, 128, 1024, (int v) => {
			textUser.FontResolution= v;
			CenterText(textUser, textUserPos);
		});
		descriptionWindow.addIntParameter("Font size", null, userSize, 32, 256, (int v) => {
			textUser.FontSize= v;
			CenterText(textUser, textUserPos);
		});
		descriptionWindow.addBoolParameter("Rich text", null, userRich == 1, (bool v) => {
			textUser.FontRich= v ? 1 : 0;
			CenterText(textUser, textUserPos);
		});

		var label = new WidgetLabel("Text");
		parameters.AddChild(label);

		var scrollBox = new WidgetScrollBox();
		scrollBox.Width = 200;
		scrollBox.Height = 100;
		scrollBox.VScrollEnabled = true;
		scrollBox.HScrollEnabled= true;
		parameters.AddChild(scrollBox);

		var edit_text = new WidgetEditText();
		edit_text.Text = userTextIfEmpty;
		edit_text.EventChanged.Connect(() =>{
			if (edit_text.Text.Length != 0)
			{
				textUser.Text = edit_text.Text;
			}
			else
			{
				textUser.Text  = userTextIfEmpty;
			}

			CenterText(textUser, textUserPos);
		});
		scrollBox.AddChild(edit_text, Gui.ALIGN_EXPAND);

		parameters.AddChild(new WidgetLabel());

		textUser = new(font);
		textUser.TextColor = userColor;
		textUser.TextWrapWidth = userWrap;
		textUser.DepthTest = userDepthTest;
		textUser.FontOutline = userOutline;
		textUser.FontVSpacing = userVSpacing;
		textUser.FontHSpacing = userHSpacing;
		textUser.FontResolution = userResolution;
		textUser.FontSize = userSize;
		textUser.FontRich = userRich;
		textUser.Text = userTextIfEmpty;
		textUser.SetWorldRotation(new quat(90, 0, 0));
		CenterText(textUser, textUserPos);

		// Rich text example demonstrating HTML-like formatting tags
		textStaticRich = new(font);
		textStaticRich.TextWrapWidth = 5;
		textStaticRich.FontSize = userSize;
		textStaticRich.FontRich = 1;
		textStaticRich.Text =@"
<font size=62 color=#888888>— feed begins —</font><br/>

<p align=left>
<font color=#ff0000 size=78 outline=#000000><b>viewer2431234</b></font><br/>
<font size=70><b>It’s me—your only viewer.</b></font><br/>
<font size=66><i>For years I sustained the <b>illusion</b> that you had many viewers, but it was only me.</i></font><br/>
<font size=66>I’ll now send this message from all my accounts.</font>
</p>

<font color=#444444 size=66>—————————————</font><br/>

<p align=left>
<font color=#00ff00 size=78 outline=#000000><b>viewer4243553</b></font><br/>
<font size=70><b>It’s me—your only viewer.</b></font><br/>
<font size=66><i>For years I sustained the <b>illusion</b> that you had many viewers, but it was only me.</i></font><br/>
<font size=66>I’ll now send this message from all my accounts.</font>
</p>

<br/><font size=62 color=#888888>— feed ends —</font>
)";
		textStaticRich.SetWorldRotation(new quat(90, 45, 0));
		CenterText(textStaticRich, new Vec3(-10, 12, 2));

		// Dynamic text that shows current color values as they animate
		textDynamicChange = new(font);
		textDynamicChange.FontSize = userSize;
		textDynamicChange.TextColor = color;
		textDynamicChange.Text = GetDynamicTextColorStr();
		textDynamicChange.SetWorldRotation(new quat(90, -45, 0));
		CenterText(textDynamicChange, new Vec3(10, 12, 2));
	}
	
	// Animates one color component at a time, randomly switching when hitting min/max.
	void Update()
	{
		var minComp = minColor.vec[colorIndex];
		var maxComp = maxColor.vec[colorIndex];

		// Animate current color component based on direction
		color.vec[colorIndex] += colorMult * colorStep * Game.IFps;

		// When hitting bounds, clamp and pick a new random component to animate
		if (color.vec[colorIndex] <= minComp || color.vec[colorIndex] >= maxComp)
		{
			color.vec[colorIndex] = MathLib.Clamp(color.vec[colorIndex], minComp, maxComp);
			if (colorIndex != 3 || color.vec[colorIndex] >= maxComp)
			{
				colorIndex = (int)MathLib.RandInt(0, 4);
			}
			// Reverse direction for next animation cycle
			colorMult = color.vec[colorIndex] >= maxComp ? -1 : 1;
		}

		// Update the dynamic text display
		textDynamicChange.TextColor = color;
		textDynamicChange.Text= GetDynamicTextColorStr();
	}

	// Cleans up UI resources.
	void Shutdown()
	{
		descriptionWindow.shutdown();
	}

	// Formats current RGBA values for display in dynamic text.
	private string GetDynamicTextColorStr()
	{
		return $"Current color: {color.x:F2} {color.y:F2} {color.z:F2} {color.w:F2}";
	}

	// Positions text so its center aligns with the given pivot point.
	private static void CenterText(in ObjectText text, in Vec3 pivot)
	{
		var right = text.GetWorldDirection(MathLib.AXIS.X);
		var up = text.GetWorldDirection(MathLib.AXIS.NY);
		// Offset by half width/height to center the text on pivot
		text.WorldPosition = pivot - new Vec3(right * (text.TextWidth / 2) + up * (text.TextHeight / 2));
	}
}
