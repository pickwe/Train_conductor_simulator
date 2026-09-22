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
/*
 */
#include <new>
#define UNIGINE_NOTHROW_T	std::nothrow_t

#ifdef _LINUX
	#define __STDC_WANT_LIB_EXT1__ 1 // We want getenv_s
#endif

/*
 */
#include <time.h>
#include <math.h>
#include <float.h>
#include <functional>
#include <ctype.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <stdarg.h>
#include <stdint.h>
#include <assert.h>
#include <limits.h>
#include <utility>

#define UNIGINE_CONCATENATE_IMPL(A, B) A ## B
#define UNIGINE_CONCATENATE(A, B) UNIGINE_CONCATENATE_IMPL(A, B)

#ifdef _WIN32
	#ifndef NOMINMAX
		#define NOMINMAX
	#endif
#endif

/*
 */
#ifdef _WIN32
	#define UNIGINE_EXPORT		__declspec(dllexport)
	#define UNIGINE_IMPORT		__declspec(dllimport)
	#define UNIGINE_INLINE		__forceinline
	#define UNIGINE_RESTRICT	__restrict
#elif UNIGINE_PS5
	#define UNIGINE_EXPORT		__declspec(dllexport)
	#define UNIGINE_IMPORT		__declspec(dllimport)
	#define UNIGINE_INLINE		inline
	#define UNIGINE_RESTRICT	__restrict
#else
	#define UNIGINE_EXPORT		__attribute__((visibility("default")))
	#define UNIGINE_IMPORT
	#define UNIGINE_INLINE		__inline__
	#define UNIGINE_RESTRICT	__restrict
#endif

#if defined(UNIGINE_EMBED)
	#define UNIGINE_API
#elif defined(UNIGINE)
	#define UNIGINE_API			UNIGINE_EXPORT
#else
	#define UNIGINE_API			UNIGINE_IMPORT
#endif

/*
 */
#define UNIGINE_UNUSED(EXP)		(void)EXP;

/*
 */
#ifdef __GNUC__
	#define UNIGINE_DEPRECATED(id) id __attribute__ ((deprecated))
#elif defined(_MSC_VER)
	#define UNIGINE_DEPRECATED(id) __declspec(deprecated) id
#else
	#define UNIGINE_DEPRECATED(id) id
#endif

/*
 */
#ifdef __GNUC__
#define UNIGINE_LIKELY(EXP) __builtin_expect(!!(EXP), 1)
#define UNIGINE_UNLIKELY(EXP) __builtin_expect(!!(EXP), 0)
#else
#define UNIGINE_LIKELY(EXP) (EXP)
#define UNIGINE_UNLIKELY(EXP) (EXP)
#endif

/*
 */
#ifdef __GNUC__
	#define __UNIGINE_FUNCTION__ __PRETTY_FUNCTION__
#elif defined(__clang__)
	#define __UNIGINE_FUNCTION__ __PRETTY_FUNCTION__
#elif defined(_MSC_VER)
	#define __UNIGINE_FUNCTION__ __FUNCTION__
#endif

/*
 */
#ifdef __GNUC__
	#define UNIGINE_PRINTF(FORMAT,LIST)	__attribute__((format(printf,FORMAT,LIST)))
	#define UNIGINE_SCANF(FORMAT,LIST)	__attribute__((format(scanf,FORMAT,LIST)))
#else
	#define UNIGINE_PRINTF(FORMAT,LIST)
	#define UNIGINE_SCANF(FORMAT,LIST)
#endif

/*
 */
#include "UnigineLog.h"

/*
 */
#ifdef NDEBUG
	#define UNIGINE_FATAL(EXP)	(void)(EXP)
	#define UNIGINE_ASSERT(EXP)	(void)(EXP)
#else
	#ifdef _LINUX
		#define UNIGINE_FATAL(EXP) { Unigine::Log::fatal("%s:%d: Assertion: '%s'\n",__FILE__,__LINE__,EXP); }
		#define UNIGINE_ASSERT(EXP) { if (EXP) { } else { Unigine::Log::fatal("%s:%d: %s: Assertion: '%s'\n",__FILE__,__LINE__,__func__,#EXP); } }
	#elif defined(_MACOS) || defined(_IOS)
		#define UNIGINE_FATAL(EXP) { Unigine::Log::fatal("%s:%d: %s: Assertion: '%s'\n",__FILE__,__LINE__,__PRETTY_FUNCTION__,EXP); }
		#define UNIGINE_ASSERT(EXP) { if (EXP) { } else { Unigine::Log::fatal("%s:%d: %s: Assertion: '%s'\n",__FILE__,__LINE__,__PRETTY_FUNCTION__,#EXP); } }
	#else
		#define UNIGINE_FATAL(EXP) { Unigine::Log::fatal("%s:%d: Assertion: '%s'\n",__FILE__,__LINE__,EXP); }
		#define UNIGINE_ASSERT(EXP) { if (EXP) { } else { Unigine::Log::fatal("%s:%d: Assertion: '%s'\n",__FILE__,__LINE__,#EXP); } }
	#endif
