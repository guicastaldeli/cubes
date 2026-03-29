namespace App.Root.World;

using App.Root.Player;
using OpenTK.Mathematics;

class WorldBoundary {
    private PlayerController playerController;
    private RigidBody rigidBody;

    private float floor = 10.0f;
    private float ceiling = 10.0f; 

    public WorldBoundary(PlayerController playerController, RigidBody rigidBody) {
        this.playerController = playerController;
        this.rigidBody = rigidBody;
    }

    // Apply
    public void apply() {
        Vector3 pos = rigidBody.getPosition();
        Vector3? spawn = Platform.Platform.height;

        if(pos.Y <= floor && spawn.HasValue) {
            playerController.setPosition(
                spawn.Value.X,
                spawn.Value.Y,
                spawn.Value.Z
            );
        }
        if(pos.Y >= ceiling) {
            pos.Y = ceiling;
            rigidBody.setPosition(pos);
            rigidBody.setVelocity(new Vector3(
                rigidBody.getVelocity().X,
                0.0f,
                rigidBody.getVelocity().Z
            ));
        }
    }
}