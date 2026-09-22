// Demonstrates real-time procedural mesh modification with multi-threading support.
// Creates an animated wave surface by modifying vertices each frame. Shows different
// threading modes (Main/Background), apply modes (Copy/Move, Async/Force), and
// optional manual MeshRender creation and collision data generation.

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unigine;
using System;


#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Continuously modifies mesh geometry to create animated wave surface.
public partial class ProceduralMeshModifier : Component
{
	// Grid resolution for wave surface
	private int size = 128;
	private float isize;

	// Mesh objects for RAM and VRAM data
	private ObjectMeshStatic objectMesh;
	private Mesh meshRAM;
	private MeshRender meshVRAM;

	// Signals other threads when component is shutting down
	private bool isDeleted;

	// Current procedural mode and storage settings
	private ObjectMeshStatic.PROCEDURAL_MODE currentMode;
	private int currentMeshRenderFlag;

	// Option: create collision data for intersection/physics
	private bool isCollisionEnabled = false;
	// Option: generate mesh on background thread
	private bool isThreadAsync = true;
	// Option: use async apply (non-blocking) vs force (blocking)
	private bool isAsyncMode = true;
	// Option: copy mesh data (preserves original) vs move (transfers ownership)
	private bool isCopyMode = true;
	// Option: manually create MeshRender instead of automatic
	private bool isMeshvramManual = false;
	// Cached manual mode state for thread safety
	private bool updatedMeshvramManual;

	// Prevents overlapping mesh modification cycles
	private bool isRunning = false;

	// UI elements
	private SampleDescriptionWindow sampleDescriptionWindow;
	private WidgetComboBox threadCombo;
	private WidgetComboBox asyncCombo;
	private WidgetComboBox moveCombo;
	private Dictionary<string, ObjectMeshStatic.PROCEDURAL_MODE> modesMap;
	private Dictionary<string, int> usageMap;
	private WidgetComboBox modeCombo;
	private WidgetComboBox usageCombo;
	private WidgetCheckBox meshvramCheckbox;
	private WidgetLabel warningLabel;

	// UI is created and mesh objects are initialized.
	void Init()
	{
		InitGui();

		isDeleted = false;
		updatedMeshvramManual = isMeshvramManual;

		isize = 30.0f / size;

		meshRAM = new Mesh();
		meshVRAM = new MeshRender();
		objectMesh = new ObjectMeshStatic();
		objectMesh.SetMeshProceduralMode(currentMode);
		objectMesh.WorldPosition = Vec3.ONE;
	}
	
	// Mesh modification cycle is started if not already running.
	void Update()
	{
		int id = Profiler.BeginMicro("Component Update");

		// Skip if previous modification cycle is still active
		if (isRunning || objectMesh.IsMeshProceduralActive)
		{
			Profiler.EndMicro(id);
			return;
		}

		isRunning = true;

		isMeshvramManual = updatedMeshvramManual;

		// Recreate mesh object if procedural mode changed
		if (objectMesh.MeshProceduralMode != currentMode)
		{
			objectMesh.DeleteLater();
			objectMesh = new ObjectMeshStatic();
			objectMesh.SetMeshProceduralMode(currentMode);
			objectMesh.WorldPosition = Vec3.ONE;
		}

		// Choose between background thread (non-blocking) or main thread processing
		if (isThreadAsync)
		{
			// Background thread: mesh generation doesn't block rendering
			AsyncQueue.RunAsync(AsyncQueue.ASYNC_THREAD.BACKGROUND, AsyncUpdateRAM);
		}
		else
		{
			// Main thread: synchronous update within this frame
			UpdateRAM();
			if (isMeshvramManual)
				UpdateVRAM();
			ApplyData();
		}

		Profiler.EndMicro(id);
	}

	// Thread shutdown is signaled and resources are cleaned up safely.
	void Shutdown()
	{
		// Signal background threads to stop
		isDeleted = true;

		ShutdownGui();

		// Wait for any active mesh modification to complete before cleanup
		lock (meshRAM)
		{
			meshRAM.Clear();
			meshVRAM.Clear();
			objectMesh.DeleteLater();
		}
	}

