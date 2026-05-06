/**

    Entity Generator to handle
    main controller and generation.

    */
namespace App.Root.World.Entity;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.Utils;
using OpenTK.Mathematics;
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

    Entity Generator main class.

    */
class EntityGenerator : WorldHandler {
    private static readonly string DATA_FILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entity/Entity.lua");
    
    private Mesh mesh;
    private CollisionManager collisionManager;

    private bool initialized = false;

    public EntityGenerator([Inject] Mesh mesh, [Inject] CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    }

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

    /**
    
        Generate
    
        */
    private void generate(Dictionary<string, MeshData> meshTypes) {
        foreach(var (meshType, meshData) in meshTypes) {
            List<EntityProps> group = EntityFactory.generate(meshType);

            foreach(var prop in group) {
                MeshData data = EntityFactory.clone(meshData);

                mesh.add(prop.Id, data);
                mesh.setScale(prop.Id, prop.Scale);
                mesh.setColor(prop.Id, prop.Color);

                var rotationRad = prop.Rotation * (MathF.PI / 180.0f);
                var rotationMatrix =
                    Matrix4.CreateRotationX(rotationRad.X) *
                    Matrix4.CreateRotationY(rotationRad.Y) *
                    Matrix4.CreateRotationZ(rotationRad.Z);
                mesh.setRotationMatrix(prop.Id, rotationMatrix);

                var renderer = mesh.getMeshRenderer(prop.Id);
                if(renderer != null) {
                    renderer.isInstanced = true;
                    renderer.isInteractive = true;
                }
            }
        }

        initialized = true;
    }

    /**
    
        Render
    
        */
    public override void render() {
        if(!initialized) {
            var meshTypes = load();
            generate(meshTypes);

            initialized = true;
        }
    }

    /**
    
        Update
    
        */
    public override void update() {
        
    }
}