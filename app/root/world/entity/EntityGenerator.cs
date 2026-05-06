/**

    Entity Generator to handle
    main controller and generation.

    */
namespace App.Root.World.Entity;
using App.Root.Mesh;
using NLua;

/**

    Loader Setter Helper

    */
class Setter {
    /**
    
        Set
    
        */
    public static void set(LuaTable entities, Dictionary<string, MeshData> res) {
        foreach(var key in entities.Keys) {
            if(entities[key] is not LuaTable entry) continue;

            string? id = entry["id"] as string;
            string? loader = entry["loader"] as string;
            if(id == null || loader == null) continue;

            MeshData content = loader switch {
                "data" => MeshDataLoader.load(id),
                "model" => MeshModelLoader.loadModel("id"),
                _ => throw new Exception($"Unknown loader '{loader}' for entity '{id}'")
            };

            res[id] = content;
        }
    }
}


/**

    Entity Generator main class

    */
class EntityGenerator {
    private static readonly string DATA_FILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entity/Entity.lua");
    
    /**
    
        Load
    
        */
    public static Dictionary<string, MeshData> load() {
        using Lua data = new Lua();
        data.DoFile(DATA_FILE);
        if(data["Entities"] is not LuaTable entities) {
            throw new Exception("Entity.lua err!");
        }

        var res = new Dictionary<string, MeshData>();
        Setter.set(entities, res);
        return res;
    }
}