namespace App.Root.Player;
using System.Net;

class PlayerData {
    public string id {
        get;
    }

    public IPEndPoint endPoint {
        get;
    }

    public float x {
        get;
        set;
    }

    public float y {
        get;
        set;
    }

    public float z {
        get;
        set;
    }

    public float yaw {
        get;
        set;
    }

    public float pitch {
        get;
        set;
    }

    public long lastPing {
        get;
        set;
    }


    public PlayerData(string id, IPEndPoint endPoint) {
        this.id = id;
        this.endPoint = endPoint;
        this.lastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public void updateState(
        float x,
        float y,
        float z,
        float yaw,
        float pitch
    ) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.yaw = yaw;
        this.pitch = pitch;
    }

    public bool isTimedOut() {
        long timeoutMs = 5000;
        bool time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastPing > timeoutMs;
        return time;
    }

    public void updatePing() {
        lastPing= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}