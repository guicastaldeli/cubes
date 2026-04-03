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

        init();
        set();
    }

    public Network getNetwork() {
        return network;
    }

    ///
    /// Set
    /// 
    private void set() {
        GL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);
        GL.Viewport(0, 0, Window.WIDTH, Window.HEIGHT);

        GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
    }
    
    ///
    /// Update
    /// 
    private void update() {
        tick.update();
        window.updateTitle(tick.getTickCount(), tick.getFps());

        screenController.update();
        uiController.update();
    }

    /// 
    /// Render
    /// 
    private void render() {
        screenController.render();
        uiController.render();
    }

    ///
    /// Init
    /// 
    public void init() {
        scene = new Scene(
            window, 
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
            this,
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
    }
    
    ///
    /// Run
    /// 
    public void run() {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        window.run(() => {
            render();
            update();
        });
    }
}