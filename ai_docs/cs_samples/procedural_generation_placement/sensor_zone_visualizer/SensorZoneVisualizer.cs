// Procedural "sensor zone" mesh generator: near/far are radial distances
// (spherical caps), the bounds are set by horizontal/vertical angles. Handles the
// full geometry, including fields of view greater than 180 degrees. Change the
// parameters and call Refresh() to rebuild the mesh.

using System.Collections.Generic;
using Unigine;

public partial class SensorZoneVisualizer : Component
{
	[ShowInEditor, Parameter(Title = "Material")]
	public Material Mat = null;
	[ShowInEditor, Parameter(Title = "Change color")]
	public bool ChangeColor = false;
	[ShowInEditor, ParameterColor]
	public vec4 MatColor = new vec4(0.0f, 1.0f, 1.0f, 1.0f);
	// Name of the material parameter driven by the color (see SetColor).
	[ShowInEditor, Parameter(Title = "Color parameter")]
	public string ColorParameter = "Emission Color";
	[ShowInEditor, Parameter(Title = "Vertical angles")]
	public vec2 VerticalAngles = new vec2(-30.0f, 30.0f);
	[ShowInEditor, Parameter(Title = "Horizontal angles")]
	public vec2 HorizontalAngles = new vec2(-30.0f, 30.0f);
	[ShowInEditor, Parameter(Title = "Stacks")]
	public int NumStacks = 64;
	[ShowInEditor, Parameter(Title = "Slices")]
	public int NumSlices = 64;
	[ShowInEditor, Parameter(Title = "Side segments")]
	public int NumSideSegments = 9;
	[ShowInEditor, Parameter(Title = "Near")]
	public float Near = 0.5f;
	[ShowInEditor, Parameter(Title = "Far")]
	public float Far = 2.0f;

	// Angle constants (the engine's per-string bidi helpers keep angles in radians).
	private const float PI = (float)System.Math.PI;
	private const float PI2 = 2.0f * PI;
	private const float PI05 = 0.5f * PI;
	private const float EPS = 1.0e-6f;

	private Mesh mesh;
	private ObjectMeshStatic obj;
	private Material material = null;
	private vec4 currentColor;
	private int materialParamIndex = -1;

	private int stacks = 64;
	private int slices = 64;
	private int deviders = 5;

	private float nearDist = 0.5f;
	private float farDist = 2.0f;

	private bool changed = false;

	private float minVerticalAngle;
	private float maxVerticalAngle;
	private float minHorizontalAngle;
	private float maxHorizontalAngle;

	// One surface's geometry buffers, appended into the mesh with an offset.
	private class SectorSurface
	{
		public List<vec3> vertices = new List<vec3>();
		public List<vec3> normals = new List<vec3>();
		public List<quat> tangents = new List<quat>();
		public int offset = 0;
	}

	public void Refresh()
	{
		InitParams();
	}

	public vec4 GetColor()
	{
		return currentColor;
	}

	public void SetColor(vec4 color)
	{
		if (materialParamIndex < 0)
			return;
		currentColor = color;
		material.SetParameterFloat4(materialParamIndex, color);
	}

	// Init order 1 so the sample (order 2) can read the resolved color afterwards.
	[MethodInit(Order = 1)]
	void Init()
	{
		InitParams();

		mesh = new Mesh();
		obj = new ObjectMeshStatic();
		obj.SetMeshProceduralMode(ObjectMeshStatic.PROCEDURAL_MODE.FILE);
		node.AddChild(obj);

		if (Mat != null)
		{
			material = Mat.Inherit();
			materialParamIndex = material.FindParameter(ColorParameter);
			if (materialParamIndex > -1)
			{
				currentColor = material.GetParameterFloat4(materialParamIndex);
				// keep the color logic in one place (SetColor); only override the
				// material's own color when the sample asked for it
				if (ChangeColor)
					SetColor(MatColor);
			}
		}

		// Build the initial mesh and assign the material once here: the single
		// surface is reused on every rebuild, so Update() only refreshes geometry.
		UpdateMesh(mesh);
		obj.ApplyMoveMeshProceduralForce(mesh, 0);
		if (material != null)
			obj.SetMaterial(material, "*");
		changed = false;
	}

