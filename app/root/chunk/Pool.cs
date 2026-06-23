namespace App.Root.Chunk;

/**

    Pool utils...

    */
public interface IPool {
    int Available { get; }
    int TotalCreated { get; }
    void Clear();
    object GetObject();
    void ReturnObject(object item);
}

public interface IPool<T> : IPool where T : class {
    T Get();
    void Return(T item);
}

public interface IPoolable {
    void Reset();
}

public class PoolableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IPoolable where TKey : notnull {
    public void Reset() => Clear();
}

public class PoolableList<T> : List<T>, IPoolable {
    public void Reset() => Clear();
}

public class PoolableHashSet<T> : HashSet<T>, IPoolable {
    public void Reset() => Clear();
}

public class PoolableStack<T> : Stack<T>, IPoolable {
    public void Reset() => Clear();
}

public class PoolableQueue<T> : Queue<T>, IPoolable {
    public void Reset() => Clear();
}

/**

    Pool class...

    */
public class Pool<T> : IPool where T : class, new() {
    private readonly Stack<T> pool = new Stack<T>();
    private readonly Func<T> factory;
    private readonly Action<T> resetAction;
    
    private readonly int maxSize;
    private int createCount = 0;
    
    private readonly object _lock = new object();

    public int Available {
        get {
            lock(_lock) {
                return pool.Count;
            }
        }
    }

    public int TotalCreated {
        get {
            lock(_lock) {
                return createCount;
            }
        }
    }

    int IPool.Available => Available;
    int IPool.TotalCreated => TotalCreated;
    void IPool.Clear() => Clear();
    object IPool.GetObject() => Get();
    void IPool.ReturnObject(object item) => Return((T)item);

    public Pool(
        int initialSize = 32,
        int maxSize = 256,
        Func<T>? factory = null,
        Action<T>? resetAction = null
    ) {
        this.maxSize = maxSize;
        this.factory = factory ?? (() => new T());
        this.resetAction = resetAction ?? ((T item) => {});

        for(int i = 0; i < initialSize; i++) {
            var item = this.factory();
            this.pool.Push(item);
            this.createCount++;
        }
    }

    /**
     *
     * Get
     *
     */
    public T Get() {
        lock(_lock) {
            if(pool.Count > 0) {
                return pool.Pop();
            }
        }

        if(createCount < maxSize) {
            createCount++;
            return factory();
        }

        return factory();
    }

    /**
     *
     * Return
     *
     */
    public void Return(T item) {
        if(item == null) return;

        lock(_lock) {
            if(pool.Count < maxSize) {
                resetAction.Invoke(item);
                pool.Push(item);
            }
        }
    }

    /**
     *
     * Clear
     *
     */
    public void Clear() {
        lock(_lock) {
            pool.Clear();
            createCount = 0;
        }
    }
}