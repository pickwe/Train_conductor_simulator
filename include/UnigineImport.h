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

#include "UniginePtr.h"
#include "UnigineMathLib.h"
#include "UnigineMesh.h"
#include "UnigineLights.h"
#include "UniginePlayers.h"
#include "UnigineSkeleton.h"

namespace Unigine
{

class ImportMesh;
class ImportMeshSkinned;
class ImportLight;
class ImportCamera;
class ImportSkeleton;
class Importer;

class UNIGINE_API ImportNode : public APIInterface
{
public:
	typedef const ImportNode ConstImportNode;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportNode> create();
	void setName(const char *name);
	const char *getName() const;
	void setFilepath(const char *filepath);
	const char *getFilepath() const;
	void setParent(const Ptr<ImportNode> &parent);
	Ptr<ImportNode> getParent() const;
	void addChild(const Ptr<ImportNode> &node);
	bool removeChild(const Ptr<ImportNode> &node);
	Ptr<ImportNode> getChild(int index) const;
	int getNumChildren() const;
	void setMesh(const Ptr<ImportMesh> &mesh);
	Ptr<ImportMesh> getMesh() const;
	void setMeshSkinned(const Ptr<ImportMeshSkinned> &skinned);
	Ptr<ImportMeshSkinned> getMeshSkinned() const;
	void setLight(const Ptr<ImportLight> &light);
	Ptr<ImportLight> getLight() const;
	void setCamera(const Ptr<ImportCamera> &camera);
	Ptr<ImportCamera> getCamera() const;
	void setTransform(const Math::dmat4 &transform);
	Math::dmat4 getTransform() const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportNode> ImportNodePtr;
typedef Ptr<const ImportNode> ConstImportNodePtr;
typedef const ImportNode ConstImportNode;


class UNIGINE_API ImportLight : public APIInterface
{
public:
	typedef const ImportLight ConstImportLight;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportLight> create();
	void setNode(const Ptr<ImportNode> &node);
	Ptr<ImportNode> getNode() const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportLight> ImportLightPtr;
typedef Ptr<const ImportLight> ConstImportLightPtr;
typedef const ImportLight ConstImportLight;


class UNIGINE_API ImportCamera : public APIInterface
{
public:
	typedef const ImportCamera ConstImportCamera;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportCamera> create();
	void setNode(const Ptr<ImportNode> &node);
	Ptr<ImportNode> getNode() const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportCamera> ImportCameraPtr;
typedef Ptr<const ImportCamera> ConstImportCameraPtr;
typedef const ImportCamera ConstImportCamera;


class UNIGINE_API ImportAnimation : public APIInterface
{
public:
	typedef const ImportAnimation ConstImportAnimation;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportAnimation> create();
	void setName(const char *name);
	const char *getName() const;
	void setFilepath(const char *filepath);
	const char *getFilepath() const;
	void setMinTime(float time);
	float getMinTime() const;
	void setMaxTime(float time);
	float getMaxTime() const;
	void setSkeleton(const Ptr<ImportSkeleton> &skeleton);
	Ptr<ImportSkeleton> getSkeleton() const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportAnimation> ImportAnimationPtr;
typedef Ptr<const ImportAnimation> ConstImportAnimationPtr;
typedef const ImportAnimation ConstImportAnimation;


class UNIGINE_API ImportTexture : public APIInterface
{
public:
	typedef const ImportTexture ConstImportTexture;
	static constexpr bool HAS_OWNERSHIP = false;

