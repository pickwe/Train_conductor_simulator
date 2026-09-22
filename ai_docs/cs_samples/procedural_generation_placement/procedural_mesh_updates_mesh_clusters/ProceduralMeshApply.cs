// Demonstrates applying procedural mesh changes to an ObjectMeshCluster.
// Creates a grid of instanced spheres that animate by changing their
// tessellation over time, showing async mesh application workflow.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
#endif

// Animates procedural sphere mesh applied to a mesh cluster.
public partial class ProceduralMeshApply : Component
{
	// Procedural mesh data
	private Mesh mesh;
	// Cluster object for instanced rendering
	private ObjectMeshCluster cluster;

	// Sphere geometry parameters
	private float radius = 0.5f;
	private const int maxNumStacks = 30;
	private const int minNumStacks = 2;
	private int numStacks = 2;
	private int numSlices = 3;

	// Controls animation direction (increasing or decreasing tessellation)
	private bool isIncreasing = true;

	// Timer for controlling mesh update frequency
	private float changeRate = 0.1f;
	private float currentTime = 0.0f;

	// Grid dimensions for cluster instances
	private const int size = 20;
	// Spacing between instances
	private float offset = 1.0f;

	// Mesh cluster is created with grid of transforms and procedural mode is set.
	void Init()
	{
		mesh = new Mesh();
		cluster = new ObjectMeshCluster();

		// Before changing mesh choose Procedural Mode:
		// - Disable - procedural mode is disabled
		// - Dynamic - fastest performance, stored in RAM and VRAM, not automatically unloaded from
		// memory.
		// - Blob - moderate performance, stored in RAM and VRAM, automatically unloaded from memory.
		// - File - slowest performance, all data stored on disk, automatically unloaded from memory.
		cluster.SetMeshProceduralMode(ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC);
		cluster.WorldPosition = new Vec3(0.0f, 0.0f, 3.0f);

		// Create cluster transforms
		List<Mat4> transforms = new List<Mat4>();
		float fieldOffset = (1.0f + offset) * size / 2.0f;

		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				transforms.Add(MathLib.Translate(new Vec3(x + x * offset - fieldOffset, y + y * offset - fieldOffset, 1.5f)));
			}
		}

		cluster.AppendMeshes(transforms.ToArray());

		Visualizer.Enabled = true;
	}
	
	// Mesh geometry is updated and applied asynchronously to cluster each frame.
	void Update()
	{
		// Update mesh geometry with new tessellation
		UpdateMesh(mesh);

		// Apply new mesh. You can do it Force or Async.
		// Changing mesh_render_flag you can choose where to store MeshRender data: in RAM or VRAM
		// 0 - store everything in VRAM (default behavior)
		// USAGE_DYNAMIC_VERTEX - store vertices on RAM
		// USAGE_DYNAMIC_INDICES - store indices on RAM
		// USAGE_DYNAMIC_ALL - store both vertices and indices on RAM
		cluster.ApplyMoveMeshProceduralAsync(mesh, 0);
		Visualizer.RenderObject(cluster, vec4.GREEN);
	}

	// Mesh and cluster are cleaned up on shutdown.
	void Shitdown()
	{
		mesh.Clear();
		cluster.DeleteLater();

		Visualizer.Enabled = false;
	}

	// Sphere tessellation is animated by changing stacks and slices over time.
	private void UpdateMesh(Mesh mesh)
	{
		currentTime += Game.IFps;

		if (currentTime > changeRate)
		{
			currentTime = 0.0f;

			numSlices = isIncreasing ? numSlices + 1 : numSlices - 1;
			numStacks = isIncreasing ? numStacks + 1 : numStacks - 1;

			if (numStacks == maxNumStacks)
				isIncreasing = false;

			if (numStacks <= minNumStacks)
			{
				isIncreasing = true;
				numStacks = minNumStacks;
				numSlices = numStacks + 1;
			}
		}

		// Recreate sphere with current tessellation settings
		mesh.Clear();
		mesh.AddSphereSurface("sphere", radius, numStacks, numSlices);
	}
}
