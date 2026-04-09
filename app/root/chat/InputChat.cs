namespace App.Root.Chat;
using App.Root.Packets;
using App.Root.Screen;
using OpenTK.Windowing.GraphicsLibraryFramework;

class InputChat {
    private ChatController chatController;
    private ScreenController screenController;
    private Network network;

    public InputChat(ScreenController screenController, Network network) {
        this.chatController = ChatController.getInstance();
        this.screenController = screenController;
        this.network = network;
    }

    public void onKeyDown(Keys key) {
        if(key == Keys.Escape) {
            if(chatController.isOpen()) {
                chatController.close();
                return;
            }
        }

        if(key == Keys.T && 
            !chatController.isOpen() && 
            screenController.isRunning()
        ) {
            chatController.open();
            return;
        }

        if(key == Keys.Enter && chatController.isOpen()) {
            string msg = chatController.getText().Trim();
            if(msg.Length > 0) {
                network.getClient()?.send(new PacketChat {
                    userId = network.userId,
                    username = network.username,
                    message = msg
                });
            }

            chatController.close();
            return;
        }

        if(chatController.isOpen()) {
            chatController.handleKey(key, 1);
            return;
        }
    }
}