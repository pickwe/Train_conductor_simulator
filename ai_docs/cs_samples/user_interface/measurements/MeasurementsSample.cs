// Sample shell: enables the Visualizer (which draws every ruler and zone) and
// exposes two sliders that drive all DistanceMeasurement / ZoneRadiusVisualizer
// components in the world at once. The measuring and drawing live in those
// components, so each can be reused on its own node without this sample.

using Unigine;

public partial class MeasurementsSample : Component
{
	private SampleDescriptionWindow descriptionWindow = new SampleDescriptionWindow();

	// All measurement/zone components in the world, driven together by the two
	// sliders below.
	private DistanceMeasurement[] measurements;
	private ZoneRadiusVisualizer[] zones;

	void Init()
	{
		// The Visualizer is the only thing drawing the result, so enable it here and
		// disable it again in Shutdown().
		Visualizer.Enabled = true;

		// The measuring and drawing live in these components; the sample only drives
		// their shared parameters from one place.
		measurements = ComponentSystem.FindComponentsInWorld<DistanceMeasurement>(true);
		zones = ComponentSystem.FindComponentsInWorld<ZoneRadiusVisualizer>(true);

		descriptionWindow.createWindow();

		// One slider drives every DistanceMeasurement at once.
		float greenDistance = measurements.Length > 0 ? measurements[0].greenDistance : 25.0f;
		descriptionWindow.addFloatParameter("Green distance", "Distance at which a ruler turns fully green",
			greenDistance, 1.0f, 100.0f, (float value) => {
				for (int i = 0; i < measurements.Length; i++)
					if (measurements[i] != null)
						measurements[i].greenDistance = value;
			});

		// One slider drives every ZoneRadiusVisualizer at once.
		float zoneRadius = zones.Length > 0 ? zones[0].zoneRadius : 3.0f;
		descriptionWindow.addFloatParameter("Zone radius", "Radius drawn around each target node",
			zoneRadius, 0.0f, 20.0f, (float value) => {
				for (int i = 0; i < zones.Length; i++)
					if (zones[i] != null)
						zones[i].zoneRadius = value;
			});
	}

	void Shutdown()
	{
		Visualizer.Enabled = false;
		descriptionWindow.shutdown();
	}
}
