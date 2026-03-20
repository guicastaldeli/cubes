namespace App.Root;

abstract class NetworkUpdateHandler {
    public virtual void update() {}
}

class NetworkUpdate {
    private List<NetworkUpdateHandler> handlers = new();

    public void update() {
        foreach(var handler in handlers) handler.update();
    }
}