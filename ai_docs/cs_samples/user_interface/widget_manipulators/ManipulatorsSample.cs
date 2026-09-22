// UI controller for the Manipulators component demo.
// Provides checkboxes to enable/disable individual axis transformations and
// toggle buttons to switch between local and world coordinate systems.
// Works with the Manipulators component to configure manipulator widget behavior.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Parameter panel for configuring 3D object manipulator widgets.
public partial class ManipulatorsSample : Component
{
	// Reference to the Manipulators component that handles actual transforms
	private Manipulators component;

	// Checkboxes for enabling/disabling rotation on each axis
	private WidgetCheckBox xAxisRotationCheckBox;
	private WidgetCheckBox yAxisRotationCheckBox;
	private WidgetCheckBox zAxisRotationCheckBox;

	// Checkboxes for enabling/disabling translation on each axis
	private WidgetCheckBox xAxisTranslationCheckBox;
	private WidgetCheckBox yAxisTranslationCheckBox;
	private WidgetCheckBox zAxisTranslationCheckBox;

	// Checkboxes for enabling/disabling scale on each axis
	private WidgetCheckBox xAxisScaleCheckBox;
	private WidgetCheckBox yAxisScaleCheckBox;
	private WidgetCheckBox zAxisScaleCheckBox;

	// Toggle buttons for switching coordinate system basis
	private WidgetButton localBasisButton;
	private WidgetButton worldBasisButton;

	// Current basis mode - false for world, true for local
	private bool localBasis = false;

	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();

	// Builds UI with checkboxes for axis control and basis toggle buttons.
	void Init()
	{
		// Get reference to the Manipulators component on same node
		component = GetComponent<Manipulators>(node);
		if(component == null )
		{
			Log.ErrorLine("ManipulatorsSample::init: cannot find WidgetManipulators component!");
		}

		sampleDescriptionWindow.createWindow();

		WidgetGroupBox parameters = sampleDescriptionWindow.getParameterGroupBox();

		// Translation axis controls - X, Y, Z checkboxes in horizontal layout
		var hBox = new WidgetHBox(10);
		parameters.AddChild(hBox, Gui.ALIGN_LEFT);
		var label = new WidgetLabel("Translation:");
		label.FontWrap = 1;
		label.FontRich = 1;
		label.Width = 100;
		hBox.AddChild(label, Gui.ALIGN_LEFT);
		xAxisTranslationCheckBox = new WidgetCheckBox("X");
		xAxisTranslationCheckBox.Checked = true;
		xAxisTranslationCheckBox.EventChanged.Connect(() =>
		{
			component.XAxisTranslation = xAxisTranslationCheckBox.Checked;
		});
		hBox.AddChild(xAxisTranslationCheckBox, Gui.ALIGN_LEFT);
		yAxisTranslationCheckBox = new WidgetCheckBox("Y");
		yAxisTranslationCheckBox.Checked = true;
		yAxisTranslationCheckBox.EventChanged.Connect(() =>
		{
			component.YAxisTranslation = yAxisTranslationCheckBox.Checked;
		});
		hBox.AddChild(yAxisTranslationCheckBox, Gui.ALIGN_LEFT);
		zAxisTranslationCheckBox = new WidgetCheckBox("Z");
		zAxisTranslationCheckBox.Checked = true;
		zAxisTranslationCheckBox.EventChanged.Connect(() =>
		{
			component.ZAxisTranslation = zAxisTranslationCheckBox.Checked;
		});
		hBox.AddChild(zAxisTranslationCheckBox, Gui.ALIGN_LEFT);

		// Rotation axis controls
		hBox = new WidgetHBox(10);
		parameters.AddChild(hBox, Gui.ALIGN_LEFT);
		label = new WidgetLabel("Rotation:");
		label.FontWrap = 1;
		label.FontRich = 1;
		label.Width = 100;
		hBox.AddChild(label, Gui.ALIGN_LEFT);
		xAxisRotationCheckBox = new WidgetCheckBox("X");
		xAxisRotationCheckBox.Checked = true;
		xAxisRotationCheckBox.EventChanged.Connect(() =>
		{
			component.XAxisRotation = xAxisRotationCheckBox.Checked;
		});
		hBox.AddChild(xAxisRotationCheckBox, Gui.ALIGN_LEFT);
		yAxisRotationCheckBox = new WidgetCheckBox("Y");
		yAxisRotationCheckBox.Checked = true;
		yAxisRotationCheckBox.EventChanged.Connect(() =>
		{
			component.YAxisRotation = yAxisRotationCheckBox.Checked;
		});
		hBox.AddChild(yAxisRotationCheckBox, Gui.ALIGN_LEFT);
		zAxisRotationCheckBox = new WidgetCheckBox("Z");
		zAxisRotationCheckBox.Checked = true;
		zAxisRotationCheckBox.EventChanged.Connect(() =>
		{
			component.ZAxisRotation = zAxisRotationCheckBox.Checked;
		});
		hBox.AddChild(zAxisRotationCheckBox, Gui.ALIGN_LEFT);

		// Scale axis controls
		hBox = new WidgetHBox(10);
		parameters.AddChild(hBox, Gui.ALIGN_LEFT);
		label = new WidgetLabel("Scale:");
		label.FontWrap = 1;
		label.FontRich = 1;
		label.Width = 100;
		hBox.AddChild(label, Gui.ALIGN_LEFT);
		xAxisScaleCheckBox = new WidgetCheckBox("X");
		xAxisScaleCheckBox.Checked = true;
		xAxisScaleCheckBox.EventChanged.Connect(() =>
		{
			component.XAxisScale = xAxisScaleCheckBox.Checked;
		});
		hBox.AddChild(xAxisScaleCheckBox, Gui.ALIGN_LEFT);
		yAxisScaleCheckBox = new WidgetCheckBox("Y");
		yAxisScaleCheckBox.Checked = true;
		yAxisScaleCheckBox.EventChanged.Connect(() =>
		{
			component.YAxisScale = yAxisScaleCheckBox.Checked;
		});
		hBox.AddChild(yAxisScaleCheckBox, Gui.ALIGN_LEFT);
		zAxisScaleCheckBox = new WidgetCheckBox("Z");
		zAxisScaleCheckBox.Checked = true;
		zAxisScaleCheckBox.EventChanged.Connect(() =>
		{
			component.ZAxisScale = zAxisScaleCheckBox.Checked;
		});
		hBox.AddChild(zAxisScaleCheckBox, Gui.ALIGN_LEFT);

		// World/Local basis toggle - mutually exclusive toggle buttons
		hBox = new WidgetHBox(10);
		parameters.AddChild(hBox, Gui.ALIGN_LEFT);
		worldBasisButton = new WidgetButton("World");
		hBox.AddChild(worldBasisButton, Gui.ALIGN_LEFT);
		worldBasisButton.Toggleable = true;
		worldBasisButton.Toggled = true;
		worldBasisButton.EventChanged.Connect(() =>
		{
			localBasis = !worldBasisButton.Toggled;
			localBasisButton.Toggled = localBasis;
			component.LocalBasis = localBasis;
		});
		localBasisButton = new WidgetButton("Local");
		hBox.AddChild(localBasisButton, Gui.ALIGN_LEFT);
		localBasisButton.Toggleable = true;
		localBasisButton.Toggled = false;
		localBasisButton.EventChanged.Connect(() =>
		{
			localBasis = localBasisButton.Toggled;
			worldBasisButton.Toggled = !localBasis;
			component.LocalBasis = localBasis;
		});
	}
	
	void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
	}
}
