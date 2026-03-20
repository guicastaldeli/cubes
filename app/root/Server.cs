namespace App.Root;
using App.Root.Env;
using App.Root.Env.World;
using App.Root.Packets;
using App.Root.Player;
using App.Root.ServerData;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server {
    private UdpClient udpServer = null!;
    private Thread? serverThread;
    private Thread? tickThread;

    private bool running = false;
    public int port;

    public ConcurrentDictionary<string, ServerPlayer> players = new();
    public int maxPlayers;

    private ServerDataManager serverDataManager;

    public Action? onTick;

    public Server(int port, int maxPlayers) {
        this.port = port;
        this.maxPlayers = maxPlayers;

        this.serverDataManager = new ServerDataManager(this);
    }

    // Get Server Data Manager
    public ServerDataManager getServerDataManager() {
        return serverDataManager;
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
                    case PacketType.PING:
                        serverDataManager.getServerPing().handle(json, remote);
                        break;
                    case PacketType.DATA:
                        serverDataManager.getServerData().handle(json, remote);
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
            try {
                foreach(var (id, player) in players) {
                    if(player.isTimedOut()) {
                        players.TryRemove(id, out _);
                        Console.WriteLine($"Player {id} timed out");
                    }
                }
                if(players.Count > 0) onTick?.Invoke();
                Thread.Sleep(50);
            } catch(Exception err) {
                if(running) Console.Error.WriteLine("Server tick error: " + err.Message);
            }
        }
    }

    ///
    /// Send
    /// 
    public void send(Packet packet, IPEndPoint endPoint) {
        try {
            string json = packet.serialize();
            byte[] data = Encoding.UTF8.GetBytes(json);
            udpServer.Send(data, data.Length, endPoint);
        } catch(Exception err) {
            Console.Error.WriteLine("Server send error: " + err.Message);
        }
    }

    ///
    /// Stop
    /// 
    public void stop() {
        running = false;
        udpServer?.Close();
        Console.WriteLine("Server stopped!");
    }
}