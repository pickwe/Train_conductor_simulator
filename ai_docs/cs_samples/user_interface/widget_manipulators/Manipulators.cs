// Interactive 3D manipulator widgets for translating, rotating, and scaling objects.
// Click to select objects in the scene, then use hotkeys W/E/R to switch between
// translate/rotate/scale manipulators. F to focus on object, U/Esc to deselect.
// Supports local/world coordinate basis and per-axis enable/disable.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

using Unigine;

// Runtime object manipulation using WidgetManipulator (translate/rotate/scale gizmos).
public partial class Manipulators : Component
{
	// Mask for raycasting to detect clickable objects
	[ShowInEditor][ParameterMask]
	private int intersectionMask = 1;
	// When true, transforms parent node instead of clicked node
	[ShowInEditor]
	private bool transformParent = true;

	// Rotation axis lock properties - each toggles corresponding axis in rotator mask.
	// When disabled, that axis handle won't appear and rotation is blocked on that axis.
	private bool xAxisRotation = true;
	public bool XAxisRotation
	{
		get { return xAxisRotation; }

		set
		{
			xAxisRotation = value;
			if (xAxisRotation)
				objectRotator.Mask = objectRotator.Mask | WidgetManipulator.MASK_X;
			else
				objectRotator.Mask = objectRotator.Mask & ~(WidgetManipulator.MASK_X);
		}
	}

	private bool yAxisRotation = true;
	public bool YAxisRotation
	{
		get { return yAxisRotation; }

		set
		{
			yAxisRotation = value;
			if (yAxisRotation)
				objectRotator.Mask = objectRotator.Mask | WidgetManipulator.MASK_Y;
			else
				objectRotator.Mask = objectRotator.Mask & ~(WidgetManipulator.MASK_Y);
		}
	}

	private bool zAxisRotation = true;
	public bool ZAxisRotation
	{
		get { return zAxisRotation; }

		set
		{
			zAxisRotation = value;
			if (zAxisRotation)
				objectRotator.Mask = objectRotator.Mask | WidgetManipulator.MASK_Z;
			else
				objectRotator.Mask = objectRotator.Mask & ~(WidgetManipulator.MASK_Z);
		}
	}

	// Enables or disables all rotation axes at once.
	public void SetAxesRotation(bool value)
	{
		XAxisRotation = value;
		YAxisRotation = value;
		ZAxisRotation = value;
	}

	// Translation axis lock properties - same pattern as rotation.
	private bool xAxisTranslation = true;
	public bool XAxisTranslation
	{
		get { return xAxisTranslation; }

		set
		{
			xAxisTranslation = value;
			if (xAxisTranslation)
				objectTranslator.Mask = objectTranslator.Mask | WidgetManipulator.MASK_X;
			else
				objectTranslator.Mask = objectTranslator.Mask & ~(WidgetManipulator.MASK_X);
		}
	}

	private bool yAxisTranslation = true;
	public bool YAxisTranslation
	{
		get { return yAxisTranslation; }

		set
		{
			yAxisTranslation = value;
			if (yAxisTranslation)
				objectTranslator.Mask = objectTranslator.Mask | WidgetManipulator.MASK_Y;
			else
				objectTranslator.Mask = objectTranslator.Mask & ~(WidgetManipulator.MASK_Y);
		}
	}

	private bool zAxisTranslation = true;
	public bool ZAxisTranslation
	{
		get { return zAxisTranslation; }

		set
		{
			zAxisTranslation = value;
			if (zAxisTranslation)
				objectTranslator.Mask = objectTranslator.Mask | WidgetManipulator.MASK_Z;
			else
				objectTranslator.Mask = objectTranslator.Mask & ~(WidgetManipulator.MASK_Z);
		}
	}

	// Enables or disables all translation axes at once.
	public void SetAxesTranslation(bool value)
	{
		XAxisTranslation = value;
		YAxisTranslation = value;
		ZAxisTranslation = value;
	}

	// Scale axis lock properties - same pattern as rotation and translation.
	private bool xAxisScale = true;
	public bool XAxisScale
	{
		get { return xAxisScale; }

		set
		{
			xAxisScale = value;
			if (xAxisScale)
				objectScaler.Mask = objectScaler.Mask | WidgetManipulator.MASK_X;
			else
				objectScaler.Mask = objectScaler.Mask & ~(WidgetManipulator.MASK_X);
		}
	}

