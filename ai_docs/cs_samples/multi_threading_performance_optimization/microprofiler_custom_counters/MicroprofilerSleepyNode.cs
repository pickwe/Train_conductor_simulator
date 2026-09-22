// Demonstrates microprofiler instrumentation across component lifecycle methods.
// Each method is wrapped with profiler markers and includes artificial delays
// to visualize timing in the profiler output.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unigine;

// Profiles component methods with artificial delays for visualization.
public partial class MicroprofilerSleepyNode : Component
{
	// Simulates 1ms workload for profiler visualization.
	private void Init()
	{
		int id = Profiler.BeginMicro("MicroprofilerSleepyUnit::Init()");
		Sleep(1000);
		Profiler.EndMicro(id);
	}

	// Simulates 2ms workload for profiler visualization.
	private void UpdateAsyncThread()
	{
		int id = Profiler.BeginMicro("MicroprofilerSleepyUnit::UpdateAsyncThread()");
		Sleep(2000);
		Profiler.EndMicro(id);
	}

	// Simulates 0.5ms workload for profiler visualization.
	private void UpdateSyncThread()
	{
		int id = Profiler.BeginMicro("MicroprofilerSleepyUnit::UpdateSyncThread()");
		Sleep(500);
		Profiler.EndMicro(id);
	}

	// Rotates node and simulates 0.75ms workload for profiler visualization.
	private void Update()
	{
		int id = Profiler.BeginMicro("MicroprofilerSleepyUnit::Update()");
		// Rotate node around Z axis
		node.Rotate(0.0f, 0.0f, 3.0f);
		Sleep(750);
		Profiler.EndMicro(id);
	}

	// Simulates 0.5ms workload for profiler visualization.
	private void PostUpdate()
	{
		int id = Profiler.BeginMicro("MicroprofilerSleepyUnit::PostUpdate()");
		Sleep(500);
		Profiler.EndMicro(id);
	}

	// Simulates 0.02ms workload for profiler visualization.
	private void UpdatePhysics()
	{
		int id = Profiler.BeginMicro("MicroprofilerSleepyUnit::UpdatePhysics()");
		Sleep(20);
		Profiler.EndMicro(id);
	}

	// Simulates 0.01ms workload for profiler visualization.
	private void Swap()
	{
		int id = Profiler.BeginMicro("MicroprofilerSleepyUnit::Swap()");
		Sleep(10);
		Profiler.EndMicro(id);
	}

	// Simulates 1ms workload for profiler visualization.
	private void Shutdown()
	{
		int id = Profiler.BeginMicro("MicroprofilerSleepyUnit::Shutdown()");
		Sleep(1000);
		Profiler.EndMicro(id);
	}

	// Utility method to sleep for specified microseconds.
	private static void Sleep(int microseconds)
	{
		Thread.Sleep(new TimeSpan(0, 0, 0, 0, 0, microseconds));
	}
}
