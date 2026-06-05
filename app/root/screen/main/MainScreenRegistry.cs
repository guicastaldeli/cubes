namespace App.Root.Screen.Main;
using App.Root.Utils;

abstract class MainScreenHandler : Screen {
    protected MainScreen mainScreen = null!;
    
    public MainScreenHandler(string path, string name) : base(path, name) {}
    
    public virtual void open() {}
    public virtual void close() {}
}

class MainScreenRegistry {
    private MainScreen mainScreen;

    private ServiceContainer ServiceContainer = new ServiceContainer();
    private List<MainScreenHandler> el = new();
    private bool isRegistered = false;

    public MainScreenRegistry(MainScreen mainScreen) {
        this.mainScreen = mainScreen;
        ServiceContainer.Register(mainScreen);
    }

    public void handleAction(string action) {
        foreach(var handler in el) {
            if(handler.isActive()) {
                handler.handleAction(action);
                return;
            }
        }
    }

    public void handleMouseMove(int mouseX, int mouseY) {
        foreach(var handler in el) {
            if(handler.isActive()) {
                handler.handleMouseMove(mouseX, mouseY);
                return;
            }
        }
    }

    public void handleKeyPress(int key, int action) {
        foreach(var handler in el) {
            if(handler.isActive()) {
                handler.handleKeyPress(key, action);
                return;
            }
        }
    }

    public string? checkClick(int mouseX, int mouseY) {
        foreach(var handler in el) {
            if(handler.isActive()) {
                return handler.checkClick(mouseX, mouseY);
            }
        }
        return null;
    }

    public void onWindowResize(int width, int height) {
        foreach(var handler in el) handler.onWindowResize(width, height);
    }

    public bool anyActive() {
        return el.Any(h => h.isActive());
    }

    /**
    
        Get
    
        */
    public T? get<T>() where T : MainScreenHandler {
        T? val = el.OfType<T>().FirstOrDefault();
        return val;
    }

    public List<MainScreenHandler> getAll() {
        return el;
    }

    /**
    
        Render
    
        */
    public void update() {
        foreach(var handler in el) {
            if(handler.isActive()) {
                handler.update();
            }
        }
    }

    /**
    
        Render
    
        */
    public void render() {
        foreach(var handler in el) {
            if(handler.isActive()) {
                handler.render();
            }
        }
    }

    /**
    
        Init
    
        */
    public void init() {
        if(isRegistered) return;

        var registry = new ClassRegistry(ServiceContainer);
        var result = registry.ORegister<MainScreenHandler>();
        
        el = result;
        foreach(var handler in el) {
            Screen.screenController.register(handler, mainScreen);
        }

        isRegistered = true;
    }
}