// Simulates an expanding explosion sphere that fractures and pushes objects.
// Creates a radial shockwave that applies force to nearby objects and triggers
// body fracture cracking based on distance-attenuated force.

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

using System.Collections.Generic;
using Unigine;

// Expanding explosion that fractures and pushes nearby objects.
public partial class BodyFractureExplosion : Component
{
	// Maximum explosion radius
	public float MaxRadius = 10.0f;
	// Expansion speed of explosion sphere
	public float Speed = 100.0f;
	// Maximum force applied at center
	public float Force = 100.0f;

	// Current explosion radius
	private float radius = 0.0f;

	// Triggers explosion by resetting radius.
	public void Explode()
	{
		radius = 0.0f;
	}

	// Explosion starts inactive at max radius.
	void Init()
	{
		radius = MaxRadius;
	}

	// Explosion sphere expands, applying force to intersecting objects.
	void Update()
	{
		// Skip if explosion is inactive or completed
		if (MaxRadius == 0 || radius > MaxRadius)
			return;

		// Create bounding sphere at current explosion radius
		BoundSphere sphere = new(new vec3(node.WorldPosition), radius);
		// Force decreases linearly with distance from center
		var actualForce = Force * (1 - radius / MaxRadius);
		// Visualize explosion sphere as red wireframe
		Visualizer.RenderBoundSphere(sphere, mat4.IDENTITY, vec4.RED, 0.01f);

		// Find all objects intersecting the explosion sphere
		List<Object> objects = [];
		if (World.GetIntersection(new WorldBoundSphere(sphere), objects))
		{
			foreach (var obj in objects)
			{
				var body = obj.Body;
				if (body == null)
					continue;

				// Calculate direction from explosion center to object
				var dir = new vec3(obj.WorldPosition - node.WorldPosition);
				if (dir.Length2 != 0)
					dir.Normalize();

				// Trigger fracture cracking if object has BodyFractureUnit
				var fracture = GetComponent<BodyFractureUnit>(obj);
				fracture?.Crack(obj.WorldPosition, -dir, actualForce);

				// Apply outward impulse to rigid bodies
				var rigid = body as BodyRigid;
				rigid?.AddLinearImpulse(dir * actualForce);
			}
		}

		// Expand explosion radius over time
		radius += Speed * Game.IFps;
	}
}
