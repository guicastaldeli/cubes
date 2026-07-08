namespace App.Root.Chunk;

public static class PoolManager {
    private static readonly Dictionary<string, IPool> pools = new Dictionary<string, IPool>();
    private static readonly object _lock = new object();

    /**
     *
     * Register Pool
     *
     */
    public static void RegisterPool(string id, IPool pool) {
        lock(_lock) {
            if(!pools.ContainsKey(id)) {
                pools[id] = pool;
                Console.WriteLine($"[PoolManager] Registered pool '{id}'");
            }
        }
    }

    /**
     *
     * Get Pool
     *
     */
    // Get Pool
    public static Pool<T> GetPool<T>(
        string id,
        int initialSize = 32,
        int maxSize = 256,
        Func<T>? factory = null,
        Action<T>? resetAction = null
    ) where T : class, new() {
        lock(_lock) {
            if(!pools.TryGetValue(id, out var obj)) {
                obj = new Pool<T>(initialSize, maxSize, factory, resetAction);
                pools[id] = obj;
                Console.WriteLine($"[PoolManager] Created pool '{id}' for type {typeof(T).Name} (initial: {initialSize}, max: {maxSize})");
            }

            return (Pool<T>)obj;
        }
    }

    // Get Pool if Exists
    public static Pool<T>? GetPoolIfExists<T>(string id) where T : class, new() {
        lock(_lock) {
            if(pools.TryGetValue(id, out var obj)) {
                return (Pool<T>)obj;
            }

            return null;
        }
    }

    // Get Pool Direct
    public static IPool? DGetPool(string id) {
        lock(_lock) {
            IPool? val = pools.TryGetValue(id, out var pool) ? pool : null;
            return val;
        }
    }

    // Get Object
    public static object? GetObject(string id) {
        lock(_lock) {
            if(pools.TryGetValue(id, out var pool)) {
                return pool.GetObject();
            }

            return null;
        }
    }

    /**
     *
     * Has Pool
     *
     */
    public static bool HasPool(string id) {
        lock(_lock) {
            return pools.ContainsKey(id);
        }
    }

    /**
     *
     * Clear
     *
     */
    // Clear Pool
    public static void ClearPool(string id) {
        lock(_lock) {
            if(pools.TryGetValue(id, out var obj)) {
                obj.Clear();
                pools.Remove(id);
                Console.WriteLine($"[PoolManager] Cleared pool '{id}'");
            }
        }
    }

    // Clear All
    public static void ClearAll() {
        lock(_lock) {
            foreach(var (id, obj) in pools) {
                obj.Clear();
            }

            pools.Clear();
            Console.WriteLine("[PoolManager] Cleared all pools");
        }
    }

    /**
     *
     * Get Stats
     *
     */
    public static Dictionary<string, (int available, int total)> GetStats() {
        var stats = new Dictionary<string, (int, int)>();
        
        lock(_lock) {
            foreach(var (id, obj) in pools) {
                int available = obj.Available;
                int total = obj.TotalCreated;
                stats[id] = (available, total);
            }
        }

        return stats;
    }

    /**
     *
     * Return
     *
     */
    public static void ReturnObject(string id, object item) {
        lock(_lock) {
            if(pools.TryGetValue(id, out var pool)) {
                pool.ReturnObject(item);
            }
        }
    }
}