	void Update()
	{
		if (changed)
		{
			UpdateMesh(mesh);
			obj.ApplyMoveMeshProceduralForce(mesh, 0);
			changed = false;
		}
	}

	void Shutdown()
	{
		if (obj)
			obj.DeleteLater();
		if (mesh != null)
			mesh.Clear();
	}

	protected override void OnEnable()
	{
		if (obj)
			obj.Enabled = true;
	}

	protected override void OnDisable()
	{
		if (obj)
			obj.Enabled = false;
	}

	private void InitParams()
	{
		vec2 vAngles = MathLib.Clamp(VerticalAngles, new vec2(-180.0f, -180.0f), new vec2(180.0f, 180.0f));
		vec2 hAngles = MathLib.Clamp(HorizontalAngles, new vec2(-180.0f, -180.0f), new vec2(180.0f, 180.0f));

		minHorizontalAngle = hAngles.x;
		maxHorizontalAngle = hAngles.y;

		minVerticalAngle = vAngles.x;
		maxVerticalAngle = vAngles.y;

		slices = NumSlices < 2 ? 2 : NumSlices;
		stacks = NumStacks < 2 ? 2 : NumStacks;
		deviders = NumSideSegments < 1 ? 1 : NumSideSegments;

		nearDist = Near > 0.1f ? Near : 0.1f;
		farDist = Far > 0.2f ? Far : 0.2f;
		changed = true;
	}

