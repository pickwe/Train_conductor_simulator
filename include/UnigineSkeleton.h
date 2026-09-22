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
#include "UnigineGUID.h"
#include "UniginePair.h"

namespace Unigine
{

class Skeleton;
typedef const Skeleton ConstSkeleton;
class SkeletonPoseMatrix;
class SkeletonPoseDecomposed;

class UNIGINE_API SkeletonPoseDecomposed : public APIInterface
{
public:
	typedef const SkeletonPoseDecomposed ConstSkeletonPoseDecomposed;
	static constexpr bool HAS_OWNERSHIP = true;


	enum SPACE_TYPE
	{
		SPACE_TYPE_LOCAL = 0,
		SPACE_TYPE_OBJECT,
	};
	static Ptr<SkeletonPoseDecomposed> create();
	static Ptr<SkeletonPoseDecomposed> create(const Ptr<ConstSkeleton> &skeleton);
	void clear();
	void setSkeleton(const Ptr<ConstSkeleton> &skeleton);
	Ptr<ConstSkeleton> getSkeleton() const;
	int getNumTransforms() const;
	void setTransform(int index, const Math::Transform &transform);
	Math::Transform getTransform(int index) const;
	SkeletonPoseDecomposed::SPACE_TYPE getSpaceType() const;
	void convertToObjectSpace();
	void convertToLocalSpace();
	void inverse();
	void renderJoints(const Math::Mat4 &world_offset, float basis_length = 0.03f, bool depth_test = false) const;
	void renderSelectedJoints(const Vector<int> &joints, const Math::Mat4 &world_offset, float basis_length = 0.03f, bool depth_test = false) const;
	void renderJointNames(const Math::Mat4 &world_offset, const Math::vec4 &color, int outline = 0, int font_size = -1) const;
	void renderSelectedJointNames(const Vector<int> &joints, const Math::Mat4 &world_offset, const Math::vec4 &color, int outline = 0, int font_size = -1) const;
	void renderBones(const Math::Mat4 &world_offset, const Math::vec4 &color, float radius = 0.01f, bool depth_test = false) const;
	void renderSelectedBones(const Vector<int> &end_joints, const Math::Mat4 &world_offset, const Math::vec4 &color, float radius = 0.01f, bool depth_test = false) const;
	void assignFrom(const Ptr<SkeletonPoseDecomposed> &other);
	void assignFromMatrixPose(const Ptr<SkeletonPoseMatrix> &other);
	bool copyFromCompatible(const Ptr<SkeletonPoseDecomposed> &other);
	bool copyFromCompatible(const Ptr<SkeletonPoseMatrix> &other);
	bool blend(const Ptr<ConstSkeletonPoseDecomposed> &pose, float weight);
	bool blendByMask(const Ptr<ConstSkeletonPoseDecomposed> &pose, float weight, const char *mask_name);
	bool convertToAdditive(const Ptr<ConstSkeletonPoseDecomposed> &ref_pose);
	bool applyAdditivePose(const Ptr<ConstSkeletonPoseDecomposed> &additive_pose);
};
typedef Ptr<SkeletonPoseDecomposed> SkeletonPoseDecomposedPtr;
typedef Ptr<const SkeletonPoseDecomposed> ConstSkeletonPoseDecomposedPtr;
typedef const SkeletonPoseDecomposed ConstSkeletonPoseDecomposed;


class UNIGINE_API SkeletonPoseMatrix : public APIInterface
{
public:
	typedef const SkeletonPoseMatrix ConstSkeletonPoseMatrix;
	static constexpr bool HAS_OWNERSHIP = true;


