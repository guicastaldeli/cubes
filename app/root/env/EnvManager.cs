using App.Root.Env.World;

namespace App.Root.Env;

class EnvManager {
    private WorldManager worldManager;

    public EnvManager() {
        this.worldManager = new WorldManager();
    }

    // Get World Manager
    public WorldManager getWorldManager() {
        return worldManager;
    }
}