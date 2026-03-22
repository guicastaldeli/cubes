namespace App.Root;
using App.Root.Packets;

class Network {
    private Server server = null!;
    private Client client = null!;
    private IPGetter ipGetter;
    private Port port;

    private NetworkUpdate networkUpdate = null!;
    private DataSnapshot? cachedSnapshot = null;

    public bool isConnected => client?.connected ?? false;
    public string? playerId => client?.playerId;

    public Network() {
        this.ipGetter = new IPGetter();
        this.port = new Port();
    }

    // Get Server
    public Server getServer() {
        return server;
    }

    // Get Port
    public Port getPort() {
        return port;
    }

    // Get Client
    public Client getClient() {
        return client;
    }   

    // Poll Data
    public DataSnapshot? pollData() {
        if(client == null) return null;
        client.incomingData.TryDequeue(out var snapshot);
        return snapshot;
    }

    public void pollAndCache() {
        DataSnapshot? latest = null;
        DataSnapshot? s;
        while(client.incomingData.TryDequeue(out s)) {
            latest = s;
        }
        cachedSnapshot = latest;
    }

    // Cached Snapshot
    public DataSnapshot? getCachedSnapshot() {
        return cachedSnapshot;
    }

    // Is Host
    public bool isHost() {
        return server != null;
    }

    ///
    /// Update
    /// 
    public void initNetworkUpdate() {
        networkUpdate = new NetworkUpdate();
    }
    
    public NetworkUpdate getNetworkUpdate() {
        return networkUpdate;
    }

    ///
    /// Host
    /// 
    public void host(int port, int maxPlayers) {
        server = new Server(port, maxPlayers);
        server.start();

        string localIp = ipGetter.getLocal();
        string color = "\x1b[94m";
        string bold = "\x1b[1m";
        Console.WriteLine($"{color}{bold}~~~~~~~~~~ Server IP: {localIp}:{port} ~~~~~~~~~~");

        client = new Client();
        client.connect(localIp, port);
    }

    ///
    /// Join
    /// 
    public void join(string ip, int port) {
        client = new Client();
        client.connect(ip, port);
    }

    ///
    /// Stop
    /// 
    public void stop() {
        client?.disconnect();
        server?.stop();
        Thread.Sleep(200);
        client = null!;
        server = null!;
    }

    // Send State
    public void sendState(
        float x, 
        float y, 
        float z, 
        float yaw, 
        float pitch
    ) {
        if(client == null) return;
        var snapshot = new DataSnapshot();
        snapshot.data[DataType.PLAYER] = new List<Dictionary<string, object>> {
            new() {
                ["id"] = playerId ?? "",
                ["x"] = x, ["y"] = y, ["z"] = z,
                ["yaw"] = yaw, ["pitch"] = pitch
            }
        };
        client.send(PacketData.fromSnapshot(snapshot));
    }
}