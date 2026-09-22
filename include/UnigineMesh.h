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
#include "UnigineVector.h"
#include "UnigineEllipsoid.h"
#include "UnigineSkeleton.h"
#include "UnigineGUID.h"

namespace Unigine
{


class UNIGINE_API MeshSkinnedAnimation : public APIInterface
{
public:
	typedef const MeshSkinnedAnimation ConstMeshSkinnedAnimation;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<MeshSkinnedAnimation> create();
	static Ptr<MeshSkinnedAnimation> create(const Ptr<ConstMeshSkinnedAnimation> &src);
	static Ptr<MeshSkinnedAnimation> create(const char *path);
	void initSourceSkeleton(const Ptr<ConstSkeleton> &skeleton);
	void initSourceSkeleton(const char *path);
	Ptr<ConstSkeleton> getSharedSkeleton() const;
	void setSharedSkeletonFileGUID(const UGUID &sharedskeletonfileguid);
	UGUID getSharedSkeletonFileGUID() const;
	void setSharedSkeletonPath(const char *path);
	const char *getSharedSkeletonPath() const;
	void assignFrom(const Ptr<MeshSkinnedAnimation> &src);
	void clear();
	void flipYZ();
	size_t getMemoryUsage() const;
	int info(const char *path) const;
	int load(const char *path);
	int loadJoints(const char *path);
	int save(const char *path) const;
	int getNumSrcJoints() const;
	int findSrcJoint(const char *name) const;
	const char *getSrcJointName(int bone) const;
	int getSrcJointParent(int bone) const;
	int getNumAnimatedJoints() const;
	void setAnimatedJointID(int index, int src_joint_id);
	int getAnimatedJointID(int index) const;
	void getAnimatedJoints(Vector<int> &joints) const;
	void setAnimatedJoints(const Vector<int> &joints);
	void addAnimatedJoint(int src_joint);
	void removeAnimatedJoint(int index);
	const char *getAnimatedJointName(int index) const;
	int findAnimatedJointByID(int src_joint_id) const;
	int findAnimatedJointByName(const char *name) const;
	void setFps(int fps);
	int getFps() const;
	float getDuration() const;
	void setRootMotionPresent(bool present);
	bool isRootMotionPresent() const;
	void setNumFrames(int frames);
	int getNumFrames() const;
	Math::Transform getFrameJointTransform(int frame, int joint) const;
	void setFrame(int frame, const Vector<Math::mat4> &joints);
	void setFrame(int frame, int index, const Math::mat4 &joint);
	void getFrame(int frame, Vector<Math::mat4> &joints) const;
	bool getPoseByFrame(const Ptr<SkeletonPoseDecomposed> &out_pose, float frame, bool is_loop = false) const;
	bool getPoseByTime(const Ptr<SkeletonPoseDecomposed> &out_pose, float time, bool is_loop = false) const;
	bool getPoseByNormalizedTime(const Ptr<SkeletonPoseDecomposed> &out_pose, float normalized_time, bool is_loop = false) const;
	bool updatePoseByFrame(const Ptr<SkeletonPoseDecomposed> &out_pose, float frame, bool is_loop = false) const;
	bool updatePoseByTime(const Ptr<SkeletonPoseDecomposed> &out_pose, float time, bool is_loop = false) const;
	bool updatePoseByNormalizedTime(const Ptr<SkeletonPoseDecomposed> &out_pose, float normalized_time, bool is_loop = false) const;
	Math::Transform extractRootMotionByFrames(float begin_frame, float end_frame, bool is_forward) const;
	Math::Transform extractRootMotionByTimes(float begin_time, float end_time, bool is_forward) const;
	int getNumMarkerSyncs() const;
	int addMarkerSync(const char *name, float time);
	void removeMarkerSync(int index);
	void clearMarkerSyncs();
	const char *getMarkerSyncName(int index) const;
	unsigned int getMarkerSyncNameHash(int index) const;
	void setMarkerSyncName(int index, const char *name);
	float getMarkerSyncTime(int index) const;
	void setMarkerSyncTime(int index, float time);
	int getMarkerSyncId(int index) const;
	void setMarkerSyncId(int index, int id);
	int findMarkerSyncById(int id) const;
	bool getMarkerSyncInterval(float current_time, bool is_loop, unsigned int &prev_hash, unsigned int &next_hash, float &ratio, const Vector<unsigned int> &common_hashes) const;
	float getTimeFromMarkerSyncInterval(unsigned int prev_hash, unsigned int next_hash, float ratio, float current_time, bool is_loop, const Vector<unsigned int> &common_hashes) const;
};
typedef Ptr<MeshSkinnedAnimation> MeshSkinnedAnimationPtr;
typedef Ptr<const MeshSkinnedAnimation> ConstMeshSkinnedAnimationPtr;
typedef const MeshSkinnedAnimation ConstMeshSkinnedAnimation;


class UNIGINE_API Mesh : public APIInterface
{
public:
	typedef const Mesh ConstMesh;
	static constexpr bool HAS_OWNERSHIP = true;