	enum SPACE_TYPE
	{
		SPACE_TYPE_LOCAL = 0,
		SPACE_TYPE_OBJECT,
	};
	static Ptr<SkeletonPoseMatrix> create();
	static Ptr<SkeletonPoseMatrix> create(const Ptr<ConstSkeleton> &skeleton);
	void clear();
	void setSkeleton(const Ptr<ConstSkeleton> &skeleton);
	Ptr<ConstSkeleton> getSkeleton() const;
	int getNumTransforms() const;
	void setTransform(int index, const Math::mat4 &transform);
	Math::mat4 getTransform(int index) const;
	SkeletonPoseMatrix::SPACE_TYPE getSpaceType() const;
	void convertToObjectSpace();
	void convertToLocalSpace();
	void inverse();
	void renderJoints(const Math::Mat4 &world_offset, float basis_length = 0.03f, bool depth_test = false) const;
	void renderSelectedJoints(const Vector<int> &joints, const Math::Mat4 &world_offset, float basis_length = 0.03f, bool depth_test = false) const;
	void renderJointNames(const Math::Mat4 &world_offset, const Math::vec4 &color, int outline = 0, int font_size = -1) const;
	void renderSelectedJointNames(const Vector<int> &joints, const Math::Mat4 &world_offset, const Math::vec4 &color, int outline = 0, int font_size = -1) const;
	void renderBones(const Math::Mat4 &world_offset, const Math::vec4 &color, float radius = 0.01f, bool depth_test = false) const;
	void renderSelectedBones(const Vector<int> &end_joints, const Math::Mat4 &world_offset, const Math::vec4 &color, float radius = 0.01f, bool depth_test = false) const;
	void assignFrom(const Ptr<SkeletonPoseMatrix> &other);
	void assignFromPoseDecomposed(const Ptr<SkeletonPoseDecomposed> &other);
	bool copyFromCompatible(const Ptr<SkeletonPoseMatrix> &other);
	bool copyFromCompatible(const Ptr<SkeletonPoseDecomposed> &other);
	bool blend(const Ptr<SkeletonPoseMatrix> &pose, float weight);
	bool blendByMask(const Ptr<SkeletonPoseMatrix> &pose, float weight, const char *mask_name);
};
typedef Ptr<SkeletonPoseMatrix> SkeletonPoseMatrixPtr;
typedef Ptr<const SkeletonPoseMatrix> ConstSkeletonPoseMatrixPtr;
typedef const SkeletonPoseMatrix ConstSkeletonPoseMatrix;


class UNIGINE_API Skeleton : public APIInterface
{
public:
	typedef const Skeleton ConstSkeleton;
	static constexpr bool HAS_OWNERSHIP = true;


