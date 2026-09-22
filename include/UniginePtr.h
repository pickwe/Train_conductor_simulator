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

#include <UnigineMemory.h>
#include <UnigineThread.h>

#include <UnigineVector.h>
#include <UnigineHashMap.h>
#include <UnigineMap.h>
#include <UnigineSet.h>
#include <UnigineEvent.h>

#include <type_traits>

namespace Unigine
{

class UnigineBaseObject;
class APIInterface;

#ifndef UNIGINE_EMBED
	#define UNIGINE_BASE_OBJECT \
		Unigine::APIInterface *create_interface() override;
	#define UNIGINE_BASE_OBJECT_IMMEDIATE_DELETE \
		Unigine::APIInterface *create_interface() override; \
		void delete_safe() override { delete_force(); }
#else
	#define UNIGINE_BASE_OBJECT \
		Unigine::APIInterface *create_interface() override { return nullptr; }
	#define UNIGINE_BASE_OBJECT_IMMEDIATE_DELETE \
		Unigine::APIInterface *create_interface() override { return nullptr; } \
		void delete_safe() override { delete_force(); }
#endif

class UNIGINE_API UnigineBaseObject
{
public:
	UnigineBaseObject() {}
	virtual ~UnigineBaseObject();

	UnigineBaseObject(const UnigineBaseObject &) = delete;
	UnigineBaseObject &operator=(const UnigineBaseObject&) = delete;
	UnigineBaseObject(UnigineBaseObject &&other) noexcept;
	UnigineBaseObject &operator=(UnigineBaseObject &&other) noexcept;

	APIInterface *getInterface();

	template<typename T>
	T *getInterface() { return static_cast<T*>(getInterface()); }
	bool hasInterface() const { return api_interface != nullptr; }

	bool isDeletedLater() const { return AtomicGet(&deleted_later_id) != -1; }

	void deleteLater();
	void deleteForce();

	static int getNumDeletedLaterObjects();

public: //internal
	static void processDeleteQueue();

protected:
	friend APIInterface;

	virtual void delete_safe();
	void delete_later();
	void delete_force();

	virtual APIInterface * create_interface() = 0;

	APIInterface *volatile api_interface { nullptr };

	int deleted_later_id = -1;
};

#ifndef UNIGINE_EMBED
	template<class T, class = typename std::enable_if<!std::is_convertible<T*, UnigineBaseObject*>::value, void>::type>
	inline APIInterface *getAPIInterface(T *) { return nullptr; }
	inline APIInterface *getAPIInterface(UnigineBaseObject *object) { return object ? object->getInterface() : nullptr; }
#else
	template<class T>
	inline APIInterface *getAPIInterface(T *) { return nullptr; }
#endif

// api interface
class UNIGINE_API APIInterface
{
public:
	APIInterface() = default;
	virtual ~APIInterface() = 0;

	UNIGINE_INLINE void counterAPIInterfaceInc()
	{
		AtomicAdd(&counter, 1);
	}
	UNIGINE_INLINE void counterAPIInterfaceDec()
	{
		const int old_counter = AtomicAdd(&counter, -1);
		if (old_counter == 1)
		{
			if (isInternalObjectNull())
				delete this;
			else if (isAPIInterfaceOwner() && isInternalObjectValid())
				obj->delete_safe();
		}
	}
	UNIGINE_INLINE int getAPIInterfaceCounter() const { return AtomicGet(&counter); }

	UNIGINE_INLINE bool isOwner() const { return owner; }
	UNIGINE_INLINE int isValid() const { return obj != nullptr; }
	UNIGINE_INLINE int isNull() const { return obj == nullptr; }
	UNIGINE_INLINE int isDeleted() const { return is_deleted; }
	UNIGINE_INLINE void setAPIInterfaceOwner(bool owner_) { owner = owner_; }
	UNIGINE_INLINE bool isAPIInterfaceOwner() const { return owner; }

