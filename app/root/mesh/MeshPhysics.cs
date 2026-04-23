/**

    Helper Mesh Physics class of Meshes
    to help detect the physics type...

    */
namespace App.Root.Mesh;
using App.Root.Physics;

static class MeshPhysics {
    /**

        Update

        */
    public static void update(MeshData data, string id, Type? type) {
        if(data == null) return;

        PhysicsRegistry physicsRegistry = PhysicsRegistry.getInstance();

        switch(type) {
            case Type.DYNAMIC:
                physicsRegistry.register(id, data, Type.DYNAMIC);
                break;
            case Type.RECEIVER:
                physicsRegistry.register(id, data, Type.RECEIVER);
                break;
        }

        return;
    }
}