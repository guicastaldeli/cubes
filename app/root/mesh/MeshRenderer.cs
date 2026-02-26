namespace App.Root.Mesh;
using App.Root.Player;
using App.Root.Shaders;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

class MeshRenderer {
    public readonly ShaderProgram shaderProgram;
    private MeshData? meshData;
    private Camera? camera;

    private int vao;
    private int vbo;
    private int ebo;
    private int colorVbo;
    private int normalVbo;
    private int texCoordsVbo;
    private int vertexCount;

    private Matrix4 modelMatrix = Matrix4.Identity;
    private Matrix4 rotationMatrix = Matrix4.Identity;

    private Vector3 position = Vector3.Zero;
    private Vector3 scale = Vector3.One;
    private float rotationAngle = 0.0f;

    private bool hasScale = false;
    private bool isDynamic = false;
    private bool hasColors = false;
    private bool hasTex = false;
    private int texId = -1;

    public MeshRenderer(ShaderProgram shaderProgram) {
        this.shaderProgram = shaderProgram;
    }

    // Set Data
    public void setData(MeshData data) {
        meshData = data;
        isDynamic = data.isDynamic;
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

    // Cleanup
    public void cleanup() {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        
        if(vbo != 0) GL.DeleteBuffer(vbo);
        if(normalVbo != 0) GL.DeleteBuffer(normalVbo);
        if(colorVbo != 0) GL.DeleteBuffer(colorVbo);
        if(texCoordsVbo != 0) GL.DeleteBuffer(texCoordsVbo);
        if(ebo != 0) GL.DeleteBuffer(ebo);
        if(vao != 0) GL.DeleteVertexArray(vao);
    }
}