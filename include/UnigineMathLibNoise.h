/* Copyright (C) 2005-2026, UNIGINE. All rights reserved.
*
* This file is a part of the UNIGINE 2 SDK.
*
* Your use and / or redistribution of this software in source and / or
* binary form, with or without modification, is subject to: (i) your
* ongoing acceptance of and compliance with the terms and conditions of
* the UNIGINE License Agreement; and (ii) your inclusion of this notice
* in any version of this software that you use or redistribute.
* A copy of the UNIGINE License Agreement is available by contacting
* UNIGINE. at http://unigine.com/
*/
#pragma once

#include "UnigineBase.h"
#include "UnigineMathLibCommon.h"

namespace Unigine
{
namespace Math
{

class Noise
{
public:
	enum
	{
		A = 1664525,
		C = 1013904223,
		MAX_RANDOM = 0x7fffffff,
		SAMPLES = 256,
	};

	UNIGINE_INLINE static Noise &getNoise() { return noise; }

	UNIGINE_INLINE Noise() { setSeed((unsigned int)time(NULL)); }
	UNIGINE_INLINE Noise(unsigned int seed_) { setSeed(seed_); }

	/// <summary>Sets a new seed for the noise generator.</summary>
	UNIGINE_INLINE void setSeed(unsigned int seed_)
	{
		seed = seed_;
		if (seed < 1)
			seed = 1;

		// make random arrays
		for (int i = 0; i < SAMPLES; i++)
		{
			permutation[i] = i;
			gradient1[i][0] = get_random_float();
			gradient2[i][0] = get_random_float();
			gradient2[i][1] = get_random_float();
			gradient3[i][0] = get_random_float();
			gradient3[i][1] = get_random_float();
			gradient3[i][2] = get_random_float();
			normalize2(gradient2[i]);
			normalize3(gradient3[i]);
		}

		// permutation array
		for (int i = 0; i < SAMPLES; i++)
		{
			int j = (get_random_int() >> 16) % SAMPLES;
			int k = permutation[i];
			permutation[i] = permutation[j];
			permutation[j] = k;
		}

		// duplicate randoms
		for (int i = 0; i < SAMPLES + 2; i++)
		{
			permutation[SAMPLES + i] = permutation[i];
			gradient1[SAMPLES + i][0] = gradient1[i][0];
			gradient2[SAMPLES + i][0] = gradient2[i][0];
			gradient2[SAMPLES + i][1] = gradient2[i][1];
			gradient3[SAMPLES + i][0] = gradient3[i][0];
			gradient3[SAMPLES + i][1] = gradient3[i][1];
			gradient3[SAMPLES + i][2] = gradient3[i][2];
		}
	}
	/// <summary>Returns the current seed used by the noise generator.</summary>
	UNIGINE_INLINE unsigned int getSeed() const { return seed; }

	UNIGINE_INLINE float get1D(float x) const
	{
		int bx0, bx1;
		float rx0, rx1;

		setup(x, bx0, bx1, rx0, rx1);

		float sx = fade(rx0);

		float a = gradient1[permutation[bx0]][0] * rx0;
		float b = gradient1[permutation[bx1]][0] * rx1;

		return lerp(a, b, sx);
	}
	/// <summary>Returns the two-dimensional noise.</summary>
	/// <param name="x">Seed 1.</param>
	/// <param name="y">Seed 2.</param>
	UNIGINE_INLINE float get2D(float x, float y) const
	{
		int bx0, bx1, by0, by1;
		float rx0, rx1, ry0, ry1;

		setup(x, bx0, bx1, rx0, rx1);
		setup(y, by0, by1, ry0, ry1);

		float sx = fade(rx0);
		float sy = fade(ry0);

		int b00 = permutation[permutation[bx0] + by0];
		int b10 = permutation[permutation[bx1] + by0];
		int b01 = permutation[permutation[bx0] + by1];
		int b11 = permutation[permutation[bx1] + by1];

		float a = lerp(dot2(gradient2[b00], rx0, ry0), dot2(gradient2[b10], rx1, ry0), sx);
		float b = lerp(dot2(gradient2[b01], rx0, ry1), dot2(gradient2[b11], rx1, ry1), sx);

		return lerp(a, b, sy);
	}
	UNIGINE_INLINE float get2D(const vec2 &xy) const { return get2D(xy.x, xy.y); }

