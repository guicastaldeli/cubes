namespace App.Root;
using System.Net;
using System.Net.Sockets;

class IPGetter {
    private string localhost = "127.0.0.1";

    // Get Local IP
    public string getLocal() {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        
        var endPoint = socket.LocalEndPoint as IPEndPoint;
        return endPoint?.Address.ToString() ?? localhost; 
    }
}