	enum LIGHTMAP_RESOLUTION
	{
		LIGHTMAP_RESOLUTION_MODE_32,
		LIGHTMAP_RESOLUTION_MODE_64,
		LIGHTMAP_RESOLUTION_MODE_128,
		LIGHTMAP_RESOLUTION_MODE_256,
		LIGHTMAP_RESOLUTION_MODE_512,
		LIGHTMAP_RESOLUTION_MODE_1024,
		LIGHTMAP_RESOLUTION_MODE_2048,
		LIGHTMAP_RESOLUTION_MODE_4096,
	};

	enum
	{
		VERTEX_CACHE = 1 << 0,	// vertex cache optimization
		BACK_TO_FRONT = 1 << 1,	// back to front order
		VERTEX_CACHE_LEGACY = 1 << 2,
		BACK_TO_FRONT_LEGACY = 1 << 3,
		NUM_WEIGHTS = 4,	// vertex cache optimization
	};

	enum
	{
		COLLISION_DATA_BOUNDS = 1 << 0,
		COLLISION_DATA_EDGES = 1 << 1,
		COLLISION_DATA_SPATIAL_TREE = 1 << 3,
		COLLISION_DATA_ALL = COLLISION_DATA_BOUNDS | COLLISION_DATA_EDGES | COLLISION_DATA_SPATIAL_TREE,
	};
	static Ptr<Mesh> create();
	static Ptr<Mesh> create(const char *path);
	static Ptr<Mesh> create(const Ptr<ConstMesh> &constmesh);
	void assignFrom(const Ptr<ConstMesh> &mesh);
	void swap(const Ptr<Mesh> &mesh);
	int info(const char *path) const;
	int load(const char *path);
	int save(const char *path) const;
	void clear();
	bool flipYZ(int surface = -1);
	bool flipTangent(int surface = -1);
	bool hasSpatialTree(int surface = -1) const;
	void clearSpatialTree(int surface = -1);
	void createSpatialTree(int surface = -1);
	void setSpatialTreeTriangles(int triangles);
	int getSpatialTreeTriangles() const;
	bool hasEdges(int surface = -1) const;
	void clearEdges(int surface = -1);
	void createEdges(int surface = -1);
	void createBounds(int surface = -1);
	bool hasCollisionData(int surface = -1, int flags = Mesh::COLLISION_DATA_ALL) const;
	void clearCollisionData(int surface = -1, int flags = Mesh::COLLISION_DATA_ALL);
	void createCollisionData(int surface = -1, int flags = Mesh::COLLISION_DATA_ALL);
	bool removeIndices(int surface = -1);
	bool createIndices(int surface = -1);
	bool optimizeIndices(int flags, int surface = -1);
	bool createNormals(int surface = -1);
	bool createNormals(float angle, int surface = -1);
	bool createTangents(int surface = -1);
	bool createTangents(float angle, const Vector<int> &surfaces);
	bool getRandomPoint(Math::vec3 &ret_point, Math::vec3 &ret_normal, Math::vec3 &ret_velocity, int surface) const;
	bool getIntersection(const Math::vec3 &p0, const Math::vec3 &p1, Math::vec3 *ret_point, Math::vec3 *ret_normal, int *ret_index, int surface) const;
	bool getIntersection(const Math::dvec3 &p0, const Math::dvec3 &p1, Math::dvec3 *ret_point, Math::vec3 *ret_normal, int *ret_index, int surface) const;
	int getNumSurfaces() const;
	int findSurface(const char *name) const;
	void sortSurfaces();
	void clearSurface(int surface = -1);
	int addSurface(const char *name = 0);
	void setSurfaceName(int surface, const char *name);
	const char *getSurfaceName(int surface) const;
	void setSurfaceLightmapUVChannel(int surface, char uv_channel);
	char getSurfaceLightmapUVChannel(int surface) const;
	void setSurfaceLightmapResolution(int surface, Mesh::LIGHTMAP_RESOLUTION resolution);
	Mesh::LIGHTMAP_RESOLUTION getSurfaceLightmapResolution(int surface) const;
	bool getSurfaceCollision(const Math::BoundBox &bb, Vector<int> &surfaces) const;
	bool getSurfaceCollision(Math::BoundFrustum &bf, Vector<int> &surfaces) const;
	bool getSurfaceCollision(const Math::vec3 &p0, const Math::vec3 &p1, Vector<int> &surfaces) const;
	bool getTriangleCollision(const Math::BoundBox &bb, Vector<int> &triangles, int surface) const;
	bool getTriangleCollision(const Math::BoundFrustum &bf, Vector<int> &triangles, int surface) const;
	bool getTriangleCollision(const Math::vec3 &p0, const Math::vec3 &p1, Vector<int> &triangles, int surface) const;
	typedef bool (*CollisionFilter)(const Math::BoundBox &bb);
	typedef void (*CollisionResult)(int);
	void getSurfaceCollision(CollisionFilter filter, CollisionResult surface_index_result) const;
	void getTriangleCollision(CollisionFilter filter, CollisionResult triangle_index_result, int surface) const;
	bool setSurfaceTransform(const Math::mat4 &transform, int surface = -1);
	int addMeshSurface(const char *v, const Ptr<ConstMesh> &mesh, int surface, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addMeshSurface(int v, const Ptr<ConstMesh> &mesh, int surface, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addEmptySurface(const char *name, int num_vertex, int num_indices);
	int addBoxSurface(const char *name, const Math::vec3 &size, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addPlaneSurface(const char *name, float width, float height, float step, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addSphereSurface(const char *name, float radius, int stacks, int slices, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addCapsuleSurface(const char *name, float radius, float height, int stacks, int slices, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addCylinderSurface(const char *name, float radius, float height, int stacks, int slices, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addPrismSurface(const char *name, float size_0, float size_1, float height, int sides, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addIcosahedronSurface(const char *name, float radius, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int addDodecahedronSurface(const char *name, float radius, int colision_data_flags = Mesh::COLLISION_DATA_ALL);
	int getNumCVertex(int surface = 0) const;
	int getNumTVertex(int surface = 0) const;
	void remapCVertex(int surface = 0);
	void setNumVertex(int size, int surface = 0);
	int getNumVertex(int surface) const;
	void addVertex(const Vector<Math::vec3> &vertices, int surface = 0);
	void addVertex(const Math::vec3 &vertex, int surface = 0);
	void setVertex(int num, const Math::vec3 &vertex, int surface = 0);
	Math::vec3 getVertex(int num, int surface = 0) const;
	void setNumTexCoords0(int size, int surface = 0);
	int getNumTexCoords0(int surface = 0) const;
	void addTexCoords0(const Vector<Math::vec2> &texcoords, int surface = 0);
	void addTexCoord0(const Math::vec2 &texcoord, int surface = 0);
	void setTexCoord0(int num, const Math::vec2 &texcoord, int surface = 0);
	Math::vec2 getTexCoord0(int num, int surface = 0) const;
	void setNumTexCoords1(int size, int surface = 0);
	int getNumTexCoords1(int surface = 0) const;
	void addTexCoords1(const Vector<Math::vec2> &texcoords, int surface = 0);
	void addTexCoord1(const Math::vec2 &texcoord, int surface = 0);
	void setTexCoord1(int num, const Math::vec2 &texcoord, int surface = 0);
	Math::vec2 getTexCoord1(int num, int surface = 0) const;
	void setNumNormals(int size, int surface = 0);
	int getNumNormals(int surface = 0) const;
	void addNormals(const Vector<Math::vec3> &normals, int surface = 0);
	void addNormal(const Math::vec3 &normal, int surface = 0);
	void setNormal(int num, const Math::vec3 &normal, int surface = 0);
	Math::vec3 getNormal(int num, int surface = 0) const;
	void setNumTangents(int size, int surface = 0);
	int getNumTangents(int surface) const;
	void addTangents(const Vector<Math::quat> &tangents, int surface = 0);
	void addTangent(const Math::quat &tangent, int surface = 0);
	void setTangent(int num, const Math::quat &tangent, int surface = 0);
	Math::quat getTangent(int num, int surface = 0) const;
	void setNumColors(int size, int surface = 0);
	int getNumColors(int surface = 0) const;
	void addColors(const Vector<Math::vec4> &colors, int surface = 0);
	void addColor(const Math::vec4 &color, int surface = 0);
	void setColor(int num, const Math::vec4 &color, int surface = 0);
	Math::vec4 getColor(int num, int surface = 0) const;
	void setNumCIndices(int size, int surface = 0);
	int getNumCIndices(int surface = 0) const;
	void addCIndices(const Vector<int> &indices, int surface = 0);
	void addCIndex(int index, int surface = 0);
	void setCIndex(int num, int index, int surface = 0);
	int getCIndex(int num, int surface = 0) const;
	void setNumTIndices(int size, int surface = 0);
	int getNumTIndices(int surface = 0) const;
	void addTIndices(const Vector<int> &indices, int surface = 0);
	void addTIndex(int index, int surface = 0);
	void setTIndex(int num, int index, int surface = 0);
	int getTIndex(int num, int surface = 0) const;
	void setNumIndices(int size, int surface = 0);
	int getNumIndices(int surface = 0) const;
	void addIndices(const Vector<int> &indices, int surface = 0);
	void addIndex(int index, int surface = 0);
	void setIndex(int num, int index, int surface = 0);
	int getIndex(int num, int surface = 0) const;
	Vector<Math::vec3> &getVertices(int surface = 0);
	Vector<Math::vec3> &getNormals(int surface = 0);
	Vector<Math::quat> &getTangents(int surface = 0);
	Vector<Math::vec2> &getTexCoords0(int surface = 0);
	Vector<Math::vec2> &getTexCoords1(int surface = 0);
	Vector<Math::bvec4> &getColors(int surface = 0);
	Vector<int> &getCIndices(int surface = 0);
	Vector<int> &getTIndices(int surface = 0);
	const Vector<Math::vec3> &getVertices(int surface = 0) const;
	const Vector<Math::vec3> &getNormals(int surface = 0) const;
	const Vector<Math::quat> &getTangents(int surface = 0) const;
	const Vector<Math::vec2> &getTexCoords0(int surface = 0) const;
	const Vector<Math::vec2> &getTexCoords1(int surface = 0) const;
	const Vector<Math::bvec4> &getColors(int surface = 0) const;
	const Vector<int> &getCIndices(int surface = 0) const;
	const Vector<int> &getTIndices(int surface = 0) const;
	Math::BoundBox getBoundBox() const;
	Math::BoundBox getBoundBox(int surface) const;
	Math::BoundSphere getBoundSphere() const;
	Math::BoundSphere getBoundSphere(int surface) const;
	void setBoundBox(const Math::BoundBox &bb, int surface);
	void setBoundBox(const Math::BoundBox &bb);
	void setBoundSphere(const Math::BoundSphere &bs, int surface);
	void setBoundSphere(const Math::BoundSphere &bs);
	size_t getMemoryUsage() const;
};
typedef Ptr<Mesh> MeshPtr;
typedef Ptr<const Mesh> ConstMeshPtr;
typedef const Mesh ConstMesh;


class UNIGINE_API MeshSkinned : public APIInterface
{
public:
	typedef const MeshSkinned ConstMeshSkinned;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<MeshSkinned> create();
	static Ptr<MeshSkinned> create(const char *path);
	void initSourceSkeleton(const Ptr<ConstSkeleton> &skeleton);
	void initSourceSkeleton(const char *path);
	Ptr<ConstSkeleton> getSharedSkeleton() const;
	void setSharedSkeletonFileGUID(const UGUID &sharedskeletonfileguid);
	UGUID getSharedSkeletonFileGUID() const;
	void setSharedSkeletonPath(const char *path);
	const char *getSharedSkeletonPath() const;
	void assignFrom(const Ptr<MeshSkinned> &mesh);
	int info(const char *path) const;
	int load(const char *path);
	int save(const char *path) const;
	void clear();
	bool flipYZ(int surface = -1);
	bool flipTangent(int surface = -1);
	bool createBounds(int surface = -1);
	bool removeIndices(int surface = -1);
	bool createIndices(int surface = -1);
	bool optimizeIndices(int flags, int surface = -1);
	bool createNormals(int surface = -1, int target = -1);
	bool createNormals(float angle, int surface = -1, int target = -1);
	bool createTangents(int surface = -1, int target = -1);
	bool createTangents(float angle, const Vector<int> &surfaces);
	int getNumSrcJoints() const;
	int findSrcJoint(const char *name) const;
	const char *getSrcJointName(int bone) const;
	int getSrcJointParent(int bone) const;
	Math::mat4 getSrcJointObjectITransform(int joint) const;
	Math::Transform getSrcJointBindLocalTransform(int joint) const;
	void setSrcJointRestLocalTransform(int index, const Math::Transform &transform);
	Math::Transform getSrcJointRestLocalTransform(int index) const;
	int getNumSurfaces() const;
	int findSurface(const char *name) const;
	void sortSurfaces();
	void clearSurface(int surface = -1, int target = -1);
	int addSurface(const char *name = 0);
	void setSurfaceName(int surface, const char *name);
	const char *getSurfaceName(int surface) const;
	void setNumSurfaceTargets(int surface, int num);
	int getNumSurfaceTargets(int surface) const;
	void setSurfaceTargetName(int surface, int target, const char *name);
	const char *getSurfaceTargetName(int surface, int target) const;
	int findSurfaceTarget(int surface, const char *name) const;
	int setSurfaceTransform(const Math::mat4 &transform, int surface = -1, int target = -1);
	int addMeshSurface(const char *v, const Ptr<ConstMeshSkinned> &mesh, int surface, int target = -1);
	int addMeshSurface(int v, const Ptr<ConstMeshSkinned> &mesh, int surface, int target = -1);
	int addEmptySurface(const char *name, int num_vertex, int num_indices);
	int addSurfaceTarget(int surface, const char *name = 0);
	int addBoxSurface(const char *name, const Math::vec3 &size);
	int addPlaneSurface(const char *name, float width, float height, float step);
	int addSphereSurface(const char *name, float radius, int stacks, int slices);
	int addCapsuleSurface(const char *name, float radius, float height, int stacks, int slices);
	int addCylinderSurface(const char *name, float radius, float height, int stacks, int slices);
	int addPrismSurface(const char *name, float size_0, float size_1, float height, int sides);
	int addIcosahedronSurface(const char *name, float radius);
	int addDodecahedronSurface(const char *name, float radius);
	int getNumCVertex(int surface = 0) const;
	int getNumTVertex(int surface = 0) const;
	int remapCVertex(int surface = 0);
	void setNumWeights(int size, int surface = 0);
	int getNumWeights(int surface = 0) const;
	void setWeightCount(int num, int count, int surface = 0);
	int getWeightCount(int num, int surface = 0) const;
	void setWeightJoints(int num, const Math::ivec4 &joints, int surface = 0);
	Math::ivec4 getWeightJoints(int num, int surface = 0) const;
	void setWeightWeights(int num, const Math::vec4 &weights, int surface = 0);
	Math::vec4 getWeightWeights(int num, int surface = 0) const;
	void setNumVertex(int size, int surface = 0, int target = 0);
	int getNumVertex(int surface, int target = 0) const;
	void addVertex(const Vector<Math::vec3> &vertices, int surface = 0, int target = 0);
	void addVertex(const Math::vec3 &vertex, int surface = 0, int target = 0);
	void setVertex(int num, const Math::vec3 &vertex, int surface = 0, int target = 0);
	Math::vec3 getVertex(int num, int surface = 0, int target = 0) const;
	void setNumTexCoords0(int size, int surface = 0);
	int getNumTexCoords0(int surface = 0) const;
	void addTexCoords0(const Vector<Math::vec2> &texcoords, int surface = 0);
	void addTexCoord0(const Math::vec2 &texcoord, int surface = 0);
	void setTexCoord0(int num, const Math::vec2 &texcoord, int surface = 0);
	Math::vec2 getTexCoord0(int num, int surface = 0) const;
	void setNumTexCoords1(int size, int surface = 0);
	int getNumTexCoords1(int surface = 0) const;
	void addTexCoords1(const Vector<Math::vec2> &texcoords, int surface = 0);
	void addTexCoord1(const Math::vec2 &texcoord, int surface = 0);
	void setTexCoord1(int num, const Math::vec2 &texcoord, int surface = 0);
	Math::vec2 getTexCoord1(int num, int surface = 0) const;
	void setNumNormals(int size, int surface = 0, int target = 0);
	int getNumNormals(int surface = 0, int target = 0) const;
	void addNormals(const Vector<Math::vec3> &normals, int surface = 0, int target = 0);
	void addNormal(const Math::vec3 &normal, int surface = 0, int target = 0);
	void setNormal(int num, const Math::vec3 &normal, int surface = 0, int target = 0);
	Math::vec3 getNormal(int num, int surface = 0, int target = 0) const;
	void setNumTangents(int size, int surface = 0, int target = 0);
	int getNumTangents(int surface, int target = 0) const;
	void addTangents(const Vector<Math::quat> &tangents, int surface = 0, int target = 0);
	void addTangent(const Math::quat &tangent, int surface = 0, int target = 0);
	void setTangent(int num, const Math::quat &tangent, int surface = 0, int target = 0);
	Math::quat getTangent(int num, int surface = 0, int target = 0) const;
	void setNumColors(int size, int surface = 0);
	int getNumColors(int surface = 0) const;
	void addColors(const Vector<Math::vec4> &colors, int surface = 0);
	void addColor(const Math::vec4 &color, int surface = 0);
	void setColor(int num, const Math::vec4 &color, int surface = 0);
	Math::vec4 getColor(int num, int surface = 0) const;
	void setNumCIndices(int size, int surface = 0);
	int getNumCIndices(int surface = 0) const;
	void addCIndices(const Vector<int> &indices, int surface = 0);
	void addCIndex(int index, int surface = 0);
	void setCIndex(int num, int index, int surface = 0);
	int getCIndex(int num, int surface = 0) const;
	void setNumTIndices(int size, int surface = 0);
	int getNumTIndices(int surface = 0) const;
	void addTIndices(const Vector<int> &indices, int surface = 0);
	void addTIndex(int index, int surface = 0);
	void setTIndex(int num, int index, int surface = 0);
	int getTIndex(int num, int surface = 0) const;
	void setNumIndices(int size, int surface = 0);
	int getNumIndices(int surface = 0) const;
	void addIndices(const Vector<int> &indices, int surface = 0);
	void addIndex(int index, int surface = 0);
	void setIndex(int num, int index, int surface = 0);
	int getIndex(int num, int surface = 0) const;
	Vector<Math::vec3> &getVertices(int surface = 0, int target = 0);
	Vector<Math::vec3> &getNormals(int surface = 0, int target = 0);
	Vector<Math::quat> &getTangents(int surface = 0, int target = 0);
	Vector<Math::vec2> &getTexCoords0(int surface = 0);
	Vector<Math::vec2> &getTexCoords1(int surface = 0);
	Vector<Math::bvec4> &getColors(int surface = 0);
	Vector<int> &getCIndices(int surface = 0);
	Vector<int> &getTIndices(int surface = 0);
	const Vector<Math::vec3> &getVertices(int surface = 0, int target = 0) const;
	const Vector<Math::vec3> &getNormals(int surface = 0, int target = 0) const;
	const Vector<Math::quat> &getTangents(int surface = 0, int target = 0) const;
	const Vector<Math::vec2> &getTexCoords0(int surface = 0) const;
	const Vector<Math::vec2> &getTexCoords1(int surface = 0) const;
	const Vector<Math::bvec4> &getColors(int surface = 0) const;
	const Vector<int> &getCIndices(int surface = 0) const;
	const Vector<int> &getTIndices(int surface = 0) const;
	Math::BoundBox getBoundBox() const;
	Math::BoundBox getBoundBox(int surface) const;
	Math::BoundSphere getBoundSphere() const;
	Math::BoundSphere getBoundSphere(int surface) const;
	void setBoundBox(const Math::BoundBox &bb, int surface);
	void setBoundBox(const Math::BoundBox &bb);
	void setBoundSphere(const Math::BoundSphere &bs, int surface);
	void setBoundSphere(const Math::BoundSphere &bs);
	size_t getSystemMemoryUsage() const;
};
typedef Ptr<MeshSkinned> MeshSkinnedPtr;
typedef Ptr<const MeshSkinned> ConstMeshSkinnedPtr;
typedef const MeshSkinned ConstMeshSkinned;

} // namespace Unigine
