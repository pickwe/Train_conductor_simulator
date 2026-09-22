// Comprehensive showcase of all Visualizer debug rendering primitives.
// The Visualizer is a powerful tool for debugging and visual feedback, allowing
// you to draw 2D and 3D shapes, text, bounds, and object outlines without
// needing actual geometry in the scene. This component demonstrates every
// available Visualizer method organized across multiple display pedestals.

using System.Collections.Generic;
using Unigine;
using System.Linq;


#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Demonstrates all available Visualizer rendering methods for debug drawing.
public partial class VisualizerUsage : Component
{
	// Toggles for 2D primitives (controlled via UI checkboxes)
	public bool renderPoint2D = false;
	public bool renderLine2D = false;
	public bool renderTriangle2D = false;
	public bool renderQuad2D = false;
	public bool renderRectangle = false;
	public bool renderMessage2D = false;

	// Example objects for node/object bound visualization
	[ShowInEditor]
	[Parameter(Title = "Node bound box example")]
	private Node node_boundBox_example = null;
	[ShowInEditor]
	[Parameter(Title = "Node bound sphere example")]
	private Node node_boundSphere_example = null;
	[ShowInEditor]
	[Parameter(Title = "Object example")]
	private Object object_example = null;
	[ShowInEditor]
	[Parameter(Title = "Object solid example")]
	private Object object_solid_example = null;
	[ShowInEditor]
	[Parameter(Title = "Surface example")]
	private Object surface_example = null;
	[ShowInEditor]
	[Parameter(Title = "Surface solid example")]
	private Object surface_solid_example = null;
	[ShowInEditor]
	[Parameter(Title = "Object surface bound box example")]
	private Object object_surface_boundBox_example = null;
	[ShowInEditor]
	[Parameter(Title = "Object surface bound sphere example")]
	private Object object_surface_boundSphere_example = null;

	// Display pedestals where 3D primitives are rendered
	[ShowInEditor]
	[Parameter(Title = "Object surface bound sphere example")]
	private List<Node> postament_nodes = null;

	// Validates all required references and configures visualizer mode.
	void Init()
	{
		if (!node_boundBox_example || !node_boundSphere_example)
		{
			Log.Error("VisualizerUsage.Init() example nodes are not assigned: \n");
		}
		if (!object_example || !object_solid_example || !surface_example || !surface_solid_example
			|| !object_surface_boundBox_example || !object_surface_boundSphere_example)
		{
			Log.Error("VisualizerUsage.Init() example objects are not assigned: \n");
		}
		foreach (var item in postament_nodes)
		{

			if (!node)
			{
				Log.Error($"VisualizerUsage.Init() postament node with index: {postament_nodes.IndexOf(item)} is not assigned\n");
			}
		}
		// Disable depth testing so visualizer draws on top of everything
		Visualizer.Mode = Visualizer.MODE.ENABLED_DEPTH_TEST_DISABLED;
	}

	// Renders all visualizer examples each frame.
	void Update()
	{
		Update3D();
		Update2D();
	}

	// Draws 2D primitives in screen space (coordinates are 0-1 normalized).
	private void Update2D()
	{
		if (renderPoint2D) Visualizer.RenderPoint2D(new vec2(0.1f, 0.5), 0.01f, vec4.RED);
		if (renderLine2D) Visualizer.RenderLine2D(new vec2(0.1, 0.55), new vec2(0.15, 0.55), new vec2(0.15, 0.60), new vec2(0.20, 0.60), vec4.RED);
		if (renderTriangle2D) Visualizer.RenderTriangle2D(new vec2(0.2, 0.65), new vec2(0.1, 0.62), new vec2(0.1, 0.68), vec4.RED);
		if (renderQuad2D) Visualizer.RenderQuad2D(new vec2(0.1, 0.8), new vec2(0.08, 0.75), new vec2(0.1, 0.70), new vec2(0.12, 0.75), vec4.RED);
		if (renderRectangle) Visualizer.RenderRectangle(new vec4(0.1f, 0.1f, 0.15f, 0.15f), vec4.RED);
		if (renderMessage2D) Visualizer.RenderMessage2D(new vec3(0.1, 0.95, 0), new vec3(0, 0, 0), "renderMessage2D example", vec4.RED);
	}

