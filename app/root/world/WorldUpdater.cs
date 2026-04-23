

namespace App.Root.World;
using App.Root;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.Packets;
using App.Root.Physics;
using OpenTK.Mathematics;

/**

    World Updater class
    to update all world-related
    things for Multiplayer.

    */
class WorldUpdater {
    private static WorldUpdater? instance;

    private Window? window;
    private Mesh? mesh;
    private CollisionManager? collisionManager;
    private Server? server;
    private Client? client;
    
    public static WorldUpdater getInstance() {
        if(instance == null) {
            instance = new WorldUpdater();
        }
        return instance;
    }

    /**

        Init
    
        */
    public void init(
        Window window,
        Mesh mesh, 
        CollisionManager collisionManager
    ) {
        this.window = window;
        this.mesh = mesh;
        this.collisionManager = collisionManager;

        MeshCollider.init(mesh, collisionManager);
    }

    // Set Client
    public void setClient(Client client) {
        this.client = client;
    }

    // Set Server
    public void setServer(Server server) {
        this.server = server;
    }

    /**
    
        Mesh
    
        */
    ///
    /// Add
    /// 
    public void addMesh(
        string id,
        string meshType,
        Vector3 position,
        Vector3 scale,
        int texId,
        string texPath
    ) {
        applyAddMesh(id, meshType, position, scale, texId, texPath);

        var packet = new PacketMeshUpdate {
            action = MeshAction.ADD,
            meshId = id,
            meshType = meshType,
            x = position.X, y = position.Y, z = position.Z,
            scaleX = scale.X, scaleY = scale.Y, scaleZ = scale.Z,
            texId = texId,
            texPath = texPath
        };

        broadcast(packet);
    }

    public void applyAddMesh(
        string id,
        string meshType,
        Vector3 position,
        Vector3 scale,
        int texId,
        string texPath
    ) {
        if(window == null || mesh == null || collisionManager == null) return;

        window.queueOnRenderThread(() => {
            MeshData data = MeshLoader.load(meshType);
            mesh.add(id, data);
            mesh.setPosition(id, position);
            mesh.setScale(id, scale);

            if(texId > 0) mesh.setTexture(id, texId, texPath);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInstanced = true;

            MeshCollider.update(data, id);

            if(!PhysicsRegistry.getInstance().has(id)) {
                PhysicsRegistry.getInstance().register(id, data, Type.DYNAMIC);
            }

            MeshInteractionRegistry.getInstance().register(
                id,
                State.BREAKABLE,
                mesh
            );
        });
    }
    
    ///
    /// Remove
    /// 
    public void removeMesh(string id) {
        applyRemoveMesh(id);

        var packet = new PacketMeshUpdate {
            action = MeshAction.REMOVE,
            meshId = id
        };

        broadcast(packet);
    }

    public void applyRemoveMesh(string id) {
        if(window == null || mesh == null || collisionManager == null) return;

        window.queueOnRenderThread(() => {
            mesh.remove(id);
            collisionManager.removeCollider(id);
        });

        PhysicsRegistry.getInstance().unregister(id);
        MeshInteractionRegistry.getInstance().unregister(id);
        MeshRegistry.unregister(id);
    }

    /**
    
        Broadcast
    
        */
    public void broadcast(PacketMeshUpdate packet) {
        if(server != null) {
            foreach(var player in server.players.Values) {
                server.send(packet, player.endPoint);
            }
        } else {
            if(client != null) {
                client.send(packet);
            }
        }
    }
}