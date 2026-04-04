/*

    -- Controller to handle and register
        all the Packets for both 
        Client and Server...

    */
namespace App.Root;
using App.Root.Packets;
using System.Net;

interface PacketHandler {
    PacketType getType();

    void handle(string json) {}
    void handle(string json, IPEndPoint remote) {}
}

enum Context {
    CLIENT,
    SERVER
}

class PacketController {
    public static Dictionary<PacketType, PacketHandler> clientHandlers = new();
    public static Dictionary<PacketType, PacketHandler> serverHandlers = new();

    // Register
    public static void register(PacketHandler handler, Context context) {
        if(context == Context.CLIENT) {
            clientHandlers[handler.getType()] = handler;
        } else if(context == Context.SERVER) {
            serverHandlers[handler.getType()] = handler;
        }
    }

    // Try Get
    public static bool tryGet(
        PacketType type,
        Context context,
        out PacketHandler? handler 
    ) {
        Dictionary<PacketType, PacketHandler> handlers;
        if(context == Context.CLIENT) {
            handlers = clientHandlers;
        } else {
            handlers = serverHandlers;
        }
        return handlers.TryGetValue(type, out handler);
    }
}