	template<typename T>
	UNIGINE_INLINE T *getInternalObject() const { return static_cast<T*>(obj); }
	UNIGINE_INLINE UnigineBaseObject *getInternalObject() const { return obj; }
	bool isInternalObjectValid() const { return obj != nullptr; }
	bool isInternalObjectNull() const { return obj == nullptr; }
	bool isInternalObjectDeleted() const { return is_deleted; }

	UNIGINE_INLINE Event<APIInterface *> &getEventAPIInterfaceDestroy() { return event_destroy.create(); }

protected:
	UNIGINE_INLINE void setInternalObject(UnigineBaseObject *const obj_)
	{
		obj = obj_;
		is_deleted = obj_->isDeletedLater();
	}

protected:
	friend UnigineBaseObject;
	template<typename OtherType>
	friend class Ptr;

	UNIGINE_INLINE void object_destructor()
	{
		is_deleted = 1;
		obj = nullptr;
		if (counter == 0)
			delete this;
	}

	UNIGINE_INLINE void delete_later()
	{
		if (isInternalObjectNull() || is_deleted)
			return;

		{
			ScopedSpinLockChar lock(deleted_mutex);
			if (is_deleted)
				return;
			is_deleted = 1;
		}

		obj->delete_later();
	}
	UNIGINE_INLINE void delete_force()
	{
		if (isInternalObjectNull() || is_deleted)
			return;

		{
			ScopedSpinLockChar lock(deleted_mutex);
			if (is_deleted)
				return;
			is_deleted = 1;
		}

		if (obj->isDeletedLater())
		{
			obj = nullptr;
		} else
		{
			obj->delete_force();
			// warning: 'this' may be already dead here
			// this->obj is nulled during APIInterface::object_destructor
		}
	}

	UnigineBaseObject *obj{nullptr};
	EventHolder<EventInvoker<APIInterface *>> event_destroy;

	volatile int counter{0};
	volatile char deleted_mutex{0};
	bool owner{false};
	volatile bool is_deleted = 0;
};

inline APIInterface::~APIInterface()
{
	if (event_destroy.isNull())
		return;
	event_destroy.get()->run(this);
}

namespace Internal
{

template<typename T> struct APIInterfaceHasDeleteHelper { static constexpr bool value = !T::HAS_OWNERSHIP; };
template<> struct APIInterfaceHasDeleteHelper<APIInterface> { static constexpr bool value = true; };

template<typename T, bool Enabled>
struct APIInterfaceDeleter
{
	void deleteLater() const { static_cast<const T *>(this)->delete_later(); }
	void deleteForce() const { static_cast<const T *>(this)->delete_force(); }
	void deleteForce() { static_cast<T *>(this)->delete_force(); }
};

template<typename T>
struct APIInterfaceDeleter<T, false>
{
	// Disabled for ownership objects. Use reference counter instead
	void deleteLater() const = delete;
	void deleteForce() const = delete;
	void deleteForce() = delete;
};

} // namespace Internal

// Smart pointer api interface
template <typename Type>
class Ptr : public Internal::APIInterfaceDeleter<Ptr<Type>, Internal::APIInterfaceHasDeleteHelper<Type>::value>
{

public:
	Ptr(std::nullptr_t): ptr(nullptr) {}
	Ptr(): ptr(nullptr) {}

	Ptr(UnigineBaseObject *obj, bool take_ownership)
	{
		ptr = nullptr;
		if (obj)
			ptr = static_cast<Type*>(obj->getInterface());
		if (ptr && take_ownership)
			api_interface()->setAPIInterfaceOwner(true);
		grab();
	}

	Ptr(const Ptr &pointer)
		: ptr(pointer.ptr)
	{
		grab();
	}

	Ptr(Ptr &&pointer)
		: ptr(pointer.ptr)
	{
		pointer.ptr = nullptr;
	}

	template<typename OtherType, class = typename std::enable_if<std::is_convertible<OtherType *, Type *>::value, void>::type>
	Ptr(const Ptr<OtherType> &pointer)
	{
		Type *new_val = static_cast<OtherType *>(pointer.ptr);
		ptr = new_val;
		grab();
	}

