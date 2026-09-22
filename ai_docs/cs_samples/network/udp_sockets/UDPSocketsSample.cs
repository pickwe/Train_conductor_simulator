// Demonstrates UDP socket networking with sender/receiver pattern.
// Can run as either sender or receiver, streaming camera position
// and text messages using connectionless UDP datagrams.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unigine;


#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// UDP socket sender/receiver for camera position streaming.
public partial class UDPSocketsSample : Component
{
	// Current peer (sender or receiver)
	private Peer peer;
	// Saved background update mode
	private Engine.BACKGROUND_UPDATE previousBackgroundUpdateMode = Engine.BACKGROUND_UPDATE.BACKGROUND_UPDATE_DISABLED;

	// Background update and console are enabled.
	public void Init()
	{
		previousBackgroundUpdateMode = Engine.BackgroundUpdate;
		consoleWasOnscreen = Unigine.Console.Onscreen;
		Unigine.Console.Onscreen = true;
		Engine.BackgroundUpdate = Engine.BACKGROUND_UPDATE.BACKGROUND_UPDATE_RENDER_NON_MINIMIZED;
		InitGui();
	}

	// Peer is updated each frame.
	public void Update()
	{
		peer?.Update();
	}

	// Peer is disposed and settings are restored on shutdown.
	public void Shutdown()
	{
		peer?.Dispose();
		Engine.BackgroundUpdate = previousBackgroundUpdateMode;
		Unigine.Console.Onscreen = consoleWasOnscreen;

		connections.DisconnectAll();

		var parent = WindowManager.MainWindow;
		var window = sampleDescriptionWindow.MainWindow;
		if (window && parent)
			parent.RemoveChild(window);
	}


	// Sample UI window
	SampleDescriptionWindow sampleDescriptionWindow = null;
	// Saved console state
	private bool consoleWasOnscreen;

	private EventConnections connections = new EventConnections();
	private WidgetButton senderButton;
	private WidgetButton receiverButton;
	private WidgetEditLine hostnameEditLine;
	private WidgetEditLine portEditLine;

	// UI with sender/receiver buttons and connection settings is created.
	private void InitGui()
	{
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow(Gui.ALIGN_RIGHT);
		WidgetGroupBox groupBox = sampleDescriptionWindow.getParameterGroupBox();

		var vbox = new WidgetVBox();
		var buttonsHBox = new WidgetHBox();

		senderButton = new WidgetButton("Sender");
		senderButton.Toggleable = true;
		senderButton.Height = 24;
		senderButton.EventClicked.Connect(connections, OnSenderButtonClicked);
		buttonsHBox.AddChild(senderButton);

		receiverButton = new WidgetButton("Receiver");
		receiverButton.Toggleable = true;
		receiverButton.Height = 24;
		receiverButton.EventClicked.Connect(connections, OnReceiverButtonClicked);
		buttonsHBox.AddChild(receiverButton);

		var spacer = new WidgetSpacer();
		spacer.Orientation = 0;
		buttonsHBox.AddChild(spacer);

		hostnameEditLine = new WidgetEditLine(DEFAULT_SERVER_HOSTNAME);
		hostnameEditLine.Width = 100;
		hostnameEditLine.Height = 18;
		hostnameEditLine.FontVOffset = -2;
		buttonsHBox.AddChild(hostnameEditLine);
		buttonsHBox.AddChild(new WidgetLabel(":"));

		portEditLine = new WidgetEditLine(DEFAULT_SERVER_PORT.ToString());
		portEditLine.Width = 60;
		portEditLine.Height = 18;
		portEditLine.FontVOffset = -2;
		portEditLine.Validator = Gui.VALIDATOR_UINT;
		buttonsHBox.AddChild(portEditLine);

		vbox.AddChild(buttonsHBox);
		vbox.AddChild(new WidgetVBox(0, 5));

		var clientsGroupBox = new WidgetGroupBox("", 0, 5);
		vbox.AddChild(clientsGroupBox);

		groupBox.AddChild(vbox, Gui.ALIGN_TOP);
		groupBox.Arrange();

		for (int i = 0; i < Engine.NumArgs; i++)
		{
			String str = Engine.GetArg(i);
			if (str == "-server")
				receiverButton.Toggled = true;
			else if (str == "-client")
				senderButton.Toggled = true;
		}
	}

