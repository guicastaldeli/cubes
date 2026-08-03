namespace App.Root._Sync;
using App.Root._Crypto;
using App.Root._Binary;
using System.Reflection;

/**

    Conflict Resoltion

    */
public enum ConflictResolution {
    SERVER_AUTHORITY,
    LAST_WRITE_WINS,
    MERGE_WITH_ARBITER,
    LOCK_BASED
}

/**

    Sync Mode

    */
public enum SyncMode {
    BROADCAST,
    PRIVATE,
    PERSISTENT
}

/**

    Sync Ignore Attribute

    */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
public class SyncIgnoreAttribute : Attribute {
    public SyncIgnoreAttribute() {}
}

/**

    Sync Field Attribute

    */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class SyncFieldAttribute : Attribute {
    public string? Key { get; set; }
    public bool Ignore { get; set; } = false;
    public float DeltaThreshold { get; set; } = 0.0f;
    public bool AlwaysSync { get; set; } = false;
    public bool IsReadOnly { get; set; } = false;

    public SyncFieldAttribute() {}
    public SyncFieldAttribute(string Key) {
        this.Key = Key;
    }
}

/**

    Data Sync Attribute

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DataSyncAttribute : Attribute {
    public string? Id { get; set; }
    public SyncMode Mode { get; set; } = SyncMode.BROADCAST;
    public ConflictResolution Resolution { get; set; } = ConflictResolution.SERVER_AUTHORITY;
    public bool EnableDelta { get; set; } = true;
    public bool ValidateAlways { get; set; } = true;
    public string? RequiredPermission { get; set; }
    public float SyncThreshold { get; set; } = 0.0f;

    public DataSyncAttribute() {}
    public DataSyncAttribute(string Id) {
        this.Id = Id;
    } 
}

/**

    Sync Manager main class.

    */
public class SyncManager {
    private static SyncManager? instance;
    public static SyncManager Instance => instance ??= new SyncManager();

    private string sessionId = "";

    private Dictionary<string, object> syncRegistry = new();
    private Dictionary<string, DateTime> lastSyncTime = new();
    private Dictionary<string, byte[]> lastState = new();
    private Dictionary<string, object> usedFlags = new();

    private SyncQueue queue;
    private SyncThread syncThread;

    private CryptoProvider? crypto;

    private event Action<PacketSync>? OnPacketReceived;
    private event Action<string, object>? OnDataSynced;

    private bool isRunning = false;

    public SyncManager() {
        queue = new SyncQueue();
        syncThread = new SyncThread(this);

        PacketSyncTypes.Init();
    }

    // Get Crypto
    public CryptoProvider? GetCrypto() {
        CryptoProvider? val = crypto;
        return val;
    }

    // Get Queue
    public SyncQueue GetQueue() {
        SyncQueue val = queue;
        return val;
    }

    // Get Session Id
    public string GetSessionId() {
        string val = sessionId;
        return val;
    }

    // Is Running
    public bool IsRunning() {
        bool val = isRunning;
        return val;
    }

    // Is Locked
    private bool IsLocked(object data) {
        string dataId = data.GetType().Name.ToLower();

        var attr = data.GetType().GetCustomAttribute<DataSyncAttribute>();
        if(attr != null && !string.IsNullOrEmpty(attr.Id)) dataId = attr.Id;

        return LockSync.Instance.IsLocked(dataId);
    }

    // Compare Byte Arrays
    private bool CompareByteArrays(byte[] a, byte[] b) {
        if(a.Length != b.Length) return false;
        for(int i = 0; i < a.Length; i++) if(a[i] != b[i]) return false;

        return true;
    }

    // Resolve Conflict
    private object ResolveConflict(object existing, object incoming, ConflictResolution resolution, long timestamp) {
        switch(resolution) {
            case ConflictResolution.SERVER_AUTHORITY:
                return existing;
            case ConflictResolution.LAST_WRITE_WINS:
                if(timestamp > lastSyncTime.GetValueOrDefault(existing.GetType().Name.ToLower()).Ticks) {
                    return incoming;
                }

                return existing;
            case ConflictResolution.MERGE_WITH_ARBITER:
                return MergeData(existing, incoming);
            case ConflictResolution.LOCK_BASED:
                if(IsLocked(existing)) return existing;
                return incoming;
            default:
                return existing;
        }
    }

    // Merge Data
    private object MergeData(object existing, object incoming) {
        var type = existing.GetType();
        var merged = existing;

        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            if(prop.CanWrite) {
                var existingValue = prop.GetValue(existing);
                var incomingValue = prop.GetValue(incoming);

                if(!Equals(existingValue, incomingValue)) prop.SetValue(merged, incomingValue);
            }
        }

