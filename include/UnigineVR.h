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

#include "UnigineBase.h"
#include "UniginePtr.h"
#include "UnigineMathLib.h"
#include "UnigineCallback.h"
#include "UnigineMaterial.h"
#include "UnigineTextures.h"
#include "UnigineViewport.h"
#include "UnigineInput.h"

namespace Unigine
{

class VRMixedReality;
class VRHandTracking;
class VREyeTracking;
//////////////////////////////////////////////////////////////////////////

class UNIGINE_API VR
{
public:
	static bool isInitialized();

	enum API
	{
		API_NULL,
		API_VARJO,
		API_OPENVR,
		API_OPENXR,
	};

	enum RUNTIME_TYPE
	{
		RUNTIME_TYPE_UNKNOWN,
		RUNTIME_TYPE_NULL,
		RUNTIME_TYPE_STEAMVR,
		RUNTIME_TYPE_OCULUS,
		RUNTIME_TYPE_VARJO,
		RUNTIME_TYPE_WMR,
	};

	enum VIEWPORT_TYPE
	{
		VIEWPORT_TYPE_CONTEXT,
		VIEWPORT_TYPE_FOCUS,
		VIEWPORT_TYPE_NUM,
	};

	enum EYE_TYPE
	{
		EYE_TYPE_LEFT,
		EYE_TYPE_RIGHT,
		EYE_TYPE_NUM,
	};

	enum
	{
		TOTAL_VIEWPORTS = 4,
	};

	enum MIRROR_MODE
	{
		MIRROR_MODE_BEGIN,
		MIRROR_MODE_BLACK_SCREEN = MIRROR_MODE_BEGIN,
		MIRROR_MODE_MONO_LEFT,
		MIRROR_MODE_MONO_RIGHT,
		MIRROR_MODE_STEREO,
		MIRROR_MODE_END = MIRROR_MODE_STEREO,
	};

	enum WINDOW_MODE
	{
		WINDOW_MODE_BEGIN,
		WINDOW_MODE_DISABLED = WINDOW_MODE_BEGIN,
		WINDOW_MODE_MAIN,
		WINDOW_MODE_END = WINDOW_MODE_MAIN,
	};

	enum TRACKING_SPACE
	{
		TRACKING_SPACE_BEGIN,
		TRACKING_SPACE_LOCAL = TRACKING_SPACE_BEGIN,
		TRACKING_SPACE_ABSOLUTE,
		TRACKING_SPACE_END = TRACKING_SPACE_ABSOLUTE,
	};

	enum PERIPHERAL_RENDERING_DEBUG_GAZE_OVERRIDE_MODE
	{
		PERIPHERAL_RENDERING_DEBUG_GAZE_OVERRIDE_MODE_DISABLED,
		PERIPHERAL_RENDERING_DEBUG_GAZE_OVERRIDE_MODE_FIXED,
		PERIPHERAL_RENDERING_DEBUG_GAZE_OVERRIDE_MODE_MOUSE,
		PERIPHERAL_RENDERING_DEBUG_GAZE_OVERRIDE_MODE_COORD,
	};

	enum FOVEATED_MODE
	{
		FOVEATED_MODE_DISABLED,
		FOVEATED_MODE_FIXED,
		FOVEATED_MODE_DYNAMIC,
	};

	enum DEBUG_MODE
	{
		DEBUG_MODE_DISABLED,
		DEBUG_MODE_ENABLED,
		DEBUG_MODE_ENABLED_BREAK_ON_ERROR,
	};

	enum SHOW_PROFILER
	{
		SHOW_PROFILER_DISABLED = 0,
		SHOW_PROFILER_BASIC,
		SHOW_PROFILER_ADVANCED,
	};

