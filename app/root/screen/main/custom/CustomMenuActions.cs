namespace App.Root.Screen.Main.Custom;

class CustomMenuActions {
    private CustomMenu customMenu;

    public CustomMenuActions(CustomMenu customMenu) {
        this.customMenu = customMenu;
    }

    // Back
    public void back() {
        customMenu.hide();
        customMenu.mainScreen.show();
    }
}