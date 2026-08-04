using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using App.Root._Sync;

namespace App.Root;

public class Server {
    private static class Data {
        private const int MAX_PLAYERS = 8;

        public static int Port { get; set; }
        public static int MaxPlayers { get; set; } = MAX_PLAYERS;

        public static bool IsRunning => Network.I.IsRunning;
        
        public static int ClientCount => Server.I.clients.Count;
        public static List<IPEndPoint> GetClients => Server.I.clients.Values.ToList();
    }

    private static Server? instance;
    public static Server I => instance ??= new Server();
    
    private Network network = null!;
    private SyncManager syncManager = null!;
    public ConcurrentDictionary<string, IPEndPoint> clients = new();

    public event Action<IPEndPoint, byte[]>? OnDataReceived;

    public event Action<IPEndPoint>? OnClientConnected;
    public event Action<IPEndPoint>? OnClientDisconnected;

    public Server() {}
    public Server(Network network) {
        this.network = network;
        this.syncManager = SyncManager.I;

        this.network.OnDataReceived += OnNetworkDataReceived;
    }
    
    /**
     *
     * Start
     *
     */
    public void Start(int port) {
        if(network.isRunning) return;

        Data.Port = port;
        network.udpClient = new UdpClient(port);
        network.udpClient.Client.SendBufferSize = Network.BUFFER_SIZE;
        network.udpClient.Client.ReceiveBufferSize = Network.BUFFER_SIZE;

        network.isRunning = true;
        network.receiveThread = new Thread(network.ReceiveLoop) {
            IsBackground = true,
            Name = "Network-Server"
        };
        network.receiveThread.Start();

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine($"[Network] Server started on port {port}...");
        Console.ResetColor();
    }
}