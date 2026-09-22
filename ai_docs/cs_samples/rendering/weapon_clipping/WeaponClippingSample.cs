// Provides UI controls for the weapon clipping demonstration sample.
// Allows toggling shadow rendering for the weapon layer, which can be
// skipped for better performance since weapon shadows are rarely needed.
// Also handles mouse capture for first-person camera control.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// UI controller for weapon clipping sample with shadow toggle option.
public partial class WeaponClippingSample : Component
{
	// Reference to the weapon clipping rendering component
	[ShowInEditor]
	private WeaponClipping weaponClipping = null;

	// UI window for sample controls
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	// Stores original mouse mode to restore on shutdown
	private Input.MOUSE_HANDLE mouse_handle = Input.MOUSE_HANDLE.USER;

	// Sets up UI and captures mouse for first-person camera control.
	void Init()
	{
		sampleDescriptionWindow.createWindow();

		// Grab mouse for FPS-style camera look
		mouse_handle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;

		// Add toggle for shadow rendering on weapon layer
		sampleDescriptionWindow.addBoolParameter("Skip Shadow", null, true, OnShadowsCheckboxChanged);
	}

	// Updates weapon viewport skip flags based on shadow checkbox state.
	private void OnShadowsCheckboxChanged(bool is_checked)
	{
		if (weaponClipping == null)
		{
			Log.Message("WeaponClippingSample::OnShadowsCheckboxChanged(): weaponClippingNode is not set");
			return;
		}

		// Always skip velocity buffer, optionally skip shadows for performance
		int flags = Viewport.SKIP_VELOCITY_BUFFER;
		if (is_checked)
		{
			flags |= Viewport.SKIP_SHADOWS;
		}
		weaponClipping.RenderViewport.SkipFlags = flags;
	}

	// Restores original mouse mode and cleans up UI.
	private void Shutdown()
	{
		Input.MouseHandle = mouse_handle;
		sampleDescriptionWindow.shutdown();
	}
}
