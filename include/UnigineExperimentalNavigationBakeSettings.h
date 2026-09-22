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


class UNIGINE_API ExperimentalNavigationBakeSettings : public APIInterface
{
public:
	typedef const ExperimentalNavigationBakeSettings ConstExperimentalNavigationBakeSettings;
	static constexpr bool HAS_OWNERSHIP = false;


	enum PARTITIONING
	{
		PARTITIONING_WATERSHED = 0,
		PARTITIONING_MONOTONE,
		PARTITIONING_CHUNKY,
	};
	void setBakeSize(const Math::vec3 &size);
	Math::vec3 getBakeSize() const;
	void setCellSize(float size);
	float getCellSize() const;
	void setCellHeight(float height);
	float getCellHeight() const;
	void setAgentRadius(float radius);
	float getAgentRadius() const;
	void setAgentHeight(float height);
	float getAgentHeight() const;
	void setMaxSlopeAngle(float angle);
	float getMaxSlopeAngle() const;
	void setMaxStepHeight(float height);
	float getMaxStepHeight() const;
	void setTileResolution(int resolution);
	int getTileResolution() const;
	void setPartitioning(ExperimentalNavigationBakeSettings::PARTITIONING partitioning);
	ExperimentalNavigationBakeSettings::PARTITIONING getPartitioning() const;
	void setRegionMinSize(float size);
	float getRegionMinSize() const;
	void setRegionMergeSize(float size);
	float getRegionMergeSize() const;
	void setRegionChunkSize(int size);
	int getRegionChunkSize() const;
	void setMaxEdgeError(float error);
	float getMaxEdgeError() const;
	void setSimplificationElevationRatio(float ratio);
	float getSimplificationElevationRatio() const;
	void setDetailSampleDistance(float distance);
	float getDetailSampleDistance() const;
	void setDetailSampleError(float error);
	float getDetailSampleError() const;
	void setTerrainSampleScale(float scale);
	float getTerrainSampleScale() const;
	void setBakeMask(int mask);
	int getBakeMask() const;
	void resetBakeSize();
	void resetCellSize();
	void resetCellHeight();
	void resetAgentRadius();
	void resetAgentHeight();
	void resetMaxSlopeAngle();
	void resetMaxStepHeight();
	void resetTileResolution();
	void resetPartitioning();
	void resetRegionMinSize();
	void resetRegionMergeSize();
	void resetRegionChunkSize();
	void resetMaxEdgeError();
	void resetSimplificationElevationRatio();
	void resetDetailSampleDistance();
	void resetDetailSampleError();
	void resetTerrainSampleScale();
	void resetBakeMask();
};
typedef Ptr<ExperimentalNavigationBakeSettings> ExperimentalNavigationBakeSettingsPtr;
typedef Ptr<const ExperimentalNavigationBakeSettings> ConstExperimentalNavigationBakeSettingsPtr;
typedef const ExperimentalNavigationBakeSettings ConstExperimentalNavigationBakeSettings;

} // namespace Unigine