	private void UpdateMesh(Mesh mesh)
	{
		if (mesh.NumSurfaces != 1)
		{
			mesh.Clear();
			mesh.AddSurface("");
		}
		else
		{
			mesh.ClearSurface();
		}

		List<vec3> vertices = new List<vec3>();
		List<vec3> normals = new List<vec3>();
		List<quat> tangents = new List<quat>();
		List<int> cindices = new List<int>();
		List<int> tindices = new List<int>();

		void AppendIndex(params int[] indices)
		{
			cindices.AddRange(indices);
			tindices.AddRange(indices);
		}

		void AppendSurface(SectorSurface s)
		{
			s.offset = vertices.Count;
			vertices.AddRange(s.vertices);
			normals.AddRange(s.normals);
			tangents.AddRange(s.tangents);
		}

		void AddSideIndexes(int col, int numStripSegments, int offset, bool leftSide)
		{
			int baseIndex = col * (numStripSegments + 1) + offset;
			int baseUp = baseIndex + (numStripSegments + 1);
			for (int k = 0; k < numStripSegments; k++)
			{
				if (leftSide)
					AppendIndex(baseIndex, baseUp + 1, baseIndex + 1, baseIndex, baseUp, baseUp + 1);
				else
					AppendIndex(baseIndex, baseIndex + 1, baseUp + 1, baseIndex, baseUp + 1, baseUp);
				baseIndex++;
				baseUp++;
			}
		}

		void AddSphereIndexes(int numCols, int col, int row, int offset)
		{
			int baseIndex = (row * numCols + col) * 2 + offset;
			int baseRight = baseIndex + 2;
			int baseUp = baseIndex + numCols * 2;
			int baseRightUp = baseUp + 2;

			AppendIndex(baseIndex, baseRight, baseRightUp, baseIndex, baseRightUp, baseUp);
			AppendIndex(baseIndex + 1, baseUp + 1, baseRightUp + 1, baseIndex + 1, baseRightUp + 1, baseRight + 1);
		}

		SectorSurface sphere = new SectorSurface();
		SectorSurface left = new SectorSurface();
		SectorSurface right = new SectorSurface();
		SectorSurface top = new SectorSurface();
		SectorSurface bottom = new SectorSurface();
		SectorSurface topSector = new SectorSurface();
		SectorSurface bottomSector = new SectorSurface();

		float maxHorizontal = maxHorizontalAngle * MathLib.DEG2RAD;
		float minHorizontal = minHorizontalAngle * MathLib.DEG2RAD;
		float horizontalFov = maxHorizontal - minHorizontal;

		float minVertical = minVerticalAngle * MathLib.DEG2RAD;
		float maxVertical = maxVerticalAngle * MathLib.DEG2RAD;

		int horizontalSegments = (int)(slices * horizontalFov / PI2);

		float extraMaxV = 0.0f;
		float extraMinV = 0.0f;
		bool inverseTop = false;
		bool inverseBottom = false;
		int extraHSeg = -1;

		if (minVertical < -PI05)
		{
			inverseBottom = true;
			extraMinV = (minVertical + PI05);
			minVertical = -PI05;
			extraHSeg = (int)MathLib.Ceil(horizontalSegments * (PI2 - horizontalFov) / horizontalFov);
		}

		if (maxVertical > PI05)
		{
			inverseTop = true;
			extraMaxV = (maxVertical - PI05);
			maxVertical = PI05;
			extraHSeg = (int)MathLib.Ceil(horizontalSegments * (PI2 - horizontalFov) / horizontalFov);
		}

		float vFov = maxVertical - minVertical;

		if (vFov <= EPS && horizontalFov <= EPS)
			return;

		int vSegs = (int)(stacks * vFov / PI2);

		int cols = horizontalSegments + 1;
		int rows = vSegs + 1;

		int colsExtra = extraHSeg + 1;

		int rowsBottom = 0;
		int rowsTop = 0;
		int topRow = rows;
		int bottomRow = 0;

		if (inverseTop || inverseBottom)
		{
			float bottomBorder = minVertical - extraMinV;
			float topBorder = maxVertical - extraMaxV;

			float unit = vFov / vSegs;

			for (int j = 0; j < rows; j++)
			{
				float v = MathLib.Lerp(minVertical, maxVertical, (float)j / vSegs);
				if (v <= bottomBorder)
					rowsBottom++;

				if (v >= topBorder)
					rowsTop++;
			}

			topRow = rows - rowsTop;
			bottomRow = rowsBottom - 1;
			if (bottomRow > 0 && (bottomBorder - bottomRow * unit < unit) && bottomRow < topRow)
				bottomRow++;
		}

		for (int j = 0; j < rows; j++)
		{
			float v = MathLib.Lerp(minVertical, maxVertical, (float)j / vSegs);
			float cosV = MathLib.Cos(v);
			float cosSign = MathLib.Sign(v);
			bool isPole = cosV < EPS;

			for (int i = 0; i < cols; i++)
			{
				float h = MathLib.Lerp(minHorizontal, maxHorizontal, (float)i / horizontalSegments);

				vec3 dir = new vec3(MathLib.Sin(h) * cosV, MathLib.Cos(h) * cosV, MathLib.Sin(v));
				dir = MathLib.Normalize(dir);

				vec3 xyDir = new vec3(cosSign * MathLib.Sin(h), cosSign * MathLib.Cos(h), 0.0f);
				xyDir = MathLib.Normalize(xyDir);

				// sphere data
				AddSphereData(sphere, dir, isPole);

				// left side
				if (i == 0 && j >= bottomRow && j <= topRow)
					AddHSidesVertexData(left, dir, !isPole ? vec3.DOWN : xyDir);

				// right side
				if (i == cols - 1 && j >= bottomRow && j <= topRow)
					AddHSidesVertexData(right, dir, !isPole ? vec3.UP : -xyDir);

				// bottom side
				if (!inverseBottom && j == 0)
					AddVSidesVertexData(bottom, dir, vec3.DOWN);

				// top side
				if (!inverseTop && j == rows - 1)
					AddVSidesVertexData(top, dir, vec3.UP);
			}

			if ((!inverseBottom && !inverseTop) || (j > bottomRow && j < topRow))
				continue;

			// create additional sides for vFov > 180
			for (int i = 0; i < colsExtra; i++)
			{
				float h = MathLib.Lerp(maxHorizontal, PI2 + minHorizontal, (float)i / extraHSeg);

				vec3 dir = new vec3(MathLib.Sin(h) * cosV, MathLib.Cos(h) * cosV, MathLib.Sin(v));
				dir = MathLib.Normalize(dir);

				vec3 xyDir = new vec3(cosSign * MathLib.Sin(h), cosSign * MathLib.Cos(h), 0.0f);
				xyDir = MathLib.Normalize(xyDir);

				if (inverseBottom)
				{
					// bottom additional sphere
					if (j <= bottomRow)
						AddSphereData(bottomSector, dir, isPole);

					// bottom side
					if (j == bottomRow)
						AddVSidesVertexData(bottom, dir, vec3.UP);
				}

				if (inverseTop)
				{
					// top additional sphere
					if (j >= topRow)
						AddSphereData(topSector, dir, isPole);

					// top side
					if (j == topRow)
						AddVSidesVertexData(top, dir, vec3.DOWN);
				}
			}
		}

		AppendSurface(sphere);
		bool isOpenRing = horizontalFov < PI2;
		if (isOpenRing)
		{
			AppendSurface(left);
			AppendSurface(right);
		}
		AppendSurface(bottom);
		AppendSurface(top);

		AppendSurface(bottomSector);
		AppendSurface(topSector);

		// indexes for main sector surfaces
		for (int j = 0; j < vSegs; j++)
		{
			for (int i = 0; i < horizontalSegments; i++)
			{
				// sphere
				AddSphereIndexes(cols, i, j, sphere.offset);

				// top + bottom sides
				if (j == 0)
				{
					if (!inverseTop)
						AddSideIndexes(i, deviders, top.offset, true);
					if (!inverseBottom)
						AddSideIndexes(i, deviders, bottom.offset, false);
				}
			}

			// left + right sides
			if (isOpenRing && j >= bottomRow && j < topRow)
			{
				AddSideIndexes(j - bottomRow, 1, left.offset, true);
				AddSideIndexes(j - bottomRow, 1, right.offset, false);
			}
		}

		// indexes for additional surfaces for vFov > 180
		for (int i = 0; i < extraHSeg; i++)
		{
			if (inverseBottom)
			{
				// bottom side
				AddSideIndexes(i, deviders, bottom.offset, true);
				// bottom additional sphere
				for (int j = 0; j < bottomRow; j++)
					AddSphereIndexes(colsExtra, i, j, bottomSector.offset);
			}
			if (inverseTop)
			{
				// top side
				AddSideIndexes(i, deviders, top.offset, false);
				// top additional sphere
				for (int j = 0; j < rowsTop - 1; j++)
					AddSphereIndexes(colsExtra, i, j, topSector.offset);
			}
		}

		if (vertices.Count > 0)
		{
			mesh.AddVertex(vertices.ToArray());
			mesh.AddNormals(normals.ToArray());
			mesh.AddTangents(tangents.ToArray());
		}
		if (cindices.Count > 0)
		{
			mesh.AddCIndices(cindices.ToArray());
			mesh.AddTIndices(tindices.ToArray());
		}

		mesh.CreateBounds();
	}

