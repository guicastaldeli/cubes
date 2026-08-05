namespace App.Root;
using App.Root._Sync;
using App.Root._Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;


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

    private HashSet<string> handshakeComplete = new();

    public Server() {}
    public Server(Network network) {
        this.network = network;
        this.syncManager = SyncManager.I;

        this.network.OnDataReceived += OnNetworkDataReceived;
    }

    // Send Handshake
    private void SendHandshake(IPEndPoint endPoint) {
    var handshake = syncManager.GetHandshakePacket();

    using var writer = new BinaryWriter();
    writer.Write(handshake.SessionId);
    writer.Write(handshake.Key);
    writer.Write(handshake.Iv);
    writer.Write(handshake.Timestamp);

    var payload = writer.GetBytes();
    var packet = new Packet {
        DataId = "__handshake__",
        Action = "__handshake__",
        Payload = payload,
        Timestamp = DateTime.UtcNow.Ticks,
        IsDelta = false,
        SessionId = handshake.SessionId,
        IsHandshake = true,
        IsHandshakeResponse = false
    };

    Console.BackgroundColor = ConsoleColor.Blue;
    Console.WriteLine($"[Server] 📤 Sending handshake to {endPoint}");
    Console.ResetColor();

    // Send directly to client - NO ENCRYPTION
    network.Send(packet.ToBytes(), endPoint);
}

    // Process Packets
    public void ProcessPackets() {
        network.ProcessReceived();
    }

    // On Sync Packet
    private void OnSyncPacket(Packet packet) {
        var data = packet.ToBytes();
        Broadcast(data);
    }

    // On Network Data Received
    private void OnNetworkDataReceived(IPEndPoint endPoint, byte[] data) {
        string key = endPoint.ToString();
        if(!clients.ContainsKey(key)) {
            if(clients.Count >= Data.MaxPlayers) {
                Console.WriteLine($"[Server] Max players reached, rejecting {endPoint}");
                return;
            }

            clients[key] = endPoint;
            OnClientConnected?.Invoke(endPoint);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[Server] Client connected: {endPoint} ({clients.Count}/{Data.MaxPlayers})");
            Console.ResetColor();

            SendHandshake(endPoint);
        }

        OnDataReceived?.Invoke(endPoint, data);

        try {
            var packet = Packet.FromBytes(data);
            if(packet.IsHandshakeResponse) {
                handshakeComplete.Add(key);
                Console.WriteLine($"[Server] Handshake complete for {endPoint}");
                return;
            }

            if(!handshakeComplete.Contains(key)) {
                Console.WriteLine($"[Server] Handshake not complete for {endPoint}, ignoring packet");
                return;
            }

            syncManager.ApplyPacket(packet);
        } catch(Exception err) {
            Console.WriteLine($"[Server] Error processing packet: {err.Message}");
        }
    }

    /**
     *
     * Send To Client
     *
     */
    public void SendToClient(IPEndPoint endPoint, byte[] data) {
        network.Send(data, endPoint);
    }

    public void SendToClient(IPEndPoint endPoint, Packet packet) {
        var data = packet.ToBytes();
        network.Send(data, endPoint);
    }
    
    /**
     *
     * Start
     *
     */
    public void Start(int port, int maxPlayers) {
        if(network.IsRunning) return;

        if(!Port.IsAvailable(port)) {
            Console.WriteLine($"[Server] Port {port} is not available, finding another...");
            port = Port.Get();
            Console.WriteLine($"[Server] Using port {port} instead");
        }

        Data.Port = port;
        Data.MaxPlayers = maxPlayers;
        
        network.udpClient = new UdpClient(port);
        network.udpClient.Client.SendBufferSize = Network.BUFFER_SIZE;
        network.udpClient.Client.ReceiveBufferSize = Network.BUFFER_SIZE;

        network.IsRunning = true;

        network.receiveThread = new Thread(network.ReceiveLoop) { IsBackground = true, Name = "Network-Server" };
        network.receiveThread.Start();

        syncManager.Start();
        syncManager.OnPacketReceived += OnSyncPacket;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[Server] --- Server started with IP: {IP.Get()} on PORT: {Data.Port} ---");
        Console.ResetColor();
    }

    /**
     *
     * Broadcast
     *
     */
    // Broadcast
    public void Broadcast(byte[] data) {
        foreach(var client in clients.Values) {
            network.Send(data, client);
        }
    }

    public void Broadcast(Packet packet) {
        var data = packet.ToBytes();
        Broadcast(data);
    }

    // Broadcast Except
    public void BroascastExcept(byte[] data, IPEndPoint exclude) {
        foreach(var client in clients.Values) {
            if(!clients.Equals(exclude)) {
                network.Send(data, client);
            }
        }
    }

    /**
     *
     * Stop
     *
     */
    public void Stop() {
        clients.Clear();
        network.Disconnect();
        syncManager.Stop();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- Server stopped! ---");
        Console.ResetColor();
    }
}