
using System.Collections;
using System.Reflection;

/**

    Global State Manager

    */
namespace App.Root;

/**

    Managed State

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ManagedStateAttribute : Attribute {}

/**

    Skip Reset State

    */
[AttributeUsage(AttributeTargets.Field)]
public sealed class SkipResetAttribute : Attribute {}

/**

    Main State Manager class

    */
public static class StateManager {
    private static readonly Dictionary<object, Dictionary<FieldInfo, object?>> snapshots = new();
    private static readonly Dictionary<Type, Dictionary<FieldInfo, object?>> staticSnapshots = new();

    /**
     * 
     * Deep Clone
     *
     */
    private static object? DeepClone(object? value) {
        if(value == null) return null;
        var type = value.GetType();

        if(type.IsValueType) return value;
        if(type == typeof(string)) return value;

        if(type.IsArray) {
            var arr = (Array)value;
            var clone = Array.CreateInstance(type.GetElementType()!, arr.Length);
            Array.Copy(arr, clone, arr.Length);

            return clone;
        }

        if(type.IsGenericType) {
            var def = type.GetGenericTypeDefinition();

            if(def == typeof(List<>)) {
                var clone = Activator.CreateInstance(type);
                var add = type.GetMethod("Add")!;
                foreach(var item in (IEnumerable)value) {
                    add.Invoke(clone, new[] { item });
                }

                return clone;
            }
            if(def == typeof(HashSet<>)) {
                var clone = Activator.CreateInstance(type);
                var add = type.GetMethod("Add")!;
                foreach(var item in (IEnumerable)value) {
                    add.Invoke(clone, new[] { item });
                }

                return clone;
            }
            if(def == typeof(Dictionary<,>)) {
                var clone = Activator.CreateInstance(type);
                var add = type.GetMethod("Add")!;
                foreach(DictionaryEntry pair in (IDictionary)value) {
                    add.Invoke(clone, new[] { pair.Key, pair.Value });
                }

                return clone;
            }
            if(def == typeof(Queue<>)) {
                var clone = Activator.CreateInstance(type);
                var enqueue = type.GetMethod("Enqueue")!;
                foreach(var item in (IEnumerable)value) {
                    enqueue.Invoke(clone, new[] { item });
                }

                return clone;
            }
            if(def == typeof(Stack<>)) {
                var clone = Activator.CreateInstance(type);
                var push = type.GetMethod("Push")!;
                var items = ((IEnumerable)value).Cast<object>().Reverse();
                foreach(var item in items) {
                    push.Invoke(clone, new[] { item });
                }

                return clone;
            }
        }

        return value;
    }

    public static bool isRegistered(object instance) {
        return snapshots.ContainsKey(instance);
    }
    
    /**
     * 
     * Capture Snapshot
     *
     */
    private static Dictionary<FieldInfo, object?> CaptureSnapshot(object instance) {
        var type = instance.GetType();
        var fields = new Dictionary<FieldInfo, object?>();

        var t = type;
        while(t != null && t != typeof(object)) {
            foreach(var field in t.GetFields(
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly
            )) {
                if(field.GetCustomAttribute<SkipResetAttribute>() != null) continue;

                var value = field.GetValue(instance);
                fields[field] = DeepClone(value);
            }

            t = t.BaseType;
        }

        return fields;
    }

    /**
     * 
     * Register
     *
     */
    public static void Register(object instance) {
        var type = instance.GetType();
        if(type.GetCustomAttribute<ManagedStateAttribute>() == null) {
            throw new InvalidOperationException(
                $"[StateManager] '{type.Name}' must be decorated with [ManagedState] to register."
            );
        }

        var snapshot = CaptureSnapshot(instance);
        snapshots[instance] = snapshot;

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"[StateManager] Registered: {type.Name}");
        Console.ResetColor();
    }

    public static void SRegister(Type type) {
        if(type.GetCustomAttribute<ManagedStateAttribute>() == null) {
            throw new InvalidOperationException(
                $"[StateManager] '{type.Name}' must be decorated with [ManagedState] to register."
            );
        }

        var fields = new Dictionary<FieldInfo, object?>();
        foreach(var field in type.GetFields(
            BindingFlags.Static |
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.DeclaredOnly
        )) {
            if(field.GetCustomAttribute<SkipResetAttribute>() != null) continue;
            fields[field] = DeepClone(field.GetValue(null));
        }

        staticSnapshots[type] = fields;

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"[StateManager] Registered (Static): {type.Name}");
        Console.ResetColor();
    }

    /**
     * 
     * Unregister
     *
     */
    public static void Unregister(object instance) {
        snapshots.Remove(instance);
    }

    /**
     * 
     *
     *
     */
    public static void Reset(object instance) {
        if(!snapshots.TryGetValue(instance, out var snapshot)) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[StateManager] WARN: No snapshot for {instance.GetType().Name}");
            Console.ResetColor();
            return;
        }

        foreach(var (field, value) in snapshot) {
            field.SetValue(instance, DeepClone(value));
        }
    }

    public static void SReset(Type type) {
        if(!staticSnapshots.TryGetValue(type, out var snapshot)) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[StateManager] WARN: No snapshot for {type.Name}");
            Console.ResetColor();
            return;
        }

        foreach(var (field, value) in snapshot) {
            field.SetValue(null, DeepClone(value));
        }
    }

    public static void ResetAll() {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[StateManager] Soft reset — {snapshots.Count} instances");
        Console.ResetColor();

        foreach(var instance in snapshots.Keys) {
            Reset(instance);
        }

        foreach(var type in staticSnapshots.Keys) {
            var snapshot = staticSnapshots[type];
            foreach(var (field, value) in snapshot) {
                field.SetValue(null, DeepClone(value));
            }
        }
    }
}

