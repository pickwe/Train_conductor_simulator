// Comprehensive reference for all component parameter types and attributes in UNIGINE.
// Demonstrates supported data types (primitives, vectors, enums, nodes, materials),
// editor attributes (ShowInEditor, HideInEditor, ParameterMask, ParameterSlider, etc.),
// and component lifecycle methods (Init, Update, Shutdown, etc.) with custom methods.

using System.Collections.Generic;
using Unigine;

// Reference component showcasing all parameter types, attributes, and lifecycle methods.
public partial class ComponentParameters : Component
{
	// All standard types with public modifier are shown in the editor
	public int intVariable;
	public float floatVariable;
	public double doubleVariable;
	public bool boolVariable;
	public string stringVariable;
	public ivec2 integerVec2Variable;
	public vec2 floatVec2Variable;
	public dvec2 doubleVec2Variable;
	public ivec3 integerVec3Variable;
	public vec3 floatVec3Variable;
	public dvec3 doubleVec3Variable;
	public ivec4 integerVec4Variable;
	public vec4 floatVec4Variable;
	public dvec4 doubleVec4Variable;

	public enum MyEnum
	{
		Item0,
		Item1,
		Item2
	}

	public MyEnum enumVariable;

	public Material materialVariable;
	public Property propertyVariable;
	public Node nodeVariable;

	// All descendants of Node also are shown in the editor
	public NodeDummy nodeDummyVariable;
	public ObjectMeshStatic meshStaticVariable;

	// All components and their descendants with a public modifier are shown in the editor
	public Component componentVariable;
	public ComponentParameters inheritedComponentVariable;

	// You can also specify an abstract component as a parameter -
	// it will accept any component derived from this abstract component
	public AbstractComponent abstractComponentVariable;

	// And you can even specify a C# interface as a parameter -
	// it will accept any component that implements this interface
	public Interface interfaceComponentVariable;

	// Empty arrays and arrays with values are shown in the editor
	public int[] intEmptyArray;
	public int[] intValuesArray = { 1, 2, 3 };

	// Empty Lists and Lists with values are shown in the editor
	public List<int> intEmptyList;
	public List<int> intValuesList = new List<int> { 1, 2, 3 };

	// Internal, protected and private variables are not shown in the editor in the general case
	#pragma warning disable CS0414, CS0169, CS0649
	internal int hiddenInternalIntVariable;
	protected int hiddenProtectedIntVariable;
	private int hiddenPrivateIntVariable;

	// But you can use ShowInEditor attribute to show internal, protected and private variables
	[ShowInEditor]
	internal int internalIntVariable;

	[ShowInEditor]
	private int privateIntVariable;

	[ShowInEditor]
	protected int protectedIntVariable;

	// Also you can hide public variable in the editor using the HideInEditor attribute
	[HideInEditor]
	public int hiddenIntVariable = 1;
	#pragma warning restore CS0414, CS0169, CS0649

	// Nested classes are shown in the editor
	public class MyClass
	{
		public int myClassParameter = 1;
	}

	public MyClass myClassVaribale;

	// And nested structures are shown in the editor too
	public struct MyStruct
	{
		public int myStructParameter;
	}

	public MyStruct myStructVariable;

	// You can use the Parameter attribute to change the display name of the variable,
	// add a tooltip for the variable, and organize part of the variables into groups
	[Parameter(Group = "Group Name", Title = "New Name", Tooltip = "Tooltip Text")]
	public int parameterInt;

