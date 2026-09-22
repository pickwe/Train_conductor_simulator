// Demonstrates sun movement control based on game time. Rotates the sun node
// continuously or manually, converting time to rotation angle. Fires events
// when time changes for other components to react.

using Unigine;
using static Unigine.MathLib;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Controls sun rotation based on game time with optional continuous movement.
public partial class SunController : Component
{
	public const string COMPONENT_DESCRIPTION = "This component controls sun based on its own game time and various adjustable parameters";

	// Enables automatic sun rotation over time
	[ShowInEditor]
	[Parameter(Title = "Is sun moving continuously")]
	public bool isContinuous = true;
	public bool IsContinuous { get { return isContinuous; } set { isContinuous = value; } }

	// Multiplier for time progression speed
	[ShowInEditor]
	[Parameter(Title = "Scale of continuous time rotation")]
	private float timescale = 2000.0f;
	public float Timescale { get { return timescale; } set { timescale = value; } }

	// Initial sun orientation for rotation offset
	private quat sunInitTilt = quat.IDENTITY;

	// Current time in seconds (initialized to noon)
	private double currentTime = 720 * 60;
	public double Time { get { return currentTime; } }

	// Maximum time value (24 hours in seconds)
	private const int maxTimeSec = 60 * 60 * 24;
	public int MaxTimeSec => maxTimeSec;

	// Event fired when time changes
	private EventInvoker<double> timeChangedEvent = new EventInvoker<double>();

	public Event<double> EventOnTimeChanged
	{
		get
		{
			return timeChangedEvent;
		}
	}

	// Sets current time and updates sun position immediately.
	public void SetTime(double time)
	{
		currentTime = time;
		RefreshSunPostion();
		timeChangedEvent.Run((int)currentTime);
	}

	// Initial sun rotation is stored for later calculations.
	private void Init()
	{
		sunInitTilt = node.GetWorldRotation();
	}

	// Time advances and sun position updates if continuous mode is enabled.
	private void Update()
	{
		if (IsContinuous)
		{
			currentTime += Game.IFps * Timescale;
			if (currentTime > maxTimeSec)
				// So we won't lose delta time
				currentTime -= maxTimeSec;
			RefreshSunPostion();
			timeChangedEvent.Run(currentTime); // displaying only integer part
		}
	}

	// Sun rotation is calculated from current time and applied to node.
	private void RefreshSunPostion()
	{
		// Convert time to rotation angle (-180 to 180 degrees)
		double time = currentTime % maxTimeSec;
		double k = MathLib.InverseLerp(0.0, maxTimeSec, time);
		float angle = (float)MathLib.Lerp(-180, 180.0, k);
		node.SetWorldRotation(sunInitTilt * new quat(angle, 0.0f, 0.0f));
	}
}
