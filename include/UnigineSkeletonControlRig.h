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

namespace Unigine
{


class UNIGINE_API IKInfo : public APIInterface
{
public:
	typedef const IKInfo ConstIKInfo;
	static constexpr bool HAS_OWNERSHIP = true;

	static bool convertible(IKInfo *obj) { return obj != nullptr; }

	enum TYPE
	{
		IK_INFO = 0,
		IK_INFO_TWO_BONE,
		IK_INFO_CHAIN,
	};

	enum SOLVER_MODE
	{
		SOLVER_MODE_HARD,
		SOLVER_MODE_SOFT,
		SOLVER_MODE_STRETCHING,
	};
	IKInfo::TYPE getType() const;
	void setTarget(const Math::vec3 &target);
	Math::vec3 getTarget() const;
	void setPole(const Math::vec3 &pole);
	Math::vec3 getPole() const;
	void setWeight(float weight);
	float getWeight() const;
	void setSolverMode(IKInfo::SOLVER_MODE mode);
	IKInfo::SOLVER_MODE getSolverMode() const;
	void setSoftness(float softness);
	float getSoftness() const;
	void setMaxStretchScale(float scale);
	float getMaxStretchScale() const;
};
typedef Ptr<IKInfo> IKInfoPtr;
typedef Ptr<const IKInfo> ConstIKInfoPtr;
typedef const IKInfo ConstIKInfo;


class UNIGINE_API IKInfoTwoBone : public IKInfo
{
public:
typedef const IKInfoTwoBone ConstIKInfoTwoBone;
	static bool convertible(IKInfo *obj) { return obj && obj->getType() == IKInfo::IK_INFO_TWO_BONE; }
	static Ptr<IKInfoTwoBone> create();
	void setRootJoint(int joint);
	int getRootJoint() const;
	void setMidJoint(int joint);
	int getMidJoint() const;
	void setEndJoint(int joint);
	int getEndJoint() const;
	void setUsePole(bool pole);
	bool isUsePole() const;
};
typedef Ptr<IKInfoTwoBone> IKInfoTwoBonePtr;
typedef Ptr<const IKInfoTwoBone> ConstIKInfoTwoBonePtr;
typedef const IKInfoTwoBone ConstIKInfoTwoBone;


class UNIGINE_API LookAtInfo : public APIInterface
{
public:
	typedef const LookAtInfo ConstLookAtInfo;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<LookAtInfo> create();
	void setJoint(int joint);
	int getJoint() const;
	void setForwardAxis(const Math::vec3 &axis);
	Math::vec3 getForwardAxis() const;
	void setUpAxis(const Math::vec3 &axis);
	Math::vec3 getUpAxis() const;
	void setTarget(const Math::vec3 &target);
	Math::vec3 getTarget() const;
	void setPole(const Math::vec3 &pole);
	Math::vec3 getPole() const;
	void setWeight(float weight);
	float getWeight() const;
	void setUsePole(bool pole);
	bool isUsePole() const;
	void setMaxAngle(float angle);
	float getMaxAngle() const;
};
typedef Ptr<LookAtInfo> LookAtInfoPtr;
typedef Ptr<const LookAtInfo> ConstLookAtInfoPtr;
typedef const LookAtInfo ConstLookAtInfo;


class UNIGINE_API JointLimitInfo : public APIInterface
{
public:
	typedef const JointLimitInfo ConstJointLimitInfo;
	static constexpr bool HAS_OWNERSHIP = true;

	static bool convertible(JointLimitInfo *obj) { return obj != nullptr; }

	enum TYPE
	{
		JOINT_LIMIT_INFO = 0,
		JOINT_LIMIT_INFO_HINGE,
		JOINT_LIMIT_INFO_CONE,
		JOINT_LIMIT_INFO_CONE_ASYM,
		JOINT_LIMIT_INFO_TWIST,
		JOINT_LIMIT_INFO_HINGE_TWIST,
		JOINT_LIMIT_INFO_CONE_TWIST,
		JOINT_LIMIT_INFO_CONE_ASYM_TWIST,
	};

