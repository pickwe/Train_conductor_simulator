// Demonstrates track file playback using the Tracker system. Position, rotation,
// and scale tracks are loaded and played back with looping based on min/max time
// ranges. Tracks can be accessed by name or ID.

using System;
using System.Collections;
using System.Collections.Generic;
using Unigine;

// Plays back multiple animation tracks with automatic looping.
public partial class TrackPlayback : Component
{
	// Path to the scale animation track file
	[ShowInEditor]
	[ParameterFile(Filter = ".track")]
	private string scaleTrackPath = "";

	// Cached ID for position track
	private int positionTrackID = -1;
	// Cached ID for scale track
	private int scaleTrackID = -1;

	// Current playback time for position track
	private float positionTrackTime = 0.0f;
	// Current playback time for rotation track
	private float rotationTrackTime = 0.0f;
	// Current playback time for scale track
	private float scaleTrackTime = 0.0f;

	// Track IDs are resolved and initial playback times are set.
	private void Init()
	{
		if (!Tracker.IsInitialized)
			return;

		// Retrieve position track ID from pre-registered tracks
		if (Tracker.ContainsTrack("position_track"))
		{
			positionTrackID = Tracker.GetTrackID("position_track");
			positionTrackTime = Tracker.GetMinTime(positionTrackID);
		}

		// Initialize rotation track time using track name lookup
		if (Tracker.ContainsTrack("rotation_track"))
			rotationTrackTime = Tracker.GetMinTime("rotation_track");

		// Dynamically add scale track from file path
		scaleTrackID = Tracker.AddTrack(scaleTrackPath);
		if (scaleTrackID != -1)
			scaleTrackTime = Tracker.GetMinTime(scaleTrackID);
	}
	
	// Track times are advanced and looped each frame.
	private void Update()
	{
		if (!Tracker.IsInitialized)
			return;

		// Update position track using ID-based access
		if (positionTrackID != -1)
		{
			float minTime = Tracker.GetMinTime(positionTrackID);
			float maxTime = Tracker.GetMaxTime(positionTrackID);
			float unitTime = Tracker.GetUnitTime(positionTrackID);

			// Advance time and loop when reaching end
			positionTrackTime += Game.IFps / unitTime;
			if (positionTrackTime >= maxTime)
				positionTrackTime = minTime;

			Tracker.SetTime(positionTrackID, positionTrackTime);
		}

		// Update rotation track using name-based access
		if (Tracker.ContainsTrack("rotation_track"))
		{
			float minTime = Tracker.GetMinTime("rotation_track");
			float maxTime = Tracker.GetMaxTime("rotation_track");
			float unitTime = Tracker.GetUnitTime("rotation_track");

			rotationTrackTime += Game.IFps / unitTime;
			if (rotationTrackTime >= maxTime)
				rotationTrackTime = minTime;

			Tracker.SetTime("rotation_track", rotationTrackTime);
		}

		// Update scale track using ID-based access
		if (scaleTrackID != -1)
		{
			float minTime = Tracker.GetMinTime(scaleTrackID);
			float maxTime = Tracker.GetMaxTime(scaleTrackID);
			float unitTime = Tracker.GetUnitTime(scaleTrackID);

			scaleTrackTime += Game.IFps / unitTime;
			if (scaleTrackTime >= maxTime)
				scaleTrackTime = minTime;

			Tracker.SetTime(scaleTrackID, scaleTrackTime);
		}
	}
}
