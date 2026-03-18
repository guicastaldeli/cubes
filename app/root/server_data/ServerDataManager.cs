namespace App.Root.ServerData;
using System.Collections.Concurrent;
using System.Net;
using App.Root.Player;

class ServerDataManager {
    private Server server;
    private ConcurrentDictionary<string, PlayerData> players;
    private int maxPlayers;

    private ServerJoin serverJoin;
    private ServerLeave serverLeave;
    private ServerPing serverPing;
    private ServerState serverState;

    public ServerDataManager(
        Server server,
        ConcurrentDictionary<string, PlayerData> players,
        int maxPlayers
    ) {
        this.server = server;
        this.players = players;
        this.maxPlayers = maxPlayers;

        this.serverJoin = new ServerJoin(server, players, maxPlayers);
        this.serverLeave = new ServerLeave(server, players);
        this.serverPing = new ServerPing(server, players);
        this.serverState = new ServerState(server, players);
    }

    // Get Server Join
    public ServerJoin getServerJoin() {
        return serverJoin;
    }

    // Get Server Leave
    public ServerLeave getServerLeave() {
        return serverLeave;
    }

    // Get Server Ping
    public ServerPing getServerPing() {
        return serverPing;
    }

    // Get Server State
    public ServerState getServerState() {
        return serverState;
    }
}