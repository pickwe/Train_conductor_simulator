// RTS-style unit selection system using click and drag-box selection.
// Draws a green rectangle on screen during selection, then uses frustum
// intersection to select all objects within the box. Single-click selects
// individual objects via raycast. Selected objects get CameraUnitSelection notification.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unigine;
#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

// Handles click and drag-box selection with visual feedback and frustum culling.
public partial class CameraSelection : Component
{

	// Combined center of all selected objects (recalculated on access)
	private Vec3 selectedObjectsBoundSpherePosition;
	public Vec3 Center
	{
		get
		{
			UpdateBoundSphere();
			return selectedObjectsBoundSpherePosition;
		}
	}

	// Radius of bounding sphere containing all selected objects (for camera focus)
	private Scalar selectedObjectsBoundSphereRadius;
	public Scalar BoundRadius
	{
		get
		{
			UpdateBoundSphere();
			return selectedObjectsBoundSphereRadius;
		}
	}

	// True if any objects are currently selected
	public bool Selection
	{
		get { return selectedObjects.Count != 0; }
	}

	// True while drag-selection is in progress
	private bool isSelection;

	// Screen coordinates for selection rectangle
	private ivec2 selectionStartMousePosition;
	// Normalized screen coordinates (0-1) for rectangle corners
	private vec2 upperLeftSelectionCorner;
	private vec2 bottomRightSelectionCorner;

	// Frustum built from selection rectangle for intersection testing
	private WorldBoundFrustum frustum;
	private List<Unigine.Object> selectedObjects = new List<Unigine.Object>(0);

	// Computes combined bounding sphere of all selected objects.
	private void UpdateBoundSphere()
	{
		WorldBoundSphere bs = new WorldBoundSphere();
		for(int i =0; i < selectedObjects.Count; i++)
		{
			bs.Expand(selectedObjects[i].WorldBoundSphere);
		}
		selectedObjectsBoundSpherePosition = bs.Center;
		// Scale up radius for comfortable camera framing
		selectedObjectsBoundSphereRadius = bs.Radius * 4.0f;
	}

	// Handles mouse input for drag-box and click selection.
	private void Update()
	{
		if(!Unigine.Console.Active)
		{
			Visualizer.Enabled = true;

			// Left-click starts selection
			if(Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT))
			{
				isSelection = true;
				selectionStartMousePosition = Input.MousePosition - WindowManager.MainWindow.ClientPosition;
			}

			// While dragging, draw green selection rectangle
			if(isSelection)
			{
				// Convert screen coordinates to normalized 0-1 range (Y is flipped)
				ivec2 windowSize = WindowManager.MainWindow.ClientRenderSize;
				upperLeftSelectionCorner.x = selectionStartMousePosition.x * 1.0f / windowSize.x;
				upperLeftSelectionCorner.y = 1.0f - selectionStartMousePosition.y * 1.0f / windowSize.y;
				ivec2 currentMousePosition = Input.MousePosition - WindowManager.MainWindow.ClientPosition;
				bottomRightSelectionCorner.x = currentMousePosition.x * 1.0f / windowSize.x;
				bottomRightSelectionCorner.y = 1.0f - currentMousePosition.y * 1.0f / windowSize.y;

				Visualizer.RenderRectangle(new vec4(upperLeftSelectionCorner.x , upperLeftSelectionCorner.y, bottomRightSelectionCorner.x, bottomRightSelectionCorner.y), vec4.GREEN);
			}

			// Left-click release: finalize selection
			if(Input.IsMouseButtonUp(Input.MOUSE_BUTTON.LEFT))
			{
				Player camera = Game.Player;

				// Deselect all previously selected objects
				foreach(var it in selectedObjects)
				{
					var cameraUnitSelectionComponent = GetComponent<CameraUnitSelection>(it);
					cameraUnitSelectionComponent.Selected = false;
				}

				isSelection = false;
				ivec2 selectionFinishPosition = Input.MousePosition - WindowManager.MainWindow.ClientPosition;
				float width = MathLib.Abs(selectionStartMousePosition.x - selectionFinishPosition.x);
				float height = MathLib.Abs(selectionStartMousePosition.y - selectionFinishPosition.y);

				// Build frustum from selection rectangle and find all objects inside
				mat4 perspective = camera.GetProjectionFromMainWindow(selectionStartMousePosition.x, selectionStartMousePosition.y, selectionFinishPosition.x, selectionFinishPosition.y);

				frustum.Set(perspective, MathLib.Inverse(camera.WorldTransform));

				World.GetIntersection(frustum, selectedObjects);

				// If drag-box found nothing, try single-click raycast
				if(selectedObjects.Count == 0)
				{
					ivec2 mouse = Input.MousePosition;
					vec3 dir = camera.GetDirectionFromMainWindow(mouse.x, mouse.y);
					Unigine.Object obj = World.GetIntersection(camera.WorldPosition, camera.WorldPosition + dir * 10000, ~0);
					if(obj != null)
					{
						selectedObjects.Add(obj);
					}
				}

				// Mark selected objects, removing any without CameraUnitSelection component
				for (int i =0; i < selectedObjects.Count; i++)
				{
					var cameraUnitSelectionComponent = GetComponent<CameraUnitSelection>(selectedObjects[i]);
					if (cameraUnitSelectionComponent)
					{
						cameraUnitSelectionComponent.Selected = true;
					}
					else
					{
						// Object is not a selectable unit, remove from list
						selectedObjects.Remove(selectedObjects[i]);
						i--;
					}
				}
				UpdateBoundSphere();
			}
		}
	}
}
