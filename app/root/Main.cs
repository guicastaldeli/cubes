namespace App.Root;
using App.Root.Animation;
using App.Root.Scene;
using App.Root.Screen;
using App.Root.Screen.Main;
using App.Root.Shaders;
using App.Root.UI;
using App.Root.Voip;
using OpenTK.Graphics.OpenGL;

class Main {
    private Window window;
    private Tick tick;
    private ShaderProgram shaderProgram;
    private Input.Input input;
    private Mesh.Mesh mesh;
    private Network network;

    private MainScene mainScene = null!;
    private ScreenController screenController = null!;
    private UIController uiController = null!;

    public Main() {
        this.window = new Window();
        this.tick = new Tick();
        this.shaderProgram = new ShaderProgram();
        this.input = new Input.Input(window, tick);

        this.mesh = new Mesh.Mesh(
            window, 
            shaderProgram, 
            input
        );

        this.network = new Network();
        VoiceController.getInstance().setNetwork(network);

        this.window.onResize = handleResize;

        init();
        set();
    }

    // Handle Resize
    public void handleResize(int width, int height) {
        GL.Viewport(0, 0, width, height);

        mainScene?.onWindowResize(width, height);
        
        uiController.onWindowResize(width, height);
        screenController.onWindowResize(width, height);
    }

    // Switch to Main Screen
    private void switchToMainScreen() {
        screenController.switchTo(MainScreen.ID);
    }

    /**
     * 
     * Set
     *
     */
    private void set() {        
        GL.Viewport(0, 0, Window.WIDTH, Window.HEIGHT);

        GL.Enable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
    }

    private void glColor() {
        /* TEST COLORS ~~-~~-~-~- */
        if(Controller.getInstance(Instance.PROD)) {
            GL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);
        } else if(Controller.getInstance(Instance.DEV)) {
            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        } else if(Controller.getInstance(Instance.DEBUG)) {
            GL.ClearColor(6.0f, 2.5f, 0.5f, 1.0f);
        }
        /* ~]~]]]~]] */
    }
    
    /**
     * 
     * Update
     *
     */
    private void update() {
        tick.update();
        window.updateTitle(tick.getTickCount(), tick.getFps());

        screenController.update();
        uiController.update();
        AnimationController.Update();
    }

    /**
     * 
     * Render
     *
     */
    private void render() {
        screenController.render();
        uiController.render();
    }

    /**
     * 
     * Init
     *
     */
    public void init() {
        mainScene = new MainScene(
            window, 
            tick,
            shaderProgram, 
            input,
            mesh
        );
        mainScene.setNetwork(network);
        
        screenController = new ScreenController(
            tick,
            input,
            window,
            shaderProgram,
            mainScene,
            network,
            Window.WIDTH, Window.HEIGHT
        );
        switchToMainScreen();

        mainScene.setScreenController(screenController);

        uiController = new UIController(
            shaderProgram,
            input,
            mesh,
            Window.WIDTH, Window.HEIGHT
        );
        
        input.setScreenController(screenController);
        input.setUIController(uiController);
        input.setNetwork(network);
        input.init();

        mainScene.setUIController(uiController);
    }

    /**
     *
     * Clear
     *
     */
    private void clear() {
        glColor();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
    
    /**
     * 
     * Run
     *
     */
    public void run() {
        window.run(() => {
            clear();
            render();
            update();
        });
    }
}