	// Wave surface geometry is generated based on current time.
	private void UpdateMesh(Mesh mesh)
	{
		int id = Profiler.BeginMicro("ProceduralMeshModifier.UpdateMesh");

		float time = Game.Time;

		if (mesh.NumSurfaces != 1)
		{
			mesh.Clear();
			mesh.AddSurface("");
		}
		else
		{
			mesh.ClearSurface();
		}

		var vertices = new vec3[size * size];

		for (int y = 0; y < size; y++)
		{
			float Y = y * isize - 15.0f;
			float Z = MathLib.Cos(Y + time);

			for (int x = 0; x < size; x++)
			{
				float X = x * isize - 15.0f;
				vertices[y * size + x] = (new vec3(X, Y, Z * MathLib.Sin(X + time)));
			}
		}

		mesh.AddVertex(vertices);

		// Pre-allocate index arrays to avoid repeated reallocations
		var cindices = new List<int>();
		cindices.Capacity = (size - 1) * (size - 1) * 6;
		var tindices = new List<int>();
		tindices.Capacity = (size - 1) * (size - 1) * 6;

		var addIndex = (int index) =>
		{
			cindices.Add(index);
			tindices.Add(index);
		};

		for (int y = 0; y < size - 1; y++)
		{
			int offset = size * y;
			for (int x = 0; x < size - 1; x++)
			{
				addIndex(offset);
				addIndex(offset + 1);
				addIndex(offset + size);
				addIndex(offset + size);
				addIndex(offset + 1);
				addIndex(offset + size + 1);
				offset++;
			}
		}

		mesh.AddCIndices(cindices.ToArray());
		mesh.AddTIndices(tindices.ToArray());

		cindices.Clear();
		tindices.Clear();

		mesh.CreateTangents();

		{
			int idScope = Profiler.BeginMicro("CreateCollisionData");

			// If you plan to use intersection and collision with this mesh then it's better to create
			// CollisionData. Otherwise intersections and collisions would be highly ineffective in
			// terms of performance
			if (isCollisionEnabled)
			{
				// Creates both Spatial Tree and Edges for effective intersection and collision
				// respectively
				mesh.CreateCollisionData();

				// You can create only Spatial Tree if you need only intersection
				//		mesh.CreateSpatialTree();
				// Or you can create only Edges if you need only collision
				//		mesh.CreateEdges();
			}
			else
				// Or you can create mesh without collision data or even delete existing one if there is
				// no need for it.
				// To check if mesh has collisionData you can use:
				//		mesh.HasCollisionData();
				//		mesh.HasSpatialTree();
				//		mesh.HasEdges();
				mesh.ClearCollisionData();

			Profiler.EndMicro(idScope);
		}

		mesh.CreateBounds();

		Profiler.EndMicro(id);
	}

	// Mesh geometry is updated with thread-safe locking.
	private void UpdateRAM()
	{
		int id = Profiler.BeginMicro("ProceduralMeshModifier.UpdateRAM");

		// Abort if component is shutting down
		if (isDeleted)
		{
			Profiler.EndMicro(id);
			return;
		}

		// Lock prevents concurrent access from multiple threads
		lock (meshRAM)
		{
			UpdateMesh(meshRAM);
		}
		Profiler.EndMicro(id);
	}

	// Background thread entry point that chains to GPU or Main thread.
	private void AsyncUpdateRAM()
	{
		UpdateRAM();

		if (isMeshvramManual)
		{
			// Manual MeshRender requires GPU_STREAM thread for VRAM upload
			AsyncQueue.RunAsync(AsyncQueue.ASYNC_THREAD.GPU_STREAM, AsyncUpdateVRAM);
		}
		else
		{
			// If you don't need to load MeshRender manually and automatic loading inside
			// applyMeshProcedural methods is enough then return to Main thread
			AsyncQueue.RunAsync(AsyncQueue.ASYNC_THREAD.MAIN, () =>
			{
				// Verify component still exists before applying
				if (!node || node.IsDeleted)
					return;
				ApplyData();
			});
		}
	}

