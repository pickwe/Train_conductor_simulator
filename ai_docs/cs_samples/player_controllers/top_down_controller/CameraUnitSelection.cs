// Selectable unit marker that displays a visual indicator when the unit is selected.
// Loads a selection circle asset (typically a decal or mesh) and parents it to the unit.
// The selection circle is shown/hidden based on the Selected property set by CameraSelection.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Unit selection indicator that shows/hides a visual marker when selected.
public partial class CameraUnitSelection : Component
{
	// Asset path to the selection indicator (decal, mesh, or effect)
	public AssetLink selectionCircle;
	// Position offset from unit center (slight Z lift prevents z-fighting)
	public vec3 offset = new vec3(0f, 0f, 0.01f);

	private bool selected = false;
	// Selection state - automatically shows/hides the indicator
	public bool Selected
	{
		get { return selected; }
		set
		{
			selected = value;
			if (selectionCircleNode != null)
				selectionCircleNode.Enabled = selected;
		}
	}

	// Loaded selection indicator node, parented to unit
	private Node selectionCircleNode = null;

	// Loads selection indicator from asset and attaches to unit.
	private void Init()
	{
		selectionCircleNode = World.LoadNode(selectionCircle.Path);
		if(selectionCircleNode == null)
		{
			Log.Error($"UnitSelectionCircle::init(): cannot load node {selectionCircle.Path}\n");
			return;
		}

		// Parent to unit so indicator follows movement
		selectionCircleNode.Parent = node;
		selectionCircleNode.Position = offset;
		// Start hidden until selected
		selectionCircleNode.Enabled = false;
	}
}
