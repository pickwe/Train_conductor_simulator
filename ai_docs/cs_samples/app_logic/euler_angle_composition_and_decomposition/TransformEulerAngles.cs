// Demonstrates Euler angle rotation composition and decomposition. Shows how
// different rotation sequences (XYZ, XZY, YXZ, etc.) produce different results.
// Visualizes rotation axes and gimbals in 3D space with adjustable UI controls.

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

using System;
using Unigine;

// Visualizes Euler angle composition and decomposition with different rotation sequences.
public partial class TransformEulerAngles : Component
{
	// Supported rotation sequence orders
	public enum COMPOSITION_TYPE
	{
		XYZ = 0,
		XZY,
		YXZ,
		YZX,
		ZXY,
		ZYX,
		COUNT
	}

	// Current input Euler angles
	private vec3 EulerAngles = vec3.ZERO;
	// Angles extracted from current rotation
	private vec3 DecompositionAngles = vec3.ZERO;

	// Selected composition sequence
	private COMPOSITION_TYPE compositionType = COMPOSITION_TYPE.XYZ;
	// Selected decomposition sequence
	private COMPOSITION_TYPE decompositionType = COMPOSITION_TYPE.XYZ;

	// Stores original visualizer state
	private bool visualizerEnabled = false;
	// UI window for parameter controls
	private SampleDescriptionWindow sampleDescriptionWindow;
	// Status text displayed in UI window
	string status = null;

	// UI is created and visualizer is enabled for rendering rotation gimbals.
	private void Init()
	{
		initGui();
		UpdateDecompositionAngles();

		visualizerEnabled = Visualizer.Enabled;
		Visualizer.Enabled = true;
	}

	// Local and global axes are rendered along with rotation gimbal circles.
	private void Update()
	{
		Vec3 planePos = node.WorldPosition;
		vec3 planeX = node.GetWorldDirection(MathLib.AXIS.X);
		vec3 planeY = node.GetWorldDirection(MathLib.AXIS.Y);
		vec3 planeZ = node.GetWorldDirection(MathLib.AXIS.Z);

		// Render local axes of the object
		Visualizer.RenderVector(planePos, planePos + planeX * 1.5f, vec4.RED);
		Visualizer.RenderVector(planePos, planePos + planeY * 1.5f, vec4.GREEN);
		Visualizer.RenderVector(planePos, planePos + planeZ * 1.5f, vec4.BLUE);

		Visualizer.RenderMessage3D(planePos + planeX * 1.5f, vec3.ZERO, "X", vec4.BLACK, 1);
		Visualizer.RenderMessage3D(planePos + planeY * 1.5f, vec3.ZERO, "Y", vec4.BLACK, 1);
		Visualizer.RenderMessage3D(planePos + planeZ * 1.5f, vec3.ZERO, "Z", vec4.BLACK, 1);

		// Render global reference axes
		Vec3 start = new Vec3(-2.0f, -2.0f, 0.2f);

		Visualizer.RenderVector(start, start + vec3.RIGHT * 2.0f, vec4.RED);
		Visualizer.RenderVector(start, start + vec3.FORWARD * 2.0f, vec4.GREEN);
		Visualizer.RenderVector(start, start + vec3.UP * 2.0f, vec4.BLUE);

		// Render rotation gimbal circles
		mat4 x = mat4.IDENTITY;
		mat4 y = mat4.IDENTITY;
		mat4 z = mat4.IDENTITY;
		vec3 radii = vec3.ZERO;

		GetCircleMatrix(out x, out y, out z, out radii);

		Visualizer.RenderCircle(radii.x, new Mat4(x), vec4.RED);
		Visualizer.RenderCircle(radii.y, new Mat4(y), vec4.GREEN);
		Visualizer.RenderCircle(radii.z, new Mat4(z), vec4.BLUE);

		// Render direction arrows on gimbal circles
		Vec3 arrowStart = planePos + new vec3(x.AxisY) * radii.x;
		Vec3 arrowFinish = arrowStart + new vec3(x.AxisY) * 0.1f;
		Visualizer.RenderVector(arrowStart, arrowFinish, vec4.RED, 1.0f);

		arrowStart = planePos + new vec3(y.AxisY) * radii.y;
		arrowFinish = arrowStart + new vec3(y.AxisY) * 0.1f;
		Visualizer.RenderVector(arrowStart, arrowFinish, vec4.GREEN, 1.0f);

		arrowStart = planePos + new vec3(z.AxisY) * radii.z;
		arrowFinish = arrowStart + new vec3(z.AxisY) * 0.1f;
		Visualizer.RenderVector(arrowStart, arrowFinish, vec4.BLUE, 1.0f);
	}

