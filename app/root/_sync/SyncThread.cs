namespace App.Root._Sync;
using System.Collections.Concurrent;

public class SyncThread {
    private SyncManager syncManager;

    private Thread thread;
    private ConcurrentQueue<PacketSync> sendQueue = new();
    private ConcurrentQueue<PacketSync> receiveQueue = new();

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
    public void EnqueuePacket(PacketSync packet) {
        sendQueue.Enqueue(packet);
        signal.Set();
    }

    // Enqueue Received Packet
    public void EnqueueReceivedPacket(PacketSync packet) {
        receiveQueue.Enqueue(packet);
        signal.Set();
    }

    /**
     *
     * Process
     *
     */
    // Process Send Packet
    private void ProcessSendPacket(PacketSync packet) {
        try {
            var bytes = packet.ToBytes();
            Console.WriteLine($"[SyncThread] Sent packet: {packet.DataId} ({bytes.Length} bytes)");
        } catch (Exception ex) {
            Console.WriteLine($"[SyncThread] Send error: {ex.Message}");
        }
    }

    // Process Receive Packet
    private void ProcessReceivePacket(PacketSync packet) {
        try {
            SyncDispatcher.I.Enqueue(() => { syncManager.ApplyPacket(packet); });
            Console.WriteLine($"[SyncThread] Received packet: {packet.DataId}");
        } catch (Exception ex) {
            Console.WriteLine($"[SyncThread] Receive error: {ex.Message}");
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
            } catch (Exception ex) {
                Console.WriteLine($"[SyncThread] Error: {ex.Message}");
            }
        }
    }
}