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

#include "UnigineMathLibVec2.h"
#include "UnigineMathLibVec3.h"
#include "UnigineMathLibVec4.h"

#include "UnigineMathLibDVec2.h"
#include "UnigineMathLibDVec3.h"
#include "UnigineMathLibDVec4.h"

#include "UnigineMathLibHVec2.h"
#include "UnigineMathLibHVec3.h"
#include "UnigineMathLibHVec4.h"

#include "UnigineMathLibIVec2.h"
#include "UnigineMathLibIVec3.h"
#include "UnigineMathLibIVec4.h"

#include "UnigineMathLibSVec4.h"
#include "UnigineMathLibBVec4.h"

#include "UnigineMathLibMat2.h"
#include "UnigineMathLibMat3.h"
#include "UnigineMathLibMat4.h"

#include "UnigineMathLibDMat4.h"

#include "UnigineMathLibQuat.h"

#include "UnigineMathLibSimd.h"
#include "UnigineMathLibGeometry.h"
#include "UnigineMathLibNumerical.h"
#include "UnigineMathLibRandom.h"
#include "UnigineMathLibNoise.h"
#include "UnigineMathLibSHBasis.h"
#include "UnigineMathLibColorSpaces.h"

namespace Unigine
{
namespace Math
{

static_assert(sizeof(vec2) == 2 * sizeof(float), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<vec2>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(vec3) == 4 * sizeof(float), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<vec3>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(vec4) == 4 * sizeof(float), "struct size must remain unchanged");
static_assert(IsTriviallyRelocatable<vec4>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(dvec2) == 2 * sizeof(double), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<dvec2>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(dvec3) == 4 * sizeof(double), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<dvec3>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(dvec4) == 4 * sizeof(double), "struct size must remain unchanged");
static_assert(IsTriviallyRelocatable<dvec4>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(hvec2) == 2 * sizeof(half), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<hvec2>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(hvec3) == 4 * sizeof(half), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<hvec3>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(hvec4) == 4 * sizeof(half), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<hvec4>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(ivec2) == 2 * sizeof(int), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<ivec2>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(ivec3) == 4 * sizeof(int), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<ivec3>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(ivec4) == 4 * sizeof(int), "struct size must remain unchanged");
static_assert(IsTriviallyRelocatable<ivec4>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(bvec4) == 4 * sizeof(unsigned char), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<bvec4>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(svec4) == 4 * sizeof(unsigned short), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<svec4>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(mat2) == 4 * sizeof(float), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<mat2>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(mat3) == 12 * sizeof(float), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<mat3>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(mat4) == 16 * sizeof(float), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<mat4>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(dmat4) == 12 * sizeof(double), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<dmat4>::value == true, "struct isn't trivially copyable");
static_assert(sizeof(quat) == 4 * sizeof(float), "struct size must remain unchanged");
static_assert(std::is_trivially_copyable<quat>::value == true, "struct isn't trivially copyable");

//////////////////////////////////////////////////////////////////////////
// vec2
//////////////////////////////////////////////////////////////////////////
/// <summary>Constructor. Initializes the vector using a given three-component vec3 source vector: x=v.x, y=v.y.</summary>
UNIGINE_INLINE vec2::vec2(const vec3 &v)
	: x(v.x)
	, y(v.y)
{
	UNIGINE_ASSERT_ALIGNED8(this);
}
/// <summary>Constructor. Initializes the vector using a given four-component vec4 source vector: x=v.x, y=v.y.</summary>
UNIGINE_INLINE vec2::vec2(const vec4 &v)
	: x(v.x)
	, y(v.y)
{
	UNIGINE_ASSERT_ALIGNED8(this);
}
/// <summary>Constructor. Initializes the vector using a given dvec2 source vector.</summary>
UNIGINE_INLINE vec2::vec2(const dvec2 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
{
	UNIGINE_ASSERT_ALIGNED8(this);
}
/// <summary>Constructor. Initializes the vector using a given ivec2 source vector.</summary>
UNIGINE_INLINE vec2::vec2(const ivec2 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
{
	UNIGINE_ASSERT_ALIGNED8(this);
}
/// <summary>Constructor. Initializes the vector using a given hvec2 source vector.</summary>
UNIGINE_INLINE vec2::vec2(const hvec2 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
{
	UNIGINE_ASSERT_ALIGNED8(this);
}

/// <summary>Performs componentwise division of vectors.</summary>
UNIGINE_INLINE vec2 operator/(const vec2 &v0, const ivec2 &v1) { return vec2(v0.x / v1.x, v0.y / v1.y); }
/// <summary>Returns the result of division of the vector by the value of the specified arguments.</summary>
UNIGINE_INLINE vec2 &vec2::operator/=(const ivec2 &val) { x /= val.x; y /= val.y; return *this; }

/// <summary>Returns a vector containing the largest integral values each being less than or equal to the corresponding vector component.</summary>
UNIGINE_INLINE ivec2 vec2::floor() const
{
	return ivec2(toInt(Math::floor(x)), toInt(Math::floor(y)));
}
/// <summary>Returns a vector containing the smallest integral values each being greater than or equal to the corresponding vector component.</summary>
UNIGINE_INLINE ivec2 vec2::ceil() const
{
	return ivec2(toInt(Math::ceil(x)), toInt(Math::ceil(y)));
}

//////////////////////////////////////////////////////////////////////////
// vec3
//////////////////////////////////////////////////////////////////////////
/// <summary>Constructor. Initializes the vector using a given four-component vec4 source vector: x=v.x, y=v.y, z=v.z.</summary>
UNIGINE_INLINE vec3::vec3(const vec4 &v)
{
	UNIGINE_ASSERT_ALIGNED16(this);
	#ifdef USE_SSE
		vec = v.vec;
	#else
		x = v.x;
		y = v.y;
		z = v.z;
		w = 0.0f;
	#endif
}
/// <summary>Constructor. Initializes the vector using a given hvec3 source vector.</summary>
UNIGINE_INLINE vec3::vec3(const hvec3 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
	, z(toFloat(v.z))
	, align(0.0f)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given dvec3 source vector.</summary>
UNIGINE_INLINE vec3::vec3(const dvec3 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
	, z(toFloat(v.z))
	, align(0.0f)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}

#ifdef USE_SSE
	UNIGINE_INLINE vec3::vec3(const ivec3 &v)
	{
		UNIGINE_ASSERT_ALIGNED16(this);
		vec = _mm_cvtepi32_ps(v.vec);
	}
#else
	UNIGINE_INLINE vec3::vec3(const ivec3 &v)
		: x(toFloat(v.x))
		, y(toFloat(v.y))
		, z(toFloat(v.z))
		, align(0.0f)
	{
	}
#endif

/// <summary>Sets the vector using a vec4 source vector: x=v.x, y=v.y, z=v.z.</summary>
UNIGINE_INLINE void vec3::set(const vec4 &v_)
{
	#ifdef USE_SSE
		vec = v_.vec;
	#else
		x = v_.x;
		y = v_.y;
		z = v_.z;
		w = 0.0f;
	#endif
}

//////////////////////////////////////////////////////////////////////////
// vec4
//////////////////////////////////////////////////////////////////////////
/// <summary>Constructor. Initializes the vector using a given dvec4 source vector.</summary>
UNIGINE_INLINE vec4::vec4(const dvec4 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
	, z(toFloat(v.z))
	, w(toFloat(v.w))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using given two dvec2 source vectors.</summary>
UNIGINE_INLINE vec4::vec4(const dvec2 &v0, const dvec2 & v1)
	: x(toFloat(v0.x))
	, y(toFloat(v0.y))
	, z(toFloat(v1.x))
	, w(toFloat(v1.y))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given hvec4 source vector.</summary>
UNIGINE_INLINE vec4::vec4(const hvec4 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
	, z(toFloat(v.z))
	, w(toFloat(v.w))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given svec4 source vector.</summary>
UNIGINE_INLINE vec4::vec4(const svec4 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
	, z(toFloat(v.z))
	, w(toFloat(v.w))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given bvec4 source vector.</summary>
UNIGINE_INLINE vec4::vec4(const bvec4 &v)
	: x(toFloat(v.x))
	, y(toFloat(v.y))
	, z(toFloat(v.z))
	, w(toFloat(v.w))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}

#ifdef USE_SSE
	UNIGINE_INLINE vec4::vec4(const ivec4 &v)
	{
		UNIGINE_ASSERT_ALIGNED16(this);
		vec = _mm_cvtepi32_ps(v.vec);
	}
#else
	UNIGINE_INLINE vec4::vec4(const ivec4 &v)
		: x(toFloat(v.x))
		, y(toFloat(v.y))
		, z(toFloat(v.z))
		, w(toFloat(v.w))
	{}
#endif

/// <summary>Sets the vector using a quaternion.</summary>
UNIGINE_INLINE void vec4::set(const quat &q)
{
	#ifdef USE_SSE
		vec = q.vec;
	#else
		x = q.x;
		y = q.y;
		z = q.z;
		w = q.w;
	#endif
}

/// <summary>Converts a color string in the Web format (RRGGBB / #RRGGBB or RRGGBBAA / #RRGGBBAA) into its vec4 equivalent.</summary>
UNIGINE_INLINE vec4 vec4::parseColor(const char *str)
{
	int length = (int)strlen(str);
	ivec4 c = ivec4(255);
	if (length == 6 && String::sscanf(str, "%02x%02x%02x", &c.x, &c.y, &c.z) == 3)
		return vec4(c) / 255.0f;
	else if (length == 7 && String::sscanf(str, "#%02x%02x%02x", &c.x, &c.y, &c.z) == 3)
		return vec4(c) / 255.0f;
	else if (length == 8 && String::sscanf(str, "%02x%02x%02x%02x", &c.x, &c.y, &c.z, &c.w) == 4)
		return vec4(c) / 255.0f;
	else if (length == 9 && String::sscanf(str, "#%02x%02x%02x%02x", &c.x, &c.y, &c.z, &c.w) == 4)
		return vec4(c) / 255.0f;
	else
		Log::error("vec4::parseColor(): bad color format \"%s\"\n", str);
	return vec4(1.0f, 0.0f, 0.0f, 1.0f);
}

//////////////////////////////////////////////////////////////////////////
// dvec2
//////////////////////////////////////////////////////////////////////////
/// <summary>Constructor. Initializes the vector using a given three-component vec3 vector: x=v.x, y=v.y.</summary>
UNIGINE_INLINE dvec2::dvec2(const dvec3 &v)
	: x(v.x)
	, y(v.y)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given four-component vec4 source vector: x=v.x, y=v.y.</summary>
UNIGINE_INLINE dvec2::dvec2(const dvec4 &v)
	: x(v.x)
	, y(v.y)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given ivec2 source vector.</summary>
UNIGINE_INLINE dvec2::dvec2(const ivec2 &v)
	: x(toDouble(v.x))
	, y(toDouble(v.y))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given hvec2 source vector.</summary>
UNIGINE_INLINE dvec2::dvec2(const hvec2 &v)
	: x(toDouble(v.x))
	, y(toDouble(v.y))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}

/// <summary>Returns a vector containing the largest integral values each being less than or equal to the corresponding vector component.</summary>
UNIGINE_INLINE ivec2 dvec2::floor() const
{
	return ivec2(toInt(Math::floor(x)), toInt(Math::floor(y)));
}
/// <summary>Returns a vector containing the smallest integral values each being greater than or equal to the corresponding vector component.</summary>
UNIGINE_INLINE ivec2 dvec2::ceil() const
{
	return ivec2(toInt(Math::ceil(x)), toInt(Math::ceil(y)));
}

//////////////////////////////////////////////////////////////////////////
// dvec3
//////////////////////////////////////////////////////////////////////////
/// <summary>Constructor. Initializes the vector using a given four-component dvec4 source vector: x=v.x, y=v.y, z=v.z.</summary>
UNIGINE_INLINE dvec3::dvec3(const dvec4 &v): x(v.x), y(v.y), z(v.z), align(0) {}
UNIGINE_INLINE dvec3::dvec3(const hvec3 &v)
	: x(toDouble(v.x))
	, y(toDouble(v.y))
	, z(toDouble(v.z))
	, align(0)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given ivec3 source vector.</summary>
UNIGINE_INLINE dvec3::dvec3(const ivec3 &v)
	: x(toDouble(v.x))
	, y(toDouble(v.y))
	, z(toDouble(v.z))
	, align(0)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}

//////////////////////////////////////////////////////////////////////////
// dvec4
//////////////////////////////////////////////////////////////////////////
/// <summary>Constructor. Initializes the vector using a given hvec4 source vector.</summary>
UNIGINE_INLINE dvec4::dvec4(const hvec4 &v)
	: x(toDouble(v.x))
	, y(toDouble(v.y))
	, z(toDouble(v.z))
	, w(toDouble(v.w))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given ivec4 source vector.</summary>
UNIGINE_INLINE dvec4::dvec4(const ivec4 &v)
	: x(toDouble(v.x))
	, y(toDouble(v.y))
	, z(toDouble(v.z))
	, w(toDouble(v.w))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given svec4 source vector.</summary>
UNIGINE_INLINE dvec4::dvec4(const svec4 &v)
	: x(toDouble(v.x))
	, y(toDouble(v.y))
	, z(toDouble(v.z))
	, w(toDouble(v.w))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given bvec4 source vector.</summary>
UNIGINE_INLINE dvec4::dvec4(const bvec4 &v)
	: x(toDouble(v.x))
	, y(toDouble(v.y))
	, z(toDouble(v.z))
	, w(toDouble(v.w))
{
	UNIGINE_ASSERT_ALIGNED16(this);
}

/// <summary>Converts a color string in the Web format (RRGGBB / #RRGGBB or RRGGBBAA / #RRGGBBAA) into its vec4 equivalent.</summary>
UNIGINE_INLINE dvec4 dvec4::parseColor(const char *str)
{
	int length = (int)strlen(str);
	ivec4 c = ivec4(255);
	if (length == 6 && String::sscanf(str, "%02x%02x%02x", &c.x, &c.y, &c.z) == 3)
		return dvec4(c) / 255.0;
	else if (length == 7 && String::sscanf(str, "#%02x%02x%02x", &c.x, &c.y, &c.z) == 3)
		return dvec4(c) / 255.0;
	else if (length == 8 && String::sscanf(str, "%02x%02x%02x%02x", &c.x, &c.y, &c.z, &c.w) == 4)
		return dvec4(c) / 255.0;
	else if (length == 9 && String::sscanf(str, "#%02x%02x%02x%02x", &c.x, &c.y, &c.z, &c.w) == 4)
		return dvec4(c) / 255.0;
	else
		Log::error("dvec4::parseColor(): bad color format \"%s\"\n", str);
	return dvec4(1.0, 0.0, 0.0, 1.0);
}

//////////////////////////////////////////////////////////////////////////
// ivec3
//////////////////////////////////////////////////////////////////////////
/// <summary>Constructor. Initializes the vector using a given ivec4 source vector.</summary>
UNIGINE_INLINE ivec3::ivec3(const ivec4 &v)
	: x(v.x)
	, y(v.y)
	, z(v.z)
	, align(0)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}

//////////////////////////////////////////////////////////////////////////
// ivec4
//////////////////////////////////////////////////////////////////////////
/// <summary>Constructor. Initializes the vector using a given svec4 source vector.</summary>
UNIGINE_INLINE ivec4::ivec4(const svec4 &v)
	: x(v.x)
	, y(v.y)
	, z(v.z)
	, w(v.w)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}
/// <summary>Constructor. Initializes the vector using a given bvec4 source vector.</summary>
UNIGINE_INLINE ivec4::ivec4(const bvec4 & v)
	: x(v.x)
	, y(v.y)
	, z(v.z)
	, w(v.w)
{
	UNIGINE_ASSERT_ALIGNED16(this);
}

//////////////////////////////////////////////////////////////////////////
// mat2
//////////////////////////////////////////////////////////////////////////
/// <summary>Sets the matrix using a given mat3 source matrix (3x3). Values m00, m01, m10, m11 from the source matrix are used.</summary>
UNIGINE_INLINE void mat2::set(const mat3 &m)
{
	m00 = m.m00;
	m01 = m.m01;
	m10 = m.m10;
	m11 = m.m11;
}
/// <summary>Sets the matrix using a given mat4 source matrix (4x4). Values m00, m01, m10, m11 from the source matrix are used.</summary>
UNIGINE_INLINE void mat2::set(const mat4 &m)
{
	m00 = m.m00;
	m01 = m.m01;
	m10 = m.m10;
	m11 = m.m11;
}
/// <summary>Sets the matrix using a given dmat4 source matrix (3x4). Values m00, m01, m10, m11 from the source matrix are used.</summary>
UNIGINE_INLINE void mat2::set(const dmat4 &m)
{
	m00 = toFloat(m.m00);
	m01 = toFloat(m.m01);
	m10 = toFloat(m.m10);
	m11 = toFloat(m.m11);
}

//////////////////////////////////////////////////////////////////////////
// mat3
//////////////////////////////////////////////////////////////////////////
/// <summary>Sets the matrix using a given mat4 source matrix (4x4). The matrix elements are set equal to corresponding elements of the source matrix.</summary>
UNIGINE_INLINE void mat3::set(const mat4 &m)
{
	#ifdef USE_SSE
		sse.v0 = m.sse.v0;
		sse.v1 = m.sse.v1;
		sse.v2 = m.sse.v2;
	#else
		m00 = m.m00;
		m01 = m.m01;
		m02 = m.m02;
		m10 = m.m10;
		m11 = m.m11;
		m12 = m.m12;
		m20 = m.m20;
		m21 = m.m21;
		m22 = m.m22;
	#endif
}
/// <summary>Sets the matrix using a given dmat4 source matrix (3x4). The matrix elements are set equal to corresponding elements of the source matrix.</summary>
UNIGINE_INLINE void mat3::set(const dmat4 &m)
{
	m00 = toFloat(m.m00);
	m01 = toFloat(m.m01);
	m02 = toFloat(m.m02);
	m10 = toFloat(m.m10);
	m11 = toFloat(m.m11);
	m12 = toFloat(m.m12);
	m20 = toFloat(m.m20);
	m21 = toFloat(m.m21);
	m22 = toFloat(m.m22);
}
/// <summary>Sets the matrix using a given source quaternion.</summary>
UNIGINE_INLINE void mat3::set(const quat &q)
{
	*this = q.getMat3();
}
/// <summary>Returns the quaternion of the matrix values.</summary>
UNIGINE_INLINE quat mat3::getQuat() const
{
	quat ret;
	float trace = m00 + m11 + m22;
	if (trace > 0.0f)
	{
		float s = sqrt(trace + 1.0f);
		ret.w = s * 0.5f;
		s = rcp(s) * 0.5f;
		ret.x = (m21 - m12) * s;
		ret.y = (m02 - m20) * s;
		ret.z = (m10 - m01) * s;
	} else if (m00 > m11 && m00 > m22)
	{
		float s = sqrt(m00 - m11 - m22 + 1.0f);
		ret.x = s * 0.5f;
		s = rcp(s) * 0.5f;
		ret.y = (m10 + m01) * s;
		ret.z = (m02 + m20) * s;
		ret.w = (m21 - m12) * s;
	} else if (m11 > m22)
	{
		float s = sqrt(m11 - m00 - m22 + 1.0f);
		ret.y = s * 0.5f;
		s = rcp(s) * 0.5f;
		ret.z = (m21 + m12) * s;
		ret.x = (m10 + m01) * s;
		ret.w = (m02 - m20) * s;
	} else
	{
		float s = sqrt(m22 - m00 - m11 + 1.0f);
		ret.z = s * 0.5f;
		s = rcp(s) * 0.5f;
		ret.x = (m02 + m20) * s;
		ret.y = (m21 + m12) * s;
		ret.w = (m10 - m01) * s;
	}
	return ret;
}

//////////////////////////////////////////////////////////////////////////
// mat4
//////////////////////////////////////////////////////////////////////////
/// <summary>Sets new matrix values using a given dmat4 source matrix (3x4). The matrix elements are filled using corresponding elements of the source matrix.</summary>
UNIGINE_INLINE void mat4::set(const dmat4 &m)
{
	m00 = toFloat(m.m00);
	m01 = toFloat(m.m01);
	m02 = toFloat(m.m02);
	m03 = toFloat(m.m03);
	m10 = toFloat(m.m10);
	m11 = toFloat(m.m11);
	m12 = toFloat(m.m12);
	m13 = toFloat(m.m13);
	m20 = toFloat(m.m20);
	m21 = toFloat(m.m21);
	m22 = toFloat(m.m22);
	m23 = toFloat(m.m23);
	m30 = 0.0f;
	m31 = 0.0f;
	m32 = 0.0f;
	m33 = 1.0f;
}
/// <summary>Sets new matrix values using a given source quaternion.</summary>
UNIGINE_INLINE void mat4::set(const quat &q)
{
	mat3 m = q.getMat3();
	m00 = m.m00;
	m01 = m.m01;
	m02 = m.m02;
	m03 = 0.0f;
	m10 = m.m10;
	m11 = m.m11;
	m12 = m.m12;
	m13 = 0.0f;
	m20 = m.m20;
	m21 = m.m21;
	m22 = m.m22;
	m23 = 0.0f;
	m30 = 0.0f;
	m31 = 0.0f;
	m32 = 0.0f;
	m33 = 1.0f;
}
/// <summary>Sets new matrix values using a given quaternion for rotation and scale values and a vec3 vector for translation values.</summary>
UNIGINE_INLINE void mat4::set(const quat &q, const vec3 &v)
{
	mat3 m = q.getMat3();
	m00 = m.m00;
	m01 = m.m01;
	m02 = m.m02;
	m03 = v.x;
	m10 = m.m10;
	m11 = m.m11;
	m12 = m.m12;
	m13 = v.y;
	m20 = m.m20;
	m21 = m.m21;
	m22 = m.m22;
	m23 = v.z;
	m30 = 0.0f;
	m31 = 0.0f;
	m32 = 0.0f;
	m33 = 1.0f;
}
/// <summary>Returns the quaternion, representing the rotation part of the matrix.</summary>
UNIGINE_INLINE quat mat4::getRotate() const
{
	mat3 rotate;
	orthonormalize(rotate, mat3(*this));
	return rotate.getQuat();
}

/// <summary>Composes a rotation matrix from the corresponding Euler angles. The Euler angles are specified in the axis rotation sequence - XYZ (in UNIGINE: x - pitch, y - roll, z - yaw). It is an order of the rings in the three-axis gimbal set: X axis used as the outer ring (independent ring), while Z axis as the inner one (its rotation depends on other 2 rings).</summary>
UNIGINE_INLINE mat4 composeRotationXYZ(const vec3 &r)
{
	return mat4(quat(r.x, r.y, r.z));
}
/// <summary>Composes a rotation matrix from the corresponding Euler angles. The Euler angles are specified in the axis rotation sequence - XZY (in UNIGINE: x - pitch, z - yaw, y - roll). It is an order of the rings in the three-axis gimbal set: X axis used as the outer ring (independent ring), while Y axis as the inner one (its rotation depends on other 2 rings).</summary>
UNIGINE_INLINE mat4 composeRotationXZY(const vec3 &r)
{
	return mat4(quat(1.0f, 0.0f, 0.0f, r.x) * quat(0.0f, 0.0f, 1.0f, r.z) * quat(0.0f, 1.0f, 0.0f, r.y));
}
/// <summary>Composes a rotation matrix from the corresponding Euler angles. The Euler angles are specified in the axis rotation sequence - YXZ (in UNIGINE: y - roll, x - pitch, z - yaw). It is an order of the rings in the three-axis gimbal set: Y axis used as the outer ring (independent ring), while Z axis as the inner one (its rotation depends on other 2 rings).</summary>
UNIGINE_INLINE mat4 composeRotationYXZ(const vec3 &r)
{
	return mat4(quat(0.0f, 1.0f, 0.0f, r.y) * (quat(1.0f, 0.0f, 0.0f, r.x) * quat(0.0f, 0.0f, 1.0f, r.z)));
}
/// <summary>Composes a rotation matrix from the corresponding Euler angles. The Euler angles are specified in the axis rotation sequence - YZX (in UNIGINE: y - roll, z - yaw, x - pitch). It is an order of the rings in the three-axis gimbal set: Y axis used as the outer ring (independent ring), while X axis as the inner one (its rotation depends on other 2 rings).</summary>
UNIGINE_INLINE mat4 composeRotationYZX(const vec3 &r)
{
	return mat4(quat(0.0f, 1.0f, 0.0f, r.y) * (quat(0.0f, 0.0f, 1.0f, r.z) * quat(1.0f, 0.0f, 0.0f, r.x)));
}
/// <summary>Composes a rotation matrix from the corresponding Euler angles. The Euler angles are specified in the axis rotation sequence - ZXY (in UNIGINE: z - yaw, x - pitch, y - roll). It is an order of the rings in the three-axis gimbal set: Z axis used as the outer ring (independent ring), while Y axis as the inner one (its rotation depends on other 2 rings).</summary>
UNIGINE_INLINE mat4 composeRotationZXY(const vec3 &r)
{
	return mat4(quat(0.0f, 0.0f, 1.0f, r.z) * (quat(1.0f, 0.0f, 0.0f, r.x) * quat(0.0f, 1.0f, 0.0f, r.y)));
}
/// <summary>Composes a rotation matrix from the corresponding Euler angles. The Euler angles are specified in the axis rotation sequence - ZYX (in UNIGINE: z - yaw, y - roll, x - pitch). It is an order of the rings in the three-axis gimbal set: Z axis used as the outer ring (independent ring), while X axis as the inner one (its rotation depends on other 2 rings). </summary>
UNIGINE_INLINE mat4 composeRotationZYX(const vec3 &r)
{
	return mat4(quat(0.0f, 0.0f, 1.0f, r.z) * (quat(0.0f, 1.0f, 0.0f, r.y) * quat(1.0f, 0.0f, 0.0f, r.x)));
}

/// <summary>Decomposes a given transformation matrix into a dual quaternion (representing both translation and rotation) and a scale vector. The dual-quaternion model is an accurate, computationally efficient, robust, and flexible method of representing rigid transforms and it is used in skeletal animation.</summary>
UNIGINE_INLINE void decomposeTransform(const mat4 &m, quat &q0, quat &q1, vec3 &scale)
{
	mat3 rotate, rotation = mat3(m);
	orthonormalize(rotate, rotation);
	q0 = rotate.getQuat();
	q1.x = (m.m03 * q0.w + m.m13 * q0.z - m.m23 * q0.y) * 0.5f;
	q1.y = (-m.m03 * q0.z + m.m13 * q0.w + m.m23 * q0.x) * 0.5f;
	q1.z = (m.m03 * q0.y - m.m13 * q0.x + m.m23 * q0.w) * 0.5f;
	q1.w = (m.m03 * q0.x + m.m13 * q0.y + m.m23 * q0.z) * 0.5f;
	scale.x = rotate.m00 * rotation.m00 + rotate.m10 * rotation.m10 + rotate.m20 * rotation.m20;
	scale.y = rotate.m01 * rotation.m01 + rotate.m11 * rotation.m11 + rotate.m21 * rotation.m21;
	scale.z = rotate.m02 * rotation.m02 + rotate.m12 * rotation.m12 + rotate.m22 * rotation.m22;
}
/// <summary>Decomposes a given transformation matrix into translation, rotation and scale components.</summary>
UNIGINE_INLINE void decomposeTransform(const mat4 &m, vec3 &position, quat &rot, vec3 &scale)
{
	mat3 rotate, rotation = mat3(m);
	orthonormalize(rotate, rotation);
	position.x = m.m03;
	position.y = m.m13;
	position.z = m.m23;
	rot = rotate.getQuat();
	scale.x = rotate.m00 * rotation.m00 + rotate.m10 * rotation.m10 + rotate.m20 * rotation.m20;
	scale.y = rotate.m01 * rotation.m01 + rotate.m11 * rotation.m11 + rotate.m21 * rotation.m21;
	scale.z = rotate.m02 * rotation.m02 + rotate.m12 * rotation.m12 + rotate.m22 * rotation.m22;
}
/// <summary>Decomposes a given transformation matrix into a vector representing translation and uniform scale and a quaternion representing rotation.</summary>
UNIGINE_INLINE void decomposeTransform(const mat4 &m, vec4 &position, quat &rot)
{
	mat3 rotate, rotation = mat3(m);
	orthonormalize(rotate, rotation);
	float sx = rotate.m00 * rotation.m00 + rotate.m10 * rotation.m10 + rotate.m20 * rotation.m20;
	float sy = rotate.m01 * rotation.m01 + rotate.m11 * rotation.m11 + rotate.m21 * rotation.m21;
	float sz = rotate.m02 * rotation.m02 + rotate.m12 * rotation.m12 + rotate.m22 * rotation.m22;
	position.x = m.m03;
	position.y = m.m13;
	position.z = m.m23;
	position.w = (sx + sy + sz) * (1.0f / 3.0f);
	rot = rotate.getQuat();
}

/// <summary>Returns the transformation matrix for the specified position, rotation and scale.</summary>
UNIGINE_INLINE mat4 &composeTransform(mat4 &ret, const quat &q0, const quat &q1, const vec3 &scale)
{
	float ilength = Math::rcp(dot(q0, q1)) * 2.0f;
	float sx = scale.x;
	float sy = scale.y;
	float sz = scale.z;
	float nx = q0.x * ilength;
	float ny = q0.y * ilength;
	float nz = q0.z * ilength;
	float nw = q0.w * ilength;
	float xx = q0.x * nx;
	float yy = q0.y * ny;
	float zz = q0.z * nz;
	float ww = q0.w * nw;
	float xy = q0.x * ny;
	float xz = q0.x * nz;
	float xw = q0.x * nw;
	float yz = q0.y * nz;
	float yw = q0.y * nw;
	float zw = q0.z * nw;
	ret.m00 = (ww + xx - yy - zz) * sx * 0.5f;
	ret.m10 = (xy + zw) * sx;
	ret.m20 = (xz - yw) * sx;
	ret.m30 = 0.0f;
	ret.m01 = (xy - zw) * sy;
	ret.m11 = (ww + yy - xx - zz) * sy * 0.5f;
	ret.m21 = (yz + xw) * sy;
	ret.m31 = 0.0f;
	ret.m02 = (xz + yw) * sz;
	ret.m12 = (yz - xw) * sz;
	ret.m22 = (ww + zz - xx - yy) * sz * 0.5f;
	ret.m32 = 0.0f;
	ret.m03 = q1.w * nx + q1.x * nw + q1.z * ny - q1.y * nz;
	ret.m13 = q1.w * ny + q1.x * nz + q1.y * nw - q1.z * nx;
	ret.m23 = q1.w * nz + q1.y * nx + q1.z * nw - q1.x * ny;
	ret.m33 = 1.0f;
	return ret;
}
/// <summary>Returns the transformation matrix for the specified position, rotation and scale.</summary>
UNIGINE_INLINE mat4 &composeTransform(mat4 &ret, const vec3 &position, const quat &rot, const vec3 &scale)
{
	float sx = scale.x;
	float sy = scale.y;
	float sz = scale.z;
	float x2 = rot.x + rot.x;
	float y2 = rot.y + rot.y;
	float z2 = rot.z + rot.z;
	float xx2 = rot.x * x2;
	float yy2 = rot.y * y2;
	float zz2 = rot.z * z2;
	float zx2 = rot.z * x2;
	float xy2 = rot.x * y2;
	float yz2 = rot.y * z2;
	float wx2 = rot.w * x2;
	float wy2 = rot.w * y2;
	float wz2 = rot.w * z2;
	ret.m00 = (1.0f - yy2 - zz2) * sx;
	ret.m10 = (xy2 + wz2) * sx;
	ret.m20 = (zx2 - wy2) * sx;
	ret.m30 = 0.0f;
	ret.m01 = (xy2 - wz2) * sy;
	ret.m11 = (1.0f - xx2 - zz2) * sy;
	ret.m21 = (yz2 + wx2) * sy;
	ret.m31 = 0.0f;
	ret.m02 = (zx2 + wy2) * sz;
	ret.m12 = (yz2 - wx2) * sz;
	ret.m22 = (1.0f - xx2 - yy2) * sz;
	ret.m32 = 0.0f;
	ret.m03 = position.x;
	ret.m13 = position.y;
	ret.m23 = position.z;
	ret.m33 = 1.0f;
	return ret;
}
/// <summary>Returns the transformation matrix for the specified position and rotation.</summary>
UNIGINE_INLINE mat4 &composeTransform(mat4 &ret, const vec4 &position, const quat &rot)
{
	float x2 = (rot.x + rot.x) * position.w;
	float y2 = (rot.y + rot.y) * position.w;
	float z2 = (rot.z + rot.z) * position.w;
	float xx2 = rot.x * x2;
	float yy2 = rot.y * y2;
	float zz2 = rot.z * z2;
	float zx2 = rot.z * x2;
	float xy2 = rot.x * y2;
	float yz2 = rot.y * z2;
	float wx2 = rot.w * x2;
	float wy2 = rot.w * y2;
	float wz2 = rot.w * z2;
	ret.m00 = position.w - yy2 - zz2;
	ret.m10 = xy2 + wz2;
	ret.m20 = zx2 - wy2;
	ret.m30 = 0.0f;
	ret.m01 = xy2 - wz2;
	ret.m11 = position.w - xx2 - zz2;
	ret.m21 = yz2 + wx2;
	ret.m31 = 0.0f;
	ret.m02 = zx2 + wy2;
	ret.m12 = yz2 - wx2;
	ret.m22 = position.w - xx2 - yy2;
	ret.m32 = 0.0f;
	ret.m03 = position.x;
	ret.m13 = position.y;
	ret.m23 = position.z;
	ret.m33 = 1.0f;
	return ret;
}

/// <summary>Resets the transformation scale by replacing the scale component of the transformation matrix with a vec3_one (vector filled with ones). The original scale is stored in the vector argument.</summary>
UNIGINE_INLINE mat4 &removeScale(mat4 &ret, vec3 &scale)
{
	vec3 pos;
	quat rot;
	decomposeTransform(ret, pos, rot, scale);
	composeTransform(ret, pos, rot, vec3_one);
	return ret;
}

/// <summary>Resets the transformation scale by replacing the scale component of the transformation matrix with a vec3_one (vector filled with ones).</summary>
UNIGINE_INLINE mat4 &removeScale(mat4 &ret)
{
	vec3 scale;
	removeScale(ret, scale);
	return ret;
}

/// <summary>Returns the interpolated matrix according to the following formula: m0 + (m 1 - m 0) * k .</summary>
UNIGINE_INLINE mat4 &lerp(mat4 &ret, const mat4 &m0, const mat4 &m1, float k)
{
	vec3 positions[3];
	quat rotations[3];
	vec3 scales[3];
	decomposeTransform(m0, positions[0], rotations[0], scales[0]);
	decomposeTransform(m1, positions[1], rotations[1], scales[1]);
	lerp(positions[2], positions[0], positions[1], k);
	slerp(rotations[2], rotations[0], rotations[1], k);
	lerp(scales[2], scales[0], scales[1], k);
	return composeTransform(ret, positions[2], rotations[2], scales[2]);
}

//////////////////////////////////////////////////////////////////////////
// dmat4
//////////////////////////////////////////////////////////////////////////
/// <summary>Sets new matrix values using a given source quaternion.</summary>
UNIGINE_INLINE void dmat4::set(const quat &q)
{
	mat3 m = q.getMat3();
	m00 = m.m00;
	m01 = m.m01;
	m02 = m.m02;
	m03 = 0.0;
	m10 = m.m10;
	m11 = m.m11;
	m12 = m.m12;
	m13 = 0.0;
	m20 = m.m20;
	m21 = m.m21;
	m22 = m.m22;
	m23 = 0.0;
}
/// <summary>Sets new matrix values using a given quaternion and a dvec3 vector.</summary>
UNIGINE_INLINE void dmat4::set(const quat &q, const dvec3 &v)
{
	mat3 m = q.getMat3();
	m00 = m.m00;
	m01 = m.m01;
	m02 = m.m02;
	m03 = v.x;
	m10 = m.m10;
	m11 = m.m11;
	m12 = m.m12;
	m13 = v.y;
	m20 = m.m20;
	m21 = m.m21;
	m22 = m.m22;
	m23 = v.z;
}
/// <summary>Returns the quaternion, representing the rotation part of the matrix.</summary>
UNIGINE_INLINE quat dmat4::getRotate() const
{
	mat3 rotate;
	orthonormalize(rotate, mat3(*this));
	return rotate.getQuat();
}

/// <summary>Resets the transformation scale by replacing the scale component of the transformation matrix with a vec3_one (vector filled with ones). The original scale is stored in the vector argument.</summary>
UNIGINE_INLINE dmat4 &removeScale(dmat4 &ret, vec3 &scale)
{
	dvec3 pos;
	quat rot;
	decomposeTransform(ret, pos, rot, scale);
	composeTransform(ret, pos, rot, vec3_one);
	return ret;
}

/// <summary>Resets the transformation scale by replacing the scale component of the transformation matrix with a vec3_one (vector filled with ones).</summary>
UNIGINE_INLINE dmat4 &removeScale(dmat4 &ret)
{
	vec3 scale;
	removeScale(ret, scale);
	return ret;
}

/// <summary>Returns the interpolated matrix according to the following formula: m0 + (m 1 - m 0) * k .</summary>
UNIGINE_INLINE dmat4 &lerp(dmat4 &ret, const dmat4 &m0, const dmat4 &m1, double k)
{
	dvec3 positions[3];
	quat rotations[3];
	vec3 scales[3];
	decomposeTransform(m0, positions[0], rotations[0], scales[0]);
	decomposeTransform(m1, positions[1], rotations[1], scales[1]);
	lerp(positions[2], positions[0], positions[1], k);
	slerp(rotations[2], rotations[0], rotations[1], (float)k);
	lerp(scales[2], scales[0], scales[1], (float)k);
	return composeTransform(ret, positions[2], rotations[2], scales[2]);
}

/// <summary>Decomposes a given transformation matrix into translation, rotation and scale components.</summary>
UNIGINE_INLINE void decomposeTransform(const dmat4 &m, dvec3 &position, quat &rot, vec3 &scale)
{
	mat3 rotate, rotation = mat3(m);
	orthonormalize(rotate, rotation);
	position.x = m.m03;
	position.y = m.m13;
	position.z = m.m23;
	rot = rotate.getQuat();
	scale.x = rotate.m00 * rotation.m00 + rotate.m10 * rotation.m10 + rotate.m20 * rotation.m20;
	scale.y = rotate.m01 * rotation.m01 + rotate.m11 * rotation.m11 + rotate.m21 * rotation.m21;
	scale.z = rotate.m02 * rotation.m02 + rotate.m12 * rotation.m12 + rotate.m22 * rotation.m22;
}
/// <summary>Returns the transformation matrix for the specified position, rotation and scale.</summary>
UNIGINE_INLINE dmat4 &composeTransform(dmat4 &ret, const dvec3 &position, const quat &rot, const vec3 &scale)
{
	float sx = scale.x;
	float sy = scale.y;
	float sz = scale.z;
	float x2 = rot.x + rot.x;
	float y2 = rot.y + rot.y;
	float z2 = rot.z + rot.z;
	float xx2 = rot.x * x2;
	float yy2 = rot.y * y2;
	float zz2 = rot.z * z2;
	float zx2 = rot.z * x2;
	float xy2 = rot.x * y2;
	float yz2 = rot.y * z2;
	float wx2 = rot.w * x2;
	float wy2 = rot.w * y2;
	float wz2 = rot.w * z2;
	ret.m00 = (1.0f - yy2 - zz2) * sx;
	ret.m10 = (xy2 + wz2) * sx;
	ret.m20 = (zx2 - wy2) * sx;
	ret.m01 = (xy2 - wz2) * sy;
	ret.m11 = (1.0f - xx2 - zz2) * sy;
	ret.m21 = (yz2 + wx2) * sy;
	ret.m02 = (zx2 + wy2) * sz;
	ret.m12 = (yz2 - wx2) * sz;
	ret.m22 = (1.0f - xx2 - yy2) * sz;
	ret.m03 = position.x;
	ret.m13 = position.y;
	ret.m23 = position.z;
	return ret;
}


//////////////////////////////////////////////////////////////////////////
// Constants
//////////////////////////////////////////////////////////////////////////
constexpr Vec2 Vec2_zero(0.0f, ConstexprTag{});
constexpr Vec2 Vec2_one(1.0f, ConstexprTag{});
constexpr Vec2 Vec2_half(0.5f, ConstexprTag{});
constexpr Vec2 Vec2_eps(Consts::EPS, ConstexprTag{});
constexpr Vec2 Vec2_inf(Consts::INF, ConstexprTag{});

constexpr Vec3 Vec3_zero(0.0f, ConstexprTag{});
constexpr Vec3 Vec3_one(1.0f, ConstexprTag{});
constexpr Vec3 Vec3_eps(Consts::EPS, ConstexprTag{});
constexpr Vec3 Vec3_inf(Consts::INF, ConstexprTag{});
constexpr Vec3 Vec3_up(0.0f, 0.0f, 1.0f, ConstexprTag{});
constexpr Vec3 Vec3_down(0.0f, 0.0f, -1.0f, ConstexprTag{});
constexpr Vec3 Vec3_forward(0.0f, 1.0f, 0.0f, ConstexprTag{});
constexpr Vec3 Vec3_back(0.0f, -1.0f, 0.0f, ConstexprTag{});
constexpr Vec3 Vec3_right(1.0f, 0.0f, 0.0f, ConstexprTag{});
constexpr Vec3 Vec3_left(-1.0f, 0.0f, 0.0f, ConstexprTag{});

constexpr Vec4 Vec4_zero(0.0f, ConstexprTag{});
constexpr Vec4 Vec4_one(1.0f, ConstexprTag{});
constexpr Vec4 Vec4_eps(Consts::EPS, ConstexprTag{});
constexpr Vec4 Vec4_inf(Consts::INF, ConstexprTag{});

#if defined(USE_DOUBLE) || defined(UNIGINE_DOUBLE)
	constexpr Mat4 Mat4_zero(0.0, ConstexprTag{});
	constexpr Mat4 Mat4_one(1.0, ConstexprTag{});
	constexpr Mat4 Mat4_identity(1.0, 0.0, 0.0,
								 0.0, 1.0, 0.0,
								 0.0, 0.0, 1.0,
								 0.0, 0.0, 0.0, ConstexprTag{});
#else
	constexpr Mat4 Mat4_zero(0.0f, ConstexprTag{});
	constexpr Mat4 Mat4_one(1.0f, ConstexprTag{});
	constexpr Mat4 Mat4_identity(1.0f, 0.0f, 0.0f, 0.0f,
								 0.0f, 1.0f, 0.0f, 0.0f,
								 0.0f, 0.0f, 1.0f, 0.0f,
								 0.0f, 0.0f, 0.0f, 1.0f, ConstexprTag{});
#endif

//////////////////////////////////////////////////////////////////////////
// Utils
//////////////////////////////////////////////////////////////////////////
/// <summary>Rotates the source quaternion towards the target quaternion by an angular step of max_angle (note, that the rotation will not overshoot).</summary>
/// <param name="source">Source quaternion.</param>
/// <param name="target">Target quaternion.</param>
/// <param name="max_angle">Angular step, in degrees. If a negative value is specified, 0 will be used instead.</param>
UNIGINE_INLINE quat rotateTowards(const quat &source, const quat &target, float max_angle)
{
	if (max_angle <= 0.0f)
		return source;

	float angle = getAngle(source, target);
	if (angle < max_angle)
		return target;
	return slerp(source, target, max_angle / angle);
}
/// <summary>Rotates the source vector towards the target vector by an angular step of max_angle (note, that the rotation will not overshoot).</summary>
/// <param name="source">Source vector.</param>
/// <param name="target">Target vector.</param>
/// <param name="max_angle">Angular step, in degrees. If a negative value is specified, 0 will be used instead.</param>
UNIGINE_INLINE vec3 rotateTowards(const vec3 &source, const vec3 &target, float max_angle)
{
	if (compare(max_angle, 0.0f))
		return source;

	vec3 real_target = target * sign(max_angle);
	max_angle = abs(max_angle);

	vec3 n = cross(source, real_target);
	if (compare(length(n), 0.0f)) // vectors are colinear
	{
		n = cross(source, vec3_up);
		if (compare(length(n), 0.0f)) // both vectors are colinear with Z
			n = cross(source, vec3_back);
	}

	float angle = getAngle(source, real_target, n);
	if (angle < max_angle)
		return real_target;

	return quat(n, max_angle) * source;
}

/// <summary>Returns the rotation quaternion for the specified "forward" and "up" directions.</summary>
UNIGINE_INLINE quat rotationFromDir(const vec3 &forward, const vec3 &up)
{
	vec3 x, y, z;
	y = normalize(forward);
	cross(x, y, up).normalize();
	cross(z, x, y).normalize();

	mat3 rotation{x, y, z};
	return rotation.getQuat();
}
/// <summary>Returns the rotation quaternion for the specified "forward" direction (the default "up" vector is used).</summary>
UNIGINE_INLINE quat rotationFromDir(const vec3 &forward)
{
	return rotationFromDir(forward, vec3_up);
}

/// <summary>Returns the rotation quaternion for the specified source and target directions.</summary>
UNIGINE_INLINE quat rotationFromTo(const vec3 &from_dir, const vec3 &to_dir)
{
	float d = dot(from_dir, to_dir);
	if (compare(d, 1.0f))
		return quat_identity;

	if (compare(d, -1.0f))
	{
		vec3 axis = cross(from_dir, vec3_forward);
		if (compare(axis.length2(), 0.0f))
			axis = cross(from_dir, vec3_right);
		return quat(axis, 180.0f);
	}

	quat ret;
	vec3 axis = cross(from_dir, to_dir);
	ret.x = axis.x;
	ret.y = axis.y;
	ret.z = axis.z;
	ret.w = 1.0f + d;
	ret.normalize();
	return ret;
}

UNIGINE_INLINE bool insideRect(const ivec2 &coord, const ivec2 &rect_min, const ivec2 &rect_max)
{
	if (coord.x < rect_min.x)
		return false;
	if (coord.y < rect_min.y)
		return false;
	if (coord.x > rect_max.x)
		return false;
	if (coord.y > rect_max.y)
		return false;
	return true;
}

/// <summary>Returns a blue noise value for the given pixel coordinates.</summary>
UNIGINE_INLINE vec4 blueNoise(int x, int y)
{
	static constexpr vec4 blue_noise_16x16[] =
	{
		{0.086275f, 0.149020f, 0.031373f, 0.847059f, ConstexprTag{}}, {0.894118f, 0.807843f, 0.835294f, 0.713726f, ConstexprTag{}}, {0.356863f, 0.717647f, 0.529412f, 0.956863f, ConstexprTag{}}, {0.819608f, 0.090196f, 0.184314f, 0.556863f, ConstexprTag{}}, {0.098039f, 0.180392f, 0.003922f, 0.207843f, ConstexprTag{}}, {0.254902f, 0.682353f, 0.776471f, 0.294118f, ConstexprTag{}}, {0.635294f, 0.341176f, 0.458824f, 0.592157f, ConstexprTag{}}, {0.047059f, 0.874510f, 0.992157f, 0.156863f, ConstexprTag{}}, {0.160784f, 0.756863f, 0.839216f, 0.976471f, ConstexprTag{}}, {0.588235f, 0.247059f, 0.129412f, 0.274510f, ConstexprTag{}}, {0.070588f, 0.435294f, 0.698039f, 0.807843f, ConstexprTag{}}, {0.756863f, 0.078431f, 0.333333f, 0.368627f, ConstexprTag{}}, {0.615686f, 0.623529f, 0.219608f, 0.600000f, ConstexprTag{}}, {0.705882f, 0.011765f, 0.980392f, 0.098039f, ConstexprTag{}}, {0.121569f, 0.521569f, 0.423529f, 0.690196f, ConstexprTag{}}, {0.803922f, 0.658824f, 0.662745f, 0.921569f, ConstexprTag{}},
		{0.403922f, 0.466667f, 0.603922f, 0.517647f, ConstexprTag{}}, {0.035294f, 0.298039f, 0.227451f, 0.105882f, ConstexprTag{}}, {0.682353f, 0.937255f, 0.396078f, 0.435294f, ConstexprTag{}}, {0.447059f, 0.494118f, 0.874510f, 0.800000f, ConstexprTag{}}, {0.552941f, 0.003922f, 0.278431f, 0.066667f, ConstexprTag{}}, {0.964706f, 0.749020f, 0.501961f, 0.929412f, ConstexprTag{}}, {0.345098f, 0.278431f, 0.090196f, 0.407843f, ConstexprTag{}}, {0.847059f, 0.403922f, 0.650980f, 0.819608f, ConstexprTag{}}, {0.396078f, 0.047059f, 0.035294f, 0.333333f, ConstexprTag{}}, {0.788235f, 0.501961f, 0.517647f, 0.639216f, ConstexprTag{}}, {0.537255f, 0.800000f, 0.427451f, 0.709804f, ConstexprTag{}}, {0.266667f, 0.549020f, 0.623529f, 0.545098f, ConstexprTag{}}, {0.423529f, 0.286275f, 0.756863f, 0.141176f, ConstexprTag{}}, {0.968628f, 0.823529f, 0.294118f, 0.301961f, ConstexprTag{}}, {0.517647f, 0.407843f, 0.505882f, 0.462745f, ConstexprTag{}}, {0.658824f, 1.000000f, 0.905882f, 0.031373f, ConstexprTag{}},
		{0.780392f, 0.207843f, 0.454902f, 0.176471f, ConstexprTag{}}, {0.592157f, 0.380392f, 0.121569f, 0.243137f, ConstexprTag{}}, {0.282353f, 0.850980f, 0.701961f, 0.317647f, ConstexprTag{}}, {0.184314f, 0.607843f, 0.572549f, 0.611765f, ConstexprTag{}}, {0.760784f, 0.431373f, 0.807843f, 0.490196f, ConstexprTag{}}, {0.129412f, 0.909804f, 0.913726f, 0.733333f, ConstexprTag{}}, {0.698039f, 0.541176f, 0.717647f, 0.666667f, ConstexprTag{}}, {0.466667f, 0.152941f, 0.552941f, 0.109804f, ConstexprTag{}}, {0.219608f, 0.933333f, 0.309804f, 0.470588f, ConstexprTag{}}, {0.976471f, 0.701961f, 0.254902f, 0.050980f, ConstexprTag{}}, {0.105882f, 0.352941f, 0.952941f, 0.211765f, ConstexprTag{}}, {0.368627f, 0.964706f, 0.886275f, 0.878431f, ConstexprTag{}}, {0.858824f, 0.164706f, 0.007843f, 0.933333f, ConstexprTag{}}, {0.039216f, 0.898039f, 0.160784f, 0.772549f, ConstexprTag{}}, {0.203922f, 0.105882f, 0.800000f, 0.396078f, ConstexprTag{}}, {0.313726f, 0.705882f, 0.082353f, 0.811765f, ConstexprTag{}},
		{0.141176f, 0.031373f, 0.741176f, 0.882353f, ConstexprTag{}}, {0.972549f, 0.643137f, 0.305882f, 0.678431f, ConstexprTag{}}, {0.854902f, 0.121569f, 0.960784f, 0.972549f, ConstexprTag{}}, {0.490196f, 0.258824f, 0.047059f, 0.007843f, ConstexprTag{}}, {0.909804f, 0.792157f, 0.356863f, 0.850980f, ConstexprTag{}}, {0.007843f, 0.203922f, 0.223529f, 0.258824f, ConstexprTag{}}, {0.580392f, 0.639216f, 0.113725f, 0.188235f, ConstexprTag{}}, {0.290196f, 0.839216f, 0.392157f, 0.890196f, ConstexprTag{}}, {0.886275f, 0.596078f, 0.858824f, 0.572549f, ConstexprTag{}}, {0.721569f, 0.219608f, 0.737255f, 0.952941f, ConstexprTag{}}, {0.501961f, 0.117647f, 0.192157f, 0.419608f, ConstexprTag{}}, {0.149020f, 0.662745f, 0.478431f, 0.501961f, ConstexprTag{}}, {0.925490f, 0.450980f, 0.576471f, 0.000000f, ConstexprTag{}}, {0.568627f, 0.745098f, 0.690196f, 0.254902f, ConstexprTag{}}, {0.827451f, 0.325490f, 0.349020f, 0.631373f, ConstexprTag{}}, {0.478431f, 0.572549f, 0.937255f, 0.552941f, ConstexprTag{}},
		{0.701961f, 0.780392f, 0.176471f, 0.372549f, ConstexprTag{}}, {0.223529f, 0.882353f, 0.784314f, 0.756863f, ConstexprTag{}}, {0.062745f, 0.533333f, 0.486275f, 0.454902f, ConstexprTag{}}, {0.627451f, 0.329412f, 0.635294f, 0.160784f, ConstexprTag{}}, {0.329412f, 0.980392f, 0.156863f, 0.352941f, ConstexprTag{}}, {0.411765f, 0.101961f, 0.439216f, 0.529412f, ConstexprTag{}}, {0.807843f, 0.364706f, 0.972549f, 0.780392f, ConstexprTag{}}, {0.176471f, 0.058824f, 0.592157f, 0.376471f, ConstexprTag{}}, {0.058824f, 0.423529f, 0.788235f, 0.239216f, ConstexprTag{}}, {0.611765f, 0.305882f, 0.141176f, 0.752941f, ConstexprTag{}}, {0.317647f, 0.878431f, 0.364706f, 0.090196f, ConstexprTag{}}, {0.662745f, 0.019608f, 0.070588f, 0.670588f, ConstexprTag{}}, {0.733333f, 0.584314f, 0.823529f, 0.345098f, ConstexprTag{}}, {0.250980f, 0.231373f, 0.403922f, 0.721569f, ConstexprTag{}}, {0.349020f, 0.505882f, 0.247059f, 0.984314f, ConstexprTag{}}, {0.901961f, 0.956863f, 0.545098f, 0.058824f, ConstexprTag{}},
		{0.427451f, 0.415686f, 0.058824f, 0.298039f, ConstexprTag{}}, {0.556863f, 0.184314f, 0.388235f, 0.121569f, ConstexprTag{}}, {0.376471f, 0.725490f, 0.258824f, 0.584314f, ConstexprTag{}}, {0.737255f, 0.043137f, 0.925490f, 0.901961f, ConstexprTag{}}, {0.239216f, 0.462745f, 0.764706f, 0.647059f, ConstexprTag{}}, {0.996078f, 0.698039f, 0.686275f, 0.094118f, ConstexprTag{}}, {0.670588f, 0.517647f, 0.282353f, 0.996078f, ConstexprTag{}}, {0.533333f, 0.776471f, 0.474510f, 0.027451f, ConstexprTag{}}, {0.949020f, 0.996078f, 0.019608f, 0.615686f, ConstexprTag{}}, {0.431373f, 0.737255f, 0.941177f, 0.858824f, ConstexprTag{}}, {0.019608f, 0.478431f, 0.678431f, 0.282353f, ConstexprTag{}}, {0.800000f, 0.788235f, 0.611765f, 0.815686f, ConstexprTag{}}, {0.458824f, 0.376471f, 0.996078f, 0.580392f, ConstexprTag{}}, {0.090196f, 0.843137f, 0.109804f, 0.152941f, ConstexprTag{}}, {0.639216f, 0.074510f, 0.878431f, 0.203922f, ConstexprTag{}}, {0.000000f, 0.274510f, 0.647059f, 0.486275f, ConstexprTag{}},
		{0.294118f, 0.674510f, 0.968628f, 0.937255f, ConstexprTag{}}, {0.952941f, 0.482353f, 0.827451f, 0.835294f, ConstexprTag{}}, {0.109804f, 0.925490f, 0.588235f, 0.223529f, ConstexprTag{}}, {0.843137f, 0.615686f, 0.011765f, 0.725490f, ConstexprTag{}}, {0.152941f, 0.827451f, 0.537255f, 0.423529f, ConstexprTag{}}, {0.462745f, 0.290196f, 0.890196f, 0.290196f, ConstexprTag{}}, {0.082353f, 0.894118f, 0.066667f, 0.701961f, ConstexprTag{}}, {0.360784f, 0.250980f, 0.643137f, 0.458824f, ConstexprTag{}}, {0.776471f, 0.168627f, 0.239216f, 0.509804f, ConstexprTag{}}, {0.235294f, 0.552941f, 0.533333f, 0.172549f, ConstexprTag{}}, {0.850980f, 0.066667f, 0.317647f, 0.400000f, ConstexprTag{}}, {0.380392f, 0.952941f, 0.447059f, 0.117647f, ConstexprTag{}}, {0.192157f, 0.188235f, 0.211765f, 0.917647f, ConstexprTag{}}, {0.941177f, 0.619608f, 0.290196f, 0.443137f, ConstexprTag{}}, {0.525490f, 0.921569f, 0.713726f, 0.866667f, ConstexprTag{}}, {0.772549f, 0.133333f, 0.498039f, 0.619608f, ConstexprTag{}},
		{0.180392f, 0.360784f, 0.321569f, 0.082353f, ConstexprTag{}}, {0.654902f, 0.062745f, 0.207843f, 0.392157f, ConstexprTag{}}, {0.784314f, 0.235294f, 0.125490f, 0.513726f, ConstexprTag{}}, {0.513726f, 0.388235f, 0.419608f, 0.035294f, ConstexprTag{}}, {0.603922f, 0.160784f, 0.329412f, 0.925490f, ConstexprTag{}}, {0.709804f, 0.576471f, 0.196078f, 0.560784f, ConstexprTag{}}, {0.874510f, 0.015686f, 0.831373f, 0.137255f, ConstexprTag{}}, {0.270588f, 0.670588f, 0.384314f, 0.803922f, ConstexprTag{}}, {0.117647f, 0.392157f, 0.909804f, 0.321569f, ConstexprTag{}}, {0.647059f, 0.631373f, 0.172549f, 0.686275f, ConstexprTag{}}, {0.549020f, 0.333333f, 0.847059f, 0.964706f, ConstexprTag{}}, {0.988235f, 0.262745f, 0.749020f, 0.533333f, ConstexprTag{}}, {0.286275f, 0.529412f, 0.556863f, 0.737255f, ConstexprTag{}}, {0.596078f, 0.694118f, 0.792157f, 0.313726f, ConstexprTag{}}, {0.125490f, 0.313726f, 0.027451f, 0.039216f, ConstexprTag{}}, {0.866667f, 0.803922f, 0.443137f, 0.776471f, ConstexprTag{}},
		{0.470588f, 0.968628f, 0.898039f, 0.694118f, ConstexprTag{}}, {0.258824f, 0.556863f, 0.752941f, 0.270588f, ConstexprTag{}}, {0.043137f, 0.866667f, 0.666667f, 0.792157f, ConstexprTag{}}, {0.905882f, 0.764706f, 0.870588f, 0.192157f, ConstexprTag{}}, {0.305882f, 0.109804f, 0.725490f, 0.337255f, ConstexprTag{}}, {0.015686f, 0.945098f, 0.094118f, 0.843137f, ConstexprTag{}}, {0.400000f, 0.447059f, 0.509804f, 0.227451f, ConstexprTag{}}, {0.572549f, 0.811765f, 0.780392f, 0.949020f, ConstexprTag{}}, {0.921569f, 0.125490f, 0.694118f, 0.078431f, ConstexprTag{}}, {0.474510f, 0.913726f, 0.415686f, 0.874510f, ConstexprTag{}}, {0.156863f, 0.721569f, 0.098039f, 0.219608f, ConstexprTag{}}, {0.074510f, 0.094118f, 0.050980f, 0.019608f, ConstexprTag{}}, {0.690196f, 0.870588f, 0.929412f, 0.650980f, ConstexprTag{}}, {0.831373f, 0.443137f, 0.376471f, 0.235294f, ConstexprTag{}}, {0.392157f, 0.027451f, 0.152941f, 0.415686f, ConstexprTag{}}, {0.717647f, 0.592157f, 0.615686f, 0.541176f, ConstexprTag{}},
		{0.984314f, 0.709804f, 0.086275f, 0.341176f, ConstexprTag{}}, {0.584314f, 0.439216f, 0.270588f, 0.658824f, ConstexprTag{}}, {0.364706f, 0.282353f, 0.549020f, 0.960784f, ConstexprTag{}}, {0.435294f, 0.666667f, 0.470588f, 0.596078f, ConstexprTag{}}, {0.211765f, 0.345098f, 1.000000f, 0.450980f, ConstexprTag{}}, {0.956863f, 0.509804f, 0.607843f, 0.745098f, ConstexprTag{}}, {0.811765f, 0.741176f, 0.298039f, 0.011765f, ConstexprTag{}}, {0.188235f, 0.317647f, 0.137255f, 0.635294f, ConstexprTag{}}, {0.729412f, 0.227451f, 0.580392f, 0.411765f, ConstexprTag{}}, {0.309804f, 0.486275f, 0.976471f, 0.588235f, ConstexprTag{}}, {0.898039f, 0.831373f, 0.266667f, 0.764706f, ConstexprTag{}}, {0.749020f, 0.411765f, 0.482353f, 0.349020f, ConstexprTag{}}, {0.494118f, 0.156863f, 0.654902f, 0.498039f, ConstexprTag{}}, {0.227451f, 0.760784f, 0.200000f, 0.823529f, ConstexprTag{}}, {0.027451f, 0.498039f, 0.843137f, 0.988235f, ConstexprTag{}}, {0.333333f, 0.211765f, 0.525490f, 0.164706f, ConstexprTag{}},
		{0.094118f, 0.835294f, 0.407843f, 0.909804f, ConstexprTag{}}, {0.823529f, 0.145098f, 0.945098f, 0.478431f, ConstexprTag{}}, {0.164706f, 0.007843f, 0.023529f, 0.129412f, ConstexprTag{}}, {0.694118f, 0.992157f, 0.164706f, 0.054902f, ConstexprTag{}}, {0.752941f, 0.603922f, 0.368627f, 0.886275f, ConstexprTag{}}, {0.631373f, 0.192157f, 0.250980f, 0.674510f, ConstexprTag{}}, {0.498039f, 0.862745f, 0.917647f, 0.364706f, ConstexprTag{}}, {0.050980f, 0.054902f, 0.462745f, 0.537255f, ConstexprTag{}}, {0.678431f, 0.972549f, 0.015686f, 0.262745f, ConstexprTag{}}, {0.415686f, 0.600000f, 0.352941f, 0.149020f, ConstexprTag{}}, {0.031373f, 0.000000f, 0.733333f, 0.466667f, ConstexprTag{}}, {0.352941f, 0.654902f, 0.866667f, 0.070588f, ConstexprTag{}}, {0.623529f, 0.988235f, 0.596078f, 0.898039f, ConstexprTag{}}, {0.439216f, 0.243137f, 0.337255f, 0.603922f, ConstexprTag{}}, {0.890196f, 0.901961f, 0.988235f, 0.101961f, ConstexprTag{}}, {0.541176f, 0.086275f, 0.705882f, 0.749020f, ConstexprTag{}},
		{0.247059f, 0.337255f, 0.231373f, 0.003922f, ConstexprTag{}}, {0.643137f, 0.537255f, 0.345098f, 0.231373f, ConstexprTag{}}, {0.929412f, 0.796079f, 0.803922f, 0.827451f, ConstexprTag{}}, {0.066667f, 0.474510f, 0.639216f, 0.403922f, ConstexprTag{}}, {0.545098f, 0.254902f, 0.768628f, 0.278431f, ConstexprTag{}}, {0.113725f, 0.082353f, 0.062745f, 0.168627f, ConstexprTag{}}, {0.341176f, 0.690196f, 0.854902f, 0.494118f, ConstexprTag{}}, {0.243137f, 0.545098f, 0.658824f, 0.913726f, ConstexprTag{}}, {0.945098f, 0.427451f, 0.215686f, 0.717647f, ConstexprTag{}}, {0.607843f, 0.768628f, 0.811765f, 0.980392f, ConstexprTag{}}, {0.862745f, 0.200000f, 0.541176f, 0.796079f, ConstexprTag{}}, {0.200000f, 0.356863f, 0.117647f, 0.662745f, ConstexprTag{}}, {0.796079f, 0.568627f, 0.431373f, 0.286275f, ConstexprTag{}}, {0.960784f, 0.301961f, 0.000000f, 0.196078f, ConstexprTag{}}, {0.145098f, 0.635294f, 0.760784f, 0.380392f, ConstexprTag{}}, {0.764706f, 0.396078f, 0.133333f, 0.568627f, ConstexprTag{}},
		{0.509804f, 0.686275f, 0.584314f, 0.305882f, ConstexprTag{}}, {0.407843f, 0.196078f, 0.682353f, 0.627451f, ConstexprTag{}}, {0.870588f, 0.890196f, 0.862745f, 0.729412f, ConstexprTag{}}, {0.482353f, 0.733333f, 0.203922f, 0.549020f, ConstexprTag{}}, {0.274510f, 0.400000f, 0.513726f, 0.992157f, ConstexprTag{}}, {0.980392f, 0.929412f, 0.411765f, 0.784314f, ConstexprTag{}}, {0.839216f, 0.627451f, 0.709804f, 0.113725f, ConstexprTag{}}, {0.454902f, 0.372549f, 0.313726f, 0.831373f, ConstexprTag{}}, {0.768628f, 0.141176f, 0.964706f, 0.047059f, ConstexprTag{}}, {0.137255f, 0.266667f, 0.627451f, 0.309804f, ConstexprTag{}}, {0.529412f, 0.886275f, 0.168627f, 0.388235f, ConstexprTag{}}, {0.278431f, 0.513726f, 0.901961f, 0.125490f, ConstexprTag{}}, {0.101961f, 0.815686f, 0.243137f, 0.945098f, ConstexprTag{}}, {0.576471f, 0.035294f, 0.301961f, 0.705882f, ConstexprTag{}}, {0.301961f, 0.752941f, 0.819608f, 0.447059f, ConstexprTag{}}, {0.674510f, 0.960784f, 0.450980f, 0.870588f, ConstexprTag{}},
		{0.196078f, 0.098039f, 0.933333f, 0.968628f, ConstexprTag{}}, {0.011765f, 0.611765f, 0.490196f, 0.505882f, ConstexprTag{}}, {0.325490f, 0.309804f, 0.074510f, 0.184314f, ConstexprTag{}}, {0.792157f, 0.039216f, 0.286275f, 0.329412f, ConstexprTag{}}, {0.384314f, 0.137255f, 0.949020f, 0.074510f, ConstexprTag{}}, {0.172549f, 0.784314f, 0.560784f, 0.607843f, ConstexprTag{}}, {0.666667f, 0.294118f, 0.149020f, 0.250980f, ConstexprTag{}}, {0.564706f, 0.819608f, 0.039216f, 0.439216f, ConstexprTag{}}, {0.078431f, 0.917647f, 0.494118f, 0.200000f, ConstexprTag{}}, {0.372549f, 0.713726f, 0.400000f, 0.623529f, ConstexprTag{}}, {1.000000f, 0.070588f, 0.078431f, 0.564706f, ConstexprTag{}}, {0.650980f, 0.678431f, 0.674510f, 0.854902f, ConstexprTag{}}, {0.725490f, 0.129412f, 0.956863f, 0.247059f, ConstexprTag{}}, {0.388235f, 0.454902f, 0.521569f, 0.525490f, ConstexprTag{}}, {0.054902f, 0.172549f, 0.631373f, 0.062745f, ConstexprTag{}}, {0.937255f, 0.490196f, 0.054902f, 0.788235f, ConstexprTag{}},
		{0.741176f, 0.858824f, 0.372549f, 0.427451f, ConstexprTag{}}, {0.619608f, 0.419608f, 0.145098f, 0.043137f, ConstexprTag{}}, {0.133333f, 0.949020f, 0.729412f, 0.894118f, ConstexprTag{}}, {0.713726f, 0.650980f, 0.435294f, 0.768628f, ConstexprTag{}}, {0.600000f, 0.525490f, 0.105882f, 0.474510f, ConstexprTag{}}, {0.023529f, 0.458824f, 0.894118f, 0.698039f, ConstexprTag{}}, {0.878431f, 0.215686f, 0.815686f, 0.941177f, ConstexprTag{}}, {0.298039f, 0.023529f, 0.600000f, 0.356863f, ConstexprTag{}}, {0.207843f, 0.470588f, 0.745098f, 0.682353f, ConstexprTag{}}, {0.815686f, 0.560784f, 0.274510f, 0.905882f, ConstexprTag{}}, {0.443137f, 0.321569f, 0.796079f, 0.482353f, ConstexprTag{}}, {0.003922f, 0.384314f, 0.466667f, 0.023529f, ConstexprTag{}}, {0.486275f, 0.941177f, 0.360784f, 0.741176f, ConstexprTag{}}, {0.882353f, 0.588235f, 0.721569f, 0.325490f, ConstexprTag{}}, {0.835294f, 0.905882f, 0.180392f, 0.643137f, ConstexprTag{}}, {0.450980f, 0.270588f, 0.882353f, 0.145098f, ConstexprTag{}},
		{0.262745f, 0.050980f, 0.772549f, 0.360784f, ConstexprTag{}}, {0.992157f, 0.564706f, 0.325490f, 0.266667f, ConstexprTag{}}, {0.521569f, 0.239216f, 0.984314f, 0.654902f, ConstexprTag{}}, {0.215686f, 0.368627f, 0.619608f, 0.133333f, ConstexprTag{}}, {0.917647f, 0.847059f, 0.670588f, 0.384314f, ConstexprTag{}}, {0.419608f, 0.976471f, 0.341176f, 0.862745f, ConstexprTag{}}, {0.745098f, 0.580392f, 0.235294f, 0.015686f, ConstexprTag{}}, {0.505882f, 0.113725f, 0.380392f, 0.521569f, ConstexprTag{}}, {0.933333f, 0.647059f, 0.188235f, 0.760784f, ConstexprTag{}}, {0.686275f, 0.984314f, 0.921569f, 0.086275f, ConstexprTag{}}, {0.321569f, 0.176471f, 0.568627f, 0.180392f, ConstexprTag{}}, {0.913726f, 0.854902f, 0.043137f, 0.431373f, ConstexprTag{}}, {0.231373f, 0.729412f, 0.850980f, 1.000000f, ConstexprTag{}}, {0.168627f, 0.223529f, 0.101961f, 0.839216f, ConstexprTag{}}, {0.337255f, 0.349020f, 0.564706f, 0.215686f, ConstexprTag{}}, {0.560784f, 0.772549f, 0.262745f, 0.576471f, ConstexprTag{}}
	};

	ivec2 coord(x, y);
	ivec2 coord_noise = coord % 15;
	coord = (coord / 15) % 64;
	vec4 noise = blue_noise_16x16[coord_noise.x * 16 + coord_noise.y];

	if ((coord.x * coord.y) % 3 == 1)
	{
		vec4 noise_temp = noise;
		noise.x = noise_temp.y;
		noise.y = noise_temp.x;
		noise.z = noise_temp.w;
		noise.w = noise_temp.z;
	}
	if ((coord.x + coord.y) % 2 == 1)
	{
		vec4 noise_temp = noise;
		noise.x = noise_temp.w;
		noise.y = noise_temp.z;
		noise.z = noise_temp.y;
		noise.w = noise_temp.x;
	}

	return frac(noise + vec4(itof(coord.x * 64 + coord.y) * Consts::GOLDEN_RATIO));
}

/// <summary>Returns a generated set of points with X and Y coordinates in the [-1; 1] range that describe a circle with uniform distribution of samples inside. This method is suitable for getting uniform distribution of coordinates for a circle. For example you can use it is a loop to generate UV coordinates offset for making the uniform circular blur effect. You can use the current loop iteration index as the i argument and the maximum number of iterations — as count.</summary>
/// <param name="i">Index of the current point for the disk.</param>
/// <param name="count">Number of points.</param>
/// <param name="noise">Normalized noise.</param>
UNIGINE_INLINE vec2 vogelDisk(const unsigned int i, const unsigned int count, float noise)
{
	const float r = sqrtf(float(i) + 0.5f) / sqrtf(float(count));
	float theta = 2.4f * float(i) + noise * Consts::PI;

	vec2 ret = vec2_zero;
	sincos(theta, ret.x, ret.y);
	return ret * r;
}
/// <summary>Returns a generated set of points with X and Y coordinates in the [-1; 1] range that describe a circle with uniform distribution of samples inside. This method is suitable for getting uniform distribution of coordinates for a circle. For example you can use it is a loop to generate UV coordinates offset for making the uniform circular blur effect. You can use the current loop iteration index as the i argument and the maximum number of iterations — as count.</summary>
/// <param name="i">Index of the current point for the disk.</param>
/// <param name="count">Number of points.</param>
UNIGINE_INLINE vec2 vogelDisk(const unsigned int i, const unsigned int count)
{
	vec2 ret = vec2_zero;
	sincos(2.4f * float(i), ret.x, ret.y);
	return ret * sqrtf(float(i) + 0.5f) / sqrtf(float(count));
}

/// <summary>Returns the value with the reversed order of the bits.</summary>
UNIGINE_INLINE unsigned int reverseBits(unsigned int value)
{
	unsigned int retval = 0;
	for (int i = 31; i >= 0; i--)
	{
		retval |= (value & 1) << i;
		value >>= 1;
	}
	return retval;
}
UNIGINE_INLINE  vec2 xx(const  vec2 &v) { return {v.x,v.x}; }
UNIGINE_INLINE  vec2 xx(const  vec3 &v) { return {v.x,v.x}; }
UNIGINE_INLINE  vec2 xx(const  vec4 &v) { return {v.x,v.x}; }
UNIGINE_INLINE dvec2 xx(const dvec2 &v) { return {v.x,v.x}; }
UNIGINE_INLINE dvec2 xx(const dvec3 &v) { return {v.x,v.x}; }
UNIGINE_INLINE dvec2 xx(const dvec4 &v) { return {v.x,v.x}; }
UNIGINE_INLINE ivec2 xx(const ivec2 &v) { return {v.x,v.x}; }
UNIGINE_INLINE ivec2 xx(const ivec3 &v) { return {v.x,v.x}; }
UNIGINE_INLINE ivec2 xx(const ivec4 &v) { return {v.x,v.x}; }
UNIGINE_INLINE  vec2 xy(const  vec2 &v) { return {v.x,v.y}; }
UNIGINE_INLINE  vec2 xy(const  vec3 &v) { return {v.x,v.y}; }
UNIGINE_INLINE  vec2 xy(const  vec4 &v) { return {v.x,v.y}; }
UNIGINE_INLINE dvec2 xy(const dvec2 &v) { return {v.x,v.y}; }
UNIGINE_INLINE dvec2 xy(const dvec3 &v) { return {v.x,v.y}; }
UNIGINE_INLINE dvec2 xy(const dvec4 &v) { return {v.x,v.y}; }
UNIGINE_INLINE ivec2 xy(const ivec2 &v) { return {v.x,v.y}; }
UNIGINE_INLINE ivec2 xy(const ivec3 &v) { return {v.x,v.y}; }
UNIGINE_INLINE ivec2 xy(const ivec4 &v) { return {v.x,v.y}; }
UNIGINE_INLINE  vec2 xz(const  vec3 &v) { return {v.x,v.z}; }
UNIGINE_INLINE  vec2 xz(const  vec4 &v) { return {v.x,v.z}; }
UNIGINE_INLINE dvec2 xz(const dvec3 &v) { return {v.x,v.z}; }
UNIGINE_INLINE dvec2 xz(const dvec4 &v) { return {v.x,v.z}; }
UNIGINE_INLINE ivec2 xz(const ivec3 &v) { return {v.x,v.z}; }
UNIGINE_INLINE ivec2 xz(const ivec4 &v) { return {v.x,v.z}; }
UNIGINE_INLINE  vec2 xw(const  vec4 &v) { return {v.x,v.w}; }
UNIGINE_INLINE dvec2 xw(const dvec4 &v) { return {v.x,v.w}; }
UNIGINE_INLINE ivec2 xw(const ivec4 &v) { return {v.x,v.w}; }
UNIGINE_INLINE  vec2 yx(const  vec2 &v) { return {v.y,v.x}; }
UNIGINE_INLINE  vec2 yx(const  vec3 &v) { return {v.y,v.x}; }
UNIGINE_INLINE  vec2 yx(const  vec4 &v) { return {v.y,v.x}; }
UNIGINE_INLINE dvec2 yx(const dvec2 &v) { return {v.y,v.x}; }
UNIGINE_INLINE dvec2 yx(const dvec3 &v) { return {v.y,v.x}; }
UNIGINE_INLINE dvec2 yx(const dvec4 &v) { return {v.y,v.x}; }
UNIGINE_INLINE ivec2 yx(const ivec2 &v) { return {v.y,v.x}; }
UNIGINE_INLINE ivec2 yx(const ivec3 &v) { return {v.y,v.x}; }
UNIGINE_INLINE ivec2 yx(const ivec4 &v) { return {v.y,v.x}; }
UNIGINE_INLINE  vec2 yy(const  vec2 &v) { return {v.y,v.y}; }
UNIGINE_INLINE  vec2 yy(const  vec3 &v) { return {v.y,v.y}; }
UNIGINE_INLINE  vec2 yy(const  vec4 &v) { return {v.y,v.y}; }
UNIGINE_INLINE dvec2 yy(const dvec2 &v) { return {v.y,v.y}; }
UNIGINE_INLINE dvec2 yy(const dvec3 &v) { return {v.y,v.y}; }
UNIGINE_INLINE dvec2 yy(const dvec4 &v) { return {v.y,v.y}; }
UNIGINE_INLINE ivec2 yy(const ivec2 &v) { return {v.y,v.y}; }
UNIGINE_INLINE ivec2 yy(const ivec3 &v) { return {v.y,v.y}; }
UNIGINE_INLINE ivec2 yy(const ivec4 &v) { return {v.y,v.y}; }
UNIGINE_INLINE  vec2 yz(const  vec3 &v) { return {v.y,v.z}; }
UNIGINE_INLINE  vec2 yz(const  vec4 &v) { return {v.y,v.z}; }
UNIGINE_INLINE dvec2 yz(const dvec3 &v) { return {v.y,v.z}; }
UNIGINE_INLINE dvec2 yz(const dvec4 &v) { return {v.y,v.z}; }
UNIGINE_INLINE ivec2 yz(const ivec3 &v) { return {v.y,v.z}; }
UNIGINE_INLINE ivec2 yz(const ivec4 &v) { return {v.y,v.z}; }
UNIGINE_INLINE  vec2 yw(const  vec4 &v) { return {v.y,v.w}; }
UNIGINE_INLINE dvec2 yw(const dvec4 &v) { return {v.y,v.w}; }
UNIGINE_INLINE ivec2 yw(const ivec4 &v) { return {v.y,v.w}; }
UNIGINE_INLINE  vec2 zx(const  vec3 &v) { return {v.z,v.x}; }
UNIGINE_INLINE  vec2 zx(const  vec4 &v) { return {v.z,v.x}; }
UNIGINE_INLINE dvec2 zx(const dvec3 &v) { return {v.z,v.x}; }
UNIGINE_INLINE dvec2 zx(const dvec4 &v) { return {v.z,v.x}; }
UNIGINE_INLINE ivec2 zx(const ivec3 &v) { return {v.z,v.x}; }
UNIGINE_INLINE ivec2 zx(const ivec4 &v) { return {v.z,v.x}; }
UNIGINE_INLINE  vec2 zy(const  vec3 &v) { return {v.z,v.y}; }
UNIGINE_INLINE  vec2 zy(const  vec4 &v) { return {v.z,v.y}; }
UNIGINE_INLINE dvec2 zy(const dvec3 &v) { return {v.z,v.y}; }
UNIGINE_INLINE dvec2 zy(const dvec4 &v) { return {v.z,v.y}; }
UNIGINE_INLINE ivec2 zy(const ivec3 &v) { return {v.z,v.y}; }
UNIGINE_INLINE ivec2 zy(const ivec4 &v) { return {v.z,v.y}; }
UNIGINE_INLINE  vec2 zz(const  vec3 &v) { return {v.z,v.z}; }
UNIGINE_INLINE  vec2 zz(const  vec4 &v) { return {v.z,v.z}; }
UNIGINE_INLINE dvec2 zz(const dvec3 &v) { return {v.z,v.z}; }
UNIGINE_INLINE dvec2 zz(const dvec4 &v) { return {v.z,v.z}; }
UNIGINE_INLINE ivec2 zz(const ivec3 &v) { return {v.z,v.z}; }
UNIGINE_INLINE ivec2 zz(const ivec4 &v) { return {v.z,v.z}; }
UNIGINE_INLINE  vec2 zw(const  vec4 &v) { return {v.z,v.w}; }
UNIGINE_INLINE dvec2 zw(const dvec4 &v) { return {v.z,v.w}; }
UNIGINE_INLINE ivec2 zw(const ivec4 &v) { return {v.z,v.w}; }
UNIGINE_INLINE  vec2 wx(const  vec4 &v) { return {v.w,v.x}; }
UNIGINE_INLINE dvec2 wx(const dvec4 &v) { return {v.w,v.x}; }
UNIGINE_INLINE ivec2 wx(const ivec4 &v) { return {v.w,v.x}; }
UNIGINE_INLINE  vec2 wy(const  vec4 &v) { return {v.w,v.y}; }
UNIGINE_INLINE dvec2 wy(const dvec4 &v) { return {v.w,v.y}; }
UNIGINE_INLINE ivec2 wy(const ivec4 &v) { return {v.w,v.y}; }
UNIGINE_INLINE  vec2 wz(const  vec4 &v) { return {v.w,v.z}; }
UNIGINE_INLINE dvec2 wz(const dvec4 &v) { return {v.w,v.z}; }
UNIGINE_INLINE ivec2 wz(const ivec4 &v) { return {v.w,v.z}; }
UNIGINE_INLINE  vec2 ww(const  vec4 &v) { return {v.w,v.w}; }
UNIGINE_INLINE dvec2 ww(const dvec4 &v) { return {v.w,v.w}; }
UNIGINE_INLINE ivec2 ww(const ivec4 &v) { return {v.w,v.w}; }
UNIGINE_INLINE  vec3 xxx(const  vec2 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE  vec3 xxx(const  vec3 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE  vec3 xxx(const  vec4 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE dvec3 xxx(const dvec2 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE dvec3 xxx(const dvec3 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE dvec3 xxx(const dvec4 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE ivec3 xxx(const ivec2 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE ivec3 xxx(const ivec3 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE ivec3 xxx(const ivec4 &v) { return {v.x,v.x,v.x}; }
UNIGINE_INLINE  vec3 xxy(const  vec2 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE  vec3 xxy(const  vec3 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE  vec3 xxy(const  vec4 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE dvec3 xxy(const dvec2 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE dvec3 xxy(const dvec3 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE dvec3 xxy(const dvec4 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE ivec3 xxy(const ivec2 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE ivec3 xxy(const ivec3 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE ivec3 xxy(const ivec4 &v) { return {v.x,v.x,v.y}; }
UNIGINE_INLINE  vec3 xxz(const  vec3 &v) { return {v.x,v.x,v.z}; }
UNIGINE_INLINE  vec3 xxz(const  vec4 &v) { return {v.x,v.x,v.z}; }
UNIGINE_INLINE dvec3 xxz(const dvec3 &v) { return {v.x,v.x,v.z}; }
UNIGINE_INLINE dvec3 xxz(const dvec4 &v) { return {v.x,v.x,v.z}; }
UNIGINE_INLINE ivec3 xxz(const ivec3 &v) { return {v.x,v.x,v.z}; }
UNIGINE_INLINE ivec3 xxz(const ivec4 &v) { return {v.x,v.x,v.z}; }
UNIGINE_INLINE  vec3 xxw(const  vec4 &v) { return {v.x,v.x,v.w}; }
UNIGINE_INLINE dvec3 xxw(const dvec4 &v) { return {v.x,v.x,v.w}; }
UNIGINE_INLINE ivec3 xxw(const ivec4 &v) { return {v.x,v.x,v.w}; }
UNIGINE_INLINE  vec3 xyx(const  vec2 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE  vec3 xyx(const  vec3 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE  vec3 xyx(const  vec4 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE dvec3 xyx(const dvec2 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE dvec3 xyx(const dvec3 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE dvec3 xyx(const dvec4 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE ivec3 xyx(const ivec2 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE ivec3 xyx(const ivec3 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE ivec3 xyx(const ivec4 &v) { return {v.x,v.y,v.x}; }
UNIGINE_INLINE  vec3 xyy(const  vec2 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE  vec3 xyy(const  vec3 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE  vec3 xyy(const  vec4 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE dvec3 xyy(const dvec2 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE dvec3 xyy(const dvec3 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE dvec3 xyy(const dvec4 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE ivec3 xyy(const ivec2 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE ivec3 xyy(const ivec3 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE ivec3 xyy(const ivec4 &v) { return {v.x,v.y,v.y}; }
UNIGINE_INLINE  vec3 xyz(const  vec3 &v) { return {v.x,v.y,v.z}; }
UNIGINE_INLINE  vec3 xyz(const  vec4 &v) { return {v.x,v.y,v.z}; }
UNIGINE_INLINE dvec3 xyz(const dvec3 &v) { return {v.x,v.y,v.z}; }
UNIGINE_INLINE dvec3 xyz(const dvec4 &v) { return {v.x,v.y,v.z}; }
UNIGINE_INLINE ivec3 xyz(const ivec3 &v) { return {v.x,v.y,v.z}; }
UNIGINE_INLINE ivec3 xyz(const ivec4 &v) { return {v.x,v.y,v.z}; }
UNIGINE_INLINE  vec3 xyw(const  vec4 &v) { return {v.x,v.y,v.w}; }
UNIGINE_INLINE dvec3 xyw(const dvec4 &v) { return {v.x,v.y,v.w}; }
UNIGINE_INLINE ivec3 xyw(const ivec4 &v) { return {v.x,v.y,v.w}; }
UNIGINE_INLINE  vec3 xzx(const  vec3 &v) { return {v.x,v.z,v.x}; }
UNIGINE_INLINE  vec3 xzx(const  vec4 &v) { return {v.x,v.z,v.x}; }
UNIGINE_INLINE dvec3 xzx(const dvec3 &v) { return {v.x,v.z,v.x}; }
UNIGINE_INLINE dvec3 xzx(const dvec4 &v) { return {v.x,v.z,v.x}; }
UNIGINE_INLINE ivec3 xzx(const ivec3 &v) { return {v.x,v.z,v.x}; }
UNIGINE_INLINE ivec3 xzx(const ivec4 &v) { return {v.x,v.z,v.x}; }
UNIGINE_INLINE  vec3 xzy(const  vec3 &v) { return {v.x,v.z,v.y}; }
UNIGINE_INLINE  vec3 xzy(const  vec4 &v) { return {v.x,v.z,v.y}; }
UNIGINE_INLINE dvec3 xzy(const dvec3 &v) { return {v.x,v.z,v.y}; }
UNIGINE_INLINE dvec3 xzy(const dvec4 &v) { return {v.x,v.z,v.y}; }
UNIGINE_INLINE ivec3 xzy(const ivec3 &v) { return {v.x,v.z,v.y}; }
UNIGINE_INLINE ivec3 xzy(const ivec4 &v) { return {v.x,v.z,v.y}; }
UNIGINE_INLINE  vec3 xzz(const  vec3 &v) { return {v.x,v.z,v.z}; }
UNIGINE_INLINE  vec3 xzz(const  vec4 &v) { return {v.x,v.z,v.z}; }
UNIGINE_INLINE dvec3 xzz(const dvec3 &v) { return {v.x,v.z,v.z}; }
UNIGINE_INLINE dvec3 xzz(const dvec4 &v) { return {v.x,v.z,v.z}; }
UNIGINE_INLINE ivec3 xzz(const ivec3 &v) { return {v.x,v.z,v.z}; }
UNIGINE_INLINE ivec3 xzz(const ivec4 &v) { return {v.x,v.z,v.z}; }
UNIGINE_INLINE  vec3 xzw(const  vec4 &v) { return {v.x,v.z,v.w}; }
UNIGINE_INLINE dvec3 xzw(const dvec4 &v) { return {v.x,v.z,v.w}; }
UNIGINE_INLINE ivec3 xzw(const ivec4 &v) { return {v.x,v.z,v.w}; }
UNIGINE_INLINE  vec3 xwx(const  vec4 &v) { return {v.x,v.w,v.x}; }
UNIGINE_INLINE dvec3 xwx(const dvec4 &v) { return {v.x,v.w,v.x}; }
UNIGINE_INLINE ivec3 xwx(const ivec4 &v) { return {v.x,v.w,v.x}; }
UNIGINE_INLINE  vec3 xwy(const  vec4 &v) { return {v.x,v.w,v.y}; }
UNIGINE_INLINE dvec3 xwy(const dvec4 &v) { return {v.x,v.w,v.y}; }
UNIGINE_INLINE ivec3 xwy(const ivec4 &v) { return {v.x,v.w,v.y}; }
UNIGINE_INLINE  vec3 xwz(const  vec4 &v) { return {v.x,v.w,v.z}; }
UNIGINE_INLINE dvec3 xwz(const dvec4 &v) { return {v.x,v.w,v.z}; }
UNIGINE_INLINE ivec3 xwz(const ivec4 &v) { return {v.x,v.w,v.z}; }
UNIGINE_INLINE  vec3 xww(const  vec4 &v) { return {v.x,v.w,v.w}; }
UNIGINE_INLINE dvec3 xww(const dvec4 &v) { return {v.x,v.w,v.w}; }
UNIGINE_INLINE ivec3 xww(const ivec4 &v) { return {v.x,v.w,v.w}; }
UNIGINE_INLINE  vec3 yxx(const  vec2 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE  vec3 yxx(const  vec3 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE  vec3 yxx(const  vec4 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE dvec3 yxx(const dvec2 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE dvec3 yxx(const dvec3 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE dvec3 yxx(const dvec4 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE ivec3 yxx(const ivec2 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE ivec3 yxx(const ivec3 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE ivec3 yxx(const ivec4 &v) { return {v.y,v.x,v.x}; }
UNIGINE_INLINE  vec3 yxy(const  vec2 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE  vec3 yxy(const  vec3 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE  vec3 yxy(const  vec4 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE dvec3 yxy(const dvec2 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE dvec3 yxy(const dvec3 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE dvec3 yxy(const dvec4 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE ivec3 yxy(const ivec2 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE ivec3 yxy(const ivec3 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE ivec3 yxy(const ivec4 &v) { return {v.y,v.x,v.y}; }
UNIGINE_INLINE  vec3 yxz(const  vec3 &v) { return {v.y,v.x,v.z}; }
UNIGINE_INLINE  vec3 yxz(const  vec4 &v) { return {v.y,v.x,v.z}; }
UNIGINE_INLINE dvec3 yxz(const dvec3 &v) { return {v.y,v.x,v.z}; }
UNIGINE_INLINE dvec3 yxz(const dvec4 &v) { return {v.y,v.x,v.z}; }
UNIGINE_INLINE ivec3 yxz(const ivec3 &v) { return {v.y,v.x,v.z}; }
UNIGINE_INLINE ivec3 yxz(const ivec4 &v) { return {v.y,v.x,v.z}; }
UNIGINE_INLINE  vec3 yxw(const  vec4 &v) { return {v.y,v.x,v.w}; }
UNIGINE_INLINE dvec3 yxw(const dvec4 &v) { return {v.y,v.x,v.w}; }
UNIGINE_INLINE ivec3 yxw(const ivec4 &v) { return {v.y,v.x,v.w}; }
UNIGINE_INLINE  vec3 yyx(const  vec2 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE  vec3 yyx(const  vec3 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE  vec3 yyx(const  vec4 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE dvec3 yyx(const dvec2 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE dvec3 yyx(const dvec3 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE dvec3 yyx(const dvec4 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE ivec3 yyx(const ivec2 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE ivec3 yyx(const ivec3 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE ivec3 yyx(const ivec4 &v) { return {v.y,v.y,v.x}; }
UNIGINE_INLINE  vec3 yyy(const  vec2 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE  vec3 yyy(const  vec3 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE  vec3 yyy(const  vec4 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE dvec3 yyy(const dvec2 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE dvec3 yyy(const dvec3 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE dvec3 yyy(const dvec4 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE ivec3 yyy(const ivec2 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE ivec3 yyy(const ivec3 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE ivec3 yyy(const ivec4 &v) { return {v.y,v.y,v.y}; }
UNIGINE_INLINE  vec3 yyz(const  vec3 &v) { return {v.y,v.y,v.z}; }
UNIGINE_INLINE  vec3 yyz(const  vec4 &v) { return {v.y,v.y,v.z}; }
UNIGINE_INLINE dvec3 yyz(const dvec3 &v) { return {v.y,v.y,v.z}; }
UNIGINE_INLINE dvec3 yyz(const dvec4 &v) { return {v.y,v.y,v.z}; }
UNIGINE_INLINE ivec3 yyz(const ivec3 &v) { return {v.y,v.y,v.z}; }
UNIGINE_INLINE ivec3 yyz(const ivec4 &v) { return {v.y,v.y,v.z}; }
UNIGINE_INLINE  vec3 yyw(const  vec4 &v) { return {v.y,v.y,v.w}; }
UNIGINE_INLINE dvec3 yyw(const dvec4 &v) { return {v.y,v.y,v.w}; }
UNIGINE_INLINE ivec3 yyw(const ivec4 &v) { return {v.y,v.y,v.w}; }
UNIGINE_INLINE  vec3 yzx(const  vec3 &v) { return {v.y,v.z,v.x}; }
UNIGINE_INLINE  vec3 yzx(const  vec4 &v) { return {v.y,v.z,v.x}; }
UNIGINE_INLINE dvec3 yzx(const dvec3 &v) { return {v.y,v.z,v.x}; }
UNIGINE_INLINE dvec3 yzx(const dvec4 &v) { return {v.y,v.z,v.x}; }
UNIGINE_INLINE ivec3 yzx(const ivec3 &v) { return {v.y,v.z,v.x}; }
UNIGINE_INLINE ivec3 yzx(const ivec4 &v) { return {v.y,v.z,v.x}; }
UNIGINE_INLINE  vec3 yzy(const  vec3 &v) { return {v.y,v.z,v.y}; }
UNIGINE_INLINE  vec3 yzy(const  vec4 &v) { return {v.y,v.z,v.y}; }
UNIGINE_INLINE dvec3 yzy(const dvec3 &v) { return {v.y,v.z,v.y}; }
UNIGINE_INLINE dvec3 yzy(const dvec4 &v) { return {v.y,v.z,v.y}; }
UNIGINE_INLINE ivec3 yzy(const ivec3 &v) { return {v.y,v.z,v.y}; }
UNIGINE_INLINE ivec3 yzy(const ivec4 &v) { return {v.y,v.z,v.y}; }
UNIGINE_INLINE  vec3 yzz(const  vec3 &v) { return {v.y,v.z,v.z}; }
UNIGINE_INLINE  vec3 yzz(const  vec4 &v) { return {v.y,v.z,v.z}; }
UNIGINE_INLINE dvec3 yzz(const dvec3 &v) { return {v.y,v.z,v.z}; }
UNIGINE_INLINE dvec3 yzz(const dvec4 &v) { return {v.y,v.z,v.z}; }
UNIGINE_INLINE ivec3 yzz(const ivec3 &v) { return {v.y,v.z,v.z}; }
UNIGINE_INLINE ivec3 yzz(const ivec4 &v) { return {v.y,v.z,v.z}; }
UNIGINE_INLINE  vec3 yzw(const  vec4 &v) { return {v.y,v.z,v.w}; }
UNIGINE_INLINE dvec3 yzw(const dvec4 &v) { return {v.y,v.z,v.w}; }
UNIGINE_INLINE ivec3 yzw(const ivec4 &v) { return {v.y,v.z,v.w}; }
UNIGINE_INLINE  vec3 ywx(const  vec4 &v) { return {v.y,v.w,v.x}; }
UNIGINE_INLINE dvec3 ywx(const dvec4 &v) { return {v.y,v.w,v.x}; }
UNIGINE_INLINE ivec3 ywx(const ivec4 &v) { return {v.y,v.w,v.x}; }
UNIGINE_INLINE  vec3 ywy(const  vec4 &v) { return {v.y,v.w,v.y}; }
UNIGINE_INLINE dvec3 ywy(const dvec4 &v) { return {v.y,v.w,v.y}; }
UNIGINE_INLINE ivec3 ywy(const ivec4 &v) { return {v.y,v.w,v.y}; }
UNIGINE_INLINE  vec3 ywz(const  vec4 &v) { return {v.y,v.w,v.z}; }
UNIGINE_INLINE dvec3 ywz(const dvec4 &v) { return {v.y,v.w,v.z}; }
UNIGINE_INLINE ivec3 ywz(const ivec4 &v) { return {v.y,v.w,v.z}; }
UNIGINE_INLINE  vec3 yww(const  vec4 &v) { return {v.y,v.w,v.w}; }
UNIGINE_INLINE dvec3 yww(const dvec4 &v) { return {v.y,v.w,v.w}; }
UNIGINE_INLINE ivec3 yww(const ivec4 &v) { return {v.y,v.w,v.w}; }
UNIGINE_INLINE  vec3 zxx(const  vec3 &v) { return {v.z,v.x,v.x}; }
UNIGINE_INLINE  vec3 zxx(const  vec4 &v) { return {v.z,v.x,v.x}; }
UNIGINE_INLINE dvec3 zxx(const dvec3 &v) { return {v.z,v.x,v.x}; }
UNIGINE_INLINE dvec3 zxx(const dvec4 &v) { return {v.z,v.x,v.x}; }
UNIGINE_INLINE ivec3 zxx(const ivec3 &v) { return {v.z,v.x,v.x}; }
UNIGINE_INLINE ivec3 zxx(const ivec4 &v) { return {v.z,v.x,v.x}; }
UNIGINE_INLINE  vec3 zxy(const  vec3 &v) { return {v.z,v.x,v.y}; }
UNIGINE_INLINE  vec3 zxy(const  vec4 &v) { return {v.z,v.x,v.y}; }
UNIGINE_INLINE dvec3 zxy(const dvec3 &v) { return {v.z,v.x,v.y}; }
UNIGINE_INLINE dvec3 zxy(const dvec4 &v) { return {v.z,v.x,v.y}; }
UNIGINE_INLINE ivec3 zxy(const ivec3 &v) { return {v.z,v.x,v.y}; }
UNIGINE_INLINE ivec3 zxy(const ivec4 &v) { return {v.z,v.x,v.y}; }
UNIGINE_INLINE  vec3 zxz(const  vec3 &v) { return {v.z,v.x,v.z}; }
UNIGINE_INLINE  vec3 zxz(const  vec4 &v) { return {v.z,v.x,v.z}; }
UNIGINE_INLINE dvec3 zxz(const dvec3 &v) { return {v.z,v.x,v.z}; }
UNIGINE_INLINE dvec3 zxz(const dvec4 &v) { return {v.z,v.x,v.z}; }
UNIGINE_INLINE ivec3 zxz(const ivec3 &v) { return {v.z,v.x,v.z}; }
UNIGINE_INLINE ivec3 zxz(const ivec4 &v) { return {v.z,v.x,v.z}; }
UNIGINE_INLINE  vec3 zxw(const  vec4 &v) { return {v.z,v.x,v.w}; }
UNIGINE_INLINE dvec3 zxw(const dvec4 &v) { return {v.z,v.x,v.w}; }
UNIGINE_INLINE ivec3 zxw(const ivec4 &v) { return {v.z,v.x,v.w}; }
UNIGINE_INLINE  vec3 zyx(const  vec3 &v) { return {v.z,v.y,v.x}; }
UNIGINE_INLINE  vec3 zyx(const  vec4 &v) { return {v.z,v.y,v.x}; }
UNIGINE_INLINE dvec3 zyx(const dvec3 &v) { return {v.z,v.y,v.x}; }
UNIGINE_INLINE dvec3 zyx(const dvec4 &v) { return {v.z,v.y,v.x}; }
UNIGINE_INLINE ivec3 zyx(const ivec3 &v) { return {v.z,v.y,v.x}; }
UNIGINE_INLINE ivec3 zyx(const ivec4 &v) { return {v.z,v.y,v.x}; }
UNIGINE_INLINE  vec3 zyy(const  vec3 &v) { return {v.z,v.y,v.y}; }
UNIGINE_INLINE  vec3 zyy(const  vec4 &v) { return {v.z,v.y,v.y}; }
UNIGINE_INLINE dvec3 zyy(const dvec3 &v) { return {v.z,v.y,v.y}; }
UNIGINE_INLINE dvec3 zyy(const dvec4 &v) { return {v.z,v.y,v.y}; }
UNIGINE_INLINE ivec3 zyy(const ivec3 &v) { return {v.z,v.y,v.y}; }
UNIGINE_INLINE ivec3 zyy(const ivec4 &v) { return {v.z,v.y,v.y}; }
UNIGINE_INLINE  vec3 zyz(const  vec3 &v) { return {v.z,v.y,v.z}; }
UNIGINE_INLINE  vec3 zyz(const  vec4 &v) { return {v.z,v.y,v.z}; }
UNIGINE_INLINE dvec3 zyz(const dvec3 &v) { return {v.z,v.y,v.z}; }
UNIGINE_INLINE dvec3 zyz(const dvec4 &v) { return {v.z,v.y,v.z}; }
UNIGINE_INLINE ivec3 zyz(const ivec3 &v) { return {v.z,v.y,v.z}; }
UNIGINE_INLINE ivec3 zyz(const ivec4 &v) { return {v.z,v.y,v.z}; }
UNIGINE_INLINE  vec3 zyw(const  vec4 &v) { return {v.z,v.y,v.w}; }
UNIGINE_INLINE dvec3 zyw(const dvec4 &v) { return {v.z,v.y,v.w}; }
UNIGINE_INLINE ivec3 zyw(const ivec4 &v) { return {v.z,v.y,v.w}; }
UNIGINE_INLINE  vec3 zzx(const  vec3 &v) { return {v.z,v.z,v.x}; }
UNIGINE_INLINE  vec3 zzx(const  vec4 &v) { return {v.z,v.z,v.x}; }
UNIGINE_INLINE dvec3 zzx(const dvec3 &v) { return {v.z,v.z,v.x}; }
UNIGINE_INLINE dvec3 zzx(const dvec4 &v) { return {v.z,v.z,v.x}; }
UNIGINE_INLINE ivec3 zzx(const ivec3 &v) { return {v.z,v.z,v.x}; }
UNIGINE_INLINE ivec3 zzx(const ivec4 &v) { return {v.z,v.z,v.x}; }
UNIGINE_INLINE  vec3 zzy(const  vec3 &v) { return {v.z,v.z,v.y}; }
UNIGINE_INLINE  vec3 zzy(const  vec4 &v) { return {v.z,v.z,v.y}; }
UNIGINE_INLINE dvec3 zzy(const dvec3 &v) { return {v.z,v.z,v.y}; }
UNIGINE_INLINE dvec3 zzy(const dvec4 &v) { return {v.z,v.z,v.y}; }
UNIGINE_INLINE ivec3 zzy(const ivec3 &v) { return {v.z,v.z,v.y}; }
UNIGINE_INLINE ivec3 zzy(const ivec4 &v) { return {v.z,v.z,v.y}; }
UNIGINE_INLINE  vec3 zzz(const  vec3 &v) { return {v.z,v.z,v.z}; }
UNIGINE_INLINE  vec3 zzz(const  vec4 &v) { return {v.z,v.z,v.z}; }
UNIGINE_INLINE dvec3 zzz(const dvec3 &v) { return {v.z,v.z,v.z}; }
UNIGINE_INLINE dvec3 zzz(const dvec4 &v) { return {v.z,v.z,v.z}; }
UNIGINE_INLINE ivec3 zzz(const ivec3 &v) { return {v.z,v.z,v.z}; }
UNIGINE_INLINE ivec3 zzz(const ivec4 &v) { return {v.z,v.z,v.z}; }
UNIGINE_INLINE  vec3 zzw(const  vec4 &v) { return {v.z,v.z,v.w}; }
UNIGINE_INLINE dvec3 zzw(const dvec4 &v) { return {v.z,v.z,v.w}; }
UNIGINE_INLINE ivec3 zzw(const ivec4 &v) { return {v.z,v.z,v.w}; }
UNIGINE_INLINE  vec3 zwx(const  vec4 &v) { return {v.z,v.w,v.x}; }
UNIGINE_INLINE dvec3 zwx(const dvec4 &v) { return {v.z,v.w,v.x}; }
UNIGINE_INLINE ivec3 zwx(const ivec4 &v) { return {v.z,v.w,v.x}; }
UNIGINE_INLINE  vec3 zwy(const  vec4 &v) { return {v.z,v.w,v.y}; }
UNIGINE_INLINE dvec3 zwy(const dvec4 &v) { return {v.z,v.w,v.y}; }
UNIGINE_INLINE ivec3 zwy(const ivec4 &v) { return {v.z,v.w,v.y}; }
UNIGINE_INLINE  vec3 zwz(const  vec4 &v) { return {v.z,v.w,v.z}; }
UNIGINE_INLINE dvec3 zwz(const dvec4 &v) { return {v.z,v.w,v.z}; }
UNIGINE_INLINE ivec3 zwz(const ivec4 &v) { return {v.z,v.w,v.z}; }
UNIGINE_INLINE  vec3 zww(const  vec4 &v) { return {v.z,v.w,v.w}; }
UNIGINE_INLINE dvec3 zww(const dvec4 &v) { return {v.z,v.w,v.w}; }
UNIGINE_INLINE ivec3 zww(const ivec4 &v) { return {v.z,v.w,v.w}; }
UNIGINE_INLINE  vec3 wxx(const  vec4 &v) { return {v.w,v.x,v.x}; }
UNIGINE_INLINE dvec3 wxx(const dvec4 &v) { return {v.w,v.x,v.x}; }
UNIGINE_INLINE ivec3 wxx(const ivec4 &v) { return {v.w,v.x,v.x}; }
UNIGINE_INLINE  vec3 wxy(const  vec4 &v) { return {v.w,v.x,v.y}; }
UNIGINE_INLINE dvec3 wxy(const dvec4 &v) { return {v.w,v.x,v.y}; }
UNIGINE_INLINE ivec3 wxy(const ivec4 &v) { return {v.w,v.x,v.y}; }
UNIGINE_INLINE  vec3 wxz(const  vec4 &v) { return {v.w,v.x,v.z}; }
UNIGINE_INLINE dvec3 wxz(const dvec4 &v) { return {v.w,v.x,v.z}; }
UNIGINE_INLINE ivec3 wxz(const ivec4 &v) { return {v.w,v.x,v.z}; }
UNIGINE_INLINE  vec3 wxw(const  vec4 &v) { return {v.w,v.x,v.w}; }
UNIGINE_INLINE dvec3 wxw(const dvec4 &v) { return {v.w,v.x,v.w}; }
UNIGINE_INLINE ivec3 wxw(const ivec4 &v) { return {v.w,v.x,v.w}; }
UNIGINE_INLINE  vec3 wyx(const  vec4 &v) { return {v.w,v.y,v.x}; }
UNIGINE_INLINE dvec3 wyx(const dvec4 &v) { return {v.w,v.y,v.x}; }
UNIGINE_INLINE ivec3 wyx(const ivec4 &v) { return {v.w,v.y,v.x}; }
UNIGINE_INLINE  vec3 wyy(const  vec4 &v) { return {v.w,v.y,v.y}; }
UNIGINE_INLINE dvec3 wyy(const dvec4 &v) { return {v.w,v.y,v.y}; }
UNIGINE_INLINE ivec3 wyy(const ivec4 &v) { return {v.w,v.y,v.y}; }
UNIGINE_INLINE  vec3 wyz(const  vec4 &v) { return {v.w,v.y,v.z}; }
UNIGINE_INLINE dvec3 wyz(const dvec4 &v) { return {v.w,v.y,v.z}; }
UNIGINE_INLINE ivec3 wyz(const ivec4 &v) { return {v.w,v.y,v.z}; }
UNIGINE_INLINE  vec3 wyw(const  vec4 &v) { return {v.w,v.y,v.w}; }
UNIGINE_INLINE dvec3 wyw(const dvec4 &v) { return {v.w,v.y,v.w}; }
UNIGINE_INLINE ivec3 wyw(const ivec4 &v) { return {v.w,v.y,v.w}; }
UNIGINE_INLINE  vec3 wzx(const  vec4 &v) { return {v.w,v.z,v.x}; }
UNIGINE_INLINE dvec3 wzx(const dvec4 &v) { return {v.w,v.z,v.x}; }
UNIGINE_INLINE ivec3 wzx(const ivec4 &v) { return {v.w,v.z,v.x}; }
UNIGINE_INLINE  vec3 wzy(const  vec4 &v) { return {v.w,v.z,v.y}; }
UNIGINE_INLINE dvec3 wzy(const dvec4 &v) { return {v.w,v.z,v.y}; }
UNIGINE_INLINE ivec3 wzy(const ivec4 &v) { return {v.w,v.z,v.y}; }
UNIGINE_INLINE  vec3 wzz(const  vec4 &v) { return {v.w,v.z,v.z}; }
UNIGINE_INLINE dvec3 wzz(const dvec4 &v) { return {v.w,v.z,v.z}; }
UNIGINE_INLINE ivec3 wzz(const ivec4 &v) { return {v.w,v.z,v.z}; }
UNIGINE_INLINE  vec3 wzw(const  vec4 &v) { return {v.w,v.z,v.w}; }
UNIGINE_INLINE dvec3 wzw(const dvec4 &v) { return {v.w,v.z,v.w}; }
UNIGINE_INLINE ivec3 wzw(const ivec4 &v) { return {v.w,v.z,v.w}; }
UNIGINE_INLINE  vec3 wwx(const  vec4 &v) { return {v.w,v.w,v.x}; }
UNIGINE_INLINE dvec3 wwx(const dvec4 &v) { return {v.w,v.w,v.x}; }
UNIGINE_INLINE ivec3 wwx(const ivec4 &v) { return {v.w,v.w,v.x}; }
UNIGINE_INLINE  vec3 wwy(const  vec4 &v) { return {v.w,v.w,v.y}; }
UNIGINE_INLINE dvec3 wwy(const dvec4 &v) { return {v.w,v.w,v.y}; }
UNIGINE_INLINE ivec3 wwy(const ivec4 &v) { return {v.w,v.w,v.y}; }
UNIGINE_INLINE  vec3 wwz(const  vec4 &v) { return {v.w,v.w,v.z}; }
UNIGINE_INLINE dvec3 wwz(const dvec4 &v) { return {v.w,v.w,v.z}; }
UNIGINE_INLINE ivec3 wwz(const ivec4 &v) { return {v.w,v.w,v.z}; }
UNIGINE_INLINE  vec3 www(const  vec4 &v) { return {v.w,v.w,v.w}; }
UNIGINE_INLINE dvec3 www(const dvec4 &v) { return {v.w,v.w,v.w}; }
UNIGINE_INLINE ivec3 www(const ivec4 &v) { return {v.w,v.w,v.w}; }
UNIGINE_INLINE  vec4 xxxx(const  vec2 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 xxxx(const  vec3 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 xxxx(const  vec4 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 xxxx(const dvec2 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 xxxx(const dvec3 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 xxxx(const dvec4 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 xxxx(const ivec2 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 xxxx(const ivec3 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 xxxx(const ivec4 &v) { return {v.x,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 xxxy(const  vec2 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 xxxy(const  vec3 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 xxxy(const  vec4 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 xxxy(const dvec2 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 xxxy(const dvec3 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 xxxy(const dvec4 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 xxxy(const ivec2 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 xxxy(const ivec3 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 xxxy(const ivec4 &v) { return {v.x,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 xxxz(const  vec3 &v) { return {v.x,v.x,v.x,v.z}; }
UNIGINE_INLINE  vec4 xxxz(const  vec4 &v) { return {v.x,v.x,v.x,v.z}; }
UNIGINE_INLINE dvec4 xxxz(const dvec3 &v) { return {v.x,v.x,v.x,v.z}; }
UNIGINE_INLINE dvec4 xxxz(const dvec4 &v) { return {v.x,v.x,v.x,v.z}; }
UNIGINE_INLINE ivec4 xxxz(const ivec3 &v) { return {v.x,v.x,v.x,v.z}; }
UNIGINE_INLINE ivec4 xxxz(const ivec4 &v) { return {v.x,v.x,v.x,v.z}; }
UNIGINE_INLINE  vec4 xxxw(const  vec4 &v) { return {v.x,v.x,v.x,v.w}; }
UNIGINE_INLINE dvec4 xxxw(const dvec4 &v) { return {v.x,v.x,v.x,v.w}; }
UNIGINE_INLINE ivec4 xxxw(const ivec4 &v) { return {v.x,v.x,v.x,v.w}; }
UNIGINE_INLINE  vec4 xxyx(const  vec2 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 xxyx(const  vec3 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 xxyx(const  vec4 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 xxyx(const dvec2 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 xxyx(const dvec3 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 xxyx(const dvec4 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 xxyx(const ivec2 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 xxyx(const ivec3 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 xxyx(const ivec4 &v) { return {v.x,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 xxyy(const  vec2 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 xxyy(const  vec3 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 xxyy(const  vec4 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 xxyy(const dvec2 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 xxyy(const dvec3 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 xxyy(const dvec4 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 xxyy(const ivec2 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 xxyy(const ivec3 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 xxyy(const ivec4 &v) { return {v.x,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 xxyz(const  vec3 &v) { return {v.x,v.x,v.y,v.z}; }
UNIGINE_INLINE  vec4 xxyz(const  vec4 &v) { return {v.x,v.x,v.y,v.z}; }
UNIGINE_INLINE dvec4 xxyz(const dvec3 &v) { return {v.x,v.x,v.y,v.z}; }
UNIGINE_INLINE dvec4 xxyz(const dvec4 &v) { return {v.x,v.x,v.y,v.z}; }
UNIGINE_INLINE ivec4 xxyz(const ivec3 &v) { return {v.x,v.x,v.y,v.z}; }
UNIGINE_INLINE ivec4 xxyz(const ivec4 &v) { return {v.x,v.x,v.y,v.z}; }
UNIGINE_INLINE  vec4 xxyw(const  vec4 &v) { return {v.x,v.x,v.y,v.w}; }
UNIGINE_INLINE dvec4 xxyw(const dvec4 &v) { return {v.x,v.x,v.y,v.w}; }
UNIGINE_INLINE ivec4 xxyw(const ivec4 &v) { return {v.x,v.x,v.y,v.w}; }
UNIGINE_INLINE  vec4 xxzx(const  vec3 &v) { return {v.x,v.x,v.z,v.x}; }
UNIGINE_INLINE  vec4 xxzx(const  vec4 &v) { return {v.x,v.x,v.z,v.x}; }
UNIGINE_INLINE dvec4 xxzx(const dvec3 &v) { return {v.x,v.x,v.z,v.x}; }
UNIGINE_INLINE dvec4 xxzx(const dvec4 &v) { return {v.x,v.x,v.z,v.x}; }
UNIGINE_INLINE ivec4 xxzx(const ivec3 &v) { return {v.x,v.x,v.z,v.x}; }
UNIGINE_INLINE ivec4 xxzx(const ivec4 &v) { return {v.x,v.x,v.z,v.x}; }
UNIGINE_INLINE  vec4 xxzy(const  vec3 &v) { return {v.x,v.x,v.z,v.y}; }
UNIGINE_INLINE  vec4 xxzy(const  vec4 &v) { return {v.x,v.x,v.z,v.y}; }
UNIGINE_INLINE dvec4 xxzy(const dvec3 &v) { return {v.x,v.x,v.z,v.y}; }
UNIGINE_INLINE dvec4 xxzy(const dvec4 &v) { return {v.x,v.x,v.z,v.y}; }
UNIGINE_INLINE ivec4 xxzy(const ivec3 &v) { return {v.x,v.x,v.z,v.y}; }
UNIGINE_INLINE ivec4 xxzy(const ivec4 &v) { return {v.x,v.x,v.z,v.y}; }
UNIGINE_INLINE  vec4 xxzz(const  vec3 &v) { return {v.x,v.x,v.z,v.z}; }
UNIGINE_INLINE  vec4 xxzz(const  vec4 &v) { return {v.x,v.x,v.z,v.z}; }
UNIGINE_INLINE dvec4 xxzz(const dvec3 &v) { return {v.x,v.x,v.z,v.z}; }
UNIGINE_INLINE dvec4 xxzz(const dvec4 &v) { return {v.x,v.x,v.z,v.z}; }
UNIGINE_INLINE ivec4 xxzz(const ivec3 &v) { return {v.x,v.x,v.z,v.z}; }
UNIGINE_INLINE ivec4 xxzz(const ivec4 &v) { return {v.x,v.x,v.z,v.z}; }
UNIGINE_INLINE  vec4 xxzw(const  vec4 &v) { return {v.x,v.x,v.z,v.w}; }
UNIGINE_INLINE dvec4 xxzw(const dvec4 &v) { return {v.x,v.x,v.z,v.w}; }
UNIGINE_INLINE ivec4 xxzw(const ivec4 &v) { return {v.x,v.x,v.z,v.w}; }
UNIGINE_INLINE  vec4 xxwx(const  vec4 &v) { return {v.x,v.x,v.w,v.x}; }
UNIGINE_INLINE dvec4 xxwx(const dvec4 &v) { return {v.x,v.x,v.w,v.x}; }
UNIGINE_INLINE ivec4 xxwx(const ivec4 &v) { return {v.x,v.x,v.w,v.x}; }
UNIGINE_INLINE  vec4 xxwy(const  vec4 &v) { return {v.x,v.x,v.w,v.y}; }
UNIGINE_INLINE dvec4 xxwy(const dvec4 &v) { return {v.x,v.x,v.w,v.y}; }
UNIGINE_INLINE ivec4 xxwy(const ivec4 &v) { return {v.x,v.x,v.w,v.y}; }
UNIGINE_INLINE  vec4 xxwz(const  vec4 &v) { return {v.x,v.x,v.w,v.z}; }
UNIGINE_INLINE dvec4 xxwz(const dvec4 &v) { return {v.x,v.x,v.w,v.z}; }
UNIGINE_INLINE ivec4 xxwz(const ivec4 &v) { return {v.x,v.x,v.w,v.z}; }
UNIGINE_INLINE  vec4 xxww(const  vec4 &v) { return {v.x,v.x,v.w,v.w}; }
UNIGINE_INLINE dvec4 xxww(const dvec4 &v) { return {v.x,v.x,v.w,v.w}; }
UNIGINE_INLINE ivec4 xxww(const ivec4 &v) { return {v.x,v.x,v.w,v.w}; }
UNIGINE_INLINE  vec4 xyxx(const  vec2 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 xyxx(const  vec3 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 xyxx(const  vec4 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 xyxx(const dvec2 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 xyxx(const dvec3 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 xyxx(const dvec4 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 xyxx(const ivec2 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 xyxx(const ivec3 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 xyxx(const ivec4 &v) { return {v.x,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 xyxy(const  vec2 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 xyxy(const  vec3 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 xyxy(const  vec4 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 xyxy(const dvec2 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 xyxy(const dvec3 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 xyxy(const dvec4 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 xyxy(const ivec2 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 xyxy(const ivec3 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 xyxy(const ivec4 &v) { return {v.x,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 xyxz(const  vec3 &v) { return {v.x,v.y,v.x,v.z}; }
UNIGINE_INLINE  vec4 xyxz(const  vec4 &v) { return {v.x,v.y,v.x,v.z}; }
UNIGINE_INLINE dvec4 xyxz(const dvec3 &v) { return {v.x,v.y,v.x,v.z}; }
UNIGINE_INLINE dvec4 xyxz(const dvec4 &v) { return {v.x,v.y,v.x,v.z}; }
UNIGINE_INLINE ivec4 xyxz(const ivec3 &v) { return {v.x,v.y,v.x,v.z}; }
UNIGINE_INLINE ivec4 xyxz(const ivec4 &v) { return {v.x,v.y,v.x,v.z}; }
UNIGINE_INLINE  vec4 xyxw(const  vec4 &v) { return {v.x,v.y,v.x,v.w}; }
UNIGINE_INLINE dvec4 xyxw(const dvec4 &v) { return {v.x,v.y,v.x,v.w}; }
UNIGINE_INLINE ivec4 xyxw(const ivec4 &v) { return {v.x,v.y,v.x,v.w}; }
UNIGINE_INLINE  vec4 xyyx(const  vec2 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 xyyx(const  vec3 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 xyyx(const  vec4 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 xyyx(const dvec2 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 xyyx(const dvec3 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 xyyx(const dvec4 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 xyyx(const ivec2 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 xyyx(const ivec3 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 xyyx(const ivec4 &v) { return {v.x,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 xyyy(const  vec2 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 xyyy(const  vec3 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 xyyy(const  vec4 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 xyyy(const dvec2 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 xyyy(const dvec3 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 xyyy(const dvec4 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 xyyy(const ivec2 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 xyyy(const ivec3 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 xyyy(const ivec4 &v) { return {v.x,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 xyyz(const  vec3 &v) { return {v.x,v.y,v.y,v.z}; }
UNIGINE_INLINE  vec4 xyyz(const  vec4 &v) { return {v.x,v.y,v.y,v.z}; }
UNIGINE_INLINE dvec4 xyyz(const dvec3 &v) { return {v.x,v.y,v.y,v.z}; }
UNIGINE_INLINE dvec4 xyyz(const dvec4 &v) { return {v.x,v.y,v.y,v.z}; }
UNIGINE_INLINE ivec4 xyyz(const ivec3 &v) { return {v.x,v.y,v.y,v.z}; }
UNIGINE_INLINE ivec4 xyyz(const ivec4 &v) { return {v.x,v.y,v.y,v.z}; }
UNIGINE_INLINE  vec4 xyyw(const  vec4 &v) { return {v.x,v.y,v.y,v.w}; }
UNIGINE_INLINE dvec4 xyyw(const dvec4 &v) { return {v.x,v.y,v.y,v.w}; }
UNIGINE_INLINE ivec4 xyyw(const ivec4 &v) { return {v.x,v.y,v.y,v.w}; }
UNIGINE_INLINE  vec4 xyzx(const  vec3 &v) { return {v.x,v.y,v.z,v.x}; }
UNIGINE_INLINE  vec4 xyzx(const  vec4 &v) { return {v.x,v.y,v.z,v.x}; }
UNIGINE_INLINE dvec4 xyzx(const dvec3 &v) { return {v.x,v.y,v.z,v.x}; }
UNIGINE_INLINE dvec4 xyzx(const dvec4 &v) { return {v.x,v.y,v.z,v.x}; }
UNIGINE_INLINE ivec4 xyzx(const ivec3 &v) { return {v.x,v.y,v.z,v.x}; }
UNIGINE_INLINE ivec4 xyzx(const ivec4 &v) { return {v.x,v.y,v.z,v.x}; }
UNIGINE_INLINE  vec4 xyzy(const  vec3 &v) { return {v.x,v.y,v.z,v.y}; }
UNIGINE_INLINE  vec4 xyzy(const  vec4 &v) { return {v.x,v.y,v.z,v.y}; }
UNIGINE_INLINE dvec4 xyzy(const dvec3 &v) { return {v.x,v.y,v.z,v.y}; }
UNIGINE_INLINE dvec4 xyzy(const dvec4 &v) { return {v.x,v.y,v.z,v.y}; }
UNIGINE_INLINE ivec4 xyzy(const ivec3 &v) { return {v.x,v.y,v.z,v.y}; }
UNIGINE_INLINE ivec4 xyzy(const ivec4 &v) { return {v.x,v.y,v.z,v.y}; }
UNIGINE_INLINE  vec4 xyzz(const  vec3 &v) { return {v.x,v.y,v.z,v.z}; }
UNIGINE_INLINE  vec4 xyzz(const  vec4 &v) { return {v.x,v.y,v.z,v.z}; }
UNIGINE_INLINE dvec4 xyzz(const dvec3 &v) { return {v.x,v.y,v.z,v.z}; }
UNIGINE_INLINE dvec4 xyzz(const dvec4 &v) { return {v.x,v.y,v.z,v.z}; }
UNIGINE_INLINE ivec4 xyzz(const ivec3 &v) { return {v.x,v.y,v.z,v.z}; }
UNIGINE_INLINE ivec4 xyzz(const ivec4 &v) { return {v.x,v.y,v.z,v.z}; }
UNIGINE_INLINE  vec4 xyzw(const  vec4 &v) { return {v.x,v.y,v.z,v.w}; }
UNIGINE_INLINE dvec4 xyzw(const dvec4 &v) { return {v.x,v.y,v.z,v.w}; }
UNIGINE_INLINE ivec4 xyzw(const ivec4 &v) { return {v.x,v.y,v.z,v.w}; }
UNIGINE_INLINE  vec4 xywx(const  vec4 &v) { return {v.x,v.y,v.w,v.x}; }
UNIGINE_INLINE dvec4 xywx(const dvec4 &v) { return {v.x,v.y,v.w,v.x}; }
UNIGINE_INLINE ivec4 xywx(const ivec4 &v) { return {v.x,v.y,v.w,v.x}; }
UNIGINE_INLINE  vec4 xywy(const  vec4 &v) { return {v.x,v.y,v.w,v.y}; }
UNIGINE_INLINE dvec4 xywy(const dvec4 &v) { return {v.x,v.y,v.w,v.y}; }
UNIGINE_INLINE ivec4 xywy(const ivec4 &v) { return {v.x,v.y,v.w,v.y}; }
UNIGINE_INLINE  vec4 xywz(const  vec4 &v) { return {v.x,v.y,v.w,v.z}; }
UNIGINE_INLINE dvec4 xywz(const dvec4 &v) { return {v.x,v.y,v.w,v.z}; }
UNIGINE_INLINE ivec4 xywz(const ivec4 &v) { return {v.x,v.y,v.w,v.z}; }
UNIGINE_INLINE  vec4 xyww(const  vec4 &v) { return {v.x,v.y,v.w,v.w}; }
UNIGINE_INLINE dvec4 xyww(const dvec4 &v) { return {v.x,v.y,v.w,v.w}; }
UNIGINE_INLINE ivec4 xyww(const ivec4 &v) { return {v.x,v.y,v.w,v.w}; }
UNIGINE_INLINE  vec4 xzxx(const  vec3 &v) { return {v.x,v.z,v.x,v.x}; }
UNIGINE_INLINE  vec4 xzxx(const  vec4 &v) { return {v.x,v.z,v.x,v.x}; }
UNIGINE_INLINE dvec4 xzxx(const dvec3 &v) { return {v.x,v.z,v.x,v.x}; }
UNIGINE_INLINE dvec4 xzxx(const dvec4 &v) { return {v.x,v.z,v.x,v.x}; }
UNIGINE_INLINE ivec4 xzxx(const ivec3 &v) { return {v.x,v.z,v.x,v.x}; }
UNIGINE_INLINE ivec4 xzxx(const ivec4 &v) { return {v.x,v.z,v.x,v.x}; }
UNIGINE_INLINE  vec4 xzxy(const  vec3 &v) { return {v.x,v.z,v.x,v.y}; }
UNIGINE_INLINE  vec4 xzxy(const  vec4 &v) { return {v.x,v.z,v.x,v.y}; }
UNIGINE_INLINE dvec4 xzxy(const dvec3 &v) { return {v.x,v.z,v.x,v.y}; }
UNIGINE_INLINE dvec4 xzxy(const dvec4 &v) { return {v.x,v.z,v.x,v.y}; }
UNIGINE_INLINE ivec4 xzxy(const ivec3 &v) { return {v.x,v.z,v.x,v.y}; }
UNIGINE_INLINE ivec4 xzxy(const ivec4 &v) { return {v.x,v.z,v.x,v.y}; }
UNIGINE_INLINE  vec4 xzxz(const  vec3 &v) { return {v.x,v.z,v.x,v.z}; }
UNIGINE_INLINE  vec4 xzxz(const  vec4 &v) { return {v.x,v.z,v.x,v.z}; }
UNIGINE_INLINE dvec4 xzxz(const dvec3 &v) { return {v.x,v.z,v.x,v.z}; }
UNIGINE_INLINE dvec4 xzxz(const dvec4 &v) { return {v.x,v.z,v.x,v.z}; }
UNIGINE_INLINE ivec4 xzxz(const ivec3 &v) { return {v.x,v.z,v.x,v.z}; }
UNIGINE_INLINE ivec4 xzxz(const ivec4 &v) { return {v.x,v.z,v.x,v.z}; }
UNIGINE_INLINE  vec4 xzxw(const  vec4 &v) { return {v.x,v.z,v.x,v.w}; }
UNIGINE_INLINE dvec4 xzxw(const dvec4 &v) { return {v.x,v.z,v.x,v.w}; }
UNIGINE_INLINE ivec4 xzxw(const ivec4 &v) { return {v.x,v.z,v.x,v.w}; }
UNIGINE_INLINE  vec4 xzyx(const  vec3 &v) { return {v.x,v.z,v.y,v.x}; }
UNIGINE_INLINE  vec4 xzyx(const  vec4 &v) { return {v.x,v.z,v.y,v.x}; }
UNIGINE_INLINE dvec4 xzyx(const dvec3 &v) { return {v.x,v.z,v.y,v.x}; }
UNIGINE_INLINE dvec4 xzyx(const dvec4 &v) { return {v.x,v.z,v.y,v.x}; }
UNIGINE_INLINE ivec4 xzyx(const ivec3 &v) { return {v.x,v.z,v.y,v.x}; }
UNIGINE_INLINE ivec4 xzyx(const ivec4 &v) { return {v.x,v.z,v.y,v.x}; }
UNIGINE_INLINE  vec4 xzyy(const  vec3 &v) { return {v.x,v.z,v.y,v.y}; }
UNIGINE_INLINE  vec4 xzyy(const  vec4 &v) { return {v.x,v.z,v.y,v.y}; }
UNIGINE_INLINE dvec4 xzyy(const dvec3 &v) { return {v.x,v.z,v.y,v.y}; }
UNIGINE_INLINE dvec4 xzyy(const dvec4 &v) { return {v.x,v.z,v.y,v.y}; }
UNIGINE_INLINE ivec4 xzyy(const ivec3 &v) { return {v.x,v.z,v.y,v.y}; }
UNIGINE_INLINE ivec4 xzyy(const ivec4 &v) { return {v.x,v.z,v.y,v.y}; }
UNIGINE_INLINE  vec4 xzyz(const  vec3 &v) { return {v.x,v.z,v.y,v.z}; }
UNIGINE_INLINE  vec4 xzyz(const  vec4 &v) { return {v.x,v.z,v.y,v.z}; }
UNIGINE_INLINE dvec4 xzyz(const dvec3 &v) { return {v.x,v.z,v.y,v.z}; }
UNIGINE_INLINE dvec4 xzyz(const dvec4 &v) { return {v.x,v.z,v.y,v.z}; }
UNIGINE_INLINE ivec4 xzyz(const ivec3 &v) { return {v.x,v.z,v.y,v.z}; }
UNIGINE_INLINE ivec4 xzyz(const ivec4 &v) { return {v.x,v.z,v.y,v.z}; }
UNIGINE_INLINE  vec4 xzyw(const  vec4 &v) { return {v.x,v.z,v.y,v.w}; }
UNIGINE_INLINE dvec4 xzyw(const dvec4 &v) { return {v.x,v.z,v.y,v.w}; }
UNIGINE_INLINE ivec4 xzyw(const ivec4 &v) { return {v.x,v.z,v.y,v.w}; }
UNIGINE_INLINE  vec4 xzzx(const  vec3 &v) { return {v.x,v.z,v.z,v.x}; }
UNIGINE_INLINE  vec4 xzzx(const  vec4 &v) { return {v.x,v.z,v.z,v.x}; }
UNIGINE_INLINE dvec4 xzzx(const dvec3 &v) { return {v.x,v.z,v.z,v.x}; }
UNIGINE_INLINE dvec4 xzzx(const dvec4 &v) { return {v.x,v.z,v.z,v.x}; }
UNIGINE_INLINE ivec4 xzzx(const ivec3 &v) { return {v.x,v.z,v.z,v.x}; }
UNIGINE_INLINE ivec4 xzzx(const ivec4 &v) { return {v.x,v.z,v.z,v.x}; }
UNIGINE_INLINE  vec4 xzzy(const  vec3 &v) { return {v.x,v.z,v.z,v.y}; }
UNIGINE_INLINE  vec4 xzzy(const  vec4 &v) { return {v.x,v.z,v.z,v.y}; }
UNIGINE_INLINE dvec4 xzzy(const dvec3 &v) { return {v.x,v.z,v.z,v.y}; }
UNIGINE_INLINE dvec4 xzzy(const dvec4 &v) { return {v.x,v.z,v.z,v.y}; }
UNIGINE_INLINE ivec4 xzzy(const ivec3 &v) { return {v.x,v.z,v.z,v.y}; }
UNIGINE_INLINE ivec4 xzzy(const ivec4 &v) { return {v.x,v.z,v.z,v.y}; }
UNIGINE_INLINE  vec4 xzzz(const  vec3 &v) { return {v.x,v.z,v.z,v.z}; }
UNIGINE_INLINE  vec4 xzzz(const  vec4 &v) { return {v.x,v.z,v.z,v.z}; }
UNIGINE_INLINE dvec4 xzzz(const dvec3 &v) { return {v.x,v.z,v.z,v.z}; }
UNIGINE_INLINE dvec4 xzzz(const dvec4 &v) { return {v.x,v.z,v.z,v.z}; }
UNIGINE_INLINE ivec4 xzzz(const ivec3 &v) { return {v.x,v.z,v.z,v.z}; }
UNIGINE_INLINE ivec4 xzzz(const ivec4 &v) { return {v.x,v.z,v.z,v.z}; }
UNIGINE_INLINE  vec4 xzzw(const  vec4 &v) { return {v.x,v.z,v.z,v.w}; }
UNIGINE_INLINE dvec4 xzzw(const dvec4 &v) { return {v.x,v.z,v.z,v.w}; }
UNIGINE_INLINE ivec4 xzzw(const ivec4 &v) { return {v.x,v.z,v.z,v.w}; }
UNIGINE_INLINE  vec4 xzwx(const  vec4 &v) { return {v.x,v.z,v.w,v.x}; }
UNIGINE_INLINE dvec4 xzwx(const dvec4 &v) { return {v.x,v.z,v.w,v.x}; }
UNIGINE_INLINE ivec4 xzwx(const ivec4 &v) { return {v.x,v.z,v.w,v.x}; }
UNIGINE_INLINE  vec4 xzwy(const  vec4 &v) { return {v.x,v.z,v.w,v.y}; }
UNIGINE_INLINE dvec4 xzwy(const dvec4 &v) { return {v.x,v.z,v.w,v.y}; }
UNIGINE_INLINE ivec4 xzwy(const ivec4 &v) { return {v.x,v.z,v.w,v.y}; }
UNIGINE_INLINE  vec4 xzwz(const  vec4 &v) { return {v.x,v.z,v.w,v.z}; }
UNIGINE_INLINE dvec4 xzwz(const dvec4 &v) { return {v.x,v.z,v.w,v.z}; }
UNIGINE_INLINE ivec4 xzwz(const ivec4 &v) { return {v.x,v.z,v.w,v.z}; }
UNIGINE_INLINE  vec4 xzww(const  vec4 &v) { return {v.x,v.z,v.w,v.w}; }
UNIGINE_INLINE dvec4 xzww(const dvec4 &v) { return {v.x,v.z,v.w,v.w}; }
UNIGINE_INLINE ivec4 xzww(const ivec4 &v) { return {v.x,v.z,v.w,v.w}; }
UNIGINE_INLINE  vec4 xwxx(const  vec4 &v) { return {v.x,v.w,v.x,v.x}; }
UNIGINE_INLINE dvec4 xwxx(const dvec4 &v) { return {v.x,v.w,v.x,v.x}; }
UNIGINE_INLINE ivec4 xwxx(const ivec4 &v) { return {v.x,v.w,v.x,v.x}; }
UNIGINE_INLINE  vec4 xwxy(const  vec4 &v) { return {v.x,v.w,v.x,v.y}; }
UNIGINE_INLINE dvec4 xwxy(const dvec4 &v) { return {v.x,v.w,v.x,v.y}; }
UNIGINE_INLINE ivec4 xwxy(const ivec4 &v) { return {v.x,v.w,v.x,v.y}; }
UNIGINE_INLINE  vec4 xwxz(const  vec4 &v) { return {v.x,v.w,v.x,v.z}; }
UNIGINE_INLINE dvec4 xwxz(const dvec4 &v) { return {v.x,v.w,v.x,v.z}; }
UNIGINE_INLINE ivec4 xwxz(const ivec4 &v) { return {v.x,v.w,v.x,v.z}; }
UNIGINE_INLINE  vec4 xwxw(const  vec4 &v) { return {v.x,v.w,v.x,v.w}; }
UNIGINE_INLINE dvec4 xwxw(const dvec4 &v) { return {v.x,v.w,v.x,v.w}; }
UNIGINE_INLINE ivec4 xwxw(const ivec4 &v) { return {v.x,v.w,v.x,v.w}; }
UNIGINE_INLINE  vec4 xwyx(const  vec4 &v) { return {v.x,v.w,v.y,v.x}; }
UNIGINE_INLINE dvec4 xwyx(const dvec4 &v) { return {v.x,v.w,v.y,v.x}; }
UNIGINE_INLINE ivec4 xwyx(const ivec4 &v) { return {v.x,v.w,v.y,v.x}; }
UNIGINE_INLINE  vec4 xwyy(const  vec4 &v) { return {v.x,v.w,v.y,v.y}; }
UNIGINE_INLINE dvec4 xwyy(const dvec4 &v) { return {v.x,v.w,v.y,v.y}; }
UNIGINE_INLINE ivec4 xwyy(const ivec4 &v) { return {v.x,v.w,v.y,v.y}; }
UNIGINE_INLINE  vec4 xwyz(const  vec4 &v) { return {v.x,v.w,v.y,v.z}; }
UNIGINE_INLINE dvec4 xwyz(const dvec4 &v) { return {v.x,v.w,v.y,v.z}; }
UNIGINE_INLINE ivec4 xwyz(const ivec4 &v) { return {v.x,v.w,v.y,v.z}; }
UNIGINE_INLINE  vec4 xwyw(const  vec4 &v) { return {v.x,v.w,v.y,v.w}; }
UNIGINE_INLINE dvec4 xwyw(const dvec4 &v) { return {v.x,v.w,v.y,v.w}; }
UNIGINE_INLINE ivec4 xwyw(const ivec4 &v) { return {v.x,v.w,v.y,v.w}; }
UNIGINE_INLINE  vec4 xwzx(const  vec4 &v) { return {v.x,v.w,v.z,v.x}; }
UNIGINE_INLINE dvec4 xwzx(const dvec4 &v) { return {v.x,v.w,v.z,v.x}; }
UNIGINE_INLINE ivec4 xwzx(const ivec4 &v) { return {v.x,v.w,v.z,v.x}; }
UNIGINE_INLINE  vec4 xwzy(const  vec4 &v) { return {v.x,v.w,v.z,v.y}; }
UNIGINE_INLINE dvec4 xwzy(const dvec4 &v) { return {v.x,v.w,v.z,v.y}; }
UNIGINE_INLINE ivec4 xwzy(const ivec4 &v) { return {v.x,v.w,v.z,v.y}; }
UNIGINE_INLINE  vec4 xwzz(const  vec4 &v) { return {v.x,v.w,v.z,v.z}; }
UNIGINE_INLINE dvec4 xwzz(const dvec4 &v) { return {v.x,v.w,v.z,v.z}; }
UNIGINE_INLINE ivec4 xwzz(const ivec4 &v) { return {v.x,v.w,v.z,v.z}; }
UNIGINE_INLINE  vec4 xwzw(const  vec4 &v) { return {v.x,v.w,v.z,v.w}; }
UNIGINE_INLINE dvec4 xwzw(const dvec4 &v) { return {v.x,v.w,v.z,v.w}; }
UNIGINE_INLINE ivec4 xwzw(const ivec4 &v) { return {v.x,v.w,v.z,v.w}; }
UNIGINE_INLINE  vec4 xwwx(const  vec4 &v) { return {v.x,v.w,v.w,v.x}; }
UNIGINE_INLINE dvec4 xwwx(const dvec4 &v) { return {v.x,v.w,v.w,v.x}; }
UNIGINE_INLINE ivec4 xwwx(const ivec4 &v) { return {v.x,v.w,v.w,v.x}; }
UNIGINE_INLINE  vec4 xwwy(const  vec4 &v) { return {v.x,v.w,v.w,v.y}; }
UNIGINE_INLINE dvec4 xwwy(const dvec4 &v) { return {v.x,v.w,v.w,v.y}; }
UNIGINE_INLINE ivec4 xwwy(const ivec4 &v) { return {v.x,v.w,v.w,v.y}; }
UNIGINE_INLINE  vec4 xwwz(const  vec4 &v) { return {v.x,v.w,v.w,v.z}; }
UNIGINE_INLINE dvec4 xwwz(const dvec4 &v) { return {v.x,v.w,v.w,v.z}; }
UNIGINE_INLINE ivec4 xwwz(const ivec4 &v) { return {v.x,v.w,v.w,v.z}; }
UNIGINE_INLINE  vec4 xwww(const  vec4 &v) { return {v.x,v.w,v.w,v.w}; }
UNIGINE_INLINE dvec4 xwww(const dvec4 &v) { return {v.x,v.w,v.w,v.w}; }
UNIGINE_INLINE ivec4 xwww(const ivec4 &v) { return {v.x,v.w,v.w,v.w}; }
UNIGINE_INLINE  vec4 yxxx(const  vec2 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 yxxx(const  vec3 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 yxxx(const  vec4 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 yxxx(const dvec2 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 yxxx(const dvec3 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 yxxx(const dvec4 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 yxxx(const ivec2 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 yxxx(const ivec3 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 yxxx(const ivec4 &v) { return {v.y,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 yxxy(const  vec2 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 yxxy(const  vec3 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 yxxy(const  vec4 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 yxxy(const dvec2 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 yxxy(const dvec3 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 yxxy(const dvec4 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 yxxy(const ivec2 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 yxxy(const ivec3 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 yxxy(const ivec4 &v) { return {v.y,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 yxxz(const  vec3 &v) { return {v.y,v.x,v.x,v.z}; }
UNIGINE_INLINE  vec4 yxxz(const  vec4 &v) { return {v.y,v.x,v.x,v.z}; }
UNIGINE_INLINE dvec4 yxxz(const dvec3 &v) { return {v.y,v.x,v.x,v.z}; }
UNIGINE_INLINE dvec4 yxxz(const dvec4 &v) { return {v.y,v.x,v.x,v.z}; }
UNIGINE_INLINE ivec4 yxxz(const ivec3 &v) { return {v.y,v.x,v.x,v.z}; }
UNIGINE_INLINE ivec4 yxxz(const ivec4 &v) { return {v.y,v.x,v.x,v.z}; }
UNIGINE_INLINE  vec4 yxxw(const  vec4 &v) { return {v.y,v.x,v.x,v.w}; }
UNIGINE_INLINE dvec4 yxxw(const dvec4 &v) { return {v.y,v.x,v.x,v.w}; }
UNIGINE_INLINE ivec4 yxxw(const ivec4 &v) { return {v.y,v.x,v.x,v.w}; }
UNIGINE_INLINE  vec4 yxyx(const  vec2 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 yxyx(const  vec3 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 yxyx(const  vec4 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 yxyx(const dvec2 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 yxyx(const dvec3 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 yxyx(const dvec4 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 yxyx(const ivec2 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 yxyx(const ivec3 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 yxyx(const ivec4 &v) { return {v.y,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 yxyy(const  vec2 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 yxyy(const  vec3 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 yxyy(const  vec4 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 yxyy(const dvec2 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 yxyy(const dvec3 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 yxyy(const dvec4 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 yxyy(const ivec2 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 yxyy(const ivec3 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 yxyy(const ivec4 &v) { return {v.y,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 yxyz(const  vec3 &v) { return {v.y,v.x,v.y,v.z}; }
UNIGINE_INLINE  vec4 yxyz(const  vec4 &v) { return {v.y,v.x,v.y,v.z}; }
UNIGINE_INLINE dvec4 yxyz(const dvec3 &v) { return {v.y,v.x,v.y,v.z}; }
UNIGINE_INLINE dvec4 yxyz(const dvec4 &v) { return {v.y,v.x,v.y,v.z}; }
UNIGINE_INLINE ivec4 yxyz(const ivec3 &v) { return {v.y,v.x,v.y,v.z}; }
UNIGINE_INLINE ivec4 yxyz(const ivec4 &v) { return {v.y,v.x,v.y,v.z}; }
UNIGINE_INLINE  vec4 yxyw(const  vec4 &v) { return {v.y,v.x,v.y,v.w}; }
UNIGINE_INLINE dvec4 yxyw(const dvec4 &v) { return {v.y,v.x,v.y,v.w}; }
UNIGINE_INLINE ivec4 yxyw(const ivec4 &v) { return {v.y,v.x,v.y,v.w}; }
UNIGINE_INLINE  vec4 yxzx(const  vec3 &v) { return {v.y,v.x,v.z,v.x}; }
UNIGINE_INLINE  vec4 yxzx(const  vec4 &v) { return {v.y,v.x,v.z,v.x}; }
UNIGINE_INLINE dvec4 yxzx(const dvec3 &v) { return {v.y,v.x,v.z,v.x}; }
UNIGINE_INLINE dvec4 yxzx(const dvec4 &v) { return {v.y,v.x,v.z,v.x}; }
UNIGINE_INLINE ivec4 yxzx(const ivec3 &v) { return {v.y,v.x,v.z,v.x}; }
UNIGINE_INLINE ivec4 yxzx(const ivec4 &v) { return {v.y,v.x,v.z,v.x}; }
UNIGINE_INLINE  vec4 yxzy(const  vec3 &v) { return {v.y,v.x,v.z,v.y}; }
UNIGINE_INLINE  vec4 yxzy(const  vec4 &v) { return {v.y,v.x,v.z,v.y}; }
UNIGINE_INLINE dvec4 yxzy(const dvec3 &v) { return {v.y,v.x,v.z,v.y}; }
UNIGINE_INLINE dvec4 yxzy(const dvec4 &v) { return {v.y,v.x,v.z,v.y}; }
UNIGINE_INLINE ivec4 yxzy(const ivec3 &v) { return {v.y,v.x,v.z,v.y}; }
UNIGINE_INLINE ivec4 yxzy(const ivec4 &v) { return {v.y,v.x,v.z,v.y}; }
UNIGINE_INLINE  vec4 yxzz(const  vec3 &v) { return {v.y,v.x,v.z,v.z}; }
UNIGINE_INLINE  vec4 yxzz(const  vec4 &v) { return {v.y,v.x,v.z,v.z}; }
UNIGINE_INLINE dvec4 yxzz(const dvec3 &v) { return {v.y,v.x,v.z,v.z}; }
UNIGINE_INLINE dvec4 yxzz(const dvec4 &v) { return {v.y,v.x,v.z,v.z}; }
UNIGINE_INLINE ivec4 yxzz(const ivec3 &v) { return {v.y,v.x,v.z,v.z}; }
UNIGINE_INLINE ivec4 yxzz(const ivec4 &v) { return {v.y,v.x,v.z,v.z}; }
UNIGINE_INLINE  vec4 yxzw(const  vec4 &v) { return {v.y,v.x,v.z,v.w}; }
UNIGINE_INLINE dvec4 yxzw(const dvec4 &v) { return {v.y,v.x,v.z,v.w}; }
UNIGINE_INLINE ivec4 yxzw(const ivec4 &v) { return {v.y,v.x,v.z,v.w}; }
UNIGINE_INLINE  vec4 yxwx(const  vec4 &v) { return {v.y,v.x,v.w,v.x}; }
UNIGINE_INLINE dvec4 yxwx(const dvec4 &v) { return {v.y,v.x,v.w,v.x}; }
UNIGINE_INLINE ivec4 yxwx(const ivec4 &v) { return {v.y,v.x,v.w,v.x}; }
UNIGINE_INLINE  vec4 yxwy(const  vec4 &v) { return {v.y,v.x,v.w,v.y}; }
UNIGINE_INLINE dvec4 yxwy(const dvec4 &v) { return {v.y,v.x,v.w,v.y}; }
UNIGINE_INLINE ivec4 yxwy(const ivec4 &v) { return {v.y,v.x,v.w,v.y}; }
UNIGINE_INLINE  vec4 yxwz(const  vec4 &v) { return {v.y,v.x,v.w,v.z}; }
UNIGINE_INLINE dvec4 yxwz(const dvec4 &v) { return {v.y,v.x,v.w,v.z}; }
UNIGINE_INLINE ivec4 yxwz(const ivec4 &v) { return {v.y,v.x,v.w,v.z}; }
UNIGINE_INLINE  vec4 yxww(const  vec4 &v) { return {v.y,v.x,v.w,v.w}; }
UNIGINE_INLINE dvec4 yxww(const dvec4 &v) { return {v.y,v.x,v.w,v.w}; }
UNIGINE_INLINE ivec4 yxww(const ivec4 &v) { return {v.y,v.x,v.w,v.w}; }
UNIGINE_INLINE  vec4 yyxx(const  vec2 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 yyxx(const  vec3 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 yyxx(const  vec4 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 yyxx(const dvec2 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 yyxx(const dvec3 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 yyxx(const dvec4 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 yyxx(const ivec2 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 yyxx(const ivec3 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 yyxx(const ivec4 &v) { return {v.y,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 yyxy(const  vec2 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 yyxy(const  vec3 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 yyxy(const  vec4 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 yyxy(const dvec2 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 yyxy(const dvec3 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 yyxy(const dvec4 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 yyxy(const ivec2 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 yyxy(const ivec3 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 yyxy(const ivec4 &v) { return {v.y,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 yyxz(const  vec3 &v) { return {v.y,v.y,v.x,v.z}; }
UNIGINE_INLINE  vec4 yyxz(const  vec4 &v) { return {v.y,v.y,v.x,v.z}; }
UNIGINE_INLINE dvec4 yyxz(const dvec3 &v) { return {v.y,v.y,v.x,v.z}; }
UNIGINE_INLINE dvec4 yyxz(const dvec4 &v) { return {v.y,v.y,v.x,v.z}; }
UNIGINE_INLINE ivec4 yyxz(const ivec3 &v) { return {v.y,v.y,v.x,v.z}; }
UNIGINE_INLINE ivec4 yyxz(const ivec4 &v) { return {v.y,v.y,v.x,v.z}; }
UNIGINE_INLINE  vec4 yyxw(const  vec4 &v) { return {v.y,v.y,v.x,v.w}; }
UNIGINE_INLINE dvec4 yyxw(const dvec4 &v) { return {v.y,v.y,v.x,v.w}; }
UNIGINE_INLINE ivec4 yyxw(const ivec4 &v) { return {v.y,v.y,v.x,v.w}; }
UNIGINE_INLINE  vec4 yyyx(const  vec2 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 yyyx(const  vec3 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 yyyx(const  vec4 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 yyyx(const dvec2 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 yyyx(const dvec3 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 yyyx(const dvec4 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 yyyx(const ivec2 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 yyyx(const ivec3 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 yyyx(const ivec4 &v) { return {v.y,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 yyyy(const  vec2 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 yyyy(const  vec3 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 yyyy(const  vec4 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 yyyy(const dvec2 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 yyyy(const dvec3 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 yyyy(const dvec4 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 yyyy(const ivec2 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 yyyy(const ivec3 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 yyyy(const ivec4 &v) { return {v.y,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 yyyz(const  vec3 &v) { return {v.y,v.y,v.y,v.z}; }
UNIGINE_INLINE  vec4 yyyz(const  vec4 &v) { return {v.y,v.y,v.y,v.z}; }
UNIGINE_INLINE dvec4 yyyz(const dvec3 &v) { return {v.y,v.y,v.y,v.z}; }
UNIGINE_INLINE dvec4 yyyz(const dvec4 &v) { return {v.y,v.y,v.y,v.z}; }
UNIGINE_INLINE ivec4 yyyz(const ivec3 &v) { return {v.y,v.y,v.y,v.z}; }
UNIGINE_INLINE ivec4 yyyz(const ivec4 &v) { return {v.y,v.y,v.y,v.z}; }
UNIGINE_INLINE  vec4 yyyw(const  vec4 &v) { return {v.y,v.y,v.y,v.w}; }
UNIGINE_INLINE dvec4 yyyw(const dvec4 &v) { return {v.y,v.y,v.y,v.w}; }
UNIGINE_INLINE ivec4 yyyw(const ivec4 &v) { return {v.y,v.y,v.y,v.w}; }
UNIGINE_INLINE  vec4 yyzx(const  vec3 &v) { return {v.y,v.y,v.z,v.x}; }
UNIGINE_INLINE  vec4 yyzx(const  vec4 &v) { return {v.y,v.y,v.z,v.x}; }
UNIGINE_INLINE dvec4 yyzx(const dvec3 &v) { return {v.y,v.y,v.z,v.x}; }
UNIGINE_INLINE dvec4 yyzx(const dvec4 &v) { return {v.y,v.y,v.z,v.x}; }
UNIGINE_INLINE ivec4 yyzx(const ivec3 &v) { return {v.y,v.y,v.z,v.x}; }
UNIGINE_INLINE ivec4 yyzx(const ivec4 &v) { return {v.y,v.y,v.z,v.x}; }
UNIGINE_INLINE  vec4 yyzy(const  vec3 &v) { return {v.y,v.y,v.z,v.y}; }
UNIGINE_INLINE  vec4 yyzy(const  vec4 &v) { return {v.y,v.y,v.z,v.y}; }
UNIGINE_INLINE dvec4 yyzy(const dvec3 &v) { return {v.y,v.y,v.z,v.y}; }
UNIGINE_INLINE dvec4 yyzy(const dvec4 &v) { return {v.y,v.y,v.z,v.y}; }
UNIGINE_INLINE ivec4 yyzy(const ivec3 &v) { return {v.y,v.y,v.z,v.y}; }
UNIGINE_INLINE ivec4 yyzy(const ivec4 &v) { return {v.y,v.y,v.z,v.y}; }
UNIGINE_INLINE  vec4 yyzz(const  vec3 &v) { return {v.y,v.y,v.z,v.z}; }
UNIGINE_INLINE  vec4 yyzz(const  vec4 &v) { return {v.y,v.y,v.z,v.z}; }
UNIGINE_INLINE dvec4 yyzz(const dvec3 &v) { return {v.y,v.y,v.z,v.z}; }
UNIGINE_INLINE dvec4 yyzz(const dvec4 &v) { return {v.y,v.y,v.z,v.z}; }
UNIGINE_INLINE ivec4 yyzz(const ivec3 &v) { return {v.y,v.y,v.z,v.z}; }
UNIGINE_INLINE ivec4 yyzz(const ivec4 &v) { return {v.y,v.y,v.z,v.z}; }
UNIGINE_INLINE  vec4 yyzw(const  vec4 &v) { return {v.y,v.y,v.z,v.w}; }
UNIGINE_INLINE dvec4 yyzw(const dvec4 &v) { return {v.y,v.y,v.z,v.w}; }
UNIGINE_INLINE ivec4 yyzw(const ivec4 &v) { return {v.y,v.y,v.z,v.w}; }
UNIGINE_INLINE  vec4 yywx(const  vec4 &v) { return {v.y,v.y,v.w,v.x}; }
UNIGINE_INLINE dvec4 yywx(const dvec4 &v) { return {v.y,v.y,v.w,v.x}; }
UNIGINE_INLINE ivec4 yywx(const ivec4 &v) { return {v.y,v.y,v.w,v.x}; }
UNIGINE_INLINE  vec4 yywy(const  vec4 &v) { return {v.y,v.y,v.w,v.y}; }
UNIGINE_INLINE dvec4 yywy(const dvec4 &v) { return {v.y,v.y,v.w,v.y}; }
UNIGINE_INLINE ivec4 yywy(const ivec4 &v) { return {v.y,v.y,v.w,v.y}; }
UNIGINE_INLINE  vec4 yywz(const  vec4 &v) { return {v.y,v.y,v.w,v.z}; }
UNIGINE_INLINE dvec4 yywz(const dvec4 &v) { return {v.y,v.y,v.w,v.z}; }
UNIGINE_INLINE ivec4 yywz(const ivec4 &v) { return {v.y,v.y,v.w,v.z}; }
UNIGINE_INLINE  vec4 yyww(const  vec4 &v) { return {v.y,v.y,v.w,v.w}; }
UNIGINE_INLINE dvec4 yyww(const dvec4 &v) { return {v.y,v.y,v.w,v.w}; }
UNIGINE_INLINE ivec4 yyww(const ivec4 &v) { return {v.y,v.y,v.w,v.w}; }
UNIGINE_INLINE  vec4 yzxx(const  vec3 &v) { return {v.y,v.z,v.x,v.x}; }
UNIGINE_INLINE  vec4 yzxx(const  vec4 &v) { return {v.y,v.z,v.x,v.x}; }
UNIGINE_INLINE dvec4 yzxx(const dvec3 &v) { return {v.y,v.z,v.x,v.x}; }
UNIGINE_INLINE dvec4 yzxx(const dvec4 &v) { return {v.y,v.z,v.x,v.x}; }
UNIGINE_INLINE ivec4 yzxx(const ivec3 &v) { return {v.y,v.z,v.x,v.x}; }
UNIGINE_INLINE ivec4 yzxx(const ivec4 &v) { return {v.y,v.z,v.x,v.x}; }
UNIGINE_INLINE  vec4 yzxy(const  vec3 &v) { return {v.y,v.z,v.x,v.y}; }
UNIGINE_INLINE  vec4 yzxy(const  vec4 &v) { return {v.y,v.z,v.x,v.y}; }
UNIGINE_INLINE dvec4 yzxy(const dvec3 &v) { return {v.y,v.z,v.x,v.y}; }
UNIGINE_INLINE dvec4 yzxy(const dvec4 &v) { return {v.y,v.z,v.x,v.y}; }
UNIGINE_INLINE ivec4 yzxy(const ivec3 &v) { return {v.y,v.z,v.x,v.y}; }
UNIGINE_INLINE ivec4 yzxy(const ivec4 &v) { return {v.y,v.z,v.x,v.y}; }
UNIGINE_INLINE  vec4 yzxz(const  vec3 &v) { return {v.y,v.z,v.x,v.z}; }
UNIGINE_INLINE  vec4 yzxz(const  vec4 &v) { return {v.y,v.z,v.x,v.z}; }
UNIGINE_INLINE dvec4 yzxz(const dvec3 &v) { return {v.y,v.z,v.x,v.z}; }
UNIGINE_INLINE dvec4 yzxz(const dvec4 &v) { return {v.y,v.z,v.x,v.z}; }
UNIGINE_INLINE ivec4 yzxz(const ivec3 &v) { return {v.y,v.z,v.x,v.z}; }
UNIGINE_INLINE ivec4 yzxz(const ivec4 &v) { return {v.y,v.z,v.x,v.z}; }
UNIGINE_INLINE  vec4 yzxw(const  vec4 &v) { return {v.y,v.z,v.x,v.w}; }
UNIGINE_INLINE dvec4 yzxw(const dvec4 &v) { return {v.y,v.z,v.x,v.w}; }
UNIGINE_INLINE ivec4 yzxw(const ivec4 &v) { return {v.y,v.z,v.x,v.w}; }
UNIGINE_INLINE  vec4 yzyx(const  vec3 &v) { return {v.y,v.z,v.y,v.x}; }
UNIGINE_INLINE  vec4 yzyx(const  vec4 &v) { return {v.y,v.z,v.y,v.x}; }
UNIGINE_INLINE dvec4 yzyx(const dvec3 &v) { return {v.y,v.z,v.y,v.x}; }
UNIGINE_INLINE dvec4 yzyx(const dvec4 &v) { return {v.y,v.z,v.y,v.x}; }
UNIGINE_INLINE ivec4 yzyx(const ivec3 &v) { return {v.y,v.z,v.y,v.x}; }
UNIGINE_INLINE ivec4 yzyx(const ivec4 &v) { return {v.y,v.z,v.y,v.x}; }
UNIGINE_INLINE  vec4 yzyy(const  vec3 &v) { return {v.y,v.z,v.y,v.y}; }
UNIGINE_INLINE  vec4 yzyy(const  vec4 &v) { return {v.y,v.z,v.y,v.y}; }
UNIGINE_INLINE dvec4 yzyy(const dvec3 &v) { return {v.y,v.z,v.y,v.y}; }
UNIGINE_INLINE dvec4 yzyy(const dvec4 &v) { return {v.y,v.z,v.y,v.y}; }
UNIGINE_INLINE ivec4 yzyy(const ivec3 &v) { return {v.y,v.z,v.y,v.y}; }
UNIGINE_INLINE ivec4 yzyy(const ivec4 &v) { return {v.y,v.z,v.y,v.y}; }
UNIGINE_INLINE  vec4 yzyz(const  vec3 &v) { return {v.y,v.z,v.y,v.z}; }
UNIGINE_INLINE  vec4 yzyz(const  vec4 &v) { return {v.y,v.z,v.y,v.z}; }
UNIGINE_INLINE dvec4 yzyz(const dvec3 &v) { return {v.y,v.z,v.y,v.z}; }
UNIGINE_INLINE dvec4 yzyz(const dvec4 &v) { return {v.y,v.z,v.y,v.z}; }
UNIGINE_INLINE ivec4 yzyz(const ivec3 &v) { return {v.y,v.z,v.y,v.z}; }
UNIGINE_INLINE ivec4 yzyz(const ivec4 &v) { return {v.y,v.z,v.y,v.z}; }
UNIGINE_INLINE  vec4 yzyw(const  vec4 &v) { return {v.y,v.z,v.y,v.w}; }
UNIGINE_INLINE dvec4 yzyw(const dvec4 &v) { return {v.y,v.z,v.y,v.w}; }
UNIGINE_INLINE ivec4 yzyw(const ivec4 &v) { return {v.y,v.z,v.y,v.w}; }
UNIGINE_INLINE  vec4 yzzx(const  vec3 &v) { return {v.y,v.z,v.z,v.x}; }
UNIGINE_INLINE  vec4 yzzx(const  vec4 &v) { return {v.y,v.z,v.z,v.x}; }
UNIGINE_INLINE dvec4 yzzx(const dvec3 &v) { return {v.y,v.z,v.z,v.x}; }
UNIGINE_INLINE dvec4 yzzx(const dvec4 &v) { return {v.y,v.z,v.z,v.x}; }
UNIGINE_INLINE ivec4 yzzx(const ivec3 &v) { return {v.y,v.z,v.z,v.x}; }
UNIGINE_INLINE ivec4 yzzx(const ivec4 &v) { return {v.y,v.z,v.z,v.x}; }
UNIGINE_INLINE  vec4 yzzy(const  vec3 &v) { return {v.y,v.z,v.z,v.y}; }
UNIGINE_INLINE  vec4 yzzy(const  vec4 &v) { return {v.y,v.z,v.z,v.y}; }
UNIGINE_INLINE dvec4 yzzy(const dvec3 &v) { return {v.y,v.z,v.z,v.y}; }
UNIGINE_INLINE dvec4 yzzy(const dvec4 &v) { return {v.y,v.z,v.z,v.y}; }
UNIGINE_INLINE ivec4 yzzy(const ivec3 &v) { return {v.y,v.z,v.z,v.y}; }
UNIGINE_INLINE ivec4 yzzy(const ivec4 &v) { return {v.y,v.z,v.z,v.y}; }
UNIGINE_INLINE  vec4 yzzz(const  vec3 &v) { return {v.y,v.z,v.z,v.z}; }
UNIGINE_INLINE  vec4 yzzz(const  vec4 &v) { return {v.y,v.z,v.z,v.z}; }
UNIGINE_INLINE dvec4 yzzz(const dvec3 &v) { return {v.y,v.z,v.z,v.z}; }
UNIGINE_INLINE dvec4 yzzz(const dvec4 &v) { return {v.y,v.z,v.z,v.z}; }
UNIGINE_INLINE ivec4 yzzz(const ivec3 &v) { return {v.y,v.z,v.z,v.z}; }
UNIGINE_INLINE ivec4 yzzz(const ivec4 &v) { return {v.y,v.z,v.z,v.z}; }
UNIGINE_INLINE  vec4 yzzw(const  vec4 &v) { return {v.y,v.z,v.z,v.w}; }
UNIGINE_INLINE dvec4 yzzw(const dvec4 &v) { return {v.y,v.z,v.z,v.w}; }
UNIGINE_INLINE ivec4 yzzw(const ivec4 &v) { return {v.y,v.z,v.z,v.w}; }
UNIGINE_INLINE  vec4 yzwx(const  vec4 &v) { return {v.y,v.z,v.w,v.x}; }
UNIGINE_INLINE dvec4 yzwx(const dvec4 &v) { return {v.y,v.z,v.w,v.x}; }
UNIGINE_INLINE ivec4 yzwx(const ivec4 &v) { return {v.y,v.z,v.w,v.x}; }
UNIGINE_INLINE  vec4 yzwy(const  vec4 &v) { return {v.y,v.z,v.w,v.y}; }
UNIGINE_INLINE dvec4 yzwy(const dvec4 &v) { return {v.y,v.z,v.w,v.y}; }
UNIGINE_INLINE ivec4 yzwy(const ivec4 &v) { return {v.y,v.z,v.w,v.y}; }
UNIGINE_INLINE  vec4 yzwz(const  vec4 &v) { return {v.y,v.z,v.w,v.z}; }
UNIGINE_INLINE dvec4 yzwz(const dvec4 &v) { return {v.y,v.z,v.w,v.z}; }
UNIGINE_INLINE ivec4 yzwz(const ivec4 &v) { return {v.y,v.z,v.w,v.z}; }
UNIGINE_INLINE  vec4 yzww(const  vec4 &v) { return {v.y,v.z,v.w,v.w}; }
UNIGINE_INLINE dvec4 yzww(const dvec4 &v) { return {v.y,v.z,v.w,v.w}; }
UNIGINE_INLINE ivec4 yzww(const ivec4 &v) { return {v.y,v.z,v.w,v.w}; }
UNIGINE_INLINE  vec4 ywxx(const  vec4 &v) { return {v.y,v.w,v.x,v.x}; }
UNIGINE_INLINE dvec4 ywxx(const dvec4 &v) { return {v.y,v.w,v.x,v.x}; }
UNIGINE_INLINE ivec4 ywxx(const ivec4 &v) { return {v.y,v.w,v.x,v.x}; }
UNIGINE_INLINE  vec4 ywxy(const  vec4 &v) { return {v.y,v.w,v.x,v.y}; }
UNIGINE_INLINE dvec4 ywxy(const dvec4 &v) { return {v.y,v.w,v.x,v.y}; }
UNIGINE_INLINE ivec4 ywxy(const ivec4 &v) { return {v.y,v.w,v.x,v.y}; }
UNIGINE_INLINE  vec4 ywxz(const  vec4 &v) { return {v.y,v.w,v.x,v.z}; }
UNIGINE_INLINE dvec4 ywxz(const dvec4 &v) { return {v.y,v.w,v.x,v.z}; }
UNIGINE_INLINE ivec4 ywxz(const ivec4 &v) { return {v.y,v.w,v.x,v.z}; }
UNIGINE_INLINE  vec4 ywxw(const  vec4 &v) { return {v.y,v.w,v.x,v.w}; }
UNIGINE_INLINE dvec4 ywxw(const dvec4 &v) { return {v.y,v.w,v.x,v.w}; }
UNIGINE_INLINE ivec4 ywxw(const ivec4 &v) { return {v.y,v.w,v.x,v.w}; }
UNIGINE_INLINE  vec4 ywyx(const  vec4 &v) { return {v.y,v.w,v.y,v.x}; }
UNIGINE_INLINE dvec4 ywyx(const dvec4 &v) { return {v.y,v.w,v.y,v.x}; }
UNIGINE_INLINE ivec4 ywyx(const ivec4 &v) { return {v.y,v.w,v.y,v.x}; }
UNIGINE_INLINE  vec4 ywyy(const  vec4 &v) { return {v.y,v.w,v.y,v.y}; }
UNIGINE_INLINE dvec4 ywyy(const dvec4 &v) { return {v.y,v.w,v.y,v.y}; }
UNIGINE_INLINE ivec4 ywyy(const ivec4 &v) { return {v.y,v.w,v.y,v.y}; }
UNIGINE_INLINE  vec4 ywyz(const  vec4 &v) { return {v.y,v.w,v.y,v.z}; }
UNIGINE_INLINE dvec4 ywyz(const dvec4 &v) { return {v.y,v.w,v.y,v.z}; }
UNIGINE_INLINE ivec4 ywyz(const ivec4 &v) { return {v.y,v.w,v.y,v.z}; }
UNIGINE_INLINE  vec4 ywyw(const  vec4 &v) { return {v.y,v.w,v.y,v.w}; }
UNIGINE_INLINE dvec4 ywyw(const dvec4 &v) { return {v.y,v.w,v.y,v.w}; }
UNIGINE_INLINE ivec4 ywyw(const ivec4 &v) { return {v.y,v.w,v.y,v.w}; }
UNIGINE_INLINE  vec4 ywzx(const  vec4 &v) { return {v.y,v.w,v.z,v.x}; }
UNIGINE_INLINE dvec4 ywzx(const dvec4 &v) { return {v.y,v.w,v.z,v.x}; }
UNIGINE_INLINE ivec4 ywzx(const ivec4 &v) { return {v.y,v.w,v.z,v.x}; }
UNIGINE_INLINE  vec4 ywzy(const  vec4 &v) { return {v.y,v.w,v.z,v.y}; }
UNIGINE_INLINE dvec4 ywzy(const dvec4 &v) { return {v.y,v.w,v.z,v.y}; }
UNIGINE_INLINE ivec4 ywzy(const ivec4 &v) { return {v.y,v.w,v.z,v.y}; }
UNIGINE_INLINE  vec4 ywzz(const  vec4 &v) { return {v.y,v.w,v.z,v.z}; }
UNIGINE_INLINE dvec4 ywzz(const dvec4 &v) { return {v.y,v.w,v.z,v.z}; }
UNIGINE_INLINE ivec4 ywzz(const ivec4 &v) { return {v.y,v.w,v.z,v.z}; }
UNIGINE_INLINE  vec4 ywzw(const  vec4 &v) { return {v.y,v.w,v.z,v.w}; }
UNIGINE_INLINE dvec4 ywzw(const dvec4 &v) { return {v.y,v.w,v.z,v.w}; }
UNIGINE_INLINE ivec4 ywzw(const ivec4 &v) { return {v.y,v.w,v.z,v.w}; }
UNIGINE_INLINE  vec4 ywwx(const  vec4 &v) { return {v.y,v.w,v.w,v.x}; }
UNIGINE_INLINE dvec4 ywwx(const dvec4 &v) { return {v.y,v.w,v.w,v.x}; }
UNIGINE_INLINE ivec4 ywwx(const ivec4 &v) { return {v.y,v.w,v.w,v.x}; }
UNIGINE_INLINE  vec4 ywwy(const  vec4 &v) { return {v.y,v.w,v.w,v.y}; }
UNIGINE_INLINE dvec4 ywwy(const dvec4 &v) { return {v.y,v.w,v.w,v.y}; }
UNIGINE_INLINE ivec4 ywwy(const ivec4 &v) { return {v.y,v.w,v.w,v.y}; }
UNIGINE_INLINE  vec4 ywwz(const  vec4 &v) { return {v.y,v.w,v.w,v.z}; }
UNIGINE_INLINE dvec4 ywwz(const dvec4 &v) { return {v.y,v.w,v.w,v.z}; }
UNIGINE_INLINE ivec4 ywwz(const ivec4 &v) { return {v.y,v.w,v.w,v.z}; }
UNIGINE_INLINE  vec4 ywww(const  vec4 &v) { return {v.y,v.w,v.w,v.w}; }
UNIGINE_INLINE dvec4 ywww(const dvec4 &v) { return {v.y,v.w,v.w,v.w}; }
UNIGINE_INLINE ivec4 ywww(const ivec4 &v) { return {v.y,v.w,v.w,v.w}; }
UNIGINE_INLINE  vec4 zxxx(const  vec3 &v) { return {v.z,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 zxxx(const  vec4 &v) { return {v.z,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 zxxx(const dvec3 &v) { return {v.z,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 zxxx(const dvec4 &v) { return {v.z,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 zxxx(const ivec3 &v) { return {v.z,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 zxxx(const ivec4 &v) { return {v.z,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 zxxy(const  vec3 &v) { return {v.z,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 zxxy(const  vec4 &v) { return {v.z,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 zxxy(const dvec3 &v) { return {v.z,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 zxxy(const dvec4 &v) { return {v.z,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 zxxy(const ivec3 &v) { return {v.z,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 zxxy(const ivec4 &v) { return {v.z,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 zxxz(const  vec3 &v) { return {v.z,v.x,v.x,v.z}; }
UNIGINE_INLINE  vec4 zxxz(const  vec4 &v) { return {v.z,v.x,v.x,v.z}; }
UNIGINE_INLINE dvec4 zxxz(const dvec3 &v) { return {v.z,v.x,v.x,v.z}; }
UNIGINE_INLINE dvec4 zxxz(const dvec4 &v) { return {v.z,v.x,v.x,v.z}; }
UNIGINE_INLINE ivec4 zxxz(const ivec3 &v) { return {v.z,v.x,v.x,v.z}; }
UNIGINE_INLINE ivec4 zxxz(const ivec4 &v) { return {v.z,v.x,v.x,v.z}; }
UNIGINE_INLINE  vec4 zxxw(const  vec4 &v) { return {v.z,v.x,v.x,v.w}; }
UNIGINE_INLINE dvec4 zxxw(const dvec4 &v) { return {v.z,v.x,v.x,v.w}; }
UNIGINE_INLINE ivec4 zxxw(const ivec4 &v) { return {v.z,v.x,v.x,v.w}; }
UNIGINE_INLINE  vec4 zxyx(const  vec3 &v) { return {v.z,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 zxyx(const  vec4 &v) { return {v.z,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 zxyx(const dvec3 &v) { return {v.z,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 zxyx(const dvec4 &v) { return {v.z,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 zxyx(const ivec3 &v) { return {v.z,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 zxyx(const ivec4 &v) { return {v.z,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 zxyy(const  vec3 &v) { return {v.z,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 zxyy(const  vec4 &v) { return {v.z,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 zxyy(const dvec3 &v) { return {v.z,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 zxyy(const dvec4 &v) { return {v.z,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 zxyy(const ivec3 &v) { return {v.z,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 zxyy(const ivec4 &v) { return {v.z,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 zxyz(const  vec3 &v) { return {v.z,v.x,v.y,v.z}; }
UNIGINE_INLINE  vec4 zxyz(const  vec4 &v) { return {v.z,v.x,v.y,v.z}; }
UNIGINE_INLINE dvec4 zxyz(const dvec3 &v) { return {v.z,v.x,v.y,v.z}; }
UNIGINE_INLINE dvec4 zxyz(const dvec4 &v) { return {v.z,v.x,v.y,v.z}; }
UNIGINE_INLINE ivec4 zxyz(const ivec3 &v) { return {v.z,v.x,v.y,v.z}; }
UNIGINE_INLINE ivec4 zxyz(const ivec4 &v) { return {v.z,v.x,v.y,v.z}; }
UNIGINE_INLINE  vec4 zxyw(const  vec4 &v) { return {v.z,v.x,v.y,v.w}; }
UNIGINE_INLINE dvec4 zxyw(const dvec4 &v) { return {v.z,v.x,v.y,v.w}; }
UNIGINE_INLINE ivec4 zxyw(const ivec4 &v) { return {v.z,v.x,v.y,v.w}; }
UNIGINE_INLINE  vec4 zxzx(const  vec3 &v) { return {v.z,v.x,v.z,v.x}; }
UNIGINE_INLINE  vec4 zxzx(const  vec4 &v) { return {v.z,v.x,v.z,v.x}; }
UNIGINE_INLINE dvec4 zxzx(const dvec3 &v) { return {v.z,v.x,v.z,v.x}; }
UNIGINE_INLINE dvec4 zxzx(const dvec4 &v) { return {v.z,v.x,v.z,v.x}; }
UNIGINE_INLINE ivec4 zxzx(const ivec3 &v) { return {v.z,v.x,v.z,v.x}; }
UNIGINE_INLINE ivec4 zxzx(const ivec4 &v) { return {v.z,v.x,v.z,v.x}; }
UNIGINE_INLINE  vec4 zxzy(const  vec3 &v) { return {v.z,v.x,v.z,v.y}; }
UNIGINE_INLINE  vec4 zxzy(const  vec4 &v) { return {v.z,v.x,v.z,v.y}; }
UNIGINE_INLINE dvec4 zxzy(const dvec3 &v) { return {v.z,v.x,v.z,v.y}; }
UNIGINE_INLINE dvec4 zxzy(const dvec4 &v) { return {v.z,v.x,v.z,v.y}; }
UNIGINE_INLINE ivec4 zxzy(const ivec3 &v) { return {v.z,v.x,v.z,v.y}; }
UNIGINE_INLINE ivec4 zxzy(const ivec4 &v) { return {v.z,v.x,v.z,v.y}; }
UNIGINE_INLINE  vec4 zxzz(const  vec3 &v) { return {v.z,v.x,v.z,v.z}; }
UNIGINE_INLINE  vec4 zxzz(const  vec4 &v) { return {v.z,v.x,v.z,v.z}; }
UNIGINE_INLINE dvec4 zxzz(const dvec3 &v) { return {v.z,v.x,v.z,v.z}; }
UNIGINE_INLINE dvec4 zxzz(const dvec4 &v) { return {v.z,v.x,v.z,v.z}; }
UNIGINE_INLINE ivec4 zxzz(const ivec3 &v) { return {v.z,v.x,v.z,v.z}; }
UNIGINE_INLINE ivec4 zxzz(const ivec4 &v) { return {v.z,v.x,v.z,v.z}; }
UNIGINE_INLINE  vec4 zxzw(const  vec4 &v) { return {v.z,v.x,v.z,v.w}; }
UNIGINE_INLINE dvec4 zxzw(const dvec4 &v) { return {v.z,v.x,v.z,v.w}; }
UNIGINE_INLINE ivec4 zxzw(const ivec4 &v) { return {v.z,v.x,v.z,v.w}; }
UNIGINE_INLINE  vec4 zxwx(const  vec4 &v) { return {v.z,v.x,v.w,v.x}; }
UNIGINE_INLINE dvec4 zxwx(const dvec4 &v) { return {v.z,v.x,v.w,v.x}; }
UNIGINE_INLINE ivec4 zxwx(const ivec4 &v) { return {v.z,v.x,v.w,v.x}; }
UNIGINE_INLINE  vec4 zxwy(const  vec4 &v) { return {v.z,v.x,v.w,v.y}; }
UNIGINE_INLINE dvec4 zxwy(const dvec4 &v) { return {v.z,v.x,v.w,v.y}; }
UNIGINE_INLINE ivec4 zxwy(const ivec4 &v) { return {v.z,v.x,v.w,v.y}; }
UNIGINE_INLINE  vec4 zxwz(const  vec4 &v) { return {v.z,v.x,v.w,v.z}; }
UNIGINE_INLINE dvec4 zxwz(const dvec4 &v) { return {v.z,v.x,v.w,v.z}; }
UNIGINE_INLINE ivec4 zxwz(const ivec4 &v) { return {v.z,v.x,v.w,v.z}; }
UNIGINE_INLINE  vec4 zxww(const  vec4 &v) { return {v.z,v.x,v.w,v.w}; }
UNIGINE_INLINE dvec4 zxww(const dvec4 &v) { return {v.z,v.x,v.w,v.w}; }
UNIGINE_INLINE ivec4 zxww(const ivec4 &v) { return {v.z,v.x,v.w,v.w}; }
UNIGINE_INLINE  vec4 zyxx(const  vec3 &v) { return {v.z,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 zyxx(const  vec4 &v) { return {v.z,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 zyxx(const dvec3 &v) { return {v.z,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 zyxx(const dvec4 &v) { return {v.z,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 zyxx(const ivec3 &v) { return {v.z,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 zyxx(const ivec4 &v) { return {v.z,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 zyxy(const  vec3 &v) { return {v.z,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 zyxy(const  vec4 &v) { return {v.z,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 zyxy(const dvec3 &v) { return {v.z,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 zyxy(const dvec4 &v) { return {v.z,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 zyxy(const ivec3 &v) { return {v.z,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 zyxy(const ivec4 &v) { return {v.z,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 zyxz(const  vec3 &v) { return {v.z,v.y,v.x,v.z}; }
UNIGINE_INLINE  vec4 zyxz(const  vec4 &v) { return {v.z,v.y,v.x,v.z}; }
UNIGINE_INLINE dvec4 zyxz(const dvec3 &v) { return {v.z,v.y,v.x,v.z}; }
UNIGINE_INLINE dvec4 zyxz(const dvec4 &v) { return {v.z,v.y,v.x,v.z}; }
UNIGINE_INLINE ivec4 zyxz(const ivec3 &v) { return {v.z,v.y,v.x,v.z}; }
UNIGINE_INLINE ivec4 zyxz(const ivec4 &v) { return {v.z,v.y,v.x,v.z}; }
UNIGINE_INLINE  vec4 zyxw(const  vec4 &v) { return {v.z,v.y,v.x,v.w}; }
UNIGINE_INLINE dvec4 zyxw(const dvec4 &v) { return {v.z,v.y,v.x,v.w}; }
UNIGINE_INLINE ivec4 zyxw(const ivec4 &v) { return {v.z,v.y,v.x,v.w}; }
UNIGINE_INLINE  vec4 zyyx(const  vec3 &v) { return {v.z,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 zyyx(const  vec4 &v) { return {v.z,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 zyyx(const dvec3 &v) { return {v.z,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 zyyx(const dvec4 &v) { return {v.z,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 zyyx(const ivec3 &v) { return {v.z,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 zyyx(const ivec4 &v) { return {v.z,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 zyyy(const  vec3 &v) { return {v.z,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 zyyy(const  vec4 &v) { return {v.z,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 zyyy(const dvec3 &v) { return {v.z,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 zyyy(const dvec4 &v) { return {v.z,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 zyyy(const ivec3 &v) { return {v.z,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 zyyy(const ivec4 &v) { return {v.z,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 zyyz(const  vec3 &v) { return {v.z,v.y,v.y,v.z}; }
UNIGINE_INLINE  vec4 zyyz(const  vec4 &v) { return {v.z,v.y,v.y,v.z}; }
UNIGINE_INLINE dvec4 zyyz(const dvec3 &v) { return {v.z,v.y,v.y,v.z}; }
UNIGINE_INLINE dvec4 zyyz(const dvec4 &v) { return {v.z,v.y,v.y,v.z}; }
UNIGINE_INLINE ivec4 zyyz(const ivec3 &v) { return {v.z,v.y,v.y,v.z}; }
UNIGINE_INLINE ivec4 zyyz(const ivec4 &v) { return {v.z,v.y,v.y,v.z}; }
UNIGINE_INLINE  vec4 zyyw(const  vec4 &v) { return {v.z,v.y,v.y,v.w}; }
UNIGINE_INLINE dvec4 zyyw(const dvec4 &v) { return {v.z,v.y,v.y,v.w}; }
UNIGINE_INLINE ivec4 zyyw(const ivec4 &v) { return {v.z,v.y,v.y,v.w}; }
UNIGINE_INLINE  vec4 zyzx(const  vec3 &v) { return {v.z,v.y,v.z,v.x}; }
UNIGINE_INLINE  vec4 zyzx(const  vec4 &v) { return {v.z,v.y,v.z,v.x}; }
UNIGINE_INLINE dvec4 zyzx(const dvec3 &v) { return {v.z,v.y,v.z,v.x}; }
UNIGINE_INLINE dvec4 zyzx(const dvec4 &v) { return {v.z,v.y,v.z,v.x}; }
UNIGINE_INLINE ivec4 zyzx(const ivec3 &v) { return {v.z,v.y,v.z,v.x}; }
UNIGINE_INLINE ivec4 zyzx(const ivec4 &v) { return {v.z,v.y,v.z,v.x}; }
UNIGINE_INLINE  vec4 zyzy(const  vec3 &v) { return {v.z,v.y,v.z,v.y}; }
UNIGINE_INLINE  vec4 zyzy(const  vec4 &v) { return {v.z,v.y,v.z,v.y}; }
UNIGINE_INLINE dvec4 zyzy(const dvec3 &v) { return {v.z,v.y,v.z,v.y}; }
UNIGINE_INLINE dvec4 zyzy(const dvec4 &v) { return {v.z,v.y,v.z,v.y}; }
UNIGINE_INLINE ivec4 zyzy(const ivec3 &v) { return {v.z,v.y,v.z,v.y}; }
UNIGINE_INLINE ivec4 zyzy(const ivec4 &v) { return {v.z,v.y,v.z,v.y}; }
UNIGINE_INLINE  vec4 zyzz(const  vec3 &v) { return {v.z,v.y,v.z,v.z}; }
UNIGINE_INLINE  vec4 zyzz(const  vec4 &v) { return {v.z,v.y,v.z,v.z}; }
UNIGINE_INLINE dvec4 zyzz(const dvec3 &v) { return {v.z,v.y,v.z,v.z}; }
UNIGINE_INLINE dvec4 zyzz(const dvec4 &v) { return {v.z,v.y,v.z,v.z}; }
UNIGINE_INLINE ivec4 zyzz(const ivec3 &v) { return {v.z,v.y,v.z,v.z}; }
UNIGINE_INLINE ivec4 zyzz(const ivec4 &v) { return {v.z,v.y,v.z,v.z}; }
UNIGINE_INLINE  vec4 zyzw(const  vec4 &v) { return {v.z,v.y,v.z,v.w}; }
UNIGINE_INLINE dvec4 zyzw(const dvec4 &v) { return {v.z,v.y,v.z,v.w}; }
UNIGINE_INLINE ivec4 zyzw(const ivec4 &v) { return {v.z,v.y,v.z,v.w}; }
UNIGINE_INLINE  vec4 zywx(const  vec4 &v) { return {v.z,v.y,v.w,v.x}; }
UNIGINE_INLINE dvec4 zywx(const dvec4 &v) { return {v.z,v.y,v.w,v.x}; }
UNIGINE_INLINE ivec4 zywx(const ivec4 &v) { return {v.z,v.y,v.w,v.x}; }
UNIGINE_INLINE  vec4 zywy(const  vec4 &v) { return {v.z,v.y,v.w,v.y}; }
UNIGINE_INLINE dvec4 zywy(const dvec4 &v) { return {v.z,v.y,v.w,v.y}; }
UNIGINE_INLINE ivec4 zywy(const ivec4 &v) { return {v.z,v.y,v.w,v.y}; }
UNIGINE_INLINE  vec4 zywz(const  vec4 &v) { return {v.z,v.y,v.w,v.z}; }
UNIGINE_INLINE dvec4 zywz(const dvec4 &v) { return {v.z,v.y,v.w,v.z}; }
UNIGINE_INLINE ivec4 zywz(const ivec4 &v) { return {v.z,v.y,v.w,v.z}; }
UNIGINE_INLINE  vec4 zyww(const  vec4 &v) { return {v.z,v.y,v.w,v.w}; }
UNIGINE_INLINE dvec4 zyww(const dvec4 &v) { return {v.z,v.y,v.w,v.w}; }
UNIGINE_INLINE ivec4 zyww(const ivec4 &v) { return {v.z,v.y,v.w,v.w}; }
UNIGINE_INLINE  vec4 zzxx(const  vec3 &v) { return {v.z,v.z,v.x,v.x}; }
UNIGINE_INLINE  vec4 zzxx(const  vec4 &v) { return {v.z,v.z,v.x,v.x}; }
UNIGINE_INLINE dvec4 zzxx(const dvec3 &v) { return {v.z,v.z,v.x,v.x}; }
UNIGINE_INLINE dvec4 zzxx(const dvec4 &v) { return {v.z,v.z,v.x,v.x}; }
UNIGINE_INLINE ivec4 zzxx(const ivec3 &v) { return {v.z,v.z,v.x,v.x}; }
UNIGINE_INLINE ivec4 zzxx(const ivec4 &v) { return {v.z,v.z,v.x,v.x}; }
UNIGINE_INLINE  vec4 zzxy(const  vec3 &v) { return {v.z,v.z,v.x,v.y}; }
UNIGINE_INLINE  vec4 zzxy(const  vec4 &v) { return {v.z,v.z,v.x,v.y}; }
UNIGINE_INLINE dvec4 zzxy(const dvec3 &v) { return {v.z,v.z,v.x,v.y}; }
UNIGINE_INLINE dvec4 zzxy(const dvec4 &v) { return {v.z,v.z,v.x,v.y}; }
UNIGINE_INLINE ivec4 zzxy(const ivec3 &v) { return {v.z,v.z,v.x,v.y}; }
UNIGINE_INLINE ivec4 zzxy(const ivec4 &v) { return {v.z,v.z,v.x,v.y}; }
UNIGINE_INLINE  vec4 zzxz(const  vec3 &v) { return {v.z,v.z,v.x,v.z}; }
UNIGINE_INLINE  vec4 zzxz(const  vec4 &v) { return {v.z,v.z,v.x,v.z}; }
UNIGINE_INLINE dvec4 zzxz(const dvec3 &v) { return {v.z,v.z,v.x,v.z}; }
UNIGINE_INLINE dvec4 zzxz(const dvec4 &v) { return {v.z,v.z,v.x,v.z}; }
UNIGINE_INLINE ivec4 zzxz(const ivec3 &v) { return {v.z,v.z,v.x,v.z}; }
UNIGINE_INLINE ivec4 zzxz(const ivec4 &v) { return {v.z,v.z,v.x,v.z}; }
UNIGINE_INLINE  vec4 zzxw(const  vec4 &v) { return {v.z,v.z,v.x,v.w}; }
UNIGINE_INLINE dvec4 zzxw(const dvec4 &v) { return {v.z,v.z,v.x,v.w}; }
UNIGINE_INLINE ivec4 zzxw(const ivec4 &v) { return {v.z,v.z,v.x,v.w}; }
UNIGINE_INLINE  vec4 zzyx(const  vec3 &v) { return {v.z,v.z,v.y,v.x}; }
UNIGINE_INLINE  vec4 zzyx(const  vec4 &v) { return {v.z,v.z,v.y,v.x}; }
UNIGINE_INLINE dvec4 zzyx(const dvec3 &v) { return {v.z,v.z,v.y,v.x}; }
UNIGINE_INLINE dvec4 zzyx(const dvec4 &v) { return {v.z,v.z,v.y,v.x}; }
UNIGINE_INLINE ivec4 zzyx(const ivec3 &v) { return {v.z,v.z,v.y,v.x}; }
UNIGINE_INLINE ivec4 zzyx(const ivec4 &v) { return {v.z,v.z,v.y,v.x}; }
UNIGINE_INLINE  vec4 zzyy(const  vec3 &v) { return {v.z,v.z,v.y,v.y}; }
UNIGINE_INLINE  vec4 zzyy(const  vec4 &v) { return {v.z,v.z,v.y,v.y}; }
UNIGINE_INLINE dvec4 zzyy(const dvec3 &v) { return {v.z,v.z,v.y,v.y}; }
UNIGINE_INLINE dvec4 zzyy(const dvec4 &v) { return {v.z,v.z,v.y,v.y}; }
UNIGINE_INLINE ivec4 zzyy(const ivec3 &v) { return {v.z,v.z,v.y,v.y}; }
UNIGINE_INLINE ivec4 zzyy(const ivec4 &v) { return {v.z,v.z,v.y,v.y}; }
UNIGINE_INLINE  vec4 zzyz(const  vec3 &v) { return {v.z,v.z,v.y,v.z}; }
UNIGINE_INLINE  vec4 zzyz(const  vec4 &v) { return {v.z,v.z,v.y,v.z}; }
UNIGINE_INLINE dvec4 zzyz(const dvec3 &v) { return {v.z,v.z,v.y,v.z}; }
UNIGINE_INLINE dvec4 zzyz(const dvec4 &v) { return {v.z,v.z,v.y,v.z}; }
UNIGINE_INLINE ivec4 zzyz(const ivec3 &v) { return {v.z,v.z,v.y,v.z}; }
UNIGINE_INLINE ivec4 zzyz(const ivec4 &v) { return {v.z,v.z,v.y,v.z}; }
UNIGINE_INLINE  vec4 zzyw(const  vec4 &v) { return {v.z,v.z,v.y,v.w}; }
UNIGINE_INLINE dvec4 zzyw(const dvec4 &v) { return {v.z,v.z,v.y,v.w}; }
UNIGINE_INLINE ivec4 zzyw(const ivec4 &v) { return {v.z,v.z,v.y,v.w}; }
UNIGINE_INLINE  vec4 zzzx(const  vec3 &v) { return {v.z,v.z,v.z,v.x}; }
UNIGINE_INLINE  vec4 zzzx(const  vec4 &v) { return {v.z,v.z,v.z,v.x}; }
UNIGINE_INLINE dvec4 zzzx(const dvec3 &v) { return {v.z,v.z,v.z,v.x}; }
UNIGINE_INLINE dvec4 zzzx(const dvec4 &v) { return {v.z,v.z,v.z,v.x}; }
UNIGINE_INLINE ivec4 zzzx(const ivec3 &v) { return {v.z,v.z,v.z,v.x}; }
UNIGINE_INLINE ivec4 zzzx(const ivec4 &v) { return {v.z,v.z,v.z,v.x}; }
UNIGINE_INLINE  vec4 zzzy(const  vec3 &v) { return {v.z,v.z,v.z,v.y}; }
UNIGINE_INLINE  vec4 zzzy(const  vec4 &v) { return {v.z,v.z,v.z,v.y}; }
UNIGINE_INLINE dvec4 zzzy(const dvec3 &v) { return {v.z,v.z,v.z,v.y}; }
UNIGINE_INLINE dvec4 zzzy(const dvec4 &v) { return {v.z,v.z,v.z,v.y}; }
UNIGINE_INLINE ivec4 zzzy(const ivec3 &v) { return {v.z,v.z,v.z,v.y}; }
UNIGINE_INLINE ivec4 zzzy(const ivec4 &v) { return {v.z,v.z,v.z,v.y}; }
UNIGINE_INLINE  vec4 zzzz(const  vec3 &v) { return {v.z,v.z,v.z,v.z}; }
UNIGINE_INLINE  vec4 zzzz(const  vec4 &v) { return {v.z,v.z,v.z,v.z}; }
UNIGINE_INLINE dvec4 zzzz(const dvec3 &v) { return {v.z,v.z,v.z,v.z}; }
UNIGINE_INLINE dvec4 zzzz(const dvec4 &v) { return {v.z,v.z,v.z,v.z}; }
UNIGINE_INLINE ivec4 zzzz(const ivec3 &v) { return {v.z,v.z,v.z,v.z}; }
UNIGINE_INLINE ivec4 zzzz(const ivec4 &v) { return {v.z,v.z,v.z,v.z}; }
UNIGINE_INLINE  vec4 zzzw(const  vec4 &v) { return {v.z,v.z,v.z,v.w}; }
UNIGINE_INLINE dvec4 zzzw(const dvec4 &v) { return {v.z,v.z,v.z,v.w}; }
UNIGINE_INLINE ivec4 zzzw(const ivec4 &v) { return {v.z,v.z,v.z,v.w}; }
UNIGINE_INLINE  vec4 zzwx(const  vec4 &v) { return {v.z,v.z,v.w,v.x}; }
UNIGINE_INLINE dvec4 zzwx(const dvec4 &v) { return {v.z,v.z,v.w,v.x}; }
UNIGINE_INLINE ivec4 zzwx(const ivec4 &v) { return {v.z,v.z,v.w,v.x}; }
UNIGINE_INLINE  vec4 zzwy(const  vec4 &v) { return {v.z,v.z,v.w,v.y}; }
UNIGINE_INLINE dvec4 zzwy(const dvec4 &v) { return {v.z,v.z,v.w,v.y}; }
UNIGINE_INLINE ivec4 zzwy(const ivec4 &v) { return {v.z,v.z,v.w,v.y}; }
UNIGINE_INLINE  vec4 zzwz(const  vec4 &v) { return {v.z,v.z,v.w,v.z}; }
UNIGINE_INLINE dvec4 zzwz(const dvec4 &v) { return {v.z,v.z,v.w,v.z}; }
UNIGINE_INLINE ivec4 zzwz(const ivec4 &v) { return {v.z,v.z,v.w,v.z}; }
UNIGINE_INLINE  vec4 zzww(const  vec4 &v) { return {v.z,v.z,v.w,v.w}; }
UNIGINE_INLINE dvec4 zzww(const dvec4 &v) { return {v.z,v.z,v.w,v.w}; }
UNIGINE_INLINE ivec4 zzww(const ivec4 &v) { return {v.z,v.z,v.w,v.w}; }
UNIGINE_INLINE  vec4 zwxx(const  vec4 &v) { return {v.z,v.w,v.x,v.x}; }
UNIGINE_INLINE dvec4 zwxx(const dvec4 &v) { return {v.z,v.w,v.x,v.x}; }
UNIGINE_INLINE ivec4 zwxx(const ivec4 &v) { return {v.z,v.w,v.x,v.x}; }
UNIGINE_INLINE  vec4 zwxy(const  vec4 &v) { return {v.z,v.w,v.x,v.y}; }
UNIGINE_INLINE dvec4 zwxy(const dvec4 &v) { return {v.z,v.w,v.x,v.y}; }
UNIGINE_INLINE ivec4 zwxy(const ivec4 &v) { return {v.z,v.w,v.x,v.y}; }
UNIGINE_INLINE  vec4 zwxz(const  vec4 &v) { return {v.z,v.w,v.x,v.z}; }
UNIGINE_INLINE dvec4 zwxz(const dvec4 &v) { return {v.z,v.w,v.x,v.z}; }
UNIGINE_INLINE ivec4 zwxz(const ivec4 &v) { return {v.z,v.w,v.x,v.z}; }
UNIGINE_INLINE  vec4 zwxw(const  vec4 &v) { return {v.z,v.w,v.x,v.w}; }
UNIGINE_INLINE dvec4 zwxw(const dvec4 &v) { return {v.z,v.w,v.x,v.w}; }
UNIGINE_INLINE ivec4 zwxw(const ivec4 &v) { return {v.z,v.w,v.x,v.w}; }
UNIGINE_INLINE  vec4 zwyx(const  vec4 &v) { return {v.z,v.w,v.y,v.x}; }
UNIGINE_INLINE dvec4 zwyx(const dvec4 &v) { return {v.z,v.w,v.y,v.x}; }
UNIGINE_INLINE ivec4 zwyx(const ivec4 &v) { return {v.z,v.w,v.y,v.x}; }
UNIGINE_INLINE  vec4 zwyy(const  vec4 &v) { return {v.z,v.w,v.y,v.y}; }
UNIGINE_INLINE dvec4 zwyy(const dvec4 &v) { return {v.z,v.w,v.y,v.y}; }
UNIGINE_INLINE ivec4 zwyy(const ivec4 &v) { return {v.z,v.w,v.y,v.y}; }
UNIGINE_INLINE  vec4 zwyz(const  vec4 &v) { return {v.z,v.w,v.y,v.z}; }
UNIGINE_INLINE dvec4 zwyz(const dvec4 &v) { return {v.z,v.w,v.y,v.z}; }
UNIGINE_INLINE ivec4 zwyz(const ivec4 &v) { return {v.z,v.w,v.y,v.z}; }
UNIGINE_INLINE  vec4 zwyw(const  vec4 &v) { return {v.z,v.w,v.y,v.w}; }
UNIGINE_INLINE dvec4 zwyw(const dvec4 &v) { return {v.z,v.w,v.y,v.w}; }
UNIGINE_INLINE ivec4 zwyw(const ivec4 &v) { return {v.z,v.w,v.y,v.w}; }
UNIGINE_INLINE  vec4 zwzx(const  vec4 &v) { return {v.z,v.w,v.z,v.x}; }
UNIGINE_INLINE dvec4 zwzx(const dvec4 &v) { return {v.z,v.w,v.z,v.x}; }
UNIGINE_INLINE ivec4 zwzx(const ivec4 &v) { return {v.z,v.w,v.z,v.x}; }
UNIGINE_INLINE  vec4 zwzy(const  vec4 &v) { return {v.z,v.w,v.z,v.y}; }
UNIGINE_INLINE dvec4 zwzy(const dvec4 &v) { return {v.z,v.w,v.z,v.y}; }
UNIGINE_INLINE ivec4 zwzy(const ivec4 &v) { return {v.z,v.w,v.z,v.y}; }
UNIGINE_INLINE  vec4 zwzz(const  vec4 &v) { return {v.z,v.w,v.z,v.z}; }
UNIGINE_INLINE dvec4 zwzz(const dvec4 &v) { return {v.z,v.w,v.z,v.z}; }
UNIGINE_INLINE ivec4 zwzz(const ivec4 &v) { return {v.z,v.w,v.z,v.z}; }
UNIGINE_INLINE  vec4 zwzw(const  vec4 &v) { return {v.z,v.w,v.z,v.w}; }
UNIGINE_INLINE dvec4 zwzw(const dvec4 &v) { return {v.z,v.w,v.z,v.w}; }
UNIGINE_INLINE ivec4 zwzw(const ivec4 &v) { return {v.z,v.w,v.z,v.w}; }
UNIGINE_INLINE  vec4 zwwx(const  vec4 &v) { return {v.z,v.w,v.w,v.x}; }
UNIGINE_INLINE dvec4 zwwx(const dvec4 &v) { return {v.z,v.w,v.w,v.x}; }
UNIGINE_INLINE ivec4 zwwx(const ivec4 &v) { return {v.z,v.w,v.w,v.x}; }
UNIGINE_INLINE  vec4 zwwy(const  vec4 &v) { return {v.z,v.w,v.w,v.y}; }
UNIGINE_INLINE dvec4 zwwy(const dvec4 &v) { return {v.z,v.w,v.w,v.y}; }
UNIGINE_INLINE ivec4 zwwy(const ivec4 &v) { return {v.z,v.w,v.w,v.y}; }
UNIGINE_INLINE  vec4 zwwz(const  vec4 &v) { return {v.z,v.w,v.w,v.z}; }
UNIGINE_INLINE dvec4 zwwz(const dvec4 &v) { return {v.z,v.w,v.w,v.z}; }
UNIGINE_INLINE ivec4 zwwz(const ivec4 &v) { return {v.z,v.w,v.w,v.z}; }
UNIGINE_INLINE  vec4 zwww(const  vec4 &v) { return {v.z,v.w,v.w,v.w}; }
UNIGINE_INLINE dvec4 zwww(const dvec4 &v) { return {v.z,v.w,v.w,v.w}; }
UNIGINE_INLINE ivec4 zwww(const ivec4 &v) { return {v.z,v.w,v.w,v.w}; }
UNIGINE_INLINE  vec4 wxxx(const  vec4 &v) { return {v.w,v.x,v.x,v.x}; }
UNIGINE_INLINE dvec4 wxxx(const dvec4 &v) { return {v.w,v.x,v.x,v.x}; }
UNIGINE_INLINE ivec4 wxxx(const ivec4 &v) { return {v.w,v.x,v.x,v.x}; }
UNIGINE_INLINE  vec4 wxxy(const  vec4 &v) { return {v.w,v.x,v.x,v.y}; }
UNIGINE_INLINE dvec4 wxxy(const dvec4 &v) { return {v.w,v.x,v.x,v.y}; }
UNIGINE_INLINE ivec4 wxxy(const ivec4 &v) { return {v.w,v.x,v.x,v.y}; }
UNIGINE_INLINE  vec4 wxxz(const  vec4 &v) { return {v.w,v.x,v.x,v.z}; }
UNIGINE_INLINE dvec4 wxxz(const dvec4 &v) { return {v.w,v.x,v.x,v.z}; }
UNIGINE_INLINE ivec4 wxxz(const ivec4 &v) { return {v.w,v.x,v.x,v.z}; }
UNIGINE_INLINE  vec4 wxxw(const  vec4 &v) { return {v.w,v.x,v.x,v.w}; }
UNIGINE_INLINE dvec4 wxxw(const dvec4 &v) { return {v.w,v.x,v.x,v.w}; }
UNIGINE_INLINE ivec4 wxxw(const ivec4 &v) { return {v.w,v.x,v.x,v.w}; }
UNIGINE_INLINE  vec4 wxyx(const  vec4 &v) { return {v.w,v.x,v.y,v.x}; }
UNIGINE_INLINE dvec4 wxyx(const dvec4 &v) { return {v.w,v.x,v.y,v.x}; }
UNIGINE_INLINE ivec4 wxyx(const ivec4 &v) { return {v.w,v.x,v.y,v.x}; }
UNIGINE_INLINE  vec4 wxyy(const  vec4 &v) { return {v.w,v.x,v.y,v.y}; }
UNIGINE_INLINE dvec4 wxyy(const dvec4 &v) { return {v.w,v.x,v.y,v.y}; }
UNIGINE_INLINE ivec4 wxyy(const ivec4 &v) { return {v.w,v.x,v.y,v.y}; }
UNIGINE_INLINE  vec4 wxyz(const  vec4 &v) { return {v.w,v.x,v.y,v.z}; }
UNIGINE_INLINE dvec4 wxyz(const dvec4 &v) { return {v.w,v.x,v.y,v.z}; }
UNIGINE_INLINE ivec4 wxyz(const ivec4 &v) { return {v.w,v.x,v.y,v.z}; }
UNIGINE_INLINE  vec4 wxyw(const  vec4 &v) { return {v.w,v.x,v.y,v.w}; }
UNIGINE_INLINE dvec4 wxyw(const dvec4 &v) { return {v.w,v.x,v.y,v.w}; }
UNIGINE_INLINE ivec4 wxyw(const ivec4 &v) { return {v.w,v.x,v.y,v.w}; }
UNIGINE_INLINE  vec4 wxzx(const  vec4 &v) { return {v.w,v.x,v.z,v.x}; }
UNIGINE_INLINE dvec4 wxzx(const dvec4 &v) { return {v.w,v.x,v.z,v.x}; }
UNIGINE_INLINE ivec4 wxzx(const ivec4 &v) { return {v.w,v.x,v.z,v.x}; }
UNIGINE_INLINE  vec4 wxzy(const  vec4 &v) { return {v.w,v.x,v.z,v.y}; }
UNIGINE_INLINE dvec4 wxzy(const dvec4 &v) { return {v.w,v.x,v.z,v.y}; }
UNIGINE_INLINE ivec4 wxzy(const ivec4 &v) { return {v.w,v.x,v.z,v.y}; }
UNIGINE_INLINE  vec4 wxzz(const  vec4 &v) { return {v.w,v.x,v.z,v.z}; }
UNIGINE_INLINE dvec4 wxzz(const dvec4 &v) { return {v.w,v.x,v.z,v.z}; }
UNIGINE_INLINE ivec4 wxzz(const ivec4 &v) { return {v.w,v.x,v.z,v.z}; }
UNIGINE_INLINE  vec4 wxzw(const  vec4 &v) { return {v.w,v.x,v.z,v.w}; }
UNIGINE_INLINE dvec4 wxzw(const dvec4 &v) { return {v.w,v.x,v.z,v.w}; }
UNIGINE_INLINE ivec4 wxzw(const ivec4 &v) { return {v.w,v.x,v.z,v.w}; }
UNIGINE_INLINE  vec4 wxwx(const  vec4 &v) { return {v.w,v.x,v.w,v.x}; }
UNIGINE_INLINE dvec4 wxwx(const dvec4 &v) { return {v.w,v.x,v.w,v.x}; }
UNIGINE_INLINE ivec4 wxwx(const ivec4 &v) { return {v.w,v.x,v.w,v.x}; }
UNIGINE_INLINE  vec4 wxwy(const  vec4 &v) { return {v.w,v.x,v.w,v.y}; }
UNIGINE_INLINE dvec4 wxwy(const dvec4 &v) { return {v.w,v.x,v.w,v.y}; }
UNIGINE_INLINE ivec4 wxwy(const ivec4 &v) { return {v.w,v.x,v.w,v.y}; }
UNIGINE_INLINE  vec4 wxwz(const  vec4 &v) { return {v.w,v.x,v.w,v.z}; }
UNIGINE_INLINE dvec4 wxwz(const dvec4 &v) { return {v.w,v.x,v.w,v.z}; }
UNIGINE_INLINE ivec4 wxwz(const ivec4 &v) { return {v.w,v.x,v.w,v.z}; }
UNIGINE_INLINE  vec4 wxww(const  vec4 &v) { return {v.w,v.x,v.w,v.w}; }
UNIGINE_INLINE dvec4 wxww(const dvec4 &v) { return {v.w,v.x,v.w,v.w}; }
UNIGINE_INLINE ivec4 wxww(const ivec4 &v) { return {v.w,v.x,v.w,v.w}; }
UNIGINE_INLINE  vec4 wyxx(const  vec4 &v) { return {v.w,v.y,v.x,v.x}; }
UNIGINE_INLINE dvec4 wyxx(const dvec4 &v) { return {v.w,v.y,v.x,v.x}; }
UNIGINE_INLINE ivec4 wyxx(const ivec4 &v) { return {v.w,v.y,v.x,v.x}; }
UNIGINE_INLINE  vec4 wyxy(const  vec4 &v) { return {v.w,v.y,v.x,v.y}; }
UNIGINE_INLINE dvec4 wyxy(const dvec4 &v) { return {v.w,v.y,v.x,v.y}; }
UNIGINE_INLINE ivec4 wyxy(const ivec4 &v) { return {v.w,v.y,v.x,v.y}; }
UNIGINE_INLINE  vec4 wyxz(const  vec4 &v) { return {v.w,v.y,v.x,v.z}; }
UNIGINE_INLINE dvec4 wyxz(const dvec4 &v) { return {v.w,v.y,v.x,v.z}; }
UNIGINE_INLINE ivec4 wyxz(const ivec4 &v) { return {v.w,v.y,v.x,v.z}; }
UNIGINE_INLINE  vec4 wyxw(const  vec4 &v) { return {v.w,v.y,v.x,v.w}; }
UNIGINE_INLINE dvec4 wyxw(const dvec4 &v) { return {v.w,v.y,v.x,v.w}; }
UNIGINE_INLINE ivec4 wyxw(const ivec4 &v) { return {v.w,v.y,v.x,v.w}; }
UNIGINE_INLINE  vec4 wyyx(const  vec4 &v) { return {v.w,v.y,v.y,v.x}; }
UNIGINE_INLINE dvec4 wyyx(const dvec4 &v) { return {v.w,v.y,v.y,v.x}; }
UNIGINE_INLINE ivec4 wyyx(const ivec4 &v) { return {v.w,v.y,v.y,v.x}; }
UNIGINE_INLINE  vec4 wyyy(const  vec4 &v) { return {v.w,v.y,v.y,v.y}; }
UNIGINE_INLINE dvec4 wyyy(const dvec4 &v) { return {v.w,v.y,v.y,v.y}; }
UNIGINE_INLINE ivec4 wyyy(const ivec4 &v) { return {v.w,v.y,v.y,v.y}; }
UNIGINE_INLINE  vec4 wyyz(const  vec4 &v) { return {v.w,v.y,v.y,v.z}; }
UNIGINE_INLINE dvec4 wyyz(const dvec4 &v) { return {v.w,v.y,v.y,v.z}; }
UNIGINE_INLINE ivec4 wyyz(const ivec4 &v) { return {v.w,v.y,v.y,v.z}; }
UNIGINE_INLINE  vec4 wyyw(const  vec4 &v) { return {v.w,v.y,v.y,v.w}; }
UNIGINE_INLINE dvec4 wyyw(const dvec4 &v) { return {v.w,v.y,v.y,v.w}; }
UNIGINE_INLINE ivec4 wyyw(const ivec4 &v) { return {v.w,v.y,v.y,v.w}; }
UNIGINE_INLINE  vec4 wyzx(const  vec4 &v) { return {v.w,v.y,v.z,v.x}; }
UNIGINE_INLINE dvec4 wyzx(const dvec4 &v) { return {v.w,v.y,v.z,v.x}; }
UNIGINE_INLINE ivec4 wyzx(const ivec4 &v) { return {v.w,v.y,v.z,v.x}; }
UNIGINE_INLINE  vec4 wyzy(const  vec4 &v) { return {v.w,v.y,v.z,v.y}; }
UNIGINE_INLINE dvec4 wyzy(const dvec4 &v) { return {v.w,v.y,v.z,v.y}; }
UNIGINE_INLINE ivec4 wyzy(const ivec4 &v) { return {v.w,v.y,v.z,v.y}; }
UNIGINE_INLINE  vec4 wyzz(const  vec4 &v) { return {v.w,v.y,v.z,v.z}; }
UNIGINE_INLINE dvec4 wyzz(const dvec4 &v) { return {v.w,v.y,v.z,v.z}; }
UNIGINE_INLINE ivec4 wyzz(const ivec4 &v) { return {v.w,v.y,v.z,v.z}; }
UNIGINE_INLINE  vec4 wyzw(const  vec4 &v) { return {v.w,v.y,v.z,v.w}; }
UNIGINE_INLINE dvec4 wyzw(const dvec4 &v) { return {v.w,v.y,v.z,v.w}; }
UNIGINE_INLINE ivec4 wyzw(const ivec4 &v) { return {v.w,v.y,v.z,v.w}; }
UNIGINE_INLINE  vec4 wywx(const  vec4 &v) { return {v.w,v.y,v.w,v.x}; }
UNIGINE_INLINE dvec4 wywx(const dvec4 &v) { return {v.w,v.y,v.w,v.x}; }
UNIGINE_INLINE ivec4 wywx(const ivec4 &v) { return {v.w,v.y,v.w,v.x}; }
UNIGINE_INLINE  vec4 wywy(const  vec4 &v) { return {v.w,v.y,v.w,v.y}; }
UNIGINE_INLINE dvec4 wywy(const dvec4 &v) { return {v.w,v.y,v.w,v.y}; }
UNIGINE_INLINE ivec4 wywy(const ivec4 &v) { return {v.w,v.y,v.w,v.y}; }
UNIGINE_INLINE  vec4 wywz(const  vec4 &v) { return {v.w,v.y,v.w,v.z}; }
UNIGINE_INLINE dvec4 wywz(const dvec4 &v) { return {v.w,v.y,v.w,v.z}; }
UNIGINE_INLINE ivec4 wywz(const ivec4 &v) { return {v.w,v.y,v.w,v.z}; }
UNIGINE_INLINE  vec4 wyww(const  vec4 &v) { return {v.w,v.y,v.w,v.w}; }
UNIGINE_INLINE dvec4 wyww(const dvec4 &v) { return {v.w,v.y,v.w,v.w}; }
UNIGINE_INLINE ivec4 wyww(const ivec4 &v) { return {v.w,v.y,v.w,v.w}; }
UNIGINE_INLINE  vec4 wzxx(const  vec4 &v) { return {v.w,v.z,v.x,v.x}; }
UNIGINE_INLINE dvec4 wzxx(const dvec4 &v) { return {v.w,v.z,v.x,v.x}; }
UNIGINE_INLINE ivec4 wzxx(const ivec4 &v) { return {v.w,v.z,v.x,v.x}; }
UNIGINE_INLINE  vec4 wzxy(const  vec4 &v) { return {v.w,v.z,v.x,v.y}; }
UNIGINE_INLINE dvec4 wzxy(const dvec4 &v) { return {v.w,v.z,v.x,v.y}; }
UNIGINE_INLINE ivec4 wzxy(const ivec4 &v) { return {v.w,v.z,v.x,v.y}; }
UNIGINE_INLINE  vec4 wzxz(const  vec4 &v) { return {v.w,v.z,v.x,v.z}; }
UNIGINE_INLINE dvec4 wzxz(const dvec4 &v) { return {v.w,v.z,v.x,v.z}; }
UNIGINE_INLINE ivec4 wzxz(const ivec4 &v) { return {v.w,v.z,v.x,v.z}; }
UNIGINE_INLINE  vec4 wzxw(const  vec4 &v) { return {v.w,v.z,v.x,v.w}; }
UNIGINE_INLINE dvec4 wzxw(const dvec4 &v) { return {v.w,v.z,v.x,v.w}; }
UNIGINE_INLINE ivec4 wzxw(const ivec4 &v) { return {v.w,v.z,v.x,v.w}; }
UNIGINE_INLINE  vec4 wzyx(const  vec4 &v) { return {v.w,v.z,v.y,v.x}; }
UNIGINE_INLINE dvec4 wzyx(const dvec4 &v) { return {v.w,v.z,v.y,v.x}; }
UNIGINE_INLINE ivec4 wzyx(const ivec4 &v) { return {v.w,v.z,v.y,v.x}; }
UNIGINE_INLINE  vec4 wzyy(const  vec4 &v) { return {v.w,v.z,v.y,v.y}; }
UNIGINE_INLINE dvec4 wzyy(const dvec4 &v) { return {v.w,v.z,v.y,v.y}; }
UNIGINE_INLINE ivec4 wzyy(const ivec4 &v) { return {v.w,v.z,v.y,v.y}; }
UNIGINE_INLINE  vec4 wzyz(const  vec4 &v) { return {v.w,v.z,v.y,v.z}; }
UNIGINE_INLINE dvec4 wzyz(const dvec4 &v) { return {v.w,v.z,v.y,v.z}; }
UNIGINE_INLINE ivec4 wzyz(const ivec4 &v) { return {v.w,v.z,v.y,v.z}; }
UNIGINE_INLINE  vec4 wzyw(const  vec4 &v) { return {v.w,v.z,v.y,v.w}; }
UNIGINE_INLINE dvec4 wzyw(const dvec4 &v) { return {v.w,v.z,v.y,v.w}; }
UNIGINE_INLINE ivec4 wzyw(const ivec4 &v) { return {v.w,v.z,v.y,v.w}; }
UNIGINE_INLINE  vec4 wzzx(const  vec4 &v) { return {v.w,v.z,v.z,v.x}; }
UNIGINE_INLINE dvec4 wzzx(const dvec4 &v) { return {v.w,v.z,v.z,v.x}; }
UNIGINE_INLINE ivec4 wzzx(const ivec4 &v) { return {v.w,v.z,v.z,v.x}; }
UNIGINE_INLINE  vec4 wzzy(const  vec4 &v) { return {v.w,v.z,v.z,v.y}; }
UNIGINE_INLINE dvec4 wzzy(const dvec4 &v) { return {v.w,v.z,v.z,v.y}; }
UNIGINE_INLINE ivec4 wzzy(const ivec4 &v) { return {v.w,v.z,v.z,v.y}; }
UNIGINE_INLINE  vec4 wzzz(const  vec4 &v) { return {v.w,v.z,v.z,v.z}; }
UNIGINE_INLINE dvec4 wzzz(const dvec4 &v) { return {v.w,v.z,v.z,v.z}; }
UNIGINE_INLINE ivec4 wzzz(const ivec4 &v) { return {v.w,v.z,v.z,v.z}; }
UNIGINE_INLINE  vec4 wzzw(const  vec4 &v) { return {v.w,v.z,v.z,v.w}; }
UNIGINE_INLINE dvec4 wzzw(const dvec4 &v) { return {v.w,v.z,v.z,v.w}; }
UNIGINE_INLINE ivec4 wzzw(const ivec4 &v) { return {v.w,v.z,v.z,v.w}; }
UNIGINE_INLINE  vec4 wzwx(const  vec4 &v) { return {v.w,v.z,v.w,v.x}; }
UNIGINE_INLINE dvec4 wzwx(const dvec4 &v) { return {v.w,v.z,v.w,v.x}; }
UNIGINE_INLINE ivec4 wzwx(const ivec4 &v) { return {v.w,v.z,v.w,v.x}; }
UNIGINE_INLINE  vec4 wzwy(const  vec4 &v) { return {v.w,v.z,v.w,v.y}; }
UNIGINE_INLINE dvec4 wzwy(const dvec4 &v) { return {v.w,v.z,v.w,v.y}; }
UNIGINE_INLINE ivec4 wzwy(const ivec4 &v) { return {v.w,v.z,v.w,v.y}; }
UNIGINE_INLINE  vec4 wzwz(const  vec4 &v) { return {v.w,v.z,v.w,v.z}; }
UNIGINE_INLINE dvec4 wzwz(const dvec4 &v) { return {v.w,v.z,v.w,v.z}; }
UNIGINE_INLINE ivec4 wzwz(const ivec4 &v) { return {v.w,v.z,v.w,v.z}; }
UNIGINE_INLINE  vec4 wzww(const  vec4 &v) { return {v.w,v.z,v.w,v.w}; }
UNIGINE_INLINE dvec4 wzww(const dvec4 &v) { return {v.w,v.z,v.w,v.w}; }
UNIGINE_INLINE ivec4 wzww(const ivec4 &v) { return {v.w,v.z,v.w,v.w}; }
UNIGINE_INLINE  vec4 wwxx(const  vec4 &v) { return {v.w,v.w,v.x,v.x}; }
UNIGINE_INLINE dvec4 wwxx(const dvec4 &v) { return {v.w,v.w,v.x,v.x}; }
UNIGINE_INLINE ivec4 wwxx(const ivec4 &v) { return {v.w,v.w,v.x,v.x}; }
UNIGINE_INLINE  vec4 wwxy(const  vec4 &v) { return {v.w,v.w,v.x,v.y}; }
UNIGINE_INLINE dvec4 wwxy(const dvec4 &v) { return {v.w,v.w,v.x,v.y}; }
UNIGINE_INLINE ivec4 wwxy(const ivec4 &v) { return {v.w,v.w,v.x,v.y}; }
UNIGINE_INLINE  vec4 wwxz(const  vec4 &v) { return {v.w,v.w,v.x,v.z}; }
UNIGINE_INLINE dvec4 wwxz(const dvec4 &v) { return {v.w,v.w,v.x,v.z}; }
UNIGINE_INLINE ivec4 wwxz(const ivec4 &v) { return {v.w,v.w,v.x,v.z}; }
UNIGINE_INLINE  vec4 wwxw(const  vec4 &v) { return {v.w,v.w,v.x,v.w}; }
UNIGINE_INLINE dvec4 wwxw(const dvec4 &v) { return {v.w,v.w,v.x,v.w}; }
UNIGINE_INLINE ivec4 wwxw(const ivec4 &v) { return {v.w,v.w,v.x,v.w}; }
UNIGINE_INLINE  vec4 wwyx(const  vec4 &v) { return {v.w,v.w,v.y,v.x}; }
UNIGINE_INLINE dvec4 wwyx(const dvec4 &v) { return {v.w,v.w,v.y,v.x}; }
UNIGINE_INLINE ivec4 wwyx(const ivec4 &v) { return {v.w,v.w,v.y,v.x}; }
UNIGINE_INLINE  vec4 wwyy(const  vec4 &v) { return {v.w,v.w,v.y,v.y}; }
UNIGINE_INLINE dvec4 wwyy(const dvec4 &v) { return {v.w,v.w,v.y,v.y}; }
UNIGINE_INLINE ivec4 wwyy(const ivec4 &v) { return {v.w,v.w,v.y,v.y}; }
UNIGINE_INLINE  vec4 wwyz(const  vec4 &v) { return {v.w,v.w,v.y,v.z}; }
UNIGINE_INLINE dvec4 wwyz(const dvec4 &v) { return {v.w,v.w,v.y,v.z}; }
UNIGINE_INLINE ivec4 wwyz(const ivec4 &v) { return {v.w,v.w,v.y,v.z}; }
UNIGINE_INLINE  vec4 wwyw(const  vec4 &v) { return {v.w,v.w,v.y,v.w}; }
UNIGINE_INLINE dvec4 wwyw(const dvec4 &v) { return {v.w,v.w,v.y,v.w}; }
UNIGINE_INLINE ivec4 wwyw(const ivec4 &v) { return {v.w,v.w,v.y,v.w}; }
UNIGINE_INLINE  vec4 wwzx(const  vec4 &v) { return {v.w,v.w,v.z,v.x}; }
UNIGINE_INLINE dvec4 wwzx(const dvec4 &v) { return {v.w,v.w,v.z,v.x}; }
UNIGINE_INLINE ivec4 wwzx(const ivec4 &v) { return {v.w,v.w,v.z,v.x}; }
UNIGINE_INLINE  vec4 wwzy(const  vec4 &v) { return {v.w,v.w,v.z,v.y}; }
UNIGINE_INLINE dvec4 wwzy(const dvec4 &v) { return {v.w,v.w,v.z,v.y}; }
UNIGINE_INLINE ivec4 wwzy(const ivec4 &v) { return {v.w,v.w,v.z,v.y}; }
UNIGINE_INLINE  vec4 wwzz(const  vec4 &v) { return {v.w,v.w,v.z,v.z}; }
UNIGINE_INLINE dvec4 wwzz(const dvec4 &v) { return {v.w,v.w,v.z,v.z}; }
UNIGINE_INLINE ivec4 wwzz(const ivec4 &v) { return {v.w,v.w,v.z,v.z}; }
UNIGINE_INLINE  vec4 wwzw(const  vec4 &v) { return {v.w,v.w,v.z,v.w}; }
UNIGINE_INLINE dvec4 wwzw(const dvec4 &v) { return {v.w,v.w,v.z,v.w}; }
UNIGINE_INLINE ivec4 wwzw(const ivec4 &v) { return {v.w,v.w,v.z,v.w}; }
UNIGINE_INLINE  vec4 wwwx(const  vec4 &v) { return {v.w,v.w,v.w,v.x}; }
UNIGINE_INLINE dvec4 wwwx(const dvec4 &v) { return {v.w,v.w,v.w,v.x}; }
UNIGINE_INLINE ivec4 wwwx(const ivec4 &v) { return {v.w,v.w,v.w,v.x}; }
UNIGINE_INLINE  vec4 wwwy(const  vec4 &v) { return {v.w,v.w,v.w,v.y}; }
UNIGINE_INLINE dvec4 wwwy(const dvec4 &v) { return {v.w,v.w,v.w,v.y}; }
UNIGINE_INLINE ivec4 wwwy(const ivec4 &v) { return {v.w,v.w,v.w,v.y}; }
UNIGINE_INLINE  vec4 wwwz(const  vec4 &v) { return {v.w,v.w,v.w,v.z}; }
UNIGINE_INLINE dvec4 wwwz(const dvec4 &v) { return {v.w,v.w,v.w,v.z}; }
UNIGINE_INLINE ivec4 wwwz(const ivec4 &v) { return {v.w,v.w,v.w,v.z}; }
UNIGINE_INLINE  vec4 wwww(const  vec4 &v) { return {v.w,v.w,v.w,v.w}; }
UNIGINE_INLINE dvec4 wwww(const dvec4 &v) { return {v.w,v.w,v.w,v.w}; }
UNIGINE_INLINE ivec4 wwww(const ivec4 &v) { return {v.w,v.w,v.w,v.w}; }

struct WeightPoint1D
{
	float position{0.0f};
	float normalized_weight{0.0f};

	WeightPoint1D() = default;

	WeightPoint1D(float blend_position)
	{
		position = blend_position;
	}
};

UNIGINE_INLINE void blend1DWeights(float blend_point, Vector<WeightPoint1D> &sorted_points)
{
	if (sorted_points.empty())
		return;

	if (sorted_points.size() == 1)
	{
		sorted_points[0].normalized_weight = 1.0f;
		return;
	}

	for (auto &wp : sorted_points)
		wp.normalized_weight = 0.0f;

	if (blend_point < sorted_points.first().position)
	{
		sorted_points.first().normalized_weight = 1.0f;
		return;
	}

	if (sorted_points.last().position < blend_point)
	{
		sorted_points.last().normalized_weight = 1.0f;
		return;
	}

	for (int i = 0; i < sorted_points.size() - 1; i++)
	{
		if (sorted_points[i].position <= blend_point && blend_point <= sorted_points[i + 1].position)
		{
			float distance = (sorted_points[i + 1].position - sorted_points[i].position);
			if (distance > 0.0f)
			{
				sorted_points[i].normalized_weight = (sorted_points[i + 1].position - blend_point) / distance;
				sorted_points[i + 1].normalized_weight = 1.0f - sorted_points[i].normalized_weight;
			} else
			{
				sorted_points[i].normalized_weight = 0.5f;
				sorted_points[i + 1].normalized_weight = 0.5f;
			}
			break;
		}
	}
}

UNIGINE_INLINE void blend1DWeights(float blend_point, const Vector<float> &sorted_points, Vector<float> &out_normalized_weights)
{
	out_normalized_weights.resize(sorted_points.size());

	if (sorted_points.empty())
		return;

	if (sorted_points.size() == 1)
	{
		out_normalized_weights[0] = 1.0f;
		return;
	}

	for (int i = 0; i < out_normalized_weights.size(); i++)
		out_normalized_weights[i] = 0.0f;

	if (blend_point < sorted_points.first())
	{
		out_normalized_weights.first() = 1.0f;
		return;
	}

	if (sorted_points.last() < blend_point)
	{
		out_normalized_weights.last() = 1.0f;
		return;
	}

	for (int i = 0; i < sorted_points.size() - 1; i++)
	{
		if (sorted_points[i] <= blend_point && blend_point <= sorted_points[i + 1])
		{
			float distance = (sorted_points[i + 1] - sorted_points[i]);
			if (distance > 0.0f)
			{
				out_normalized_weights[i] = (sorted_points[i + 1] - blend_point) / distance;
				out_normalized_weights[i + 1] = 1.0f - out_normalized_weights[i];
			} else
			{
				out_normalized_weights[i] = 0.5f;
				out_normalized_weights[i + 1] = 0.5f;
			}
			break;
		}
	}
}

struct WeightPoint2D
{
	vec2 position;
	float normalized_weight{0.0f};

	WeightPoint2D() = default;

	WeightPoint2D(const vec2 &blend_position)
	{
		position = blend_position;
	}
};

UNIGINE_INLINE void blend2DCartesianWeights(const vec2 &blend_point, Vector<WeightPoint2D> &points)
{
	if (points.empty())
		return;

	if (points.size() == 1)
	{
		points[0].normalized_weight = 1.0f;
		return;
	}

	for (auto &wp : points)
		wp.normalized_weight = 0.0f;

	float total_weight = 0.0f;
	for (int i = 0; i < points.size(); i++)
	{
		const vec2 projected_vector = blend_point - points[i].position;

		float point_weight = Consts::INF;
		for (int j = 0; j < points.size(); j++)
		{
			if (j == i)
				continue;

			vec2 projection_direction = points[j].position - points[i].position;
			float denom = projection_direction.length2();
			if (denom == 0.0f)
				continue;
			float weight = 1.0f - dot(projected_vector, projection_direction) / denom;
			weight = max(weight, 0.0f);
			point_weight = min(point_weight, weight);
		}

		points[i].normalized_weight = point_weight;
		total_weight += point_weight;
	}

	if (total_weight > 0.0f)
	{
		for (int i = 0; i < points.size(); i++)
			points[i].normalized_weight /= total_weight;
	} else
	{
		float fallback_weight = 1.0f / points.size();
		for (int i = 0; i < points.size(); i++)
			points[i].normalized_weight = fallback_weight;
	}
}

UNIGINE_INLINE void blend2DCartesianWeights(const vec2 &blend_point, const Vector<vec2> &points, Vector<float> &out_normalized_weights)
{
	out_normalized_weights.resize(points.size());

	if (points.empty())
		return;

	if (points.size() == 1)
	{
		out_normalized_weights[0] = 1.0f;
		return;
	}

	for (int i = 0; i < out_normalized_weights.size(); i++)
		out_normalized_weights[i] = 0.0f;

	float total_weight = 0.0f;
	for (int i = 0; i < points.size(); i++)
	{
		const vec2 projected_vector = blend_point - points[i];

		float point_weight = Consts::INF;
		for (int j = 0; j < points.size(); j++)
		{
			if (j == i)
				continue;

			vec2 projection_direction = points[j] - points[i];
			float denom = projection_direction.length2();
			if (denom == 0.0f)
				continue;
			float weight = 1.0f - dot(projected_vector, projection_direction) / denom;
			weight = max(weight, 0.0f);
			point_weight = min(point_weight, weight);
		}

		out_normalized_weights[i] = point_weight;
		total_weight += point_weight;
	}

	if (total_weight > 0.0f)
	{
		for (int i = 0; i < points.size(); i++)
			out_normalized_weights[i] /= total_weight;
	} else
	{
		float fallback_weight = 1.0f / points.size();
		for (int i = 0; i < points.size(); i++)
			out_normalized_weights[i] = fallback_weight;
	}
}

UNIGINE_INLINE void blend2DPolarWeights(const vec2 &blend_point, Vector<WeightPoint2D> &points)
{
	if (points.empty())
		return;

	if (points.size() == 1)
	{
		points[0].normalized_weight = 1.0f;
		return;
	}

	for (auto &wp : points)
		wp.normalized_weight = 0.0f;

	auto get_signed_angle = [](const vec2 &a, const vec2 &b)
	{
		return Math::atan2(a.x * b.y - a.y * b.x, a.x * b.x + a.y * b.y);
	};

	const float alpha_multiplier = 2.0f;
	const float blend_point_length = blend_point.length();

	float total_weight = 0.0f;
	for (int i = 0; i < points.size(); i++)
	{
		float pi_length = points[i].position.length();

		float point_weight = Consts::INF;
		for (int j = 0; j < points.size(); j++)
		{
			if (j == i)
				continue;

			float pj_length = points[j].position.length();
			float pi_pj_avg_length = (pj_length + pi_length) * 0.5f;
			if (pi_pj_avg_length == 0.0f)
				continue;

			vec2 projected_vector {
				(blend_point_length - pi_length) / pi_pj_avg_length,
				get_signed_angle(blend_point, points[i].position) * alpha_multiplier
			};

			vec2 projection_direction {
				(pj_length - pi_length) / pi_pj_avg_length,
				get_signed_angle(points[j].position, points[i].position) * alpha_multiplier
			};

			float denom = projection_direction.length2();
			if (denom == 0.0f)
				continue;
			float weight = 1.0f - dot(projected_vector, projection_direction) / denom;
			weight = max(weight, 0.0f);
			point_weight = min(weight, point_weight);
		}

		points[i].normalized_weight = point_weight;
		total_weight += point_weight;
	}

	if (total_weight > 0.0f)
	{
		for (int i = 0; i < points.size(); i++)
			points[i].normalized_weight /= total_weight;
	} else
	{
		float fallback_weight = 1.0f / points.size();
		for (int i = 0; i < points.size(); i++)
			points[i].normalized_weight = fallback_weight;
	}
}

UNIGINE_INLINE void blend2DPolarWeights(const vec2 &blend_point, Vector<vec2> &points, Vector<float> &out_normalized_weights)
{
	out_normalized_weights.resize(points.size());

	if (points.empty())
		return;

	if (points.size() == 1)
	{
		out_normalized_weights[0] = 1.0f;
		return;
	}

	for (int i = 0; i < out_normalized_weights.size(); i++)
		out_normalized_weights[i] = 0.0f;

	auto get_signed_angle = [](const vec2 &a, const vec2 &b)
	{
		return Math::atan2(a.x * b.y - a.y * b.x, a.x * b.x + a.y * b.y);
	};

	const float alpha_multiplier = 2.0f;
	const float blend_point_length = blend_point.length();

	float total_weight = 0.0f;
	for (int i = 0; i < points.size(); i++)
	{
		float pi_length = points[i].length();

		float point_weight = Consts::INF;
		for (int j = 0; j < points.size(); j++)
		{
			if (j == i)
				continue;

			float pj_length = points[j].length();
			float pi_pj_avg_length = (pj_length + pi_length) * 0.5f;
			if (pi_pj_avg_length == 0.0f)
				continue;

			vec2 projected_vector{
				(blend_point_length - pi_length) / pi_pj_avg_length,
				get_signed_angle(blend_point, points[i]) * alpha_multiplier
			};

			vec2 projection_direction{
				(pj_length - pi_length) / pi_pj_avg_length,
				get_signed_angle(points[j], points[i]) * alpha_multiplier
			};

			float denom = projection_direction.length2();
			if (denom == 0.0f)
				continue;
			float weight = 1.0f - dot(projected_vector, projection_direction) / denom;
			weight = max(weight, 0.0f);
			point_weight = min(weight, point_weight);
		}

		out_normalized_weights[i] = point_weight;
		total_weight += point_weight;
	}

	if (total_weight > 0.0f)
	{
		for (int i = 0; i < points.size(); i++)
			out_normalized_weights[i] /= total_weight;
	} else
	{
		float fallback_weight = 1.0f / points.size();
		for (int i = 0; i < points.size(); i++)
			out_normalized_weights[i] = fallback_weight;
	}
}

UNIGINE_INLINE vec3 getWeightedAverage(const Vector<vec3> &points, const Vector<float> &normalized_weigths)
{
	vec3 res;
	int size = min(points.size(), normalized_weigths.size());
	for (int i = 0; i < size; i++)
		res += points[i] * normalized_weigths[i];
	return res;
}

UNIGINE_INLINE quat getWeightedAverage(const Vector<quat> &rotations, const Vector<float> &normalized_weigths)
{
	quat res;
	int size = min(rotations.size(), normalized_weigths.size());
	float total_weight = 0.0f;
	for (int i = 0; i < size; i++)
	{
		if (normalized_weigths[i] < Consts::EPS)
			continue;

		total_weight += normalized_weigths[i];
		res = slerp(res, rotations[i], normalized_weigths[i] / total_weight);
	}
	return res;
}

} // end namespace Math

} // end namespace Unigine

#include "UnigineMathLibBounds.h"
#include "UnigineMathLib2d.h"
#include "UnigineMathLibPolygon.h"
#include "UnigineMathLibConvex.h"
#include "UnigineMathLibConcave.h"
#include "UnigineMathLibTransforms.h"

#ifdef NDEBUG
	#define UNIGINE_ASSERT_MATH(VALUE) VALUE
#else
	UNIGINE_INLINE Unigine::Math::vec2 &unigine_assert_math(Unigine::Math::vec2 &v) { UNIGINE_ASSERT_ALIGNED8(&v); return v; }
	UNIGINE_INLINE Unigine::Math::vec3 &unigine_assert_math(Unigine::Math::vec3 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::vec4 &unigine_assert_math(Unigine::Math::vec4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::dvec2 &unigine_assert_math(Unigine::Math::dvec2 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::dvec3 &unigine_assert_math(Unigine::Math::dvec3 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::dvec4 &unigine_assert_math(Unigine::Math::dvec4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::ivec2 &unigine_assert_math(Unigine::Math::ivec2 &v) { UNIGINE_ASSERT_ALIGNED8(&v); return v; }
	UNIGINE_INLINE Unigine::Math::ivec3 &unigine_assert_math(Unigine::Math::ivec3 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::ivec4 &unigine_assert_math(Unigine::Math::ivec4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::quat &unigine_assert_math(Unigine::Math::quat &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::mat2 &unigine_assert_math(Unigine::Math::mat2 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::mat3 &unigine_assert_math(Unigine::Math::mat3 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::mat4 &unigine_assert_math(Unigine::Math::mat4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::dmat4 &unigine_assert_math(Unigine::Math::dmat4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::BoundSphere &unigine_assert_math(Unigine::Math::BoundSphere &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::BoundBox &unigine_assert_math(Unigine::Math::BoundBox &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::BoundFrustum &unigine_assert_math(Unigine::Math::BoundFrustum &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::FloatTransform &unigine_assert_math(Unigine::Math::FloatTransform &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE Unigine::Math::DoubleTransform &unigine_assert_math(Unigine::Math::DoubleTransform &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	#if defined(USE_DOUBLE) || defined(UNIGINE_DOUBLE)
		UNIGINE_INLINE Unigine::Math::WorldBoundSphere &unigine_assert_math(Unigine::Math::WorldBoundSphere &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
		UNIGINE_INLINE Unigine::Math::WorldBoundBox &unigine_assert_math(Unigine::Math::WorldBoundBox &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
		UNIGINE_INLINE Unigine::Math::WorldBoundFrustum &unigine_assert_math(Unigine::Math::WorldBoundFrustum &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	#endif

	UNIGINE_INLINE const Unigine::Math::vec2 &unigine_assert_math(const Unigine::Math::vec2 &v) { UNIGINE_ASSERT_ALIGNED8(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::vec3 &unigine_assert_math(const Unigine::Math::vec3 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::vec4 &unigine_assert_math(const Unigine::Math::vec4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::dvec2 &unigine_assert_math(const Unigine::Math::dvec2 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::dvec3 &unigine_assert_math(const Unigine::Math::dvec3 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::dvec4 &unigine_assert_math(const Unigine::Math::dvec4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::ivec2 &unigine_assert_math(const Unigine::Math::ivec2 &v) { UNIGINE_ASSERT_ALIGNED8(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::ivec3 &unigine_assert_math(const Unigine::Math::ivec3 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::ivec4 &unigine_assert_math(const Unigine::Math::ivec4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::quat &unigine_assert_math(const Unigine::Math::quat &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::mat2 &unigine_assert_math(const Unigine::Math::mat2 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::mat3 &unigine_assert_math(const Unigine::Math::mat3 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::mat4 &unigine_assert_math(const Unigine::Math::mat4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::dmat4 &unigine_assert_math(const Unigine::Math::dmat4 &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::BoundSphere &unigine_assert_math(const Unigine::Math::BoundSphere &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::BoundBox &unigine_assert_math(const Unigine::Math::BoundBox &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::BoundFrustum &unigine_assert_math(const Unigine::Math::BoundFrustum &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::FloatTransform &unigine_assert_math(const Unigine::Math::FloatTransform &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	UNIGINE_INLINE const Unigine::Math::DoubleTransform &unigine_assert_math(const Unigine::Math::DoubleTransform &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	#if defined(USE_DOUBLE) || defined(UNIGINE_DOUBLE)
		UNIGINE_INLINE const Unigine::Math::WorldBoundSphere &unigine_assert_math(const Unigine::Math::WorldBoundSphere &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
		UNIGINE_INLINE const Unigine::Math::WorldBoundBox &unigine_assert_math(const Unigine::Math::WorldBoundBox &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
		UNIGINE_INLINE const Unigine::Math::WorldBoundFrustum &unigine_assert_math(const Unigine::Math::WorldBoundFrustum &v) { UNIGINE_ASSERT_ALIGNED16(&v); return v; }
	#endif

	UNIGINE_INLINE Unigine::Math::vec2 *unigine_assert_math(Unigine::Math::vec2 *v) { UNIGINE_ASSERT_ALIGNED8(v); return v; }
	UNIGINE_INLINE Unigine::Math::vec3 *unigine_assert_math(Unigine::Math::vec3 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::vec4 *unigine_assert_math(Unigine::Math::vec4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::dvec3 *unigine_assert_math(Unigine::Math::dvec3 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::dvec2 *unigine_assert_math(Unigine::Math::dvec2 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::dvec4 *unigine_assert_math(Unigine::Math::dvec4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::ivec2 *unigine_assert_math(Unigine::Math::ivec2 *v) { UNIGINE_ASSERT_ALIGNED8(v); return v; }
	UNIGINE_INLINE Unigine::Math::ivec3 *unigine_assert_math(Unigine::Math::ivec3 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::ivec4 *unigine_assert_math(Unigine::Math::ivec4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::quat *unigine_assert_math(Unigine::Math::quat *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::mat2 *unigine_assert_math(Unigine::Math::mat2 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::mat3 *unigine_assert_math(Unigine::Math::mat3 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::mat4 *unigine_assert_math(Unigine::Math::mat4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::dmat4 *unigine_assert_math(Unigine::Math::dmat4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::BoundSphere *unigine_assert_math(Unigine::Math::BoundSphere *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::BoundBox *unigine_assert_math(Unigine::Math::BoundBox *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::BoundFrustum *unigine_assert_math(Unigine::Math::BoundFrustum *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::FloatTransform *unigine_assert_math(Unigine::Math::FloatTransform *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE Unigine::Math::DoubleTransform *unigine_assert_math(Unigine::Math::DoubleTransform *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	#if defined(USE_DOUBLE) || defined(UNIGINE_DOUBLE)
		UNIGINE_INLINE Unigine::Math::WorldBoundSphere *unigine_assert_math(Unigine::Math::WorldBoundSphere *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
		UNIGINE_INLINE Unigine::Math::WorldBoundBox *unigine_assert_math(Unigine::Math::WorldBoundBox *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
		UNIGINE_INLINE Unigine::Math::WorldBoundFrustum *unigine_assert_math(Unigine::Math::WorldBoundFrustum *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	#endif

	UNIGINE_INLINE const Unigine::Math::vec2 *unigine_assert_math(const Unigine::Math::vec2 *v) { UNIGINE_ASSERT_ALIGNED8(v); return v; }
	UNIGINE_INLINE const Unigine::Math::vec3 *unigine_assert_math(const Unigine::Math::vec3 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::vec4 *unigine_assert_math(const Unigine::Math::vec4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::dvec3 *unigine_assert_math(const Unigine::Math::dvec3 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::dvec2 *unigine_assert_math(const Unigine::Math::dvec2 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::dvec4 *unigine_assert_math(const Unigine::Math::dvec4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::ivec2 *unigine_assert_math(const Unigine::Math::ivec2 *v) { UNIGINE_ASSERT_ALIGNED8(v); return v; }
	UNIGINE_INLINE const Unigine::Math::ivec3 *unigine_assert_math(const Unigine::Math::ivec3 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::ivec4 *unigine_assert_math(const Unigine::Math::ivec4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::quat *unigine_assert_math(const Unigine::Math::quat *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::mat2 *unigine_assert_math(const Unigine::Math::mat2 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::mat3 *unigine_assert_math(const Unigine::Math::mat3 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::mat4 *unigine_assert_math(const Unigine::Math::mat4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::dmat4 *unigine_assert_math(const Unigine::Math::dmat4 *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::BoundSphere *unigine_assert_math(const Unigine::Math::BoundSphere *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::BoundBox *unigine_assert_math(const Unigine::Math::BoundBox *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::BoundFrustum *unigine_assert_math(const Unigine::Math::BoundFrustum *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::FloatTransform *unigine_assert_math(const Unigine::Math::FloatTransform *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	UNIGINE_INLINE const Unigine::Math::DoubleTransform *unigine_assert_math(const Unigine::Math::DoubleTransform *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	#if defined(USE_DOUBLE) || defined(UNIGINE_DOUBLE)
		UNIGINE_INLINE const Unigine::Math::WorldBoundSphere *unigine_assert_math(const Unigine::Math::WorldBoundSphere *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
		UNIGINE_INLINE const Unigine::Math::WorldBoundBox *unigine_assert_math(const Unigine::Math::WorldBoundBox *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
		UNIGINE_INLINE const Unigine::Math::WorldBoundFrustum *unigine_assert_math(const Unigine::Math::WorldBoundFrustum *v) { UNIGINE_ASSERT_ALIGNED16(v); return v; }
	#endif

	#define UNIGINE_ASSERT_MATH(VALUE) unigine_assert_math(VALUE)
#endif

namespace Unigine
{
template<typename Type>
struct Hasher;

template<>
struct Hasher<Math::ivec2> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::ivec2 &v) { return v.hash(); } };
template<>
struct Hasher<Math::ivec3> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::ivec3 &v) { return v.hash(); } };
template<>
struct Hasher<Math::ivec4> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::ivec3 &v) { return v.hash(); } };

template<>
struct Hasher<Math::vec2> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::vec2 &v) { return v.hash(); } };
template<>
struct Hasher<Math::vec3> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::vec3 &v) { return v.hash(); } };
template<>
struct Hasher<Math::vec4> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::vec3 &v) { return v.hash(); } };
template<>
struct Hasher<Math::quat> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::quat &v) { return v.hash(); } };

template<>
struct Hasher<Math::dvec2> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::dvec2 &v) { return v.hash(); } };
template<>
struct Hasher<Math::dvec3> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::dvec3 &v) { return v.hash(); } };
template<>
struct Hasher<Math::dvec4> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::dvec3 &v) { return v.hash(); } };

template<>
struct Hasher<Math::dmat4> { using HashType = unsigned int; UNIGINE_INLINE static HashType create(const Math::dmat4 &v) { return v.hash(); } };
}
