/**

    Text Entity Renderer main class

    */
namespace App.Root.Text;
using App.Root.Mesh;
using App.Root.Player;
using App.Root.Shaders;
using OpenTK.Mathematics;

class TextEntityRenderer {
    private Window window;
    private ShaderProgram shaderProgram;
    private Camera camera;
    private Mesh mesh;

    private Dictionary<string, TextEntity> entities = new();

    public TextEntityRenderer(Window window, ShaderProgram shaderProgram, Camera camera, Mesh mesh) {
        this.window = window;
        this.shaderProgram = shaderProgram;
        this.camera = camera;
        this.mesh = mesh;

        new TextEntity(window, shaderProgram, camera, mesh);
    }

    /**
    
        Get
    
        */
    public TextEntity? get(string id) {
        TextEntity? val = entities.TryGetValue(id, out var e) ? e : null;
        return val;
    }

    /**

        Add
    
        */
    public TextEntity add(
        string id,
        string path,
        Vector3 worldPosition,
        float scale = 1.0f,
        float maxDistance = 10.0f
    ) {
        var entity = new TextEntity(window, shaderProgram, camera, mesh).S(id, path, worldPosition, scale, maxDistance);
        entity.init();

        entities[id] = entity;
        
        return entity;
    }

    /**
    
        Render
    
        */
    public void render() {
        foreach(var e in entities.Values) e.render();
    }

    /**
    
        Remove
    
        */
    public void remove(string id) {
        entities.TryGetValue(id, out var e);
        e?.cleanup();
        entities.Remove(id);
    }
}