	private void OnReceiverButtonClicked()
	{
		peer?.Dispose();
		peer = null;
		// Disable buttons so click events won't be triggered
		receiverButton.Enabled = false;
		senderButton.Enabled = false;
		if (receiverButton.Toggled)
		{
			var hostname = hostnameEditLine.Text;
			if (!ushort.TryParse(portEditLine.Text, out ushort port) || port == 0)
			{
				Log.Warning($"Invalid port: {portEditLine.Text}\n");
				receiverButton.Toggled = false;
				receiverButton.Enabled = true;
				senderButton.Enabled = true;
				return;
			}
			peer = new Receiver(hostname, port);
			senderButton.Toggled = false;
			hostnameEditLine.Enabled = false;
			portEditLine.Enabled = false;
		}
		else
		{
			hostnameEditLine.Enabled = true;
			portEditLine.Enabled = true;
		}
		// Enable buttons back
		receiverButton.Enabled = true;
		senderButton.Enabled = true;
	}

	private void OnSenderButtonClicked()
	{
		peer?.Dispose();
		peer = null;
		// Disable buttons so click events won't be triggered
		receiverButton.Enabled = false;
		senderButton.Enabled = false;

		if (senderButton.Toggled)
		{
			var hostname = hostnameEditLine.Text;
			if (!ushort.TryParse(portEditLine.Text, out ushort port) || port == 0)
			{
				Log.Warning($"Invalid port: {portEditLine.Text}\n");
				senderButton.Toggled = false;
				receiverButton.Enabled = true;
				senderButton.Enabled = true;
				return;
			}
			peer = new Sender(hostname, port);

			receiverButton.Toggled = false;
			hostnameEditLine.Enabled = false;
			portEditLine.Enabled = false;
		}
		else
		{
			hostnameEditLine.Enabled = true;
			portEditLine.Enabled = true;
		}
		// Enable buttons back
		receiverButton.Enabled = true;
		senderButton.Enabled = true;
	}


	// =================== Constants ===================
	private const string DEFAULT_SERVER_HOSTNAME = "127.0.0.1";
	private const ushort DEFAULT_SERVER_PORT = 64000;

	private const int SEND_BUFFER_SIZE = 4096;
	private const int RECEIVE_BUFFER_SIZE = 4096;

	// =================== Messages ===================[

	public abstract class Message
	{
		public enum MessageType { TEXT = 0, CAMERA = 1 }

		public struct Header
		{
			public MessageType MessageType;
			public int Size; // Full message size (including header)
			public ulong Pack(Blob destinationBlob)
			{
				destinationBlob.WriteInt((int)MessageType);
				destinationBlob.WriteInt(Size);
				return sizeof(int) * 2;
			}
			public ulong Unpack(Blob sourceBlob)
			{
				MessageType = (MessageType)sourceBlob.ReadInt();
				Size = (int)sourceBlob.ReadInt();
				return sizeof(int) * 2;
			}
		}
		[HideInEditor]
		public Header header;

		public virtual ulong Pack(Blob destinationBlob)
		{
			return 0;
		}

		public virtual ulong Unpack(Blob sourceBlob)
		{
			return 0;
		}
	}

	public class TextMessage : Message
	{
		[HideInEditor]
		public string Text = string.Empty;

		public TextMessage()
		{
			header = new Header
			{
				MessageType = MessageType.TEXT,
				Size = 0
			};
		}
		public TextMessage(string text) : this()
		{
			Text = text ?? string.Empty;
		}

