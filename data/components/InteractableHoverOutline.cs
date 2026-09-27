using System.Collections.Generic;
using Unigine;

/// <summary>Toggles the auxiliary material pass on an interactable node hierarchy.</summary>
public static class InteractableHoverOutline
{
	private sealed class ObjectState
	{
		public Unigine.Object Target;
		public int[] AuxiliaryStates;
	}

	private static Node outlinedRoot;
	private static Unigine.Object sourceHit;
	private static readonly List<ObjectState> outlinedObjects = new List<ObjectState>();

	public static void Draw(Node targetRoot, Unigine.Object hitObject)
	{
		if (targetRoot == outlinedRoot && hitObject == sourceHit)
			return;

		RestorePreviousObjects();
		if (targetRoot == null)
			return;

		outlinedRoot = targetRoot;
		sourceHit = hitObject;
		OutlineEnabledHierarchy(targetRoot);
	}

	private static void OutlineEnabledHierarchy(Node current)
	{
		if (current == null || !current.Enabled)
			return;

		Unigine.Object currentObject = current as Unigine.Object;
		if (currentObject != null && !(currentObject is ObjectGui))
		{
			ObjectState state = new ObjectState
			{
				Target = currentObject,
				AuxiliaryStates = new int[currentObject.NumSurfaces]
			};
			for (int surface = 0; surface < currentObject.NumSurfaces; surface++)
			{
				state.AuxiliaryStates[surface] = currentObject.GetMaterialState("auxiliary", surface);
				currentObject.SetMaterialState("auxiliary", 1, surface);
			}
			outlinedObjects.Add(state);
		}

		for (int child = 0; child < current.NumChildren; child++)
			OutlineEnabledHierarchy(current.GetChild(child));
	}

	private static void RestorePreviousObjects()
	{
		foreach (ObjectState state in outlinedObjects)
		{
			if (state.Target == null || state.AuxiliaryStates == null)
				continue;

			int count = MathLib.Min(state.Target.NumSurfaces, state.AuxiliaryStates.Length);
			for (int surface = 0; surface < count; surface++)
				state.Target.SetMaterialState("auxiliary", state.AuxiliaryStates[surface], surface);
		}

		outlinedObjects.Clear();
		outlinedRoot = null;
		sourceHit = null;
	}

	public static void Shutdown()
	{
		RestorePreviousObjects();
	}
}
