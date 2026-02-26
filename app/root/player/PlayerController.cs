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

    private bool movingForward = false;
    private bool movingBackward = false;
    private bool movingLeft = false;
    private bool movingRight = false;

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

    // Apply Movement
    private void applyMov() {
        Vector3 front = camera.getFront();
        Vector3 right = camera.getRight();

        Vector3 horizontalFront = Vector3.Normalize(new Vector3(front.X, 0.0f, front.Z));
        Vector3 horizontalRight = Vector3.Normalize(new Vector3(right.X, 0.0f, right.Z));

        Vector3 vel = Vector3.Zero;
        float speed = movSpeed * Tick.getDeltaTimeI();
        if(movingForward) vel += horizontalFront * speed;
        if(movingBackward) vel -= horizontalFront * speed;
        if(movingLeft) vel -= horizontalRight * speed;
        if(movingRight) vel += horizontalRight * speed;

        position += vel;
        camera.setPosition(position);
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
        }
    }

    public void update() {
        applyMov();
    }
}