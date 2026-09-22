// Renders a custom GUI onto a texture that can be applied to any 3D object's
// material. This enables in-world UI displays like monitors, signs, or interactive
// panels. The GUI is rendered using a separate render target and can be updated
// automatically or manually for performance optimization.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unigine;
using Object = Unigine.Object;

// Renders GUI widgets onto a texture for display on 3D surfaces.
public partial class GuiToTexture : Component
{
	// Name of the surface where texture will be applied
	[ShowInEditor] private string surfaceName = "";

	// Material texture slot names to bind the GUI texture
	[ShowInEditor] private string[] textureSlotNames = [];

	// Resolution of the render texture (higher = sharper but more expensive)
	[ShowInEditor] public ivec2 TextureResolution = new(2048, 2048);

	// Texture that receives the rendered GUI output
	private Texture guiTexture;

	// Render target used for offscreen GUI rendering
	private RenderTarget renderTarget;

	// When true, GUI is re-rendered every frame automatically
	public bool AutoUpdateEnabled = true;

	// The GUI instance that widgets can be added to
	public Gui Gui { get; private set; }

	// Performs the full GUI render loop: saves state, renders widgets, restores state.
	public void RenderToTexture()
	{
		// Save current render state so we can restore it after GUI rendering
		RenderState.SaveState();

		// Now we clear state, so that our rendered texture won't be affected by other render activities
		RenderState.ClearStates();

		// Set viewport size matching texture resolution
		RenderState.SetViewport(0, 0, TextureResolution.x, TextureResolution.y);

		// Now we bind gui texture to slot 0, because gui renders in slot 0
		renderTarget.BindColorTexture(0, guiTexture);
		// Enable render target
		renderTarget.Enable();

		// Clear texture and fill it with black color
		RenderState.ClearBuffer(RenderState.BUFFER_COLOR, vec4.BLACK);

		// Now we need to perform the whole gui render loop

		// Enable gui so that it will be updated and rendered
		Gui.Enable();

		// Update all widgets
		Gui.Update();

		// Render gui
		Gui.PreRender();
		Gui.Render();

		// Disable gui
		Gui.Disable();

		// Now we need to free render target and unbind texture
		renderTarget.Disable();
		renderTarget.UnbindColorTexture(0);

		// Create texture mipmaps (set of textures of different resolutions to ensure correct rendering at longer distances)
		guiTexture.CreateMipmaps();

		// Pop render state from top of the stack to let render pipeline continue as usual
		RenderState.RestoreState();
	}

	// Creates GUI, texture, and render target. Runs before other components (Order = -1).
	[MethodInit(Order = -1)]
	void Init()
	{
		// Get the object this component is attached to
		var obj = node as Object;
		if (obj == null)
		{
			Log.Error("GuiToTexture.Init(): component must be assigned to an object");
			return;
		}

		// Find the required surface
		int surface = obj.FindSurface(surfaceName);
		if (surface == -1)
		{
			Log.Error("GuiToTexture.Init(): surface with name %s not found", surfaceName);
			return;
		}


		renderTarget = new RenderTarget();

		// We need to inherit material, because there might be other objects that are using this material
		// and we don't want all objects in the scene to get gui from this component
		Material material = obj.GetMaterialInherit(surface);

		Gui = Gui.Create();
		Gui.Size = TextureResolution;

		guiTexture = new Texture();

		// here we need to specify format of texture: rgba8
		// and set the flag FORMAT_USAGE_RENDER to be able to render into the texture
		// we also need to specify the sampler by setting another flag (bilinear sampler in our case)
		guiTexture.Create2D(TextureResolution.x, TextureResolution.y, Texture.FORMAT_RGBA8,
			Texture.FORMAT_USAGE_RENDER | Texture.SAMPLER_FILTER_BILINEAR);

		for (int i = 0, num_textures = textureSlotNames.Length; i < num_textures; i++)
		{
			material.SetTexture(textureSlotNames[i], guiTexture);
		}
	}

	// GUI is rendered to texture each frame if auto-update is enabled.
	void Update()
	{
		if (AutoUpdateEnabled)
			RenderToTexture();
	}

}
