namespace App.Root._Sync;
using App.Root._Binary;
using App.Root._Crypto; 

public class SyncPacket {
    public string DataId { get; set; } = "";
    public string Action { get; set; }= "";
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public long Timestamp { get; set; }
    public bool IsDelta { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public byte[]? Checksum { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsResponse { get; set; }
    public string? RequestId { get; set; }

    public SyncPacket() {}
    public SyncPacket(string DataId, string Action, byte[] Payload, bool IsDelta = false) {
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
        writer.Write(Payload.Length);
        writer.Write(Timestamp);
        writer.Write(IsDelta);
        writer.Write(UserId ?? "");
        writer.Write(SessionId ?? "");
        writer.Write(Checksum?.Length ?? 0);
        writer.Write(SequenceNumber);
        writer.Write(IsResponse);
        writer.Write(RequestId ?? "");

        if(Payload.Length > 0) writer.Write(Payload);
        if(Checksum != null && Checksum.Length > 0) writer.Write(Checksum);

        return writer.GetBytes();
    }

    // From Bytes
    public static SyncPacket FromBytes(byte[] data) {
        using var reader = new BinaryReader(data);
        
        int payloadLength = reader.ReadInt();
        int checksumLength = reader.ReadInt();
        
        var packet = new SyncPacket();
        packet.DataId = reader.ReadString();
        packet.Action = reader.ReadString();
        if(payloadLength > 0) packet.Payload = reader.GetIBinaryReader().ReadBytes(payloadLength);
        packet.Timestamp = reader.ReadLong();
        packet.IsDelta = reader.ReadBool();
        packet.UserId = reader.ReadString();
        packet.SessionId = reader.ReadString();
        if(checksumLength > 0) packet.Checksum = reader.GetIBinaryReader().ReadBytes(checksumLength);
        packet.SequenceNumber = reader.ReadInt();
        packet.IsResponse = reader.ReadBool();
        packet.RequestId = reader.ReadString();

        return packet;
    }

    // Create Response
    public SyncPacket CreateResponse(byte[] payload) {
        SyncPacket val = new SyncPacket {
            DataId = this.DataId,
            Action = this.Action + "_response",
            Payload = this.Payload,
            Timestamp = this.Timestamp,
            IsDelta = this.IsDelta,
            UserId = this.UserId,
            SessionId = this.SessionId,
            SequenceNumber = this.SequenceNumber,
            IsResponse = true,
            RequestId = this.RequestId ?? Guid.NewGuid().ToString()
        };

        return val;
    }

    // Is Valid
    public bool IsValid() {
        if(string.IsNullOrEmpty(DataId)) return false;
        if(string.IsNullOrEmpty(Action)) return false;
        if(Timestamp <= 0) return false;
        if(Checksum != null && Checksum.Length > 0) {
            if(!CryptoProvider.VerifyHash(Payload, Checksum)) {
                return false;
            }
        }

        return true;
    }
}