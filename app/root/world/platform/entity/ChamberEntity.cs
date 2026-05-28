/**

    Chamber Entity main class

    */
namespace App.Root.World.Platform.Entity;
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using App.Root.Player;
using App.Root.Resource;
using App.Root.Utils;
using App.Root.UI;
using App.Root.Text;
using App.Root.World.Points;
using App.Root.World.Entity;
using OpenTK.Mathematics;
using AppWindow = App.Root.Window;
using WorldPlatform = Root.World.Platform.Platform;
using OpenTK.Windowing.GraphicsLibraryFramework;
using App.Root.Screen;

class ChamberEntity : PlatformRegistry.PlatformRegistryHandler {
    public const string CHAMBER_ENTITY_ID = "chamber";

    private AppWindow window;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private WorldPlatform platform;
    private PlayerController playerController;

    private ChamberDialog chamberDialog;

    (float x, float y, float z) pos = (-3.0f, 2.5f, -3.0f);

    private bool initialized = false;
    private bool deposited = false;
    
    public ChamberEntity(
        [Inject] AppWindow window,
        [Inject] Mesh mesh, 
        [Inject] CollisionManager collisionManager, 
        [Inject] WorldPlatform platform, 
        [Inject] PlayerController playerController
    ) {
        this.window = window;
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
                showDialog(chamberText, els);
            } else {
                chamberText.setVisible(false);
            }
        };

        streamEvent(chamberText, els);
    }

    // Stream Event
    private void streamEvent(TextEntity text, dynamic els) {
        string id = "deposit";

        Input.listen(id, Keys.E);

        EventStream.on(id, (data) => {
            if(data is not (int key, int action)) return;
            if(action != KeyAction.Press) return;
            if(playerController.getRaycaster().cast() != CHAMBER_ENTITY_ID) return;
            deposit(text, els);
        });
    }

    // Show Dialog
    private void showDialog(TextEntity text, dynamic els) {
        if(!deposited) {
            text.setElementVisible(els.deposit.id);
        } else {
            text.setElementVisible(els.plusPoints.id);
        }
    }

    /**
    
        Deposit
    
        */
    private void deposit(TextEntity textEntity, dynamic els) {
        var held = mesh.getMeshInteractionController().getHeldMesh();
        if(held == null) return;

        int? xp = XpRegistry.Get(held.InstanceId!);
        if(xp == null) return;

        int added = Xp.ConvertToPoints(xp.Value);
        Points.Add(xp.Value);

        deposited = true;

        DocParser.Replace("points", added);
        
        window.queueOnRenderThread(() => {
            textEntity.refresh(els.plusPoints.id);
            textEntity.setElementVisible(els.plusPoints.id);
        });
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