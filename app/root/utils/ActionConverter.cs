namespace App.Root.Utils;

using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

/**

    Action Converter attribute

    */
[AttributeUsage(AttributeTargets.Class)]
public class ActionConverterAttribute : Attribute {}

/**

    Converted Action result

    */
public class ConvertedAction {
    public string? MethodName { get; set; }
    public string? TypeName { get; set; }
    public int? IntId { get; set; }
    public string? StringId {  get; set; }
    public object? Param => (object?)StringId ?? IntId;

    public void Deconstruct(out string? TypeName, out int? IntId) {
        TypeName = this.TypeName;
        IntId = this.IntId;
    }
}

/**

    Action Converter main class.

    */
public static class ActionConverter {
    private static Dictionary<string, HashSet<int>> typeIdCache = new();
    private static Dictionary<string, HashSet<string>> typeStringIdCache = new();
    private static Dictionary<Type, PropertyInfo?> idPropertyCache = new();

    private static bool initialized = false;

    // Extract Method Name
    private static string ExtractMethodName(string action) {
        var colonIndex = action.IndexOf(':');
        if(colonIndex > 0) return action.Substring(0, colonIndex);

        var separatorIndex = action.IndexOfAny(new char[] { '_', '.' });
        if(separatorIndex > 0) return action.Substring(0, separatorIndex);

        return action;
    }

    // Extract Type Part
    private static string ExtractTypePart(string action) {
        var separatorIndex = action.IndexOfAny(new char[] { ':', '_', '.' });
        if(separatorIndex > 0) return action.Substring(0, separatorIndex);
        return action;
    }

    // Extract Param
    private static string? ExtractParam(string action) {
        var colonIndex = action.IndexOf(':');
        if(colonIndex > 0) return action.Substring(colonIndex + 1);
        return null;
    }

    // Extract All Numbers
    private static List<int> ExtractAllNumbers(string text) {
        if(Guid.TryParse(text, out _)) return new List<int>();

        List<int> numbers = new List<int>();

        string p = @"-?\d+";
        var matches = Regex.Matches(text, p);
        foreach(Match match in matches) {
            if(int.TryParse(match.Value, out int num)) {
                numbers.Add(num);
            }
        }

        return numbers;
    }

    // Extract Ids from Data
    private static void ExtractIdsFromData() {
        var dataIds = Data.GetAllDataIds();

        foreach(var id in dataIds) {
            var data = Data.GetData(id);
            if(data == null) continue;

            var dataType = data.GetType();
            if(!dataType.IsGenericType || dataType.GetGenericTypeDefinition() != typeof(List<>)) continue;

            var list = data as IList;
            if(list == null || list.Count == 0) continue;

            var elementType = dataType.GetGenericArguments()[0];
            var idProp = FindIdProp(elementType);
            if(idProp == null) continue;

            var intIds = new HashSet<int>();
            var stringIds = new HashSet<string>();

            foreach(var item in list) {
                if(item == null) continue;
                
                var val = idProp.GetValue(item);
                if(val is int intVal) {
                    intIds.Add(intVal);
                } else if(val is string strVal) {
                    stringIds.Add(strVal);
                }

                if(intIds.Count > 0) {
                    typeIdCache[id] = intIds;
                    Console.WriteLine($"[ActionConverter] Cached {intIds.Count} int IDs for: {id}");
                }
                if(stringIds.Count > 0) {
                    typeStringIdCache[id] = stringIds;
                    Console.WriteLine($"[ActionConverter] Cached {stringIds.Count} string IDs for: {id}");
                }
            }
        }
    }

    // Find Id Prop
    private static PropertyInfo? FindIdProp(Type type) {
        if(idPropertyCache.TryGetValue(type, out var cached)) return cached;

        string id = "id";

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach(var prop in props) {
            var keyAttr = prop.GetCustomAttribute<ConverterKey>();
            if(keyAttr != null && string.Equals(keyAttr.Key, id, StringComparison.OrdinalIgnoreCase)) {
                idPropertyCache[type] = prop;
                return prop;
            }
        }
        foreach(var prop in props) {
            if(string.Equals(prop.Name, id, StringComparison.OrdinalIgnoreCase)) {
                idPropertyCache[type] = prop;
                return prop;
            }
        }
        foreach(var prop in props) {
            if((prop.PropertyType == typeof(int) || prop.PropertyType == typeof(string)) &&
                prop.Name.Contains(id, StringComparison.OrdinalIgnoreCase)) {
                idPropertyCache[type] = prop;
                return prop;
            }
        }

        idPropertyCache[type] = null;
        return null;
    }

