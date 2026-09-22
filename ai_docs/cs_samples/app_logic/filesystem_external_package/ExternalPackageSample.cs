// Demonstrates custom external package implementation for virtual file system.
// Creates a custom Package that serves mesh files from a single source file,
// allowing multiple virtual files to be loaded from one actual mesh file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Unigine;
using static Unigine.Image;

// Registers an external package and creates mesh objects from virtual files.
public partial class ExternalPackageSample : Component
{
	// Number of virtual files in the package
	private int num_files = 64;
	// Custom package instance
	private ExternalPackage package;

	// External package is registered and mesh objects are created from virtual files.
	void Init()
	{
		package = new ExternalPackage(num_files);
		FileSystem.AddExternPackage("package", package);

		// Create mesh objects at random positions using virtual file paths
		for (int i = 0; i < num_files; i += 1)
		{
			ObjectMeshStatic mesh_static = new ObjectMeshStatic(String.Format("{0}.mesh", i));

			vec3 position = random_vec3(new vec3(4.0f, 4.0f, 2.0f)) + vec3.UP * 2.0f;
			quat rotation = new quat(Game.GetRandomFloat(0.0f, 360.0f), Game.GetRandomFloat(0.0f, 360.0f), Game.GetRandomFloat(0.0f, 360.0f));

			mesh_static.WorldTransform = new mat4(rotation, position);
		}
	}

	// External package is removed and disposed.
	void Shutdown()
	{
		FileSystem.RemovePackage("package");
		package.Dispose();
	}

	// Generates random vector within centered bounds.
	private vec3 random_vec3(vec3 size)
	{
		return random_vec3(-size * 0.5f, size * 0.5f);
	}

	// Generates random vector within specified range.
	private vec3 random_vec3(vec3 from, vec3 to)
	{
		return new vec3 {
			Game.GetRandomFloat(from.x, to.x),
			Game.GetRandomFloat(from.y, to.y),
			Game.GetRandomFloat(from.z, to.z)
		};
	}
}

// Custom package that serves multiple virtual mesh files from a single source.
class ExternalPackage : Package, IDisposable
{
	// Source file handle
	private Unigine.File file;
	// Number of virtual files to expose
	private int num_files = 0;
	// Disposal tracking flag
	private bool disposed = false;

	// Creates package and saves a box mesh as the source file.
	public ExternalPackage(int num_files)
	{
		this.num_files = num_files;
		file = new Unigine.File();

		// Create and save a simple box mesh to use as source
		Mesh mesh = new Mesh();
		mesh.AddBoxSurface("box", vec3.ONE);

		string path = World.Path + @"/" + ".." + @"/" + ".temporary" + @"/" + "box.mesh";

		if (mesh.Save(path) > 0)
		{
			file.Open(path, "rb");
		}
	}

	// Returns the number of virtual files in the package.
	public override int GetNumFiles()
	{
		return num_files;
	}

	// Returns virtual file path for given index.
	public override string GetFilePath(int num)
	{
		return String.Format("{0}.mesh", num);
	}

	// Selects a file and returns its size if found.
	public override bool SelectFile(string name, out ulong size)
	{
		bool exists = FindFile(name) == 1 ? true : false;

		size = 0;
		if (exists)
			size = file.GetSize();

		return exists;
	}

	// Reads file data into the provided buffer.
	public override bool ReadFile(IntPtr data, ulong size)
	{
		if (!file.IsOpened)
			return false;

		file.SeekSet(0);
		ulong written = file.Read(data, size);

		return written == size;
	}

	// Checks if a virtual file exists in the package.
	public override int FindFile(string name)
	{
		for (int i = 0; i < num_files; i += 1)
		{
			if (String.Format("{0}.mesh", i) == name)
				return 1;
		}

		return 0;
	}

	// Returns the size of a virtual file.
	public override ulong GetFileSize(int num)
	{
		return file.GetSize();
	}

	// Destructor calls dispose for cleanup.
	~ExternalPackage()
	{
		Dispose(disposing: false);
	}

	// Disposes managed resources when called explicitly.
	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
			return;

		if (disposing)
		{
			file.Dispose();
		}

		disposed = true;
	}

	// Public dispose method for explicit cleanup.
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}

