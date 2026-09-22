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

namespace Unigine
{

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API SystemInfo
{
public:

	enum GPU_VENDOR
	{
		GPU_VENDOR_UNKNOWN = 0,
		GPU_VENDOR_AMD,
		GPU_VENDOR_NVIDIA,
		GPU_VENDOR_INTEL,
		GPU_VENDOR_ARM,
		GPU_VENDOR_APPLE,
		GPU_VENDOR_IMGTEC,
		GPU_VENDOR_QUALCOMM,
	};

	enum GPU_TYPE
	{
		GPU_TYPE_DISCRETE = 0,
		GPU_TYPE_INTEGRATED,
		GPU_TYPE_OTHER,
		GPU_TYPE_UNSUPPORTED,
	};

	enum PLATFORM_TYPE
	{
		PLATFORM_TYPE_WINDOWS,
		PLATFORM_TYPE_LINUX,
		PLATFORM_TYPE_XBOX_SERIES_S,
		PLATFORM_TYPE_XBOX_SERIES_X,
		PLATFORM_TYPE_PS5,
		PLATFORM_TYPE_PS5_PRO,
	};

	struct MemoryStatistics
	{
		bool out_of_ram_committed{false};
		bool out_of_ram{false};
		bool out_of_vram{false};

		long long ram_free{0};
		long long ram_usage{0};
		long long ram_limit{0};
		long long ram_total{0};

		long long ram_committed_free{0};
		long long ram_committed_usage{0};
		long long ram_committed_background{0};
		long long ram_committed_limit{0};
		long long ram_committed_total{0};

		long long gpu_ram_usage{0};
		long long gpu_ram_background{0};

		long long gpu_vram_free{0};
		long long gpu_vram_usage{0};
		long long gpu_vram_background{0};
		long long gpu_vram_limit{0};
		long long gpu_vram_budget{0};
		long long gpu_vram_total{0};
	};

	static SystemInfo::MemoryStatistics getMemoryStatistics();
	static bool outOfRamCommitted();
	static bool outOfRam();
	static bool outOfVRam();
	static long long getRamFree();
	static long long getRamUsage();
	static long long getRamLimit();
	static long long getRamTotal();
	static unsigned int getRamFrequency();
	static unsigned int getSwapSize();
	static long long getRamCommittedFree();
	static long long getRamCommittedUsage();
	static long long getRamCommittedBackground();
	static long long getRamCommittedLimit();
	static long long getRamCommittedTotal();
	static long long getGpuRamUsage();
	static long long getGpuRamBackground();
	static long long getGpuVRamFree();
	static long long getGpuVRamUsage();
	static long long getGpuVRamBackground();
	static long long getGpuVRamLimit();
	static long long getGpuVRamBudget();
	static long long getGpuVRamTotal();
	static const char *getBinaryInfo();
	static const char *getRevisionInfo();
	static const char *getOSInfo();
	static const char *getCPUInfo();
	static const char *getEngineInfo();
	static int getCPUFrequency();
	static int getCPUCores();
	static int getCPUThreads();
	static const char *getCPUName();
	static bool hasMMX();
	static bool hasSSE();
	static bool hasSSE2();
	static bool hasSSE3();
	static bool hasSSE4();
	static bool hasSSE5();
	static bool hasAVX();
	static bool hasAVX2();
	static bool hasNeon();
	static bool hasHyperThreading();
	static void logInfo();
	static int getGPUCount();
	static int getGPUActive();
	static int findGPUByLuid(unsigned long long luid);
	static int findGPUByDeviceID(unsigned int device_id);
	static SystemInfo::GPU_TYPE getGPUType(int video_adapter_num = -1);
	static SystemInfo::GPU_VENDOR getGPUVendor(int video_adapter_num = -1);
	static int getGPUMemory(int video_adapter_num = -1);
	static int getGPUID(int video_adapter_num = -1);
	static unsigned int getGPUDeviceID(int video_adapter_num = -1);
	static unsigned long long getGPULuid(int video_adapter_num = -1);
	static const char *getGPUUuidString(int video_adapter_num = -1);
	static const char *getGPUDescription(int video_adapter_num = -1);
	static const char *getGPUVendorName(int video_adapter_num = -1);
	static const char *getGPUDriver(int video_adapter_num = -1);
	static SystemInfo::PLATFORM_TYPE getPlatformType();
	static int getProcessID();
	static const char *getEnvironmentVariable(const char *env_variable);
};

} // namespace Unigine
