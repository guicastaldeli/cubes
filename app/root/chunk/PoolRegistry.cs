namespace App.Root.Chunk;

public static class PoolRegistry {
    private static readonly Dictionary<string, (IPool pool, object value)> registry = new();

    /**
     *
     * Get
     *
     */
    // Get Pool
    public static IPool? GetPool(string id) {
        IPool? val = registry.TryGetValue(id, out var entry) ? entry.pool : null;
        return val;
    }

    // Get Value
    public static object? GetValue(string id) {
        object? val = registry.TryGetValue(id, out var entry) ? entry.value : null;
        return val;
    }

    /**
     *
     * Return
     *
     */
    // Return
    public static void Return(string id) {
        if(registry.TryGetValue(id, out var entry)) {
            entry.pool.ReturnObject(entry.value);
            registry.Remove(id);
        }
    }

    // Return All
    public static void ReturnAll() {
        foreach(var (id, entry) in registry) {
            entry.pool.ReturnObject(entry.value);
        }
    }

    /**
     *
     * Has
     *
     */
    public static bool Has(string id) {
        bool val = registry.ContainsKey(id);
        return val;
    }

    /**
     *
     * Register
     *
     */
    public static void Register(string id, IPool pool, object value) {
        registry[id] = (pool, value);
    }
}