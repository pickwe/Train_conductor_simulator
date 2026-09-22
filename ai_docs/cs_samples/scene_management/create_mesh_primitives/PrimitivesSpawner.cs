// Demonstrates procedural primitive mesh creation. Creates box, sphere,
// cylinder, capsule, prism, and plane meshes using the Mesh API and
// instantiates them as dynamic mesh objects.

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

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Creates procedural primitive meshes at initialization.
public partial class PrimitivesSpawner : Component
{
	// All primitive types are created and positioned.
	private void Init()
	{
		CreateBox();
		CreateSphere();
		CreateCylinder();
		CreateCapsule();
		CreatePrism();
		CreatePlane();
	}

	// Creates a unit box mesh.
	private void CreateBox()
	{
		// Create mesh for ObjectMeshDynamic constructor
		Mesh boxMesh = new Mesh();

		// Add a box surface with given size to the mesh
		boxMesh.AddBoxSurface("box_surface", new vec3(1.0f, 1.0f, 1.0f));

		// Create an ObjectMeshDynamic node and set position
		ObjectMeshDynamic box = new ObjectMeshDynamic(boxMesh);
		box.WorldPosition = new Vec3(-5.0f, 0.0f, 1.5f);

		// Clear the mesh after node creation
		boxMesh.Clear();
	}

	// Creates a sphere mesh with radius 0.5.
	private void CreateSphere()
	{
		Mesh sphereMesh = new Mesh();
		sphereMesh.AddSphereSurface("sphere_surface", 0.5f, 16, 16);

		ObjectMeshDynamic sphere = new ObjectMeshDynamic(sphereMesh);
		sphere.WorldPosition = new Vec3(-3.0f, 0.0f, 1.5f);

		sphereMesh.Clear();
	}

	// Creates a cylinder mesh.
	private void CreateCylinder()
	{
		Mesh cylinderMesh = new Mesh();
		cylinderMesh.AddCylinderSurface("cylinder_surface", 0.5f, 1.0f, 16, 16);

		ObjectMeshDynamic cylinder = new ObjectMeshDynamic(cylinderMesh);
		cylinder.WorldPosition = new Vec3(-1.0f, 0.0f, 1.5f);

		cylinderMesh.Clear();
	}

	// Creates a capsule mesh.
	private void CreateCapsule()
	{
		Mesh capsuleMesh = new Mesh();
		capsuleMesh.AddCapsuleSurface("capsule_surface", 0.5f, 1.0f, 16, 16);

		ObjectMeshDynamic capsule = new ObjectMeshDynamic(capsuleMesh);
		capsule.WorldPosition = new Vec3(1.0f, 0.0f, 1.5f);

		capsuleMesh.Clear();
	}

	// Creates a pentagonal prism mesh.
	private void CreatePrism()
	{
		Mesh prismMesh = new Mesh();
		prismMesh.AddPrismSurface("prism_surface", 0.5f, 1.0f, 0.5f, 5);

		ObjectMeshDynamic prism = new ObjectMeshDynamic(prismMesh);
		prism.WorldPosition = new Vec3(3.0f, 0.0f, 1.5f);

		prismMesh.Clear();
	}

	// Creates a unit plane mesh.
	private void CreatePlane()
	{
		Mesh planeMesh = new Mesh();
		planeMesh.AddPlaneSurface("plane_surface", 1.0f, 1.0f, 1);

		ObjectMeshDynamic plane = new ObjectMeshDynamic(planeMesh);
		plane.WorldPosition = new Vec3(5.0f, 0.0f, 1.5f);

		planeMesh.Clear();
	}

}