    // Match Type Name
    private static string? MatchTypeName(string methodName, IEnumerable<string> keys) {
        foreach(var key in keys) {
            if(key.Contains(methodName, StringComparison.OrdinalIgnoreCase) || methodName.Contains(key, StringComparison.OrdinalIgnoreCase)) {
                return key;
            }

            var singular = WordInflector.ToSingular(key);
            if(methodName.Contains(singular, StringComparison.OrdinalIgnoreCase)) return key;

            var plural = WordInflector.ToPlural(key);
            if(methodName.Contains(plural, StringComparison.OrdinalIgnoreCase)) return key;
        }   

        return null;
    }

    /**
     *
     * Convert
     *
     */
    public static ConvertedAction Convert(string action) {
        var result = new ConvertedAction();
        if(string.IsNullOrEmpty(action)) {
            Console.WriteLine("[ActionConverter] Empty action");
            return result;
        }

        if(!initialized) Init();

        result.MethodName = ExtractMethodName(action);
        
        var param = ExtractParam(action);
        if(param == null) {
            var typePart = ExtractTypePart(action);
            var extractIds = ExtractAllNumbers(action);

            foreach(var entry in typeIdCache) {
                if(entry.Key.Contains(typePart, StringComparison.OrdinalIgnoreCase) || typePart.Contains(entry.Key, StringComparison.OrdinalIgnoreCase)) {
                    foreach(var eid in extractIds) {
                        if(entry.Value.Contains(eid)) {
                            result.TypeName = entry.Key;
                            result.IntId = eid;
                            Console.WriteLine($"[ActionConverter] Converted (legacy int): {action} -> {result.TypeName}:{result.IntId}");
                            return result;
                        }
                    }
                }
            }

            //Console.WriteLine($"[ActionConverter] Plain method: {result.MethodName}");
            return result;
        }

        var guidMatch = Guid.TryParse(param, out _);
        if(guidMatch) {
            result.StringId = param;
            result.TypeName = 
                MatchTypeName(result.MethodName, typeStringIdCache.Keys) ??
                MatchTypeName(result.MethodName, typeIdCache.Keys) ??
                result.MethodName;

            Console.WriteLine($"[ActionConverter] Converted (GUID): {action} -> {result.TypeName}:{result.StringId}");
            return result;
        }

        var stringMatch = MatchTypeName(result.MethodName, typeStringIdCache.Keys);
        if(stringMatch != null && typeStringIdCache[stringMatch].Contains(param)) {
            result.TypeName = stringMatch;
            result.StringId = param;
            Console.WriteLine($"[ActionConverter] Converted (string): {action} -> {result.TypeName}:{result.StringId}");
            return result;
        }

        var intIds = ExtractAllNumbers(param);
        var intMatch = MatchTypeName(result.MethodName, typeIdCache.Keys);
        if(intMatch != null) {
            foreach(var id in intIds) {
                if(typeIdCache[intMatch].Contains(id)) {
                    result.TypeName = intMatch;
                    result.IntId = id;
                    Console.WriteLine($"[ActionConverter] Converted (int): {action} -> {result.TypeName}:{result.IntId}");
                    return result;
                }
            }
        }

        result.StringId = param;
        result.TypeName = result.MethodName;
        Console.WriteLine($"[ActionConverter] Passthrough param: {action} -> {result.TypeName}:{result.StringId}");
        return result;
    }

    /**
     *
     * Init
     *
     */
    public static void Init() {
        if(initialized) return;

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => {
                try { return a.GetTypes(); }
                catch{ return new Type[0]; }
            })
            .Where(t => t.GetCustomAttribute<ActionConverterAttribute>() != null);
        foreach(var type in types) {
            var typeId = type.Name.ToLower();
            if(!typeIdCache.ContainsKey(typeId)) typeIdCache[typeId] = new HashSet<int>();
            if(!typeStringIdCache.ContainsKey(typeId)) typeStringIdCache[typeId] = new HashSet<string>();
        }

        ExtractIdsFromData();
        initialized = true;
    }
}