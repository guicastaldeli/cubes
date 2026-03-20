namespace App.Root.Env.World;
using App.Root.Collider;
using System.Reflection;

class World : WorldHandler {
    private List<WorldHandler> el = new ();

    private Mesh.Mesh mesh;
    private CollisionManager collisionManager;
    
    public World(Mesh.Mesh mesh, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        register();
    } 

    public Mesh.Mesh getMesh() {
        return mesh;
    }

    ///
    /// Get
    /// 
    public T? get<T>() where T : WorldHandler {
        return el.OfType<T>().FirstOrDefault();
    }

    ///
    /// Register
    /// 
    private void register() {
        var baseType = typeof(WorldHandler);
        var excluded = new[] {
            typeof(WorldHandler),
            typeof(World),
            typeof(WorldManager)
        };

        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsSubclassOf(baseType) &&
                !excluded.Contains(t)
            );
        foreach(var type in types) {
            var ctor = type.GetConstructor(new[] {
                typeof(Mesh.Mesh),
                typeof(CollisionManager)
            });
            if(ctor != null) {
                var instance = (WorldHandler)ctor.Invoke(new object[] {
                    mesh,
                    collisionManager
                });
                el.Add(instance);
            }
        }
    }

    // Render
    public override void render() {
        foreach(var e in el) e.render();
    }

    // Update
    public override void update() {
        foreach(var e in el) e.update();
    }
}