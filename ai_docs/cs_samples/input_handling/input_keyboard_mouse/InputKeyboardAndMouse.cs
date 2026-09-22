// Tracks keyboard and mouse input state. Monitors key presses, mouse button
// clicks, cursor position, movement deltas, and scroll wheel. Provides text
// input capture through Unicode event handling.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Unigine;

// Monitors keyboard keys and mouse input state.
public partial class InputKeyboardAndMouse : Component
{
	// Last typed character as Unicode string
	public string LastInputSymbol { get; private set; } = null;
	// Last key that was pressed down
	public Input.KEY? LastKeyDown { get; private set; } = null;
	// Last key that is currently held
	public Input.KEY? LastKeyPressed { get; private set; } = null;
	// Last key that was released
	public Input.KEY? LastKeyUp { get; private set; } = null;

	// Last mouse button pressed down
	public Input.MOUSE_BUTTON? LastMouseButtonDown { get; private set; } = null;
	// Last mouse button currently held
	public Input.MOUSE_BUTTON? LastMouseButtonPressed { get; private set; } = null;
	// Last mouse button released
	public Input.MOUSE_BUTTON? LastMouseButtonUp { get; private set; } = null;

	// Current mouse cursor position
	public ivec2? MouseCoord { get; private set; } = null;
	// Last non-zero mouse position delta (pixels)
	public ivec2? LastMouseCoordDelta { get; private set; } = null;
	// Last non-zero mouse movement delta
	public vec2? LastMouseDelta { get; private set; } = null;

	// Last vertical scroll wheel delta
	public int? LastMouseWheel { get; private set; } = null;
	// Last horizontal scroll wheel delta
	public int? LastMouseWheelHorizontal { get; private set; } = null;

	// Current mouse handle mode
	public Input.MOUSE_HANDLE? MouseHandle { get; private set; } = null;

	// Cached arrays of all key and button types
	private Array keys = null;
	private Array mouseButtons = null;

	// Tracks currently pressed keys to detect new presses
	private HashSet<Input.KEY> pressedKeys = null;
	private HashSet<Input.MOUSE_BUTTON> pressedMouseButtons = null;

	// Input arrays and event handlers are initialized.
	private void Init()
	{
		keys = Enum.GetValues(typeof(Input.KEY));
		mouseButtons = Enum.GetValues(typeof(Input.MOUSE_BUTTON));

		pressedKeys = new HashSet<Input.KEY>();
		pressedMouseButtons = new HashSet<Input.MOUSE_BUTTON>();

		InputKeyboardAndMouseUI.mouseHandleChanged += OnMouseHandleChanged;

		Input.EventTextPress.Connect(OnTextPressed);
	}

	// Keyboard and mouse states are polled each frame.
	private void Update()
	{
		// Poll all keyboard keys
		foreach (var key in keys)
		{
			Input.KEY currentKey = (Input.KEY)key;

			// Skip any ANY_* keys
			if (currentKey >= Input.KEY.ANY_SHIFT)
				continue;

			if (Input.IsKeyDown(currentKey))
				LastKeyDown = currentKey;

			if (Input.IsKeyPressed(currentKey) && !pressedKeys.Contains(currentKey))
			{
				LastKeyPressed = currentKey;
				pressedKeys.Add(currentKey);
			}

			if (Input.IsKeyUp(currentKey))
			{
				LastKeyUp = currentKey;
				pressedKeys.Remove(currentKey);
			}
		}

		// Poll all mouse buttons
		foreach (var button in mouseButtons)
		{
			Input.MOUSE_BUTTON currentButton = (Input.MOUSE_BUTTON)button;
			if (currentButton == Input.MOUSE_BUTTON.MOUSE_NUM_BUTTONS)
				continue;

			if (Input.IsMouseButtonDown(currentButton))
				LastMouseButtonDown = currentButton;

			if (Input.IsMouseButtonPressed(currentButton) && !pressedMouseButtons.Contains(currentButton))
			{
				LastMouseButtonPressed = currentButton;
				pressedMouseButtons.Add(currentButton);
			}

			if (Input.IsMouseButtonUp(currentButton))
			{
				LastMouseButtonUp = currentButton;
				pressedMouseButtons.Remove(currentButton);
			}
		}

		// Update mouse position and deltas
		MouseCoord = Input.MousePosition;

		if (Input.MouseDeltaPosition.Length2 > 0)
			LastMouseCoordDelta = Input.MouseDeltaPosition;

		if (Input.MouseDeltaPosition.Length2 > 0)
			LastMouseDelta = Input.MouseDeltaPosition;

		if (Input.MouseWheel != 0)
			LastMouseWheel = Input.MouseWheel;

		if (Input.MouseWheelHorizontal != 0)
			LastMouseWheelHorizontal = Input.MouseWheelHorizontal;

		MouseHandle = Input.MouseHandle;
	}

	// UI event handler is unsubscribed on shutdown.
	private void Shutdown()
	{
		InputKeyboardAndMouseUI.mouseHandleChanged -= OnMouseHandleChanged;
	}

	// Applies mouse handle mode change from UI.
	private void OnMouseHandleChanged(Input.MOUSE_HANDLE handle)
	{
		Input.MouseHandle = handle;
	}

	// Converts Unicode input to string for display.
	private void OnTextPressed(uint unicode)
	{
		byte[] bytes = BitConverter.GetBytes(unicode);
		LastInputSymbol = Encoding.Unicode.GetString(bytes);
	}
}
