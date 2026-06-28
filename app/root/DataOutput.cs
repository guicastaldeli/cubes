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
    public string? Section { get; set; }

    public DataOutputAttribute(string? Path = null, string? Section = null) {
        this.Path = Path;
        this.Section = Section;
    }

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

        public DataOutputInfo(Type Type, string Path, string Section, string Id) {
            this.Type = Type;
            this.Section = Section;
            this.Id = Id;
            this.Path = DPath.Combine(AppDomain.CurrentDomain.BaseDirectory, Path);
        }
    }

    /**
     *
     * Data Output main
     *
     */
    private static Dictionary<string, DataOutputInfo> outputRegistry = new();

    private static bool initialized = false;

    // Get Registered Ids
    public static List<string> GetRegisteredIds() {
        List<string> val = outputRegistry.Keys.ToList();
        return val;
    }

    // Has Saved Data
    public static bool HasSavedData(string id) {
        if(!outputRegistry.TryGetValue(id, out var info)) return false;

        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, info.Path);
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
        string path = info.Path;
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
                        string path = attr.Path!;
                        string section = attr.Section ?? DataOutputAttribute.GenerateSection(type);

                        outputRegistry[id] = new DataOutputInfo(type, path, section, id);

                        Console.WriteLine($"[DataOutput] Registered {type.Name} with ID: {id} -> {path}:{section}");
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
    public static void Load(string id) {
        if(!outputRegistry.TryGetValue(id, out var info)) {
            Console.WriteLine($"[DataOutput] No registered output for {id}");
            return;
        }

        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, info.Path);
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
}