		public override ulong Pack(Blob destinationBlob)
		{
			ulong cursor = destinationBlob.Tell();

			header.Pack(destinationBlob); // Writing empty header to reserve memory for it in blob

			// Writing all content
			int length = Text?.Length ?? 0;
			if (length > 0)
			{
				destinationBlob.WriteString(Text);
			}
			ulong cursorEnd = destinationBlob.Tell();
			ulong size = cursorEnd - cursor;
			header.Size = (int)size;

			// writing header data to reserved memory in the blob
			destinationBlob.SeekSet(cursor);
			header.Pack(destinationBlob);

			// Setting cursor back to the data end
			destinationBlob.SeekSet(cursorEnd);
			return size;
		}

		public override ulong Unpack(Blob sourceBlob)
		{
			ulong cursor = sourceBlob.Tell();

			header.Unpack(sourceBlob);
			Text = sourceBlob.ReadString();

			return sourceBlob.Tell() - cursor;
		}
	}

	public class CameraMessage : Message
	{
		[HideInEditor]
		public dvec3 Position;
		[HideInEditor]
		public quat Rotation;

		public CameraMessage()
		{
			header = new Header
			{
				MessageType = MessageType.CAMERA,
				Size = 0
			};
		}
		public CameraMessage(dvec3 position, quat rotation) : this()
		{
			this.Position = position;
			this.Rotation = rotation;
		}

		public override ulong Pack(Blob destinationBlob)
		{
			ulong cursor = destinationBlob.Tell();
			ulong headerSize = header.Pack(destinationBlob); // Writing empty header to reserve memory for it in blob

			// Writing all content
			destinationBlob.WriteDVec3(Position);
			destinationBlob.WriteQuat(Rotation);
			ulong cursorEnd = destinationBlob.Tell();
			ulong size = cursorEnd - cursor;
			header.Size = (int)size;

			// Writing header data to reserved memory in the blob
			destinationBlob.SeekSet(cursor);
			header.Pack(destinationBlob);

			// Setting cursor back to the data end
			destinationBlob.SeekSet(cursorEnd);
			return size;
		}

		public override ulong Unpack(Blob sourceBlob)
		{
			ulong cursor = sourceBlob.Tell();
			header.Unpack(sourceBlob);

			Position = sourceBlob.ReadDVec3();
			Rotation = sourceBlob.ReadQuat();
			return sourceBlob.Tell() - cursor;
		}
	}

	// =================== A thread-safe queue for network messages. ===================
	// The queue is used to keep message sending sequence
	public class MessageQueue
	{
		private Queue<Message> queue = new Queue<Message>();
		private object mutex = new object();
		private int maxSize = 8;

		public void Push(Message message)
		{
			lock (mutex)
			{
				queue.Enqueue(message);

				if (queue.Count >= maxSize)
				{
					Message oldMessage = queue.Dequeue();
				}
			}
		}

		public Message Pop()
		{
			lock (mutex)
			{
				if (queue.Count > 0)
				{
					Message message = queue.Dequeue();
					return message;
				}
			}
			return null;
		}

		public void Clear()
		{
			lock (mutex)
			{
				queue.Clear();
			}
		}
	}

	// ================================================================
	// A simple abstract class for Sender and Receiver, used to make the sample's logic a bit more concise.
	public class Peer : IDisposable
	{
		public virtual void Update() { }
		public virtual void Dispose()
		{
			throw new NotImplementedException();
		}
	};

	// The 'Sender'. It sends messages to a network peer (Receiver) at the given address.
	public class Sender : Peer
	{
		// The Sender's network thread. Takes messages from the queue and sends them via UDP.
		public class NetworkWorker : IDisposable
		{
			public NetworkWorker() { }
			public NetworkWorker(Socket s)
			{
				if (s == null)
				{
					Log.Error("Can't initiate Sender.Network. Get null socket");
					return;
				}
				socket = s;
				Start();
			}
			private Unigine.Socket socket;
			private MessageQueue queue = new MessageQueue();
			private CancellationTokenSource cancellationToken;
			private Task workerTask;

			public string Hostname => socket?.GetHost();
			public ushort Port => (ushort)socket?.GetPort();
			public bool IsActive => socket != null && socket.IsOpened;

