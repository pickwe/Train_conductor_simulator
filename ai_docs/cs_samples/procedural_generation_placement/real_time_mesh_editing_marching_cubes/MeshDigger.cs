// Interactive terrain deformation using the Marching Cubes algorithm.
// Generates a procedural voxel-based ground mesh that can be dug into or built up
// by clicking with the mouse. Uses asynchronous mesh generation to avoid frame
// drops during complex mesh updates. Hold Ctrl while clicking to add material
// instead of removing it, creating a fully editable terrain system.

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
#endif
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;
using System.Reflection;

// Controls terrain digging via mouse interaction using Marching Cubes procedural mesh.
public partial class MeshDigger : Component
{
	// Number of voxels per axis - higher values mean more detailed terrain
	[Parameter(Tooltip = "Number of marching cubes along one side of the field")]
	public int fieldSize = 64;

	// Size of each voxel in world units - smaller values increase resolution
	[Parameter(Tooltip = "Marching Cube edge length")]
	public float marchingCubeSize = 0.2f;

	// Radius of the digging brush in voxel units
	public float diggingRadius = 2.0f;

	// Material applied to the generated terrain mesh
	[Parameter(Title = "Material", Tooltip = "Ground material")]
	public Material mat;

	// Handles asynchronous voxel field updates and mesh regeneration
	private AsyncMarchingCubes marchingCubes;
	// The generated terrain mesh object used for intersection testing
	private Unigine.Object groundObject;
	// Inverse transform to convert world coordinates to local voxel space
	private Mat4 groundITransform;

	// Stores hit information when raycasting against terrain
	private WorldIntersection intersection = new();

	// UI window for adjusting digging parameters
	private SampleDescriptionWindow samplesDescriptionWindow = new();

	// Initializes the Marching Cubes terrain system and UI controls.
	void Init()
	{
		// Check the minimum values of field_size and marching_cube_size
		if (fieldSize < 4)
			fieldSize = 4;
		marchingCubeSize = MathLib.Clamp(marchingCubeSize, 0.05f, 1.0f);

		// Check if the ground material exists and set mesh_height parameter to ensure correct texture clamping
		if (mat == null)
			Log.Error("MeshDigger.Init(): set Ground Material!\n");
		else
			mat.SetParameterFloat("mesh_height", fieldSize * marchingCubeSize);

		marchingCubes = new AsyncMarchingCubes(fieldSize, marchingCubeSize);
		marchingCubes.setMaterial(mat);

		// Get the object created in the AsyncMarchingCubes() method to check for intersections
		groundObject = marchingCubes.getObject();
		groundITransform = groundObject.IWorldTransform;

		Visualizer.Enabled = true;

		samplesDescriptionWindow.createWindow();
		samplesDescriptionWindow.addFloatParameter("Digging Radius", "Radius in marching cubes units",
			diggingRadius, 1.0f, 30.0f, (float value) => { diggingRadius = value; });
	}

	// Casts ray from camera through mouse position and applies digging brush on click.
	void Update()
	{
		// Process any pending mesh updates
		marchingCubes.Update();

		// Skip digging if cursor is over UI widgets
		var w = Gui.GetCurrent().GetUnderCursorWidget();
		if (w && w.Type != Widget.TYPE.WIDGET_ENGINE)
			return;

		// Perform an intersection check at the current mouse cursor position
		ivec2 mouse = Input.MousePosition;
		Vec3 p0 = Game.Player.WorldPosition;
		Vec3 p1 = p0 + new Vec3(Game.Player.GetDirectionFromMainWindow(mouse.x, mouse.y)) * 100.0f;

		// Check for an intersection with the ground object
		Unigine.Object obj = World.GetIntersection(p0, p1, ~0, intersection);
		if (obj && obj == groundObject)
		{
			// Try digging the mesh when the left mouse button is clicked
			if (Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT))
			{
				// Convert the intersection point coordinates from world space to Marching Cubes coordinates
				var pos = groundITransform * intersection.Point;
				pos = pos / marchingCubeSize;

				AsyncMarchingCubes.BrushSphere sphere = new AsyncMarchingCubes.BrushSphere();
				sphere.pos = new vec3(pos);
				sphere.radius = diggingRadius;
				sphere.k = Input.IsKeyPressed(Input.KEY.ANY_CTRL) ? 1 : -1;
				marchingCubes.addBrush(sphere);
			}
			Visualizer.RenderSphere(diggingRadius * marchingCubeSize,
				MathLib.Translate(intersection.Point), vec4.BLUE);
		}
	}

	// Cleans up resources and UI on shutdown.
	void Shutdown()
	{
		marchingCubes = null;
		groundObject = null;

		Visualizer.Enabled = false;
		samplesDescriptionWindow.shutdown();
	}
}

