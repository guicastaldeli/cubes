namespace App.Root;
using System.Reflection;

/**

    Data Input Attribute

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DataInputAttribute : Attribute {
    public string? Id { get; }

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
    private static Dictionary<string, Func<object?>> extractors = new();
    private static Dictionary<string, Type> registeredTypes = new();
    private static HashSet<object> scanned = new();
    
    private static bool initialized = false;

    // Get Registered Ids
    public static List<string> GetRegisteredIds() {
        List<string> val = extractors.Keys.ToList();
        return val;
    }

    /**
     *
     * Get Data
     *
     */
    public static T? GetData<T>(string id) {
        T? val = Data.GetData<T>(id);
        return val;
    }

    public static object? GetData(string id) {
        object? val = Data.GetData(id);
        return val;
    }

    /**
     *
     * Register
     *
     */
    private static void Register(Type type) {
        bool found = false;

        string id = DataInputAttribute.GenerateId(type);
        registeredTypes[id] = type;

        var baseMethods = typeof(DataHandler).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<DataInjectionAttribute>() != null)
            .ToList();

        foreach(var baseMethod in baseMethods) {
            var methods = type.GetMethod(baseMethod.Name,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Static | BindingFlags.Instance);
            if(methods != null) {
                if(methods.IsStatic) {
                    extractors[id] = () => methods.Invoke(null, null);
                } else {
                    extractors[id] = () => {
                        var instance = Activator.CreateInstance(type);
                        return methods.Invoke(instance, null);
                    };
                }

                Console.WriteLine($"[DataInput] Registered {type.Name} using convention: {baseMethod.Name}");
                found = true;
                break;
            }
        }

        if(!found) {
            Console.WriteLine($"[DataInput] Warning: No matching methods found in {type.Name}");
            extractors[id] = () => {
                Console.WriteLine($"[DataInput] {type.Name} has no matching DataHandler methods");
                return null;
            };
        }
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
                if(data != null) Data.UpdateData(id, data);
                Console.WriteLine($"[DataInput] Reloaded data: {id}");
            } catch(Exception err) {
                Console.WriteLine($"[DataInput] Error reloading {id}: {err.Message}");
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
     * Load
     *
     */
    // Load
    public static void Load(string id) {
        if(extractors.TryGetValue(id, out var extractor)) {
            try {
                var data = extractor();
                if(data != null) Data.RegisterData(id, data);
                Console.WriteLine($"[DataInput] Loaded data: {id}");
            } catch(Exception err) {
                Console.WriteLine($"[DataInput] Error loading {id}: {err.Message}");
            }
        }
    }

    // Load All
    public static void LoadAll() {
        foreach(var e in extractors) {
            try {
                var data = e.Value();
                if(data != null) Data.RegisterData(e.Key, data);
                Console.WriteLine($"[DataInput] Loaded data: {e.Key}");
            } catch(Exception err) {
                Console.WriteLine($"[DataInput] Error loading {e.Key}: {err.Message}");
            }
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
                var types = assembly.GetTypes().Where(t => t.GetCustomAttribute<DataInputAttribute>() != null);
                foreach(var type in types) Register(type);
            } catch(Exception err) {
                throw new Exception($"[DataInput] Error scanning assembly: {err.Message}");
            }
        }

        initialized = true;
        Console.WriteLine("[DataInput] Initialized");
    }
}