	/// <summary>Returns the three-dimensional (3D) white noise.</summary>
	/// <param name="x">Seed 1.</param>
	/// <param name="y">Seed 2.</param>
	/// <param name="z">Seed 3.</param>
	UNIGINE_INLINE float get3D(float x, float y, float z) const
	{
		int bx0, bx1, by0, by1, bz0, bz1;
		float rx0, rx1, ry0, ry1, rz0, rz1;

		setup(x, bx0, bx1, rx0, rx1);
		setup(y, by0, by1, ry0, ry1);
		setup(z, bz0, bz1, rz0, rz1);

		float sx = fade(rx0);
		float sy = fade(ry0);
		float sz = fade(rz0);

		int b00 = permutation[permutation[bx0] + by0];
		int b10 = permutation[permutation[bx1] + by0];
		int b01 = permutation[permutation[bx0] + by1];
		int b11 = permutation[permutation[bx1] + by1];

		float a0 = lerp(dot3(gradient3[b00 + bz0], rx0, ry0, rz0), dot3(gradient3[b10 + bz0], rx1, ry0, rz0), sx);
		float b0 = lerp(dot3(gradient3[b01 + bz0], rx0, ry1, rz0), dot3(gradient3[b11 + bz0], rx1, ry1, rz0), sx);
		float a1 = lerp(dot3(gradient3[b00 + bz1], rx0, ry0, rz1), dot3(gradient3[b10 + bz1], rx1, ry0, rz1), sx);
		float b1 = lerp(dot3(gradient3[b01 + bz1], rx0, ry1, rz1), dot3(gradient3[b11 + bz1], rx1, ry1, rz1), sx);

		float c0 = lerp(a0, b0, sy);
		float c1 = lerp(a1, b1, sy);

		return lerp(c0, c1, sz);
	}
	UNIGINE_INLINE float get3D(const vec3 &xyz) const { return get3D(xyz.x, xyz.y, xyz.z); }

	/// <summary>Returns the one-dimensional turbulence noise.</summary>
	/// <param name="x">Seed.</param>
	/// <param name="frequency">Frequency.</param>
	UNIGINE_INLINE float getTurbulence1D(float x, int frequency) const
	{
		float res = 0.0f;
		for (int f = frequency; f >= 1; f >>= 1)
			res += get1D(x * f) / f;
		return res;
	}
	/// <summary>Returns the two-dimensional turbulence noise.</summary>
	/// <param name="x">Seed 1.</param>
	/// <param name="y">Seed 2.</param>
	/// <param name="frequency">Frequency.</param>
	UNIGINE_INLINE float getTurbulence2D(float x, float y, int frequency) const
	{
		float res = 0.0f;
		for (int f = frequency; f >= 1; f >>= 1)
			res += get2D(x * f, y * f) / f;
		return res;
	}
	UNIGINE_INLINE float getTurbulence2D(const vec2 &xy, int frequency) const { return getTurbulence2D(xy.x, xy.y, frequency); }

	/// <summary>Returns the three-dimensional turbulence noise.</summary>
	/// <param name="x">Seed 1.</param>
	/// <param name="y">Seed 2.</param>
	/// <param name="z">Seed 3.</param>
	/// <param name="frequency">Frequency.</param
	UNIGINE_INLINE float getTurbulence3D(float x, float y, float z, int frequency) const
	{
		float res = 0.0f;
		for (int f = frequency; f >= 1; f >>= 1)
			res += get3D(x * f, y * f, z * f) / f;
		return res;
	}
	UNIGINE_INLINE float getTurbulence3D(const vec3 &xyz, int frequency) const { return getTurbulence3D(xyz.x, xyz.y, xyz.z, frequency); }

	/// <summary>Returns the one-dimensional tileable noise.</summary>
	/// <param name="x">Seed.</param>
	/// <param name="width">Tile width.</param>
	UNIGINE_INLINE float getTileable1D(float x, float width) const
	{
		x -= (int)(x / width) * width;
		if (x < 0.0f)
			x += width;
		return (get1D(x) * (width - x) + get1D(x - width) * x) / width;
	}

