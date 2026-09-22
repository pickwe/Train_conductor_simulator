// Creates a real-time clock display rendered onto a texture using GuiToTexture.
// The clock updates only when the second changes to avoid unnecessary rendering.
// This demonstrates manual GUI texture updates for better performance when
// content doesn't need to change every frame.

using System;
using Unigine;

// Real-time clock widget rendered to texture with manual updates.
public partial class WidgetClock : Component
{
	// GuiToTexture setup and clock label creation.
	void Init()
	{
		guiToTexture = ComponentSystem.GetComponent<GuiToTexture>(node);
		if (guiToTexture == null)
		{
			Log.Error("WidgetClock.Init(): No GuiToTexture component found\n");
			return;
		}

		// Get our custom GUI
		var gui = guiToTexture.Gui;
		widgetTimer = new WidgetLabel(gui) { FontSize = 150, FontColor = vec4.RED };

		// Disable auto update, because we will update gui manually in SetTime method
		guiToTexture.AutoUpdateEnabled = false;

		// Add widget as a child in gui
		gui.AddChild(widgetTimer, Gui.ALIGN_OVERLAP);

		CenterPosition = guiToTexture.TextureResolution / 2;
		previousTime = DateTime.Now.TimeOfDay;


		// Set time and update gui
		SetTime(previousTime);
		// Now we don't need to interact with GuiToTexture, it will be updated on its own
		// starting from here, we will just update the state of our custom widget
	}

	// Updates clock display only when second changes for efficiency.
	void Update()
	{
		var now = DateTime.Now.TimeOfDay;
		// Skip rendering if less than 1 second passed
		if (now - previousTime < TimeSpan.FromSeconds(1))
		{
			return;
		}

		SetTime(now);
		previousTime = now;
	}


	public ivec2 CenterPosition
	{
		get
		{
			return centerPosition;
		}
		set
		{
			centerPosition = value;
			AdjustScreenPosition();
		}
	}

	// Updates clock text and triggers manual GUI render.
	private void SetTime(TimeSpan time)
	{
		widgetTimer.Text = time.ToString("hh\\:mm\\:ss");
		AdjustScreenPosition();
		// Manually render since auto-update is disabled
		guiToTexture.RenderToTexture();
	}

	// Centers the clock label based on its text size.
	private void AdjustScreenPosition()
	{
		ivec2 widgetSize;
		widgetSize.y = widgetTimer.GetTextRenderSize(widgetTimer.Text).y;
		widgetSize.x = widgetTimer.GetTextRenderSize(widgetTimer.Text).x;
		widgetTimer.SetPosition(centerPosition.x - widgetSize.x / 2, centerPosition.y - widgetSize.y / 2);
	}

	// Center position for the clock display
	private ivec2 centerPosition;
	// Label widget showing the time
	private WidgetLabel widgetTimer;
	// Last update time for throttling
	private TimeSpan previousTime;
	// Reference to the GUI texture renderer
	private GuiToTexture guiToTexture;

}
