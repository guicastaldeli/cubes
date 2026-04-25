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
    public static void update(MeshData data, string id, PhysicsType? type) {
        if(data == null) return;

        PhysicsRegistry physicsRegistry = PhysicsRegistry.getInstance();

        switch(type) {
            case PhysicsType.DYNAMIC:
                physicsRegistry.register(id, data, PhysicsType.DYNAMIC);
                break;
            case PhysicsType.RECEIVER:
                physicsRegistry.register(id, data, PhysicsType.RECEIVER);
                break;
        }

        return;
    }
}