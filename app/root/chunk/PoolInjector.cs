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
        if(PoolManager.HasPool(id)) return PoolManager.GetPoolIfExists<object>(id)!;

        var factory = CreateFactory(itemType);
        
        var resetAction = CreateResetAction(itemType);

        var poolType = typeof(Pool<>).MakeGenericType(itemType);
        var getPoolMethod = typeof(PoolManager).GetMethod(nameof(PoolManager.GetPool))!.MakeGenericMethod(itemType);
        var pool = getPoolMethod.Invoke(null, new object?[] {id, initialSize, maxSize, factory, resetAction });

        return (IPool)pool!;
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
        if(type.GetConstructor(Type.EmptyTypes) != null) return () => Activator.CreateInstance(type)!;

        if(type.IsGenericType) {
            var genericType = type.GetGenericTypeDefinition();
            
            if(genericType == typeof(Dictionary<,>)) {
                return () => {
                    var dict = (IDictionary)Activator.CreateInstance(type)!;
                    return dict;
                };
            }
            if(genericType == typeof(List<>)) {
                return () => {
                    var list = (IList)Activator.CreateInstance(type)!;
                    return list;
                };
            }
            if(genericType == typeof(HashSet<>).GetGenericTypeDefinition()) {
                return () => {
                    var set = Activator.CreateInstance(type)!;
                    return set;
                };
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