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
#include "UnigineMathLibCommon.h"

#include "UnigineMathLibVec3.h"
#include "UnigineMathLibQuat.h"
#include "UnigineMathLibMat4.h"

namespace Unigine
{
namespace Math
{

struct FloatTransform;
struct DoubleTransform;

#if defined(USE_DOUBLE) || defined(UNIGINE_DOUBLE)
	typedef Unigine::Math::FloatTransform Transform;
	typedef Unigine::Math::DoubleTransform WorldTransform;
#else
	typedef Unigine::Math::FloatTransform Transform;
	typedef Unigine::Math::FloatTransform WorldTransform;
#endif

//////////////////////////////////////////////////////////////////////////
// FloatTransform
//////////////////////////////////////////////////////////////////////////
struct alignas(16) FloatTransform
{
	vec3 position;
	quat rotation;
	vec3 scale{vec3_one};

	UNIGINE_INLINE FloatTransform() = default;

	UNIGINE_INLINE FloatTransform(const vec3 &position, const quat &rotation, const vec3 &scale)
		: position(position), rotation(rotation), scale(scale)
	{}

	UNIGINE_INLINE void setMat(const mat4 &mat)
	{
		decomposeTransform(mat, position, rotation, scale);
	}

	UNIGINE_INLINE void setMat(const dmat4 &mat)
	{
		decomposeTransform(mat4(mat), position, rotation, scale);
	}

	UNIGINE_INLINE mat4 &getMat(mat4 &result) const
	{
		return composeTransform(result, position, rotation, scale);
	}

	UNIGINE_INLINE mat4 &getIMat(mat4 &result) const
	{
		mat4 mat;
		composeTransform(mat, position, rotation, scale);
		return inverse4(result, mat);
	}

	UNIGINE_INLINE mat4 getMat() const
	{
		mat4 result;
		return composeTransform(result, position, rotation, scale);
	}

	UNIGINE_INLINE mat4 getIMat() const
	{
		mat4 result;
		return inverse4(composeTransform(result, position, rotation, scale));
	}
};

static_assert(std::is_trivially_copyable<FloatTransform>::value == true, "struct isn't trivially copyable");

const FloatTransform float_transform_identity;

UNIGINE_INLINE FloatTransform &inverse(FloatTransform &out_res, const FloatTransform &t)
{
	out_res.scale = vec3_one / t.scale;
	out_res.rotation = inverse(t.rotation);
	out_res.position = out_res.rotation * (out_res.scale * (-t.position));
	return out_res;
}

UNIGINE_INLINE FloatTransform inverse(const FloatTransform &t)
{
	FloatTransform res;
	inverse(res, t);
	return res;
}

UNIGINE_INLINE FloatTransform &mul(FloatTransform &out_res, const FloatTransform &t0, const FloatTransform &t1)
{
	out_res.scale = t0.scale * t1.scale;
	out_res.rotation = t0.rotation * t1.rotation;
	out_res.position = t0.position + t0.rotation * (t0.scale * t1.position);
	return out_res;
}

UNIGINE_INLINE FloatTransform mul(const FloatTransform &t0, const FloatTransform &t1)
{
	FloatTransform res;
	mul(res, t0, t1);
	return res;
}

UNIGINE_INLINE vec3 &mul(vec3 &out_res, const FloatTransform &t, const vec3 &v)
{
	out_res = t.position + t.rotation * (t.scale * v);
	return out_res;
}

UNIGINE_INLINE vec3 mul(const FloatTransform &t, const vec3 &v)
{
	vec3 res;
	mul(res, t, v);
	return res;
}

UNIGINE_INLINE dvec3 &mul(dvec3 &out_res, const FloatTransform &t, const dvec3 &v)
{
	out_res = dvec3(t.position) + t.rotation * (dvec3(t.scale) * v);
	return out_res;
}

UNIGINE_INLINE dvec3 mul(const FloatTransform &t, const dvec3 &v)
{
	dvec3 res;
	mul(res, t, v);
	return res;
}

UNIGINE_INLINE FloatTransform &blendTransform(FloatTransform &out_res, const FloatTransform &t0, const FloatTransform &t1, float k)
{
	out_res.position = lerp(t0.position, t1.position, k);
	out_res.rotation = slerp(t0.rotation, t1.rotation, k);
	out_res.scale = lerp(t0.scale, t1.scale, k);
	return out_res;
}

UNIGINE_INLINE FloatTransform blendTransform(const FloatTransform &t0, const FloatTransform &t1, float k)
{
	FloatTransform res;
	blendTransform(res, t0, t1, k);
	return res;
}

UNIGINE_INLINE FloatTransform operator*(const FloatTransform &a, const FloatTransform &b)
{
	FloatTransform ret;
	return mul(ret, a, b);
}

UNIGINE_INLINE vec3 operator*(const FloatTransform &t, const vec3 &v)
{
	vec3 ret;
	return mul(ret, t, v);
}

UNIGINE_INLINE dvec3 operator*(const FloatTransform &t, const dvec3 &v)
{
	dvec3 ret;
	return mul(ret, t, v);
}

//////////////////////////////////////////////////////////////////////////
// DoubleTransform
//////////////////////////////////////////////////////////////////////////
struct alignas(16) DoubleTransform
{
	dvec3 position;
	quat rotation;
	vec3 scale{vec3_one};

