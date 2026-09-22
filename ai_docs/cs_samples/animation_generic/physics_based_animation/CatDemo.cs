// Demo controller that manages multiple easing algorithms for cat movement.
// Each child node contains a different MotionMode component (Linear, EaseIn,
// EaseOut, Spring variants). User can switch between modes via dropdown.

using System.Collections.Generic;
using Unigine;

// Demo mode controller for the cat chasing sample.
// Manages switching between different motion interpolation modes
// (Linear, EaseIn, EaseOut, Spring, etc.) via a dropdown menu.
// Each child node represents a different easing algorithm.
public partial class CatDemo : Component
{
	[ShowInEditor]
	public Node catNode = null;
	[ShowInEditor]
	public Node laserNode = null;

	private int mode = 0;

	private WidgetGroupBox demoBox;
	private WidgetComboBox demoComboBox;
	private List<MotionMode> motionModes = new List<MotionMode>();

	// MotionMode components are collected from child nodes and wired up with target references.
	[MethodInit(Order = -1)]
	private void Init()
	{
		if (laserNode == null)
			Log.Error("CatDemo.Init(): cannot find laserNode!\n");
		if (catNode == null)
			Log.Error("CatDemo.Init(): cannot find catNode!\n");

		demoComboBox = new WidgetComboBox();

		// Collect all MotionMode components from child nodes.
		// Each child node represents a different easing algorithm
		// (Linear, EaseIn, EaseOut, Spring variants, etc.)
		for (int i = 0; i < node.NumChildren; i++)
		{
			MotionMode motionMode = ComponentSystem.GetComponent<MotionMode>(node.GetChild(i));
			if (motionMode == null)
				continue;

			// Wire up target and persecutor references for each motion mode
			motionMode.targetNode = laserNode;
			motionMode.persecutorNode = catNode;
			motionMode.Enabled = false;
			motionModes.Add(motionMode);
		}

		if (motionModes.Count == 0)
			Log.Error("CatDemo.Init(): no MotionMode components found in the child nodes!\n");
	}

	// Demo mode GUI elements are created and added to the sample window.
	public void InitGui(WidgetWindow window)
	{
		demoBox = new WidgetGroupBox("Demo", 8, 4);
		demoBox.AddChild(new WidgetLabel("Choose animation type for the cat:"), Gui.ALIGN_LEFT);

		// Cat movement modes are added to the demo combobox
		foreach (MotionMode motionMode in motionModes)
			demoComboBox.AddItem(motionMode.node.Name);

		demoComboBox.CurrentItem = mode;
		demoComboBox.EventChanged.Connect(() => UpdateCatMode());
		demoComboBox.Enabled = false;
		demoComboBox.Arrange();
		demoBox.AddChild(demoComboBox, Gui.ALIGN_LEFT);

		window.AddChild(demoBox, Gui.ALIGN_LEFT);
	}

	// Called when the demo mode is activated. GUI is enabled and current motion mode is applied.
	protected override void OnEnable()
	{
		// The GUI is built by CatSample after this component is initialized
		if (demoBox == null)
			return;

		demoComboBox.Enabled = true;
		UpdateCatMode();
		demoBox.Hidden = false;
	}

	// Called when the demo mode is deactivated. Current motion mode and GUI are disabled.
	protected override void OnDisable()
	{
		if (mode < motionModes.Count)
			motionModes[mode].Enabled = false;

		if (demoBox == null)
			return;

		demoComboBox.Enabled = false;
		demoBox.Hidden = true;
	}

	// Reads the selected mode from the combobox and switches to it.
	private void UpdateCatMode()
	{
		SwitchCatMode(demoComboBox.CurrentItem);
	}

	// Disables the current motion mode and enables the new one.
	private void SwitchCatMode(int newMode)
	{
		if (newMode < 0 || newMode >= motionModes.Count)
			return;

		motionModes[mode].Enabled = false;
		mode = newMode;
		motionModes[mode].Enabled = true;
	}

	private void Shutdown()
	{
		ShutdownGui();
	}

	// GUI widgets are released via DeleteLater for safe deferred destruction.
	private void ShutdownGui()
	{
		if (demoBox)
			demoBox.DeleteLater();
		if (demoComboBox)
			demoComboBox.DeleteLater();
	}
}
