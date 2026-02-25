namespace App.Root;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

class Window : NativeWindow {
    private static int width = 800;
    private static int height = 600;
    private static string title = "build";
    
    public Window() : base(new NativeWindowSettings() {
        ClientSize = new Vector2i(width, height),
        Title = title,
        API = ContextAPI.OpenGL
    }) {
        Context.MakeCurrent();
        GL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);
        GL.Viewport(0, 0, width, height);
    }

    public void Run() {
        while(!IsExiting) {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            Context.SwapBuffers();
            ProcessEvents(0);
        }
    }
}