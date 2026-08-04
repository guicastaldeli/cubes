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
    public bool Sync { get; set; } = true;

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
    public static SyncManager I => instance ??= new SyncManager();

    private string sessionId = "";

    private Dictionary<string, object> syncRegistry = new();
    private Dictionary<string, DateTime> lastSyncTime = new();
    private Dictionary<string, byte[]> lastState = new();
    private Dictionary<string, object> usedFlags = new();

    private SyncQueue queue;
    private SyncThread syncThread;

    private CryptoProvider? crypto;

    public event Action<Packet>? OnPacketReceived;
    public event Action<string, object>? OnDataSynced;

    private bool isRunning = false;

    public SyncManager() {
        queue = new SyncQueue();
        syncThread = new SyncThread(this);

        PacketTypes.Init();

        Data.OnDataChanged += OnDataChanged;
        Data.OnDataRegistered += OnDataRegistered;
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

        return LockSync.I.IsLocked(dataId);
    }

    // Compare Byte Arrays
    private bool CompareByteArrays(byte[] a, byte[] b) {
        if(a.Length != b.Length) return false;
        for(int i = 0; i < a.Length; i++) if(a[i] != b[i]) return false;

        return true;
    }

    // Apply Dictionary To Object
    private void ApplyDictionaryToObject(object target, Dictionary<string, object> data) {
        var type = target.GetType();

        foreach(var d in data) {
            var prop = type.GetProperty(d.Key);
            if(prop != null && prop.CanWrite) {
                try {
                    var value = Convert.ChangeType(d.Value, prop.PropertyType);
                    prop.SetValue(target, value);
                } catch {
                    prop.SetValue(target, d.Value);
                }

                continue;
            }

            var field = type.GetField(d.Key);
            if(field != null) {
                try {
                    var value = Convert.ChangeType(d.Value, field.FieldType);
                    field.SetValue(target, value);
                } catch {
                    field.SetValue(target, d.Value);
                }
            }
        }
    }

    // Resolve Conflict
    private object ResolveConflict(object existing, object incoming, ConflictResolution resolution, long timestamp) {
        switch(resolution) {
            case ConflictResolution.SERVER_AUTHORITY:
                return existing;
            case ConflictResolution.LAST_WRITE_WINS:
                if(timestamp > lastSyncTime.GetValueOrDefault(existing.GetType().Name.ToLower()).Ticks) {
                    if(incoming is Dictionary<string, object> d1) ApplyDictionaryToObject(existing, d1);
                    return existing;
                }

                return existing;
            case ConflictResolution.MERGE_WITH_ARBITER:
                if(incoming is Dictionary<string, object> d2) {
                    return MergeData(existing, d2);
                }

                return existing;
            case ConflictResolution.LOCK_BASED:
                if(IsLocked(existing)) return existing;
                return incoming;
            default:
                return existing;
        }
    }

    // Merge Data
    private object MergeData(object existing, Dictionary<string, object> incoming) {
        var type = existing.GetType();
        
        var merged = Activator.CreateInstance(type);
        if(merged == null) return existing;

        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            if(prop.CanWrite) {
                var existingValue = prop.GetValue(existing);
                prop.SetValue(merged, existingValue);
            }
        }

        ApplyDictionaryToObject(merged, incoming);
        return merged;
    }

    // Create And Send Packet
    private void CreateAndSendPacket(string dataId, Dictionary<string, object> data, bool isFull) {
        using var writer = new BinaryWriter();
        writer.WriteObject(data);

        var payload = writer.GetBytes();
        if(crypto != null) payload = crypto.Encrypt(payload);

        var packet = new Packet {
            DataId = dataId,
            Action = isFull ? PacketTypes.ACTION_FULL_SYNC : PacketTypes.ACTION_DELTA_SYNC,
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
        if(!PacketTypes.IsRegistered(dataId)) return;

        var data = Data.GetData(dataId);
        if (data == null) return;

        var attr = data.GetType().GetCustomAttribute<DataSyncAttribute>();
        if(attr == null || !attr.Sync) return;

        var serialized = Data.SerializeStoreData(data);
        if(serialized == null) return;

        using var writer = new BinaryWriter();
        writer.WriteObject(serialized);

        var currentState = writer.GetBytes();
        if(lastState.TryGetValue(dataId, out var prevState)) {
            if(CompareByteArrays(prevState, currentState)) {
                return;
            }
        }

        CreateAndSendPacket(dataId, serialized, !attr.EnableDelta);

        lastState[dataId] = currentState;
        lastSyncTime[dataId] = DateTime.UtcNow;
    }

    // Full Sync
    public void FullSync() {
        foreach(var id in Data.GetAllDataIds()) {
            var data = Data.GetData(id);
            if(data == null) continue;

            var attr = data.GetType().GetCustomAttribute<DataSyncAttribute>();
            if(attr == null || !attr.Sync) continue;

            var serialized = Data.SerializeStoreData(data);
            if(serialized == null) continue;

            CreateAndSendPacket(id, serialized, true);

            using var writer = new BinaryWriter();
            writer.WriteObject(serialized);

            lastState[id] = writer.GetBytes();
            lastSyncTime[id] = DateTime.UtcNow;
        }
    }

    // Apply Packet
    public void ApplyPacket(Packet packet) {
        if(!isRunning) return;

        if(!packet.IsValid()) {
            Console.WriteLine($"[SyncManager] Invalid packet: {packet.DataId}");
            return;
        }

        if(!PacketTypes.IsRegistered(packet.DataId)) {
            Console.WriteLine($"[SyncManager] Unknown data ID: {packet.DataId}");
            return;
        }

        if(crypto != null && packet.Payload.Length > 0) {
            packet.Payload = crypto.Decrypt(packet.Payload);
        }

        using var reader = new BinaryReader(packet.Payload);
        
        var data = reader.ReadObject<Dictionary<string, object>>();
        if(data == null) return;
    
        var existing = Data.GetData(packet.DataId);
        if(existing == null) {
            var type = PacketTypes.GetType(packet.DataId);
            if(type != null) {
                existing = Activator.CreateInstance(type);
                Data.RegisterData(packet.DataId, existing!);
            }
        }

        var resolution = PacketTypes.GetConflictResolution(packet.DataId);
        var resolved = ResolveConflict(existing!, data, resolution, packet.Timestamp);
        if(existing != null && resolved != null && resolved != existing) {
            var type = existing.GetType();
            foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if(prop.CanWrite) {
                    var value = prop.GetValue(resolved);
                    prop.SetValue(existing, value);
                }
            }
        }

        using var writer = new BinaryWriter();
        writer.WriteObject(data);
        lastState[packet.DataId] = writer.GetBytes();
        lastSyncTime[packet.DataId] = DateTime.UtcNow;

        OnDataSynced?.Invoke(packet.DataId, existing!);
    }

    // On Data Registered
    private void OnDataRegistered(string id, object data) {
        if(!isRunning) return;

        if(!lastState.ContainsKey(id)) {
            using var writer = new BinaryWriter();
            writer.WriteObject(Data.SerializeStoreData(data));

            lastState[id] = writer.GetBytes();
            lastSyncTime[id] = DateTime.UtcNow;
        }
    }

    // On Data Changed
    private void OnDataChanged(string id, object data) {
        if(!isRunning) return;

        var attr = data.GetType().GetCustomAttribute<DataSyncAttribute>();
        if(attr != null && attr.Sync) TriggerSync(id);
    }

    /**
     *
     * Register Sync
     *
     */
    public void RegisterSync<T>(T data) where T : class {
        Data.RegisterStoreData(data);

        string id = Data.GetId(data);
        if(!lastState.ContainsKey(id)) {
            using var writer = new BinaryWriter();
            writer.WriteObject(Data.SerializeStoreData(data));

            lastState[id] = writer.GetBytes();
            lastSyncTime[id] = DateTime.UtcNow;
        }

        Console.WriteLine($"[SyncManager] Registered {id} for sync");
    }

    public void RegisterSync(string id, object data) {
        Data.RegisterData(id, data);

        if(!lastState.ContainsKey(id)) {
            using var writer = new BinaryWriter();
            writer.WriteObject(Data.SerializeStoreData(data));

            lastState[id] = writer.GetBytes();
            lastSyncTime[id] = DateTime.UtcNow;
        }

        Console.WriteLine($"[SyncManager] Registered {id} for sync");
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