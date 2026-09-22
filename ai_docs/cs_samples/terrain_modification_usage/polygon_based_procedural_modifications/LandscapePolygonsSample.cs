// Demonstrates landscape polygon editing and mesh generation. Allows
// creating meshes, decals, and terrain modifications from editable polygon
// figures with interactive manipulator controls.

using System.Collections.Generic;
using Unigine;
using System.Linq;

#region Math aliases
#if UNIGINE_DOUBLE
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;

#endif
#endregion

// Polygon-based landscape editing with mesh and decal generation.
public partial class LandscapePolygonsSample : Component
{
	// Polygon figure nodes with child points
	[ShowInEditor, Parameter(Title = "Figures")]
	private Node[] figures = null;

	// Landscape layer map for terrain modifications
	[ShowInEditor, Parameter(Title = "Layer Map")]
	private Node layerMapNode = null;

	// Manipulator component for figure/point editing
	[ShowInEditor, Parameter(Title = "Manipulators")]
	private Manipulators manipulators = null;

	// Material for ortho decal rendering
	[ShowInEditor, Parameter(Title = "DecalOrtho material")]
	private Material orthoMaterial = null;

	// Material for mesh rendering to texture
	[ShowInEditor, Parameter(Title = "Mesh render material")]
	private Material meshMaterial = null;

	// Clutter node for mask-based placement
	[ShowInEditor, Parameter(Title = "Clutter node")]
	private Node clutterNode = null;

	// Layer map reference
	private LandscapeLayerMap layerMap;
	// Clutter reference
	private ObjectMeshClutter clutter;

	// Textures created for ortho decals
	private readonly List<Texture> orthoDecalTextures = new List<Texture>();
	// Created nodes for cleanup
	private readonly List<Node> created_nodes = new List<Node>();
	// Initial figure positions for reset
	private readonly List<Vec3> figuresInitialPositions = new List<Vec3>();
	// Initial point positions for reset
	private readonly List<List<Vec3>> pointsInitialPositions = new List<List<Vec3>>();

	// Sample UI window
	private SampleDescriptionWindow descriptionWindow = null;
	// Polygon editor for layer map operations
	private LayerMapPolygonEditor layerMapEditor = null;

	// References are validated and UI is created.
	[Method(InvokeDisabled = false, Order = 2)]
	private void Init()
	{
		if (figures != null)
		{
			for (int i = 0; i < figures.Count(); i++)
			{
				if (!figures[i])
				{
					Log.Warning("LandscapePolygonsSample.Init: invalid figures array value.\n");
					return;
				}
				else
				{
					figuresInitialPositions.Add(figures[i].WorldPosition);
					int child_number = figures[i].NumChildren;
					pointsInitialPositions.Add(new List<Vec3>());
					for (int j = 0; j < child_number; j++)
					{
						pointsInitialPositions.Last().Add(figures[i].GetChild(j).WorldPosition);
					}
				}
			}

			if (manipulators == null)
			{
				Log.Warning("LandscapePolygonsSample.Init:  Manipulators component is unspecified.\n");
				return;
			}
			manipulators.SetAxesRotation(false);
			manipulators.SetAxesScale(false);
			clutter = clutterNode as ObjectMeshClutter;
			if (!clutter)
			{
				Log.Warning("LandscapePolygonsSample.Init: clutter node is unspecified.\n");
				return;
			}

			layerMap = layerMapNode as LandscapeLayerMap;
			if (!layerMap)
			{
				Log.Warning("LandscapePolygonsSample.Init: layer map is unspecified.\n");
				return;
			}

			if (!meshMaterial)
			{
				Log.Warning("LandscapePolygonsSample.Init: mesh_material is unspecified.\n");
				return;
			}

			layerMapEditor = new LayerMapPolygonEditor(layerMap, meshMaterial);
			layerMapEditor.SetClutter(clutter);

			if (!orthoMaterial)
			{
				Log.Warning("LandscapePolygonsSample.Init: ortho_material is unspecified.\n");
				return;
			}

			InitGui();
			Visualizer.Enabled = true;
		}
	}

	// Polygon triangulation is visualized each frame.
	private void Update()
	{
		DrawTriangulationAll();
	}

