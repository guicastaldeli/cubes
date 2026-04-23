namespace App.Root;
using App.Root.ClientData;
using App.Root.Packets;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;
using App.Root.Info;

class Client {
    public string? userId {
        get;
        set;
    }

    public bool connected {
        get;
        set;
    }

    private ClientDataManager clientDataManager;

    private UdpClient udpClient = null!;
    private IPEndPoint serverEndPoint = null!;
    private Thread? receiveThread;
    private Thread? pingThread;

    public ConcurrentQueue<DataSnapshot> incomingData = new();
    private bool running = false;

    private PacketReassember reassember = new PacketReassember();

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

                if(type == PacketType.CHUNK) {
                    var chunk = Packet.deserialize<PacketChunk>(json);
                    if(chunk == null) continue;

                    string? reassembled = reassember.tryReassemble(chunk);
                    if(reassembled == null) continue;

                    json = reassembled;
                    type = Packet.peekType(json);
                    if(type == null) continue;
                }
                if(PacketController.tryGet(
                    type.Value,
                    Context.CLIENT, 
                    out var handler
                )) {
                    if(handler == null) return;
                    handler.handle(json);
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
                    userId = userId
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

        send(new PacketJoin {
            userId = 
                InfoController.getInstance()
                    .getUserInfo()
                    .getId(),
            username =
                InfoController.getInstance()
                    .getUserInfo()
                    .getUsername()
        });

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        string italic = "\x1b[3m";
        Console.WriteLine($"{italic}*** Client Connecting to {ip}:{port} ***");
        Console.ResetColor();
    }

    ///
    /// Send
    /// 
    public void send(Packet packet) {
        try {
            packet.userId = userId;

            var packets = PacketChuncking.chunk(packet);
            foreach(var p in packets) {
                string json = p.serialize();
                byte[] data = Encoding.UTF8.GetBytes(json);
                udpClient.Send(data, data.Length);
            }
        } catch(Exception err) {
            Console.Error.WriteLine("Client send error: " + err.Message);
        }
    }

    ///
    /// Disconnect
    /// 
    public void disconnect() {
        if(connected) send(new PacketLeave {
            userId = userId
        });
        running = false;
        connected = false;
        udpClient?.Close();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Client Disconnected");
        Console.ResetColor();
    }
}