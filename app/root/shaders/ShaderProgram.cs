namespace App.Root.Shaders;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class ShaderProgram {
    public readonly int programId;
    private Dictionary<string, int> uniformLocations = new();

    public ShaderProgram(List<ShaderModule> modules) {
        programId = GL.CreateProgram();
        if(programId == 0) throw new Exception("Could not create shader program");

        List<int> shaderIds = new();
        Dictionary<ShaderType, StringBuilder> sources = new();
        
        foreach(var data in modules) {
            string content = ShaderLoader.load(data.File);
            if(!sources.ContainsKey(data.Type)) sources[data.Type] = new StringBuilder();
            sources[data.Type].AppendLine(content);
            Console.WriteLine("Shader Loaded: " + sources + ": " + content);
        }
        foreach(var entry in sources) {
            int shaderId = createShader(entry.Value.ToString(), entry.Key);
            GL.AttachShader(programId, shaderId);
            shaderIds.Add(shaderId);
        }

        GL.LinkProgram(programId);
        GL.GetProgram(
            programId,
            GetProgramParameterName.LinkStatus,
            out int linkStatus
        );

        if(linkStatus == 0) throw new Exception("Error linking shader: " + GL.GetProgramInfoLog(programId));
        shaderIds.ForEach(id => GL.DeleteShader(id));
    }

    // Create Shader
    private int createShader(string source, ShaderType type) {
        int id = GL.CreateShader(type);
        if(id == 0) throw new Exception("Error creating shader type: " + type);

        GL.ShaderSource(id, source);
        GL.CompileShader(id);
        GL.GetShader(
            id,
            ShaderParameter.CompileStatus,
            out int status
        );

        if(status == 0) throw new Exception("Error compiling shader: " + GL.GetShaderInfoLog(id));
        return id;
    }

    ///
    /// 
    /// Set/Get Uniform
    /// 
    ///
    public void setUniform(string name, float x, float y) {
        int loc = getUniformLocation(name);
        if(loc != -1) GL.Uniform2(loc, x, y);
    }

    public void setUniform(string name, float x, float y, float z) {
        int loc = getUniformLocation(name);
        if(loc != -1) GL.Uniform3(loc, x, y, z);
    }

    public void setUniform(string name, int val) {
        int loc = getUniformLocation(name);
        if(loc != -1) GL.Uniform1(loc, val);
    }

    public void setUniform(string name, float val) {
        int loc = getUniformLocation(name);
        if(loc != -1) GL.Uniform1(loc, val);
    }

    public void setUniform(string name, Matrix4 matrix) {
        int loc = getUniformLocation(name);
        if(loc != -1) GL.UniformMatrix4(loc, false, ref matrix);
    }

    public void setUniform(string name, float x, float y, float z, float w) {
        int loc = getUniformLocation(name);
        if(loc != -1) GL.Uniform4(loc, x, y, z, w);
    }

    public int getUniformLocation(string name) {
        if(!uniformLocations.ContainsKey(name)) {
            uniformLocations[name] = GL.GetUniformLocation(programId, name);
        }
        return uniformLocations[name];
    }

    ///
    /// 
    /// Bind
    /// 
    /// 
    public void bind() => GL.UseProgram(programId);
    public void unbind() => GL.UseProgram(0);

    public void cleanup() {
        unbind();
        if(programId != 0) GL.DeleteProgram(programId);
        uniformLocations.Clear();
    }
}