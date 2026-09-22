using Unigine;

// This component demonstrates widget animation using AnimationBindRuntime.
// Unlike node/material binds, a runtime bind can target any engine object
// (widgets, bodies, cameras) by pointing directly at the instance at runtime.
// Shows animating widget position, font size, and color properties.
public partial class WidgetAnimationSample : Component
{
	// A player does not own a sequence built in code, so the sample keeps it alive itself
	private AnimationSequence sequence;
	private AnimationSequencePlayer player;

	private Widget widgets;
	private Widget playerLabel0;
	private Widget playerLabel1;
	private Widget versusLabel;

	private void Init()
	{
		// Widgets come first: a runtime bind points at an existing instance
		CreateWidgets();

		CreateAnimations();

		player.Play();
	}

	private void Shutdown()
	{
		widgets.DeleteLater();

		player.Stop();
	}

	// Create the "Player 0 vs Player 1" title widgets
	private void CreateWidgets()
	{
		widgets = new WidgetVBox();

		playerLabel0 = new WidgetLabel("Player 0");
		playerLabel0.FontOutline = 1;
		playerLabel0.FontSize = 100;
		playerLabel0.SetPosition(250, 300);
		playerLabel0.FontColor = vec4.RED;
		widgets.AddChild(playerLabel0, Gui.ALIGN_OVERLAP);

		playerLabel1 = new WidgetLabel("Player 1");
		playerLabel1.FontOutline = 1;
		playerLabel1.FontSize = 100;
		playerLabel1.SetPosition(975, 500);
		playerLabel1.FontColor = vec4.GREEN;
		widgets.AddChild(playerLabel1, Gui.ALIGN_OVERLAP);

		versusLabel = new WidgetLabel("vs");
		versusLabel.FontOutline = 1;
		versusLabel.FontSize = 100;
		versusLabel.SetPosition(750, 400);
		widgets.AddChild(versusLabel, Gui.ALIGN_OVERLAP);

		WindowManager.MainWindow.AddChild(widgets, Gui.ALIGN_OVERLAP);
	}

	// Create animation sequence for the title animation
	private void CreateAnimations()
	{
		sequence = new AnimationSequence();

		// Both player labels are animated the same way, they only slide in from opposite sides
		AddPlayerLabelChannels(playerLabel0, 0, 250);		// Off-screen left, then in place
		AddPlayerLabelChannels(playerLabel1, 1225, 975);	// Off-screen right, then in place

		// "vs" label: color cycling animation (cyan -> yellow -> cyan)
		AnimationChannelFVec4 versusColorChannel = new AnimationChannelFVec4("widget.font_color");
		versusColorChannel.SetBind(CreateWidgetBind(versusLabel));
		versusColorChannel.AddValue(0.0f, new vec4(0.0f, 1.0f, 1.0f, 0.001f));
		versusColorChannel.AddValue(0.25f, new vec4(0.0f, 1.0f, 1.0f, 0.001f));
		versusColorChannel.AddValue(0.3f, new vec4(0.0f, 1.0f, 1.0f, 1.0f));	// Fade in
		versusColorChannel.AddValue(1.5f, new vec4(1.0f, 1.0f, 0.0f, 1.0f));	// Shift to yellow
		versusColorChannel.AddValue(2.70f, new vec4(0.0f, 1.0f, 1.0f, 1.0f));	// Back to cyan
		versusColorChannel.AddValue(2.75f, new vec4(0.0f, 1.0f, 1.0f, 0.001f));	// Fade out
		versusColorChannel.AddValue(3.0f, new vec4(0.0f, 1.0f, 1.0f, 0.001f));
		versusColorChannel.AddValue(5.0f, new vec4(0.0f, 1.0f, 1.0f, 0.001f));
		sequence.AddChannel(versusColorChannel);

		player = new AnimationSequencePlayer(sequence);
		player.Loop = true;
	}

	// Runtime binding: the bind holds the widget instance itself, not an asset description
	private static AnimationBindRuntime CreateWidgetBind(Widget widget)
	{
		AnimationBindRuntime bind = new AnimationBindRuntime();
		bind.Widget = widget;

		return bind;
	}

	// Slide, fade and scale one of the player labels. Every channel carries its own bind,
	// so a label animated by several parameters needs a channel per parameter.
	private void AddPlayerLabelChannels(Widget label, int hiddenPositionX, int visiblePositionX)
	{
		AnimationBindRuntime bind = CreateWidgetBind(label);

		// Slide the label in and back out
		AnimationChannelInt positionChannel = new AnimationChannelInt("widget.position_x");
		positionChannel.SetBind(bind);
		positionChannel.AddValue(0.0f, hiddenPositionX);
		positionChannel.AddValue(0.25f, visiblePositionX);	// Slide in
		positionChannel.AddValue(2.75f, visiblePositionX);	// Hold
		positionChannel.AddValue(3.0f, hiddenPositionX);		// Slide out
		positionChannel.AddValue(5.0f, hiddenPositionX);
		sequence.AddChannel(positionChannel);

		// Fade the label in and out (alpha component)
		AnimationChannelFloat colorChannel = new AnimationChannelFloat("widget.font_color_w");
		colorChannel.SetBind(bind);
		colorChannel.AddValue(0.0f, 0.001f);
		colorChannel.AddValue(0.25f, 1.0f);
		colorChannel.AddValue(2.75f, 1.0f);
		colorChannel.AddValue(3.0f, 0.001f);
		colorChannel.AddValue(5.0f, 0.001f);
		sequence.AddChannel(colorChannel);

		// Font size animation (scale effect)
		AnimationChannelInt fontSizeChannel = new AnimationChannelInt("widget.font_size");
		fontSizeChannel.SetBind(bind);
		fontSizeChannel.AddValue(0.0f, 80);
		fontSizeChannel.AddValue(0.25f, 100);
		fontSizeChannel.AddValue(2.75f, 100);
		fontSizeChannel.AddValue(3.0f, 80);
		fontSizeChannel.AddValue(5.0f, 80);
		sequence.AddChannel(fontSizeChannel);
	}
}
