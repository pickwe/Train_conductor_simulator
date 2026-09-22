// Demonstrates rendering a mesh to a texture using two different approaches:
// 1. Renderer.RenderMesh - high-level API that handles camera setup automatically
// 2. MeshRender.Render - low-level API giving full control over render state and shaders
// The result is displayed on a preview plane with an interactive manipulator to
// move the render camera. Useful for creating silhouettes, masks, or custom render passes.

using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Renders mesh to texture using high-level or low-level rendering approaches.
public partial class MeshToMaskTextureSample : Component
{
	// The mesh object to render to texture
	[ShowInEditor, Parameter(Title = "Mesh node")]
	private Node mesh_node = null;

	// Plane that displays the rendered texture result
	[ShowInEditor, Parameter(Title = "Object view", Tooltip = "Display plane with material that shows generated texture")]
	private Node object_view = null;

	// Position from which the mesh is rendered (camera position)
	[ShowInEditor, Parameter(Title = "View point")]
	private Node view_point = null;

	// Dummy player used for camera-based rendering approach
	[ShowInEditor, Parameter(Title = "Player node")]
	private Node player_node = null;

	// Custom material with shader for rendering the mesh
	[ShowInEditor, Parameter(Title = "Render material")]
	private Material material = null;

	// Runtime mesh data loaded from the mesh node
	private Mesh mesh_to_render;
	// Target texture where mesh is rendered
	private Texture texture;
	// Player camera for high-level rendering mode
	private PlayerDummy player;

	// Widget for interactive camera position control
	private WidgetManipulatorTranslator manipulator = null;
	// UI window for sample settings
	private SampleDescriptionWindow window = null;
	// Switches between high-level (false) and low-level (true) rendering
	private bool is_manual = false;

	// Configuration data for low-level mesh rendering
	private class RenderData
	{
		public Mat4 mesh_transform = Mat4.IDENTITY;
		public Vec3 camera_position = Vec3.ZERO;
		public ivec2 camera_size = new ivec2(10, 10);
		public float zNear = 0.01f;
		public float zFar = 25.0f;
		public Material material = null;
		public string pass_name = null;
	}

	// Validates parameters, loads mesh, creates render texture, and sets up preview display.
	private void Init()
	{
		if (!view_point)
		{
			Log.Error("MeshToMaskTextureSample.Init: The camera point is unspecified\n");
			return;
		}
		if (!material)
		{
			Log.Error("MeshToMaskTextureSample.Init: The render material is unspecified\n");
			return;
		}

		var objectMeshStaticNode = mesh_node as ObjectMeshStatic;
		if (!objectMeshStaticNode)
		{
			Log.Error("MeshToMaskTextureSample.Init: Mesh to render node is unspecified or not ObjectMeshStatic\n");
			return;
		}

		player = player_node as PlayerDummy;
		if (!player)
		{
			Log.Error("MeshToMaskTextureSample.Init: Player node is unspecified or not PlayerDummy\n");
			return;
		}

		// Load mesh geometry from file into CPU-side mesh object
		mesh_to_render = new Mesh(objectMeshStaticNode.MeshPath);
		if (!mesh_to_render)
		{
			Log.Error("MeshToMaskTextureSample.Init: Mesh for rendering is invalid\n");
			return;
		}

		InitGui();

		// Create render target texture with appropriate format for offscreen rendering
		texture = new Texture();
		texture.Create2D(
			256, 256,
			Texture.FORMAT_RGBA8,
			(int)(Texture.SAMPLER_FILTER_LINEAR | Texture.SAMPLER_ANISOTROPY_16 | Texture.FORMAT_USAGE_RENDER)
		);
		texture.ClearBuffer(vec4.BLACK);

		// Bind rendered texture to preview plane's albedo slot
		var viewObj = object_view as ObjectMeshStatic;
		if (viewObj)
		{
			var mat = viewObj.GetMaterial(0).Inherit();
			int albedoId = mat.FindTexture("albedo");
			if (albedoId >= 0)
				mat.SetTexture(albedoId, texture);

			// Adjust UV to handle potential render target flipping
			mat.SetParameterFloat4("uv_transform",
				new vec4(-1.0f, Render.IsFlipped ? -1.0f : 1.0f, 0.0f, 0.0f));

			viewObj.SetMaterial(mat, 0);
		}
		else
		{
			Log.Warning("MeshToMaskTextureSample.Init: 'Object view' is not ObjectMeshStatic, skip binding preview material.\n");
		}

		// Create manipulator widget for interactive camera positioning
		var gui = Gui.GetCurrent();
		manipulator = new WidgetManipulatorTranslator(gui)
		{
			Transform = view_point.WorldTransform
		};
		gui.AddChild(manipulator);

		// Update camera position when manipulator is moved
		manipulator.EventChanged.Connect(() =>
		{
			var transform = manipulator.Transform;
			view_point.WorldPosition = transform.Translate;
			player.WorldTransform = transform;
			player.WorldLookAt(mesh_node.WorldPosition);
		});

		Visualizer.Enabled = true;
	}

