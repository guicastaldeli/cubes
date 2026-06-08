namespace App.Root.Mesh;
using App.Root.Player;
using App.Root.Shaders;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using App.Root.Resource;

class MeshRenderer : DataEntry {
    private Window window;
    private ShaderProgram shaderProgram;
    private Mesh mesh;
    private MeshData? meshData;
    private Camera? camera;

    private string meshType = "";

    private int vao;
    private int vbo;
    private int ebo;
    private int colorVbo;
    private int normalVbo;
    private int texCoordsVbo;
    private int vertexCount;

    public bool isInstanced = false;
    public int instanceVbo = 0;
    private int instanceCount = 0;
    private int instanceRotationVbo = 0;
    private int instanceScaleVbo = 0;
    private List<Vector3> cachedInstancePositions = new();
    private List<float> cachedInstanceRotations = new();
    private List<float> cachedInstanceScales = new();

    private int stencilFbo = 0;
    private int stencilTexture = 0;
    private int stencilDepthTexture = 0;

    private static int sceneDepthFbo = 0;
    private static int sceneDepthTexture = 0;
    private static int sceneColorTexture = 0;

    private int quadVao = 0;
    private int quadVbo = 0;

    private int instanceColorVbo;
    private List<float[]> cachedInstanceColors = new();

    private int instanceTexIdVbo = 0;
    public List<int> cacheInstanceTexIds = new();
    public Dictionary<string, int> texPathCache = new();
    public List<string?> cachedInstanceTexPaths = new();

    public bool renderOnTop = false;

    private Matrix4 modelMatrix = Matrix4.Identity;
    private Matrix4 rotationMatrix = Matrix4.Identity;

    private Vector3 position = Vector3.Zero;
    private Vector3 scale = Vector3.One;

    private bool hasScale = false;
    private bool isDynamic = false;
    public bool hasColors = false;
    private bool hasTex = false;
    private int texId = -1;
    private string texPath = "";

    public bool isInteractive = false;
    public bool isHud = false;

    private string id = "";
    private bool networkControlled = false;

    private bool visible = true;

    public MeshRenderer(Window window, ShaderProgram shaderProgram, Mesh mesh) {
        this.window = window;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;
    }

    // Get Cached Instance Positions
    public List<Vector3> getCachedInstancePositions() {
        return cachedInstancePositions;
    }

    // Get Instance VBO Initialized
    public bool getInstanceVboInitialized() {
        bool val = instanceVbo != 0;
        return val;
    }

    // Camera
    public void setCamera(Camera camera) {
        this.camera = camera;
    }

    public Camera? getCamera() {
        return camera;
    }

    // Rotation Matrix
    public void setRotationMatrix(Matrix4 matrix) {
        rotationMatrix = matrix;
    }

    // Model Matrix
    public void setModelMatrix(Matrix4 m) {
        modelMatrix = m;
    }

    // Network Controlled
    public void setNetworkControlled(bool val) {
        networkControlled = val;
    }

    // Set Visible
    public void setVisible(bool visible) {
        this.visible = visible;
    }

    /**
     * 
     * On Window Resize
     *
     */
    public void onWindowResize(int width, int height) {
        setupStencilFramebuffer(width, height);
    }

    /**
     * 
     * Data
     *
     */
    public void setData(MeshData data) {
        meshData = data;
        isDynamic = data.isDynamic;
        meshType = data.meshType;

        createBuffers();
        setupStencilFramebuffer(window.getWidth(), window.getHeight());
        setupFullscreenQuad();
    } 

