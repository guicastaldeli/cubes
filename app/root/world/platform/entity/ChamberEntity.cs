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
using App.Root.UI;
using OpenTK.Mathematics;

class ChamberEntity : PlatformEntity.PlatformEntityHandler {
    public const string CHAMBER_ENTITY_ID = "chamber";

    private Mesh mesh;
    private CollisionManager collisionManager;
    private Platform platform;
    private PlayerController playerController;

    private ChamberDialog chamberDialog;

    (float x, float y, float z) pos = (-3.0f, 2.5f, -3.0f);

    private bool initialized = false;
    
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
        UI.uiController.register(chamberDialog);
        
        Vector3 dPos = new Vector3(pos.x, pos.y, pos.z);

        var chamberText = mesh.getTextEntityRenderer()!.add(
            CHAMBER_ENTITY_ID,
            ChamberDialog.PATH,
            dPos,
            maxDistance: 8.0f
        );

        var els = chamberDialog.get();

        playerController.getRaycaster().onHit += (string? id) => {
            if(id == CHAMBER_ENTITY_ID) {
                chamberText.setVisible(true);
                chamberText.setElementVisible(els.deposit.id);
            } else {
                chamberText.setVisible(false);
            }
        };
    }

    /**
    
        Set
    
        */
    public void set() {
        if(initialized) return;
        initialized = true;

        string path = "chamber.obj";

        MeshData data = MeshModelLoader.loadModel(path);
        data.isModel = true;
        data.modelPath = path;
        data.isEntity = 1;
        data.entityType = "chamber";
        data.colliderShape = ColliderType.CUBE;

        mesh.add(CHAMBER_ENTITY_ID, data);

        Vector3 dPos = new Vector3(pos.x, pos.y, pos.z);
        mesh.setPosition(CHAMBER_ENTITY_ID, dPos);

        var renderer = mesh.getMeshRenderer(CHAMBER_ENTITY_ID);
        if(renderer != null) renderer.isInteractive = true;

        string texPath = "world/chamber-test.png";
        int texId = TextureLoader.load(texPath);
        mesh.setTexture(CHAMBER_ENTITY_ID, texId, texPath);

        collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(CHAMBER_ENTITY_ID), CHAMBER_ENTITY_ID));

        MeshInteractionRegistry.getInstance().register(CHAMBER_ENTITY_ID, State.UNBREAKABLE, mesh);
    
        activateDialog();
    }

    /**

        Render
    
        */
    public override void render() {
        set();
        //chamberDialog.render();
        base.render();
    }

    /**
    
        Update
    
        */
    public override void update() {
        //chamberDialog.update();
        base.update();
    }

    /**
    
        Init
    
        */
    private void init() {
        chamberDialog.init();
    }
}