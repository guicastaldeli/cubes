namespace App.Root;
using System.Collections.Concurrent;
using System.Text;
using App.Root.Packets;

class PacketReassember {
    private class ChunkBuffer {
        public Dictionary<int, byte[]> chunks = new();
        public int totalChunks;
        public PacketType originalType;
        public long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private ConcurrentDictionary<string, ChunkBuffer> buffers = new();
    private const int BUFFER_TIMEOUT_MS = 5000;

    // Try Reassemble
    public string? tryReassemble(PacketChunk chunk) {
        if(!buffers.ContainsKey(chunk.packetId)) {
            buffers[chunk.packetId] = new ChunkBuffer {
                totalChunks = chunk.totalChunks,
                originalType = chunk.originalType
            };
        }

        var buffer = buffers[chunk.packetId];
        byte[] payloadBytes = Convert.FromBase64String(chunk.payload);
        buffer.chunks[chunk.chunkIndex] = payloadBytes;
        if(buffer.chunks.Count == buffer.totalChunks) {
            var ordered = buffer.chunks
                .OrderBy(kvp => kvp.Key)
                .SelectMany(kvp => kvp.Value)
                .ToArray();

            buffers.TryRemove(chunk.packetId, out _);

            string reassembled = Encoding.UTF8.GetString(ordered);
            //Console.WriteLine($"Reassembled {chunk.originalType} packet from {chunk.totalChunks} chunks");
            return reassembled;
        }

        return null;
    }
    
    // Cleanup Stale
    public void cleanupStale() {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var stale = buffers
            .Where(kvp => now - kvp.Value.createdAt > BUFFER_TIMEOUT_MS)
            .Select(kvp => kvp.Key)
            .ToList();
        foreach(var key in stale) {
            if(buffers.TryRemove(key, out var buffer)) {
                Console.WriteLine($"Cleaned up incomplete packet: {buffer.chunks.Count}/{buffer.totalChunks} chunks");
            }
        }
    }
}