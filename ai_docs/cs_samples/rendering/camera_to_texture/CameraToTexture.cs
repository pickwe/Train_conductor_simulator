// Renders a secondary camera view onto a texture that is applied to an object's
// material. This creates a "TV screen" or "security camera" effect where one
// camera's view is displayed on a surface in the scene.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Renders camera view to texture for display on object surface.
public partial class CameraToTexture : Component
{
	// Camera whose view will be rendered to texture
	public Player player_camera;
	// Render target texture where camera view is stored
	private Texture texture = new();
	// Viewport used to render camera to texture
	private Viewport viewport = new();

	// Creates texture, inherits material, and binds texture to albedo slot.
	void Init()
	{
		// Create 256x256 HDR texture with linear filtering for camera output
		texture.Create2D(256, 256, Texture.FORMAT_RG11B10F, Texture.SAMPLER_FILTER_LINEAR | Texture.SAMPLER_ANISOTROPY_16 | Texture.FORMAT_USAGE_RENDER);
		Object obj = node as Object;
		Material mat = obj.GetMaterial(0);
		// Inherit material so changes don't affect other objects using same material
		mat = mat.Inherit();
		// Assign our render texture to the material's albedo slot
		mat.SetTexture(mat.FindTexture("albedo"), texture);
		// Adjust UV transform to handle flipped rendering if needed
		mat.SetParameterFloat4("uv_transform", new vec4(-1.0f, Render.IsFlipped ? -1.0f : 1.0f, 0.0f, 0.0f));
		obj.SetMaterial(mat, 0);
	}

	// Camera view is rendered to texture every frame.
	void Update()
	{
		// Render the player camera's view into our texture
		viewport.RenderTexture2D(player_camera.Camera, texture);
	}
}
