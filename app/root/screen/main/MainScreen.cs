namespace App.Root.Screen.Main;
using App.Root.Screen.Main.Client;
using App.Root.Screen.Main.Server;

class MainScreen : Screen {
    public static readonly String PATH = DIR + "main/main_screen.xml";
    
    public MainScreenAction mainScreenAction;

    public ClientDialog clientDialog;
    public ServerDialog serverDialog;

    public MainScreen() : 
    base(PATH, "main") {
        this.mainScreenAction = new MainScreenAction(screenController, this);

        this.clientDialog = new ClientDialog(this);
        this.serverDialog = new ServerDialog();
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
        
        switch(action) {
            case "client":
                mainScreenAction.openClient();
                break;
            case "server":
                mainScreenAction.openServer();
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
        base.handleMouseMove(mouseX, mouseY);
    }

    // Check Click
    public override string? checkClick(int mouseX, int mouseY) {
        if(clientDialog.isActive()) return clientDialog.checkClick(mouseX, mouseY);
        if(serverDialog.isActive()) return serverDialog.checkClick(mouseX, mouseY);
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
        base.update();    
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
        base.render();
    }
}