	// You can use the ParameterMask attribute to display the int variable as various types of masks
	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.COLLISION)]
	public int collisionMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.EXCLUSION)]
	public int exclusionMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.FIELD)]
	public int fieldMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.GENERAL)]
	public int generalMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	public int intersectionMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.MATERIAL)]
	public int materialMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.NAVIGATION)]
	public int navigationMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.OBSTACLE)]
	public int obstacleMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.PHYSICAL)]
	public int physicalMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.PHYSICS_INTERSECTION)]
	public int physicsIntersectionMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.SHADOWS)]
	public int shadowsMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.SOUND_OCCLUSION)]
	public int soundOcclusionMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.SOUND_REVERB)]
	public int soundReverbMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.SOUND_SOURCE)]
	public int soundSourceMask = 1;

	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.VIEWPORT)]
	public int viewportMask = 1;

	// You can use the ParameterSlider attribute to display variables of type int,
	// float and double with various restrictions: minimum and maximum values, the ability
	// to change the minimum and maximum values, the ability to only reduce the minimum value,
	// the ability to only increase the maximum value, changes the slider to a logarithmic
	[ParameterSlider(Min = -100.0f, Max = 100.0f)]
	public int intSlider = 0;

	[ParameterSlider(Min = -100.0f, Max = 100.0f)]
	public float floatSlider = 0.0f;

	[ParameterSlider(Min = -100.0f, Max = 100.0f)]
	public double doubleSlider = 0.0f;

	// You can use the ParameterColor attribute to set the vec4 variable as color using the color widget
	[ParameterColor]
	public vec4 color = vec4.ONE;

	// Link to any asset
	public AssetLink assetLink;

	// You can use the ParameterAsset attribute to set an asset link filter
	[ParameterAsset(Filter = ".world")]
	public AssetLink assetLinkFilter;

	// Link to node asset
	public AssetLinkNode assetLinkNode;

	// You can use the ParameterFile attribute to set the string variable as a path to a file with fixed extensions
	[ParameterFile(Filter = ".node")]
	public string file = "";

	// You can use the ParameterMaterial for Material
	[ParameterMaterial(ParentGUID = "83eb006a9be234c16ec9872c5bd2589b5c8e353e")]
	public Material material;

	// You can use the ParameterProperty for Property
	[ParameterProperty(InternalOnly = false, ParentGUID = "b73d8487fe8b783a79152a51f3475f7c5a3634ab")]
	public Property property;

	// You can use ParameterSwitch attribute to set the int variable as the index of one of the items
	[ParameterSwitch(Items = "switch_item_0,switch_item_1,switch_item_2")]
	public int switchVariable = 1;

	// You can use ParameterCondition to selectively display component parameters if certain conditions are met
	public bool showVariable = true;

	[ParameterCondition(nameof(showVariable), 1)]
	public int conditionVariable;

	// You can use curve for non-linear value dependencies
	public Curve2d curve2D = new Curve2d();

	// This method is called by the Engine, when the component is ready for initialization
	protected override void OnReady()
	{
		Log.Message("OnReady\n");
	}

	// This method is called by the Engine, when the component and node become enabled and active
	protected override void OnEnable()
	{
		Log.Message("OnEnable\n");
	}

	// This method is called by the Engine, when the component and node become disabled
	protected override void OnDisable()
	{
		Log.Message("OnDisable\n");
	}

	// Engine calls this function on world initialization
	// Using the Order parameter of the Method attribute, you can change the order of method calls
	// Also, using the InvokeDisabled parameter, you can configure the method call for the disabled component
	[Method(InvokeDisabled = true, Order = 0)]
	private void Init()
	{
		// Create and initialize all necessary resources

		Log.Message("Init\n");
	}

	// Engine calls this function before the update() and the postUpdate()
	private void UpdateSyncThread()
	{
		// Specify all logic functions you want to be executed before the Update()

		Log.Message("UpdateSyncThread\n");
	}

	// Engine calls this function after execution of all updateSyncThread() functions
	private void UpdateAsyncThread()
	{
		// Specify all logic functions you want to be called every frame independently of the rendering framerate

		Log.Message("UpdateAsyncThread\n");
	}

	// Engine calls this function before updating each render frame
	private void Update()
	{
		// Specify all logic functions you want to be called every frame

		Log.Message("Update\n");
	}

	// Engine calls this function before rendering each render frame
	private void PostUpdate()
	{
		// Correct behavior according to the updated node states in the same frame

		Log.Message("PostUpdate\n");
	}

	// Engine calls this function before updating each physics frame
	private void UpdatePhysics()
	{
		// Simulate physics: perform continuous operations

		Log.Message("UpdatePhysics\n");
	}

	// Engine calls this function before updating each render frame
	private void Swap()
	{
		Log.Message("Swap\n");
	}

	// Engine calls this function on the world shutdown
	private void Shutdown()
	{
		// Perform cleanup on component shutdown

		Log.Message("Shutdown\n");
	}

	// Using the MethodInit attribute, you can create any number of Init methods
	[MethodInit(Order = 1)]
	private void MyInit()
	{
		Log.Message("MyInit\n");
	}

	// Using the MethodUpdateSyncThread attribute, you can create any number of UpdateSyncThread methods
	[MethodUpdateSyncThread]
	private void MyUpdateSyncThread()
	{
		Log.Message("MyUpdateSyncThread\n");
	}

	// Using the MethodUpdateAsyncThread attribute, you can create any number of UpdateAsyncThread methods
	[MethodUpdateAsyncThread]
	private void MyUpdateAsyncThread()
	{
		Log.Message("MyUpdateAsyncThread\n");
	}

	// Using the MethodUpdate attribute, you can create any number of Update methods
	[MethodUpdate(Order = 1)]
	private void MyUpdate()
	{
		Log.Message("MyUpdate\n");
	}

	// Using the MethodPostUpdate attribute, you can create any number of PostUpdate methods
	[MethodPostUpdate(Order = 1)]
	private void MyPostUpdate()
	{
		Log.Message("MyPostUpdate\n");
	}

	// Using the MethodUpdatePhysics attribute, you can create any number of UpdatePhysics methods
	[MethodUpdatePhysics(Order = 1)]
	private void MyUpdatePhysics()
	{
		Log.Message("MyUpdatePhysics\n");
	}

	// Using the MethodSwap attribute, you can create any number of Swap methods
	[MethodSwap(Order = 1)]
	private void MySwap()
	{
		Log.Message("MySwap\n");
	}

	// Using the MethodShutdown attribute, you can create any number of Shutdown methods
	[MethodShutdown(Order = 1)]
	private void MyShutdown()
	{
		Log.Message("MyShutdown\n");
	}
}
