namespace App.Root.Chunk;

public class Pool<T> where T : class, new() {
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

    public Pool(
        int initialSize = 32,
        int maxSize = 256,
        Func<T>? factory = null,
        Action<T>? resetAction = null
    ) {
        this.maxSize = maxSize;
        this.factory = factory ?? (() => new T());
        this.resetAction = resetAction ?? (() => new());

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