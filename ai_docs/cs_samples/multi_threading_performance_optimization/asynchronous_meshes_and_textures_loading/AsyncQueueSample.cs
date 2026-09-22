// Demonstrates asynchronous loading of meshes and images using AsyncQueue.
// Meshes are loaded and placed as dynamic objects, images are displayed
// as sprites at the bottom of the screen.

#region Math Variables
#if UNIGINE_DOUBLE
using Scalar = System.Double;
using Vec2 = Unigine.dvec2;
using Vec3 = Unigine.dvec3;
using Vec4 = Unigine.dvec4;
using Mat4 = Unigine.dmat4;
#else
using Scalar = System.Single;
using Vec2 = Unigine.vec2;
using Vec3 = Unigine.vec3;
using Vec4 = Unigine.vec4;
using Mat4 = Unigine.mat4;
using WorldBoundBox = Unigine.BoundBox;
using WorldBoundSphere = Unigine.BoundSphere;
using WorldBoundFrustum = Unigine.BoundFrustum;
#endif
#endregion

using System.Collections.Generic;
using Unigine;

// Loads meshes and images asynchronously using the AsyncQueue system.
public partial class AsyncQueueSample : Component
{
	// Mesh file paths to load asynchronously
	[ShowInEditor][ParameterFile]
	private string[] meshes = null;
	// Texture file paths to load asynchronously
	[ShowInEditor][ParameterFile]
	private string[] textures = null;

	// Tracks pending mesh load requests
	struct AsyncLoadRequest
	{
		public string name;
		public int id;
	}

	// Queue of pending mesh load requests
	private List<AsyncLoadRequest> meshLoadRequest = new List<AsyncLoadRequest>();

	// Counter for horizontal placement of loaded meshes
	private int objectsPlaced = 0;

	// Sprites displaying loaded images
	private List<WidgetSprite> sprites = new List<WidgetSprite>();

	// Connection to image loaded event for cleanup
	private EventConnection imageLoadedConnection;

	// Async load requests are queued for all meshes and images.
	void Init()
	{
		// Queue mesh load requests
		for(int i =0; i < meshes.Length; i++)
		{
			string name = meshes[i];
			AsyncLoadRequest request = new AsyncLoadRequest();
			request.name = name;
			request.id = AsyncQueue.LoadMesh(name);
			meshLoadRequest.Add(request);
		}

		// Queue image load requests
		for(int i =0; i < textures.Length; i++)
		{
			AsyncQueue.LoadImage(textures[i]);
		}

		// Subscribe to image loaded callback
		imageLoadedConnection = AsyncQueue.EventImageLoaded.Connect(ImageLoadedCallback);

		Console.Onscreen = true;
	}
	
	// Completed mesh loads are processed and placed in the scene.
	void Update()
	{
		for(int i =0; i < meshLoadRequest.Count; i++)
		{
			AsyncLoadRequest request = meshLoadRequest[i];
			// Skip if mesh is not ready yet
			if (AsyncQueue.CheckMesh(request.id) == 0)
				continue;

			// Take loaded mesh from queue
			Mesh mesh = AsyncQueue.TakeMesh(request.id);
			if(mesh != null)
			{
				// Create dynamic mesh object and position it
				ObjectMeshDynamic objectMeshDynamic = new ObjectMeshDynamic(mesh);
				Scalar initialPos = -5;
				Scalar step = 5;

				objectMeshDynamic.Position = new Vec3(initialPos + (float)objectsPlaced * step, 0.0f, 0.0f);
				objectsPlaced++;

				// Clean up request
				AsyncQueue.RemoveMesh(request.id);
				Log.MessageLine($"Loaded mesh {request.name}");

				meshLoadRequest.RemoveAt(i--);
			}
		}
		
	}

	// Sprites are cleaned up and event is disconnected on shutdown.
	void Shutdown()
	{
		foreach(WidgetSprite sprite in sprites)
		{
			sprite.DeleteLater();
		}

		Console.Onscreen = false;

		imageLoadedConnection.Disconnect();
	}

	// Callback creates sprite from loaded image and positions it on screen.
	private void ImageLoadedCallback(string name, int id)
	{
		// Take image from queue
		Image loadedImage = AsyncQueue.TakeImage(id);
		if (loadedImage == null)
			return;

		// Clean up queue entry
		AsyncQueue.RemoveImage(id);
		Log.MessageLine($"Image {name} loaded");

		// Create sprite widget and set loaded image
		var sprite = new WidgetSprite();
		sprites.Add(sprite);
		sprite.SetImage(loadedImage);
		sprite.Width = 100;
		sprite.Height = 100;
		WindowManager.MainWindow.AddChild(sprite, Gui.ALIGN_OVERLAP | Gui.ALIGN_BACKGROUND);

		// Position sprite horizontally based on load order
		ivec2 initialSpritePosition = new ivec2(0, WindowManager.MainWindow.Size.y - 200);

		ivec2 newPos = new ivec2(initialSpritePosition.x + sprites.Count * 100, initialSpritePosition.y);
		sprite.SetPosition(newPos.x, newPos.y);
	}
}
