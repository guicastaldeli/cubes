/**

    Platform Entity main class

    */
namespace App.Root.World.Platform;
using App.Root.Collider;
using App.Root.Player;
using App.Root.Utils;
using System.Reflection;

class PlatformEntity {
    /**

        Platform Entity Handler

        */
    public abstract class PlatformEntityHandler {
        public virtual void render() {}
        public virtual void update() {}
    }
    
    private Mesh.Mesh mesh;
    private CollisionManager collisionManager;
    private Platform platform;
    private PlayerController playerController;

    private ServiceContainer ServiceContainer = new ServiceContainer();
    private bool isRegistered = false;

    private List<PlatformEntityHandler> el = new();

    public PlatformEntity(
        Mesh.Mesh mesh, 
        CollisionManager collisionManager, 
        Platform platform,
        PlayerController playerController
    ) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.platform = platform;
        this.playerController = playerController;

        ServiceContainer.Register(mesh);
        ServiceContainer.Register(collisionManager);
        ServiceContainer.Register(platform);
        ServiceContainer.Register(playerController);
    }

    /**
    
        Init
    
        */
    public void init() {
        if(isRegistered) return;

        var registry = new ClassRegistry(ServiceContainer);
        el = registry.Register<PlatformEntityHandler>();

        isRegistered = true;
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