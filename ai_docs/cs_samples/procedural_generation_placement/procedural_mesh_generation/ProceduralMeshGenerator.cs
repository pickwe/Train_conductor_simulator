// Demonstrates asynchronous procedural mesh generation using RunGenerateMeshProceduralAsync.
// Creates a grid of box meshes with selectable procedural mode and MeshRender flags.
// Shows memory usage statistics and provides UI for controlling generation parameters.

using System.Collections.Generic;
using Unigine;
using System.Threading;


#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Generates a large grid of procedural mesh objects asynchronously.
public partial class ProceduralMeshGenerator : Component
{
	// Grid dimensions (size x size objects)
	private int size = 128;
	private int newSize;
	private int numObjects;
	// Spacing between objects
	private float offset = 0.5f;

	// Bytes to megabytes conversion factor
	private const float B2MB = 1.0f / (1024 * 1024);

	// Thread-safe counters for created objects and memory usage
	private int numCreatedObjects = 0;
	private long dynamicRam = 0;
	private long dynamicVram = 0;

	// State flags for async operations
	private bool isCreatingObjects = false;
	private bool isDeletingDone = false;

	// Collection of generated mesh objects
	private List<ObjectMeshStatic> boxObjects = new List<ObjectMeshStatic>();

	// Current procedural mode settings
	private ObjectMeshStatic.PROCEDURAL_MODE currentMode;
	private ObjectMeshStatic.PROCEDURAL_MODE lastMode;
	int currentMeshRenderFlag;

	// UI elements
	private SampleDescriptionWindow sampleDescriptionWindow;
	private Dictionary<string, ObjectMeshStatic.PROCEDURAL_MODE> modesMap;
	private Dictionary<string, int> flagsMap;
	private WidgetComboBox modeCombo;
	private WidgetComboBox flagsCombo;
	private WidgetEditLine editline;
	private WidgetSpinBox spinbox;
	private WidgetButton generateButton;
	private WidgetButton clearButton;

	// UI is initialized and profiler is enabled.
	void Init()
	{

		numObjects = size * size;
		newSize = size;

		InitGui();
	}

	// Creation state is monitored and memory stats are updated each frame.
	void Update()
	{
		// Check if all objects finished creating
		if (isCreatingObjects && numCreatedObjects == numObjects)
		{
			SetGuiEnabled(true);
			isCreatingObjects = false;
		}

		// Handle completion of object deletion
		if (isDeletingDone)
		{
			isDeletingDone = false;
			if (isCreatingObjects)
				CreateObjects();
			else
				SetGuiEnabled(true);
		}

		// Apply new field size when no creation in progress
		if (!isCreatingObjects && newSize != size)
		{
			size = newSize;
			numObjects = size * size;
		}

		// Display current memory usage
		UpdateStats();
	}

	// Objects are cleared and profiler is disabled on shutdown.
	void Shutdown()
	{
		ClearObjects();
		ShutdownGui();
	}

