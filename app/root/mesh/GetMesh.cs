namespace App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Player;
using OpenTK.Mathematics;
using App.Root.Collider;

class GetMesh {
    public readonly ShaderProgram shaderProgram;
    private Camera? camera;

    private readonly Dictionary<string, MeshData> meshDataMap = new();
    private readonly Dictionary<string, MeshRenderer> meshRendererMap = new();

    public GetMesh(ShaderProgram shaderProgram) {
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

    // Position
    public void setPosition(string id, Vector3 position) {
        getMeshRenderer(id)?.setPosition(position);
    }

    public void setPosition(string id, float x, float y, float z) {
        getMeshRenderer(id)?.setPosition(x, y, z);
    }

    public Vector3 getPosition(string id) {
        Vector3 val = getMeshRenderer(id)?.getPosition() ?? Vector3.Zero;
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

    // Set Color
    public void setColor(string id, string hex) {
        MeshData? data = getData(id);
        MeshRenderer? meshRenderer = getMeshRenderer(id);
        if(data == null || meshRenderer == null) return;

        data.setColorHex(hex);
        float[]? colors = data.getColors();
        if(colors != null) meshRenderer.updateColors(colors);
    }

    // Get BBox
    public BBox getBBox(string id) {
        Vector3 pos = getPosition(id);
        MeshData? meshData = getData(id);

        float sizeX = 1.0f;
        float sizeY = 1.0f;
        float sizeZ = 1.0f;

        if(meshData != null && meshData.hasScale()) {
            float[]? scale = meshData.getScale();
            if(scale != null) {
                sizeX = scale[0];
                sizeY = scale[1];
                sizeZ = scale[2];
            }    
        } else {
            MeshRenderer? meshRenderer = getMeshRenderer(id);
            if(meshRenderer != null) {
                Vector3 scale = meshRenderer.getScale();
                sizeX = scale.X;
                sizeY = scale.Y;
                sizeZ = scale.Z;
            }
        }

        return new BBox(
            pos.X - sizeX / 2, pos.Y - sizeY / 2, pos.Z - sizeZ / 2,
            pos.X + sizeX / 2, pos.Y + sizeY / 2, pos.Z + sizeZ / 2
        );
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
    /// Update
    /// 
    public void update() {
        foreach(var meshRenderer in meshRendererMap.Values) {
            meshRenderer.update();
        }
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