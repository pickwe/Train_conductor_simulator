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
#include "UnigineAnimationParameters.h"
#include "UnigineNode.h"
#include "UnigineMaterial.h"
#include "UnigineXml.h"
#include "UnigineStreams.h"
#include "UnigineJson.h"
#include "UnigineWidgets.h"
#include "UnigineSkeleton.h"

namespace Unigine
{

class Gui;
class EngineWindow;
class Viewport;
class Body;
class Joint;
class Shape;
class Camera;
class LightLensFlare;
class ParticleModifier;
class AmbientSource;
class AnimScript;
class Property;
class RenderEnvironmentPreset;
class AnimationSequence;

class UNIGINE_API AnimationCurve : public APIInterface
{
public:
	typedef const AnimationCurve ConstAnimationCurve;
	static constexpr bool HAS_OWNERSHIP = true;

	static bool convertible(AnimationCurve *obj) { return obj != nullptr; }

	enum TYPE
	{
		ANIMATION_CURVE = 0,
		ANIMATION_CURVE_INT,
		ANIMATION_CURVE_FLOAT,
		ANIMATION_CURVE_DOUBLE,
		ANIMATION_CURVE_BOOL,
		ANIMATION_CURVE_SCALAR,
		ANIMATION_CURVE_QUAT,
		ANIMATION_CURVE_STRING,
		ANIMATION_CURVE_UGUID,
	};

	enum KEY_TYPE
	{
		KEY_TYPE_AUTO_FLAT = 0,
		KEY_TYPE_LINEAR,
		KEY_TYPE_SMOOTH,
		KEY_TYPE_ALIGNED,
		KEY_TYPE_BREAK,
		KEY_TYPE_CONSTANT,
		NUM_KEY_TYPES,
	};

