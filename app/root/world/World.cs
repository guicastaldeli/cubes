namespace App.Root.World;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Physics;
using App.Root.Shaders;
using App.Root.Utils;
using System.Reflection;

class World : WorldHandler {
    private ServiceContainer ServiceContainer = new ServiceContainer();
    private bool isRegistered = false;

    private List<WorldHandler> el = new();

    private Window window;
    private Tick tick;
    private ShaderProgram shaderProgram;
    private Mesh.Mesh mesh;
    private CollisionManager collisionManager;
    private TimeCycle timeCycle;

    private WorldManager worldManager;
    private WorldBoundary worldBoundary;

    public const float WORLD_BOUNDARY = 25.0f;
    
    public World(
        Window window,
        Tick tick,
        ShaderProgram shaderProgram,
        Mesh.Mesh mesh, 
        WorldManager worldManager,
        CollisionManager collisionManager,
        TimeCycle timeCycle
    ) {
        this.window = window;
        this.tick = tick;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
        this.worldManager = worldManager;
        this.collisionManager = collisionManager;
        this.timeCycle = timeCycle;

        ServiceContainer.Register(window);
        ServiceContainer.Register(tick);
        ServiceContainer.Register(mesh);
        ServiceContainer.Register(shaderProgram);
        ServiceContainer.Register(worldManager);
        ServiceContainer.Register(collisionManager);
        ServiceContainer.Register(timeCycle);

        WorldUpdater.getInstance().init(window, mesh, collisionManager);
        this.worldBoundary = new WorldBoundary(
            worldManager.getPlayerController(),
            worldManager.getPlayerController().getRigidBody()
        );

        PhysicsRegistry.getInstance().init(mesh, collisionManager);

        Register();
        setCollision();
    } 

    // Get Mesh
    public Mesh.Mesh getMesh() {
        return mesh;
    }

    // Get World Manager
    public WorldManager getWorldManager() {
        return worldManager;
    }

    // Set Collision
    public void setCollision() {
        collisionManager.addStaticCollider(new BoundaryObject(WORLD_BOUNDARY));
    }

    /**

        Get
    
        */ 
    public T? get<T>() where T : WorldHandler {
        return el.OfType<T>().FirstOrDefault();
    }

    /**
    
        Render
    
        */
    public override void render() {
        foreach(var e in el) e.render();
    }

    /**
    
        Update
    
        */
    public override void update() {
        foreach(var e in el) e.update();
        worldBoundary.apply();

        PhysicsRegistry.getInstance().update();
    }

    /**

        Register
    
        */ 
    private void Register() {
        if(isRegistered) return;

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
            var instance = CreateInstance(type);
            if(instance != null) {
                el.Add(instance);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Registered: {type.Name}");
                Console.ResetColor();
            } else {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Failed to register: {type.Name}");
                Console.ResetColor();
            }
        }

        isRegistered = true;
    }

    /**

        Create Instance
    
        */ 
    private WorldHandler? CreateInstance(Type type) {
        var constructors = type.GetConstructors();

        foreach(var ctor in constructors.OrderByDescending(c => c.GetParameters().Length)) {
            var parameters = ctor.GetParameters();
            var args = new object?[parameters.Length];
            bool canResolve = true;

            for(int i = 0; i < parameters.Length; i++) {
                var param = parameters[i];
                var hasInjectionAttr = param.GetCustomAttribute<InjectAttribute>() != null;
                if(hasInjectionAttr || ServiceContainer.Has(param.ParameterType)) {
                    var service = ServiceContainer.Get(param.ParameterType);
                    if(service != null) {
                        args[i] = service;
                    }
                    else if(param.IsOptional) {
                        args[i] = param.DefaultValue;
                    }
                    else {
                        canResolve = false;
                        break;
                    }
                }
                else if(param.IsOptional) {
                    args[i] = param.DefaultValue;
                }
                else {
                    canResolve = false;
                    break;
                }

            }

            if(canResolve) {
                try {
                    return (WorldHandler)ctor.Invoke(args);
                } catch(Exception err) {
                    Console.WriteLine($"Error creating {type.Name}: {err.Message}");
                    return null;
                }
            }
        }

        return null;
    }
}