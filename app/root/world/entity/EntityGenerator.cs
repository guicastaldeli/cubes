/**

    Entity Generator to handle
    main controller and generation.

    */
namespace App.Root.World.Entity;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.Utils;
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
                "model" => MeshModelLoader.loadModel(id),
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
    private static readonly string DATA_FILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/entity/Entity.lua");
    
    private Tick tick;
    private Mesh mesh;
    private CollisionManager collisionManager;

    private EntitySpawner entitySpawner;

    private bool initialized = false;

    public EntityGenerator([Inject] Tick tick, [Inject] Mesh mesh, [Inject] CollisionManager collisionManager) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    
        this.entitySpawner = new EntitySpawner(tick, mesh, collisionManager);
    }

    /**
    
        Load
    
        */
    public static Dictionary<string, MeshData> load() {
        using Lua data = new Lua();

        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        
        data.DoFile(DATA_FILE);
        
        Directory.SetCurrentDirectory(originalDir);
        
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
            EntityProps entity = EntityFactory.generate(meshType);
            MeshData data = EntityFactory.clone(meshData);

            mesh.add(entity.Id, data);
            mesh.setScale(entity.Id, entity.Scale);
            mesh.setColor(entity.Id, entity.Color);
            mesh.setRotationMatrix(entity.Id, RotationEntity.R(entity));

            entitySpawner.render(entity);

            var renderer = mesh.getMeshRenderer(entity.Id);
            if(renderer != null) {
                renderer.isInstanced = true;
                renderer.isInteractive = true;
                var spawnPos = entitySpawner.getPositions(entity.Id);
                
                renderer.setInstanceData(
                    spawnPos,
                    Converter.ToRgbaList(entity.Color, spawnPos.Count),
                    Converter.ToRotationList(entity.Rotation, spawnPos.Count)
                );
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
        entitySpawner.update();
    }
}