	enum EXTRAPOLATION
	{
		EXTRAPOLATION_CONSTANT = 0,
		EXTRAPOLATION_LINEAR,
		EXTRAPOLATION_CYCLE,
		EXTRAPOLATION_PING_PONG,
		NUM_EXTRAPOLATION_TYPES,
	};
	static float wrapSourceTime(float t, float duration, AnimationCurve::EXTRAPOLATION pre_infinity, AnimationCurve::EXTRAPOLATION post_infinity);
	AnimationCurve::TYPE getType() const;
	const char *getTypeName() const;
};
typedef Ptr<AnimationCurve> AnimationCurvePtr;
typedef Ptr<const AnimationCurve> ConstAnimationCurvePtr;
typedef const AnimationCurve ConstAnimationCurve;


class UNIGINE_API AnimationCurveInt : public AnimationCurve
{
public:
typedef const AnimationCurveInt ConstAnimationCurveInt;
	static bool convertible(AnimationCurve *obj) { return obj && obj->getType() == AnimationCurve::ANIMATION_CURVE_INT; }
	static Ptr<AnimationCurveInt> create();
	void assignFrom(const Ptr<AnimationCurveInt> &curve);
	int getDefaultKeyValue() const;
	int addKey(float time, int value);
	int addKey(float time, int value, AnimationCurve::KEY_TYPE type);
	int addKey(float time, int value, AnimationCurve::KEY_TYPE type, const Math::vec2 &left_tangent, const Math::vec2 &right_tangent);
	void removeKey(int index);
	int moveKey(int index, float new_time);
	int getNumKeys() const;
	void setKeyType(int index, AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	AnimationCurve::KEY_TYPE getKeyType(int index) const;
	float getKeyTime(int index) const;
	void setKeyValue(int index, int value);
	int getKeyValue(int index) const;
	void setKeyLeftTangent(int index, const Math::vec2 &left_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyLeftTangent(int index) const;
	void setKeyRightTangent(int index, const Math::vec2 &right_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyRightTangent(int index) const;
	void setTypeOfAllKeys(AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	float getMinTime() const;
	float getMaxTime() const;
	void setPreInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPreInfinity() const;
	void setPostInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPostInfinity() const;
	int getValueByTime(float time);
	int getValueByNormalizedTime(float normalized_time);
	void clear();
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationCurveInt> AnimationCurveIntPtr;
typedef Ptr<const AnimationCurveInt> ConstAnimationCurveIntPtr;
typedef const AnimationCurveInt ConstAnimationCurveInt;


class UNIGINE_API AnimationCurveFloat : public AnimationCurve
{
public:
typedef const AnimationCurveFloat ConstAnimationCurveFloat;
	static bool convertible(AnimationCurve *obj) { return obj && obj->getType() == AnimationCurve::ANIMATION_CURVE_FLOAT; }
	static Ptr<AnimationCurveFloat> create();
	void assignFrom(const Ptr<AnimationCurveFloat> &curve);
	float getDefaultKeyValue() const;
	int addKey(float time, float value);
	int addKey(float time, float value, AnimationCurve::KEY_TYPE type);
	int addKey(float time, float value, AnimationCurve::KEY_TYPE type, const Math::vec2 &left_tangent, const Math::vec2 &right_tangent);
	void removeKey(int index);
	int moveKey(int index, float new_time);
	int getNumKeys() const;
	void setKeyType(int index, AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	AnimationCurve::KEY_TYPE getKeyType(int index) const;
	float getKeyTime(int index) const;
	void setKeyValue(int index, float value);
	float getKeyValue(int index) const;
	void setKeyLeftTangent(int index, const Math::vec2 &left_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyLeftTangent(int index) const;
	void setKeyRightTangent(int index, const Math::vec2 &right_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyRightTangent(int index) const;
	void setTypeOfAllKeys(AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	float getMinTime() const;
	float getMaxTime() const;
	void setPreInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPreInfinity() const;
	void setPostInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPostInfinity() const;
	float getValueByTime(float time);
	float getValueByNormalizedTime(float normalized_time);
	void clear();
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationCurveFloat> AnimationCurveFloatPtr;
typedef Ptr<const AnimationCurveFloat> ConstAnimationCurveFloatPtr;
typedef const AnimationCurveFloat ConstAnimationCurveFloat;


class UNIGINE_API AnimationCurveDouble : public AnimationCurve
{
public:
typedef const AnimationCurveDouble ConstAnimationCurveDouble;
	static bool convertible(AnimationCurve *obj) { return obj && obj->getType() == AnimationCurve::ANIMATION_CURVE_DOUBLE; }
	static Ptr<AnimationCurveDouble> create();
	void assignFrom(const Ptr<AnimationCurveDouble> &curve);
	double getDefaultKeyValue() const;
	int addKey(float time, double value);
	int addKey(float time, double value, AnimationCurve::KEY_TYPE type);
	int addKey(float time, double value, AnimationCurve::KEY_TYPE type, const Math::vec2 &left_tangent, const Math::vec2 &right_tangent);
	void removeKey(int index);
	int moveKey(int index, float new_time);
	int getNumKeys() const;
	void setKeyType(int index, AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	AnimationCurve::KEY_TYPE getKeyType(int index) const;
	float getKeyTime(int index) const;
	void setKeyValue(int index, double value);
	double getKeyValue(int index) const;
	void setKeyLeftTangent(int index, const Math::vec2 &left_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyLeftTangent(int index) const;
	void setKeyRightTangent(int index, const Math::vec2 &right_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyRightTangent(int index) const;
	void setTypeOfAllKeys(AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	float getMinTime() const;
	float getMaxTime() const;
	void setPreInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPreInfinity() const;
	void setPostInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPostInfinity() const;
	double getValueByTime(float time);
	double getValueByNormalizedTime(float normalized_time);
	void clear();
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationCurveDouble> AnimationCurveDoublePtr;
typedef Ptr<const AnimationCurveDouble> ConstAnimationCurveDoublePtr;
typedef const AnimationCurveDouble ConstAnimationCurveDouble;


class UNIGINE_API AnimationCurveBool : public AnimationCurve
{
public:
typedef const AnimationCurveBool ConstAnimationCurveBool;
	static bool convertible(AnimationCurve *obj) { return obj && obj->getType() == AnimationCurve::ANIMATION_CURVE_BOOL; }
	static Ptr<AnimationCurveBool> create();
	void assignFrom(const Ptr<AnimationCurveBool> &curve);
	bool getDefaultKeyValue() const;
	int addKey(float time, bool value);
	int addKey(float time, bool value, AnimationCurve::KEY_TYPE type);
	int addKey(float time, bool value, AnimationCurve::KEY_TYPE type, const Math::vec2 &left_tangent, const Math::vec2 &right_tangent);
	void removeKey(int index);
	int moveKey(int index, float new_time);
	int getNumKeys() const;
	void setKeyType(int index, AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	AnimationCurve::KEY_TYPE getKeyType(int index) const;
	float getKeyTime(int index) const;
	void setKeyValue(int index, bool value);
	bool getKeyValue(int index) const;
	void setKeyLeftTangent(int index, const Math::vec2 &left_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyLeftTangent(int index) const;
	void setKeyRightTangent(int index, const Math::vec2 &right_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyRightTangent(int index) const;
	void setTypeOfAllKeys(AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	float getMinTime() const;
	float getMaxTime() const;
	void setPreInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPreInfinity() const;
	void setPostInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPostInfinity() const;
	bool getValueByTime(float time);
	bool getValueByNormalizedTime(float normalized_time);
	void clear();
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationCurveBool> AnimationCurveBoolPtr;
typedef Ptr<const AnimationCurveBool> ConstAnimationCurveBoolPtr;
typedef const AnimationCurveBool ConstAnimationCurveBool;


class UNIGINE_API AnimationCurveScalar : public AnimationCurve
{
public:
typedef const AnimationCurveScalar ConstAnimationCurveScalar;
	static bool convertible(AnimationCurve *obj) { return obj && obj->getType() == AnimationCurve::ANIMATION_CURVE_SCALAR; }
	static Ptr<AnimationCurveScalar> create();
	void assignFrom(const Ptr<AnimationCurveScalar> &curve);
	Math::Scalar getDefaultKeyValue() const;
	int addKey(float time, Math::Scalar value);
	int addKey(float time, Math::Scalar value, AnimationCurve::KEY_TYPE type);
	int addKey(float time, Math::Scalar value, AnimationCurve::KEY_TYPE type, const Math::vec2 &left_tangent, const Math::vec2 &right_tangent);
	void removeKey(int index);
	int moveKey(int index, float new_time);
	int getNumKeys() const;
	void setKeyType(int index, AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	AnimationCurve::KEY_TYPE getKeyType(int index) const;
	float getKeyTime(int index) const;
	void setKeyValue(int index, Math::Scalar value);
	Math::Scalar getKeyValue(int index) const;
	void setKeyLeftTangent(int index, const Math::vec2 &left_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyLeftTangent(int index) const;
	void setKeyRightTangent(int index, const Math::vec2 &right_tangent, float value_time_ratio = 1.0f);
	Math::vec2 getKeyRightTangent(int index) const;
	void setTypeOfAllKeys(AnimationCurve::KEY_TYPE type, float value_time_ratio = 1.0f);
	float getMinTime() const;
	float getMaxTime() const;
	void setPreInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPreInfinity() const;
	void setPostInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPostInfinity() const;
	Math::Scalar getValueByTime(float time);
	Math::Scalar getValueByNormalizedTime(float normalized_time);
	void clear();
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationCurveScalar> AnimationCurveScalarPtr;
typedef Ptr<const AnimationCurveScalar> ConstAnimationCurveScalarPtr;
typedef const AnimationCurveScalar ConstAnimationCurveScalar;


class UNIGINE_API AnimationCurveQuat : public AnimationCurve
{
public:
typedef const AnimationCurveQuat ConstAnimationCurveQuat;
	static bool convertible(AnimationCurve *obj) { return obj && obj->getType() == AnimationCurve::ANIMATION_CURVE_QUAT; }
	static Ptr<AnimationCurveQuat> create();
	void assignFrom(const Ptr<AnimationCurveQuat> &curve);
	Math::quat getDefaultKeyValue() const;
	int addKey(float time, const Math::quat &value);
	void removeKey(int index);
	int moveKey(int index, float new_time);
	int getNumKeys() const;
	float getKeyTime(int index) const;
	void setKeyValue(int index, const Math::quat &value);
	Math::quat getKeyValue(int index) const;
	float getMinTime() const;
	float getMaxTime() const;
	void setPreInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPreInfinity() const;
	void setPostInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPostInfinity() const;
	Math::quat getValueByTime(float time);
	Math::quat getValueByNormalizedTime(float normalized_time);
	void clear();
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationCurveQuat> AnimationCurveQuatPtr;
typedef Ptr<const AnimationCurveQuat> ConstAnimationCurveQuatPtr;
typedef const AnimationCurveQuat ConstAnimationCurveQuat;


class UNIGINE_API AnimationCurveString : public AnimationCurve
{
public:
typedef const AnimationCurveString ConstAnimationCurveString;
	static bool convertible(AnimationCurve *obj) { return obj && obj->getType() == AnimationCurve::ANIMATION_CURVE_STRING; }
	static Ptr<AnimationCurveString> create();
	void assignFrom(const Ptr<AnimationCurveString> &curve);
	String getDefaultKeyValue() const;
	int addKey(float time, const char *value);
	void removeKey(int index);
	int moveKey(int index, float new_time);
	int getNumKeys() const;
	float getKeyTime(int index) const;
	void setKeyValue(int index, const char *value);
	const char *getKeyValue(int index) const;
	float getMinTime() const;
	float getMaxTime() const;
	void setPreInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPreInfinity() const;
	void setPostInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPostInfinity() const;
	String getValueByTime(float time);
	String getValueByNormalizedTime(float normalized_time);
	void clear();
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationCurveString> AnimationCurveStringPtr;
typedef Ptr<const AnimationCurveString> ConstAnimationCurveStringPtr;
typedef const AnimationCurveString ConstAnimationCurveString;


class UNIGINE_API AnimationCurveUGUID : public AnimationCurve
{
public:
typedef const AnimationCurveUGUID ConstAnimationCurveUGUID;
	static bool convertible(AnimationCurve *obj) { return obj && obj->getType() == AnimationCurve::ANIMATION_CURVE_UGUID; }
	static Ptr<AnimationCurveUGUID> create();
	void assignFrom(const Ptr<AnimationCurveUGUID> &curve);
	UGUID getDefaultKeyValue() const;
	int addKey(float time, const UGUID &value);
	void removeKey(int index);
	int moveKey(int index, float new_time);
	int getNumKeys() const;
	float getKeyTime(int index) const;
	void setKeyValue(int index, const UGUID &value);
	UGUID getKeyValue(int index) const;
	float getMinTime() const;
	float getMaxTime() const;
	void setPreInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPreInfinity() const;
	void setPostInfinity(AnimationCurve::EXTRAPOLATION infinity);
	AnimationCurve::EXTRAPOLATION getPostInfinity() const;
	UGUID getValueByTime(float time);
	UGUID getValueByNormalizedTime(float normalized_time);
	void clear();
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationCurveUGUID> AnimationCurveUGUIDPtr;
typedef Ptr<const AnimationCurveUGUID> ConstAnimationCurveUGUIDPtr;
typedef const AnimationCurveUGUID ConstAnimationCurveUGUID;

class AnimationChannelInfo;
class AnimationBind;

class UNIGINE_API AnimationChannel : public APIInterface
{
public:
	typedef const AnimationChannel ConstAnimationChannel;
	static constexpr bool HAS_OWNERSHIP = true;

	static bool convertible(AnimationChannel *obj) { return obj != nullptr; }

	enum TYPE
	{
		ANIMATION_CHANNEL = 0,
		ANIMATION_CHANNEL_INT,
		ANIMATION_CHANNEL_BOOL,
		ANIMATION_CHANNEL_QUAT,
		ANIMATION_CHANNEL_SCALAR,
		ANIMATION_CHANNEL_FLOAT,
		ANIMATION_CHANNEL_DOUBLE,
		ANIMATION_CHANNEL_STRING,
		ANIMATION_CHANNEL_UGUID,
		ANIMATION_CHANNEL_IVEC2,
		ANIMATION_CHANNEL_IVEC3,
		ANIMATION_CHANNEL_IVEC4,
		ANIMATION_CHANNEL_VEC2,
		ANIMATION_CHANNEL_FVEC2,
		ANIMATION_CHANNEL_DVEC2,
		ANIMATION_CHANNEL_VEC3,
		ANIMATION_CHANNEL_FVEC3,
		ANIMATION_CHANNEL_DVEC3,
		ANIMATION_CHANNEL_VEC4,
		ANIMATION_CHANNEL_FVEC4,
		ANIMATION_CHANNEL_DVEC4,
		ANIMATION_CHANNEL_NODE,
		ANIMATION_CHANNEL_BONES,
		ANIMATION_CHANNEL_EVENT,
		ANIMATION_CHANNEL_SKELETON_ANIMATION,
		ANIMATION_CHANNEL_SOUND,
		ANIMATION_CHANNEL_EVENT_STATE,
		ANIMATION_CHANNEL_SUB_SEQUENCE,
		ANIMATION_CHANNEL_FOLLOW_PATH,
		NUM_ANIMATION_CHANNELS,
	};

	enum EVENT_PHASE
	{
		EVENT_PHASE_POINT = 0,
		EVENT_PHASE_BEGIN,
		EVENT_PHASE_END,
	};
	Ptr<AnimationChannelInfo> getInfo() const;
	AnimationChannel::TYPE getType() const;
	const char *getTypeName() const;
	static int typeFromName(const char *name);
	AnimParams::PARAM getParam() const;
	int getParamIndex() const;
	void setParamName(const char *name);
	const char *getParamName() const;

	enum SLOT_ACCESS
	{
		SLOT_ACCESS_BY_INDEX = 0,
		SLOT_ACCESS_BY_NAME,
	};
	void setSlotAccess(AnimationChannel::SLOT_ACCESS access);
	AnimationChannel::SLOT_ACCESS getSlotAccess() const;
	void setCustomName(const char *name);
	const char *getCustomName() const;
	Ptr<AnimationBind> getBindCopy() const;
	void setBind(const Ptr<AnimationBind> &bind);
	int getBindType() const;
	float getMinTime() const;
	float getMaxTime() const;
	float getDuration() const;
	void setDisplayValueMin(float val);
	float getDisplayValueMin() const;
	void setDisplayValueMax(float val);
	float getDisplayValueMax() const;
	void setOrder(int order);
	int getOrder() const;
	void setRowGroup(int group);
	int getRowGroup() const;
	void setGroupID(int groupid);
	int getGroupID() const;
	void setID(int id);
	int getID() const;
	void setEnabled(bool enabled);
	bool isEnabled() const;

	enum PROPERTY
	{
		PROPERTY_PITCH = 0,
		PROPERTY_SOUND_TRIM_IN,
		PROPERTY_SOUND_DURATION,
		PROPERTY_SOUND_FADE_IN,
		PROPERTY_SOUND_FADE_OUT,
		NUM_PROPERTIES,
	};
	int getNumPropertyCurves() const;
	int getPropertyCurveKind(int index) const;
	Ptr<AnimationCurveFloat> getPropertyCurve(AnimationChannel::PROPERTY kind) const;
	void setPropertyCurve(AnimationChannel::PROPERTY kind, const Ptr<AnimationCurveFloat> &curve);
	void removePropertyCurve(AnimationChannel::PROPERTY kind);
	float getPropertyValueByTime(AnimationChannel::PROPERTY kind, float time) const;
	int getNumComponents() const;
	int getComponentPropertyKind(int component) const;
	int getComponentNumKeys(int component) const;
	float getComponentKeyTime(int component, int key) const;
	double getComponentKeyValue(int component, int key) const;
	double getComponentValueByTime(int component, float time) const;
	int getComponentKeyType(int component, int key) const;
	void setComponentKeyType(int component, int key, int type, float value_time_ratio = 1.0f);
	Math::vec2 getComponentKeyLeftTangent(int component, int key) const;
	Math::vec2 getComponentKeyRightTangent(int component, int key) const;
	void setComponentKeyLeftTangent(int component, int key, const Math::vec2 &tangent, float value_time_ratio = 1.0f);
	void setComponentKeyRightTangent(int component, int key, const Math::vec2 &tangent, float value_time_ratio = 1.0f);
	int addComponentKey(int component, float time, double value, int type);
	void removeComponentKey(int component, int key);
	int moveComponentKey(int component, int key, float new_time);
	void setComponentKeyValue(int component, int key, double value);
	int getComponentPreInfinity(int component) const;
	int getComponentPostInfinity(int component) const;
	void setComponentPreInfinity(int component, int mode);
	void setComponentPostInfinity(int component, int mode);
	bool isConstantSpeedSupported() const;
	void setConstantSpeed(bool enabled, bool force_update = false);
	bool isConstantSpeed() const;
	float getConstantSpeedTime(float time) const;
	UGUID getKeyValueUGUID(int key) const;
	void setKeyValueUGUID(int key, const UGUID &value);
	int addKeyValueUGUID(float time, const UGUID &value);
	UGUID getValueByTimeUGUID(float time) const;
	String getKeyValueString(int key) const;
	void setKeyValueString(int key, const char *value);
	int addKeyValueString(float time, const char *value);
	String getValueByTimeString(float time) const;
	int getNumClips() const;
	int addClip(float begin_time, float length, const char *content, int row = 0);
	void removeClip(int index);
	int getNumClipRows() const;
	float getClipBeginTime(int index) const;
	void setClipBeginTime(int index, float v);
	float getClipLength(int index) const;
	void setClipLength(int index, float v);
	float getClipIn(int index) const;
	void setClipIn(int index, float v);
	float getClipSpeedAt(int index, float local_time) const;
	int getClipRow(int index) const;
	void setClipRow(int index, int row);
	bool isClipEnabled(int index) const;
	void setClipEnabled(int index, bool enabled);
	int getClipPreInfinity(int index) const;
	void setClipPreInfinity(int index, int mode);
	int getClipPostInfinity(int index) const;
	void setClipPostInfinity(int index, int mode);
	Math::vec4 getClipColor(int index) const;
	void setClipColor(int index, const Math::vec4 &color);
	const char *getClipContent(int index) const;
	void setClipContent(int index, const char *content);
	const char *getClipContentName(int index) const;
	void setClipContentName(int index, const char *name);
	void setClipNode(int index, const Ptr<Node> &node);
	void setClipNodeDescription(int index, int node_id, const char *node_name);
	int getClipNodeID(int index) const;
	const char *getClipNodeName(int index) const;
	int getClipNodeAccess(int index) const;
	void setClipNodeAccess(int index, int access);
	int getClipLiveNodeID(int index) const;
	Ptr<AnimationSequence> getClipSequence(int index) const;
	void setClipSequence(int index, const Ptr<AnimationSequence> &sequence);
	UGUID getClipSlotGUID(int index) const;
	bool isClipEmbedded(int index) const;
	void setClipEmbedded(int index, bool embedded);
	Ptr<AnimationCurveFloat> getClipWeightCurve(int index) const;
	void setClipWeightCurve(int index, const Ptr<AnimationCurveFloat> &curve);
	Ptr<AnimationCurveFloat> getClipSpeedMultiplierCurve(int index) const;
	void setClipSpeedMultiplierCurve(int index, const Ptr<AnimationCurveFloat> &curve);
	Ptr<AnimationCurveFloat> getClipPropertyCurve(int index, AnimationChannel::PROPERTY kind) const;
	void setClipPropertyCurve(int index, AnimationChannel::PROPERTY kind, const Ptr<AnimationCurveFloat> &curve);
	void removeClipPropertyCurve(int index, AnimationChannel::PROPERTY kind);
	float getClipPropertyValueByTime(int index, AnimationChannel::PROPERTY kind, float local_time) const;
	bool hasClipPropertyCurve(int index, AnimationChannel::PROPERTY kind) const;
	bool saveClipToBlob(int clip_index, const Ptr<Blob> &blob) const;
	int loadClipFromBlob(float begin_time, const Ptr<Blob> &blob);
	float nextClipBeginAfter(float after_begin, int row, float none_value) const;
	void refreshTimesFromClips();

	enum RESOLVE_STATE
	{
		RESOLVED = 0,
		PARAM_MISSING,
		TYPE_MISMATCH,
		ENUM_DRIFT,
	};
	AnimationChannel::RESOLVE_STATE getResolveState() const;
	bool isResolved() const;
	void setAuthoredTypeName(const char *name);
	const char *getAuthoredTypeName() const;
	void setAuthoredEnumItems(const char *items);
	const char *getAuthoredEnumItems() const;
	void setAuthoredTitle(const char *title);
	const char *getAuthoredTitle() const;
	void setAuthoredParamName(const char *name);
	const char *getAuthoredParamName() const;
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationChannel> AnimationChannelPtr;
typedef Ptr<const AnimationChannel> ConstAnimationChannelPtr;
typedef const AnimationChannel ConstAnimationChannel;


class UNIGINE_API AnimationChannelInfo : public APIInterface
{
public:
	typedef const AnimationChannelInfo ConstAnimationChannelInfo;
	static constexpr bool HAS_OWNERSHIP = true;

	AnimationChannel::TYPE getType() const;
	void setParam(AnimParams::PARAM param);
	AnimParams::PARAM getParam() const;
	void setParamIndex(int index);
	int getParamIndex() const;
	void setParamName(const char *name);
	const char *getParamName() const;
	void setSlotAccess(AnimationChannel::SLOT_ACCESS access);
	AnimationChannel::SLOT_ACCESS getSlotAccess() const;
};
typedef Ptr<AnimationChannelInfo> AnimationChannelInfoPtr;
typedef Ptr<const AnimationChannelInfo> ConstAnimationChannelInfoPtr;
typedef const AnimationChannelInfo ConstAnimationChannelInfo;


class UNIGINE_API AnimationChannelInt : public AnimationChannel
{
public:
typedef const AnimationChannelInt ConstAnimationChannelInt;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_INT; }
	static Ptr<AnimationChannelInt> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelInt> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelInt> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelInt> create(const char *param);
	static Ptr<AnimationChannelInt> create(const char *param, int param_index);
	static Ptr<AnimationChannelInt> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelInt> &channel);
	int getDefaultValue() const;
	int getValueByTime(float time);
	int getValueByNormalizedTime(float normalized_time);
	void addValue(float time, int value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurve(const Ptr<AnimationCurveInt> &curve);
	Ptr<AnimationCurveInt> getCurve() const;
};
typedef Ptr<AnimationChannelInt> AnimationChannelIntPtr;
typedef Ptr<const AnimationChannelInt> ConstAnimationChannelIntPtr;
typedef const AnimationChannelInt ConstAnimationChannelInt;


class UNIGINE_API AnimationChannelBool : public AnimationChannel
{
public:
typedef const AnimationChannelBool ConstAnimationChannelBool;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_BOOL; }
	static Ptr<AnimationChannelBool> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelBool> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelBool> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelBool> create(const char *param);
	static Ptr<AnimationChannelBool> create(const char *param, int param_index);
	static Ptr<AnimationChannelBool> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelBool> &channel);
	bool getDefaultValue() const;
	bool getValueByTime(float time);
	bool getValueByNormalizedTime(float normalized_time);
	void addValue(float time, bool value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurve(const Ptr<AnimationCurveBool> &curve);
	Ptr<AnimationCurveBool> getCurve() const;
};
typedef Ptr<AnimationChannelBool> AnimationChannelBoolPtr;
typedef Ptr<const AnimationChannelBool> ConstAnimationChannelBoolPtr;
typedef const AnimationChannelBool ConstAnimationChannelBool;


class UNIGINE_API AnimationChannelQuat : public AnimationChannel
{
public:
typedef const AnimationChannelQuat ConstAnimationChannelQuat;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_QUAT; }

	enum MODE
	{
		MODE_QUAT = 0,
		MODE_ANGLES_XYZ,
		MODE_ANGLES_ZYX,
	};
	static Ptr<AnimationChannelQuat> create(AnimationChannelQuat::MODE mode, AnimParams::PARAM param);
	static Ptr<AnimationChannelQuat> create(AnimationChannelQuat::MODE mode, AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelQuat> create(AnimationChannelQuat::MODE mode, AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelQuat> create(AnimationChannelQuat::MODE mode, const char *param);
	static Ptr<AnimationChannelQuat> create(AnimationChannelQuat::MODE mode, const char *param, int param_index);
	static Ptr<AnimationChannelQuat> create(AnimationChannelQuat::MODE mode, const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelQuat> &channel);
	void setMode(AnimationChannelQuat::MODE mode);
	AnimationChannelQuat::MODE getMode() const;
	Math::quat getDefaultValue() const;
	Math::quat getValueByTime(float time);
	Math::quat getValueByNormalizedTime(float normalized_time);
	void addQuatValue(float time, const Math::quat &value);
	void addAnglesValue(float time, const Math::vec3 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setQuatCurve(const Ptr<AnimationCurveQuat> &curve);
	Ptr<AnimationCurveQuat> getQuatCurve() const;
	void setCurveX(const Ptr<AnimationCurveFloat> &curvex);
	Ptr<AnimationCurveFloat> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveFloat> &curvey);
	Ptr<AnimationCurveFloat> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveFloat> &curvez);
	Ptr<AnimationCurveFloat> getCurveZ() const;
};
typedef Ptr<AnimationChannelQuat> AnimationChannelQuatPtr;
typedef Ptr<const AnimationChannelQuat> ConstAnimationChannelQuatPtr;
typedef const AnimationChannelQuat ConstAnimationChannelQuat;


class UNIGINE_API AnimationChannelScalar : public AnimationChannel
{
public:
typedef const AnimationChannelScalar ConstAnimationChannelScalar;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_SCALAR; }
	static Ptr<AnimationChannelScalar> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelScalar> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelScalar> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelScalar> create(const char *param);
	static Ptr<AnimationChannelScalar> create(const char *param, int param_index);
	static Ptr<AnimationChannelScalar> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelScalar> &channel);
	Math::Scalar getDefaultValue() const;
	Math::Scalar getValueByTime(float time);
	Math::Scalar getValueByNormalizedTime(float normalized_time);
	void addValue(float time, Math::Scalar value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurve(const Ptr<AnimationCurveScalar> &curve);
	Ptr<AnimationCurveScalar> getCurve() const;
};
typedef Ptr<AnimationChannelScalar> AnimationChannelScalarPtr;
typedef Ptr<const AnimationChannelScalar> ConstAnimationChannelScalarPtr;
typedef const AnimationChannelScalar ConstAnimationChannelScalar;


class UNIGINE_API AnimationChannelFloat : public AnimationChannel
{
public:
typedef const AnimationChannelFloat ConstAnimationChannelFloat;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_FLOAT; }
	static Ptr<AnimationChannelFloat> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelFloat> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelFloat> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelFloat> create(const char *param);
	static Ptr<AnimationChannelFloat> create(const char *param, int param_index);
	static Ptr<AnimationChannelFloat> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelFloat> &channel);
	float getDefaultValue() const;
	float getValueByTime(float time);
	float getValueByNormalizedTime(float normalized_time);
	void addValue(float time, float value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurve(const Ptr<AnimationCurveFloat> &curve);
	Ptr<AnimationCurveFloat> getCurve() const;
};
typedef Ptr<AnimationChannelFloat> AnimationChannelFloatPtr;
typedef Ptr<const AnimationChannelFloat> ConstAnimationChannelFloatPtr;
typedef const AnimationChannelFloat ConstAnimationChannelFloat;


class UNIGINE_API AnimationChannelDouble : public AnimationChannel
{
public:
typedef const AnimationChannelDouble ConstAnimationChannelDouble;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_DOUBLE; }
	static Ptr<AnimationChannelDouble> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelDouble> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelDouble> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelDouble> create(const char *param);
	static Ptr<AnimationChannelDouble> create(const char *param, int param_index);
	static Ptr<AnimationChannelDouble> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelDouble> &channel);
	double getDefaultValue() const;
	double getValueByTime(float time);
	double getValueByNormalizedTime(float normalized_time);
	void addValue(float time, double value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurve(const Ptr<AnimationCurveDouble> &curve);
	Ptr<AnimationCurveDouble> getCurve() const;
};
typedef Ptr<AnimationChannelDouble> AnimationChannelDoublePtr;
typedef Ptr<const AnimationChannelDouble> ConstAnimationChannelDoublePtr;
typedef const AnimationChannelDouble ConstAnimationChannelDouble;


class UNIGINE_API AnimationChannelString : public AnimationChannel
{
public:
typedef const AnimationChannelString ConstAnimationChannelString;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_STRING; }
	static Ptr<AnimationChannelString> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelString> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelString> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelString> create(const char *param);
	static Ptr<AnimationChannelString> create(const char *param, int param_index);
	static Ptr<AnimationChannelString> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelString> &channel);
	String getDefaultValue() const;
	String getValueByTime(float time);
	String getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const char *value);
	void setCurve(const Ptr<AnimationCurveString> &curve);
	Ptr<AnimationCurveString> getCurve() const;
};
typedef Ptr<AnimationChannelString> AnimationChannelStringPtr;
typedef Ptr<const AnimationChannelString> ConstAnimationChannelStringPtr;
typedef const AnimationChannelString ConstAnimationChannelString;


class UNIGINE_API AnimationChannelUGUID : public AnimationChannel
{
public:
typedef const AnimationChannelUGUID ConstAnimationChannelUGUID;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_UGUID; }
	static Ptr<AnimationChannelUGUID> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelUGUID> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelUGUID> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelUGUID> create(const char *param);
	static Ptr<AnimationChannelUGUID> create(const char *param, int param_index);
	static Ptr<AnimationChannelUGUID> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelUGUID> &channel);
	UGUID getDefaultValue() const;
	UGUID getValueByTime(float time);
	UGUID getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const UGUID &value);
	void setCurve(const Ptr<AnimationCurveUGUID> &curve);
	Ptr<AnimationCurveUGUID> getCurve() const;
};
typedef Ptr<AnimationChannelUGUID> AnimationChannelUGUIDPtr;
typedef Ptr<const AnimationChannelUGUID> ConstAnimationChannelUGUIDPtr;
typedef const AnimationChannelUGUID ConstAnimationChannelUGUID;


class UNIGINE_API AnimationChannelSkeletonAnimation : public AnimationChannel
{
public:
typedef const AnimationChannelSkeletonAnimation ConstAnimationChannelSkeletonAnimation;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_SKELETON_ANIMATION; }
	static Ptr<AnimationChannelSkeletonAnimation> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelSkeletonAnimation> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelSkeletonAnimation> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelSkeletonAnimation> create(const char *param);
	static Ptr<AnimationChannelSkeletonAnimation> create(const char *param, int param_index);
	static Ptr<AnimationChannelSkeletonAnimation> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelSkeletonAnimation> &channel);
};
typedef Ptr<AnimationChannelSkeletonAnimation> AnimationChannelSkeletonAnimationPtr;
typedef Ptr<const AnimationChannelSkeletonAnimation> ConstAnimationChannelSkeletonAnimationPtr;
typedef const AnimationChannelSkeletonAnimation ConstAnimationChannelSkeletonAnimation;


class UNIGINE_API AnimationChannelSound : public AnimationChannel
{
public:
typedef const AnimationChannelSound ConstAnimationChannelSound;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_SOUND; }
	static Ptr<AnimationChannelSound> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelSound> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelSound> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelSound> create(const char *param);
	static Ptr<AnimationChannelSound> create(const char *param, int param_index);
	static Ptr<AnimationChannelSound> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelSound> &channel);
};
typedef Ptr<AnimationChannelSound> AnimationChannelSoundPtr;
typedef Ptr<const AnimationChannelSound> ConstAnimationChannelSoundPtr;
typedef const AnimationChannelSound ConstAnimationChannelSound;


class UNIGINE_API AnimationChannelIVec2 : public AnimationChannel
{
public:
typedef const AnimationChannelIVec2 ConstAnimationChannelIVec2;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_IVEC2; }
	static Ptr<AnimationChannelIVec2> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelIVec2> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelIVec2> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelIVec2> create(const char *param);
	static Ptr<AnimationChannelIVec2> create(const char *param, int param_index);
	static Ptr<AnimationChannelIVec2> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelIVec2> &channel);
	Math::ivec2 getDefaultValue() const;
	Math::ivec2 getValueByTime(float time);
	Math::ivec2 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::ivec2 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveInt> &curvex);
	Ptr<AnimationCurveInt> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveInt> &curvey);
	Ptr<AnimationCurveInt> getCurveY() const;
};
typedef Ptr<AnimationChannelIVec2> AnimationChannelIVec2Ptr;
typedef Ptr<const AnimationChannelIVec2> ConstAnimationChannelIVec2Ptr;
typedef const AnimationChannelIVec2 ConstAnimationChannelIVec2;


class UNIGINE_API AnimationChannelIVec3 : public AnimationChannel
{
public:
typedef const AnimationChannelIVec3 ConstAnimationChannelIVec3;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_IVEC3; }
	static Ptr<AnimationChannelIVec3> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelIVec3> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelIVec3> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelIVec3> create(const char *param);
	static Ptr<AnimationChannelIVec3> create(const char *param, int param_index);
	static Ptr<AnimationChannelIVec3> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelIVec3> &channel);
	Math::ivec3 getDefaultValue() const;
	Math::ivec3 getValueByTime(float time);
	Math::ivec3 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::ivec3 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveInt> &curvex);
	Ptr<AnimationCurveInt> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveInt> &curvey);
	Ptr<AnimationCurveInt> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveInt> &curvez);
	Ptr<AnimationCurveInt> getCurveZ() const;
};
typedef Ptr<AnimationChannelIVec3> AnimationChannelIVec3Ptr;
typedef Ptr<const AnimationChannelIVec3> ConstAnimationChannelIVec3Ptr;
typedef const AnimationChannelIVec3 ConstAnimationChannelIVec3;


class UNIGINE_API AnimationChannelIVec4 : public AnimationChannel
{
public:
typedef const AnimationChannelIVec4 ConstAnimationChannelIVec4;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_IVEC4; }
	static Ptr<AnimationChannelIVec4> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelIVec4> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelIVec4> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelIVec4> create(const char *param);
	static Ptr<AnimationChannelIVec4> create(const char *param, int param_index);
	static Ptr<AnimationChannelIVec4> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelIVec4> &channel);
	Math::ivec4 getDefaultValue() const;
	Math::ivec4 getValueByTime(float time);
	Math::ivec4 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::ivec4 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveInt> &curvex);
	Ptr<AnimationCurveInt> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveInt> &curvey);
	Ptr<AnimationCurveInt> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveInt> &curvez);
	Ptr<AnimationCurveInt> getCurveZ() const;
	void setCurveW(const Ptr<AnimationCurveInt> &curvew);
	Ptr<AnimationCurveInt> getCurveW() const;
};
typedef Ptr<AnimationChannelIVec4> AnimationChannelIVec4Ptr;
typedef Ptr<const AnimationChannelIVec4> ConstAnimationChannelIVec4Ptr;
typedef const AnimationChannelIVec4 ConstAnimationChannelIVec4;


