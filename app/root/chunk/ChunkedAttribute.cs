namespace App.Root.Chunk;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
class ChunkedAttribute : Attribute {
    public int RenderDistance { 
        get; 
    }

    public ChunkedAttribute(int renderDistance = 8) {
        RenderDistance = renderDistance;
    }
}