	// Created nodes are cleaned up on shutdown.
	private void Shutdown()
	{
		// Reset all modifications and cleanup
		Reset();
		// Destroy UI window
		if (descriptionWindow != null)
		{
			descriptionWindow.shutdown();
			descriptionWindow = null;
		}
		// Disable landscape mask visualization
		Render.ShowLandscapeMask = 0;
	}

	// Restores all figure and point positions to initial state.
	private void RevertPointsPositions()
	{
		for (int figure = 0; figure < figures.Count(); figure++)
		{
			figures[figure].WorldPosition = figuresInitialPositions[figure];
			int child_number = figures[figure].NumChildren;
			pointsInitialPositions.Add(new List<Vec3>());
			for (int point = 0; point < child_number; point++)
			{
				figures[figure].GetChild(point).WorldPosition = pointsInitialPositions[figure][point];
			}
		}
		manipulators.UpdateManipulatorTransform();
	}

	private void InitGui()
	{
		descriptionWindow = new SampleDescriptionWindow();
		descriptionWindow.createWindow();

		var paramsBox = descriptionWindow.getParameterGroupBox();

		var figureHBox = new WidgetHBox();
		paramsBox.AddChild(figureHBox, Gui.ALIGN_LEFT);

		var selectedFigureLabel = new WidgetLabel("Manipulator transform mode:   ");
		figureHBox.AddChild(selectedFigureLabel, Gui.ALIGN_LEFT);

		var manipulatorComboBox = new WidgetComboBox();
		figureHBox.AddChild(manipulatorComboBox);
		manipulatorComboBox.AddItem("Figure");
		manipulatorComboBox.AddItem("Point");
		manipulatorComboBox.EventChanged.Connect(() =>
		{
			int item = manipulatorComboBox.CurrentItem;
			switch (item)
			{
				case 0: // Figure
					manipulators.SetTransformParent(true);
					break;
				case 1: // Point
					manipulators.SetTransformParent(false);
					break;
			}
		});
		manipulatorComboBox.CurrentItem = 0;

		var renderMaskHBox = new WidgetHBox();
		paramsBox.AddChild(renderMaskHBox, Gui.ALIGN_LEFT);

		var showClutterMaskLabel = new WidgetLabel("Show layer map's clutter mask:   ");
		renderMaskHBox.AddChild(showClutterMaskLabel, Gui.ALIGN_LEFT);

		var renderMaskCheckBox = new WidgetCheckBox();
		renderMaskHBox.AddChild(renderMaskCheckBox);
		renderMaskCheckBox.EventChanged.Connect(() =>
		{
			if (renderMaskCheckBox.Checked)
			{
				foreach (var node in created_nodes)
				{
					node.Enabled = false;

				}
				Render.ShowLandscapeMask = 1;
			}
			else
			{
				foreach (var node in created_nodes)
				{
					node.Enabled = true;
				}
				Render.ShowLandscapeMask = 0;
			}
		});

		var featuresLabel = new WidgetLabel("Generation features:");
		paramsBox.AddChild(featuresLabel, Gui.ALIGN_LEFT);

		var buttons_grid_box = new WidgetGridBox(3, 2, 2);
		paramsBox.AddChild(buttons_grid_box);

		var button = new WidgetButton("ObjectMeshStatic");
		button.EventClicked.Connect(GenerateMeshButton);
		buttons_grid_box.AddChild(button, Gui.ALIGN_EXPAND);

		button = new WidgetButton("DecalMesh");
		button.EventClicked.Connect(GenerateDecalMeshButton);
		buttons_grid_box.AddChild(button, Gui.ALIGN_EXPAND);

		button = new WidgetButton("DecalOrtho");
		button.EventClicked.Connect(GenerateDecalOrthoButton);
		buttons_grid_box.AddChild(button, Gui.ALIGN_EXPAND);

		button = new WidgetButton("Terrain mask");
		button.EventClicked.Connect(DrawTerrainMaskButton);
		buttons_grid_box.AddChild(button, Gui.ALIGN_EXPAND);

		button = new WidgetButton("Level terrain");
		button.EventClicked.Connect(LevelTerrainButton);
		buttons_grid_box.AddChild(button, Gui.ALIGN_EXPAND);

		button = new WidgetButton("Lower terrain");
		button.EventClicked.Connect(LowerTerrainButton);
		buttons_grid_box.AddChild(button, Gui.ALIGN_EXPAND);

		var resetBtn = new WidgetButton("Reset");
		resetBtn.EventClicked.Connect(Reset);
		paramsBox.AddChild(resetBtn, Gui.ALIGN_BOTTOM);
	}


