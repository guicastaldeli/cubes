namespace App.Root;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

public class Network : IDisposable {
    private static Network? instance;
    public static Network I => instance ??= new Network();
    
    public const int BUFFER_SIZE = 65536;
    public const int MAX_PACKET_SIZE = 1400;
    
    public UdpClient? udpClient;
    public IPEndPoint? remoteEndPoint;
    public Thread? receiveThread;

    private ConcurrentQueue<Action> receiveQueue = new();

    public event Action<IPEndPoint, byte[]>? OnDataReceived;

    public bool IsRunning => isRunning;
    public bool IsConnected => remoteEndPoint != null && isRunning;

    public bool isRunning = false;

    // Process Received
    public void ProcessReceived() {
        while(receiveQueue.TryDequeue(out var action)) {
            try {
                action();
            } catch(Exception err) {
                Console.WriteLine($"[Network] Process error: {err.Message}");
            }
        }
    }

    // Receive Loop
    public void ReceiveLoop() {
        while(isRunning) {
            try {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                byte[] data = udpClient!.Receive(ref endPoint);
                receiveQueue.Enqueue(() => OnDataReceived?.Invoke(endPoint, data));
            } catch(SocketException err) when (
                err.SocketErrorCode == SocketError.ConnectionReset ||
                err.SocketErrorCode == SocketError.ConnectionAborted
            ) {
                if(isRunning) {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"[Network] Connection reset: {err.Message}");
                    Console.ResetColor();
                }
            } catch(Exception err) {
                if(isRunning) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Network] Receive error: {err.Message}");
                    Console.ResetColor();
                }
            }
        }
    }

    // Dispose
    public void Dispose() {
        if(udpClient != null) udpClient.Dispose();
    }

    /**
     *
     * Send
     *
     */
    // Send
    public void Send(byte[] data, IPEndPoint endPoint) {
        try {
            if(udpClient == null || !isRunning) return;
            if(data.Length > MAX_PACKET_SIZE) {
                SendChunked(data, endPoint);
                return;
            }

            udpClient.Send(data, data.Length, endPoint);
        } catch(Exception err) {
            Console.WriteLine($"[Network] Send error: {err.Message}");
        }
    }

    // Send Chunked
    private void SendChunked(byte[] data, IPEndPoint endPoint) {
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
    public void Disconnect() {
        isRunning = false;
        if(udpClient != null) udpClient.Close();
        if(receiveThread != null) receiveThread.Join(1000);
        receiveQueue.Clear();

        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("[Network] Disconnected");
        Console.ResetColor();
    }
}