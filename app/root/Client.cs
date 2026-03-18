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

    private bool running = false;

    public ConcurrentQueue<PacketWorld> incomingWorld = new();

    private ClientDataManager clientDataManager;

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

                }
            }
        }
    }

    ///
    /// Connect
    /// 
    public void connect(string ip, int port) {
        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        running = true;

        receiveThread = new Thread(receiveLoop) {
            IsBackground = true
        };
        receiveThread.Start();

        pingThread = new Thread(pingLoop) {
            IsBackground = true
        };
        pingThread.Start();

        send(PacketJoin());
        Console.WriteLine($"Connecting to {ip}:{port}");
    }
}