	enum PROFILER_POSITION
	{
		PROFILER_POSITION_BOTTOM_LEFT = 1,
		PROFILER_POSITION_BOTTOM_CENTER,
		PROFILER_POSITION_BOTTOM_RIGHT,
		PROFILER_POSITION_CENTER_LEFT,
		PROFILER_POSITION_CENTER_CENTER,
		PROFILER_POSITION_CENTER_RIGHT,
		PROFILER_POSITION_TOP_LEFT,
		PROFILER_POSITION_TOP_CENTER,
		PROFILER_POSITION_TOP_RIGHT,
	};
	static void setDebugMode(VR::DEBUG_MODE mode);
	static VR::DEBUG_MODE getDebugMode();
	static void setRenderEnabled(bool enabled);
	static bool isRenderEnabled();
	static void setRenderWhileHMDIdle(bool idle);
	static bool isRenderWhileHMDIdle();
	static void setPeripheralRenderingModeEnabled(bool enabled);
	static bool isPeripheralRenderingModeEnabled();
	static void setPeripheralRenderingDebugEnabled(bool enabled);
	static bool isPeripheralRenderingDebugEnabled();
	static void setPeripheralRenderingDebugGazeOverrideMode(VR::PERIPHERAL_RENDERING_DEBUG_GAZE_OVERRIDE_MODE mode);
	static VR::PERIPHERAL_RENDERING_DEBUG_GAZE_OVERRIDE_MODE getPeripheralRenderingDebugGazeOverrideMode();
	static void setPeripheralRenderingDebugGazeOverrideCoord(const Math::vec2 &coord);
	static Math::vec2 getPeripheralRenderingDebugGazeOverrideCoord();
	static void setPeripheralRenderingBorderWidth(float width);
	static float getPeripheralRenderingBorderWidth();
	static void setPeripheralRenderingFOVScale(const Math::vec2 &scale);
	static Math::vec2 getPeripheralRenderingFOVScale();
	static void setPeripheralRenderingFocusScale(const Math::vec2 &scale);
	static Math::vec2 getPeripheralRenderingFocusScale();
	static void setPeripheralRenderingFocusDeadzone(float deadzone);
	static float getPeripheralRenderingFocusDeadzone();
	static void setMirrorMode(VR::MIRROR_MODE mode);
	static VR::MIRROR_MODE getMirrorMode();
	static void setMirrorCrop(float crop);
	static float getMirrorCrop();
	static void setMirrorCropOffset(const Math::vec2 &offset);
	static Math::vec2 getMirrorCropOffset();
	static void setEmulationMirrorMode(VR::MIRROR_MODE mode);
	static VR::MIRROR_MODE getEmulationMirrorMode();
	static void setEmulationMirrorCrop(float crop);
	static float getEmulationMirrorCrop();
	static void setEmulationMirrorCropOffset(const Math::vec2 &offset);
	static Math::vec2 getEmulationMirrorCropOffset();
	static void setShowProfiler(VR::SHOW_PROFILER profiler);
	static VR::SHOW_PROFILER getShowProfiler();
	static void setShowProfilerMisc(bool misc);
	static bool isShowProfilerMisc();
	static void setShowProfilerPerformance(bool performance);
	static bool isShowProfilerPerformance();
	static void setShowProfilerMemory(bool memory);
	static bool isShowProfilerMemory();
	static void setProfilerPosition(VR::PROFILER_POSITION position);
	static VR::PROFILER_POSITION getProfilerPosition();
	static void setProfilerBackgroundAlpha(int alpha);
	static int getProfilerBackgroundAlpha();
	static void setWindowMode(VR::WINDOW_MODE mode);
	static VR::WINDOW_MODE getWindowMode();
	static void setTrackingSpace(VR::TRACKING_SPACE space);
	static VR::TRACKING_SPACE getTrackingSpace();
	static void setMotionPrediction(bool prediction);
	static bool isMotionPrediction();
	static void setMotionPredictionVelocityPrecision(float precision);
	static float getMotionPredictionVelocityPrecision();
	static void setMotionPredictionVelocityTimeDelta(float delta);
	static float getMotionPredictionVelocityTimeDelta();
	static void setFoveatedRenderingEnabled(bool enabled);
	static bool isFoveatedRenderingEnabled();
	static void setFoveatedFixedEyeCenterBias(float bias);
	static float getFoveatedFixedEyeCenterBias();
	static const char *getApiName();
	static VR::API getApiType();
	static const char *getInputRuntimeName();
	static VR::RUNTIME_TYPE getInputRuntimeType();
	static const char *getInputRuntimeVersion();
	static float getHMDRefreshRate();
	static Math::ivec2 getHMDResolution(VR::VIEWPORT_TYPE viewport_type = VR::VIEWPORT_TYPE_CONTEXT);
	static Math::Vec3 getHandTrackingOffset();
	static bool isPeripheralRenderingModeUsed();
	static bool isHandTrackingOffsetSupported();
	static bool hasFeatureMixedReality();
	static bool hasFeatureHandTracking();
	static bool hasFeatureEyeTracking();
	static bool hasFeatureMotionPrediction();
	static bool hasFeatureFoveatedRendering();
	static bool hasFeatureReportProximitySensor();
	static bool hasFeatureSupportForTreadmill();
	static bool hasFeatureSupportForBaseStations();
	static bool hasFeatureSupportForTrackers();
	static bool hasFeatureSupportForRenderModel();
	static bool hasFeatureSupportForRenderModelComponents();
	static bool hasFeatureFadeToColor();
	static bool hasFeatureFadeGrid();
	static bool hasFeatureGetAudioDevice();
	static bool isHMDConnected();
	static bool isUsingFoveatedRendering();
	static Ptr<Viewport> getViewport();
	static bool isSteamVRDashboardActive();
	static Math::Mat4 getPlayerWorldTransform();
	static Math::Mat4 getPlayerModelview();
	static Math::Mat4 getPlayerIModelview();
	static VR::FOVEATED_MODE getFoveatedMode();
	static bool resetZeroPose();
	static void fadeToColor(float seconds, const Math::vec4 &color, bool background);
	static void fadeGrid(float seconds, bool fade_in);
	static Math::Mat4 getAbsoluteToLocalWorldTransform();
	static void clearScriptableMaterials(VR::EYE_TYPE eye);
	static int getNumScriptableMaterials(VR::EYE_TYPE eye);
	static Ptr<Material> getScriptableMaterial(VR::EYE_TYPE eye, int num);
	static void setScriptableMaterial(VR::EYE_TYPE eye, int num, const Ptr<Material> &material);
	static void insertScriptableMaterial(VR::EYE_TYPE eye, int num, const Ptr<Material> &material);
	static int findScriptableMaterial(VR::EYE_TYPE eye, const Ptr<Material> &material);
	static void addScriptableMaterial(VR::EYE_TYPE eye, const Ptr<Material> &material);
	static void removeScriptableMaterial(VR::EYE_TYPE eye, int num);
	static void swapScriptableMaterials(VR::EYE_TYPE eye, int num_0, int num_1);
	static void setScriptableMaterialEnabled(VR::EYE_TYPE eye, int num, bool enabled);
	static bool getScriptableMaterialEnabled(VR::EYE_TYPE eye, int num);
	static void setEyeOffset(VR::EYE_TYPE eye, const Math::mat4 &offset);
	static Math::mat4 getEyeOffset(VR::EYE_TYPE eye);
	static void resetEyeOffset(VR::EYE_TYPE eye);
	static void resetEyeOffset();
	static bool isEyeOverriddenFOV();
	static void setEyeOverrideFOV(float eyeoverridefov);
	static float getEyeOverrideFOV();
	static void resetEyeOverrideFOV();
	static float getEyeNativeFOV();
	static Event<int> &getEventDeviceRenderModelChanged();
	static Event<> &getEventAudioSettingsChanged();
	static Event<bool> &getEventRenderModelsVisibility();
};

} // namespace Unigine
