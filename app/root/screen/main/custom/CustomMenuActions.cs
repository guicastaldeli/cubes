namespace App.Root.Screen.Main.Custom;
using App.Root.Info;

class CustomMenuActions {
    private CustomMenu customMenu;

    public CustomMenuActions(CustomMenu customMenu) {
        this.customMenu = customMenu;
    }

    // Confirm
    public void confirm() {
        var inputEl = customMenu.inputField.getText("usernameInput");
        if(string.IsNullOrWhiteSpace(inputEl)) return;

        InfoController.getInstance().getUserInfo().setUsername(inputEl);
        customMenu.mainScreen.getMainScreenAction().refreshUsername();

        back();
    }

    // Back
    public void back() {
        customMenu.hide();
        customMenu.mainScreen.show();
    }
}