#endif

/*
 */
#define UNIGINE_IS_ALIGNED4(VALUE) (((size_t)(VALUE) & 3) == 0)
#define UNIGINE_IS_ALIGNED8(VALUE) (((size_t)(VALUE) & 7) == 0)
#define UNIGINE_IS_ALIGNED16(VALUE) (((size_t)(VALUE) & 15) == 0)
#define UNIGINE_IS_ALIGNED128(VALUE) (((size_t)(VALUE) & 127) == 0)
#define UNIGINE_ASSERT_ALIGNED4(VALUE) UNIGINE_ASSERT(UNIGINE_IS_ALIGNED4(VALUE))
#define UNIGINE_ASSERT_ALIGNED8(VALUE) UNIGINE_ASSERT(UNIGINE_IS_ALIGNED8(VALUE))
#define UNIGINE_ASSERT_ALIGNED16(VALUE) UNIGINE_ASSERT(UNIGINE_IS_ALIGNED16(VALUE))
#define UNIGINE_ASSERT_ALIGNED128(VALUE) UNIGINE_ASSERT(UNIGINE_IS_ALIGNED128(VALUE))

/*
 */
#define UNIGINE_DECLARE_USE_MEMORY \
static UNIGINE_INLINE void *operator new(size_t size) { return Unigine::Memory::allocate(size); } \
static UNIGINE_INLINE void *operator new[](size_t size) { return Unigine::Memory::allocate(size); } \
static UNIGINE_INLINE void operator delete(void *ptr) { Unigine::Memory::deallocate(ptr); } \
static UNIGINE_INLINE void operator delete[](void *ptr) { Unigine::Memory::deallocate(ptr); } \
static UNIGINE_INLINE void operator delete(void *ptr,size_t size) { Unigine::Memory::deallocate(ptr,size); } \
static UNIGINE_INLINE void operator delete[](void *ptr,size_t size) { Unigine::Memory::deallocate(ptr,size); } \
static UNIGINE_INLINE void *operator new(size_t, void *here) { return here; } \
static UNIGINE_INLINE void operator delete(void *, void *) {}

/*
 */
namespace Unigine
{

struct ConstexprTag {};

constexpr size_t constexpr_strlen(const char *str)
{
	return *str ? 1 + constexpr_strlen(str + 1) : 0;
}

constexpr bool constexpr_strcmp(const char *str0, const char *str1)
{
	return (*str0 && *str1 && (*str0 == *str1)) ? constexpr_strcmp(str0 + 1, str1 + 1)
												: (*str0 == *str1) && (*str0 == 0);
}

template <class F>
struct YCombinator
{
	template <class... Args>
	decltype(auto) operator()(Args&&... args) const
	{
		return f(std::ref(*this), std::forward<Args>(args)...);
	}

	F f;
};

template <class F>
YCombinator<std::decay_t<F>> makeRecursiveLambda(F &&f)
{
	return {std::forward<F>(f)};
}

template <typename F>
struct ScopeExit
{
	~ScopeExit() { f(); }

	F f;
};

template <typename F>
ScopeExit<F> makeScopeExit(F &&f)
{
	return {std::forward<F>(f)};
}
#define UNIGINE_SCOPE_EXIT(lambda) \
	auto UNIGINE_CONCATENATE(scope_exit_var, __LINE__) = Unigine::makeScopeExit(lambda);

} // namespace Unigine

// Minimal WinAPI
#ifdef _WIN32

struct _RTL_CRITICAL_SECTION;
struct _RTL_SRWLOCK;

typedef int BOOL;
typedef unsigned char BOOLEAN;

typedef _RTL_CRITICAL_SECTION* LPCRITICAL_SECTION;

typedef _RTL_SRWLOCK RTL_SRWLOCK;
typedef RTL_SRWLOCK *PSRWLOCK;

