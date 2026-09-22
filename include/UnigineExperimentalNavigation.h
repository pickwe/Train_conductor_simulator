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

#include "UnigineNode.h"
#include "UnigineMesh.h"
#include "UniginePathFinding.h"
#include "UnigineExperimentalNavigationBakeSettings.h"

namespace Unigine
{


class UNIGINE_API ExperimentalNavigationMeshFilter : public APIInterface
{
public:
	typedef const ExperimentalNavigationMeshFilter ConstExperimentalNavigationMeshFilter;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ExperimentalNavigationMeshFilter> create();
	void setNavigationMask(int mask);
	int getNavigationMask() const;
	void setAgentRadius(float radius);
	float getAgentRadius() const;
	void setAgentHeight(float height);
	float getAgentHeight() const;
	void setMaxStepHeight(float height);
	float getMaxStepHeight() const;
	void setMaxSlopeAngle(float angle);
	float getMaxSlopeAngle() const;
	void setSnapSize(const Math::vec3 &size);
	Math::vec3 getSnapSize() const;
	void setIncludeFlags(int flags);
	int getIncludeFlags() const;
	void setExcludeFlags(int flags);
	int getExcludeFlags() const;
	void setAreaCostOverride(int area_index, float cost);
	float getAreaCost(int area_index) const;
	bool isAreaCostOverridden(int area_index) const;
	void resetAreaCostOverride(int area_index);
	void setObstacleMask(int mask);
	int getObstacleMask() const;
	void addExcludeObstacle(const Ptr<Obstacle> &obstacle);
	void removeExcludeObstacle(const Ptr<Obstacle> &obstacle);
	void removeExcludeObstacle(int num);
	void clearExcludeObstacles();
	int getNumExcludeObstacles() const;
	Ptr<Obstacle> getExcludeObstacle(int num) const;
	Ptr<ExperimentalNavigationMeshFilter> clone() const;
};
typedef Ptr<ExperimentalNavigationMeshFilter> ExperimentalNavigationMeshFilterPtr;
typedef Ptr<const ExperimentalNavigationMeshFilter> ConstExperimentalNavigationMeshFilterPtr;
typedef const ExperimentalNavigationMeshFilter ConstExperimentalNavigationMeshFilter;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API ExperimentalNavigationMesh : public Node
{
public:
	typedef const ExperimentalNavigationMesh ConstExperimentalNavigationMesh;

	static Node::TYPE type() { return Node::EXPERIMENTAL_NAVIGATION_MESH; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }


	enum BAKE_MODE
	{
		BAKE_MODE_STATIC = 0,
		BAKE_MODE_STATIC_CARVE,
		BAKE_MODE_DYNAMIC,
	};
	static Ptr<ExperimentalNavigationMesh> create();
	static Ptr<ExperimentalNavigationMesh> find(const Ptr<ExperimentalNavigationMeshFilter> &filter);
	static Ptr<ExperimentalNavigationMesh> find(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &point, Math::Vec3 &ret_point, long long &ret_polygon);
	void setNavigationMask(int mask);
	int getNavigationMask() const;
	bool setMeshPath(const char *path, bool force_load = false);
	const char *getMeshPath() const;
	bool isFileLoaded() const;
	Ptr<ExperimentalNavigationBakeSettings> getBakeSettings() const;
	void setBakeMode(ExperimentalNavigationMesh::BAKE_MODE mode);
	ExperimentalNavigationMesh::BAKE_MODE getBakeMode() const;
	void resetBakeMode();
	bool isNeedBake() const;
	void setStreamingEnabled(bool enabled);
	bool isStreamingEnabled() const;
	int getNumTiles() const;
	int getNumLoadedTiles() const;
	int getNumPolygons() const;
	float getWalkableArea() const;
	unsigned long long getDataHash() const;
	unsigned long long getDataVersion() const;
	ExperimentalNavigationMesh::BAKE_MODE getDataBakeMode() const;
	float getDataAgentRadius() const;
	float getDataAgentHeight() const;
	float getDataCellSize() const;
	int getDataTileResolution() const;
	float getDataTileWorldSize() const;
	float getDataMaxSlopeAngle() const;
	float getDataMaxStepHeight() const;
	bool getMesh(Ptr<Mesh> &ret_mesh) const;
	void getTriangles(Vector<Math::Vec3> &ret_vertices, Vector<int> &ret_indices) const;
	void getTriangles(Vector<Math::Vec3> &ret_vertices, Vector<int> &ret_indices, Vector<int> &ret_areas) const;
	bool findNearestPoint(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &point, Math::Vec3 &ret_point) const;
	bool findPointOutsideObstacles(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &point, Math::Vec3 &ret_point) const;
	bool isPointInObstacle(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &point) const;
	long long findNearestPolygon(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &point) const;
	bool isPolygonValid(long long polygon) const;
	int getPolygonAreaIndex(long long polygon) const;
	int getPolygonFlags(long long polygon) const;
	bool getNearestBoundary(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &point, float max_radius, Math::Vec3 &ret_point, Math::vec3 &ret_normal, float &ret_distance, long long start_polygon = 0) const;
	bool getBoundaryIntersection(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &from, const Math::Vec3 &to, Math::Vec3 &ret_point, Math::vec3 &ret_normal, long long start_polygon = 0) const;
	bool getSlidePoint(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &from, const Math::Vec3 &to, Math::Vec3 &ret_point, long long start_polygon = 0) const;
	int getSlidePoints(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Vector<Math::Vec3> &points, Vector<Math::Vec3> &ret_points, Vector<long long> &ret_polygons, long long start_polygon = 0) const;
	int getPolygons(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::WorldBoundBox &bounds, Vector<long long> &ret_polygons) const;
	bool findRandomPoint(const Ptr<ExperimentalNavigationMeshFilter> &filter, Math::Vec3 &ret_point) const;
	bool findRandomReachablePoint(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &point, float max_cost, Math::Vec3 &ret_point, long long start_polygon = 0) const;
	int findReachablePoints(const Ptr<ExperimentalNavigationMeshFilter> &filter, const Math::Vec3 &point, float max_cost, Vector<Math::Vec3> &ret_points, Vector<long long> &ret_polygons, long long start_polygon = 0) const;
	Event<const Math::WorldBoundBox &> &getEventTilesChanged();

private:

	EventHolder<EventInterfaceInvoker<const Math::WorldBoundBox &>> event_tiles_changed;
	EventInterfaceConnection<EventInterfaceInvoker<const Math::WorldBoundBox &>> event_tiles_changed_connection;
};
typedef Ptr<ExperimentalNavigationMesh> ExperimentalNavigationMeshPtr;
typedef Ptr<const ExperimentalNavigationMesh> ConstExperimentalNavigationMeshPtr;
typedef const ExperimentalNavigationMesh ConstExperimentalNavigationMesh;


class UNIGINE_API ExperimentalNavigationPath : public APIInterface
{
public:
	typedef const ExperimentalNavigationPath ConstExperimentalNavigationPath;
	static constexpr bool HAS_OWNERSHIP = true;


