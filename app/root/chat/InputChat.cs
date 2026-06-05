namespace App.Root.Chat;
using App.Root.Packets;
using App.Root.Player;
using App.Root.Screen;
using OpenTK.Windowing.GraphicsLibraryFramework;

/**

    Chat Actions

    */
enum ChatActions {
    CLOSE,
    OPEN,
    SEND
}

/**

    Input Chat main class

    */
class InputChat {
    private ChatController chatController;
    private ScreenController screenController;
    private Network network;

    private Dictionary<Keys, ChatActions>? map;

    public InputChat(ScreenController screenController, Network network) {
        this.chatController = ChatController.getInstance();
        this.screenController = screenController;
        this.network = network;

        Mapper.Set<InputChat>();
    }

    // Close
    private void close() {
        if(chatController.isOpen()) {
            chatController.close();
        }
    }

    // Open
    private void open() {
        if(!chatController.isOpen() && screenController.isRunning()) {
            chatController.open();
        }
    }

    // Send
    private void send() {
        if(!chatController.isOpen()) return;

        string msg = chatController.getText().Trim();
        if(msg.Length > 0) {
            network.getClient()?.send(new PacketChat {
                userId = network.userId,
                username = network.username,
                message = msg
            });
        }

        chatController.close();
    }

    /**
    
        Handle Action
    
        */
    private void handleAction(ChatActions action) {
        switch(action) {
            case ChatActions.CLOSE: 
                close();
                break;
            case ChatActions.OPEN:
                open();
                break;
            case ChatActions.SEND:
                send();
                break;
        }
    }

    /**
    
        On Key Down
    
        */
    public void onKeyDown(Keys key) {
        map ??= new() {
            { Keys.Escape, ChatActions.CLOSE },
            { Keys.T, ChatActions.OPEN },
            { Keys.Enter, ChatActions.SEND }
        };

        if(map.TryGetValue(key, out var action)) {
            Mapper.Key(key);
            handleAction(action);
            return;
        }

        if(chatController.isOpen()) {
            chatController.handleKey(key, KeyAction.Press);
        }
    }
}