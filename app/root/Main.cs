namespace App.Root;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

class Main {
    private Window window;

    public Main() {
        window = new Window();
        render();
    }

    private void render() {
        GL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);
        GL.Viewport(0, 0, Window.WIDTH, Window.HEIGHT);
    }
    
    public void run() {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        window.run();
    }
}