	template <typename OtherType, class = typename std::enable_if<std::is_convertible<OtherType *, Type *>::value, void>::type>
	Ptr(Ptr<OtherType> &&pointer)
	{
		Type *new_val = static_cast<OtherType *>(pointer.ptr);
		ptr = new_val;
		pointer.ptr = nullptr;
	}

	explicit Ptr(Type *pointer): ptr(pointer) { grab(); }

	~Ptr() { clear(); }

	Ptr &operator=(std::nullptr_t)
	{
		clear();
		return *this;
	}

	Ptr &operator=(const Ptr &pointer)
	{
		if (ptr == pointer.ptr)
			return *this;

		clear();
		ptr = pointer.ptr;
		grab();
		return *this;
	}

	Ptr &operator=(Ptr &&pointer)
	{
		if (this == &pointer)
			return *this;

		clear();
		ptr = pointer.ptr;
		pointer.ptr = nullptr;
		return *this;
	}

	template<typename OtherType, class = typename std::enable_if<std::is_convertible<OtherType *, Type *>::value, void>::type>
	Ptr<Type> &operator=(const Ptr<OtherType> &pointer)
	{
		Type *new_val = static_cast<OtherType *>(pointer.ptr);
		if (new_val == ptr)
			return *this;

		clear();
		ptr = new_val;
		grab();
		return *this;
	}

	template <typename OtherType, class = typename std::enable_if<std::is_convertible<OtherType *, Type *>::value, void>::type>
	Ptr<Type> &operator=(Ptr<OtherType> &&pointer)
	{
		Type *new_val = static_cast<OtherType *>(pointer.ptr);

		clear();
		ptr = new_val;
		pointer.ptr = nullptr;
		return *this;
	}

	template<typename OtherType>
	Ptr<Type> &operator=(const OtherType *pointer)
	{
		Type *new_val = static_cast<OtherType *>(pointer);
		if (new_val == ptr)
			return *this;

		clear();
		ptr = new_val;
		grab();
		return *this;
	}

	Type *get() const
	{
		if (!ptr || api_interface()->isInternalObjectNull())
			return nullptr;
		return ptr;
	}
	Type *operator->() const
	{
		assert(!isNull());
		return get();
	}
	Type &operator*() const
	{
		assert(!isNull());
		return *get();
	}

	void clear()
	{
		if (ptr)
			api_interface()->counterAPIInterfaceDec();
		ptr = 0;
	}

	template<typename T>
	T *getInternalObject() const
	{
		if (!ptr || api_interface()->isInternalObjectNull())
			return nullptr;
		return api_interface()->template getInternalObject<T>();
	}

	bool isValid() const { return get() != nullptr; }
	bool isNull() const { return get() == nullptr; }
	bool isDeleted() const { return isNull() || api_interface()->isInternalObjectDeleted(); }
	explicit operator bool() const { return isValid(); }

	UNIGINE_DECLARE_USE_MEMORY

private:
	template<typename OtherType>
	friend class Ptr;
	template<typename OtherType, bool Enabled>
	friend struct Internal::APIInterfaceDeleter;

	void grab()
	{
		if (ptr)
			api_interface()->counterAPIInterfaceInc();
	}
	void delete_later() const
	{
		if (!isNull())
			api_interface()->delete_later();
	}
	void delete_force() const
	{
		if (!isNull())
			api_interface()->delete_force();
	}
	void delete_force()
	{
		if (!isNull())
			api_interface()->delete_force();
		clear();
	}

	APIInterface *api_interface() const { return static_cast<APIInterface *>(const_cast<typename std::remove_const<Type>::type *> (ptr)); }

