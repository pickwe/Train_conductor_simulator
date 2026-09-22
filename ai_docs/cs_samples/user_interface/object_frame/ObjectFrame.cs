// Draws a 2D bounding frame around 3D objects on screen using WidgetCanvas.
// Projects the object's world-space bounding box corners to screen coordinates,
// then renders a colored border with label. Includes visibility distance culling,
// line-of-sight occlusion checking, and JSON metadata export for ML training data.

using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using BoundBox = Unigine.WorldBoundBox;
#else
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using BoundBox = Unigine.BoundBox;
#endif
#endregion

// Component that renders 2D bounding boxes around 3D objects with labels.
public partial class ObjectFrame : Component
{
	// Border thickness in pixels
	[ShowInEditor]
	[Parameter(Title = "Border Size")]
	private int borderSize = 3;

	// Border color for the frame rectangle
	[ShowInEditor]
	[ParameterColor(Title = "Border Color")]
	private vec4 borderColor = vec4.BLUE;

	// Label text displayed in frame header
	[ShowInEditor]
	[Parameter(Title = "Name")]
	private string labelText = "Name";

	// Font size for label text
	[ShowInEditor]
	[Parameter(Title = "Name Size")]
	private int labelTextSize = 15;

	// Outline thickness for label readability
	[ShowInEditor]
	[Parameter(Title = "Name Outline")]
	private int labelTextOutline = 1;

	// Label text color
	[ShowInEditor]
	[ParameterColor(Title = "Name Color")]
	private vec4 labelTextColor = vec4.WHITE;

	// Maximum distance from camera where frame is visible
	[ShowInEditor]
	[Parameter(Title = "Visibility Distance")]
	private float visibilityDistance = 40.0f;

	// Shared canvas for all ObjectFrame instances (static for efficiency)
	static WidgetCanvas canvas;
	static bool canvasClean;
	static int componentsCount;

	// Cached bounding box data
	BoundBox boundBox;
	private Vec3[] points;
	Vec3 localCenter;

	float maxVisibilityDistance;

	// Tracking for visibility and screen coordinates
	bool isFrameRendered;
	ivec2 minPoint;
	ivec2 maxPoint;

	// Collects mesh bounds and creates shared canvas if first instance.
	void Init()
	{
		componentsCount++;
		// Reset transform to get local bounds, then restore
		var initTransform = node.WorldTransform;
		node.WorldTransform = new Mat4(MathLib.Scale(node.Scale));
		collectMeshBoundBox(node);

		points = boundBox.Points;
		localCenter = boundBox.Center;
		node.WorldTransform = initTransform;

		// Create shared canvas on first ObjectFrame initialization
		if (!canvas)
		{
			var gui = Gui.GetCurrent();
			canvas = new WidgetCanvas(gui);
			gui.AddChild(canvas, Gui.ALIGN_OVERLAP | Gui.ALIGN_EXPAND);
			canvasClean = false;
		}

		// Squared distance for faster comparison
		maxVisibilityDistance = visibilityDistance * visibilityDistance;
	}

