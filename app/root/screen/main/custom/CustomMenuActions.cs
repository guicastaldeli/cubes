namespace App.Root.Screen.Main.Custom;
using App.Root.Info;
using App.Root.Utils;

class CustomMenuActions {
    private static List<string> Elements = new() {
        "usernameInput"
    };

    private CustomMenu customMenu;

    public CustomMenuActions(CustomMenu customMenu) {
        this.customMenu = customMenu;
    }

    /**
    
        Get
    
        */
    public dynamic get() {
        return ElementEntry.C(id => customMenu.getElementById(id), Elements);
    }

    // Confirm
    public void confirm() {
        ScreenElement inputEl = get().usernameInput;
        string text = inputEl.getText();
        if(string.IsNullOrWhiteSpace(text)) return;

        InfoController.getInstance().getUserInfo().setUsername(text);
        customMenu.mainScreen.getMainScreenAction().refreshUsername();

        back();
    }

    // Back
    public void back() {
        customMenu.hide();
        customMenu.mainScreen.show();
    }
}