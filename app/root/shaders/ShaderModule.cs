namespace App.Root.Shaders;
using OpenTK.Graphics.OpenGL;

class ShaderModule {
    public ShaderType Type {
        get;
    }

    public string File {
        get;
    }

    public ShaderModule(ShaderType type, string file) {
        Type = type;
        File = file;
    }
}