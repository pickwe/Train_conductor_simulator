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

#pragma once

#include "UnigineString.h"
#include "UnigineVector.h"

namespace Unigine
{

// Contains a vector of path patterns (wildcards) which you may check by using `matchPath` or
// `matchNormalizedPath` function.
// There are rules:
// - A directory separator is a slash '/'.
// - If there is a directory separator at the beginning or middle (or both) of the pattern,
//   then the pattern is relative to the directory of the data path or a level of the UMOUNT file
//   itself. Otherwise the pattern may also match at any level below the data path or the UMOUNT
//   file level.
//   - E.g. the pattern "foo/" is equal "**/foo/**". It matches "foo/", "foo/bar", "bar/foo/",
//     "zoo/foo/bar";
//   - E.g. the pattern "foo/*" matches only paths in the root directory "foo/". It matches "foo/",
//     "foo/bar", "foo/bar/", but not "bar/foo/"
//   - E.g. the pattern "/build/" matches paths "/build/foo", "/build/foo/bbar", "build/foo",
//     "build/foo/bar/" but it doesn't match "foo/build/";
// - A user may insert a backslash into a pattern to use symbols `[` or `]` in paths, e.g. the
//   pattern "file\\[1\\].txt" matches "file[1].txt". Using a backslash for `*` and `?` is
//   not supported, because these symbols are forbidden in a file name on Windows and our
//   FileSystem tries to behave the same for Windows and Linux systems.
class PathWildcards
{
public:
	PathWildcards() = default;
	explicit PathWildcards(const char *pattern) { add(pattern); }
	explicit PathWildcards(const Vector<String> &patterns) { add(patterns); }

	UNIGINE_INLINE bool empty() const { return items.empty(); }
	UNIGINE_INLINE int size() const { return items.size(); }
	UNIGINE_API void clear();

	UNIGINE_API void add(const char *pattern);
	UNIGINE_API void add(const Vector<String> &patterns);

	UNIGINE_API void set(const char *pattern);
	UNIGINE_API void set(const Vector<String> &patterns);

	UNIGINE_API void get(Vector<String> &patterns) const;
	UNIGINE_API Vector<String> get() const;
	UNIGINE_API const char *getPattern(int i) const;

	UNIGINE_API bool matchPath(const char *path) const;
	// Expects `String::normalizePath` or `String::normalizeDirPath` is called for `normalized_path`
	UNIGINE_API bool matchNormalizedPath(const char *normalized_path) const;

	UNIGINE_API static StringStack<> normalizePattern(const char *p);

private:
	void add_internal(const char *pattern);
	bool match_normalized_path(const char *normalized_path, int length) const;

	enum FLAGS : int
	{
		FLAGS_NONE = 0,
		FLAGS_BASENAME = 1 << 0,
	};
	struct Item
	{
		int internal_pattern_index{0};
		int user_pattern_index{0};
		FLAGS flags{FLAGS_NONE};
	};

	Vector<Item> items;
	// These strings contain all patterns which are divided by the null-terminated symbol
	String internal_patterns;
	String user_patterns;
};

} // namespace Unigine
