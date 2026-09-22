// Demonstrates automatic day/night switching based on sun position or time.
// Toggles material emission and node visibility when transitioning between
// day and night states. Supports both zenith angle and time-based control.

using System;
using System.Collections.Generic;
using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
using Mat4 = Unigine.dmat4;
#else
using Vec3 = Unigine.vec3;
using Mat4 = Unigine.mat4;
#endif
#endregion

// Switches materials and nodes between day and night states based on sun position.
public partial class DayNightSwitcher : Component
{
	// Control type determines how day/night is detected
	public enum CONTROL_TYPE
	{
		Zenith = 0,
		Time = 1,
	};

	// Method used to determine day or night state
	[ShowInEditor]
	[Parameter(Title = "Switch control type")]
	private CONTROL_TYPE switchControlType = CONTROL_TYPE.Zenith;

	// Reference to sun controller for time and position data
	[ShowInEditor]
	[Parameter(Title = "Sun controller")]
	private SunController sun = null;

	// Angle threshold for zenith-based day/night detection
	[ShowInEditor]
	[Parameter(Title = "Sun zenit threshold")]
	private double sunZenitThreshold = 85.0d;
	public double SunZenitThreshold { get { return sunZenitThreshold; } }

	// Time when morning begins (hours, minutes)
	[ShowInEditor]
	[Parameter(Title = "Morning time bound")]
	private ivec2 timeMorning = new ivec2(7, 30);

	// Time when evening begins (hours, minutes)
	[ShowInEditor]
	[Parameter(Title = "Evening time bound")]
	private ivec2 timeEvening = new ivec2(19, 30);
	public ivec2 TimeEvening { get { return timeEvening; } }

	// Material parameter name for emission control
	[ShowInEditor]
	[Parameter(Title = "Emission material parameter name")]
	private String emissionMaterialParameterName = "emission_scale";

	// Materials with emission enabled during daytime
	[ShowInEditor]
	[Parameter(Title = "Materials that enabled during day")]
	private List<Material> materialsDayEnabled = new List<Material>();

	// Materials with emission enabled during nighttime
	[ShowInEditor]
	[Parameter(Title = "Materials that enabled during night")]
	private List<Material> materialsNightEnabled = new List<Material>();

	// Nodes visible only during daytime
	[ShowInEditor]
	[Parameter(Title = "Nodes that enabled during day")]
	private List<Node> nodesDayEnabled = new List<Node>();

	// Nodes visible only during nighttime
	[ShowInEditor]
	[Parameter(Title = "Nodes that enabled during night")]
	private List<Node> nodesNightEnabled = new List<Node>();

	// Stores original emission values for restoration
	private Dictionary<UGUID, float> defaultEmissionScale = new Dictionary<UGUID, float>();
	// Tracks current day/night state (-1 = uninitialized)
	private int isDay = -1;

	// Event listener is connected and default emission values are stored.
	private void Init()
	{
		if (!sun)
		{
			Log.Error("DayNightSwitchSample::init can't find SunController component on the sun node!\n");
		}
		sun.EventOnTimeChanged.Connect(OnTimeChange);

		foreach (var mat in materialsDayEnabled)
		{
			int param = mat.FindParameter(emissionMaterialParameterName);
			if (param == -1)
			{
				Log.Error("DayNightSwitchSample::init materials_day_enabled got wrong material without emission!\n");
			}
			defaultEmissionScale.Add(mat.GUID, mat.GetParameterFloat(param));
		}

		foreach (var mat in materialsNightEnabled)
		{
			int param = mat.FindParameter(emissionMaterialParameterName);
			if (param == -1)
			{
				Log.Error("DayNightSwitchSample::init materials_night_enabled got wrong material without emission!\n");
			}
			defaultEmissionScale.Add(mat.GUID, mat.GetParameterFloat(param));
		}
		OnTimeChange();
	}

