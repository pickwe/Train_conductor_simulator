// Demonstrates container widgets for organizing UI layouts.
// Shows VBox/HBox for linear stacking, VPaned/HPaned for resizable splits,
// GridBox for grid layouts, GroupBox for titled sections, TabBox for tabbed content,
// and ScrollBox for scrollable regions. Containers can nest for complex layouts.

using Unigine;

// Reference sample for all container widget types in UNIGINE GUI.
public partial class WidgetsContainers : Component
{
	// Root vertical container
	private WidgetVBox vBox = null;
	// Resizable vertical split between two sections
	private WidgetVPaned vPaned = null;

	// Horizontal containers for top and bottom sections
	private WidgetHBox hBoxTop = null;
	private WidgetHBox hBoxBottom = null;

	// Resizable horizontal split in top section
	private WidgetHPaned hPanedTop = null;

	// Grid layout for arranging items in rows/columns
	private WidgetGridBox gridBox = null;
	// Titled container with border
	private WidgetGroupBox groupBox = null;

	// Tabbed container and scrollable container
	private WidgetTabBox tabBox = null;
	private WidgetScrollBox scrollBox = null;
	// Builds a complex nested layout demonstrating all container types.
	[MethodInit(Order = -1)]
	private void Init()
	{
		Gui gui = Gui.GetCurrent();

		// Root VBox - vertical stacking of all content
		vBox = new WidgetVBox(gui);
		vBox.Background = 1;
		gui.AddChild(vBox, Gui.ALIGN_EXPAND);

		// VPaned splits screen into top/bottom resizable sections
		vPaned = new WidgetVPaned(gui);
		vPaned.Width = 450;
		vBox.AddChild(vPaned, Gui.ALIGN_EXPAND);

		// Top section HBox (blue tint) - horizontal layout
		hBoxTop = new WidgetHBox(gui);
		hBoxTop.Background = 1;
		hBoxTop.BackgroundColor = new vec4(0.0f, 0.0f, 1.0f, 0.5f);
		vPaned.AddChild(hBoxTop, Gui.ALIGN_EXPAND);

		// Bottom section HBox (cyan tint)
		hBoxBottom = new WidgetHBox(gui);
		hBoxBottom.Background = 1;
		hBoxBottom.BackgroundColor = new vec4(0.0f, 1.0f, 1.0f, 0.5f);
		vPaned.AddChild(hBoxBottom, Gui.ALIGN_EXPAND);

		// HPaned inside top section - left/right resizable split
		hPanedTop = new WidgetHPaned(gui);
		hBoxTop.AddChild(hPanedTop, Gui.ALIGN_EXPAND);

		// GridBox (red tint) - 3 columns, items flow left-to-right
		gridBox = new WidgetGridBox(gui, 3, 100, 100);
		gridBox.Background = 1;
		gridBox.BackgroundColor = new vec4(1.0f, 0.0f, 0.0f, 0.5f);
		gridBox.AddChild(new WidgetLabel(gui, "Item 0") { FontSize = 30 }, Gui.ALIGN_CENTER);
		gridBox.AddChild(new WidgetLabel(gui, "Item 1") { FontSize = 30 }, Gui.ALIGN_CENTER);
		gridBox.AddChild(new WidgetLabel(gui, "Item 2") { FontSize = 30 }, Gui.ALIGN_CENTER);
		gridBox.AddChild(new WidgetLabel(gui, "Item 3") { FontSize = 30 }, Gui.ALIGN_CENTER);
		gridBox.AddChild(new WidgetLabel(gui, "Item 4") { FontSize = 30 }, Gui.ALIGN_CENTER);
		gridBox.AddChild(new WidgetLabel(gui, "Item 5") { FontSize = 30 }, Gui.ALIGN_CENTER);
		gridBox.AddChild(new WidgetLabel(gui, "Item 6") { FontSize = 30 }, Gui.ALIGN_CENTER);
		gridBox.AddChild(new WidgetLabel(gui, "Item 7") { FontSize = 30 }, Gui.ALIGN_CENTER);
		gridBox.AddChild(new WidgetLabel(gui, "Item 8") { FontSize = 30 }, Gui.ALIGN_CENTER);

		hPanedTop.AddChild(gridBox, Gui.ALIGN_OVERLAP);

		// GroupBox (green tint) - titled section with border
		groupBox = new WidgetGroupBox(gui, "Group Box", 30, 30);
		groupBox.Background = 1;
		groupBox.BackgroundColor = new vec4(0.0f, 1.0f, 0.0f, 0.5f);
		groupBox.AddChild(new WidgetLabel(gui, "Item 0") { FontSize = 30 });
		groupBox.AddChild(new WidgetLabel(gui, "Item 1") { FontSize = 30 });
		groupBox.AddChild(new WidgetLabel(gui, "Item 2") { FontSize = 30 });
		groupBox.AddChild(new WidgetLabel(gui, "Item 3") { FontSize = 30 });
		hPanedTop.AddChild(groupBox, Gui.ALIGN_EXPAND);

		// TabBox - switchable tabbed content
		tabBox = new WidgetTabBox(gui);
		tabBox.AddTab("Tab 0");
		tabBox.AddChild(new WidgetLabel(gui, "Tab 0 Content") { FontSize = 50 });
		tabBox.AddTab("Tab 1");
		tabBox.AddChild(new WidgetLabel(gui, "Tab 1 Content") { FontSize = 50 });
		tabBox.AddTab("Tab 2");
		tabBox.AddChild(new WidgetLabel(gui, "Tab 2 Content") { FontSize = 50 });
		hBoxBottom.AddChild(tabBox, Gui.ALIGN_EXPAND);

		// ScrollBox - scrollable region for overflow content
		scrollBox = new WidgetScrollBox(gui, 30, 30);
		scrollBox.BackgroundColor = new vec4(0.0f, 0.0f, 1.0f, 0.5f);

		scrollBox.Width = 250;
		scrollBox.Height = 250;
		scrollBox.AddChild(new WidgetLabel(gui, "Item 0") { FontSize = 20 });
		scrollBox.AddChild(new WidgetLabel(gui, "Item 1") { FontSize = 20 });
		scrollBox.AddChild(new WidgetLabel(gui, "Item 2") { FontSize = 20 });
		scrollBox.AddChild(new WidgetLabel(gui, "Item 3") { FontSize = 20 });
		scrollBox.AddChild(new WidgetLabel(gui, "Item 4") { FontSize = 20 });
		scrollBox.AddChild(new WidgetLabel(gui, "Item 5") { FontSize = 20 });
		scrollBox.AddChild(new WidgetLabel(gui, "Item 6") { FontSize = 20 });
		scrollBox.AddChild(new WidgetLabel(gui, "Item 7") { FontSize = 20 });
		scrollBox.AddChild(new WidgetLabel(gui, "Item 8") { FontSize = 20 });
		hBoxBottom.AddChild(scrollBox, Gui.ALIGN_EXPAND);

		vBox.SetFocus();
	}
	private void Shutdown()
	{
		Gui.GetCurrent().RemoveChild(vBox);
	}
}