	enum LIMIT_MODE
	{
		LIMIT_MODE_FREE = 0,
		LIMIT_MODE_LIMITED,
		LIMIT_MODE_LOCKED,
	};
	JointLimitInfo::TYPE getType() const;
	void setJoint(int joint);
	int getJoint() const;
	void setMode(JointLimitInfo::LIMIT_MODE mode);
	JointLimitInfo::LIMIT_MODE getMode() const;
	void setWeight(float weight);
	float getWeight() const;
	void setPreferredLocalRotation(const Math::quat &rotation);
	Math::quat getPreferredLocalRotation() const;
	void setPreferredStrength(float strength);
	float getPreferredStrength() const;
};
typedef Ptr<JointLimitInfo> JointLimitInfoPtr;
typedef Ptr<const JointLimitInfo> ConstJointLimitInfoPtr;
typedef const JointLimitInfo ConstJointLimitInfo;


class UNIGINE_API JointLimitInfoHinge : public JointLimitInfo
{
public:
typedef const JointLimitInfoHinge ConstJointLimitInfoHinge;
	static bool convertible(JointLimitInfo *obj) { return obj && obj->getType() == JointLimitInfo::JOINT_LIMIT_INFO_HINGE; }
	static Ptr<JointLimitInfoHinge> create();
	void setHingeAxis(const Math::vec3 &axis);
	Math::vec3 getHingeAxis() const;
	void setMinAngle(float angle);
	float getMinAngle() const;
	void setMaxAngle(float angle);
	float getMaxAngle() const;
};
typedef Ptr<JointLimitInfoHinge> JointLimitInfoHingePtr;
typedef Ptr<const JointLimitInfoHinge> ConstJointLimitInfoHingePtr;
typedef const JointLimitInfoHinge ConstJointLimitInfoHinge;


class UNIGINE_API JointLimitInfoCone : public JointLimitInfo
{
public:
typedef const JointLimitInfoCone ConstJointLimitInfoCone;
	static bool convertible(JointLimitInfo *obj) { return obj && obj->getType() == JointLimitInfo::JOINT_LIMIT_INFO_CONE; }
	static Ptr<JointLimitInfoCone> create();
	void setForwardAxis(const Math::vec3 &axis);
	Math::vec3 getForwardAxis() const;
	void setConeAxis(const Math::vec3 &axis);
	Math::vec3 getConeAxis() const;
	void setConeAngle(float angle);
	float getConeAngle() const;
};
typedef Ptr<JointLimitInfoCone> JointLimitInfoConePtr;
typedef Ptr<const JointLimitInfoCone> ConstJointLimitInfoConePtr;
typedef const JointLimitInfoCone ConstJointLimitInfoCone;


class UNIGINE_API JointLimitInfoConeAsym : public JointLimitInfo
{
public:
typedef const JointLimitInfoConeAsym ConstJointLimitInfoConeAsym;
	static bool convertible(JointLimitInfo *obj) { return obj && obj->getType() == JointLimitInfo::JOINT_LIMIT_INFO_CONE_ASYM; }
	static Ptr<JointLimitInfoConeAsym> create();
	void setForwardAxis(const Math::vec3 &axis);
	Math::vec3 getForwardAxis() const;
	void setUpAxis(const Math::vec3 &axis);
	Math::vec3 getUpAxis() const;
	void setConeAxis(const Math::vec3 &axis);
	Math::vec3 getConeAxis() const;
	void setSwingLeftAngle(float angle);
	float getSwingLeftAngle() const;
	void setSwingRightAngle(float angle);
	float getSwingRightAngle() const;
	void setSwingUpAngle(float angle);
	float getSwingUpAngle() const;
	void setSwingDownAngle(float angle);
	float getSwingDownAngle() const;
};
typedef Ptr<JointLimitInfoConeAsym> JointLimitInfoConeAsymPtr;
typedef Ptr<const JointLimitInfoConeAsym> ConstJointLimitInfoConeAsymPtr;
typedef const JointLimitInfoConeAsym ConstJointLimitInfoConeAsym;


class UNIGINE_API JointLimitInfoTwist : public JointLimitInfo
{
public:
typedef const JointLimitInfoTwist ConstJointLimitInfoTwist;
	static bool convertible(JointLimitInfo *obj) { return obj && obj->getType() == JointLimitInfo::JOINT_LIMIT_INFO_TWIST; }
	static Ptr<JointLimitInfoTwist> create();
	void setForwardAxis(const Math::vec3 &axis);
	Math::vec3 getForwardAxis() const;
	void setMinAngle(float angle);
	float getMinAngle() const;
	void setMaxAngle(float angle);
	float getMaxAngle() const;
};
typedef Ptr<JointLimitInfoTwist> JointLimitInfoTwistPtr;
typedef Ptr<const JointLimitInfoTwist> ConstJointLimitInfoTwistPtr;
typedef const JointLimitInfoTwist ConstJointLimitInfoTwist;


class UNIGINE_API JointLimitInfoHingeTwist : public JointLimitInfo
{
public:
typedef const JointLimitInfoHingeTwist ConstJointLimitInfoHingeTwist;
	static bool convertible(JointLimitInfo *obj) { return obj && obj->getType() == JointLimitInfo::JOINT_LIMIT_INFO_HINGE_TWIST; }
	static Ptr<JointLimitInfoHingeTwist> create();
	void setForwardAxis(const Math::vec3 &axis);
	Math::vec3 getForwardAxis() const;
	void setHingeAxis(const Math::vec3 &axis);
	Math::vec3 getHingeAxis() const;
	void setHingeMinAngle(float angle);
	float getHingeMinAngle() const;
	void setHingeMaxAngle(float angle);
	float getHingeMaxAngle() const;
	void setTwistMinAngle(float angle);
	float getTwistMinAngle() const;
	void setTwistMaxAngle(float angle);
	float getTwistMaxAngle() const;
};
typedef Ptr<JointLimitInfoHingeTwist> JointLimitInfoHingeTwistPtr;
typedef Ptr<const JointLimitInfoHingeTwist> ConstJointLimitInfoHingeTwistPtr;
typedef const JointLimitInfoHingeTwist ConstJointLimitInfoHingeTwist;


class UNIGINE_API JointLimitInfoConeTwist : public JointLimitInfo
{
public:
typedef const JointLimitInfoConeTwist ConstJointLimitInfoConeTwist;
	static bool convertible(JointLimitInfo *obj) { return obj && obj->getType() == JointLimitInfo::JOINT_LIMIT_INFO_CONE_TWIST; }
	static Ptr<JointLimitInfoConeTwist> create();
	void setForwardAxis(const Math::vec3 &axis);
	Math::vec3 getForwardAxis() const;
	void setConeAxis(const Math::vec3 &axis);
	Math::vec3 getConeAxis() const;
	void setConeAngle(float angle);
	float getConeAngle() const;
	void setTwistMinAngle(float angle);
	float getTwistMinAngle() const;
	void setTwistMaxAngle(float angle);
	float getTwistMaxAngle() const;
};
typedef Ptr<JointLimitInfoConeTwist> JointLimitInfoConeTwistPtr;
typedef Ptr<const JointLimitInfoConeTwist> ConstJointLimitInfoConeTwistPtr;
typedef const JointLimitInfoConeTwist ConstJointLimitInfoConeTwist;


class UNIGINE_API JointLimitInfoConeAsymTwist : public JointLimitInfo
{
public:
typedef const JointLimitInfoConeAsymTwist ConstJointLimitInfoConeAsymTwist;
	static bool convertible(JointLimitInfo *obj) { return obj && obj->getType() == JointLimitInfo::JOINT_LIMIT_INFO_CONE_ASYM_TWIST; }
	static Ptr<JointLimitInfoConeAsymTwist> create();
	void setForwardAxis(const Math::vec3 &axis);
	Math::vec3 getForwardAxis() const;
	void setUpAxis(const Math::vec3 &axis);
	Math::vec3 getUpAxis() const;
	void setConeAxis(const Math::vec3 &axis);
	Math::vec3 getConeAxis() const;
	void setSwingLeftAngle(float angle);
	float getSwingLeftAngle() const;
	void setSwingRightAngle(float angle);
	float getSwingRightAngle() const;
	void setSwingUpAngle(float angle);
	float getSwingUpAngle() const;
	void setSwingDownAngle(float angle);
	float getSwingDownAngle() const;
	void setTwistMinAngle(float angle);
	float getTwistMinAngle() const;
	void setTwistMaxAngle(float angle);
	float getTwistMaxAngle() const;
};
typedef Ptr<JointLimitInfoConeAsymTwist> JointLimitInfoConeAsymTwistPtr;
typedef Ptr<const JointLimitInfoConeAsymTwist> ConstJointLimitInfoConeAsymTwistPtr;
typedef const JointLimitInfoConeAsymTwist ConstJointLimitInfoConeAsymTwist;


class UNIGINE_API JointLimitSetInfo : public APIInterface
{
public:
	typedef const JointLimitSetInfo ConstJointLimitSetInfo;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<JointLimitSetInfo> create();
	void addLimit(const Ptr<JointLimitInfo> &limit);
	int getNumLimits() const;
	Ptr<JointLimitInfo> getLimit(int num) const;
	void clearLimits();
};
typedef Ptr<JointLimitSetInfo> JointLimitSetInfoPtr;
typedef Ptr<const JointLimitSetInfo> ConstJointLimitSetInfoPtr;
typedef const JointLimitSetInfo ConstJointLimitSetInfo;


class UNIGINE_API IKInfoChain : public IKInfo
{
public:
typedef const IKInfoChain ConstIKInfoChain;
	static bool convertible(IKInfo *obj) { return obj && obj->getType() == IKInfo::IK_INFO_CHAIN; }
	static Ptr<IKInfoChain> create();

