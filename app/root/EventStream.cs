/**

    Event Stream System to emit
    and get events from everywhere...

    */
namespace App.Root;

static class EventStream {
    private static Dictionary<string, object> events = new();
    private static Dictionary<string, List<Action<object>>> subscribers = new();

    /**
    
        Set
    
        */
    public static void set(string streamId, object data) {
        events[streamId] = data;

        if(subscribers.TryGetValue(streamId, out var handlers)) {
            foreach(var handler in handlers) {
                handler(data);
            }
        }
    }

    public static void set<T>(string streamId, Dictionary<string, T> incoming) where T : class {
        var existing = get<Dictionary<string, T>>(streamId) ?? new();
        
        foreach(var (k, v) in incoming) {
            existing[k] = v;
        }

        set(streamId, (object)existing);
    }

    /**
    
        Get
    
        */
    public static T? get<T>(string streamId) where T : class {
        if(events.TryGetValue(streamId, out var data)) {
            T? val = data as T;
            return val;
        }

        return null;
    }

    /**
    
        On
    
        */
    public static void on(string streamId, Action<object> handler) {
        if(!subscribers.ContainsKey(streamId)) {
            subscribers[streamId] = new List<Action<object>>();
        }

        subscribers[streamId].Add(handler);
    }

    /**
    
        Select
    
        */
    public static TResult? select<T, TResult>(string streamId, Func<T, TResult> selector) where T : class {
        T? data = get<T>(streamId);
        if(data == null) return default;
        return selector(data);
    }

    /**
    
        Update
    
        */
    public static void update(string streamId, Action<dynamic> updater) {
        if(!events.TryGetValue(streamId, out var data)) return;

        updater((dynamic)data);
        
        if(subscribers.TryGetValue(streamId, out var handlers)) {
            foreach(var handler in handlers) {
                handler(data);
            }
        }
    }

    /**
    
        Remove
    
        */
    public static void remove(string streamId) {
        events.Remove(streamId);
    }
}