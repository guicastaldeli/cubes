namespace App.Root;
using App.Root.Utils;
using DPath = System.IO.Path;
using System.Collections;
using System.Reflection;
using System.Text.Json;

/**

    Data Output Attribute

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DataOutputAttribute : Attribute {
    public string? Path { get; set; }
    public Type? PathProvider { get; set; }
    public string? PathMethod { get; set; }
    public string? Section { get; set; }
    public Type[]? MethodArgs { get; set; }

    public DataOutputAttribute(string? Path = null, string? Section = null) {
        this.Path = Path;
        this.Section = Section;
    }
    public DataOutputAttribute(Type PathProvider, string PathMethod, string? Section = null, Type[]? MethodArgs = null) {
        this.PathProvider = PathProvider;
        this.PathMethod = PathMethod;
        this.Section = Section;
        this.MethodArgs = MethodArgs;
    }

    // Generate Section
    public static string GenerateSection(Type type) {
        string val = type.Name.ToLower();
        return val;
    }
}


/**

    Data Output main class

    */
public static class DataOutput {
    /**
     *
     * Data Output Info
     *
     */
    public class DataOutputInfo {
        public Type Type { get; set; }
        public string Section { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public Type? PathProvider { get; set; }
        public string? PathMethod { get; set; }
        public Type[]? MethodArgs { get; set; }

        public static string PATH_DIR = DPath.GetFullPath(DPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "root"));

        public DataOutputInfo(Type Type, string Path, string Section, string Id) {
            this.Type = Type;
            this.Section = Section;
            this.Id = Id;
            this.Path = DPath.Combine(PATH_DIR, Path);
            this.PathProvider = null;
            this.PathMethod = null;
            this.MethodArgs = null;
        }
        public DataOutputInfo(Type Type, Type PathProvider, string PathMethod, string Section, string Id, Type[]? MethodArgs = null) {
            this.Type = Type;
            this.Section = Section;
            this.Id = Id;
            this.PathProvider = PathProvider;
            this.PathMethod = PathMethod;
            this.MethodArgs = MethodArgs;
            this.Path = ResolvePath();
        }

        // Get Full Path
        public string GetFullPath() {
            if(PathProvider != null && !string.IsNullOrEmpty(PathMethod)) {
                return ResolvePath();
            }

            return Path;
        }

        // Resolve Path
        private string ResolvePath() {
            if(PathProvider == null || string.IsNullOrEmpty(PathMethod)) return "";

            try {
                MethodInfo? method = null;

                if(MethodArgs != null && MethodArgs.Length > 0) {
                    method = PathProvider.GetMethod(PathMethod, 
                        BindingFlags.Public | BindingFlags.NonPublic | 
                        BindingFlags.Static | BindingFlags.FlattenHierarchy,
                        null, MethodArgs, null);
                } else {
                    method = PathProvider.GetMethod(PathMethod, 
                        BindingFlags.Public | BindingFlags.NonPublic | 
                        BindingFlags.Static | BindingFlags.FlattenHierarchy,
                        null, Type.EmptyTypes, null);
                }

                if(method != null) {
                    var param = method.GetParameters();
                    object?[]? args = null;

                    if(param.Length > 0) {
                        args = new object?[param.Length];
                        for(int i = 0; i < param.Length; i++) {
                            if(param[i].ParameterType.IsValueType) {
                                args[i] = Activator.CreateInstance(param[i].ParameterType);
                            } else {
                                args[i] = null;
                            }
                        }
                    }

                    var result = method.Invoke(null, args);
                    if(result != null) {
                        string resolved = result.ToString() ?? "";
                        Console.WriteLine($"[DataOutput] Called {PathProvider.Name}.{PathMethod}() = {resolved}");
                        return resolved;
                    }
                }
            } catch(Exception err) {
                Console.WriteLine($"[DataOutput] Error calling {PathProvider?.Name}.{PathMethod}: {err.Message}");
            }

            return "";
        }
    }

    /**
     *
     * Data Output main
     *
     */
    private static Dictionary<string, DataOutputInfo> outputRegistry = new();
    private static string? currentSavePath = null;

    private static bool initialized = false;

