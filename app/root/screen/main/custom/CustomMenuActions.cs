namespace App.Root.Screen.Main.Custom;
using App.Root.Info;
using App.Root.Utils;
using App.Root.Input;

class CustomMenuActions {
    private static List<string> Elements = new() {
        "usernameInput"
    };

    private CustomMenu customMenu;

    public CustomMenuActions(CustomMenu customMenu) {
        this.customMenu = customMenu;
    }

    /**
     * 
     * Get
     *
     */
    public dynamic get() {
        return ElementEntry.C(id => customMenu.getElementById(id), Elements);
    }

    // Confirm
    [GlobalInput]
    public void confirm() {
        string inputEl = get().usernameInput.text;
        if(string.IsNullOrWhiteSpace(inputEl)) return;

        InfoController.getInstance().getUserInfo().setUsername(inputEl);
        customMenu.getMainScreen().getMainScreenAction().refreshUsername();

        back();
    }

    // Back
    [GlobalInput]
    public void back() {
        customMenu.hide();
        customMenu.getMainScreen().show();
    }
}