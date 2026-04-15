namespace App.Root;
using System.Net;
using System.Net.Sockets;

class Port {
    public int get() {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }
}