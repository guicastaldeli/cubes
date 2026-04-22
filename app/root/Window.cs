namespace App.Root;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Concurrent;

class Window : NativeWindow {
    public static int WIDTH = 800;
    public static int HEIGHT = 600;
    public static string TITLE = "client";

    private volatile Action? pendingAction = null;
    private ConcurrentQueue<Action> pendingActions = new();
    private List<Action> persistentRenderActions = new();

    public Action<Keys>? onKeyDown;
    public Action<Keys>? onKeyUp;

    public Action<int, int>? onMouseMove;
    public Action<int, int>? onMouseClick;
    public Action<int, bool>? onMouseButton;

    public Action<int, int>? onResize;

    public Window() : base(new NativeWindowSettings() {
        ClientSize = new Vector2i(WIDTH, HEIGHT),
        Title = TITLE,
        API = ContextAPI.OpenGL
    }) {
        Context.MakeCurrent();
        
        // Key
        KeyDown += args => onKeyDown?.Invoke(args.Key);
        KeyUp += args => onKeyUp?.Invoke(args.Key);
        
        // Mouse
        MouseMove += args => onMouseMove?.Invoke((int)args.X, (int)args.Y);
        MouseDown += args => {
            if(args.Button == MouseButton.Left) {
                onMouseClick?.Invoke((int)MousePosition.X, (int)MousePosition.Y);
            }
        };
        MouseDown += args => onMouseButton?.Invoke((int)args.Button, true);
        MouseUp += args => onMouseButton?.Invoke((int)args.Button, false);
    
        // Window
        Resize += args => {
            WIDTH = args.Width;
            HEIGHT = args.Height;
            queueOnRenderThread(() => {
                GL.Viewport(0, 0, WIDTH, HEIGHT);
                onResize?.Invoke(WIDTH, HEIGHT);
            });
        };
    }

    // Width and Height 
    public int getWidth() {
        return WIDTH;
    }

    public int getHeight() {
        return HEIGHT;
    }

    /**
    
        Render
    
        */
    public void queueOnRenderThread(Action action) {
        pendingActions.Enqueue(action);
    }

    public void addPersistentAction(Action action) {
        persistentRenderActions.Add(action);
    }

    /**
    
        Run
    
        */
    public void run(Action renderCallback) {
        Thread thread = new Thread(() => {
            Context.MakeCurrent();

            while(!IsExiting) {
                while(pendingActions.TryDequeue(out var action)) action();

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                renderCallback();

                foreach(var action in persistentRenderActions) action();

                Context.SwapBuffers();
            }
        });

        Context.MakeNoneCurrent();
        thread.Start();
        while(!IsExiting) ProcessEvents(0.016);
        thread.Join();
    }

    /**
    
        Update
    
        */
    public void updateTitle(int tickCount, int fps) {
        if(Controller.getInstance(Instance.PROD)) {
            Title = TITLE;
        } else {
            Title = 
                TITLE + 
                $" ({Controller.getCurrentName()})" +
                " / Tick: " + tickCount +
                " / FPS: " + fps;
        }
    }
}