	Type *ptr;
};

template<typename PtrType, typename OtherType>
bool operator==(const Ptr<PtrType> &lhs, const OtherType &rhs) { return lhs.get() == rhs; }
template<typename PtrType, typename OtherType>
bool operator==(const OtherType &lhs, const  Ptr<PtrType> &rhs) { return lhs == rhs.get(); }
template<typename PtrType, typename OtherType>
bool operator!=(const Ptr<PtrType> &lhs, const OtherType &rhs) { return lhs.get() != rhs; }
template<typename PtrType, typename OtherType>
bool operator!=(const OtherType &lhs, const  Ptr<PtrType> &rhs) { return lhs != rhs.get(); }
template<typename LeftType, typename RightType>
bool operator==(const Ptr<LeftType> &lhs, const Ptr<RightType> &rhs) { return lhs.get() == rhs.get(); }
template<typename LeftType, typename RightType>
bool operator!=(const Ptr<LeftType> &lhs, const Ptr<RightType> &rhs) { return lhs.get() != rhs.get(); }
template<typename LeftType, typename RightType>
bool operator<(const Ptr<LeftType> &lhs, const Ptr<RightType> &rhs) { return lhs.get() < rhs.get(); }

template<typename To, typename From>
Ptr<To> static_ptr_cast(const Ptr<From> &ptr) { return Ptr<To>(static_cast<To*>(ptr.get())); }
template<typename To, typename From>
Ptr<To> dynamic_ptr_cast(const Ptr<From> &ptr) { return Ptr<To>(dynamic_cast<To *>(ptr.get())); }
template<typename To, typename From>
Ptr<To> checked_ptr_cast(const Ptr<From> &ptr)
{
	if (!To::convertible(ptr.get()))
		return nullptr;
	return Ptr<To>(static_cast<To *>(ptr.get()));
}

typedef Ptr<APIInterface> BaseObjectPtr;

template<typename ...Args>
class EventInterfaceInvoker : public EventInvoker<Args...>
{
public:

	void internal_set_data(EventBase *event, APIInterface *i, EventConnectionId(*f)(void *, void *, EventConnection *), EventConnection *connection)
	{
		engine_event = event;
		api_interface = i;
		connection_func = f;
		internal_connection = connection;
	}

	bool internal_need_init()
	{
		return engine_event == nullptr;
	}

	void internal_clear()
	{
		if (!this->callbacks.empty())
		{
			if (api_interface)
				api_interface->counterAPIInterfaceDec();
			while (this->callbacks.size())
				delete this->callbacks.takeLast();
		}

		engine_event = nullptr;
		api_interface = nullptr;
		connection_func = nullptr;
		if (internal_connection)
			internal_connection->disconnect();
		internal_connection = nullptr;
	}

	bool empty() const override
	{
		assert(engine_event != nullptr);
		return this->callbacks.empty() || engine_event->empty();
	}

	void setEnabled(bool mode) override
	{
		assert(engine_event != nullptr);
		EventBase::setEnabled(mode);
		engine_event->setEnabled(mode);
	}

	bool isEnabled() const override
	{
		assert(engine_event != nullptr);
		return engine_event->isEnabled();
	}

protected:

	EventConnectionId append(CallbackBase *c) override
	{
		assert(engine_event != nullptr);
		if (this->callbacks.empty())
		{
			if (api_interface)
				api_interface->counterAPIInterfaceInc();
			if (connection_func)
				connection_func(this, engine_event, internal_connection);
		}
		return EventBase::append(c);
	}

	bool remove(CallbackBase *callback) override
	{
		bool result = EventBase::remove(callback);
		if (result && this->callbacks.empty())
		{
			if (api_interface)
				api_interface->counterAPIInterfaceDec();
			if (internal_connection)
				internal_connection->disconnect();
		}
		return result;
	}

	bool remove(EventConnection *connection) override
	{
		bool result = EventBase::remove(connection);
		if (result && this->callbacks.empty())
		{
			if (api_interface)
				api_interface->counterAPIInterfaceDec();
			if (internal_connection)
				internal_connection->disconnect();
		}
		return result;
	}

