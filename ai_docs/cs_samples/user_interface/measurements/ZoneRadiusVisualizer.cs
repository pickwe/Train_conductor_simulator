// Draws a flat zone ring plus a labeled radius line around this node's position
// every frame. Attach it to any node you want to mark with a zone and enable the
// Visualizer (the sample does that) to see it. Reusable on its own.

using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

public partial class ZoneRadiusVisualizer : Component
{
	// Radius of the zone circle drawn around this node.
	[ShowInEditor, Parameter(Title = "Zone radius")]
	public float zoneRadius = 3.0f;

	void Update()
	{
		if (zoneRadius <= 0.0f)
			return;

		// A white ring plus a labeled radius line, redrawn each frame at the node's
		// current position.
		Vec3 position = node.WorldPosition;
		DistanceVisualizer.RenderZoneCircle(position, zoneRadius);
		DistanceVisualizer.RenderZoneRadius(position, zoneRadius);
	}
}
