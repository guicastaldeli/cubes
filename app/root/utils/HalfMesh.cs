using App.Root.Mesh;
using OpenTK.Mathematics;

namespace App.Root.Utils;

public static class HalfMesh {
    /**

        Half Height
    
        */
    public static float HalfHeight(MeshData data, Vector3 scale) {
        float[]? vertices = data.getVertices();
        if(vertices == null) return 0.5f;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for(int i = 1; i < vertices.Length; i += 3) {
            if(vertices[i] < minY) minY = vertices[i];
            if(vertices[i] > maxY) maxY = vertices[i];
        }

        float meshHeight = (maxY - minY) * scale.Y;
        
        if(data.isModel) return meshHeight;
        return meshHeight / 2.0f;
    }

    /**

        Half Extents
    
        */
    public static Vector3 HalfExtents(MeshData data, Vector3 scale) {
        float[]? vertices = data.getVertices();
        if(vertices == null) return Vector3.One * 0.5f;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        for(int i = 0; i < vertices.Length; i += 3) {
            if(vertices[i]   < minX) minX = vertices[i];
            if(vertices[i]   > maxX) maxX = vertices[i];
            if(vertices[i+1] < minY) minY = vertices[i+1];
            if(vertices[i+1] > maxY) maxY = vertices[i+1];
            if(vertices[i+2] < minZ) minZ = vertices[i+2];
            if(vertices[i+2] > maxZ) maxZ = vertices[i+2];
        }

        return new Vector3(
            (maxX - minX) * scale.X / 2.0f,
            (maxY - minY) * scale.Y / 2.0f,
            (maxZ - minZ) * scale.Z / 2.0f
        );
    }
}