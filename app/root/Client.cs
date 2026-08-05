namespace App.Root;

using System.Net;
using System.Net.Sockets;
using App.Root._Sync;
using App.Root.Info;

public class Client {
    private static class Data {
        public static string? UserId { get; set; } = InfoController.UserId;
        public static string? Username { get; set; } = InfoController.Username;
        
        public static string? ServerIp { get; set; }
        public static int ServerPort { get; set; }

        public static bool IsConnected => Network.I.IsConnected;
    }

    private Network network;
    private SyncManager syncManager;
    private SyncQueue queue;

    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public event Action<byte[]>? OnDataReceived;
    public event Action<Packet>? OnSyncPacketReceived;

    public Client(Network network) {
        this.network = network;
        this.syncManager = SyncManager.I;
        this.queue = new SyncQueue();

        this.network.OnDataReceived += OnNetworkDataReceived;
    }

    // On Sync Packet
    private void OnSyncPacket(Packet packet) {
        var data = packet.ToBytes();
        SendToServer(data);
    }

    // On Network Data Received
    private void OnNetworkDataReceived(IPEndPoint endPoint, byte[] data) {
        OnDataReceived?.Invoke(data);
        
        try {
            var packet = Packet.FromBytes(data);
            if(packet.IsValid()) {
                if(packet.IsHandshake || packet.IsHandshakeResponse) {
                    HandshakePacket.Handle(packet, syncManager, SendPacket);
                    return;
                }

                OnSyncPacketReceived?.Invoke(packet);
                syncManager.ApplyPacket(packet);
            }
        } catch(Exception err) {
            Console.WriteLine($"[Client] Error processing packet: {err.Message}");
        }
    }

    // Process Queue
    public void ProcessQueue() {
        if(!Data.IsConnected) return;

        var packets = queue.DequeueAll();
        foreach(var packet in packets) Send(packet);

        Console.WriteLine($"[Client] Processed {packets.Count} queued packets");
    }

    // Process Packets
    public void ProcessPackets() {
        network.ProcessReceived();
    }

    // Send Packet
    private void SendPacket() {
        Send(new Packet {
            DataId = "client_join",
            Action = "join",
            UserId = Data.UserId,
            Timestamp = DateTime.UtcNow.Ticks,
            IsControl = true
        });
    }

    /**
     *
     * Connect
     *
     */
    public void Connect(string ip, int port) {
        if(!IP.IsValid(ip)) throw new ArgumentException($"Invalid IP address: {ip}");

        Data.ServerIp = ip;
        Data.ServerPort = port;

        network.remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        
        network.udpClient = new UdpClient();
        network.udpClient.Connect(network.remoteEndPoint);
        network.udpClient.Client.SendBufferSize = Network.BUFFER_SIZE;
        network.udpClient.Client.ReceiveBufferSize = Network.BUFFER_SIZE;

        network.IsRunning = true;

        network.receiveThread = new Thread(network.ReceiveLoop) { IsBackground = true, Name = "Network-Client" };
        network.receiveThread.Start();

        syncManager.OnPacketReceived += OnSyncPacket;

        OnConnected?.Invoke();

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[Client] Connected to {ip}:{port} as {Data.Username} ({Data.UserId})");
        Console.ResetColor();
    }

    /**
     *
     * Disconnect
     *
     */
    public void Disconnect() {
        if(Data.IsConnected) {
            Send(new Packet {
                DataId = "client_leave",
                Action = "leave",
                UserId = Data.UserId,
                Timestamp = DateTime.UtcNow.Ticks,
                IsControl = true
            });
        }

        network.Disconnect();
        syncManager.Stop();

        OnDisconnected?.Invoke();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Client] Disconnected");
        Console.ResetColor();
    }

    /**
     *
     * Send
     *
     */
    // Send
    public void Send(Packet packet) {
        if(!Data.IsConnected) {
            queue.Enqueue(packet);
            Console.WriteLine("[Client] Not connected, queuing packet");
            return;
        }

        packet.UserId = Data.UserId;
        
        var data = packet.ToBytes();
        SendToServer(data);
    }

    public void Send(byte[] data) {
        if(!Data.IsConnected) {
            Console.WriteLine("[Client] Not connected, cannot send");
            return;
        }

        SendToServer(data);
    }

    // Send To Server
    public void SendToServer(byte[] data) {
        if(network.remoteEndPoint != null) {
            network.Send(data, network.remoteEndPoint);
        }
    }
}