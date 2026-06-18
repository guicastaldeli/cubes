namespace App.Root.Player;
using App.Root.Collider;
using App.Root.Info;
using App.Root.Mesh;
using App.Root.Shaders;
using App.Root.World;
using App.Root.UI;
using App.Root.Input;
using AppWindow = App.Root.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using App.Root.Chunk;

[ManagedState]
class PlayerController : DataEntry {
    /**
     * 
     * Movement Direction
     *
     */
    public enum MoveDir {
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    /**
     * 
     * Player Mode
     *
     */
    public enum PlayerMode {
        NORMAL,
        FLY
    }

    /**
     * 
     * Player Controller main
     *
     */
    public static PlayerController? instance;

    private AppWindow window;
    private Camera camera;
    private Input input;
    private PlayerInput playerInput;
    private RigidBody rigidBody;
    private CollisionManager? collisionManager;
    private ShaderProgram shaderProgram;
    private Mesh mesh;
    private PlayerMesh playerMesh;
    private Raycaster raycaster;
    private Network? network;
    private NetworkPlayer networkPlayer;

    private Mode mode;

    private WorldManager? worldManager = null!;

    private float posX = 50.0f;
    private float posY = 50.0f;
    private float posZ = 50.0f;
    private Vector3 position;
    
    private float sizeX = 1.0f;
    private float sizeY = 2.0f;
    private float sizeZ = 1.0f;
    private Vector3 size;
    
    private bool normalMode = true;
    private float moveSpeed = 90.0f;
    private bool movingForward = false;
    private bool movingBackward = false;
    private bool movingLeft = false;
    private bool movingRight = false;
    private bool movingUp = false;
    private bool movingDown = false;
    private float jumpForce = 8.0f;
    private float flySpeed = 90.0f;

    private Dictionary<MoveDir, (Keys key, Action<bool> apply)>? moveMap;

    private string id = "";
    private string username = InfoController.getInstance().getUserInfo().getUsername();

    private bool spawned = false;

    public PlayerController(
        AppWindow window,
        Input input, 
        ShaderProgram shaderProgram,
        Mesh mesh
    ) {
        instance = this;

        this.position = new Vector3(posX, posY, posZ);
        this.size = new Vector3(sizeX, sizeY, sizeZ);

        this.window = window;
        this.input = input;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;

        this.camera = new Camera();
        this.playerInput = new PlayerInput(input, this);
        this.rigidBody = new RigidBody(position, size);
        this.playerMesh = new PlayerMesh(window, camera, this, mesh);

        Data.getInstance().register(DataType.PLAYER, this);

        this.networkPlayer = new NetworkPlayer(this);

        this.raycaster = new Raycaster(camera, mesh);

        this.mode = new Mode(window, camera, mesh, this);

        Mapper.Set<PlayerController>();
        
        init();   

        StateManager.Register(this);

        EventStream.on("chunk-ready", _ => setSpawn());
    }

    // Get Window
    public AppWindow getWindow() {
        return window;
    }

    // Get Mode
    public Mode getMode() {
        return mode;
    }

    // Get Input
    public Input getInput() {
        return input;
    }

    // Get Rigid Body
    public RigidBody getRigidBody() {
        return rigidBody;
    }

    // Get Mesh
    public Mesh getMesh() {
        return mesh;
    }

    // Get Player Mesh
    public PlayerMesh getPlayerMesh() {
        return playerMesh;
    }

    // Get Camera
    public Camera getCamera() {
        return camera;
    }

    // Get Player Input
    public PlayerInput getPlayerInput() {
        return playerInput;
    }

    // Get Raycaster
    public Raycaster getRaycaster() {
        return raycaster;
    }

    // Set Collision Manager
    public void setCollisionManager(CollisionManager collisionManager) {
        this.collisionManager = collisionManager;
    }

    // Set World Manager
    public void setWorldManager(WorldManager worldManager) {
        this.worldManager = worldManager;
    }

    /**
     * 
     * Position
     *
     */
    public void setPosition(float x, float y, float z) {
        position = new Vector3(x, y, z);
        rigidBody.setPosition(position);
        camera.setPosition(position);
    }
    
    public Vector3 getPosition() {
        return new Vector3(position);
    }

    /**
     * 
     * Movement
     *
     */
    private void applyMove() {
        Vector3 front = camera.getFront();
        Vector3 right = camera.getRight();

        Vector3 horizontalFront = Vector3.Normalize(new Vector3(front.X, 0.0f, front.Z));
        Vector3 horizontalRight = Vector3.Normalize(new Vector3(right.X, 0.0f, right.Z));

        float speed = normalMode ? moveSpeed : flySpeed;
        Vector3 currentVel = rigidBody.getVelocity();
        Vector3 targetVel = Vector3.Zero;

        if(movingForward) targetVel += horizontalFront * speed;
        if(movingBackward) targetVel -= horizontalFront * speed;
        if(movingLeft) targetVel -= horizontalRight * speed;
        if(movingRight) targetVel += horizontalRight * speed;
        if(!normalMode) {
            if(movingUp) targetVel.Y = speed;
            else if(movingDown) targetVel.Y = -speed;
            else targetVel.Y = 0;

            rigidBody.setGravityEnabled(false);
            rigidBody.setVelocity(targetVel);
        } else {
            rigidBody.setGravityEnabled(true);
            
            rigidBody.setVelocity(new Vector3(
                targetVel.X,
                currentVel.Y,
                targetVel.Z
            ));
        }
    }

