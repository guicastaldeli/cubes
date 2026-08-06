namespace App.Root;
using System.Net;
using System.Net.Sockets;

public static class Port {
    private static int? cachedPort;

    // Is Available
    public static bool IsAvailable(int port) {
        try {
            using var listener = new TcpListener(IPAddress.Loopback, port);
        
            listener.Start();
            listener.Stop();

            return true;
        } catch(Exception err) {
            Console.WriteLine(err);
            return false;
        }
    }

    // Reset
    public static void Reset() {
        cachedPort = null;
    }

    // Get Default
    public static int GetDefault() {
        int val = 0;
        return val;
    }

    /**
     *
     * Get
     *
     */
    public static int Get() {
        if(cachedPort.HasValue) return cachedPort.Value;

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        cachedPort = port;
        return port;
    }

    /**
     *
     * Set
     *
     */
    public static void Set(int port) {
        cachedPort = port;
    }
}