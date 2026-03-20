namespace App.Root.ServerData;
using System.Collections.Concurrent;
using System.Net;
using App.Root.Player;

class ServerDataManager {
    private Server server;
    private ServerJoin serverJoin;
    private ServerLeave serverLeave;
    private ServerPing serverPing;
    private ServerState serverState;
    private ServerWorldData serverWorldData;

    public ServerDataManager(Server server) {
        this.server = server;
        this.serverJoin = new ServerJoin(server);
        this.serverLeave = new ServerLeave(server);
        this.serverPing = new ServerPing(server);
        this.serverState = new ServerState(server);
        this.serverWorldData = new ServerWorldData(server);
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

    // Get Server World Data
    public ServerWorldData getServerWorldData() {
        return serverWorldData;
    }
}