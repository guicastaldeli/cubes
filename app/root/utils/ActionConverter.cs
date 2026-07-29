namespace App.Root.Utils;

using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using App.Root.Input;

/**

    Action Converter attribute

    */
[AttributeUsage(AttributeTargets.Class)]
public class ActionConverterAttribute : Attribute {}

/**

    Action Converter main class.

    */
public static class ActionConverter {
    private static Dictionary<string, HashSet<int>> typeIdCache = new();
    private static Dictionary<Type, PropertyInfo?> idPropertyCache = new();

    private static bool initialized = false;

    // Extract Type Part
    private static string ExtractTypePart(string action) {
        var separatorIndex = action.IndexOfAny(new char[] { ':', '-', '_', '.' });
        if(separatorIndex > 0) return action.Substring(0, separatorIndex);
        return action;
    }

    // Extract All Number
    private static List<int> ExtractAllNumbers(string text) {
        string r = @"-?\d+";

        var numbers = new List<int>();
        var matches = Regex.Matches(text, r);

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
        
        foreach(var dataId in dataIds) {
            var data = Data.GetData(dataId);
            if(data == null) continue;
            
            var dataType = data.GetType();
            if(!dataType.IsGenericType || dataType.GetGenericTypeDefinition() != typeof(List<>)) continue;
            
            var list = data as IList;
            if(list == null || list.Count == 0) continue;
            
            var elementType = dataType.GetGenericArguments()[0];
            var idProp = FindIdProp(elementType);
            if(idProp == null) continue;
            
            var ids = new HashSet<int>();
            foreach(var item in list) {
                if(item == null) continue;
                var val = idProp.GetValue(item);
                if(val is int intVal) {
                    ids.Add(intVal);
                }
            }
            
            if(ids.Count > 0) {
                typeIdCache[dataId] = ids;
                Console.WriteLine($"[ActionConverter] Cached {ids.Count} IDs for: {dataId}");
            }
        }
    }

    // Find Id Prop
    private static PropertyInfo? FindIdProp(Type type) {
        if(idPropertyCache.TryGetValue(type, out var cached)) return cached;

        (string a, string b) Dict = ( "id", "Id" );

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach(var prop in props) {
            var keyAttr = prop.GetCustomAttribute<ConverterKey>();
            if(keyAttr != null && string.Equals(keyAttr.Key, Dict.a, StringComparison.OrdinalIgnoreCase)) {
                idPropertyCache[type] = prop;
                return prop;
            }
        }
        foreach(var prop in props) {
            if(string.Equals(prop.Name, Dict.a, StringComparison.OrdinalIgnoreCase)) {
                idPropertyCache[type] = prop;
                return prop;
            }
        }
        foreach(var prop in props) {
            if(prop.PropertyType == typeof(int) &&
                (prop.Name.Contains(Dict.a, StringComparison.OrdinalIgnoreCase) ||
                prop.Name.Contains(Dict.b, StringComparison.OrdinalIgnoreCase))) {
                idPropertyCache[type] = prop;
                return prop;
            }
        }

        idPropertyCache[type] = null;
        return null;
    }

    /**
     *
     * Convert
     *
     */
    public static (string? typeName, int? id) Convert(string action) {
        if(string.IsNullOrEmpty(action)) {
            Console.WriteLine("[ActionConverter] Empty action");
            return (null, null);
        }

        if(!initialized) Init();

        var typePart = ExtractTypePart(action);
        if(string.IsNullOrEmpty(typePart)) {
            Console.WriteLine($"[ActionConverter] Could not extract type from: {action}");
            return (null, null);
        }

        var extractedIds = ExtractAllNumbers(action);
        if(extractedIds.Count == 0) {
            Console.WriteLine($"[ActionConverter] No ID found in: {action}");
            return (null, null);
        }

        foreach(var id in typeIdCache) {
            var cachedTypeId = id.Key;
            var cachedIds = id.Value;

            if(cachedTypeId.Contains(typePart, StringComparison.OrdinalIgnoreCase) ||
                typePart.Contains(cachedTypeId, StringComparison.OrdinalIgnoreCase)) {
                foreach(var eid in extractedIds) {
                    if(cachedIds.Contains(eid)) {
                        Console.WriteLine($"[ActionConverter] Converted: {action} -> {cachedTypeId}:{eid}");
                        return (cachedTypeId, eid);
                    }
                }
            }
        }

        Console.WriteLine($"[ActionConverter] No match found for: {action}");
        return (null, null);
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
                catch { return new Type[0]; }
            })
            .Where(t => t.GetCustomAttribute<ActionConverterAttribute>() != null);

        foreach(var type in types) {
            var typeId = type.Name.ToLower();
            if(!typeIdCache.ContainsKey(typeId)) {
                typeIdCache[typeId] = new HashSet<int>();
            }
        }

        ExtractIdsFromData();
        initialized = true;
    }
}