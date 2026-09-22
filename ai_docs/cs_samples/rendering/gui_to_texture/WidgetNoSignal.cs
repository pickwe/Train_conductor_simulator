// Creates a bouncing "No Signal" label that moves across a texture like the classic
// DVD screensaver. Uses GuiToTexture with auto-update enabled, demonstrating how
// widget state changes are automatically reflected in the rendered texture each frame.
// The label bounces off the texture boundaries with proper vector reflection.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Bouncing "No Signal" label rendered to texture with auto-update.
public partial class WidgetNoSignal : Component
{
	// Creates label widget and adds it to the GuiToTexture GUI.
	void Init()
	{

		// Get GuiToTexture component
		GuiToTexture guiToTexture = ComponentSystem.GetComponent<GuiToTexture>(node);

		// Get GUI from GuiToTexture component
		Gui gui = guiToTexture.Gui;

		// Create a widget that you want to render in GUI
		label = new WidgetLabel(gui) { FontSize = 150, Text = "No Signal", FontColor = vec4.RED };

		container = new WidgetVBox() { Background = 1, BackgroundColor = vec4.BLUE };
		container.AddChild(label, Gui.ALIGN_EXPAND | Gui.ALIGN_BACKGROUND);

		// Add the widget to GUI children
		gui.AddChild(container, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);

		// AutoUpdateEnabled is true by default, so GuiToTexture renders every frame automatically.
		// We only need to update widget state - the texture will reflect changes on next frame.
	}

	// Moves label by accumulated delta and checks for boundary collisions.
	void Update()
	{
		// Calculate frame movement with delta time for smooth animation
		float frameSpeed = labelSpeed * Game.IFps;
		vec2 delta = direction * frameSpeed;
		int posX = container.PositionX;
		int posY = container.PositionY;

		// Accumulate sub-pixel movement for smoother animation
		if (new ivec2(posX, posY) + new ivec2(accumulatedDelta) == new ivec2(posX, posY))
		{
			accumulatedDelta += delta;
			return;
		}

		// Apply accumulated movement to widget position
		container.PositionX = posX + (int)accumulatedDelta.x;
		container.PositionY = posY + (int)accumulatedDelta.y;

		accumulatedDelta = new vec2(0, 0);

		// Check and handle boundary collisions
		ReflectDirection();
	}

	// Handles collision with texture boundaries by reflecting movement direction.
	private void ReflectDirection()
	{
		ivec2 size = container.ParentGui.Size;

		int labelPosX = container.PositionX;
		int labelPosY = container.PositionY;

		int xRightCornerDelta = label.GetTextRenderSize(label.Text).x;
		int yBottomCornerDelta = label.GetTextRenderSize(label.Text).y;

		var leftTopCornerPos = new ivec2(labelPosX, labelPosY);
		var rightTopCornerPos = new ivec2(labelPosX + xRightCornerDelta, labelPosY);
		var leftBottomCornerPos = new ivec2(labelPosX, labelPosY + yBottomCornerDelta);
		var rightBottomCornerPos = new ivec2(labelPosX + xRightCornerDelta,
			labelPosY + yBottomCornerDelta);

		// Check the top left corner
		{
			// Intersecting with top
			if (leftTopCornerPos.x > 0 && leftTopCornerPos.y < 0)
			{
				container.PositionY = 0;
				direction = ReflectVector(direction, new vec2(0, 1));
				return;
			}

			if (leftTopCornerPos.x < 0 && leftTopCornerPos.y > 0)
			{
				container.PositionX = 0;
				direction = ReflectVector(direction, new vec2(1, 0));
				return;
			}

			// Intersecting with corner
			if (leftTopCornerPos.x < 0 && leftTopCornerPos.y < 0)
			{
				direction = MathLib.Normalize(new vec2(1, 1));
				container.PositionX = 0;
				container.PositionY = 0;
				return;
			}
		}

		// Check the top right corner
		{
			// Right corner
			if (rightTopCornerPos.x > size.x && rightTopCornerPos.y > 0)
			{
				container.PositionX = size.x - xRightCornerDelta;
				direction = ReflectVector(direction, new vec2(-1, 0));
				return;
			}

			if (rightTopCornerPos.x > size.x && rightTopCornerPos.y < 0)
			{
				container.PositionX = size.x - xRightCornerDelta;
				container.PositionY = 0;
				direction = new vec2(-1, 1);
				return;
			}
		}

		// Check the bottom left corner
		{
			if (leftBottomCornerPos.x < 0 && leftBottomCornerPos.y < size.y)
			{
				container.PositionX = 0;
				direction = ReflectVector(direction, new vec2(1, 0));
				return;
			}

			if (leftBottomCornerPos.x > 0 && leftBottomCornerPos.y > size.y)
			{
				container.PositionY = size.y - yBottomCornerDelta;
				direction = ReflectVector(direction, new vec2(0, -1));
				return;
			}

			if (leftBottomCornerPos.x < 0 && leftBottomCornerPos.y > size.y)
			{
				container.PositionX = 0;
				container.PositionY = yBottomCornerDelta;
				direction = new vec2(1, -1);
				return;
			}
		}

		// Bottom right corner
		{
			if (rightBottomCornerPos.x > size.x && rightBottomCornerPos.y < size.y)
			{
				container.PositionX = size.x - xRightCornerDelta;
				direction = ReflectVector(direction, new vec2(-1, 0));
				return;
			}

			if (rightBottomCornerPos.x > size.x && rightBottomCornerPos.y > size.y)
			{
				container.PositionX = size.x - xRightCornerDelta;
				container.PositionY = size.y - yBottomCornerDelta;
				direction = new vec2(-1, -1);
				return;
			}
		}
	}

	// Reflects a vector off a surface defined by its normal.
	static vec2 ReflectVector(vec2 vector, vec2 normal)
	{
		return MathLib.Normalize(vector - normal * MathLib.Dot(vector, normal) * 2);
	}

	// Movement speed in pixels per second
	[ShowInEditor] private float labelSpeed = 1000;

	// Container holding the label with background
	private WidgetVBox container;
	// The "No Signal" text label
	private WidgetLabel label;

	// Sub-pixel movement accumulator for smooth animation
	private vec2 accumulatedDelta;
	// Current movement direction (normalized)
	private vec2 direction = new vec2(1, 1);
}
