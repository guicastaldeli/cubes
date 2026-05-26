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
            string? tex = entry["tex"] as string;
            if(id == null || loader == null) continue;

            MeshData content = loader switch {
                "data" => MeshDataLoader.load(id),
                "model" => MeshModelLoader.loadModel(id),
                _ => throw new Exception($"Unknown loader '{loader}' for entity '{id}'")
            };

            if(tex != null) content.texPath = tex;

            res[id] = content;
        }
    }
}

/**

    Entity Generator main class.

    */
class MeshEntityGenerator : WorldHandler {
    private static readonly string DATA_FILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world/entity/MeshEntity.lua");
    
    private Tick tick;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private MeshEntitySpawner entitySpawner;

    private Queue<string> generationQueue = new();

    private bool initialized = false;

    public MeshEntityGenerator([Inject] Tick tick, [Inject] Mesh mesh, [Inject] CollisionManager collisionManager) {
        this.tick = tick;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    
        this.entitySpawner = new MeshEntitySpawner(tick, mesh, collisionManager);
    
        MeshEntityCollider.onEntityRemoved += meshType => {
            generationQueue.Enqueue(meshType);
        };
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
            throw new Exception("MeshEntity.lua err!");
        }

        var res = new Dictionary<string, MeshData>();
        Setter.set(entities, res);
        return res;
    }

    /**
    
        Generate
    
        */
    private void generate(Dictionary<string, MeshData> meshTypes) {
        var entityProps = new Dictionary<string, EntityProps>();
        var entityInstances = new Dictionary<string, List<Instance>>();

        foreach(var (type, data) in meshTypes) {
            var entities = MeshEntityFactory.generate(data, type);
            
            foreach(var entity in entities) {
                entityProps[entity.Id] = entity;
                entityInstances[entity.Id] = entitySpawner.getInstances(entity.Id);

                MeshData meshData = MeshEntityFactory.clone(data);
                meshData.isEntity = 1;
                meshData.entityType = "mesh";

                mesh.add(entity.Id, meshData);
                mesh.setScale(entity.Id, entity.Scale);
                mesh.setColor(entity.Id, entity.Color);
                mesh.setRotationMatrix(entity.Id, RotationEntity.R(entity));
                if(entity.TexId.HasValue && entity.TexId > 0) mesh.setTexture(entity.Id, entity.TexId.Value, entity.Tex!);
                
                entitySpawner.render(entity);

                var entityData = mesh.getMeshRenderer(entity.Id);
                if(entityData != null) {
                    entityData.isInstanced = true;
                    entityData.isInteractive = true;
                    
                    List<Vector3> position = entitySpawner.getPositions(entity.Id);
                    List<float[]> color = Converter.ToRgbaList(entity.Color, position.Count);
                    List<float> rotation = Converter.ToRotationList(entity.Rotation, position.Count); 
                    List<string?> texPath = Converter.ToTexPath(meshData.texPath, position.Count);
                    List<int>? texId = Converter.ToTexId(entity.TexId, position.Count);
                    
                    entityData.setInstanceData(position, color, rotation, texPath, texId);
                }
            }
        }

        MeshEntityFactory.setEvent(
            MeshEntityCollider.colliderIds,
            entityProps,
            entityInstances
        );

        initialized = true;
    }

    private void generateNew(string type, MeshData data) {
        var entityProps = new Dictionary<string, EntityProps>();
        var entityInstances = new Dictionary<string, List<Instance>>();

        var entities = MeshEntityFactory.generate(data, type);
            
        foreach(var entity in entities) {
            MeshData meshData = MeshEntityFactory.clone(data);
            meshData.isEntity = 1;

            mesh.add(entity.Id, meshData);
            mesh.setScale(entity.Id, entity.Scale);
            mesh.setColor(entity.Id, entity.Color);
            mesh.setRotationMatrix(entity.Id, RotationEntity.R(entity));
            if(entity.TexId.HasValue && entity.TexId > 0) mesh.setTexture(entity.Id, entity.TexId.Value, entity.Tex!);
                
            entitySpawner.render(entity);
            entityProps[entity.Id] = entity;
            entityInstances[entity.Id] = entitySpawner.getInstances(entity.Id);

            var entityData = mesh.getMeshRenderer(entity.Id);
            if(entityData != null) {
                entityData.isInstanced = true;
                entityData.isInteractive = true;
                    
                List<Vector3> position = entitySpawner.getPositions(entity.Id);
                List<float[]> color = Converter.ToRgbaList(entity.Color, position.Count);
                List<float> rotation = Converter.ToRotationList(entity.Rotation, position.Count); 
                List<string?> texture = Converter.ToTexPath(meshData.texPath, position.Count);
                    
                entityData.setInstanceData(position, color, rotation, texture);
            }
        }

        MeshEntityFactory.setEvent(
            MeshEntityCollider.colliderIds,
            entityProps,
            entityInstances
        );
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

        while(generationQueue.Count > 0) {
            string meshType = generationQueue.Dequeue();
            var meshTypes = load();

            if(meshTypes.TryGetValue(meshType, out MeshData? data)) {
                generateNew(meshType, data);
            }
        }
    }
}