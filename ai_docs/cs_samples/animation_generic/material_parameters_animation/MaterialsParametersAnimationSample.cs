// Shows the current values of the animated material parameters in the sample description window.
// Every row is refreshed each frame, so the result of each Materials* component is visible
// as a value and not only as a change of the picture on the screen.

using Unigine;

// Displays the state of the material parameters animated by the Materials* components.
public partial class MaterialsParametersAnimationSample : Component
{
	// Materials of the objects the animating components are attached to
	private Material albedoColorMaterial = null;
	private Material metalnessMaterial = null;
	private Material emissionMaterial = null;
	private Material castWorldShadowMaterial = null;

	// The texture component knows which of its two textures is applied right now
	private MaterialsAlbedoTexture albedoTextureComponent = null;

	private WidgetGridBox stateGrid = null;
	private WidgetEditLine albedoColorRedField = null;
	private WidgetEditLine albedoColorGreenField = null;
	private WidgetEditLine albedoColorBlueField = null;
	private WidgetEditLine albedoTextureField = null;
	private WidgetEditLine metalnessField = null;
	private WidgetEditLine emissionField = null;
	private WidgetEditLine castWorldShadowField = null;

	// Builds the state group box inside the window created by DescriptionWindowCreator.
	private void Init()
	{
		DescriptionWindowCreator windowCreator = FindComponentInWorld<DescriptionWindowCreator>();
		if (windowCreator == null)
		{
			// The window belongs to the utils node reference, without it there is nowhere to show the state
			Log.Warning("MaterialsParametersAnimationSample: no DescriptionWindowCreator in the world, state is not shown\n");
			return;
		}

		WidgetGroupBox stateBox = new WidgetGroupBox("State", 9, 3);
		windowCreator.getWindow().MainWindow.AddChild(stateBox);

		stateGrid = new WidgetGridBox(2);
		stateBox.AddChild(stateGrid, Gui.ALIGN_LEFT);

		// A row is created only for a parameter that is really animated in this world
		albedoColorMaterial = GetMaterial(FindComponentInWorld<MaterialsAlbedoColor>());
		if (albedoColorMaterial)
		{
			albedoColorRedField = AddStateField("albedo_color.r");
			albedoColorGreenField = AddStateField("albedo_color.g");
			albedoColorBlueField = AddStateField("albedo_color.b");
		}

		albedoTextureComponent = FindComponentInWorld<MaterialsAlbedoTexture>();
		if (albedoTextureComponent != null)
			albedoTextureField = AddStateField("albedo texture");

		metalnessMaterial = GetMaterial(FindComponentInWorld<MaterialsMetalness>());
		if (metalnessMaterial)
			metalnessField = AddStateField("metalness");

		emissionMaterial = GetMaterial(FindComponentInWorld<MaterialsEmission>());
		if (emissionMaterial)
			emissionField = AddStateField("emission");

		castWorldShadowMaterial = GetMaterial(FindComponentInWorld<MaterialsCastWorldShadow>());
		if (castWorldShadowMaterial)
			castWorldShadowField = AddStateField("cast_world_shadow");
	}

	// Shows the values the animating components have set this frame.
	private void Update()
	{
		if (albedoColorRedField)
		{
			vec4 color = albedoColorMaterial.GetParameterFloat4("albedo_color");
			albedoColorRedField.Text = color.x.ToString("0.00");
			albedoColorGreenField.Text = color.y.ToString("0.00");
			albedoColorBlueField.Text = color.z.ToString("0.00");
		}

		if (albedoTextureField)
			albedoTextureField.Text = albedoTextureComponent.IsFirstTextureApplied ? "first" : "second";

		if (metalnessField)
			metalnessField.Text = metalnessMaterial.GetParameterFloat("metalness").ToString("0.00");

		if (emissionField)
			emissionField.Text = emissionMaterial.GetState("emission") != 0 ? "on" : "off";

		if (castWorldShadowField)
			castWorldShadowField.Text = castWorldShadowMaterial.CastWorldShadow ? "on" : "off";
	}

	// The window is owned by DescriptionWindowCreator, so there is nothing to release here

	// Material of the object a component is attached to, null if the component is not in the world
	private static Material GetMaterial(Component component)
	{
		if (component == null)
			return null;

		Unigine.Object objectNode = component.node as Unigine.Object;
		if (!objectNode || objectNode.NumSurfaces == 0)
			return null;

		return objectNode.GetMaterial(0);
	}

	// A read-only row "parameter name | current value" in the state grid
	private WidgetEditLine AddStateField(string name)
	{
		WidgetHBox nameBox = new WidgetHBox();
		nameBox.AddChild(new WidgetLabel(name));
		nameBox.AddChild(new WidgetHBox(6));
		stateGrid.AddChild(nameBox, Gui.ALIGN_LEFT);

		WidgetEditLine valueField = new WidgetEditLine();
		valueField.Editable = false;
		valueField.FontVOffset = -2;
		valueField.FontColor = new vec4(0.9f, 0.9f, 0.9f, 1.0f);
		valueField.Width = 50;
		stateGrid.AddChild(valueField, Gui.ALIGN_LEFT);

		return valueField;
	}
}