	static Ptr<ImportTexture> create();
	void setFilepath(const char *filepath);
	const char *getFilepath() const;
	void setOriginalFilepath(const char *filepath);
	const char *getOriginalFilepath() const;
	void setPreset(const char *preset);
	const char *getPreset() const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportTexture> ImportTexturePtr;
typedef Ptr<const ImportTexture> ConstImportTexturePtr;
typedef const ImportTexture ConstImportTexture;


class UNIGINE_API ImportMaterial : public APIInterface
{
public:
	typedef const ImportMaterial ConstImportMaterial;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportMaterial> create();
	void setName(const char *name);
	const char *getName() const;
	void setFilepath(const char *filepath);
	const char *getFilepath() const;
	int getNumParameters() const;
	Math::vec4 getParameter(int index) const;
	const char *getParameterName(int index) const;
	void setParameter(const char *name, const Math::vec4 &value);
	bool hasParameter(const char *name) const;
	void removeParameter(const char *name);
	Math::vec4 getParameter(const char *name) const;
	int getNumTextures() const;
	Ptr<ImportTexture> getTexture(int index) const;
	const char *getTextureName(int index) const;
	void setTexture(const char *name, const Ptr<ImportTexture> &value);
	bool hasTexture(const char *name) const;
	void removeTexture(const char *name);
	Ptr<ImportTexture> getTexture(const char *name) const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportMaterial> ImportMaterialPtr;
typedef Ptr<const ImportMaterial> ConstImportMaterialPtr;
typedef const ImportMaterial ConstImportMaterial;


class UNIGINE_API ImportSurface : public APIInterface
{
public:
	typedef const ImportSurface ConstImportSurface;
	static constexpr bool HAS_OWNERSHIP = true;

	void copyFrom(const Ptr<ImportSurface> &o);
	void setName(const char *name);
	const char *getName() const;
	void setMinVisibleDistance(float distance);
	float getMinVisibleDistance() const;
	void setMaxVisibleDistance(float distance);
	float getMaxVisibleDistance() const;
	void setMinFadeDistance(float distance);
	float getMinFadeDistance() const;
	void setMaxFadeDistance(float distance);
	float getMaxFadeDistance() const;
	void setMaterial(const Ptr<ImportMaterial> &material);
	Ptr<ImportMaterial> getMaterial() const;
	void setSourceIndex(int index);
	int getSourceIndex() const;
	void setTargetSurface(int surface);
	int getTargetSurface() const;
	void setBoundBox(const Math::WorldBoundBox &box);
	Math::WorldBoundBox getBoundBox() const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportSurface> ImportSurfacePtr;
typedef Ptr<const ImportSurface> ConstImportSurfacePtr;
typedef const ImportSurface ConstImportSurface;


class UNIGINE_API ImportGeometry : public APIInterface
{
public:
	typedef const ImportGeometry ConstImportGeometry;
	static constexpr bool HAS_OWNERSHIP = true;

