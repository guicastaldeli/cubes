namespace App.Root.ClientData;

class ClientDataManager {
    private Client client;
    private ClientJoin clientJoin;
    private ClientData clientData;

    public ClientDataManager(Client client) {
        this.client = client;
        this.clientJoin = new ClientJoin(client);
        this.clientData = new ClientData(client);
    } 

    // Get Client Join
    public ClientJoin getClientJoin() {
        return clientJoin;
    }

    // Get Client Data
    public ClientData getClientData() {
        return clientData;
    }
}