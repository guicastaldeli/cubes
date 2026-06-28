namespace App.Root;
using System.Reflection;
using NLua;

/**

    Data Input Attribute

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DataInputAttribute : Attribute {
    public string Id { get; }

    public DataInputAttribute() {
        Id = null;
    }

    internal static string GenerateId(Type type) {
        string val = type.Name.ToLower();
        return val;
    }
}

/**

    Data Input main class

    */
public static class DataInput {
    private static Dictionary<string, object> dataCache = new();
    private static Dictionary<string, Type> registeredTypes = new();
    private static Dictionary<string, Func<object>> extractors = new();
    
    private static bool initialized = false;

    /**
     *
     * Register Type
     *
     */
    private static void RegisterType(Type type, string id) {
        var method = type.GetMethod("ExtractData", BindingFlags.Public | BindingFlags.Static);

        if(method != null && method.ReturnType == typeof(object)) {
            extractors[id] = () => method.Invoke(null, null);
        } else {
            var instanceMethod = type.GetMethod("ExtractData", BindingFlags.Public | BindingFlags.Instance);
            if(instanceMethod != null && instanceMethod.ReturnType == typeof(object)) {
                extractors[id] = () => {
                    var instance = Activator.CreateInstance(type);
                    return instanceMethod.Invoke(instance, null);
                };
            } else {
                var prop = type.GetProperty("Data", BindingFlags.Public | BindingFlags.Static);
                if(prop != null) {
                    extractors[id] = () => prop.GetValue(null);
                } else {
                    var field = type.GetField("Data", BindingFlags.Public | BindingFlags.Static);
                    if(field != null) extractors[id] = () => field.GetValue(null);
                }
            }
        }

        registeredTypes[id] = type;
        Console.WriteLine($"[DataInput] Registered {type.Name} with ID: {id}");
    }

    /**
     *
     * Load
     *
     */
    // Load
    public static void Load(string id) {
        if(extractors.TryGetValue(id, out var extractor)) {
            try {
                var data = extractor();
                Data.RegisterData(id, data);
            } catch(Exception err) {
                throw new Exception($"DataInput -- Load -- Error: {err}");
            }
        }
    }

    // Load All
    public static void LoadAll() {
        foreach(var e in extractors) {
            try {
                var data = e.Value();
                Data.RegisterData(e.Key, data);
            } catch(Exception err) {
                throw new Exception($"DataInput -- LoadAll -- Error: {err}");
            }
        }
    }

    /**
     *
     * Get
     *
     */
    // Get Data
    public static T GetData<T>(string id) {
        T val = Data.GetData<T>(id);
        return val;
    }

    public static object GetData(string id) {
        object val = Data.GetData(id);
        return val;
    }

    // Get Registered Ids
    public static List<string> GetRegisteredIds() {
        List<string> val = extractors.Keys.ToList();
        return val;
    }

    /**
     *
     * Reload
     *
     */
    // Reload
    public static void Reload(string id) {
        if(extractors.TryGetValue(id, out var extractor)) {
            try {
                var data = extractor();
                Data.UpdateData(id, data);
            } catch(Exception err) {
                throw new Exception($"DataInput -- Reload -- Error: {err}");
            }
        }
    }

    // Reload All
    public static void ReloadAll() {
        foreach(var e in extractors) {
            Reload(e.Key);
        }
    }

    /**
     *
     * Init
     *
     */
    public static void Init() {
        if(initialized) return;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach(var assembly in assemblies) {
            try {
                var types = assembly.GetTypes();
                foreach(var type in types) {
                    var attr = type.GetCustomAttribute<DataInputAttribute>();
                    if(attr != null) {
                        string id = DataInputAttribute.GenerateId(type);
                        RegisterTypes(type, id);
                    }
                }
            } catch(Exception err) {
                throw new Exception($"Data Input -- Init -- Error {err}");
            }
        }

        initialized = true;
    }
}