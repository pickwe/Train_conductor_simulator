// Draws a colored distance "ruler" from this node to each target node every
// frame: red when close, green at greenDistance and beyond. Attach it to any
// node, assign the targets and enable the Visualizer (the sample does that) to
// see the rulers. Reusable on its own, no sample code required.

using System.Collections.Generic;
using Unigine;

public partial class DistanceMeasurement : Component
{
	// Nodes whose distance to this node is measured and drawn.
	[ShowInEditor, Parameter(Title = "Targets")]
	public List<Node> targets = new List<Node>();

	// Distance (in units) at which a ruler becomes fully green.
	[ShowInEditor, Parameter(Title = "Green distance")]
	public float greenDistance = 25.0f;

	// This node plus its sub-tree: the "from" end of every ruler, also excluded
	// from the surface-clipping raycast. Collected once (hierarchy is static).
	private List<Node> ownHierarchy;

	void Init()
	{
		// The controlled node is the "from" end of every ruler; its hierarchy does
		// not change at runtime, so collect it once.
		ownHierarchy = DistanceVisualizer.CollectHierarchy(node);
	}

	void Update()
	{
		if (ownHierarchy == null || ownHierarchy.Count == 0)
			return;

		for (int i = 0; i < targets.Count; i++)
		{
			Node target = targets[i];
			// Targets can be deleted at runtime, so always check before use.
			if (!target)
				continue;

			// Let SetupDistanceVector resolve the endpoints to the object surfaces
			// so the arrows touch the meshes instead of their centers.
			DistanceVisualizer.DistanceVector vector = new DistanceVisualizer.DistanceVector();
			vector.from.nodes = ownHierarchy;
			vector.to.nodes = DistanceVisualizer.CollectHierarchy(target);
			DistanceVisualizer.SetupDistanceVector(vector);

			// Color the ruler red (near) to green (far) by the measured distance.
			float length = (float)MathLib.Distance(vector.from.point, vector.to.point);
			float t = greenDistance > 0.0f ? MathLib.Clamp(length / greenDistance, 0.0f, 1.0f) : 1.0f;
			vector.color = DistanceVisualizer.RedGreenGradient(t);
			vector.color.w = 0.6f;
			vector.labelColor = vector.color;
			vector.labelColor.w = 1.0f;

			DistanceVisualizer.RenderDistanceVector(vector);
		}
	}
}