	void copyFrom(const Ptr<ImportGeometry> &o);
	Ptr<ImportSurface> addSurface();
	void copySurfacesFrom(const Ptr<ImportGeometry> &other);
	void moveSurfacesFrom(const Ptr<ImportGeometry> &other);
	Ptr<ImportSurface> getSurface(int index) const;
	int getNumSurfaces() const;
	void clearSurfaces();
	void setTransform(const Math::dmat4 &transform);
	Math::dmat4 getTransform() const;
	void setBoundBox(const Math::WorldBoundBox &box);
	Math::WorldBoundBox getBoundBox() const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportGeometry> ImportGeometryPtr;
typedef Ptr<const ImportGeometry> ConstImportGeometryPtr;
typedef const ImportGeometry ConstImportGeometry;


class UNIGINE_API ImportMesh : public APIInterface
{
public:
	typedef const ImportMesh ConstImportMesh;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportMesh> create();
	void setName(const char *name);
	const char *getName() const;
	void setFilepath(const char *filepath);
	const char *getFilepath() const;
	int getNumNodes() const;
	void addNode(const Ptr<ImportNode> &node);
	bool removeNode(const Ptr<ImportNode> &node);
	Ptr<ImportNode> getNode(int index) const;
	int getNumGeometries() const;
	Ptr<ImportGeometry> addGeometry();
	void copyGeometriesFrom(const Ptr<ImportMesh> &other);
	void moveGeometriesFrom(const Ptr<ImportMesh> &other);
	Ptr<ImportGeometry> getGeometry(int index) const;
	void clearGeometries();
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportMesh> ImportMeshPtr;
typedef Ptr<const ImportMesh> ConstImportMeshPtr;
typedef const ImportMesh ConstImportMesh;


class UNIGINE_API ImportMeshSkinned : public APIInterface
{
public:
	typedef const ImportMeshSkinned ConstImportMeshSkinned;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportMeshSkinned> create();
	void setName(const char *name);
	const char *getName() const;
	void setFilepath(const char *filepath);
	const char *getFilepath() const;
	void setAnimationFilepath(const char *filepath);
	const char *getAnimationFilepath() const;
	int getNumNodes() const;
	void addNode(const Ptr<ImportNode> &node);
	bool removeNode(const Ptr<ImportNode> &node);
	Ptr<ImportNode> getNode(int index) const;
	int getNumGeometries() const;
	Ptr<ImportGeometry> addGeometry();
	void copyGeometriesFrom(const Ptr<ImportMeshSkinned> &other);
	void moveGeometriesFrom(const Ptr<ImportMeshSkinned> &other);
	Ptr<ImportGeometry> getGeometry(int index) const;
	void clearGeometries();
	void setSkeleton(const Ptr<ImportSkeleton> &skeleton);
	Ptr<ImportSkeleton> getSkeleton() const;
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportMeshSkinned> ImportMeshSkinnedPtr;
typedef Ptr<const ImportMeshSkinned> ConstImportMeshSkinnedPtr;
typedef const ImportMeshSkinned ConstImportMeshSkinned;


class UNIGINE_API ImportSkeleton : public APIInterface
{
public:
	typedef const ImportSkeleton ConstImportSkeleton;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportSkeleton> create();
	void setName(const char *name);
	const char *getName() const;
	void setFilepath(const char *filepath);
	const char *getFilepath() const;
	int getNumJointNodes() const;
	Ptr<ImportNode> addJointNode();
	Ptr<ImportNode> getJointNode(int index) const;
	int getJointNodeIndex(const Ptr<ImportNode> &node) const;
	void createSortedJointsHierarchy();
	void setData(void *data);
	void * getData() const;
};
typedef Ptr<ImportSkeleton> ImportSkeletonPtr;
typedef Ptr<const ImportSkeleton> ConstImportSkeletonPtr;
typedef const ImportSkeleton ConstImportSkeleton;


class UNIGINE_API ImportScene : public APIInterface
{
public:
	typedef const ImportScene ConstImportScene;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportScene> create();
	int getNumNodes() const;
	Ptr<ImportNode> getNode(int i) const;
	Ptr<ImportNode> addNode(const Ptr<ImportNode> &parent);
	bool removeNode(const Ptr<ImportNode> &node);
	int getNumMeshes() const;
	Ptr<ImportMesh> getMesh(int i) const;
	Ptr<ImportMesh> addMesh(const Ptr<ImportNode> &node);
	void removeMesh(const Ptr<ImportMesh> &mesh);
	int getNumMeshSkinneds() const;
	Ptr<ImportMeshSkinned> getMeshSkinned(int i) const;
	Ptr<ImportMeshSkinned> addMeshSkinned(const Ptr<ImportNode> &node);
	void removeMeshSkinned(const Ptr<ImportMeshSkinned> &mesh_skinned);
	int getNumLights() const;
	Ptr<ImportLight> getLight(int i) const;
	Ptr<ImportLight> addLight(const Ptr<ImportNode> &node);
	void removeLight(const Ptr<ImportLight> &light);
	int getNumCameras() const;
	Ptr<ImportCamera> getCamera(int i) const;
	Ptr<ImportCamera> addCamera(const Ptr<ImportNode> &node);
	void removeCamera(const Ptr<ImportCamera> &camera);
	int getNumAnimations() const;
	Ptr<ImportAnimation> getAnimation(int i) const;
	Ptr<ImportAnimation> addAnimation();
	void removeAnimation(const Ptr<ImportAnimation> &animation);
	int getNumSkeletons() const;
	Ptr<ImportSkeleton> getSkeleton(int i) const;
	Ptr<ImportSkeleton> addSkeleton();
	void removeSkeleton(const Ptr<ImportSkeleton> &skeleton);
	int getNumMaterials() const;
	Ptr<ImportMaterial> getMaterial(int i) const;
	Ptr<ImportMaterial> addMaterial();
	Ptr<ImportMaterial> getMaterial(const char *name) const;
	void replaceMaterial(const Ptr<ImportMaterial> &old_material, const Ptr<ImportMaterial> &new_material);
	void removeMaterial(const Ptr<ImportMaterial> &material);
	int getNumTextures() const;
	Ptr<ImportTexture> getTexture(int i) const;
	Ptr<ImportTexture> getTexture(const char *filepath);
	Ptr<ImportTexture> findTexture(const char *filepath) const;
};
typedef Ptr<ImportScene> ImportScenePtr;
typedef Ptr<const ImportScene> ConstImportScenePtr;
typedef const ImportScene ConstImportScene;


class UNIGINE_API ImportProcessor : public APIInterface
{
public:
	typedef const ImportProcessor ConstImportProcessor;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ImportProcessor> create();
	ImportProcessor();
	ImportProcessor(void *internal);
	~ImportProcessor() override;
	Ptr<ImportProcessor> getImportProcessor() const;
	void setImporter(const Ptr<Importer> &importer);
	Ptr<Importer> getImporter() const;
	void setOutputPath(const char *path);
	const char *getOutputPath() const;
	bool processScene(const Ptr<ImportScene> &scene);
	bool processTexture(const Ptr<ImportTexture> &import_texture);
	bool processMesh(const Ptr<Mesh> &mesh, const Ptr<ImportMesh> &import_mesh);
	bool processMeshSkinned(const Ptr<MeshSkinned> &mesh_skinned, const Ptr<ImportMeshSkinned> &import_mesh_skinned);
	bool processLight(const Ptr<Light> &light, const Ptr<ImportLight> &import_light);
	bool processCamera(const Ptr<Player> &camera, const Ptr<ImportCamera> &import_camera);
	bool processAnimation(const Ptr<MeshSkinnedAnimation> &animation, const Ptr<ImportAnimation> &import_animation);
	bool processAnimation(const Ptr<MeshSkinnedAnimation> &animation, const Ptr<ImportMeshSkinned> &import_mesh_skinned, const Ptr<ImportAnimation> &import_animation);
	bool processNode(const Ptr<Node> &node, const Ptr<ImportNode> &import_node);
	bool processNodeChild(const Ptr<Node> &node_parent, const Ptr<ImportNode> &import_node_parent, const Ptr<Node> &node_child, const Ptr<ImportNode> &import_node_child);
	bool processMaterial(const Ptr<Material> &material, const Ptr<ImportMaterial> &import_material);
	bool processSkeleton(const Ptr<Skeleton> &skeleton, const Ptr<ImportSkeleton> &import_skeleton);
protected:
	friend class ImportProcessorWrapper;
	virtual bool onProcessScene(const Ptr<ImportScene> &scene);
	virtual bool onProcessTexture(const Ptr<ImportTexture> &import_texture);
	virtual bool onProcessMesh(const Ptr<Mesh> &mesh, const Ptr<ImportMesh> &import_mesh);
	virtual bool onProcessMeshSkinned(const Ptr<MeshSkinned> &mesh_skinned, const Ptr<ImportMeshSkinned> &import_mesh_skinned);
	virtual bool onProcessLight(const Ptr<Light> &light, const Ptr<ImportLight> &import_light);
	virtual bool onProcessCamera(const Ptr<Player> &camera, const Ptr<ImportCamera> &import_camera);
	virtual bool onProcessAnimation(const Ptr<MeshSkinnedAnimation> &animation, const Ptr<ImportAnimation> &import_animation);
	virtual bool onProcessAnimation(const Ptr<MeshSkinnedAnimation> &animation, const Ptr<ImportMeshSkinned> &import_mesh_skinned, const Ptr<ImportAnimation> &import_animation);
	virtual bool onProcessNode(const Ptr<Node> &node, const Ptr<ImportNode> &import_node);
	virtual bool onProcessNodeChild(const Ptr<Node> &node_parent, const Ptr<ImportNode> &import_node_parent, const Ptr<Node> &node_child, const Ptr<ImportNode> &import_node_child);
	virtual bool onProcessMaterial(const Ptr<Material> &material, const Ptr<ImportMaterial> &import_material);
	virtual bool onProcessSkeleton(const Ptr<Skeleton> &skeleton, const Ptr<ImportSkeleton> &import_skeleton);
};
typedef Ptr<ImportProcessor> ImportProcessorPtr;
typedef Ptr<const ImportProcessor> ConstImportProcessorPtr;
typedef const ImportProcessor ConstImportProcessor;


class UNIGINE_API Importer : public APIInterface
{
public:
	typedef const Importer ConstImporter;
	static constexpr bool HAS_OWNERSHIP = true;