	enum RETARGET_MODE
	{
		RETARGET_MODE_POSE = 0,
		RETARGET_MODE_BIND,
		RETARGET_MODE_PROPORTION,
	};
	static Ptr<Skeleton> create();
	static void reloadResource(const char *path);
	void beginEditingHierarchy();
	void endEditingHierarchy(Vector<int> &old_to_new_joint);
	void endEditingHierarchy();
	bool isEditingHierarchy() const;
	int addJoint(const char *name = "", short parent = -1);
	void removeJoint(int index);
	int getNumJoints() const;
	void setJointName(int index, const char *name);
	const char *getJointName(int index) const;
	void setJointParent(int index, short parent);
	short getJointParent(int index) const;
	int findJoint(const char *name) const;
	void getRootJoints(Vector<int> &out_indices) const;
	void getChildJoints(int index, Vector<int> &out_indices) const;
	int getJointNumChildren(int index) const;
	int getJointChild(int joint_index, int child_index) const;
	bool isEqualHierarchy(const Ptr<ConstSkeleton> &skeleton) const;
	bool isEqual(const Ptr<ConstSkeleton> &skeleton) const;
	void setJointBindLocalTransform(int index, const Math::Transform &transform);
	void setJointBindLocalTransform(int index, const Math::mat4 &transform);
	Math::Transform getJointBindLocalTransform(int index) const;
	void setBindPose(const Ptr<ConstSkeletonPoseDecomposed> &pose);
	void getBindPose(const Ptr<SkeletonPoseDecomposed> &pose) const;
	void flipYZ();
	int addBlendMask(const char *name);
	void removeBlendMask(int index);
	int getNumBlendMasks() const;
	void setBlendMaskName(int index, const char *name);
	const char *getBlendMaskName(int index) const;
	void setBlendMaskInfluence(int mask_index, int joint_index, float influence);
	float getBlendMaskInfluence(int mask_index, int joint_index) const;
	int findBlendMask(const char *name) const;
	int addBlendTimeProfile(const char *name);
	void removeBlendTimeProfile(int index);
	int getNumBlendTimeProfiles() const;
	void setBlendTimeProfileName(int index, const char *name);
	const char *getBlendTimeProfileName(int index) const;
	void setBlendTimeProfileScale(int profile_index, int joint_index, float scale);
	float getBlendTimeProfileScale(int profile_index, int joint_index) const;
	int findBlendTimeProfile(const char *name) const;
	int addBlendWeightProfile(const char *name);
	void removeBlendWeightProfile(int index);
	int getNumBlendWeightProfiles() const;
	void setBlendWeightProfileName(int index, const char *name);
	const char *getBlendWeightProfileName(int index) const;
	void setBlendWeightProfileScale(int profile_index, int joint_index, float scale);
	float getBlendWeightProfileScale(int profile_index, int joint_index) const;
	int findBlendWeightProfile(const char *name) const;
	void setRetargetDataEnabled(bool enabled);
	bool isRetargetDataEnabled() const;
	void setRetargetDataMode(int joint_index, Skeleton::RETARGET_MODE mode);
	Skeleton::RETARGET_MODE getRetargetDataMode(int joint_index) const;
	float getRetargetDataBindLength(int joint_index) const;
	bool isRetargetDataCustomLength(int joint_index) const;
	void clearRetargetDataCustomLength(int joint_index);
	void setRetargetDataCustomLength(int joint_index, float length);
	float getRetargetDataCustomLength(int joint_index) const;
	float getRetargetDataLength(int joint_index) const;
	bool load(const char *path);
	bool save(const char *path) const;
	void clear();
	void assignFrom(const Ptr<Skeleton> &skeleton);
	UGUID getFileGUID() const;
	size_t getSystemMemoryUsage() const;
};
typedef Ptr<Skeleton> SkeletonPtr;
typedef Ptr<const Skeleton> ConstSkeletonPtr;
typedef const Skeleton ConstSkeleton;


class UNIGINE_API SkeletonRetargeter : public APIInterface
{
public:
	typedef const SkeletonRetargeter ConstSkeletonRetargeter;
	static constexpr bool HAS_OWNERSHIP = false;

	static bool convertible(SkeletonRetargeter *obj) { return obj != nullptr; }

	enum TYPE
	{
		SKELETON_RETARGETER = 0,
		SKELETON_RETARGETER_EQUALS,
		SKELETON_RETARGETER_NAMES_MATCHING,
		SKELETON_RETARGETER_TRANSLATIONS,
	};