	/// <summary>Returns the two-dimensional tileable noise.</summary>
	/// <param name="x">Seed 1.</param>
	/// <param name="y">Seed 2.</param>
	/// <param name="width">Tile width.</param>
	/// <param name="height">Tile height.</param>
	UNIGINE_INLINE float getTileable2D(float x, float y, float width, float height) const
	{
		x -= (int)(x / width) * width;
		y -= (int)(y / height) * height;
		if (x < 0.0f)
			x += width;
		if (y < 0.0f)
			y += height;
		return (get2D(x, y) * (width - x) * (height - y) +
				get2D(x - width, y) * x * (height - y) +
				get2D(x, y - height) * (width - x) * y +
				get2D(x - width, y - height) * x * y) / (width * height);
	}
	UNIGINE_INLINE float getTileable2D(const vec2 &xy, float width, float height) const { return getTileable2D(xy.x, xy.y, width, height); }

	/// <summary>Returns the three-dimensional tileable noise.</summary>
	/// <param name="x">Seed 1.</param>
	/// <param name="y">Seed 2.</param>
	/// <param name="z">Seed 3.</param>
	/// <param name="width">Tile width.</param>
	/// <param name="height">Tile height.</param>
	/// <param name="depth">Tile depth.</param>
	UNIGINE_INLINE float getTileable3D(float x, float y, float z, float width, float height, float depth) const
	{
		x -= (int)(x / width) * width;
		y -= (int)(y / height) * height;
		z -= (int)(z / depth) * depth;
		if (x < 0.0f)
			x += width;
		if (y < 0.0f)
			y += height;
		if (z < 0.0f)
			z += depth;
		return (get3D(x, y, z) * (width - x) * (height - y) * (depth - z) +
				get3D(x - width, y, z) * x * (height - y) * (depth - z) +
				get3D(x, y - height, z) * (width - x) * y * (depth - z) +
				get3D(x - width, y - height, z) * x * y * (depth - z) +
				get3D(x, y, z - depth) * (width - x) * (height - y) * z +
				get3D(x - width, y, z - depth) * x * (height - y) * z +
				get3D(x, y - height, z - depth) * (width - x) * y * z +
				get3D(x - width, y - height, z - depth) * x * y * z) / (width * height * depth);
	}
	UNIGINE_INLINE float getTileable3D(const vec3 &xyz, float width, float height, float depth) const { return getTileable3D(xyz.x, xyz.y, xyz.z, width, height, depth); }

	/// <summary>Returns the one-dimensional tileable turbulence noise.</summary>
	/// <param name="x">Seed.</param>
	/// <param name="width">Tile width.</param>
	/// <param name="frequency">Frequency.</param>
	UNIGINE_INLINE float getTileableTurbulence1D(float x, float width, int frequency) const
	{
		float res = 0.0f;
		for (int f = frequency; f >= 1; f >>= 1)
			res += getTileable1D(x * f, width * f) / f;
		return res;
	}
	/// <summary>Returns the two-dimensional tileable turbulence noise.</summary>
	/// <param name="x">Seed 1.</param>
	/// <param name="y">Seed 2.</param>
	/// <param name="width">Tile width.</param>
	/// <param name="height">Tile height.</param>
	/// <param name="frequency">Frequency.</param>
	UNIGINE_INLINE float getTileableTurbulence2D(float x, float y, float width, float height, int frequency) const
	{
		float res = 0.0f;
		for (int f = frequency; f >= 1; f >>= 1)
			res += getTileable2D(x * f, y * f, width * f, height * f) / f;
		return res;
	}
	UNIGINE_INLINE float getTileableTurbulence2D(const vec2 &xy, float width, float height, int frequency) const { return getTileableTurbulence2D(xy.x, xy.y, width, height, frequency); }

	/// <summary>Returns the three-dimensional tileable turbulence noise.</summary>
	/// <param name="x">Seed 1.</param>
	/// <param name="y">Seed 2.</param>
	/// <param name="z">Seed 3.</param>
	/// <param name="width">Tile width.</param>
	/// <param name="height">Tile height.</param>
	/// <param name="depth">Tile depth.</param>
	/// <param name="frequency">Frequency.</param>
	UNIGINE_INLINE float getTileableTurbulence3D(float x, float y, float z, float width, float height, float depth, int frequency) const
	{
		float res = 0.0f;
		for (int f = frequency; f >= 1; f >>= 1)
			res += getTileable3D(x * f, y * f, z * f, width * f, height * f, depth * f) / f;
		return res;
	}
	UNIGINE_INLINE float getTileableTurbulence3D(const vec3 &xyz, float width, float height, float depth, int frequency) const { return getTileableTurbulence3D(xyz.x, xyz.y, xyz.z, width, height, depth, frequency); }

private:
	UNIGINE_INLINE static void setup(float x, int &b0, int &b1, float &r0, float &r1)
	{
		b0 = (int)(x + 4096.0f) & 255;
		b1 = (b0 + 1) & 255;
		r0 = (x + 4096.0f) - (int)(x + 4096.0f);
		r1 = r0 - 1.0f;
	}
	UNIGINE_INLINE static float fade(float x)
	{
		return x * x * x * (x * (x * 6.0f - 15.0f) + 10.0f);
	}