	enum LIMIT_SOURCE
	{
		LIMIT_SOURCE_NONE,
		LIMIT_SOURCE_POLE,
		LIMIT_SOURCE_JOINT,
	};
	void setNumJoints(int joints);
	int getNumJoints() const;
	void setJoint(int num, int joint);
	int getJoint(int num) const;
	void setNumIterations(int iterations);
	int getNumIterations() const;
	void setDistanceTolerance(float tolerance);
	float getDistanceTolerance() const;
	void setRestartAttempts(int attempts);
	int getRestartAttempts() const;
	void resetSolveState();
	void setLimitSource(IKInfoChain::LIMIT_SOURCE source);
	IKInfoChain::LIMIT_SOURCE getLimitSource() const;
	void setLimitSet(const Ptr<JointLimitSetInfo> &set);
	Ptr<JointLimitSetInfo> getLimitSet() const;
};
typedef Ptr<IKInfoChain> IKInfoChainPtr;
typedef Ptr<const IKInfoChain> ConstIKInfoChainPtr;
typedef const IKInfoChain ConstIKInfoChain;


class UNIGINE_API LookAtChainInfo : public APIInterface
{
public:
	typedef const LookAtChainInfo ConstLookAtChainInfo;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<LookAtChainInfo> create();
	void setNumJoints(int joints);
	int getNumJoints() const;
	void setJoint(int num, int joint);
	int getJoint(int num) const;
	void setJointForwardAxis(int num, const Math::vec3 &forward_axis);
	Math::vec3 getJointForwardAxis(int num) const;
	void setJointUpAxis(int num, const Math::vec3 &up_axis);
	Math::vec3 getJointUpAxis(int num) const;
	void setJointWeight(int num, float weight);
	float getJointWeight(int num) const;
	void setTarget(const Math::vec3 &target);
	Math::vec3 getTarget() const;
	void setPole(const Math::vec3 &pole);
	Math::vec3 getPole() const;
	void setUsePole(bool pole);
	bool isUsePole() const;
	void setNumIterations(int iterations);
	int getNumIterations() const;
	void setAngleTolerance(float tolerance);
	float getAngleTolerance() const;
	void setMaxAngle(float angle);
	float getMaxAngle() const;
	void setWeight(float weight);
	float getWeight() const;
	void setLimitSet(const Ptr<JointLimitSetInfo> &set);
	Ptr<JointLimitSetInfo> getLimitSet() const;
};
typedef Ptr<LookAtChainInfo> LookAtChainInfoPtr;
typedef Ptr<const LookAtChainInfo> ConstLookAtChainInfoPtr;
typedef const LookAtChainInfo ConstLookAtChainInfo;

} // namespace Unigine
