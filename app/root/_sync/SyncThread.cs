namespace App.Root._Sync;
using System.Collections.Concurrent;

public class SyncThread {
    private SyncManager syncManager;

    private Thread thread;
    private ConcurrentQueue<Packet> sendQueue = new();
    private ConcurrentQueue<Packet> receiveQueue = new();

    private AutoResetEvent signal = new AutoResetEvent(false);

    private bool running = false;

    public SyncThread(SyncManager syncManager) {
        this.syncManager = syncManager;
        this.thread = new Thread(Run) {
            IsBackground = true,
            Name = "SyncThread"
        };
    }

    /**
     *
     * Enqueue
     *
     */
    // Enqueue Packet
    public void EnqueuePacket(Packet packet) {
        sendQueue.Enqueue(packet);
        signal.Set();
    }

    // Enqueue Received Packet
    public void EnqueueReceivedPacket(Packet packet) {
        receiveQueue.Enqueue(packet);
        signal.Set();
    }

    /**
     *
     * Process
     *
     */
    // Process Send Packet
    private void ProcessSendPacket(Packet packet) {
        try {
            var bytes = packet.ToBytes();
            Console.WriteLine($"[SyncThread] Sent packet: {packet.DataId} ({bytes.Length} bytes)");
        } catch(Exception err) {
            Console.WriteLine($"[SyncThread] Send error: {err.Message}");
        }
    }

    // Process Receive Packet
    private void ProcessReceivePacket(Packet packet) {
        try {
            SyncDispatcher.I.Enqueue(() => { syncManager.ApplyPacket(packet); });
            Console.WriteLine($"[SyncThread] Received packet: {packet.DataId}");
        } catch(Exception err) {
            Console.WriteLine($"[SyncThread] Receive error: {err.Message}");
        }
    }
    
    /**
     *
     * Start
     *
     */
    public void Start() {
        running = true;
        thread.Start();
    }

    /**
     *
     * Stop
     *
     */
    public void Stop() {
        running = false;
        
        signal.Set();
        
        int t = 1000;
        thread.Join(t);
    }

    /**
     *
     * Run
     *
     */
    private void Run() {
        while(running) {
            try {
                while(sendQueue.TryDequeue(out var packet)) {
                    ProcessSendPacket(packet);
                }
                while(receiveQueue.TryDequeue(out var packet)) {
                    ProcessReceivePacket(packet);
                }

                int t = 50;
                signal.WaitOne(t);
            } catch(Exception err) {
                Console.WriteLine($"[SyncThread] Error: {err.Message}");
            }
        }
    }
}