namespace App.Root.ServerData;

class ServerDataManager {
    private Server server;
    private ServerJoin serverJoin;
    private ServerLeave serverLeave;
    private ServerPing serverPing;
    private ServerPlayerState serverPlayerState;
    private ServerChat serverChat;

    public ServerDataManager(Server server) {
        this.server = server;
        
        this.serverJoin = new ServerJoin(server);
        this.serverLeave = new ServerLeave(server);
        this.serverPing = new ServerPing(server);
        this.serverPlayerState = new ServerPlayerState(server);
        this.serverChat = new ServerChat(server);
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
    public ServerPlayerState getServerPlayerState() {
        return serverPlayerState;
    }

    // Get Server Chat
    public ServerChat getServerChat() {
        return serverChat;
    }
}