	private void AddSphereData(SectorSurface s, vec3 dir, bool isPole)
	{
		s.vertices.Add(dir * nearDist);
		s.vertices.Add(dir * farDist);

		CreateSphereVertexData(s, -dir, vec3.DOWN);
		CreateSphereVertexData(s, dir, !isPole ? vec3.UP : vec3.RIGHT);
	}

	private void CreateSphereVertexData(SectorSurface s, vec3 normal, vec3 up)
	{
		vec3 tangent = MathLib.Normalize(MathLib.Cross(normal, up));
		quat q = new quat();
		q.Set(tangent, MathLib.Cross(normal, tangent), normal);
		s.normals.Add(normal);
		s.tangents.Add(q);
	}

	private void AddHSidesVertexData(SectorSurface s, vec3 dir, vec3 up)
	{
		vec3 normal = MathLib.Normalize(MathLib.Cross(dir, up));
		vec3 tangent = MathLib.Normalize(MathLib.Cross(dir, normal));
		quat q = new quat();
		q.Set(tangent, dir, normal);

		s.vertices.Add(dir * nearDist);
		s.vertices.Add(dir * farDist);
		s.normals.Add(normal);
		s.normals.Add(normal);
		s.tangents.Add(q);
		s.tangents.Add(q);
	}

	private void AddVSidesVertexData(SectorSurface s, vec3 dir, vec3 up)
	{
		vec3 tangent = MathLib.Normalize(MathLib.Cross(dir, up));
		vec3 normal = MathLib.Normalize(MathLib.Cross(tangent, dir));
		quat q = new quat();
		q.Set(tangent, dir, normal);

		for (int k = 0; k < deviders + 1; k++)
		{
			s.vertices.Add(dir * (nearDist + (farDist - nearDist) * k / deviders));
			s.normals.Add(normal);
			s.tangents.Add(q);
		}
	}
}