    // Get Registered Ids
    public static List<string> GetRegisteredIds() {
        List<string> val = outputRegistry.Keys.ToList();
        return val;
    }

    // Has Saved Data
    public static bool HasSavedData(string id) {
        if(!outputRegistry.TryGetValue(id, out var info)) return false;

        string fullPath = info.GetFullPath();
        if(!File.Exists(fullPath)) return false;

        try {
            string data = File.ReadAllText(fullPath);
            var allData = JsonSerializer.Deserialize<Dictionary<string, object>>(data);
            
            bool val = allData != null && allData.ContainsKey(info.Section);
            return val;
        } catch(Exception err) {
            throw new Exception($"DataOutput -- HaSavedData -- Error {err}");
        }
    }

    // Set Save Path
    public static void SetSavePath(string savePath) {
        currentSavePath = savePath;
        Console.WriteLine($"[DataOutput] Save path set to: {savePath}");
    }

    // Get Current Save Path
    public static string GetCurrentSavePath() {
        string val = currentSavePath ?? "err";
        return val;
    }
 
    /**
     *
     * Save
     *
     */
    // Save
    public static void Save(string id) {
        if(!outputRegistry.TryGetValue(id, out var info)) {
            Console.WriteLine($"[DataOutput] No registered output for {id}");
            return;
        }

        var data = Data.GetData(id);
        if(data == null) {
            data = Data.GetAllDataIds()
                .Select(k => Data.GetData(k))
                .FirstOrDefault(d => d != null && d.GetType() == info.Type);
        }
        if(data == null) {
            Console.WriteLine($"[DataOutput] No data found for {id}");
            return;
        }

        SaveData(info, data);
    }

    // Save All
    public static void SaveAll() {
        foreach(var o in outputRegistry) {
            Save(o.Key);
        }
    }

    // Save Data
    public static void SaveData(DataOutputInfo info, object data) {
        string path = info.GetFullPath();
        string? directory = DPath.GetDirectoryName(path);
        if(!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

        Dictionary<string, object> existingData = new();
        if(File.Exists(path)) {
            try {
                string text = File.ReadAllText(path);
                existingData = JsonSerializer.Deserialize<Dictionary<string, object>>(text) ?? new Dictionary<string, object>();
            } catch(Exception err) {
                throw new Exception($"DataOutput -- SaveData -- Error: {err}");
            }
        }

        var serializedData = SerializeData(data);
        if(serializedData != null) existingData[info.Section] = serializedData;

        string outputText = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, outputText);
        Console.WriteLine($"[DataOutput] Saved {info.Id} to {info.Path}:{info.Section}");
    }

    /**
     *
     * Serialize
     *
     */
    private static object? SerializeData(object data) {
        if(data == null || data.GetType().IsPrimitive || data is string) {
            return data;
        }

        if(data is IEnumerable en) {
            var list = new List<object>();
            foreach(var item in en) list.Add(SerializeData(item)!);
            return list;
        }
        if(data is IDictionary<string, object> dict) {
            var res = new Dictionary<string, object>();
            foreach(var d in dict) res[d.Key] = SerializeData(d.Value)!;
            return res;
        }

