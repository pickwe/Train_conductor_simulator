// Demonstrates HTTP requests with JSON response parsing. Provides a city
// search interface that queries a geocoding API, then fetches weather
// forecast data for selected cities from a weather API.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Unigine;

// City search and weather forecast via HTTP API requests.
public partial class SimpleHttpRequestSample : Component
{
	// Sample UI window
	private SampleDescriptionWindow descriptionWindow = new SampleDescriptionWindow();

	// City results list
	private WidgetListBox listBox;
	// City name search input
	private WidgetEditLine searchEditLine;

	// Pending city search requests
	private List<Task<HttpResponseMessage>> cityRequests = new List<Task<HttpResponseMessage>>();
	// Pending forecast request
	private Task<HttpResponseMessage> forecastRequest;

	// UI is initialized.
	private void Init()
	{
		InitGui();
	}

	// UI is cleaned up on shutdown.
	private void Shutdown()
	{
		descriptionWindow.shutdown();
	}

	// Completed HTTP requests are processed and results displayed.
	private void Update()
	{
		// Handle finished city search request
		if (cityRequests.Count > 0 && cityRequests.Last().IsCompleted)
		{
			listBox.Clear();

			var task = cityRequests.Last();
			try
			{
				HttpResponseMessage result = task.Result;
				if (result.IsSuccessStatusCode)
				{
					// Read response body as string
					string jsonText = result.Content.ReadAsStringAsync().Result;

					// Parse JSON response using UNIGINE Json class
					Json json = new Json();
					if (!json.Parse(jsonText))
					{
						Log.Error("Failed to parse JSON response.\n");
						return;
					}
					Log.Message("Response:\n{0}\n", json.GetFormattedSubTree());

					// Extract city results array from response
					Json results = json.GetChild("results");
					if (results != null && results.GetNumChildren() > 0)
					{
						// Iterate over found cities
						for (int i = 0; i < results.GetNumChildren(); i++)
						{
							Json child = results.GetChild(i);
							// Build display name: city, country, region
							string name = child.GetChild("name").GetString();

							if (child.IsChild("country"))
								name += ", " + child.GetChild("country").GetString();

							if (child.IsChild("admin1"))
								name += ", " + child.GetChild("admin1").GetString();

							// Add city to list and store coordinates for forecast request
							int item = listBox.AddItem(name);

							double lat = child.GetChild("latitude").GetNumber();
							double lon = child.GetChild("longitude").GetNumber();

							string data = string.Format(System.Globalization.CultureInfo.InvariantCulture, "latitude={0}&longitude={1}", lat, lon);
							listBox.SetItemData(item, data);
						}
					}
					else
					{
						// No cities found - highlight input in red
						searchEditLine.FontColor = vec4.RED;
					}
				}
				else
				{
					Log.Error($"{(int)result.StatusCode} {result.ReasonPhrase}\n");
				}
			}
			catch (Exception e)
			{
				Log.Error($"City request failed: {e.Message}\n");
			}

			cityRequests.Clear();
		}

		// Handle finished forecast request
		if (forecastRequest != null && forecastRequest.IsCompleted)
		{
			string resultText = "";

			try
			{
				HttpResponseMessage result = forecastRequest.Result;

				if (result.IsSuccessStatusCode)
				{
					// Read and parse JSON response
					string jsonText = result.Content.ReadAsStringAsync().Result;

					Json json = new Json();
					if (!json.Parse(jsonText))
					{
						Log.Error("Failed to parse JSON response.\n");
						return;
					}
					Log.Message("Forecast response:\n{0}\n", json.GetFormattedSubTree());

					// Get current weather data and units sections
					var current = json.GetChild("current");
					var units = json.GetChild("current_units");

					// Build weather info string with values and units
					resultText += "Temperature: ";
					resultText += MathLib.ToFloat(json.GetChild("current").GetChild("temperature_2m").GetNumber()).ToString();
					resultText += json.GetChild("current_units").GetChild("temperature_2m").GetString();
					resultText += "\n";

					resultText += "Humidity: ";
					resultText += MathLib.ToFloat(json.GetChild("current").GetChild("relative_humidity_2m").GetNumber()).ToString();
					resultText += json.GetChild("current_units").GetChild("relative_humidity_2m").GetString();
					resultText += "\n";

					resultText += "Precipitation: ";
					resultText += MathLib.ToFloat(json.GetChild("current").GetChild("precipitation").GetNumber()).ToString();
					resultText += json.GetChild("current_units").GetChild("precipitation").GetString();
					resultText += "\n";

					resultText += "Wind Speed: ";
					resultText += MathLib.ToFloat(json.GetChild("current").GetChild("wind_speed_10m").GetNumber()).ToString();
					resultText += json.GetChild("current_units").GetChild("wind_speed_10m").GetString();
					resultText += "\n";

					resultText += "Wind Direction: ";
					resultText += MathLib.ToFloat(json.GetChild("current").GetChild("wind_direction_10m").GetNumber()).ToString();
					resultText += json.GetChild("current_units").GetChild("wind_direction_10m").GetString();
					resultText += "\n";
				}
				else
				{
					Log.Message($"{(int)result.StatusCode} {result.ReasonPhrase}\n");
				}
			}
			catch (Exception e)
			{
				Log.Error($"Forecast request failed: {e.Message}\n");
			}

			// Display weather info in UI
			descriptionWindow.setStatus(resultText);
			forecastRequest = null;
		}
	}

