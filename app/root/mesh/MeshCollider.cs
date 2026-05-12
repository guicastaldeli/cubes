/**

    Helper Mesh Collider class of Meshes
    to help detect the collider type...

    */
namespace App.Root.Mesh;
using App.Root.Collider;
using App.Root.Collider.Types;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;

/**

    Collider Type

    */
static class ColliderType {
    public const string CUBE = "cube";
    public const string SPHERE = "sphere";
    public const string TRIANGLE = "triangle";
}

/**

    Mesh Collider main class

    */
static class MeshCollider {
    public static Mesh mesh = null!;
    public static MeshData data = null!;
    public static CollisionManager collisionManager = null!;

    private static Dictionary<string, string> colliderTypes = new();
    private static Dictionary<string, Vector3> cachedSize = new();

    private static Dictionary<string, Vector3> instancedPositions = new();
    private static Dictionary<string, float> instancedScales = new();
    private static Dictionary<string, string> instancedMeshTypes = new();
    private static Dictionary<string, BBox> instancedBBoxes = new();

    private static Dictionary<string, float[]> cachedVertices = new();
    private static Dictionary<string, int[]> cachedIndices = new();

    // Position
    public static Vector3 getInstancedPosition(string colliderId) {
        Vector3 val = 
            instancedPositions.TryGetValue(colliderId, out Vector3 pos) ? 
                pos : 
                Vector3.Zero;

        return val;
    }

    // Size
    public static Vector3 getInstancedSize(string colliderId) {
        if(instancedMeshTypes.TryGetValue(colliderId, out string? meshType) &&
            instancedScales.TryGetValue(colliderId, out float scale) &&
            cachedSize.TryGetValue(meshType, out Vector3 size)) {
            return size * scale;
        }

        return Vector3.One;
    }

    /**
    
        Cached
    
        */
    // Cache Mesh Data
    private static void cacheMeshData(string meshType, MeshData data) {
        if(cachedVertices.ContainsKey(meshType)) return;
        
        float[]? verts = data.getVertices();
        int[]? inds = data.getIndices();
        if(verts != null) cachedVertices[meshType] = verts.ToArray();
        if(inds != null) cachedIndices[meshType] = inds.ToArray();

        getCachedSize(meshType);
    }

    // Get Cached Size
    public static Vector3 getCachedSize(string meshId) {
        if(cachedSize.TryGetValue(meshId, out Vector3 size)) return size;

        BBox bbox = mesh.getBBox(meshId);
        size = new Vector3(
            bbox.getSizeX(), 
            bbox.getSizeY(), 
            bbox.getSizeZ()
        );
        cachedSize[meshId] = size;

        return size;
    }

    /**

        Init

        */
    public static void init(Mesh mesh, CollisionManager collisionManager) {
        MeshCollider.mesh = mesh;
        MeshCollider.collisionManager = collisionManager; 
    }

    /**

        Set

        */
    public static void setInstanced(
        MeshData data, 
        string colliderId,
        Vector3 position, 
        float scale, 
        string meshType
    ) {
        if(data?.colliderShape == null) return;

        colliderTypes[colliderId] = data.colliderShape;
        instancedPositions[colliderId] = position;
        instancedScales[colliderId] = scale;
        instancedMeshTypes[colliderId] = meshType;

        Vector3 size = getCachedSize(meshType) * scale;

        instancedBBoxes[colliderId] = BBox.setFromCenterI(position, size);

        switch(data.colliderShape) {
            case ColliderType.CUBE:
                collisionManager.addStaticCollider(new StaticObject(() => instancedBBoxes[colliderId], colliderId));
                break;
            case ColliderType.SPHERE:
                collisionManager.addStaticCollider(
                    new SphereObject(
                        () => getInstancedPosition(colliderId),
                        () => getInstancedSize(colliderId),
                        colliderId
                    )
                );
                break;
            case ColliderType.TRIANGLE:
                cacheMeshData(meshType, data);
                collisionManager.addStaticCollider(
                    new TriangleObject(
                        () => getInstancedPosition(colliderId),
                        () => cachedVertices[meshType],
                        () => cachedIndices[meshType],
                        () => Vector3.One * instancedScales[colliderId],
                        () => instancedBBoxes[colliderId],
                        colliderId
                    )
                );
                break;
        }
    }

    /**

        Update

        */
    public static void update(MeshData data, string id) {
        if(data == null) return;

        switch(data.colliderShape) {
            case ColliderType.CUBE:
                collisionManager.addStaticCollider(new StaticObject(mesh.getBBox(id), id));
                break;
            case ColliderType.SPHERE:
                collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
                break;
            case ColliderType.TRIANGLE:
                collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
                break;
        }

        return;
    }

    public static void updateInstanced(string colliderId, Vector3 newPosition) {
        if(!instancedPositions.ContainsKey(colliderId)) return;
        instancedPositions[colliderId] = newPosition;

        if(instancedBBoxes.TryGetValue(colliderId, out BBox? bbox)) {
            string meshType = instancedMeshTypes[colliderId];
            float scale = instancedScales[colliderId];
            Vector3 size = cachedSize[meshType] * scale;
            bbox.setFromCenter(newPosition, size);
        }
    }

    /**
    
        Remove
    
        */
    public static void removeInstanced(string colliderId) {
        collisionManager.removeCollider(colliderId);
        colliderTypes.Remove(colliderId);
        instancedPositions.Remove(colliderId);
        instancedScales.Remove(colliderId);
        instancedMeshTypes.Remove(colliderId);
        instancedBBoxes.Remove(colliderId);
    }
}