	enum
	{
		IMPORT_LIGHTS = 1 << 0,
		IMPORT_CAMERAS = 1 << 1,
		IMPORT_MESHES = 1 << 2,
		IMPORT_ANIMATIONS = 1 << 3,
		IMPORT_TEXTURES = 1 << 4,
		IMPORT_MATERIALS = 1 << 5,
		IMPORT_SKELETONS = 1 << 6,
	};

	enum Axis
	{
		None = -1,
		X,
		NX,
		Y,
		NY,
		Z,
		NZ,
	};
	static Ptr<Importer> create();
	Importer();
	Importer(void *internal);
	~Importer() override;
	Ptr<Importer> getImporter() const;
	bool containsParameter(const char *name) const;
	void setParameterInt(const char *name, int v);
	int getParameterInt(const char *name) const;
	void setParameterFloat(const char *name, float v);
	float getParameterFloat(const char *name) const;
	void setParameterDouble(const char *name, double v);
	double getParameterDouble(const char *name) const;
	void setParameterString(const char *name, const char *v);
	const char *getParameterString(const char *name) const;
	bool addPreProcessor(const char *type_name);
	void removePreProcessor(const char *type_name);
	bool hasPreProcessor(const char *type_name);
	bool addPostProcessor(const char *type_name);
	void removePostProcessor(const char *type_name);
	bool hasPostProcessor(const char *type_name);
	void setMeshesProcessor(const char *processor);
	const char *getMeshesProcessor() const;
	void setMeshSkinnedsProcessor(const char *processor);
	const char *getMeshSkinnedsProcessor() const;
	void setNodesProcessor(const char *processor);
	const char *getNodesProcessor() const;
	void setTexturesProcessor(const char *processor);
	const char *getTexturesProcessor() const;
	void setMaterialsProcessor(const char *processor);
	const char *getMaterialsProcessor() const;
	void setCamerasProcessor(const char *processor);
	const char *getCamerasProcessor() const;
	void setLightsProcessor(const char *processor);
	const char *getLightsProcessor() const;
	void setAnimationsProcessor(const char *processor);
	const char *getAnimationsProcessor() const;
	void setSkeletonsProcessor(const char *processor);
	const char *getSkeletonsProcessor() const;
	Ptr<ImportScene> getScene() const;
	bool init(const char *filepath, int flags = ~0);
	bool import(const char *output_path);
	const char *getSourceFilepath() const;
	void setOutputFilepath(const char *filepath);
	const char *getOutputFilepath() const;
	int getFlags() const;
	bool computeBoundBox(const Ptr<ImportMesh> &import_mesh);
	bool preprocess();
	Ptr<Node> convertNode(const Ptr<ImportProcessor> &processor, const Ptr<ImportNode> &root_node);
	bool importTexture(const Ptr<ImportProcessor> &processor, const Ptr<ImportTexture> &import_texture);
	bool importMaterial(const Ptr<ImportProcessor> &processor, const Ptr<Material> &material, const Ptr<ImportMaterial> &import_material);
	bool importMesh(const Ptr<ImportProcessor> &processor, const Ptr<Mesh> &mesh, const Ptr<ImportMesh> &import_mesh);
	bool importMeshSkinned(const Ptr<ImportProcessor> &processor, const Ptr<MeshSkinned> &mesh_skinned, const Ptr<ImportMeshSkinned> &import_mesh_skinned);
	Ptr<Light> importLight(const Ptr<ImportProcessor> &processor, const Ptr<ImportLight> &import_light);
	Ptr<Player> importCamera(const Ptr<ImportProcessor> &processor, const Ptr<ImportCamera> &import_camera);
	bool importAnimation(const Ptr<ImportProcessor> &processor, const Ptr<MeshSkinnedAnimation> &animation, const Ptr<ImportAnimation> &import_animation);
	bool importAnimation(const Ptr<ImportProcessor> &processor, const Ptr<MeshSkinnedAnimation> &animation, const Ptr<ImportMeshSkinned> &import_mesh_skinned, const Ptr<ImportAnimation> &import_animation);
	bool importSkeleton(const Ptr<ImportProcessor> &processor, const Ptr<Skeleton> &skeleton, const Ptr<ImportSkeleton> &import_skeleton);
	Ptr<Node> importNode(const Ptr<ImportProcessor> &processor, const Ptr<ImportNode> &import_node);
	bool importNodeChild(const Ptr<ImportProcessor> &processor, const Ptr<Node> &node_parent, const Ptr<ImportNode> &import_node_parent, const Ptr<Node> &node_child, const Ptr<ImportNode> &import_node_child);
	bool postprocess();
	bool checkSupportedAnimation(const Ptr<ImportMeshSkinned> &import_mesh_skinned, const Ptr<ImportAnimation> &import_animation) const;
	bool checkDefaultAnimation(const Ptr<ImportMeshSkinned> &import_mesh_skinned, const Ptr<ImportAnimation> &import_animation) const;
	static bool getBasis(Importer::Axis up_axis, Importer::Axis front_axis, Math::dmat4 &ret);
protected:
	friend class ImporterWrapper;
	virtual bool onComputeBoundBox(const Ptr<ImportMesh> &import_mesh);
	virtual bool onInit(const Ptr<ImportScene> &import_scene, const char *filepath);
	virtual bool onImport(const char *output_path);
	virtual bool onImportTexture(const Ptr<ImportProcessor> &processor, const Ptr<ImportTexture> &import_texture);
	virtual bool onImportMaterial(const Ptr<ImportProcessor> &processor, const Ptr<Material> &material, const Ptr<ImportMaterial> &import_material);
	virtual Ptr<Light> onImportLight(const Ptr<ImportProcessor> &processor, const Ptr<ImportLight> &import_light);
	virtual Ptr<Player> onImportCamera(const Ptr<ImportProcessor> &processor, const Ptr<ImportCamera> &import_camera);
	virtual bool onImportMesh(const Ptr<ImportProcessor> &processor, const Ptr<Mesh> &mesh, const Ptr<ImportMesh> &import_mesh);
	virtual bool onImportMeshSkinned(const Ptr<ImportProcessor> &processor, const Ptr<MeshSkinned> &mesh_skinned, const Ptr<ImportMeshSkinned> &import_mesh_skinned);
	virtual Ptr<Node> onImportNode(const Ptr<ImportProcessor> &processor, const Ptr<ImportNode> &import_node);
	virtual bool onImportNodeChild(const Ptr<ImportProcessor> &processor, const Ptr<Node> &node_parent, const Ptr<ImportNode> &import_node_parent, const Ptr<Node> &node_child, const Ptr<ImportNode> &import_node_child);
	virtual bool onImportAnimation(const Ptr<ImportProcessor> &processor, const Ptr<MeshSkinnedAnimation> &animation, const Ptr<ImportAnimation> &import_animation);
	virtual bool onImportAnimation(const Ptr<ImportProcessor> &processor, const Ptr<MeshSkinnedAnimation> &animation, const Ptr<ImportMeshSkinned> &import_mesh_skinned, const Ptr<ImportAnimation> &import_animation);
	virtual bool onImportSkeleton(const Ptr<ImportProcessor> &processor, const Ptr<Skeleton> &skeleton, const Ptr<ImportSkeleton> &import_skeleton);
	virtual bool onCheckSupportedAnimation(const Ptr<ImportMeshSkinned> &import_mesh_skinned, const Ptr<ImportAnimation> &import_animation) const;
	virtual bool onCheckDefaultAnimation(const Ptr<ImportMeshSkinned> &import_mesh_skinned, const Ptr<ImportAnimation> &import_animation) const;
};
typedef Ptr<Importer> ImporterPtr;
typedef Ptr<const Importer> ConstImporterPtr;
typedef const Importer ConstImporter;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API Import
{
public:
	static bool isInitialized();
	struct ImporterID
	{
		String vendor_name;
		String importer_name;
		friend UNIGINE_INLINE bool operator==(const ImporterID &id0, const ImporterID &id1)
		{
			return id0.vendor_name == id1.vendor_name && id0.importer_name == id1.importer_name;
		}
		friend UNIGINE_INLINE bool operator!=(const ImporterID &id0, const ImporterID &id1)
		{
			return id0.vendor_name != id1.vendor_name || id0.importer_name != id1.importer_name;
		}
	};
	using ImporterCreationFunction = Importer * (*)(void *args);
	using ImporterDeletionFunction = void (*)(Importer *importer, void *args);
	static void *registerImporter(const ImporterID &id, const char *extension, ImporterCreationFunction creation_func, ImporterDeletionFunction deletion_func, void *args = nullptr, int priority = 0);
	static void *registerImporter(const char *vendor_name, const char *importer_name, const char *extension, ImporterCreationFunction creation_func, ImporterDeletionFunction deletion_func, void *args = nullptr, int priority = 0);
	static bool containsImporter(const Import::ImporterID &id, const char *extension);
	static bool containsImporter(const char *vendor_name, const char *importer_name, const char *extension);
	static bool unregisterImporter(void *handle);
	static int getImporterPriority(const Import::ImporterID &id, const char *extension);
	static int getImporterPriority(const char *vendor_name, const char *importer_name, const char *extension);
	static bool isSupportedExtension(const char *extension);
	static Vector<String> getSupportedExtensions();
	static Vector<String> getImporterExtensions(const char *vendor_name, const char *importer_name);
	static Vector<String> getImporterExtensions(const Import::ImporterID &id);
	static Vector<Import::ImporterID> getImporterIDsByExtension(const char *extension, bool sort_by_priority = true);
	static Vector<Import::ImporterID> getImporterIDsByExtension(const char *vendor_name, const char *extension, bool sort_by_priority = true);
	static Vector<Import::ImporterID> getImporterIDsByExtension(const Import::ImporterID &id, const char *extension, bool sort_by_priority = true);
	static Vector<Import::ImporterID> getImporterIDsByExtension(const char *vendor_name, const char *importer_name, const char *extension, bool sort_by_priority = true);
	static Vector<Import::ImporterID> getImporterIDs();
	static Ptr<Importer> createImporter(const Import::ImporterID &id, const char *extension);
	static Ptr<Importer> createImporter(const char *vendor_name, const char *importer_name, const char *extension);
	static Ptr<Importer> createImporterByFileName(const char *filename);
	static Ptr<Importer> createImporterByFileName(const char *vendor_name, const char *filename);
	static Ptr<Importer> createImporterByFileName(const Import::ImporterID &id, const char *filename);
	static Ptr<Importer> createImporterByFileName(const char *vendor_name, const char *importer_name, const char *filename);
	static String doImport(const char *filepath, const char *output_path);
	static String doImport(const char *vendor_name, const char *filepath, const char *output_path);
	static String doImport(const Import::ImporterID &id, const char *filepath, const char *output_path);
	static String doImport(const char *vendor_name, const char *importer_name, const char *filepath, const char *output_path);
	using ImportProcessorCreationFunction = ImportProcessor * (*)(void *args);
	using ImportProcessorDeletionFunction = void (*)(ImportProcessor *processor, void *args);
	static void *registerImportProcessor(const char *type_name, ImportProcessorCreationFunction creation_func, ImportProcessorDeletionFunction deletion_function, void *args = nullptr);
	static bool containsImportProcessor(const char *type_name);
	static Ptr<ImportProcessor> createImportProcessor(const char *type_name);
	static bool unregisterImportProcessor(void *handle);
};

} // namespace Unigine
