/**

    Entity Factory class

    */
namespace App.Root.World.Entity;

using App.Root.Mesh;
using OpenTK.Mathematics;

/**

    Entity Props

    */
record EntityProps(
    string Id,
    string StackId,
    string MeshType,
    string Color,
    float Scale,
    Vector3 Rotation
);

/**

    Entity Factory main class.

    */
class EntityFactory {
    private static readonly Random range = new Random();

    // Color
    private static string color() {
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
    private static float scale() {
        float f1 = 0.5f;
        float f2 = 1.5f;
        
        float val = f1 + (float)range.NextDouble() * f2;
        return val;
    }

    // Rotation
    private static Vector3 rotation() {
        float f = 360.0f;

        Vector3 val = new Vector3(
            (float)(range.NextDouble() * f),
            (float)(range.NextDouble() * f),
            (float)(range.NextDouble() * f)
        );

        return val;
    }

    // Id
    private static string id() {
        string s = "N";
        int num = 8;

        string val = Guid.NewGuid().ToString(s)[..num];
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
    public static List<EntityProps> generate(string meshType) {
        string idVal = $"{meshType}_{id()}";
        string stackIdVal = $"s_{id()}";
        string colorVal = color();
        float scaleVal = scale();
        Vector3 rotationVal = rotation();

        var group = new List<EntityProps>();
        int copies = range.Next(5, 21);

        for(int i = 0; i < copies; i++) {
            group.Add(new EntityProps(
                Id: idVal,
                StackId: stackIdVal,
                MeshType: meshType,
                Color: colorVal,
                Scale: scaleVal,
                Rotation: rotationVal
            ));
        }

        return group;
    }
}