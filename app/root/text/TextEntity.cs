using App.Root.Player;
using App.Root.Screen;
using App.Root.Shaders;
using App.Root.UI;
using OpenTK.Mathematics;

namespace App.Root.Text;

class TextEntity {
    private const string MESH = "quad";

    private string id;
    private string path;

    private Vector3 worldPosition;
    private float maxDistance;
    private float scale;

    private Window window;
    private ShaderProgram shaderProgram;
    private Camera camera;
    private UIData? uiData;
    private TextRenderer textRenderer;
    private Mesh.Mesh mesh;

    private bool visible = false;
    private bool initialized = false;

    private Dictionary<string, int> elTextures = new();

    public TextEntity(
        string id,
        string path,
        Window window,
        ShaderProgram shaderProgram,
        Camera camera,
        Vector3 worldPosition,
        Mesh.Mesh mesh,
        float scale = 1.0f,
        float maxDistance = 10.0f
    ) {
        this.id = id;
        this.path = path;

        this.window = window;
        this.shaderProgram = shaderProgram;
        this.camera = camera;
        
        this.worldPosition = worldPosition;
        this.mesh = mesh;

        this.scale = scale;
        this.maxDistance = maxDistance;

        this.textRenderer = new TextRenderer(shaderProgram, window.getWidth(), window.getHeight());
        
        this.uiData = DocParser.parseUI(path, window.getWidth(), window.getHeight()).
    }

    // Get Mesh Id
    private string getMeshId(string elId) {
        string val = $"text_entity_{id}_{elId}";
        return val;
    }

    // Set Visible
    public void setVisible(bool visible) {
        this.visible = visible;
        foreach(var el in uiData?.elements ?? new()) {
            mesh.setVisible(getMeshId(el.id), visible && el.visible);
        }
    }

    // Set Element Visible
    public void setElementVisible(string elId, bool visible) {
        if(uiData == null) return;

        var el = DocParser.getElementById(uiData, elId);
        if(el != null) {
            el.visible = visible;
            mesh.setVisible(getMeshId(elId), visible);
        }
    }

    /**
    
        Update
    
        */
}