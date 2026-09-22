// Displays an in-world GUI panel on an ObjectGui showing target information.
// Shows the target name, real-time distance from camera, and physical size.
// Used as a visual indicator for zoom target demonstration.

using System.Globalization;
using Unigine;

// In-world info panel that updates distance from camera in real-time.
public partial class TargetGui : Component
{
	private ObjectGui objectGui;
	private Gui gui;
	private WidgetLabel distanceLabel;

	// Creates window with target name, distance, and size labels.
	void Init()
	{
		objectGui = node as ObjectGui;
		if (!objectGui)
		{
			Log.Error("TargetGui.Init is not ObjectGui!\n");
		}
		gui = objectGui.GetGui();

		// Create centered window filling the ObjectGui surface
		WidgetWindow window = new WidgetWindow();
		gui.AddChild(window, Gui.ALIGN_CENTER);

		WidgetVBox vbox = new WidgetVBox();
		vbox.SetSpace(1, 3);
		window.AddChild(vbox, Gui.ALIGN_CENTER);
		window.Width = gui.Width;
		window.Height = gui.Height;

		// Target name from node name
		WidgetLabel targetLabel = new WidgetLabel(node.Name);
		targetLabel.FontSize = 50;
		vbox.AddChild(targetLabel);

		// Distance label - updated every frame
		double distance = (node.WorldPosition - Game.Player.WorldPosition).Length;
		distanceLabel = new WidgetLabel("Distance to label : " + distance.ToString("0.00", CultureInfo.InvariantCulture) + " units");
		distanceLabel.FontSize = 50;
		vbox.AddChild(distanceLabel);

		// Physical size of the ObjectGui in world units
		WidgetLabel sizeLabel = new WidgetLabel("Size of label : " + objectGui.PhysicalWidth + "x" + objectGui.PhysicalHeight);
		sizeLabel.FontSize = 50;
		vbox.AddChild(sizeLabel);
	}

	// Updates distance label as camera moves.
	void Update()
	{
		double distance = (node.WorldPosition - Game.Player.WorldPosition).Length;
		distanceLabel.Text = "Distance to label : " + distance.ToString("0.00", CultureInfo.InvariantCulture) + " units";
	}
}
