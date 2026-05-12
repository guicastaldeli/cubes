

/**

    Entity Collider helper class

    */
namespace App.Root.World.Entity;
using App.Root.Collider;
using App.Root.Mesh;
using OpenTK.Mathematics;

static class EntityCollider {
    public static Mesh? mesh = null!;
    public static CollisionManager? collisionManager = null!;

    public static Dictionary<string, List<string>> colliderIds = new();
    private static Dictionary<string, string> colliderToEntity = new();

    /**
    
        Resolve
    
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
        int lastUnder = colliderId.LastIndexOf('_');
        if(lastUnder >= 0 && int.TryParse(colliderId[(lastUnder + 1)..], out int idx)) {
            return idx;
        }
        return 0;
    }

    /**
    
        Init
    
        */
    public static void init(Mesh mesh, CollisionManager collisionManager) {
        EntityCollider.mesh = mesh;
        EntityCollider.collisionManager = collisionManager;

        MeshCollider.init(mesh, collisionManager);
    }

    /**
    
        Create
    
        */
    public static void create(EntityProps entity, List<Instance> list) {
        MeshData? data = mesh?.getData(entity.Id);
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
        }
    }

    /**
    
        Update
    
        */
    public static void update(string id, int i, Vector3 pos) {
        if(!colliderIds.ContainsKey(id)) return;
        if(i >= colliderIds[id].Count) return;

        string colliderId = colliderIds[id][i];
        MeshCollider.updateInstanced(colliderId, pos);
    }

    /**
    
        Cleanup
    
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
    }
}