class UNIGINE_API AnimationChannelVec2 : public AnimationChannel
{
public:
typedef const AnimationChannelVec2 ConstAnimationChannelVec2;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_VEC2; }
	static Ptr<AnimationChannelVec2> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelVec2> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelVec2> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelVec2> create(const char *param);
	static Ptr<AnimationChannelVec2> create(const char *param, int param_index);
	static Ptr<AnimationChannelVec2> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelVec2> &channel);
	Math::Vec2 getDefaultValue() const;
	Math::Vec2 getValueByTime(float time);
	Math::Vec2 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::Vec2 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveScalar> &curvex);
	Ptr<AnimationCurveScalar> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveScalar> &curvey);
	Ptr<AnimationCurveScalar> getCurveY() const;
};
typedef Ptr<AnimationChannelVec2> AnimationChannelVec2Ptr;
typedef Ptr<const AnimationChannelVec2> ConstAnimationChannelVec2Ptr;
typedef const AnimationChannelVec2 ConstAnimationChannelVec2;


class UNIGINE_API AnimationChannelFVec2 : public AnimationChannel
{
public:
typedef const AnimationChannelFVec2 ConstAnimationChannelFVec2;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_FVEC2; }
	static Ptr<AnimationChannelFVec2> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelFVec2> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelFVec2> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelFVec2> create(const char *param);
	static Ptr<AnimationChannelFVec2> create(const char *param, int param_index);
	static Ptr<AnimationChannelFVec2> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelFVec2> &channel);
	Math::vec2 getDefaultValue() const;
	Math::vec2 getValueByTime(float time);
	Math::vec2 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::vec2 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveFloat> &curvex);
	Ptr<AnimationCurveFloat> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveFloat> &curvey);
	Ptr<AnimationCurveFloat> getCurveY() const;
};
typedef Ptr<AnimationChannelFVec2> AnimationChannelFVec2Ptr;
typedef Ptr<const AnimationChannelFVec2> ConstAnimationChannelFVec2Ptr;
typedef const AnimationChannelFVec2 ConstAnimationChannelFVec2;