	// Grid of mesh objects is created asynchronously with callbacks.
	private void CreateObjects()
	{
		isCreatingObjects = true;

		if (boxObjects.Count > 0)
		{
			ClearObjects();
			return;
		}

		float fieldOffset = (1.0f + offset) * size / 2.0f;

		boxObjects.Capacity = numObjects;
		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				var obj = new ObjectMeshStatic();
				obj.WorldPosition = new Vec3(x + x * offset - fieldOffset, y + y * offset - fieldOffset, 1.5f);

				obj.SetMeshProceduralMode(currentMode);

				// This method will itself generate new mesh by create_mesh callback, load MeshRender,
				// apply new mesh to object and then call create_done callback when everything is done
				obj.RunGenerateMeshProceduralAsync(CreateMesh, () => { CreateDone(obj); }, currentMeshRenderFlag);

				boxObjects.Add(obj);
			}
		}
	}

	// Callback that generates box mesh geometry.
	private void CreateMesh(Mesh mesh)
	{
		mesh.AddBoxSurface("box", vec3.ONE);
		// Dynamic procedural meshes are constantly stored in memory,
		// so we can calculate memory usage right after creation
		if (currentMode == ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC)
			Interlocked.Add(ref dynamicRam, (long)mesh.MemoryUsage);
	}

	// Callback invoked when mesh generation completes for an object.
	private void CreateDone(ObjectMeshStatic obj)
	{
		if (obj == null)
			return;

		Interlocked.Increment(ref numCreatedObjects);

		// Dynamic procedural meshes are constantly stored in memory,
		// so we can calculate memory usage right after creation.
		// File and Blob procedural meshes don't occupy VRAM till active.
		if (currentMode != ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC)
			return;

		MeshRender mVram = obj.GetMeshCurrentVRAM();
		if (mVram != null)
		{
			Interlocked.Add(ref dynamicRam, (long)mVram.GetSystemMemoryUsage());
			Interlocked.Add(ref dynamicVram, (long)mVram.GetVideoMemoryUsage());
		}
	}

	// All mesh objects are deleted and counters are reset.
	private void ClearObjects()
	{
		Interlocked.Exchange(ref numCreatedObjects, 0);
		Interlocked.Exchange(ref dynamicRam, 0);
		Interlocked.Exchange(ref dynamicVram, 0);

		foreach (var obj in boxObjects)
		{
			obj.DeleteLater();
		}
		boxObjects.Clear();
		isDeletingDone = true;
	}

	// Memory usage statistics are retrieved and displayed.
	private void UpdateStats()
	{
		long ram = 0, residentVram = 0;

		switch (lastMode)
		{
			// For Dynamic mode, memory usage is accumulated at creation time and remains constant
			case ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC:
				ram = Interlocked.Read(ref dynamicRam);
				residentVram = Interlocked.Read(ref dynamicVram);
				break;
			// File and Blob modes stream meshes in and out. We sum each object's resident mesh
			// memory (active + cached).
			case ObjectMeshStatic.PROCEDURAL_MODE.BLOB:
			case ObjectMeshStatic.PROCEDURAL_MODE.FILE:
				foreach (var obj in boxObjects)
				{
					MeshRender mVram = obj.GetMeshCurrentVRAM();
					if (mVram != null)
					{
						ram += (long)mVram.GetSystemMemoryUsage();
						residentVram += (long)mVram.GetVideoMemoryUsage();
					}
				}
				break;
			default: break;
		}

		string status = $"Num ready objects: {numCreatedObjects}\nRAM: {(int)(ram * B2MB)} MB\nVRAM Resident: {(int)(residentVram * B2MB)} MB";
		sampleDescriptionWindow.setStatus(status);
	}

	// UI window with mode selectors, field size input, and action buttons is created.
	private void InitGui()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow(Gui.ALIGN_RIGHT);
		var params_box = sampleDescriptionWindow.getParameterGroupBox();

		var gridbox = new WidgetGridBox(2, 10);
		params_box.AddChild(gridbox);

		// Procedural Mode selector (Dynamic/File/Blob)
		modesMap = new Dictionary<string, ObjectMeshStatic.PROCEDURAL_MODE>();
		modesMap["Dynamic"] = ObjectMeshStatic.PROCEDURAL_MODE.DYNAMIC;
		modesMap["File"] = ObjectMeshStatic.PROCEDURAL_MODE.FILE;
		modesMap["Blob"] = ObjectMeshStatic.PROCEDURAL_MODE.BLOB;

		var label = new WidgetLabel("Procedural Mode");
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
		flagsMap = new Dictionary<string, int>();
		flagsMap["None"] = 0;
		flagsMap["DYNAMIC_VERTEX"] = MeshRender.USAGE_DYNAMIC_VERTEX;
		flagsMap["DYNAMIC_INDICES"] = MeshRender.USAGE_DYNAMIC_INDICES;
		flagsMap["DYNAMIC_ALL"] = MeshRender.USAGE_DYNAMIC_ALL;

		label = new WidgetLabel("MeshRender Flag");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		flagsCombo = new WidgetComboBox();
		flagsCombo.AddItem("None");
		flagsCombo.AddItem("DYNAMIC_VERTEX");
		flagsCombo.AddItem("DYNAMIC_INDICES");
		flagsCombo.AddItem("DYNAMIC_ALL");
		gridbox.AddChild(flagsCombo, Gui.ALIGN_EXPAND);

		currentMeshRenderFlag = 0;
		flagsCombo.CurrentItem = 0;
		flagsCombo.EventChanged.Connect(() =>
		{
			var item = flagsCombo.GetCurrentItemText();
			currentMeshRenderFlag = flagsMap[item];
		});

		// Field size input with spinbox
		label = new WidgetLabel("Field Size");
		gridbox.AddChild(label, Gui.ALIGN_LEFT);

		var spinbox_hbox = new WidgetHBox();
		gridbox.AddChild(spinbox_hbox, Gui.ALIGN_EXPAND);

		spinbox = new WidgetSpinBox(1, 1000);
		spinbox.Value = size;
		spinbox.EventChanged.Connect(() => { newSize = spinbox.Value; });

		editline = new WidgetEditLine();
		editline.Validator = Gui.VALIDATOR_UINT;
		editline.EventFocusOut.Connect(() =>
		{
			int text = int.Parse(editline.Text);
			newSize = MathLib.Clamp(text, spinbox.MinValue, spinbox.MaxValue);
			if (text != newSize)
				editline.Text = newSize.ToString();
		});
		editline.Text = size.ToString();
		editline.AddAttach(spinbox);

		spinbox_hbox.AddChild(editline);
		spinbox_hbox.AddChild(spinbox, Gui.ALIGN_EXPAND);

		// Vertical spacer
		var spacer = new WidgetSpacer();
		spacer.Orientation = 1;
		params_box.AddChild(spacer, Gui.ALIGN_EXPAND);

		// Generate and Clear action buttons
		var hbox = new WidgetHBox(10);
		params_box.AddChild(hbox, Gui.ALIGN_EXPAND);
		generateButton = new WidgetButton("Generate");
		generateButton.EventClicked.Connect(OnGenerateButton);
		hbox.AddChild(generateButton, Gui.ALIGN_EXPAND);
		clearButton = new WidgetButton("Clear");
		clearButton.EventClicked.Connect(OnClearButton);
		hbox.AddChild(clearButton, Gui.ALIGN_EXPAND);
	}

	// UI window is released and dictionaries are cleared.
	private void ShutdownGui()
	{
		sampleDescriptionWindow.shutdown();
		modesMap.Clear();
		flagsMap.Clear();
	}

	// Enables or disables action buttons during async operations.
	private void SetGuiEnabled(bool enabled)
	{
		generateButton.Enabled = enabled;
		clearButton.Enabled = enabled;
	}

	// Starts object generation when Generate button is clicked.
	private void OnGenerateButton()
	{
		SetGuiEnabled(false);

		lastMode = currentMode;

		CreateObjects();
	}

	// Clears all objects when Clear button is clicked.
	private void OnClearButton()
	{
		SetGuiEnabled(false);
		ClearObjects();
	}
}
