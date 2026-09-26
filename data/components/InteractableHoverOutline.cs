using Unigine;

/// <summary>Toggles the auxiliary material pass on the object under the crosshair.</summary>
public static class InteractableHoverOutline
{
	private static Unigine.Object outlinedObject;
	private static int[] previousAuxiliaryStates;

	public static void Draw(Unigine.Object target)
	{
		if (target == outlinedObject)
			return;

		RestorePreviousObject();
		if (target == null)
			return;

		outlinedObject = target;
		previousAuxiliaryStates = new int[outlinedObject.NumSurfaces];
		for (int surface = 0; surface < outlinedObject.NumSurfaces; surface++)
		{
			previousAuxiliaryStates[surface] = outlinedObject.GetMaterialState("auxiliary", surface);
			outlinedObject.SetMaterialState("auxiliary", 1, surface);
		}
	}

	private static void RestorePreviousObject()
	{
		if (outlinedObject != null && previousAuxiliaryStates != null)
		{
			int count = MathLib.Min(outlinedObject.NumSurfaces, previousAuxiliaryStates.Length);
			for (int surface = 0; surface < count; surface++)
				outlinedObject.SetMaterialState("auxiliary", previousAuxiliaryStates[surface], surface);
		}

		outlinedObject = null;
		previousAuxiliaryStates = null;
	}

	public static void Shutdown()
	{
		RestorePreviousObject();
	}
}
