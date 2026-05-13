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
    
        Remove
    
        */
    public static void remove(string streamId) {
        events.Remove(streamId);
    }
}