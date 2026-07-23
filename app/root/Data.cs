namespace App.Root;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class StoreDataAttribute : Attribute {
    public string? Id { get; set; }
    public string? Section { get; set; }

    public StoreDataAttribute() {}
    public StoreDataAttribute(string Id) {
        this.Id = Id;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class StoreFieldAttribute : Attribute {
    public string? Key { get; set; }
    public bool Ignore { get; set; } = false;

    public StoreFieldAttribute() {}
    public StoreFieldAttribute(string Key) {
        this.Key = Key;
    }
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

    /**
     *
     * Get Data Type
     *
     */
    public static object? GetDataType(Type type) {
        var ids = Data.GetAllDataIds();

        foreach(var id in ids) {
            var data = Data.GetData(id);
            if(data == null) continue;

            var dataTypeActual = data.GetType();
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                if(dataTypeActual.IsGenericType && dataTypeActual.GetGenericTypeDefinition() == typeof(List<>)) {
                    var requestedElementType = type.GetGenericArguments()[0];
                    var actualElementType = dataTypeActual.GetGenericArguments()[0];
                    if(requestedElementType == actualElementType) return data;
                }
            }
            if(dataTypeActual == type || type.IsAssignableFrom(dataTypeActual)) {
                return data;
            }
        }

        return null;
    }

    /**
     *
     * Find
     *
     */
    public static string? FindId(object data) {
        var ids = Data.GetAllDataIds();
        foreach(var id in ids) {
            var d = Data.GetData(id);
            if(d == data) return id;
        }

        return null;
    }
}

/**
    
    Main Data class

    */
[ManagedState]
static class Data {
    public class StoreFieldInfo {
        public string Key { get; set; } = "";
        public PropertyInfo? Property { get; set; }
        public FieldInfo? Field { get; set; }
        public Type FieldType { get; set; } = null!;
        public bool Ignore { get; set; }
        public bool IsStatic { get; set; }
    }

    private static ConcurrentDictionary<string, List<StoreFieldInfo>> storeFieldCache = new();

    private static ConcurrentDictionary<string, object> dataStore = new();
    private static ConcurrentDictionary<string, Type> dataTypes = new();
    private static ConcurrentDictionary<string, DateTime> dataTimestamps = new();

    private static bool initialized = false;
    private static bool storeDataInitialized = false;

    static Data() {
        StateManager.SRegister(typeof(Data));
        Init();
    }

    // Get All Store Data Ids
    public static List<string> GetAllStoreDataIds() {
        List<string> val = storeFieldCache.Keys.ToList();
        return val;
    }

    // Get Store Fields
    public static List<StoreFieldInfo>? GetStoreFields(string id) {
        if(storeFieldCache.TryGetValue(id, out var fields)) {
            return fields;
        }

        return null;
    }

    public static List<StoreFieldInfo>? GetStoreFields(Type type) {
        var attr = type.GetCustomAttribute<StoreDataAttribute>();
        if(attr == null) return null;

        List<StoreFieldInfo>? val = GetStoreFields(attr.Id ?? type.Name.ToLower());
        return val;
    }

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

    // Has Data
    public static bool HasData(string id) {
        bool val = dataStore.ContainsKey(id);
        return val;
    }
    
    /**
     *
     * Serialize Store Data
     *
     */
    public static Dictionary<string, object>? SerializeStoreData(object obj) {
        if(obj == null) return null;

        var result = new Dictionary<string, object>();

        var type = obj.GetType();
        var attr = type.GetCustomAttribute<StoreDataAttribute>();
        if(attr == null) {
            Console.WriteLine($"[Data] No StoreData attribute on {type.Name}");
            return null;
        }

        string id = attr.Id ?? type.Name.ToLower();
        var fields = GetStoreFields(id);
        if(fields == null) {
            CacheStoreFields(type);
            fields = GetStoreFields(id);
            if(fields == null) return null;
        }
        foreach(var field in fields) {
            try {
                object? value = null;
                if(field.Property != null) {
                    value = field.Property.GetValue(obj);
                } else if(field.Field != null) {
                    value = field.Field.GetValue(obj);
                }

                if(value != null) result[field.Key] = value;
            } catch(Exception ex) {
                Console.WriteLine($"[Data] Error serializing {field.Key}: {ex.Message}");
            }
        }

        return result;
    }

    /**
     *
     * Deserialize Store Data
     *
     */
    public static void DeserializeStoreData(object obj, Dictionary<string, object> data) {
        if(obj == null || data == null) return;

        var type = obj.GetType();
        var attr = type.GetCustomAttribute<StoreDataAttribute>();
        if(attr == null) return;

        string id = attr.Id ?? type.Name.ToLower();
        var fields = GetStoreFields(id);
        if(fields == null) {
            CacheStoreFields(type);
            fields = GetStoreFields(id);
            if(fields == null) return;
        }
        foreach(var field in fields) {
            if(!data.TryGetValue(field.Key, out var value)) continue;
            if(value == null) continue;

            try {
                var converted = Convert.ChangeType(value, field.FieldType);

                if(field.Property != null) {
                    field.Property.SetValue(obj, converted);
                } else if(field.Field != null) {
                    field.Field.SetValue(obj, converted);
                }
            } catch(Exception err) {
                Console.WriteLine($"[Data] Error deserializing {field.Key}: {err.Message}");
            }
        }
    }

    /**
     *
     * Register
     *
     */
    // Register Store Data
    public static void RegisterStoreData(object obj) {
        if(obj == null) return;

        var type = obj.GetType();
        var attr = type.GetCustomAttribute<StoreDataAttribute>();
        if(attr == null) return;

        string id = attr.Id ?? type.Name.ToLower();
        if(!storeFieldCache.ContainsKey(id)) CacheStoreFields(type);

        RegisterData(id, obj);
        Console.WriteLine($"[Data] Registered StoreData: {type.Name} (ID: {id})");
    }

    // Register Data
    public static void RegisterData(string id, object data) {
        if(string.IsNullOrEmpty(id)) throw new ArgumentException("Data ID cannot be null or empty!!");
        
        dataStore[id] = data;
        dataTypes[id] = data.GetType();
        dataTimestamps[id] = DateTime.Now;

        Console.WriteLine($"[Data] Registered data: {id} ({dataTypes[id]?.Name ?? "null"})");
    }

    private static void CacheStoreFields(Type type) {
        var attr = type.GetCustomAttribute<StoreDataAttribute>();
        if(attr == null) return;

        string id = attr.Id ?? type.Name.ToLower();
        var fields = new List<StoreFieldInfo>();
        
        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
            var fieldAttr = prop.GetCustomAttribute<StoreFieldAttribute>();
            if(fieldAttr != null && !fieldAttr.Ignore) {
                fields.Add(new StoreFieldInfo {
                    Key = fieldAttr.Key ?? prop.Name,
                    Property = prop,
                    Field = null,
                    FieldType = prop.PropertyType,
                    Ignore = fieldAttr.Ignore,
                    IsStatic = prop.GetMethod?.IsStatic ?? false
                });
            }
        }
        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
            var fieldAttr = field.GetCustomAttribute<StoreFieldAttribute>();
            if(fieldAttr != null && !fieldAttr.Ignore) {
                fields.Add(new StoreFieldInfo {
                    Key = fieldAttr.Key ?? field.Name,
                    Property = null,
                    Field = field,
                    FieldType = field.FieldType,
                    Ignore = fieldAttr.Ignore,
                    IsStatic = field.IsStatic
                });
            }
        }

        storeFieldCache[id] = fields;
        Console.WriteLine($"[Data] Cached {fields.Count} StoreFields for {type.Name} (ID: {id})");
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
     * Update
     *
     */
    public static void Update(string id, object data) {
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
     * Init
     *
     */
    // Init Store Data
    private static void InitStoreData() {
        if(storeDataInitialized) return;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach(var assembly in assemblies) {
            try {
                var types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<StoreDataAttribute>() != null &&
                        !t.IsAbstract &&
                        !t.IsInterface);
                foreach(var type in types) {
                    CacheStoreFields(type);
                }
            } catch(Exception err) {
                Console.WriteLine($"[Data] Error scanning StoreData: {err.Message}");
            }
        }
    }

    // Init
    public static void Init() {
        if(initialized) return;

        DataInput.Init();
        DataOutput.Init();

        DataInput.LoadAll();
        LoadAllSavedData();

        InitStoreData();

        initialized = true;
        Console.WriteLine("[Data] Data system initialized");
    }

    /**
     *
     * Clear
     *
     */
    public static void Clear() {
        dataStore.Clear();
        dataTypes.Clear();
        dataTimestamps.Clear();
    }

    /**
     *
     * Remove
     *
     */
    public static bool Remove(string id) {
        bool removed = dataStore.TryRemove(id, out _);

        dataTypes.TryRemove(id, out _);
        dataTimestamps.TryRemove(id, out _);

        return removed;
    }
}