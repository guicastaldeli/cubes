namespace App.Root.ClientData;

class ClientDataManager {
    private ClientJoin clientJoin;
    private ClientWorld clientWorld;

    private Client client;

    public ClientDataManager(Client client) {
        this.client = client;

        this.clientJoin = new ClientJoin(client);
        this.clientWorld = new ClientWorld(client);
    } 

    // Get Client Join
    public ClientJoin getClientJoin() {
        return clientJoin;
    }

    // Get Client World
    public ClientWorld getClientWorld() {
        return clientWorld;
    }
}