	// Search UI with city list and forecast display is created.
	private void InitGui()
	{
		// Create and center sample window
		descriptionWindow = new SampleDescriptionWindow();
		descriptionWindow.createWindow();

		var window = descriptionWindow.MainWindow;
		window.Arrange();
		var gui = Gui.GetCurrent();

		var size = gui.Size / 2 - new ivec2(window.Width, window.Height) / 2;
		window.SetPosition(size.x, size.y);

		// Create search group box with input field
		var searchGroupBox = new WidgetGroupBox("Search", 20, 10);
		window.AddChild(searchGroupBox, Gui.ALIGN_EXPAND);

		searchEditLine = new WidgetEditLine(gui);
		searchEditLine.SetToolTip("Search city by name");
		searchGroupBox.AddChild(searchEditLine, Gui.ALIGN_EXPAND);

		// Handle search input - send geocoding request on key press
		searchEditLine.EventKeyPressed.Connect(() =>
		{
			// Reset input color to white
			searchEditLine.FontColor = vec4.WHITE;
			string name = searchEditLine.Text;

			if (string.IsNullOrWhiteSpace(name))
				return;

			// Build geocoding API query
			string query = $"/v1/search?name={name}";
			descriptionWindow.setStatus("Searching...");

			// Start async city search request
			cityRequests.Add(Task.Run(async () =>
			{
				using var client = new HttpClient();
				client.BaseAddress = new Uri("https://geocoding-api.open-meteo.com");
				return await client.GetAsync(query);
			}));
		});

		// Create city results list
		listBox = new WidgetListBox(gui);
		searchGroupBox.AddChild(listBox, Gui.ALIGN_LEFT);

		// Handle city selection - trigger forecast request
		listBox.EventChanged.Connect(HandleSelectedCity);

		descriptionWindow.setStatus("Type city name");
	}

	// Fetches weather forecast for the selected city.
	private void HandleSelectedCity()
	{
		descriptionWindow.setStatus("");

		int index = listBox.CurrentItem;
		if (index != -1)
		{
			descriptionWindow.setStatus("Requesting forecast...");

			// Create new forecast request
			// api.open-meteo.com/v1/forecast?latitude=12.34&longitude=12.34&current=temperature_2m,relative_humidity_2m,precipitation,wind_speed_10m,wind_direction_10m
			string query = $"/v1/forecast?{listBox.GetCurrentItemData()}&current=temperature_2m,relative_humidity_2m,precipitation,wind_speed_10m,wind_direction_10m";

			// Start async forecast request
			forecastRequest = Task.Run(async () =>
			{
				using var client = new HttpClient();
				client.BaseAddress = new Uri("https://api.open-meteo.com");
				return await client.GetAsync(query);
			});
		}
	}
}