// Handles Marching Cubes mesh generation asynchronously to prevent frame drops.
// Maintains a 3D voxel field where positive values are solid and negative are empty,
// then extracts the surface mesh by interpolating along cube edges.
class AsyncMarchingCubes
{
	public AsyncMarchingCubes(int num_cubes, float cube_edge = 0.2f) { Create(num_cubes, cube_edge); }
	~AsyncMarchingCubes() { Destroy(); }

	// Queue a spherical brush operation to modify the terrain
	public void addBrush(BrushSphere a) { actions.Add(a); }

	public Unigine.Object getObject() { return objectMesh; }
	public void setMaterial(Material mat) { material = mat; }

	// Defines a spherical brush for adding or removing material from the terrain
	public class BrushSphere
	{
		public vec3 pos = new();       // Center position in voxel coordinates
		public float radius = 6;        // Brush radius in voxels
		public float k = 2;             // Intensity: positive adds, negative removes
	};

	// The procedural mesh object that displays the terrain
	private ObjectMeshStatic objectMesh = new();
	// 3D texture storing voxel density values for normal calculation
	private Unigine.Image field = new();
	// Fast array access to voxel field data for mesh generation
	private float[] fieldData;

	// Grid dimensions
	private int size = 0;
	private int size2 = 0;           // size * size for fast indexing
	private float cubeEdgeLength = 0.2f;

	// Brush operations queued for processing
	private List<BrushSphere> actions = [];
	// Brush operations being processed in async thread
	private List<BrushSphere> asyncActions = [];

	private Material material;

	// Flag indicating whether the object is already deleted
	private bool isDeleted = false;

	// Unit cube corner positions used by Marching Cubes algorithm
	private static vec3 cell_0 = new(0.0f, 0.0f, 0.0f);
	private static vec3 cell_1 = new(1.0f, 0.0f, 0.0f);
	private static vec3 cell_2 = new(1.0f, 1.0f, 0.0f);
	private static vec3 cell_3 = new(0.0f, 1.0f, 0.0f);
	private static vec3 cell_4 = new(0.0f, 0.0f, 1.0f);
	private static vec3 cell_5 = new(1.0f, 0.0f, 1.0f);
	private static vec3 cell_6 = new(1.0f, 1.0f, 1.0f);
	private static vec3 cell_7 = new(0.0f, 1.0f, 1.0f);

	// Sets up the voxel grid and generates initial terrain with Perlin noise.
	public void Create(int num_cubes, float cube_edge = 0.2f)
	{
		isDeleted = false;

		size = num_cubes;
		size2 = size * size;
		cubeEdgeLength = cube_edge;

		objectMesh = new ObjectMeshStatic();
		objectMesh.SetMeshProceduralMode(ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC);

		// Place the ground object at the center of the world
		float object_offset = num_cubes * cubeEdgeLength * 0.5f;
		objectMesh.WorldPosition = new Vec3(-object_offset);

		//field = new Unigine.Image();
		field.Create3D(size, size, size + 1, Image.FORMAT_R32F);
		fieldData = new float[size * size * (size + 1)];

		// Generate voxel field data
		CreateField();

		Run(true);
	}

	// Thread-safe cleanup of mesh and voxel data.
	public void Destroy()
	{
		isDeleted = true;

		size = 0;

		lock (objectMesh)
		{
			objectMesh = null;
			if (field.IsValidPtr)
				field.Clear();
			actions.Clear();
			asyncActions.Clear();
		}
	}

	// Call each frame to process pending brush operations.
	public void Update() { Run(); }

	// Starts async mesh regeneration if there are pending actions.
	private void Run(bool force = false)
	{
		// Skip if already processing or no new actions
		if (objectMesh.IsMeshProceduralActive || (actions.Count == 0 && !force))
			return;

		// Pass all previously saved actions to apply in an async thread
		Swap(ref asyncActions, ref actions);
		actions.Clear();

		// Update the mesh asynchronously in a separated thread
		objectMesh.RunGenerateMeshProceduralAsync(UpdateRam, UpdateRamDone, MeshRender.USAGE_DYNAMIC_ALL);
	}

