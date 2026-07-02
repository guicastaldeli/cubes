using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace App.Root;

/**
    
    Data Type

    */
enum DataType {
    MESH,
    PLAYER,
    WORLD
}

/**
    
    Data Entry interface

    */
interface DataEntry {
    string? getId() => null;
    Dictionary<string, object> serialize();
}

/**
    
    Data Handler

    */
[AttributeUsage(AttributeTargets.Method)]
public class DataInjectionAttribute : Attribute {}

public abstract class DataHandler {
    [DataInjection] public virtual object? ExtractData() => Route();
    [DataInjection] public virtual void SaveData(object data) => Route(data);
    [DataInjection] public virtual bool HasData() => Route<bool>();

    protected virtual object? Route([CallerMemberName] string? name = null) => null;
    protected virtual void Route(object data, [CallerMemberName] string? name = null) {}
    protected virtual T? Route<T>([CallerMemberName] string? name = null) => default;
}

/**
    
    This Data class

    */
public static class ThisData {
    /**
     *
     * Get Id
     *
     */
    public static string GetId<T>() where T : class {
        string id = typeof(T).Name.ToLower();
        return id;
    }

    public static string GetId(Type type) {
        string val = type.Name.ToLower();
        return val;
    }

    public static string GetId(object obj) {
        if(obj == null) throw new ArgumentNullException(nameof(obj));

        string val = obj.GetType().Name.ToLower();
        return val;
    }

    /**
     *
     * Get Data
     *
     */
    public static T? GetData<T>() where T : class {
        string id = GetId<T>();
        T? val = Data.GetData<T>(id);
        return val;
    }

    public static object? GetData(Type type) {
        string id = GetId(type);
        object? val = Data.GetData(id);
        return val;
    }

    public static (string? id, T? data) EGetData<T>() where T : class {
        var ids = Data.GetAllDataIds();
        
        foreach(var id in ids) {
            var d = Data.GetData(id);
            if(d is T td) return (id, td);
        
        }
        return (null, null);
    }

    public static List<T>? LGetData<T>() where T : class {
        string id = typeof(T).Name.ToLower();
        
        List<T>? val = Data.GetData<List<T>>(id);
        return val;
    }
}

/**
    
    Main Data class

    */
[ManagedState]
static class Data {
    private static Dictionary<DataType, List<DataEntry>> entries = new();
    private static ConcurrentDictionary<string, object> dataStore = new();
    private static ConcurrentDictionary<string, Type> dataTypes = new();
    private static ConcurrentDictionary<string, DateTime> dataTimestamps = new();

    private static bool initialized = false;

    static Data() {
        StateManager.SRegister(typeof(Data));
        Init();
    }

    // Register
    public static void Register(DataType type, DataEntry entry) {
        if(!entries.ContainsKey(type)) entries[type] = new();
        entries[type].Add(entry);
    }

    // Unregister
    public static void Unregister(DataType type, DataEntry entry) {
        if(entries.TryGetValue(type, out var list)) list.Remove(entry);
    }

    // Get
    public static List<DataEntry> Get(DataType type) {
        List<DataEntry> val = entries.TryGetValue(type, out var list) ? list : new();
        return val;
    }

    // Snapshot
    public static DataSnapshot Snapshot() {
        DataSnapshot val = new DataSnapshot(entries);
        return val;
    }

    // Apply Snapshot
    public static void ApplySnapshot(DataSnapshot snapshot, DataType type, Action<Dictionary<string, object>> handler) {
        foreach(var entry in snapshot.get(type)) {
            handler(entry);
        }
    }

    // Clear
    public static void Clear(DataType type) {
        if(entries.ContainsKey(type)) entries[type].Clear();
    }

    // Clear All
    public static void ClearAll() {
        entries.Clear();
    }

    /**
        ***
        
        ***
                */
    
    /**
     * 
     * Register
     *
     */
    // Register Data
    public static void RegisterData(string id, object data) {
        if(string.IsNullOrEmpty(id)) throw new ArgumentException("Data ID cannot be null or empty!!");

        dataStore[id] = data;
        dataTypes[id] = data.GetType();
        dataTimestamps[id] = DateTime.Now;

        Console.WriteLine($"[Data] Registered data: {id} ({dataTypes[id]?.Name ?? "null"})");
    }

    /**
     * 
     * Get
     *
     */
    // Get Data
    public static object? GetData(string id) {
        if(dataStore.TryGetValue(id, out var data)) return data;
        return null;
    }

    public static T? GetData<T>(string id) {
        if(dataStore.TryGetValue(id, out var data)) {
            if(data is T typedData) {
                return typedData;
            }

            try {
                if(data != null && typeof(T).IsAssignableFrom(data.GetType())) {
                    return (T)data;
                }
            } catch(Exception err) {
                throw new Exception($"Data -- Get Data -- Error -- {err.Message}");
            }
        }

        return default;
    }

    // Get All Data Ids
    public static List<string> GetAllDataIds() {
        List<string> val = dataStore.Keys.ToList();
        return val;
    }

    // Get Data Timestamp
    public static DateTime? GetDataTimestamp(string id) {
        if(dataTimestamps.TryGetValue(id, out var timestamp)) {
            return timestamp;
        }
        return null;
    }

    /**
     * 
     * Has Data
     *
     */
    public static bool HasData(string id) {
        bool val = dataStore.ContainsKey(id);
        return val;
    }

    /**
     * 
     * Save
     *
     */
    // Save Data
    public static void SaveData(string id) {
        DataOutput.Save(id);
    }

    // Save All Data
    public static void SaveAllData() {
        DataOutput.SaveAll();
    }

    /**
     * 
     * Load
     *
     */
    // Load Saved Data
    public static void LoadSavedData(string id) {
        DataOutput.Load(id);
    }

    // Load All Saved Data
    private static void LoadAllSavedData() {
        var ids = DataOutput.GetRegisteredIds();
        foreach(var id in ids) {
            try {
                if(DataOutput.HasSavedData(id)) {
                    DataOutput.Load(id);
                    Console.WriteLine($"[Data] Loaded saved data for: {id}");
                }
            } catch(Exception err) {
                Console.WriteLine($"[Data] Error loading saved data for {id}: {err.Message}");
            }
        }
    }
    
    /**
     * 
     * Reload
     *
     */
    public static void ReloadData(string id) {
        DataInput.Reload(id);
    }

    /**
     * 
     * Init
     *
     */
    private static void Init() {
        if(initialized) return;

        DataInput.Init();
        DataOutput.Init();

        DataInput.LoadAll();

        LoadAllSavedData();

        initialized = true;
        Console.WriteLine("[Data] Data system initialized");
    }

    /**
     * 
     * Update
     *
     */
    public static void UpdateData(string id, object data) {
        if(dataStore.ContainsKey(id)) {
            dataStore[id] = data;
            dataTypes[id] = data.GetType();
            dataTimestamps[id] = DateTime.Now;

            Console.WriteLine($"[Data] Updated data: {id}");
        } else {
            RegisterData(id, data);
        }
    }

    /**
     * 
     * Clear
     *
     */
    // Clear All Data
    public static void ClearAllData() {
        dataStore.Clear();
        dataTypes.Clear();
        dataTimestamps.Clear();
    }

    /**
     * 
     * Remove Data
     *
     */
    public static bool RemoveData(string id) {
        bool removed = dataStore.TryRemove(id, out _);
        dataTypes.TryRemove(id, out _);
        dataTimestamps.TryRemove(id, out _);
        return removed;
    }
}