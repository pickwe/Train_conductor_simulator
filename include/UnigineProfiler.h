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

#include "UnigineMathLib.h"
#include "UnigineGui.h"
#include "UnigineWidgets.h"

namespace Unigine
{

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API Profiler
{
public:
	static bool isInitialized();

	enum COUNTER_VR_FLAG
	{
		COUNTER_VR_FLAG_DISABLED = 0,
		COUNTER_VR_FLAG_PERFORMANCE_ADVANCED = 1 << 1,
	};
	static void setEnabled(bool enabled);
	static bool isEnabled();
	static void begin(const char *name);
	static void begin(const char *name, const Math::vec4 &color);
	static float end();
	static void initThread(const char *name, int priority = 0);
	static void shutdownThread();
	static int beginMicro(const char *name, bool gpu = false);
	static void endMicro(int id);
	static float getValue(const char *name);
	static void setValue(const char *name, const char *units, int value, int max_value, float *arg5, Profiler::COUNTER_VR_FLAG vr_flags = Profiler::COUNTER_VR_FLAG_DISABLED, bool show_chart = true);
	static void setValue(const char *name, const char *units, float value, float max_value, float *arg5, Profiler::COUNTER_VR_FLAG vr_flags = Profiler::COUNTER_VR_FLAG_DISABLED, bool show_chart = true);
	static void startProfilerDump(const char *path = nullptr);
	static void startProfilerDump(int frames, const char *path = nullptr);
	static void startProfilerDump(float seconds, const char *path = nullptr);
	static bool isProfilerDumpEnabled();
	static void stopProfilerDump();
	static int getNumCounters();
	static int findCounter(const char *name);
	static const char *getCounterName(int num);
	static const char *getCounterText(int num);
	static Math::vec4 getCounterColor(int num);
	static float getCounterValue(int num);
	static long long getCounterFrame(int num);
	static bool isCounterActive(int num);
	static bool isCounterSeparator(int num);
	static const char *getMicroprofileUrl();
	static void setGui(const Ptr<Gui> &gui);
	static Ptr<Gui> getGui();
	static Event<> &getEventProfileDumpStart();
	static Event<> &getEventProfileDumpEnd();
};


struct ScopedProfiler
{
	int id{-1};
	ScopedProfiler(const char *name, bool gpu = false)
		: id(Profiler::isInitialized() ? Profiler::beginMicro(name, gpu) : -1)
	{}
	~ScopedProfiler()
	{
		if (id != -1)
			Profiler::endMicro(id);
	}
};
#define UNIGINE_PROFILER_SCOPED(NAME) ::Unigine::ScopedProfiler UNIGINE_CONCATENATE(unigine_prof, __LINE__)(NAME)
#define UNIGINE_PROFILER_SCOPED_GPU(NAME) ::Unigine::ScopedProfiler UNIGINE_CONCATENATE(unigine_prof, __LINE__)(NAME, true)
#define UNIGINE_PROFILER_FUNCTION ::Unigine::ScopedProfiler UNIGINE_CONCATENATE(unigine_prof, __LINE__)(__FUNCTION__)
#define UNIGINE_PROFILER_FUNCTION_GPU ::Unigine::ScopedProfiler UNIGINE_CONCATENATE(unigine_prof, __LINE__)(__FUNCTION__, true)


class UNIGINE_API ProfilerDump : public APIInterface
{
public:
	typedef const ProfilerDump ConstProfilerDump;
	static constexpr bool HAS_OWNERSHIP = false;


	enum DUMP_PARAMETER_TYPE
	{
		DUMP_PARAMETER_TYPE_INT,
		DUMP_PARAMETER_TYPE_FLOAT,
	};

