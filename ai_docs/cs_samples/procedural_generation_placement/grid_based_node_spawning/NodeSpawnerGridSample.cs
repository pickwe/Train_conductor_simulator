// Spawns nodes in a configurable 2D grid pattern at runtime.
// Demonstrates dynamic node instantiation using World.LoadNode() with adjustable
// grid dimensions and cell spacing. The pivot can be set to corner or center
// origin. Useful for procedural level generation, placing objects in formations,
// or creating tile-based systems.

using System.Collections.Generic;
using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

// Spawns a grid of nodes with interactive UI controls for configuration.
public partial class NodeSpawnerGridSample : Component
{
	// Path to the .node file to instantiate at each grid cell
	[ParameterFile(Filter=".node")]
	public string nodeToSpawn = null;
	// Spacing between nodes in world units
	private float cellSize = 5.0f;
	// Number of nodes along X and Y axes
	private ivec2 gridSize = new(5, 5);
	// Tracks all spawned nodes for cleanup when grid changes
	private List<Node> spawnedNodes = [];
	// When true, grid is centered at origin; when false, starts at origin corner
	private bool isPivotCenter = false;

	// Creates UI sliders and spawns initial grid.
	private void Init()
	{
		var descriptionWindowCreator = FindComponentInWorld<DescriptionWindowCreator>();
		var sampleDescriptionWindow = descriptionWindowCreator.getWindow();
		sampleDescriptionWindow.addIntParameter("Grid Size X", "Grid Size X", 5, 1, 100, (int v) =>
		{
			gridSize.x = v;
			Redraw();
		});
		sampleDescriptionWindow.addIntParameter("Grid Size Y", "Grid Size Y", 5, 1, 100, (int v) =>
		{
			gridSize.y = v;
			Redraw();
		});
		sampleDescriptionWindow.addFloatParameter("Cell Size", "Cell Size", 2.0f, 0.5f, 100.0f, (float v) =>
		{
			cellSize = v;
			Redraw();
		});

		WidgetGroupBox parameters = sampleDescriptionWindow.getParameterGroupBox();
		WidgetHBox hbox = new();
		WidgetCheckBox checkbox = new("Spawn from center");
		checkbox.EventClicked.Connect(() =>
		{
			isPivotCenter = checkbox.Checked;
			Redraw();
		});

		WidgetLabel label = new();

		hbox.AddChild(label);
		hbox.AddChild(checkbox);
		parameters.AddChild(hbox, Gui.ALIGN_LEFT);

		Redraw();
	}

	// Destroys existing nodes and recreates the grid with current settings.
	private void Redraw()
	{
		// Clean up previously spawned nodes
		if (spawnedNodes.Count != 0)
		{
			foreach (var spawnedNode in spawnedNodes) {
				spawnedNode.DeleteLater();
			}
			spawnedNodes.Clear();
		}
		SpawnGrid();
	}

	// Instantiates nodes in a grid pattern based on current parameters.
	private void SpawnGrid()
	{
		for (int x = 0; x < gridSize.x; ++x)
		{
			double xPos = isPivotCenter ? x * cellSize - gridSize.x * cellSize / 2 : x * cellSize;
			for (int y = 0; y < gridSize.y; ++y)
			{
				double yPos = isPivotCenter ? y * cellSize - gridSize.y * cellSize / 2 : y * cellSize;
				Node spawnedNode = World.LoadNode(nodeToSpawn);
				spawnedNode.Transform = new Mat4(MathLib.Translate(xPos, yPos, 0));
				spawnedNodes.Add(spawnedNode);
			}
		}
	}
}
