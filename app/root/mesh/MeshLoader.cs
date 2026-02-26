namespace App.Root.Mesh;
using NLua;

class MeshLoader {
    private static readonly string DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data/");

    private static float[] toFloatArray(LuaTable table) {
        int len = (int)(long)table.Values.Count;
        float[] arr = new float[len];
        for(int i = 1; i <= len; i++) arr[i-1] = Convert.ToSingle(table[i]);
        return arr;
    }

    private static int[] toIntArray(LuaTable table) {
        int len = (int)(long)table.Values.Count;
        int[] arr = new int[len];
        for(int i = 1; i <= len; i++) arr[i-1] = Convert.ToInt32(table[i]);
        return arr;
    }

    ///
    /// Parse
    /// 
    private static MeshData parse(Lua data, string meshId) {
        string meshType = data["meshType"] as string ?? meshId;
        MeshData meshData = new MeshData(meshId, meshType);

        if(data["vertices"] is LuaTable vert) meshData.setVertices(toFloatArray(vert));
        if(data["indices"] is LuaTable idx) meshData.setIndices(toIntArray(idx));
        if(data["normals"] is LuaTable norm) meshData.setNormals(toFloatArray(norm));
        if(data["texCoords"] is LuaTable texCoords) meshData.setTexCoords(toFloatArray(texCoords));
        if(data["scale"] is LuaTable scale) meshData.setScale(toFloatArray(scale));
        if(data["rotation"] is LuaTable rotation) {
            if(rotation["axis"] is string axis) meshData.addData(MeshData.DataType.ROTATION_AXIS, axis);
            if(rotation["speed"] is double speed) meshData.addData(MeshData.DataType.ROTATION_SPEED, (float)speed);
        } 

        return meshData;
    } 

    ///
    /// Load
    /// 
    public static MeshData load(string meshId) {
        string file = meshId.ToLower() + ".lua";
        string path = Path.Combine(DIR, file);
        if(!File.Exists(path)) throw new IOException("Mesh file not found: " + path);

        try {
            using Lua data = new Lua();
            data.DoFile(path);
            return parse(data, meshId);
        } catch(Exception err) {
            throw new Exception("Failed to load mesh: " + meshId, err);
        }
    }
}