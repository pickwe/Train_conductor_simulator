// Demonstrates procedural mesh generation along a spline path. Creates ribbon,
// square pipe, or round pipe geometry from control points with configurable
// width, subdivision, and UV tiling. Supports interactive manipulation of
// control points with real-time mesh regeneration.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;
using System.Drawing;


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

// Generates procedural mesh along spline with ribbon or pipe shapes.
public partial class SplineGenerationSample : Component
{
	// Control points defining the spline path
	[ShowInEditor, Parameter(Title = "Spline node")]
	private List<Node> splineNodes= null;

	// Width of generated mesh
	[ShowInEditor, Parameter(Title = "Mesh Width")]
	private float meshWidthParam = 5.0f;

	// Number of subdivisions across width
	[ShowInEditor, Parameter(Title = "Width Subdivision")]
	private int widthSubdivisionParam = 5;

	// Length of each spline segment
	[ShowInEditor, Parameter(Title = "Segment Length")]
	private float segmentsLengthParam = 3.0f;

	// UV tiling along spline length
	[ShowInEditor, Parameter(Title = "Length Tiling")]
	private float lengthTiling = 1.0f;

	// UV tiling across spline width
	[ShowInEditor, Parameter(Title = "Width Tiling")]
	private float widthTiling = 1.0f;

	// Stretch UVs to fit entire spline length
	[ShowInEditor, Parameter(Title = "Stretch UV Along Length")]
	private bool stretchUVAlongLengthParam = false;

	// Stretch UVs to fit entire mesh width
	[ShowInEditor, Parameter(Title = "Stretch UV Along Width")]
	private bool stretchUVAlongWidthParam = false;

	// Display wireframe overlay
	[ShowInEditor, Parameter(Title = "Show Wireframe")]
	private bool showWireframeParam = false;

	// Material applied to generated mesh
	[ShowInEditor, Parameter(Title = "Spline Object Material")]
	private Material splineObjectMaterial=null;

	// Manipulator gizmos for interactive control
	[ShowInEditor, Parameter(Title = "Manipulators")]
	private Manipulators manipulators = null;

	// Type of geometry to generate
	enum GENERATION_TYPE
	{
		RIBBON,
		SQUARE_PIPE,
		ROUND_PIPE
	};

	// Procedural mesh object for rendering
	private ObjectMeshStatic splineMeshObject = null;
	// UI window reference
	private SampleDescriptionWindow descriptionWindow = null;
	// Computed spline points for visualization
	private List<Vec3> currentSplinePoints = new();
	// Event connections for manipulator callbacks
	private EventConnections eventConnections = new();
	// Last generated shape type for regeneration
	GENERATION_TYPE lastGenerationType = GENERATION_TYPE.RIBBON;

	// Mesh object and UI are initialized.
	private void Init()
	{
		splineMeshObject = new ObjectMeshStatic();
		// Dynamic mode stores mesh in RAM for rendering
		splineMeshObject.SetMeshProceduralMode(ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC, 0);
		splineMeshObject.Immovable = false;

		if (!splineObjectMaterial)
		{
			Log.Warning("SplineGenerationSample: 'Spline Object Material' is unspecified.\n");
			return;
		}

		if (!manipulators)
		{
			Log.Warning("SplineGenerationSample: 'Manipulators' is unspecified.\n");
			return;
		}
		// Regenerate mesh when manipulators are moved
		manipulators.EventTransformChanged.Connect(eventConnections, Regenerate);
		Visualizer.Enabled = showWireframeParam;
		InitGui();
	}

	// UI and visualizer are cleaned up on shutdown.
	private void Shutdown()
	{
		descriptionWindow?.shutdown();
		Visualizer.Enabled = false;
	}

	// Spline points and mesh wireframe are visualized.
	private void Update()
	{
		// Draw control points
		foreach (var point in currentSplinePoints)
		{
			Visualizer.RenderPoint3D(point, 0.1f, vec4.RED);
		}
		// Draw mesh wireframe if enabled
		if (splineMeshObject)
		{
			Visualizer.RenderObjectSurface(splineMeshObject, 0, vec4.WHITE);
		}
	}

	// Generates flat ribbon mesh along spline.
	private void GenerateSplineMesh()
	{
		var points = GetSplinePoints();
		if (!GeometryGenerator.ComputeSplinePoints(points, currentSplinePoints, segmentsLengthParam))
		{
			Log.Message("SplineGenerationSample::GenerateSplineMesh Invalid spline points");
			return;
		}
		var mesh = GeometryGenerator.GenerateSplineMesh(currentSplinePoints, meshWidthParam, widthSubdivisionParam, segmentsLengthParam, GetUVSettings());
		splineMeshObject.ApplyMoveMeshProceduralForce(mesh, 0);
		splineMeshObject.SetMaterial(splineObjectMaterial, 0);
		lastGenerationType = GENERATION_TYPE.RIBBON;
	}