	bool remove(uint32_t hash) override
	{
		bool result = EventBase::remove(hash);
		if (result && this->callbacks.empty())
		{
			if (api_interface)
				api_interface->counterAPIInterfaceDec();
			if (internal_connection)
				internal_connection->disconnect();
		}
		return result;
	}

private:

	APIInterface *api_interface{nullptr};
	EventBase *engine_event{nullptr};
	EventConnectionId(*connection_func)(void *, void *, EventConnection *){nullptr};
	EventConnection *internal_connection{nullptr};
};

template<typename T>
class UNIGINE_API EventInterfaceConnection : public EventConnection
{
public:
	EventInterfaceConnection() = default;
	EventInterfaceConnection(EventInterfaceConnection &&other) = delete;
	EventInterfaceConnection(const EventInterfaceConnection &other) = delete;
	EventInterfaceConnection &operator=(EventInterfaceConnection &&other) = delete;
	EventInterfaceConnection &operator=(const EventInterfaceConnection &other) = delete;
	~EventInterfaceConnection() = default;
	void internal_set_data(T *event_interface)
	{
		i = event_interface;
	}
protected:
	void clear() override
	{
		EventConnection::clear();
		if (i)
		{
			i->internal_clear();
			i = nullptr;
		}
	}
private:
	T *i{nullptr};
};

namespace Internal
{

template <typename Type>
struct PointerWrapper
{
	PointerWrapper(Type *ptr_): ptr(ptr_) {}
	PointerWrapper(Type &ref): ptr(&ref) {}

	operator typename std::remove_const<Type>::type *() const { return const_cast<typename std::remove_const<Type>::type *> (ptr); }
	operator typename std::remove_const<Type>::type &() const { assert(ptr != nullptr); return *const_cast<typename std::remove_const<Type>::type *> (ptr); }

	Type *ptr;
};

template <typename Type>
PointerWrapper<Type> WrapPointer(Type *ptr) { return PointerWrapper<Type>(ptr); }

template <typename Type>
PointerWrapper<Type> WrapPointer(Type &ref) { return PointerWrapper<Type>(ref); }

template <typename Type>
PointerWrapper<Type> GetInternalObject(APIInterface *obj)
{
	return PointerWrapper<Type>(obj ? obj->getInternalObject<Type>() : nullptr);
}
template <typename Type>
PointerWrapper<Type> GetInternalObject(const APIInterface *obj)
{
	return PointerWrapper<Type>(obj ? obj->getInternalObject<Type>() : nullptr);
}

template <typename Type, typename APIClass>
PointerWrapper<Type> GetInternalObject(const Ptr<APIClass> &obj)
{
	return GetInternalObject<Type>(obj.get());
}

namespace Conversion
{
template<typename To, typename From>
To api_convert(From from)
{
	return from;
}
template<typename To, typename From>
To api_convert(From *from)
{
	return To(from, false);
}
template<typename To, typename From>
To api_convert(Ptr<From> from)
{
	return GetInternalObject<typename std::remove_pointer<To>::type>(from.get());
}
} // namespace Conversion

template <typename From, typename To>
VectorStack<To> api_wrap(const Vector<From> &v)
{
	VectorStack<To> ret;
	ret.allocate(v.size());
	for (int i = 0; i < v.size(); i++)
		ret.appendFast(Conversion::api_convert<To>(v[i]));
	return ret;
}

// SetToSet
template <typename From, typename To>
Set<To> api_wrap(const Set<From> &v)
{
	Set<To> ret;
	auto end = v.end();
	for (auto it = v.begin(); it != end; ++it)
		ret.append(Conversion::api_convert<To>(it->key));
	return ret;
}

} // namespace Internal

#if (defined(__cplusplus) && __cplusplus >= 201703L) || (defined(_MSVC_LANG) && _MSVC_LANG >= 201703L)

template <bool UseDestroy, bool UseValidate, int UniqueObjectsLimit, int ChunkBytes, int LimitChunkFree, int Align>
struct HashImmutableMemoryPool
{
	using HashType = unsigned long long;

