// Downloads an image via HTTP and applies it to a material texture slot.
// Uses async HTTP request to fetch image data without blocking the main
// thread, then loads the image and assigns it to the specified surface.

using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unigine;

// Downloads image via HTTP and applies to material texture.
public partial class HttpImageToTexture : Component
{
	// HTTP host URL
	[ShowInEditor]
	[Parameter(Tooltip = "HTTP host, e.g. 'https://eu.httpbin.org'")]
	private string host = "https://eu.httpbin.org";

	// Request path after host
	[ShowInEditor]
	[Parameter(Tooltip = "Request path, e.g. '/image/'")]
	private string args = "/image/";

	// Material surface index to modify
	[ShowInEditor]
	[Parameter(Tooltip = "Surface index of the material to modify")]
	private int surface = 0;

	// Texture slot name in material
	[ShowInEditor]
	[Parameter(Tooltip = "Texture slot name, e.g. 'albedo'")]
	private string textureName = "albedo";

	// Downloaded image data
	private Image image;
	// Async HTTP download task
	private Task<HttpResponseMessage> downloadTask;

	// HTTP request is started on a background thread.
	void Init()
	{
		string requestUrl = host + args;
		// Sending http get request for specified URL in different thread
		downloadTask = Task.Run(async () =>
		{
			try
			{
				using var client = new HttpClient();
				return await client.GetAsync(requestUrl);
			}
			catch (Exception e)
			{
				Log.Error($"HttpImageRequestSample request failed: {e.Message}\n");
				return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
			}
		});
	}

	// Download completion is checked and image is applied to material.
	void Update()
	{
		// Process completed download
		if (downloadTask != null && downloadTask.IsCompleted)
		{
			HttpResponseMessage result = downloadTask.Result;

			if (result != null && result.IsSuccessStatusCode)
			{
				try
				{
					// Read image data
					byte[] data = result.Content.ReadAsByteArrayAsync().Result;
					image = new Image();

					if (!image.Load(data, data.Length))
					{
						Log.Error("HttpImageRequestSample failed to load image from response.\n");

						using (File file = new File("wrong_image", "wb"))
						{
							// Prevent GC-Garbage Collector from moving the managed array while native code writes into it.
							// This provides a stable pointer to pass into the native file.
							GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
							nint nativePtr = handle.AddrOfPinnedObject();
							file.Write(nativePtr, (ulong)data.Length);

							file.Close();
							handle.Free();
						}
						Log.Error("Saved broken image to 'data/wrong_image'\n");
					}

					Unigine.Object obj = node as Unigine.Object;
					if (obj == null)
					{
						Log.Error($"HttpImageRequestSample can only be assigned to Objects: {node.Name}({node.ID})\n");
						return;
					}

					Material mat = obj.GetMaterialInherit(surface);
					if (mat == null)
					{
						Log.Error($"HttpImageRequestSample cannot find material on surface {surface}\n");
						return;
					}

					int slot = mat.FindTexture(textureName);
					if (slot == -1)
					{
						Log.Error($"HttpImageRequestSample cannot find texture slot '{textureName}'\n");
						return;
					}

					// Apply the image to the material slot
					mat.SetTextureImage(slot, image);
					Log.Message($"Texture updated from {host + args}\n");
				}
				catch (Exception e)
				{
					Log.Error($"HttpImageRequestSample processing failed: {e.Message}\n");
				}
			}
			else
			{
				Log.Message($"{(int)result.StatusCode} {result.ReasonPhrase}\n");
			}
			downloadTask = null;
		}
	}
}
