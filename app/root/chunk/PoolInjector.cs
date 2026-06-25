using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace App.Root.Chunk;

/**

    Poolable Attribute

    */
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public class PoolableAttribute : Attribute {
    public string Id { get; }
    public Type ItemType { get; }
    public int InitialSize { get; set; } = 32;
    public int MaxSize { get; set; } = 256;

    public PoolableAttribute(string Id, Type ItemType) {
        this.Id = Id;
        this.ItemType = ItemType;
    }
}

/**

    Pool Injector main class

    */
public static class PoolInjector {
    private static readonly Dictionary<object, bool> injected = new();

    // Get Or Create Pool
    private static IPool GetOrCreatePool(Type itemType, string id, int initialSize, int maxSize) {
        if(PoolManager.HasPool(id)) return PoolManager.DGetPool(id)!;

        var factory = CreateFactory(itemType);
        var resetAction = CreateResetAction(itemType);
        var poolType = typeof(Pool<>).MakeGenericType(itemType);

        var constructor = poolType.GetConstructor(new[] { typeof(int), typeof(int), typeof(Func<object>), typeof(Action<object>) });
        if(constructor == null) throw new Exception($"No constructor found for {poolType.Name}");
        
        var pool = constructor.Invoke(new object?[] { initialSize, maxSize, factory, resetAction });

        var registerMethod = typeof(PoolManager).GetMethod(nameof(PoolManager.RegisterPool))!;
        registerMethod.Invoke(null, new object?[] { id, pool });

        return (IPool)pool!;
    }

    // Create Instance
    private static T CreateInstance<T>() where T : new() {
        return new T();
    }

    private static T CreateInstanceWithActivator<T>() where T : class {
        return (T)Activator.CreateInstance(typeof(T), true)!;
    }

    /**
     *
     * Process
     * 
     */
    // Process Field
    private static void ProcessField(object target, FieldInfo field) {
        var attr = field.GetCustomAttribute<PoolableAttribute>();
        if(attr == null) return;

        var fieldType = field.FieldType;
        var pool = GetOrCreatePool(fieldType, attr.Id, attr.InitialSize, attr.MaxSize);
        var value = pool.GetObject();

        field.SetValue(target, value);
        PoolRegistry.Register(attr.Id, pool, value);
    }

    // Process Property 
    private static void ProcessProperty(object target, PropertyInfo prop) {
        if(!prop.CanWrite) return;

        var attr = prop.GetCustomAttribute<PoolableAttribute>();
        if(attr == null) return;

        var propType = prop.PropertyType;
        var pool = GetOrCreatePool(propType, attr.Id, attr.InitialSize, attr.MaxSize);
        var value = pool.GetObject();

        prop.SetValue(target, value);
        PoolRegistry.Register(attr.Id, pool, value);
    }

    /**
     *
     * Create
     * 
     */
    // Create Factory
    private static Func<object>? CreateFactory(Type type) {
        if(type.GetConstructor(Type.EmptyTypes) != null) {
            return () => Activator.CreateInstance(type)!;
        }

        if(type.IsGenericType) {
            var genericType = type.GetGenericTypeDefinition();

            if(genericType == typeof(PoolableDictionary<,>) ||
                genericType == typeof(PoolableList<>) ||
                genericType == typeof(PoolableHashSet<>)
            ) {
                return () => {
                    try {
                        return Activator.CreateInstance(type, true)!;
                    } catch {
                        return Activator.CreateInstance(type)!;
                    }
                };
            }

            try {
                return () => Activator.CreateInstance(type, true)!;
            } catch {
                return null;
            }
        }

        return null;
    }

    // Create Reset Action
    private static Action<object>? CreateResetAction(Type type) {
        if(typeof(IPoolable).IsAssignableFrom(type)) {
            return (item) => ((IPoolable)item).Reset();
        }

        return null;
    }

    /**
     *
     * Inject
     * 
     */
    public static void Inject(object target) {
        if(target == null) return;
        if(injected.ContainsKey(target)) return;

        var type = target.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach(var field in fields) ProcessField(target, field);
        foreach(var prop in properties) ProcessProperty(target, prop);

        injected[target] = true;
    }
}