	// Generates square pipe mesh along spline.
	private void GenerateSplineSquarePipe()
	{
		var points = GetSplinePoints();
		if (!GeometryGenerator.ComputeSplinePoints(points, currentSplinePoints, segmentsLengthParam))
		{
			Log.Message("SplineGenerationSample::GenerateSplineSquarePipe Invalid spline points");
			return;
		}
		var mesh = GeometryGenerator.GenerateSplineMeshSquare(currentSplinePoints, meshWidthParam, widthSubdivisionParam, segmentsLengthParam, GetUVSettings());
		splineMeshObject.ApplyMoveMeshProceduralForce(mesh, 0);
		splineMeshObject.SetMaterial(splineObjectMaterial, 0);
		lastGenerationType = GENERATION_TYPE.SQUARE_PIPE;
	}

	// Generates round pipe mesh along spline.
	private void GenerateSplineRoundPipe()
	{
		var points = GetSplinePoints();
		if (!GeometryGenerator.ComputeSplinePoints(points, currentSplinePoints, segmentsLengthParam))
		{
			Log.Message("SplineGenerationSample::GenerateSplineRoundPipe Invalid spline points");
			return;
		}
		var mesh = GeometryGenerator.GenerateSplineMeshRound(currentSplinePoints, meshWidthParam * 0.5f, widthSubdivisionParam, segmentsLengthParam, GetUVSettings());
		splineMeshObject.ApplyMoveMeshProceduralForce(mesh, 0);
		splineMeshObject.SetMaterial(splineObjectMaterial, 0);
		lastGenerationType = GENERATION_TYPE.ROUND_PIPE;
	}

	// Regenerates mesh using last selected generation type.
	private void Regenerate()
	{
		switch (lastGenerationType)
		{
			case GENERATION_TYPE.RIBBON:
				GenerateSplineMesh();
				break;
			case GENERATION_TYPE.SQUARE_PIPE:
				GenerateSplineSquarePipe();
				break;
			case GENERATION_TYPE.ROUND_PIPE:
				GenerateSplineRoundPipe();
				break;
			default:
				GenerateSplineMesh();
				break;
		}
	}

	// Clears generated mesh and spline points.
	private void Clear()
	{
		currentSplinePoints.Clear();
		splineMeshObject.ApplyMoveMeshProceduralForce(new Mesh(), 0);
	}

	// Collects world positions from spline control nodes.
	private List<Vec3> GetSplinePoints()
	{
		var points = new List<Vec3>();
		if (splineNodes==null)
		{
			return points;
		}

		foreach (var node in splineNodes)
		{
			points.Add(node.WorldPosition);
		}
		return points;
	}

	// Creates UV settings structure from current parameters.
	private GeometryGenerator.SplineMeshUVSettings GetUVSettings()
	{
		return new GeometryGenerator.SplineMeshUVSettings
		{
			length_tiling_amount = lengthTiling,
			width_tiling_amount = widthTiling,
			stretch_along_length = stretchUVAlongLengthParam,
			stretch_along_width = stretchUVAlongWidthParam
		};
	}

	// UI window with parameter controls and generation buttons is created.
	private void InitGui()
	{
		descriptionWindow = new SampleDescriptionWindow();
		descriptionWindow.createWindow();
		var paramsBox = descriptionWindow.getParameterGroupBox();

		// Mesh generation parameters
		descriptionWindow.addFloatParameter("Mesh Width", "Mesh Width", meshWidthParam, 0.1f, 10.0f, v => { meshWidthParam = v; Regenerate(); });
		descriptionWindow.addIntParameter("Width Subdivision", "Width Subdivision", widthSubdivisionParam, 1, 20, v => { widthSubdivisionParam = v; Regenerate(); });
		descriptionWindow.addFloatParameter("Segment Length", "Segment Length", segmentsLengthParam, 0.1f, 5.0f, v => { segmentsLengthParam = v; Regenerate(); });
		descriptionWindow.addFloatParameter("UV Length Tiling", "Length Tiling", lengthTiling, 0.05f, 1.5f, v => { lengthTiling = v; Regenerate(); });
		descriptionWindow.addFloatParameter("UV Width Tiling", "Width Tiling", widthTiling, 0.5f, 5f, v => { widthTiling = v; Regenerate(); });
		descriptionWindow.addBoolParameter("Stretch UV Along Length", "Stretch UV Along Length", stretchUVAlongLengthParam, v => { stretchUVAlongLengthParam = v; Regenerate(); });
		descriptionWindow.addBoolParameter("Stretch UV Along Width", "Stretch UV Along Width", stretchUVAlongWidthParam, v => { stretchUVAlongWidthParam = v; Regenerate(); });
		descriptionWindow.addBoolParameter("Show Wireframe", "Show Wireframe", showWireframeParam, v =>
		{
			showWireframeParam = v;
			Visualizer.Enabled = v;
		});

		// Generation type buttons
		var buttonsGrid = new WidgetGridBox(3, 2, 2);
		paramsBox.AddChild(buttonsGrid);

		var btn = new WidgetButton("Ribbon");
		btn.EventClicked.Connect(GenerateSplineMesh);
		buttonsGrid.AddChild(btn);

		btn = new WidgetButton("Square Pipe");
		btn.EventClicked.Connect(GenerateSplineSquarePipe);
		buttonsGrid.AddChild(btn);

		btn = new WidgetButton("Round Pipe");
		btn.EventClicked.Connect(GenerateSplineRoundPipe);
		buttonsGrid.AddChild(btn);

		btn = new WidgetButton("Clear");
		btn.EventClicked.Connect(Clear);
		paramsBox.AddChild(btn);
	}
}