	// Visualizer state is restored and UI window is released.
	private void Shutdown()
	{
		Visualizer.Enabled = visualizerEnabled;
		sampleDescriptionWindow.shutdown();
	}

	// UI sliders and combo boxes for angle control are created.
	private void initGui()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow();

		var pitch_slider = sampleDescriptionWindow.addIntParameter("Pitch (X)", "Pitch (X)", 0, -180, 180, (int value) =>
		{
			EulerAngles.x = value;
			UpdateRotation();
			UpdateDecompositionAngles();
		});

		var roll_slider = sampleDescriptionWindow.addIntParameter("Roll (Y)", "Roll (Y)", 0, -180, 180, (int value) =>
		{
			EulerAngles.y = value;
			UpdateRotation();
			UpdateDecompositionAngles();
		});

		var yaw_slider = sampleDescriptionWindow.addIntParameter("Yaw (Z)", "Yaw (Z)", 0, -180, 180, (int value) =>
		{
			EulerAngles.z = value;
			UpdateRotation();
			UpdateDecompositionAngles();
		});

		var parameters = sampleDescriptionWindow.getParameterGroupBox();

		WidgetComboBox composition_combo_box;
		WidgetComboBox decomposition_combo_box;

		{
			var hbox = new WidgetHBox();
			hbox.AddChild(new WidgetLabel("Composition sequence: "), Gui.ALIGN_LEFT);

			var combo_box = new WidgetComboBox();

			combo_box.AddItem("XYZ");
			combo_box.AddItem("XZY");
			combo_box.AddItem("YXZ");
			combo_box.AddItem("YZX");
			combo_box.AddItem("ZXY");
			combo_box.AddItem("ZYX");

			combo_box.EventChanged.Connect(() =>
			{
				compositionType = (COMPOSITION_TYPE)combo_box.CurrentItem;
				UpdateRotation();
			});

			composition_combo_box = combo_box;

			hbox.AddChild(combo_box);
			parameters.AddChild(hbox, Gui.ALIGN_LEFT);
		}

		{
			var hbox = new WidgetHBox();
			hbox.AddChild(new WidgetLabel("Decomposition sequence: "), Gui.ALIGN_LEFT);

			var combo_box = new WidgetComboBox();

			combo_box.AddItem("XYZ");
			combo_box.AddItem("XZY");
			combo_box.AddItem("YXZ");
			combo_box.AddItem("YZX");
			combo_box.AddItem("ZXY");
			combo_box.AddItem("ZYX");

			combo_box.EventChanged.Connect(() =>
			{
				decompositionType = (COMPOSITION_TYPE)combo_box.CurrentItem;
				UpdateDecompositionAngles();
			});

			decomposition_combo_box = combo_box;

			hbox.AddChild(combo_box);
			parameters.AddChild(hbox, Gui.ALIGN_LEFT);
		}

		var reset_button = new WidgetButton("Reset");
		reset_button.EventClicked.Connect(() =>
		{
			EulerAngles = vec3.ZERO;
			UpdateRotation();
			UpdateDecompositionAngles();

			yaw_slider.Value = 0;
			pitch_slider.Value = 0;
			roll_slider.Value = 0;
			composition_combo_box.CurrentItem = 0;
			decomposition_combo_box.CurrentItem = 0;
		});

