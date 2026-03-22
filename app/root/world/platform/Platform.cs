namespace App.Root.World.Platform;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Resource;
using App.Root.World;

class Platform : WorldHandler {
    private Mesh.Mesh mesh;
    private CollisionManager collisionManager;

    private BoundaryObject? boundaryObject;
    private StaticObject? staticObject;

    private float timer = 0.0f;
    
    public Platform(Mesh.Mesh mesh, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    } 

    ///
    /// Render
    /// 
    public override void render() {
        mesh.add("cube");
        mesh.setPosition("cube", 0.0f, 0.0f, -3.0f);

        string texPath = "env/test.jpg";
        int texId = TextureLoader.load(texPath);
        mesh.setTexture("cube", texId, texPath);

        collisionManager.addStaticCollider(new BoundaryObject(World.WORLD_BOUNDARY));
        collisionManager.addStaticCollider(new StaticObject(mesh.getBBox("cube"), "cube"));
    }

    ///
    /// Update
    /// 
    public override void update() {
        timer += Tick.getDeltaTimeI();
        if(timer >= 1.0f) {
            timer = 0.0f;
            var pos = mesh.getPosition("cube");
            //mesh.setPosition("cube", pos.X + 0.5f, pos.Y, pos.Z);
        }
    }
}