	UNIGINE_INLINE DoubleTransform() = default;

	UNIGINE_INLINE DoubleTransform(const dvec3 &position, const quat &rotation, const vec3 &scale)
		: position(position), rotation(rotation), scale(scale)
	{}

	UNIGINE_INLINE void setMat(const dmat4 &mat)
	{
		decomposeTransform(mat, position, rotation, scale);
	}

	UNIGINE_INLINE dmat4 &getMat(dmat4 &result) const
	{
		return composeTransform(result, position, rotation, scale);
	}

	UNIGINE_INLINE dmat4 &getIMat(dmat4 &result) const
	{
		dmat4 mat;
		composeTransform(mat, position, rotation, scale);
		return inverse4(result, mat);
	}

	UNIGINE_INLINE dmat4 getMat() const
	{
		dmat4 result;
		return composeTransform(result, position, rotation, scale);
	}

	UNIGINE_INLINE dmat4 getIMat() const
	{
		dmat4 result;
		return inverse4(composeTransform(result, position, rotation, scale));
	}
};

static_assert(std::is_trivially_copyable<DoubleTransform>::value == true, "struct isn't trivially copyable");

const DoubleTransform double_transform_identity;

UNIGINE_INLINE DoubleTransform &inverse(DoubleTransform &out_res, const DoubleTransform &t)
{
	out_res.scale = vec3_one / t.scale;
	out_res.rotation = inverse(t.rotation);
	out_res.position = out_res.rotation * (dvec3(out_res.scale) * (-t.position));
	return out_res;
}

UNIGINE_INLINE DoubleTransform inverse(const DoubleTransform &t)
{
	DoubleTransform res;
	inverse(res, t);
	return res;
}

UNIGINE_INLINE DoubleTransform &mul(DoubleTransform &out_res, const DoubleTransform &t0, const DoubleTransform &t1)
{
	out_res.scale = t0.scale * t1.scale;
	out_res.rotation = t0.rotation * t1.rotation;
	out_res.position = t0.position + t0.rotation * (dvec3(t0.scale) * t1.position);
	return out_res;
}

UNIGINE_INLINE DoubleTransform mul(const DoubleTransform &t0, const DoubleTransform &t1)
{
	DoubleTransform res;
	mul(res, t0, t1);
	return res;
}

UNIGINE_INLINE vec3 &mul(vec3 &out_res, const DoubleTransform &t, const vec3 &v)
{
	out_res = vec3(t.position) + t.rotation * (t.scale * v);
	return out_res;
}

UNIGINE_INLINE vec3 mul(const DoubleTransform &t, const vec3 &v)
{
	vec3 res;
	mul(res, t, v);
	return res;
}

UNIGINE_INLINE dvec3 &mul(dvec3 &out_res, const DoubleTransform &t, const dvec3 &v)
{
	out_res = t.position + t.rotation * (dvec3(t.scale) * v);
	return out_res;
}

UNIGINE_INLINE dvec3 mul(const DoubleTransform &t, const dvec3 &v)
{
	dvec3 res;
	mul(res, t, v);
	return res;
}

UNIGINE_INLINE DoubleTransform &blendTransform(DoubleTransform &out_res, const DoubleTransform &t0, const DoubleTransform &t1, float k)
{
	out_res.position = lerp(t0.position, t1.position, k);
	out_res.rotation = slerp(t0.rotation, t1.rotation, k);
	out_res.scale = lerp(t0.scale, t1.scale, k);
	return out_res;
}

UNIGINE_INLINE DoubleTransform blendTransform(const DoubleTransform &t0, const DoubleTransform &t1, float k)
{
	DoubleTransform res;
	blendTransform(res, t0, t1, k);
	return res;
}

UNIGINE_INLINE vec3 operator*(const DoubleTransform &t, const vec3 &v)
{
	vec3 ret;
	return mul(ret, t, v);
}

UNIGINE_INLINE dvec3 operator*(const DoubleTransform &t, const dvec3 &v)
{
	dvec3 ret;
	return mul(ret, t, v);
}

const Transform transform_identity;
const WorldTransform world_transform_identity;

} // namespace Math
} // namespace Unigine
