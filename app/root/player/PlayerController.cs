namespace App.Root.Player;
using OpenTK.Mathematics;

class PlayerController {
    public enum MovDir {
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    private Camera camera;
    private PlayerInputMap playerInputMap;

    private Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
    private float movSpeed = 5.0f;
    private float yVel = 0.0f;

    private bool movingForward = false;
    private bool movingBackward = false;
    private bool movingLeft = false;
    private bool movingRight = false;
    private bool movingUp = false;
    private bool movingDown = false;
    private bool onGround = true;

    private float jumpForce = 8.0f;
    private float gravity = -20.0f;

    private bool flyMode = false;
    private float flySpeed = 10.0f;

    public PlayerController() {
        camera = new Camera();
        playerInputMap = new PlayerInputMap(this);
    }

    // Get Camera
    public Camera getCamera() {
        return camera;
    }

    // Get Input Map
    public PlayerInputMap getPlayerInputMap() {
        return playerInputMap;
    }

    // Position
    public void setPosition(float x, float y, float z) {
        position = new Vector3(x, y, z);
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

        Vector3 vel = Vector3.Zero;
        float speed = (flyMode ? flySpeed : movSpeed) * Tick.getDeltaTimeI();
        if(movingForward) vel += horizontalFront * speed;
        if(movingBackward) vel -= horizontalFront * speed;
        if(movingLeft) vel -= horizontalRight * speed;
        if(movingRight) vel += horizontalRight * speed;
        if(flyMode) {
            if(movingUp) vel.Y += speed;
            if(movingDown) vel.Y -= speed;
        } else {
            yVel += gravity * Tick.getDeltaTimeI();
            vel.Y = yVel * Tick.getDeltaTimeI();
            if(position.Y + vel.Y <= 0.0f) {
                vel.Y = -position.Y;
                yVel = 0.0f;
                onGround = true;
            }
        }

        position += vel;
        camera.setPosition(position);
    }

    public void jump() {
        if(onGround) {
            yVel = jumpForce;
            onGround = false;
        }
    }

    public void toggleFlyMode() {
        flyMode = !flyMode;
        Console.WriteLine("Fly mode: " + (flyMode ? "ON" : "OFF"));
    }

    public bool isInFlyMode() {
        return flyMode;
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
    }
}