namespace App.Root.World.Platform;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Mesh.Particle;
using App.Root.Physics;
using App.Root.Resource;
using OpenTK.Mathematics;

class Platform : WorldHandler {
    private Mesh mesh;
    private CollisionManager collisionManager;

    public string GRID_ID = "grid";
    private string MESH = "cube";

    private float x = 0.0f;
    private float y = 0.0f;
    private float z = 0.0f;
    private Vector3 offset = Vector3.Zero;
    
    private int sizeX = 10;
    private int sizeY = 3;
    private int sizeZ = 10;
    private float spacing = 1.0f;

    private bool initialized = false;
    
    public Platform(Mesh mesh, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    }

    // Set Client
    public void setClient() {
        if(initialized) return;
        set(renderMesh: false);
    } 

    // Set Position
    private void setPosition() {
        offset = new Vector3(x, y, z);
    }

    // Height
    public Vector3 getHeight() {
        Vector3 meshSize = mesh.getSize(GRID_ID);
        float topY = offset.Y + (sizeY * spacing) + (meshSize.Y / 2.0f);
        Vector3 res = new Vector3(offset.X, topY, offset.Z); 
        return res;
    }

    public static Vector3? height {
        get;
        private set;
    }

    public static float? topSurfaceY {
        get;
        private set;
    }

    /**

        Temporary Meshes to test 
        raycaster for objects

        */
        private int spawnCounter = 0;