	// Draws all 3D primitives at pedestal positions throughout the scene.
	private void Update3D()
	{
		int i = 0;
		//1-10
		Vec3 current_point = GetPostamentPoint(i);
		Visualizer.RenderLine3D(current_point, current_point + Vec3.UP, current_point + Vec3.UP + Vec3.RIGHT / 2, current_point + Vec3.UP * 2, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderPoint3D(current_point, 0.5f, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderTriangle3D(current_point, current_point + Vec3.DOWN / 2 + Vec3.LEFT / 2, current_point + Vec3.DOWN / 2 + Vec3.RIGHT / 2, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderQuad3D(current_point + Vec3.LEFT / 2, current_point + Vec3.LEFT / 2 + Vec3.UP, current_point + Vec3.UP + Vec3.RIGHT / 2, current_point + Vec3.RIGHT / 2, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderBillboard3D(current_point, 0.5f, vec4.ZERO, true);// try use setTextureName
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderVector(current_point + Vec3.DOWN / 2 + Vec3.LEFT / 2, current_point + Vec3.UP / 2 + Vec3.RIGHT / 2, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderDirection(current_point + Vec3.DOWN / 2 + Vec3.LEFT / 2, new vec3(1, 0, 1), vec4.RED, 0.25f, false);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderBox(vec3.ONE, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSolidBox(vec3.ONE, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		mat4 proj = MathLib.Perspective(60, 16 / 9, 0.1f, 1);
		Mat4 modelview = MathLib.LookAt(current_point + vec3.LEFT / 2, current_point, vec3.UP);
		//transformation matrix equals inversed modelview
		Visualizer.RenderFrustum(proj, MathLib.Inverse(modelview), vec4.RED);
		i++;


		//10-20
		current_point = GetPostamentPoint(i);
		Visualizer.RenderCircle(0.5f, new Mat4(quat.IDENTITY * new quat(90, 0, 0), current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSector(0.5f, 60, new Mat4(quat.IDENTITY * new quat(90, 0, 0), current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderCone(0.5f, 30, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSphere(0.5f, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSolidSphere(0.5f, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderCapsule(0.5f, 0.5f, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSolidCapsule(0.5f, 0.5f, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderCylinder(1, 1, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSolidCylinder(1, 1, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		vec3 center = new vec3(0, 0, 0);// check out documentation for center format information
		Visualizer.RenderMessage3D(current_point, center, "renderMessage3D exapmle", vec4.RED);
		i++;

		//20-30
		current_point = GetPostamentPoint(i);
		Visualizer.RenderEllipse(new vec3(0.5, 1, 1.5), new Mat4(quat.IDENTITY, current_point + Vec3.UP), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSolidEllipse(new vec3(0.5, 1, 1.5), new Mat4(quat.IDENTITY, current_point + Vec3.UP), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		BoundBox bb = new BoundBox(new vec3(-0.5f, -0.5f, -0.5f), new vec3(0.5f, 0.5f, 0.5f));
		Visualizer.RenderBoundBox(bb, new Mat4(quat.IDENTITY * new quat(90, 0, 0), current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		BoundSphere bs = new BoundSphere(vec3.ZERO, 0.5f);
		Visualizer.RenderBoundSphere(bs, new Mat4(quat.IDENTITY, current_point), vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderNodeBoundBox(node_boundBox_example, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderNodeBoundSphere(node_boundSphere_example, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderObject(object_example, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSolidObject(object_solid_example, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderObjectSurface(surface_example, 0, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderSolidObjectSurface(surface_solid_example, 0, vec4.RED);
		i++;


		//30-32
		current_point = GetPostamentPoint(i);
		Visualizer.RenderObjectSurfaceBoundBox(object_surface_boundBox_example, 0, vec4.RED);
		i++;

		current_point = GetPostamentPoint(i);
		Visualizer.RenderObjectSurfaceBoundSphere(object_surface_boundSphere_example, 0, vec4.RED);
		i++;
	}

	// Returns the world position of a pedestal node by index for placing primitives.
	Vec3 GetPostamentPoint(int index)
	{
		Vec3 result = Vec3.ONE;
		if (index < postament_nodes.Count)
		{
			Node node = postament_nodes[index];
			if (node)
			{
				result = node.WorldPosition;
			}
			else
			{
				Log.Message($"VisualizerUsage.GetNextPostamentPoint postamentNodes: Node with index: {index} is null \n");
			}
		}
		else
		{
			Log.Message("Visualizer usage doesn't have enough pedestal's display nodes to draw all visualizer examples\n");
		}
		return result;
	}
}
