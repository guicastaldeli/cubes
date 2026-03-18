namespace App.Root;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using App.Root.Packets;
using App.Root.Player;
using App.Root.ServerData;

class Server {
    private UdpClient udpServer = null!;
    private ConcurrentDictionary<string, PlayerData> players = new();
    private Thread? serverThread;
    private Thread? tickThread;
    private bool running = false;

    private int port;
    private int maxPlayers;

    private ServerDataManager serverDataManager;

    public Server(int port, int maxPlayers) {
        this.port = port;
        this.maxPlayers = maxPlayers;

        this.serverDataManager = new ServerDataManager(this, players, maxPlayers);
    }

    ///
    /// Start
    /// 
    public void start() {
        udpServer = new UdpClient(port);
        running = true;

        serverThread = new Thread(receiveLoop) {
            IsBackground = true
        };
        serverThread.Start();

        tickThread = new Thread(tickLoop) {
            IsBackground = true
        };
        tickThread.Start();

        Console.WriteLine($"Server started on port {port}");
    }

    // Receive Loop
    private void receiveLoop() {
        while(running) {
            try {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpServer.Receive(ref remote);
                string json = Encoding.UTF8.GetString(data);

                PacketType? type = Packet.peekType(json);
                if(type == null) continue;
                switch(type) {
                    case PacketType.JOIN:
                        handleJoin(json, remote);
                        break;
                }
            }
        }
    }
}