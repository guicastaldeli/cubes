/**

    Entity Factory class

    */
namespace App.Root.World.Entity;
using App.Root.Mesh;
using App.Root.Utils;
using OpenTK.Mathematics;

/**

    Entity Props

    */
public record EntityProps(
    string Id,
    string StackId,
    string MeshType,
    string Color,
    float Scale,
    List<Vector3> Position,
    float Rotation
);

/**

    Converter Helper...

    */
public static class Converter {
    /**
    
        To Rgba List
    
        */
    public static List<float[]> ToRgbaList(string hex, int count) {
        var (r, g, b) = HexToRgb.C(hex);
        float a = 1.0f;
        float[] rgba = new float[] { r, g, b, a };

        List<float[]> val = Enumerable.Repeat(rgba, count).ToList();
        return val;
    }

    /**
    
        To Rotation List
    
        */
    public static List<float> ToRotationList(float rotation, int count) {
        List<float> val = Enumerable.Repeat(rotation, count).ToList();
        return val;
    }
}

/**

    Entity Factory main class.

    */
class EntityFactory {
    private static readonly Random range = new Random();

    // Id
    private static string Id() {
        string s = "N";
        int num = 8;

        string val = Guid.NewGuid().ToString(s)[..num];
        return val;
    }

    // Color
    private static string Color() {
        float f1 = 0.5f;
        float f2 = 0.2f;

        float[] channels = { 0, 0, 0 };
        int primary = range.Next(3);
        int secoundary = (primary + 1 + range.Next(2)) % 3;

        channels[primary] = f1 + (float)range.NextDouble() * f2;
        channels[secoundary] = (float)range.NextDouble() * f1;

        int r = (int)(channels[0] * 255);
        int g = (int)(channels[1] * 255);
        int b = (int)(channels[2] * 255);

        string val = $"#{r:X2}{g:X2}{b:X2}";
        return val;
    }

    // Scale
    private static float Scale() {
        float f1 = 0.5f;
        float f2 = 1.5f;
        
        float val = f1 + (float)range.NextDouble() * f2;
        return val;
    }

    // Position
    private static List<Vector3> Position(int count) {
        var list = new List<Vector3>();
        
        for(int i = 0; i < count; i++) {
            list.Add(new Vector3(
                (float)(range.NextDouble()),
                (float)(range.NextDouble()),
                (float)(range.NextDouble())
            ));
        }

        return list;
    }

    // Rotation
    private static float Rotation() {
        float val = 0.0f;
        return val;
    }

    /**
    
        Clone
    
        */
    public static MeshData clone(MeshData src) {
        MeshData c = new MeshData(src.id, src.meshType);

        float[]? vertices = src.getVertices();
        int[]? indices = src.getIndices();
        float[]? normals = src.getNormals();
        float[]? texCoords = src.getTexCoords();
        float[]? colors = src.getColors();
        float[]? scale = src.getScale();
        
        if(vertices != null) c.setVertices(vertices.ToArray());
        if(indices != null) c.setIndices(indices.ToArray());
        if(normals != null) c.setNormals(normals.ToArray());
        if(texCoords != null) c.setTexCoords(texCoords.ToArray());
        if(colors != null) c.setColors(colors.ToArray());
        if(scale != null) c.setScale(scale.ToArray());
        c.shaderType = src.shaderType;
        c.shaderAddon = src.shaderAddon;
        c.isDynamic = src.isDynamic;
        c.isModel = src.isModel;
        c.colliderShape = src.colliderShape;
        c.colliderRadius = src.colliderRadius;
        
        return c;
    }

    /**
    
        Generate
    
        */
    public static EntityProps generate(string meshType) {        
        int min = 5;
        int max = 21;
        int count = range.Next(min, max);

        string idVal = $"{meshType}_{Id()}";
        string stackIdVal = $"s_{Id()}";
        string colorVal = Color();
        float scaleVal = Scale();
        List<Vector3> positionVal = Position(count);
        float rotationVal = Rotation();

        EntityProps val = new EntityProps(
            Id: idVal,
            StackId: stackIdVal,
            MeshType: meshType,
            Color: colorVal,
            Scale: scaleVal,
            Position: positionVal,
            Rotation: rotationVal
        );

        return val;
    }
}