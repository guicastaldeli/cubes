namespace App.Root.Chunk;
using System.Reflection;

/**

    Chunk Attribute per Chunk

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
class ChunkedAttribute : Attribute {
    public int RenderDistance { 
        get; 
    }

    public ChunkedAttribute(int renderDistance = 8) {
        RenderDistance = renderDistance;
    }

    /**
     * 
     * Register
     *
     */
    public static void R(ChunkHandler handler, List<ChunkHandler> chunkedHandlers, Dictionary<ChunkHandler, HashSet<ChunkCoord>> handlerActiveChunks) {
        var attr = handler.GetType().GetCustomAttribute<ChunkedAttribute>();

        if(attr != null) {
            chunkedHandlers.Add(handler);
            handlerActiveChunks[handler] = new HashSet<ChunkCoord>();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[ChunkManager] Chunked: {handler.GetType().Name}");
            Console.ResetColor();
        }
    }
}

/**

    Chunk Attribute per Instance

    */
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
class IChunkedAttribute : Attribute {
    /**
     * 
     * Register
     *
     */
    public static void R(ChunkHandler handler, List<ChunkHandler> globalHandlers) {
        var attr = handler.GetType().GetCustomAttribute<IChunkedAttribute>();
        
        if(attr != null) {
            globalHandlers.Add(handler);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"[ChunkManager] Global: {handler.GetType().Name}");
            Console.ResetColor();
        }
    }
}