		parameters.AddChild(reset_button, Gui.ALIGN_LEFT);
	}

	// Rotation matrix is computed from Euler angles using selected composition sequence.
	private void UpdateRotation()
	{
		// Get new rotation based on current composition type
		mat4 rot = mat4.IDENTITY;
		switch (compositionType)
		{
			case COMPOSITION_TYPE.XYZ: rot = MathLib.ComposeRotationXYZ(EulerAngles); break;
			case COMPOSITION_TYPE.XZY: rot = MathLib.ComposeRotationXZY(EulerAngles); break;
			case COMPOSITION_TYPE.YXZ: rot = MathLib.ComposeRotationYXZ(EulerAngles); break;
			case COMPOSITION_TYPE.YZX: rot = MathLib.ComposeRotationYZX(EulerAngles); break;
			case COMPOSITION_TYPE.ZXY: rot = MathLib.ComposeRotationZXY(EulerAngles); break;
			case COMPOSITION_TYPE.ZYX: rot = MathLib.ComposeRotationZYX(EulerAngles); break;
		}

		node.SetWorldRotation(new quat(rot));
	}

	// Euler angles are extracted from current rotation using selected decomposition sequence.
	private void UpdateDecompositionAngles()
	{
		// Update decomposition angles based on current decomposition type
		mat3 rot = node.GetWorldRotation().Mat3;
		switch (decompositionType)
		{
			case COMPOSITION_TYPE.XYZ: DecompositionAngles = MathLib.DecomposeRotationXYZ(rot); break;
			case COMPOSITION_TYPE.XZY: DecompositionAngles = MathLib.DecomposeRotationXZY(rot); break;
			case COMPOSITION_TYPE.YXZ: DecompositionAngles = MathLib.DecomposeRotationYXZ(rot); break;
			case COMPOSITION_TYPE.YZX: DecompositionAngles = MathLib.DecomposeRotationYZX(rot); break;
			case COMPOSITION_TYPE.ZXY: DecompositionAngles = MathLib.DecomposeRotationZXY(rot); break;
			case COMPOSITION_TYPE.ZYX: DecompositionAngles = MathLib.DecomposeRotationZYX(rot); break;
		}

		status = String.Format($"Decomposition angles:\nPitch (X):\t{DecompositionAngles.x:0.00}\nRoll (Y):\t{DecompositionAngles.y:0.00}\nYaw (Z):\t{DecompositionAngles.z:0.00}\n");
		sampleDescriptionWindow.setStatus(status);
	}

	// Computes transformation matrices for gimbal circles based on composition sequence.
	private void GetCircleMatrix(out mat4 xMat, out mat4 yMat, out mat4 zMat, out vec3 radii)
	{
		float x = EulerAngles.x;
		float y = EulerAngles.y;
		float z = EulerAngles.z;

		float bigRadius = 1.4f;
		float middleRadius = 1.3f;
		float smallRadius = 1.2f;

		xMat = MathLib.Rotate(new quat(x, 0.0f, 0.0f));
		yMat = MathLib.Rotate(new quat(x, y, 0.0f));
		zMat = MathLib.Rotate(new quat(x, y, z));

		radii = new vec3(1.4f, 1.3f, 1.2f);

		switch (compositionType)
		{
			case COMPOSITION_TYPE.XYZ:
				xMat = MathLib.ComposeRotationXYZ(new vec3(x, 0.0f, 0.0f));
				yMat = MathLib.ComposeRotationXYZ(new vec3(x, y, 0.0f));
				zMat = MathLib.ComposeRotationXYZ(new vec3(x, y, z));
				radii = new vec3(bigRadius, middleRadius, smallRadius);
				break;

			case COMPOSITION_TYPE.XZY:
				xMat = MathLib.ComposeRotationXZY(new vec3(x, 0.0f, 0.0f));
				zMat = MathLib.ComposeRotationXZY(new vec3(x, 0.0f, z));
				yMat = MathLib.ComposeRotationXZY(new vec3(x, y, z));
				radii = new vec3(bigRadius, smallRadius, middleRadius);
				break;

			case COMPOSITION_TYPE.YXZ:
				yMat = MathLib.ComposeRotationYXZ(new vec3(0.0f, y, 0.0f));
				xMat = MathLib.ComposeRotationYXZ(new vec3(x, y, 0.0f));
				zMat = MathLib.ComposeRotationYXZ(new vec3(x, y, z));
				radii = new vec3(middleRadius, bigRadius, smallRadius);
				break;

			case COMPOSITION_TYPE.YZX:
				yMat = MathLib.ComposeRotationYZX(new vec3(0.0f, y, 0.0f));
				zMat = MathLib.ComposeRotationYZX(new vec3(0.0f, y, z));
				xMat = MathLib.ComposeRotationYZX(new vec3(x, y, z));
				radii = new vec3(smallRadius, bigRadius, middleRadius);
				break;

			case COMPOSITION_TYPE.ZXY:
				zMat = MathLib.ComposeRotationZXY(new vec3(0.0f, 0.0f, z));
				xMat = MathLib.ComposeRotationZXY(new vec3(x, 0.0f, z));
				yMat = MathLib.ComposeRotationZXY(new vec3(x, y, z));
				radii = new vec3(middleRadius, smallRadius, bigRadius);
				break;

			case COMPOSITION_TYPE.ZYX:
				zMat = MathLib.ComposeRotationZYX(new vec3(0.0f, 0.0f, z));
				yMat = MathLib.ComposeRotationZYX(new vec3(0.0f, y, z));
				xMat = MathLib.ComposeRotationZYX(new vec3(x, y, z));
				radii = new vec3(smallRadius, middleRadius, bigRadius);
				break;
		}

		mat4 tr = new mat4(MathLib.Translate(node.WorldPosition));

		xMat = tr * xMat * MathLib.Rotate(new quat(0, 90, 0));
		yMat = tr * yMat * MathLib.Rotate(new quat(90, 0, 0));
		zMat = tr * zMat;
	}
}