    public void setInstanceData(
        List<Vector3> positions, 
        List<float[]>? colors = null, 
        List<float>? rotations = null,
        List<string?>? texPaths = null,
        List<int>? texIds = null,
        List<float>? scales = null
    ) {
        if(positions.Count == 0) return;

        cachedInstancePositions = positions;
        if(colors != null) cachedInstanceColors = colors;
        if(rotations != null) cachedInstanceRotations = rotations;
        if(texPaths != null) cachedInstanceTexPaths = texPaths;
        if(scales != null) cachedInstanceScales = scales;

        List<int> texId = TextureLoader.setInstancedTex(this, positions, texIds, texPaths);

        instanceCount = positions.Count;

        GL.BindVertexArray(vao);

        float[] posData = new float[positions.Count * 3];
        for(int i = 0; i < positions.Count; i++) {
            posData[i*3+0] = positions[i].X;
            posData[i*3+1] = positions[i].Y;
            posData[i*3+2] = positions[i].Z;
        }

        if(instanceVbo == 0) instanceVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, posData.Length * sizeof(float), posData, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(4);
        GL.VertexAttribDivisor(4, 1);

        if(colors != null && colors.Count == positions.Count) {
            float[] colorData = new float[colors.Count * 4];
            for(int i = 0; i < colors.Count; i++) {
                colorData[i*4+0] = colors[i][0];
                colorData[i*4+1] = colors[i][1];
                colorData[i*4+2] = colors[i][2];
                colorData[i*4+3] = colors[i][3];
            }

            if(instanceColorVbo == 0) instanceColorVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceColorVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, colorData.Length * sizeof(float), colorData, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(5);
            GL.VertexAttribDivisor(5, 1);
        }

        if(rotations != null && rotations.Count == positions.Count) {
            float[] rotationData = new float[rotations.Count * 4];
            for(int i = 0; i < rotations.Count; i++) {
                rotationData[i*4+0] = rotations[i];
                rotationData[i*4+1] = 0;
                rotationData[i*4+2] = 0;
                rotationData[i*4+3] = 0;
            }

            if(instanceRotationVbo == 0) instanceRotationVbo =  GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceRotationVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, rotationData.Length * sizeof(float), rotationData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(6);
            GL.VertexAttribDivisor(6, 1);
        }

        if(texId.Count == positions.Count) {
            int[] texIdData = texId.ToArray();

            if(instanceTexIdVbo == 0) instanceTexIdVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceTexIdVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, texIdData.Length * sizeof(int), texIdData, BufferUsageHint.DynamicDraw);
            GL.VertexAttribIPointer(7, 1, VertexAttribIntegerType.Int, 0, 0);
            GL.EnableVertexAttribArray(7);
            GL.VertexAttribDivisor(7, 1);
        }

        if(scales != null && scales.Count == positions.Count) {
            float[] scaleData = scales.ToArray();

            if(instanceScaleVbo == 0) instanceScaleVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceScaleVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, scaleData.Length * sizeof(float), scaleData, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(8, 1, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(8);
            GL.VertexAttribDivisor(8, 1);
        }

        GL.BindVertexArray(0);
    }

    /**
     *
     * Texture
     *
     */
    public void setTex(int id) {
        if(id > 0) {
            texId = id;
            hasTex = true;
        } else {
            texId = -1;
            hasTex = false;
        }
    }

    public void setTex(int id, string path = "") {
        this.texPath = path;
        if(id > 0) {
            texId = id;
            hasTex = true;
        } else {
            texId = -1;
            hasTex = false;
        }
    }

    public string getTexPath() {
        return texPath;
    }

    public int getTexId() {
        return texId;
    }

    /**
     *
     * Scale
     *
     */
    public void setScale(Vector3 scale) {
        this.scale = scale;
        hasScale = true;
    }

    public void setScale(float scale) {
        setScale(new Vector3(scale, scale, scale));
    }

    public void setScale(float x, float y, float z) {
        setScale(new Vector3(x, y, z));
    }

    public Vector3 getScale() {
        return new Vector3(scale);
    }

    public bool isScaled() {
        return hasScale;
    }

