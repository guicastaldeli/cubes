/**

    Platform Entity main class

    */
namespace App.Root.World.Platform;
using App.Root.Collider;
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

    private List<PlatformEntityHandler> el = new();

    public PlatformEntity(Mesh.Mesh mesh, CollisionManager collisionManager, Platform platform) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.platform = platform;
    }

    /**
    
        Init
    
        */
    public void init() {
        Console.WriteLine("YTESTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT");
        var baseType = typeof(PlatformEntityHandler);
        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsSubclassOf(baseType)
            );

        foreach(var type in types) {
        }
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