        var resDict = new Dictionary<string, object>();
        var type = data.GetType();

        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            if(prop.GetIndexParameters().Length > 0) continue;
            try {
                var val = prop.GetValue(data);
                if(val != null) resDict[prop.Name.ToLower()] = SerializeData(val)!;
            } catch(Exception err) {
                throw new Exception($"DataOutput -- Serialize Data -- Error {err}");
            }
        }
        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            try {
                var val = field.GetValue(data);
                if(val != null) resDict[field.Name.ToLower()] = SerializeData(val)!;
            } catch(Exception err) {
                throw new Exception($"DataOutput -- Serialize Data -- Error {err}");
            }
        }

        return resDict;
    }

    /**
     *
     * Deserialize
     *
     */
    private static object? DeserializeData(object data, Type targetType) {
        if(data == null) return null;

        if(targetType.IsPrimitive || targetType == typeof(string)) {
            return Convert.ChangeType(data, targetType);
        }

        if(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>)) {
            var elementType = targetType.GetGenericArguments()[0];
            var convertedItems = new List<object>();

            if(data is IEnumerable en) {
                foreach(var item in en) {
                    var converted = DeserializeData(item, elementType);
                    convertedItems.Add(converted!);
                }
            }

            return CollectionFactory.CreateList(elementType, convertedItems);
        }
        if(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
            var keyType = targetType.GetGenericArguments()[0];
            var valueType = targetType.GetGenericArguments()[1];

            if(data is IDictionary<string, object> dictData) {
                var convertedDict = new Dictionary<string, object>();
                foreach(var d in dictData) {
                    var converted = DeserializeData(d.Value, valueType);
                    convertedDict[d.Key] = converted!;
                }

                return CollectionFactory.CreateDictionary(keyType, valueType, convertedDict);
            }
        }
        if(data is IDictionary<string, object> objData) {
            var instance = Activator.CreateInstance(targetType);
            
            foreach(var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if(objData.TryGetValue(prop.Name.ToLower(), out var val)) {
                    try {
                        var converted = DeserializeData(val, prop.PropertyType);
                        prop.SetValue(instance, converted);
                    } catch(Exception err) {
                        throw new Exception($"DataOutput -- Deserialize Data -- Error {err.Message}");
                    }
                }
            }
            foreach(var field in targetType.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                if(objData.TryGetValue(field.Name.ToLower(), out var val)) {
                    try {
                        var converted = DeserializeData(val, field.FieldType);
                        field.SetValue(instance, converted);
                    } catch(Exception err) {
                        throw new Exception($"DataOutput -- Deserialize Data -- Error {err.Message}");
                    }
                }
            }

            return instance;
        }

        return data;
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
                    var attr = type.GetCustomAttribute<DataOutputAttribute>();
                    if(attr != null) {
                        string id = DataInputAttribute.GenerateId(type);
                        string section = attr.Section ?? DataOutputAttribute.GenerateSection(type);
                        DataOutputInfo info;

                        if(attr.PathProvider != null && !string.IsNullOrEmpty(attr.PathMethod)) {
                            info = new DataOutputInfo(type, attr.PathProvider, attr.PathMethod, section, id);
                            Console.WriteLine($"[DataOutput] Registered {type.Name} with ID: {id} -> {attr.PathProvider.Name}.{attr.PathMethod}():{section}");
                        } else if(!string.IsNullOrEmpty(attr.Path)) {
                            info = new DataOutputInfo(type, attr.Path, section, id);
                            Console.WriteLine($"[DataOutput] Registered {type.Name} with ID: {id} -> {attr.Path}:{section}");
                        } else {
                            Console.WriteLine($"[DataOutput] Warning: No path specified for {type.Name}");
                            continue;
                        }

                        outputRegistry[id] = info;
                    }
                }
            } catch(Exception err) {
                throw new Exception($"DataOutput -- Init -- Error {err.Message}");
            }
        }

        initialized = true;
    }

    /**
     *
     * Load
     *
     */
    // Load
    public static void Load(string id) {
        if(!outputRegistry.TryGetValue(id, out var info)) {
            Console.WriteLine($"[DataOutput] No registered output for {id}");
            return;
        }

        string fullPath = info.GetFullPath();
        if(!File.Exists(fullPath)) {
            Console.WriteLine($"[DataOutput] File not found: {fullPath}");
            return;
        }

        try {
            string data = File.ReadAllText(fullPath);
            var allData = JsonSerializer.Deserialize<Dictionary<string, object>>(data);

            if(allData != null && allData.TryGetValue(info.Section, out var sectionData)) {
                var convertedData = DeserializeData(sectionData, info.Type);
                if(convertedData != null) Data.RegisterData(id, convertedData);
                Console.WriteLine($"[DataOutput] Loaded {id} from {info.Path}:{info.Section}");
            }
        } catch(Exception err) {
            throw new Exception($"DataOutput -- Load {id} -- Error {err.Message}");
        }
    }

    // Load All
    public static void LoadAll() {
        foreach(var id in outputRegistry.Keys) {
            try {
                if(HasSavedData(id)) {
                    Load(id);
                    Console.WriteLine($"[DataOutput] Loaded all data for: {id}");
                }
            } catch(Exception err) {
                Console.WriteLine($"[DataOutput] Error loading {id}: {err.Message}");
            }
        }
    }
}