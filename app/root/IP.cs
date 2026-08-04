namespace App.Root;
using System.Net;
using System.Net.Sockets;

public class IP {
    private static string? cachedIp;

    // Is Valid
    public static bool IsValid(string ip) {
        bool val = IPAddress.TryParse(ip, out _);
        return val;
    }

    /**
     *
     * Get
     *
     */
    public static string Get() {
        if(!string.IsNullOrEmpty(cachedIp)) return cachedIp;

        try {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);

            if(socket.LocalEndPoint is IPEndPoint endPoint) {
                cachedIp = endPoint.Address.ToString();
                return cachedIp;
            }
        } catch(Exception err) {
            Console.WriteLine(err);
        }

        cachedIp = "127.0.0.1";
        return cachedIp;
    }

    /**
     *
     * Get All
     *
     */
    public static List<string> GetAll() {
        var ips = new List<string>();
        foreach(var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
            if(ip.AddressFamily == AddressFamily.InterNetwork) {
                ips.Add(ip.ToString());
            }
        }

        return ips;
    }
}