	// Resets all terrain modifications and removes created nodes.
	private void Reset()
	{
		// Asynchronously reset landscape layer map modifications
		if (layerMap)
			Landscape.AsyncResetModifications(layerMap.GetGUID());

		// Delete all created mesh/decal nodes
		foreach (var n in created_nodes)
			if (n) n.DeleteLater();
		created_nodes.Clear();
		// Refresh clutter to reflect mask changes
		if (clutter)
			clutter.Invalidate();
		RevertPointsPositions();
	}

	// Triangulates all polygon figures and draws wireframe visualization.
	private void DrawTriangulationAll()
	{
		if (figures == null) return;

		foreach (var figure in figures)
		{
			if (!figure) continue;

			int count = figure.NumChildren;
			if (count < 3) continue;

			// Collect 3D points and 2D projections for triangulation
			var points = new List<Vec3>(count);
			var mesh_points = new List<vec2>(count);
			var indices = new List<ushort>();

			double avgZ = 0.0;
			for (int i = 0; i < count; i++)
			{
				var p = figure.GetChild(i).WorldPosition;
				points.Add(p);
				mesh_points.Add(new vec2(p.x, p.y));
				avgZ += p.z;
			}
			avgZ /= count;

			// Perform 2D polygon triangulation and visualize result
			MathLib.TriangulatePolygon(mesh_points, indices);
			DrawTriangulation(points, indices);
		}
	}

	// Draws triangle edges using the Visualizer.
	private static void DrawTriangulation(List<Vec3> points, IList<ushort> indices)
	{
		for (int i = 0; i + 2 < indices.Count; i += 3)
		{
			int ia = indices[i + 0];
			int ib = indices[i + 1];
			int ic = indices[i + 2];
			Vec3 A = points[ia];
			Vec3 B = points[ib];
			Vec3 C = points[ic];

			Visualizer.RenderLine3D(A, B, vec4.RED);
			Visualizer.RenderLine3D(B, C, vec4.RED);
			Visualizer.RenderLine3D(C, A, vec4.RED);
		}
	}

	// Renders mesh to texture using orthographic projection from above.
	private void RenderMeshToTexture(Mesh mesh, Texture texture, Material render_mat, vec4 color)
	{
		if (!texture) { Log.Error("RenderMeshToTexture: texture is null\n"); return; }
		if (!mesh) { Log.Error("RenderMeshToTexture: mesh is null\n"); return; }
		if (!render_mat) { Log.Error("RenderMeshToTexture: material is null\n"); return; }

		// Create mesh render object for GPU rendering
		var meshRender = new MeshRender();
		meshRender.Load(mesh);

		// Calculate orthographic projection bounds from mesh size
		Mat4 mesh_transform = Mat4.IDENTITY;
		var bb = mesh.BoundBox;
		var bbSize = bb.maximum - bb.minimum;
		double half_width = bbSize.x * 0.5;
		double half_height = bbSize.y * 0.5;

		mat4 projection = MathLib.Ortho(
			(float)(-half_width), (float)(half_width),
			(float)(-half_height), (float)(half_height),
			0.01f, 1000.0f
		);

		// Position camera above mesh looking down
		Mat4 camera_transform = MathLib.SetTo(
			mesh_transform.Translate + Vec3.UP * 10.0f,
			mesh_transform.Translate,
			vec3.FORWARD,
			MathLib.AXIS.NZ
		);

		// Setup render target and state
		var renderTarget = Render.GetTemporaryRenderTarget();
		RenderState.SaveState();
		RenderState.ClearStates();
		RenderState.SetBlendFunc(RenderState.BLEND_NONE, RenderState.BLEND_NONE);
		RenderState.ClearBuffer(RenderState.BUFFER_COLOR, vec4.BLACK);

		renderTarget.BindColorTexture(0, texture);
		renderTarget.Enable();

		// Set camera matrices for rendering
		Renderer.Modelview = MathLib.Inverse(camera_transform);
		Renderer.OldModelview = MathLib.Inverse(camera_transform);
		Renderer.Projection = projection;
		Renderer.OldProjection = projection;

		// Setup shader transform parameters
		vec4[] transforms = new vec4[3];
		mat4 rel = (mat4)(MathLib.Inverse(camera_transform) * mat4.IDENTITY);

		transforms[0] = rel.GetRow(0);
		transforms[1] = rel.GetRow(1);
		transforms[2] = rel.GetRow(2);

		var shader = render_mat.GetShaderForce("render_mesh");
		shader.SetParameterArrayFloat4("s_transform", transforms, 3);
		render_mat.SetParameterFloat4("mesh_color", color);

		// Render mesh with material
		var pass = render_mat.GetRenderPass("render_mesh");
		Renderer.SetMaterial(pass, render_mat);
		Renderer.SetShaderParameters(pass, render_mat, false);

		meshRender.Render(MeshRender.MODE_TRIANGLES);

		// Cleanup render target and restore state
		renderTarget.Disable();
		renderTarget.UnbindAll();
		RenderState.RestoreState();
		Render.ReleaseTemporaryRenderTarget(renderTarget);
	}

