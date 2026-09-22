// Demonstrates 2D pathfinding with dynamic target following. The node
// moves along calculated paths toward a randomly positioned target,
// recalculating routes as needed to navigate around obstacles.

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

// Follows dynamic target using 2D pathfinding on navigation mesh.
public partial class PathRoute2DWithTarget : Component
{
	// Movement speed toward target
	public float speed = 4.0f;
	// Navigation mesh for pathfinding
	public NavigationMesh navigationMesh = null;

	// Path to target node reference file
	[ParameterFile(Filter = ".node")]
	public string targetReferencePath = "";

	// Whether to render the calculated path
	public bool visualizeRoute = false;

	// Color for route visualization
	[ParameterColor]
	public vec4 routeColor = vec4.ZERO;

	// PathRoute object for path calculation
	private PathRoute route = null;
	// Target node to follow
	private Node target = null;

	// Target is spawned at random position and route is initialized.
	private void Init()
	{
		if (!navigationMesh)
			return;

		// Create target
		target = World.LoadNode(targetReferencePath);
		if (target)
		{
			// Set random position in navigation mesh
			target.WorldPosition = new Vec3(Game.GetRandomFloat(-55.0f, 55.0f), Game.GetRandomFloat(-55.0f, 55.0f), 0.5f);

			// Keep target position inside navigation mesh
			while (!navigationMesh.Inside2D(target.WorldPosition, 0.5f))
				target.WorldPosition = new Vec3(Game.GetRandomFloat(-55.0f, 55.0f), Game.GetRandomFloat(-55.0f, 55.0f), 0.5f);

			// Create route for path calculation
			route = new PathRoute();

			// Set point radius inside navigation mesh
			route.Radius = 0.5f;

			// Clamp speed to non-negative value
			speed = MathLib.Max(0.0f, speed);

			// Enable visualizer for route rendering
			if (visualizeRoute)
				Visualizer.Enabled = true;
		}
	}
	
	// Node follows path to target, recalculating when target moves.
	private void Update()
	{
		if (!navigationMesh || !target)
			return;

		// Relocate target when reached
		if ((target.WorldPosition - node.WorldPosition).Length < 1.0f)
		{
			target.WorldPosition = new Vec3(Game.GetRandomFloat(-55.0f, 55.0f), Game.GetRandomFloat(-55.0f, 55.0f), 0.5f);

			// Keep target position inside navigation mesh
			while (!navigationMesh.Inside2D(target.WorldPosition, 0.5f))
				target.WorldPosition = new Vec3(Game.GetRandomFloat(-55.0f, 55.0f), Game.GetRandomFloat(-55.0f, 55.0f), 0.5f);
		}

		// If current path is ready, try to move node
		if (route.IsReady)
		{
			// On success, change direction and position
			if (route.IsReached)
			{
				// Enable target if it was hidden inside obstacle
				if (!target.Enabled)
					target.Enabled = true;

				// Get new direction for node
				vec3 direction = new vec3(route.GetPoint(1) - route.GetPoint(0));
				if (direction.Length2 > MathLib.EPSILON)
				{
					// Get rotation target based on new direction
					quat targetRotation = new quat(MathLib.SetTo(vec3.ZERO, direction.Normalized, vec3.UP, MathLib.AXIS.Y));

					// Smoothly interpolate rotation
					quat currentRotation = MathLib.Slerp(node.GetWorldRotation(), targetRotation, Game.IFps * 8.0f);
					node.SetWorldRotation(currentRotation);

					// Translate forward and validate position stays in navigation mesh
					Vec3 lastValidPosition = node.WorldPosition;
					node.Translate(Vec3.FORWARD * Game.IFps * speed);

					// Restore last position if node is outside navigation mesh
					if (!navigationMesh.Inside2D(node.WorldPosition, route.Radius))
						node.WorldPosition = lastValidPosition;
				}

				// Render current path
				if (visualizeRoute)
					route.RenderVisualizer(routeColor);

				// Try to create new path
				route.Create2D(node.WorldPosition + vec3.UP * 0.5f, target.WorldPosition, 1);
			}
			else
			{
				// Hide target and relocate, as it may be inside obstacle
				target.Enabled = false;
				target.WorldPosition = new Vec3(Game.GetRandomFloat(-55.0f, 55.0f), Game.GetRandomFloat(-55.0f, 55.0f), 0.5f);

				// Keep target position inside navigation mesh
				while (!navigationMesh.Inside2D(target.WorldPosition, 0.5f))
					target.WorldPosition = new Vec3(Game.GetRandomFloat(-55.0f, 55.0f), Game.GetRandomFloat(-55.0f, 55.0f), 0.5f);

				// Try to create new path
				route.Create2D(node.WorldPosition + vec3.UP * 0.5f, target.WorldPosition, 1);
			}
		}
		// Try to create new path
		else if (!route.IsQueued)
			route.Create2D(node.WorldPosition + vec3.UP * 0.5f, target.WorldPosition, 1);
	}

	// Visualizer is disabled on shutdown.
	private void Shutdown()
	{
		Visualizer.Enabled = false;
	}
}
