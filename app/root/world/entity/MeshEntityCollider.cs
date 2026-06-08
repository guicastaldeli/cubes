/**

    Mesh Entity Collider helper class

    */
namespace App.Root.World.Entity;
using App.Root.Collider;
using App.Root.Mesh;
using OpenTK.Mathematics;

static class MeshEntityCollider {
    private static Mesh? mesh = null!;
    private static CollisionManager? collisionManager = null!;
    private static MeshEntitySpawner? entitySpawner = null!;

    public static Dictionary<string, List<string>> colliderIds = new();
    public static Dictionary<string, string> colliderToEntity = new();

    private static Dictionary<string, Vector3> lastPositions = new();

    public static Action<string>? onEntityRemoved;

    private static bool eventsRegistered = false;

    /**
     * 
     * Init
     *
     */
    public static void init(
        Mesh mesh, 
        CollisionManager collisionManager,
        MeshEntitySpawner entitySpawner
    ) {
        MeshEntityCollider.mesh = mesh;
        MeshEntityCollider.collisionManager = collisionManager;
        MeshEntityCollider.entitySpawner = entitySpawner;

        MeshCollider.init(mesh, collisionManager);
    }

    /*
    
        On Events
    
        */
    public static void onEvents() {
        if(eventsRegistered) return;
        eventsRegistered = true;
        
        // Collider Remove
        EventStream.on("collider-remove", (data) => {
            if(data is string colliderId) {
                foreach(var (entityId, list) in colliderIds) {
                    int index = list.IndexOf(colliderId);
                    if(index >= 0) {
                        entitySpawner?.removeInstance(entityId, index);
                        break;
                    }
                }
                foreach(var list in colliderIds.Values) {
                    list.Remove(colliderId);
                }

                colliderToEntity.Remove(colliderId);
                MeshCollider.removeInstanced(colliderId);
            }
        });
    }

    /**
     * 
     * Resolve 
     *
     */
    // Entity Id
    public static string? resolveEntityId(string colliderId) {
        string? val = 
            colliderToEntity.TryGetValue(colliderId, out string? entityId) ? 
                entityId : 
                null;
        return val; 
    }

    // Instance Index
    public static int resolveInstanceIndex(string colliderId) {
        var stream = EventStream.get<Dictionary<string, List<string>>>("stream-id");
        if(stream == null) return 0;

        foreach(var (entityId, data) in stream) {
            int idx = data.IndexOf(colliderId);
            if(idx != -1) return idx;
        }

        return 0;
    }

    /**
     * 
     * Create
     *
     */
    public static void create(EntityProps entity, List<Instance> list) {
        MeshData? data = mesh?.getData(entity.MeshType);
        if(data?.colliderShape == null) return;

        if(mesh != null && !mesh.hasMesh(entity.MeshType)) {
            mesh.add(entity.MeshType, data);
        }
        if(!colliderIds.ContainsKey(entity.Id)) {
            colliderIds[entity.Id] = new List<string>();
        }

        for(int i = 0; i < list.Count; i++) {
            string id = $"{entity.Id}_{i}";
            Vector3 position = list[i].Position;

            MeshCollider.setInstanced(data, id, position, entity.Scale, entity.MeshType);
            colliderIds[entity.Id].Add(id);
            colliderToEntity[id] = entity.Id;
            
            MeshEntityFactory.setInteraction(data, entity, id);
        }
    }

    /**
     * 
     * Update
     *
     */
    public static void update(string id, int i, Vector3 pos) {
        if(!colliderIds.ContainsKey(id)) return;
        if(i >= colliderIds[id].Count) return;

        string colliderId = colliderIds[id][i];
        lastPositions[colliderId] = pos;
        MeshCollider.updateInstanced(colliderId, pos);
    }

    /**
     * 
     * Cleanup
     *
     */
    public static void cleanup() {
        if(mesh == null || collisionManager == null) return;

        foreach(var list in colliderIds.Values) {
            foreach(var id in list) {
                collisionManager.removeCollider(id);
                mesh.removeData(id);
            }
        }

        colliderIds.Clear();
        colliderToEntity.Clear();
        lastPositions.Clear();
    }

    public static void cleanupRemoved() {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        if(collisionManager == null || mesh == null) return;

        collisionManager.processRemovals();
        
        var toRemove = new List<string>();

        foreach(var (entityId, colliderList) in colliderIds) {
            bool anyAlive = false;

            for(int i = colliderList.Count - 1; i >= 0; i--) {
                string colliderId = colliderList[i];

                if(!collisionManager.colliderExists(colliderId)) {
                    colliderList.RemoveAt(i);
                    colliderToEntity.Remove(colliderId);
                    mesh.removeData(colliderId);
                } else {
                    anyAlive = true;
                }
            }

            if(!anyAlive) {
                toRemove.Add(entityId);
            }
        }

        var stream = EventStream.get<Dictionary<string, EntityProps>>("stream-props");
        foreach(var id in toRemove) {
            colliderIds.Remove(id);

            if(stream != null && stream.TryGetValue(id, out EntityProps? data)) {
                onEntityRemoved?.Invoke(data.MeshType);
                stream.Remove(id);
            }
        }

        collisionManager.clearRemoved();

        sw.Stop();
        if(sw.ElapsedMilliseconds > 1) Console.WriteLine($"cleanupRemoved: {sw.ElapsedMilliseconds}ms | colliders: {colliderIds.Count}");
    }
}