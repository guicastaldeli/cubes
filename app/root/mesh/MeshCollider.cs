/**

    Helper Mesh Collider class of Meshes
    to help detect the collider type...

    */
namespace App.Root.Mesh;
using App.Root.Collider;
using App.Root.Collider.Types;

static class Types {
    public const string CUBE = "cube";
    public const string SPHERE = "sphere";
    public const string TRIANGLE = "triangle";
}

static class MeshCollider {
    public static Mesh mesh = null!;
    public static MeshData data = null!;
    public static CollisionManager collisionManager = null!;

    /**

        Init

        */
    public static void init(Mesh mesh, CollisionManager collisionManager) {
        MeshCollider.mesh = mesh;
        MeshCollider.collisionManager = collisionManager; 
    }

    /**

        Update

        */
    public static void update(MeshData data, string id) {
        if(data == null) return;

        switch(data.colliderShape) {
            case Types.CUBE:
                collisionManager.addStaticCollider(new StaticObject(mesh.getBBox(id), id));
                break;
            case Types.SPHERE:
                collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
                break;
            case Types.TRIANGLE:
                collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
                break;
        }

        return;
    }
}