	// Emission values are cleared and event listener is disconnected.
	private void Shutdown()
	{
		defaultEmissionScale.Clear();
		sun.EventOnTimeChanged.Disconnect(OnTimeChange);
	}

	// Sets the control type and triggers immediate update.
	public void SetControlType(CONTROL_TYPE type)
	{
		switchControlType = type;
		OnTimeChange();
	}

	// Sets zenith threshold angle and triggers immediate update.
	public void SetZenithThreshold(float value)
	{
		value = MathLib.Clamp(value, 0.0f, 180.0f);
		sunZenitThreshold = value;
		OnTimeChange();
	}

	// Sets morning time boundary and triggers immediate update.
	public void SetMorningControlTime(ivec2 timeMorning)
	{
		this.timeMorning = timeMorning;
		OnTimeChange();
	}

	// Sets evening time boundary and triggers immediate update.
	public void SetEveningControlTime(ivec2 timeEvening)
	{
		this.timeEvening = timeEvening;
		OnTimeChange();
	}

	// Returns morning time in total minutes.
	public int GetControlMorningTime() { return timeMorning.x * 60 + timeMorning.y; }

	// Returns evening time in total minutes.
	public int GetControlEveningTime() { return timeEvening.x * 60 + timeEvening.y; }

	// Day/night state is evaluated and nodes are switched if state changed.
	private void OnTimeChange()
	{
		switch (switchControlType)
		{
			case CONTROL_TYPE.Zenith:
				{
					// Zenith angle
					bool day = true;// is day after sun is rotated
					if (sun.node)
					{
						double currentAngle = MathLib.Angle(Vec3.UP, sun.node.GetWorldDirection(MathLib.AXIS.Z));
						day = currentAngle < sunZenitThreshold;
					}

					// isDay initialized value is -1 so that we are always switching nodes for the first check depending on world initial setup
					if ((day ? 1 : 0) != isDay)
					{
						SwitchNodes(day);
						isDay = day ? 1 : 0;
					}
					break;
				}
			case CONTROL_TYPE.Time:
				{
					// Time
					int time = ((int)sun.Time / 60);
					bool day = time > (timeMorning.x * 60 + timeMorning.y)
							&& time < (timeEvening.x * 60 + timeEvening.y); // is day after sun is rotated

					// isDay initialized value is -1 so that we are always switching nodes for the first time depending on world initial setup
					if ((day ? 1 : 0) != isDay)
					{
						SwitchNodes(day);
						isDay = day ? 1 : 0;
					}
					break;
				}
			default:
				break;
		}
	}
	// Material emissions and node visibility are toggled based on day/night state.
	private void SwitchNodes(bool day)
	{
		// Toggle material emissions
		if (materialsDayEnabled != null)
		{
			foreach (var mat in materialsDayEnabled)
			{
				if (mat != null)
				{
					mat.SetParameterFloat(emissionMaterialParameterName, day ? defaultEmissionScale[mat.GUID] : 0);
				}
				else
				{
					Log.Warning("DayNightSwitcher::on_time_changed: day materail is null\n");
				}
			}
		}

		foreach (var mat in materialsNightEnabled)
		{
			if (mat != null)
			{
				mat.SetParameterFloat(emissionMaterialParameterName, !day ? defaultEmissionScale[mat.GUID] : 0);
			}
			else
			{
				Log.Warning("DayNightSwitcher::on_time_changed: day materail is null\n");
			}
		}

		// Toggle node visibility
		foreach (var node in nodesDayEnabled)
		{
			if (node)
			{
				node.Enabled = day;
			}
			else
			{
				Log.Warning("DayNightSwitcher::on_time_changed: got null node \n");
			}
		}
		foreach (var node in nodesNightEnabled)
		{

			if (node)
			{
				node.Enabled = !day;
			}
			else
			{
				Log.Warning("DayNightSwitcher::on_time_changed: got null node \n");
			}
		}
	}
}
