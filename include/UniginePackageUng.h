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

namespace Unigine
{


class UNIGINE_API PackageUng : public APIInterface
{
public:
	typedef const PackageUng ConstPackageUng;
	static constexpr bool HAS_OWNERSHIP = true;

	static bool isSupported();
	static Ptr<PackageUng> create(const char *password = "", const char *compress_extension_list = "json xml txt node world prop track mat basemat");
	bool load(const char *path);
	bool createPackage(const char *path);
	void close();
	int getNumFiles() const;
	const char *getFilePath(int num);
	bool selectFile(const char *path, size_t &size);
	bool readFile(unsigned char *data, size_t size);
	bool writeFile(const char *path, const unsigned char *data, size_t size);
	int findFile(const char *path) const;
	bool containsFile(const char *name) const;
	size_t getFileSize(int num) const;
	size_t getFileCompressedSize(int num) const;
	bool isFileCompressed(int num) const;
	int getNumCompressExtensions() const;
	const char *getCompressExtension(int num) const;
	bool isCompressExtension(const char *ext) const;
};
typedef Ptr<PackageUng> PackageUngPtr;
typedef Ptr<const PackageUng> ConstPackageUngPtr;
typedef const PackageUng ConstPackageUng;

} // namespace Unigine