	static_assert(LimitChunkFree < ChunkBytes, "LimitChunkFree must be less than ChunkBytes");
	static_assert(UniqueObjectsLimit > 64, "UniqueObjetsLimit muster be more than 64");
	static constexpr int HashTableSize = (UniqueObjectsLimit * 3) / 2;
	static_assert(HashTableSize < INT_MAX, "HashTableSize must be less than INT_MAX");

	UNIGINE_INLINE const void *getPtr(int id) const { return id < 0 ? nullptr : data[id].fetch(); }
	UNIGINE_INLINE HashType getHash(int id) const { return id < 0 ? 0 : keys[id].fetch(); }

	UNIGINE_INLINE size_t getPoolMemoryUsage() const { return memory_usage + sizeof(HashImmutableMemoryPool); }

	UNIGINE_INLINE void destroy()
	{
		if (UseDestroy == false)
			return;

		ScopedLock lock(mutex_allocations);
		destroy_counter.fetchInc();

		for (int i = 0; i < allocations.size(); i++)
		{
			Memory::deallocate(allocations[i]);
			allocations[i] = nullptr;
		}
		allocations.destroy();

		for (int i = 0; i < HashTableSize; ++i)
		{
			keys[i].store(0);
			data[i].store(nullptr);
		}
		memory_usage = 0;
	}

	UNIGINE_INLINE void *alloc(int bytes)
	{
		if (Align && bytes % Align)
			bytes = bytes + (Align - bytes % Align);

		auto append_alloc = [&](void *ptr, size_t size)
		{
			memory_usage.fetchAdd(size);
			if (!UseDestroy)
				return;
			ScopedLock lock(mutex_allocations);
			allocations.append(ptr);
		};

		if (bytes > ChunkBytes)
		{
			void *ptr = Memory::allocate(bytes);
			append_alloc(ptr, bytes);
			return ptr;
		}

		thread_local Chunk *chunk{nullptr};
		thread_local Vector<Chunk *> free_chunks;
		thread_local Vector<int> free_size;
		thread_local int free_max{0};
		thread_local int counter{0};

		if (UseDestroy && destroy_counter != counter)
		{
			chunk = nullptr;
			free_chunks.clear();
			free_size.clear();
			free_max = 0;
			counter = destroy_counter;
		}

		if (chunk)
		{
			int free_bytes = chunk->getFree();
			if (bytes <= free_bytes)
			{
				return chunk->alloc(bytes);
			}

			if (LimitChunkFree && free_bytes >= LimitChunkFree)
			{
				free_chunks.append(chunk);
				free_size.append(free_bytes);
				if (free_max < free_bytes)
					free_max = free_bytes;
			}
		}

		if (LimitChunkFree && bytes <= free_max)
		{
			for (int i = 0; i < free_size.size(); i++)
			{
				if (bytes <= free_size[i])
				{
					chunk = free_chunks[i];
					free_chunks.removeFast(i);
					free_size.removeFast(i);
					return chunk->alloc(bytes);
				}
			}
		}

		chunk = new Chunk();
		append_alloc(chunk, sizeof(Chunk));
		return chunk->alloc(bytes);
	}

	template <class CopyFunc, class ValidateFunc>
	UNIGINE_INLINE int set(CopyFunc &&alloc_func, ValidateFunc &&validate_func, HashType hash)
	{
		assert(hash != 0);
		int id = (int)(hash % HashTableSize);
		for (int i = 0; i < HashTableSize; ++i)
		{
			HashType key = keys[id].fetch();
			if (key == 0)
			{
				if (keys[id].compareAndSwap(0, hash))
				{
					#ifndef NDEBUG
						int num = allocations_counter.fetchInc() + 1;
						assert(num < UniqueObjectsLimit && "HashImmutableMemoryPool overflow");
					#endif
					data[id].store(alloc_func());
					return id;
				}
				key = keys[id].fetch();
			}
			if (key == hash)
			{
				BackoffSpinner spinner;
				while (data[id].fetch() == nullptr)
					spinner.spin();
				if (UseValidate)
				{
					if (validate_func(data[id].fetch()) == false)
						continue;
				} else
				{
					assert(validate_func(data[id].fetch()) && "HashImmutableMemoryPool colision hash");
				}
				return id;
			}

			id = (id + 1) % HashTableSize;
		}

		assert(false && "HashImmutableMemoryPool overflow");
		return -1;
	}


private:
	struct Chunk
	{
		alignas(16) char data[ChunkBytes];
		int offset{0};

