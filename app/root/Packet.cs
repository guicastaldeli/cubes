namespace App.Root;
using App.Root._Sync;
using App.Root._Binary;
using App.Root._Crypto;

public class Packet {
    public string DataId { get; set; } = "";
    public string Action { get; set; }= "";
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    [SyncTimestamp] public long Timestamp { get; set; }
    public bool IsDelta { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public byte[]? Checksum { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsResponse { get; set; }
    public string? RequestId { get; set; }
    public bool IsControl { get; set; } = false;
    public bool IsHandshake { get; set; } = false;
    public bool IsHandshakeResponse { get; set; } = false;

    public Packet() {}
    public Packet(string DataId, string Action, byte[] Payload, bool IsDelta = false) {
        this.DataId = DataId;
        this.Action = Action;
        this.Payload = Payload;
        this.IsDelta =  IsDelta;
        this.Timestamp = DateTime.UtcNow.Ticks;
    }

    // To Bytes
    public byte[] ToBytes() {
        using var writer = new BinaryWriter();
        writer.Write(DataId ?? "");
        writer.Write(Action ?? "");
        writer.Write(Timestamp);
        writer.Write(IsDelta);
        writer.Write(UserId ?? "");
        writer.Write(SessionId ?? "");
        writer.Write(SequenceNumber);
        writer.Write(IsResponse);
        writer.Write(RequestId ?? "");
        writer.Write(IsControl);
        writer.Write(IsHandshake);
        writer.Write(IsHandshakeResponse);
        writer.Write(Payload.Length);
        if(Payload.Length > 0) {
            writer.WriteRaw(Payload);
        }
        writer.Write(Checksum?.Length ?? 0);
        if(Checksum != null && Checksum.Length > 0) {
            writer.WriteRaw(Checksum);
        }

        return writer.GetBytes();
    }

    // From Bytes
    public static Packet FromBytes(byte[] data) {
        using var reader = new BinaryReader(data);
        
        var packet = new Packet();
        packet.DataId = reader.ReadString();
        packet.Action = reader.ReadString();
        packet.Timestamp = reader.ReadLong();
        packet.IsDelta = reader.ReadBool();
        packet.UserId = reader.ReadString();
        packet.SessionId = reader.ReadString();
        packet.SequenceNumber = reader.ReadInt();
        packet.IsResponse = reader.ReadBool();
        packet.RequestId = reader.ReadString();
        packet.IsControl = reader.ReadBool();
        packet.IsHandshake = reader.ReadBool();
        packet.IsHandshakeResponse = reader.ReadBool();
        
        int payloadLength = reader.ReadInt();
        if(payloadLength > 0) {
            packet.Payload = reader.ReadBytes(payloadLength);
        } else {
            packet.Payload = Array.Empty<byte>();
        }

        int checksumLength = reader.ReadInt();
        if(checksumLength > 0) {
            packet.Checksum = reader.ReadBytes(checksumLength);
        } else {
            packet.Checksum = null;
        }


        return packet;
    }

    // Create Response
    public Packet CreateResponse(byte[] payload) {
        Packet val = new Packet {
            DataId = this.DataId,
            Action = this.Action + "_response",
            Payload = payload,
            Timestamp = this.Timestamp,
            IsDelta = this.IsDelta,
            UserId = this.UserId,
            SessionId = this.SessionId,
            SequenceNumber = this.SequenceNumber,
            IsResponse = true,
            RequestId = this.RequestId ?? Guid.NewGuid().ToString(),
            IsControl = this.IsControl,
            IsHandshake = this.IsHandshake,
            IsHandshakeResponse = this.IsHandshakeResponse
        };

        return val;
    }

    // Is Valid
    public bool IsValid() {
        if(string.IsNullOrEmpty(DataId)) return false;
        if(string.IsNullOrEmpty(Action)) return false;
        if(Timestamp <= 0) return false;
        /*if(Checksum != null && Checksum.Length > 0) { FIX LATER...
            if(!CryptoProvider.VerifyHash(Payload, Checksum)) {
                return false;
            }
        }*/

        //Console.WriteLine($"[Packet] Packet is valid!");
        return true;
    }
}