	// MeshRender data is loaded from RAM mesh to VRAM.
	private void UpdateVRAM()
	{
		int id = Profiler.BeginMicro("ProceduralMeshModifier.UpdateVRAM");

		if (isDeleted)
		{
			Profiler.EndMicro(id);
			return;
		}

		// Lock ensures mesh data is consistent during VRAM upload
		lock (meshRAM)
		{
			meshVRAM.Load(meshRAM);
		}
		Profiler.EndMicro(id);
	}

	// GPU thread entry point that chains back to Main thread.
	private void AsyncUpdateVRAM()
	{
		UpdateVRAM();

		// Return to Main thread for final application
		AsyncQueue.RunAsync(AsyncQueue.ASYNC_THREAD.MAIN, () => {
			if (!node || node.IsDeleted)
				return;
			ApplyData();
		});
	}

	// Mesh is applied to object using selected mode (Async/Force, Copy/Move).
	private void ApplyData()
	{
		int id = Profiler.BeginMicro("ProceduralMeshModifier.ApplyData");

		// If apply happens in async mode it will be processed on another thread without blocking Main
		// thread. Otherwise in Force mode Main thread won't leave the scope of this function until
		// apply is finished.

		if (isAsyncMode)
		{
			if (isMeshvramManual)
			{
				// You can use manually created MeshRender only in Move mode
				objectMesh.ApplyMoveMeshProceduralAsync(meshRAM, meshVRAM);
			}
			else
			{
				if (isCopyMode)
					// With Copy mode data from mesh_ram will be copied for internal use and mesh_ram
					// itself won't be changed
					objectMesh.ApplyCopyMeshProceduralAsync(meshRAM, currentMeshRenderFlag);
				else
					// With Move mode data will be taken from mesh_ram for internal use so mesh_ram
					// will change
					objectMesh.ApplyMoveMeshProceduralAsync(meshRAM, currentMeshRenderFlag);
			}
		}
		else
		{
			if (isMeshvramManual)
			{
				objectMesh.ApplyMoveMeshProceduralForce(meshRAM, meshVRAM);
			}
			else
			{
				if (isCopyMode)
					objectMesh.ApplyCopyMeshProceduralForce(meshRAM, currentMeshRenderFlag);
				else
					objectMesh.ApplyMoveMeshProceduralForce(meshRAM, currentMeshRenderFlag);
			}
		}

		// Full cycle of mesh modification is finished
		isRunning = false;

		Profiler.EndMicro(id);
	}

	// UI window with all parameter controls is created.
	private void InitGui()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow(Gui.ALIGN_RIGHT);
		var params_box = sampleDescriptionWindow.getParameterGroupBox();

		var gridbox = new WidgetGridBox(2, 10);
		params_box.AddChild(gridbox, Gui.ALIGN_EXPAND);

		// Thread selector (Main/Background)
		var label = new WidgetLabel("Thread");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		threadCombo = new WidgetComboBox();
		threadCombo.AddItem("Main");
		threadCombo.AddItem("Background");
		gridbox.AddChild(threadCombo, Gui.ALIGN_EXPAND);

		threadCombo.CurrentItem = 1;
		threadCombo.EventChanged.Connect(() =>
		{
			isThreadAsync = threadCombo.CurrentItem != 0;
		});

		// Procedural mode selector (Dynamic/File/Blob)
		modesMap = new Dictionary<string, ObjectMeshStatic.PROCEDURAL_MODE>();
		modesMap["Dynamic"] = ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC;
		modesMap["File"] = ObjectMeshStatic.PROCEDURAL_MODE.FILE;
		modesMap["Blob"] = ObjectMeshStatic.PROCEDURAL_MODE.BLOB;