	// Creates UI with dropdown to switch between rendering modes.
	private void InitGui()
	{
		window = new SampleDescriptionWindow();
		window.createWindow();

		var paramsBox = window.getParameterGroupBox();

		var hbox = new WidgetHBox();
		paramsBox.AddChild(hbox, Gui.ALIGN_LEFT);

		var modeLabel = new WidgetLabel("Rendering class:");
		hbox.AddChild(modeLabel, Gui.ALIGN_LEFT);

		var mode = new WidgetComboBox();
		hbox.AddChild(mode, Gui.ALIGN_LEFT);
		mode.AddItem("Renderer.RenderMesh");
		mode.AddItem("MeshRender.Render");

		mode.EventChanged.Connect(() =>
		{
			is_manual = mode.CurrentItem == 1;
		});
	}

	// Renders mesh to texture using selected method and shows camera frustum.
	private void Update()
	{
		var player = Game.Player;
		// Keep manipulator synchronized with current camera matrices
		if (manipulator != null && player != null)
		{
			manipulator.Projection = player.Projection;
			manipulator.Modelview = player.Camera.Modelview;
		}

		if (is_manual)
		{
			// Low-level mode: manually set up all render state and shader parameters
			Visualizer.Enabled = true;

			var data = new RenderData
			{
				mesh_transform = mesh_node.WorldTransform,
				camera_position = view_point.WorldPosition,
				material = material,
				pass_name = "mesh",
				camera_size = new ivec2(10, 10),
				zNear = 0.01f,
				zFar = 25.0f
			};

			// Create orthographic projection for mask-style rendering
			float halfSize = 10.0f * 0.5f;
			Mat4 projection = MathLib.Ortho(-halfSize, halfSize, -halfSize, halfSize, data.zNear, data.zFar);
			Mat4 camera_transform = MathLib.SetTo(
				new Vec3(data.camera_position),
				data.mesh_transform.Translate,
				vec3.UP,
				MathLib.AXIS.NZ
			);

			// Show render camera frustum for debugging
			Visualizer.RenderFrustum(new mat4( projection), camera_transform, vec4.RED);

			RenderMesh(mesh_to_render, texture, data);
		}
		else
		{
			// High-level mode: use Renderer.RenderMesh which handles matrices internally
			Visualizer.RenderFrustum(this.player.Projection, this.player.WorldTransform, vec4.RED);
			RenderMesh(mesh_to_render, texture, material, "mesh", mesh_node.WorldTransform, this.player.Camera);
		}
	}

	// Cleans up manipulator widget and UI window.
	private void Shutdown()
	{
		if (manipulator != null)
		{
			manipulator.DeleteLater();
			manipulator = null;
		}
		if (window != null)
		{
			window.shutdown();
			window = null;
		}
	}