    private void setInstancedScales(List<Vector3> positions) {
        float[] scaleData = Enumerable.Repeat(1.0f, positions.Count).ToArray();
        if(instanceScaleVbo == 0) instanceScaleVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, instanceScaleVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, scaleData.Length * sizeof(float), scaleData, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(8, 1, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(8);
        GL.VertexAttribDivisor(8, 1);
    }

    /**
     * 
     * Position
     *
     */
    public Vector3 getPosition() {
        return new Vector3(position);
    }

    public void setPosition(Vector3 position) {
        this.position = position;
    }

    public void setPosition(float x, float y, float z) {
        position = new Vector3(x, y, z);
    }

    public void setInstancePositions(List<Vector3> positions) {
        cachedInstancePositions = positions;
        instanceCount = positions.Count;

        float[] data = new float[positions.Count * 3];
        for(int i = 0; i < positions.Count; i++) {
            data[i * 3 + 0] = positions[i].X;
            data[i * 3 + 1] = positions[i].Y;
            data[i * 3 + 2] = positions[i].Z;
        }

        instanceVbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(4);
        GL.VertexAttribDivisor(4, 1);
        
        setInstancedScales(positions);

        GL.BindVertexArray(0);
    }

    /**
     * 
     * Buffers
     *
     */
    // Main Buffers
    private void createBuffers() {
        if(meshData == null) return;

        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        // Vertices
        float[]? vertices = meshData.getVertices();
        if(vertices != null) {
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            vertexCount = vertices.Length / 3;
        }
        // Normals
        float[]? normals = meshData.getNormals();
        if(normals != null) {
            normalVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, normals.Length * sizeof(float), normals, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(1);
        }
        // Colors
        float[]? colors = meshData.getColors();
        if(colors != null) {
            colorVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(2);
            hasColors = true;
        }
        //Tex Coords
        float[]? texCoords = meshData.getTexCoords();
        if(texCoords != null) {
            texCoordsVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, texCoordsVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Length * sizeof(float), texCoords, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(3);
        }
        // Indices
        int[]? indices = meshData.getIndices();
        if(indices != null) {
            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);
            vertexCount = indices.Length;
        }

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    // Stencil Buffer
    public void setupStencilFramebuffer(int width, int height) {
        if(stencilFbo != 0) GL.DeleteFramebuffer(stencilFbo);
        if(stencilTexture != 0) GL.DeleteTexture(stencilTexture);
        if(stencilDepthTexture != 0) GL.DeleteTexture(stencilDepthTexture);

        stencilTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, stencilTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        stencilDepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, stencilDepthTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        stencilFbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, stencilFbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, stencilTexture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, stencilDepthTexture, 0);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if(status != FramebufferErrorCode.FramebufferComplete) {
            throw new Exception($"FBO setup failed!: {status}");
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    // Scene Buffers
    public void setupSceneFramebuffer(int width, int height) {
        if(sceneDepthFbo != 0) GL.DeleteFramebuffer(sceneDepthFbo);
        if(sceneDepthTexture != 0) GL.DeleteFramebuffer(sceneDepthTexture);
        if(sceneColorTexture != 0) GL.DeleteTexture(sceneColorTexture);

        sceneDepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, sceneDepthTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        sceneColorTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, sceneColorTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        sceneDepthFbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, sceneDepthFbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, sceneColorTexture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, sceneDepthTexture, 0);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if(status != FramebufferErrorCode.FramebufferComplete) {
            throw new Exception($"Scene FBO setup failed!: {status}");
        }    

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    // Quad Buffers
    public void setupFullscreenQuad() {
        float[] verts = {
            -1.0f, -1.0f, 0.0f,   0.0f, 0.0f,
            1.0f, -1.0f, 0.0f,    1.0f, 0.0f,
            -1.0f, 1.0f, 0.0f,    0.0f, 1.0f,
            1.0f, 1.0f, 0.0f,     1.0f, 1.0f
        };

        quadVao = GL.GenVertexArray();
        quadVbo = GL.GenBuffer();
    
        GL.BindVertexArray(quadVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, quadVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.BindVertexArray(0);
    }

    /**
     * 
     * Update
     *
     */
    // Main
    public void update() {
        if(meshData == null) return;
        if(meshData.hasRotation() == true) updateRotation();
    }

    // Update Rotation
    public void updateRotation() {
        if(networkControlled) return;
        if(meshData == null || !meshData.hasRotation()) return;

        string? axis = meshData.getRotationAxis();
        float speed = meshData.getRotationSpeed();
        float deltaTime = Tick.getDeltaTimeI();

        switch(axis?.ToLower()) {
            case "x":
                rotationMatrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(speed * deltaTime));
                break;
            case "y":
                rotationMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(speed * deltaTime));
                break;
            case "z":
                rotationMatrix *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(speed * deltaTime));
                break;
        }
    }

