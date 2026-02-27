namespace App.Root.Player;

using App.Root.Collider;
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
    private RigidBody rigidBody = null!;
    private CollisionManager collisionManager = null!;

    private Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 size = new Vector3(1.0f, 2.0f, 1.0f);
    private float movSpeed = 5.0f;

    private bool movingForward = false;
    private bool movingBackward = false;
    private bool movingLeft = false;
    private bool movingRight = false;
    private bool movingUp = false;
    private bool movingDown = false;

    private float jumpForce = 8.0f;

    private bool flyMode = false;
    private float flySpeed = 10.0f;

    public PlayerController() {
        camera = new Camera();
        playerInputMap = new PlayerInputMap(this);
        rigidBody = new RigidBody(position, size);
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
            CollisionResult collision = collisionManager.checkCollision(rigidBody);
            collisionManager.resolveCollision(rigidBody, collision);
        }
        position = rigidBody.getPosition();

        if(!flyMode) {
            if(position.Y <= 0.0f) {
                position.Y = 0.0f;
                rigidBody.setPosition(position);
                rigidBody.setVelocity(new Vector3(
                    rigidBody.getVelocity().X,
                    0.0f,
                    rigidBody.getVelocity().Z
                ));
                rigidBody.setOnGround(true);
            } else {
                rigidBody.setOnGround(false);
            }
        }

        camera.setPosition(position);
    }
}