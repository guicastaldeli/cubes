namespace App.Root;
using App.Root.Screen;
using App.Root.Shaders;
using App.Root.UI;
using App.Root.Voip;
using OpenTK.Graphics.OpenGL;

class Main {
    private Window window;
    private Tick tick;
    private ShaderProgram shaderProgram;
    private Input input;
    private Network network;

    private Scene scene = null!;
    private ScreenController screenController = null!;
    private UIController uiController = null!;

    public Main() {
        window = new Window();
        tick = new Tick();
        shaderProgram = new ShaderProgram();
        input = new Input(window, tick);

        network = new Network();
        VoiceController.getInstance().setNetwork(network);

        window.onResize = handleResize;

        init();
        set();
    }

    // Handle Resize
    public void handleResize(int width, int height) {
        GL.Viewport(0, 0, width, height);

        scene?.onWindowResize(width, height);
        
        uiController.onWindowResize(width, height);
        screenController.onWindowResize(width, height);
    }

    /**
    
        Set

        */
    private void set() {
        if(Controller.getInstance(Instance.PROD)) {
            GL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);
        } else if(Controller.getInstance(Instance.DEV)) {
            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        } else if(Controller.getInstance(Instance.DEBUG)) {
            GL.ClearColor(6.0f, 2.5f, 0.5f, 1.0f);
        }
        
        GL.Viewport(0, 0, Window.WIDTH, Window.HEIGHT);

        GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
    }
    
    /**
    
        Update

        */
    private void update() {
        tick.update();
        window.updateTitle(tick.getTickCount(), tick.getFps());

        screenController.update();
        uiController.update();
    }

    /**
    
        Render

        */ 
    private void render() {
        screenController.render();
        uiController.render();
    }

    /**
    
        Init

        */
    public void init() {
        scene = new Scene(
            window, 
            tick,
            shaderProgram, 
            input
        );
        scene.setNetwork(network);
        
        screenController = new ScreenController(
            tick,
            input,
            window,
            shaderProgram,
            scene,
            network,
            Window.WIDTH, Window.HEIGHT
        );
        screenController.switchTo(ScreenController.SCREENS.MAIN);

        scene.setScreenController(screenController);

        uiController = new UIController(
            shaderProgram,
            Window.WIDTH, Window.HEIGHT
        );
        
        input.setScreenController(screenController);
        input.setUIController(uiController);
        input.setNetwork(network);
        input.init();

        scene.setUIController(uiController);
    }
    
    /**
    
        Run

        */
    public void run() {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        window.run(() => {
            render();
            update();
        });
    }
}