        return merged;
    }

    // Create And Send Packet
    private void CreateAndSendPacket(string dataId, object data, bool isFull) {
        using var writer = new BinaryWriter();
        writer.WriteObject(data);

        var payload = writer.GetBytes();
        if(crypto != null) payload = crypto.Encrypt(payload);

        var packet = new PacketSync {
            DataId = dataId,
            Action = isFull ? PacketSyncTypes.ACTION_FULL_SYNC : PacketSyncTypes.ACTION_DELTA_SYNC,
            Payload = payload,
            Timestamp = DateTime.UtcNow.Ticks,
            IsDelta = !isFull,
            SessionId = sessionId,
            Checksum = CryptoProvider.ComputeHash(payload)
        };

        syncThread.EnqueuePacket(packet);
        OnPacketReceived?.Invoke(packet);
    }

    // Trigger Sync
    public void TriggerSync(string dataId) {
        if(!isRunning) return;
        if(!PacketSyncTypes.IsRegistered(dataId)) return;

        if(syncRegistry.TryGetValue(dataId, out var data)) {
            using var writer = new BinaryWriter();
            writer.WriteObject(data);

            var currentState = writer.GetBytes();

            if(lastState.TryGetValue(dataId, out var prevState)) {
                if(!CompareByteArrays(prevState, currentState)) {
                    var attr = PacketSyncTypes.GetAttribute(dataId);
                    bool isDelta = attr?.EnableDelta ?? true;

                    CreateAndSendPacket(dataId, data, !isDelta);

                    lastState[dataId] = currentState;
                    lastSyncTime[dataId] = DateTime.UtcNow;
                    usedFlags[dataId] = false;
                }
            } else {
                CreateAndSendPacket(dataId, data, true);

                lastState[dataId] = currentState;
                lastSyncTime[dataId] = DateTime.UtcNow;
                usedFlags[dataId] = false;
            }
        }
    }

    // Full Sync
    public void FullSync() {
        foreach(var (id, data) in syncRegistry) {
            using var writer = new BinaryWriter();
            writer.WriteObject(data);

            lastState[id] = writer.GetBytes();

            CreateAndSendPacket(id, data, true);
        }
    }

    // Apply Packet
    public void ApplyPacket(PacketSync packet) {
        if(!isRunning) return;

        if(!packet.IsValid()) {
            Console.WriteLine($"[SyncManager] Invalid packet: {packet.DataId}");
            return;
        }

        if(!PacketSyncTypes.IsRegistered(packet.DataId)) {
            Console.WriteLine($"[SyncManager] Unknown data ID: {packet.DataId}");
            return;
        }

        if(crypto != null && packet.Payload.Length > 0) {
            packet.Payload = crypto.Decrypt(packet.Payload);
        }

        if(!syncRegistry.TryGetValue(packet.DataId, out var existing)) {
            Console.WriteLine($"[SyncManager] Data not in registry: {packet.DataId}");
            return;
        }

        using var reader = new BinaryReader(packet.Payload);
        
        var newData = reader.ReadObject();
        if(newData == null) return;

        var resolution = PacketSyncTypes.GetConflictResolution(packet.DataId);
        var resolved = ResolveConflict(existing, newData, resolution, packet.Timestamp);
        if(resolved != null && resolved != existing) {
            var type = existing.GetType();
            foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if(prop.CanWrite) {
                    var value = prop.GetValue(resolved);
                    prop.SetValue(existing, value);
                }
            }
        }

        using var writer = new BinaryWriter();
        writer.WriteObject(existing);

        lastState[packet.DataId] = writer.GetBytes();
        lastSyncTime[packet.DataId] = DateTime.UtcNow;

        OnDataSynced?.Invoke(packet.DataId, existing);
    }

    /**
     *
     * Register Sync
     *
     */
    public void RegisterSync<T>(T data, string? customId = null) where T : class {
        var type = data.GetType();
        var attr = type.GetCustomAttribute<DataSyncAttribute>();

        string id = customId ?? attr?.Id ?? type.Name.ToLower();
        if(!PacketSyncTypes.IsRegistered(id)) PacketSyncTypes.RegisterType<T>(id);

        syncRegistry[id] = data;
        lastSyncTime[id] = DateTime.UtcNow;
        usedFlags[id] = false;

        using var writer = new BinaryWriter();
        writer.WriteObject(data);
        lastState[id] = writer.GetBytes();

        Console.WriteLine($"[SyncManager] Registered {id} for syncing");
    }

    public void RegisterSync(string id, object data) {
        var type = data.GetType();
        var attr = type.GetCustomAttribute<DataSyncAttribute>();

        if(!PacketSyncTypes.IsRegistered(id)) PacketSyncTypes.RegisterType(type, id);
        syncRegistry[id] = data;
        lastSyncTime[id] = DateTime.UtcNow;
        usedFlags[id] = false;

        using var writer = new BinaryWriter();
        writer.WriteObject(data);
        lastState[id] = writer.GetBytes();

        Console.WriteLine($"[SyncManager] Registered {id} for syncing");
    }

    /**
     *
     * Start
     *
     */
    public void Start(string? sessionId = null) {
        if(isRunning) return;

        this.sessionId = sessionId ?? KeyExchange.GenerateSessionId();

        var (key, iv) = CryptoProvider.GenerateSessionKey();
        crypto = new CryptoProvider(key, iv);

        syncThread.Start();

        isRunning = true;

        Console.WriteLine($"[SyncManager] Started with session: {this.sessionId}");
    }

    /**
     *
     * Stop
     *
     */
    public void Stop() {
        isRunning = false;
        syncThread.Stop();

        Console.WriteLine("[SyncManager] Stopped");
    }
}