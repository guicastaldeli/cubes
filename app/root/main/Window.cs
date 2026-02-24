namespace App.Root.Main;
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
    }

    public void Run() {
        while(!IsExiting) {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            Context.SwapBuffers();
            ProcessEvents(0);
        }
    }
}