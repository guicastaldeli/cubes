/**

    Packet Chunking to fragment
    the packet data into chunks to
    handle more data coming.


    */
namespace App.Root;
using App.Root.Packets;
using System.Text;

class PacketChuncking {
    private const int MAX_CHUNK_SIZE = 1200;
    private const int HEADER_OVERHEAD = 300;
    private const int MAX_PAYLOAD_SIZE = MAX_CHUNK_SIZE - HEADER_OVERHEAD;

    // Needs Chunking
    public static bool needsChunking(string json) {
        bool val = Encoding.UTF8.GetByteCount(json) > MAX_PAYLOAD_SIZE;
        return val;
    }
    
    /*
    
        Chunk
    
        */
    public static List<Packet> chunk(Packet packet) {
        string json = packet.serialize();
        byte[] data = Encoding.UTF8.GetBytes(json);
        if(data.Length <= MAX_PAYLOAD_SIZE) {
            return new List<Packet> { packet };
        }

        //Console.WriteLine($"Chunking {packet.type} packet: {data.Length} bytes into {Math.Ceiling((double)data.Length / MAX_PAYLOAD_SIZE)} chunks");

        var chunks = new List<Packet>();
        string packetId = Guid.NewGuid().ToString();
        int totalChunks = (int)Math.Ceiling((double)data.Length / MAX_PAYLOAD_SIZE);

        for(int i = 0; i < totalChunks; i++) {
            int offset = i * MAX_PAYLOAD_SIZE;
            int length = Math.Min(MAX_PAYLOAD_SIZE, data.Length - offset);
            byte[] chunkData = new byte[length];
            Array.Copy(data, offset, chunkData, 0, length);

            chunks.Add(new PacketChunk {
                packetId = packetId,
                chunkIndex = i,
                totalChunks = totalChunks,
                payload = Convert.ToBase64String(chunkData),
                originalType = packet.type,
                userId = packet.userId
            });
        }

        return chunks;
    }
}