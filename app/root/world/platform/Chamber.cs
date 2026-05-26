namespace App.Root.World.Platform;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Resource;

/**

    Chamber main class

    */
class Chamber : PlatformEntity.PlatformEntityHandler {
    private Mesh mesh;
    private CollisionManager collisionManager;
    private Platform platform;
    
    public Chamber(Mesh mesh, CollisionManager collisionManager, Platform platform) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.platform = platform;
    }

    /**
    
        Set
    
        */
    public void set() {
        string id = "chamber";
        string path = "chamber.obj";

        MeshData data = MeshModelLoader.loadModel(path);
        data.isModel = true;
        data.modelPath = path;
        data.colliderShape = ColliderType.CUBE;

        mesh.add(id, data);
        mesh.setPosition(id, -3.0f, 10.0f, -3.0f);

        var renderer = mesh.getMeshRenderer(id);
        if(renderer != null) renderer.isInteractive = true;

        string texPath = "world/chamber-test.png";
        int texId = TextureLoader.load(texPath);
        mesh.setTexture(id, texId, texPath);

        collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));

        MeshInteractionRegistry.getInstance().register(id, State.UNBREAKABLE, mesh);
    }

    /**

        Render
    
        */
    public void render() {
        set();
    }
}