        private void spawnMesh(string meshType, Vector3 position, float scale, string texPath, string stackId) {
            string id = $"{meshType}_{spawnCounter++}";

            MeshData data = MeshDataLoader.load(meshType);
            mesh.add(id, data);
            mesh.setPosition(id, position);
            if(scale != 1.0f) mesh.setScale(id, scale);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInteractive = true;

            collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));
            MeshInteractionRegistry.getInstance().register(id, State.BREAKABLE, mesh, stackId);
        }

        private void spawnGrid(string meshType, Vector3 origin, int cols, int rows, float scale = 1.0f, float spacing = 1.0f, string texPath = "env/test.jpg") {
            string stackId = $"{meshType}_wall";

            for(int r = 0; r < rows; r++) {
                for(int c = 0; c < cols; c++) {
                    float px = origin.X + c * spacing;
                    float py = origin.Y + r * spacing;
                    spawnMesh(meshType, new Vector3(px, py, origin.Z), scale, texPath, stackId);
                }
            }
        }

        public void set2() {
            string id = "cubic";
            string stackId = "cubic_stack";
            string mesht = "cube";
            MeshData data = MeshDataLoader.load(mesht);
            mesh.add(id, data);
            mesh.setPosition(id, 0.0f, 10.0f, -3.0f);
            mesh.setScale(id, 0.5f);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInteractive = true;

            string texPath = "env/test.jpg";
            int texId = TextureLoader.load(texPath);
            mesh.setTexture(id, texId, texPath);

            collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));
            //collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
            //collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
        
            MeshInteractionRegistry.getInstance().register(
                id,
                State.BREAKABLE,
                mesh,
                Type.DYNAMIC,
                stackId
            );
        }

        public void set3() {
            string id = "cubic2";
            string mesht = "sphere";
            MeshData data = MeshDataLoader.load(mesht);
            mesh.add(id, data);
            mesh.setPosition(id, 2.0f, 10.0f, -3.0f);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInteractive = true;

            string texPath = "env/test.jpg";
            int texId = TextureLoader.load(texPath);
            mesh.setTexture(id, texId, texPath);

            collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));
            //collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
            //collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
        
            MeshInteractionRegistry.getInstance().register(
                id,
                State.BREAKABLE,
                mesh,
                Type.DYNAMIC
            );
        }

        public void set4() {
            string id = "dino";
            string path = "resource/test/dino.obj";

            MeshData data = MeshModelLoader.loadModel(path);
            data.isModel = true;
            data.modelPath = path;
            data.colliderShape = Types.CUBE;

            mesh.add(id, data);
            mesh.setPosition(id, -3.0f, 10.0f, -3.0f);

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null) renderer.isInteractive = true;

            string texPath = "test/dino.png";
            int texId = TextureLoader.load(texPath);
            mesh.setTexture(id, texId, texPath);

            collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));
            //collisionManager.addStaticCollider(new TriangleObject(mesh, id, id));
            //collisionManager.addStaticCollider(new SphereObject(mesh, id, id));
        
            MeshInteractionRegistry.getInstance().register(
                id,
                State.BREAKABLE,
                mesh,
                Type.DYNAMIC,
                meshType: path
            );
        }

        ///
        /// Particles
        /// 
        private int frameCounter = 0;
        private ParticleEntity? particleEntity = null;

        private void emitParticle() {
            ParticleController particleController = mesh.getParticleController()!;
            Random random = new Random();

            Vector3 position = new Vector3(0.0f, 10.0f, -3.0f);
            Vector3 color = new Vector3(1.0f, 1.0f, 1.0f); 
            int amount = 5;
            float size = 0.1f;
            float speed = 0.3f;
            float lifetime = 2.5f;
            Vector3 velNum = new Vector3(5.0f, 5.0f, 5.0f);

            if(particleEntity == null) {
                particleEntity = particleController.emit(
                    position,
                    color,
                    amount,
                    size,
                    speed,
                    lifetime,
                    velNum,
                    () => {
                        return new Vector3(
                            random.NextSingle(),
                            random.NextSingle(),
                            random.NextSingle()
                        );
                    }
                );
            } else {
                particleEntity.set(
                    new Vector3(0.0f, 10.0f, -3.0f),
                    true,
                    () => {
                        return new Vector3(
                            random.NextSingle(),
                            random.NextSingle(),
                            random.NextSingle()
                        );
                    }
                );
            }
        }
    /**
        ****
        ****
        ****

        */

    /**
    
        Set

        */ 
    private void setMesh(List<Vector3> positions) {
        var renderer = mesh.getMeshRenderer(GRID_ID);
        if(renderer != null) {
            renderer.isInstanced = true;
            renderer.setInstancePositions(positions);
        }
    }

    private void set(bool renderMesh = true) {
        setPosition();
        MeshRegistry.register(GRID_ID);

        List<Vector3> positions = new();
        float offsetX = -(sizeX / 2.0f) * spacing + offset.X;
        float offsetZ = -(sizeZ / 2.0f) * spacing + offset.Z;
        
        mesh.add(GRID_ID, MESH);
        MeshInteractionRegistry.getInstance().register(
            GRID_ID,
            State.GRID,
            mesh,
            Type.RECEIVER
        );

        height = getHeight();
        Vector3 size = mesh.getSize(GRID_ID);
        Vector3 half = size / 2.0f;
        topSurfaceY = offset.Y + (sizeY - 1) * spacing + half.Y;

        for(int x = 0; x < sizeX; x++) {
            for(int y = 0; y < sizeY; y++) {
                for(int z = 0; z < sizeZ; z++) {
                    string id = $"cube_{x}_{y}_{z}";

                    float px = offsetX + x * spacing;
                    float py = (y * spacing) + offset.Y;
                    float pz = offsetZ + z * spacing;
                    positions.Add(new Vector3(px, py, pz));

                    collisionManager.addStaticCollider(new StaticObject(
                        new Vector3(px, py, pz), 
                        half.X, half.Y, half.Z,
                        id
                    ));
                }
            }
        }

        if(renderMesh) {
            setMesh(positions);
            Console.WriteLine($"Platform draw calls: 1 (instanced {positions.Count} cubes)");
        } else {
            mesh.remove(GRID_ID);
        }

        initialized = true;
    }

    /**
    
        Render

        */  
    public override void render() {
        if(!initialized) {
            set();

            set2();
            set3();
            set4();
            
            spawnGrid("cube", new Vector3(4f, 3f, -3f), 5, 3);
            
            initialized = true;
        }
    }

    /**
    
        Update

        */ 
    public override void update() {
        frameCounter++;

        if(frameCounter % 10 == 0) {
            emitParticle();
        }
    }
}