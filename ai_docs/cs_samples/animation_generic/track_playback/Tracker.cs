// Provides a C# interface for the UnigineScript Tracker system. Tracks are loaded
// from .track files and managed via a wrapper script. Static methods allow adding,
// removing, and controlling track playback from any component.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Static tracker manager that wraps UnigineScript tracker functionality.
public partial class Tracker : Component
{
	// Path to the UnigineScript tracker wrapper file
	[ShowInEditor]
	[ParameterFile]
	private string trackerWrapperUsc = "";

	// List of track files to load on initialization
	[ShowInEditor]
	[ParameterFile(Filter = ".track")]
	private List<string> trackFiles = new List<string>();

	// Flag indicating if wrapper script is loaded
	private static bool isWrapperLoaded = false;

	// Function name variables for UnigineScript calls
	private static readonly Variable initFunc = new Variable("TrackerWrapper::init");
	private static readonly Variable shutdownFunc = new Variable("TrackerWrapper::shutdown");
	private static readonly Variable addTrackFunc = new Variable("TrackerWrapper::addTrack");
	private static readonly Variable removeTrackFunc = new Variable("TrackerWrapper::removeTrack");
	private static readonly Variable getMinTimeFunc = new Variable("TrackerWrapper::getMinTime");
	private static readonly Variable getMaxTimeFunc = new Variable("TrackerWrapper::getMaxTime");
	private static readonly Variable getUnitTimeFunc = new Variable("TrackerWrapper::getUnitTime");
	private static readonly Variable setFunc = new Variable("TrackerWrapper::set");

	// Mapping of track names to their IDs
	private static Dictionary<string, int> trackIDs = new Dictionary<string, int>();

	// Reusable variables for UnigineScript function calls
	private static Variable trackFileVar = new Variable("");
	private static Variable trackIdVar = new Variable(0);
	private static Variable timeVar = new Variable(0.0f);

	// Indicates whether the tracker system is ready for use
	public static bool IsInitialized { get; private set; } = false;

	// Adds a track file to the tracker and returns its ID.
	public static int AddTrack(string trackFile)
	{
		if (IsInitialized && FileSystem.IsFileExist(trackFile) && FileSystem.GetExtension(trackFile) == "track")
		{
			// Extract track name from file path
			string trackName = trackFile.Split('.')[0];
			string[] parts = trackName.Split('/');
			trackName = parts[parts.Length - 1];

			if (!trackIDs.ContainsKey(trackName))
			{
				// Call wrapper to add track and cache the returned ID
				trackFileVar.String = trackFile;
				int trackID = Engine.RunWorldFunction(addTrackFunc, trackFileVar).Int;
				trackIDs[trackName] = trackID;

				return trackID;
			}
			else
			{
				Log.Warning($"Tracker::AddTrack: {trackFile} already added\n");
				return trackIDs[trackName];
			}
		}

		return -1;
	}

	// Removes a track by its ID and cleans up the name mapping.
	public static void RemoveTrack(int trackID)
	{
		if (IsInitialized && trackIDs.ContainsValue(trackID))
		{
			trackIdVar.Int = trackID;
			Engine.RunWorldFunction(removeTrackFunc, trackIdVar);

			// Find and remove the name-to-ID mapping
			string itemToRemove = "";
			foreach (var pair in trackIDs)
			{
				if (pair.Value.Equals(trackID))
					itemToRemove = pair.Key;
			}

			trackIDs.Remove(itemToRemove);
		}
	}

	// Removes a track by its name.
	public static void RemoveTrack(string trackName)
	{
		if (IsInitialized && trackIDs.ContainsKey(trackName))
			RemoveTrack(trackIDs[trackName]);
	}

	// Checks if a track with the given name exists.
	public static bool ContainsTrack(string trackName)
	{
		if (IsInitialized)
			return trackIDs.ContainsKey(trackName);

		return false;
	}

	// Returns the ID for a track by name, or -1 if not found.
	public static int GetTrackID(string trackName)
	{
		if (IsInitialized && trackIDs.ContainsKey(trackName))
			return trackIDs[trackName];

		return -1;
	}

	// Returns the minimum time value for a track by ID.
	public static float GetMinTime(int trackID)
	{
		if (IsInitialized)
		{
			trackIdVar.Int = trackID;
			return Engine.RunWorldFunction(getMinTimeFunc, trackIdVar).Float;
		}

		return 0.0f;
	}

	// Returns the minimum time value for a track by name.
	public static float GetMinTime(string trackName)
	{
		if (IsInitialized && trackIDs.ContainsKey(trackName))
			return GetMinTime(trackIDs[trackName]);

		return 0.0f;
	}

	// Returns the maximum time value for a track by ID.
	public static float GetMaxTime(int trackID)
	{
		if (IsInitialized)
		{
			trackIdVar.Int = trackID;
			return Engine.RunWorldFunction(getMaxTimeFunc, trackIdVar).Float;
		}

		return 0.0f;
	}

	// Returns the maximum time value for a track by name.
	public static float GetMaxTime(string trackName)
	{
		if (IsInitialized && trackIDs.ContainsKey(trackName))
			return GetMaxTime(trackIDs[trackName]);

		return 0.0f;
	}

	// Returns the unit time (time per frame) for a track by ID.
	public static float GetUnitTime(int trackID)
	{
		if (IsInitialized)
		{
			trackIdVar.Int = trackID;
			return Engine.RunWorldFunction(getUnitTimeFunc, trackIdVar).Float;
		}

		return 0.0f;
	}

	// Returns the unit time for a track by name.
	public static float GetUnitTime(string trackName)
	{
		if (IsInitialized && trackIDs.ContainsKey(trackName))
			return GetUnitTime(trackIDs[trackName]);

		return 0.0f;
	}

	// Sets the current playback time for a track by ID.
	public static void SetTime(int trackID, float time)
	{
		if (IsInitialized)
		{
			timeVar.Float = time;
			trackIdVar.Int = trackID;
			Engine.RunWorldFunction(setFunc, trackIdVar, timeVar);
		}
	}

	// Sets the current playback time for a track by name.
	public static void SetTime(string trackName, float time)
	{
		if (IsInitialized && trackIDs.ContainsKey(trackName))
			SetTime(trackIDs[trackName], time);
	}

	// Tracker wrapper is validated and initialized before other components.
	[MethodInit(Order = int.MinValue)]
	private void Init()
	{
		// Verify tracker wrapper script is set as world script
		string trackerWrapperGUID = FileSystem.GuidToPath(FileSystem.GetGUID(trackerWrapperUsc));
		isWrapperLoaded = (World.ScriptName == trackerWrapperGUID);

		if (!isWrapperLoaded)
		{
			Log.WarningLine($"{trackerWrapperUsc} not setup as world script. Setup it in Editor.");
			return;
		}

		// Initialize tracker in UnigineScript wrapper
		Engine.RunWorldFunction(initFunc);
		IsInitialized = true;

		// Load all configured track files
		if (trackFiles != null)
		{
			foreach (var track in trackFiles)
				AddTrack(track);
		}
	}

	// Tracker resources are released after all other components.
	[MethodShutdown(Order = int.MaxValue)]
	private void Shutdown()
	{
		if (IsInitialized)
		{
			// Shutdown tracker in wrapper script
			Engine.RunWorldFunction(shutdownFunc);

			trackIDs.Clear();
			isWrapperLoaded = false;
			IsInitialized = false;
		}
	}
}
