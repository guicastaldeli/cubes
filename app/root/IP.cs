namespace App.Root;
using System.Net;
using System.Net.Sockets;

public class IP {
    private static readonly Lazy<IP> _instance = new Lazy<IP>(() => new IP());
    public static IP Instance => _instance.Value;
    
    private static string localhost = "127.0.0.1";

    /**
     *
     * Get Local
     *
     */
    public static string GetLocal() {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        
        var endPoint = socket.LocalEndPoint as IPEndPoint;
        return endPoint?.Address.ToString() ?? localhost; 
    }
}