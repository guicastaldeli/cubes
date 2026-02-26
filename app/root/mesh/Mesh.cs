namespace App.Root.Mesh;
using App.Root.Shaders;
using OpenTK.Mathematics;

class Mesh {
    public readonly ShaderProgram shaderProgram;
    private Camera? camera;

    private readonly Dictionary<string, MeshData> meshDataMap = new();
    private readonly Dictionary<string, MeshRenderer> meshRendererMap = new();

    public Mesh(ShaderProgram shaderProgram) {
        this.shaderProgram = shaderProgram;
    }

    // Camera
    public void setCamera(Camera camera) {
        this.camera = camera;
        foreach(var meshRenderer in meshRendererMap.Values) {
            meshRenderer.setCamera(camera);
        }
    }

    // Mesh Renderer
    public MeshRenderer? getMeshRenderer(string id) {
        MeshRenderer? val = 
            meshRendererMap.TryGetValue(id, out var r) ?
            r :
            null;
        
        return val;
    }

    public Dictionary<string, MeshRenderer> getMeshRendererMap() {
        return meshRendererMap;
    }

    // Has Mesh
    public bool hasMesh(string id) {
        bool val = meshRendererMap.ContainsKey(id);
        return val;
    }

    // Get Data
    public MeshData? getData(string id) {
        MeshData? val = 
            meshDataMap.TryGetValue(id, out var d) ?
            d :
            null;

        return val;   
    }

    // Scale
    public void setScale(string id, Vector3 scale) {
        getMeshRenderer(id)?.setScale(scale);
    }

    public void setScale(string id, float scale) {
        getMeshRenderer(id)?.setScale(scale);
    }

    public void setScale(string id, float x, float y, float z) {
        getMeshRenderer(id)?.setScale(x, y, z);
    }

    // Set Texture
    public void setTexture(string id, int texId) {
        getMeshRenderer(id)?.setTex(texId);
    }

    // Set Model Matrix
    public void setModelMatrix(string id, Matrix4 matrix) {
        getMeshRenderer(id)?.setModelMatrix(matrix);
    }

    ///
    /// Add
    /// 
    public void add(string id) {
        MeshData data = MeshLoader.load(id);
        addToMap(id, data);
    }

    public void add(string id, MeshData data) {
        addToMap(id, data);
    }

    public void addToMap(string id, MeshData data) {
        meshDataMap[id] = data;
        MeshRenderer meshRenderer = new MeshRenderer(shaderProgram);
        meshRenderer.setData(data);
        if(camera != null) meshRenderer.setCamera(camera);
        meshRendererMap[id] = meshRenderer;
    }

    ///
    /// Remove
    /// 
    public void remove(string id) {
        if(meshRendererMap.TryGetValue(id, out var meshRenderer)) {
            meshRenderer.cleanup();
            meshRendererMap.Remove(id);
        }
        meshDataMap.Remove(id);
    }

    public void cleanup() {
        foreach(var meshRenderer in meshRendererMap.Values) {
            meshRenderer.cleanup();
        }
        meshRendererMap.Clear();
        meshDataMap.Clear();
    }

    ///
    /// Render
    /// 
    public void render(string id) {
        getMeshRenderer(id)?.render();
    }

    public void renderAll() {
        foreach(var entry in meshRendererMap) {
            entry.Value.render();
        }
    }
}