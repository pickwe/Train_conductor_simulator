// Demonstrates various AsyncQueue task execution modes. Provides UI to test
// single-thread async tasks on different thread types, as well as multi-thread
// sync and async task execution with configurable thread counts.

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unigine;

// Tests different AsyncQueue task execution modes and threading options.
public partial class AsyncQueueTasksSample : Component
{
	// Sample description window for UI
	private SampleDescriptionWindow sampleDescriptionWindow = new SampleDescriptionWindow();

	// UI with thread type selector and execution mode buttons is created.
	void Init()
	{
		Console.Onscreen = true;

		// Create sample UI window
		sampleDescriptionWindow.createWindow(Gui.ALIGN_RIGHT);

		WidgetGroupBox parameters = sampleDescriptionWindow.getParameterGroupBox();

		// Thread type selector
		var asyncThreadTypeHBox = new WidgetHBox(5);
		parameters.AddChild(asyncThreadTypeHBox, Gui.ALIGN_EXPAND);

		var asyncThreadTypeLabel = new WidgetLabel("Task Thread Type");
		asyncThreadTypeHBox.AddChild(asyncThreadTypeLabel);

		// Available thread types for async execution
		var asyncThreadTypeCombobox = new WidgetComboBox();
		asyncThreadTypeCombobox.AddItem("BACKGORUND");
		asyncThreadTypeCombobox.AddItem("ASYNC");
		asyncThreadTypeCombobox.AddItem("GPU STREAM");
		asyncThreadTypeCombobox.AddItem("FILE STREAM");
		asyncThreadTypeCombobox.AddItem("MAIN");
		asyncThreadTypeCombobox.AddItem("NEW");
		asyncThreadTypeHBox.AddChild(asyncThreadTypeCombobox, Gui.ALIGN_EXPAND);

		// Single async task button
		var runAsyncButton = new WidgetButton("Run Async");
		runAsyncButton.EventClicked.Connect(() =>
		{
			// Run task asynchronously on selected thread type
			// Priority options: CRITICAL (high), DEFAULT (medium), BACKGROUND (low)
			AsyncQueue.RunAsync((AsyncQueue.ASYNC_THREAD)(asyncThreadTypeCombobox.CurrentItem), AsyncTask);
		});
		parameters.AddChild(runAsyncButton, Gui.ALIGN_EXPAND);

		var spacer = new WidgetSpacer();
		parameters.AddChild(spacer, Gui.ALIGN_EXPAND);

		// Thread count selector for multithread mode
		var multithreadHBox = new WidgetHBox(5);
		parameters.AddChild(multithreadHBox, Gui.ALIGN_EXPAND);

		var multithreadLabel = new WidgetLabel("Num threads");
		multithreadHBox.AddChild(multithreadLabel);

		var spinboxHBox = new WidgetHBox();
		var multithreadEditline = new WidgetEditLine();
		multithreadEditline.Editable = false;
		var multithreadSpinbox = new WidgetSpinBox();
		multithreadEditline.AddAttach(multithreadSpinbox);
		multithreadSpinbox.MinValue = 1;
		multithreadSpinbox.MaxValue = 20;
		multithreadSpinbox.Value = 1;
		spinboxHBox.AddChild(multithreadEditline);
		spinboxHBox.AddChild(multithreadSpinbox);
		multithreadHBox.AddChild(spinboxHBox, Gui.ALIGN_RIGHT);

		// Option to wait for completion within frame
		var frame_checkbox = new WidgetCheckBox("Wait for multithreaded task to complete in frame");
		parameters.AddChild(frame_checkbox, Gui.ALIGN_LEFT);

		// Async multithread: runs on multiple threads, non-blocking
		var run_async_multithread_button = new WidgetButton("Run Async Multithread");
		run_async_multithread_button.EventClicked.Connect(() =>
		{
			// Run a task in a multithread mode, current thread number and total amount of threads are passed to the callback
			// Does not block the thread from which it is called
			if (frame_checkbox.Checked)
				AsyncQueue.RunFrameAsyncMultiThread(MultithreadTask, multithreadSpinbox.Value);
			else
				AsyncQueue.RunAsyncMultiThread(MultithreadTask, multithreadSpinbox.Value);
		});
		parameters.AddChild(run_async_multithread_button, Gui.ALIGN_EXPAND);

		// Sync multithread: runs on multiple threads, blocks until all complete
		var run_sync_multithread_button = new WidgetButton("Run Sync Multithread");
		run_sync_multithread_button.EventClicked.Connect(() =>
		{
			// Run a task in a multithread mode, current thread number and total amount of threads are passed to the callback
			// Blocks the thread from which it was called (the calling thread will be unblocked after the task is completed in all threads)
			if (frame_checkbox.Checked)
				AsyncQueue.RunFrameSyncMultiThread(MultithreadTask, multithreadSpinbox.Value);
			else
				AsyncQueue.RunSyncMultiThread(MultithreadTask, multithreadSpinbox.Value);
		});
		parameters.AddChild(run_sync_multithread_button, Gui.ALIGN_EXPAND);
	}
	
	// Console and UI are cleaned up on shutdown.
	void Shutdown()
	{
		Console.Onscreen = false;
		sampleDescriptionWindow.shutdown();
	}

	// Simple async task that logs its thread ID.
	private void AsyncTask()
	{
		// Simulate work
		Thread.Sleep(200);

		Log.MessageLine("This is async task, thread id: " + Thread.CurrentThread.ManagedThreadId.ToString());
	}

	// Multithread task logs current thread index and total thread count.
	private void MultithreadTask(int currentThread, int totalThreads)
	{
		// Simulate work
		Thread.Sleep(200);

		Log.MessageLine($"This is multithread task(current thread: {currentThread}, total number of threads: {totalThreads}), thread id: " + Thread.CurrentThread.ManagedThreadId.ToString());
	}
}
