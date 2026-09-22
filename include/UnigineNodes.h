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
#include "UnigineAnimation.h"
#include "UnigineSkeleton.h"
#include "UnigineSkeletonControlRig.h"

namespace Unigine
{

class AnimationSequence;
//////////////////////////////////////////////////////////////////////////

class UNIGINE_API NodeSequencePlayer : public Node
{
public:
	typedef const NodeSequencePlayer ConstNodeSequencePlayer;

	static Node::TYPE type() { return Node::NODE_SEQUENCE_PLAYER; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<NodeSequencePlayer> create();
	void play();
	void pause();
	void stop();
	bool isPlaying() const;
	void setTime(float time);
	float getTime() const;
	void setTimeRegion(float from, float to);
	float getDuration() const;
	void setTimeFrom(float from);
	float getTimeFrom() const;
	void setTimeTo(float to);
	float getTimeTo() const;
	void setUseCustomLoop(bool loop);
	bool isUseCustomLoop() const;
	void setLoop(bool loop);
	bool isLoop() const;
	void setUseCustomSpeed(bool speed);
	bool isUseCustomSpeed() const;
	void setSpeed(float speed);
	float getSpeed() const;
	void setSequenceFileGUID(const UGUID &sequencefileguid);
	UGUID getSequenceFileGUID() const;
	void setSequencePath(const char *path);
	const char *getSequencePath() const;
	Ptr<AnimationSequencePlayer> getPlayer() const;
	void setPlayOnEnable(bool enable);
	bool isPlayOnEnable() const;
	void setRestartOnEnable(bool enable);
	bool isRestartOnEnable() const;
	void setUseCustomOriginalsRestoreMode(bool mode);
	bool isUseCustomOriginalsRestoreMode() const;
	void setOriginalsRestoreMode(AnimationSequence::ORIGINALS_RESTORE_MODE mode);
	AnimationSequence::ORIGINALS_RESTORE_MODE getOriginalsRestoreMode() const;
};
typedef Ptr<NodeSequencePlayer> NodeSequencePlayerPtr;
typedef Ptr<const NodeSequencePlayer> ConstNodeSequencePlayerPtr;
typedef const NodeSequencePlayer ConstNodeSequencePlayer;

class AnimScript;
class ObjectMeshSkinned;
//////////////////////////////////////////////////////////////////////////

class UNIGINE_API NodeSkeletonPose : public Node
{
public:
	typedef const NodeSkeletonPose ConstNodeSkeletonPose;

	static Node::TYPE type() { return Node::NODE_SKELETON_POSE; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }


	enum MODE
	{
		MODE_ANIM_SCRIPT = 0,
		MODE_ANIM_LAYERS,
	};

