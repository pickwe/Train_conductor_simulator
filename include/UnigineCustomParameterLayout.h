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
#include "UnigineGUID.h"
#include "UnigineString.h"

namespace Unigine
{


class UNIGINE_API CustomParameterLayout : public APIInterface
{
public:
	typedef const CustomParameterLayout ConstCustomParameterLayout;
	static constexpr bool HAS_OWNERSHIP = false;


	enum TYPE
	{
		TYPE_FLOAT,
		TYPE_INT,
		TYPE_UINT,
	};
	int getNumParameters() const;
	int findParameter(const char *name) const;
	int findParameterByGUID(const UGUID &guid) const;
	UGUID getParameterGUID(int num) const;
	static String makeParameterURI(const UGUID &guid);
	static UGUID parseParameterURI(const char *uri);
	const char *getParameterName(int num) const;
	CustomParameterLayout::TYPE getParameterType(int num) const;
	void setParameterName(int num, const char *name);
	void setParameterType(int num, CustomParameterLayout::TYPE type);
	void removeParameter(int num);
	void swapParameters(int num0, int num1);
	void moveParameter(int from, int to);
	int addParameterFloat(const char *name, float default_value, const UGUID &guid = UGUID());
	void setParameterDefaultFloat(int num, float value);
	float getParameterDefaultFloat(int num) const;
	void setParameterMinFloat(int num, float value);
	float getParameterMinFloat(int num) const;
	void setParameterMaxFloat(int num, float value);
	float getParameterMaxFloat(int num) const;
	int addParameterInt(const char *name, int default_value, const UGUID &guid = UGUID());
	void setParameterDefaultInt(int num, int value);
	int getParameterDefaultInt(int num) const;
	void setParameterMinInt(int num, int value);
	int getParameterMinInt(int num) const;
	void setParameterMaxInt(int num, int value);
	int getParameterMaxInt(int num) const;
	unsigned int addParameterUInt(const char *name, unsigned int default_value, const UGUID &guid = UGUID());
	void setParameterDefaultUInt(int num, unsigned int value);
	unsigned int getParameterDefaultUInt(int num) const;
	void setParameterMinUInt(int num, unsigned int value);
	unsigned int getParameterMinUInt(int num) const;
	void setParameterMaxUInt(int num, unsigned int value);
	unsigned int getParameterMaxUInt(int num) const;
	int getStructureSize() const;
};
typedef Ptr<CustomParameterLayout> CustomParameterLayoutPtr;
typedef Ptr<const CustomParameterLayout> ConstCustomParameterLayoutPtr;
typedef const CustomParameterLayout ConstCustomParameterLayout;

} // namespace Unigine
