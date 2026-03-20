using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using App.Root.ClientData;
using App.Root.Packets;

namespace App.Root;

class Client {
    public string? playerId {
        get;
        set;
    }

    public bool connected {
        get;
        set;
    }

    private UdpClient udpClient = null!;
    private IPEndPoint serverEndPoint = null!;
    private Thread? receiveThread;
    private Thread? pingThread;

    private ClientDataManager clientDataManager;

    public ConcurrentQueue<PacketWorld> incomingWorld = new();
    public ConcurrentQueue<PacketWorldData> incomingWorldData = new();

    private bool running = false;

    public Client() {
        this.clientDataManager = new ClientDataManager(this);
    }

    // Receive Loop
    private void receiveLoop() {
        while(running) {
            try {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref remote);
                string json = Encoding.UTF8.GetString(data);

                PacketType? type = Packet.peekType(json);
                if(type == null) continue;
                switch(type) {
                    case PacketType.JOIN:
                        clientDataManager.getClientJoin().handle(json);
                        break;
                    case PacketType.WORLD:
                        clientDataManager.getClientWorld().handle(json);
                        break;
                    case PacketType.WORLD_DATA:
                        clientDataManager.getClientWorldData().handle(json);
                        break;
                    case PacketType.PING:
                        break;
                }
            } catch(Exception err) {
                if(running) Console.Error.WriteLine("Client receive error: " + err.Message);
            }
        }
    }

    // Ping Loop
    private void pingLoop() {
        while(running) {
            try {
                if(connected) send(new PacketPing {
                    playerId = playerId
                });
                Thread.Sleep(1000);
            } catch(Exception err) {
                if(running) Console.Error.WriteLine("Client ping error: " + err.Message);
            }
        }
    }

    ///
    /// Connect
    /// 
    public void connect(string ip, int port) {
        serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        udpClient = new UdpClient();
        udpClient.Connect(serverEndPoint);
        running = true;

        receiveThread = new Thread(receiveLoop) {
            IsBackground = true
        };
        receiveThread.Start();

        pingThread = new Thread(pingLoop) {
            IsBackground = true
        };
        pingThread.Start();

        send(new PacketJoin());
        Console.WriteLine($"Connecting to {ip}:{port}");
    }

    ///
    /// Send
    /// 
    public void send(Packet packet) {
        try {
            string json = packet.serialize();
            byte[] data = Encoding.UTF8.GetBytes(json);
            udpClient.Send(data, data.Length);
        } catch(Exception err) {
            Console.Error.WriteLine("Client send error: " + err.Message);
        }
    }

    public void sendState(
        float x,
        float y,
        float z,
        float yaw,
        float pitch
    ) {
        if(!connected) return;
        send(new PacketState {
            playerId = playerId,
            x = x, y = y, z = z,
            yaw = yaw, pitch = pitch
        });
    }

    ///
    /// Disconnect
    /// 
    public void disconnect() {
        if(connected) send(new PacketLeave {
            playerId = playerId
        });
        running = false;
        connected = false;
        udpClient?.Close();
        Console.WriteLine("Disconnected");
    }
}