class UNIGINE_API AnimationChannelDVec2 : public AnimationChannel
{
public:
typedef const AnimationChannelDVec2 ConstAnimationChannelDVec2;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_DVEC2; }
	static Ptr<AnimationChannelDVec2> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelDVec2> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelDVec2> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelDVec2> create(const char *param);
	static Ptr<AnimationChannelDVec2> create(const char *param, int param_index);
	static Ptr<AnimationChannelDVec2> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelDVec2> &channel);
	Math::dvec2 getDefaultValue() const;
	Math::dvec2 getValueByTime(float time);
	Math::dvec2 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::dvec2 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveDouble> &curvex);
	Ptr<AnimationCurveDouble> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveDouble> &curvey);
	Ptr<AnimationCurveDouble> getCurveY() const;
};
typedef Ptr<AnimationChannelDVec2> AnimationChannelDVec2Ptr;
typedef Ptr<const AnimationChannelDVec2> ConstAnimationChannelDVec2Ptr;
typedef const AnimationChannelDVec2 ConstAnimationChannelDVec2;


class UNIGINE_API AnimationChannelVec3 : public AnimationChannel
{
public:
typedef const AnimationChannelVec3 ConstAnimationChannelVec3;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_VEC3; }
	static Ptr<AnimationChannelVec3> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelVec3> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelVec3> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelVec3> create(const char *param);
	static Ptr<AnimationChannelVec3> create(const char *param, int param_index);
	static Ptr<AnimationChannelVec3> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelVec3> &channel);
	Math::Vec3 getDefaultValue() const;
	Math::Vec3 getValueByTime(float time);
	Math::Vec3 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::Vec3 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveScalar> &curvex);
	Ptr<AnimationCurveScalar> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveScalar> &curvey);
	Ptr<AnimationCurveScalar> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveScalar> &curvez);
	Ptr<AnimationCurveScalar> getCurveZ() const;
};
typedef Ptr<AnimationChannelVec3> AnimationChannelVec3Ptr;
typedef Ptr<const AnimationChannelVec3> ConstAnimationChannelVec3Ptr;
typedef const AnimationChannelVec3 ConstAnimationChannelVec3;