    // Update Colors
    public void updateColors(float[] colors) {
        if(colorVbo == 0) {
            colorVbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(2);
            GL.BindVertexArray(0);

            hasColors = true;

            return;
        }

        hasColors = true;

        GL.BindBuffer(BufferTarget.ArrayBuffer, colorVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.DynamicDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    } 

    // Update Instance Data
    public void updateInstanceData(
        List<Vector3> positions, 
        List<float[]> colors, 
        List<float> rotations,
        List<string?>? texPaths = null,
        List<int>? texIds = null,
        List<float>? scales = null
    ) {
        //if(positions.Count != instanceCount) return;

        instanceCount = positions.Count;
        cachedInstancePositions = positions;
        cachedInstanceRotations = rotations;
        if(scales != null) cachedInstanceScales = scales;

        List<int> texId = TextureLoader.setInstancedTex(this, positions, texIds, texPaths);

        float[] posData = new float[positions.Count * 3];
        for(int i = 0; i < positions.Count; i++) {
            posData[i * 3 + 0] = positions[i].X;
            posData[i * 3 + 1] = positions[i].Y;
            posData[i * 3 + 2] = positions[i].Z;
        }
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, posData.Length * sizeof(float), posData, BufferUsageHint.DynamicDraw);

        if(colors != null && colors.Count == positions.Count && instanceColorVbo != 0) {
            float[] colorData = new float[colors.Count * 4];
                for(int i = 0; i < colors.Count; i++) {
                colorData[i * 4 + 0] = colors[i][0];
                colorData[i * 4 + 1] = colors[i][1];
                colorData[i * 4 + 2] = colors[i][2];
                colorData[i * 4 + 3] = colors[i][3];
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceColorVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, colorData.Length * sizeof(float), colorData, BufferUsageHint.DynamicDraw);
        }

        if(rotations != null && rotations.Count == positions.Count && instanceRotationVbo != 0) {
            float[] rotationData = new float[rotations.Count * 4];
            for(int i = 0; i < rotations.Count; i++) {
                rotationData[i*4+0] = rotations[i];
                rotationData[i*4+1] = 0;
                rotationData[i*4+2] = 0;
                rotationData[i*4+3] = 0;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceRotationVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, rotationData.Length * sizeof(float), rotationData, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);      
        }

        if(texId.Count == positions.Count && instanceTexIdVbo != 0) {
            int[] texIdData = texId.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceTexIdVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, texIdData.Length * sizeof(int), texIdData, BufferUsageHint.DynamicDraw);
        }

