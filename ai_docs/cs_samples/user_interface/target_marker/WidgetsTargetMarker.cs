// Off-screen target indicator that shows directional arrows when target is outside view.
// When target is visible on screen, displays a point marker at its position.
// When target is off-screen or behind camera, shows a rotated arrow sprite at screen edge
// pointing toward the target. Handles edge cases like corners and targets behind the camera.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Widget-based HUD marker for tracking objects even when not visible.
public partial class WidgetsTargetMarker : Component
{
	// Sprite image for directional arrow shown when target is off-screen
	[ShowInEditor] public AssetLink arrowSprite;
	// Sprite image for point marker shown when target is on-screen
	[ShowInEditor] public AssetLink pointSprite;

	// The node to track - marker points toward this object
	[ShowInEditor] public Node target;

	// Pivot point for point sprite (0.5, 0.5 = centered)
	[ShowInEditor] private vec2 pointSpritePivot = new vec2(0.5f, 0.5f);

	// Widget sprites for rendering the markers
	private WidgetSprite arrow;
	private WidgetSprite point;

	// Reference to camera (must be attached to a Player node)
	private Player camera;

	// Cached sprite dimensions for positioning calculations
	private int pointWidth;
	private int pointHeight;
	private int arrowWidth;
	private int arrowHalfWidth;
	private int arrowHeight;
	private int arrowHalfHeight;

	// Loads sprite assets and validates required references.
	private void Init()
	{
		// Load arrow sprite for off-screen indication
		if (!arrowSprite.IsFileExist)
		{
			Log.ErrorLine("WidgetsTargetMarker.Init(): Source file for the pointer sprite image is not found.");
			return;
		}
		arrow = new WidgetSprite(arrowSprite.Path);
		WindowManager.MainWindow.AddChild(arrow, Gui.ALIGN_OVERLAP);

		// Load point sprite for on-screen marker
		if (!pointSprite.IsFileExist)
		{
			Log.ErrorLine("WidgetsTargetMarker.Init(): Source file for the marker sprite image is not found.");
			return;
		}
		point = new WidgetSprite(pointSprite.Path);
		WindowManager.MainWindow.AddChild(point, Gui.ALIGN_OVERLAP);

		if (!target)
		{
			Log.ErrorLine("WidgetsTargetMarker.Init(): No target object specified.");
			return;
		}

		// This component must be attached to a Player (camera) node
		camera = node as Player;
		if (!camera)
		{
			Log.ErrorLine("WidgetsTargetMarker.Init(): Camera is not valid.");
			return;
		}
	}

	// Calculates target screen position and shows appropriate marker (point or arrow).
	private void Update()
	{
		if (!arrow || !point || !camera || !target)
			return;

		// Cache sprite dimensions for positioning math
		arrowWidth = arrow.GetLayerWidth(0);
		arrowHalfWidth = arrowWidth / 2;
		arrowHeight = arrow.GetLayerHeight(0);
		arrowHalfHeight = arrowHeight / 2;

		pointWidth = point.GetLayerWidth(0);
		pointHeight = point.GetLayerHeight(0);
		// Apply pivot offset so sprite centers on target position
		mat4 translation = MathLib.Translate(new vec3(-pointWidth * pointSpritePivot.x, -pointHeight * pointSpritePivot.y, 0.0f) * WindowManager.MainWindow.DpiScale);
		point.Transform = translation;

		// Start with both markers hidden, show appropriate one below
		arrow.Hidden = true;
		point.Hidden = true;

		int width = WindowManager.MainWindow.ClientSize.x;
		int height = WindowManager.MainWindow.ClientSize.y;
		int halfWidth = width / 2;
		int halfHeight = height / 2;

		int x = 0;
		int y = 0;

		vec3 targetDirection = new vec3(target.WorldBoundBox.Center - camera.WorldPosition);

		// Check if target is behind camera using dot product with view direction
		bool behind = MathLib.Dot(camera.GetWorldDirection(), targetDirection) < 0;

		if(!behind)
		{
			// Target in front of camera - project directly to screen coordinates
			camera.GetScreenPosition(out x, out y, target.WorldBoundBox.Center, width, height);
			// Convert to centered coordinate system (0,0 at screen center)
			x -= halfWidth;
			y -= halfHeight;
			y *= -1;  // Flip Y since screen coords are top-down
		}
		else
		{
			// Target behind camera - reflect position to find arrow direction
			vec3 inverseScreenPlaneNormal = new vec3(camera.ViewDirection * -1);
			vec3 relativeToCameraTargetPosition = (vec3)(target.WorldBoundBox.Center - camera.WorldPosition);

			// Reflect target position through camera plane to get meaningful screen direction
			vec3 orthoProjectionTarget = inverseScreenPlaneNormal * MathLib.Dot(relativeToCameraTargetPosition, inverseScreenPlaneNormal);
			vec3 reflectedTargetPosition = (vec3)((relativeToCameraTargetPosition - orthoProjectionTarget * 2) + camera.WorldPosition);
			camera.GetScreenPosition(out x, out y, reflectedTargetPosition, width, height);
			x -= halfWidth;
			y -= halfHeight;
			// Force arrow to point downward when behind
			if (y > 0)
				y *= -1;
		}

		// Target is visible on screen - show point marker
		if (!behind && x >= -halfWidth && x <= halfWidth && y >= -halfHeight && y <= halfHeight)
		{
			point.Hidden = false;
			point.SetPosition(x + halfWidth, -y + halfHeight);
		}
		else
		{
			// Target off-screen - show arrow at screen edge pointing toward target
			int point_x = 0;
			int point_y = 0;
			GetIntersectionWithScreenRect(out point_x, out point_y, x, y, halfWidth, halfHeight);
			float angle = 0.0f;

			// Special handling for corners - rotate arrow to point diagonally
			if (halfHeight - MathLib.Abs(point_y) <= arrowHalfHeight && halfWidth - MathLib.Abs(point_x) <= arrowHalfWidth)
			{
				float dx, dy;

				if (point_y > 0)
					dy = point_y - (halfHeight - arrowHalfWidth);
				else
					dy = point_y + (halfHeight - arrowHalfWidth);

				if (point_x > 0)
					dx = point_x - (halfWidth - arrowHalfWidth);
				else
					dx = point_x + (halfWidth - arrowHalfWidth);

				angle = -MathLib.Atan2(dy, dx) * MathLib.RAD2DEG;

				if (point_x > 0)
					point_x = halfWidth - arrowWidth;
				else
					point_x = -halfWidth;

				if (point_y > 0)
					point_y = halfHeight;
				else
					point_y = -halfHeight + arrowHeight;
			}
			// Top edge - arrow points up
			else if (point_y == halfHeight)
			{
				point_x -= arrowHalfWidth;
				angle = -90;
			}
			// Bottom edge - arrow points down
			else if (point_y == -halfHeight)
			{
				point_y += arrowHeight;
				point_x -= arrowHalfWidth;
				angle = 90;
			}
			// Left edge - arrow points left
			else if (point_x == -halfWidth)
			{
				point_y += arrowHalfHeight;
				angle = 180;
			}
			// Right edge - arrow points right
			else if (point_x == halfWidth)
			{
				point_x -= arrowWidth;
				point_y += arrowHalfHeight;
				angle = 0;
			}

			arrow.Hidden = false;
			arrow.SetPosition(point_x + halfWidth, -point_y + halfHeight);

			// Rotate arrow around its center to point toward target
			mat4 rotation = new mat4(
				MathLib.Translate(new vec3(arrowHalfWidth, arrowHalfHeight, 0.0f) * WindowManager.MainWindow.DpiScale) *
				MathLib.Rotate(new quat(vec3.UP, angle)) *
				MathLib.Translate(new vec3(-arrowHalfWidth, -arrowHalfHeight, 0.0f) * WindowManager.MainWindow.DpiScale)
			);

			arrow.Transform = rotation;
		}
	}