	enum STATUS
	{
		STATUS_COMPLETE = 0,
		STATUS_PARTIAL,
		STATUS_NO_PATH,
	};

	enum FAILURE
	{
		FAILURE_NONE = 0,
		FAILURE_START_OUTSIDE,
		FAILURE_TARGET_OUTSIDE,
		FAILURE_DISCONNECTED,
		FAILURE_NO_FILTER,
		FAILURE_NO_MATCHING_MESH,
		FAILURE_POLYGON_MESH_MISMATCH,
		FAILURE_DATA_MISSING,
		FAILURE_SEARCH_NODES_LIMIT,
		FAILURE_COST_LIMIT,
		FAILURE_POLYGON_LIMIT,
		FAILURE_OBSTACLE_BLOCKED,
		FAILURE_NO_AVAILABLE_MESH,
		FAILURE_NAVIGATION_MASK_MISMATCH,
	};

	enum STRAIGHT_MODE
	{
		STRAIGHT_MODE_CORNERS = 0,
		STRAIGHT_MODE_AREA_CROSSINGS,
		STRAIGHT_MODE_ALL_CROSSINGS,
	};
	ExperimentalNavigationPath::STATUS getStatus() const;
	ExperimentalNavigationPath::FAILURE getFailureReason() const;
	Ptr<ExperimentalNavigationMesh> getNavigationMesh() const;
	float getLength() const;
	float getCost() const;
	ExperimentalNavigationPath::STRAIGHT_MODE getStraightMode() const;
	int getNumPoints() const;
	Math::Vec3 getPoint(int num) const;
	Math::Vec3 getPointLocal(int num) const;
	long long getPointPolygon(int num) const;
	int getPointAreaIndex(int num) const;
	int getPointFlags(int num) const;
	void renderVisualizer(const Math::vec4 &color) const;
	void clear();
};
typedef Ptr<ExperimentalNavigationPath> ExperimentalNavigationPathPtr;
typedef Ptr<const ExperimentalNavigationPath> ConstExperimentalNavigationPathPtr;
typedef const ExperimentalNavigationPath ConstExperimentalNavigationPath;


class UNIGINE_API ExperimentalNavigationPathFetch : public APIInterface
{
public:
	typedef const ExperimentalNavigationPathFetch ConstExperimentalNavigationPathFetch;
	static constexpr bool HAS_OWNERSHIP = true;