	// Generates initial terrain using 2D Perlin noise for height variation.
	private void CreateField()
	{
		Image.Pixel pixel = new Image.Pixel();
		int id;

		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				// Scale coordinates for noise generation
				float scale = 10.0f;
				vec2 p2D = new vec2(x, y) / size * scale;

				// Noise generated height value in range [0.f, 1.f]
				float h = MathLib.Clamp(Game.GetNoise2(p2D, new vec2(size), 1), -0.5f, 0.5f);
				h = MathLib.Saturate(h * 0.5f + 0.5f);

				for (int z = 0; z < size; z++)
				{
					// Normalized voxel's z-position
					float v = 1.0f - (float)z / size;

					id = z * size * size + y * size + x;

					// Calculate voxel height value. where 1.0f corresponds to the top and -1.f - to the bottom
					float val = MathLib.Clamp(v * 6.0f - h, -1.0f, 1.0f);
					fieldData[id] = val;

					pixel.f.x = val;
					field.Set3D(x, y, z, pixel);
				}

				id = size * size * size + y * size + x;
				fieldData[id] = -1.0f;

				pixel.f.x = -1.0f;
				field.Set3D(x, y, size, pixel);
			}
		}
	}

	// Extracts surface mesh from voxel field using Marching Cubes algorithm.
	// Iterates through each cube in the grid, determines which of 256 cases
	// applies based on corner values, and generates triangles accordingly.
	private void MarchingCubes(Mesh mesh)
	{
		// Prepare mesh surface for new geometry
		if (mesh.NumSurfaces <= 0)
		{
			mesh.Clear();
			mesh.AddSurface("");
		}
		else
		{
			mesh.ClearSurface();
		}

		// Collect generated geometry
		var vertices = new List<vec3>();
		var tangents = new List<quat>();
		var indices = new List<int>();

		vec4 yzwx(vec4 v) => new vec4(v.y, v.z, v.w, v.x);
		int toInt(bool val) => val ? 1 : 0;

		Dictionary<string, int> vertex_hash = [];

		float d = 1.0f / (float)size;
		vec3 o100 = new vec3(d, 0, 0);
		vec3 o010 = new vec3(0, d, 0);
		vec3 o001 = new vec3(0, 0, d);

		// Density values at bottom (z) and top (z+1) face corners
		vec4 field_03 = new vec4();
		vec4 field_47 = new vec4();
		// Interpolated vertex positions on the 12 edges of the cube
		vec3[] vertex = new vec3[12];

		// Iterate through each cube in the voxel grid
		for (int z = 0; z <= size - 1; z++)
		{
			for (int y = 0; y < size - 1; y++)
			{
				for (int x = 0; x < size - 1; x++)
				{
					// Sample density values at the 8 cube corners
					// Bottom face (z): corners 0,1,2,3 stored in field_03
					int id = z * size2 + y * size + x;
					field_03.x = fieldData[id + 0];
					field_03.y = fieldData[id + 1];
					field_03.w = fieldData[id + size + 0];
					field_03.z = fieldData[id + size + 1];

					// Top face (z+1): corners 4,5,6,7 stored in field_47
					id += size2;
					field_47.x = fieldData[id + 0];
					field_47.y = fieldData[id + 1];
					field_47.w = fieldData[id + size + 0];
					field_47.z = fieldData[id + size + 1];

					// Build 8-bit configuration index: each bit indicates if corner is inside (>0)
					// This determines which of 256 cases applies to this cube
					int index = toInt(field_03.x > 0) + toInt(field_03.y > 0) * 2 + toInt(field_03.z > 0) * 4
						+ toInt(field_03.w > 0) * 8;

					index |= toInt(field_47.x > 0) * 16 + toInt(field_47.y > 0) * 32
						+ toInt(field_47.z > 0) * 64 + toInt(field_47.w > 0) * 128;

					// Skip if all corners are inside (255) or outside (0) - no surface
					if (index == 0 || index == 255)
						continue;

					// Get bitmask of which edges are intersected by the surface
					short edges = marchingCubesEdges[index];

					// Interpolate vertex positions along edges where surface crosses
					// k = interpolation factor based on density values at edge endpoints
					// Edges 0-3: bottom face (z) horizontal edges connecting corners 0-1-2-3
					// Edge 0: corner 0 to 1, Edge 1: corner 1 to 2, etc.
					if ((edges & 0x00f) != 0)
					{
						vec4 k = field_03 / (field_03 - yzwx(field_03));
						if ((edges & 0x001) != 0)
							vertex[0] = MathLib.Lerp(cell_0, cell_1, k.x);
						if ((edges & 0x002) != 0)
							vertex[1] = MathLib.Lerp(cell_1, cell_2, k.y);
						if ((edges & 0x004) != 0)
							vertex[2] = MathLib.Lerp(cell_2, cell_3, k.z);
						if ((edges & 0x008) != 0)
							vertex[3] = MathLib.Lerp(cell_3, cell_0, k.w);
					}
					// Edges 4-7: top face (z+1) horizontal edges connecting corners 4-5-6-7
					// Edge 4: corner 4 to 5, Edge 5: corner 5 to 6, etc.
					if ((edges & 0x0f0) != 0)
					{
						vec4 k = field_47 / (field_47 - yzwx(field_47));
						if ((edges & 0x010) != 0)
							vertex[4] = MathLib.Lerp(cell_4, cell_5, k.x);
						if ((edges & 0x020) != 0)
							vertex[5] = MathLib.Lerp(cell_5, cell_6, k.y);
						if ((edges & 0x040) != 0)
							vertex[6] = MathLib.Lerp(cell_6, cell_7, k.z);
						if ((edges & 0x080) != 0)
							vertex[7] = MathLib.Lerp(cell_7, cell_4, k.w);
					}
					// Edges 8-11: vertical edges connecting bottom to top face
					// Edge 8: corner 0 to 4, Edge 9: corner 1 to 5, etc.
					if ((edges & 0xf00) != 0)
					{
						vec4 k = field_03 / (field_03 - field_47);
						if ((edges & 0x100) != 0)
							vertex[8] = MathLib.Lerp(cell_0, cell_4, k.x);
						if ((edges & 0x200) != 0)
							vertex[9] = MathLib.Lerp(cell_1, cell_5, k.y);
						if ((edges & 0x400) != 0)
							vertex[10] = MathLib.Lerp(cell_2, cell_6, k.z);
						if ((edges & 0x800) != 0)
							vertex[11] = MathLib.Lerp(cell_3, cell_7, k.w);
					}

					vec3 xyz = new vec3(x, y, z);

					// Build triangles from lookup table, processing 3 vertex indices at a time
					for (index *= 16; marchingCubesTriangles[index] != -1; index += 3)
					{
						// Lambda to add vertex with normal and tangent, using hash for deduplication
						var addVertex = (vec3 p) =>
						{
							string hash = p.ToString();

							if (vertex_hash.TryGetValue(hash, out var value))
							{
								indices.Add(value);
							}
							else
							{
								vec3 uvw = p * d;

								// Calculate normal from density gradient using central differences
								// Sample field at +/- half voxel offset in each axis direction
								// Normal points from high density (inside) to low density (outside)
								vec3 N = new vec3
								{
									x = field.Get3D(MathLib.Saturate(uvw - o100 * 0.5f)).f.r
										- field.Get3D(MathLib.Saturate(uvw + o100 * 0.5f)).f.r,
									y = field.Get3D(MathLib.Saturate(uvw - o010 * 0.5f)).f.r
										- field.Get3D(MathLib.Saturate(uvw + o010 * 0.5f)).f.r,
									z = field.Get3D(MathLib.Saturate(uvw - o001 * 0.5f)).f.r
										- field.Get3D(MathLib.Saturate(uvw + o001 * 0.5f)).f.r
								};
								N.Normalize();

								// Build tangent space basis (T, B, N) for correct lighting
								// Start with a vector perpendicular to Y axis, then orthogonalize
								vec3 T = new vec3(N.z, 0.0f, -N.x);
								T = T - N * MathLib.Dot(T, N);
								T.Normalize();
								quat q = new quat();
								// Bitangent completes the orthonormal basis via cross product
								var B = MathLib.Cross(N, T);
								B.Normalize();
								q.Set(T, B, N);

								vertex_hash.Add(hash, vertices.Count);
								indices.Add(vertices.Count);
								vertices.Add(p * cubeEdgeLength);
								tangents.Add(q);
							}
						};

						// Add triangle vertices in reverse order (2,1,0) to flip winding for correct face culling
						addVertex(vertex[marchingCubesTriangles[index + 2]] + xyz);
						addVertex(vertex[marchingCubesTriangles[index + 1]] + xyz);
						addVertex(vertex[marchingCubesTriangles[index + 0]] + xyz);
					}
				}
			}
		}

		mesh.AddVertex(vertices.ToArray());
		mesh.AddTangents(tangents.ToArray());
		mesh.AddIndices(indices.ToArray());

		// Create collision data for effective intersection and collision detection
		mesh.ClearCollisionData();
		mesh.CreateCollisionData();
		mesh.CreateBounds();
	}

	// Processes all queued brush operations by modifying voxel density values.
	private void BrushField()
	{
		foreach (var action in asyncActions)
			AddSphere(action.pos, action.radius, action.k);
		asyncActions.Clear();
	}

	// Modifies voxel field within a spherical region, using smooth falloff
	// from center. Positive k adds material, negative k removes it.
	private void AddSphere(vec3 pos, float radius, float k)
	{
		Image.Pixel pixel = new Image.Pixel();

		// Define the bounds of affected voxels
		ivec3 minPos = new ivec3(MathLib.Max(MathLib.Floor(pos - radius), vec3.ZERO));
		ivec3 maxPos = new ivec3(MathLib.Min(MathLib.Ceil(pos + radius), new vec3(size - 1)));

		// Find affected voxels and adjust their height
		for (int z = minPos.z; z <= maxPos.z; z++)
		{
			for (int y = minPos.y; y <= maxPos.y; y++)
			{
				for (int x = minPos.x; x <= maxPos.x; x++)
				{
					vec3 voxCoord = new vec3(x, y, z);
					float dist = MathLib.Distance2(voxCoord, pos);
					if (dist > radius * radius)
						continue;

					int id = z * size * size + y * size + x;
					// Smooth falloff: value = (1 - distance/radius) * k
					// Full effect at center, linearly decreasing to zero at edge
					float value = (MathLib.Saturate(1.0f - MathLib.Sqrt(dist) / radius)) * k;
					float height = MathLib.Clamp(fieldData[id] + value, -1.0f, 1.0f);

					// Update field value
					fieldData[id] = height;

					pixel.f.x = height;
					field.Set3D(x, y, z, pixel);
				}
			}
		}
	}

	// Async callback: applies brush modifications and regenerates mesh geometry.
	private void UpdateRam(Mesh mesh)
	{
		if (isDeleted) return;

		lock (objectMesh)
		{
			BrushField();
			MarchingCubes(mesh);
		}
	}

	// Callback when async mesh generation completes - enables collision and intersection.
	private void UpdateRamDone()
	{
		if (isDeleted)
			return;

		lock (objectMesh)
		{
			if (objectMesh && objectMesh.NumSurfaces > 0)
			{
				objectMesh.SetIntersection(true, 0);
				objectMesh.SetIntersectionMask(~0, 0);
				objectMesh.SetCollision(true, 0);
				objectMesh.SetCollisionMask(~0, 0);
				objectMesh.SetMaterial(material, 0);
			}
		}
	}

	// Swaps two lists for double-buffering brush operations.
	private void Swap<T>(ref List<T> a, ref List<T> b)
	{
		var temp = a;
		a = b;
		b = temp;
	}

	// Marching Cubes lookup table: bitmask indicating which of 12 edges are intersected
	// by the isosurface for each of 256 possible corner configurations.
	// Each cube has 8 corners that can be inside (positive) or outside (negative) the surface,
	// giving 2^8 = 256 combinations. Bits 0-11 indicate edges 0-11 respectively.
	static short[] marchingCubesEdges = {0x000, 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605,
												 0x70c, 0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00, 0x190, 0x099, 0x393, 0x29a,
												 0x596, 0x49f, 0x795, 0x69c, 0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90, 0x230,
												 0x339, 0x033, 0x13a, 0x636, 0x73f, 0x435, 0x53c, 0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33,
												 0xc39, 0xd30, 0x3a0, 0x2a9, 0x1a3, 0x0aa, 0x7a6, 0x6af, 0x5a5, 0x4ac, 0xbac, 0xaa5, 0x9af,
												 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0, 0x460, 0x569, 0x663, 0x76a, 0x066, 0x16f, 0x265, 0x36c,
												 0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60, 0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6,
												 0x0ff, 0x3f5, 0x2fc, 0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0, 0x650, 0x759,
												 0x453, 0x55a, 0x256, 0x35f, 0x055, 0x15c, 0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859,
												 0x950, 0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0x0cc, 0xfcc, 0xec5, 0xdcf, 0xcc6,
												 0xbca, 0xac3, 0x9c9, 0x8c0, 0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc, 0x0cc,
												 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0, 0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f,
												 0xf55, 0xe5c, 0x15c, 0x055, 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650, 0xaf0, 0xbf9, 0x8f3,
												 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc, 0x2fc, 0x3f5, 0x0ff, 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
												 0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c, 0x36c, 0x265, 0x16f, 0x066, 0x76a,
												 0x663, 0x569, 0x460, 0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac, 0x4ac, 0x5a5,
												 0x6af, 0x7a6, 0x0aa, 0x1a3, 0x2a9, 0x3a0, 0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35,
												 0xa3c, 0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x033, 0x339, 0x230, 0xe90, 0xf99, 0xc93, 0xd9a,
												 0xa96, 0xb9f, 0x895, 0x99c, 0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x099, 0x190, 0xf00,
												 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c, 0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203,
												 0x109, 0x000};

	// Marching Cubes lookup table: triangle vertex indices for each configuration.
	// Each configuration maps to up to 5 triangles (15 vertex indices). Index -1 marks end.
	// Values 0-11 refer to the 12 cube edges where vertices are interpolated.
	// Layout: 256 configurations * 16 entries each (15 indices + padding).
	static int[] marchingCubesTriangles = {
	-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1,
	3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1,
	3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1, 3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1, 9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1,
	1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, 9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, 2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1,
	8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1, 9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, 4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1,
	3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1, 1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1, 4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1, 4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1,
	9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1,
	1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1, 5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1, 2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1,
	9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1, 0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, 2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1,
	10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, 4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1, 5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1, 5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1,
	9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1, 0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1, 1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1, 10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1, 8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1, 2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1,
	7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, 9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1, 2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1, 11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1,
	9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1, 5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1, 11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1, 11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1,
	1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1, 9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1, 5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1,
	2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, 0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, 5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1,
	6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1, 0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1, 3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1, 6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1,
	5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1, 1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, 10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1,
	6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, 1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1, 8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1, 7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1,
	3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, 5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1, 0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, 9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1,
	8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1, 5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1, 0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1, 6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1,
	10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, 10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1, 8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1,
	1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1, 3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1, 0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1,
	10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1, 0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1, 3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1, 6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1,
	9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1, 8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1, 3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1, 6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1, 0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1, 10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1, 10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1,
	1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1, 2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1, 7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1, 7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1, 2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1, 1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1, 11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1,
	8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1, 0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1, 7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1,
	10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, 2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, 6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1,
	7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1, 2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, 1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1,
	10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1, 10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1, 0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1, 7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1,
	6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1, 8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1, 9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1,
	6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1, 1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1, 4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1, 10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1,
	8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, 0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1, 1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1,
	8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1, 10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1, 4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1, 10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, 5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, 11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1,
	9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, 6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1, 7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1, 3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1,
	7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1, 9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1, 3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1, 6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1,
	9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1, 1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1, 4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1, 7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1,
	6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1, 3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1, 0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1, 6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1,
	1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1, 0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1, 11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1, 6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1,
	5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1, 9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1, 1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1, 1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1, 10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1, 0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1, 5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1, 10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1,
	11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1, 0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1, 9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1, 7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1,
	2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, 8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1, 9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1, 9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1,
	1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1, 9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1, 9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, 5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1, 0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1, 10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1,
	2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1, 0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1, 0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1, 9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1, 5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1, 3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1, 5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1,
	8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1, 0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1, 9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1, 0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1, 1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1, 3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1,
	4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1, 9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1, 11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1, 11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1,
	2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1, 9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1, 3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1, 1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1, 4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1, 4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1, 0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1, 3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1, 3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1, 0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1, 9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1, 1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
};
}
