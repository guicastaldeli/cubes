namespace App.Root.Screen.Pause;
using App.Root.Utils;
using System.Reflection;

class PauseScreen : Screen {
    public const string ID = "pause";
    public static readonly string PATH = DIR + "pause/pause_screen.xml";
    
    public PauseScreenAction pauseScreenAction;

    public PauseScreen() : base(PATH, ID) {
        this.pauseScreenAction = new PauseScreenAction(
            tick, 
            input,
            screenController, 
            this,
            network
        );
    }

    // Check Click
    public override string? checkClick(int mouseX, int mouseY) {
        return base.checkClick(mouseX, mouseY);
    }

    /**
     * 
     * Handle
     *
     */
    // Handle Action
    public override void handleAction(string action) {
        var converted = ActionConverter.Convert(action);
            if(converted.MethodName == null) return;

            var method = pauseScreenAction.GetType().GetMethod(converted.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if(method != null) {
                var args = converted.Param != null ? new object[] { converted.Param } : null;
                method.Invoke(pauseScreenAction, args);
            }
    }

    // Handle Mouse Move
    public override void handleMouseMove(int mouseX, int mouseY) {
        base.handleMouseMove(mouseX, mouseY);
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        base.update();    
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        base.render();
    }
}