        if(scales != null && scales.Count == positions.Count && instanceScaleVbo != 0) {
            float[] scaleData = scales.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceScaleVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, scaleData.Length * sizeof(float), scaleData, BufferUsageHint.DynamicDraw);
        }

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    /**
     * 
     * Render
     *
     */
    // Main
    public void render() {
        /*
        int total = mesh.getMeshRendererMap().Count;
        int instanced = mesh.getMeshRendererMap().Values.Count(r => r.isInstanced);
        int staticM = mesh.getMeshRendererMap().Values.Count(r => !r.isInstanced);
        int entityTypes = mesh.getMeshRendererMap().Values.Count(r => r.isInstanced && r.isInteractive);
        Console.WriteLine($"Total: {total} | Instanced: {instanced} | Static: {staticM} | EntityTypes: {entityTypes}");
        */
        if(mesh == null) {
            Console.Error.WriteLine("Mesh is null!");
            return;
        }
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, sceneDepthFbo);
        GL.Viewport(0, 0, window.getWidth(), window.getHeight());
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach(var entry in mesh.getMeshRendererMap()) {
            if(entry.Value.isHud) continue;
            if(entry.Value.renderOnTop) continue;
            if(entry.Value.isInstanced) {
                entry.Value.renderInstanced();
            } else {
                entry.Value.renderMesh();
            }
        }

        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sceneDepthFbo);
        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        GL.BlitFramebuffer(
            0, 0, window.getWidth(), window.getHeight(),
            0, 0, window.getWidth(), window.getHeight(),
            ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit,
            BlitFramebufferFilter.Nearest
        );

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    // Mesh
    public void renderMesh() {
        if(!visible) return;
        if(meshData == null || camera == null) return;

        if(!isDynamic) {
            if(hasScale) {
                modelMatrix =
                    Matrix4.CreateScale(scale) *
                    rotationMatrix *
                    Matrix4.CreateTranslation(position);
            } else if(meshData.hasScale()) {
                float[]? s = meshData.getScale();
                if(s != null) {
                    modelMatrix =
                        Matrix4.CreateScale(s[0], s[1], s[2]) *
                        rotationMatrix *
                        Matrix4.CreateTranslation(position);
                } else {
                    modelMatrix =
                        rotationMatrix *
                        Matrix4.CreateTranslation(position);
                }
            } else {
                modelMatrix =
                    rotationMatrix *
                    Matrix4.CreateTranslation(position);
            }
        }

        shaderProgram.bind();
        if(meshData.isEntity != 0) {
            shaderProgram.setUniform("isEntity", meshData.isEntity);
        } else {
            shaderProgram.setUniform("isEntity", 0);
        }
        if(meshData.shaderType != 0) {
            shaderProgram.setUniform("shaderType", meshData.shaderType);
        } else {
            shaderProgram.setUniform("shaderType", 0);
        }
        shaderProgram.setUniform("shaderAddon", meshData.shaderAddon);
        shaderProgram.setUniform("isInv", 0);
        shaderProgram.setUniform("isInstanced", 0);
        shaderProgram.setUniform("hasInstanceColor", 0);
        shaderProgram.setUniform("uModel", modelMatrix);
        shaderProgram.setUniform("uView", camera.getView());
        shaderProgram.setUniform("uProjection", camera.getProjection());
        shaderProgram.setUniform("uHasColors", hasColors ? 1 : 0);
        shaderProgram.setUniform("hasTex", hasTex ? 1 : 0);
        shaderProgram.setUniform("canvasSize", (float)window.getWidth(), (float)window.getHeight());
        if(hasColors && meshData.getColors() != null) {
            float[]? color = meshData.getColors();
            if(color != null) {
                if(color.Length >= 4) {
                    shaderProgram.setUniform(
                        "uColor", 
                        color[0], 
                        color[1], 
                        color[2],
                        color[3]
                    );
                }
            }
        }
        if(hasTex) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texId);
            shaderProgram.setUniform("uSampler", 0);
        }

        GL.BindVertexArray(vao);

        int[]? indices = meshData.getIndices();
        if(indices != null) {
            GL.DrawElements(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, 0);
        } else {
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
        }

        GL.BindVertexArray(0);
        if(hasTex) GL.BindTexture(TextureTarget.Texture2D, 0);
        shaderProgram.unbind();
    }

    // Instanced Meshes
    public void renderInstanced() {
        if(!visible || meshData == null || camera == null) return;

        if(hasScale) {
            modelMatrix =
                Matrix4.CreateScale(scale) *
                rotationMatrix *
                Matrix4.CreateTranslation(position);
        } else if(meshData.hasScale()) {
            float[]? s = meshData.getScale();
            if(s != null) {
                modelMatrix =
                    Matrix4.CreateScale(s[0], s[1], s[2]) *
                    rotationMatrix *
                    Matrix4.CreateTranslation(position);
            } else {
                modelMatrix =
                    rotationMatrix *
                    Matrix4.CreateTranslation(position);
            }
        } else {
            modelMatrix =
                rotationMatrix *
                Matrix4.CreateTranslation(position);
        }

        shaderProgram.bind();
        if(meshData.isEntity != 0) {
            shaderProgram.setUniform("isEntity", meshData.isEntity);
        } else {
            shaderProgram.setUniform("isEntity", 0);
        }
        if(meshData.shaderType != 0) {
            shaderProgram.setUniform("shaderType", meshData.shaderType);
        } else {
            shaderProgram.setUniform("shaderType", 0);
        }
        shaderProgram.setUniform("shaderAddon", meshData.shaderAddon);
        shaderProgram.setUniform("hasInstanceColor", instanceColorVbo != 0 ? 1 : 0);
        shaderProgram.setUniform("uModel", modelMatrix);
        shaderProgram.setUniform("isInv", 0);
        shaderProgram.setUniform("uView", camera.getView());
        shaderProgram.setUniform("uProjection", camera.getProjection());
        shaderProgram.setUniform("uHasColors", hasColors ? 1 : 0);
        shaderProgram.setUniform("isInstanced", 1);
        shaderProgram.setUniform("canvasSize", (float)window.getWidth(), (float)window.getHeight());
        shaderProgram.setUniform("hasTex", hasTex ? 1 : 0);
        if(hasTex) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texId);
            shaderProgram.setUniform("uSampler", 0);
        }

        GL.BindVertexArray(vao);
        int[]? indices = meshData.getIndices();
        if(indices != null) {
            GL.DrawElementsInstanced(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, instanceCount);
        } else {
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, vertexCount, instanceCount);
        }
        
        GL.BindVertexArray(0);
        if(hasTex) GL.BindTexture(TextureTarget.Texture2D, 0);
        shaderProgram.unbind();
    }

    // Orthographic
    public void renderOrto(int screenWidth, int screenHeight) {
        if(!visible) return;
        if(meshData == null) return;

        bool depthTestWasEnabled = GL.IsEnabled(EnableCap.DepthTest);
        bool depthMaskWasEnabled = true;
        GL.GetBoolean(GetPName.DepthWritemask, out depthMaskWasEnabled);

        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.DepthMask(false);
        GL.Disable(EnableCap.DepthTest);

        Matrix4 ortho = Matrix4.CreateOrthographicOffCenter(
            0, screenWidth,
            0, screenHeight,
            -1.0f, 1.0f
        );

        Matrix4 model = 
            Matrix4.CreateScale(scale) *
            Matrix4.CreateTranslation(position);

        shaderProgram.bind();

        if(meshData.enableInverted == true && meshData.screenTexOverride != 0) {
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, meshData.screenTexOverride);
            GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, screenWidth, screenHeight);
            shaderProgram.setUniform("uScreenTexture", 1);
            shaderProgram.setUniform("isInv", meshData.invertedValue);
            GL.ActiveTexture(TextureUnit.Texture0);
        } else {
            shaderProgram.setUniform("isInv", 0);
        }

        if(meshData.shaderType != 0) {
            shaderProgram.setUniform("shaderType", meshData.shaderType);
        } else {
            shaderProgram.setUniform("shaderType", 3);
        }
        shaderProgram.setUniform("shaderAddon", meshData.shaderAddon);
        shaderProgram.setUniform("uModel", model);
        shaderProgram.setUniform("uView", Matrix4.Identity);
        shaderProgram.setUniform("uProjection", ortho);
        shaderProgram.setUniform("uHasColors", hasColors ? 1 : 0);
        shaderProgram.setUniform("isInstanced", 0);
        shaderProgram.setUniform("screenSize", (float)screenWidth, (float)screenHeight);
        shaderProgram.setUniform("canvasSize", (float)screenWidth, (float)screenHeight);
        shaderProgram.setUniform("hasTex", hasTex ? 1 : 0);
        if(hasTex) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.Enable(EnableCap.Blend);
            shaderProgram.setUniform("uSampler", 0);
        }

        GL.BindVertexArray(vao);

        int[]? indices = meshData.getIndices();
        if(indices != null) {
            GL.DrawElements(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, 0);
        } else {
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
        }

        GL.BindVertexArray(0);
        if(hasTex) {
            GL.Disable(EnableCap.Blend);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        GL.DepthMask(depthMaskWasEnabled);
        if(depthTestWasEnabled) GL.Enable(EnableCap.DepthTest);
        shaderProgram.unbind();
    }

    // Flat Meshes
    public void renderFlat(Vector3? instancePos = null, int instanceIndex = 0) {
        if(meshData == null || camera == null) return;

        Matrix4 model;
        if(hasScale) {
            model =
                Matrix4.CreateScale(scale) *
                rotationMatrix *
                Matrix4.CreateTranslation(position);
        } else if(meshData.hasScale()) {
            float[]? s = meshData.getScale();
            if(s != null) {
                model = Matrix4.CreateScale(s[0], s[1], s[2]) *
                rotationMatrix *
                Matrix4.CreateTranslation(position);
            } else {
                model = 
                    rotationMatrix *
                    Matrix4.CreateTranslation(position);
            }
        } else {
            model =
                rotationMatrix *
                Matrix4.CreateTranslation(position);
        }

        shaderProgram.bind();
        shaderProgram.setUniform("shaderType", 5);
        shaderProgram.setUniform("uModel", model);
        shaderProgram.setUniform("uView", camera.getView());
        shaderProgram.setUniform("uProjection", camera.getProjection());

        GL.BindVertexArray(vao);
        int[]? indices = meshData.getIndices();
        if(isInstanced) {
            shaderProgram.setUniform("isInstanced", 1);

            int drawCount = instanceCount;
            if(instancePos.HasValue && instanceVbo != 0) {
                float[] single = { 
                    instancePos.Value.X, 
                    instancePos.Value.Y, 
                    instancePos.Value.Z 
                };
                
                GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);
                GL.BufferData(BufferTarget.ArrayBuffer, single.Length * sizeof(float), single, BufferUsageHint.DynamicDraw);
                
                if(instanceRotationVbo != 0 && cachedInstanceRotations.Count > instanceIndex) {
                    float[] rotation = { cachedInstanceRotations[instanceIndex], 0, 0, 0 };
                    GL.BindBuffer(BufferTarget.ArrayBuffer, instanceRotationVbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, rotation.Length * sizeof(float), rotation, BufferUsageHint.DynamicDraw);
                }

                if(instanceScaleVbo != 0 && cachedInstanceScales.Count > instanceIndex) {
                    float[] scaleData = { cachedInstanceScales[instanceIndex] };
                    GL.BindBuffer(BufferTarget.ArrayBuffer, instanceScaleVbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, scaleData.Length * sizeof(float), scaleData, BufferUsageHint.DynamicDraw);
                }

                drawCount = 1;
            }

            if(indices != null) {
                GL.DrawElementsInstanced(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, drawCount);
            } else {
                GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, vertexCount, drawCount);
            }

            if(instancePos.HasValue) {
                if(cachedInstancePositions.Count > 0) {
                    float[] posData = new float[cachedInstancePositions.Count * 3];
                    for(int i = 0; i < cachedInstancePositions.Count; i++) {
                        posData[i*3+0] = cachedInstancePositions[i].X;
                        posData[i*3+1] = cachedInstancePositions[i].Y;
                        posData[i*3+2] = cachedInstancePositions[i].Z;
                    }

                    GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, posData.Length * sizeof(float), posData, BufferUsageHint.DynamicDraw);
                }

                if(instanceRotationVbo != 0 && cachedInstanceRotations.Count > 0) {
                    float[] rotationData = new float[cachedInstanceRotations.Count * 4];
                    for(int i = 0; i < cachedInstanceRotations.Count; i++) {
                        rotationData[i*4+0] = cachedInstanceRotations[i];
                        rotationData[i*4+1] = 0; 
                        rotationData[i*4+2] = 0; 
                        rotationData[i*4+3] = 0;
                    }

                    GL.BindBuffer(BufferTarget.ArrayBuffer, instanceRotationVbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, rotationData.Length * sizeof(float), rotationData, BufferUsageHint.DynamicDraw);
                }

                if(instanceScaleVbo != 0 && cachedInstanceScales.Count > 0) {
                    float[] scaleData = cachedInstanceScales.ToArray();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, instanceScaleVbo);
                    GL.BufferData(BufferTarget.ArrayBuffer, scaleData.Length * sizeof(float), scaleData, BufferUsageHint.DynamicDraw);
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
        } else {
            shaderProgram.setUniform("isInstanced", 0);
            if(indices != null) {
                GL.DrawElements(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, 0);
            } else {
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
            }
        }


        GL.BindVertexArray(0);
        shaderProgram.unbind();
    }

    // Outline
    public void renderOutline(List<MeshRenderer> selectedMeshes, Vector3? instancePos = null, int instanceIndex = 0) {
        if(!visible) return;
        if(meshData == null || camera == null) return;

        /*
        Console.WriteLine($"Stencil Texture: {stencilTexture}");
        Console.WriteLine($"Stencil Depth Texture: {stencilDepthTexture}");
        Console.WriteLine($"Scene Depth Texture: {sceneDepthTexture}");
        */

        float[] prevClearColor = new float[4];
        GL.GetFloat(GetPName.ColorClearValue, prevClearColor);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, stencilFbo);
        GL.Viewport(0, 0, window.getWidth(), window.getHeight());
        GL.ClearColor(0, 0, 0, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach(var mesh in selectedMeshes) {
            mesh.renderFlat(instancePos, instanceIndex);
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.ClearColor(
            prevClearColor[0],
            prevClearColor[1],
            prevClearColor[2],
            prevClearColor[3]
        );

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        shaderProgram.bind();
        shaderProgram.setUniform("shaderType", 6);
        
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, stencilTexture);
        shaderProgram.setUniform("stencilTexture", 1);

        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, stencilDepthTexture);
        shaderProgram.setUniform("stencilDepthTexture", 2);

        GL.ActiveTexture(TextureUnit.Texture3);
        GL.BindTexture(TextureTarget.Texture2D, sceneDepthTexture);
        shaderProgram.setUniform("sceneDepthTexture", 3);

        shaderProgram.setUniform("canvasSize", (float)window.getWidth(), (float)window.getHeight());
        shaderProgram.setUniform("outlineColor", 0.0f, 1.0f, 0.0f, 1.0f);
        shaderProgram.setUniform("outlineSize", 5.0f);

        GL.BindVertexArray(quadVao);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        GL.BindVertexArray(0);

        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.ActiveTexture(TextureUnit.Texture0);

        GL.Disable(EnableCap.Blend);
        GL.DepthMask(true);
        GL.Enable(EnableCap.DepthTest);
        shaderProgram.unbind();
    }

    /**
     * 
     * Cleanup
     *
     */
    public void cleanup() {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        
        if(vbo != 0) GL.DeleteBuffer(vbo);
        if(normalVbo != 0) GL.DeleteBuffer(normalVbo);
        if(colorVbo != 0) GL.DeleteBuffer(colorVbo);
        if(texCoordsVbo != 0) GL.DeleteBuffer(texCoordsVbo);
        if(instanceVbo != 0) GL.DeleteBuffer(instanceVbo);
        if(instanceColorVbo != 0) GL.DeleteBuffer(instanceColorVbo);
        if(ebo != 0) GL.DeleteBuffer(ebo);
        if(vao != 0) GL.DeleteVertexArray(vao);
    }

    /**
     * 
     * Remove
     * 
     */
    public void removeInstance(int index) {
        if(index < 0 || index >= cachedInstancePositions.Count) return;

        cachedInstancePositions.RemoveAt(index);
        cachedInstanceColors.RemoveAt(index);
        cachedInstanceRotations.RemoveAt(index);
        if(cachedInstanceTexPaths.Count > index) cachedInstanceTexPaths.RemoveAt(index);
        if(cacheInstanceTexIds.Count > index) cacheInstanceTexIds.RemoveAt(index);
        cachedInstanceScales.RemoveAt(index);

        instanceCount = cachedInstancePositions.Count;

        if(instanceCount > 0) {
            updateInstanceData(
                cachedInstancePositions, 
                cachedInstanceColors, 
                cachedInstanceRotations, 
                cachedInstanceTexPaths,
                cacheInstanceTexIds,
                cachedInstanceScales
            );
        }
    }

    /**
    
        Data Entry
    
        */ 
    public void setId(string id) {
        this.id = id;
        
        Root.Data.getInstance().register(Root.DataType.MESH, this);
        ServerSnapshot.getInstance().register(Root.DataType.MESH, this);
    }

    public string getId() {
        return id;
    }

    public Dictionary<string, object> serialize() {
        var dict = new Dictionary<string, object> {
            ["id"] = id,
            ["meshType"] = meshType,
            ["x"] = position.X,
            ["y"] = position.Y,
            ["z"] = position.Z,
            ["texId"] = texId,
            ["texPath"] = texPath,
            ["isInstanced"] = isInstanced,
            ["r00"] = rotationMatrix.M11, ["r01"] = rotationMatrix.M12, ["r02"] = rotationMatrix.M13,
            ["r10"] = rotationMatrix.M21, ["r11"] = rotationMatrix.M22, ["r12"] = rotationMatrix.M23,
            ["r20"] = rotationMatrix.M31, ["r21"] = rotationMatrix.M32, ["r22"] = rotationMatrix.M33
        };
        if(isInstanced && cachedInstancePositions.Count > 0) {
            dict["instancePositions"] = cachedInstancePositions.Select(p => 
                new Dictionary<string, object> {
                    ["x"] = p.X, 
                    ["y"] = p.Y, 
                    ["z"] = p.Z
                }
            ).ToList();
        }

        return dict;
    }
}