extern "C"
{
	__declspec(dllimport) unsigned long __stdcall GetCurrentThreadId();
	__declspec(dllimport) BOOL __stdcall SwitchToThread();
	__declspec(dllimport) void __stdcall Sleep(unsigned long dwMilliseconds);

	__declspec(dllimport) void __stdcall InitializeCriticalSection(LPCRITICAL_SECTION lpCriticalSection);
	__declspec(dllimport) void __stdcall DeleteCriticalSection(LPCRITICAL_SECTION lpCriticalSection);
	__declspec(dllimport) BOOL __stdcall TryEnterCriticalSection(LPCRITICAL_SECTION lpCriticalSection);
	__declspec(dllimport) void __stdcall EnterCriticalSection(LPCRITICAL_SECTION lpCriticalSection);
	__declspec(dllimport) void __stdcall LeaveCriticalSection(LPCRITICAL_SECTION lpCriticalSection);

	__declspec(dllimport) void __stdcall InitializeSRWLock(PSRWLOCK SRWLock);
	__declspec(dllimport) void __stdcall AcquireSRWLockShared(PSRWLOCK SRWLock);
	__declspec(dllimport) void __stdcall ReleaseSRWLockShared(PSRWLOCK SRWLock);
	__declspec(dllimport) BOOLEAN __stdcall TryAcquireSRWLockShared(PSRWLOCK SRWLock);
	__declspec(dllimport) void __stdcall AcquireSRWLockExclusive(PSRWLOCK SRWLock);
	__declspec(dllimport) void __stdcall ReleaseSRWLockExclusive(PSRWLOCK SRWLock);
	__declspec(dllimport) BOOLEAN __stdcall TryAcquireSRWLockExclusive(PSRWLOCK SRWLock);
}

namespace Unigine
{
namespace WinAPI
{

struct CRITICAL_SECTION { void *Opaque1[1]; long Opaque2[2]; void *Opaque3[3]; };
struct SRWLOCK { void *Ptr; };

UNIGINE_INLINE unsigned long GetCurrentThreadId() noexcept
{
	return ::GetCurrentThreadId();
}
UNIGINE_INLINE BOOL SwitchToThread() noexcept
{
	return ::SwitchToThread();
}
UNIGINE_INLINE void Sleep(unsigned long dwMilliseconds) noexcept
{
	::Sleep(dwMilliseconds);
}

UNIGINE_INLINE void InitializeCriticalSection(CRITICAL_SECTION *lpCriticalSection) noexcept
{
	::InitializeCriticalSection(reinterpret_cast<LPCRITICAL_SECTION>(lpCriticalSection));
}
UNIGINE_INLINE void DeleteCriticalSection(CRITICAL_SECTION *lpCriticalSection) noexcept
{
	::DeleteCriticalSection(reinterpret_cast<LPCRITICAL_SECTION>(lpCriticalSection));
}
UNIGINE_INLINE BOOL TryEnterCriticalSection(CRITICAL_SECTION *lpCriticalSection) noexcept
{
	return ::TryEnterCriticalSection(reinterpret_cast<LPCRITICAL_SECTION>(lpCriticalSection));
}
UNIGINE_INLINE void EnterCriticalSection(CRITICAL_SECTION *lpCriticalSection) noexcept
{
	::EnterCriticalSection(reinterpret_cast<LPCRITICAL_SECTION>(lpCriticalSection));
}
UNIGINE_INLINE void LeaveCriticalSection(CRITICAL_SECTION *lpCriticalSection) noexcept
{
	::LeaveCriticalSection(reinterpret_cast<LPCRITICAL_SECTION>(lpCriticalSection));
}

UNIGINE_INLINE void InitializeSRWLock(SRWLOCK *SRWLock) noexcept
{
	::InitializeSRWLock(reinterpret_cast<PSRWLOCK>(SRWLock));
}
UNIGINE_INLINE void AcquireSRWLockShared(SRWLOCK *SRWLock) noexcept
{
	::AcquireSRWLockShared(reinterpret_cast<PSRWLOCK>(SRWLock));
}
UNIGINE_INLINE void ReleaseSRWLockShared(SRWLOCK *SRWLock) noexcept
{
	::ReleaseSRWLockShared(reinterpret_cast<PSRWLOCK>(SRWLock));
}
UNIGINE_INLINE BOOLEAN TryAcquireSRWLockShared(SRWLOCK *SRWLock) noexcept
{
	return ::TryAcquireSRWLockShared(reinterpret_cast<PSRWLOCK>(SRWLock));
}
UNIGINE_INLINE void AcquireSRWLockExclusive(SRWLOCK *SRWLock) noexcept
{
	::AcquireSRWLockExclusive(reinterpret_cast<PSRWLOCK>(SRWLock));
}
UNIGINE_INLINE void ReleaseSRWLockExclusive(SRWLOCK *SRWLock) noexcept
{
	::ReleaseSRWLockExclusive(reinterpret_cast<PSRWLOCK>(SRWLock));
}
UNIGINE_INLINE BOOLEAN TryAcquireSRWLockExclusive(SRWLOCK *SRWLock) noexcept
{
	return ::TryAcquireSRWLockExclusive(reinterpret_cast<PSRWLOCK>(SRWLock));
}
} // namespace WinAPI
} // namespace Unigine
#endif

/*
 */
#include "UnigineMemory.h"
