// Spring variant using direct physics parameters (stiffness, damping) from component
// properties. Provides RefreshSpring() to reset values, useful when gameplay logic
// modifies spring parameters dynamically (e.g., difficulty ramping).

using Unigine;

// Spring motion with direct control over physics parameters.
// Exposes stiffness and damping as editable properties for fine-tuning.
// Provides runtime setters for dynamic difficulty adjustment (used in game mode).
public partial class SpringRegular : SpringMotion
{
	[ShowInEditor, Parameter(Title = "Stiffness")]
	public float defaultStiffness = 3.0f;
	[ShowInEditor, Parameter(Title = "Damping")]
	public float defaultDamping = 0.75f;

	// Runtime access for the game mode, which ramps difficulty up (not an editor parameter)
	public float Stiffness
	{
		get { return stiffness; }
		set { stiffness = value; }
	}

	public float Damping
	{
		get { return damping; }
		set { damping = value; }
	}

	public bool Finished
	{
		get { return finished; }
	}

	// Spring settings are reset to property-defined defaults.
	public void RefreshSpring()
	{
		RefreshSpringSettings();
	}

	// Called when component is deactivated. Spring parameters are reset to defaults.
	protected override void OnDisable()
	{
		// Reset to default values when component is disabled
		RefreshSpringSettings();
	}

	// Stiffness and damping are set from property-defined defaults.
	protected override void RefreshSpringSettings()
	{
		// Use property-defined defaults (allows reset after game mode difficulty ramping)
		stiffness = defaultStiffness;
		damping = defaultDamping;
	}
}
