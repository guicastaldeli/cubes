namespace App.Root.Mesh;
using App.Root.Player;
using App.Root.Shaders;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

class MeshRenderer : DataEntry {
    public readonly ShaderProgram shaderProgram;
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

    private int instanceVbo = 0;
    private int instanceCount = 0;
    public bool isInstanced = false;
    private List<Vector3> cachedInstancePositions = new();

    private Matrix4 modelMatrix = Matrix4.Identity;
    private Matrix4 rotationMatrix = Matrix4.Identity;

    private Vector3 position = Vector3.Zero;
    private Vector3 scale = Vector3.One;

    private bool hasScale = false;
    private bool isDynamic = false;
    private bool hasColors = false;
    private bool hasTex = false;
    private int texId = -1;
    private string texPath = "";

    private string id = "";
    private bool networkControlled = false;

    private bool visible = true;

    public MeshRenderer(ShaderProgram shaderProgram) {
        this.shaderProgram = shaderProgram;
    }

    // Set Data
    public void setData(MeshData data) {
        meshData = data;
        isDynamic = data.isDynamic;
        meshType = data.meshType;
        createBuffers();
    } 

    // Camera
    public void setCamera(Camera camera) {
        this.camera = camera;
    }

    public Camera? getCamera() {
        return camera;
    }

    // Position
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
        GL.BindVertexArray(0);
    }

    // Scale
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

    // Rotation Matrix
    public void setRotationMatrix(Matrix4 matrix) {
        rotationMatrix = matrix;
    }

    // Texture
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

    // Model Matrix
    public void setModelMatrix(Matrix4 m) {
        modelMatrix = m;
    }

    // Colors
    public void updateColors(float[] colors) {
        if(colorVbo == 0) return;
        GL.BindBuffer(BufferTarget.ArrayBuffer, colorVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.DynamicDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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

    // Network Controlled
    public void setNetworkControlled(bool val) {
        networkControlled = val;
    }

    // Set Visible
    public void setVisible(bool visible) {
        this.visible = visible;
    }

    // Create Buffers
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

    ///
    /// Update
    /// 
    public void update() {
        if(meshData == null) return;
        if(meshData.hasRotation() == true) updateRotation();
    }

    ///
    /// Render
    /// 
    public void render() {
        if(!visible) return;
        if(meshData == null || camera == null) return;

        modelMatrix = rotationMatrix * Matrix4.CreateTranslation(position);
        if(!isDynamic) {
            if(hasScale) {
                modelMatrix *= Matrix4.CreateScale(scale);
            } else if(meshData.hasScale()) {
                float[]? s = meshData.getScale();
                if(s != null) modelMatrix *= Matrix4.CreateScale(s[0], s[1], s[2]);
            }
        }

        shaderProgram.bind();
        shaderProgram.setUniform("shaderType", 0);
        shaderProgram.setUniform("uModel", modelMatrix);
        shaderProgram.setUniform("uView", camera.getView());
        shaderProgram.setUniform("uProjection", camera.getProjection());
        shaderProgram.setUniform("uHasColors", hasColors ? 1 : 0);
        shaderProgram.setUniform("hasTex", hasTex ? 1 : 0);
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

    public void renderInstanced() {
        if(!visible || meshData == null || camera == null) return;

        shaderProgram.bind();
        shaderProgram.setUniform("shaderType", 0);
        shaderProgram.setUniform("uModel", Matrix4.Identity);
        shaderProgram.setUniform("uView", camera.getView());
        shaderProgram.setUniform("uProjection", camera.getProjection());
        shaderProgram.setUniform("uHasColors", hasColors ? 1 : 0);
        shaderProgram.setUniform("hasTex", 0);
        shaderProgram.setUniform("isInstanced", 1);

        GL.BindVertexArray(vao);
        int[]? indices = meshData.getIndices();
        if(indices != null) {
            GL.DrawElementsInstanced(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, instanceCount);
        } else {
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, vertexCount, instanceCount);
        }
        
        GL.BindVertexArray(0);
        shaderProgram.unbind();
    }

    // Cleanup
    public void cleanup() {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        
        if(vbo != 0) GL.DeleteBuffer(vbo);
        if(normalVbo != 0) GL.DeleteBuffer(normalVbo);
        if(colorVbo != 0) GL.DeleteBuffer(colorVbo);
        if(texCoordsVbo != 0) GL.DeleteBuffer(texCoordsVbo);
        if(instanceVbo != 0) GL.DeleteBuffer(instanceVbo);
        if(ebo != 0) GL.DeleteBuffer(ebo);
        if(vao != 0) GL.DeleteVertexArray(vao);
    }

    ///
    /// Data Entry
    /// 
    public void setId(string id) {
        this.id = id;
        Data.getInstance().register(Root.DataType.MESH, this);
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