	// Projects bounding box to screen and draws frame if visible.
	void Update()
	{
		var window = WindowManager.MainWindow;
		var player = Game.Player;
		if (!window || !player)
			return;

		isFrameRendered = false;
		// Clear canvas once per frame (first component clears)
		if (!canvasClean)
		{
			canvasClean = true;
			canvas.Clear();
		}

		// Distance culling check
		float distance = (float)(node.WorldPosition - player.WorldPosition).Length2;
		if (distance > maxVisibilityDistance)
			return;

		// Project all 8 bounding box corners to screen space
		var screenSize = window.ClientSize;
		minPoint = new ivec2(screenSize.x, screenSize.y);
		maxPoint = new ivec2(0, 0);

		int x = 0, y = 0;
		bool visible = false;
		var transform = node.WorldTransform;
		foreach (var point in points)
		{
			visible |= getScreenPosition(ref x, ref y, new Vec3(transform * point), player, screenSize);
			minPoint.x = MathLib.Min(minPoint.x, x);
			minPoint.y = MathLib.Min(minPoint.y, y);
			maxPoint.x = MathLib.Max(maxPoint.x, x);
			maxPoint.y = MathLib.Max(maxPoint.y, y);
		}

		// Skip if entirely behind camera or off screen
		if (!visible || !isRectangleOnScreen(screenSize, minPoint, maxPoint))
			return;

		// Occlusion check - ray from camera to object center
		var worldCenter = transform * localCenter;
		var obj = World.GetIntersection(player.WorldPosition, new Vec3(worldCenter), ~0);
		if (!obj || !isChild(obj))
			return;

		int order = (int)(-distance * 100);
		ivec2 minMaxPoint = new ivec2(minPoint.x, maxPoint.y);
		ivec2 maxMinPoint = new ivec2(maxPoint.x, minPoint.y);

		drawRectangle(order, minMaxPoint, minPoint - new ivec2(borderSize));
		drawRectangle(order, minPoint, maxMinPoint + new ivec2(1, -1) * borderSize);
		drawRectangle(order, maxMinPoint, maxPoint + new ivec2(borderSize));
		drawRectangle(order, maxPoint, minMaxPoint + new ivec2(-1, 1) * borderSize);

		int textId = canvas.AddText(order + 1);
		canvas.FontSize = labelTextSize;
		ivec2 textSize = canvas.ToUnitSize(canvas.GetTextRenderSize(labelText));

		var headerPointMin = minPoint - new ivec2(borderSize);
		var headerPointMax = headerPointMin + new ivec2(textSize.x, -textSize.y) + new ivec2(2, -1) * borderSize;
		drawRectangle(order, headerPointMin, headerPointMax);

		canvas.SetTextPosition(textId, new vec2(minPoint.x, headerPointMax.y));
		canvas.SetTextText(textId, labelText);
		canvas.SetTextSize(textId, labelTextSize);
		canvas.SetTextAlign(textId, Gui.ALIGN_LEFT | Gui.ALIGN_BOTTOM | Gui.ALIGN_BACKGROUND);
		canvas.SetTextColor(textId, labelTextColor);
		canvas.SetTextOutline(textId, labelTextOutline);

		isFrameRendered = true;
	}

	// Resets the canvas clean flag after all components have rendered.
	void PostUpdate()
	{
		if (canvasClean)
			canvasClean = false;
	}

	// Cleans up shared canvas when last instance is destroyed.
	void Shutdown()
	{
		componentsCount--;
		if (componentsCount == 0 && (canvas || !canvas.IsDeleted))
		{
			canvas.DeleteLater();
			canvas = null;
		}
	}

	// Globally enables/disables all ObjectFrame rendering.
	public static void setObjectFramesEnabled(bool enabled)
	{
		if (!canvas)
			return;

		var gui = Gui.GetCurrent();
		if (enabled)
			gui.AddChild(canvas, Gui.ALIGN_OVERLAP | Gui.ALIGN_EXPAND | Gui.ALIGN_BACKGROUND);
		else
			gui.RemoveChild(canvas);
	}

	// Returns whether this frame was rendered in the current frame.
	public bool isVisible()
	{
		return isFrameRendered;
	}

	// Exports object metadata as JSON for ML training data generation.
	public Json getJsonMeta()
	{
		Json json = new Json();
		json.SetObject();
		json.AddChild("node_id", node.ID);
		json.AddChild("node_name", node.Name);

		var pos = node.WorldPosition;
		var posJson = json.AddChild("position");
		posJson.SetObject();
		posJson.AddChild("x", pos.x);
		posJson.AddChild("y", pos.y);
		posJson.AddChild("z", pos.z);

		var rot = node.GetWorldRotation();
		var rotJson = json.AddChild("rotation");
		rotJson.SetObject();
		rotJson.AddChild("x", rot.GetAngle(vec3.RIGHT));
		rotJson.AddChild("y", rot.GetAngle(vec3.FORWARD));
		rotJson.AddChild("z", rot.GetAngle(vec3.UP));

		var minFrameJson = json.AddChild("screen_position");
		minFrameJson.SetObject();
		minFrameJson.AddChild("min_x", minPoint.x);
		minFrameJson.AddChild("min_y", minPoint.y);
		minFrameJson.AddChild("max_x", maxPoint.x);
		minFrameJson.AddChild("max_y", maxPoint.y);

		return json;
	}