	enum DUMP_CONSOLE_VARIABLE_TYPE
	{
		DUMP_CONSOLE_VARIABLE_TYPE_STRING,
		DUMP_CONSOLE_VARIABLE_TYPE_INT,
		DUMP_CONSOLE_VARIABLE_TYPE_FLOAT,
		DUMP_CONSOLE_VARIABLE_TYPE_VEC2,
		DUMP_CONSOLE_VARIABLE_TYPE_VEC3,
		DUMP_CONSOLE_VARIABLE_TYPE_VEC4,
		DUMP_CONSOLE_VARIABLE_TYPE_PALETTE,
	};
	static Ptr<ProfilerDump> create();
	bool load(const char *path);
	bool info(const char *path);
	int getDumpVersion() const;
	const char *getEngineVersion() const;
	const char *getEngineInfo() const;
	const char *getBinaryInfo() const;
	const char *getFeatures() const;
	const char *getRevisionInfo() const;
	const char *getOsInfo() const;
	const char *getCpuInfo() const;
	long long getTotalMemory() const;
	int getNumGPUs() const;
	const char *getGPUDescription(int num) const;
	const char *getGPUType(int num) const;
	const char *getGPUVendor(int num) const;
	const char *getGPUDriver(int num) const;
	int getGPUMemory(int num) const;
	bool getGPUCommonHeaps(int num) const;
	int getGPUPriority(int num) const;
	long long getGPULuid(int num) const;
	unsigned int getGPUDeviceId(int num) const;
	int getNumConsoleVariables() const;
	const char *getConsoleVariableName(int num) const;
	ProfilerDump::DUMP_CONSOLE_VARIABLE_TYPE getConsoleVariableType(int num) const;
	const char *getConsoleVariableValueString(int num) const;
	int getConsoleVariableValueInt(int num) const;
	float getConsoleVariableValueFloat(int num) const;
	Math::vec2 getConsoleVariableValueVec2(int num) const;
	Math::vec3 getConsoleVariableValueVec3(int num) const;
	Math::vec4 getConsoleVariableValueVec4(int num) const;
	Palette getConsoleVariableValuePalette(int num) const;
	int getNumConsoleVariableHistory(int num) const;
	long long getConsoleVariableHistoryFrame(int num, int h) const;
	const char *getConsoleVariableHistoryValueString(int num, int h) const;
	int getConsoleVariableHistoryValueInt(int num, int h) const;
	float getConsoleVariableHistoryValueFloat(int num, int h) const;
	Math::vec2 getConsoleVariableHistoryValueVec2(int num, int h) const;
	Math::vec3 getConsoleVariableHistoryValueVec3(int num, int h) const;
	Math::vec4 getConsoleVariableHistoryValueVec4(int num, int h) const;
	Palette getConsoleVariableHistoryValuePalette(int num, int h) const;
	long long getStartFrame() const;
	int getNumFrames() const;
	long long getFrame(int num) const;
	long long getFrameTimeMicroseconds(int num) const;
	double getFrameTimeSeconds(int num) const;
	double getFrameTimeMilliseconds(int num) const;
	int getNumFrameParameters(int frame) const;
	int getFrameParameterIndex(int frame, int num_parameter) const;
	int getFrameParameterValueInt(int frame, int num_parameter) const;
	float getFrameParameterValueFloat(int frame, int num_parameter) const;
	bool containsFrameParameterIndex(int frame, int paramter_index) const;
	int getNumDumpParameters() const;
	const char *getDumpParameterName(int num) const;
	const char *getDumpParameterUnits(int num) const;
	ProfilerDump::DUMP_PARAMETER_TYPE getDumpParameterType(int num) const;
	int getDumpParameterIntMin(int num) const;
	int getDumpParameterIntMax(int num) const;
	int getDumpParameterIntAvg(int num) const;
	float getDumpParameterFloatMin(int num) const;
	float getDumpParameterFloatMax(int num) const;
	float getDumpParameterFloatAvg(int num) const;
};
typedef Ptr<ProfilerDump> ProfilerDumpPtr;
typedef Ptr<const ProfilerDump> ConstProfilerDumpPtr;
typedef const ProfilerDump ConstProfilerDump;

} // namespace Unigine
