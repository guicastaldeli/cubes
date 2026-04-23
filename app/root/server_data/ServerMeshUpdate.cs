namespace App.Root.ServerData;
using App.Root;
using App.Root.Packets;
using App.Root.World;
using System.Net;
using OpenTK.Mathematics;

class ServerMeshUpdate : PacketHandler {
    private Server server;

    public ServerMeshUpdate(Server server) {
        this.server = server;
    } 

    // Get Type
    public PacketType getType() {
        return PacketType.MESH_UPDATE;
    }

    // Handle
    public void handle(string json, IPEndPoint remote) {
        var packet = Packet.deserialize<PacketMeshUpdate>(json);
        if(packet == null) return;

        var updater = WorldUpdater.getInstance();

        switch(packet.action) {
            case Action.PLACE:
                updater.applyPlaceMesh(
                    packet.meshId,
                    packet.meshType,
                    new Vector3(packet.x, packet.y, packet.z),
                    new Vector3(packet.scaleX, packet.scaleY, packet.scaleZ),
                    packet.texId,
                    packet.texPath
                );
                break;
            case Action.REMOVE:
                updater.applyRemoveMesh(packet.meshId);
                break;
        }

        foreach(var player in server.players.Values) {
            if(player.endPoint.Equals(remote)) continue;
            server.send(packet, player.endPoint);
        }
    }
}