	enum RETARGET_DIRECTION
	{
		RETARGET_DIRECTION_FORWARD = 0,
		RETARGET_DIRECTION_BACKWARD,
	};
	SkeletonRetargeter::TYPE getType() const;
	const char *getTypeName() const;
	UGUID getFirstFileGUID() const;
	UGUID getSecondFileGUID() const;
	void retarget(SkeletonRetargeter::RETARGET_DIRECTION retarget_direction, const Ptr<SkeletonPoseDecomposed> &out_pose, const Ptr<SkeletonPoseDecomposed> &compatible_pose) const;
	void retarget(SkeletonRetargeter::RETARGET_DIRECTION retarget_direction, const Ptr<SkeletonPoseMatrix> &out_pose, const Ptr<SkeletonPoseMatrix> &compatible_pose) const;
	void scaleRootMotion(SkeletonRetargeter::RETARGET_DIRECTION retarget_direction, Math::Transform &delta) const;
	static Ptr<SkeletonRetargeter> findRetargeter(const Ptr<ConstSkeleton> &skeleton_0, const Ptr<ConstSkeleton> &skeleton_1, int &out_retarget_direction);
	static Ptr<SkeletonRetargeter> findRetargeter(const Ptr<ConstSkeleton> &skeleton_0, const Ptr<ConstSkeleton> &skeleton_1);
	static Ptr<SkeletonRetargeter> findRetargeter(const UGUID &skeleton_file_guid_0, const UGUID &skeleton_file_guid_1, int &out_retarget_direction);
	static Ptr<SkeletonRetargeter> findRetargeter(const UGUID &skeleton_file_guid_0, const UGUID &skeleton_file_guid_1);
};
typedef Ptr<SkeletonRetargeter> SkeletonRetargeterPtr;
typedef Ptr<const SkeletonRetargeter> ConstSkeletonRetargeterPtr;
typedef const SkeletonRetargeter ConstSkeletonRetargeter;


class UNIGINE_API SkeletonRetargeterEquals : public SkeletonRetargeter
{
public:
typedef const SkeletonRetargeterEquals ConstSkeletonRetargeterEquals;
	static bool convertible(SkeletonRetargeter *obj) { return obj && obj->getType() == SkeletonRetargeter::SKELETON_RETARGETER_EQUALS; }
	static Ptr<SkeletonRetargeterEquals> create(const UGUID & first_file_guid, const UGUID & second_file_guid);
	static Ptr<SkeletonRetargeterEquals> create(const char *first_file, const char *second_file);
};
typedef Ptr<SkeletonRetargeterEquals> SkeletonRetargeterEqualsPtr;
typedef Ptr<const SkeletonRetargeterEquals> ConstSkeletonRetargeterEqualsPtr;
typedef const SkeletonRetargeterEquals ConstSkeletonRetargeterEquals;


class UNIGINE_API SkeletonRetargeterNamesMatching : public SkeletonRetargeter
{
public:
typedef const SkeletonRetargeterNamesMatching ConstSkeletonRetargeterNamesMatching;
	static bool convertible(SkeletonRetargeter *obj) { return obj && obj->getType() == SkeletonRetargeter::SKELETON_RETARGETER_NAMES_MATCHING; }
	static Ptr<SkeletonRetargeterNamesMatching> create(const UGUID & first_file_guid, const UGUID & second_file_guid);
	static Ptr<SkeletonRetargeterNamesMatching> create(const char *first_file, const char *second_file);
	bool setNamesMatching(const Vector<String> &first_names, const Vector<String> &second_names);
	bool setAutoMatching();
	bool setAutoMatching(const Ptr<ConstSkeleton> &first_skeleton, const Ptr<ConstSkeleton> &second_skeleton);
};
typedef Ptr<SkeletonRetargeterNamesMatching> SkeletonRetargeterNamesMatchingPtr;
typedef Ptr<const SkeletonRetargeterNamesMatching> ConstSkeletonRetargeterNamesMatchingPtr;
typedef const SkeletonRetargeterNamesMatching ConstSkeletonRetargeterNamesMatching;


class UNIGINE_API SkeletonRetargeterTranslations : public SkeletonRetargeter
{
public:
typedef const SkeletonRetargeterTranslations ConstSkeletonRetargeterTranslations;
	static bool convertible(SkeletonRetargeter *obj) { return obj && obj->getType() == SkeletonRetargeter::SKELETON_RETARGETER_TRANSLATIONS; }
	static Ptr<SkeletonRetargeterTranslations> create(const UGUID & first_file_guid, const UGUID & second_file_guid);
	static Ptr<SkeletonRetargeterTranslations> create(const char *first_file, const char *second_file);
	bool setMatching(const Vector<String> &first_names, const Vector<String> &second_names);
	bool setAutoMatching();
	bool setAutoMatching(const Ptr<ConstSkeleton> &first_skeleton, const Ptr<ConstSkeleton> &second_skeleton);
};
typedef Ptr<SkeletonRetargeterTranslations> SkeletonRetargeterTranslationsPtr;
typedef Ptr<const SkeletonRetargeterTranslations> ConstSkeletonRetargeterTranslationsPtr;
typedef const SkeletonRetargeterTranslations ConstSkeletonRetargeterTranslations;

} // namespace Unigine