class UNIGINE_API AnimationChannelFollowPath : public AnimationChannel
{
public:
typedef const AnimationChannelFollowPath ConstAnimationChannelFollowPath;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_FOLLOW_PATH; }
	static Ptr<AnimationChannelFollowPath> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelFollowPath> create(AnimParams::PARAM param, int param_index);
	void assignFrom(const Ptr<AnimationChannelFollowPath> &channel);
	void addValue(float time, const Math::Vec3 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveScalar> &curvex);
	Ptr<AnimationCurveScalar> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveScalar> &curvey);
	Ptr<AnimationCurveScalar> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveScalar> &curvez);
	Ptr<AnimationCurveScalar> getCurveZ() const;
	void setForwardAxis(MathLib::AXIS axis);
	MathLib::AXIS getForwardAxis() const;
	void setUp(const Math::vec3 &up);
	Math::vec3 getUp() const;
	void setLookahead(float lookahead);
	float getLookahead() const;
};
typedef Ptr<AnimationChannelFollowPath> AnimationChannelFollowPathPtr;
typedef Ptr<const AnimationChannelFollowPath> ConstAnimationChannelFollowPathPtr;
typedef const AnimationChannelFollowPath ConstAnimationChannelFollowPath;


class UNIGINE_API AnimationChannelFVec3 : public AnimationChannel
{
public:
typedef const AnimationChannelFVec3 ConstAnimationChannelFVec3;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_FVEC3; }
	static Ptr<AnimationChannelFVec3> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelFVec3> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelFVec3> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelFVec3> create(const char *param);
	static Ptr<AnimationChannelFVec3> create(const char *param, int param_index);
	static Ptr<AnimationChannelFVec3> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelFVec3> &channel);
	Math::vec3 getDefaultValue() const;
	Math::vec3 getValueByTime(float time);
	Math::vec3 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::vec3 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveFloat> &curvex);
	Ptr<AnimationCurveFloat> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveFloat> &curvey);
	Ptr<AnimationCurveFloat> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveFloat> &curvez);
	Ptr<AnimationCurveFloat> getCurveZ() const;
};
typedef Ptr<AnimationChannelFVec3> AnimationChannelFVec3Ptr;
typedef Ptr<const AnimationChannelFVec3> ConstAnimationChannelFVec3Ptr;
typedef const AnimationChannelFVec3 ConstAnimationChannelFVec3;


class UNIGINE_API AnimationChannelDVec3 : public AnimationChannel
{
public:
typedef const AnimationChannelDVec3 ConstAnimationChannelDVec3;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_DVEC3; }
	static Ptr<AnimationChannelDVec3> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelDVec3> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelDVec3> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelDVec3> create(const char *param);
	static Ptr<AnimationChannelDVec3> create(const char *param, int param_index);
	static Ptr<AnimationChannelDVec3> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelDVec3> &channel);
	Math::dvec3 getDefaultValue() const;
	Math::dvec3 getValueByTime(float time);
	Math::dvec3 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::dvec3 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveDouble> &curvex);
	Ptr<AnimationCurveDouble> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveDouble> &curvey);
	Ptr<AnimationCurveDouble> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveDouble> &curvez);
	Ptr<AnimationCurveDouble> getCurveZ() const;
};
typedef Ptr<AnimationChannelDVec3> AnimationChannelDVec3Ptr;
typedef Ptr<const AnimationChannelDVec3> ConstAnimationChannelDVec3Ptr;
typedef const AnimationChannelDVec3 ConstAnimationChannelDVec3;


class UNIGINE_API AnimationChannelVec4 : public AnimationChannel
{
public:
typedef const AnimationChannelVec4 ConstAnimationChannelVec4;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_VEC4; }
	static Ptr<AnimationChannelVec4> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelVec4> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelVec4> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelVec4> create(const char *param);
	static Ptr<AnimationChannelVec4> create(const char *param, int param_index);
	static Ptr<AnimationChannelVec4> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelVec4> &channel);
	Math::Vec4 getDefaultValue() const;
	Math::Vec4 getValueByTime(float time);
	Math::Vec4 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::Vec4 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveScalar> &curvex);
	Ptr<AnimationCurveScalar> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveScalar> &curvey);
	Ptr<AnimationCurveScalar> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveScalar> &curvez);
	Ptr<AnimationCurveScalar> getCurveZ() const;
	void setCurveW(const Ptr<AnimationCurveScalar> &curvew);
	Ptr<AnimationCurveScalar> getCurveW() const;
};
typedef Ptr<AnimationChannelVec4> AnimationChannelVec4Ptr;
typedef Ptr<const AnimationChannelVec4> ConstAnimationChannelVec4Ptr;
typedef const AnimationChannelVec4 ConstAnimationChannelVec4;


class UNIGINE_API AnimationChannelFVec4 : public AnimationChannel
{
public:
typedef const AnimationChannelFVec4 ConstAnimationChannelFVec4;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_FVEC4; }
	static Ptr<AnimationChannelFVec4> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelFVec4> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelFVec4> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelFVec4> create(const char *param);
	static Ptr<AnimationChannelFVec4> create(const char *param, int param_index);
	static Ptr<AnimationChannelFVec4> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelFVec4> &channel);
	Math::vec4 getDefaultValue() const;
	Math::vec4 getValueByTime(float time);
	Math::vec4 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::vec4 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveFloat> &curvex);
	Ptr<AnimationCurveFloat> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveFloat> &curvey);
	Ptr<AnimationCurveFloat> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveFloat> &curvez);
	Ptr<AnimationCurveFloat> getCurveZ() const;
	void setCurveW(const Ptr<AnimationCurveFloat> &curvew);
	Ptr<AnimationCurveFloat> getCurveW() const;
};
typedef Ptr<AnimationChannelFVec4> AnimationChannelFVec4Ptr;
typedef Ptr<const AnimationChannelFVec4> ConstAnimationChannelFVec4Ptr;
typedef const AnimationChannelFVec4 ConstAnimationChannelFVec4;


class UNIGINE_API AnimationChannelDVec4 : public AnimationChannel
{
public:
typedef const AnimationChannelDVec4 ConstAnimationChannelDVec4;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_DVEC4; }
	static Ptr<AnimationChannelDVec4> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelDVec4> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelDVec4> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelDVec4> create(const char *param);
	static Ptr<AnimationChannelDVec4> create(const char *param, int param_index);
	static Ptr<AnimationChannelDVec4> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelDVec4> &channel);
	Math::dvec4 getDefaultValue() const;
	Math::dvec4 getValueByTime(float time);
	Math::dvec4 getValueByNormalizedTime(float normalized_time);
	void addValue(float time, const Math::dvec4 &value, AnimationCurve::KEY_TYPE type = AnimationCurve::KEY_TYPE_LINEAR);
	void setCurveX(const Ptr<AnimationCurveDouble> &curvex);
	Ptr<AnimationCurveDouble> getCurveX() const;
	void setCurveY(const Ptr<AnimationCurveDouble> &curvey);
	Ptr<AnimationCurveDouble> getCurveY() const;
	void setCurveZ(const Ptr<AnimationCurveDouble> &curvez);
	Ptr<AnimationCurveDouble> getCurveZ() const;
	void setCurveW(const Ptr<AnimationCurveDouble> &curvew);
	Ptr<AnimationCurveDouble> getCurveW() const;
};
typedef Ptr<AnimationChannelDVec4> AnimationChannelDVec4Ptr;
typedef Ptr<const AnimationChannelDVec4> ConstAnimationChannelDVec4Ptr;
typedef const AnimationChannelDVec4 ConstAnimationChannelDVec4;


class UNIGINE_API AnimationChannelNode : public AnimationChannel
{
public:
typedef const AnimationChannelNode ConstAnimationChannelNode;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_NODE; }
	static Ptr<AnimationChannelNode> create(AnimParams::PARAM param);
	static Ptr<AnimationChannelNode> create(AnimParams::PARAM param, int param_index);
	static Ptr<AnimationChannelNode> create(AnimParams::PARAM param, const char *param_name);
	static Ptr<AnimationChannelNode> create(const char *param);
	static Ptr<AnimationChannelNode> create(const char *param, int param_index);
	static Ptr<AnimationChannelNode> create(const char *param, const char *param_name);
	void assignFrom(const Ptr<AnimationChannelNode> &channel);
	Ptr<Node> getDefaultValue() const;
	Ptr<Node> getValueByTime(float time);
	Ptr<Node> getValueByNormalizedTime(float normalized_time);
	void addValue(float time, int node_id);
	void setCurveID(const Ptr<AnimationCurveInt> &curveid);
	Ptr<AnimationCurveInt> getCurveID() const;
};
typedef Ptr<AnimationChannelNode> AnimationChannelNodePtr;
typedef Ptr<const AnimationChannelNode> ConstAnimationChannelNodePtr;
typedef const AnimationChannelNode ConstAnimationChannelNode;


class UNIGINE_API AnimationChannelBones : public AnimationChannel
{
public:
typedef const AnimationChannelBones ConstAnimationChannelBones;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_BONES; }
	static Ptr<AnimationChannelBones> create();
	void assignFrom(const Ptr<AnimationChannelBones> &channel);
	void setNumBones(int bones);
	int getNumBones() const;
	int getValueByTime(float time, Vector<Math::mat4> &transforms);
	void addValue(float time, Vector<Math::mat4> &transforms);
	void setCurvePosX(int bone_index, const Ptr<AnimationCurveFloat> &in_curve);
	Ptr<AnimationCurveFloat> getCurvePosX(int bone_index) const;
	void setCurvePosY(int bone_index, const Ptr<AnimationCurveFloat> &in_curve);
	Ptr<AnimationCurveFloat> getCurvePosY(int bone_index) const;
	void setCurvePosZ(int bone_index, const Ptr<AnimationCurveFloat> &in_curve);
	Ptr<AnimationCurveFloat> getCurvePosZ(int bone_index) const;
	void setCurveRot(int bone_index, const Ptr<AnimationCurveQuat> &in_curve);
	Ptr<AnimationCurveQuat> getCurveRot(int bone_index) const;
	void setCurveScaleX(int bone_index, const Ptr<AnimationCurveFloat> &in_curve);
	Ptr<AnimationCurveFloat> getCurveScaleX(int bone_index) const;
	void setCurveScaleY(int bone_index, const Ptr<AnimationCurveFloat> &in_curve);
	Ptr<AnimationCurveFloat> getCurveScaleY(int bone_index) const;
	void setCurveScaleZ(int bone_index, const Ptr<AnimationCurveFloat> &in_curve);
	Ptr<AnimationCurveFloat> getCurveScaleZ(int bone_index) const;
};
typedef Ptr<AnimationChannelBones> AnimationChannelBonesPtr;
typedef Ptr<const AnimationChannelBones> ConstAnimationChannelBonesPtr;
typedef const AnimationChannelBones ConstAnimationChannelBones;