		label = new WidgetLabel("Procedural Mode");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		modeCombo = new WidgetComboBox();
		modeCombo.AddItem("Dynamic");
		modeCombo.AddItem("File");
		modeCombo.AddItem("Blob");
		gridbox.AddChild(modeCombo, Gui.ALIGN_EXPAND);

		currentMode = ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC;
		modeCombo.CurrentItem = 0;
		modeCombo.EventChanged.Connect(() =>
		{
			var item = modeCombo.GetCurrentItemText();
			currentMode = modesMap[item];
		});

		// MeshRender storage flag selector
		usageMap = new Dictionary<string, int>();
		usageMap["None"] = 0;
		usageMap["DYNAMIC_VERTEX"] = MeshRender.USAGE_DYNAMIC_VERTEX;
		usageMap["DYNAMIC_INDICES"] = MeshRender.USAGE_DYNAMIC_INDICES;
		usageMap["DYNAMIC_ALL"] = MeshRender.USAGE_DYNAMIC_ALL;

		label = new WidgetLabel("MeshRender Flag");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		usageCombo = new WidgetComboBox();
		usageCombo.AddItem("None");
		usageCombo.AddItem("DYNAMIC_VERTEX");
		usageCombo.AddItem("DYNAMIC_INDICES");
		usageCombo.AddItem("DYNAMIC_ALL");
		gridbox.AddChild(usageCombo, Gui.ALIGN_EXPAND);

		currentMeshRenderFlag = 0;
		usageCombo.CurrentItem = 0;
		usageCombo.EventChanged.Connect(() =>
		{
			var item = usageCombo.GetCurrentItemText();
			currentMeshRenderFlag = usageMap[item];
		});

		// Async/Force mode selector
		label = new WidgetLabel("Async Mode");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		asyncCombo = new WidgetComboBox();
		asyncCombo.AddItem("Async");
		asyncCombo.AddItem("Force");
		gridbox.AddChild(asyncCombo, Gui.ALIGN_EXPAND);

		asyncCombo.CurrentItem = 0;
		asyncCombo.EventChanged.Connect(() =>
		{
			isAsyncMode = asyncCombo.CurrentItem == 0;
		});

		// Copy/Move apply mode selector
		label = new WidgetLabel("Apply mode");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		moveCombo = new WidgetComboBox();
		moveCombo.AddItem("Copy");
		moveCombo.AddItem("Move");
		gridbox.AddChild(moveCombo, Gui.ALIGN_EXPAND);

		isCopyMode = true;
		moveCombo.CurrentItem = 0;
		moveCombo.EventChanged.Connect(() =>
		{
			isCopyMode = moveCombo.CurrentItem == 0;
			UpdateWarning();
		});

		// Collision data generation checkbox
		label = new WidgetLabel("Create CollisionData");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		var collison_checkbox = new WidgetCheckBox();
		gridbox.AddChild(collison_checkbox, Gui.ALIGN_EXPAND);
		collison_checkbox.EventChanged.Connect(() => { isCollisionEnabled = collison_checkbox.Checked; });

		// Manual MeshRender creation checkbox
		label = new WidgetLabel("Manual MeshRender");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		meshvramCheckbox = new WidgetCheckBox();
		gridbox.AddChild(meshvramCheckbox, Gui.ALIGN_EXPAND);
		meshvramCheckbox.EventChanged.Connect(() =>
		{
			updatedMeshvramManual = meshvramCheckbox.Checked;
			UpdateWarning();
		});

		// Warning label for manual MeshRender mode restrictions
		warningLabel = new WidgetLabel("MeshRender can be used only with mode \"Move\". Apply mode \"Copy\" will be ignored ")
		{
			FontWrap = 1,
			Hidden = true,
			FontColor = vec4.RED
		};

		params_box.FontWrap = 1;
		params_box.AddChild(warningLabel, Gui.ALIGN_EXPAND);
	}

	private void UpdateWarning()
	{
		warningLabel.Hidden = !(updatedMeshvramManual && isCopyMode);
	}

	// UI window is released on shutdown.
	private void ShutdownGui()
	{
		sampleDescriptionWindow.shutdown();
	}
}
