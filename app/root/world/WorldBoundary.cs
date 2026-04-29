/**

    World Boundary main class

    */
namespace App.Root.World;

using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Player;
using OpenTK.Mathematics;

class WorldBoundary {
    private PlayerController playerController;
    private RigidBody rigidBody;
    private CollisionManager collisionManager;

    public WorldBoundary(PlayerController playerController, RigidBody rigidBody, CollisionManager collisionManager) {
        this.playerController = playerController;
        this.rigidBody = rigidBody;
        this.collisionManager = collisionManager;
    }

    // Get Boundary Object
    public BoundaryObject? getBoundaryObject() {
        BoundaryObject? val = collisionManager.getColliders()
            .OfType<BoundaryObject>()
            .FirstOrDefault();
        return val;
    }

    /**
    
        Apply
    
        */
    public void apply() {
        Vector3 pos = rigidBody.getPosition();
        Vector3? spawn = Platform.Platform.height;
        
        var boundary = getBoundaryObject();
        if(boundary == null) return;

        float minHeight = boundary.getMinHeight();
        float maxHeight = boundary.getMaxHeight();

        if(pos.Y <= minHeight && spawn.HasValue) {
            playerController.setPosition(
                spawn.Value.X,
                spawn.Value.Y,
                spawn.Value.Z
            );
            return;
        }
        
        if(pos.Y >= maxHeight) {
            pos.Y = maxHeight;
            rigidBody.setPosition(pos);
            rigidBody.setVelocity(new Vector3(
                rigidBody.getVelocity().X,
                0.0f,
                rigidBody.getVelocity().Z
            ));
        }
    }
}