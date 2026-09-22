// Demonstrates water surface height fetch and ray intersection queries.
// Visualizes water height sampling and intersection points with normals
// across a grid, with adjustable quality and precision parameters.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Unigine;

#region Math Variables
#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif
#endregion

// Visualizes water height fetch and intersection queries.
public partial class WaterFetchIntersection : Component
{
	// Global water object for queries
	[ShowInEditor]
	private ObjectWaterGlobal water = null;

	// Use fetch (true) or intersection (false) mode
	bool fetch = true;
	// Number of query points in grid
	int numIntersection = 100;
	// Visualization point size
	float intersectPointSize = 0.2f;
	// Angle for intersection rays
	float intersectionAngle = 0.0f;

	// Checkbox for normal display
	WidgetCheckBox normalCb;
	// Sample UI window
	SampleDescriptionWindow sampleDescriptionWindow = null;

	// Initializes visualizer and creates parameter UI.
	void Init()
	{
		if (!water)
			Log.Error("WaterFetchIntersectionSample.Init(): cannot get GlobalWater\n");

		// Enable debug visualization for points and vectors
		Visualizer.Enabled = true;
		initGui();
	}

	// Performs water queries across a grid and visualizes results each frame.
	void Update()
	{
		bool normalShow = normalCb.Checked;
		// Create square grid from total number of query points
		int count = (int)MathLib.Sqrt((float)numIntersection);
		// Intersection result containers
		ObjectIntersection oi = new ObjectIntersection();
		ObjectIntersectionNormal oin = new ObjectIntersectionNormal();

		// Iterate over grid positions
		for (int i = 0; i < count; i++)
		{
			for (int j = 0; j < count; j++)
			{
				Vec3 pos = new Vec3((float)i, (float)j, 0);
				if (fetch)
				{
					// Fetch mode: sample water height at XY position
					float v = water.FetchHeight(pos);
					pos.z += v;
					// Draw blue point at water surface height
					Visualizer.RenderPoint3D(pos, intersectPointSize, vec4.BLUE);

					if (normalShow)
					{
						// Fetch and draw surface normal as white vector
						vec3 n = water.FetchNormal(pos);
						Visualizer.RenderVector(pos, pos + new Vec3(n), vec4.WHITE);
					}
				}
				else
				{
					// Intersection mode: cast ray at specified angle
					Vec3 dir = new Vec3(MathLib.Sin(intersectionAngle * MathLib.DEG2RAD), 0,
					MathLib.Cos(intersectionAngle * MathLib.DEG2RAD));

					if (normalShow)
					{
						// Use intersection with normal to get surface orientation
						if (water.GetIntersection(pos + dir * 100, pos - dir * 100, oin, 0))
						{
							// Draw green point at intersection, white normal, blue ray
							Visualizer.RenderPoint3D(oin.Point, intersectPointSize,
								vec4.GREEN);
							Visualizer.RenderVector(oin.Point,
								oin.Point + new Vec3(oin.Normal), vec4.WHITE);
							Visualizer.RenderVector(oin.Point + dir * 2.0f, oin.Point,
								vec4.BLUE);
						}
					}
					else
					{
						// Basic intersection without normal data
						if (water.GetIntersection(pos + dir * 100, pos - dir * 100, oi, 0))
						{
							// Draw green point at intersection and blue ray direction
							Visualizer.RenderPoint3D(oi.Point, intersectPointSize, vec4.GREEN);
							Visualizer.RenderVector(oi.Point + dir * 2.0f, oi.Point,
								vec4.BLUE);
						}
					}
				}
			}
		}
	}

	// Cleans up UI and disables visualizer.
	void Shutdown()
	{
		sampleDescriptionWindow.shutdown();
		Visualizer.Enabled = false;
	}

	// Creates UI for switching between Fetch/Intersection modes and adjusting parameters.
	private void initGui()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow();

		var parameters = sampleDescriptionWindow.getParameterGroupBox();

		var hboxRequestType = new WidgetHBox();
		var requestTypeLabel = new WidgetLabel("Request type: ");
		var fetchButton = new WidgetButton("Fetch");
		var intersectionButton = new WidgetButton("Intersection");
		normalCb = new WidgetCheckBox("Show normals");

		fetchButton.Toggleable = true;
		intersectionButton.Toggleable = true;

		intersectionButton.Toggled = true;
		fetch = false;

		hboxRequestType.AddChild(requestTypeLabel);
		hboxRequestType.AddChild(fetchButton);

		hboxRequestType.AddChild(intersectionButton);
		hboxRequestType.AddChild(normalCb);

		parameters.AddChild(hboxRequestType, Gui.ALIGN_LEFT);

		fetchButton.EventChanged.Connect(() =>
		{
			fetch = fetchButton.Toggled;
			intersectionButton.Toggled = !fetch;
		});

		intersectionButton.EventChanged.Connect(() =>
		{
			fetch = !intersectionButton.Toggled;
			fetchButton.Toggled = fetch;
		});

		sampleDescriptionWindow.addIntParameter("Number of requests", "num request", numIntersection, 1, 10000,
			(int v) => { numIntersection = v; });

		sampleDescriptionWindow.addFloatParameter("Point size", "Point size", intersectPointSize, 0.1f, 1.0f,
			(float v) => { intersectPointSize = v; });

		sampleDescriptionWindow.addFloatParameter("Beaufort", "beaufort", 6.0f, 0.0f, 12.0f,
			(float v) => { water.Beaufort = v; });
		water.Beaufort = 6.0f;
		sampleDescriptionWindow.addParameterSpacer();

		sampleDescriptionWindow.addFloatParameter("Fetch Amplitude Threshold", "fetch amplitude threshold",
			water.FetchAmplitudeThreshold, 0.001f, 0.5f,
			(float v) => { water.FetchAmplitudeThreshold = v; });

		sampleDescriptionWindow.addIntParameter("Fetch Steepness Quality", "fetch steepness quality",
			(int)water.FetchSteepnessQuality, 0, 4, (int v) =>
			{
				water.FetchSteepnessQuality = (ObjectWaterGlobal.STEEPNESS_QUALITY)v;
			});

		sampleDescriptionWindow.addParameterSpacer();

		sampleDescriptionWindow.addFloatParameter("Intersection Amplitude Threshold",
			"intersection amplitude threshold", water.IntersectionAmplitudeThreshold, 0.001f,
			0.5f, (float v) => { water.IntersectionAmplitudeThreshold = v; });

		sampleDescriptionWindow.addIntParameter("Intersection Steepness Quality",
			"intersection steepness quality", (int)water.IntersectionSteepnessQuality, 0, 4,
			(int v) =>
			{
				water.IntersectionSteepnessQuality = (ObjectWaterGlobal.STEEPNESS_QUALITY)v;
			});

		sampleDescriptionWindow.addFloatParameter("Intersection Precision", "intersection precision",
			water.IntersectionPrecision, 0.001f, 2.0f,
			(float v) => { water.IntersectionPrecision = v; });

		sampleDescriptionWindow.addFloatParameter("Intersection Angle", "intersection angle", 5.0f,
			-30.0f, 30.0f, (float v) => { water.IntersectionPrecision = v; });
	}
}
