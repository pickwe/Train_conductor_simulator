// Creates an animated 3D volume texture with procedurally generated density fields.
// Multiple spherical fields move and bounce within a volume, their density values
// are computed per-voxel to create organic blob-like shapes that merge and separate.
// The resulting 3D texture is applied to a volume cloud material for rendering.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Animated 3D volume texture with bouncing spherical density fields.
public partial class ImagesSample : Component
{
	// Size of the simulation space
	private float size;
	// Movement speed of density fields
	private float velocity;
	// Base radius of field influence
	private float radius;

	// Number of spherical field sources
	private int num_fields;
	// Current position of each field
	private vec3[] positions;
	// Movement velocity of each field
	private vec3[] velocities;
	// Radius of influence for each field
	private float[] radiuses;

	// 3D image storing density values for volume rendering
	private Image image;
	// Volume cloud material using the 3D texture
	Material material;

	// Creates 3D image, initializes density fields, and sets up volume box.
	void Init()
	{
		// Create a 32x32x32 voxel grid to store density values
		image = new Image();
		image.Create3D(32, 32, 32, Image.FORMAT_RGBA8);

		image_init();

		// Create a volume box that will render the 3D texture as volumetric clouds
		ObjectVolumeBox obj = new ObjectVolumeBox(new vec3(20.0f));
		obj.SetMaterial(Materials.FindManualMaterial("Unigine::volume_cloud_base"), "*");
		obj.SetMaterialState("samples", 2, 0);
		obj.Transform = MathLib.Translate(new vec3(0.0f, 0.0f, 1.0f));
		// Inherit material to avoid affecting other objects using the same base material
		material = obj.GetMaterialInherit(0);
	}
	
	// Updates field positions and regenerates 3D density texture.
	void Update()
	{
		image_update();
		// Upload the updated 3D image to material's density texture
		material.SetTextureImage(material.FindTexture("density_3d"), image);
	}

	// Initializes field positions, velocities and radii with random values.
	private void image_init()
	{
		// Simulation space is a cube from (0,0,0) to (size,size,size)
		size = 2.0f;
		velocity = 1.0f;
		radius = 0.5f;

		// Create 16 spherical density fields that will move around
		num_fields = 16;
		positions = new vec3[num_fields];
		velocities = new vec3[num_fields];
		radiuses = new float[num_fields];

		// Randomize initial state for each field
		for (int i = 0; i < num_fields; i++)
		{
			positions[i] = MathLib.RandVec3(0.0f, size);
			velocities[i] = MathLib.RandVec3(-velocity, velocity);
			radiuses[i] = MathLib.RandFloat(radius / 2.0f, radius);
		}
	}

	// Moves fields, bounces off boundaries, and computes density for each voxel.
	private void image_update()
	{
		float ifps = Game.IFps;

		// Move fields and bounce off volume boundaries
		for (int i = 0; i < num_fields; i++)
		{
			vec3 p = positions[i] + velocities[i] * ifps;
			// Reverse velocity when hitting a wall
			if (p.x < 0.0f || p.x > size) velocities[i].x = -velocities[i].x;
			if (p.y < 0.0f || p.y > size) velocities[i].y = -velocities[i].y;
			if (p.z < 0.0f || p.z > size) velocities[i].z = -velocities[i].z;
			positions[i] += velocities[i] * ifps;
		}

		int width = image.Width;
		int height = image.Height;
		int depth = image.Depth;

		// Calculate voxel size in simulation space
		float iwidth = size / width;
		float iheight = size / height;
		float idepth = size / depth;
		vec3 position = new vec3(0.0f);


		Image.Pixel pixel = new Image.Pixel();

		// Iterate through each voxel in the 3D grid
		for (int z = 0; z < depth; z++)
		{
			position.z = z * idepth;
			for (int y = 0; y < height; y++)
			{
				position.y = y * iheight;
				for (int x = 0; x < width; x++)
				{
					position.x = x * iwidth;
					// Accumulate density contributions from all nearby fields
					float field = 0.0f;
					for (int i = 0; i < num_fields; i++)
					{
						// Distance2 returns squared distance for faster computation
						float distance = MathLib.Distance2(positions[i], position);
						// Add density if voxel is within field's radius (closer = denser)
						if (distance < radiuses[i])
							field += radiuses[i] - distance;

						// Convert density to grayscale pixel (0-255), clamped by Saturate
						pixel.i.x = pixel.i.y = pixel.i.z = pixel.i.w = (int)(MathLib.Saturate(field) * 255.0f);
						image.Set3D(x, y, z, pixel);
					}
				}
			}
		}
	}
}
