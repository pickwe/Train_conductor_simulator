// Demonstrates TCP socket networking with client/server architecture.
// Can run as either client or server, sending camera position and text
// messages between connected peers using serialized message protocol.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// TCP socket client/server for camera sync and messaging.
public partial class TCPSocketsSample : Unigine.Component
{
	// Current host (client or server)
	private Host host;
	// Saved background update mode
	private Engine.BACKGROUND_UPDATE previousBackgroundUpdateMode = Engine.BACKGROUND_UPDATE.BACKGROUND_UPDATE_DISABLED;

	// Background update is enabled and UI is initialized.
	public void Init()
	{
		previousBackgroundUpdateMode = Engine.BackgroundUpdate;
		Engine.BackgroundUpdate = Engine.BACKGROUND_UPDATE.BACKGROUND_UPDATE_RENDER_NON_MINIMIZED;
		InitGui();
	}

	// Host is updated and UI is refreshed each frame.
	public void Update()
	{
		host?.Update();
		UpdateGui();
	}

	public void Shutdown()
	{
		host?.Dispose();
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
	private WidgetButton clientButton;
	private WidgetButton serverButton;
	private WidgetEditLine hostnameEditLine;
	private WidgetEditLine portEditLine;
	private WidgetGroupBox clientsGroupBox;

	// UI with client/server buttons and connection settings is created.
	private void InitGui()
	{
		consoleWasOnscreen = Unigine.Console.Onscreen;
		Unigine.Console.Onscreen = true;
		sampleDescriptionWindow = new SampleDescriptionWindow();
		sampleDescriptionWindow.createWindow(Gui.ALIGN_RIGHT);
		WidgetGroupBox groupBox = sampleDescriptionWindow.getParameterGroupBox();

		var vbox = new WidgetVBox();
		var buttonsHBox = new WidgetHBox();

		clientButton = new WidgetButton("Client");
		clientButton.Toggleable = true;
		clientButton.Height = 24;
		clientButton.EventClicked.Connect(connections, OnClientButtonClicked);
		buttonsHBox.AddChild(clientButton);

		serverButton = new WidgetButton("Server");
		serverButton.Toggleable = true;
		serverButton.Height = 24;
		serverButton.EventClicked.Connect(connections, OnServerButtonClicked);
		buttonsHBox.AddChild(serverButton);

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

		clientsGroupBox = new WidgetGroupBox("", 0, 5);
		vbox.AddChild(clientsGroupBox);

		groupBox.AddChild(vbox, Gui.ALIGN_TOP);
		groupBox.Arrange();

		for (int i = 0; i < Engine.NumArgs; i++)
		{
			String str = Engine.GetArg(i);
			if (str == "-server")
				serverButton.Toggled = true;
			else if (str == "-client")
				clientButton.Toggled = true;
		}
	}

	public void UpdateGui()
	{
		if (clientsGroupBox == null) return;

		void AddConnectionGui(string host, ushort port)
		{
			var hBox = new WidgetHBox();
			hBox.AddChild(new WidgetLabel($"{host}:{port}"));
			clientsGroupBox.AddChild(hBox);
		}
		while (clientsGroupBox.NumChildren > 0)
		{
			var child = clientsGroupBox.GetChild(0);
			clientsGroupBox.RemoveChild(child);
		}

		if (host is Client client)
		{
			if (client.IsConnectionActive)
				AddConnectionGui(client.Hostname, client.Port);
		}
		else if (host is Server server)
		{
			foreach (var clientWorker in server.Clients)
				AddConnectionGui(clientWorker.Hostname, clientWorker.Port);
		}
	}

	private void OnServerButtonClicked()
	{
		host?.Dispose();
		host = null;
		// Disable buttons so click events won't be triggered
		serverButton.Enabled = false;
		clientButton.Enabled = false;
		if (serverButton.Toggled)
		{
			var hostname = hostnameEditLine.Text;
			if (!ushort.TryParse(portEditLine.Text, out ushort port) || port == 0)
			{
				Log.Warning($"Invalid port: {portEditLine.Text}\n");
				serverButton.Toggled = false;
				serverButton.Enabled = true;
				clientButton.Enabled = true;
				return;
			}
			host = new Server(hostname, port);
			clientButton.Toggled = false;
			hostnameEditLine.Enabled = false;
			portEditLine.Enabled = false;
		}
		else
		{
			hostnameEditLine.Enabled = true;
			portEditLine.Enabled = true;
		}
		// Enable buttons back
		serverButton.Enabled = true;
		clientButton.Enabled = true;
	}

	private void OnClientButtonClicked()
	{
		host?.Dispose();
		host = null;
		// Disable buttons so click events won't be triggered
		serverButton.Enabled = false;
		clientButton.Enabled = false;

		if (clientButton.Toggled)
		{
			var hostname = hostnameEditLine.Text;
			if (!ushort.TryParse(portEditLine.Text, out ushort port) || port == 0)
			{
				Log.Warning($"Invalid port: {portEditLine.Text}\n");
				clientButton.Toggled = false;
				serverButton.Enabled = true;
				clientButton.Enabled = true;
				return;
			}
			host = new Client(hostname, port);

			serverButton.Toggled = false;
			hostnameEditLine.Enabled = false;
			portEditLine.Enabled = false;
		}
		else
		{
			hostnameEditLine.Enabled = true;
			portEditLine.Enabled = true;
		}
		// Enable buttons back
		serverButton.Enabled = true;
		clientButton.Enabled = true;
	}

	// =================== Constants ===================
	private const string DEFAULT_SERVER_HOSTNAME = "127.0.0.1";
	private const ushort DEFAULT_SERVER_PORT = 64000;

	private const int SEND_BUFFER_SIZE = 4096;
	private const int RECEIVE_BUFFER_SIZE = 4096;
	private const int MAX_CLIENT_CONNECTIONS = 8;

	private const int CLIENT_CONNECTION_TIMEOUT_MS = 10_000;
	private const int CLIENT_WRITE_TIMEOUT_US = 1_000;
	private const int CLIENT_READ_TIMEOUT_US = 1_000;

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

	// A simple abstract class for Client and Server, a helper to make this sample's logic more concise.
	public class Host : IDisposable
	{
		public virtual void Dispose()
		{ }

		public virtual void Update() { }
	}
	public class Client : Host
	{
		// Create a generic socket and start the Client's network work.

		// Note that the provided address is used as the target address in the subsequent `connect` call.
		// The socket's own address is set automatically!

		// The Server's network thread. It simply accepts new Client connections.
		public class NetworkWorker : IDisposable
		{
			private Unigine.Socket socket;
			private MessageQueue sendQueue = new MessageQueue();
			private MessageQueue receiveQueue = new MessageQueue();
			private CancellationTokenSource cancellationToken;
			private Task workerTask;
			private bool connect;

			public string Hostname => socket?.GetHost();
			public ushort Port => (ushort)socket?.GetPort();
			public bool IsActive => socket != null && socket.IsOpened;
			public NetworkWorker() { }
			public NetworkWorker(Socket s, bool connect)
			{
				socket = s;
				this.connect = connect;
				Start();
			}

			public void Dispose() => Reset();

			public void Send(Message m) => sendQueue.Push(m);
			public Message Receive()
			{
				return receiveQueue.Pop();
			}

			private void Start()
			{
				cancellationToken = new CancellationTokenSource();
				if (socket == null)
				{
					Log.Error("Client.NetworkWorker.Start() error. Can't start worker socket in uninitialized\n");
					return;
				}
				workerTask = Task.Run(() => Process(), cancellationToken.Token);
			}

			private void Reset()
			{
				cancellationToken?.Cancel();
				// Close the socket first to unblock any blocking calls (Connect, Read, Write),
				// then wait for the worker thread to finish.
				if (socket != null && socket.IsValidPtr)
					socket.Close();
				try { workerTask?.Wait(); } catch { }
				sendQueue.Clear();
				receiveQueue.Clear();
			}

			private enum ReceiveState { RECEIVE_HEADER, RECEIVE_PAYLOAD, UNPACK_MESSAGE }

			private void Process()
			{
				ulong receiveSize = 0;
				ulong sendSize = 0;

				Blob sendBlob = new Blob();
				Blob receiveBlob = new Blob();
				ReceiveState state = ReceiveState.RECEIVE_HEADER;

				if (connect)
				{
					string host = socket.GetHost();
					int port = socket.GetPort();
					if (socket.Connect(CLIENT_CONNECTION_TIMEOUT_MS))
					{
						Log.Message($"[Client::NetworkWorker] Connected to {host}:{port}\n");
					}
					else
					{
						Log.Warning($"[Client::NetworkWorker] Could not connect to {host}:{port}\n");
						return;
					}
				}

				while (!cancellationToken.IsCancellationRequested)
				{
					// --------- Send ---------
					if (sendBlob.GetSize() == 0)
					{
						var msg = sendQueue.Pop();
						if (msg != null)
						{
							sendSize = msg.Pack(sendBlob);
							sendBlob.SeekSet(0);
						}
					}
					if (sendBlob.GetSize() > 0 && socket.IsReadyToWrite(CLIENT_WRITE_TIMEOUT_US))
					{
						ulong written = socket.WriteStream(sendBlob, sendSize);
						if (written != sendSize)
						{
							// Could not send the message in full. Something must've gone wrong with the connection.

							Log.Message("[Client::NetworkWorker] Remote closed or error. Closing.\n");
							if (socket.IsValidPtr)
								socket.Close();
							return;
						}
						sendBlob.Clear();
					}

					// --------- Receive incoming messages ---------
					// Constantly trying to read header. If successful, trying to read whole data based on header
					switch (state)
					{
						case ReceiveState.RECEIVE_HEADER:

							// Going to read the next message's header.
							ulong headerSize = (ulong)Marshal.SizeOf<Message.Header>();
							receiveBlob.Resize(headerSize);
							receiveBlob.SeekSet(0);
							receiveSize = headerSize;
							break;

						case ReceiveState.RECEIVE_PAYLOAD:
							{
								receiveBlob.SeekSet(0);
								Message.Header header = new();
								header.Unpack(receiveBlob);

								// We don't reset the blob cursor back to start here after Blob::read
								// so that the next call to Socket.recvStream with this blob
								// does not overwrite the header.

								int size = header.Size;
								bool valid = (size >= 0
									&& size <= RECEIVE_BUFFER_SIZE)
									&& (header.MessageType >= 0 && (int)header.MessageType < Enum.GetNames(typeof(Message.MessageType)).Length);
								if (valid)
								{
									receiveBlob.Resize((ulong)size);
									receiveSize = Math.Max(0, (ulong)size - (sizeof(int) * 2));
								}
								else
								{
									// The message size or type are invalid.

									// This is probably because we somehow lost track of where messages start and end
									// in the incoming data stream. Our position in the stream is now undetermined,
									// and there isn't much we can do at this point. Close the connection.
									if (size > RECEIVE_BUFFER_SIZE)
										Log.Message($"[Client::NetworkWorker] Payload too large ({size})!\n");
									if (header.MessageType < 0 || (int)header.MessageType >= Enum.GetNames(typeof(Message.MessageType)).Length)
										Log.Message($"[Client::NetworkWorker] Invalid type ({header.MessageType})!\n");
									Log.Message("[Client::NetworkWorker] Closing the connection.\n");
									if (socket.IsValidPtr)
										socket.Close();
									return;
								}
							}
							break;

						case ReceiveState.UNPACK_MESSAGE:
							{
								receiveBlob.SeekSet(0);
								Message.Header header = new();
								header.Unpack(receiveBlob);
								Message message = null;
								switch (header.MessageType)
								{
									case Message.MessageType.TEXT: message = new TextMessage(); break;
									case Message.MessageType.CAMERA: message = new CameraMessage(); break;
								}
								if (message != null)
								{
									receiveBlob.SeekSet(0);
									message.Unpack(receiveBlob);
									receiveQueue.Push(message);
								}
								state = ReceiveState.RECEIVE_HEADER;
								receiveBlob.Clear();
								receiveSize = 0;
							}
							break;
					}

					if (receiveSize > 0 && socket.IsReadyToRead(CLIENT_READ_TIMEOUT_US))
					{
						// Read the next portion (header or payload) of the message from the socket.

						ulong read = socket.ReadStream(receiveBlob, receiveSize);// Reset the blob cursor the start so that we read the whole message, including the header.
						if (read == 0 || read != receiveSize)
						{
							Log.Message("[Client::NetworkWorker] Remote terminated or error. Closing.\n");
							if (socket.IsValidPtr)
								socket.Close();
							return;
						}

						switch (state)
						{
							case ReceiveState.RECEIVE_HEADER: state = ReceiveState.RECEIVE_PAYLOAD; break;
							case ReceiveState.RECEIVE_PAYLOAD: state = ReceiveState.UNPACK_MESSAGE; break;
						}
					}
				}
			}
		}

		private NetworkWorker networkWorker;
		public bool IsConnectionActive => networkWorker != null && networkWorker.IsActive;
		public string Hostname => networkWorker?.Hostname;
		public ushort Port => networkWorker?.Port ?? 0;
		public Client() { }
		public Client(string hostname, ushort port)
		{
			// Create TCP stream socket with target server address
			var socket = new Socket(Socket.SOCKET_TYPE.STREAM, hostname, port);
			if (!socket.IsOpened)
			{
				Log.Warning($"Could not resolve specified hostname ({hostname})!\n");
				return;
			}

			// Configure socket buffer sizes and non-blocking mode
			socket.Send(SEND_BUFFER_SIZE);
			socket.Recv(RECEIVE_BUFFER_SIZE);
			socket.Nonblock();

			// Start network worker thread that will connect and handle communication
			networkWorker = new NetworkWorker(socket, connect: true);

			// Disable player control - camera will be synced from server
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

		// Process incoming messages from server each frame.
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
					// Apply camera position/rotation from server
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

		private void OnMessageSendCommand(int argc, string[] argv)
		{
			var text = string.Join(" ", argv, 1, Math.Max(0, argc - 1));
			networkWorker.Send(new TextMessage(text));
		}
	}

	// =================== Server ===================
	public class Server : Host
	{
		// The Server's network thread. It simply accepts new Client connections.
		public class NetworkWorker : IDisposable
		{
			private Socket socket;
			private ConcurrentQueue<Socket> accepted = new ConcurrentQueue<Socket>();
			private CancellationTokenSource cancelToken;
			private Task workerTask;
			public string HostName => socket?.GetHost();
			public ushort Port => (ushort)socket?.GetPort();

			public NetworkWorker() { }
			public NetworkWorker(Socket s)
			{
				socket = s; Start();
			}
			public void Dispose() => Shutdown();
			public bool TryDequeue(out Socket s) => accepted.TryDequeue(out s);

			private void Start()
			{
				if (socket == null)
				{
					Log.Error("Server.NetworkWorker.Start() error. Can't start worker socket in uninitialized\n");
					return;
				}
				cancelToken = new CancellationTokenSource();
				workerTask = Task.Run(() => Process(cancelToken.Token), cancelToken.Token);
			}

			private void Shutdown()
			{
				cancelToken?.Cancel();
				// Close the socket first to unblock Accept(), then wait for the thread to finish.
				if (socket != null && socket.IsValidPtr)
					socket.Close();
				try { workerTask?.Wait(); } catch { }
				cancelToken?.Dispose();
			}
			private void Process(CancellationToken token)
			{
				while (!token.IsCancellationRequested)
				{
					var client = new Socket(Socket.SOCKET_TYPE.STREAM);
					if (socket.Accept(client))
						accepted.Enqueue(client);
				}
			}
		}

		private List<Client.NetworkWorker> clients = new List<Client.NetworkWorker>(MAX_CLIENT_CONNECTIONS);
		private NetworkWorker acceptWork;
		public List<Client.NetworkWorker> Clients => clients;
		public Server() { }
		public Server(string hostname, ushort port)
		{
			// Initialize a listen socket on the given address (hostname:port), and start the Server's network thread.
			var socket = new Socket(Socket.SOCKET_TYPE.STREAM, hostname, port);
			if (!socket.IsOpened)
			{
				Log.Warning($"Could not resolve specified hostname ({hostname})!\n");
				return;
			}
			socket.Nonblock();

			// Bind socket to the specified address
			if (!socket.Bind())
			{
				Log.Warning($"Could not bind socket to the specified address ({hostname}:{port})!\n");
				return;
			}

			// Set the socket as a 'listen' socket, and specify the max number of client connections.
			socket.Listen(MAX_CLIENT_CONNECTIONS);

			// Start background worker thread that accepts new connections
			acceptWork = new NetworkWorker(socket);

			// Register console command for sending text messages to all clients
			if (Unigine.Console.IsCommand("send_msg"))
				Unigine.Console.RemoveCommand("send_msg");
			Unigine.Console.AddCommand("send_msg", "[Network Sockets Sample] Send a text message to peer.", (argc, argv) => OnMessageSendCmd(argc, argv));
		}
		public override void Dispose()
		{
			acceptWork?.Dispose();
			foreach (var c in clients) c?.Dispose();

			if (Unigine.Console.IsCommand("send_msg"))
				Unigine.Console.RemoveCommand("send_msg");
		}

		// Manages client connections and broadcasts camera position each frame.
		public override void Update()
		{
			// Remove disconnected clients
			for (int i = 0; i < clients.Count; i++)
			{
				if (!clients[i].IsActive)
				{
					clients[i].Dispose();
					clients.RemoveAt(i);
				}
			}

			// Accept new client connections from the queue
			while (acceptWork.TryDequeue(out var socket))
			{
				var worker = new Client.NetworkWorker(socket, false);
				Log.Message($"[Server] Accepted a client connection ({worker.Hostname}:{worker.Port}).\n");
				clients.Add(worker);
			}

			// Process messages and send camera updates to each client
			for (int i = 0; i < clients.Count; i++)
			{
				var clientWorker = clients[i];

				// Check for incoming text messages
				var message = clientWorker.Receive();
				if (message is TextMessage tm && !string.IsNullOrEmpty(tm.Text))
					Log.Message($"Received a text message from Client [{i}]: {tm.Text}\n");

				// Send current camera position/rotation to client
				var player = Game.Player;
				if (player != null)
				{
					clientWorker.Send(new CameraMessage(player.WorldPosition, player.GetWorldRotation()));
				}
			}
		}

		private void OnMessageSendCmd(int argc, string[] argv)
		{
			var text = string.Join(" ", argv, 1, Math.Max(0, argc - 1));
			for (int i = 0; i < clients.Count; i++)
				clients[i].Send(new TextMessage(text));
		}
	}
}
