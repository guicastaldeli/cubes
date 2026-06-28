namespace App.Root;

using System.Collections;
using System.Reflection;
using System.Text.Json;

/**

    Data Output Attribute

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DataOutputAttribute : Attribute {
    public string Path { get; set; }
    public string Section { get; set; }

    public DataOutputAttribute(string Path = null, string Section = null) {
        this.Path = Path;
        this.Section = Section;
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
    private class DataOutputInfo {
        public Type Type { get; set; }
        public string Path { get; set; }
        public string Section { get; set; }
        public string Id { get; set; }
    }

    /**
     *
     * Data Output main
     *
     */
    private static Dictionary<string, DataOutputInfo> outputRegistry = new();

    private static bool initialized = false;

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
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, info.Path);
        string directory = Path.GetDirectoryName(fullPath);
        if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        Dictionary<string, object> existingData = new();
        if(File.Exists(fullPath)) {
            try {
                string text = File.ReadAllText(fullPath);
                existingData = JsonSerializer.Deserialize<Dictionary<string, object>>(text) ?? new Dictionary<string, object>();
            } catch(Exception err) {
                throw new Exception($"DataOutput -- SaveData -- Error: {err}");
            }
        }

        var serializedData = SerializeData(data);
        existingData[info.Section] = serializedData;

        string outputText = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fullPath, outputText);
        Console.WriteLine($"[DataOutput] Saved {info.Id} to {info.Path}:{info.Section}");
    }

    /**
     *
     * Serialize
     *
     */
    private static object SerializeData(object data) {
        if(data == null || data.GetType().IsPrimitive || data is string) {
            return data;
        }

        if(data is IEnumerable en) {
            var list = new List<object>();
            foreach(var item in en) list.Add(SerializeData(item));
            return list;
        }
        if(data is IDictionary<string, object> dict) {
            var res = new Dictionary<string, object>();
            foreach(var d in dict) res[d.Key] = SerializeData(d.Value);
            return res;
        }

        var resDict = new Dictionary<string, object>();
        var type = data.GetType();

        foreach(var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            try {
                var val = prop.GetValue(data);
                resDict[prop.Name.ToLower()] = SerializeData(val);
            } catch(Exception err) {
                throw new Exception($"DataOutput -- Serialize Data -- Error {err}");
            }
        }
        foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            try {
                var val = field.GetValue(data);
                resDict[field.Name.ToLower()] = SerializeData(val);
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
    private static object DeserializeData(object data, Type targetType) {
        if(data == null) return null;

        if(targetType.IsPrimitive || targetType == typeof(string)) {
            return Convert.ChangeType(data, targetType);
        }
        if(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>)) {
            var elType = targetType.GetGenericArguments()[0];
            var list = Activator.CreateInstance(targetType);
            var addMethod = targetType.GetMethod("Add");

            if(data is IEnumerable en) {
                foreach(var item in en) {
                    var converted = DeserializeData(item, elType);
                    addMethod?.Invoke(list, new[] { converted });
                }
            }

            return list;
        }
        if(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
            var dict = Activator.CreateInstance(targetType);
            var addMethod = targetType.GetMethod("Add");

            if(data is IDictionary<string, object> dictData) {
                var valueType = targetType.GetGenericArguments()[1];
                foreach(var d in dictData) {
                    var converted = DeserializeData(d.Value, valueType);
                    addMethod?.Invoke(dict, new[] { d.Key, converted });
                }
            }

            return dict;
        }
        if(data is IDictionary<string, object> objData) {
            var instance = Activator.CreateInstance(targetType);

            foreach(var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if(objData.TryGetValue(prop.Name.ToLower(), out var val)) {
                    try {
                        var converted = DeserializeData(val, prop.PropertyType);
                        prop.SetValue(instance, converted);
                    } catch(Exception err) {
                        throw new Exception($"DataOutput -- Deserialize Data -- Error {err}");
                    }
                }
            }
            foreach(var field in targetType.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                if(objData.TryGetValue(field.Name.ToLower(), out var val)) {
                    try {
                        var converted = DeserializeData(val, field.FieldType);
                        field.SetValue(instance, converted);
                    } catch(Exception err) {
                        throw new Exception($"DataOutput -- Deserialize Data -- Error {err}");
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
                        string path = attr.Path;
                        string section = attr.Section;

                        outputRegistry[id] = new DataOutputInfo {
                            Type = type,
                            Path = path,
                            Section = section,
                            Id = id
                        };

                        Console.WriteLine($"[DataOutput] Registered {type.Name} with ID: {id} -> {path}:{section}");
                    }
                }
            } catch(Exception err) {
                throw new Exception($"DataOutput -- Init -- Error {err}");
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
                Data.RegisterData(id, convertedData);
                Console.WriteLine($"[DataOutput] Loaded {id} from {info.Path}:{info.Section}");
            }
        } catch(Exception err) {
            throw new Exception($"DataOutput -- Load {id} -- Error {err}");
        }
    }
}