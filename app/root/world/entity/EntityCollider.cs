

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

    public static Dictionary<string, List<string>> instanceColliderIds = new();

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
        MeshData? meshData = mesh?.getData(entity.Id);
        if(meshData?.colliderShape == null) return;

        if(!instanceColliderIds.ContainsKey(entity.Id)) {
            instanceColliderIds[entity.Id] = new List<string>();
        }

        for(int i = 0; i < list.Count; i++) {
            string id = $"{entity.Id}_c_{i}";

            MeshData colliderData = EntityFactory.clone(meshData);
            colliderData.meshType = entity.MeshType;

            if(mesh == null) return;
            mesh.add(id, colliderData);
            mesh.setScale(id, entity.Scale);
            mesh.setPosition(id, list[i].Position);
            mesh.setVisible(id, false);

            MeshCollider.update(meshData, id);

            instanceColliderIds[entity.Id].Add(id);
        }
    }

    /**
    
        Update
    
        */
    public static void update(string id, int i, Vector3 pos) {
        if(!instanceColliderIds.ContainsKey(id)) return;
        if(i >= instanceColliderIds[id].Count) return;

        string colliderId = instanceColliderIds[id][i];
        if(mesh != null) mesh.setPosition(colliderId, pos);
    }

    /**
    
        Cleanup
    
        */
    public static void cleanup() {
        if(mesh == null || collisionManager == null) return;

        foreach(var (entityId, colliderIds) in instanceColliderIds) {
            foreach(var colliderId in colliderIds) {
                collisionManager.removeCollider(colliderId);
                mesh.remove(colliderId);
            }
        }

        instanceColliderIds.Clear();
    }
}