	private bool yAxisScale = true;
	public bool YAxisScale
	{
		get { return yAxisScale; }

		set
		{
			yAxisScale = value;
			if (yAxisScale)
				objectScaler.Mask = objectScaler.Mask | WidgetManipulator.MASK_Y;
			else
				objectScaler.Mask = objectScaler.Mask & ~(WidgetManipulator.MASK_Y);
		}
	}

	private bool zAxisScale = true;
	public bool ZAxisScale
	{
		get { return zAxisScale; }

		set
		{
			zAxisScale = value;
			if (zAxisScale)
				objectScaler.Mask = objectScaler.Mask | WidgetManipulator.MASK_Z;
			else
				objectScaler.Mask = objectScaler.Mask & ~(WidgetManipulator.MASK_Z);
		}
	}

	// Enables or disables all scale axes at once.
	public void SetAxesScale(bool value)
	{
		XAxisScale = value;
		YAxisScale = value;
		ZAxisScale = value;
	}

	// When true, manipulator axes align with object's local orientation.
	// When false, axes align with world coordinate system.
	private bool localBasis = false;
	public bool LocalBasis
	{
		get { return localBasis; }

		set
		{
			localBasis = value;
			SetManipulatorsBasis();
		}
	}

	// Whether an object is currently selected
	public bool Active { get { return obj != null; } }

	// The three manipulator widget types
	private WidgetManipulator currentObjectManipulator;
	private WidgetManipulatorTranslator objectTranslator;
	private WidgetManipulatorRotator objectRotator;
	private WidgetManipulatorScaler objectScaler;

	// Event fired when object transform changes via manipulator
	public Event<Object> EventTransformChanged { get { return transformChanged; } }
	private readonly EventInvoker<Object> transformChanged = new();

	// Currently selected object
	private Unigine.Object obj;

	private Gui gui;

	// Creates the three manipulator widgets and sets up event handling.
	void Init()
	{
		gui = WindowManager.MainWindow.Gui;

		// Create translate, rotate, and scale manipulator widgets
		objectTranslator = new WidgetManipulatorTranslator(gui);
		objectRotator = new WidgetManipulatorRotator(gui);
		objectScaler = new WidgetManipulatorScaler(gui);

		gui.AddChild(objectTranslator);
		gui.AddChild(objectRotator);
		gui.AddChild(objectScaler);

		// Start with all manipulators hidden
		objectTranslator.Hidden = true;
		objectRotator.Hidden = true;
		objectScaler.Hidden = true;

		// Default to translation mode
		currentObjectManipulator = objectTranslator;

		// Wire up transform application when manipulator changes
		objectTranslator.EventChanged.Connect(ApplyTransform);
		objectRotator.EventChanged.Connect(ApplyTransform);
		objectScaler.EventChanged.Connect(ApplyTransform);

		// Disable player control to allow manipulator hotkeys
		Player player = Game.Player;
		player.Controlled = false;
	}

	// Handles input for object selection, manipulator switching, and camera focus.
	void Update()
	{
		Player player = Game.Player;

		// When mouse is grabbed (camera rotation), re-enable player control
		if (Input.MouseGrab)
			player.Controlled = true;

		// Sync manipulator projection with camera for correct 3D rendering
		if (player)
		{
			objectTranslator.Projection = player.Projection;
			objectRotator.Projection = player.Projection;
			objectScaler.Projection = player.Projection;

			objectTranslator.Modelview = player.Camera.Modelview;
			objectRotator.Modelview = player.Camera.Modelview;
			objectScaler.Modelview = player.Camera.Modelview;
		}

		// Object selection via left-click raycast
		if (Input.IsMouseButtonUp(Input.MOUSE_BUTTON.LEFT) && !Input.MouseGrab)
		{
			var hoveredWidget = Gui.GetCurrent().GetUnderCursorWidget();
			if (!hoveredWidget)
			{
				obj = GetNodeUnderCursor();
				if (obj)
				{
					SwitchManipulator(currentObjectManipulator);
				}
				else
				{
					Unselect();
				}
			}
		}

		// Object is selected - handle manipulator interactions
		if (obj)
		{
			// Update manipulator position after releasing mouse
			if (Input.IsMouseButtonUp(Input.MOUSE_BUTTON.LEFT))
			{
				SwitchManipulator(currentObjectManipulator);
			}

			// Hotkeys to switch manipulator type (only when mouse not grabbed)
			if (!Input.MouseGrab)
			{
				if (Input.IsKeyDown(Input.KEY.W))
					SwitchManipulator(objectTranslator);

				if (Input.IsKeyDown(Input.KEY.E))
					SwitchManipulator(objectRotator);

				if (Input.IsKeyDown(Input.KEY.R))
					SwitchManipulator(objectScaler);
			}

			// F key - focus camera on selected object
			if (Input.IsKeyDown(Input.KEY.F))
			{
				vec3 inversePlayerViewDirection = -player.ViewDirection;
				WorldBoundSphere bs = new WorldBoundSphere();
				bs.Clear();
				GetMeshBS(obj, ref bs);
				// Position camera at 2x bounding sphere radius from center
				player.WorldPosition = bs.Center + new Vec3(inversePlayerViewDirection * ((float)bs.Radius * 2.0f));
			}

			// U or Escape - deselect object
			if (Input.IsKeyDown(Input.KEY.U) || Input.IsKeyDown(Input.KEY.ESC))
			{
				Unselect();
			}
		}
	}