class UNIGINE_API AnimationChannelEvent : public AnimationChannel
{
public:
typedef const AnimationChannelEvent ConstAnimationChannelEvent;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_EVENT; }
	static Ptr<AnimationChannelEvent> create();
	void assignFrom(const Ptr<AnimationChannelEvent> &channel);
	int getDefaultValue() const;
	int getValueByTime(float time);
	int getValueByNormalizedTime(float normalized_time);
	void addValue(float time, int value);
	void setCurve(const Ptr<AnimationCurveInt> &curve);
	Ptr<AnimationCurveInt> getCurve() const;
};
typedef Ptr<AnimationChannelEvent> AnimationChannelEventPtr;
typedef Ptr<const AnimationChannelEvent> ConstAnimationChannelEventPtr;
typedef const AnimationChannelEvent ConstAnimationChannelEvent;


class UNIGINE_API AnimationChannelEventState : public AnimationChannel
{
public:
typedef const AnimationChannelEventState ConstAnimationChannelEventState;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_EVENT_STATE; }
	static Ptr<AnimationChannelEventState> create();
	void assignFrom(const Ptr<AnimationChannelEventState> &channel);
	int addInterval(float start, float end, int payload);
	void setInterval(int index, float start, float end, int payload);
	void removeInterval(int index);
	void clearIntervals();
	int getNumIntervals() const;
	float getIntervalStart(int index) const;
	float getIntervalEnd(int index) const;
	int getIntervalPayload(int index) const;
};
typedef Ptr<AnimationChannelEventState> AnimationChannelEventStatePtr;
typedef Ptr<const AnimationChannelEventState> ConstAnimationChannelEventStatePtr;
typedef const AnimationChannelEventState ConstAnimationChannelEventState;


class UNIGINE_API AnimationChannelSubSequence : public AnimationChannel
{
public:
typedef const AnimationChannelSubSequence ConstAnimationChannelSubSequence;
	static bool convertible(AnimationChannel *obj) { return obj && obj->getType() == AnimationChannel::ANIMATION_CHANNEL_SUB_SEQUENCE; }
	static Ptr<AnimationChannelSubSequence> create();
	void assignFrom(const Ptr<AnimationChannelSubSequence> &channel);
	int addSubSequence(float begin_time, float length, const UGUID &sequence_guid);
	int addEmbeddedSubSequence(float begin_time, float length, const Ptr<AnimationSequence> &sequence);
};
typedef Ptr<AnimationChannelSubSequence> AnimationChannelSubSequencePtr;
typedef Ptr<const AnimationChannelSubSequence> ConstAnimationChannelSubSequencePtr;
typedef const AnimationChannelSubSequence ConstAnimationChannelSubSequence;


class UNIGINE_API AnimationBind : public APIInterface
{
public:
	typedef const AnimationBind ConstAnimationBind;
	static constexpr bool HAS_OWNERSHIP = true;

	static bool convertible(AnimationBind *obj) { return obj != nullptr; }

	enum TYPE
	{
		ANIMATION_BIND = 0,
		ANIMATION_BIND_NODE,
		ANIMATION_BIND_PROPERTY_PARAMETER,
		ANIMATION_BIND_MATERIAL,
		ANIMATION_BIND_RUNTIME,
		ANIMATION_BIND_COMPONENT,
	};

	enum NODE_ACCESS
	{
		NODE_ACCESS_BY_ID = 0,
		NODE_ACCESS_BY_NAME,
		NODE_ACCESS_INNER_BY_NAME,
	};

	enum ASSET_ACCESS
	{
		ASSET_ACCESS_BY_GUID = 0,
		ASSET_ACCESS_BY_NAME,
	};

	enum QUERY_SCOPE
	{
		QUERY_SCOPE_ALL_NODES = 0,
		QUERY_SCOPE_SUBTREE_NODES,
		QUERY_SCOPE_NODES_FROM_REFERENCE,
		QUERY_SCOPE_ASSET_INHERITANCE,
	};

	enum MATCH_BY
	{
		MATCH_BY_TARGET = 0,
		MATCH_BY_NAME_PATTERN,
		MATCH_BY_PROPERTY,
		MATCH_BY_COMPONENT,
		MATCH_BY_NODE_TYPE,
	};

