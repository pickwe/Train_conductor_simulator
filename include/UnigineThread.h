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

#include "UnigineBase.h"
#include "UnigineVector.h"
#include "UnigineTimer.h"

#ifdef _WIN32
	#include <intrin.h>
#elif _LINUX || UNIGINE_PS5
	#include <pthread.h>
	#include <unistd.h>
#else
	#error "Platform unsupported"
#endif

#include <xmmintrin.h>
#include <stdint.h>

namespace Unigine
{
class CallbackBase;
struct QueueCPUTasks;
struct QueueCPUShaders;
class UNIGINE_API CPUTask;
class UNIGINE_API CPUShader;
class UNIGINE_API ThreadsPool;

//////////////////////////////////////////////////////////////////////////
/// Atomics and locks.
//////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////
/// AtomicGet
//////////////////////////////////////////////////////////////////////////

/// Atomic read, 8-bit.
/// Because simply accessing the variable directly is actually unsafe!
UNIGINE_INLINE char AtomicGet(const volatile char *ptr) noexcept
{
	#ifdef _WIN32
		char value = *ptr;
		_ReadWriteBarrier();
		return value;
	#else
		return __atomic_load_n(ptr, __ATOMIC_ACQUIRE);
	#endif
}

/// Atomic bool, 8-bit.
/// Because simply accessing the variable directly is actually unsafe!
UNIGINE_INLINE bool AtomicGet(const volatile bool *ptr) noexcept
{
	return AtomicGet(reinterpret_cast<const volatile char *>(ptr)) != 0;
}

/// Atomic read, 16-bit.
/// Because simply accessing the variable directly is actually unsafe!
UNIGINE_INLINE short AtomicGet(const volatile short *ptr) noexcept
{
	#ifdef _WIN32
		short value = *ptr;
		_ReadWriteBarrier();
		return value;
	#else
		return __atomic_load_n(ptr, __ATOMIC_ACQUIRE);
	#endif
}

/// Atomic read, 32-bit.
/// Because simply accessing the variable directly is actually unsafe!
UNIGINE_INLINE int AtomicGet(const volatile int *ptr) noexcept
{
	#ifdef _WIN32
		int value = *ptr;
		_ReadWriteBarrier();
		return value;
	#else
		return __atomic_load_n(ptr, __ATOMIC_ACQUIRE);
	#endif
}

/// Atomic read, 64-bit.
/// Because simply accessing the variable directly is actually unsafe!
UNIGINE_INLINE long long AtomicGet(const volatile long long *ptr) noexcept
{
	#ifdef _WIN32
		long long value = *ptr;
		_ReadWriteBarrier();
		return value;
	#else
		return __atomic_load_n(ptr, __ATOMIC_ACQUIRE);
	#endif
}

//////////////////////////////////////////////////////////////////////////
/// AtomicSet
//////////////////////////////////////////////////////////////////////////

/// Atomic set, 8-bit.
UNIGINE_INLINE void AtomicSet(volatile char *ptr, char value) noexcept
{
	#ifdef _WIN32
		_ReadWriteBarrier();
		*ptr = value;
	#else
		__atomic_store_n(ptr, value, __ATOMIC_RELEASE);
	#endif
}

/// Atomic set, 8-bit.
UNIGINE_INLINE void AtomicSet(volatile bool *ptr, bool value) noexcept
{
	AtomicSet(reinterpret_cast<volatile char *>(ptr), char(value));
}

/// Atomic set, 16-bit.
UNIGINE_INLINE void AtomicSet(volatile short *ptr, short value) noexcept
{
	#ifdef _WIN32
		_ReadWriteBarrier();
		*ptr = value;
	#else
		__atomic_store_n(ptr, value, __ATOMIC_RELEASE);
	#endif
}

/// Atomic set, 32-bit.
UNIGINE_INLINE void AtomicSet(volatile int *ptr, int value) noexcept
{
	#ifdef _WIN32
		_ReadWriteBarrier();
		*ptr = value;
	#else
		__atomic_store_n(ptr, value, __ATOMIC_RELEASE);
	#endif
}

/// Atomic set, 64-bit.
UNIGINE_INLINE void AtomicSet(volatile long long *ptr, long long value) noexcept
{
	#ifdef _WIN32
		_ReadWriteBarrier();
		*ptr = value;
	#else
		__atomic_store_n(ptr, value, __ATOMIC_RELEASE);
	#endif
}

//////////////////////////////////////////////////////////////////////////
/// AtomicSwap
//////////////////////////////////////////////////////////////////////////

/// Atomic swap, 8-bit.
/// Returns the previous value.
UNIGINE_INLINE char AtomicSwap(volatile char *ptr, char value) noexcept
{
	#ifdef _WIN32
		return _InterlockedExchange8(ptr, value);
	#else
		return __atomic_exchange_n(ptr, value, __ATOMIC_ACQ_REL);
	#endif
}

/// Atomic swap, 8-bit.
/// Returns the previous value.
UNIGINE_INLINE bool AtomicSwap(volatile bool *ptr, bool value) noexcept
{
	return AtomicSwap(reinterpret_cast<volatile char *>(ptr), char(value)) != 0;
}

/// Atomic swap, 16-bit.
/// Returns the previous value.
UNIGINE_INLINE short AtomicSwap(volatile short *ptr, short value) noexcept
{
	#ifdef _WIN32
		return _InterlockedExchange16(ptr, value);
	#else
		return __atomic_exchange_n(ptr, value, __ATOMIC_ACQ_REL);
	#endif
}

/// Atomic swap, 32-bit.
/// Returns the previous value.
UNIGINE_INLINE int AtomicSwap(volatile int *ptr, int value) noexcept
{
	#ifdef _WIN32
		return _InterlockedExchange(reinterpret_cast<volatile long *>(ptr), value);
	#else
		return __atomic_exchange_n(ptr, value, __ATOMIC_ACQ_REL);
	#endif
}

/// Atomic swap, 64-bit.
/// Returns the previous value.
UNIGINE_INLINE long long AtomicSwap(volatile long long *ptr, long long value) noexcept
{
	#ifdef _WIN32
		return _InterlockedExchange64(ptr, value);
	#else
		return __atomic_exchange_n(ptr, value, __ATOMIC_ACQ_REL);
	#endif
}

//////////////////////////////////////////////////////////////////////////
/// AtomicCAS
//////////////////////////////////////////////////////////////////////////

/// Atomic CAS (Compare-And-Swap), 8-bit.
UNIGINE_INLINE bool AtomicCAS(volatile char *ptr, char old_value, char new_value) noexcept
{
	assert((((size_t)ptr) % (sizeof(char))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return AtomicGet(ptr) == old_value && (_InterlockedCompareExchange8(ptr, new_value, old_value) == old_value);
	#else
		return AtomicGet(ptr) == old_value && (__sync_val_compare_and_swap(ptr, old_value, new_value) == old_value);
	#endif
}

/// Atomic CAS (Compare-And-Swap), 16-bit.
UNIGINE_INLINE bool AtomicCAS(volatile short *ptr, short old_value, short new_value) noexcept
{
	assert((((size_t)ptr) % (sizeof(short))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return AtomicGet(ptr) == old_value && (_InterlockedCompareExchange16(ptr, new_value, old_value) == old_value);
	#else
		return AtomicGet(ptr) == old_value && (__sync_val_compare_and_swap(ptr, old_value, new_value) == old_value);
	#endif
}

/// Atomic CAS (Compare-And-Swap), 32-bit.
UNIGINE_INLINE bool AtomicCAS(volatile int *ptr, int old_value, int new_value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return AtomicGet(ptr) == old_value && (_InterlockedCompareExchange((volatile long *)ptr, new_value, old_value) == old_value);
	#else
		return AtomicGet(ptr) == old_value && (__sync_val_compare_and_swap(ptr, old_value, new_value) == old_value);
	#endif
}

/// Atomic CAS (Compare-And-Swap), 64-bit.
UNIGINE_INLINE bool AtomicCAS(volatile long long *ptr, long long old_value, long long new_value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return AtomicGet(ptr) == old_value && (_InterlockedCompareExchange64(ptr, new_value, old_value) == old_value);
	#else
		return AtomicGet(ptr) == old_value && (__sync_val_compare_and_swap(ptr, old_value, new_value) == old_value);
	#endif
}
/// Atomic CAS (Compare-And-Swap), pointer.
UNIGINE_INLINE bool AtomicCAS(void *volatile * ptr, void *old_value, void *new_value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return *ptr == old_value && (_InterlockedCompareExchangePointer(ptr, new_value, old_value) == old_value);
	#else
		return *ptr == old_value && (__sync_val_compare_and_swap(ptr, old_value, new_value) == old_value);
	#endif
}

//////////////////////////////////////////////////////////////////////////
/// AtomicAdd
//////////////////////////////////////////////////////////////////////////

/// Atomic add, 8-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE char AtomicAdd(volatile char *ptr, char value) noexcept
{
	assert((((size_t)ptr) % (sizeof(char))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedExchangeAdd8(ptr, value);
	#else
		return __sync_fetch_and_add(ptr, value);
	#endif
}

/// Atomic add, 16-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE short AtomicAdd(volatile short *ptr, short value) noexcept
{
	assert((((size_t)ptr) % (sizeof(short))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedExchangeAdd16(ptr, value);
	#else
		return __sync_fetch_and_add(ptr, value);
	#endif
}

/// Atomic add, 32-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE int AtomicAdd(volatile int *ptr, int value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedExchangeAdd((volatile long *)ptr, (long)value);
	#else
		return __sync_fetch_and_add(ptr, value);
	#endif
}

/// Atomic add, 64-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE long long AtomicAdd(volatile long long *ptr, long long value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedExchangeAdd64((volatile long long *)ptr, (long long)value);
	#else
		return __sync_fetch_and_add(ptr, value);
	#endif
}

//////////////////////////////////////////////////////////////////////////
/// AtomicInc
//////////////////////////////////////////////////////////////////////////

/// Atomic increment, 8-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE char AtomicInc(volatile char *ptr) noexcept
{
	return AtomicAdd(ptr, 1);
}

/// Atomic increment, 16-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE short AtomicInc(volatile short *ptr) noexcept
{
	return AtomicAdd(ptr, 1);
}

/// Atomic increment, 32-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE int AtomicInc(volatile int *ptr) noexcept
{
	return AtomicAdd(ptr, 1);
}

/// Atomic increment, 64-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE long long AtomicInc(volatile long long *ptr) noexcept
{
	return AtomicAdd(ptr, 1);
}

//////////////////////////////////////////////////////////////////////////
/// AtomicDec
//////////////////////////////////////////////////////////////////////////

/// Atomic decrement, 8-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE char AtomicDec(volatile char *ptr) noexcept
{
	return AtomicAdd(ptr, -1);
}

/// Atomic increment, 16-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE short AtomicDec(volatile short *ptr) noexcept
{
	return AtomicAdd(ptr, -1);
}

/// Atomic increment, 32-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE int AtomicDec(volatile int *ptr) noexcept
{
	return AtomicAdd(ptr, -1);
}

/// Atomic increment, 64-bit.
/// Returns the previous value (just before adding).
UNIGINE_INLINE long long AtomicDec(volatile long long *ptr) noexcept
{
	return AtomicAdd(ptr, -1);
}

//////////////////////////////////////////////////////////////////////////
/// AtomicAnd
//////////////////////////////////////////////////////////////////////////

/// Atomic and, 8-bit.
/// Returns the previous value
UNIGINE_INLINE char AtomicAnd(volatile char *ptr, char value) noexcept
{
	assert((((size_t)ptr) % (sizeof(char))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedAnd8(ptr, value);
	#else
		return __sync_fetch_and_and(ptr, value);
	#endif
}

/// Atomic and, 16-bit.
/// Returns the previous value
UNIGINE_INLINE short AtomicAnd(volatile short *ptr, short value) noexcept
{
	assert((((size_t)ptr) % (sizeof(short))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedAnd16(ptr, value);
	#else
		return __sync_fetch_and_and(ptr, value);
	#endif
}

/// Atomic and, 32-bit.
/// Returns the previous value
UNIGINE_INLINE int AtomicAnd(volatile int *ptr, int value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedAnd((volatile long *)ptr, (long)value);
	#else
		return __sync_fetch_and_and(ptr, value);
	#endif
}

/// Atomic and, 64-bit.
/// Returns the previous value
UNIGINE_INLINE long long AtomicAnd(volatile long long *ptr, long long value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedAnd64((volatile long long *)ptr, (long long)value);
	#else
		return __sync_fetch_and_and(ptr, value);
	#endif
}

//////////////////////////////////////////////////////////////////////////
/// AtomicOr
//////////////////////////////////////////////////////////////////////////

/// Atomic or, 8-bit.
/// Returns the previous value
UNIGINE_INLINE char AtomicOr(volatile char *ptr, char value) noexcept
{
	assert((((size_t)ptr) % (sizeof(char))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedOr8(ptr, value);
	#else
		return __sync_fetch_and_or(ptr, value);
	#endif
}

/// Atomic or, 16-bit.
/// Returns the previous value
UNIGINE_INLINE short AtomicOr(volatile short *ptr, short value) noexcept
{
	assert((((size_t)ptr) % (sizeof(short))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedOr16(ptr, value);
	#else
		return __sync_fetch_and_or(ptr, value);
	#endif
}

/// Atomic or, 32-bit.
/// Returns the previous value
UNIGINE_INLINE int AtomicOr(volatile int *ptr, int value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedOr((volatile long *)ptr, (long)value);
	#else
		return __sync_fetch_and_or(ptr, value);
	#endif
}

/// Atomic or, 64-bit.
/// Returns the previous value
UNIGINE_INLINE long long AtomicOr(volatile long long *ptr, long long value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedOr64((volatile long long *)ptr, (long long)value);
	#else
		return __sync_fetch_and_or(ptr, value);
	#endif
}

//////////////////////////////////////////////////////////////////////////
/// AtomicXor
//////////////////////////////////////////////////////////////////////////

/// Atomic xor, 8-bit.
/// Returns the previous value
UNIGINE_INLINE char AtomicXor(volatile char *ptr, char value) noexcept
{
	assert((((size_t)ptr) % (sizeof(char))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedXor8(ptr, value);
	#else
		return __sync_fetch_and_xor(ptr, value);
	#endif
}

/// Atomic xor, 16-bit.
/// Returns the previous value
UNIGINE_INLINE short AtomicXor(volatile short *ptr, short value) noexcept
{
	assert((((size_t)ptr) % (sizeof(short))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedXor16(ptr, value);
	#else
		return __sync_fetch_and_xor(ptr, value);
	#endif
}

/// Atomic xor, 32-bit.
/// Returns the previous value
UNIGINE_INLINE int AtomicXor(volatile int *ptr, int value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedXor((volatile long *)ptr, (long)value);
	#else
		return __sync_fetch_and_xor(ptr, value);
	#endif
}

/// Atomic xor, 64-bit.
/// Returns the previous value
UNIGINE_INLINE long long AtomicXor(volatile long long *ptr, long long value) noexcept
{
	assert((((size_t)ptr) % (sizeof(int))) == 0 && "unaligned atomic!");
	#ifdef _WIN32
		return _InterlockedXor64((volatile long long *)ptr, (long long)value);
	#else
		return __sync_fetch_and_xor(ptr, value);
	#endif
}

//////////////////////////////////////////////////////////////////////////
/// Thread
//////////////////////////////////////////////////////////////////////////

/// Thread wrapper class.
class UNIGINE_API Thread
{
public:
	enum class Type: char
	{
		Main,

		PoolSync,
		PoolAsync,
		PoolCritical,
		PoolCommon,
		PoolBackground,
		PoolRenderFlush,
		PoolFileStream,
		PoolGPUStream,

		Sound,
		Input,
		Monitor,
		Dump,
		Ultraleap,
		Network,
		WakeUp,

		GenericUser,
		USC,
		Unknown,
	};

	Thread(bool auto_shutdown = true, Type type = Type::Unknown, const char *name = nullptr, int microprofile_priority = -1);
	virtual ~Thread();

	// Runs the thread.
	// size argument accepts thread stack size in bytes.
	// Returns 1 if the thread was successfully run; otherwise, 0 is returned.
	virtual bool run(size_t size = 0x100000);

	// Stops the thread.
	// Returns 1 if the operation was a success; otherwise, 0 is returned.
	bool stop();

	// Signals the thread to wake up.
	void signal();

	// Terminates the thread.
	// Returns 1 if the operation was a success; otherwise, 0 is returned.
	bool terminate();

	// Shutdown the thread.
	// platform specific stop and terminate
	virtual bool shutdown();

	static void allThreadsShutdown();
	static bool isShutdown() noexcept;

	UNIGINE_INLINE Type getType() const noexcept { return type; }
	UNIGINE_INLINE const char *getName() const noexcept { return name; }

	// Checks if the thread is running.
	// Returns 1 if the thread is running; otherwise, 0 is returned.
	UNIGINE_INLINE bool isRunning() const noexcept { return AtomicGet(&running) && !isShutdown(); }

	// Checks if the thread is waiting.
	// Returns 1 if the thread is waiting; otherwise, 0 is returned.
	UNIGINE_INLINE bool isWaiting() const noexcept { return AtomicGet(&is_waiting); }

	// Sets the thread priority.
	// Acceptable priority values are in the range [-3;3].
	// The default thread priority is 0.
	// Returns 1 if the operation was a success; otherwise, 0 is returned.
	int setPriority(int priority);

	// Returns the thread priority in range [-3;3].
	int getPriority() const;

	// Returns a platform specific thread
	UNIGINE_INLINE auto getThread() const noexcept { return thread; }

	// Sets thread's affinity mask
	int setAffinityMask(long long mask);

	// Returns thread's affinity mask
	// returns -1 if mask is not set.
	long long getAffinityMask() const;

	#ifdef _WIN32
		typedef unsigned long ID;
		UNIGINE_INLINE static ID getID() noexcept { return WinAPI::GetCurrentThreadId(); }
		// Suspends thread execution.
		// Timeout value is provided in milliseconds.
		UNIGINE_INLINE static void sleep(unsigned int ms) noexcept { WinAPI::Sleep(ms); }
		UNIGINE_INLINE static void switchThread() noexcept
		{
			_mm_pause();
			if (!WinAPI::SwitchToThread())
				WinAPI::Sleep(0);
		}
	#else
		typedef pthread_t ID;
		UNIGINE_INLINE static ID getID() noexcept { return pthread_self(); }
		UNIGINE_INLINE auto getThreadMutex() const noexcept { return mutex; }
		// Suspends thread execution.
		// Timeout value is provided in milliseconds.
		UNIGINE_INLINE static void sleep(unsigned int ms) noexcept { ::usleep(ms * 1000); }
		UNIGINE_INLINE static void switchThread() noexcept
		{
			_mm_pause();
			sched_yield();
		}
	#endif

	UNIGINE_INLINE ID getRunningID() const noexcept { return thread_id; }
	// Returns the current thread identifier.
	UNIGINE_INLINE bool isCurrentThread() const noexcept { return getID() == getRunningID(); }

	static Thread *getCurrentThread();
	static Type getCurrentThreadType();
	static void set_current_thread_type(Type type);
	UNIGINE_INLINE static bool isCurrentThreadType(Type type) { return getCurrentThreadType() == type; }
	UNIGINE_INLINE static bool isMainThread() { return isCurrentThreadType(Type::Main); }

	UNIGINE_INLINE static bool isPoolSyncThread() { return isCurrentThreadType(Type::PoolSync); }
	UNIGINE_INLINE static bool isPoolAsyncThread() { return isCurrentThreadType(Type::PoolAsync); }
	UNIGINE_INLINE static bool isPoolCriticalThread() { return isCurrentThreadType(Type::PoolCritical); }
	UNIGINE_INLINE static bool isPoolCommonThread() { return isCurrentThreadType(Type::PoolCommon); }
	UNIGINE_INLINE static bool isPoolBackgroundThread() { return isCurrentThreadType(Type::PoolBackground); }
	UNIGINE_INLINE static bool isPoolRenderFlushThread() { return isCurrentThreadType(Type::PoolRenderFlush); }
	UNIGINE_INLINE static bool isPoolFileStreamThread() { return isCurrentThreadType(Type::PoolFileStream); }
	UNIGINE_INLINE static bool isPoolGPUStreamThread() { return isCurrentThreadType(Type::PoolGPUStream); }
	UNIGINE_INLINE static bool isPoolThread()
	{
		switch (getCurrentThreadType())
		{
			case Type::PoolSync:
			case Type::PoolAsync:
			case Type::PoolCritical:
			case Type::PoolCommon:
			case Type::PoolBackground:
			case Type::PoolRenderFlush:
			case Type::PoolFileStream:
			case Type::PoolGPUStream:
				return true;
			default:
				return false;
		}
		return false;
	}

protected:
	struct WaitVariable;
	friend ThreadsPool;

	// Thread process function.
	virtual void process() = 0;

	// Puts the thread into the waiting state.
	void wait();

	#ifdef _WIN32
		static unsigned long __stdcall thread_handler(void *data);
		void *thread;
	#else
		static void *thread_handler(void *data);
		pthread_t thread;
		pthread_mutex_t mutex;
	#endif

	const char *name{nullptr};
	WaitVariable *wait_variable{nullptr};

	long long affinity_mask{0};
	ID thread_id;

	int microprofile_priority{0};
	mutable int priority{0};

	Type type{Type::Unknown};
	volatile char run_mutex{0};
	volatile bool running{false};
	volatile bool is_waiting{false};
	volatile bool clean_stopped{false}; // set to 1 by thread_handler() on clean exit from process()
};

//////////////////////////////////////////////////////////////////////////
/// Mutex Spin Lock
//////////////////////////////////////////////////////////////////////////

class BackoffSpinner
{
public:
	UNIGINE_INLINE void spin() noexcept
	{
		if (backoff >= yield_threshold)
		{
			Thread::switchThread();
		} else
		{
			int n = (1 << (backoff > max_exponent ? max_exponent : backoff));
			for (int i = 0; i < n; ++i)
				_mm_pause();
			++backoff;
		}
	}
	UNIGINE_INLINE void clear() noexcept { backoff = 0; }

private:
	static constexpr int yield_threshold = 31;
	static constexpr int max_exponent = 7;
	unsigned char backoff{0};
};

/// Spinlock, based on atomic CAS.
/// Now with unlocked dirty reads, proper waits, and exponential backoff.
template<typename Type>
UNIGINE_INLINE void SpinLockTepmlate(volatile Type *ptr, const Type old_value, const Type new_value) noexcept
{
	BackoffSpinner spinner;
	while (AtomicCAS(ptr, old_value, new_value) == false)
	{
		do
		{
			spinner.spin();
		} while (AtomicGet(ptr) != old_value);
	}
}
UNIGINE_INLINE void SpinLock(volatile char *ptr, const char old_value, const char new_value) noexcept { return SpinLockTepmlate<char>(ptr, old_value, new_value); }
UNIGINE_INLINE void SpinLock(volatile short *ptr, const short old_value, const short new_value) noexcept { return SpinLockTepmlate<short>(ptr, old_value, new_value); }
UNIGINE_INLINE void SpinLock(volatile int *ptr, const int old_value, const int new_value) noexcept { return SpinLockTepmlate<int>(ptr, old_value, new_value); }
UNIGINE_INLINE void SpinLock(volatile long long *ptr, const long long old_value, const long long new_value) noexcept { return SpinLockTepmlate<long long>(ptr, old_value, new_value); }

/// Waits for, but does not acquire the lock.
UNIGINE_INLINE void WaitLock(volatile char *ptr, const char value) noexcept { return SpinLock(ptr, value, value); }
UNIGINE_INLINE void WaitLock(volatile short *ptr, const short value) noexcept { return SpinLock(ptr, value, value); }
UNIGINE_INLINE void WaitLock(volatile int *ptr, const int value) noexcept { return SpinLock(ptr, value, value); }
UNIGINE_INLINE void WaitLock(volatile long long *ptr, const long long value) noexcept { return SpinLock(ptr, value, value); }

template<typename Type>
UNIGINE_INLINE void MutexSpinLock(volatile Type &mutex) noexcept { SpinLock(&mutex, 0, 1); }
template<typename Type>
UNIGINE_INLINE bool MutexSpinTryLock(volatile Type &mutex) noexcept { return AtomicCAS(&mutex, 0, 1); }
template<typename Type>
UNIGINE_INLINE void MutexSpinUnlock(volatile Type &mutex) noexcept { AtomicSet(&mutex, 0); }
template<typename Type>
UNIGINE_INLINE bool MutexSpinIsLocked(volatile Type &mutex) noexcept { return AtomicGet(&mutex) != 0; }
template<typename Type>
UNIGINE_INLINE void MutexSpinWaitLock(volatile Type &mutex) noexcept { SpinLock(&mutex, 0, 0); }

template<typename Type>
class ScopedSpinLockInteger
{
public:
	UNIGINE_INLINE ScopedSpinLockInteger(volatile Type &m): mutex(m) { SpinLock(&mutex, 0, 1); }
	UNIGINE_INLINE ~ScopedSpinLockInteger() noexcept { AtomicSet(&mutex, 0); }
private:
	volatile Type &mutex;
};
using ScopedSpinLockChar = ScopedSpinLockInteger<char>;
using ScopedSpinLockShort = ScopedSpinLockInteger<short>;
using ScopedSpinLockInt = ScopedSpinLockInteger<int>;
using ScopedSpinLockLongLong = ScopedSpinLockInteger<long long>;

using ScopedSpinLock8 = ScopedSpinLockInteger<char>;
using ScopedSpinLock16 = ScopedSpinLockInteger<short>;
using ScopedSpinLock32 = ScopedSpinLockInteger<int>;
using ScopedSpinLock64 = ScopedSpinLockInteger<long long>;

template<typename MutexType>
class ScopedMutexLock
{
public:
	UNIGINE_INLINE ScopedMutexLock(MutexType &m): mutex(m) { mutex.lock(); }
	UNIGINE_INLINE ~ScopedMutexLock() noexcept { mutex.unlock(); }
private:
	MutexType &mutex;
};

/// Scoped reader lock, based on RW-mutex.
template<typename MutexType>
class ScopedMutexReaderLock
{
public:
	UNIGINE_INLINE ScopedMutexReaderLock(MutexType &m): mutex(m) { mutex.lockRead(); }
	UNIGINE_INLINE ~ScopedMutexReaderLock() noexcept { mutex.unlockRead(); }
private:
	MutexType &mutex;
};

/// Scoped writer lock, based on RW-mutex.
template<typename MutexType>
class ScopedMutexWriterLock
{
public:
	UNIGINE_INLINE ScopedMutexWriterLock(MutexType &m): mutex(m) { mutex.lockWrite(); }
	UNIGINE_INLINE ~ScopedMutexWriterLock() noexcept { mutex.unlockWrite(); }
private:
	MutexType &mutex;
};


//////////////////////////////////////////////////////////////////////////
/// ReentrantMutex
//////////////////////////////////////////////////////////////////////////
template<typename MutexType>
class ReentrantMutexBase
{
public:
	ReentrantMutexBase() = default;

	UNIGINE_INLINE void lock() noexcept
	{
		auto id = AtomicGet(&thread_id);

		// C style cast, because static_cast doesn't work for PS5 & reinterpreter cast doesn't for work win32
		if (id != LLONG_MAX && id == (long long)(Thread::getID()))
		{
			++depth;
			assert(depth > 0); // Overflow check.
		} else
		{
			mutex.lock();
			AtomicSet(&thread_id, (long long)(Thread::getID()));
		}
	}
	UNIGINE_INLINE bool tryLock() noexcept
	{
		auto id = AtomicGet(&thread_id);

		// C style cast, because static_cast doesn't work for PS5 & reinterpreter cast doesn't for work win32
		if (id != LLONG_MAX && id == (long long)(Thread::getID()))
		{
			++depth;
			assert(depth > 0); // Overflow check.
		} else
		{
			if (!mutex.tryLock())
				return false;
			AtomicSet(&thread_id, (long long)(Thread::getID()));
		}
		return true;
	}
	UNIGINE_INLINE void unlock() noexcept
	{
		if (depth > 0)
		{
			--depth;
		} else
		{
			AtomicSet(&thread_id, LLONG_MAX);
			mutex.unlock();
		}
	}

	UNIGINE_INLINE bool isLocked() const noexcept { return AtomicGet(&thread_id) != LLONG_MAX; }

	void waitForce()
	{
		lock();
		unlock();
	}
	void wait()
	{
		auto id = AtomicGet(&thread_id);
		if (id == LLONG_MAX || id == (long long)(Thread::getID()))
			return;
		waitForce();
	}

protected:
	volatile long long thread_id{LLONG_MAX};
	int depth{0};
	MutexType mutex;
};

//////////////////////////////////////////////////////////////////////////
/// Mutex Spin Lock
//////////////////////////////////////////////////////////////////////////

/// Simple mutex, based on atomic CAS (SpinLock).
class MutexSpin
{
public:
	MutexSpin() = default;
	UNIGINE_INLINE void lock() noexcept { SpinLock(&is_locked, 0, 1); }
	UNIGINE_INLINE bool tryLock() noexcept { return AtomicCAS(&is_locked, 0, 1); }
	UNIGINE_INLINE void unlock() noexcept { AtomicSet(&is_locked, 0); }
	UNIGINE_INLINE bool isLocked() const noexcept { return AtomicGet(&is_locked) != 0; }
	UNIGINE_INLINE void wait() noexcept { SpinLock(&is_locked, 0, 0); }
private:
	mutable volatile char is_locked{0};
};
using ScopedSpinLock = ScopedMutexLock<MutexSpin>;

//////////////////////////////////////////////////////////////////////////
/// Reentrant Mutex Spin Lock
//////////////////////////////////////////////////////////////////////////

/// Reentrant mutex spin lock.
/// May be locked multiply times by the same thread.
/// Must be unlocked corresponding number of times.
using ReentrantMutexSpin = ReentrantMutexBase<MutexSpin>;
static_assert(sizeof(ReentrantMutexSpin) == 16, "Must be equal!");

using ScopedReentrantSpinLock = ScopedMutexLock<ReentrantMutexSpin>;

//////////////////////////////////////////////////////////////////////////
/// RW Mutex Spin Lock
//////////////////////////////////////////////////////////////////////////

/// Write-preferring readers-writer mutex spin lock.
class RWMutexSpin
{
public:
	RWMutexSpin() = default;

	UNIGINE_INLINE void lockRead() noexcept
	{
		BackoffSpinner spinner;
		while (true)
		{
			while (AtomicGet(&writer_lock))
				spinner.spin();

			AtomicInc(&readers_counter);
			if (AtomicGet(&writer_lock) == 0)
				break;
			AtomicDec(&readers_counter);
		}
	}
	UNIGINE_INLINE bool tryLockRead() noexcept
	{
		if (AtomicGet(&writer_lock))
			return false;

		AtomicInc(&readers_counter);
		if (AtomicGet(&writer_lock) == 0)
			return true;

		AtomicDec(&readers_counter);
		return false;
	}
	UNIGINE_INLINE void unlockRead() noexcept { AtomicDec(&readers_counter); }

	UNIGINE_INLINE void lockWrite() noexcept
	{
		BackoffSpinner spinner;
		while (true)
		{
			while (AtomicGet(&writer_lock))
				spinner.spin();

			if (AtomicCAS(&writer_lock, 0, 1))
			{
				while (AtomicGet(&readers_counter))
					spinner.spin();
				break;
			}
		}
	}
	UNIGINE_INLINE bool tryLockWrite() noexcept
	{
		if (AtomicGet(&writer_lock) || AtomicGet(&readers_counter))
			return false;

		if (!AtomicCAS(&writer_lock, 0, 1))
			return false;

		BackoffSpinner spinner;
		while (AtomicGet(&readers_counter))
			spinner.spin();
		return true;
	}
	UNIGINE_INLINE void unlockWrite() noexcept
	{
		AtomicSet(&writer_lock, 0);
	}

	UNIGINE_INLINE void lock() noexcept { lockWrite(); }
	UNIGINE_INLINE bool tryLock() noexcept { return tryLockWrite(); }
	UNIGINE_INLINE void unlock() noexcept { unlockWrite(); }

private:
	mutable volatile int readers_counter{0};
	mutable volatile char writer_lock{0};
};
using ScopedReaderSpinLock = ScopedMutexReaderLock<RWMutexSpin>;
using ScopedWriterSpinLock = ScopedMutexWriterLock<RWMutexSpin>;

template<typename MutexType>
class MutexAdvance
{
public:
	MutexAdvance() = default;

	UNIGINE_INLINE void lock() noexcept
	{
		mutex.lock();
		AtomicSet(&is_locked, 1);
	}
	UNIGINE_INLINE void unlock() noexcept
	{
		AtomicSet(&is_locked, 0);
		mutex.unlock();
	}
	UNIGINE_INLINE bool tryLock() noexcept
	{
		if (AtomicGet(&is_locked))
			return false;
		if (mutex.tryLock())
		{
			AtomicSet(&is_locked, 1);
			return true;
		}
		return false;
	}

	UNIGINE_INLINE bool isLocked() noexcept
	{
		return AtomicGet(&is_locked) != 0;
	}
	UNIGINE_INLINE void waitForce() noexcept
	{
		lock();
		unlock();
	}
	UNIGINE_INLINE void wait() noexcept
	{
		if (isLocked())
			waitForce();
	}
private:
	MutexType mutex;
	volatile char is_locked{0};
};

#ifdef _WIN32
namespace WinAPI
{
//////////////////////////////////////////////////////////////////////////
/// MutexCriticalSection (use WinAPI::CRITICAL_SECTION)
//////////////////////////////////////////////////////////////////////////
class MutexCriticalSection
{
public:
	UNIGINE_INLINE MutexCriticalSection() noexcept { WinAPI::InitializeCriticalSection(&mutex); }
	UNIGINE_INLINE ~MutexCriticalSection() noexcept { WinAPI::DeleteCriticalSection(&mutex); }

	UNIGINE_INLINE void lock() noexcept { WinAPI::EnterCriticalSection(&mutex); }
	UNIGINE_INLINE bool tryLock() noexcept { return WinAPI::TryEnterCriticalSection(&mutex) != 0; }
	UNIGINE_INLINE void unlock() noexcept { WinAPI::LeaveCriticalSection(&mutex); }

private:
	WinAPI::CRITICAL_SECTION mutex;
};
using ScopedCriticalSectionLock = ScopedMutexLock<MutexCriticalSection>;

//////////////////////////////////////////////////////////////////////////
/// Mutex Slim (use WinAPI::SRWLOCK)
//////////////////////////////////////////////////////////////////////////
class MutexSlim
{
public:
	UNIGINE_INLINE MutexSlim() noexcept { WinAPI::InitializeSRWLock(&mutex); }
	UNIGINE_INLINE ~MutexSlim() noexcept { assert(!is_locked()); }

	UNIGINE_INLINE void lock() noexcept { WinAPI::AcquireSRWLockExclusive(&mutex); }
	UNIGINE_INLINE bool tryLock() noexcept { return WinAPI::TryAcquireSRWLockExclusive(&mutex) != 0; }
	UNIGINE_INLINE void unlock() noexcept { WinAPI::ReleaseSRWLockExclusive(&mutex); }

private:
	UNIGINE_INLINE bool is_locked() noexcept
	{
		if (tryLock() == false)
			return true;
		unlock();
		return false;
	}
	WinAPI::SRWLOCK mutex;
};
using ScopedSlimLock = ScopedMutexLock<MutexSlim>;

//////////////////////////////////////////////////////////////////////////
/// Reentrant Mutex Slim (use WinAPI::SRWLOCK)
//////////////////////////////////////////////////////////////////////////
using ReentrantMutexSlim = ReentrantMutexBase<MutexSlim>;
using ScopedReentrantSlimLock = ScopedMutexLock<ReentrantMutexSlim>;

//////////////////////////////////////////////////////////////////////////
/// RW Mutex Slim (use WinAPI::SRWLOCK)
//////////////////////////////////////////////////////////////////////////
class RWMutexSlim
{
public:
	UNIGINE_INLINE RWMutexSlim() { WinAPI::InitializeSRWLock(&mutex); }
	UNIGINE_INLINE ~RWMutexSlim() { assert(!is_locked()); }

	UNIGINE_INLINE void lockRead() noexcept { WinAPI::AcquireSRWLockShared(&mutex); }
	UNIGINE_INLINE bool tryLockRead() noexcept { return WinAPI::TryAcquireSRWLockShared(&mutex) != 0; }
	UNIGINE_INLINE void unlockRead() noexcept { WinAPI::ReleaseSRWLockShared(&mutex); }

	UNIGINE_INLINE void lockWrite() noexcept { WinAPI::AcquireSRWLockExclusive(&mutex);  }
	UNIGINE_INLINE bool tryLockWrite() noexcept { return WinAPI::TryAcquireSRWLockExclusive(&mutex) != 0; }
	UNIGINE_INLINE void unlockWrite() noexcept { WinAPI::ReleaseSRWLockExclusive(&mutex); }

	UNIGINE_INLINE void lock() noexcept { lockWrite();  }
	UNIGINE_INLINE bool tryLock() noexcept { return tryLockWrite(); }
	UNIGINE_INLINE void unlock() noexcept { unlockWrite(); }

private:
	UNIGINE_INLINE bool is_locked() noexcept
	{
		if (tryLockWrite() == false)
			return true;
		unlockWrite();
		return false;
	}
	WinAPI::SRWLOCK mutex;
};
using ScopedReaderSlimLock = ScopedMutexReaderLock<RWMutexSlim>;
using ScopedWriterSlimLock = ScopedMutexWriterLock<RWMutexSlim>;

using MutexSlimAdvance = MutexAdvance<MutexSlim>;
using ScopedSlimAdvanceLock = ScopedMutexLock<MutexSlimAdvance>;

using MutexCriticalSectionAdvance = MutexAdvance<MutexCriticalSection>;
using ScopedCriticalSectionAdvanceLock = ScopedMutexLock<MutexCriticalSectionAdvance>;

}
#else
namespace UnixAPI
{
//////////////////////////////////////////////////////////////////////////
/// Mutex PThread (use pthread_mutex_t)
//////////////////////////////////////////////////////////////////////////
class MutexPThread
{
public:
	UNIGINE_INLINE MutexPThread() noexcept { pthread_mutex_init(&mutex, NULL); }
	UNIGINE_INLINE ~MutexPThread() noexcept { pthread_mutex_destroy(&mutex); }

	UNIGINE_INLINE void lock() noexcept { pthread_mutex_lock(&mutex); }
	UNIGINE_INLINE bool tryLock() noexcept { return pthread_mutex_trylock(&mutex) == 0; }
	UNIGINE_INLINE void unlock() noexcept { pthread_mutex_unlock(&mutex); }
private:
	pthread_mutex_t mutex;
};
using ScopedPThreadLock = ScopedMutexLock<MutexPThread>;

//////////////////////////////////////////////////////////////////////////
/// Reentrant Mutex PThread (use pthread_mutex_t)
//////////////////////////////////////////////////////////////////////////
using ReentrantMutexPThread = ReentrantMutexBase<MutexPThread>;
using ScopedReentrantPThreadLock = ScopedMutexLock<ReentrantMutexPThread>;

using MutexPThreadAdvance = MutexAdvance<MutexPThread>;
using ScopedPThreadAdvanceLock = ScopedMutexLock<MutexPThreadAdvance>;
}
#endif

// Default implemetation Mutex
using Mutex = MutexSpin;
using ScopedLock = ScopedSpinLock;

using ReentrantMutex = ReentrantMutexSpin;
using ScopedReentrantLock = ScopedReentrantSpinLock;

using RWMutex = RWMutexSpin;
using ScopedReaderLock = ScopedReaderSpinLock;
using ScopedWriterLock = ScopedWriterSpinLock;

// Default implemetation MutexNative
#ifdef _WIN32
	using MutexNative = WinAPI::MutexSlim;
	using ScopedNativeLock = WinAPI::ScopedSlimLock;

	using MutexNativeAdvance = WinAPI::MutexSlimAdvance;
	using ScopedNativeAdvanceLock = WinAPI::ScopedSlimAdvanceLock;

	using ReentrantMutexNative = WinAPI::ReentrantMutexSlim;
	using ScopedReentrantNativeLock = WinAPI::ScopedReentrantSlimLock;

	using RWMutexNative = WinAPI::RWMutexSlim;
	using ScopedReaderNativeLock = WinAPI::ScopedReaderSlimLock;
	using ScopedWriterNativeLock = WinAPI::ScopedWriterSlimLock;
#else
	using MutexNative = UnixAPI::MutexPThread;
	using ScopedNativeLock = UnixAPI::ScopedPThreadLock;

	using MutexNativeAdvance = UnixAPI::MutexPThreadAdvance;
	using ScopedNativeAdvanceLock = UnixAPI::ScopedPThreadAdvanceLock;

	using ReentrantMutexNative = UnixAPI::ReentrantMutexPThread;
	using ScopedReentrantNativeLock = UnixAPI::ScopedReentrantPThreadLock;

	using RWMutexNative = RWMutex;
	using ScopedReaderNativeLock = ScopedReaderLock;
	using ScopedWriterNativeLock = ScopedWriterLock;
#endif

////////////////////////////////////////////////////////////////////////////////
/// Atomic
////////////////////////////////////////////////////////////////////////////////

namespace AtomicImpl
{
template<typename T>
struct is_size8 { static constexpr bool value = sizeof(T) == 1; };
template<typename T>
struct is_size16 { static constexpr bool value = sizeof(T) > 1; };
template<typename T>
struct is_size32 { static constexpr bool value = sizeof(T) > 2; };
template<typename T>
struct is_size64 { static constexpr bool value = sizeof(T) > 4; };
template<typename T0, typename T1>
struct is_same_size { static constexpr bool value = sizeof(T0) == sizeof(T1); };

template <typename T>
using conditional_raw16 = typename std::conditional_t<is_size16<T>::value, short, char>;
template <typename T>
using conditional_raw32 = typename std::conditional_t<is_size32<T>::value, int, conditional_raw16<T>>;
template <typename T>
using getRawType = typename std::conditional_t<is_size64<T>::value, long long, conditional_raw32<T>>;

template <typename Type, typename MutexType = MutexSpin>
struct AtomicWithMutex final
{
public:
	static constexpr bool isLockFree = false;

	AtomicWithMutex() = default;
	UNIGINE_INLINE AtomicWithMutex(const Type &v): value(v) {}
	UNIGINE_INLINE AtomicWithMutex(const AtomicWithMutex &v)
	{
		ScopedMutexLock<MutexType> lock1(v.mutex);
		ScopedMutexLock<MutexType> lock2(mutex);
		value = v.value;
	}

	UNIGINE_INLINE operator Type() const noexcept { return fetch(); }

	UNIGINE_INLINE Type &getUnsafeValue() noexcept { return value; }
	UNIGINE_INLINE const Type &getUnsafeValue() const noexcept { return value; }

	UNIGINE_INLINE Type *getValuePtr() noexcept { return &value; }
	UNIGINE_INLINE const Type *getValuePtr() const noexcept { return &value; }

	UNIGINE_INLINE MutexType &getMutex() noexcept { return mutex; }
	UNIGINE_INLINE const MutexType &getMutex() const noexcept { return mutex; }

	UNIGINE_INLINE Type fetch() const noexcept
	{
		ScopedMutexLock<MutexType> lock(mutex);
		return value;
	}
	UNIGINE_INLINE void store(const Type &v) noexcept
	{
		ScopedMutexLock<MutexType> lock(mutex);
		value = v;
	}

	UNIGINE_INLINE const Type &operator=(const Type &v) noexcept
	{
		store(v);
		return v;
	}
	UNIGINE_INLINE Type operator=(const AtomicWithMutex &v) noexcept
	{
		if (this == &v)
			return value;
		ScopedMutexLock<MutexType> lock1(v.mutex);
		ScopedMutexLock<MutexType> lock2(mutex);
		value = v.value;
		return value;
	}

	/// Atomic swap
	UNIGINE_INLINE Type swap(const Type &v) noexcept
	{
		ScopedMutexLock<MutexType> lock(mutex);
		Type old_value = value;
		value = v;
		return old_value;
	}
	UNIGINE_INLINE bool compareAndSwap(const Type &old_value, const Type &new_value) noexcept
	{
		if (value != old_value)
			return false;
		ScopedMutexLock<MutexType> lock(mutex);
		if (value != old_value)
			return false;
		value = new_value;
		return true;
	}
	UNIGINE_INLINE void spinLock(const Type &old_value, const Type &new_value) noexcept
	{
		BackoffSpinner spinner;
		while (compareAndSwap(old_value, new_value) == false)
		{
			do
			{
				spinner.spin();
			} while (value != old_value);
		}
	}
	UNIGINE_INLINE void waitValue(const Type &v) noexcept { spinLock(v, v); }

private:
	mutable volatile Type value;
	mutable MutexType mutex;
};

template <typename Type>
struct alignas(sizeof(getRawType<Type>)) AtomicLockFreeRaw final
{
	using RawType = getRawType<Type>;

	static constexpr bool isLockFree = true;

	AtomicLockFreeRaw() = default;
	UNIGINE_INLINE AtomicLockFreeRaw(Type v): raw_value(*(reinterpret_cast<RawType *>(&v))) {}
	UNIGINE_INLINE AtomicLockFreeRaw(const AtomicLockFreeRaw &v) noexcept
	{
		#ifdef _WIN32
			RawType new_v = v.raw_value;
			_ReadWriteBarrier();
			raw_value = new_v;
		#else
			Type new_v = v.fetch();
			store(new_v);
		#endif
	}

	UNIGINE_INLINE operator Type() const noexcept { return fetch(); }

	UNIGINE_INLINE RawType &getUnsafeRawValue() noexcept { return raw_value; }
	UNIGINE_INLINE const RawType &getUnsafeRawValue() const noexcept { return raw_value; }
	UNIGINE_INLINE RawType *getRawValuePtr() noexcept { return &raw_value; }
	UNIGINE_INLINE const RawType *getRawValuePtr() const noexcept { return &raw_value; }

	UNIGINE_INLINE Type &getUnsafeValue() noexcept { return *(reinterpret_cast<Type *>(&raw_value)); }
	UNIGINE_INLINE const Type &getUnsafeValue() const noexcept { return *(reinterpret_cast<const Type *>(&raw_value)); }
	UNIGINE_INLINE Type *getValuePtr() noexcept { return  reinterpret_cast<Type *>(&raw_value); }
	UNIGINE_INLINE const Type *getValuePtr() const noexcept { return reinterpret_cast<const Type *>(&raw_value); }

	/// Atomic read
	UNIGINE_INLINE Type fetch() const noexcept
	{
		#ifdef _WIN32
			RawType v = raw_value;
			_ReadWriteBarrier();
			return *(reinterpret_cast<Type *>(&v));
		#else
			RawType v = __atomic_load_n(&raw_value, __ATOMIC_ACQUIRE);
			return *(reinterpret_cast<Type *>(&v));
		#endif
	}

	/// Atomic write
	UNIGINE_INLINE void store(Type v) noexcept
	{
		#ifdef _WIN32
			_ReadWriteBarrier();
			raw_value = *(reinterpret_cast<RawType *>(&v));
		#else
			__atomic_store_n(&raw_value, *(reinterpret_cast<RawType *>(&v)), __ATOMIC_RELEASE);
		#endif
	}

	UNIGINE_INLINE Type operator=(Type v) noexcept { store(v); return v; }
	UNIGINE_INLINE Type operator=(const AtomicLockFreeRaw &v) noexcept
	{
		#ifdef _WIN32
			RawType new_v = v.raw_value;
			_ReadWriteBarrier();
			raw_value = new_v;
			return *(reinterpret_cast<Type*>(&new_v));
		#else
			Type new_v = v.fetch();
			store(new_v);
			return new_v;
		#endif
	}
	/// Atomic swap
	UNIGINE_INLINE Type swap(Type v) noexcept
	{
		return *(reinterpret_cast<Type *>(&AtomicSwap(&raw_value, *(reinterpret_cast<RawType *>(&v)))));
	}
	/// Atomic CAS (Compare-And-Swap)
	UNIGINE_INLINE bool compareAndSwap(Type old_value, Type new_value) noexcept
	{
		return AtomicCAS(&raw_value, *(reinterpret_cast<RawType *>(&old_value)), *(reinterpret_cast<RawType *>(&new_value)));
	}
	/// Spinlock, based on atomic CAS.
	UNIGINE_INLINE void spinLock(Type old_value, Type new_value) noexcept
	{
		SpinLock(&raw_value, *(reinterpret_cast<RawType *>(&old_value)), *(reinterpret_cast<RawType *>(&new_value)));
	}
	/// Wait until the atom equals the value (used spinLock)
	UNIGINE_INLINE void waitValue(Type v) noexcept
	{
		WaitLock(&raw_value, *(reinterpret_cast<RawType *>(&v)));
	}

private:
	volatile RawType raw_value;
};

template <typename Type>
struct alignas(sizeof(getRawType<Type>)) AtomicLockFreeAlign final
{
	using RawType = getRawType<Type>;

	static constexpr bool isLockFree = true;

	AtomicLockFreeAlign() = default;
	UNIGINE_INLINE AtomicLockFreeAlign(Type v) noexcept
	{
		CastUnion v_c;
		v_c.raw = 0;
		v_c.value = v;
		raw_value = v_c.raw;
	}
	UNIGINE_INLINE AtomicLockFreeAlign(const AtomicLockFreeAlign &v) noexcept
	{
		#ifdef _WIN32
			CastUnion new_v;
			new_v.raw = v.raw_value;
			_ReadWriteBarrier();
			raw_value = new_v.raw;
		#else
			Type new_v = v.fetch();
			store(new_v);
		#endif
	}

	UNIGINE_INLINE operator Type() const noexcept { return fetch(); }

	UNIGINE_INLINE RawType &getUnsafeRawValue() noexcept { return raw_value; }
	UNIGINE_INLINE const RawType &getUnsafeRawValue() const noexcept { return raw_value; }
	UNIGINE_INLINE RawType *getRawValuePtr() noexcept { return &raw_value; }
	UNIGINE_INLINE const RawType *getRawValuePtr() const noexcept { return &raw_value; }

	UNIGINE_INLINE Type &getUnsafeValue() noexcept { return *(reinterpret_cast<Type *>(&raw_value)); }
	UNIGINE_INLINE const Type &getUnsafeValue() const noexcept { return *(reinterpret_cast<const Type *>(&raw_value)); }
	UNIGINE_INLINE Type *getValuePtr() noexcept { return  reinterpret_cast<Type *>(&raw_value); }
	UNIGINE_INLINE const Type *getValuePtr() const noexcept { return reinterpret_cast<const Type *>(&raw_value); }

	/// Atomic read
	UNIGINE_INLINE Type fetch() const noexcept
	{
		#ifdef _WIN32
			RawType v = raw_value;
			_ReadWriteBarrier();
			return *(reinterpret_cast<Type *>(&v));
		#else
			RawType v = __atomic_load_n(&raw_value, __ATOMIC_ACQUIRE);
			return *(reinterpret_cast<Type *>(&v));
		#endif
	}

	/// Atomic write
	UNIGINE_INLINE void store(Type v) noexcept
	{
		CastUnion v_c;
		v_c.raw = 0;
		v_c.value = v;

		#ifdef _WIN32
			_ReadWriteBarrier();
			raw_value = v_c.raw;
		#else
			__atomic_store_n(&raw_value, v_c.raw, __ATOMIC_RELEASE);
		#endif
	}

	UNIGINE_INLINE Type operator=(Type v) noexcept { store(v); return v; }
	UNIGINE_INLINE Type operator=(const AtomicLockFreeAlign &v) noexcept
	{
		#ifdef _WIN32
			CastUnion new_v;
			new_v.raw = v.raw_value;
			_ReadWriteBarrier();
			raw_value = new_v.raw;
			return new_v.value;
		#else
			Type new_v = v.fetch();
			store(new_v);
			return new_v;
		#endif
	}

	/// Atomic swap
	UNIGINE_INLINE Type swap(Type v) noexcept
	{
		CastUnion v_c;
		v_c.raw = 0;
		v_c.value = v;
		return *(reinterpret_cast<Type *>(&AtomicSwap(&raw_value, v_c.raw)));
	}
	/// Atomic CAS (Compare-And-Swap)
	UNIGINE_INLINE bool compareAndSwap(Type old_value, Type new_value) noexcept
	{
		CastUnion old_v_c{};
		CastUnion new_v_c{};

		old_v_c.raw = 0;
		new_v_c.raw = 0;

		old_v_c.value = old_value;
		new_v_c.value = new_value;

		return AtomicCAS(&raw_value, old_v_c.raw, new_v_c.raw);
	}
	/// Spinlock, based on atomic CAS.
	UNIGINE_INLINE void spinLock(Type old_value, Type new_value) noexcept
	{
		CastUnion old_v_c;
		CastUnion new_v_c;

		old_v_c.raw = 0;
		new_v_c.raw = 0;

		old_v_c.value = old_value;
		new_v_c.value = new_value;

		SpinLock(&raw_value, old_v_c.raw, new_v_c.raw);
	}
	/// Wait until the atom equals the value (used spinLock)
	UNIGINE_INLINE void waitValue(Type v) noexcept
	{
		CastUnion v_c;
		v_c.raw = 0;
		v_c.value = v;
		WaitLock(&raw_value, v_c.raw);
	}

private:
	union CastUnion
	{
		Type value;
		RawType raw;
	};
	volatile RawType raw_value;
};

template <typename T>
using AtomicLockFree = typename std::conditional_t<is_same_size<T, getRawType<T>>::value, AtomicLockFreeRaw<T>, AtomicLockFreeAlign<T>>;

template <typename Type>
struct alignas(sizeof(long long)) AtomicPointer
{
	static constexpr bool isLockFree = true;

	AtomicPointer() = default;
	UNIGINE_INLINE AtomicPointer(Type v): raw_value(reinterpret_cast<long long>(v)) {}
	UNIGINE_INLINE AtomicPointer(const AtomicPointer &v) noexcept
	{
		#ifdef _WIN32
			long long new_v = v.raw_value;
			_ReadWriteBarrier();
			raw_value = new_v;
		#else
			Type new_v = v.fetch();
			store(new_v);
		#endif
	}

	UNIGINE_INLINE operator Type() const noexcept { return fetch(); }

	UNIGINE_INLINE volatile long long &getUnsafeRawValue() noexcept { return raw_value; }
	UNIGINE_INLINE const volatile long long &getUnsafeRawValue() const noexcept { return raw_value; }
	UNIGINE_INLINE volatile long long *getRawValuePtr() noexcept { return &raw_value; }
	UNIGINE_INLINE const volatile long long *getRawValuePtr() const noexcept { return &raw_value; }

	UNIGINE_INLINE Type &getUnsafeValue() noexcept { return *(reinterpret_cast<Type *>(&raw_value)); }
	UNIGINE_INLINE const Type &getUnsafeValue() const noexcept { return *(reinterpret_cast<const Type *>(&raw_value)); }
	UNIGINE_INLINE Type *getValuePtr() noexcept { return  reinterpret_cast<Type *>(&raw_value); }
	UNIGINE_INLINE const Type *getValuePtr() const noexcept { return reinterpret_cast<const Type *>(&raw_value); }

	/// Atomic read
	UNIGINE_INLINE Type fetch() const noexcept
	{
		#ifdef _WIN32
			long long v = raw_value;
			_ReadWriteBarrier();
			return reinterpret_cast<Type>(v);
		#else
			return reinterpret_cast<Type>(__atomic_load_n(&raw_value, __ATOMIC_ACQUIRE));
		#endif
	}
	/// Atomic write
	UNIGINE_INLINE void store(Type v) noexcept
	{
		#ifdef _WIN32
			_ReadWriteBarrier();
			raw_value = reinterpret_cast<long long>(v);
		#else
			__atomic_store_n(&raw_value, reinterpret_cast<long long>(v), __ATOMIC_RELEASE);
		#endif
	}

	UNIGINE_INLINE Type operator=(Type v) noexcept { store(v); return v; }
	UNIGINE_INLINE Type operator=(const AtomicPointer &v) noexcept
	{
		#ifdef _WIN32
			long long new_v = v.raw_value;
			_ReadWriteBarrier();
			raw_value = new_v;
			return reinterpret_cast<Type>(new_v);
		#else
			Type new_v = v.fetch();
			store(new_v);
			return new_v;
		#endif
	}

	/// Atomic swap
	UNIGINE_INLINE Type swap(Type v) noexcept
	{
		return reinterpret_cast<Type>((AtomicSwap(&raw_value, reinterpret_cast<long long>(v))));
	}
	/// Atomic CAS (Compare-And-Swap)
	UNIGINE_INLINE bool compareAndSwap(Type old_value, Type new_value) noexcept
	{
		return AtomicCAS(&raw_value, reinterpret_cast<long long>(old_value), reinterpret_cast<long long>(new_value));
	}
	/// Spinlock, based on atomic CAS.
	UNIGINE_INLINE void spinLock(Type old_value, Type new_value) noexcept
	{
		SpinLock(&raw_value, reinterpret_cast<long long>(old_value), reinterpret_cast<long long>(new_value));
	}
	/// Wait until the atom equals the value (used spinLock)
	UNIGINE_INLINE void waitValue(Type v) noexcept
	{
		WaitLock(&raw_value, reinterpret_cast<long long>(v));
	}

	/// Atomic fetch and add
	/// Returns the previous value
	UNIGINE_INLINE Type fetchAdd(long long v) noexcept
	{
		return reinterpret_cast<Type>(AtomicAdd(&raw_value, v * SIZE_TYPE));
	}
	/// Atomic fetch and sub
	/// Returns the previous value
	UNIGINE_INLINE Type fetchSub(long long v) noexcept
	{
		return reinterpret_cast<Type>(AtomicAdd(&raw_value, -v * SIZE_TYPE));
	}
	/// Atomic fetch and increment
	/// Returns the previous value
	UNIGINE_INLINE Type fetchInc() noexcept { return fetchAdd(1); }
	/// Atomic fetch and decrement
	/// Returns the previous value
	UNIGINE_INLINE Type fetchDec() noexcept { return fetchSub(1); }

	/// Atomic add
	/// Returns the post value
	UNIGINE_INLINE Type operator+=(long long v) noexcept { return fetchAdd(v) + v; }
	/// Atomic sub
	/// Returns the post value
	UNIGINE_INLINE Type operator-=(long long v) noexcept { return fetchSub(v) - v; }

	/// Atomic increment
	/// Returns the previous value
	UNIGINE_INLINE Type operator++(int) noexcept { return fetchInc(); }
	/// Atomic decrement
	/// Returns the previous value
	UNIGINE_INLINE Type operator--(int) noexcept { return fetchDec(); }

	/// Atomic increment
	/// Returns the post value
	UNIGINE_INLINE Type operator++() noexcept { return fetchInc() + 1; }
	/// Atomic decrement
	/// Returns the post value
	UNIGINE_INLINE Type operator--() noexcept { return fetchDec() - 1; }

private:
	static constexpr long long SIZE_TYPE = sizeof(typename std::conditional<std::is_void<typename std::remove_cv<typename std::remove_pointer<Type>::type>::type>::value, void*, typename std::remove_pointer<Type>::type>::type);

	volatile long long raw_value;
};

template <typename Type>
struct AtomicInteger final
{
	static constexpr bool isLockFree = true;
	using RawType = getRawType<Type>;

	AtomicInteger() = default;
	UNIGINE_INLINE AtomicInteger(Type v): raw_value(static_cast<RawType>(v)) {}
	UNIGINE_INLINE AtomicInteger(const AtomicInteger &v) noexcept
	{
		#ifdef _WIN32
			RawType new_v = v.raw_value;
			_ReadWriteBarrier();
			raw_value = new_v;
		#else
			Type new_v = v.fetch();
			store(new_v);
		#endif
	}

	UNIGINE_INLINE operator Type() const noexcept { return fetch(); }

	UNIGINE_INLINE RawType &getUnsafeRawValue() noexcept { return raw_value; }
	UNIGINE_INLINE const RawType &getUnsafeRawValue() const noexcept { return raw_value; }
	UNIGINE_INLINE RawType *getRawValuePtr() noexcept { return &raw_value; }
	UNIGINE_INLINE const RawType *getRawValuePtr() const noexcept { return &raw_value; }

	UNIGINE_INLINE Type &getUnsafeValue() noexcept { return *(reinterpret_cast<Type *>(&raw_value)); }
	UNIGINE_INLINE const Type &getUnsafeValue() const noexcept { return *(reinterpret_cast<const Type *>(&raw_value)); }
	UNIGINE_INLINE Type *getValuePtr() noexcept { return  reinterpret_cast<Type*>(&raw_value); }
	UNIGINE_INLINE const Type *getValuePtr() const noexcept { return reinterpret_cast<const Type*>(&raw_value); }

	/// Atomic read
	UNIGINE_INLINE auto fetch() const noexcept
	{
		#ifdef _WIN32
			RawType v = raw_value;
			_ReadWriteBarrier();
			return static_cast<Type>(v);
		#else
			return static_cast<Type>(__atomic_load_n(&raw_value, __ATOMIC_ACQUIRE));
		#endif
	}
	/// Atomic write
	UNIGINE_INLINE void store(Type v) noexcept
	{
		#ifdef _WIN32
			_ReadWriteBarrier();
			raw_value = static_cast<RawType>(v);
		#else
			__atomic_store_n(&raw_value, static_cast<RawType>(v), __ATOMIC_RELEASE);
		#endif
	}

	UNIGINE_INLINE Type operator=(Type v) noexcept { store(v); return v; }
	UNIGINE_INLINE Type operator=(const AtomicInteger &v) noexcept
	{
		#ifdef _WIN32
			RawType new_v = v.raw_value;
			_ReadWriteBarrier();
			raw_value = new_v;
			return static_cast<Type>(new_v);
		#else
			Type new_v = v.fetch();
			store(new_v);
			return new_v;
		#endif
	}

	/// Atomic swap
	UNIGINE_INLINE Type swap(Type v) noexcept
	{
		return static_cast<Type>((AtomicSwap(&raw_value, static_cast<RawType>(v))));
	}
	/// Atomic CAS (Compare-And-Swap)
	UNIGINE_INLINE bool compareAndSwap(Type old_value, Type new_value) noexcept
	{
		return AtomicCAS(&raw_value, static_cast<RawType>(old_value), static_cast<RawType>(new_value));
	}
	/// Spinlock, based on atomic CAS.
	UNIGINE_INLINE void spinLock(Type old_value, Type new_value) noexcept
	{
		SpinLock(&raw_value, static_cast<RawType>(old_value), static_cast<RawType>(new_value));
	}
	/// Wait until the atom equals the value (used spinLock)
	UNIGINE_INLINE void waitValue(Type v) noexcept
	{
		WaitLock(&raw_value, static_cast<RawType>(v));
	}

	/// Atomic fetch and add
	/// Returns the previous value
	UNIGINE_INLINE Type fetchAdd(Type v) noexcept
	{
		return static_cast<Type>(AtomicAdd(&raw_value, static_cast<RawType>(v)));
	}
	/// Atomic fetch and sub
	/// Returns the previous value
	UNIGINE_INLINE Type fetchSub(Type v) noexcept
	{
		return static_cast<Type>(AtomicAdd(&raw_value, -static_cast<RawType>(v)));
	}
	/// Atomic fetch and and
	/// Returns the previous value
	UNIGINE_INLINE Type fetchAnd(Type v) noexcept
	{
		return static_cast<Type>(AtomicAnd(&raw_value, static_cast<RawType>(v)));
	}
	/// Atomic fetch and or
	/// Returns the previous value
	UNIGINE_INLINE Type fetchOr(Type v) noexcept
	{
		return static_cast<Type>(AtomicOr(&raw_value, static_cast<RawType>(v)));
	}
	/// Atomic fetch and xor
	/// Returns the previous value
	UNIGINE_INLINE Type fetchXor(Type v) noexcept
	{
		return static_cast<Type>(AtomicXor(&raw_value, static_cast<RawType>(v)));
	}
	/// Atomic fetch and increment
	/// Returns the previous value
	UNIGINE_INLINE Type fetchInc() noexcept
	{
		return static_cast<Type>(AtomicInc(&raw_value));
	}
	/// Atomic fetch and decrement
	/// Returns the previous value
	UNIGINE_INLINE Type fetchDec() noexcept
	{
		return static_cast<Type>(AtomicDec(&raw_value));
	}

	/// Atomic and
	/// Returns the post value
	UNIGINE_INLINE Type operator+=(Type v) noexcept
	{
		return static_cast<Type>(AtomicAdd(&raw_value, static_cast<RawType>(v)) + static_cast<RawType>(v));
	}
	/// Atomic sub
	/// Returns the post value
	UNIGINE_INLINE Type operator-=(Type v)
	{
		return static_cast<Type>(AtomicAdd(&raw_value, -static_cast<RawType>(v)) - static_cast<RawType>(v));
	}
	/// Atomic increment
	/// Returns the previous value
	UNIGINE_INLINE Type operator++(int) noexcept
	{
		return static_cast<Type>(AtomicInc(&raw_value));
	}
	/// Atomic decrement
	/// Returns the previous value
	UNIGINE_INLINE Type operator--(int) noexcept
	{
		return static_cast<Type>(AtomicDec(&raw_value));
	}
	/// Atomic increment
	/// Returns the post value
	UNIGINE_INLINE Type operator++() noexcept
	{
		return static_cast<Type>(AtomicInc(&raw_value) + 1);
	}
	/// Atomic decrement
	/// Returns the post value
	UNIGINE_INLINE Type operator--() noexcept
	{
		return static_cast<Type>(AtomicDec(&raw_value) - 1);
	}
	/// Atomic and
	/// Returns the post value
	UNIGINE_INLINE Type operator&=(Type v) noexcept
	{
		return static_cast<Type>(AtomicAnd(&raw_value, static_cast<RawType>(v)) & static_cast<RawType>(v));
	}
	/// Atomic or
	/// Returns the post value
	UNIGINE_INLINE Type operator|=(Type v) noexcept
	{
		return static_cast<Type>(AtomicOr(&raw_value, static_cast<RawType>(v)) | static_cast<RawType>(v));
	}
	/// Atomic xor
	/// Returns the post value
	UNIGINE_INLINE Type operator^=(Type v) noexcept
	{
		return static_cast<Type>(AtomicXor(&raw_value, static_cast<RawType>(v)) ^ static_cast<RawType>(v));
	}

private:
	volatile RawType raw_value;
};

template<typename T>
struct is_atomic_lock_free
{
	static constexpr bool value = sizeof(T) <= 8;
};

template<typename T>
struct is_atomic_integer
{
	static constexpr bool value = std::is_integral<T>::value && !std::is_same<T, bool>::value;
};

template <typename T>
using conditional_atomic_pointer = typename std::conditional_t<std::is_pointer<T>::value, AtomicPointer<T>, AtomicLockFree<T>>;
template <typename T>
using conditional_atomic_integer = typename std::conditional_t<is_atomic_integer<T>::value, AtomicInteger<T>, conditional_atomic_pointer<T>>;
template <typename T>
using Atomic = typename std::conditional_t<is_atomic_lock_free<T>::value, conditional_atomic_integer<std::remove_cv_t<T>>, AtomicWithMutex<std::remove_cv_t<T>>>;

};

template <typename T>
using Atomic = AtomicImpl::Atomic<T>;

using AtomicBool = Atomic<bool>;

using AtomicInt8 = Atomic<char>;
using AtomicInt16 = Atomic<short>;
using AtomicInt32 = Atomic<int>;
using AtomicInt64 = Atomic<long long>;

using AtomicUInt8 = Atomic<unsigned char>;
using AtomicUInt16 = Atomic<unsigned short>;
using AtomicUInt32 = Atomic<unsigned int>;
using AtomicUInt64 = Atomic<unsigned long long>;

using AtomicFloat = Atomic<float>;
using AtomicDouble = Atomic<double>;

////////////////////////////////////////////////////////////////////////////////
/// CPUShader
////////////////////////////////////////////////////////////////////////////////
class UNIGINE_API ThreadsPool
{
public:
	enum class FrameSyncMode: char
	{
		Disabled,
		Swap,
	};
	enum class Priority: char
	{
		Critical = 0,
		Highest = 1,
		AboveNormal = 2,
		Normal = 3,
		BelowNormal = 4,
		Lowest = 5,
		Background = 6,
		Idle = 7,

		Landscape = Highest,
		Streaming = Background,
	};
	static constexpr int NUM_PRIORITY = (int)Priority::Idle + 1;

	enum class PoolThread: char
	{
		Main,
		Sync,
		Async,
		Critical,
		Common,
		Background,
		FileStream,
		GPUStream,
		RenderFlush,
	};

	enum
	{
		PRIORITY_MASK_CRITICAL = 1 << (int)Priority::Critical,
		PRIORITY_MASK_HIGHEST = 1 << (int)Priority::Highest,
		PRIORITY_MASK_ABOVE_NORMAL = 1 << (int)Priority::AboveNormal,
		PRIORITY_MASK_NORMAL = 1 << (int)Priority::Normal,
		PRIORITY_MASK_BELOW_NORMAL = 1 << (int)Priority::BelowNormal,
		PRIORITY_MASK_LOWEST = 1 << (int)Priority::Lowest,
		PRIORITY_MASK_BACKGROUND = 1 << (int)Priority::Background,
		PRIORITY_MASK_IDLE = 1 << (int)Priority::Idle,

		PRIORITY_MASK_ALL =	PRIORITY_MASK_CRITICAL |
							PRIORITY_MASK_HIGHEST |
							PRIORITY_MASK_ABOVE_NORMAL |
							PRIORITY_MASK_NORMAL |
							PRIORITY_MASK_BELOW_NORMAL |
							PRIORITY_MASK_LOWEST |
							PRIORITY_MASK_BACKGROUND |
							PRIORITY_MASK_IDLE,

		PRIORITY_MASK_LANDSCAPE = 1 << (int)Priority::Landscape,
		PRIORITY_MASK_STREAMING = 1 << (int)Priority::Streaming,
	};

	enum
	{
		POOL_THREAD_MASK_MAIN = 1 << (int)PoolThread::Main,
		POOL_THREAD_MASK_SYNC = 1 << (int)PoolThread::Sync,
		POOL_THREAD_MASK_ASYNC = 1 << (int)PoolThread::Async,
		POOL_THREAD_MASK_CRITICAL = 1 << (int)PoolThread::Critical,
		POOL_THREAD_MASK_COMMON = 1 << (int)PoolThread::Common,
		POOL_THREAD_MASK_BACKGROUND = 1 << (int)PoolThread::Background,
		POOL_THREAD_MASK_FILE_STREAM = 1 << (int)PoolThread::FileStream,
		POOL_THREAD_MASK_GPU_STREAM = 1 << (int)PoolThread::GPUStream,
		POOL_THREAD_MASK_RENDER_FLUSH = 1 << (int)PoolThread::RenderFlush,

		POOL_THREAD_MASK_ALL =	POOL_THREAD_MASK_MAIN |
								POOL_THREAD_MASK_SYNC |
								POOL_THREAD_MASK_ASYNC |
								POOL_THREAD_MASK_CRITICAL |
								POOL_THREAD_MASK_COMMON |
								POOL_THREAD_MASK_BACKGROUND |
								POOL_THREAD_MASK_FILE_STREAM |
								POOL_THREAD_MASK_GPU_STREAM |
								POOL_THREAD_MASK_RENDER_FLUSH,
	};

	static int isInitialized() noexcept;

	static int getCurrentThreadIndex();
	static int getNumAllThreads();
	static int getNumAsyncThreads();
	static int getNumSyncThreads();
	static int getNumDefaultAsyncThreads();
	static int getNumDefaultSyncThreads();

	static void frameSync();
	static bool yield(int pool_mask = POOL_THREAD_MASK_ALL, int priority_mask = PRIORITY_MASK_ALL, bool only_frame_sync = false);
	static bool runProcess(int pool_mask = POOL_THREAD_MASK_ALL, int priority_mask = PRIORITY_MASK_ALL, bool only_frame_sync = true);
	static void forceWakeAll();

	static void runNewThread(CallbackBase *process, int priority = -3);

public:  // only internal methods

	static bool internal_init();
	static void internal_init_thread_main();
	static void update_main_thread();

};

class UNIGINE_API CPUShader
{
public:
	using Priority = ThreadsPool::Priority;
	using FrameSyncMode = ThreadsPool::FrameSyncMode;
	static constexpr int NUM_PRIORITY = ThreadsPool::NUM_PRIORITY;

	enum class PoolType: char
	{
		Auto,
		Async,
		Sync,
		Critical,
		Common,
		Background,
		RenderFlush,
		FileStream,
		GPUStream,
	};

	enum class WaitMode: char
	{
		Auto,
		Full,
	};

	UNIGINE_INLINE CPUShader() {};
	UNIGINE_INLINE CPUShader(Priority priority_): priority(priority_) {}
	UNIGINE_INLINE CPUShader(PoolType pool_): pool(pool_) {}
	UNIGINE_INLINE CPUShader(FrameSyncMode frame_sync_): frame_sync(frame_sync_) {}
	UNIGINE_INLINE CPUShader(PoolType pool_, Priority priority_): pool(pool_), priority(priority_) {}
	UNIGINE_INLINE CPUShader(PoolType pool_, FrameSyncMode frame_sync_): pool(pool_), frame_sync(frame_sync_) {}
	UNIGINE_INLINE CPUShader(Priority priority_, FrameSyncMode frame_sync_): priority(priority_), frame_sync(frame_sync_) {}
	UNIGINE_INLINE CPUShader(PoolType pool_, Priority priority_, FrameSyncMode frame_sync_): pool(pool_), priority(priority_), frame_sync(frame_sync_) {}
	UNIGINE_INLINE CPUShader(PoolType pool_, Priority priority_, FrameSyncMode frame_sync_, WaitMode wait_mode_): pool(pool_), priority(priority_), frame_sync(frame_sync_), wait_mode(wait_mode_) {}
	virtual ~CPUShader()
	{
		wait();
		BackoffSpinner spinner;
		while ((!isDone()) && (!Thread::isShutdown()))
			spinner.spin();
	}

	UNIGINE_INLINE void runSync(int num_threads_ = -1)
	{
		assert(ThreadsPool::isInitialized() && "CPUShader::runSync(): ThreadsPool is not initialized");
		if (Thread::isShutdown())
			return;
		if (num_threads_ == 0)
			return;
		wait();
		num_threads = static_cast<short>(num_threads_);
		run(false);
	}
	UNIGINE_INLINE void runAsync(int num_threads_ = -1)
	{
		assert(ThreadsPool::isInitialized() && "CPUShader::runAsync(): ThreadsPool is not initialized");
		if (Thread::isShutdown())
			return;
		if (num_threads_ == 0)
			return;
		wait();
		num_threads = static_cast<short>(num_threads_);
		run(true);
	}

	void wait();
	void doProcess();

	UNIGINE_INLINE bool isDone() const { return AtomicGet(&is_done); }
	UNIGINE_INLINE bool isRunning() const { return (AtomicGet(&active_threads) != -1) && !Thread::isShutdown(); }
	UNIGINE_INLINE bool isAutoDestroy() const { return auto_destroy; }
	UNIGINE_INLINE int getNumThreads() const { return num_threads; }

	UNIGINE_INLINE PoolType getPool() const { return pool; }
	UNIGINE_INLINE void setPool(PoolType p)
	{
		if (p == pool)
			return;
		wait();
		pool = p;
	}

	UNIGINE_INLINE Priority getPriority() const { return priority; }
	UNIGINE_INLINE void setPriority(Priority p)
	{
		if (p == priority)
			return;
		wait();
		priority = p;
	}

	UNIGINE_INLINE FrameSyncMode getFrameSync() const { return frame_sync; }
	UNIGINE_INLINE void setFrameSync(FrameSyncMode f)
	{
		if (f == frame_sync)
			return;
		wait();
		frame_sync = f;
	}

	UNIGINE_INLINE WaitMode getWaitMode() const { return wait_mode; }
	UNIGINE_INLINE void setWaitMode(WaitMode f)
	{
		if (f == wait_mode)
			return;
		wait();
		wait_mode = f;
	}

	virtual void process(int thread_num, int num_threads) = 0;
	virtual void done() {}

private:
	friend struct QueueCPUShaders;

	void run(bool async);
	void do_process();
	void do_process(int id);

	volatile short num_threads{0};
	volatile short active_threads{-1};
	volatile short queue_cur{-1};
	volatile short queue_num{-1};
	volatile PoolType cur_pool{PoolType::Auto};
	volatile PoolType pool{PoolType::Auto};
	volatile Priority priority{Priority::Normal};
	volatile FrameSyncMode frame_sync{FrameSyncMode::Swap};
	volatile WaitMode wait_mode{WaitMode::Auto};
	volatile bool is_done{true};

protected:
	bool auto_destroy{false};
};

class UNIGINE_API CPUTask
{
public:
	using Priority = ThreadsPool::Priority;
	using FrameSyncMode = ThreadsPool::FrameSyncMode;
	using PoolThread = ThreadsPool::PoolThread;
	static constexpr int NUM_PRIORITY = ThreadsPool::NUM_PRIORITY;

	UNIGINE_INLINE CPUTask() {}
	UNIGINE_INLINE CPUTask(const CPUTask &task): priority(task.priority), frame_sync(task.frame_sync) {}

	UNIGINE_INLINE CPUTask(Priority priority_): priority(priority_) {}
	UNIGINE_INLINE CPUTask(FrameSyncMode frame_sync_): frame_sync(frame_sync_) {}
	UNIGINE_INLINE CPUTask(Priority priority_, FrameSyncMode frame_sync_): priority(priority_), frame_sync(frame_sync_) {}

	virtual ~CPUTask() {}

	UNIGINE_INLINE PoolThread getRunPoolThread() const { return thread; }

	UNIGINE_INLINE Priority getPriority() const { return priority; }
	UNIGINE_INLINE void setPriority(Priority v) { priority = v; }

	UNIGINE_INLINE FrameSyncMode getFrameSync() const { return frame_sync; }
	UNIGINE_INLINE void setFrameSync(FrameSyncMode v) { frame_sync = v; }

	static int generateID();

	void run(PoolThread thread);
	void runMainThread();
	void runSyncThread();
	void runAsyncThread();
	void runCriticalThread();
	void runCommonThread();
	void runBackgroundThread();
	void runRenderFlushThread();
	void runFileStreamThread();
	void runGPUStreamThread();

	void unsafeStop();

	void destroy();

	virtual void process() = 0;

private:
	friend struct QueueCPUTasks;

	void do_process();

	PoolThread thread{};
	Priority priority{Priority::Normal};
	FrameSyncMode frame_sync{FrameSyncMode::Disabled};
};

class ProcessTask: public CPUTask
{
public:
	ProcessTask(PoolThread pool_ = PoolThread::Background) : pool(pool_) {}
	virtual ~ProcessTask() {}

	void runProcess()
	{
		if (Thread::isShutdown())
			return;
		if (run_mutex.tryLock())
			run(pool);
	}

protected:
	Mutex run_mutex;
	PoolThread pool;

	virtual void do_process() = 0;
	virtual bool is_active() = 0;

	void process() override
	{
		while (true)
		{
			do_process();
			run_mutex.unlock();
			if (is_active() == false)
				return;
			if (run_mutex.tryLock() == false)
				return;
		}
	}
};

template<typename State, typename Process, typename Destroy>
class CPUShaderCallable final: public CPUShader
{
public:
	UNIGINE_INLINE CPUShaderCallable(Process func_process_, Destroy func_destroy_, PoolType pool_ = PoolType::Auto, Priority priority_ = Priority::Normal, FrameSyncMode frame_sync_ = FrameSyncMode::Swap, WaitMode wait_mode_ = WaitMode::Auto)
		: CPUShader(pool_, priority_, frame_sync_, wait_mode_)
		, func_process(func_process_)
		, func_destroy(func_destroy_)
		, state()
	{}
	~CPUShaderCallable() override
	{
		wait();
		func_destroy(state);
	}
	void process(int thread_num, int num_threads) override { func_process(this, thread_num, num_threads); }

private:
	Process func_process;
	Destroy func_destroy;
	State state;
};

template<typename Process>
class CPUShaderCallableStateless final: public CPUShader
{
public:
	UNIGINE_INLINE CPUShaderCallableStateless(Process func_process_, PoolType pool_ = PoolType::Auto, Priority priority_ = Priority::Normal, FrameSyncMode frame_sync_ = FrameSyncMode::Swap, WaitMode wait_mode_ = WaitMode::Auto)
		: CPUShader(pool_, priority_, frame_sync_, wait_mode_), func_process(func_process_)
	{}
	~CPUShaderCallableStateless() override { wait(); }

	void process(int thread_num, int num_threads) override
	{
		func_process(this, thread_num, num_threads);
	}

private:
	Process func_process;
};

template <typename State, typename Process, typename Destroy>
CPUShader *makeCPUShader(Process func_process, Destroy func_destroy, CPUShader::PoolType pool_ = CPUShader::PoolType::Auto, CPUShader::Priority priority_ = CPUShader::Priority::Normal, CPUShader::FrameSyncMode frame_sync_ = CPUShader::FrameSyncMode::Swap, CPUShader::WaitMode wait_mode_ = CPUShader::WaitMode::Auto)
{
	return new CPUShaderCallable<State, Process, Destroy>(std::move(func_process), std::move(func_destroy), pool_, priority_, frame_sync_, wait_mode_);
}

template <typename State, typename Process>
CPUShader *makeCPUShader(Process func_process, CPUShader::PoolType pool_ = CPUShader::PoolType::Auto, CPUShader::Priority priority_ = CPUShader::Priority::Normal, CPUShader::FrameSyncMode frame_sync_ = CPUShader::FrameSyncMode::Swap, CPUShader::WaitMode wait_mode_ = CPUShader::WaitMode::Auto)
{
	const auto dummy_destroy = [](const State &) {};
	return new CPUShaderCallable<State, Process, decltype(dummy_destroy)>(std::move(func_process), dummy_destroy, pool_, priority_, frame_sync_, wait_mode_);
}

template <typename Process>
CPUShader *makeCPUShaderStateless(Process func_process, CPUShader::PoolType pool_ = CPUShader::PoolType::Auto, CPUShader::Priority priority_ = CPUShader::Priority::Normal, CPUShader::FrameSyncMode frame_sync_ = CPUShader::FrameSyncMode::Swap, CPUShader::WaitMode wait_mode_ = CPUShader::WaitMode::Auto)
{
	return new CPUShaderCallableStateless<Process>(std::move(func_process), pool_, priority_, frame_sync_, wait_mode_);
}

template <typename Process>
auto makeScopeCPUShaderStateless(Process func_process, CPUShader::PoolType pool_ = CPUShader::PoolType::Auto, CPUShader::Priority priority_ = CPUShader::Priority::Normal, CPUShader::FrameSyncMode frame_sync_ = CPUShader::FrameSyncMode::Swap, CPUShader::WaitMode wait_mode_ = CPUShader::WaitMode::Auto)
{
	return CPUShaderCallableStateless<Process>(std::move(func_process), pool_, priority_, frame_sync_, wait_mode_);
}

template <typename Process>
void runSyncMultiThreadFunc(Process func_process, int num_threads = -1, CPUShader::PoolType pool_ = CPUShader::PoolType::Auto, CPUShader::Priority priority_ = CPUShader::Priority::Normal, CPUShader::FrameSyncMode frame_sync_ = CPUShader::FrameSyncMode::Swap, CPUShader::WaitMode wait_mode_ = CPUShader::WaitMode::Auto)
{
	CPUShaderCallableStateless<Process> shader(std::move(func_process), pool_, priority_, frame_sync_, wait_mode_);
	shader.runSync(num_threads);
}



template<typename Callable, typename Destroy>
class AsyncTaskCallable: public CPUTask
{
public:
	AsyncTaskCallable(Callable callable_, Destroy destroy_, Priority priority_ = Priority::Normal, FrameSyncMode frame_sync_ = FrameSyncMode::Disabled)
		: CPUTask(priority_, frame_sync_)
		, callable(callable_)
		, destroy(destroy_)
	{}
	~AsyncTaskCallable() override { destroy(); }
	void process() override { callable(this); }

private:
	Callable callable;
	Destroy destroy;
};

template <typename Callable>
CPUTask *makeCPUTask(Callable callable, CPUTask::Priority priority_ = CPUTask::Priority::Normal, CPUTask::FrameSyncMode frame_sync_ = CPUTask::FrameSyncMode::Disabled)
{
	const auto dummy_destroy = []() {};
	const auto task = new AsyncTaskCallable<Callable, decltype(dummy_destroy)>(std::move(callable), dummy_destroy, priority_, frame_sync_);
	return task;
}

template <typename Callable, typename Destroy>
CPUTask *makeCPUTask(Callable callable, Destroy destroy, CPUTask::Priority priority_ = CPUTask::Priority::Normal, CPUTask::FrameSyncMode frame_sync_ = CPUTask::FrameSyncMode::Disabled)
{
	const auto task = new AsyncTaskCallable<Callable, Destroy>(std::move(callable), std::move(destroy), priority_, frame_sync_);
	return task;
}


} /* namespace Unigine */
