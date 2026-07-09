/**

    Mesh Data Loader to load
    main mesh data.

    */
namespace App.Root.Mesh;
using NLua;

class MeshDataLoader {
    // To Float Array
    private static float[] toFloatArray(LuaTable table) {
        int len = (int)(long)table.Values.Count;
        float[] arr = new float[len];
        for(int i = 1; i <= len; i++) arr[i-1] = Convert.ToSingle(table[i]);
        return arr;
    }

    // To Int Array
    private static int[] toIntArray(LuaTable table) {
        int len = (int)(long)table.Values.Count;
        int[] arr = new int[len];
        for(int i = 1; i <= len; i++) arr[i-1] = Convert.ToInt32(table[i]);
        return arr;
    }

    /**
     * 
     * Get
     *
     */
    // Get Source
    private static T? getSource<T>(dynamic src, string key) where T : class {
        if(src is LuaTable table) {
            return table[key] as T;
        } else if(src is Lua state) {
            return state[key] as T;
        }
        
        return null;
    }

    // Get Table
    private static LuaTable? getTable(dynamic src, string key) {
        if(src is LuaTable table) {
            return table[key] as LuaTable;
        } else if(src is Lua state) {
            return state[key] as LuaTable;
        }

        return null;
    }

    /**
     * 
     * Parse
     *
     */
    private static MeshData parse(Lua data, string meshId) {
        string meshType = data["meshType"] as string ?? meshId;
        MeshData meshData = new MeshData(meshId, meshType);

        dynamic src = data;
        bool isReturnTable = false;

        if(data["meshType"] == null) {
            var globals = data.GetTable("_G");
            if(globals != null) {
                foreach(var key in globals.Keys) {
                    if(globals[key] is LuaTable table && table["meshType"] != null) {
                        src = table;
                        isReturnTable = true;
                        break;
                    }
                }
            }
        }

        var map = new (string key, Action<object> setter)[] {
            ("meshType", (v) => { if(v is string s) meshData.meshType = s; }),
            ("vertices", (v) => { if(v is LuaTable t) meshData.setVertices(toFloatArray(t)); }),
            ("indices", (v) => { if(v is LuaTable t) meshData.setIndices(toIntArray(t)); }),
            ("colors", (v) => { if(v is LuaTable t) meshData.setColors(toFloatArray(t)); }),
            ("normals", (v) => { if(v is LuaTable t) meshData.setNormals(toFloatArray(t)); }),
            ("texCoords", (v) => { if(v is LuaTable t) meshData.setTexCoords(toFloatArray(t)); }),
            ("scale", (v) => { if(v is LuaTable t) meshData.setScale(toFloatArray(t)); }),
            ("rotation", (v) => {
                LuaTable? rotation = isReturnTable ?
                    (src as LuaTable)?["rotation"] as LuaTable :
                    data["rotation"] as LuaTable;
                if(rotation != null) {
                    if(rotation["axis"] is string axis) meshData.addData(MeshData.DataType.ROTATION_AXIS, axis);
                    if(rotation["speed"] is double speed) meshData.addData(MeshData.DataType.ROTATION_SPEED, (float)speed);
                }
            }),
            ("collider", (v) => {
                LuaTable? collider = isReturnTable ?
                (src as LuaTable)?["collider"] as LuaTable :
                data["collider"] as LuaTable;

                if(collider != null) {
                    if(collider["shape"] is string shape) meshData.colliderShape = shape;
                    if(collider["radius"] is double radius) meshData.colliderRadius = (float)radius;
                }
            })
        };
        foreach(var (key, setter) in map) {
            if(isReturnTable) {
                var table = src as LuaTable;
                if(table != null && table[key] != null) {
                    setter(table[key]);
                } else {
                    if(data[key] != null) {
                        setter(data[key]);
                    }
                }
            }
        }

        return meshData;
    } 

    /**
     * 
     * Load
     *
     */
    public static MeshData load(string meshId) {
        string file = meshId.ToLower() + ".lua";
        string path = Path.Combine(Mesh.DATA_DIR, file);
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