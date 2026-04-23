namespace App.Root;
using App.Root.Packets;
using App.Root.Player;
using App.Root.ServerData;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

/**


    !!! IMPORTANT REMINDER !!!

    Fix the multiplayer, the chunks,
    the meshes, the player falling when enter
    everyhting etc...

    Also, see why the Aim its causing the
    multiplayer crash!!!


    **World meshes fixed!.
    
    */

class Server {
    private ServerDataManager serverDataManager;

    private UdpClient udpServer = null!;
    private Thread? serverThread;
    private Thread? tickThread;

    public int port;
    private bool running = false;
    public int maxPlayers;
    public ConcurrentDictionary<string, ServerPlayer> players = new();

    public Action? onTick;

    private PacketReassember reassember = new PacketReassember();

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
        int bufferSize = 65536;
        udpServer = new UdpClient(port);
        udpServer.Client.SendBufferSize = bufferSize;
        udpServer.Client.ReceiveBufferSize = bufferSize;
        
        running = true;

        serverThread = new Thread(receiveLoop) {
            IsBackground = true
        };
        serverThread.Start();

        tickThread = new Thread(tickLoop) {
            IsBackground = true
        };
        tickThread.Start();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"--- Server started on port {port} | Max players: {maxPlayers} ---");
        Console.ResetColor();
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
                    Context.SERVER,
                    out var handler
                )) {
                    if(handler == null) return;
                    handler.handle(json, remote);
                }
            } catch(SocketException ex) when (
                ex.SocketErrorCode == SocketError.ConnectionReset ||
                ex.SocketErrorCode == SocketError.ConnectionAborted
            ) {
                Console.WriteLine($"Client disconnected {ex.Message}");
            } catch(Exception err) {
                if(running) Console.Error.WriteLine("Server receive error!: " + err.Message + "\n" + err.StackTrace);
            }
        }
    }

    // Tick Loop
    private void tickLoop() {
        int cleanupCounter = 0;

        while(running) {
            try {
                foreach(var (id, player) in players) {
                    if(player.isTimedOut()) {
                        players.TryRemove(id, out _);
                        ServerSnapshot.getInstance().clearAll();
                        foreach(var (_, p) in players) {
                            ServerSnapshot.getInstance().register(DataType.PLAYER, p);
                        }
                        Console.WriteLine($"Player {id} timed out");
                    }
                }

                if(++cleanupCounter >= 200) {
                    reassember.cleanupStale();
                    cleanupCounter = 0;
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
            var packets = PacketChuncking.chunk(packet);
            foreach(var p in packets) {
                string json = p.serialize();
                byte[] data = Encoding.UTF8.GetBytes(json);
                int limit = 1400;
                if(data.Length > limit) {
                    Console.Error.WriteLine($"WARNING: Packet fragment still too large: {data.Length} bytes");
                    continue;
                }

                udpServer.Send(data, data.Length, endPoint);
            }
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

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- Server stopped! ---");
        Console.ResetColor();
    }
}