	// Creates ObjectMeshStatic from polygon points for each figure.
	private void GenerateMeshButton()
	{
		if (figures == null) return;

		foreach (var figure in figures)
		{
			if (!figure) continue;

			int count = figure.NumChildren;
			if (count < 3) continue;

			// Collect world positions from child nodes
			var points = new List<Vec3>(count);
			for (int i = 0; i < count; i++)
				points.Add(figure.GetChild(i).WorldPosition);

			WorldBoundBox bb = new WorldBoundBox(points.ToArray());

			Vec2 pixelsPerUnit = (Vec2)layerMap.Resolution / (Vec2)layerMap.Size;
			ivec2 drawingCoord = new ivec2(pixelsPerUnit * new Vec2(bb.minimum - layerMap.WorldPosition));
			ivec2 drawingRez = new ivec2(pixelsPerUnit * new Vec2(bb.maximum - bb.minimum));

			// Generate triangulated mesh from polygon
			var mesh = GeometryGenerator.CreatePolygonMesh(points, false);
			if (mesh == null) continue;

			// Create mesh object with procedural mode
			var meshObj = new ObjectMeshStatic();
			meshObj.SetMeshProceduralMode(ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC, 0);
			meshObj.ApplyMoveMeshProceduralForce(mesh, 0);
			meshObj.WorldPosition = bb.Center;
			meshObj.SetIntersection(true, 0);
			meshObj.SetIntersectionMask(~0, 0);
			meshObj.Immovable = false;

			created_nodes.Add(meshObj);
		}
	}

	// Creates DecalMesh from polygon points for each figure.
	private void GenerateDecalMeshButton()
	{
		if (figures == null) return;

		foreach (var figure in figures)
		{
			if (!figure) continue;

			int count = figure.NumChildren;
			if (count < 3) continue;

			var points = new List<Vec3>(count);
			for (int i = 0; i < count; i++)
				points.Add(figure.GetChild(i).WorldPosition);

			WorldBoundBox bb = new WorldBoundBox(points.ToArray());

			Vec2 pixelsPerUnit = (Vec2)layerMap.Resolution / (Vec2)layerMap.Size;
			ivec2 drawingCoord = new ivec2(pixelsPerUnit * new Vec2(bb.minimum - layerMap.WorldPosition));
			ivec2 drawingRez = new ivec2(pixelsPerUnit * new Vec2(bb.maximum - bb.minimum));

			var mesh = GeometryGenerator.CreatePolygonMesh(points, make_flat: false);
			if (mesh == null) continue;

			// Create decal mesh with polygon shape
			var decal = new DecalMesh();
			decal.SetMeshProceduralMode(ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC, 0);
			decal.ApplyMoveMeshProceduralForce(mesh, 0);
			decal.WorldPosition = new Vec3(bb.Center.x, bb.Center.y, bb.Center.z + 10.0f);
			decal.Radius = 200.0f;
			var mat = Materials.FindManualMaterial("Unigine::decal_base");
			decal.Material = mat;
			decal.ViewportMask = ~0;
			decal.SaveToWorldEnabled = true;

			created_nodes.Add(decal);
		}
	}

