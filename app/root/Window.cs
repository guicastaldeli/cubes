namespace App.Root;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

class Window : NativeWindow {
    public static readonly int WIDTH = 800;
    public static readonly int HEIGHT = 600;
    public static readonly string TITLE = "build";
    
    public Window() : base(new NativeWindowSettings() {
        ClientSize = new Vector2i(WIDTH, HEIGHT),
        Title = TITLE,
        API = ContextAPI.OpenGL
    }) {
        Context.MakeCurrent();
    }

    public void updateTitle(int fps) {
        Title = TITLE + " / FPS: " + fps;
    }

    public void run(Action renderCallback) {
        Thread thread = new Thread(() => {
            Context.MakeCurrent();
            while(!IsExiting) {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                renderCallback();
                Context.SwapBuffers();
            }
        });

        Context.MakeNoneCurrent();
        thread.Start();
        while(!IsExiting) ProcessEvents(0.016);
        
        thread.Join();
    }
}