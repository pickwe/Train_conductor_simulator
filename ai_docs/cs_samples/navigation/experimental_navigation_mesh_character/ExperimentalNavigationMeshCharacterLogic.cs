// Interactive demo of an animated character navigating an experimental
// navigation mesh with root-motion locomotion. The character is driven by
// ExperimentalSeekerCharacter: the route is computed and followed by the
// inherited ExperimentalSeeker logic, while the movement itself comes from
// the root motion of the animation graph (a straight walk with the course
// corrected by rotating the node, animated turns in place, start and stop
// animations).
// The target node is moved with the widget manipulator, the locomotion
// settings can be tweaked in the sample window.

using Unigine;

public partial class ExperimentalNavigationMeshCharacterLogic : Component
{
	// sample visualization
	private bool showNavigationMesh = false;

	private ExperimentalNavigationMesh navigationMesh;
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	private Manipulators widgetManipulator;
	private ExperimentalSeekerCharacter character;

	[MethodInit(Order = 2)]
	private void Init()
	{
		Visualizer.Enabled = true;
		Unigine.Console.Run("experimental_navigation_show_mesh 0");
		Unigine.Console.Run("experimental_navigation_show_mesh_mode 1");
		Unigine.Console.Run("experimental_navigation_show_mesh_depth_test 1");
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		// the manipulator moves the target node; rotation and scaling make no
		// sense for a point target and are disabled
		widgetManipulator = GetComponent<Manipulators>(node);
		if (widgetManipulator != null)
		{
			widgetManipulator.SetAxesRotation(false);
			widgetManipulator.SetAxesScale(false);
		}

		var characters = ComponentSystem.FindComponentsInWorld<ExperimentalSeekerCharacter>(true);
		if (characters.Length > 0)
			character = characters[0];
		else
			Log.Warning("ExperimentalNavigationMeshCharacterLogic: no ExperimentalSeekerCharacter component in the world\n");

		// the navigation mesh the character walks on, for the visualization toggle
		if (character != null)
			navigationMesh = character.navigation as ExperimentalNavigationMesh;

		sampleDescriptionWindow.createWindow();
		CreateUi();
	}

	private void Update()
	{
		// disable player controls while dragging the manipulator
		if (widgetManipulator != null)
			Game.Player.Controlled = !widgetManipulator.Active;

		if (showNavigationMesh && navigationMesh)
			navigationMesh.RenderVisualizer();

		UpdateStatus();
	}

	private void Shutdown()
	{
		Visualizer.Enabled = false;
		Input.MouseHandle = Input.MOUSE_HANDLE.GRAB;
		sampleDescriptionWindow.shutdown();
	}

	private void CreateUi()
	{
		if (character == null)
			return;

		sampleDescriptionWindow.addFloatParameter(
			"Movement speed",
			"The walking speed (in m/s): the locomotion animations are played at the rate movement speed / walk animation speed, so the root motion matches it (the rate never drops below the min rate of the character).",
			character.movementSpeed, 0.7f, 6.0f,
			(float value) =>
			{
				character.movementSpeed = value;
			});

		sampleDescriptionWindow.addFloatParameter(
			"Rotation speed",
			"The maximum yaw rate (in degrees per second) of the course correction applied by rotating the node.",
			character.rotationSpeed, 30.0f, 360.0f,
			(float value) =>
			{
				character.rotationSpeed = value;
			});

		sampleDescriptionWindow.addFloatParameter(
			"Turn angle",
			"The angular error (in degrees) above which the character stops (when walking) and turns in place with the turn animation; below it the course is corrected by rotating the node.",
			character.turnAngle, 30.0f, 180.0f,
			(float value) =>
			{
				character.turnAngle = value;
			});

		sampleDescriptionWindow.addFloatParameter(
			"Reach distance",
			"The distance at which the target counts as reached and the character stops; must cover the travel of the stop animation.",
			character.targetReachDistance, 0.2f, 3.0f,
			(float value) =>
			{
				character.targetReachDistance = value;
			});

		sampleDescriptionWindow.addParameterSpacer();

		sampleDescriptionWindow.addBoolParameter(
			"Show Seeker Debug",
			"Draws the route corridor and the velocities of the character.",
			character.debugVisualizerEnabled,
			(bool enabled) =>
			{
				character.debugVisualizerEnabled = enabled;
			});

		sampleDescriptionWindow.addBoolParameter(
			"Show Navigation Mesh",
			"Draws the navigation mesh colored by the areas of its polygons.",
			showNavigationMesh,
			(bool enabled) =>
			{
				showNavigationMesh = enabled;
			});
	}

	private void UpdateStatus()
	{
		if (character == null)
		{
			sampleDescriptionWindow.setStatus("Add the ExperimentalSeekerCharacter component to the world.");
			return;
		}

		sampleDescriptionWindow.setStatus(
			$"Route: {ExperimentalNavigationUtils.GetRouteStatusName(character.RouteStatus)}\n" +
			$"Control state: {character.GetControlStateName()}, animation state: {character.GetLocomotionStateName()}\n" +
			$"Velocity: {character.Velocity.Length:0.00} m/s\n" +
			$"Walk animation: {character.animationSpeed:0.00} m/s at rate 1, playing at rate {character.PlaybackRate:0.00}\n");
	}
}
