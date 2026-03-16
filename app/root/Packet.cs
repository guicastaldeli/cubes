namespace App.Root;
using System.Text.Json;
using App.Root.Packets;

class Packet {
    public PacketType type {
        get;
        set;
    }

    public string? playerId {
        get;
        set;
    }

    // Serialize
    public string serialize() {
        return JsonSerializer.Serialize(this, GetType());
    }

    // Deserialize
    public static T? deserialize<T>(string json) where T : Packet {
        return JsonSerializer.Deserialize<T>(json);
    }

    public static PacketType? peekType(string json) {
        try {
            var doc = JsonDocument.Parse(json);
            if(doc.RootElement.TryGetProperty("type", out var typeProp)) {
                return (PacketType)typeProp.GetInt32();
            }
        } catch {
            Console.WriteLine("err");
        }
        
        return null;
    }
}