	// Cleans up sprite widgets.
	private void Shutdown()
	{
		arrow.DeleteLater();
		point.DeleteLater();
	}

	// Finds where a direction vector from screen center intersects the screen edge rectangle.
	// Returns the edge point in centered coordinates (-halfWidth to halfWidth, -halfHeight to halfHeight).
	private void GetIntersectionWithScreenRect(out int x, out int y, int vec_x, int vec_y, int halfWidth, int halfHeight)
	{
		// Vector points upward or horizontal (vec_y >= 0)
		if (vec_y >= 0)
		{
			// Horizontal vector (vec_y == 0) - intersects left or right edge
			if (vec_y == 0)
			{
				if (vec_x > 0)
				{
					// Points right - intersect right edge
					x = halfWidth;
					y = 0;
				}
				else
				{
					// Points left - intersect left edge
					x = -halfWidth;
					y = 0;
				}

				return;
			}

			// Try intersection with top edge first using similar triangles:
			// x/vec_x = halfHeight/vec_y, so x = halfHeight * (vec_x/vec_y)
			x = (int)(halfHeight * ((float)vec_x / (float)vec_y));
			y = halfHeight;

			// If x is within screen bounds, top edge intersection is valid
			if (x >= -halfWidth && x <= halfWidth)
				return;

			// Top edge intersection is outside bounds - try side edges
			if (vec_x >= 0)
			{
				// Straight up vector - intersect top edge at center
				if (vec_x == 0)
				{
					x = 0;
					y = halfHeight;

					return;
				}

				// Points right - intersect right edge
				// y/vec_y = halfWidth/vec_x, so y = halfWidth * (vec_y/vec_x)
				x = halfWidth;
				y = (int)(halfWidth * ((float)vec_y / (float)vec_x));

				return;
			}
			else
			{
				// Points left - intersect left edge
				x = -halfWidth;
				y = (int)(-halfWidth * ((float)vec_y / (float)vec_x));

				return;
			}
		}
		// Vector points downward (vec_y < 0)
		else
		{
			// Try intersection with bottom edge first
			x = (int)(-halfHeight * ((float)vec_x / (float)vec_y));
			y = -halfHeight;

			// If x is within screen bounds, bottom edge intersection is valid
			if (x >= -halfWidth && x <= halfWidth)
				return;

			// Bottom edge intersection is outside bounds - try side edges
			if (vec_x >= 0)
			{
				// Straight down vector - intersect bottom edge at center
				if (vec_x == 0)
				{
					x = 0;
					y = -halfHeight;

					return;
				}

				// Points right - intersect right edge
				x = halfWidth;
				y = (int)(halfWidth * ((float)vec_y / (float)vec_x));

				return;
			}
			else
			{
				// Points left - intersect left edge
				x = -halfWidth;
				y = (int)(-halfWidth * ((float)vec_y / (float)vec_x));

				return;
			}
		}
	}
}
