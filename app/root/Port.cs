namespace App.Root;
using System.Net;
using System.Net.Sockets;

public class Port {
    private static readonly Lazy<Port> _instance = new Lazy<Port>(() => new Port());
    public static Port Instance => _instance.Value;

    private int portNumber;

    /**
     *
     * Get
     *
     */
    public int Get() {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        portNumber = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return portNumber;
    }

    /**
     *
     * Set
     *
     */
    public void Set(int port) {
        portNumber = port;
    }
}