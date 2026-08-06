namespace App.Root;
using App.Root._Sync;
using App.Root.Info;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

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

    private UdpClient? udpClient;
    private Thread? receiveThread;

    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public event Action<byte[]>? OnDataReceived;
    public event Action<Packet>? OnSyncPacketReceived;

    private ConcurrentQueue<Action> receiveQueue = new();

    private bool handshakeComplete = false;

    public Client(Network network) {
        this.network = network;
        this.syncManager = SyncManager.I;
        this.queue = new SyncQueue();
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
                if(packet.IsHandshake) {
                    //Console.WriteLine($"[Client] Received handshake from server");
                    HandshakePacket.Handle(packet, syncManager, Send);
                    return;
                }
                if(packet.IsHandshakeResponse) {
                    Console.WriteLine($"[Client] Handshake response confirmed");
                    handshakeComplete = true;
                    SendPacket();
                    return;
                }
                if(!handshakeComplete) {
                    Console.WriteLine($"[Client] Waiting for handshake to complete, ignoring packet");
                    return;
                }

                OnSyncPacketReceived?.Invoke(packet);
                syncManager.ApplyPacket(packet);
            }
        } catch(Exception err) {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Client] Error processing packet: {err.Message}");
            Console.ResetColor();
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
        network.ProcessReceived(receiveQueue);
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

        udpClient = new UdpClient(Port.GetDefault());
        udpClient.Client.SendBufferSize = Network.BUFFER_SIZE;
        udpClient.Client.ReceiveBufferSize = Network.BUFFER_SIZE;

        network.IsRunning = true;

        receiveThread = new Thread(() => network.ReceiveLoop(udpClient, receiveQueue, OnNetworkDataReceived)) { IsBackground = true, Name = "Network-Client" };
        receiveThread.Start();

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

        network.Disconnect(udpClient, receiveThread, receiveQueue);
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
            network.Send(data, network.remoteEndPoint, udpClient);
        }
    }
}