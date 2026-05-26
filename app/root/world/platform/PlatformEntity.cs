
using System.Reflection;

/**

    Platform Entity main class

    */
namespace App.Root.World.Platform;

class PlatformEntity {
    /**

        Platform Entity Handler

        */
    public abstract class PlatformEntityHandler {
        public virtual void render() {}
        public virtual void update() {}
    }

    private List<PlatformEntityHandler> el = new();

    /**
    
        Init
    
        */
    public void init() {
        var baseType = typeof(PlatformEntityHandler);
        var types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsSubclassOf(baseType)
            );

        foreach(var type in types) {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if(ctor != null) {
                var instance = (PlatformEntityHandler)ctor.Invoke(null);
                el.Add(instance);

                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"Platform Registered: {type.Name}");
                Console.ResetColor();
            }
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