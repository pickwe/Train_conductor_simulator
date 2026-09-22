// Comprehensive demo showcasing all UNIGINE trigger types side-by-side.
// Demonstrates visual feedback when objects enter/leave different trigger volumes:
// - PhysicalTrigger: Physics-based triggers (sphere, capsule, cylinder, box shapes)
// - WorldTrigger: Simple bounding box trigger without physics
// - MathTriggerComponent: Custom triggers using pure math bounds checking
// - IntersectionTriggerComponent: Custom triggers using spatial intersection queries
// - NodeTrigger: Responds to node enabled/disabled and position change events
// Each trigger changes its postament's material color when activated.

using System.Collections;
using System.Collections.Generic;
using Unigine;

// Orchestrates the trigger showcase with event handlers and visualizations.
public partial class TriggerSample : Component
{
	// The movable object that enters/exits triggers
	[ShowInEditor]
	private Node targetToCheck = null;

	// Postament meshes for each trigger type (change material on activation)
	[ShowInEditor]
	private Node postamentPhysicsSphere = null;
	[ShowInEditor]
	private Node postamentPhysicsCapsule = null;
	[ShowInEditor]
	private Node postamentPhysicsCylinder = null;
	[ShowInEditor]
	private Node postamentPhysicsBox = null;
	[ShowInEditor]
	private Node postamentWorld = null;
	[ShowInEditor]
	private Node postamentMathSphere = null;
	[ShowInEditor]
	private Node postamentMathBox = null;
	[ShowInEditor]
	private Node postamentIntersectionSphere = null;
	[ShowInEditor]
	private Node postamentIntersectionBox = null;

	// Physics-based triggers (require physics bodies to detect)
	[ShowInEditor]
	PhysicalTrigger physicalTriggerSphere = null;
	[ShowInEditor]
	PhysicalTrigger physicalTriggerCapsule = null;
	[ShowInEditor]
	PhysicalTrigger physicalTriggerCylinder = null;
	[ShowInEditor]
	PhysicalTrigger physicalTriggerBox = null;

	// Simple bounding box trigger without physics requirements
	[ShowInEditor]
	WorldTrigger worldTrigger = null;

	// Custom triggers using different detection methods
	[ShowInEditor]
	private MathTriggerComponent mathTriggerSphere = null;
	[ShowInEditor]
	private MathTriggerComponent mathTriggerBox = null;
	[ShowInEditor]
	private IntersectionTriggerComponent intersectionTriggerSphere = null;
	[ShowInEditor]
	private IntersectionTriggerComponent intersectionTriggerBox = null;

	// NodeTrigger demonstrates enabled/position change events
	[ShowInEditor]
	private NodeTrigger nodeTrigger = null;
	[ShowInEditor]
	private Node triggerNodeParentNode = null;
	[ShowInEditor]
	private Node triggerNodeText = null;

	// Materials for normal and triggered states
	[ShowInEditor]
	private Material postamentMat = null;
	[ShowInEditor]
	private Material postamentMatTriggered = null;

	SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();
	Visualizer.MODE visualizerMode;

