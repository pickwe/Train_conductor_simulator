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
// DO NOT EDIT DIRECTLY. This is an auto-generated file. Your changes will be lost.

#pragma once

#include "UnigineMathLib.h"
#include "UnigineMesh.h"
#include "UniginePtr.h"
#include "UnigineVector.h"
#include "UnigineStreams.h"

namespace Unigine
{


class UNIGINE_API MeshRender : public APIInterface
{
public:
	typedef const MeshRender ConstMeshRender;
	static constexpr bool HAS_OWNERSHIP = true;


	enum
	{
		MODE_TRIANGLES = 0,
		MODE_TRIANGLE_PATCHES,
		MODE_QUAD_PATCHES,
	};

	enum
	{
		USAGE_DYNAMIC_VERTEX = 1 << 0,
		USAGE_DYNAMIC_INDICES = 1 << 1,
		USAGE_DYNAMIC_ALL = (USAGE_DYNAMIC_VERTEX | USAGE_DYNAMIC_INDICES),
	};
	static Ptr<MeshRender> create();
	bool isLoaded() const;
	bool isSupportedMultiThreadedLoad() const;
	void setFlags(int flags);
	int getFlags() const;
	bool load(const char *path);
	bool load(const Ptr<ConstMesh> &mesh);
	void clear();
	void swap(const Ptr<MeshRender> &mesh);
	void bind();
	void unbind();
	int renderSurface(int mode, int surface, int target = 0);
	int renderInstancedSurface(int mode, int num, int surface, int target = 0);
	int render(int mode, int surface = -1, int target = 0);
	int renderInstanced(int mode, int num, int surface = -1, int target = 0);
	size_t getSystemMemoryUsage() const;
	size_t getVideoMemoryUsage() const;
	void updateDebugName();
	void setDebugName(const char *name);
	const char *getDebugName() const;
};
typedef Ptr<MeshRender> MeshRenderPtr;
typedef Ptr<const MeshRender> ConstMeshRenderPtr;
typedef const MeshRender ConstMeshRender;

} // namespace Unigine