			public void Dispose() => Reset();

			public void Send(Message m) => queue.Push(m);
			private void Start()
			{
				cancellationToken = new CancellationTokenSource();
				if (socket == null)
				{
					Log.Error("Sender.NetworkWorker.Start() error. Can't start worker socket in uninitialized\n");
					return;
				}
				workerTask = Task.Run(() => Process(), cancellationToken.Token);
			}

			private void Reset()
			{
				cancellationToken?.Cancel();
				// Close the socket first to unblock any blocking calls,
				// then wait for the worker thread to finish.
				if (socket != null && socket.IsValidPtr)
					socket.Close();
				try { workerTask?.Wait(); } catch { }
				queue.Clear();
			}

			private void Process()
			{
				// Takes messages from the outgoing messages queue and sends them to the peer.
				ulong sendSize = 0;

				Blob sendBlob = new Blob();

				while (!cancellationToken.IsCancellationRequested)
				{
					// --------- Send ---------
					if (sendBlob.GetSize() == 0)
					{
						var msg = queue.Pop();
						if (msg != null)
						{
							sendSize = msg.Pack(sendBlob);
							sendBlob.SeekSet(0);
						}
					}
					if (sendBlob.GetSize() > 0)
					{
						socket.WriteStream(sendBlob, sendBlob.GetSize());
						sendBlob.Clear();
					}
				}
			}
		}

		private NetworkWorker networkWorker;
		public bool IsConnectionActive => networkWorker != null && networkWorker.IsActive;
		public string Hostname => networkWorker?.Hostname;
		public ushort Port => networkWorker?.Port ?? 0;
		public Sender() { }
		public Sender(string hostname, ushort port)
		{
			// Create a simple UDP socket.

			// Note that the given address (hostname:port) is used as the source address in subsequent
			// 'send' calls (Socket::write*). The socket's own address is set automatically.
			var socket = new Socket(Socket.SOCKET_TYPE.DGRAM, hostname, port);
			if (!socket.IsOpened)
			{
				Log.Warning($"Could not resolve specified hostname ({hostname})!\n");
				return;
			}

			// Configure send buffer size
			socket.Send(SEND_BUFFER_SIZE);

			// Start network worker thread for sending messages
			networkWorker = new NetworkWorker(socket);

			// Disable player control - sender streams camera position
			var player = Game.Player;
			if (player != null) player.Controlled = false;

			// Register console command for sending text messages
			if (Unigine.Console.IsCommand("send_msg"))
				Unigine.Console.RemoveCommand("send_msg");
			Unigine.Console.AddCommand("send_msg", "[Network Sockets Sample] Send a text message to peer.", (argc, argv) => OnMessageSendCommand(argc, argv));
		}
		public override void Dispose()
		{
			networkWorker?.Dispose();
			var player = Game.Player;
			if (player != null) player.Controlled = true;
			if (Unigine.Console.IsCommand("send_msg"))
				Unigine.Console.RemoveCommand("send_msg");
		}

		// Sends camera position to receiver each frame.
		public override void Update()
		{
			if (networkWorker == null || !networkWorker.IsActive) return;

			// Pack current camera transform and send to receiver
			var player = Game.Player;
			var message = new CameraMessage(
			(Vec3)player.WorldPosition,
			player.GetWorldRotation());
			networkWorker.Send(message);
		}

		private void OnMessageSendCommand(int argc, string[] argv)
		{
			var text = string.Join(" ", argv, 1, Math.Max(0, argc - 1));
			networkWorker.Send(new TextMessage(text));
		}
	}
	public class Receiver : Peer
	{
		public Receiver() { }
		public Receiver(string hostname, ushort port)
		{
			// Create UDP datagram socket for receiving messages
			var socket = new Socket(Socket.SOCKET_TYPE.DGRAM, hostname, port);
			if (!socket.IsOpened)
			{
				Log.Warning($"Could not resolve specified hostname ({hostname})!\n");
				return;
			}

			// Configure receive buffer and non-blocking mode
			socket.Recv(RECEIVE_BUFFER_SIZE);
			socket.Nonblock();

			// Bind the socket to the address provided the in the `Socket::create` call above.
			if (!socket.Bind())
			{
				Log.Warning("Could not bind socket to the specified address (%s:%d)!\n", hostname, port);
				return;
			}

			// Start network worker thread for receiving messages
			networkWorker = new NetworkWorker(socket);

			// Disable player control - camera will be synced from sender
			var player = Game.Player;
			if (player != null) player.Controlled = false;
		}

