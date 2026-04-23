namespace App.Root.Mesh;
using App.Root.Shaders;
using App.Root.Player;
using App.Root.Collider;
using OpenTK.Mathematics;
using OpenTK.Graphics.ES11;
using App.Root.Mesh.Particle;

class Mesh {
    private Window window;
    private ShaderProgram shaderProgram;
    private Camera? camera;
    private Input input;
    private MeshInteractionController? meshInteractionController;
    private PlayerController? playerController;
    private CollisionManager? collisionManager;
    private MeshRenderer? meshRenderer;
    private ParticleController? particleController;

    private Dictionary<string, MeshData> meshDataMap = new();
    private Dictionary<string, MeshRenderer> meshRendererMap = new();

    public Mesh(
        Window window, 
        ShaderProgram shaderProgram,
        Input input
    ) {
        this.window = window;
        this.shaderProgram = shaderProgram;
        this.input = input;

        this.particleController = new ParticleController(this);
    }

    // Set Camera
    public void setCamera(Camera camera) {
        this.camera = camera;
        foreach(var r in meshRendererMap.Values) {
            r.setCamera(camera);
        }
    }

    // Set Player Controller
    public void setPlayerController(PlayerController playerController) {
        this.playerController = playerController;
    }

    // Set Collision Manager
    public void setCollisionManager(CollisionManager collisionManager) {
        this.collisionManager = collisionManager;
    }

    // Get Particle Controller
    public ParticleController? getParticleController() {
        return particleController;
    }

    /**
    
        Mesh Renderer
    
        */
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

    /**
    
        Mesh Interaction Controller
    
        */
    public void initMeshInteractionController() {
        this.meshInteractionController = new MeshInteractionController(
            window,
            camera!,
            this,
            input,
            collisionManager!,
            playerController!.getRaycaster()
        );
    }

    public MeshInteractionController getMeshInteractionController() {
        return meshInteractionController!;
    }

    /**
    
        Has Mesh
    
        */
    public bool hasMesh(string id) {
        bool val = meshRendererMap.ContainsKey(id);
        return val;
    }

    /**
    
        Get Data
    
        */
    public MeshData? getData(string id) {
        MeshData? val = 
            meshDataMap.TryGetValue(id, out var d) ?
            d :
            null;

        return val;   
    }

    /**
    
        Position
    
        */
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

    /**
    
        Scale
    
        */
    public void setScale(string id, Vector3 scale) {
        getMeshRenderer(id)?.setScale(scale);
    }

    public void setScale(string id, float scale) {
        getMeshRenderer(id)?.setScale(scale);
    }

    public void setScale(string id, float x, float y, float z) {
        getMeshRenderer(id)?.setScale(x, y, z);
    }

    public Vector3 getDefaultScale(MeshData data) {
        if(data.hasScale()) {
            float[]? s = data.getScale();
            if(s != null && s.Length >= 3) {
                return new Vector3(s[0], s[1], s[2]);
            }
        }
        return Vector3.One;
    }

    /**
    
        Set Texture
    
        */
    public void setTexture(string id, int texId) {
        getMeshRenderer(id)?.setTex(texId);
    }

    public void setTexture(string id, int texId, string path) {
        getMeshRenderer(id)?.setTex(texId, path);
    }

    /**
    
        Set Model Matrix
    
        */
    public void setModelMatrix(string id, Matrix4 matrix) {
        getMeshRenderer(id)?.setModelMatrix(matrix);
    }

    /**
    
        Set Rotation Matrix
    
        */
    public void setRotationMatrix(string id, Matrix4 matrix) {
        getMeshRenderer(id)?.setRotationMatrix(matrix);
    }

    /**
    
        Set Network Controlled
    
        */
    public void setNetworkControlled(string id, bool val) {
        getMeshRenderer(id)?.setNetworkControlled(val);
    }

    /**
    
        Set Visible
    
        */
    public void setVisible(string id, bool visible) {
        getMeshRenderer(id)?.setVisible(visible);
    }

    /**
    
        Set Color
    
        */
    public void setColor(string id, string hex) {
        MeshData? data = getData(id);
        MeshRenderer? renderer = getMeshRenderer(id);
        if(data == null || renderer == null) return;

        data.setColorHex(hex);
        float[]? colors = data.getColors();
        if(colors != null) renderer.updateColors(colors);
    }
    
    /**
    
        Get Size
    
        */
    public Vector3 getSize(string id) {
        MeshData? data = getData(id);
        if(data == null) return Vector3.One;

        float[]? vertices = data.getVertices();
        if(vertices == null) return Vector3.One;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        for(int i = 0; i < vertices.Length; i+= 3) {
            float x = vertices[i];
            float y = vertices[i+1];
            float z = vertices[i+2];

            if(x < minX) minX = x;
            if(y < minY) minY = y;
            if(z < minZ) minZ = z;
            if(x > maxX) maxX = x;
            if(y > maxY) maxY = y;
            if(z > maxZ) maxZ = z;
        }

        return new Vector3(
            maxX - minX,
            maxY - minY,
            maxZ - minZ
        );
    }

