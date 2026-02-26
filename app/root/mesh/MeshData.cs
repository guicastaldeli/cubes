using System.Numerics;

namespace App.Root.Mesh;

class MeshData {
    public enum DataType {
        VERTICES,
        INDICES,
        COLORS,
        POSITION,
        NORMALS,
        TEX_COORDS,
        ROTATION,
        ROTATION_AXIS,
        ROTATION_SPEED,
        SCALE
    }

    private readonly Dictionary<DataType, object> data = new();
    public string meshType { get; }
    public string id { get; }
    public bool isDynamic { get; set; } = false;
    public bool isTransparent { get; set; } = false;
    public int shaderType { get; set; } = 0;
    private float[]? colors;

    public MeshData(string id, string meshType) {
        this.id = id;
        this.meshType = meshType;
    }

    // Vertices
    public void setVertices(float[] v) {
        data[DataType.VERTICES] = v;
    }

    public float[]? getVertices() {
        float[]? val = 
            data.TryGetValue(DataType.VERTICES, out var v) ?
            (float[])v :
            null;

        return val;
    }

    public int getVertexCount() {
        var v = getVertices();
        int val = v != null ? v.Length / 3 : 0;
        return val;
    }

    // Colors
    public void setColors(float[] c) {
        colors = c;
        data[DataType.COLORS] = c;
    }   

    public void setColor(float r, float g, float b, float a) {
        float[] c = new float[getVertexCount() * 4];
        for(int i = 0; i < c.Length; i += 4) {
            c[i] = r;
            c[i+1] = g;
            c[i+2] = b;
            c[i+3] = a;
        }
        setColors(c);
    }

    public void setColorHex(string hex) {
        hex = hex.TrimStart('#');
        float r = Convert.ToInt32(hex[..2], 16) / 255.0f;
        float g = Convert.ToInt32(hex[2..4], 16) / 255.0f;
        float b = Convert.ToInt32(hex[4..6], 16) / 255.0f;
        float a = hex.Length >= 8 ? Convert.ToInt32(hex[6..8], 16) / 255.0f : 1.0f;
        setColor(r, g, b, a);
    }

    public float[]? getColors() {
        if(colors != null) return colors;
        float[]? val = data.TryGetValue(DataType.COLORS, out var v ) ? (float[])v : null;
        return val; 
    }

    public void setTransparentColor(float a) {
        setColor(1.0f, 1.0f, 1.0f, a);
    }

    public void setTransparentColor(float r, float g, float b, float a) {
        setColor(r, b, g, a);
    }

    // Normals
    public void setNormals(float[] norm) {
        data[DataType.NORMALS] = norm;
    }

    public float[]? getNormals() {
        float[]? val = 
            data.TryGetValue(DataType.NORMALS, out var v) ?
            (float[])v :
            null;

        return val;
    }

    // Tex Coords
    public void setTexCoords(float[] t) {
        data[DataType.TEX_COORDS] = t;
    }

    public float[]? getTexCoords() {
        float[]? val =  
            data.TryGetValue(DataType.TEX_COORDS, out var v) ?
            (float[])v :
            null;

        return val;
    }

    // Position
    public void setPosition(Vector3 p) {
        data[DataType.POSITION] = p;
    }

    public Vector3 getPosition() {
        Vector3 val = 
            data.TryGetValue(DataType.POSITION, out var v) ?
            (Vector3)v :
            Vector3.Zero;

        return val; 
    }

    // Scale
    public void setScale(float[] s) {
        data[DataType.SCALE] = s;
    }

    public void setScale(float s) {
        setScale(new float[] { s, s, s });
    }

    public void setScale(float x, float y, float z) {
        setScale(new float[] { x, y, z });
    }

    public float[]? getScale() {
        float[]? val = 
            data.TryGetValue(DataType.SCALE, out var v) ?
            (float[])v :
            null;

        return val;
    }

    public bool hasScale() {
        bool val = data.ContainsKey(DataType.SCALE);
        return val;
    }

    // Rotation
    public void setRotation(Vector3 r) {
        data[DataType.ROTATION] = r;
    }

    public Vector3 getRotation() {
        Vector3 val = 
            data.TryGetValue(DataType.ROTATION, out var v) ?
            (Vector3)v :
            Vector3.Zero;

        return val;
    }

    public string? getRotationAxis() {
        string? val =
            data.TryGetValue(DataType.ROTATION_AXIS, out var v) ?
            (string)v :
            null;

        return val;
    }

    public float getRotationSpeed() {
        float val =
            data.TryGetValue(DataType.ROTATION_SPEED, out var v) ?
            (float)v :
            0.0f;

        return val;
    }

    public bool hasRotation() {
        bool val =
            getRotationAxis() != null &&
            getRotationSpeed() > 0.0f;

        return val;
    }

    // Data
    public bool hasData(DataType type) {
        bool val = data.ContainsKey(type);
        return val;
    }

    public void addData(DataType type, object val) {
        data[type] = val;
    }

    public T? getData<T>(DataType type) {
        T? val =
            data.TryGetValue(type, out var v) ?
            (T)v :
            default;
        
        return val;
    }
} 