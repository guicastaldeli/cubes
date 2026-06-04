/**

    Platform Registry main class

    */
namespace App.Root.World.Platform;
using App.Root.Collider;
using App.Root.Player;
using App.Root.Utils;
using App.Root.Mesh;

class PlatformRegistry {
    /**

        Platform Registry Handler

        */
    public abstract class PlatformRegistryHandler {
        public virtual void render() {}
        public virtual void update() {}
    }
    
    /**

        Platform Registry main

        */
    private Window window;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private Platform platform;
    private PlayerController playerController;

    private ServiceContainer ServiceContainer = new ServiceContainer();
    private bool isRegistryed = false;

    private List<PlatformRegistryHandler> el = new();

    public PlatformRegistry(
        Window window,
        Mesh mesh, 
        CollisionManager collisionManager, 
        Platform platform,
        PlayerController playerController
    ) {
        this.window = window;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.platform = platform;
        this.playerController = playerController;

        ServiceContainer.Register(window);
        ServiceContainer.Register(mesh);
        ServiceContainer.Register(collisionManager);
        ServiceContainer.Register(platform);
        ServiceContainer.Register(playerController);
    }

    /**
    
        Init
    
        */
    public void init() {
        if(isRegistryed) return;

        var registry = new ClassRegistry(ServiceContainer);
        registry.PRegister<WorldHandler, PlatformRegistryHandler>(result => {
            el = result;
        });

        isRegistryed = true;
    }

    /**
    
        Render
    
        */
    public void render() {
        foreach(var e in el) e.render();
    }

    /**
    
        Update
    
        */
    public void update() {
        foreach(var e in el) e.update();
    }
}