		// The Receiver's network thread. Reads incoming UDP datagrams and pushes parsed messages to the queue.
		public class NetworkWorker : IDisposable
		{
			public NetworkWorker() { }
			public NetworkWorker(Socket s)
			{
				socket = s;
				Start();
			}
			private Unigine.Socket socket;
			private MessageQueue queue = new MessageQueue();
			private CancellationTokenSource cancellationToken;
			private Task workerTask;

			public string Hostname => socket?.GetHost();
			public ushort Port => (ushort)socket?.GetPort();
			public bool IsActive => socket != null && socket.IsOpened;

			public void Dispose() => Reset();

			public Message Receive()
			{
				return queue.Pop();
			}
			public Message ExtractMessage(Blob blob)
			{
				Message.Header header = new();
				Message message = null;

				header.Unpack(blob);

				blob.SeekSet(0);

				switch (header.MessageType)
				{
					case Message.MessageType.TEXT: message = new TextMessage(); break;
					case Message.MessageType.CAMERA: message = new CameraMessage(); break;
					default: break;
				}

				if (message != null)
					message.Unpack(blob);

				return message;
			}

			private void Start()
			{
				cancellationToken = new CancellationTokenSource();
				if (socket == null)
				{
					Log.Error("Receiver.NetworkWorker.Start() error. Can't start worker socket in uninitialized\n");
					return;
				}
				workerTask = Task.Run(() => Process(), cancellationToken.Token);
			}

			private void Reset()
			{
				cancellationToken?.Cancel();
				// Close the socket first to unblock any blocking calls,
				// then wait for the worker thread to finish.
				if (socket != null && socket.IsValidPtr)
					socket.Close();
				try { workerTask?.Wait(); } catch { }
				queue.Clear();
			}

			private void Process()
			{
				// Receive incoming messages from the peer, parse and push them to the queue.

				Blob blob = new Blob();

				while (!cancellationToken.IsCancellationRequested)
				{

					ulong read = socket.ReadStream(blob, RECEIVE_BUFFER_SIZE);// Reset the blob cursor the start so that we read the whole message, including the header.
					blob.SeekSet(0);
					if (blob.GetSize() > 0)
					{
						var message = ExtractMessage(blob);

						if (message != null)
							queue.Push(message);
					}
					blob.Clear();
				}
			}
		}

		private NetworkWorker networkWorker;
		public bool IsActive => networkWorker != null && networkWorker.IsActive;
		public string Hostname => networkWorker?.Hostname;
		public ushort Port => networkWorker?.Port ?? 0;

		public override void Dispose()
		{
			networkWorker?.Dispose();
			var player = Game.Player;
		}

		// Process incoming messages from sender each frame.
		public override void Update()
		{
			if (networkWorker == null || !networkWorker.IsActive) return;

			// Process up to 16 messages per frame to avoid blocking
			int processed = 0;
			Message msg = networkWorker.Receive();
			while (msg != null && processed < 16)
			{
				switch (msg)
				{
					// Log text messages to console
					case TextMessage tm:
						if (!string.IsNullOrEmpty(tm.Text))
							Log.Message($"Received a text message from Server: {tm.Text}\n");
						break;
					// Apply camera position/rotation from sender
					case CameraMessage cm:
						{
							var player = Game.Player;
							if (player != null)
							{
								player.WorldPosition = (Vec3)cm.Position;
								player.SetWorldRotation(cm.Rotation);
							}
						}
						break;
				}
				msg = networkWorker.Receive();
				processed++;
			}
		}
	}
}
