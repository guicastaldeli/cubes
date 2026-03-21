namespace App.Root;

abstract class NetworkUpdateHandler {
    public virtual void update() {}
}

class NetworkUpdate {
    private static NetworkUpdate? instance;

    public static NetworkUpdate getInstance() {
        instance ??= new NetworkUpdate();
        return instance;
    }

    private List<NetworkUpdateHandler> handlers = new();

    // Register
    public static void register(NetworkUpdateHandler handler) {
        getInstance().handlers.Add(handler);
    }

    // Update
    public void update() {
        foreach(var handler in handlers) handler.update();
    }

    // Clear
    public static void clear() {
        getInstance().handlers.Clear();
    }
}