	// Creates DecalOrtho with polygon rendered to texture for each figure.
	private void GenerateDecalOrthoButton()
	{
		if (figures == null) return;

		foreach (var figure in figures)
		{
			if (!figure) continue;

			int count = figure.NumChildren;
			if (count < 3) continue;

			var points = new List<Vec3>(count);
			for (int i = 0; i < count; i++)
				points.Add(figure.GetChild(i).WorldPosition);

			WorldBoundBox bb = new WorldBoundBox(points.ToArray());

			Vec2 pixelsPerUnit = (Vec2)layerMap.Resolution / (Vec2)layerMap.Size;
			ivec2 drawingCoord = new ivec2(pixelsPerUnit * new Vec2(bb.minimum - layerMap.WorldPosition));
			ivec2 drawingRez = new ivec2(pixelsPerUnit * new Vec2(bb.maximum - bb.minimum));
			var bbSize = bb.maximum - bb.minimum;

			var mesh = GeometryGenerator.CreatePolygonMesh(points, make_flat: false);
			if (mesh == null) continue;

			// Create texture and render mesh silhouette to it
			var tex = new Texture();
			tex.Create2D(512, 512, Texture.FORMAT_RGBA8,
				(int)(Texture.FORMAT_USAGE_UNORDERED_ACCESS | Texture.FORMAT_USAGE_RENDER));
			tex.ClearBuffer(vec4.ZERO);

			RenderMeshToTexture(mesh, tex, meshMaterial, vec4.WHITE);
			orthoDecalTextures.Add(tex);

			// Create material with rendered texture as albedo
			var mat = orthoMaterial.Inherit();
			int albedoId = mat.FindTexture("albedo");
			if (albedoId >= 0)
				mat.SetTexture(albedoId, tex);

			// Create ortho decal with matching dimensions
			var decal = new DecalOrtho();
			decal.WorldPosition = new Vec3(bb.Center.x, bb.Center.y, bb.Center.z + 10.0f);
			decal.Radius = 200.0f;
			decal.Material = mat;
			decal.Width = (float)bbSize.x;
			decal.Height = (float)bbSize.y;
			decal.SaveToWorldEnabled = true;

			created_nodes.Add(decal);
		}
	}

	// Renders polygon shape to landscape clutter mask.
	private void DrawTerrainMaskButton()
	{
		if (figures == null || layerMapEditor == null) return;

		foreach (var figure in figures)
		{
			if (!figure) continue;

			int count = figure.NumChildren;
			if (count < 3) continue;

			var pts = new List<Vec3>(count);
			for (int i = 0; i < count; i++)
				pts.Add(figure.GetChild(i).WorldPosition);

			layerMapEditor.GenerateMeshOnMask(pts, (int)Landscape.FLAGS_DATA.MASK_0);
		}
	}

	// Levels terrain height within polygon to lowest point.
	private void LevelTerrainButton()
	{
		if (figures == null || layerMapEditor == null) return;

		foreach (var figure in figures)
		{
			if (!figure) continue;

			int count = figure.NumChildren;
			if (count < 3) continue;

			var points = new List<Vec3>(count);
			for (int i = 0; i < count; i++)
				points.Add(figure.GetChild(i).WorldPosition);

			WorldBoundBox bb = new WorldBoundBox(points.ToArray());

			Vec2 pixelsPerUnit = (Vec2)layerMap.Resolution / (Vec2)layerMap.Size;
			ivec2 drawingCoord = new ivec2(pixelsPerUnit * new Vec2(bb.minimum - layerMap.WorldPosition));
			ivec2 drawingRez = new ivec2(pixelsPerUnit * new Vec2(bb.maximum - bb.minimum));
			layerMapEditor.LevelHeightForMesh(points, bb.minimum.z);
		}
	}

	// Lowers terrain by fixed amount within polygon area.
	private void LowerTerrainButton()
	{
		if (figures == null || layerMapEditor == null) return;

		foreach (var figure in figures)
		{
			if (!figure) continue;

			int count = figure.NumChildren;
			if (count < 3) continue;

			var pts = new List<Vec3>(count);
			for (int i = 0; i < count; i++)
				pts.Add(figure.GetChild(i).WorldPosition);

			layerMapEditor.LowerTerrain(pts, 20.0f);
		}
	}
}
