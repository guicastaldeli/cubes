namespace App.Root._Sync;
using System.Collections.Concurrent;
using System.Text.Json;

public class SyncQueue {
    private ConcurrentQueue<PacketSync> queue = new();
    private string queueFile = "sync_queue.dat";

    private object lockObj = new object();

    private bool isOnline = true;

    public SyncQueue() {
        LoadQueue();
    }

    // Has Items
    public bool HasItems() {
        bool val = queue.Count > 0;
        return val;
    }

    // Count
    public int Count() {
        int val = queue.Count;
        return val;
    }

    // Set Online
    public void SetOnline(bool online) {
        if(online && !isOnline) {
            isOnline = true;
            Console.WriteLine($"[SyncQueue] Online - Processing {queue.Count} queued packets");
            ProcessQueue();
        } else if(!online && isOnline) {
            isOnline = false;
            Console.WriteLine("[SyncQueue] Offline - Queuing packets");
        }
    }

    /**
     *
     * Enqueue
     *
     */
    public void Enqueue(PacketSync packet) {
        if(!isOnline) {
            queue.Enqueue(packet);
            SaveQueue();
            Console.WriteLine($"[SyncQueue] Enqueued packet: {packet.DataId} (Queue size: {queue.Count})");
        }
    }

    /**
     *
     * Dequeue All
     *
     */
    public List<PacketSync> DequeueAll() {
        var packets = new List<PacketSync>();
        while(queue.TryDequeue(out var packet)) {
            packets.Add(packet);
        }

        return packets;
    }

    /**
     *
     * Process Queue
     *
     */
    private void ProcessQueue() {
        var packets = DequeueAll();
        foreach(var packet in packets) {
            Console.WriteLine($"[SyncQueue] Processing queued packet: {packet.DataId}");
        }

        SaveQueue();
    }

    /**
     *
     * Save Queue
     *
     */
    private void SaveQueue() {
        lock(lockObj) {
            try {
                var packets = queue.ToList();
                var json = JsonSerializer.Serialize(packets);
                File.WriteAllText(queueFile, json);
            } catch (Exception ex) {
                Console.WriteLine($"[SyncQueue] Error saving queue: {ex.Message}");
            }
        }
    }

    /**
     *
     * Load Queue
     *
     */
    private void LoadQueue() {
        lock(lockObj) {
            try {
                if(File.Exists(queueFile)) {
                    var data = File.ReadAllText(queueFile);
                    
                    var packets = JsonSerializer.Deserialize<List<PacketSync>>(data);
                    if(packets != null) {
                        foreach(var packet in packets) {
                            queue.Enqueue(packet);
                            Console.WriteLine($"[SyncQueue] Loaded {queue.Count} packets from file");
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"[SyncQueue] Error loading queue: {ex.Message}");
            }
        }
    }

    /**
     *
     * Clear
     *
     */
    // Clear
    public void Clear() {
        lock(lockObj) {
            queue.Clear();
            SaveQueue();
        }
    }

    // Clear File
    public void ClearFile() {
        lock(lockObj) {
            if(File.Exists(queueFile)) {
                File.Delete(queueFile);
            }
        }
    }
}