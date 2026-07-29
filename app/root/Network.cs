namespace App.Root;
using App.Root.Info;
using App.Root.Packets;

class Network {
    private Server server = null!;
    private Client client = null!;

    public static IP IP { get { return IP.Instance; } }
    public static Port Port { get { return Port.Instance; } }

    public bool isConnected => client?.connected ?? false;
    public string? userId => client?.userId;
    public string username => InfoController.Username;

    // Get Server
    public Server getServer() {
        return server;
    }

    // Get Client
    public Client getClient() {
        return client;
    }   

    // Is Host
    public bool isHost() {
        return server != null;
    }

    /**
     * 
     * Host
     *
     */
    // Host
    public void host(int port, int maxPlayers) {
        Port.Set(port);

        server = new Server(maxPlayers);
        server.start();

        string localIp = IP.GetLocal();
        string color = "\x1b[94m";
        string bold = "\x1b[1m";
        Console.WriteLine($"{color}{bold}~~~~~~~~~~ Server IP: {localIp}:{port} ~~~~~~~~~~");

        client = new Client();
        client.connect(localIp, port);
    }

    // Join
    public void join(string ip, int port) {
        client = new Client();
        client.connect(ip, port);
    }

    // Stop
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

    }
}