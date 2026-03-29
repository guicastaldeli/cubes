namespace App.Root.ClientData;

class ClientDataManager {
    private Client client;
    private ClientJoin clientJoin;
    private ClientData clientData;
    private ClientChat clientChat;

    public ClientDataManager(Client client) {
        this.client = client;

        this.clientJoin = new ClientJoin(client);
        this.clientData = new ClientData(client);
        this.clientChat = new ClientChat(client);
    } 

    // Get Client Join
    public ClientJoin getClientJoin() {
        return clientJoin;
    }

    // Get Client Data
    public ClientData getClientData() {
        return clientData;
    }

    // Get Client Chat
    public ClientChat getClientChat() {
        return clientChat;
    }
}