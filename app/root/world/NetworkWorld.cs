namespace App.Root.World;

using System.Text.Json;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Player;
using App.Root.Resource;
using OpenTK.Mathematics;

class NetworkWorld : NetworkUpdateHandler {
    private WorldManager worldManager;
    private Network? network;

    public NetworkWorld(WorldManager worldManager) {
        this.worldManager = worldManager;
        NetworkUpdate.register(this);
    }

    public void setNetwork(Network network) {
        this.network = network;
    }
    
    public override void update() {
        if(network == null) {
            Console.WriteLine("NetworkWorld: network is null");
            return;
        }
        if(network.isHost()) return;

        Mesh mesh = worldManager.getWorld().getMesh();

        var snapshot = network.getCachedSnapshot();
        if(snapshot == null) {
            //Console.WriteLine("Snapshot: snaphot is null");
            return;
        }

        //Console.WriteLine($"Client received snapshot with {snapshot.data.Count} types");
        foreach(var (type, list) in snapshot.data) {
            //Console.WriteLine($"client type: {type}, count: {list.Count}");
        }

        Data.getInstance().apply(snapshot, DataType.MESH, entry => {
            string? id = entry["id"] as string;
            if(string.IsNullOrEmpty(id)) return;
            if(MeshRegistry.isRuntime(id)) return;
            //Console.WriteLine($"NetworkWorld: processing mesh id={id}");

            bool instanced = entry.ContainsKey("isInstanced") && Convert.ToBoolean(entry["isInstanced"]);
            if(instanced) {
                if(!mesh.hasMesh(id)) {
                    List<Vector3> positions = new();
                    var rawEntry = entry["instancePositions"];
                    
                    if(rawEntry is string jsonStr) {
                        var parsed = JsonSerializer.Deserialize<List<Dictionary<string, float>>>(jsonStr);
                        if(parsed == null) return;

                        positions = parsed.Select(p => new Vector3(
                            p["x"], 
                            p["y"], 
                            p["z"]
                        )).ToList();
                    } else if(rawEntry is List<object> rawList) {
                        positions = rawList
                            .Cast<Dictionary<string, object>>()
                            .Select(p => new Vector3(
                                Convert.ToSingle(p["x"]),
                                Convert.ToSingle(p["y"]),
                                Convert.ToSingle(p["z"])
                            )).ToList();
                    } else {
                        Console.WriteLine($"instancePositions unexpected type: {rawEntry?.GetType()}");
                        return;
                    }

                    worldManager.getWindow().queueOnRenderThread(() => {
                        string meshType = entry.ContainsKey("meshType") ?
                            Convert.ToString(entry["meshType"]) ?? id : id;
                        mesh.add(id, meshType);

                        var renderer = mesh.getMeshRenderer(id);
                        if(renderer != null) {
                            renderer.isInstanced = true;
                            renderer.setInstancePositions(positions);
                        }
                    });
                }

                return;
            }

            float x = Convert.ToSingle(entry["x"]);
            float y = Convert.ToSingle(entry["y"]);
            float z = Convert.ToSingle(entry["z"]);

            Matrix4? rotation = null;
            if(entry.ContainsKey("r00")) {
                rotation = new Matrix4(
                    Convert.ToSingle(entry["r00"]), Convert.ToSingle(entry["r01"]), Convert.ToSingle(entry["r02"]), 0,
                    Convert.ToSingle(entry["r10"]), Convert.ToSingle(entry["r11"]), Convert.ToSingle(entry["r12"]), 0,
                    Convert.ToSingle(entry["r20"]), Convert.ToSingle(entry["r21"]), Convert.ToSingle(entry["r22"]), 0,
                    0, 0, 0, 1
                );
            }

            if(!mesh.hasMesh(id)) {
                worldManager.getWindow().queueOnRenderThread(() => {
                    mesh.add(id, PlayerMesh.PLAYER_MESH);
                    mesh.setNetworkControlled(id, true);
                    mesh.setPosition(id, x, y, z);
                    if(rotation.HasValue) mesh.setRotationMatrix(id, rotation.Value);
                    if(entry.ContainsKey("texId")) {
                        int texId = Convert.ToInt32(entry["texId"]);
                        if(texId > 0) mesh.setTexture(id, texId);
                    }
                    if(entry.ContainsKey("texPath")) {
                        string texPath = Convert.ToString(entry["texPath"]) ?? "";
                        if(!string.IsNullOrEmpty(texPath)) {
                            int texId = TextureLoader.load(texPath);
                            mesh.setTexture(id, texId);
                        }
                    }

                    var bbox = mesh.getBBox(id);
                    worldManager.getCollisionManager()?.addStaticCollider(new StaticObject(bbox, id));
                    worldManager.getCollisionManager()?.addStaticCollider(new BoundaryObject(World.WORLD_BOUNDARY));
                });
            } else {
                mesh.setPosition(id, x, y, z);
                if(rotation.HasValue) mesh.setRotationMatrix(id, rotation.Value);
            }
        });
    }
}