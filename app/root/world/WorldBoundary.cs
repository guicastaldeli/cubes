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

    public const float WORLD_BOUNDARY = 250.0f;

    public WorldBoundary(PlayerController playerController, RigidBody rigidBody, CollisionManager collisionManager) {
        this.playerController = playerController;
        this.rigidBody = rigidBody;
        this.collisionManager = collisionManager;
    }

    // Get Boundary Object
    public BoundaryObject? getBoundaryObject() {
        BoundaryObject? val = 
            collisionManager.getColliders()
            .OfType<BoundaryObject>()
            .FirstOrDefault();
        return val;
    }

    /**
     * 
     * Apply
     *
     */
    public void apply() {
        Vector3 pos = rigidBody.getPosition();
        
        var boundary = getBoundaryObject();
        if(boundary == null || !boundary.isActive()) return;
        
        boundary.setCenter(new Vector3(pos.X, 0.0f, pos.Z));

        float minHeight = boundary.getMinHeight();
        float maxHeight = boundary.getMaxHeight();

        if(pos.Y <= minHeight) {
            Vector3 spawn = playerController.setSpawnProps();
            playerController.setPosition(spawn.X, spawn.Y, spawn.Z);

            Console.WriteLine($"[WorldBoundary] spawned at {spawn}");
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