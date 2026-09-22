// Creates an interactive laser beam that detects surface hits with normal information.
// The laser ray node is scaled to match the hit distance, and a hit marker is placed
// at the intersection point aligned to the surface normal. Demonstrates using
// WorldIntersectionNormal to get detailed hit information for effects like laser
// pointers, aiming lines, or any visual ray that needs to respond to geometry.

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

// Laser beam that adjusts its length to hit surfaces and shows impact marker.
public partial class LaserRayIntersection : Component
{
	// Visual representation of the laser beam (scaled on Y axis)
	public Node laserRay = null;
	// Visual marker placed at the hit point
	public Node laserHit = null;
	// Maximum laser range before fading out
	public float laserDistance = 25.0f;

	// Intersection mask to filter which objects block the laser
	[ParameterMask(MaskType = ParameterMaskAttribute.TYPE.INTERSECTION)]
	public int mask = 1;

	// Receives detailed hit information including surface normal
	private WorldIntersectionNormal intersection = null;
	// Original laser scale (Y component is modified to match distance)
	private vec3 laserRayScale = vec3.ONE;
	// UI window showing hit status
	private SampleDescriptionWindow sampleDescriptionWindow;

	// Creates intersection result object and caches initial laser scale.
	private void Init()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow();

		if (!laserRay || !laserHit)
			return;

		// WorldIntersectionNormal gives us hit point and surface normal
		intersection = new WorldIntersectionNormal();

		// Save the source laser ray scale for changing length after intersection
		laserRayScale = laserRay.WorldScale;
	}

	// Casts ray along laser direction and updates visuals based on hit.
	private void Update()
	{
		// Check parts of laser
		if (!laserRay || !laserHit)
			return;

		// Ray cast along the laser's forward direction (Y axis)
		Vec3 firstPoint = laserRay.WorldPosition;
		Vec3 secondPoint = firstPoint + laserRay.GetWorldDirection(MathLib.AXIS.Y) * laserDistance;

		var status = "Hit Object:";
		// Try to get intersection object
		Unigine.Object hitObject = World.GetIntersection(firstPoint, secondPoint, mask, intersection);
		if (hitObject)
		{
			// Show object name
			status += $" {hitObject.Name}";

			// Scale laser beam to stop at hit point
			float length = (float)(intersection.Point - laserRay.WorldPosition).Length;
			laserRayScale.y = length;
			laserRay.WorldScale = laserRayScale;

			// Show and position hit marker at impact point
			if (!laserHit.Enabled)
				laserHit.Enabled = true;

			// Align marker to surface normal for proper decal-like effect
			laserHit.WorldPosition = intersection.Point;
			laserHit.SetWorldDirection(intersection.Normal, vec3.UP, MathLib.AXIS.Y);
		}
		else
		{
			status += " none";
			// No hit - extend laser to maximum length
			laserRayScale.y = laserDistance;
			laserRay.WorldScale = laserRayScale;

			// Hide hit marker when not hitting anything
			laserHit.Enabled = false;
		}
		sampleDescriptionWindow.setStatus(status);
	}

	// Cleans up UI on shutdown.
	private void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
	}
}