	UNIGINE_INLINE unsigned int get_random_int()
	{
		seed = (unsigned int)((unsigned long long)seed * A + C) & MAX_RANDOM;
		return seed;
	}
	UNIGINE_INLINE float get_random_float() { return (float)get_random_int() / (float)MAX_RANDOM * 2.0f - 1.0f; }

	static Noise noise;

	unsigned int seed{0};

	int permutation[SAMPLES * 2 + 2];
	float gradient1[SAMPLES * 2 + 2][1];
	float gradient2[SAMPLES * 2 + 2][2];
	float gradient3[SAMPLES * 2 + 2][3];
};

UNIGINE_INLINE float noise1D(float x) { return Noise::getNoise().get1D(x); }
UNIGINE_INLINE float noise2D(float x, float y) { return Noise::getNoise().get2D(x, y); }
UNIGINE_INLINE float noise2D(const vec2 &xy) { return Noise::getNoise().get2D(xy); }
UNIGINE_INLINE float noise3D(float x, float y, float z) { return Noise::getNoise().get3D(x, y, z); }
UNIGINE_INLINE float noise3D(const vec3 &xyz) { return Noise::getNoise().get3D(xyz); }

UNIGINE_INLINE float noiseTurbulence1D(float x, int frequency) { return Noise::getNoise().getTurbulence1D(x, frequency); }
UNIGINE_INLINE float noiseTurbulence2D(float x, float y, int frequency) { return Noise::getNoise().getTurbulence2D(x, y, frequency); }
UNIGINE_INLINE float noiseTurbulence2D(const vec2 &xy, int frequency) { return Noise::getNoise().getTurbulence2D(xy, frequency); }
UNIGINE_INLINE float noiseTurbulence3D(float x, float y, float z, int frequency) { return Noise::getNoise().getTurbulence3D(x, y, z, frequency); }
UNIGINE_INLINE float noiseTurbulence3D(const vec3 &xyz, int frequency) { return Noise::getNoise().getTurbulence3D(xyz, frequency); }

UNIGINE_INLINE float noiseTileable1D(float x, float width) { return Noise::getNoise().getTileable1D(x, width); }
UNIGINE_INLINE float noiseTileable2D(float x, float y, float width, float height) { return Noise::getNoise().getTileable2D(x, y, width, height); }
UNIGINE_INLINE float noiseTileable2D(const vec2 &xy, float width, float height) { return Noise::getNoise().getTileable2D(xy, width, height); }
UNIGINE_INLINE float noiseTileable3D(float x, float y, float z, float width, float height, float depth) { return Noise::getNoise().getTileable3D(x, y, z, width, height, depth); }
UNIGINE_INLINE float noiseTileable3D(const vec3 &xyz, float width, float height, float depth) { return Noise::getNoise().getTileable3D(xyz, width, height, depth); }

UNIGINE_INLINE float noiseTileableTurbulence1D(float x, float width, int frequency) { return Noise::getNoise().getTileableTurbulence1D(x, width, frequency); }
UNIGINE_INLINE float noiseTileableTurbulence2D(float x, float y, float width, float height, int frequency) { return Noise::getNoise().getTileableTurbulence2D(x, y, width, height, frequency); }
UNIGINE_INLINE float noiseTileableTurbulence2D(const vec2 &xy, float width, float height, int frequency) { return Noise::getNoise().getTileableTurbulence2D(xy, width, height, frequency); }
UNIGINE_INLINE float noiseTileableTurbulence3D(const vec3 &xyz, float width, float height, float depth, int frequency) { return Noise::getNoise().getTileableTurbulence3D(xyz, width, height, depth, frequency); }

} // namespace Math
} // namespace Unigine
