namespace App.Root.Chat;
using App.Root.Packets;

static class ServerMessage {
    /**

        --- User Alerts ---

        */
    public static readonly string USER_JOINED = 
        "{username} joined";

    public static readonly string USER_LEFT =
        "{username} left";

    // Format
    private static string format(string msg, string val) {
        int start = msg.IndexOf('{');
        int end = msg.IndexOf('}');
        if(start < 0 || end < 0) return msg;
        
        string res = msg[..start] + val + msg[(end+1)..];
        return res;
    }

    // Get
    public static PacketChat get(string msg, params string[] args) {
        foreach(var val in args) {
            msg = format(msg, val);
        }
        return new PacketChat {
            isServer = true,
            message = msg
        };
    }
}