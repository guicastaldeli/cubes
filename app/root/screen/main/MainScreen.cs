namespace App.Root.Screen.Main;
using App.Root.Screen.Main.Client;
using App.Root.Screen.Main.Custom;
using App.Root.Screen.Main.Server;

class MainScreen : Screen {
    public static readonly string PATH = DIR + "main/main_screen.xml";
    
    private MainScreenAction mainScreenAction;

    public ClientDialog clientDialog;
    public ServerDialog serverDialog;
    public CustomMenu customMenu;

    public MainScreen() : 
    base(PATH, "main") {
        this.mainScreenAction = new MainScreenAction(screenController, this);

        this.clientDialog = new ClientDialog(this);
        this.serverDialog = new ServerDialog(this);
        this.customMenu = new CustomMenu(this);

        this.updateScreen();
    }

    // Get Main Screen Actions
    public MainScreenAction getMainScreenAction() {
        return mainScreenAction;
    }

    // Handle Action
    public override void handleAction(string action) {
        if(clientDialog.isActive()) {
            clientDialog.handleAction(action);
            return;
        }
        if(serverDialog.isActive()) {
            serverDialog.handleAction(action);
            return;
        }
        if(customMenu.isActive()) {
            customMenu.handleAction(action);
            return;
        }
        
        switch(action) {
            case "client":
                mainScreenAction.openClient();
                break;
            case "server":
                mainScreenAction.openServer();
                break;
            case "custom":
                mainScreenAction.openCustomMenu();
                break;
            case "id":
                mainScreenAction.generateTempId();
                break;
        }
    }

    // Handle Mouse Move
    public override void handleMouseMove(int mouseX, int mouseY) {
        if(clientDialog.isActive()) {
            clientDialog.handleMouseMove(mouseX, mouseY);
            return;
        }
        if(serverDialog.isActive()) {
            serverDialog.handleMouseMove(mouseX, mouseY);
            return;
        }
        if(customMenu.isActive()) {
            customMenu.handleMouseMove(mouseX, mouseY);
            return;
        }
        base.handleMouseMove(mouseX, mouseY);
    }

    // Handle Key Press
    public override void handleKeyPress(int key, int action) {
        if(clientDialog.isActive()) {
            clientDialog.handleKeyPress(key, action);
            return;
        }
        if(serverDialog.isActive()) {
            serverDialog.handleKeyPress(key, action);
            return;
        }
        if(customMenu.isActive()) {
            customMenu.handleKeyPress(key, action);
            return;
        }
        base.handleKeyPress(key, action);
    }

    // Check Click
    public override string? checkClick(int mouseX, int mouseY) {
        if(clientDialog.isActive()) {
            return clientDialog.checkClick(mouseX, mouseY);
        }
        if(serverDialog.isActive()) {
            return serverDialog.checkClick(mouseX, mouseY);
        }
        if(customMenu.isActive()) {
            return customMenu.checkClick(mouseX, mouseY);
        }
        return base.checkClick(mouseX, mouseY);
    }

    ///
    /// Update
    ///  
    public override void update() {
        if(clientDialog.isActive()) {
            clientDialog.update();
            return;
        }
        if(serverDialog.isActive()) {
            serverDialog.update();
            return;
        }
        if(customMenu.isActive()) {
            customMenu.update();
            return;
        }
        base.update();    
    }

    private void updateScreen() {
        mainScreenAction.refreshUsername();
        mainScreenAction.handleId();
    }

    ///
    /// Render
    /// 
    public override void render() {
        if(clientDialog.isActive()) {
            clientDialog.render();
            return;
        }
        if(serverDialog.isActive()) {
            serverDialog.render();
            return;
        }
        if(customMenu.isActive()) {
            customMenu.render();
            return;
        }
        base.render();
    }

    // Reset
    public void reset() {
        clientDialog.setActive(false);
        serverDialog.setActive(false);
        customMenu.setActive(false);

        show();
    }
}