		UNIGINE_INLINE char *alloc(int bytes)
		{
			assert(bytes >= 0 && bytes <= getFree());
			char *ptr = data + offset;
			offset += bytes;
			return ptr;
		}

		UNIGINE_INLINE int getFree() const { return ChunkBytes - offset; };
	};

	Atomic<HashType> keys[HashTableSize]{};
	Atomic<const void *> data[HashTableSize]{};

	AtomicInt32 memory_usage{0};
	AtomicInt32 destroy_counter{0};
	#ifndef NDEBUG
		AtomicInt32 allocations_counter{0};
	#endif

	Mutex mutex_allocations;
	Vector<void *> allocations;
};

template <typename Type, auto &pool, typename HasherFunc = Hasher<Type>>
class ImmutablePoolPtr
{
public:
	using HashType = unsigned long long;

	ImmutablePoolPtr() = default;
	UNIGINE_INLINE ImmutablePoolPtr(const Type &value) { set(value); }
	UNIGINE_INLINE ImmutablePoolPtr(const Type *value) { set(value); }

	UNIGINE_INLINE bool isEmpty() const { return id < 0; }
	UNIGINE_INLINE bool isValid() const { return id >= 0; }

	UNIGINE_INLINE const Type *get() const { return reinterpret_cast<const Type *>(pool.getPtr(id)); }
	UNIGINE_INLINE HashType getHash() const { return pool.getHash(id); }
	UNIGINE_INLINE void setID(int id_) { id = id_; }
	UNIGINE_INLINE int getID() const { return id; }

	UNIGINE_INLINE static HashType calcHash(const Type &src)
	{
		HashType hash = (HashType)HasherFunc(src);
		return hash ? hash : 1;
	}

	UNIGINE_INLINE bool set(const Type *new_value)
	{
		if (new_value == nullptr)
		{
			clear();
			return true;
		}
		if (get() == new_value)
			return true;
		HashType new_hash = calcHash(*new_value);
		if (getHash() == new_hash)
			return true;

		auto alloc_func = [&]()
		{
			void *ptr = pool.alloc(sizeof(Type));
			new (ptr) Type(*new_value);
			return ptr;
		};
		auto validate_func = [&](const void *value)
		{
			return *reinterpret_cast<const Type *>(value) == *new_value;
		};
		id = pool.set(alloc_func, validate_func, new_hash);
		return isValid();
	}
	UNIGINE_INLINE void set(const Type &new_value) { set(&new_value); }
	UNIGINE_INLINE void set(const ImmutablePoolPtr &ptr) { id = ptr.id; }

	UNIGINE_INLINE void clear() { id = -1; }

	UNIGINE_INLINE void swap(ImmutablePoolPtr &v) { std::swap(id, v.id); }
	friend UNIGINE_INLINE void swap(ImmutablePoolPtr &v0, ImmutablePoolPtr &v1) { v0.swap(v1); }

	UNIGINE_INLINE const Type &operator*() const
	{
		assert(isValid() && "ImmutablePoolPtr::operator*(): pointer is NULL");
		return *get();
	}
	UNIGINE_INLINE const Type *operator->() const
	{
		assert(isValid() && "ImmutablePoolPtr::operator->(): pointer is NULL");
		return get();
	}

	UNIGINE_INLINE bool operator==(const ImmutablePoolPtr &o) const { return id == o.id; }
	UNIGINE_INLINE bool operator!=(const ImmutablePoolPtr &o) const { return id != o.id; }

protected:
	int id{-1};
};

#endif

} // namespace Unigine
