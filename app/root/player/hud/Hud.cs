namespace App.Root.Player.Hud;
using App.Root.Mesh;
using App.Root.Shaders;
using System.Reflection;

/**

    Main HUD Element Handler

    */
class HudElement {
    public static ShaderProgram shaderProgram = null!;
    public static Mesh mesh = null!;
    
    public string id;
    public static int screenWidth;
    public static int screenHeight;

    public HudElement(string id) {
        this.id = id;
    }
    public static void init(
        Window window,
        ShaderProgram shaderProgram,
        Mesh mesh
    ) {
        HudElement.shaderProgram = shaderProgram;
        HudElement.mesh = mesh;
        HudElement.screenWidth = window.getWidth();
        HudElement.screenHeight = window.getHeight();
    }

    public virtual void render() {}
    public virtual void update() {}
    public virtual void onWindowResize(int width, int height) {}

}

/**

    Main HUD controller,
    controls general HUD

    */
class Hud {
    private Dictionary<string, HudElement> elements = new();

    private Window window;
    private ShaderProgram shaderProgram;
    private Mesh mesh;

    public Hud(
        Window window,
        ShaderProgram shaderProgram,
        Mesh mesh
    ) {
        this.window = window;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;

        HudElement.init(
            window,
            shaderProgram,
            mesh
        );

        this.init();
    }

    // Get
    public T? get<T>(string id) where T : HudElement {
        T? val = elements.TryGetValue(id, out var el) ? el as T : null;
        return val;
    }

    // On Window Resize
    public void onWindowResize(int width, int height) {
        foreach(var el in elements.Values) {
            el.onWindowResize(width, height);
        }
    }

    ///
    /// Init
    /// 
    public void init() {
        var baseType = typeof(HudElement);

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
                var instance = (HudElement)ctor.Invoke(null);
                elements[instance.id] = instance;
            }
        }
    }

    ///
    /// Render
    /// 
    public void render() {
        foreach(var el in elements.Values) {
            el.render();
        }
    }

    ///
    /// Update
    /// 
    public void update() {
        foreach(var el in elements.Values) {
            el.update();
        }
    }
}