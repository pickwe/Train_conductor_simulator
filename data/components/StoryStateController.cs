using System;
using System.Collections.Generic;
using Unigine;

/// <summary>
/// Owns the current shift's score values and story flags.
/// Changes are exposed as events so achievements and persistence can subscribe later.
/// </summary>
public partial class StoryStateController : Component
{
	public enum Metric
	{
		Safety,
		Loyalty,
		Regulation,
		Observation
	}

	public enum StoryFlag
	{
		DisplayFaultDetected,
		InformationVerified,
		ChiefNotified,
		PassengerFound,
		PassengerInformed,
		UnderstandingConfirmed,
		BaggageAssistanceOffered
	}

	public int Safety { get; private set; } = 70;
	public int Loyalty { get; private set; } = 70;
	public int Regulation { get; private set; } = 70; // todo: remove
	public int Observation { get; private set; } // todo: remove
	public int RemainingRouteSeconds { get; private set; } = 15 * 60;

	public event Action<string, int, int> MetricChanged;
	public event Action<string, bool, bool> FlagChanged;
	public event Action<int, int> RouteTimeChanged;

	private readonly HashSet<StoryFlag> activeFlags = new HashSet<StoryFlag>();

	/// <summary>Applies a score delta and clamps the score to the 0..100 range.</summary>
	public void ChangeMetric(Metric metric, int delta)
	{
		int oldValue = GetMetricValue(metric);
		int newValue = Math.Max(0, Math.Min(100, oldValue + delta));
		if (newValue == oldValue)
			return;

		switch (metric)
		{
			case Metric.Safety: Safety = newValue; break;
			case Metric.Loyalty: Loyalty = newValue; break;
			case Metric.Regulation: Regulation = newValue; break;
			case Metric.Observation: Observation = newValue; break;
			default: throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unknown shift metric.");
		}

		MetricChanged?.Invoke(GetMetricId(metric), oldValue, newValue);
	}

	public bool IsFlagSet(StoryFlag flag)
	{
		return activeFlags.Contains(flag);
	}

	/// <summary>Sets or clears a story flag and emits an event only when it changes.</summary>
	public void SetFlag(StoryFlag flag, bool value)
	{
		bool oldValue = activeFlags.Contains(flag);
		if (oldValue == value)
			return;

		if (value)
			activeFlags.Add(flag);
		else
			activeFlags.Remove(flag);

		FlagChanged?.Invoke(GetFlagId(flag), oldValue, value);
	}

	public string[] GetActiveFlagIds()
	{
		string[] result = new string[activeFlags.Count];
		int index = 0;
		foreach (StoryFlag flag in activeFlags)
			result[index++] = GetFlagId(flag);
		return result;
	}

	public void SetRemainingRouteSeconds(int seconds)
	{
		int newValue = Math.Max(0, seconds);
		if (newValue == RemainingRouteSeconds)
			return;

		int oldValue = RemainingRouteSeconds;
		RemainingRouteSeconds = newValue;
		RouteTimeChanged?.Invoke(oldValue, newValue);
	}

	private int GetMetricValue(Metric metric)
	{
		switch (metric)
		{
			case Metric.Safety: return Safety;
			case Metric.Loyalty: return Loyalty;
			case Metric.Regulation: return Regulation;
			case Metric.Observation: return Observation;
			default: throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unknown shift metric.");
		}
	}

	private static string GetMetricId(Metric metric)
	{
		switch (metric)
		{
			case Metric.Safety: return "safety";
			case Metric.Loyalty: return "loyalty";
			case Metric.Regulation: return "regulation";
			case Metric.Observation: return "observation";
			default: throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unknown shift metric.");
		}
	}

	private static string GetFlagId(StoryFlag flag)
	{
		switch (flag)
		{
			case StoryFlag.DisplayFaultDetected: return "display_fault_detected";
			case StoryFlag.InformationVerified: return "information_verified";
			case StoryFlag.ChiefNotified: return "chief_notified";
			case StoryFlag.PassengerFound: return "passenger_found";
			case StoryFlag.PassengerInformed: return "passenger_informed";
			case StoryFlag.UnderstandingConfirmed: return "understanding_confirmed";
			case StoryFlag.BaggageAssistanceOffered: return "baggage_assistance_offered";
			default: throw new ArgumentOutOfRangeException(nameof(flag), flag, "Unknown story flag.");
		}
	}
}
