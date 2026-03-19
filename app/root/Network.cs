namespace App.Root;
using App.Root.Packets;

class Network {
    private Server server = null!;
    private Client client = null!;

    public bool isConnected => client?.connected ?? false;
    public string? playerId => client?.playerId;

    public Server getServer() {
        return server;
    }

    public Client getClient() {
        return client;
    }

    // Send State
    public void sendState(
        float x,
        float y,
        float z,
        float yaw,
        float pitch
    ) {
        client?.sendState(x, y, z, yaw, pitch);
    }

    // Poll World
    public PacketWorld? packetWorld() {
        if(client == null) return null;
        client.incomingWorld.TryDequeue(out var world);
        return world;
    }

    ///
    /// Host
    /// 
    public void host(int port, int maxPlayers) {
        server = new Server(port, maxPlayers);
        server.start();

        client = new Client();
        client.connect("127.0.0.1", port);
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
        client = null!;
        server = null!;
    }
}