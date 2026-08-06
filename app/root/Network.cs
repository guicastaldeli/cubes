namespace App.Root;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

public class Network {
    private static Network? instance;
    public static Network I => instance ??= new Network();
    
    public const int BUFFER_SIZE = 65536;
    public const int MAX_PACKET_SIZE = 1400;
    
    public IPEndPoint? remoteEndPoint;

    public bool IsRunning { get; set; } = false;
    public bool IsConnected => remoteEndPoint != null && IsRunning;

    public Server Server;
    public Client Client;

    public Network() {
        this.Server = new Server(this);
        this.Client = new Client(this);
    }

    // Process Received
    public void ProcessReceived(ConcurrentQueue<Action> queue) {
        while(queue.TryDequeue(out var action)) {
            try {
                action();
            } catch(Exception err) {
                Console.WriteLine($"[Network] Process error: {err.Message}");
            }
        }
    }

    // Process Packets
    public void ProcessPackets() {
        Server.ProcessPackets();
        Client.ProcessPackets();
    }

    // Receive Loop
    public void ReceiveLoop(UdpClient udpClient, ConcurrentQueue<Action> queue, Action<IPEndPoint, byte[]> onDataReceived) {
        while(IsRunning) {
            try {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                byte[] data = udpClient!.Receive(ref endPoint);
                
                queue.Enqueue(() => onDataReceived?.Invoke(endPoint, data));
            } catch(SocketException err) when (
                err.SocketErrorCode == SocketError.ConnectionReset ||
                err.SocketErrorCode == SocketError.ConnectionAborted
            ) {
                if(IsRunning) {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    //Console.WriteLine($"[Network] Connection reset: {err.Message}");
                    Console.ResetColor();
                }
            } catch(Exception err) {
                if(IsRunning) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Network] Receive error: {err.Message}");
                    Console.ResetColor();
                }
            }
        }
    }

    /**
     *
     * Send
     *
     */
    // Send
    public void Send(byte[] data, IPEndPoint endPoint, UdpClient? udpClient) {
        try {
            if(udpClient == null || !IsRunning) return;
            if(data.Length > MAX_PACKET_SIZE) {
                SendChunked(data, endPoint, udpClient);
                return;
            }

            udpClient.Send(data, data.Length, endPoint);
        } catch(Exception err) {
            Console.WriteLine($"[Network] Send error: {err.Message}");
        }
    }

    // Send Chunked
    private void SendChunked(byte[] data, IPEndPoint endPoint, UdpClient? udpClient) {
        const int CHUNK_SIZE = 1200;
        const int HEADER_SIZE = 8;

        int totalChunks = (int)Math.Ceiling((double)data.Length / CHUNK_SIZE);
        for(int i = 0; i < totalChunks; i++) {
            int offset = i * CHUNK_SIZE;
            int length = Math.Min(CHUNK_SIZE, data.Length - offset);
            byte[] chunkData = new byte[length + HEADER_SIZE];
            
            BitConverter.GetBytes(totalChunks).CopyTo(chunkData, 0);
            BitConverter.GetBytes(i).CopyTo(chunkData, 4);
            Array.Copy(data, offset, chunkData, HEADER_SIZE, length);

            if(udpClient != null) udpClient.Send(chunkData, chunkData.Length, endPoint);
        }
    }

    /**
     *
     * Disconnect
     *
     */
    public void Disconnect(UdpClient? udpClient, Thread? receiveThread, ConcurrentQueue<Action> queue) {
        IsRunning = false;
        if(udpClient != null) udpClient.Close();
        if(receiveThread != null) receiveThread.Join(1000);
        queue.Clear();

        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("[Network] Disconnected");
        Console.ResetColor();
    }
}