	enum CONTROL_TYPE
	{
		CONTROL_TYPE_HIERARCHY = 0,
		CONTROL_TYPE_LIST,
	};
	static Ptr<NodeSkeletonPose> create();
	Ptr<ConstSkeleton> getSkeleton() const;
	void setSkeletonFileGUID(const UGUID &guid);
	void setSkeletonPath(const char *path);
	const char *getSkeletonPath() const;
	void setMode(NodeSkeletonPose::MODE mode);
	NodeSkeletonPose::MODE getMode() const;
	void setControlType(NodeSkeletonPose::CONTROL_TYPE type);
	NodeSkeletonPose::CONTROL_TYPE getControlType() const;
	void addControlledObject(const Ptr<ObjectMeshSkinned> &controlled_object);
	void removeControlledObject(int index);
	int getNumControlledObjects() const;
	int findControlledObject(const Ptr<ObjectMeshSkinned> &controlled_object) const;
	void setControlledObject(int index, const Ptr<ObjectMeshSkinned> &controlled_object);
	Ptr<ObjectMeshSkinned> getControlledObject(int index) const;
	void setAnimScriptFileGUID(const UGUID &animscriptfileguid);
	UGUID getAnimScriptFileGUID() const;
	void setAnimScriptPath(const char *path);
	const char *getAnimScriptPath() const;
	void setAnimPath(const char *path);
	const char *getAnimPath() const;
	void play();
	void pause();
	bool isPlaying() const;
	void updatePose(float ifps);
	void getPose(const Ptr<SkeletonPoseDecomposed> &out_pose) const;
	int addLayer();
	void removeLayer(int layer);
	void setNumLayers(int layers);
	int getNumLayers() const;
	void setLayer(int layer, bool enable, float weight);
	void setLayerEnabled(int layer, bool enable);
	bool isLayerEnabled(int layer) const;
	void setLayerWeight(int layer, float weight);
	float getLayerWeight(int layer) const;
	void clearLayer(int layer);
	void importLayer(int layer);
	void copyLayer(int dest, int src);
	void inverseLayer(int dest, int src);
	void lerpLayer(int dest, int layer0, int layer1, float weight);
	void lerpLayerByMask(int dest, int layer0, int layer1, int mask_index, float weight);
	void lerpLayerByMask(int dest, int layer0, int layer1, const char *mask_name, float weight);
	void mulLayer(int dest, int layer0, int layer1, float weight = 1.0f);
	int getLayerNumFrames(int layer) const;
	float setLayerFrame(int layer, float frame, int from = -1, int to = -1);
	float getLayerFrame(int layer) const;
	int getLayerFrameFrom(int layer) const;
	int getLayerFrameTo(int layer) const;
	float getLayerDuration(int layer) const;
	void setLayerTime(int layer, float time);
	float getLayerTime(int layer) const;
	void setLayerJointTransform(int layer, int joint, const Math::mat4 &transform);
	Math::mat4 getLayerJointTransform(int layer, int joint) const;
	void setLayerJointPosition(int layer, int joint, const Math::vec3 &position);
	Math::vec3 getLayerJointPosition(int layer, int joint) const;
	void setLayerJointRotation(int layer, int joint, const Math::quat &rotation);
	Math::quat getLayerJointRotation(int layer, int joint) const;
	void setLayerJointScale(int layer, int joint, const Math::vec3 &scale);
	Math::vec3 getLayerJointScale(int layer, int joint) const;
	void setLayerFrameUsesEnabled(int layer, bool enabled);
	bool isLayerFrameUsesEnabled(int layer) const;
	void setLayerJointFrameUses(int layer, int joint, int uses);
	int getLayerJointFrameUses(int layer, int joint) const;
	void setLayerPose(int layer, const Ptr<ConstSkeletonPoseDecomposed> &pose);
	void getLayerPose(int layer, const Ptr<SkeletonPoseDecomposed> &out_pose) const;
	void resetLayerToBindPose(int layer);
	bool isLayerAnimationStreaming(int layer) const;
	void renderLayerJoints(int layer, const Math::Mat4 &world_offset, float basis_length = 0.03f, bool depth_test = false) const;
	void renderLayerJointNames(int layer, const Math::Mat4 &world_offset, const Math::vec4 &color, int outline = 0, int font_size = -1) const;
	void renderLayerBones(int layer, const Math::Mat4 &world_offset, const Math::vec4 &color, float radius = 0.01f, bool depth_test = false) const;
	long long getAnimationResourceID(const char *anim_path) const;
	void setLayerAnimationFilePath(int layer, const char *path);
	const char *getLayerAnimationFilePath(int layer) const;
	void setLayerAnimationFileGUID(int layer, const UGUID &guid);
	UGUID getLayerAnimationFileGUID(int layer) const;
	void setLayerAnimationResourceID(int layer, long long resource_id);
	long long getLayerAnimationResourceID(int layer) const;
	bool isLayerAnimationRootMotionPresent(int layer) const;
	Math::Transform extractLayerRootMotionByFrames(int layer, float begin_frame, float end_frame, bool is_forward) const;
	Math::Transform extractLayerRootMotionByTimes(int layer, float begin_time, float end_time, bool is_forward) const;
	void removeLayerRootMotion(int layer);
	int getLayerNumMarkerSyncs(int layer) const;
	const char *getLayerMarkerSyncName(int layer, int index) const;
	unsigned int getLayerMarkerSyncNameHash(int layer, int index) const;
	float getLayerMarkerSyncTime(int layer, int index) const;
	bool getLayerMarkerSyncInterval(int layer, float current_time, bool is_loop, unsigned int &prev_hash, unsigned int &next_hash, float &ratio, const Vector<unsigned int> &common_hashes) const;
	float getLayerTimeFromMarkerSyncInterval(int layer, unsigned int prev_hash, unsigned int next_hash, float ratio, float current_time, bool is_loop, const Vector<unsigned int> &common_hashes) const;
	void setJointTransform(int joint, const Math::mat4 &transform);
	Math::mat4 getJointTransform(int joint) const;
	void setJointPosition(int joint, const Math::vec3 &position);
	Math::vec3 getJointPosition(int joint) const;
	void setJointRotation(int joint, const Math::quat &rotation);
	Math::quat getJointRotation(int joint) const;
	void setJointScale(int joint, const Math::vec3 &scale);
	Math::vec3 getJointScale(int joint) const;
	Math::mat4 getLayerJointObjectTransform(int layer, int joint) const;
	void setLayerJointObjectTransform(int layer, int joint, const Math::mat4 &transform);
	void setLayerJointObjectTransformPreserveChildren(int layer, int joint, const Math::mat4 &transform);
	void forceApplyPose();
	void solveLayerTwoBoneIK(int layer, const Ptr<ConstIKInfoTwoBone> &info);
	void renderLayerTwoBoneIKDebug(int layer, const Ptr<ConstIKInfoTwoBone> &info, const Math::Mat4 &world_offset);
	void solveLayerLookAt(int layer, const Ptr<ConstLookAtInfo> &info);
	void renderLayerLookAtDebug(int layer, const Ptr<ConstLookAtInfo> &info, const Math::Mat4 &world_offset);
	void solveLayerJointHingeLimit(int layer, const Ptr<ConstJointLimitInfoHinge> &info);
	void renderLayerJointHingeLimitDebug(int layer, const Ptr<ConstJointLimitInfoHinge> &info, const Math::Mat4 &world_offset);
	void solveLayerJointConeLimit(int layer, const Ptr<ConstJointLimitInfoCone> &info);
	void renderLayerJointConeLimitDebug(int layer, const Ptr<ConstJointLimitInfoCone> &info, const Math::Mat4 &world_offset);
	void solveLayerJointConeAsymLimit(int layer, const Ptr<ConstJointLimitInfoConeAsym> &info);
	void renderLayerJointConeAsymLimitDebug(int layer, const Ptr<ConstJointLimitInfoConeAsym> &info, const Math::Mat4 &world_offset);
	void solveLayerJointTwistLimit(int layer, const Ptr<ConstJointLimitInfoTwist> &info);
	void renderLayerJointTwistLimitDebug(int layer, const Ptr<ConstJointLimitInfoTwist> &info, const Math::Mat4 &world_offset);
	void solveLayerJointHingeTwistLimit(int layer, const Ptr<ConstJointLimitInfoHingeTwist> &info);
	void renderLayerJointHingeTwistLimitDebug(int layer, const Ptr<ConstJointLimitInfoHingeTwist> &info, const Math::Mat4 &world_offset);
	void solveLayerJointConeTwistLimit(int layer, const Ptr<ConstJointLimitInfoConeTwist> &info);
	void renderLayerJointConeTwistLimitDebug(int layer, const Ptr<ConstJointLimitInfoConeTwist> &info, const Math::Mat4 &world_offset);
	void solveLayerJointConeAsymTwistLimit(int layer, const Ptr<ConstJointLimitInfoConeAsymTwist> &info);
	void renderLayerJointConeAsymTwistLimitDebug(int layer, const Ptr<ConstJointLimitInfoConeAsymTwist> &info, const Math::Mat4 &world_offset);
	void solveLayerIKChain(int layer, const Ptr<ConstIKInfoChain> &info);
	void renderLayerIKChainDebug(int layer, const Ptr<ConstIKInfoChain> &info, const Math::Mat4 &world_offset);
	void solveLayerLookAtChain(int layer, const Ptr<ConstLookAtChainInfo> &info);
	void renderLayerLookAtChainDebug(int layer, const Ptr<ConstLookAtChainInfo> &info, const Math::Mat4 &world_offset);
	void renderLayerJointLimitSetDebug(int layer, const Ptr<ConstJointLimitSetInfo> &info, const Math::Mat4 &world_offset);
	Ptr<AnimScript> getAnimScript() const;
};
typedef Ptr<NodeSkeletonPose> NodeSkeletonPosePtr;
typedef Ptr<const NodeSkeletonPose> ConstNodeSkeletonPosePtr;
typedef const NodeSkeletonPose ConstNodeSkeletonPose;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API NodeDummy : public Node
{
public:
	typedef const NodeDummy ConstNodeDummy;

	static Node::TYPE type() { return Node::NODE_DUMMY; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<NodeDummy> create();
};
typedef Ptr<NodeDummy> NodeDummyPtr;
typedef Ptr<const NodeDummy> ConstNodeDummyPtr;
typedef const NodeDummy ConstNodeDummy;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API NodeLayer : public Node
{
public:
	typedef const NodeLayer ConstNodeLayer;

	static Node::TYPE type() { return Node::NODE_LAYER; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<NodeLayer> create(const char *path);
	void setNodePath(const char *path);
	const char *getNodePath() const;
};
typedef Ptr<NodeLayer> NodeLayerPtr;
typedef Ptr<const NodeLayer> ConstNodeLayerPtr;
typedef const NodeLayer ConstNodeLayer;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API NodeReference : public Node
{
public:
	typedef const NodeReference ConstNodeReference;

	static Node::TYPE type() { return Node::NODE_REFERENCE; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<NodeReference> create(const char *path);
	Ptr<Node> getReference() const;
	Ptr<Node> detachReference() const;
	void setNodePath(const char *path);
	const char *getNodePath() const;
	static bool canBeReference(const char *path, const Ptr<Node> &node);
};
typedef Ptr<NodeReference> NodeReferencePtr;
typedef Ptr<const NodeReference> ConstNodeReferencePtr;
typedef const NodeReference ConstNodeReference;

class NodeTrigger;
template class UNIGINE_API EventHolder<EventInterfaceInvoker<const Ptr<NodeTrigger> &>>;
template class UNIGINE_API EventInterfaceConnection<EventInterfaceInvoker<const Ptr<NodeTrigger> &>>;
//////////////////////////////////////////////////////////////////////////

class UNIGINE_API NodeTrigger : public Node
{
public:
	typedef const NodeTrigger ConstNodeTrigger;

	static Node::TYPE type() { return Node::NODE_TRIGGER; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<NodeTrigger> create();
	const char *getEnabledCallbackName() const;
	void setEnabledCallbackName(const char *name);
	const char *getPositionCallbackName() const;
	void setPositionCallbackName(const char *name);
	Event<const Ptr<NodeTrigger> &> &getEventEnabled();
	Event<const Ptr<NodeTrigger> &> &getEventPosition();

private:

	EventHolder<EventInterfaceInvoker<const Ptr<NodeTrigger> &>> event_enabled;
	EventInterfaceConnection<EventInterfaceInvoker<const Ptr<NodeTrigger> &>> event_enabled_connection;
	EventHolder<EventInterfaceInvoker<const Ptr<NodeTrigger> &>> event_position;
	EventInterfaceConnection<EventInterfaceInvoker<const Ptr<NodeTrigger> &>> event_position_connection;
};
typedef Ptr<NodeTrigger> NodeTriggerPtr;
typedef Ptr<const NodeTrigger> ConstNodeTriggerPtr;
typedef const NodeTrigger ConstNodeTrigger;

class NodeExternBase;
//////////////////////////////////////////////////////////////////////////

class UNIGINE_API NodeExtern : public Node
{
public:
	typedef const NodeExtern ConstNodeExtern;

	static Node::TYPE type() { return Node::NODE_EXTERN; }
	static bool convertible(Node *node) { return (node && node->getType() == type()); }

	static Ptr<NodeExtern> create(int class_id);
	int getClassID();
	NodeExternBase *getNodeExtern() const;
};
typedef Ptr<NodeExtern> NodeExternPtr;
typedef Ptr<const NodeExtern> ConstNodeExternPtr;
typedef const NodeExtern ConstNodeExtern;


class UNIGINE_API  NodeExternBase : public virtual Base
{
public:
typedef const NodeExternBase ConstNodeExternBase;
	NodeExternBase();
	NodeExternBase(void * object);
	virtual ~NodeExternBase();
	virtual int getClassID() = 0;
	Ptr<Node> getNode() const;
	Ptr<NodeExtern> getNodeExtern() const;
	typedef NodeExternBase *(*ConstructorFunc)(void *object);
	static void addClassID(int class_id, ConstructorFunc func);
	template <class Type> static void addClassID(int class_id) { addClassID(class_id, constructor<Type>); }
	virtual void updateEnabled();
	virtual void updateTransform();
	virtual const Math::BoundBox &getBoundBox();
	virtual const Math::BoundSphere &getBoundSphere();
	virtual void renderHandler();
	virtual void renderVisualizer();
	virtual void copy(NodeExternBase *node, int is_root);
	virtual void swap(NodeExternBase *node);
	virtual int loadWorld(const Ptr<Xml> &xml);
	virtual int saveWorld(const Ptr<Xml> &xml);
	virtual int saveState(const Ptr<Stream> &stream);
	virtual int restoreState(const Ptr<Stream> &stream);
	static void savePointer(const StreamPtr &stream, NodeExternBase *node);
	static NodeExternBase *restorePointer(const StreamPtr &stream);
private:
	template <class Type> static NodeExternBase *constructor(void *object) { return new Type(object); }
	struct NodeExternData;
	NodeExternData *data;
};
typedef Ptr<NodeExternBase> NodeExternBasePtr;
typedef Ptr<const NodeExternBase> ConstNodeExternBasePtr;
typedef const NodeExternBase ConstNodeExternBase;

} // namespace Unigine
