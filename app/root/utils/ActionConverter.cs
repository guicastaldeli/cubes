namespace App.Root.Utils;

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

/**

    Action Converter attribute

    */
[AttributeUsage(AttributeTargets.Class)]
public class ActionConverterAttribute : Attribute {}

/**

    Action Converter main class.

    */
public static class ActionConverter {
    private static Dictionary<string, Type> registeredTypes = new();
    private static Dictionary<string, HashSet<int>> typeIdCache = new();
    private static Dictionary<string, Action<int>> typeHandlers = new();

    private static bool initialized = false;

    // Extract Type Part
    private static string ExtractTypePart(string action) {
        var separatorIndex = action.IndexOfAny(new char[] { ':', '-', '_', '.' });
        if(separatorIndex > 0) return action.Substring(0, separatorIndex);
        
        return action;
    }

    // Extract All Numbers
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

    /**
     *
     * Register Ids
     *
     */
    public static void RegisterIds(string typeId, IEnumerable<int> ids) {
        var key = typeId.ToLower();

        if(!typeIdCache.ContainsKey(key)) {
            typeIdCache[key] = new HashSet<int>();
        }
        foreach(var id in ids) {
            typeIdCache[key].Add(id);
        }

        Console.WriteLine($"[ActionConverter] Cached {typeIdCache[key].Count} IDs for: {key}");
    }

    /**
     *
     * Process
     *
     */
    public static void Process(string action) {
        if(string.IsNullOrEmpty(action)) {
            Console.WriteLine("[ActionConverter] Empty action");
            return;
        }

        if(!initialized) Init();
        Console.WriteLine($"[ActionConverter] Processing: {action}");

        var typePart = ExtractTypePart(action);
        if(string.IsNullOrEmpty(typePart)) {
            Console.WriteLine($"[ActionConverter] Could not extract type from: {action}");
            return;
        }

        Console.WriteLine($"[ActionConverter] Extracted type: {typePart}");

        var extractedIds = ExtractAllNumbers(action);
        if(extractedIds == null) {
            Console.WriteLine($"[ActionConverter] Could not extract id from: {action}");
            return;
        }

        foreach(var id in typeIdCache) {
            var registeredTypeId = id.Key;
            if(registeredTypeId.Contains(typePart, StringComparison.OrdinalIgnoreCase) ||
                typePart.Contains(registeredTypeId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(registeredTypeId, typePart, StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine($"[ActionConverter] Matched type: {registeredTypeId} with {typePart}");

                foreach(var eid in extractedIds) {
                    if(id.Value.Contains(eid)) {
                        Console.WriteLine($"[ActionConverter] Matched ID: {id}");

                        if(typeHandlers.TryGetValue(registeredTypeId, out var handler)) {
                            handler(eid);
                            return;
                        } else {
                            Console.WriteLine($"[ActionConverter] No handler found for: {registeredTypeId}");
                            return;
                        }
                    }
                }
            }
        }

        Console.WriteLine($"[ActionConverter] No match found for: {action}");
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
            registeredTypes[typeId] = type;

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach(var method in methods) {
                var param = method.GetParameters();
                if(param.Length == 1 && param[0].ParameterType == typeof(int)) {
                    try {
                        var handler = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), method);
                        typeHandlers[typeId] = handler;

                        Console.WriteLine($"[ActionConverter] Registered: {typeId} -> {type.Name}.{method.Name}");
                        break;
                    } catch {
                        Console.WriteLine("catch!!");
                    }
                }
            }
        }

        initialized = true;
    }
}