    /**
    
        Get BBox
    
        */
    public BBox getBBox(string id) {
        Vector3 pos = getPosition(id);
        MeshData? meshData = getData(id);
        MeshRenderer? renderer = getMeshRenderer(id);

        float sizeX = 1.0f;
        float sizeY = 1.0f;
        float sizeZ = 1.0f;

        if(renderer != null && renderer.isScaled()) {
            Vector3 scale = renderer.getScale();
            sizeX = scale.X;
            sizeY = scale.Y;
            sizeZ = scale.Z;
        } else if(meshData != null && meshData.hasScale()) {
            float[]? scale = meshData.getScale();
            if(scale != null) {
                sizeX = scale[0];
                sizeY = scale[1];
                sizeZ = scale[2];
            }    
        } else if(renderer != null) {
            Vector3 scale = renderer.getScale();
            sizeX = scale.X;
            sizeY = scale.Y;
            sizeZ = scale.Z;
        }

        return new BBox(
            pos.X - sizeX / 2, pos.Y - sizeY / 2, pos.Z - sizeZ / 2,
            pos.X + sizeX / 2, pos.Y + sizeY / 2, pos.Z + sizeZ / 2
        );
    }

    /**
    
        On Window Resize

        */
    public void onWindowResize(int width, int height) {
        if(meshRenderer != null) {
            meshRenderer.setupSceneFramebuffer(width, height);
        }

        foreach(var renderer in meshRendererMap.Values) {
            renderer.onWindowResize(width, height);
        }
    }

    /**
    
        Add

        */
    public void add(string id) {
        MeshData data = MeshLoader.load(id);
        addToMap(id, data);
    }

    public void add(string id, string meshType) {
        MeshData data = MeshLoader.load(meshType);
        addToMap(id, data);
    }

    public void add(string id, MeshData data) {
        addToMap(id, data);
    }

    public void addToMap(string id, MeshData data) {
        meshDataMap[id] = data;
        
        MeshRenderer renderer = new MeshRenderer(window, shaderProgram, this);
        renderer.setData(data);
        renderer.setId(id);

        if(camera != null) renderer.setCamera(camera);
        meshRendererMap[id] = renderer;
    }

    /**
    
        Update

        */
    public void update() {
        if(particleController != null) {
            particleController.update();
        }

        foreach(var r in meshRendererMap.Values) {
            r.update();
        }
    }

    /**
    
        Render

        */
    // Main
    public void render() {
        if(particleController != null) {
            particleController.render();
        }

        if(meshRenderer != null) {
            meshRenderer.render();

            renderOrto();
            renderOnTopMeshes();
        }
    }

    // All
    public void renderAll() {
        if(particleController != null) {
            particleController.render();
        }

        foreach(var entry in meshRendererMap) {
            if(entry.Value.isHud) continue;
            if(entry.Value.isInstanced) {
                entry.Value.renderInstanced();
            } else {
                entry.Value.render();
            }
        }
    }

    // By Id
    public void renderId(string id) {
        var renderer = getMeshRenderer(id);
        if(renderer == null) return;

        if(renderer.isInstanced) {
            getMeshRenderer(id)?.renderInstanced();
        } else {
            getMeshRenderer(id)?.render();
        }
    }

    // Orto
    public void renderOrto() {
        foreach(var entry in meshRendererMap) {
            if(entry.Value.isHud) {
                entry.Value.renderOrto(
                    window.getWidth(),
                    window.getHeight()
                );
            }
        }
    }
    
    // On Top Meshes
    private void renderOnTopMeshes() {
        foreach(var entry in getMeshRendererMap()) {
            if(entry.Value.renderOnTop && !entry.Value.isHud) {
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.DepthRange(0.0f, 0.1f);
                entry.Value.renderMesh();
                GL.DepthRange(0.0f, 1.0f);
            }
        }
    }

    // Outline
    public void renderOutline(List<string> ids) {
        List<MeshRenderer> selected = ids
            .Select(id => getMeshRenderer(id))
            .Where(r => r != null)
            .Cast<MeshRenderer>()
            .ToList();
        if(selected.Count == 0) return;

        selected[0].renderOutline(selected);
    }

    /**
    
        Init
    
        */
    public void init() {
        meshRenderer = new MeshRenderer(window, shaderProgram, this);
        meshRenderer.setupSceneFramebuffer(window.getWidth(), window.getHeight());
    }

    /**
    
        Remove and Cleanup
    
        */
    public void remove(string id) {
        if(meshRendererMap.TryGetValue(id, out var meshRenderer)) {
            meshRenderer.cleanup();
            meshRendererMap.Remove(id);
        }
        meshDataMap.Remove(id);
    }

    public void cleanup() {
        if(particleController != null) {
            particleController.cleanup();
        }

        foreach(var r in meshRendererMap.Values) {
            r.cleanup();
        }
        meshRendererMap.Clear();
        meshDataMap.Clear();
    }
}