    public void jump() {
        if(rigidBody.isOnSurface()) {
            Vector3 vel = rigidBody.getVelocity();
            vel.Y = jumpForce;
            rigidBody.setVelocity(vel);
            rigidBody.setOnSurface(false);
        }
    }

    private void applyJump(bool pressed) {
        if(normalMode) {
            if(pressed) jump();
        } else {
            movingUp = pressed;
        }
    }

    /**
     * 
     * Set
     *
     */
    public void set() {
        if(network != null) id = network.userId ?? id;
        playerMesh.set(true);
        spawned = false;

        if(EventStream.getT<bool>("chunk-ready") == true) {
            setSpawn();
        }
    }

    public Vector3 setSpawnProps() {
        float y = EventStream.getT<float>("stream-surface") ?? 1.0f;
        float spawnY = y + 10.0f;

        var chunks = EventStream.get<List<ChunkCoord>>("streamed-chunks");
        if(chunks == null || chunks.Count == 0) {
            Console.WriteLine("NO CHUNKS!!!!!!!!!!!");
            return new Vector3(0, spawnY, 0);
        }

        float cx = 0;
        float cz = 0;
        
        foreach(var c in chunks) {
            cx += c.cx;
            cz += c.cz;
        }

        cx /= chunks.Count;
        cz /= chunks.Count;

        float wx = cx * ChunkCoord.CHUNK_SIZE + ChunkCoord.CHUNK_SIZE / 2.0f;
        float wz = cz * ChunkCoord.CHUNK_SIZE + ChunkCoord.CHUNK_SIZE / 2.0f;

        Vector3 val = new Vector3(wx, spawnY, wz);
        return val;
    }

    public void setSpawn() {
        if(spawned) return;

        Vector3 spawn = setSpawnProps();
        setPosition(spawn.X, spawn.Y, spawn.Z);
        spawned = true;

        Console.WriteLine($"[PlayerController] spawned at {spawn}");
    }
    
    /**
     * 
     * Render
     *
     */
    public void render() {
        if(!input.onPauseOverlayOpen()) raycaster.onRenderOutline?.Invoke();
        UI.uiController.generate();
    }

    /**
     * 
     * Update
     *
     */
    // Update Mode
    public void updateMode(PlayerMode mode) {
        Mapper.Key(Keys.F, pressed => {
            if(!pressed) return;
            normalMode = !normalMode;

            mode = normalMode ? PlayerMode.NORMAL : PlayerMode.FLY;
            Console.WriteLine("Mode: " + mode);
        });
    }

    // Update Position
    public void updatePosition(MoveDir dir) {
        moveMap ??= new() {
            { MoveDir.FORWARD, (Keys.W, p => movingForward = p)  },
            { MoveDir.BACKWARD, (Keys.S, p => movingBackward = p) },
            { MoveDir.LEFT, (Keys.A, p => movingLeft = p) },
            { MoveDir.RIGHT, (Keys.D, p => movingRight = p) },
            { MoveDir.UP, (Keys.Space, applyJump) },    
            { MoveDir.DOWN, (Keys.LeftShift, p => movingDown = p) },
        };

        if(!moveMap.TryGetValue(dir, out var entry)) return;
        Mapper.Key(entry.key, entry.apply);
        //if(!Mapper.HasKey<PlayerController>(entry.key)) Mapper.Key(entry.key, entry.apply);
        //entry.apply(pressed);
    }

    // Update
    public void update() {
        applyMove();
        rigidBody.update();

        if(collisionManager != null) {
            var collision = collisionManager.checkCollision(rigidBody);
            collisionManager.resolveCollision(rigidBody, collision);
        }
        
        position = rigidBody.getPosition();
        camera.setPosition(position);
        
        playerMesh.update();
        raycaster.update();
        mode.update();
    }

    /**
     * 
     * Network
     *
     */
    public void setNetwork(Network network) {
        this.network = network;
    }

    public Network? getNetwork() {
        return network;
    }

    public NetworkPlayer getNetworkPlayer() {
        return networkPlayer;
    }

    public void sendState() {
        if(network == null || !network.isConnected) return;
        network.sendState(
            position.X, position.Y, position.Z,
            camera.getYaw(), camera.getPitch()
        );
    }

    /**
     * 
     * Init
     *
     */
    private void init() {
        updateMode(PlayerMode.NORMAL);
        foreach(var dir in Enum.GetValues<MoveDir>()) {
            updatePosition(dir);
        }
    }

    /**
    
        Data Entry

        */
    public static string getId() {
        if(instance != null && instance.id != null) {
            return instance.id;
        }
        return "";
    }

    public Dictionary<string, object> serialize() {
        return new Dictionary<string, object> {
            ["id"] = network?.userId ?? InfoController.getInstance().getUserInfo().getId(),
            ["username"] = username,
            ["x"] = position.X,
            ["y"] = position.Y,
            ["z"] = position.Z,
            ["yaw"] = camera.getYaw(),
            ["pitch"] = camera.getPitch()
        };
    }
}