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
    private Thread? serverThread;
    private Thread? tickThread;
    
    private bool running = false;
    public int port;

    public ConcurrentDictionary<string, PlayerData> players = new();
    public int maxPlayers;

    private ServerDataManager serverDataManager;

    public Server(int port, int maxPlayers) {
        this.port = port;
        this.maxPlayers = maxPlayers;

        this.serverDataManager = new ServerDataManager(this);
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
                        serverDataManager.getServerJoin().handle(json, remote);
                        break;
                    case PacketType.LEAVE:
                        serverDataManager.getServerLeave().handle(json, remote);
                        break;
                    case PacketType.STATE:
                        serverDataManager.getServerState().handle(json);
                        break;
                    case PacketType.PING:
                        serverDataManager.getServerPing().handle(json, remote);
                        break;                    
                }
            } catch(Exception err) {
                if(running) Console.Error.WriteLine("Server receive error!: " + err.Message);
            }
        }
    }

    // Tick Loop
    private void tickLoop() {
        while(running) {
            foreach(var (id, player) in players) {
                if(player.isTimedOut()) {
                    players.TryRemove(id, out _);
                    Console.WriteLine($"Player {id} timed out");
                }
            }
            if(players.Count > 0) world
        }
    }
}