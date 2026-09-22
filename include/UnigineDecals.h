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

#include "UnigineMaterial.h"
#include "UnigineNode.h"
#include "UnigineMesh.h"
#include "UnigineObjects.h"

namespace Unigine
{

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API Decal : public Node
{
public:
	typedef const Decal ConstDecal;

	static bool convertible(Node *node) { return node && node->isDecal(); }

	void setMaterialFilePath(const char *path);
	String getMaterialFilePath() const;
	void setMaterialGUID(const UGUID &materialguid);
	UGUID getMaterialGUID() const;
	void setMaterial(const Ptr<Material> &material);
	Ptr<Material> getMaterial() const;
	Ptr<Material> getMaterialInherit() const;
	void setRadius(float radius);
	float getRadius() const;
	void setMaxFadeDistance(float distance);
	float getMaxFadeDistance() const;
	void setMaxVisibleDistance(float distance);
	float getMaxVisibleDistance() const;
	void setMinFadeDistance(float distance);
	float getMinFadeDistance() const;
	void setMinVisibleDistance(float distance);
	float getMinVisibleDistance() const;
	void setOpacity(float opacity);
	float getOpacity() const;
	void setCastBakedGlobalIllumination(bool illumination);
	bool isCastBakedGlobalIllumination() const;
	void setViewportMask(int mask);
	int getViewportMask() const;
	void setIntersectionMask(int mask);
	int getIntersectionMask() const;
	int isMaterialInherited() const;
	int isTerrainHole() const;
	void setSurfaceRenderCustomParameterFloat(int param, float value);
	void setSurfaceRenderCustomParameterFloat(const char *name, float value);
	float getSurfaceRenderCustomParameterFloat(int param) const;
	float getSurfaceRenderCustomParameterFloat(const char *name) const;
	void setSurfaceRenderCustomParameterInt(int param, int value);
	void setSurfaceRenderCustomParameterInt(const char *name, int value);
	int getSurfaceRenderCustomParameterInt(int param) const;
	int getSurfaceRenderCustomParameterInt(const char *name) const;
	void setSurfaceRenderCustomParameterUInt(int param, unsigned int value);
	void setSurfaceRenderCustomParameterUInt(const char *name, unsigned int value);
	unsigned int getSurfaceRenderCustomParameterUInt(int param) const;
	unsigned int getSurfaceRenderCustomParameterUInt(const char *name) const;
	bool isSurfaceRenderCustomParameterOverridden(int param) const;
	void resetSurfaceRenderCustomParameter(int param);
	void resetSurfaceRenderCustomParameters();
	int inside(const Math::vec3 &p);
};
typedef Ptr<Decal> DecalPtr;
typedef Ptr<const Decal> ConstDecalPtr;
typedef const Decal ConstDecal;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API DecalMesh : public Decal
{
public:
	typedef const DecalMesh ConstDecalMesh;

	static Node::TYPE type() { return Node::DECAL_MESH; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<DecalMesh> create();
	static Ptr<DecalMesh> create(const char *path);
	Ptr<ConstMesh> getMeshCurrentRAM() const;
	Ptr<ConstMesh> getMeshForceRAM();
	Ptr<ConstMesh> getMeshAsyncRAM();
	Ptr<Mesh> getMeshDynamicRAM();
	Ptr<Mesh> createCopyMeshRAM() const;
	bool getCopyMeshRAM(Ptr<Mesh> &result) const;
	Ptr<MeshRender> getMeshCurrentVRAM();
	Ptr<MeshRender> getMeshForceVRAM();
	Ptr<MeshRender> getMeshAsyncVRAM();
	Ptr<MeshRender> getMeshDynamicVRAM();
	bool loadAsyncRAM();
	bool loadAsyncVRAM();
	bool loadForceRAM();
	bool loadForceVRAM();
	ObjectMeshStatic::PROCEDURAL_MODE getMeshProceduralMode() const;
	bool isMeshProceduralDone() const;
	bool isMeshProceduralActive() const;
	bool isMeshProceduralDynamic() const;
	void setMeshProceduralMode(ObjectMeshStatic::PROCEDURAL_MODE mode, int mesh_render_flags = 0);
	bool applyCopyMeshProceduralForce(const Ptr<ConstMesh> &mesh, int mesh_render_flags = 0);
	bool applyMoveMeshProceduralForce(const Ptr<Mesh> &mesh, int mesh_render_flags = 0);
	bool applyMoveMeshProceduralForce(const Ptr<Mesh> &mesh_ram, const Ptr<MeshRender> &mesh_vram);
	bool applyCopyMeshProceduralAsync(const Ptr<ConstMesh> &mesh, int mesh_render_flags = 0);
	bool applyMoveMeshProceduralAsync(const Ptr<Mesh> &mesh, int mesh_render_flags = 0);
	bool applyMoveMeshProceduralAsync(const Ptr<Mesh> &mesh_ram, const Ptr<MeshRender> &mesh_vram);
	bool deleteDynamicMesh();
	bool runGenerateMeshProceduralAsync(CallbackBase1<Ptr<Mesh>> *callback_generate, CallbackBase *callback_done, int mesh_render_flags = 0);
	bool runGenerateMeshProceduralAsync(CallbackBase1<Ptr<Mesh>> *callback_generate, int mesh_render_flags = 0);
	bool runGenerateMeshProceduralForce(CallbackBase1<Ptr<Mesh>> *callback_generate, CallbackBase *callback_done, int mesh_render_flags = 0);
	bool runGenerateMeshProceduralForce(CallbackBase1<Ptr<Mesh>> *callback_generate, int mesh_render_flags = 0);
	bool isMeshNull() const;
	bool isMeshLoadedRAM() const;
	bool isMeshLoadedVRAM() const;
	void setMeshPath(const char *path);
	const char *getMeshPath() const;
	int getStatDrawCalls() const;
	int getStatDrawCountViewport() const;
	int getStatDrawCountReflection() const;
	int getStatDrawCountShadow() const;
	long long getStatFrame() const;
};
typedef Ptr<DecalMesh> DecalMeshPtr;
typedef Ptr<const DecalMesh> ConstDecalMeshPtr;
typedef const DecalMesh ConstDecalMesh;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API DecalOrtho : public Decal
{
public:
	typedef const DecalOrtho ConstDecalOrtho;

	static Node::TYPE type() { return Node::DECAL_ORTHO; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<DecalOrtho> create();
	void setHeight(float height);
	float getHeight() const;
	void setWidth(float width);
	float getWidth() const;
	void setZNear(float znear);
	float getZNear() const;
	Math::mat4 getProjection() const;
};
typedef Ptr<DecalOrtho> DecalOrthoPtr;
typedef Ptr<const DecalOrtho> ConstDecalOrthoPtr;
typedef const DecalOrtho ConstDecalOrtho;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API DecalProj : public Decal
{
public:
	typedef const DecalProj ConstDecalProj;

	static Node::TYPE type() { return Node::DECAL_PROJ; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<DecalProj> create();
	void setAspect(float aspect);
	float getAspect() const;
	void setFov(float fov);
	float getFov() const;
	void setZNear(float znear);
	float getZNear() const;
	Math::mat4 getProjection() const;
};
typedef Ptr<DecalProj> DecalProjPtr;
typedef Ptr<const DecalProj> ConstDecalProjPtr;
typedef const DecalProj ConstDecalProj;

} // namespace Unigine
