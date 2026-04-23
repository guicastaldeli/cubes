namespace App.Root.ServerData;

class ServerDataManager {
    private Server server;

    private ServerJoin serverJoin;
    private ServerLeave serverLeave;
    private ServerPing serverPing;
    private ServerPlayerData serverPlayerData;
    private ServerChat serverChat;
    private ServerVoice serverVoice;
    private ServerMeshUpdate serverMeshUpdate;

    public ServerDataManager(Server server) {
        this.server = server;
        
        this.serverJoin = new ServerJoin(server);
        this.serverLeave = new ServerLeave(server);
        this.serverPing = new ServerPing(server);
        this.serverPlayerData = new ServerPlayerData(server);
        this.serverChat = new ServerChat(server);
        this.serverVoice = new ServerVoice(server);
        this.serverMeshUpdate = new ServerMeshUpdate(server);
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

    // Get Server Data
    public ServerPlayerData getServerPlayerData() {
        return serverPlayerData;
    }

    // Get Server Chat
    public ServerChat getServerChat() {
        return serverChat;
    }

    // Get Server Voice
    public ServerVoice getServerVoice() {
        return serverVoice;
    }

    // Get Server Mesh Update
    public ServerMeshUpdate getServerMeshUpdate() {
        return serverMeshUpdate;
    }
}