namespace App.Root.World;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Physics;
using App.Root.Player;
using App.Root.Shaders;
using App.Root.Utils;
using App.Root.Mesh;

[ClassRegistryIgnore]
class World : WorldHandler {
    private Window window;
    private Tick tick;
    private ShaderProgram shaderProgram;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private TimeCycle timeCycle;
    private Camera camera;

    private WorldManager worldManager;
    private WorldBoundary worldBoundary;

    private ServiceContainer ServiceContainer = new ServiceContainer();
    private bool isRegistered = false;
    
    private List<WorldHandler> el = new();

    public const float WORLD_BOUNDARY = 25.0f;
    
    public World(
        Window window,
        Tick tick,
        ShaderProgram shaderProgram,
        Mesh mesh, 
        WorldManager worldManager,
        CollisionManager collisionManager,
        TimeCycle timeCycle,
        Camera camera,
        PlayerController playerController
    ) {
        this.window = window;
        this.tick = tick;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
        this.worldManager = worldManager;
        this.collisionManager = collisionManager;
        this.timeCycle = timeCycle;
        this.camera = camera;

        ServiceContainer.Register(window);
        ServiceContainer.Register(tick);
        ServiceContainer.Register(mesh);
        ServiceContainer.Register(shaderProgram);
        ServiceContainer.Register(worldManager);
        ServiceContainer.Register(collisionManager);
        ServiceContainer.Register(timeCycle);
        ServiceContainer.Register(camera);
        ServiceContainer.Register(playerController);
        ServiceContainer.SRegister(this);

        WorldUpdater.getInstance().init(window, mesh, collisionManager);
        this.worldBoundary = new WorldBoundary(
            worldManager.getPlayerController(),
            worldManager.getPlayerController().getRigidBody(),
            collisionManager
        );

        PhysicsRegistry.getInstance().init(mesh, collisionManager);

        Register();
        
        setBoundary();
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

    public void setBoundary() {
        var boundary = new BoundaryObject(WORLD_BOUNDARY);
        collisionManager.addStaticCollider(boundary);
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
        foreach(var e in el) e.render();
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        foreach(var e in el) e.update();
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

        var registry = new ClassRegistry(ServiceContainer);
        el = registry.ORegister<WorldHandler>();

        isRegistered = true;
    }
}