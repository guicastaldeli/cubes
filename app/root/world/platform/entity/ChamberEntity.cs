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
using App.Root.Text;
using App.Root.World.Points;
using App.Root.World.Entity;
using App.Root.Screen;
using App.Root.Input;
using AppWindow = App.Root.Window;
using WorldPlatform = Root.World.Platform.Platform;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using App.Root.Chunk;

class ChamberEntity : PlatformRegistry.PlatformRegistryHandler {
    public const string CHAMBER_ENTITY_ID = "chamber";

    private AppWindow window;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private WorldPlatform platform;
    private PlayerController playerController;

    private ChamberDialog chamberDialog;

    public Vector3 pos = Vector3.Zero;
    public Vector3 storedPos;

    private bool initialized = false;
    public bool deposited = false;
    
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
        Points.Init();
        Points.Set(999);
    }

    // Get Platform Position
    private Vector3 getPlatformPosition() {
        var chunks = EventStream.get<List<ChunkCoord>>("streamed-chunks");
        if(chunks == null || chunks.Count == 0) return new Vector3(0.0f, 0.0f, 0.0f);
        
        float cx = -3.0f, cz = -3.0f;
        foreach(var chunk in chunks) {
            Vector3 pos = chunk.ToWorldPosition();
            cx += pos.X + ChunkCoord.CHUNK_SIZE / 2.0f;
            cz += pos.Z + ChunkCoord.CHUNK_SIZE / 2.0f;
        }
        cx /= chunks.Count;
        cz /= chunks.Count;

        float topY = EventStream.getT<float>("stream-top") ?? 0;
        float y = topY;

        Vector3 val = new Vector3(cx, y, cz);
        return val;
    }

    // Stream Event
    public void streamEvent(TextEntity text, dynamic els) {
        string id = "deposit";

        Input.listen(id, Keys.E);

        EventStream.on(id, (data) => {
            if(data is not (int key, int action)) return;
            if(action != KeyAction.Press) return;
            if(playerController.getRaycaster().cast() != CHAMBER_ENTITY_ID) return;
            deposit(text, els);
        });
    }

    // Get Mesh Entity Color
    private Vector3 getMeshEntityColor(PlacedMeshDef held) {
        var (r, g, b) = HexToRgb.C(held.Color!);

        Vector3 color = new Vector3(r, g, b);
        return color;
    }

    // Activate Particles
    private void activateParticles(Vector3 color) {
        var particles = mesh.getParticleController();

        if(particles != null) {
            float y = pos.Y + 1.0f;
            Vector3 emitPos = new Vector3(pos.X, y, pos.Z);
            
            particles.emit(
                emitPos,
                color,
                30,
                0.1f,
                2.0f,
                1.5f,
                new Vector3(2.0f, 4.0f, 2.0f),
                () => new Vector3(
                    color.X * (0.8f + (float)Random.Shared.NextDouble() * 0.2f),
                    color.Y * (0.8f + (float)Random.Shared.NextDouble() * 0.2f),
                    color.Z * (0.8f + (float)Random.Shared.NextDouble() * 0.2f)
                )
            );
        }
    }

    /**
     * 
     * Deposit
     *
     */
    private void deposit(TextEntity textEntity, dynamic els) {
        var held = mesh.getMeshInteractionController().getHeldMesh();
        if(held == null) return;

        int? xp = XpRegistry.Get(held.InstanceId!);
        if(xp == null) return;

        int added = Xp.ConvertToPoints(xp.Value);
        Points.Add(xp.Value);

        deposited = true;
        DocParser.Replace("displayPoints", added);

        Vector3 entityColor = getMeshEntityColor(held);
        
        window.queueOnRenderThread(() => {
            activateParticles(entityColor);

            textEntity.refresh(els.plusPoints.id);
            textEntity.setElementVisible(els.plusPoints.id);

            chamberDialog.setAnimation(textEntity, els, entityColor);
        });
    }

    /**
     * 
     * Set
     *
     */
    public void set() {
        if(initialized) return;
        initialized = true;

        string path = "chamber.obj";

        MeshData data = MeshModelLoader.loadModel(path);
        data.isModel = true;
        data.modelPath = path;
        data.isEntity = 0;
        data.entityType = "chamber";
        data.colliderShape = ColliderType.CUBE;

        mesh.add(CHAMBER_ENTITY_ID, data);

        Vector3 dpos = new Vector3(pos.X, pos.Y, pos.Z);
        mesh.setPosition(CHAMBER_ENTITY_ID, dpos);

        var renderer = mesh.getMeshRenderer(CHAMBER_ENTITY_ID);
        if(renderer != null) renderer.isInteractive = true;

        string texPath = "world/chamber-test.png";
        int texId = TextureLoader.load(texPath);
        mesh.setTexture(CHAMBER_ENTITY_ID, texId, texPath);

        collisionManager.addStaticCollider(new StaticObject(() => mesh.getBBox(CHAMBER_ENTITY_ID), CHAMBER_ENTITY_ID));

        MeshInteractionRegistry.getInstance().register(CHAMBER_ENTITY_ID, State.UNBREAKABLE, mesh);
    
        chamberDialog.activate(mesh);
    }

    /**
     * 
     * Render
     *
     */
    public override void render() {
        set();
        base.render();
    }

    /**
     * 
     * Update
     *
     */
    public override void update() {
        base.update();

        if(initialized) {
            Vector3 newPos = getPlatformPosition();
            pos = newPos;
            mesh.setPosition(CHAMBER_ENTITY_ID, pos);
            chamberDialog.updatePosition(pos);
        }
    }

    /**
     * 
     * Init
     *
     */
    private void init() {
        chamberDialog.setChamberEntity(this);
        chamberDialog.setPlayerController(playerController);
        chamberDialog.init();
    }
}