	// Low-level rendering: manually configures all render state, matrices, and shader parameters.
	private void RenderMesh(Mesh mesh, Texture targetTexture, RenderData data)
	{
		if (!targetTexture) { Log.Error("MeshToMaskTextureSample.RenderMesh: texture is null\n"); return; }
		if (!mesh) { Log.Error("MeshToMaskTextureSample.RenderMesh: mesh is null\n"); return; }
		if (data.material == null) { Log.Error("MeshToMaskTextureSample.RenderMesh: material is null\n"); return; }
		if (string.IsNullOrEmpty(data.pass_name)) { Log.Error("MeshToMaskTextureSample.RenderMesh: pass name is null or empty\n"); return; }

		targetTexture.ClearBuffer(vec4.BLACK);

		// Create GPU-ready mesh for rendering
		var meshRender = new MeshRender();
		meshRender.Load(mesh);

		// Build orthographic projection matrix
		double half_width = data.camera_size.x * 0.5;
		double half_height = data.camera_size.y * 0.5;

		mat4 projection = MathLib.Ortho(
			(float)(-half_width), (float)(half_width),
			(float)(-half_height), (float)(half_height),
			data.zNear, data.zFar
		);

		// Camera looks at mesh from specified position
		Mat4 camera_transform = MathLib.SetTo(
			new Vec3(data.camera_position),
			data.mesh_transform.Translate,
			vec3.UP,
			MathLib.AXIS.NZ
		);

		// Set up render target for offscreen rendering
		var renderTarget = Render.GetTemporaryRenderTarget();
		RenderState.SaveState();
		RenderState.ClearStates();
		RenderState.SetBlendFunc(RenderState.BLEND_NONE, RenderState.BLEND_NONE);
		RenderState.ClearBuffer(RenderState.BUFFER_COLOR, vec4.BLACK);

		renderTarget.BindColorTexture(0, targetTexture);
		renderTarget.Enable();

		// Set up camera matrices for the renderer
		Renderer.Modelview = MathLib.Inverse(camera_transform);
		Renderer.OldModelview = MathLib.Inverse(camera_transform);
		Renderer.Projection = projection;
		Renderer.OldProjection = projection;

		// Transform mesh from world space to view space for vertex shader
		vec4[] transforms = new vec4[3];
		Mat4 local = data.mesh_transform;
		mat4 rel = (mat4)(MathLib.Inverse(camera_transform) * local);

		transforms[0] = rel.GetRow(0);
		transforms[1] = rel.GetRow(1);
		transforms[2] = rel.GetRow(2);

		// Configure shader with transform data
		var shader = data.material.GetShaderForce(data.pass_name);
		shader.SetParameterArrayFloat4("s_transform", transforms, 3);

		// Bind material and shader parameters before rendering
		var pass = data.material.GetRenderPass(data.pass_name);
		Renderer.SetMaterial(pass, data.material);
		Renderer.SetShaderParameters(pass,data.material,false);

		// Execute the draw call
		meshRender.Render(MeshRender.MODE_TRIANGLES);

		// Clean up render target
		renderTarget.Disable();
		renderTarget.UnbindAll();
		RenderState.RestoreState();
		Render.ReleaseTemporaryRenderTarget(renderTarget);
	}

	// High-level rendering: uses Renderer.RenderMesh for simpler camera-based mesh rendering.
	private void RenderMesh(Mesh mesh, Texture targetTexture, Material mat, string pass_name, Mat4 mesh_transform, Camera camera)
	{
		if (!targetTexture) { Log.Error("MeshToMaskTextureSample.RenderMesh: texture is null\n"); return; }
		if (!mesh) { Log.Error("MeshToMaskTextureSample.RenderMesh: mesh is null\n"); return; }
		if (!mat) { Log.Error("MeshToMaskTextureSample.RenderMesh: material is null\n"); return; }
		if (string.IsNullOrEmpty(pass_name)) { Log.Error("MeshToMaskTextureSample.RenderMesh: pass name is null or empty\n"); return; }

		targetTexture.ClearBuffer(vec4.BLACK);

		var meshRender = new MeshRender();
		meshRender.Load(mesh);

		// Set up render target for offscreen rendering
		var renderTarget = Render.GetTemporaryRenderTarget();
		RenderState.SaveState();
		RenderState.ClearStates();
		RenderState.SetBlendFunc(RenderState.BLEND_NONE, RenderState.BLEND_NONE);
		RenderState.ClearBuffer(RenderState.BUFFER_COLOR, vec4.BLACK);

		renderTarget.BindColorTexture(0, targetTexture);
		renderTarget.Enable();

		// High-level API handles all matrix setup internally
		Renderer.RenderMesh(meshRender, mat, pass_name, mesh_transform, camera);

		renderTarget.Disable();
		renderTarget.UnbindAll();
		RenderState.RestoreState();
		Render.ReleaseTemporaryRenderTarget(renderTarget);
	}
}
