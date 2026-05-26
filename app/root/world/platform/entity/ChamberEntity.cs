/**

    Chamber main class

    */
namespace App.Root.World.Platform.Entity;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Player;
using App.Root.Resource;
using App.Root.Utils;

class ChamberEntity : PlatformEntity.PlatformEntityHandler {
    private Mesh mesh;
    private CollisionManager collisionManager;
    private Platform platform;
    private PlayerController playerController;

    private ChamberDialog chamberDialog;
    
    public ChamberEntity(
        [Inject] Mesh mesh, 
        [Inject] CollisionManager collisionManager, 
        [Inject] Platform platform, 
        [Inject] PlayerController playerController
    ) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.platform = platform;
        this.playerController = playerController;

        this.chamberDialog = new ChamberDialog();

        init();
    }

    // Activate Dialog
    private void activateDialog() {
        var dialog = UI.UI.uiController.get<ChamberDialog>(ChamberDialog.ID);
        if(dialog != null) {
            dialog.setPlayerController(playerController);
            dialog.activate();
        }
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
        data.isEntity = 1;
        data.entityType = "chamber";
        data.colliderShape = ColliderType.CUBE;

        mesh.add(id, data);
        mesh.setPosition(id, -3.0f, 2.5f, -3.0f);

        var renderer = mesh.getMeshRenderer(id);
        if(renderer != null) renderer.isInteractive = true;

        string texPath = "world/chamber-test.png";
        int texId = TextureLoader.load(texPath);
        mesh.setTexture(id, texId, texPath);

        collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(id), id));

        MeshInteractionRegistry.getInstance().register(id, State.UNBREAKABLE, mesh);
    
        activateDialog();
    }

    /**

        Render
    
        */
    public override void render() {
        set();
        chamberDialog.render();
        base.render();
    }

    /**
    
        Update
    
        */
    public override void update() {
        chamberDialog.update();
        base.update();
    }

    /**
    
        Init
    
        */
    private void init() {
        chamberDialog.init();
    }
}