	// Recursively collects mesh bounds from node hierarchy.
	private void collectMeshBoundBox(in Node n)
	{
		if (!n)
			return;

		// Handle NodeReference by looking at referenced node
		if (n.Type == Node.TYPE.NODE_REFERENCE)
			collectMeshBoundBox(((NodeReference)n).Reference);
		else if (n.Type == Node.TYPE.OBJECT_MESH_STATIC || n.Type == Node.TYPE.OBJECT_MESH_SKINNED
			|| n.Type == Node.TYPE.OBJECT_MESH_DYNAMIC)
		{
			boundBox.Expand(n.WorldBoundBox);
		}

		// Recursively process children
		for (int i = 0; i < n.NumChildren; i++)
			collectMeshBoundBox(n.GetChild(i));
	}

	// Projects world point to screen coordinates, returns true if in front of camera.
	private bool getScreenPosition(ref int x, ref int y, in Vec3 worldPoint,
		in Player player, in ivec2 screenSize)
	{
		float width = screenSize.x;
		float height = screenSize.y;

		mat4 projection = player.Projection;
		mat4 modelview = new mat4(player.Camera.Modelview);
		projection.m00 *= height / width;

		Vec4 p = projection * new Vec4(modelview * new Vec4(worldPoint, 1));
		if (p.w > 0)
		{
			// in front of camera
			x = (int)(width * (0.5f + p.x * 0.5f / p.w));
			y = ((int)(height - height * (0.5f + p.y * 0.5f / p.w)));
			return true;
		}
		else
		{
			// behind camera
			x = (int)(width - (width * (0.5f + p.x * 0.5f / p.w)));
			y = (int)(height - (height - height * (0.5f + p.y * 0.5f / p.w)));
			return false;
		}
	}

	// Draws a filled rectangle on the canvas using polygon primitives.
	private int drawRectangle(int order, in ivec2 p1, in ivec2 p2)
	{
		int id = canvas.AddPolygon(order);

		// Define quad vertices
		canvas.AddPolygonPoint(id, new vec3(p1.x, p1.y, 0));
		canvas.AddPolygonPoint(id, new vec3(p1.x, p2.y, 0));
		canvas.AddPolygonPoint(id, new vec3(p2.x, p2.y, 0));
		canvas.AddPolygonPoint(id, new vec3(p2.x, p1.y, 0));
		// Two triangles for quad
		canvas.AddPolygonIndex(id, 0);
		canvas.AddPolygonIndex(id, 1);
		canvas.AddPolygonIndex(id, 2);
		canvas.AddPolygonIndex(id, 2);
		canvas.AddPolygonIndex(id, 3);
		canvas.AddPolygonIndex(id, 0);

		canvas.SetPolygonColor(id, borderColor);
		return id;
	}

	// Returns true if rectangle overlaps visible screen area.
	private bool isRectangleOnScreen(in ivec2 screenSize, in ivec2 min, in ivec2 max)
	{
		if ((min.x <= 0 && max.x <= 0) || (min.x >= screenSize.x && max.x >= screenSize.x)
			|| (min.y <= 0 && max.y <= 0) || (min.y >= screenSize.y && max.y >= screenSize.y))
			return false;
		return true;
	}

	// Checks if node is this component's node or a descendant.
	private bool isChild(in Node n)
	{
		if (n == node)
			return true;
		var parent = n.Parent;
		if (!parent)
			parent = n.Possessor;
		return parent ? isChild(parent) : false;
	}
}