	// Cleans up manipulator widgets.
	void Shutdown()
	{
		objectTranslator.DeleteLater();
		objectRotator.DeleteLater();
		objectScaler.DeleteLater();
	}

	// Sets whether to transform parent node instead of selected node.
	public void SetTransformParent(bool value)
	{
		transformParent = value;
		UpdateManipulatorTransform();
	}

	// Syncs manipulator transform with the target node.
	public void UpdateManipulatorTransform()
	{
		if (obj && currentObjectManipulator)
		{
			Node manipulateNode = obj;

			if (transformParent && manipulateNode.Parent)
				manipulateNode = manipulateNode.Parent;

			currentObjectManipulator.Transform=manipulateNode.WorldTransform;
		}
	}

	// Recursively collects bounding sphere from node hierarchy for camera focus.
	private void GetMeshBS(in Node n, ref WorldBoundSphere bs)
	{
		if (!n) return;
		if (n.IsObject)
			bs.Expand(n.WorldBoundSphere);

		if (n.Type == Node.TYPE.NODE_REFERENCE)
			GetMeshBS((n as NodeReference).Reference, ref bs);
		for (int i = 0; i < n.NumChildren; i++)
			GetMeshBS(n.GetChild(i), ref bs);
	}

	// Transfers manipulator widget transform to the selected object.
	private void ApplyTransform()
	{
		if (obj)
		{
			Node manipulateNode = obj;

			if (transformParent && manipulateNode.Parent)
				manipulateNode = manipulateNode.Parent;

			manipulateNode.WorldTransform = currentObjectManipulator.Transform;
			transformChanged.Run(obj);
		}
	}

	// Raycasts from camera through mouse position to find object.
	private Unigine.Object GetNodeUnderCursor()
	{
		Player player = Game.Player;
		ivec2 mouse = Input.MousePosition;

		// Cast ray from camera through cursor position, up to 10000 units distance
		return World.GetIntersection(player.WorldPosition, player.WorldPosition + new Vec3(player.GetDirectionFromMainWindow(mouse.x, mouse.y) * 10000), intersectionMask);
	}

	// Activates specified manipulator and hides others.
	private void SwitchManipulator(WidgetManipulator currentManipulator)
	{
		if (obj)
		{
			SetManipulatorsBasis();

			currentObjectManipulator = currentManipulator;
			currentObjectManipulator.Hidden = false;

			Node manipulateNode = obj;

			if (transformParent && manipulateNode.Parent)
				manipulateNode = manipulateNode.Parent;

			currentObjectManipulator.Transform = manipulateNode.WorldTransform;

			// Hide inactive manipulators
			if (objectTranslator != currentObjectManipulator)
				objectTranslator.Hidden = true;
			if (objectRotator != currentObjectManipulator)
				objectRotator.Hidden = true;
			if (objectScaler != currentObjectManipulator)
				objectScaler.Hidden = true;
		}
	}

	// Clears selection and hides all manipulators.
	private void Unselect()
	{
		obj = null;

		objectTranslator.Hidden = true;
		objectRotator.Hidden = true;
		objectScaler.Hidden = true;
	}

	// Updates manipulator axes to use local or world coordinate system.
	private void SetManipulatorsBasis()
	{
		if (obj)
		{
			if (localBasis)
			{
				// Align manipulator axes with object's local orientation
				objectRotator.Basis = obj.WorldTransform;
				objectTranslator.Basis = obj.WorldTransform;
				objectScaler.Basis = obj.WorldTransform;
			}
			else
			{
				// Use world-aligned axes
				objectRotator.Basis = Mat4.IDENTITY;
				objectTranslator.Basis = Mat4.IDENTITY;
				objectScaler.Basis = Mat4.IDENTITY;
			}
		}
	}
}
