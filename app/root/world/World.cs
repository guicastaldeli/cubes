namespace App.Root.World;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Physics;
using App.Root.Player;
using App.Root.Shaders;
using App.Root.Utils;
using App.Root.Mesh;
using App.Root.Chunk;

[ManagedState]
[ClassRegistryIgnore]
class World : WorldHandler {
    private Window window;
    private Tick tick;
    private ShaderProgram shaderProgram;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private TimeCycle timeCycle;
    private Camera camera;
    private ChunkManager chunkManager;

    private WorldManager worldManager;
    private WorldBoundary worldBoundary;

    private ServiceContainer ServiceContainer = new ServiceContainer();
    private bool isRegistered = false;
    
    private List<WorldHandler> el = new();
    [SkipReset] List<WorldHandler> prevEl = new();
    
    public World(
        Window window,
        Tick tick,
        ShaderProgram shaderProgram,
        Mesh mesh, 
        WorldManager worldManager,
        CollisionManager collisionManager,
        TimeCycle timeCycle,
        Camera camera,
        PlayerController playerController,
        ChunkManager chunkManager
    ) {
        this.window = window;
        this.tick = tick;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
        this.worldManager = worldManager;
        this.collisionManager = collisionManager;
        this.timeCycle = timeCycle;
        this.camera = camera;
        this.chunkManager = chunkManager;

        ServiceContainer.Register(window);
        ServiceContainer.Register(tick);
        ServiceContainer.Register(mesh);
        ServiceContainer.Register(shaderProgram);
        ServiceContainer.Register(worldManager);
        ServiceContainer.Register(collisionManager);
        ServiceContainer.Register(timeCycle);
        ServiceContainer.Register(camera);
        ServiceContainer.Register(playerController);
        ServiceContainer.Register(chunkManager);
        ServiceContainer.SRegister(this);

        WorldUpdater.getInstance().init(window, mesh, collisionManager);
        this.worldBoundary = new WorldBoundary(
            worldManager.getPlayerController(),
            worldManager.getPlayerController().getRigidBody(),
            collisionManager
        );

        PhysicsRegistry.getInstance().init(mesh, collisionManager);

        StateManager.Register(this);

        Register();
    } 

    // Get Mesh
    public Mesh getMesh() {
        return mesh;
    }

    // Get World Manager
    public WorldManager getWorldManager() {
        return worldManager;
    }

    // Boundary
    public WorldBoundary getWorldBoundary() {
        return worldBoundary;
    }

    /**
     * 
     * Get
     *
     */
    public T? get<T>() where T : WorldHandler {
        return el.OfType<T>().FirstOrDefault();
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        if(!isRegistered) Register();
        chunkManager.render();
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        chunkManager.update();
        worldBoundary.apply();

        PhysicsRegistry.getInstance().update();
    }

    /**
     * 
     * Register
     *
     */
    private void Register() {
        if(isRegistered) return;

        foreach(var handler in prevEl) {
            StateManager.Unregister(handler);
        }

        var registry = new ClassRegistry(ServiceContainer);
        el = registry.ORegister<WorldHandler>();

        chunkManager.registerHandlers(el.Cast<ChunkHandler>().ToList());

        prevEl = new List<WorldHandler>(el);
        isRegistered = true;
    }
}