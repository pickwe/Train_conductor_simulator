// Solves the weapon clipping problem in first-person games where the weapon
// clips through walls when the player gets too close. This works by rendering
// the weapon with a separate camera that has a very small near plane, then
// compositing that render over the main scene. The weapon always appears in
// front of world geometry regardless of how close the player is to walls.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unigine;

// Renders weapon separately to prevent clipping through nearby geometry.
public partial class WeaponClipping : Component
{
	// Main game camera that renders the world
	[ShowInEditor]
	private Player mainPlayer = null;

	// Separate camera for rendering only the weapon with different near plane
	[ShowInEditor]
	private Player weaponPlayer = null;

	// Custom viewport for weapon rendering with optimized settings
	private Viewport viewport = null;
	public Viewport RenderViewport { get { return viewport; } }

	// Intermediate texture where weapon is rendered before compositing
	private Texture texture = null;

	// Current render resolution (updates on window resize)
	private int currentWidth = 0;
	private int currentHeight = 0;

	// Flag to prevent recursive rendering during weapon pass
	private bool isRenderingWeapon = false;

	// Manages event connections for cleanup on shutdown
	private EventConnections componentConnections = new EventConnections();

	// Sets up viewport, texture, and render event hooks.
	private void Init()
	{
		EngineWindow main_window = WindowManager.MainWindow;
		if (!main_window)
		{
			Engine.Quit();
			return;
		}

		ivec2 main_size = main_window.ClientRenderSize;

		currentWidth = main_size.x;
		currentHeight = main_size.y;

		// Create viewport with optimized settings for weapon layer
		viewport = new Viewport();
		viewport.NodeLightUsage = Viewport.USAGE_WORLD_LIGHT;
		viewport.SkipFlags = Viewport.SKIP_VELOCITY_BUFFER | Viewport.SKIP_SHADOWS;

		texture = new Texture();
		CreateTexture2d(ref texture);

		// Hook into render pipeline for compositing weapon over scene
		Render.EventBeginPostMaterials.Connect(componentConnections, RenderCallback);
		WindowManager.MainWindow.EventResized.Connect(componentConnections, UpdateScreenSize);
	}

	// Synchronizes weapon camera with main camera each frame.
	private void Update()
	{
		// Keep weapon camera aligned with main camera view
		weaponPlayer.Transform = mainPlayer.Camera.IModelview;
	}

	// Renders weapon to separate texture after main scene update.
	private void PostUpdate()
	{
		if (Game.Player != mainPlayer)
			return;

		RenderState.SaveState();
		RenderState.ClearStates();
		RenderState.SetViewport(0, 0, currentWidth, currentHeight);
		var target = Render.GetTemporaryRenderTarget();
		target.BindColorTexture(0, texture);
		target.Enable();
		{
			// Disable lens flares during weapon render to avoid artifacts
			bool flare = Render.LightsLensFlares;
			Render.LightsLensFlares = false;

			RenderState.ClearBuffer(RenderState.BUFFER_ALL, vec4.ZERO);
			RenderState.FlushStates();

			// Render weapon with separate near plane to transparent texture
			if (texture != null)
			{
				isRenderingWeapon = true;
				viewport.RenderTexture2D(weaponPlayer.Camera, texture);
				isRenderingWeapon = false;
			}

			Render.LightsLensFlares = flare;
		}
		target.Disable();
		target.UnbindColorTexture(0);
		RenderState.RestoreState();
		Render.ReleaseTemporaryRenderTarget(target);
	}

	// Composites weapon texture over main scene during post-materials pass.
	private void RenderCallback()
	{
		if (Game.Player != mainPlayer)
			return;

		if (isRenderingWeapon)
		{
			// Avoid recursive rendering during weapon texture generation
			return;
		}

		RenderState.SaveState();
		RenderState.ClearStates();
		RenderState.SetViewport(0, 0, currentWidth, currentHeight);

		var target = Render.GetTemporaryRenderTarget();
		target.BindColorTexture(0, Renderer.TextureColor);

		target.Enable();
		{
			// Alpha blend weapon layer over the scene
			RenderState.SetBlendFunc(RenderState.BLEND_SRC_ALPHA, RenderState.BLEND_ONE_MINUS_SRC_ALPHA);
			RenderState.FlushStates();

			// Draw weapon texture on top of rendered scene
			if (texture != null)
			{
				Render.RenderScreenMaterial("Unigine::render_copy_2d", texture);
			}
		}
		target.Disable();

		target.UnbindColorTexture(0);
		RenderState.RestoreState();
		Render.ReleaseTemporaryRenderTarget(target);
	}

	// Recreates render texture when window is resized.
	private void UpdateScreenSize()
	{
		EngineWindow main_window = WindowManager.MainWindow;
		if (!main_window)
		{
			Engine.Quit();
			return;
		}

		ivec2 main_size = main_window.ClientRenderSize;

		int appWidth = main_size.x;
		int appHeight = main_size.y;
		if (appWidth != currentWidth || appHeight != currentHeight)
		{
			currentWidth = appWidth;
			currentHeight = appHeight;
			CreateTexture2d(ref texture);
		}
	}

	// Cleans up texture, viewport, and event connections.
	private void Shutdown()
	{
		texture = null;
		viewport = null;
		componentConnections.DisconnectAll();
	}

	// Creates render target texture matching current window dimensions.
	private void CreateTexture2d(ref Texture texture)
	{
		texture.Create2D(currentWidth, currentHeight, Texture.FORMAT_RGBA8,
			Texture.SAMPLER_FILTER_LINEAR | Texture.SAMPLER_ANISOTROPY_16 | Texture.FORMAT_USAGE_RENDER);
	}
}
