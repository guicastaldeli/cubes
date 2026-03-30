namespace App.Root.Player;
using System.Net;

class ServerPlayer {
    public static int SERVER_MAX_PLAYERS = 15;
    
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

    public long lastPing;

    public ServerPlayer(string id, IPEndPoint endPoint) {
        this.id = id;
        this.endPoint = endPoint;
        this.lastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    // Update Ping
    public void updatePing() {
        lastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    // Is Timed out
    public bool isTimedOut() {
        long time = 5000;
        bool date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastPing > time;
        return date;
    }
}