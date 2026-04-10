namespace App.Root.Player;
using App.Root.Collider;
using App.Root.Info;
using App.Root.Shaders;
using App.Root.World;
using App.Root.Player.Hud;
using App.Root.World.Platform;
using OpenTK.Mathematics;

class PlayerController : DataEntry {
    public enum MovDir {
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    public static PlayerController? instance;

    private Window window;
    private Camera camera;
    private Input input;
    private PlayerInputMap playerInputMap;
    private RigidBody rigidBody;
    private CollisionManager? collisionManager;
    private ShaderProgram shaderProgram;
    private Mesh.Mesh mesh;
    private PlayerMesh playerMesh;
    private Hud.Hud hud;
    private Raycaster raycaster;

    private float posX = 50.0f;
    private float posY = 50.0f;
    private float posZ = 0.0f;
    private Vector3 position;
    
    private float sizeX = 1.0f;
    private float sizeY = 2.0f;
    private float sizeZ = 1.0f;
    private Vector3 size;
    
    private float movSpeed = 10.0f;
    private bool movingForward = false;
    private bool movingBackward = false;
    private bool movingLeft = false;
    private bool movingRight = false;
    private bool movingUp = false;
    private bool movingDown = false;
    private float jumpForce = 8.0f;

    private bool flyMode = false;
    private float flySpeed = 10.0f;

    private Network? network;
    private NetworkPlayer networkPlayer;

    private WorldManager? worldManager = null!;

    private string id = "";
    private string username =
        InfoController.getInstance()
            .getUserInfo()
            .getUsername();

    public PlayerController(
        Window window,
        Input input, 
        ShaderProgram shaderProgram,
        Mesh.Mesh mesh
    ) {
        instance = this;

        this.position = new Vector3(posX, posY, posZ);
        this.size = new Vector3(sizeX, sizeY, sizeZ);

        this.window = window;
        this.input = input;
        this.shaderProgram = shaderProgram;
        this.mesh = mesh;

        this.camera = new Camera();
        this.playerInputMap = new PlayerInputMap(this);
        this.rigidBody = new RigidBody(position, size);
        this.playerMesh = new PlayerMesh(window, this, mesh);

        Data.getInstance().register(DataType.PLAYER, this);

        this.networkPlayer = new NetworkPlayer(this);

        this.hud = new Hud.Hud(
            window, 
            shaderProgram, 
            mesh
        );

        this.raycaster = new Raycaster(camera, mesh);
    }

    // Get Window
    public Window getWindow() {
        return window;
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
    public Mesh.Mesh getMesh() {
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

    // Get Input Map
    public PlayerInputMap getPlayerInputMap() {
        return playerInputMap;
    }

    // Set Collision Manager
    public void setCollisionManager(CollisionManager collisionManager) {
        this.collisionManager = collisionManager;
    }

    // Set World Manager
    public void setWorldManager(WorldManager worldManager) {
        this.worldManager = worldManager;
    }

    // Position
    public void setPosition(float x, float y, float z) {
        position = new Vector3(x, y, z);
        rigidBody.setPosition(position);
        camera.setPosition(position);
    }
    
    public Vector3 getPosition() {
        return new Vector3(position);
    }

    // Movement
    private void applyMov() {
        Vector3 front = camera.getFront();
        Vector3 right = camera.getRight();

        Vector3 horizontalFront = Vector3.Normalize(new Vector3(front.X, 0.0f, front.Z));
        Vector3 horizontalRight = Vector3.Normalize(new Vector3(right.X, 0.0f, right.Z));

        float speed = flyMode ? flySpeed : movSpeed;
        Vector3 currentVel = rigidBody.getVelocity();
        Vector3 targetVel = Vector3.Zero;

        if(movingForward) targetVel += horizontalFront * speed;
        if(movingBackward) targetVel -= horizontalFront * speed;
        if(movingLeft) targetVel -= horizontalRight * speed;
        if(movingRight) targetVel += horizontalRight * speed;
        if(flyMode) {
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
        if(rigidBody.isOnGround()) {
            Vector3 vel = rigidBody.getVelocity();
            vel.Y = jumpForce;
            rigidBody.setVelocity(vel);
            rigidBody.setOnGround(false);
        }
    }

    public void toggleFlyMode() {
        flyMode = !flyMode;
        Console.WriteLine("Fly mode: " + (flyMode ? "ON" : "OFF"));
    }

    public bool isInFlyMode() {
        return flyMode;
    }

    // Set
    public void set() {
        if(network != null) id = network.userId ?? id;
        playerMesh.set(true);

        Vector3? spawn = Platform.height;
        if(spawn.HasValue) {
            setPosition(
                spawn.Value.X, 
                spawn.Value.Y, 
                spawn.Value.Z
            );
        }
    }

    ///
    /// Render
    /// 
    public void render() {
        hud.render();
    }

    ///
    /// Update
    /// 
    public void updatePosition(MovDir dir, bool pressed) {
        switch(dir) {
            case MovDir.FORWARD:
                movingForward = pressed;
                break;
            case MovDir.BACKWARD:
                movingBackward = pressed;
                break;
            case MovDir.LEFT:
                movingLeft = pressed;
                break;
            case MovDir.RIGHT:
                movingRight = pressed;
                break;
            case MovDir.UP:
                if(flyMode) movingUp = pressed;
                else if(pressed) jump();
                break;
            case MovDir.DOWN:
                movingDown = pressed;
                break;
        }
    }

    public void update() {
        applyMov();
        rigidBody.update();

        if(collisionManager != null) {
            var collision = collisionManager.checkCollision(rigidBody);
            collisionManager.resolveCollision(rigidBody, collision);
        }
        
        position = rigidBody.getPosition();
        camera.setPosition(position);
        playerMesh.update();

        hud.update();
        
        raycaster.update();
    }

    ///
    /// Data Entry
    /// 
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

    ///
    /// Network
    /// 
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
}