	// Wires up all trigger callbacks and creates UI controls.
	void Init()
	{
		// Enable visualization with depth testing for clear trigger shapes
		Visualizer.Enabled = true;
		visualizerMode = Visualizer.Mode;
		Visualizer.Mode = Visualizer.MODE.ENABLED_DEPTH_TEST_ENABLED;

		// Math triggers need objects explicitly registered for tracking
		mathTriggerSphere.AddObject(targetToCheck);
		mathTriggerBox.AddObject(targetToCheck);

		// === Physical Trigger callbacks - change postament material on enter/leave ===

		physicalTriggerSphere.EventEnter.Connect(() =>
		{
			ObjectMeshStatic obj = postamentPhysicsSphere as ObjectMeshStatic;
			obj.SetMaterial(postamentMatTriggered, 0);
		});

		physicalTriggerSphere.EventLeave.Connect(() =>
		{
			ObjectMeshStatic obj = postamentPhysicsSphere as ObjectMeshStatic;
			obj.SetMaterial(postamentMat, 0);
		});

		physicalTriggerCapsule.EventEnter.Connect(() =>
		{
			ObjectMeshStatic obj = postamentPhysicsCapsule as ObjectMeshStatic;
			obj.SetMaterial(postamentMatTriggered, 0);
		});

		physicalTriggerCapsule.EventLeave.Connect(() =>
		{
			ObjectMeshStatic obj = postamentPhysicsCapsule as ObjectMeshStatic;
			obj.SetMaterial(postamentMat, 0);
		});

		physicalTriggerCylinder.EventEnter.Connect(() =>
		{
			ObjectMeshStatic obj = postamentPhysicsCylinder as ObjectMeshStatic;
			obj.SetMaterial(postamentMatTriggered, 0);
		});

		physicalTriggerCylinder.EventLeave.Connect(() =>
		{
			ObjectMeshStatic obj = postamentPhysicsCylinder as ObjectMeshStatic;
			obj.SetMaterial(postamentMat, 0);
		});

		physicalTriggerBox.EventEnter.Connect(() =>
		{
			ObjectMeshStatic obj = postamentPhysicsBox as ObjectMeshStatic;
			obj.SetMaterial(postamentMatTriggered, 0);
		});

		physicalTriggerBox.EventLeave.Connect(() =>
		{
			ObjectMeshStatic obj = postamentPhysicsBox as ObjectMeshStatic;
			obj.SetMaterial(postamentMat, 0);
		});

		// === World Trigger callback - simple AABB trigger ===
		worldTrigger.EventEnter.Connect(() =>
		{
			ObjectMeshStatic obj = postamentWorld as ObjectMeshStatic;
			obj.SetMaterial(postamentMatTriggered, 0);
		});

		worldTrigger.EventLeave.Connect(() =>
		{
			ObjectMeshStatic obj = postamentWorld as ObjectMeshStatic;
			obj.SetMaterial(postamentMat, 0);
		});

		// === Math Trigger callbacks - custom pure-math detection ===
		mathTriggerSphere.EventEnter.Connect(() =>
		{
			ObjectMeshStatic obj = postamentMathSphere as ObjectMeshStatic;
			obj.SetMaterial(postamentMatTriggered, 0);
		});

		mathTriggerSphere.EventLeave.Connect(() =>
		{
			ObjectMeshStatic obj = postamentMathSphere as ObjectMeshStatic;
			obj.SetMaterial(postamentMat, 0);
		});

		mathTriggerBox.EventEnter.Connect(() =>
		{
			ObjectMeshStatic obj = postamentMathBox as ObjectMeshStatic;
			obj.SetMaterial(postamentMatTriggered, 0);
		});

		mathTriggerBox.EventLeave.Connect(() =>
		{
			ObjectMeshStatic obj = postamentMathBox as ObjectMeshStatic;
			obj.SetMaterial(postamentMat, 0);
		});

		// === Intersection Trigger callbacks - uses World.GetIntersection() ===
		// These receive the triggering node as parameter and filter by mask
		intersectionTriggerSphere.EventEnter.Connect((Node node_trigger) =>
		{
			Unigine.Object obj = node_trigger as Unigine.Object;
			// Only respond if the object has the correct intersection mask
			if (obj && (obj.GetIntersectionMask(0) == intersectionTriggerSphere.MaterialBallIntersectionMask))
			{
				ObjectMeshStatic postament = postamentIntersectionSphere as ObjectMeshStatic;
				postament.SetMaterial(postamentMatTriggered, 0);
			}
		});

		intersectionTriggerSphere.EventLeave.Connect((Node node_trigger) =>
		{
			Unigine.Object obj = node_trigger as Unigine.Object;
			if (obj && (obj.GetIntersectionMask(0) == intersectionTriggerSphere.MaterialBallIntersectionMask))
			{
				ObjectMeshStatic postament = postamentIntersectionSphere as ObjectMeshStatic;
				postament.SetMaterial(postamentMat, 0);
			}
		});

		intersectionTriggerBox.EventEnter.Connect((Node node_trigger) =>
		{
			Unigine.Object obj = node_trigger as Unigine.Object;
			if (obj && (obj.GetIntersectionMask(0) == intersectionTriggerSphere.MaterialBallIntersectionMask))
			{
				ObjectMeshStatic postament = postamentIntersectionBox as ObjectMeshStatic;
				postament.SetMaterial(postamentMatTriggered, 0);
			}
		});

		intersectionTriggerBox.EventLeave.Connect((Node node_trigger) =>
		{
			Unigine.Object obj = node_trigger as Unigine.Object;
			if (obj && (obj.GetIntersectionMask(0) == intersectionTriggerSphere.MaterialBallIntersectionMask))
			{
				ObjectMeshStatic postament = postamentIntersectionBox as ObjectMeshStatic;
				postament.SetMaterial(postamentMat, 0);
			}
		});

		// === NodeTrigger callbacks - responds to node state changes ===
		// EventEnabled fires when the node is enabled/disabled
		nodeTrigger.EventEnabled.Connect((NodeTrigger trigger) =>
		{
			var  objectText = triggerNodeText as ObjectText;
			if (trigger.Enabled)
				objectText.TextColor = vec4.WHITE;
			else
				objectText.TextColor = vec4.RED;
		});

		// EventPosition fires whenever the node's position changes
		nodeTrigger.EventPosition.Connect((NodeTrigger trigger) =>
		{
			Unigine.Object parent = trigger.Parent as Unigine.Object;
			Material material = parent.GetMaterialInherit(0);
			vec4 color = material.GetParameterFloat4("albedo_color");
			color.z += Game.IFps;
			if (color.z > 1.0f)
				color.z = 0.0f;
			material.SetParameterFloat4("albedo_color", color);
		});


		// Create UI with checkbox to toggle NodeTrigger's parent node
		sampleDescriptionWindow.createWindow();
		WidgetGroupBox parameters = sampleDescriptionWindow.getParameterGroupBox();
		var nodeTriggerCheckbox = new WidgetCheckBox("Cube Active");
		parameters.AddChild(nodeTriggerCheckbox, Gui.ALIGN_LEFT);
		nodeTriggerCheckbox.EventChanged.Connect(() =>
		{
			// Toggling parent node triggers the EventEnabled callback
			triggerNodeParentNode.Enabled = nodeTriggerCheckbox.Checked;
		});
		nodeTriggerCheckbox.Checked = true;
	}

	// Renders trigger volumes for visual debugging.
	void Update()
	{
		// WorldTrigger needs manual visualization
		Visualizer.RenderBoundBox(worldTrigger.BoundBox, worldTrigger.WorldTransform, vec4.RED);

		// PhysicalTriggers have built-in visualizer rendering
		physicalTriggerSphere.RenderVisualizer();
		physicalTriggerCapsule.RenderVisualizer();
		physicalTriggerCylinder.RenderVisualizer();
		physicalTriggerBox.RenderVisualizer();
	}

	// Restores visualizer settings and cleans up UI.
	void Shutdown()
	{
		Visualizer.Mode = visualizerMode;
		Visualizer.Enabled = false;

		sampleDescriptionWindow.shutdown();
	}
}
