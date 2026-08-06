namespace App.Root._Sync;

using System.Net;
using App.Root._Binary;

/**

    Handshake Packet

    */
[StoreData("handshake")]
public class HandshakePacket {
    [StoreField("session_id")] public string SessionId { get; set; } = "";
    [StoreField("key")] public byte[] Key { get; set; } = new byte[32];
    [StoreField("iv")] public byte[] Iv { get; set; } = new byte[16];
    [StoreField("timestamp")] public long Timestamp { get; set; }
    
    /**
     *
     * Response
     *
     */
    public static Packet Response(SyncManager syncManager) {
        var packet = new Packet {
            DataId = "__handshake-response__",
            Action = "__handshake-response__",
            Payload = Array.Empty<byte>(),
            Timestamp = DateTime.UtcNow.Ticks,
            IsDelta = false,
            SessionId = syncManager.GetSessionId(),
            IsControl = true,
            IsHandshake = false,
            IsHandshakeResponse = true
        };

        return packet;
    }

    /**
     *
     * Send
     *
     */
    public static Packet Send(SyncManager syncManager, IPEndPoint endPoint) {
        var handshake = syncManager.GetHandshakePacket();

        using var writer = new BinaryWriter();
        writer.Write(handshake.SessionId);
        writer.WriteRaw(handshake.Key);
        writer.WriteRaw(handshake.Iv);
        writer.Write(handshake.Timestamp);

        var payload = writer.GetBytes();
        var packet = new Packet {
            DataId = "__handshake__",
            Action = "__handshake__",
            Payload = payload,
            Timestamp = DateTime.UtcNow.Ticks,
            IsDelta = false,
            SessionId = handshake.SessionId,
            IsControl = true,
            IsHandshake = true,
            IsHandshakeResponse = false
        };

        Console.BackgroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[Server] Sending handshake to {endPoint}");
        Console.ResetColor();

        return packet;
    }

    /**
     *
     * Handle
     *
     */
    public static void Handle(Packet packet, SyncManager syncManager, Action<Packet> sendPacket) {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"[Client] Received handshake from server");
        Console.ResetColor();

        using var reader = new BinaryReader(packet.Payload);
        var sessionId = reader.ReadString();
        var key = reader.ReadBytes(32);
        var iv = reader.ReadBytes(16);
        var timestamp = reader.ReadLong();

        var handshake = new HandshakePacket {
            SessionId = sessionId,
            Key = key,
            Iv = iv,
            Timestamp = timestamp
        };

        syncManager.ApplyHandshake(handshake);

        Packet response = Response(syncManager);
        sendPacket(response);

        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"[Client] Handshake applied, sending confirmation");
        Console.ResetColor();
    }
}