using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
	private static void Main(string[] args)
	{
		Console.WriteLine("Выберите позицию: ");

		switch (Console.ReadLine().ToLower())
		{
			case "server":
				SimpleSocketServer.Start();
				break;
			case "client":
				SimpleSocketClient.Start();
				break;
			default:
				break;
		}
	}
}

class SimpleSocketClient
{
	private static string _name;
	
	public static void Start()
	{
		Console.Write("Введите ваше имя: ");
		_name = Console.ReadLine();

		LaunchClient().Wait();
	}

	public static async Task LaunchClient()
	{
		IPAddress address = IPAddress.Parse("127.0.0.1");
		IPEndPoint endPoint = new IPEndPoint(address, 8888);

		Socket clientSocket = new Socket(address.AddressFamily,
SocketType.Stream
, ProtocolType.Tcp);

		await clientSocket.ConnectAsync(endPoint);

		byte[] buffer = new byte[1024];
		int bytesRead;

		bytesRead =await clientSocket.ReceiveAsync(buffer, SocketFlags.None);

		string welcomeMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
		Console.WriteLine(welcomeMessage);
		_ = Task.Run(() => HandleClient(clientSocket));
		while (true)
		{
			string sendMessage = _name + ":" + Console.ReadLine(); // Alemkhan:Hello
			byte[] sendMessageBytes = Encoding.UTF8.GetBytes(sendMessage);

			await clientSocket.SendAsync(sendMessageBytes, SocketFlags.None);

			



		}

	}

	private static async void HandleClient(Socket clientSocket)
	{
		byte[] buffer = new byte[1024];
		int bytesRead;

		while (true )
		{
			bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
			string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
			string[] receivedMessages = receivedMessage.Split(":");
			Console.WriteLine($"[{receivedMessages[0]}]: {receivedMessages[1]}");
		}
	}
}

class SimpleSocketServer
{
	private static List<Socket> _clientSockets = new List<Socket>();
	public static void Start()
	{
		LaunchServer().Wait();
	}

	public static async Task LaunchServer()
	{
		IPAddress address = IPAddress.Parse("127.0.0.1");
		IPEndPoint endPoint = new IPEndPoint(address, 8888);

		Socket serverSocket = new Socket(address.AddressFamily,
SocketType.Stream
, ProtocolType.Tcp);
		serverSocket.Bind(endPoint);

		serverSocket.Listen();

		Console.WriteLine("Сервер запущен. Ожидает подключений ...");

		while (true)
		{
			Socket clientSocket = await serverSocket.AcceptAsync();
			Console.WriteLine("Клиент был подключен");
			_clientSockets.Add(clientSocket);
			string welcomeMessage = "Добро пожаловать на сервер!";
			byte[] welcomeMessageBytes = Encoding.UTF8.GetBytes(welcomeMessage);

			await clientSocket.SendAsync(welcomeMessageBytes, SocketFlags.None);

			_ =
Task.Run
(() => HandleClient(clientSocket));
		}
	}

	private static async void HandleClient(Socket clientSocket)
	{
		byte[] buffer = new byte[1024];
		int bytesRead;

		while ((bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None))>0)
		{
			
			string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
			string[] receivedMessages = receivedMessage.Split(":");
			Console.WriteLine($"[{receivedMessages[0]}]: {receivedMessages[1]}");

			NotifyAllClient(receivedMessage, clientSocket);
		}
	}
	private static async void NotifyAllClient(string message, Socket target = null)
	{
		byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        foreach (var socket  in _clientSockets)
        {
            if (socket != target)
            {
                await socket.SendAsync(messageBytes, SocketFlags.None);
            }
        }

    }
}