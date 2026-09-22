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


#ifdef _WIN32
	#ifndef D3D12_AGILITY_SDK_VERSION
		#define D3D12_AGILITY_SDK_VERSION 615
	#endif
	#ifndef D3D12_AGILITY_SDK_PATH
		#define D3D12_AGILITY_SDK_PATH ".\\D3D12\\"
	#endif
	extern "C" { __declspec(dllexport) extern const unsigned int D3D12SDKVersion = D3D12_AGILITY_SDK_VERSION; }
	extern "C" { __declspec(dllexport) extern const char *D3D12SDKPath = D3D12_AGILITY_SDK_PATH; }
#endif
