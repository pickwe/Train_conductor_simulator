// Demonstrates raycasting from the mouse cursor to detect objects in the scene.
// Casts a ray from the camera through the current mouse position and displays the
// name of any object hit. This is the foundation for object picking, tooltips,
// highlighting objects under cursor, or any mouse-based interaction system.

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

// Displays the name of the object under the mouse cursor using raycast.
public partial class MouseRayIntersection : Component
{
	// Maximum ray length for intersection detection
	public float distance = 100.0f;

	// Intersection mask to filter which objects can be picked
	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	public int mask = 1;

	// Label widget that follows the cursor and shows object name
	private WidgetLabel label = null;
	// Stores original mouse mode to restore on shutdown
	Input.MOUSE_HANDLE initHandle;

	// Sets up soft mouse cursor and creates tooltip label.
	private void Init()
	{
		// Use soft mode so cursor is visible but can still capture input
		initHandle = Input.MouseHandle;
		Input.MouseHandle = Input.MOUSE_HANDLE.SOFT;

		// Create floating label to display hit object name
		label = new WidgetLabel(Gui.GetCurrent());
		label.FontSize = 30;
		label.FontOutline = 1;
		Gui.GetCurrent().AddChild(label, Gui.ALIGN_OVERLAP);
	}

	// Casts ray through mouse position and displays hit object name.
	private void Update()
	{
		// Build ray from camera through mouse screen position
		Vec3 firstPoint = Game.Player.WorldPosition;
		ivec2 mouse_coord = Input.MousePosition;
		Vec3 secondPoint = firstPoint + Game.Player.GetDirectionFromMainWindow(mouse_coord.x, mouse_coord.y) * distance;

		// Perform intersection test with mask filtering
		Unigine.Object hitObject = World.GetIntersection(firstPoint, secondPoint, mask);
		if (hitObject)
		{
			// Change object name
			label.Text = hitObject.Name;
		}
		else
			label.Text = "empty hit object";

		// Position label near cursor
		label.SetPosition(WindowManager.MainWindow.Gui.MouseX + 25, WindowManager.MainWindow.Gui.MouseY + 25);
	}

	// Restores mouse mode and removes tooltip label.
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(label);

		Input.MouseHandle = initHandle;
	}
}
