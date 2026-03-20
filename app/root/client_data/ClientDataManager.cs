namespace App.Root.ClientData;

class ClientDataManager {
    private Client client;
    private ClientJoin clientJoin;
    private ClientWorld clientWorld;
    private ClientWorldData clientWorldData;

    public ClientDataManager(Client client) {
        this.client = client;
        this.clientJoin = new ClientJoin(client);
        this.clientWorld = new ClientWorld(client);
        this.clientWorldData = new ClientWorldData(client);
    } 

    // Get Client Join
    public ClientJoin getClientJoin() {
        return clientJoin;
    }

    // Get Client World
    public ClientWorld getClientWorld() {
        return clientWorld;
    }

    // Get Client World
    public ClientWorldData getClientWorldData() {
        return clientWorldData;
    }
}