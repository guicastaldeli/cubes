namespace App.Root._Sync;
using System.Reflection;

public static class SyncPacketTypes {
    public const string ACTION_FULL_SYNC = "full_sync";
    public const string ACTION_DELTA_SYNC = "delta_sync";
    public const string ACTION_UPDATE = "update";
    public const string ACTION_REQUEST = "request";
    public const string ACTION_RESPONSE = "response";
    public const string ACTION_VALIDATE = "validate";
    public const string ACTION_CONFIRM = "confirm";
    public const string ACTION_REJECT = "reject";
    public const string ACTION_HANDSHAKE = "handshake";
    public const string ACTION_HANDSHAKE_RESPONSE = "handshake_response";
    public const string ACTION_HEARTBEAT = "heartbeat";
    public const string ACTION_HEARTBEAT_RESPONSE = "heartbeat_response";

    private static Dictionary<string, Type> registeredTypes = new();
    private static Dictionary<string, DataSyncAttribute> registeredAttributes = new();

    private static bool isInitialized = false;
    
    // Get Type
    public static Type? GetType(string dataId) {
        Type? val = registeredTypes.TryGetValue(dataId, out var type) ? type : null;
        return val;
    }

    // Get Attribute
    public static DataSyncAttribute? GetAttribute(string dataId) {
        DataSyncAttribute? val = registeredAttributes.TryGetValue(dataId, out var attr) ? attr : null;
        return val;
    }

    // Get All Ids
    public static List<string> GetAllIds() {
        List<string> val = registeredTypes.Keys.ToList();
        return val;
    }

    // Get All Types
    public static List<Type> GetAllTypes() {
        List<Type> val = registeredTypes.Values.ToList();
        return val;
    }

    // Is Registered
    public static bool IsRegistered(string dataId) {
        bool val = registeredTypes.ContainsKey(dataId);
        return val;
    }

    // Get Sync Mode
    public static SyncMode GetSyncMode(string dataId) {
        SyncMode val = registeredAttributes.TryGetValue(dataId, out var attr) ? attr.Mode : SyncMode.BROADCAST;
        return val; 
    }

    // Get Sync Threshold
    public static float GetSyncThreshold(string dataId) {
        float val = registeredAttributes.TryGetValue(dataId, out var attr) ? attr.SyncThreshold : 0.0f;
        return val;
    }

    // Get Conflict Resolution
    public static ConflictResolution GetConflictResolution(string dataId) {
        ConflictResolution val = registeredAttributes.TryGetValue(dataId, out var attr) ? attr.Resolution : ConflictResolution.SERVER_AUTHORITY;
        return val;
    }

    // Get Required Permission
    public static string? GetRequiredPermission(string dataId) {
        string? val = registeredAttributes.TryGetValue(dataId, out var attr) ? attr.RequiredPermission : null;
        return val;
    }

    // Is Valid Data Id
    public static bool IsValidDataId(string dataId) {
        bool val = !string.IsNullOrEmpty(dataId) && registeredTypes.ContainsKey(dataId);
        return val;
    }

    // Is Valid Action
    public static bool IsValidAction(string action) {
        bool val = !string.IsNullOrEmpty(action) && (
            action == ACTION_FULL_SYNC ||
            action == ACTION_DELTA_SYNC ||
            action == ACTION_UPDATE ||
            action == ACTION_REQUEST ||
            action == ACTION_RESPONSE ||
            action == ACTION_VALIDATE ||
            action == ACTION_CONFIRM ||
            action == ACTION_REJECT ||
            action == ACTION_HANDSHAKE ||
            action == ACTION_HANDSHAKE_RESPONSE ||
            action == ACTION_HEARTBEAT ||
            action == ACTION_HEARTBEAT_RESPONSE
        );

        return val;
    }

    /**
     *
     * Create Instance
     *
     */
    public static object? CreateInstance(string dataId) {
        if(registeredTypes.TryGetValue(dataId, out var type)) {
            try {
                object? val = Activator.CreateInstance(type);
                return val;
            } catch (Exception ex) {
                Console.WriteLine($"[SyncPacketTypes] Error creating instance of {dataId}: {ex.Message}");
                return null;
            }
        }

        return null;
    }

    /**
     *
     * Init
     *
     */
    public static void Init() {
        if(isInitialized) return;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach(var assembly in assemblies) {
            try {
                var types = assembly.GetTypes().Where(t => t.GetCustomAttribute<DataSyncAttribute>() != null).ToList();
                foreach(var type in types) {
                    var attr = type.GetCustomAttribute<DataSyncAttribute>()!;
                    string id = attr.Id ?? type.Name.ToLower();

                    if(!registeredAttributes.ContainsKey(id)) {
                        registeredTypes[id] = type;
                        registeredAttributes[id] = attr;
                        Console.WriteLine($"[SyncPacketTypes] Registered sync type: {id} ({type.Name})");
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"[SyncPacketTypes] Error scanning assembly: {ex.Message}");
            }
        }

        isInitialized = true;
        Console.WriteLine($"[SyncPacketTypes] Initialized with {registeredTypes.Count} sync types");
    }

    /**
     *
     * Register Type
     *
     */
    public static void RegisterType<T>(string? customId = null) where T : class {
        var type = typeof(T);
        var attr = type.GetCustomAttribute<DataSyncAttribute>();
        if(attr == null) attr = new DataSyncAttribute();

        string id = customId ?? attr.Id ?? type.Name.ToLower();

        if(!registeredTypes.ContainsKey(id)) {
            registeredTypes[id] = type;
            registeredAttributes[id] = attr;
            Console.WriteLine($"[SyncPacketTypes] Registered sync type dynamically: {id} ({type.Name})");
        }
    } 

    /**
     *
     * Unregister
     *
     */
    public static bool Uregister(string dataId) {
        bool removed = registeredTypes.Remove(dataId);
        registeredAttributes.Remove(dataId);

        return removed;
    }

    /**
     *
     * Clear
     *
     */
    public static void Clear() {
        registeredTypes.Clear();
        registeredAttributes.Clear();

        isInitialized = false;
    }
}