	enum NODE_TYPE_GROUP
	{
		NODE_TYPE_GROUP_OBJECT = 0,
		NODE_TYPE_GROUP_LIGHT,
		NODE_TYPE_GROUP_DECAL,
		NODE_TYPE_GROUP_WORLD,
		NODE_TYPE_GROUP_FIELD,
		NODE_TYPE_GROUP_PLAYER,
		NODE_TYPE_GROUP_PHYSICAL,
		NODE_TYPE_GROUP_NAVIGATION,
		NODE_TYPE_GROUP_OBSTACLE,
		NODE_TYPE_GROUP_SOUND,
	};
	AnimationBind::TYPE getType() const;
	const char *getTypeName() const;
	void setNumTargets(int targets);
	int getNumTargets() const;
	int addTarget(AnimationBind::MATCH_BY match_by);
	void removeTarget(int i);
	void moveTarget(int from, int to);
	void moveTargets(const Vector<int> &indices, int insert_before);
	int duplicateTarget(int src);
	AnimationBind::MATCH_BY getTargetMatchBy(int i) const;
	void setTargetMatchBy(int i, AnimationBind::MATCH_BY match_by);
	int getTargetNodeID(int i) const;
	const char *getTargetNodeName(int i) const;
	int getTargetRefInnerID(int i) const;
	UGUID getTargetAssetFileGUID(int i) const;
	UGUID getTargetAssetRuntimeGUID(int i) const;
	int getTargetLiveNodeID(int i) const;
	void setTargetNode(int i, const Ptr<Node> &node);
	void setTargetNodeDescription(int i, int id, const char *name);
	void setTargetRefInnerID(int i, int id);
	AnimationBind::NODE_ACCESS getTargetNodeAccess(int i) const;
	void setTargetNodeAccess(int i, AnimationBind::NODE_ACCESS access);
	void setTargetAssetFileGUID(int i, const UGUID &guid);
	void setTargetAsset(int i, const UGUID &runtime_guid, const UGUID &file_guid);
	AnimationBind::ASSET_ACCESS getTargetAssetAccess(int i) const;
	void setTargetAssetAccess(int i, AnimationBind::ASSET_ACCESS access);
	const char *getTargetAssetName(int i) const;
	void setTargetAssetName(int i, const char *name);
	AnimationBind::QUERY_SCOPE getTargetScope(int i) const;
	void setTargetScope(int i, AnimationBind::QUERY_SCOPE scope);
	void setTargetSubtreeRoot(int i, int id, const char *name);
	int getTargetSubtreeRootID(int i) const;
	const char *getTargetSubtreeRootName(int i) const;
	AnimationBind::NODE_ACCESS getTargetSubtreeRootAccess(int i) const;
	void setTargetSubtreeRootAccess(int i, AnimationBind::NODE_ACCESS access);
	int getTargetSubtreeRootLiveNodeID(int i) const;
	UGUID getTargetNodeReferenceGUID(int i) const;
	void setTargetNodeReferenceGUID(int i, const UGUID &guid);
	UGUID getTargetInheritRootGUID(int i) const;
	void setTargetInheritRootGUID(int i, const UGUID &guid);
	const char *getTargetNamePattern(int i) const;
	void setTargetNamePattern(int i, const char *pattern);
	UGUID getTargetPropertyGUID(int i) const;
	void setTargetPropertyGUID(int i, const UGUID &guid);
	UGUID getTargetComponentGUID(int i) const;
	void setTargetComponentGUID(int i, const UGUID &guid);
	int getTargetNodeType(int i) const;
	void setTargetNodeType(int i, int type);
	bool isTargetNodeTypeIsGroup(int i) const;
	void setTargetNodeTypeIsGroup(int i, bool v);
	int getNumTargetMatches(int i) const;
	const char *getTargetMatchName(int i, int j) const;
	int getNumTargetInheritanceMatches(int i) const;
	int getResolvedNodeID(int i = 0) const;
	UGUID getResolvedAssetGUID(int i = 0) const;
	void save(const Ptr<Blob> &blob) const;
	void load(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationBind> AnimationBindPtr;
typedef Ptr<const AnimationBind> ConstAnimationBindPtr;
typedef const AnimationBind ConstAnimationBind;


class UNIGINE_API AnimationBindNode : public AnimationBind
{
public:
typedef const AnimationBindNode ConstAnimationBindNode;
	static bool convertible(AnimationBind *obj) { return obj && obj->getType() == AnimationBind::ANIMATION_BIND_NODE; }
	static Ptr<AnimationBindNode> create();
	Ptr<Node> getTargetResolvedNode(int i) const;
	void setNodes(const Vector<Ptr<Node>> &nodes);
};
typedef Ptr<AnimationBindNode> AnimationBindNodePtr;
typedef Ptr<const AnimationBindNode> ConstAnimationBindNodePtr;
typedef const AnimationBindNode ConstAnimationBindNode;


class UNIGINE_API AnimationBindPropertyParameter : public AnimationBind
{
public:
typedef const AnimationBindPropertyParameter ConstAnimationBindPropertyParameter;
	static bool convertible(AnimationBind *obj) { return obj && obj->getType() == AnimationBind::ANIMATION_BIND_PROPERTY_PARAMETER; }

	enum ACCESS
	{
		ACCESS_UNKNOWN = -1,
		ACCESS_FROM_ASSET = 0,
		ACCESS_FROM_NODE,
		ACCESS_FROM_SURFACE,
	};
	static Ptr<AnimationBindPropertyParameter> create();
	void setAccess(AnimationBindPropertyParameter::ACCESS access);
	AnimationBindPropertyParameter::ACCESS getAccess() const;
	void setNodes(const Vector<Ptr<Node>> &nodes);
	void setAssets(const Vector<UGUID> &file_guids);
	void setNodePropertyDescription(const char *name, int index);
	const char *getNodePropertyDescriptionName() const;
	int getNodePropertyDescriptionIndex() const;
	void setSurfacePattern(const char *pattern);
	const char *getSurfacePattern() const;
	int getNumTargetSurfaceMatches(int i) const;
	void setParameterPath(const char *path);
	const char *getParameterPath() const;
	Ptr<Node> getTargetResolvedNode(int i) const;
	Ptr<Property> getTargetResolvedProperty(int i) const;
};
typedef Ptr<AnimationBindPropertyParameter> AnimationBindPropertyParameterPtr;
typedef Ptr<const AnimationBindPropertyParameter> ConstAnimationBindPropertyParameterPtr;
typedef const AnimationBindPropertyParameter ConstAnimationBindPropertyParameter;


class UNIGINE_API AnimationBindMaterial : public AnimationBind
{
public:
typedef const AnimationBindMaterial ConstAnimationBindMaterial;
	static bool convertible(AnimationBind *obj) { return obj && obj->getType() == AnimationBind::ANIMATION_BIND_MATERIAL; }

	enum ACCESS
	{
		ACCESS_UNKNOWN = -1,
		ACCESS_FROM_ASSET = 0,
		ACCESS_FROM_SURFACE,
	};
	static Ptr<AnimationBindMaterial> create();
	void setAccess(AnimationBindMaterial::ACCESS access);
	AnimationBindMaterial::ACCESS getAccess() const;
	void setObjects(const Vector<Ptr<Node>> &nodes);
	void setAssets(const Vector<UGUID> &file_guids);
	void setSurfacePattern(const char *pattern);
	const char *getSurfacePattern() const;
	int getNumTargetSurfaceMatches(int i) const;
	Ptr<Object> getTargetResolvedObject(int i) const;
};
typedef Ptr<AnimationBindMaterial> AnimationBindMaterialPtr;
typedef Ptr<const AnimationBindMaterial> ConstAnimationBindMaterialPtr;
typedef const AnimationBindMaterial ConstAnimationBindMaterial;


class UNIGINE_API AnimationBindRuntime : public AnimationBind
{
public:
typedef const AnimationBindRuntime ConstAnimationBindRuntime;
	static bool convertible(AnimationBind *obj) { return obj && obj->getType() == AnimationBind::ANIMATION_BIND_RUNTIME; }
	static Ptr<AnimationBindRuntime> create();
	void setWidget(const Ptr<Widget> &widget);
	Ptr<Widget> getWidget() const;
	void setGui(const Ptr<Gui> &gui);
	Ptr<Gui> getGui() const;
	void setWindow(const Ptr<EngineWindow> &window);
	Ptr<EngineWindow> getWindow() const;
	void setViewport(const Ptr<Viewport> &viewport);
	Ptr<Viewport> getViewport() const;
	void setBody(const Ptr<Body> &body);
	Ptr<Body> getBody() const;
	void setJoint(const Ptr<Joint> &joint);
	Ptr<Joint> getJoint() const;
	void setShape(const Ptr<Shape> &shape);
	Ptr<Shape> getShape() const;
	void setCamera(const Ptr<Camera> &camera);
	Ptr<Camera> getCamera() const;
	void setLensFlare(const Ptr<LightLensFlare> &flare);
	Ptr<LightLensFlare> getLensFlare() const;
	void setParticleModifier(const Ptr<ParticleModifier> &modifier);
	Ptr<ParticleModifier> getParticleModifier() const;
	void setAmbientSource(const Ptr<AmbientSource> &source);
	Ptr<AmbientSource> getAmbientSource() const;
	void setAnimScript(const Ptr<AnimScript> &script);
	Ptr<AnimScript> getAnimScript() const;
	void setSequence(const Ptr<AnimationSequence> &sequence);
	Ptr<AnimationSequence> getSequence() const;
	void setProperty(const Ptr<Property> &property);
	Ptr<Property> getProperty() const;
	void setRenderEnvironmentPreset(const Ptr<RenderEnvironmentPreset> &preset);
	Ptr<RenderEnvironmentPreset> getRenderEnvironmentPreset() const;
};
typedef Ptr<AnimationBindRuntime> AnimationBindRuntimePtr;
typedef Ptr<const AnimationBindRuntime> ConstAnimationBindRuntimePtr;
typedef const AnimationBindRuntime ConstAnimationBindRuntime;


class UNIGINE_API AnimationBindComponent : public AnimationBind
{
public:
typedef const AnimationBindComponent ConstAnimationBindComponent;
	static bool convertible(AnimationBind *obj) { return obj && obj->getType() == AnimationBind::ANIMATION_BIND_COMPONENT; }

	enum KIND
	{
		KIND_UNKNOWN = -1,
		KIND_CPP = 0,
		KIND_CSHARP,
	};
	static Ptr<AnimationBindComponent> create();
	void setKind(AnimationBindComponent::KIND kind);
	AnimationBindComponent::KIND getKind() const;
	void setNodes(const Vector<Ptr<Node>> &nodes);
	void setComponentDescription(const UGUID &class_guid, int slot);
	UGUID getComponentClassGUID() const;
	int getComponentSlot() const;
	void setFieldPath(const char *path);
	const char *getFieldPath() const;
	void setAuthoredFieldType(const char *type);
	const char *getAuthoredFieldType() const;
	Ptr<Node> getTargetResolvedNode(int i) const;
	static AnimationBindComponent::KIND getComponentClassKind(const UGUID &property_guid);
	static UGUID findComponentClass(const UGUID &property_guid);
};
typedef Ptr<AnimationBindComponent> AnimationBindComponentPtr;
typedef Ptr<const AnimationBindComponent> ConstAnimationBindComponentPtr;
typedef const AnimationBindComponent ConstAnimationBindComponent;

class AnimationSequence;
class AnimationSequencePlayer;

class UNIGINE_API AnimationSequence : public APIInterface
{
public:
	typedef const AnimationSequence ConstAnimationSequence;
	static constexpr bool HAS_OWNERSHIP = true;


	enum EDITOR_SCENE_APPLY_MODE
	{
		EDITOR_SCENE_APPLY_MODE_ON_CHANGE = 0,
		EDITOR_SCENE_APPLY_MODE_ALWAYS_APPLY,
		EDITOR_SCENE_APPLY_MODE_NO_APPLY,
	};

	enum ORIGINALS_RESTORE_MODE
	{
		ORIGINALS_RESTORE_MODE_NEVER = 0,
		ORIGINALS_RESTORE_MODE_ON_STOP,
		ORIGINALS_RESTORE_MODE_EACH_FRAME,
	};
	static Ptr<AnimationSequence> create();
	void resolveAllChannels();
	UGUID getGUID() const;
	UGUID getFileGUID() const;
	void setName(const char *name);
	const char *getName() const;
	void assignFrom(const Ptr<AnimationSequence> &other, bool fresh_placement_identity = false);
	void addChannel(const Ptr<AnimationChannel> &channel);
	int getChannels(Vector<Ptr<AnimationChannel>> &out_channels) const;
	int getNumChannels() const;
	Ptr<AnimationChannel> findChannelByName(const char *name) const;
	int findChannelGroupByName(const char *name) const;
	int getChannelGroupChannels(int group_id, Vector<Ptr<AnimationChannel>> &out_channels) const;
	int getChannelGroupChildren(int parent_id, Vector<int> &out_group_ids) const;
	bool updateChannel(const Ptr<AnimationChannel> &channel);
	void removeChannel(const Ptr<AnimationChannel> &channel);
	Ptr<AnimationChannel> copyChannelByID(int channel_id) const;
	int addChannelGroup(int parent_id);
	void removeChannelGroup(int group_id, bool keep_children);
	int getNumChannelGroups() const;
	int getChannelGroupID(int index) const;
	bool containsChannelGroup(int group_id) const;
	int getChannelGroupParent(int group_id) const;
	void setChannelGroupParent(int group_id, int parent_id);
	int getChannelGroupOrder(int group_id) const;
	void setChannelGroupOrder(int group_id, int order);
	const char *getChannelGroupName(int group_id) const;
	void setChannelGroupName(int group_id, const char *name);
	bool isChannelGroupEnabled(int group_id) const;
	void setChannelGroupEnabled(int group_id, bool enabled);
	bool isChannelGroupCollapsed(int group_id) const;
	void setChannelGroupCollapsed(int group_id, bool collapsed);
	void setChannelGroupID(int channel_id, int group_id);
	void setChannelOrder(int channel_id, int order);
	void renumberDisplayOrder();
	void setManualDuration(float duration);
	float getManualDuration() const;
	void clear();
	float getDuration() const;
	void setSpeed(float speed);
	float getSpeed() const;
	void setLoop(bool loop);
	bool isLoop() const;
	void setDefaultOriginalsRestoreMode(AnimationSequence::ORIGINALS_RESTORE_MODE mode);
	AnimationSequence::ORIGINALS_RESTORE_MODE getDefaultOriginalsRestoreMode() const;
	void setEditorSceneApplyMode(AnimationSequence::EDITOR_SCENE_APPLY_MODE mode);
	AnimationSequence::EDITOR_SCENE_APPLY_MODE getEditorSceneApplyMode() const;
	void setEditorFps(float fps);
	float getEditorFps() const;
	void setEditorShowFrames(bool frames);
	bool isEditorShowFrames() const;
	void setEditorSnapEnabled(bool enabled);
	bool isEditorSnapEnabled() const;
	void setEditorSnapToFrames(bool frames);
	bool isEditorSnapToFrames() const;
	void setEditorSnapToOthers(bool others);
	bool isEditorSnapToOthers() const;
	void setEditorSnapToValueGrid(bool grid);
	bool isEditorSnapToValueGrid() const;
	void setEditorValueGridStep(float step);
	float getEditorValueGridStep() const;
	void setEditorRangeStart(float start);
	float getEditorRangeStart() const;
	void setEditorRangeEnd(float end);
	float getEditorRangeEnd() const;
	bool containsClipFileGUID(const UGUID &file_guid) const;
	void adoptEmbeddedClipSequences();
	void adoptEmbeddedChild(const Ptr<AnimationSequence> &child);
	void setPath(const char *path);
	const char *getPath() const;
	bool save();
	bool load();
	bool loadDataOnly();
	void saveToBlob(const Ptr<Blob> &blob);
	bool loadFromBlob(const Ptr<Blob> &blob);
};
typedef Ptr<AnimationSequence> AnimationSequencePtr;
typedef Ptr<const AnimationSequence> ConstAnimationSequencePtr;
typedef const AnimationSequence ConstAnimationSequence;

namespace AnimGraphRuntime
{
struct AnimGraphDebugContext;
}

class UNIGINE_API AnimScript : public APIInterface
{
public:
	typedef const AnimScript ConstAnimScript;
	static constexpr bool HAS_OWNERSHIP = false;


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
	bool isInit() const;
	UGUID getFileGUID() const;
	const char *getFilePath() const;
	int getNumParams() const;
	AnimScript::PARAM_TYPE getParamType(int index) const;
	const char *getParamName(int index) const;
	void resetParam(const char *name);
	void setParamInt(const char *name, int value);
	int getParamInt(const char *name) const;
	void setParamFloat(const char *name, float value);
	float getParamFloat(const char *name) const;
	void setParamBool(const char *name, bool value);
	bool getParamBool(const char *name) const;
	void setParamQuat(const char *name, const Math::quat &value);
	Math::quat getParamQuat(const char *name) const;
	void setParamVec2(const char *name, const Math::vec2 &value);
	Math::vec2 getParamVec2(const char *name) const;
	void setParamVec3(const char *name, const Math::vec3 &value);
	Math::vec3 getParamVec3(const char *name) const;
	void setParamVec4(const char *name, const Math::vec4 &value);
	Math::vec4 getParamVec4(const char *name) const;
	void setParamIVec2(const char *name, const Math::ivec2 &value);
	Math::ivec2 getParamIVec2(const char *name) const;
	void setParamIVec3(const char *name, const Math::ivec3 &value);
	Math::ivec3 getParamIVec3(const char *name) const;
	void setParamIVec4(const char *name, const Math::ivec4 &value);
	Math::ivec4 getParamIVec4(const char *name) const;
	void setParamMat3(const char *name, const Math::mat3 &value);
	Math::mat3 getParamMat3(const char *name) const;
	void setParamMat4(const char *name, const Math::mat4 &value);
	Math::mat4 getParamMat4(const char *name) const;
	void setParamAnimAsset(const char *name, const UGUID &guid);
	UGUID getParamAnimAsset(const char *name) const;
	void setParamTrigger(const char *name, bool value);
	bool getParamTrigger(const char *name) const;
	bool isActiveRootMotion() const;
	Math::Transform getRootMotionDelta() const;
	Math::vec3 getRootMotionDeltaPosition() const;
	Math::quat getRootMotionDeltaRotation() const;
	int getNumStateMachines() const;
	int findStateMachine(const char *path) const;
	int findStateMachineByName(const char *name) const;
	void setStateMachineState(int sm, int state, float duration);
	void setStateMachineStateAtTime(int sm, int state, float duration, float normalized_time);
	const char *getStateMachinePath(int sm) const;
	const char *getStateMachineName(int sm) const;
	int getStateMachineParent(int sm) const;
	const char *getStateMachineOwnerStateName(int sm) const;
	int getStateMachineNumStates(int sm) const;
	const char *getStateMachineStateName(int sm, int state) const;
	int findStateMachineState(int sm, const char *state_name) const;
	bool isStateMachineRelevant(int sm) const;
	int getStateMachineCurrentState(int sm) const;
	int getStateMachinePreviousState(int sm) const;
	const char *getStateMachineCurrentStateName(int sm) const;
	const char *getStateMachinePreviousStateName(int sm) const;
	bool isStateMachineInTransition(int sm) const;
	float getStateMachineStateTime(int sm) const;
	float getStateMachineTransitionProgress(int sm) const;
	float getStateMachineTransitionTime(int sm) const;
	float getStateMachineTransitionDuration(int sm) const;
	float getStateMachineAnimTime(int sm) const;
	float getStateMachineAnimTimeFraction(int sm) const;
	float getStateMachineAnimLength(int sm) const;
	float getStateMachineAnimTimeRemaining(int sm) const;
	float getStateMachineAnimTimeRemainingFraction(int sm) const;
	bool isStateMachineAnimEnded(int sm) const;
	int getStateMachineNumTransitionConditions(int sm) const;
	const char *getStateMachineTransitionConditionName(int sm, int index) const;
	void setDebugCollecting(bool collecting);
	bool isDebugCollecting() const;
	AnimGraphRuntime::AnimGraphDebugContext *getDebugContext();
	const AnimGraphRuntime::AnimGraphDebugContext *getDebugContext() const;
};
typedef Ptr<AnimScript> AnimScriptPtr;
typedef Ptr<const AnimScript> ConstAnimScriptPtr;
typedef const AnimScript ConstAnimScript;


class UNIGINE_API AnimationSequencePlayer : public APIInterface
{
public:
	typedef const AnimationSequencePlayer ConstAnimationSequencePlayer;
	static constexpr bool HAS_OWNERSHIP = true;

	static Ptr<AnimationSequencePlayer> create(const char *path);
	static Ptr<AnimationSequencePlayer> create(const UGUID & file_guid);
	static Ptr<AnimationSequencePlayer> create(const Ptr<AnimationSequence> &seq);
	void play();
	void pause();
	void stop();
	bool isPlaying() const;
	void setTime(float time);
	float getTime() const;
	void setTimeRegion(float from, float to);
	void setTimeFrom(float from);
	float getTimeFrom() const;
	void setTimeTo(float to);
	float getTimeTo() const;
	void setSpeed(float speed);
	float getSpeed() const;
	void resetSpeed();
	bool isSpeedCustom() const;
	void setLoop(bool loop);
	bool isLoop() const;
	void resetLoop();
	bool isLoopCustom() const;
	float getDuration() const;
	void setWeight(float weight);
	float getWeight() const;
	void applyCurrentFrame();
	void setAutoTick(bool tick);
	bool isAutoTick() const;
	void update(float dt);
	void setOriginalsRestoreMode(AnimationSequence::ORIGINALS_RESTORE_MODE mode);
	AnimationSequence::ORIGINALS_RESTORE_MODE getOriginalsRestoreMode() const;
	void resetOriginalsRestoreMode();
	bool hasSnapshot() const;
	void captureOriginals();
	void restoreOriginals();
	void clearSnapshot();
	void setProceduralMode(bool mode);
	bool isProceduralMode() const;
	void applySequenceProcedural(const Ptr<AnimationSequence> &seq);
	bool hasSequence() const;
	int getBindCount() const;
	int getBindChannelID(int index) const;
	unsigned long long getBindSequenceID(int index) const;
	const char *getBindChannelName(int index) const;
	const char *getBindPlacementPath(int index) const;
	AnimationBind::TYPE getBindType(int index) const;
	int findBind(const char *placement_path, const char *name) const;
	int findBindByName(const char *name) const;
	int findBindByChannelID(unsigned long long seq_id, int channel_id) const;
	void invalidateBinds();
	unsigned int getSequenceBindRevision() const;
	bool setBind(int index, const Ptr<AnimationBind> &bind);
	void resetBind(int index);
	void clearBinds();
	bool setBindComponent(int index, const Ptr<Node> &node, const Ptr<Property> &property);
	Ptr<AnimationBind> getActiveBind(int index) const;
	Ptr<AnimationBind> getDefaultBind(int index) const;
	bool setBindByKey(unsigned long long seq_id, int channel_id, const Ptr<AnimationBind> &bind);
	void resetBindByKey(unsigned long long seq_id, int channel_id);
	Ptr<AnimationBind> getActiveBindByKey(unsigned long long seq_id, int channel_id) const;
	Ptr<AnimationBind> getDefaultBindByKey(unsigned long long seq_id, int channel_id) const;
	Event<const char *, int, int, const char *> &getEventTriggered();
	Event<> &getEventFinished();
	int queryActiveEventIntervals(float time);
	const char *getActiveEventIntervalName(int i) const;
	int getActiveEventIntervalPayload(int i) const;
	const char *getActiveEventIntervalPlacement(int i) const;
	float getActiveEventIntervalStart(int i) const;
	float getActiveEventIntervalEnd(int i) const;
};
typedef Ptr<AnimationSequencePlayer> AnimationSequencePlayerPtr;
typedef Ptr<const AnimationSequencePlayer> ConstAnimationSequencePlayerPtr;
typedef const AnimationSequencePlayer ConstAnimationSequencePlayer;

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API Animations
{
public:
	static bool isInitialized();
	static void restoreEachFrame();
	static size_t getMemoryUsage();
	static void setComponentWritesCollecting(bool enabled);
	static void fetchComponentWrites(const Ptr<Blob> &blob);
	static void setManagedHasComponentFunc(int (*func)(int node_id, Unigine::UGUID class_guid));
	static void setSequencerMusicMuted(bool muted);
	static int getClasses(Vector<String> &out_classes);
	static int getBaseClasses(Vector<String> &out_classes);
	static int getSingletonClasses(Vector<String> &out_classes);
	static const char *getParentClass(const char *class_name);
	static int getDerivedClasses(const char *class_name, Vector<String> &out_classes);
	static int getClassParameters(const char *class_name, Vector<int> &out_parameters);
	static const char *getParameterClass(AnimParams::PARAM param);
	static AnimationBind::TYPE getParameterBindType(AnimParams::PARAM param);
	static bool isParameterTarget(AnimParams::PARAM param, const Ptr<Node> &node);
	static const char *getParameterName(AnimParams::PARAM param);
	static AnimationChannel::TYPE getParameterChannelType(AnimParams::PARAM param);
	static AnimParams::WIDGET getParameterWidget(AnimParams::PARAM param);
	static const char *getParameterItems(AnimParams::PARAM param);
	static const char *getParameterNodeType(AnimParams::PARAM param);
	static AnimParams::ACCESS getParameterAccess(AnimParams::PARAM param);
	static AnimParams::VEC_COMPONENT getParameterVecComponent(AnimParams::PARAM param);
	static const char *getParameterLogicalName(AnimParams::PARAM param);
	static const char *getParameterKeyName(AnimParams::PARAM param);
	static const char *getParameterTitle(AnimParams::PARAM param);
	static const char *getParameterMinValue(AnimParams::PARAM param);
	static const char *getParameterMaxValue(AnimParams::PARAM param);
	static const char *getParameterAssetExtension(AnimParams::PARAM param);
	static AnimParams::PARAM getParameterByName(const char *param_name);
	static AnimParams::PARAM getParameterByReflectionName(const char *class_name, const char *prop_name);
	static bool readParameterValueFloat(const Ptr<Node> &target, AnimParams::PARAM param, int param_index, int component, const char *param_name, float &out_value);
	static bool readChannelParameterValueFloat(const Ptr<AnimationChannel> &channel, int component, float &out_value);
	static String readChannelParameterValueString(const Ptr<AnimationChannel> &channel);
	static UGUID readChannelParameterValueUGUID(const Ptr<AnimationChannel> &channel);
	static Ptr<Node> readChannelParameterValueNode(const Ptr<AnimationChannel> &channel);
	static bool isChannelParameterReadable(const Ptr<AnimationChannel> &channel);
	static const char *getParameterSlotKindName(AnimParams::PARAM param);
	static int getChannelSlotCount(const Ptr<AnimationChannel> &channel);
	static int getChannelSlotNames(const Ptr<AnimationChannel> &channel, Vector<String> &out_names);
	static int getChannelSlotIndex(const Ptr<AnimationChannel> &channel, const char *name);
	static int getChannelSupportedProperties(AnimationChannel::TYPE channel_type, Vector<int> &out_kinds);
	static const char *getChannelPropertyName(AnimationChannel::PROPERTY kind);
	static float getChannelPropertyDefaultValue(AnimationChannel::PROPERTY kind);
	static float getChannelPropertyMinValue(AnimationChannel::PROPERTY kind);
	static float getChannelPropertyMaxValue(AnimationChannel::PROPERTY kind);
	static bool animToBonesChannel(const char *anim_path, const Ptr<AnimationChannelBones> &out_channel, float duration);
	static String convertLegacyTrackToSequence(const char *track_path, const char *seq_path = "", bool overwrite = false);
	static bool isAnimScriptsLoaded();
	static void rebuildAnimScripts();
	static bool isAnimScriptsRebuilding();
	static void waitAnimScriptsRebuilding();
	static bool isAnimScriptsSuccessBuild();
	static void setAnimScriptsPreviewBuild(bool build);
	static bool isAnimScriptsPreviewBuild();
	static int getNumAnimScriptTypes();
	static const char *getAnimScriptTypeName(int type_index);
	static Event<const char *, const char *, const char *> &getEventCustomLibBuild();
};

} // namespace Unigine
