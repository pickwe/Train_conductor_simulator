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
#include <UnigineMathLib.h>
#include <UnigineNodes.h>
#include <UnigineSkeletonControlRig.h>
#include <UnigineString.h>

namespace Unigine
{

namespace AnimGraphRuntime
{

using namespace Math;

struct AnimGraphContext;
struct AnimNodeBase;
struct AnimNodeAnimationPlayer;
struct AnimNodeBlendSpace2DSync;

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Sync Member
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct SyncMember
{
	enum class KIND
	{
		PLAYER = 0,
		BS2D_SYNC,
	};

	enum class ROLE
	{
		LEADER = 0,
		WEIGHTED_LEADER_AND_FOLLOWER,
		FOLLOWER,
	};

	KIND kind{KIND::PLAYER};
	AnimNodeBase *node{nullptr};

	float getDuration() const;
	float getCurrentTime() const;
	float getAccumulatedWeight() const;
	bool wasInactive() const;
	ROLE getRole() const;
	bool needsUpdate() const;
	bool isLoop() const;
	float getLeaderNextTime(float ifps) const;
	bool isTimeForward() const;

	void applyLeaderTime(float next_time, bool is_forward);
	void applyFollowerTime(float normalized_time, bool is_forward);
	void intersectMarkers(AnimGraphContext &context, Vector<unsigned int> &hashes, bool &first) const;
	bool getMarkerSyncInterval(AnimGraphContext &context, float time, unsigned int &out_prev_hash, unsigned int &out_next_hash, float &out_ratio, const Vector<unsigned int> &hashes) const;
	void applyMarkerTime(AnimGraphContext &context, unsigned int prev_hash, unsigned int next_hash, float ratio, const Vector<unsigned int> &hashes, bool is_forward, float fallback_normalized_time);
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Syncker
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimSyncker
{
	using ROLE = SyncMember::ROLE;

	enum class SYNC_METHOD
	{
		NORMALIZED_TIME = 0,
		MARKER_SYNC,
	};

	Vector<SyncMember> members;
	float normalized_time{0.0f};
	bool is_valid_leader{true};
	int leader_index{-1};

	SYNC_METHOD sync_method{SYNC_METHOD::NORMALIZED_TIME};
	Vector<unsigned int> common_hashes;

	bool hasOtherActiveMembers(const AnimNodeBase *exclude) const;
	void rebuildCommonHashes(AnimGraphContext &context, bool before_update = false);
	bool syncTimeByMarkers(AnimGraphContext &context, AnimNodeBase *target_node, bool before_update = false);
	void update(AnimGraphContext &context);
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Graph Context
///////////////////////////////////////////////////////////////////////////////////////////////////////

struct AnimGraphContext
{
	NodeSkeletonPosePtr skeleton_pose;
	float ifps{0.0f};

	Vector<AnimNodeAnimationPlayer *> self_timed_players;

	bool is_root_motion_enabled{false};
	Transform root_motion_delta;

	int tmp_layers_count{0};

	HashMap<String, int> syncker_name_to_index;
	Vector<AnimSyncker> synckers;

	// state machine context
	struct SMContext
	{
		float state_time{0.0f};
		bool anim_ended{false};
		float anim_time_remaining{0.0f};
		float anim_time_remaining_fraction{0.0f};
		float anim_length{0.0f};
		float anim_time{0.0f};
		float anim_time_fraction{0.0f};
	};
	SMContext sm;
	Vector<SMContext> sm_stack;

	void pushSMContext()
	{
		sm_stack.append(sm);
		sm = {};
	}

	void popSMContext()
	{
		sm = sm_stack.last();
		sm_stack.removeLast();
	}

	bool isActiveRootMotion() const { return is_root_motion_enabled; }

	int getTmpPoseLayer()
	{
		tmp_layers_count++;
		return skeleton_pose->addLayer();
	}

	void releaseTmpPoseLayers()
	{
		skeleton_pose->setNumLayers(skeleton_pose->getNumLayers() - tmp_layers_count);
		tmp_layers_count = 0;
	}

	int addToSyncker(AnimNodeBase *node, SyncMember::KIND kind, const char *syncker_name)
	{
		int index = -1;
		if (syncker_name_to_index.contains(syncker_name))
		{
			index = syncker_name_to_index[syncker_name];
		} else
		{
			index = synckers.size();
			synckers.append();
			syncker_name_to_index.append(syncker_name, index);
		}

		auto &members = synckers[index].members;
		for (const auto &m : members)
		{
			if (m.node == node)
				return index;
		}

		SyncMember new_member;
		new_member.kind = kind;
		new_member.node = node;
		members.append(new_member);
		return index;
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Base
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeBase
{
	enum class TYPE
	{
		UNKNOWN = 0,
		FINAL_POSE,
		ANIMATION_POSE,
		ANIMATION_PLAYER,
		ANIMATION_PLAYER_SYNC,
		BLEND,
		MAKE_ADDITIVE,
		APPLY_ADDITIVE,
		BLEND_SPACE_2D,
		BLEND_SPACE_2D_SYNC,
		STATE,
		STATE_MACHINE,
		BRANCH,
		GET_JOINT_TRANSFORM,
		SET_JOINT_TRANSFORM,
		RUNTIME_MATH,
		TWO_BONE_IK,
		LOOK_AT,
		JOINT_HINGE_LIMIT,
		JOINT_CONE_LIMIT,
		JOINT_CONE_ASYM_LIMIT,
		JOINT_TWIST_LIMIT,
		JOINT_HINGE_TWIST_LIMIT,
		JOINT_CONE_TWIST_LIMIT,
		JOINT_CONE_ASYM_TWIST_LIMIT,
		IK_CHAIN,
		JOINT_LIMIT_SET,
		LOOK_AT_CHAIN,
		CONVERT_SPACE,
	};

	AnimNodeBase(TYPE type): node_type(type) {}
	virtual ~AnimNodeBase() = default;

	// internal
	UGUID runtime_guid;
	int pose_layer{-1};
	bool need_update{false};
	bool need_evaluate{false};
	// Set by codegen: node lives outside the pose-flow tree (constraint-only Joint*Limit, Limit Set).
	bool manual_active{false};
	// Set by codegen: first consumer of OUTPUT_CONSTRAINT - its weight drives highlight intensity.
	AnimNodeBase *consumer_for_weight{nullptr};
	float accumulated_weight{0.0f};
	TYPE node_type;

	Transform root_motion_delta;

	virtual void grabConstPoseLayer(AnimGraphContext & /*context*/) {}

	virtual void update(AnimGraphContext & /*context*/) {}
	virtual void evaluate(AnimGraphContext & /*context*/) {}

	bool takeAndClearNeedUpdate()
	{
		const bool value = need_update;
		need_update = false;
		return value;
	}

	bool takeAndClearNeedEvaluate()
	{
		const bool value = need_evaluate;
		need_evaluate = false;
		return value;
	}

	int getSafePoseLayer(AnimGraphContext &context)
	{
		if (pose_layer != -1)
			return pose_layer;

		// Silent fallback — diagnostics are the caller's job.
		const int fallback_layer = context.getTmpPoseLayer();
		context.skeleton_pose->resetLayerToBindPose(fallback_layer);
		return fallback_layer;
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Pose Link Base
///////////////////////////////////////////////////////////////////////////////////////////////////////
template<typename AnimNodeType>
struct PoseLinkBase
{
	using NodeType = AnimNodeType;

	AnimNodeType *node{nullptr};

	void updateOutputPose(AnimGraphContext &context, float child_weight)
	{
		if (node != nullptr)
		{
			node->accumulated_weight = max(node->accumulated_weight, child_weight);
			node->update(context);
		}
	}

	void evaluateOutputPose(AnimGraphContext &context)
	{
		if (node != nullptr)
			node->evaluate(context);
	}

	int getOutputPose(AnimGraphContext &context) const
	{
		if (node != nullptr && node->pose_layer != -1)
			return node->pose_layer;

		const int fallback_layer = context.getTmpPoseLayer();
		context.skeleton_pose->resetLayerToBindPose(fallback_layer);
		return fallback_layer;
	}

	Transform getRootMotionDelta() const
	{
		return node ? node->root_motion_delta : transform_identity;
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Pose Link
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct PoseLink: PoseLinkBase<AnimNodeBase>
{};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Scalar Link — sources are pulled in evaluate() order, so pose-dependent producers are read fresh.
///////////////////////////////////////////////////////////////////////////////////////////////////////
template<typename T>
struct ScalarLink
{
	bool enabled{false}; // set in codegen when scalar input is connected
	AnimNodeBase *source{nullptr};
	T *value_ptr{nullptr}; // null for non-runtime connections
	T default_value{};

	void updateSource(AnimGraphContext &context, float child_weight)
	{
		if (source != nullptr)
		{
			source->accumulated_weight = max(source->accumulated_weight, child_weight);
			source->update(context);
		}
	}

	void evaluateSource(AnimGraphContext &context)
	{
		if (source != nullptr)
			source->evaluate(context);
	}

	T getValue() const
	{
		return (value_ptr != nullptr) ? *value_ptr : default_value;
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Op is a captureless lambda decayed to a function pointer — no std::function heap alloc.
// Field names are a codegen contract: `link_0`..., `output_value` (single), `output_0`... (multi).
///////////////////////////////////////////////////////////////////////////////////////////////////////

template<typename TOut, typename TIn0>
struct AnimNodeRuntimeUnary: AnimNodeBase
{
	AnimNodeRuntimeUnary(): AnimNodeBase(TYPE::RUNTIME_MATH) {}
	ScalarLink<TIn0> link_0;
	TOut output_value{};
	using OpFn = TOut (*)(const TIn0 &);
	OpFn op{nullptr};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_0.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		link_0.evaluateSource(context);
		if (op != nullptr)
			output_value = op(link_0.getValue());
	}
};

template<typename TOut, typename TIn0, typename TIn1>
struct AnimNodeRuntimeBinary: AnimNodeBase
{
	AnimNodeRuntimeBinary(): AnimNodeBase(TYPE::RUNTIME_MATH) {}
	ScalarLink<TIn0> link_0;
	ScalarLink<TIn1> link_1;
	TOut output_value{};
	using OpFn = TOut (*)(const TIn0 &, const TIn1 &);
	OpFn op{nullptr};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_0.updateSource(context, accumulated_weight);
		link_1.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		link_0.evaluateSource(context);
		link_1.evaluateSource(context);
		if (op != nullptr)
			output_value = op(link_0.getValue(), link_1.getValue());
	}
};

template<typename TOut, typename TIn0, typename TIn1, typename TIn2>
struct AnimNodeRuntimeTernary: AnimNodeBase
{
	AnimNodeRuntimeTernary(): AnimNodeBase(TYPE::RUNTIME_MATH) {}
	ScalarLink<TIn0> link_0;
	ScalarLink<TIn1> link_1;
	ScalarLink<TIn2> link_2;
	TOut output_value{};
	using OpFn = TOut (*)(const TIn0 &, const TIn1 &, const TIn2 &);
	OpFn op{nullptr};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_0.updateSource(context, accumulated_weight);
		link_1.updateSource(context, accumulated_weight);
		link_2.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		link_0.evaluateSource(context);
		link_1.evaluateSource(context);
		link_2.evaluateSource(context);
		if (op != nullptr)
			output_value = op(link_0.getValue(), link_1.getValue(), link_2.getValue());
	}
};

template<typename TOut, typename TIn0, typename TIn1, typename TIn2, typename TIn3>
struct AnimNodeRuntimeQuaternary: AnimNodeBase
{
	AnimNodeRuntimeQuaternary(): AnimNodeBase(TYPE::RUNTIME_MATH) {}
	ScalarLink<TIn0> link_0;
	ScalarLink<TIn1> link_1;
	ScalarLink<TIn2> link_2;
	ScalarLink<TIn3> link_3;
	TOut output_value{};
	using OpFn = TOut (*)(const TIn0 &, const TIn1 &, const TIn2 &, const TIn3 &);
	OpFn op{nullptr};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_0.updateSource(context, accumulated_weight);
		link_1.updateSource(context, accumulated_weight);
		link_2.updateSource(context, accumulated_weight);
		link_3.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		link_0.evaluateSource(context);
		link_1.evaluateSource(context);
		link_2.evaluateSource(context);
		link_3.evaluateSource(context);
		if (op != nullptr)
			output_value = op(link_0.getValue(), link_1.getValue(), link_2.getValue(), link_3.getValue());
	}
};

template<typename TOut, typename TIn0, typename TIn1, typename TIn2, typename TIn3, typename TIn4>
struct AnimNodeRuntimeQuinary: AnimNodeBase
{
	AnimNodeRuntimeQuinary(): AnimNodeBase(TYPE::RUNTIME_MATH) {}
	ScalarLink<TIn0> link_0;
	ScalarLink<TIn1> link_1;
	ScalarLink<TIn2> link_2;
	ScalarLink<TIn3> link_3;
	ScalarLink<TIn4> link_4;
	TOut output_value{};
	using OpFn = TOut (*)(const TIn0 &, const TIn1 &, const TIn2 &, const TIn3 &, const TIn4 &);
	OpFn op{nullptr};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_0.updateSource(context, accumulated_weight);
		link_1.updateSource(context, accumulated_weight);
		link_2.updateSource(context, accumulated_weight);
		link_3.updateSource(context, accumulated_weight);
		link_4.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		link_0.evaluateSource(context);
		link_1.evaluateSource(context);
		link_2.evaluateSource(context);
		link_3.evaluateSource(context);
		link_4.evaluateSource(context);
		if (op != nullptr)
			output_value = op(link_0.getValue(), link_1.getValue(), link_2.getValue(), link_3.getValue(), link_4.getValue());
	}
};

template<typename TIn0, typename TOut0, typename TOut1, typename TOut2>
struct AnimNodeRuntimeUnaryTripleOut: AnimNodeBase
{
	AnimNodeRuntimeUnaryTripleOut(): AnimNodeBase(TYPE::RUNTIME_MATH) {}
	ScalarLink<TIn0> link_0;
	TOut0 output_0{};
	TOut1 output_1{};
	TOut2 output_2{};
	using OpFn = void (*)(const TIn0 &, TOut0 &, TOut1 &, TOut2 &);
	OpFn op{nullptr};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_0.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		link_0.evaluateSource(context);
		if (op != nullptr)
			op(link_0.getValue(), output_0, output_1, output_2);
	}
};

template<typename TIn0, typename TIn1, typename TOut0, typename TOut1>
struct AnimNodeRuntimeBinaryDualOut: AnimNodeBase
{
	AnimNodeRuntimeBinaryDualOut(): AnimNodeBase(TYPE::RUNTIME_MATH) {}
	ScalarLink<TIn0> link_0;
	ScalarLink<TIn1> link_1;
	TOut0 output_0{};
	TOut1 output_1{};
	using OpFn = void (*)(const TIn0 &, const TIn1 &, TOut0 &, TOut1 &);
	OpFn op{nullptr};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_0.updateSource(context, accumulated_weight);
		link_1.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		link_0.evaluateSource(context);
		link_1.evaluateSource(context);
		if (op != nullptr)
			op(link_0.getValue(), link_1.getValue(), output_0, output_1);
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Final Pose
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeFinalPose: AnimNodeBase
{
	AnimNodeFinalPose(): AnimNodeBase(TYPE::FINAL_POSE) {}

	// internal
	PoseLink link;

	void grabConstPoseLayer(AnimGraphContext & /*context*/) override { pose_layer = 0; }

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		accumulated_weight = 1.0f;
		link.updateOutputPose(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link.node == nullptr)
		{
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link.evaluateOutputPose(context);
		context.skeleton_pose->copyLayer(pose_layer, link.getOutputPose(context));

		if (context.isActiveRootMotion())
		{
			root_motion_delta = link.getRootMotionDelta();
			context.root_motion_delta = root_motion_delta;
		}
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Animation Pose
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeAnimationPose: AnimNodeBase
{
	AnimNodeAnimationPose(): AnimNodeBase(TYPE::ANIMATION_POSE) {}

	// read / write prop
	UGUID asset_guid;
	float frame{0.0f};

	void grabConstPoseLayer(AnimGraphContext &context) override { pose_layer = context.skeleton_pose->addLayer(); }

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		const float num_frames = itof(context.skeleton_pose->getLayerNumFrames(pose_layer));
		frame = clamp(frame, 0.0f, max(0.0f, num_frames - 1.0f));
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		context.skeleton_pose->setLayerFrame(pose_layer, frame);
	}

	void init(AnimGraphContext &context, const UGUID &in_asset_guid)
	{
		asset_guid = in_asset_guid;
		context.skeleton_pose->setLayerAnimationFileGUID(pose_layer, asset_guid);
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Animation Player
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeAnimationPlayerBase: AnimNodeBase
{
	AnimNodeAnimationPlayerBase(TYPE type): AnimNodeBase(type) {}

	// read / write prop
	UGUID asset_guid;
	float speed{1.0f};

	// read only prop
	float duration{0.0f};
	float current_time{0.0f};

	// internal
	bool was_inactive{false};
	float previous_time{0.0f};
	bool is_time_forward{true};

	// TELEPORT: no root motion from the jump
	void teleportTime(float t)
	{
		current_time = t;
		previous_time = t;
	}

	// ADVANCE: produces a root motion delta
	void advanceTime(float new_time, bool is_forward)
	{
		previous_time = current_time;
		current_time = new_time;
		is_time_forward = is_forward;

		// Enforce direction: clamp micro-drift from float precision
		const float diff = current_time - previous_time;
		if (is_time_forward && -0.01f < diff && diff < 0.0f)
			previous_time = current_time;
		else if (is_time_forward == false && 0.0f < diff && diff < 0.01f)
			previous_time = current_time;
	}

	// BS2DSync drift protection: syncker-assigned time can contradict the leader direction.
	void clampPrevTimeIfDrift(float new_time, bool is_forward)
	{
		const float diff = new_time - previous_time;
		if (is_forward && -0.01f < diff && diff < 0.0f)
			previous_time = new_time;
		else if (is_forward == false && 0.0f < diff && diff < 0.01f)
			previous_time = new_time;
	}

	void grabConstPoseLayer(AnimGraphContext &context) override { pose_layer = context.skeleton_pose->addLayer(); }

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		context.skeleton_pose->setLayerTime(pose_layer, current_time);

		if (context.skeleton_pose->isLayerAnimationRootMotionPresent(pose_layer) && context.isActiveRootMotion())
		{
			// safety net: a backward jump here means a call site skipped teleportTime/advanceTime.
			float safe_previous_time = previous_time;
			if (Consts::EPS < duration)
			{
				if (is_time_forward && current_time < previous_time)
				{
					const float forward_path = duration - previous_time + current_time;
					if ((previous_time - current_time) < forward_path)
						safe_previous_time = current_time;
				} else if (is_time_forward == false && previous_time < current_time)
				{
					const float backward_path = duration - current_time + previous_time;
					if ((current_time - previous_time) < backward_path)
						safe_previous_time = current_time;
				}
			}

			root_motion_delta = context.skeleton_pose->extractLayerRootMotionByTimes(pose_layer, safe_previous_time, current_time, is_time_forward);
			context.skeleton_pose->removeLayerRootMotion(pose_layer);
		}
	}
};

struct AnimNodeAnimationPlayer: AnimNodeAnimationPlayerBase
{
	AnimNodeAnimationPlayer(): AnimNodeAnimationPlayerBase(TYPE::ANIMATION_PLAYER) {}

	// read / write prop
	bool is_loop{false};

	int syncker_index{-1};
	AnimSyncker::ROLE syncker_role{AnimSyncker::ROLE::WEIGHTED_LEADER_AND_FOLLOWER};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		if (was_inactive)
		{
			was_inactive = false;
			syncToGroup(context, true); // before_update: other nodes may still have need_update=true
			teleportTime(current_time); // no delta on reactivation
		}

		if (syncker_index == -1)
			context.self_timed_players.append(this);
	}

	void nextTime(AnimGraphContext &context)
	{
		is_time_forward = (0.0f <= speed);
		float new_time = current_time;
		getNextTime(context.ifps, new_time);
		advanceTime(new_time, is_time_forward);
	}

	void getNextTime(float ifps, float &out_next_time) const
	{
		out_next_time = current_time + ifps * speed;
		if (is_loop && Consts::EPS < duration)
		{
			if (out_next_time < 0.0f || duration < out_next_time)
				out_next_time = fmod(fmod(out_next_time, duration) + duration, duration);
		} else
		{
			out_next_time = clamp(out_next_time, 0.0f, duration);
		}
	}

	// before_update: need_update is stale, wasInactive() tells who is active
	void syncToGroup(AnimGraphContext &context, bool before_update = false)
	{
		if (syncker_index != -1 && context.synckers[syncker_index].hasOtherActiveMembers(this))
		{
			auto &syncker = context.synckers[syncker_index];
			syncker.rebuildCommonHashes(context, before_update);
			if (syncker.syncTimeByMarkers(context, this, before_update) == false)
				current_time = syncker.normalized_time * duration;
		} else
		{
			current_time = 0.0f;
			if (syncker_index != -1)
				context.synckers[syncker_index].normalized_time = 0.0f;
		}
	}

	void init(AnimGraphContext &context, const UGUID &in_asset_guid, float in_speed, bool in_is_loop)
	{
		asset_guid = in_asset_guid;
		speed = in_speed;
		context.skeleton_pose->setLayerAnimationFileGUID(pose_layer, asset_guid);
		duration = max(0.0f, context.skeleton_pose->getLayerDuration(pose_layer));
		is_loop = in_is_loop;
	}
};

// Returns false once the intersection is empty — caller should early-exit.
UNIGINE_INLINE bool intersectLayerMarkers(const NodeSkeletonPosePtr &skeleton_pose, int pose_layer, Vector<unsigned int> &hashes, bool &first)
{
	int num = skeleton_pose->getLayerNumMarkerSyncs(pose_layer);
	if (num == 0)
	{
		hashes.clear();
		return false;
	}

	if (first)
	{
		for (int i = 0; i < num; i++)
			hashes.appendUnique(skeleton_pose->getLayerMarkerSyncNameHash(pose_layer, i));
		first = false;
	} else
	{
		for (int i = hashes.size() - 1; 0 <= i; i--)
		{
			bool found = false;
			for (int j = 0; j < num; j++)
			{
				if (hashes[i] == skeleton_pose->getLayerMarkerSyncNameHash(pose_layer, j))
				{
					found = true;
					break;
				}
			}
			if (found == false)
				hashes.remove(i);
		}
	}

	return true;
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Animation Player Sync
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeAnimationPlayerSync: AnimNodeAnimationPlayerBase
{
	AnimNodeAnimationPlayerSync(): AnimNodeAnimationPlayerBase(TYPE::ANIMATION_PLAYER_SYNC) {}

	void applyNewTime(float in_new_time, bool in_is_forward)
	{
		clampPrevTimeIfDrift(in_new_time, in_is_forward);
		current_time = in_new_time;
		is_time_forward = in_is_forward;
	}

	void update(AnimGraphContext & /*context*/) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		previous_time = current_time;
		if (was_inactive)
		{
			was_inactive = false;
			previous_time = current_time; // no delta on reactivation
		}
	}

	void init(AnimGraphContext &context, const UGUID &in_asset_guid, float in_speed)
	{
		asset_guid = in_asset_guid;
		speed = in_speed;
		context.skeleton_pose->setLayerAnimationFileGUID(pose_layer, asset_guid);
		duration = max(0.0f, context.skeleton_pose->getLayerDuration(pose_layer));
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Sync Pose Link
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct SyncPoseLink: PoseLinkBase<AnimNodeAnimationPlayerSync>
{};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Make Additive
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeMakeAdditive: AnimNodeBase
{
	AnimNodeMakeAdditive(): AnimNodeBase(TYPE::MAKE_ADDITIVE) {}

	// internal
	PoseLink link_base_pose;
	PoseLink link_ref_pose;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_base_pose.updateOutputPose(context, accumulated_weight);
		link_ref_pose.updateOutputPose(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_base_pose.node == nullptr || link_ref_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_base_pose.evaluateOutputPose(context);
		link_ref_pose.evaluateOutputPose(context);

		pose_layer = context.getTmpPoseLayer();
		context.skeleton_pose->inverseLayer(pose_layer, link_ref_pose.getOutputPose(context));
		context.skeleton_pose->mulLayer(pose_layer, pose_layer, link_base_pose.getOutputPose(context));

		if (context.isActiveRootMotion())
			root_motion_delta = link_base_pose.getRootMotionDelta();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Apply Additive
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeApplyAdditive: AnimNodeBase
{
	AnimNodeApplyAdditive(): AnimNodeBase(TYPE::APPLY_ADDITIVE) {}

	// read / write prop
	float weight{0.0f};

	// internal
	PoseLink link_base_pose;
	PoseLink link_additive;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		weight = saturate(weight);
		link_base_pose.updateOutputPose(context, accumulated_weight);
		if (Consts::EPS < weight)
			link_additive.updateOutputPose(context, accumulated_weight * weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_base_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_base_pose.evaluateOutputPose(context);
		if (Consts::EPS < weight && link_additive.node)
		{
			link_additive.evaluateOutputPose(context);
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->mulLayer(pose_layer, link_base_pose.getOutputPose(context), link_additive.getOutputPose(context), weight);
		} else
		{
			pose_layer = link_base_pose.getOutputPose(context);
		}

		if (context.isActiveRootMotion())
			root_motion_delta = link_base_pose.getRootMotionDelta();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Graph Mask Builder - sparse mask construction for AnimNodeBlend (JointList / JointSubtree)
///////////////////////////////////////////////////////////////////////////////////////////////////////
namespace AnimGraphMaskBuilder
{
	// out_root_weight: 1.0 when the root joint is in the mask (root motion cache); missing names skipped.
	UNIGINE_INLINE void fromJointList(Vector<short> &out_joints, float &out_root_weight, const ConstSkeletonPtr &skeleton, const char *const *names, int num_names)
	{
		out_joints.clear();
		out_root_weight = 0.0f;

		if (skeleton.isValid() == false)
			return;

		for (int i = 0; i < num_names; i++)
		{
			const int j = skeleton->findJoint(names[i]);
			if (0 <= j)
			{
				out_joints.append(static_cast<short>(j));
				if (j == 0)
					out_root_weight = 1.0f;
			}
		}
	}

	UNIGINE_INLINE void fromJointSubtree(Vector<short> &out_joints, float &out_root_weight, const ConstSkeletonPtr &skeleton, const char *root_name)
	{
		out_joints.clear();
		out_root_weight = 0.0f;

		if (skeleton.isValid() == false || root_name == nullptr)
			return;

		const int root = skeleton->findJoint(root_name);
		if (root < 0)
			return;

		static thread_local Vector<int> stack;
		stack.clear();
		stack.append(root);
		while (stack.empty() == false)
		{
			const int j = stack.takeLast();
			out_joints.append(static_cast<short>(j));
			if (j == 0)
				out_root_weight = 1.0f;
			const int num_children = skeleton->getJointNumChildren(j);
			for (int c = 0; c < num_children; c++)
				stack.append(skeleton->getJointChild(j, c));
		}
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Blend
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeBlend: AnimNodeBase
{
	AnimNodeBlend(): AnimNodeBase(TYPE::BLEND) {}

	// read / write prop
	float weight{0.0f};
	int blend_mask{-1};
	Vector<short> mask_joints;
	float root_mask_weight{0.0f};

	// internal
	PoseLink link_0;
	PoseLink link_1;

	bool hasNoMask() const { return (blend_mask == -1 && mask_joints.empty()); }

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		weight = saturate(weight);
		const bool no_mask = hasNoMask();
		if (weight < Consts::EPS)
		{
			link_0.updateOutputPose(context, accumulated_weight);
		} else if (no_mask && (1.0f - Consts::EPS) < weight)
		{
			link_1.updateOutputPose(context, accumulated_weight);
		} else
		{
			link_0.updateOutputPose(context, accumulated_weight * (no_mask ? (1.0f - weight) : 1.0f));
			link_1.updateOutputPose(context, accumulated_weight * weight);
		}
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_0.node == nullptr && link_1.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		const bool no_mask = hasNoMask();
		if (weight < Consts::EPS || link_1.node == nullptr)
		{
			link_0.evaluateOutputPose(context);
			pose_layer = link_0.getOutputPose(context);

			if (context.isActiveRootMotion())
				root_motion_delta = link_0.getRootMotionDelta();
		} else if (link_0.node == nullptr || (no_mask && (1.0f - Consts::EPS) < weight))
		{
			link_1.evaluateOutputPose(context);
			pose_layer = link_1.getOutputPose(context);

			if (context.isActiveRootMotion())
				root_motion_delta = link_1.getRootMotionDelta();
		} else
		{
			link_0.evaluateOutputPose(context);
			link_1.evaluateOutputPose(context);

			pose_layer = context.getTmpPoseLayer();
			const int layer_0 = link_0.getOutputPose(context);
			const int layer_1 = link_1.getOutputPose(context);

			if (no_mask)
			{
				context.skeleton_pose->lerpLayer(pose_layer, layer_0, layer_1, weight);

				if (context.isActiveRootMotion())
					root_motion_delta = blendTransform(link_0.getRootMotionDelta(), link_1.getRootMotionDelta(), weight);
			} else if (blend_mask != -1)
			{
				context.skeleton_pose->lerpLayerByMask(pose_layer, layer_0, layer_1, blend_mask, weight);

				if (context.isActiveRootMotion())
				{
					const float joint_mask = context.skeleton_pose->getSkeleton()->getBlendMaskInfluence(blend_mask, 0);
					root_motion_delta = blendTransform(link_0.getRootMotionDelta(), link_1.getRootMotionDelta(), weight * joint_mask);
				}
			} else
			{
				// Sparse blend: JointList/JointSubtree carry no per-joint influence, one weight for all.
				context.skeleton_pose->copyLayer(pose_layer, layer_0);
				for (int i = 0, n = mask_joints.size(); i < n; i++)
				{
					const int j = mask_joints[i];
					const vec3 position_0 = context.skeleton_pose->getLayerJointPosition(layer_0, j);
					const vec3 position_1 = context.skeleton_pose->getLayerJointPosition(layer_1, j);
					const quat rotation_0 = context.skeleton_pose->getLayerJointRotation(layer_0, j);
					const quat rotation_1 = context.skeleton_pose->getLayerJointRotation(layer_1, j);
					const vec3 scale_0 = context.skeleton_pose->getLayerJointScale(layer_0, j);
					const vec3 scale_1 = context.skeleton_pose->getLayerJointScale(layer_1, j);
					context.skeleton_pose->setLayerJointPosition(pose_layer, j, lerp(position_0, position_1, weight));
					context.skeleton_pose->setLayerJointRotation(pose_layer, j, slerp(rotation_0, rotation_1, weight));
					context.skeleton_pose->setLayerJointScale(pose_layer, j, lerp(scale_0, scale_1, weight));
				}
				if (context.isActiveRootMotion())
					root_motion_delta = blendTransform(link_0.getRootMotionDelta(), link_1.getRootMotionDelta(), weight * root_mask_weight);
			}
		}
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Branch - evaluates only one of two pose branches based on condition
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeBranch: AnimNodeBase
{
	AnimNodeBranch(): AnimNodeBase(TYPE::BRANCH) {}

	// read / write prop
	bool condition{false};

	// internal
	PoseLink link_true;
	PoseLink link_false;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		if (condition)
			link_true.updateOutputPose(context, accumulated_weight);
		else
			link_false.updateOutputPose(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		PoseLink &active = condition ? link_true : link_false;
		if (active.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		active.evaluateOutputPose(context);
		pose_layer = active.getOutputPose(context);

		if (context.isActiveRootMotion())
			root_motion_delta = active.getRootMotionDelta();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Space conversion
///////////////////////////////////////////////////////////////////////////////////////////////////////
// LOCAL value is named JOINT_LOCAL (not LOCAL) to dodge a `LOCAL` macro in the anim-script compile.
enum class SPACE
{
	WORLD = 0,
	OBJECT = 1,
	JOINT_LOCAL = 2,
};

UNIGINE_INLINE Mat4 spaceToObject(AnimGraphContext &context, SPACE space, int layer, int joint)
{
	switch (space)
	{
		case SPACE::OBJECT:
			return Mat4_identity;

		case SPACE::WORLD:
		{
			Mat4 inverse_transform;
			inverse(inverse_transform, context.skeleton_pose->getWorldTransform());
			return inverse_transform;
		}
		case SPACE::JOINT_LOCAL:
		{
			if (joint < 0)
				return Mat4_identity;

			// root's parent (-1) yields identity = object
			const int parent = context.skeleton_pose->getSkeleton().get()->getJointParent(joint);
			return Mat4(context.skeleton_pose->getLayerJointObjectTransform(layer, parent));
		}
	}
	return Mat4_identity;
}

UNIGINE_INLINE Mat4 spaceConvert(AnimGraphContext &context, SPACE from, SPACE to, int layer, int from_joint, int to_joint)
{
	Mat4 inverse_to;
	inverse(inverse_to, spaceToObject(context, to, layer, to_joint));
	return inverse_to * spaceToObject(context, from, layer, from_joint);
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Convert Space base — one TYPE::CONVERT_SPACE backs all three, so the pose-traversal
// switches cast to THIS base. pose_layer stays -1.
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeConvertSpaceBase: AnimNodeBase
{
	AnimNodeConvertSpaceBase(): AnimNodeBase(TYPE::CONVERT_SPACE) {}

	// read / write prop
	SPACE from_space{SPACE::WORLD};
	SPACE to_space{SPACE::OBJECT};
	int from_joint{-1};
	int to_joint{-1};

	PoseLink link_pose;

	bool needsPose() const { return (from_space == SPACE::JOINT_LOCAL || to_space == SPACE::JOINT_LOCAL); }
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Convert Position
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeConvertPosition: AnimNodeConvertSpaceBase
{
	ScalarLink<vec3> link_in;
	vec3 output_value{vec3_zero};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		if (needsPose())
			link_pose.updateOutputPose(context, accumulated_weight);
		link_in.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		int layer = -1;
		if (needsPose())
		{
			link_pose.evaluateOutputPose(context);
			layer = link_pose.getOutputPose(context);
		}
		link_in.evaluateSource(context);
		const Mat4 transform = spaceConvert(context, from_space, to_space, layer, from_joint, to_joint);
		output_value = vec3(transform * Vec3(link_in.getValue()));
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Convert Direction
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeConvertDirection: AnimNodeConvertSpaceBase
{
	ScalarLink<vec3> link_in;
	vec3 output_value{vec3_zero};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		if (needsPose())
			link_pose.updateOutputPose(context, accumulated_weight);
		link_in.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		int layer = -1;
		if (needsPose())
		{
			link_pose.evaluateOutputPose(context);
			layer = link_pose.getOutputPose(context);
		}
		link_in.evaluateSource(context);
		const quat rotation = spaceConvert(context, from_space, to_space, layer, from_joint, to_joint).getRotate();
		output_value = rotation * link_in.getValue();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Convert Rotation
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeConvertRotation: AnimNodeConvertSpaceBase
{
	ScalarLink<quat> link_in;
	quat output_value{quat_identity};

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		if (needsPose())
			link_pose.updateOutputPose(context, accumulated_weight);
		link_in.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		int layer = -1;
		if (needsPose())
		{
			link_pose.evaluateOutputPose(context);
			layer = link_pose.getOutputPose(context);
		}
		link_in.evaluateSource(context);
		const quat frame_from = spaceToObject(context, from_space, layer, from_joint).getRotate();
		const quat frame_to = spaceToObject(context, to_space, layer, to_joint).getRotate();
		output_value = abs(inverse(frame_to) * frame_from * link_in.getValue());
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Get Joint Transform
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeGetJointTransform: AnimNodeBase
{
	AnimNodeGetJointTransform(): AnimNodeBase(TYPE::GET_JOINT_TRANSFORM) {}

	// read / write prop
	int joint_index{-1};
	bool object_space{false};

	// scalar outputs (read by consumers via direct pointer)
	vec3 output_position{vec3_zero};
	quat output_rotation{quat_identity};
	vec3 output_scale{vec3_one};

	// internal
	PoseLink link_pose;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_pose.updateOutputPose(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_pose.node == nullptr || joint_index < 0)
		{
			output_position = vec3_zero;
			output_rotation = quat_identity;
			output_scale = vec3_one;
			return;
		}

		link_pose.evaluateOutputPose(context);
		const int in_layer = link_pose.getOutputPose(context);

		if (object_space)
		{
			const mat4 object_transform = context.skeleton_pose->getLayerJointObjectTransform(in_layer, joint_index);
			decomposeTransform(object_transform, output_position, output_rotation, output_scale);
		} else
		{
			output_position = context.skeleton_pose->getLayerJointPosition(in_layer, joint_index);
			output_rotation = context.skeleton_pose->getLayerJointRotation(in_layer, joint_index);
			output_scale = context.skeleton_pose->getLayerJointScale(in_layer, joint_index);
		}
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Set Joint Transform
// An unconnected component is preserved from the input pose, in either space.
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeSetJointTransform: AnimNodeBase
{
	AnimNodeSetJointTransform(): AnimNodeBase(TYPE::SET_JOINT_TRANSFORM) {}

	// read / write prop
	int joint_index{-1};
	bool object_space{false};

	// internal
	PoseLink link_pose;

	// link_*.enabled controls whether the component is modified.
	ScalarLink<vec3> link_position;
	ScalarLink<quat> link_rotation;
	ScalarLink<vec3> link_scale;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_pose.updateOutputPose(context, accumulated_weight);
		link_position.updateSource(context, accumulated_weight);
		link_rotation.updateSource(context, accumulated_weight);
		link_scale.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_pose.evaluateOutputPose(context);
		const int in_layer = link_pose.getOutputPose(context);

		link_position.evaluateSource(context);
		link_rotation.evaluateSource(context);
		link_scale.evaluateSource(context);

		pose_layer = context.getTmpPoseLayer();
		context.skeleton_pose->copyLayer(pose_layer, in_layer);

		if (0 <= joint_index)
		{
			mat4 parent_object_transform = mat4_identity;
			quat parent_object_rotation = quat_identity;
			vec3 parent_object_scale = vec3_one;
			if (object_space && (link_position.enabled || link_rotation.enabled || link_scale.enabled))
			{
				const Skeleton *skeleton = context.skeleton_pose->getSkeleton().get();
				parent_object_transform = context.skeleton_pose->getLayerJointObjectTransform(pose_layer, skeleton->getJointParent(joint_index));
				parent_object_rotation = parent_object_transform.getRotate();
				parent_object_scale = parent_object_transform.getScale();
			}

			if (link_position.enabled)
			{
				vec3 position = link_position.getValue();
				if (object_space)
					position = inverse(parent_object_transform) * position;
				context.skeleton_pose->setLayerJointPosition(pose_layer, joint_index, position);
			}
			if (link_rotation.enabled)
			{
				quat rotation = link_rotation.getValue();
				if (object_space)
					rotation = inverse(parent_object_rotation) * rotation;
				context.skeleton_pose->setLayerJointRotation(pose_layer, joint_index, rotation);
			}
			if (link_scale.enabled)
			{
				vec3 scale = link_scale.getValue();
				if (object_space)
					scale = vec3(scale.x / parent_object_scale.x, scale.y / parent_object_scale.y, scale.z / parent_object_scale.z);
				context.skeleton_pose->setLayerJointScale(pose_layer, joint_index, scale);
			}
		}

		if (context.isActiveRootMotion())
			root_motion_delta = link_pose.getRootMotionDelta();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Two Bone IK
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeTwoBoneIK: AnimNodeBase
{
	AnimNodeTwoBoneIK(): AnimNodeBase(TYPE::TWO_BONE_IK) {}

	// internal
	PoseLink link_pose;
	IKInfoTwoBonePtr info;
	bool target_in_object_space{false};

	ScalarLink<vec3> link_target;
	ScalarLink<vec3> link_pole;
	ScalarLink<float> link_weight;
	ScalarLink<bool> link_use_pole;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_pose.updateOutputPose(context, accumulated_weight);
		link_target.updateSource(context, accumulated_weight);
		link_pole.updateSource(context, accumulated_weight);
		link_weight.updateSource(context, accumulated_weight);
		link_use_pole.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_pose.evaluateOutputPose(context);
		const int in_layer = link_pose.getOutputPose(context);

		link_target.evaluateSource(context);
		link_pole.evaluateSource(context);
		link_weight.evaluateSource(context);
		link_use_pole.evaluateSource(context);

		if (info.isValid() == false)
		{
			pose_layer = in_layer;
			if (context.isActiveRootMotion())
				root_motion_delta = link_pose.getRootMotionDelta();
			return;
		}

		if (target_in_object_space)
		{
			info->setTarget(link_target.getValue());
			info->setPole(link_pole.getValue());
		} else
		{
			Mat4 inverse_mesh_world;
			inverse(inverse_mesh_world, context.skeleton_pose->getWorldTransform());
			info->setTarget(vec3(inverse_mesh_world * Vec3(link_target.getValue())));
			info->setPole(vec3(inverse_mesh_world * Vec3(link_pole.getValue())));
		}
		info->setWeight(link_weight.getValue());
		info->setUsePole(link_use_pole.getValue());

		pose_layer = context.getTmpPoseLayer();
		context.skeleton_pose->copyLayer(pose_layer, in_layer);
		context.skeleton_pose->solveLayerTwoBoneIK(pose_layer, info);

		if (context.isActiveRootMotion())
			root_motion_delta = link_pose.getRootMotionDelta();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Look At
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeLookAt: AnimNodeBase
{
	AnimNodeLookAt(): AnimNodeBase(TYPE::LOOK_AT) {}

	// internal
	PoseLink link_pose;
	LookAtInfoPtr info;
	bool target_in_object_space{false};

	ScalarLink<vec3> link_target;
	ScalarLink<vec3> link_pole;
	ScalarLink<float> link_weight;
	ScalarLink<bool> link_use_pole;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_pose.updateOutputPose(context, accumulated_weight);
		link_target.updateSource(context, accumulated_weight);
		link_pole.updateSource(context, accumulated_weight);
		link_weight.updateSource(context, accumulated_weight);
		link_use_pole.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_pose.evaluateOutputPose(context);
		const int in_layer = link_pose.getOutputPose(context);

		link_target.evaluateSource(context);
		link_pole.evaluateSource(context);
		link_weight.evaluateSource(context);
		link_use_pole.evaluateSource(context);

		if (info.isValid() == false)
		{
			pose_layer = in_layer;
			if (context.isActiveRootMotion())
				root_motion_delta = link_pose.getRootMotionDelta();
			return;
		}

		if (target_in_object_space)
		{
			info->setTarget(link_target.getValue());
			info->setPole(link_pole.getValue());
		} else
		{
			Mat4 inverse_mesh_world;
			inverse(inverse_mesh_world, context.skeleton_pose->getWorldTransform());
			info->setTarget(vec3(inverse_mesh_world * Vec3(link_target.getValue())));
			info->setPole(vec3(inverse_mesh_world * Vec3(link_pole.getValue())));
		}
		info->setWeight(link_weight.getValue());
		info->setUsePole(link_use_pole.getValue());

		pose_layer = context.getTmpPoseLayer();
		context.skeleton_pose->copyLayer(pose_layer, in_layer);
		context.skeleton_pose->solveLayerLookAt(pose_layer, info);

		if (context.isActiveRootMotion())
			root_motion_delta = link_pose.getRootMotionDelta();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Joint Limits
///////////////////////////////////////////////////////////////////////////////////////////////////////
// constraint_only: codegen sets it when the Pose output is unconnected — skips pose work nobody reads.
template<typename InfoT, AnimNodeBase::TYPE TypeID>
struct AnimNodeJointLimitBase: AnimNodeBase
{
	AnimNodeJointLimitBase(): AnimNodeBase(TypeID) {}

	PoseLink link_pose;
	Ptr<InfoT> info;
	ScalarLink<float> link_weight;
	ScalarLink<quat> link_preferred_rotation;
	bool constraint_only{false};

	void update(AnimGraphContext &context) override final
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		// scalar links update even when constraint_only: IK Chain reads weight/preferred during its solve
		if (constraint_only == false)
			link_pose.updateOutputPose(context, accumulated_weight);
		link_weight.updateSource(context, accumulated_weight);
		link_preferred_rotation.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override final
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (constraint_only)
		{
			link_weight.evaluateSource(context);
			link_preferred_rotation.evaluateSource(context);
			if (info.isValid())
			{
				info->setWeight(link_weight.getValue());
				info->setPreferredLocalRotation(link_preferred_rotation.getValue());
			}
			// Default layer (always valid post-evaluate) — preview viz reads joint positions from it.
			pose_layer = 0;
			return;
		}

		if (link_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_pose.evaluateOutputPose(context);
		const int in_layer = link_pose.getOutputPose(context);
		link_weight.evaluateSource(context);
		link_preferred_rotation.evaluateSource(context);

		if (info.isValid() == false)
		{
			pose_layer = in_layer;
			if (context.isActiveRootMotion())
				root_motion_delta = link_pose.getRootMotionDelta();
			return;
		}

		info->setWeight(link_weight.getValue());
		info->setPreferredLocalRotation(link_preferred_rotation.getValue());
		pose_layer = context.getTmpPoseLayer();
		context.skeleton_pose->copyLayer(pose_layer, in_layer);
		invokeLimitSolver(context, pose_layer);

		if (context.isActiveRootMotion())
			root_motion_delta = link_pose.getRootMotionDelta();
	}

protected:
	virtual void invokeLimitSolver(AnimGraphContext &context, int layer) = 0;
};

struct AnimNodeJointHingeLimit: AnimNodeJointLimitBase<JointLimitInfoHinge, AnimNodeBase::TYPE::JOINT_HINGE_LIMIT>
{
protected:
	void invokeLimitSolver(AnimGraphContext &context, int layer) override
	{
		context.skeleton_pose->solveLayerJointHingeLimit(layer, info);
	}
};

struct AnimNodeJointConeLimit: AnimNodeJointLimitBase<JointLimitInfoCone, AnimNodeBase::TYPE::JOINT_CONE_LIMIT>
{
protected:
	void invokeLimitSolver(AnimGraphContext &context, int layer) override
	{
		context.skeleton_pose->solveLayerJointConeLimit(layer, info);
	}
};

struct AnimNodeJointConeAsymLimit: AnimNodeJointLimitBase<JointLimitInfoConeAsym, AnimNodeBase::TYPE::JOINT_CONE_ASYM_LIMIT>
{
protected:
	void invokeLimitSolver(AnimGraphContext &context, int layer) override
	{
		context.skeleton_pose->solveLayerJointConeAsymLimit(layer, info);
	}
};

struct AnimNodeJointTwistLimit: AnimNodeJointLimitBase<JointLimitInfoTwist, AnimNodeBase::TYPE::JOINT_TWIST_LIMIT>
{
protected:
	void invokeLimitSolver(AnimGraphContext &context, int layer) override
	{
		context.skeleton_pose->solveLayerJointTwistLimit(layer, info);
	}
};

struct AnimNodeJointHingeTwistLimit: AnimNodeJointLimitBase<JointLimitInfoHingeTwist, AnimNodeBase::TYPE::JOINT_HINGE_TWIST_LIMIT>
{
protected:
	void invokeLimitSolver(AnimGraphContext &context, int layer) override
	{
		context.skeleton_pose->solveLayerJointHingeTwistLimit(layer, info);
	}
};

struct AnimNodeJointConeTwistLimit: AnimNodeJointLimitBase<JointLimitInfoConeTwist, AnimNodeBase::TYPE::JOINT_CONE_TWIST_LIMIT>
{
protected:
	void invokeLimitSolver(AnimGraphContext &context, int layer) override
	{
		context.skeleton_pose->solveLayerJointConeTwistLimit(layer, info);
	}
};

struct AnimNodeJointConeAsymTwistLimit: AnimNodeJointLimitBase<JointLimitInfoConeAsymTwist, AnimNodeBase::TYPE::JOINT_CONE_ASYM_TWIST_LIMIT>
{
protected:
	void invokeLimitSolver(AnimGraphContext &context, int layer) override
	{
		context.skeleton_pose->solveLayerJointConeAsymTwistLimit(layer, info);
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node IK Chain
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeIKChain: AnimNodeBase
{
	AnimNodeIKChain(): AnimNodeBase(TYPE::IK_CHAIN) {}

	// internal
	PoseLink link_pose;
	IKInfoChainPtr info;
	bool target_in_object_space{false};

	ScalarLink<vec3> link_target;
	ScalarLink<vec3> link_pole;
	ScalarLink<float> link_weight;

	// Set built for a single Joint*Limit on the Limits pin; null when a Joint Limit Set node feeds it.
	JointLimitSetInfoPtr implicit_limit_set;

	// Previous frame's solved rotations: without them the greedy solver flips bend direction -> jitter.
	Vector<quat> warm_start_rotations;
	bool warm_start_valid{false};
	ConstSkeletonPtr warm_start_skeleton;
	Vector<int> warm_start_joints;

	// Output filter, applied AFTER warm-start capture so the solver keeps warm-starting from raw output.
	// output_smooth_time (sec) — fixed-tau low-pass; max_angular_speed (deg/sec) — rate clamp; 0 disables.
	float output_smooth_time{0.0f};
	float max_angular_speed{0.0f};
	Vector<quat> output_filtered_rotations;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_pose.updateOutputPose(context, accumulated_weight);
		link_target.updateSource(context, accumulated_weight);
		link_pole.updateSource(context, accumulated_weight);
		link_weight.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_pose.evaluateOutputPose(context);
		const int in_layer = link_pose.getOutputPose(context);

		link_target.evaluateSource(context);
		link_pole.evaluateSource(context);
		link_weight.evaluateSource(context);

		if (info.isValid() == false)
		{
			pose_layer = in_layer;
			if (context.isActiveRootMotion())
				root_motion_delta = link_pose.getRootMotionDelta();
			return;
		}

		if (target_in_object_space)
		{
			info->setTarget(link_target.getValue());
			info->setPole(link_pole.getValue());
		} else
		{
			Mat4 inverse_mesh_world;
			inverse(inverse_mesh_world, context.skeleton_pose->getWorldTransform());
			info->setTarget(vec3(inverse_mesh_world * Vec3(link_target.getValue())));
			info->setPole(vec3(inverse_mesh_world * Vec3(link_pole.getValue())));
		}
		// Weight is applied here, not by the solver: the layer holds the warm start, so a solver-side
		// blend would lag towards full IK instead of mixing with the animation still kept in in_layer.
		const float ik_weight = saturate(link_weight.getValue());
		info->setWeight(1.0f);

		pose_layer = context.getTmpPoseLayer();
		context.skeleton_pose->copyLayer(pose_layer, in_layer);

		if (ik_weight < Consts::EPS)
		{
			if (context.isActiveRootMotion())
				root_motion_delta = link_pose.getRootMotionDelta();
			return; // IK off: the animation pose passes through untouched
		}

		ConstSkeletonPtr skeleton = context.skeleton_pose->getSkeleton();
		const int num_chain = info->getNumJoints();

		// Also invalidates on re-route: same chain length, different joints.
		bool joints_changed = (warm_start_joints.size() != num_chain);
		for (int i = 0; i < num_chain && joints_changed == false; i++)
			joints_changed = (warm_start_joints[i] != info->getJoint(i));
		if (warm_start_skeleton != skeleton || joints_changed)
		{
			warm_start_valid = false;
			warm_start_rotations.clear();
			warm_start_rotations.resize(num_chain);
			warm_start_skeleton = skeleton;
			warm_start_joints.clear();
			warm_start_joints.resize(num_chain);
			for (int i = 0; i < num_chain; i++)
				warm_start_joints[i] = info->getJoint(i);
			output_filtered_rotations.clear();
		}

		// The RAW previous output is the continuity anchor of the multi-start escape.
		if (warm_start_valid && skeleton.isValid())
		{
			const int num_joints = skeleton->getNumJoints();
			for (int i = 0; i < num_chain; i++)
			{
				const int j = info->getJoint(i);
				if (j < 0 || num_joints <= j)
					continue;

				context.skeleton_pose->setLayerJointRotation(pose_layer, j, warm_start_rotations[i]);
			}
		}

		// No warm start, no stream to protect — the escape has to re-elect the best candidate.
		if (warm_start_valid == false)
			info->resetSolveState();
		context.skeleton_pose->solveLayerIKChain(pose_layer, info);

		// Capture BEFORE the weight blend and the filter: a blended anchor would drift toward the animation.
		// A non-finite solve must NOT latch here: the anchor is restored before every solve, so it would
		// outlive the bad input (NaN target/weight) and keep the chain dead long after the cause is fixed.
		if (skeleton.isValid() && 0 < num_chain)
		{
			const int num_joints = skeleton->getNumJoints();
			bool finite = true;
			for (int i = 0; i < num_chain; i++)
			{
				const int j = info->getJoint(i);
				if (0 <= j && j < num_joints)
				{
					const quat rot = context.skeleton_pose->getLayerJointRotation(pose_layer, j);
					finite = (finite && isNan(rot.w) == false);
					warm_start_rotations[i] = rot;
				}
			}
			warm_start_valid = finite;
		}

		const bool filter_enabled = (0.0f < output_smooth_time || 0.0f < max_angular_speed);
		if (filter_enabled && warm_start_valid && skeleton.isValid())
		{
			const int num_joints = skeleton->getNumJoints();
			if (0.0f < context.ifps)
			{
				const float dt = context.ifps;
				const float tau = output_smooth_time;
				const float max_step_deg = max_angular_speed * dt;
				const float alpha = (0.0f < tau) ? (1.0f - exp(-dt / tau)) : 1.0f;

				// First valid use after invalidation: seed from raw, no smoothing this frame.
				if (output_filtered_rotations.size() != num_chain)
					output_filtered_rotations = warm_start_rotations;

				for (int i = 0; i < num_chain; i++)
				{
					const int j = info->getJoint(i);
					if (j < 0 || num_joints <= j)
						continue;

					quat raw = warm_start_rotations[i];
					const quat prev = output_filtered_rotations[i];

					// Rate clamp BEFORE the low-pass: it clips basin-flip outliers.
					if (0.0f < max_step_deg)
					{
						const float angle = getAngle(prev, raw);
						if (max_step_deg < angle)
							raw = slerp(prev, raw, max_step_deg / angle);
					}

					const quat filtered = (0.0f < tau) ? slerp(prev, raw, alpha) : raw;
					output_filtered_rotations[i] = filtered;
					context.skeleton_pose->setLayerJointRotation(pose_layer, j, filtered);
				}
			} else if (output_filtered_rotations.size() == num_chain)
			{
				// dt == 0 (pause): hold the filtered pose — exposing the raw solve would pop by the filter lag.
				for (int i = 0; i < num_chain; i++)
				{
					const int j = info->getJoint(i);
					if (j < 0 || num_joints <= j)
						continue;

					context.skeleton_pose->setLayerJointRotation(pose_layer, j, output_filtered_rotations[i]);
				}
			}
		} else if (filter_enabled == false && output_filtered_rotations.size() != 0)
		{
			// Toggled off at runtime: drop the state so a re-enable reseeds from raw, not from a stale pose.
			output_filtered_rotations.clear();
		}

		// Mix LAST: the filter rewrites the chain from its own raw copy and would throw an earlier mix away.
		// Only the chain joints differ between the layers, so a whole-layer lerp gives the same result.
		if (ik_weight < 1.0f)
			context.skeleton_pose->lerpLayer(pose_layer, in_layer, pose_layer, ik_weight);

		if (context.isActiveRootMotion())
			root_motion_delta = link_pose.getRootMotionDelta();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Joint Limit Set
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeJointLimitSet: AnimNodeBase
{
	AnimNodeJointLimitSet(): AnimNodeBase(TYPE::JOINT_LIMIT_SET) {}

	JointLimitSetInfoPtr info;

	void update(AnimGraphContext & /*context*/) override {}
	void evaluate(AnimGraphContext & /*context*/) override {}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Look At Chain
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeLookAtChain: AnimNodeBase
{
	AnimNodeLookAtChain(): AnimNodeBase(TYPE::LOOK_AT_CHAIN) {}

	// internal
	PoseLink link_pose;
	LookAtChainInfoPtr info;
	bool target_in_object_space{false};

	ScalarLink<vec3> link_target;
	ScalarLink<vec3> link_pole;
	ScalarLink<float> link_weight;

	JointLimitSetInfoPtr implicit_limit_set;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_pose.updateOutputPose(context, accumulated_weight);
		link_target.updateSource(context, accumulated_weight);
		link_pole.updateSource(context, accumulated_weight);
		link_weight.updateSource(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_pose.evaluateOutputPose(context);
		const int in_layer = link_pose.getOutputPose(context);

		link_target.evaluateSource(context);
		link_pole.evaluateSource(context);
		link_weight.evaluateSource(context);

		if (info.isValid() == false)
		{
			pose_layer = in_layer;
			if (context.isActiveRootMotion())
				root_motion_delta = link_pose.getRootMotionDelta();
			return;
		}

		if (target_in_object_space)
		{
			info->setTarget(link_target.getValue());
			info->setPole(link_pole.getValue());
		} else
		{
			Mat4 inverse_mesh_world;
			inverse(inverse_mesh_world, context.skeleton_pose->getWorldTransform());
			info->setTarget(vec3(inverse_mesh_world * Vec3(link_target.getValue())));
			// Pole is a POINT, not a vector: the solver takes normalize(pole - bone_pos) per bone.
			info->setPole(vec3(inverse_mesh_world * Vec3(link_pole.getValue())));
		}
		info->setWeight(link_weight.getValue());

		// solved from the animation pose every frame: the per-joint weights are shares of the swing, a warm start
		// would turn them into a convergence rate
		pose_layer = context.getTmpPoseLayer();
		context.skeleton_pose->copyLayer(pose_layer, in_layer);
		context.skeleton_pose->solveLayerLookAtChain(pose_layer, info);

		if (context.isActiveRootMotion())
			root_motion_delta = link_pose.getRootMotionDelta();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Blend Space 2D
///////////////////////////////////////////////////////////////////////////////////////////////////////
template<typename Link>
struct AnimNodeBlendSpace2DBase: AnimNodeBase
{
	AnimNodeBlendSpace2DBase(TYPE type): AnimNodeBase(type) {}

	using LinkType = Link;
	using NodeType = typename Link::NodeType;

	// read / write prop
	vec2 blend_point;

	enum class MODE
	{
		CARTESIAN = 0,
		POLAR
	};
	MODE mode{MODE::CARTESIAN};

	bool use_smoothed_weights{false};
	float weight_smoothing_speed{1.0f};

	// internal
	Vector<Link> links;
	Vector<WeightPoint2D> points;
	Vector<float> smoothed_weights;

	void addSample(NodeType *linked_node, const vec2 &anchor)
	{
		Link &link = this->links.append();
		link.node = linked_node;
		this->points.append({anchor});
		smoothed_weights.append(0.0f);
	}

	void snapWeights()
	{
		if (this->points.empty())
			return;

		if (mode == MODE::CARTESIAN)
			blend2DCartesianWeights(blend_point, this->points);
		else
			blend2DPolarWeights(blend_point, this->points);

		for (int i = 0; i < smoothed_weights.size(); i++)
			smoothed_weights[i] = this->points[i].normalized_weight;
	}

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		if (this->links.empty())
			return;

		if (this->links.size() == 1)
		{
			this->points[0].normalized_weight = 1.0f;
			smoothed_weights[0] = 1.0f;
			updateLinks(context);
			return;
		}

		if (mode == MODE::CARTESIAN)
			blend2DCartesianWeights(blend_point, this->points);
		else
			blend2DPolarWeights(blend_point, this->points);

		if (use_smoothed_weights)
		{
			const float max_delta = context.ifps * weight_smoothing_speed;
			float total_weight = 0.0f;
			for (int i = 0; i < smoothed_weights.size(); i++)
			{
				smoothed_weights[i] = move_towards(smoothed_weights[i], this->points[i].normalized_weight, max_delta);
				total_weight += smoothed_weights[i];
			}
			if (Consts::EPS < total_weight)
			{
				for (int i = 0; i < smoothed_weights.size(); i++)
					smoothed_weights[i] /= total_weight;
			}
		} else
		{
			for (int i = 0; i < smoothed_weights.size(); i++)
				smoothed_weights[i] = this->points[i].normalized_weight;
		}

		updateLinks(context);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		for (int i = 0; i < links.size(); i++)
		{
			if ((1.0f - Consts::EPS) < smoothed_weights[i])
			{
				links[i].evaluateOutputPose(context);
				pose_layer = links[i].getOutputPose(context);

				if (context.isActiveRootMotion())
					root_motion_delta = links[i].getRootMotionDelta();

				return;
			}
		}

		pose_layer = context.getTmpPoseLayer();

		float total_weight = 0.0f;
		for (int i = 0; i < links.size(); i++)
		{
			if (smoothed_weights[i] < Consts::EPS)
				continue;

			links[i].evaluateOutputPose(context);

			total_weight += smoothed_weights[i];
			context.skeleton_pose->lerpLayer(pose_layer, pose_layer, links[i].getOutputPose(context), smoothed_weights[i] / total_weight);

			if (context.isActiveRootMotion())
				root_motion_delta = blendTransform(root_motion_delta, links[i].getRootMotionDelta(), smoothed_weights[i] / total_weight);
		}

		// All weights zero — bind pose is the correct result here, not an error.
		if (total_weight < Consts::EPS)
		{
			context.skeleton_pose->resetLayerToBindPose(pose_layer);

			if (context.isActiveRootMotion())
				root_motion_delta = transform_identity;
		}
	}

protected:
	virtual void updateLinks(AnimGraphContext &context)
	{
		for (int i = 0; i < this->links.size(); i++)
		{
			if (Consts::EPS < smoothed_weights[i])
				this->links[i].updateOutputPose(context, this->accumulated_weight * this->smoothed_weights[i]);
		}
	}

	float move_towards(float current, float target, float max_delta)
	{
		const float delta = target - current;
		if (abs(delta) <= max_delta)
			return target;

		return current + sign(delta) * max_delta;
	}
};

struct AnimNodeBlendSpace2D: AnimNodeBlendSpace2DBase<PoseLink>
{
	AnimNodeBlendSpace2D(): AnimNodeBlendSpace2DBase(TYPE::BLEND_SPACE_2D) {}

	bool was_inactive{false};

	void updateLinks(AnimGraphContext &context) override
	{
		if (was_inactive)
		{
			was_inactive = false;
			snapWeights();
		}

		AnimNodeBlendSpace2DBase::updateLinks(context);
	}
};

struct AnimNodeBlendSpace2DSync: AnimNodeBlendSpace2DBase<SyncPoseLink>
{
	AnimNodeBlendSpace2DSync(): AnimNodeBlendSpace2DBase(TYPE::BLEND_SPACE_2D_SYNC) {}

	// read only prop
	float duration{0.0f};
	float normalized_time{0.0f};
	float current_time{0.0f};
	float speed{1.0f};

	int syncker_index{-1};
	AnimSyncker::ROLE syncker_role{AnimSyncker::ROLE::WEIGHTED_LEADER_AND_FOLLOWER};

	// internal
	bool was_inactive{false};

	int getDominantChildIndex() const
	{
		int best = -1;
		float max_weight = -1.0f;
		for (int i = 0; i < links.size(); i++)
		{
			if (links[i].node && max_weight < smoothed_weights[i])
			{
				max_weight = smoothed_weights[i];
				best = i;
			}
		}
		return best;
	}

	void applyNormalizedTime(float in_normalized_time, bool in_is_forward)
	{
		normalized_time = in_normalized_time;
		current_time = normalized_time * duration;
		for (int i = 0; i < links.size(); i++)
		{
			if (links[i].node == nullptr)
				continue;

			links[i].node->applyNewTime(normalized_time * links[i].node->duration, in_is_forward);
		}
	}

	// before_update: need_update is stale, wasInactive() tells who is active
	void syncToGroup(AnimGraphContext &context, bool before_update = false)
	{
		if (syncker_index != -1 && context.synckers[syncker_index].hasOtherActiveMembers(this))
		{
			auto &syncker = context.synckers[syncker_index];
			syncker.rebuildCommonHashes(context, before_update);
			if (syncker.syncTimeByMarkers(context, this, before_update) == false)
				normalized_time = syncker.normalized_time;
		} else
		{
			normalized_time = 0.0f;
			if (syncker_index != -1)
				context.synckers[syncker_index].normalized_time = 0.0f;
		}
		current_time = 0.0f; // recalculated in updateLinks from normalized_time * duration
	}

	void syncChildrenTime()
	{
		for (int i = 0; i < links.size(); i++)
		{
			if (links[i].node == nullptr)
				continue;

			links[i].node->teleportTime(normalized_time * links[i].node->duration);
		}
	}

protected:
	void updateLinks(AnimGraphContext &context) override
	{
		if (was_inactive)
		{
			was_inactive = false;
			snapWeights();
			syncToGroup(context, true); // before_update: other nodes may still have need_update=true
			syncChildrenTime();
		}

		duration = 0.0f;
		speed = 0.0f;
		for (int i = 0; i < links.size(); i++)
		{
			auto *child = links[i].node;
			if (child == nullptr)
				continue;

			duration += child->duration * smoothed_weights[i];
			speed += child->speed * smoothed_weights[i];
		}

		current_time = normalized_time * duration;
		current_time += context.ifps * speed;
		const bool is_forward = (0.0f <= speed);
		if (Consts::EPS < duration)
		{
			current_time = fmod(fmod(current_time, duration) + duration, duration);
			normalized_time = current_time / duration;
		}

		for (int i = 0; i < links.size(); i++)
		{
			auto *child = links[i].node;
			if (child == nullptr || smoothed_weights[i] < Consts::EPS)
				continue;

			if (child->was_inactive)
				child->teleportTime(normalized_time * child->duration);
			links[i].updateOutputPose(context, this->accumulated_weight * smoothed_weights[i]);
			child->applyNewTime(normalized_time * child->duration, is_forward);
		}
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// SyncMember inline implementations (need complete AnimNodeAnimationPlayer & BS2DSync)
///////////////////////////////////////////////////////////////////////////////////////////////////////
UNIGINE_INLINE float SyncMember::getDuration() const
{
	if (kind == KIND::PLAYER)
		return static_cast<const AnimNodeAnimationPlayer *>(node)->duration;

	return static_cast<const AnimNodeBlendSpace2DSync *>(node)->duration;
}

UNIGINE_INLINE float SyncMember::getCurrentTime() const
{
	if (kind == KIND::PLAYER)
		return static_cast<const AnimNodeAnimationPlayer *>(node)->current_time;

	return static_cast<const AnimNodeBlendSpace2DSync *>(node)->current_time;
}

UNIGINE_INLINE float SyncMember::getAccumulatedWeight() const { return node->accumulated_weight; }

UNIGINE_INLINE bool SyncMember::wasInactive() const
{
	if (kind == KIND::PLAYER)
		return static_cast<const AnimNodeAnimationPlayer *>(node)->was_inactive;

	return static_cast<const AnimNodeBlendSpace2DSync *>(node)->was_inactive;
}

UNIGINE_INLINE SyncMember::ROLE SyncMember::getRole() const
{
	if (kind == KIND::PLAYER)
		return static_cast<const AnimNodeAnimationPlayer *>(node)->syncker_role;

	return static_cast<const AnimNodeBlendSpace2DSync *>(node)->syncker_role;
}

UNIGINE_INLINE bool SyncMember::needsUpdate() const { return node->need_update; }

UNIGINE_INLINE bool SyncMember::isLoop() const
{
	if (kind == KIND::PLAYER)
		return static_cast<const AnimNodeAnimationPlayer *>(node)->is_loop;

	return true; // BS2DSync is always loop
}

UNIGINE_INLINE float SyncMember::getLeaderNextTime(float ifps) const
{
	if (kind == KIND::PLAYER)
	{
		auto *player = static_cast<AnimNodeAnimationPlayer *>(node);
		float next_time = 0.0f;
		player->getNextTime(ifps, next_time);
		return next_time;
	}

	// BS2DSync: time already advanced in updateLinks(), just read it
	return static_cast<const AnimNodeBlendSpace2DSync *>(node)->current_time;
}

UNIGINE_INLINE bool SyncMember::isTimeForward() const
{
	if (kind == KIND::PLAYER)
		return (0.0f <= static_cast<const AnimNodeAnimationPlayer *>(node)->speed);

	return (0.0f <= static_cast<const AnimNodeBlendSpace2DSync *>(node)->speed);
}

UNIGINE_INLINE void SyncMember::applyLeaderTime(float next_time, bool is_forward)
{
	if (kind == KIND::PLAYER)
	{
		static_cast<AnimNodeAnimationPlayer *>(node)->advanceTime(next_time, is_forward);
	}
	// BS2DSync: noop - time already set in updateLinks(), children already received time
}

UNIGINE_INLINE void SyncMember::applyFollowerTime(float normalized_time, bool is_forward)
{
	if (kind == KIND::PLAYER)
	{
		auto *player = static_cast<AnimNodeAnimationPlayer *>(node);
		player->advanceTime(normalized_time * player->duration, is_forward);
	} else
	{
		static_cast<AnimNodeBlendSpace2DSync *>(node)->applyNormalizedTime(normalized_time, is_forward);
	}
}

UNIGINE_INLINE void SyncMember::intersectMarkers(AnimGraphContext &context, Vector<unsigned int> &hashes, bool &first) const
{
	if (kind == KIND::PLAYER)
	{
		intersectLayerMarkers(context.skeleton_pose, node->pose_layer, hashes, first);
	} else
	{
		// BS2DSync: intersect ALL active children with non-zero weight
		auto *blend_space = static_cast<const AnimNodeBlendSpace2DSync *>(node);
		for (int i = 0; i < blend_space->links.size(); i++)
		{
			if (blend_space->links[i].node == nullptr || blend_space->smoothed_weights[i] < Consts::EPS)
				continue;
			if (intersectLayerMarkers(context.skeleton_pose, blend_space->links[i].node->pose_layer, hashes, first) == false)
				return;
		}
	}
}

UNIGINE_INLINE bool SyncMember::getMarkerSyncInterval(AnimGraphContext &context, float time, unsigned int &out_prev_hash, unsigned int &out_next_hash, float &out_ratio, const Vector<unsigned int> &hashes) const
{
	if (kind == KIND::PLAYER)
	{
		auto *player = static_cast<const AnimNodeAnimationPlayer *>(node);
		return context.skeleton_pose->getLayerMarkerSyncInterval(player->pose_layer, time, player->is_loop, out_prev_hash, out_next_hash, out_ratio, hashes);
	}

	auto *blend_space = static_cast<const AnimNodeBlendSpace2DSync *>(node);
	const int child_index = blend_space->getDominantChildIndex();
	if (child_index < 0)
		return false;

	const auto *child = blend_space->links[child_index].node;
	if (child->duration < Consts::EPS)
		return false;

	float normalized_time = (Consts::EPS < blend_space->duration) ? (time / blend_space->duration) : 0.0f;
	const float child_time = normalized_time * child->duration;
	return context.skeleton_pose->getLayerMarkerSyncInterval(child->pose_layer, child_time, true, out_prev_hash, out_next_hash, out_ratio, hashes);
}

UNIGINE_INLINE void SyncMember::applyMarkerTime(AnimGraphContext &context, unsigned int prev_hash, unsigned int next_hash, float ratio, const Vector<unsigned int> &hashes, bool is_forward, float fallback_normalized_time)
{
	if (kind == KIND::PLAYER)
	{
		auto *player = static_cast<AnimNodeAnimationPlayer *>(node);
		const float new_time = context.skeleton_pose->getLayerTimeFromMarkerSyncInterval(player->pose_layer, prev_hash, next_hash, ratio, player->current_time, player->is_loop, hashes);
		player->advanceTime(new_time, is_forward);
	} else
	{
		auto *blend_space = static_cast<AnimNodeBlendSpace2DSync *>(node);
		const int child_index = blend_space->getDominantChildIndex();
		auto *child = (0 <= child_index) ? blend_space->links[child_index].node : nullptr;
		if (child != nullptr && Consts::EPS < child->duration)
		{
			const float child_time = context.skeleton_pose->getLayerTimeFromMarkerSyncInterval(child->pose_layer, prev_hash, next_hash, ratio, blend_space->normalized_time * child->duration, true, hashes);
			blend_space->applyNormalizedTime(child_time / child->duration, is_forward);
		} else
		{
			blend_space->applyNormalizedTime(fallback_normalized_time, is_forward);
		}
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
// AnimSyncker inline implementations
///////////////////////////////////////////////////////////////////////////////////////////////////////
UNIGINE_INLINE bool AnimSyncker::hasOtherActiveMembers(const AnimNodeBase *exclude) const
{
	for (const auto &member : members)
	{
		if (member.node != exclude && member.wasInactive() == false)
			return true;
	}
	return false;
}

UNIGINE_INLINE void AnimSyncker::rebuildCommonHashes(AnimGraphContext &context, bool before_update)
{
	common_hashes.clear();
	bool first = true;
	for (const auto &member : members)
	{
		if (before_update ? member.wasInactive() : member.needsUpdate())
			continue;

		member.intersectMarkers(context, common_hashes, first);
		if (common_hashes.empty() && first == false)
			return;
	}
}

UNIGINE_INLINE bool AnimSyncker::syncTimeByMarkers(AnimGraphContext &context, AnimNodeBase *target_node, bool before_update)
{
	if (sync_method != SYNC_METHOD::MARKER_SYNC || common_hashes.size() < 2)
		return false;

	SyncMember *target = nullptr;
	for (auto &member : members)
	{
		if (member.node == target_node)
		{
			target = &member;
			break;
		}
	}
	if (target == nullptr)
		return false;

	for (auto &member : members)
	{
		if ((before_update ? member.wasInactive() : member.needsUpdate()) || member.node == target_node)
			continue;

		unsigned int prev_hash = 0, next_hash = 0;
		float ratio = 0.0f;
		if (member.getMarkerSyncInterval(context, member.getCurrentTime(), prev_hash, next_hash, ratio, common_hashes))
		{
			target->applyMarkerTime(context, prev_hash, next_hash, ratio, common_hashes, true, normalized_time);
			return true;
		}
	}
	return false;
}

UNIGINE_INLINE void AnimSyncker::update(AnimGraphContext &context)
{
	// 1. Find leader
	int new_leader_index = -1;
	float max_weight = -1.0f;
	for (int i = 0; i < members.size(); i++)
	{
		auto &member = members[i];
		if (member.needsUpdate())
			continue;

		if (member.getRole() == ROLE::LEADER)
		{
			new_leader_index = i;
			break;
		}

		if (member.getRole() == ROLE::WEIGHTED_LEADER_AND_FOLLOWER && max_weight < member.getAccumulatedWeight())
		{
			max_weight = member.getAccumulatedWeight();
			new_leader_index = i;
		}
	}

	if (new_leader_index == -1)
	{
		is_valid_leader = false;
		max_weight = -1.0f;
		for (int i = 0; i < members.size(); i++)
		{
			auto &member = members[i];
			if (member.needsUpdate() || member.getRole() != ROLE::FOLLOWER)
				continue;

			if (max_weight < member.getAccumulatedWeight())
			{
				max_weight = member.getAccumulatedWeight();
				new_leader_index = i;
			}
		}
	} else
	{
		// Preventing time lag after a valid leader has reappeared or changed
		if (is_valid_leader == false || leader_index != new_leader_index)
		{
			rebuildCommonHashes(context);
			if (syncTimeByMarkers(context, members[new_leader_index].node) == false)
				members[new_leader_index].applyFollowerTime(normalized_time, true);
		}
		is_valid_leader = true;
	}

	leader_index = new_leader_index;
	if (leader_index == -1)
		return;
	if (members[leader_index].getDuration() < Consts::EPS)
		return;

	auto &leader = members[leader_index];

	// 2. Get next time from leader
	float next_time = leader.getLeaderNextTime(context.ifps);
	bool leader_forward = leader.isTimeForward();
	normalized_time = next_time / leader.getDuration();

	// 3. Marker sync
	bool use_markers = false;
	unsigned int prev_hash = 0, next_hash = 0;
	float ratio = 0.0f;
	if (sync_method == SYNC_METHOD::MARKER_SYNC)
	{
		rebuildCommonHashes(context);
		if (2 <= common_hashes.size())
		{
			use_markers = leader.getMarkerSyncInterval(context, next_time, prev_hash, next_hash, ratio, common_hashes);
			if (use_markers == false)
			{
				for (int i = 0; i < members.size(); i++)
				{
					if (i == leader_index || members[i].needsUpdate())
						continue;

					if (members[i].getMarkerSyncInterval(context, members[i].getCurrentTime(), prev_hash, next_hash, ratio, common_hashes))
					{
						use_markers = true;
						break;
					}
				}
			}
		}
	}

	// 4. Apply to all members
	for (int i = 0; i < members.size(); i++)
	{
		auto &member = members[i];
		if (member.needsUpdate())
			continue;

		if (i == leader_index)
			member.applyLeaderTime(next_time, leader_forward);
		else if (use_markers)
			member.applyMarkerTime(context, prev_hash, next_hash, ratio, common_hashes, leader_forward, normalized_time);
		else
			member.applyFollowerTime(normalized_time, leader_forward);
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node State
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeState: AnimNodeBase
{
	struct Transition
	{
		Transition() = default;
		Transition(const Transition &) = delete;
		Transition &operator=(const Transition &) = delete;

		Transition(Transition &&other) noexcept
			: runtime_guid(other.runtime_guid), name(std::move(other.name)), to_state(other.to_state),
			duration(other.duration), can_be_interrupted(other.can_be_interrupted),
			condition_callback(other.condition_callback), has_enter_time(other.has_enter_time),
			enter_time_callback(other.enter_time_callback),
			child_transitions(std::move(other.child_transitions))
		{
			other.to_state = nullptr;
			other.condition_callback = nullptr;
			other.enter_time_callback = nullptr;
		}

		Transition &operator=(Transition &&other) noexcept
		{
			if (this != &other)
			{
				delete condition_callback;
				delete enter_time_callback;

				to_state = other.to_state;
				duration = other.duration;
				can_be_interrupted = other.can_be_interrupted;
				condition_callback = other.condition_callback;
				runtime_guid = other.runtime_guid;
				name = std::move(other.name);
				has_enter_time = other.has_enter_time;
				enter_time_callback = other.enter_time_callback;
				child_transitions = std::move(other.child_transitions);

				other.to_state = nullptr;
				other.condition_callback = nullptr;
				other.enter_time_callback = nullptr;
			}
			return *this;
		}

		~Transition()
		{
			delete condition_callback;
			delete enter_time_callback;
		}

		UGUID runtime_guid;
		String name; // condition node label
		AnimNodeState *to_state{nullptr};
		float duration{0.2f};
		bool can_be_interrupted{false};

		using CallbackType = CallbackBase2<AnimGraphContext const &, bool &>;
		CallbackType *condition_callback{nullptr};

		bool has_enter_time{false};
		using EnterTimeCallbackType = CallbackBase2<AnimGraphContext const &, float &>;
		EnterTimeCallbackType *enter_time_callback{nullptr};

		// non-owning pointers, lifetime managed by graph class
		Vector<Transition *> child_transitions;

		// Condition nodes form a routing tree via child_transitions; out_resolved is the leaf that fired.
		// out_path collects passed conditions leaf-first, only while unwinding a successful chain.
		AnimNodeState *resolveTransition(const AnimGraphContext &context, const Transition **out_resolved = nullptr, Vector<const Transition *> *out_path = nullptr) const
		{
			// No callback = corrupted data, block transition
			if (condition_callback == nullptr)
				return nullptr;

			bool out_result = false;
			condition_callback->run(context, out_result);
			if (out_result == false)
				return nullptr;

			if (child_transitions.empty() == false)
			{
				for (auto *child : child_transitions)
				{
					AnimNodeState *result = child->resolveTransition(context, out_resolved, out_path);
					if (result)
					{
						if (out_path)
							out_path->append(this);
						return result;
					}
				}
				return nullptr;
			}

			if (out_resolved)
				*out_resolved = this;
			if (out_path)
				out_path->append(this);
			return to_state;
		}

		float computeEnterTime(const AnimGraphContext &context) const
		{
			if (has_enter_time == false || enter_time_callback == nullptr)
				return -1.0f;

			float out_time = 0.0f;
			enter_time_callback->run(context, out_time);
			return saturate(out_time);
		}
	};

	AnimNodeState(): AnimNodeBase(TYPE::STATE) {}

	// read / write props
	String state_name;

	// internal
	PoseLink link_pose;
	Vector<Transition *> transitions;

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		link_pose.updateOutputPose(context, accumulated_weight);
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;

		if (link_pose.node == nullptr)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->resetLayerToBindPose(pose_layer);
			return;
		}

		link_pose.evaluateOutputPose(context);
		pose_layer = link_pose.getOutputPose(context);

		if (context.isActiveRootMotion())
			root_motion_delta = link_pose.getRootMotionDelta();
	}

	void onEnter(AnimGraphContext &context, float enter_normalized_time = -1.0f)
	{
		resetPlaybackNodes(link_pose.node, context);
		if (0 <= enter_normalized_time)
			applyNormalizedTime(link_pose.node, context, enter_normalized_time);
	}

	void onExit(AnimGraphContext & /*context*/) {}

	static void resetPlaybackNodes(AnimNodeBase *node, AnimGraphContext &context);

	static void applyNormalizedTime(AnimNodeBase *node, AnimGraphContext &context, float normalized_time);
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node State Machine
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeStateMachine: AnimNodeBase
{
	using Transition = AnimNodeState::Transition;

	AnimNodeStateMachine(): AnimNodeBase(TYPE::STATE_MACHINE) {}

	static const AnimNodeAnimationPlayer *findRelevantPlayer(const AnimNodeBase *node)
	{
		if (node == nullptr)
			return nullptr;

		switch (node->node_type)
		{
			case TYPE::ANIMATION_PLAYER:
			{
				return static_cast<const AnimNodeAnimationPlayer *>(node);
			}
			case TYPE::STATE:
			{
				return findRelevantPlayer(static_cast<const AnimNodeState *>(node)->link_pose.node);
			}
			case TYPE::BLEND:
			{
				auto *blend = static_cast<const AnimNodeBlend *>(node);
				const AnimNodeBase *primary = (blend->weight < 0.5f) ? blend->link_0.node : blend->link_1.node;
				const AnimNodeBase *secondary = (blend->weight < 0.5f) ? blend->link_1.node : blend->link_0.node;
				auto *result = findRelevantPlayer(primary);
				return result ? result : findRelevantPlayer(secondary);
			}
			case TYPE::APPLY_ADDITIVE:
			{
				return findRelevantPlayer(static_cast<const AnimNodeApplyAdditive *>(node)->link_base_pose.node);
			}
			case TYPE::MAKE_ADDITIVE:
			{
				return findRelevantPlayer(static_cast<const AnimNodeMakeAdditive *>(node)->link_base_pose.node);
			}
			case TYPE::BRANCH:
			{
				auto *branch = static_cast<const AnimNodeBranch *>(node);
				return findRelevantPlayer(branch->condition ? branch->link_true.node : branch->link_false.node);
			}
			case TYPE::BLEND_SPACE_2D:
			{
				auto *bs = static_cast<const AnimNodeBlendSpace2D *>(node);
				const AnimNodeAnimationPlayer *best = nullptr;
				float best_w = -1.0f;
				for (int i = 0; i < bs->links.size(); i++)
				{
					if (best_w < bs->smoothed_weights[i])
					{
						auto *p = findRelevantPlayer(bs->links[i].node);
						if (p != nullptr)
						{
							best = p;
							best_w = bs->smoothed_weights[i];
						}
					}
				}
				return best;
			}
			case TYPE::STATE_MACHINE:
			{
				auto *sm = static_cast<const AnimNodeStateMachine *>(node);
				if (sm->current_state != nullptr)
					return findRelevantPlayer(sm->current_state);

				return nullptr;
			}
			case TYPE::GET_JOINT_TRANSFORM:
			{
				return findRelevantPlayer(static_cast<const AnimNodeGetJointTransform *>(node)->link_pose.node);
			}
			case TYPE::SET_JOINT_TRANSFORM:
			{
				return findRelevantPlayer(static_cast<const AnimNodeSetJointTransform *>(node)->link_pose.node);
			}
			case TYPE::CONVERT_SPACE:
			{
				return findRelevantPlayer(static_cast<const AnimNodeConvertSpaceBase *>(node)->link_pose.node);
			}
			case TYPE::RUNTIME_MATH:
			{
				return nullptr; // runtime math has no pose input
			}
			case TYPE::TWO_BONE_IK:
			{
				return findRelevantPlayer(static_cast<const AnimNodeTwoBoneIK *>(node)->link_pose.node);
			}
			case TYPE::LOOK_AT:
			{
				return findRelevantPlayer(static_cast<const AnimNodeLookAt *>(node)->link_pose.node);
			}
			case TYPE::JOINT_HINGE_LIMIT:
			{
				return findRelevantPlayer(static_cast<const AnimNodeJointHingeLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_CONE_LIMIT:
			{
				return findRelevantPlayer(static_cast<const AnimNodeJointConeLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_CONE_ASYM_LIMIT:
			{
				return findRelevantPlayer(static_cast<const AnimNodeJointConeAsymLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_TWIST_LIMIT:
			{
				return findRelevantPlayer(static_cast<const AnimNodeJointTwistLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_HINGE_TWIST_LIMIT:
			{
				return findRelevantPlayer(static_cast<const AnimNodeJointHingeTwistLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_CONE_TWIST_LIMIT:
			{
				return findRelevantPlayer(static_cast<const AnimNodeJointConeTwistLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_CONE_ASYM_TWIST_LIMIT:
			{
				return findRelevantPlayer(static_cast<const AnimNodeJointConeAsymTwistLimit *>(node)->link_pose.node);
			}
			case TYPE::IK_CHAIN:
			{
				return findRelevantPlayer(static_cast<const AnimNodeIKChain *>(node)->link_pose.node);
			}
			case TYPE::JOINT_LIMIT_SET:
			{
				// no pose input
				return nullptr;
			}
			case TYPE::LOOK_AT_CHAIN:
			{
				return findRelevantPlayer(static_cast<const AnimNodeLookAtChain *>(node)->link_pose.node);
			}
			default:
			{
				return nullptr;
			}
		}
	}

	struct TimeSourceInfo
	{
		float duration{0.0f};
		float current_time{0.0f};
	};

	static TimeSourceInfo findRelevantTimeSource(const AnimNodeBase *node)
	{
		if (node == nullptr)
			return {};

		switch (node->node_type)
		{
			case TYPE::ANIMATION_PLAYER:
			{
				auto *p = static_cast<const AnimNodeAnimationPlayer *>(node);
				TimeSourceInfo time_source;
				time_source.duration = p->duration;
				time_source.current_time = p->current_time;
				return time_source;
			}
			case TYPE::BLEND_SPACE_2D_SYNC:
			{
				auto *bs = static_cast<const AnimNodeBlendSpace2DSync *>(node);
				TimeSourceInfo time_source;
				time_source.duration = bs->duration;
				time_source.current_time = bs->current_time;
				return time_source;
			}
			case TYPE::STATE:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeState *>(node)->link_pose.node);
			}
			case TYPE::BLEND:
			{
				auto *blend = static_cast<const AnimNodeBlend *>(node);
				const AnimNodeBase *primary = (blend->weight < 0.5f) ? blend->link_0.node : blend->link_1.node;
				const AnimNodeBase *secondary = (blend->weight < 0.5f) ? blend->link_1.node : blend->link_0.node;
				auto time_source = findRelevantTimeSource(primary);
				return (Consts::EPS < time_source.duration) ? time_source : findRelevantTimeSource(secondary);
			}
			case TYPE::APPLY_ADDITIVE:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeApplyAdditive *>(node)->link_base_pose.node);
			}
			case TYPE::MAKE_ADDITIVE:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeMakeAdditive *>(node)->link_base_pose.node);
			}
			case TYPE::BRANCH:
			{
				auto *branch = static_cast<const AnimNodeBranch *>(node);
				return findRelevantTimeSource(branch->condition ? branch->link_true.node : branch->link_false.node);
			}
			case TYPE::BLEND_SPACE_2D:
			{
				auto *bs = static_cast<const AnimNodeBlendSpace2D *>(node);
				TimeSourceInfo best = {};
				float best_w = -1.0f;
				for (int i = 0; i < bs->links.size(); i++)
				{
					if (best_w < bs->smoothed_weights[i])
					{
						auto time_source = findRelevantTimeSource(bs->links[i].node);
						if (Consts::EPS < time_source.duration)
						{
							best = time_source;
							best_w = bs->smoothed_weights[i];
						}
					}
				}
				return best;
			}
			case TYPE::STATE_MACHINE:
			{
				auto *sm = static_cast<const AnimNodeStateMachine *>(node);
				if (sm->current_state != nullptr)
					return findRelevantTimeSource(sm->current_state);

				return {};
			}
			case TYPE::GET_JOINT_TRANSFORM:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeGetJointTransform *>(node)->link_pose.node);
			}
			case TYPE::SET_JOINT_TRANSFORM:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeSetJointTransform *>(node)->link_pose.node);
			}
			case TYPE::CONVERT_SPACE:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeConvertSpaceBase *>(node)->link_pose.node);
			}
			case TYPE::RUNTIME_MATH:
			{
				return {}; // runtime math has no pose input
			}
			case TYPE::TWO_BONE_IK:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeTwoBoneIK *>(node)->link_pose.node);
			}
			case TYPE::LOOK_AT:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeLookAt *>(node)->link_pose.node);
			}
			case TYPE::JOINT_HINGE_LIMIT:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeJointHingeLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_CONE_LIMIT:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeJointConeLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_CONE_ASYM_LIMIT:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeJointConeAsymLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_TWIST_LIMIT:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeJointTwistLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_HINGE_TWIST_LIMIT:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeJointHingeTwistLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_CONE_TWIST_LIMIT:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeJointConeTwistLimit *>(node)->link_pose.node);
			}
			case TYPE::JOINT_CONE_ASYM_TWIST_LIMIT:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeJointConeAsymTwistLimit *>(node)->link_pose.node);
			}
			case TYPE::IK_CHAIN:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeIKChain *>(node)->link_pose.node);
			}
			case TYPE::JOINT_LIMIT_SET:
			{
				// no pose input
				return {};
			}
			case TYPE::LOOK_AT_CHAIN:
			{
				return findRelevantTimeSource(static_cast<const AnimNodeLookAtChain *>(node)->link_pose.node);
			}
			default:
			{
				return {};
			}
		}
	}

	// read / write props
	// Full path ("MainSM/Combat/AttackSM"): plain names collide across nested machines and subgraph copies.
	String sm_path;
	Vector<AnimNodeState *> states; // non-owning
	AnimNodeStateMachine *parent_sm{nullptr};
	String owner_state_name;

	// internal
	AnimNodeState::Transition entry_transition;
	AnimNodeState *current_state{nullptr};
	AnimNodeState *previous_state{nullptr};
	bool is_blending_previous{false};
	const AnimNodeState::Transition *active_transition{nullptr};
	float transition_time{0.0f};
	float transition_duration{0.0f};
	float transition_progress{1.0f};

	float current_state_time{0.0f};

	// Snapshot for the public API: context.sm is a stack, popped at the end of update().
	float anim_time{0.0f};
	float anim_time_fraction{0.0f};
	float anim_length{0.0f};
	float anim_time_remaining{0.0f};
	float anim_time_remaining_fraction{0.0f};
	bool anim_ended{false};

	// Last fired chain, outermost first. Kept after the transition ends — it is noticed later.
	Vector<const Transition *> active_transition_path;
	Vector<const Transition *> resolved_path; // scratch, moved by takeTransitionPath()

	bool entry_resolve_failed{false};

	// interruption info
	int frozen_pose_layer{-1};
	bool has_frozen_pose{false};
	Transform frozen_root_motion_delta;
	bool pending_interruption{false};
	AnimNodeState *pending_target{nullptr};
	const AnimNodeState::Transition *pending_transition{nullptr};

	// Stood up for a state requested from code: never resolved, nameless so the chain reports nothing.
	AnimNodeState::Transition forced_transition;
	float forced_enter_time{-1.0f};
	bool forced_request{false};

	void grabConstPoseLayer(AnimGraphContext &context) override
	{
		frozen_pose_layer = context.skeleton_pose->addLayer();
	}

	// Deferred on purpose: mutating the machine while it walks its transitions races the threaded update.
	// duration 0 swaps the pose outright, above 0 blends out of a frozen snapshot.
	void requestState(int state_index, float duration, float enter_normalized_time)
	{
		if (state_index < 0 || states.size() <= state_index)
			return;

		forced_transition.to_state = states[state_index];
		forced_transition.duration = max(0.0f, duration);
		forced_enter_time = enter_normalized_time;

		resolved_path.clear(); // a chain resolved earlier this frame is not ours

		// Not pending_interruption yet: evaluate() must snapshot the pose first, or the blend would
		// start from an unwritten layer — a bind pose.
		forced_request = true;
		pending_target = states[state_index];
		pending_transition = &forced_transition;
	}

	// A request survives a reset, so it has to be dropped once consumed.
	void clearForcedRequest()
	{
		forced_request = false;
		forced_transition.to_state = nullptr;
		forced_enter_time = -1.0f;
	}

	// Leaf-first collection order flipped into the order the artist reads. Nameless entries are
	// synthetic Entry -> State links and are dropped.
	void takeTransitionPath()
	{
		active_transition_path.clear();
		for (int i = resolved_path.size() - 1; 0 <= i; i--)
		{
			if (resolved_path[i]->name.empty() == false)
				active_transition_path.append(resolved_path[i]);
		}
		resolved_path.clear();
	}

	void update(AnimGraphContext &context) override
	{
		if (takeAndClearNeedUpdate() == false)
			return;

		// entry point
		if (current_state == nullptr)
		{
			// A code request replaces the entry resolve: nothing to blend from, so a caller can aim at a
			// nested machine that is still asleep.
			if (forced_request && forced_transition.to_state != nullptr)
			{
				current_state = forced_transition.to_state;
				entry_resolve_failed = false;
				active_transition_path.clear(); // named by code, nothing fired
				resolved_path.clear();
				current_state->onEnter(context, forced_enter_time);
				current_state_time = 0.0f;
				clearForcedRequest();
			} else
			{
				const Transition *resolved_tr = nullptr;
				resolved_path.clear();
				current_state = entry_transition.resolveTransition(context, &resolved_tr, &resolved_path);
				if (current_state == nullptr)
				{
					if (entry_resolve_failed == false)
					{
						Log::warning("AnimNodeStateMachine::update(): entry_transition resolved to nullptr (no valid entry state)\n");
						entry_resolve_failed = true;
					}
					return;
				}

				entry_resolve_failed = false;
				takeTransitionPath();
				current_state->onEnter(context, resolved_tr->computeEnterTime(context));
				current_state_time = 0.0f;
			}
		}

		// apply deferred interruption from previous frame
		if (pending_interruption)
		{
			has_frozen_pose = true;
			is_blending_previous = false;

			current_state->onExit(context);
			// Unused for posing (the blend runs off the snapshot), but the API reports it as "where we came from".
			previous_state = current_state;
			current_state = pending_target;
			active_transition = pending_transition;
			takeTransitionPath();
			transition_time = 0.0f;
			transition_duration = pending_transition->duration;
			const float enter_time = (pending_transition == &forced_transition) ? forced_enter_time : pending_transition->computeEnterTime(context);
			current_state->onEnter(context, enter_time);
			current_state_time = 0.0f;

			if (pending_transition == &forced_transition)
				clearForcedRequest();

			pending_interruption = false;
			pending_target = nullptr;
			pending_transition = nullptr;
		}

		// state machine context
		context.pushSMContext();

		current_state_time += context.ifps;
		context.sm.state_time = current_state_time;

		context.sm.anim_ended = false;
		const auto *relevant_player = findRelevantPlayer(current_state);
		if (relevant_player != nullptr)
			context.sm.anim_ended = ((relevant_player->is_loop == false) && (Consts::EPS < relevant_player->duration) && ((relevant_player->duration - Consts::EPS) <= relevant_player->current_time));

		context.sm.anim_time_remaining = 0.0f;
		context.sm.anim_time_remaining_fraction = 0.0f;
		context.sm.anim_length = 0.0f;
		context.sm.anim_time = 0.0f;
		context.sm.anim_time_fraction = 0.0f;
		const auto time_source = findRelevantTimeSource(current_state);
		if (Consts::EPS < time_source.duration)
		{
			context.sm.anim_time_remaining = max(0.0f, time_source.duration - time_source.current_time);
			context.sm.anim_time_remaining_fraction = context.sm.anim_time_remaining / time_source.duration;
			context.sm.anim_length = time_source.duration;
			context.sm.anim_time = time_source.current_time;
			context.sm.anim_time_fraction = time_source.current_time / time_source.duration;
		}

		anim_ended = context.sm.anim_ended;
		anim_time_remaining = context.sm.anim_time_remaining;
		anim_time_remaining_fraction = context.sm.anim_time_remaining_fraction;
		anim_length = context.sm.anim_length;
		anim_time = context.sm.anim_time;
		anim_time_fraction = context.sm.anim_time_fraction;

		const bool can_interrupt = ((active_transition != nullptr) && active_transition->can_be_interrupted);
		const bool is_in_transition = (is_blending_previous || has_frozen_pose);

		// A code request owns this frame, or the graph enters a state first and the request re-enters it.
		if (forced_request == false && is_in_transition && can_interrupt)
		{
			resolved_path.clear();
			for (const auto t : current_state->transitions)
			{
				const Transition *resolved_tr = nullptr;
				AnimNodeState *resolved = t->resolveTransition(context, &resolved_tr, &resolved_path);
				if (resolved != nullptr)
				{
					// chain stays in resolved_path until the interruption is applied next frame
					pending_interruption = true;
					pending_target = resolved;
					pending_transition = resolved_tr;
					break;
				}
			}
		} else if (forced_request == false && is_in_transition == false)
		{
			resolved_path.clear();
			for (const auto t : current_state->transitions)
			{
				const Transition *resolved_tr = nullptr;
				AnimNodeState *resolved = t->resolveTransition(context, &resolved_tr, &resolved_path);
				if (resolved)
				{
					previous_state = current_state;
					is_blending_previous = true;
					current_state = resolved;
					active_transition = resolved_tr;
					takeTransitionPath();
					transition_time = 0.0f;
					transition_duration = resolved_tr->duration;
					previous_state->onExit(context);
					const float enter_time = resolved_tr->computeEnterTime(context);
					current_state->onEnter(context, enter_time);
					current_state_time = 0.0f;
					break;
				}
			}
		}

		// BEFORE state updates: stale progress inverts accumulated_weight → wrong leader → root motion jump
		const bool is_blending = (is_blending_previous || has_frozen_pose);
		if (is_blending)
		{
			transition_time += context.ifps;
			if (transition_duration <= transition_time)
			{
				is_blending_previous = false;
				active_transition = nullptr;
				has_frozen_pose = false;
				transition_progress = 1.0f;
			} else
			{
				if (Consts::EPS < transition_duration)
					transition_progress = saturate(transition_time / transition_duration);
				else
					transition_progress = 1.0f;
			}
		} else
		{
			transition_progress = 1.0f;
		}

		current_state->accumulated_weight = max(current_state->accumulated_weight, accumulated_weight * transition_progress);
		current_state->update(context);

		if (is_blending_previous)
		{
			previous_state->accumulated_weight = max(previous_state->accumulated_weight, accumulated_weight * (1.0f - transition_progress));
			previous_state->update(context);
		}

		context.popSMContext();
	}

	void evaluate(AnimGraphContext &context) override
	{
		if (takeAndClearNeedEvaluate() == false)
			return;
		if (current_state == nullptr)
			return;

		current_state->evaluate(context);

		if (has_frozen_pose)
		{
			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->lerpLayer(pose_layer, frozen_pose_layer, current_state->getSafePoseLayer(context), transition_progress);

			if (pending_interruption)
			{
				context.skeleton_pose->copyLayer(frozen_pose_layer, pose_layer);
				if (context.isActiveRootMotion())
					frozen_root_motion_delta = blendTransform(frozen_root_motion_delta, current_state->root_motion_delta, transition_progress);
			}

			if (context.isActiveRootMotion())
				root_motion_delta = blendTransform(frozen_root_motion_delta, current_state->root_motion_delta, transition_progress);
		} else if (is_blending_previous)
		{
			previous_state->evaluate(context);

			pose_layer = context.getTmpPoseLayer();
			context.skeleton_pose->lerpLayer(pose_layer, previous_state->getSafePoseLayer(context), current_state->getSafePoseLayer(context), transition_progress);

			if (context.isActiveRootMotion())
				root_motion_delta = blendTransform(previous_state->root_motion_delta, current_state->root_motion_delta, transition_progress);

			if (pending_interruption)
			{
				context.skeleton_pose->copyLayer(frozen_pose_layer, pose_layer);
				if (context.isActiveRootMotion())
					frozen_root_motion_delta = root_motion_delta;
			}
		} else
		{
			pose_layer = current_state->getSafePoseLayer(context);

			if (context.isActiveRootMotion())
				root_motion_delta = current_state->root_motion_delta;
		}

		// Freeze what a code request blends away from — the only moment the pose layer is still live
		// (update() runs after the temporary layers are recycled).
		if (forced_request)
		{
			context.skeleton_pose->copyLayer(frozen_pose_layer, pose_layer);
			if (context.isActiveRootMotion())
				frozen_root_motion_delta = root_motion_delta;
			forced_request = false;
			pending_interruption = true;
		}
	}
};

// Needs the complete AnimNodeStateMachine type
UNIGINE_INLINE void AnimNodeState::resetPlaybackNodes(AnimNodeBase *node, AnimGraphContext &context)
{
	if (node == nullptr)
		return;

	switch (node->node_type)
	{
		case TYPE::ANIMATION_PLAYER:
		{
			auto *p = static_cast<AnimNodeAnimationPlayer *>(node);
			p->was_inactive = false;
			p->root_motion_delta = transform_identity;
			p->syncToGroup(context, true);
			p->teleportTime(p->current_time); // no delta on state entry
			break;
		}
		case TYPE::ANIMATION_PLAYER_SYNC:
		{
			auto *p = static_cast<AnimNodeAnimationPlayerSync *>(node);
			p->teleportTime(0.0f); // no delta on state entry
			p->was_inactive = false;
			p->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::BLEND:
		{
			auto *b = static_cast<AnimNodeBlend *>(node);
			resetPlaybackNodes(b->link_0.node, context);
			resetPlaybackNodes(b->link_1.node, context);
			b->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::BLEND_SPACE_2D:
		{
			auto *bs = static_cast<AnimNodeBlendSpace2D *>(node);
			bs->was_inactive = false;
			bs->snapWeights();
			for (int i = 0; i < bs->links.size(); i++)
				resetPlaybackNodes(bs->links[i].node, context);
			bs->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::BLEND_SPACE_2D_SYNC:
		{
			auto *bs = static_cast<AnimNodeBlendSpace2DSync *>(node);
			bs->was_inactive = false;
			bs->snapWeights();
			bs->syncToGroup(context, true);
			for (int i = 0; i < bs->links.size(); i++)
				resetPlaybackNodes(bs->links[i].node, context);
			if (Consts::EPS < bs->normalized_time)
				bs->syncChildrenTime();
			bs->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::BRANCH:
		{
			auto *br = static_cast<AnimNodeBranch *>(node);
			resetPlaybackNodes(br->link_true.node, context);
			resetPlaybackNodes(br->link_false.node, context);
			br->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::MAKE_ADDITIVE:
		{
			auto *m = static_cast<AnimNodeMakeAdditive *>(node);
			resetPlaybackNodes(m->link_base_pose.node, context);
			resetPlaybackNodes(m->link_ref_pose.node, context);
			m->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::APPLY_ADDITIVE:
		{
			auto *a = static_cast<AnimNodeApplyAdditive *>(node);
			resetPlaybackNodes(a->link_base_pose.node, context);
			resetPlaybackNodes(a->link_additive.node, context);
			a->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::STATE:
		{
			auto *s = static_cast<AnimNodeState *>(node);
			resetPlaybackNodes(s->link_pose.node, context);
			s->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::STATE_MACHINE:
		{
			auto *sm = static_cast<AnimNodeStateMachine *>(node);
			sm->current_state = nullptr;
			// stale index would alias a released tmp layer if entry fails to resolve next frame
			sm->pose_layer = -1;
			sm->previous_state = nullptr;
			sm->is_blending_previous = false;
			sm->active_transition = nullptr;
			sm->transition_time = 0.0f;
			sm->transition_duration = 0.0f;
			sm->transition_progress = 1.0f;
			sm->current_state_time = 0.0f;
			sm->has_frozen_pose = false;
			sm->frozen_root_motion_delta = transform_identity;
			sm->pending_interruption = false;
			sm->pending_target = nullptr;
			sm->pending_transition = nullptr;
			sm->entry_resolve_failed = false;
			sm->root_motion_delta = transform_identity;
			sm->anim_time = 0.0f;
			sm->anim_time_fraction = 0.0f;
			sm->anim_length = 0.0f;
			sm->anim_time_remaining = 0.0f;
			sm->anim_time_remaining_fraction = 0.0f;
			sm->anim_ended = false;
			sm->active_transition_path.clear();
			sm->resolved_path.clear();
			// forced_request survives: the reset is exactly the moment the machine wakes up.
			break;
		}
		case TYPE::GET_JOINT_TRANSFORM:
		{
			auto *g = static_cast<AnimNodeGetJointTransform *>(node);
			resetPlaybackNodes(g->link_pose.node, context);
			g->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::SET_JOINT_TRANSFORM:
		{
			auto *s = static_cast<AnimNodeSetJointTransform *>(node);
			resetPlaybackNodes(s->link_pose.node, context);
			s->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::CONVERT_SPACE:
		{
			auto *c = static_cast<AnimNodeConvertSpaceBase *>(node);
			resetPlaybackNodes(c->link_pose.node, context);
			c->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::RUNTIME_MATH:
		{
			// no pose input, no playback state
			node->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::TWO_BONE_IK:
		{
			auto *ik = static_cast<AnimNodeTwoBoneIK *>(node);
			resetPlaybackNodes(ik->link_pose.node, context);
			ik->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::LOOK_AT:
		{
			auto *la = static_cast<AnimNodeLookAt *>(node);
			resetPlaybackNodes(la->link_pose.node, context);
			la->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::JOINT_HINGE_LIMIT:
		{
			auto *jh = static_cast<AnimNodeJointHingeLimit *>(node);
			resetPlaybackNodes(jh->link_pose.node, context);
			jh->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::JOINT_CONE_LIMIT:
		{
			auto *jc = static_cast<AnimNodeJointConeLimit *>(node);
			resetPlaybackNodes(jc->link_pose.node, context);
			jc->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::JOINT_CONE_ASYM_LIMIT:
		{
			auto *jca = static_cast<AnimNodeJointConeAsymLimit *>(node);
			resetPlaybackNodes(jca->link_pose.node, context);
			jca->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::JOINT_TWIST_LIMIT:
		{
			auto *jt = static_cast<AnimNodeJointTwistLimit *>(node);
			resetPlaybackNodes(jt->link_pose.node, context);
			jt->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::JOINT_HINGE_TWIST_LIMIT:
		{
			auto *jht = static_cast<AnimNodeJointHingeTwistLimit *>(node);
			resetPlaybackNodes(jht->link_pose.node, context);
			jht->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::JOINT_CONE_TWIST_LIMIT:
		{
			auto *jct = static_cast<AnimNodeJointConeTwistLimit *>(node);
			resetPlaybackNodes(jct->link_pose.node, context);
			jct->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::JOINT_CONE_ASYM_TWIST_LIMIT:
		{
			auto *jcat = static_cast<AnimNodeJointConeAsymTwistLimit *>(node);
			resetPlaybackNodes(jcat->link_pose.node, context);
			jcat->root_motion_delta = transform_identity;
			break;
		}
		case TYPE::IK_CHAIN:
		{
			auto *ikc = static_cast<AnimNodeIKChain *>(node);
			resetPlaybackNodes(ikc->link_pose.node, context);
			ikc->root_motion_delta = transform_identity;
			// Fresh start: the filter reseed is gated on output_filtered_rotations.size(), so clear it too.
			ikc->warm_start_valid = false;
			ikc->output_filtered_rotations.clear();
			break;
		}
		case TYPE::JOINT_LIMIT_SET:
		{
			// nothing to reset
			break;
		}
		case TYPE::LOOK_AT_CHAIN:
		{
			auto *lac = static_cast<AnimNodeLookAtChain *>(node);
			resetPlaybackNodes(lac->link_pose.node, context);
			lac->root_motion_delta = transform_identity;
			break;
		}
		default:
			break;
	}
}

UNIGINE_INLINE void AnimNodeState::applyNormalizedTime(AnimNodeBase *node, AnimGraphContext &context, float normalized_time)
{
	if (node == nullptr)
		return;

	switch (node->node_type)
	{
		case TYPE::ANIMATION_PLAYER:
		{
			auto *p = static_cast<AnimNodeAnimationPlayer *>(node);
			// Sync group has authority over enter_time - skip override when group is active
			if (p->syncker_index != -1 && context.synckers[p->syncker_index].hasOtherActiveMembers(p))
				break;

			if (Consts::EPS < p->duration)
				p->current_time = normalized_time * p->duration;
			if (p->syncker_index != -1)
				context.synckers[p->syncker_index].normalized_time = normalized_time;
			break;
		}
		case TYPE::ANIMATION_PLAYER_SYNC:
		{
			auto *p = static_cast<AnimNodeAnimationPlayerSync *>(node);
			if (Consts::EPS < p->duration)
				p->current_time = normalized_time * p->duration;
			break;
		}
		case TYPE::BLEND:
		{
			auto *b = static_cast<AnimNodeBlend *>(node);
			applyNormalizedTime(b->link_0.node, context, normalized_time);
			applyNormalizedTime(b->link_1.node, context, normalized_time);
			break;
		}
		case TYPE::BLEND_SPACE_2D:
		{
			auto *bs = static_cast<AnimNodeBlendSpace2D *>(node);
			for (int i = 0; i < bs->links.size(); i++)
				applyNormalizedTime(bs->links[i].node, context, normalized_time);
			break;
		}
		case TYPE::BLEND_SPACE_2D_SYNC:
		{
			auto *bs = static_cast<AnimNodeBlendSpace2DSync *>(node);
			// Sync group has authority over enter_time - skip override when group is active
			if (bs->syncker_index != -1 && context.synckers[bs->syncker_index].hasOtherActiveMembers(bs))
				break;

			bs->normalized_time = normalized_time;
			bs->current_time = normalized_time * bs->duration;
			bs->syncChildrenTime();
			if (bs->syncker_index != -1)
				context.synckers[bs->syncker_index].normalized_time = normalized_time;
			break;
		}
		case TYPE::BRANCH:
		{
			auto *br = static_cast<AnimNodeBranch *>(node);
			applyNormalizedTime(br->link_true.node, context, normalized_time);
			applyNormalizedTime(br->link_false.node, context, normalized_time);
			break;
		}
		case TYPE::MAKE_ADDITIVE:
		{
			auto *m = static_cast<AnimNodeMakeAdditive *>(node);
			applyNormalizedTime(m->link_base_pose.node, context, normalized_time);
			break;
		}
		case TYPE::APPLY_ADDITIVE:
		{
			auto *a = static_cast<AnimNodeApplyAdditive *>(node);
			applyNormalizedTime(a->link_base_pose.node, context, normalized_time);
			break;
		}
		case TYPE::STATE:
		{
			auto *s = static_cast<AnimNodeState *>(node);
			applyNormalizedTime(s->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::GET_JOINT_TRANSFORM:
		{
			auto *g = static_cast<AnimNodeGetJointTransform *>(node);
			applyNormalizedTime(g->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::SET_JOINT_TRANSFORM:
		{
			auto *s = static_cast<AnimNodeSetJointTransform *>(node);
			applyNormalizedTime(s->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::CONVERT_SPACE:
		{
			auto *c = static_cast<AnimNodeConvertSpaceBase *>(node);
			applyNormalizedTime(c->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::RUNTIME_MATH:
		{
			break; // runtime math has no pose input
		}
		case TYPE::TWO_BONE_IK:
		{
			auto *ik = static_cast<AnimNodeTwoBoneIK *>(node);
			applyNormalizedTime(ik->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::LOOK_AT:
		{
			auto *la = static_cast<AnimNodeLookAt *>(node);
			applyNormalizedTime(la->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::JOINT_HINGE_LIMIT:
		{
			auto *jh = static_cast<AnimNodeJointHingeLimit *>(node);
			applyNormalizedTime(jh->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::JOINT_CONE_LIMIT:
		{
			auto *jc = static_cast<AnimNodeJointConeLimit *>(node);
			applyNormalizedTime(jc->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::JOINT_CONE_ASYM_LIMIT:
		{
			auto *jca = static_cast<AnimNodeJointConeAsymLimit *>(node);
			applyNormalizedTime(jca->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::JOINT_TWIST_LIMIT:
		{
			auto *jt = static_cast<AnimNodeJointTwistLimit *>(node);
			applyNormalizedTime(jt->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::JOINT_HINGE_TWIST_LIMIT:
		{
			auto *jht = static_cast<AnimNodeJointHingeTwistLimit *>(node);
			applyNormalizedTime(jht->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::JOINT_CONE_TWIST_LIMIT:
		{
			auto *jct = static_cast<AnimNodeJointConeTwistLimit *>(node);
			applyNormalizedTime(jct->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::JOINT_CONE_ASYM_TWIST_LIMIT:
		{
			auto *jcat = static_cast<AnimNodeJointConeAsymTwistLimit *>(node);
			applyNormalizedTime(jcat->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::IK_CHAIN:
		{
			auto *ikc = static_cast<AnimNodeIKChain *>(node);
			applyNormalizedTime(ikc->link_pose.node, context, normalized_time);
			break;
		}
		case TYPE::JOINT_LIMIT_SET:
		{
			// no pose input
			break;
		}
		case TYPE::LOOK_AT_CHAIN:
		{
			auto *lac = static_cast<AnimNodeLookAtChain *>(node);
			applyNormalizedTime(lac->link_pose.node, context, normalized_time);
			break;
		}
		default:
			break;
	}
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Debug Info
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimNodeDebugInfo
{
	// Execution state
	HashMap<UGUID, float> child_weights;
	int pose_layer{-1};
	float accumulated_weight{0.0f}; // Weight from FinalPose to this node [0..1]

	// Time/Progress state
	float current_time{-1.0f};
	float duration{-1.0f};
	float normalized_time{-1.0f};
	float transition_progress{-1.0f};
	float transition_time{-1.0f};
	float transition_duration{-1.0f};
	float speed{1.0f};
	bool is_loop{false};
	float frame{-1.0f};

	// Weight state
	float blend_weight{-1.0f};

	// Structural state
	UGUID current_state_runtime_guid;
	UGUID previous_state_runtime_guid;
	UGUID active_transition_runtime_guid;

	// BlendSpace state
	vec2 blend_point{vec2_zero};
	Vector<float> sample_weights;

	// Animation
	UGUID asset_guid;
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Pose View Info
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct PoseViewInfo
{
	vec4 color{vec4_black};
	float bone_radius{0.0045f};
	bool depth_test{false};
	// Bones only, no per-node overlays: keeps a SubGraph's inner solver viz inside its boundary.
	bool show_only_pose{false};
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Node Debug Context
///////////////////////////////////////////////////////////////////////////////////////////////////////
struct AnimGraphDebugContext
{
	HashMap<UGUID, AnimNodeDebugInfo> active_nodes;
	HashMap<UGUID, PoseViewInfo> viewed_poses;

	// The editor dispatches by node_type for debug rendering.
	Vector<AnimNodeBase *> active_runtime_nodes;

	void addViewedPose(const UGUID &node_runtime_guid, const vec4 &color = vec4_black, bool show_only_pose = false)
	{
		PoseViewInfo info;
		info.color = color;
		info.show_only_pose = show_only_pose;
		viewed_poses[node_runtime_guid] = info;
	}

	void removeViewedPose(const UGUID &node_runtime_guid)
	{
		viewed_poses.erase(node_runtime_guid);
	}

	void setViewedPoseColor(const UGUID &node_runtime_guid, const vec4 &color)
	{
		if (viewed_poses.contains(node_runtime_guid))
			viewed_poses[node_runtime_guid].color = color;
	}

	bool isViewedPose(const UGUID &node_runtime_guid) const
	{
		return viewed_poses.contains(node_runtime_guid);
	}

	void clearViewedPoses()
	{
		viewed_poses.clear();
	}

	void renderViewedPoses(const NodeSkeletonPosePtr &skeleton_pose, bool fix_root_in_origin = false, int root_joint = 0)
	{
		if (skeleton_pose.isNull() || viewed_poses.empty())
			return;

		const int num_layers = skeleton_pose->getNumLayers();
		for (const auto &pair : viewed_poses)
		{
			auto iter = active_nodes.find(pair.key);
			if (iter == active_nodes.end())
				continue;

			const AnimNodeDebugInfo &debug_info = iter->data;
			if (debug_info.pose_layer < 0 || num_layers <= debug_info.pose_layer)
				continue;

			Mat4 transform = Mat4_identity;
			if (fix_root_in_origin && 0 <= root_joint)
			{
				const mat4 joint_transform = skeleton_pose->getLayerJointTransform(debug_info.pose_layer, root_joint);
				const Vec3 position = Vec3(joint_transform.getTranslate());
				transform = translate(Vec3(-position.x, -position.y, 0.0));
			}

			const PoseViewInfo &view_info = pair.data;
			skeleton_pose->renderLayerBones(debug_info.pose_layer, transform, view_info.color, view_info.bone_radius, view_info.depth_test);
		}
	}

	static bool isActiveNode(const AnimNodeBase *node)
	{
		return (node != nullptr && node->runtime_guid.isValid() && node->need_update == false);
	}

	void collectDebugInfo(const Vector<AnimNodeBase *> &nodes)
	{
		active_nodes.clear();
		active_runtime_nodes.clear();

		for (const auto node : nodes)
		{
			if (node == nullptr || node->runtime_guid.isEmpty())
				continue;

			// ATTENTION: every TYPE::* needs a case below — the default branch logs every frame and kills perf.
			if (node->need_update == false || node->manual_active)
			{
				active_runtime_nodes.append(node);

				AnimNodeDebugInfo info;
				info.pose_layer = node->pose_layer;
				// Highlight weight comes from the consumer chain: their own accumulated_weight is always 1.0.
				if (node->manual_active)
				{
					float weight = 1.0f;
					AnimNodeBase *src = node->consumer_for_weight;
					for (int hops = 0; src != nullptr && hops < 16; hops++)
					{
						if (src->node_type == AnimNodeBase::TYPE::IK_CHAIN)
						{
							auto *ik = static_cast<AnimNodeIKChain *>(src);
							weight = src->accumulated_weight * ik->link_weight.getValue();
							break;
						}

						if (src->node_type == AnimNodeBase::TYPE::LOOK_AT_CHAIN)
						{
							auto *lac = static_cast<AnimNodeLookAtChain *>(src);
							weight = src->accumulated_weight * lac->link_weight.getValue();
							break;
						}

						if (src->manual_active == false)
						{
							weight = src->accumulated_weight;
							break;
						}

						src = src->consumer_for_weight;
					}
					info.accumulated_weight = weight;
				} else
				{
					info.accumulated_weight = node->accumulated_weight;
				}

				switch (node->node_type)
				{
					case AnimNodeBase::TYPE::FINAL_POSE:
					{
						auto *final_pose = static_cast<AnimNodeFinalPose *>(node);
						if (isActiveNode(final_pose->link.node))
							info.child_weights[final_pose->link.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::ANIMATION_POSE:
					{
						auto *anim_pose = static_cast<AnimNodeAnimationPose *>(node);
						info.asset_guid = anim_pose->asset_guid;
						info.frame = anim_pose->frame;
						break;
					}
					case AnimNodeBase::TYPE::ANIMATION_PLAYER:
					{
						auto *player = static_cast<AnimNodeAnimationPlayer *>(node);
						info.current_time = player->current_time;
						info.duration = player->duration;
						info.normalized_time = (Consts::EPS < player->duration) ? (player->current_time / player->duration) : 0.0f;
						info.asset_guid = player->asset_guid;
						info.speed = player->speed;
						info.is_loop = player->is_loop;
						break;
					}
					case AnimNodeBase::TYPE::ANIMATION_PLAYER_SYNC:
					{
						auto *player_sync = static_cast<AnimNodeAnimationPlayerSync *>(node);
						info.current_time = player_sync->current_time;
						info.duration = player_sync->duration;
						info.normalized_time = (Consts::EPS < player_sync->duration) ? (player_sync->current_time / player_sync->duration) : 0.0f;
						info.asset_guid = player_sync->asset_guid;
						info.speed = player_sync->speed;
						break;
					}
					case AnimNodeBase::TYPE::BLEND:
					{
						auto *blend = static_cast<AnimNodeBlend *>(node);
						info.blend_weight = blend->weight;
						if (isActiveNode(blend->link_0.node))
						{
							if (blend->blend_mask == -1)
								info.child_weights[blend->link_0.node->runtime_guid] = 1.0f - blend->weight;
							else
								info.child_weights[blend->link_0.node->runtime_guid] = 1.0f;
						}
						if (isActiveNode(blend->link_1.node))
							info.child_weights[blend->link_1.node->runtime_guid] = blend->weight;
						break;
					}
					case AnimNodeBase::TYPE::BRANCH:
					{
						auto *branch = static_cast<AnimNodeBranch *>(node);
						PoseLink &active = branch->condition ? branch->link_true : branch->link_false;
						if (isActiveNode(active.node))
							info.child_weights[active.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::MAKE_ADDITIVE:
					{
						auto *make_add = static_cast<AnimNodeMakeAdditive *>(node);
						if (isActiveNode(make_add->link_base_pose.node))
							info.child_weights[make_add->link_base_pose.node->runtime_guid] = 1.0f;
						if (isActiveNode(make_add->link_ref_pose.node))
							info.child_weights[make_add->link_ref_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::APPLY_ADDITIVE:
					{
						auto *apply_add = static_cast<AnimNodeApplyAdditive *>(node);
						info.blend_weight = apply_add->weight;
						if (isActiveNode(apply_add->link_base_pose.node))
							info.child_weights[apply_add->link_base_pose.node->runtime_guid] = 1.0f;
						if (isActiveNode(apply_add->link_additive.node))
							info.child_weights[apply_add->link_additive.node->runtime_guid] = apply_add->weight;
						break;
					}
					case AnimNodeBase::TYPE::BLEND_SPACE_2D:
					{
						auto *bs = static_cast<AnimNodeBlendSpace2D *>(node);
						info.blend_point = bs->blend_point;
						info.sample_weights.clear();
						for (int i = 0; i < bs->smoothed_weights.size(); i++)
							info.sample_weights.append(bs->smoothed_weights[i]);
						for (int i = 0; i < bs->links.size(); i++)
						{
							if (isActiveNode(bs->links[i].node))
								info.child_weights[bs->links[i].node->runtime_guid] = bs->smoothed_weights[i];
						}
						break;
					}
					case AnimNodeBase::TYPE::BLEND_SPACE_2D_SYNC:
					{
						auto *bs = static_cast<AnimNodeBlendSpace2DSync *>(node);
						info.current_time = bs->current_time;
						info.duration = bs->duration;
						info.normalized_time = bs->normalized_time;
						info.speed = bs->speed;
						info.blend_point = bs->blend_point;
						info.sample_weights.clear();
						for (int i = 0; i < bs->smoothed_weights.size(); i++)
							info.sample_weights.append(bs->smoothed_weights[i]);
						for (int i = 0; i < bs->links.size(); i++)
						{
							if (isActiveNode(bs->links[i].node))
								info.child_weights[bs->links[i].node->runtime_guid] = bs->smoothed_weights[i];
						}
						break;
					}
					case AnimNodeBase::TYPE::STATE:
					{
						auto *state = static_cast<AnimNodeState *>(node);
						if (isActiveNode(state->link_pose.node))
							info.child_weights[state->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::STATE_MACHINE:
					{
						auto *sm = static_cast<AnimNodeStateMachine *>(node);
						info.transition_progress = sm->transition_progress;
						info.transition_time = sm->transition_time;
						info.transition_duration = sm->transition_duration;
						if (sm->current_state != nullptr)
							info.current_state_runtime_guid = sm->current_state->runtime_guid;
						if (sm->previous_state != nullptr)
							info.previous_state_runtime_guid = sm->previous_state->runtime_guid;
						if (sm->active_transition != nullptr)
							info.active_transition_runtime_guid = sm->active_transition->runtime_guid;
						if (isActiveNode(sm->current_state))
							info.child_weights[sm->current_state->runtime_guid] = sm->transition_progress;
						if (isActiveNode(sm->previous_state))
							info.child_weights[sm->previous_state->runtime_guid] = 1.0f - sm->transition_progress;
						break;
					}
					case AnimNodeBase::TYPE::GET_JOINT_TRANSFORM:
					{
						auto *g = static_cast<AnimNodeGetJointTransform *>(node);
						if (isActiveNode(g->link_pose.node))
							info.child_weights[g->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::SET_JOINT_TRANSFORM:
					{
						auto *s = static_cast<AnimNodeSetJointTransform *>(node);
						if (isActiveNode(s->link_pose.node))
							info.child_weights[s->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::CONVERT_SPACE:
					{
						auto *c = static_cast<AnimNodeConvertSpaceBase *>(node);
						if (isActiveNode(c->link_pose.node))
							info.child_weights[c->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::RUNTIME_MATH:
					{
						// scalar inputs via ScalarLink are not displayed in pose tree
						break;
					}
					case AnimNodeBase::TYPE::TWO_BONE_IK:
					{
						auto *ik = static_cast<AnimNodeTwoBoneIK *>(node);
						if (isActiveNode(ik->link_pose.node))
							info.child_weights[ik->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::LOOK_AT:
					{
						auto *la = static_cast<AnimNodeLookAt *>(node);
						if (isActiveNode(la->link_pose.node))
							info.child_weights[la->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::JOINT_HINGE_LIMIT:
					{
						auto *jh = static_cast<AnimNodeJointHingeLimit *>(node);
						if (isActiveNode(jh->link_pose.node))
							info.child_weights[jh->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::JOINT_CONE_LIMIT:
					{
						auto *jc = static_cast<AnimNodeJointConeLimit *>(node);
						if (isActiveNode(jc->link_pose.node))
							info.child_weights[jc->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::JOINT_CONE_ASYM_LIMIT:
					{
						auto *jca = static_cast<AnimNodeJointConeAsymLimit *>(node);
						if (isActiveNode(jca->link_pose.node))
							info.child_weights[jca->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::JOINT_TWIST_LIMIT:
					{
						auto *jt = static_cast<AnimNodeJointTwistLimit *>(node);
						if (isActiveNode(jt->link_pose.node))
							info.child_weights[jt->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::JOINT_HINGE_TWIST_LIMIT:
					{
						auto *jht = static_cast<AnimNodeJointHingeTwistLimit *>(node);
						if (isActiveNode(jht->link_pose.node))
							info.child_weights[jht->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::JOINT_CONE_TWIST_LIMIT:
					{
						auto *jct = static_cast<AnimNodeJointConeTwistLimit *>(node);
						if (isActiveNode(jct->link_pose.node))
							info.child_weights[jct->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::JOINT_CONE_ASYM_TWIST_LIMIT:
					{
						auto *jcat = static_cast<AnimNodeJointConeAsymTwistLimit *>(node);
						if (isActiveNode(jcat->link_pose.node))
							info.child_weights[jcat->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::IK_CHAIN:
					{
						auto *ikc = static_cast<AnimNodeIKChain *>(node);
						if (isActiveNode(ikc->link_pose.node))
							info.child_weights[ikc->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					case AnimNodeBase::TYPE::JOINT_LIMIT_SET:
					{
						// no child_weights, but stays in the active set so the editor still highlights it
						break;
					}
					case AnimNodeBase::TYPE::LOOK_AT_CHAIN:
					{
						auto *lac = static_cast<AnimNodeLookAtChain *>(node);
						if (isActiveNode(lac->link_pose.node))
							info.child_weights[lac->link_pose.node->runtime_guid] = 1.0f;
						break;
					}
					default:
					{
						Log::warning("AnimGraphDebugContext::collectDebugInfo(): unknown node type %d\n", static_cast<int>(node->node_type));
						break;
					}
				}

				active_nodes[node->runtime_guid] = info;
			}
		}
	}

	void clearActiveNodes()
	{
		active_nodes.clear();
	}

	void clear()
	{
		active_nodes.clear();
		viewed_poses.clear();
		active_runtime_nodes.clear();
	}
};

///////////////////////////////////////////////////////////////////////////////////////////////////////
// Anim Script Base
///////////////////////////////////////////////////////////////////////////////////////////////////////

class AnimScriptBase
{
public:
	AnimScriptBase() = default;
	virtual ~AnimScriptBase() = default;

	// execution sequence
	virtual void init()
	{
		if (skeleton_pose.isNull())
			return;

		skeleton_pose->setNumLayers(1);
		context.skeleton_pose = skeleton_pose;

		nodes.append(&final_pose);
	}

	virtual void update(float ifps)
	{
		if (skeleton_pose.isNull())
			return;

		context.skeleton_pose = skeleton_pose;
		context.ifps = ifps;

		context.self_timed_players.clear();

		for (auto n : nodes)
		{
			if (n->need_update)
			{
				if (n->node_type == AnimNodeBase::TYPE::ANIMATION_PLAYER || n->node_type == AnimNodeBase::TYPE::ANIMATION_PLAYER_SYNC)
					static_cast<AnimNodeAnimationPlayerBase *>(n)->was_inactive = true;
				else if (n->node_type == AnimNodeBase::TYPE::BLEND_SPACE_2D_SYNC)
					static_cast<AnimNodeBlendSpace2DSync *>(n)->was_inactive = true;
				else if (n->node_type == AnimNodeBase::TYPE::BLEND_SPACE_2D)
					static_cast<AnimNodeBlendSpace2D *>(n)->was_inactive = true;
			}

			n->need_update = true;
			n->need_evaluate = true;
			n->accumulated_weight = 0.0f;
		}
	}

	virtual void shutdown()
	{
		if (skeleton_pose.isValid())
			skeleton_pose->setNumLayers(1);
	}

	// parameters
	enum PARAM_TYPE
	{
		PARAM_TYPE_UNKNOWN = 0,
		PARAM_TYPE_INT,
		PARAM_TYPE_FLOAT,
		PARAM_TYPE_BOOL,
		PARAM_TYPE_QUAT,
		PARAM_TYPE_VEC2,
		PARAM_TYPE_VEC3,
		PARAM_TYPE_VEC4,
		PARAM_TYPE_IVEC2,
		PARAM_TYPE_IVEC3,
		PARAM_TYPE_IVEC4,
		PARAM_TYPE_MAT3,
		PARAM_TYPE_MAT4,
		PARAM_TYPE_ANIM_ASSET,
		PARAM_TYPE_TRIGGER,
		NUM_PARAM_TYPES,
	};

	virtual int getNumParams() const { return 0; }
	virtual int getParamType(int /*index*/) const { return PARAM_TYPE_UNKNOWN; }
	virtual const char *getParamName(int /*index*/) const { return "<wrong_param_name>"; }

	virtual void resetParam(const char * /*name*/) {}

	virtual void setParamInt(const char * /*name*/, int /*value*/) {}
	virtual int getParamInt(const char * /*name*/) const { return 0; }

	virtual void setParamFloat(const char * /*name*/, float /*value*/) {}
	virtual float getParamFloat(const char * /*name*/) const { return 0.0f; }

	virtual void setParamBool(const char * /*name*/, bool /*value*/) {}
	virtual bool getParamBool(const char * /*name*/) const { return false; }

	virtual void setParamQuat(const char * /*name*/, const quat & /*value*/) {}
	virtual const quat &getParamQuat(const char * /*name*/) const { return quat_identity; }

	virtual void setParamVec2(const char * /*name*/, const vec2 & /*value*/) {}
	virtual const vec2 &getParamVec2(const char * /*name*/) const { return vec2_zero; }

	virtual void setParamVec3(const char * /*name*/, const vec3 & /*value*/) {}
	virtual const vec3 &getParamVec3(const char * /*name*/) const { return vec3_zero; }

	virtual void setParamVec4(const char * /*name*/, const vec4 & /*value*/) {}
	virtual const vec4 &getParamVec4(const char * /*name*/) const { return vec4_zero; }

	virtual void setParamIVec2(const char * /*name*/, const ivec2 & /*value*/) {}
	virtual const ivec2 &getParamIVec2(const char * /*name*/) const { return ivec2_zero; }

	virtual void setParamIVec3(const char * /*name*/, const ivec3 & /*value*/) {}
	virtual const ivec3 &getParamIVec3(const char * /*name*/) const { return ivec3_zero; }

	virtual void setParamIVec4(const char * /*name*/, const ivec4 & /*value*/) {}
	virtual const ivec4 &getParamIVec4(const char * /*name*/) const { return ivec4_zero; }

	virtual void setParamMat3(const char * /*name*/, const mat3 & /*value*/) {}
	virtual const mat3 &getParamMat3(const char * /*name*/) const { return mat3_identity; }

	virtual void setParamMat4(const char * /*name*/, const mat4 & /*value*/) {}
	virtual const mat4 &getParamMat4(const char * /*name*/) const { return mat4_identity; }

	virtual void setParamAnimAsset(const char * /*name*/, const UGUID & /*guid*/) {}
	virtual const UGUID &getParamAnimAsset(const char * /*name*/) const { return UGUID::empty; }

	virtual void setParamTrigger(const char * /*name*/, bool /*value*/) {}
	virtual bool getParamTrigger(const char * /*name*/) const { return false; }

	void initPoseLayers()
	{
		for (auto n : nodes)
			n->grabConstPoseLayer(context);
	}

	void updateFinalPose()
	{
		// Released only now, so last frame's layers stay alive for the editor preview
		context.releaseTmpPoseLayers();

		// Manual-active nodes live OUTSIDE the pose-flow tree: drive them first, or an IK Chain inside
		// the tree reads constraint info still at its constructor defaults.
		for (auto n : nodes)
		{
			if (n != nullptr && n->manual_active)
				n->update(context);
		}

		final_pose.update(context);

		for (auto &syncker : context.synckers)
			syncker.update(context);

		for (auto player : context.self_timed_players)
			player->nextTime(context);

		// Same rationale as the update pass above.
		for (auto n : nodes)
		{
			if (n != nullptr && n->manual_active)
				n->evaluate(context);
		}

		final_pose.evaluate(context);

		if (debug_collecting)
			debug_context.collectDebugInfo(nodes);
		// renderViewedPoses called by editor with correct mesh transform
	}

	void setNodeSkeletonPose(const NodeSkeletonPosePtr &node) { skeleton_pose = node; }

	void setDebugCollecting(bool enabled) { debug_collecting = enabled; }
	bool isDebugCollecting() const { return debug_collecting; }

	AnimGraphDebugContext *getDebugContext() { return &debug_context; }
	const AnimGraphDebugContext *getDebugContext() const { return &debug_context; }

	bool isActiveRootMotion() const { return context.isActiveRootMotion(); }
	const Transform &getRootMotionDelta() const { return context.root_motion_delta; }

	// Called once from the generated init().
	void buildStateMachineIndex()
	{
		state_machines.clear();
		for (auto *n : nodes)
		{
			if (n->node_type == AnimNodeBase::TYPE::STATE_MACHINE)
				state_machines.append(static_cast<AnimNodeStateMachine *>(n));
		}

		for (int i = 0; i < state_machines.size(); i++)
		{
			for (int j = i + 1; j < state_machines.size(); j++)
			{
				if (state_machines[i]->sm_path == state_machines[j]->sm_path)
					Log::error("AnimScriptBase::buildStateMachineIndex(): duplicate state machine path \"%s\", lookups by it are ambiguous\n", state_machines[i]->sm_path.get());
			}
		}
	}

	// structure
	int getNumStateMachines() const { return state_machines.size(); }

	int findStateMachine(const char *path) const
	{
		for (int i = 0; i < state_machines.size(); i++)
		{
			if (state_machines[i]->sm_path == path)
				return i;
		}
		return -1;
	}

	// Short name is not unique - first match wins, use findStateMachine() for the full path.
	int findStateMachineByName(const char *name) const
	{
		for (int i = 0; i < state_machines.size(); i++)
		{
			if (String::equal(short_name(state_machines[i]->sm_path.get()), name))
				return i;
		}
		return -1;
	}

	const char *getStateMachinePath(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->sm_path.get() : "";
	}

	const char *getStateMachineName(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? short_name(m->sm_path.get()) : "";
	}

	int getStateMachineParent(int sm) const
	{
		const auto *m = get_state_machine(sm);
		if (m == nullptr || m->parent_sm == nullptr)
			return -1;

		for (int i = 0; i < state_machines.size(); i++)
		{
			if (state_machines[i] == m->parent_sm)
				return i;
		}
		return -1;
	}

	const char *getStateMachineOwnerStateName(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->owner_state_name.get() : "";
	}

	int getStateMachineNumStates(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->states.size() : 0;
	}

	const char *getStateMachineStateName(int sm, int state) const
	{
		const auto *m = get_state_machine(sm);
		if (m == nullptr || state < 0 || m->states.size() <= state)
			return "";

		return m->states[state]->state_name.get();
	}

	int findStateMachineState(int sm, const char *state_name) const
	{
		const auto *m = get_state_machine(sm);
		if (m == nullptr)
			return -1;

		for (int i = 0; i < m->states.size(); i++)
		{
			if (m->states[i]->state_name == state_name)
				return i;
		}
		return -1;
	}

	// current status
	bool isStateMachineRelevant(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return (m && m->need_update == false);
	}

	int getStateMachineCurrentState(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? state_index(m, m->current_state) : -1;
	}

	int getStateMachinePreviousState(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? state_index(m, m->previous_state) : -1;
	}

	const char *getStateMachineCurrentStateName(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return (m && m->current_state) ? m->current_state->state_name.get() : "";
	}

	const char *getStateMachinePreviousStateName(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return (m && m->previous_state) ? m->previous_state->state_name.get() : "";
	}

	bool isStateMachineInTransition(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return (m && (m->is_blending_previous || m->has_frozen_pose));
	}

	// timings
	float getStateMachineStateTime(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->current_state_time : 0.0f;
	}

	float getStateMachineTransitionProgress(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->transition_progress : 1.0f;
	}

	float getStateMachineTransitionTime(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->transition_time : 0.0f;
	}

	float getStateMachineTransitionDuration(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->transition_duration : 0.0f;
	}

	float getStateMachineAnimTime(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->anim_time : 0.0f;
	}

	float getStateMachineAnimTimeFraction(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->anim_time_fraction : 0.0f;
	}

	float getStateMachineAnimLength(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->anim_length : 0.0f;
	}

	float getStateMachineAnimTimeRemaining(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->anim_time_remaining : 0.0f;
	}

	float getStateMachineAnimTimeRemainingFraction(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->anim_time_remaining_fraction : 0.0f;
	}

	bool isStateMachineAnimEnded(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return (m && m->anim_ended);
	}

	// control
	void setStateMachineState(int sm, int state, float duration)
	{
		if (sm < 0 || state_machines.size() <= sm)
			return;

		state_machines[sm]->requestState(state, duration, -1.0f);
	}

	void setStateMachineStateAtTime(int sm, int state, float duration, float normalized_time)
	{
		if (sm < 0 || state_machines.size() <= sm)
			return;

		state_machines[sm]->requestState(state, duration, saturate(normalized_time));
	}

	int getStateMachineNumTransitionConditions(int sm) const
	{
		const auto *m = get_state_machine(sm);
		return m ? m->active_transition_path.size() : 0;
	}

	const char *getStateMachineTransitionConditionName(int sm, int index) const
	{
		const auto *m = get_state_machine(sm);
		if (m == nullptr || index < 0 || m->active_transition_path.size() <= index)
			return "";

		return m->active_transition_path[index]->name.get();
	}

protected:
	static const char *short_name(const char *path)
	{
		const char *name = path;
		for (const char *c = path; *c != '\0'; c++)
		{
			if (*c == '/')
				name = c + 1;
		}
		return name;
	}

	const AnimNodeStateMachine *get_state_machine(int index) const
	{
		return (0 <= index && index < state_machines.size()) ? state_machines[index] : nullptr;
	}

	static int state_index(const AnimNodeStateMachine *sm, const AnimNodeState *state)
	{
		if (state == nullptr)
			return -1;

		for (int i = 0; i < sm->states.size(); i++)
		{
			if (sm->states[i] == state)
				return i;
		}
		return -1;
	}

	NodeSkeletonPosePtr skeleton_pose;
	AnimGraphContext context;
	AnimGraphDebugContext debug_context;

	AnimNodeFinalPose final_pose;
	Vector<AnimNodeBase *> nodes;
	Vector<AnimNodeStateMachine *> state_machines;

	bool anim_assets_changed{false};
	bool debug_collecting{false};
};

} // namespace AnimGraphRuntime

} // namespace Unigine
