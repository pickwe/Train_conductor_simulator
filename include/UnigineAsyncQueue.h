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

#include "UnigineString.h"
#include "UnigineMesh.h"
#include "UnigineImage.h"
#include "UnigineNode.h"

namespace Unigine
{

//////////////////////////////////////////////////////////////////////////

class UNIGINE_API AsyncQueue
{
public:
	static bool isInitialized();

	enum ASYNC_THREAD
	{
		ASYNC_THREAD_MAIN = 0,
		ASYNC_THREAD_SYNC,
		ASYNC_THREAD_ASYNC,
		ASYNC_THREAD_COMMON,
		ASYNC_THREAD_CRITICAL,
		ASYNC_THREAD_BACKGROUND,
		ASYNC_THREAD_FILE_STREAM,
		ASYNC_THREAD_GPU_STREAM,
		ASYNC_THREAD_NEW,
		NUM_ASYNC_THREADS,
	};

	enum ASYNC_PRIORITY
	{
		ASYNC_PRIORITY_CRITICAL = 0,
		ASYNC_PRIORITY_HIGHEST = 1,
		ASYNC_PRIORITY_ABOVENORMAL = 2,
		ASYNC_PRIORITY_NORMAL = 3,
		ASYNC_PRIORITY_BELOWNORMAL = 4,
		ASYNC_PRIORITY_LOWEST = 5,
		ASYNC_PRIORITY_BACKGROUND = 6,
		ASYNC_PRIORITY_IDLE = 7,
	};
	static void runAsync(AsyncQueue::ASYNC_THREAD thread_type, CallbackBase *callback, AsyncQueue::ASYNC_PRIORITY priority = AsyncQueue::ASYNC_PRIORITY_BACKGROUND);
	static void runFrameAsyncMultiThread(CallbackBase2<int, int> *callback, int num_threads = -1);
	static void runFrameAsyncMultiThread(CallbackBase2<int, int> *callback, CallbackBase *callback_done, int num_threads = -1);
	static void runFrameSyncMultiThread(CallbackBase2<int, int> *callback, int num_threads = -1);
	static void runAsyncMultiThread(CallbackBase2<int, int> *callback, int num_threads = -1);
	static void runAsyncMultiThread(CallbackBase2<int, int> *callback, CallbackBase *callback_done, int num_threads = -1);
	static void runSyncMultiThread(CallbackBase2<int, int> *callback, int num_threads = -1);
	static bool isMainThread();
	static bool isPoolSyncThread();
	static bool isPoolAsyncThread();
	static bool isPoolCommonThread();
	static bool isPoolCriticalThread();
	static bool isPoolBackgroundThread();
	static bool isPoolRenderFlushThread();
	static bool isPoolFileStreamThread();
	static bool isPoolGPUStreamThread();
	static int loadFile(const char *name, int group = 0, float weight = 0.0f);
	static int removeFile(int id);
	static int forceFile(int id);
	static int checkFile(int id);
	static int loadImageInfo(const char *name, int group = 0, float weight = 0.0f);
	static int loadImage(const char *name, int group = 0, float weight = 0.0f);
	static int removeImage(int id);
	static int forceImage(int id);
	static int checkImage(int id);
	static Ptr<Image> getImage(int id);
	static Ptr<Image> takeImage(int id);
	static int loadMesh(const char *name, int group = 0, float weight = 0.0f);
	static int removeMesh(int id);
	static int forceMesh(int id);
	static int checkMesh(int id);
	static Ptr<Mesh> getMesh(int id);
	static Ptr<Mesh> takeMesh(int id);
	static int loadNode(const char *name, int group = 0, float weight = 0.0f);
	static int removeNode(int id);
	static int forceNode(int id);
	static int checkNode(int id);
	static Ptr<Node> getNode(int id);
	static Ptr<Node> takeNode(int id);
	static int getNodes(int id, Vector<Ptr<Node>> &nodes);
	static int takeNodes(int id, Vector<Ptr<Node>> &nodes);
	static float getTotalTime();
	static int getNumQueuedData();
	static int getNumQueuedFiles();
	static int getNumQueuedImages();
	static int getNumQueuedMeshes();
	static int getNumQueuedNodes();
	static int getNumQueuedResources();
	static int getNumLoadedData();
	static int getNumLoadedFiles();
	static int getNumLoadedImages();
	static int getNumLoadedMeshes();
	static int getNumLoadedNodes();
	static int getNumLoadedResources();
	static Event<const char *, int> &getEventFileLoaded();
	static Event<const char *, int> &getEventImageLoaded();
	static Event<const char *, int> &getEventMeshLoaded();
	static Event<const char *, int> &getEventNodeLoaded();
};

} // namespace Unigine