	enum PRIORITY
	{
		PRIORITY_LOW = 0,
		PRIORITY_NORMAL,
		PRIORITY_HIGH,
	};
	static Ptr<ExperimentalNavigationPathFetch> create();
	void setFilter(const Ptr<ExperimentalNavigationMeshFilter> &filter);
	Ptr<ExperimentalNavigationMeshFilter> getFilter() const;
	void setFrom(const Math::Vec3 &from);
	void setFrom(const Ptr<ExperimentalNavigationMesh> &navigation_mesh, long long polygon, const Math::Vec3 &from);
	Math::Vec3 getFrom() const;
	long long getFromPolygon() const;
	Ptr<ExperimentalNavigationMesh> getFromNavigationMesh() const;
	void setTo(const Math::Vec3 &to);
	void setTo(const Ptr<ExperimentalNavigationMesh> &navigation_mesh, long long polygon, const Math::Vec3 &to);
	Math::Vec3 getTo() const;
	long long getToPolygon() const;
	Ptr<ExperimentalNavigationMesh> getToNavigationMesh() const;
	void setNavigationMesh(const Ptr<ExperimentalNavigationMesh> &mesh);
	Ptr<ExperimentalNavigationMesh> getNavigationMesh() const;
	void addExcludeNavigationMesh(const Ptr<ExperimentalNavigationMesh> &navigation_mesh);
	void removeExcludeNavigationMesh(const Ptr<ExperimentalNavigationMesh> &navigation_mesh);
	void clearExcludeNavigationMeshes();
	int getNumExcludeNavigationMeshes() const;
	Ptr<ExperimentalNavigationMesh> getExcludeNavigationMesh(int num) const;
	void setHeuristicScale(float scale);
	float getHeuristicScale() const;
	void setMaxSearchNodes(int nodes);
	int getMaxSearchNodes() const;
	void setMaxCost(float cost);
	float getMaxCost() const;
	void setPriority(ExperimentalNavigationPathFetch::PRIORITY priority);
	ExperimentalNavigationPathFetch::PRIORITY getPriority() const;
	void setStraightMode(ExperimentalNavigationPath::STRAIGHT_MODE mode);
	ExperimentalNavigationPath::STRAIGHT_MODE getStraightMode() const;
	bool fetchForce();
	bool fetchForce(const Math::Vec3 &from, const Math::Vec3 &to);
	void fetchAsync();
	void fetchAsync(const Math::Vec3 &from, const Math::Vec3 &to);
	static void fetchForce(const Vector<Ptr<ExperimentalNavigationPathFetch>> &fetches);
	static void fetchAsync(const Vector<Ptr<ExperimentalNavigationPathFetch>> &fetches);
	bool isFetching() const;
	bool isReady() const;
	void waitForce();
	Event<> &getEventReady();
	Ptr<ExperimentalNavigationPath> takePath();
};
typedef Ptr<ExperimentalNavigationPathFetch> ExperimentalNavigationPathFetchPtr;
typedef Ptr<const ExperimentalNavigationPathFetch> ConstExperimentalNavigationPathFetchPtr;
typedef const ExperimentalNavigationPathFetch ConstExperimentalNavigationPathFetch;


class UNIGINE_API ExperimentalNavigationMeshCorridor : public APIInterface
{
public:
	typedef const ExperimentalNavigationMeshCorridor ConstExperimentalNavigationMeshCorridor;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ExperimentalNavigationMeshCorridor> create();
	bool setPath(const Ptr<ExperimentalNavigationPath> &path, int first_point = 0);
	void clear();
	Ptr<ExperimentalNavigationMesh> getNavigationMesh() const;
	bool isValid() const;
	ExperimentalNavigationPath::STRAIGHT_MODE getStraightMode() const;
	float getRemainingLength() const;
	float getRemainingCost() const;
	void setCornerCuttingEnabled(bool enabled);
	bool isCornerCuttingEnabled() const;
	void setCornerCuttingRange(float range);
	float getCornerCuttingRange() const;
	void setObstacleMask(int mask);
	int getObstacleMask() const;
	bool teleport(const Ptr<ExperimentalNavigationMesh> &navigation_mesh, long long polygon, const Math::Vec3 &position);
	void movePosition(const Math::Vec3 &position);
	Math::Vec3 getPosition() const;
	long long getPositionPolygon() const;
	void moveTarget(const Math::Vec3 &target);
	Math::Vec3 getTargetPosition() const;
	long long getTargetPolygon() const;
	int getNumNextCorners() const;
	Math::Vec3 getNextCorner(int num) const;
	long long getNextCornerPolygon(int num) const;
	int getNextCornerAreaIndex(int num) const;
	int getNextCornerFlags(int num) const;
	bool optimizePath();
	void renderVisualizer(const Math::vec4 &color) const;
};
typedef Ptr<ExperimentalNavigationMeshCorridor> ExperimentalNavigationMeshCorridorPtr;
typedef Ptr<const ExperimentalNavigationMeshCorridor> ConstExperimentalNavigationMeshCorridorPtr;
typedef const ExperimentalNavigationMeshCorridor ConstExperimentalNavigationMeshCorridor;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API ExperimentalNavigationMeshInvoker : public Node
{
public:
	typedef const ExperimentalNavigationMeshInvoker ConstExperimentalNavigationMeshInvoker;

	static Node::TYPE type() { return Node::EXPERIMENTAL_NAVIGATION_MESH_INVOKER; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<ExperimentalNavigationMeshInvoker> create();
	void setLoadDistance(float distance);
	float getLoadDistance() const;
	void setClearDistance(float distance);
	float getClearDistance() const;
	void setNavigationMask(int mask);
	int getNavigationMask() const;
	int getNumUsedNavigationMeshes() const;
	Ptr<ExperimentalNavigationMesh> getUsedNavigationMesh(int num) const;
	bool prefetchTilesForce(const Math::WorldBoundBox &bounds);
};
typedef Ptr<ExperimentalNavigationMeshInvoker> ExperimentalNavigationMeshInvokerPtr;
typedef Ptr<const ExperimentalNavigationMeshInvoker> ConstExperimentalNavigationMeshInvokerPtr;
typedef const ExperimentalNavigationMeshInvoker ConstExperimentalNavigationMeshInvoker;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API ExperimentalNavigationMeshAreaVolume : public Node
{
public:
	typedef const ExperimentalNavigationMeshAreaVolume ConstExperimentalNavigationMeshAreaVolume;

	static Node::TYPE type() { return Node::EXPERIMENTAL_NAVIGATION_MESH_AREA_VOLUME; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }


	enum SHAPE
	{
		SHAPE_BOX = 0,
		SHAPE_SPHERE,
		SHAPE_CYLINDER,
	};

	enum MODE
	{
		MODE_BAKE = 0,
		MODE_DYNAMIC,
	};
	static Ptr<ExperimentalNavigationMeshAreaVolume> create();
	void setShape(ExperimentalNavigationMeshAreaVolume::SHAPE shape);
	ExperimentalNavigationMeshAreaVolume::SHAPE getShape() const;
	void setSize(const Math::vec3 &size);
	Math::vec3 getSize() const;
	void setRadius(float radius);
	float getRadius() const;
	void setHeight(float height);
	float getHeight() const;
	void setMode(ExperimentalNavigationMeshAreaVolume::MODE mode);
	ExperimentalNavigationMeshAreaVolume::MODE getMode() const;
	void setAreaIndex(int index);
	int getAreaIndex() const;
	void setPriority(int priority);
	int getPriority() const;
	void setExpandByAgentRadius(bool radius);
	bool isExpandByAgentRadius() const;
	void setDynamicApplyOnlyWhenStationary(bool stationary);
	bool isDynamicApplyOnlyWhenStationary() const;
	void setDynamicMoveThreshold(float threshold);
	float getDynamicMoveThreshold() const;
	void setDynamicTimeToStationary(float stationary);
	float getDynamicTimeToStationary() const;
	void setBakeMask(int mask);
	int getBakeMask() const;
};
typedef Ptr<ExperimentalNavigationMeshAreaVolume> ExperimentalNavigationMeshAreaVolumePtr;
typedef Ptr<const ExperimentalNavigationMeshAreaVolume> ConstExperimentalNavigationMeshAreaVolumePtr;
typedef const ExperimentalNavigationMeshAreaVolume ConstExperimentalNavigationMeshAreaVolume;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API ExperimentalNavigation
{
public:
	static bool isInitialized();
	static int getNumNavigationMeshes();
	static Ptr<ExperimentalNavigationMesh> getNavigationMesh(int num);
	static int getNumAreas();
	static int getCutAreaIndex();
	static int getDefaultAreaIndex();
	static void setAreaName(int area_index, const char *name);
	static const char *getAreaName(int area_index);
	static int getAreaIndex(const char *name);
	static void setAreaCost(int area_index, float cost);
	static float getAreaCost(int area_index);
	static void setAreaFlags(int area_index, int flags);
	static int getAreaFlags(int area_index);
	static void setAreaColor(int area_index, const Math::vec4 &color);
	static Math::vec4 getAreaColor(int area_index);
	static Math::vec4 getDefaultAreaColor(int area_index);
	static int getNumFlags();
	static void setFlagName(int flag_index, const char *name);
	static const char *getFlagName(int flag_index);
	static int getFlagIndex(const char *name);
	static int getFlagMask(const char *name);
	static void setStreamingBudget(float budget);
	static float getStreamingBudget();
	static void setInvalidateBudget(float budget);
	static float getInvalidateBudget();
	static void setStreamingMemoryLimit(int limit);
	static int getStreamingMemoryLimit();
	static bool isStreamingMemoryLimitReached();
	static int getNumActiveTasks();
	static bool loadSettings(const char *path);
	static bool saveSettings(const char *path);
	static bool saveState(const Ptr<Stream> &stream);
	static bool restoreState(const Ptr<Stream> &stream);
	static Event<int> &getEventAreaChanged();
	static Event<int> &getEventFlagChanged();
	static Event<> &getEventRegistryChanged();
};


class UNIGINE_API ExperimentalNavigationBakeQuery : public APIInterface
{
public:
	typedef const ExperimentalNavigationBakeQuery ConstExperimentalNavigationBakeQuery;
	static constexpr bool HAS_OWNERSHIP = true;


	enum STATUS
	{
		STATUS_RUNNING = 0,
		STATUS_READY,
		STATUS_PARTIAL,
		STATUS_FAILED,
		STATUS_CANCELLED,
	};
	ExperimentalNavigationBakeQuery::STATUS getStatus() const;
	bool isReady() const;
	float getProgress() const;
	int getNumFailedTiles() const;
	const char *getMeshName() const;
	int getNumMeshes() const;
	int getNumBakedMeshes() const;
	int getNumFailedMeshes() const;
	int getNumBuiltTiles() const;
	int getNumPendingTiles() const;
	float getElapsedTime() const;
	bool isSamplingTerrain() const;
	void waitForce();
	void cancel();
};
typedef Ptr<ExperimentalNavigationBakeQuery> ExperimentalNavigationBakeQueryPtr;
typedef Ptr<const ExperimentalNavigationBakeQuery> ConstExperimentalNavigationBakeQueryPtr;
typedef const ExperimentalNavigationBakeQuery ConstExperimentalNavigationBakeQuery;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API ExperimentalBakeNavigation
{
public:
	static bool isInitialized();
	static void setNumWorkers(int workers);
	static int getNumWorkers();
	static int getNumWorkersAuto();
	static bool bakeForce(const Ptr<ExperimentalNavigationMesh> &navigation_mesh);
	static bool bakeForce(const Vector<Ptr<ExperimentalNavigationMesh>> &navigation_meshes);
	static bool bakeForce(const Ptr<NavigationMesh> &navigation_mesh);
	static bool bakeForce(const Vector<Ptr<NavigationMesh>> &navigation_meshes);
	static bool bakeForce(int navigation_mask);
	static Ptr<ExperimentalNavigationBakeQuery> bakeAsync(const Ptr<ExperimentalNavigationMesh> &navigation_mesh);
	static Ptr<ExperimentalNavigationBakeQuery> bakeAsync(const Vector<Ptr<ExperimentalNavigationMesh>> &navigation_meshes);
	static Ptr<ExperimentalNavigationBakeQuery> bakeAsync(const Ptr<NavigationMesh> &navigation_mesh);
	static Ptr<ExperimentalNavigationBakeQuery> bakeAsync(const Vector<Ptr<NavigationMesh>> &navigation_meshes);
	static Ptr<ExperimentalNavigationBakeQuery> bakeAsync(int navigation_mask);
	static void invalidateAsync(const Math::WorldBoundBox &bounds);
	static void invalidateAsync(const Math::WorldBoundBox &bounds, int navigation_mask);
	static void invalidateForce(const Math::WorldBoundBox &bounds);
	static void invalidateForce(const Math::WorldBoundBox &bounds, int navigation_mask);
	static bool isBaking();
	static void stop();
	static void waitForce();
};


class UNIGINE_API ExperimentalNavigationAvoidance : public APIInterface
{
public:
	typedef const ExperimentalNavigationAvoidance ConstExperimentalNavigationAvoidance;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<ExperimentalNavigationAvoidance> create();
	void setNumAgents(int agents);
	int getNumAgents() const;
	void setAgent(int num, const Math::Vec3 &position, float radius, float height, const Math::vec3 &velocity, const Math::vec3 &desired_velocity, float max_speed, float max_acceleration);
	void setAgentInteractionMask(int num, int mask);
	void setAgentPriority(int num, float priority);
	void setAgentExcludeObstacle(int num, const Ptr<Obstacle> &obstacle);
	void setAgentExcludeAgent(int num, int other);
	void setObstacleMask(int mask);
	int getObstacleMask() const;
	void setNavigationMask(int mask);
	int getNavigationMask() const;
	void setNeighborRange(float range);
	float getNeighborRange() const;
	void setMaxNeighbors(int neighbors);
	int getMaxNeighbors() const;
	void setPredictionTime(float time);
	float getPredictionTime() const;
	void setWeightDesiredVelocity(float velocity);
	float getWeightDesiredVelocity() const;
	void setWeightCurrentVelocity(float velocity);
	float getWeightCurrentVelocity() const;
	void setWeightSide(float side);
	float getWeightSide() const;
	void setWeightTimeToImpact(float impact);
	float getWeightTimeToImpact() const;
	void setWeightSeparation(float separation);
	float getWeightSeparation() const;
	void setDensityEnabled(bool enabled);
	bool isDensityEnabled() const;
	void setDensityCellSize(float size);
	float getDensityCellSize() const;
	float getAgentDensity(const Math::Vec3 &world_point) const;
	void compute(float ifps);
	Math::vec3 getAgentVelocity(int num) const;
	Math::Vec3 getAgentPosition(int num) const;
	void separateOverlappingAgents(int iterations = 4);
};
typedef Ptr<ExperimentalNavigationAvoidance> ExperimentalNavigationAvoidancePtr;
typedef Ptr<const ExperimentalNavigationAvoidance> ConstExperimentalNavigationAvoidancePtr;
typedef const ExperimentalNavigationAvoidance ConstExperimentalNavigationAvoidance;

} // namespace Unigine
