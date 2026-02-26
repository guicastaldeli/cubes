using OpenTK.Mathematics;

namespace App.Root.Player;

class RigidBody {
    private Vector3 position;
    private Vector3 velocity;
    private Vector3 acceleration;
    private Vector3 size;

    private float mass = 1.0f;
    private bool onGround = false;
    
    private bool gravityEnabled = true;
    private float gravity = -20.0f;
    private float gravityScale = 3.0f;
    private float drag = 0.1f;

    public RigidBody(Vector3 position, Vector3 size) {
        this.position = new Vector3(position);
        this.size = new Vector3(size);
        this.velocity = Vector3.Zero;
        this.acceleration = Vector3.Zero;
    }

    // Apply Force
    public void applyForce(Vector3 force) {
        acceleration += force / mass;
    }

    // Position
    public void setPosition(Vector3 position) {
        this.position = new Vector3(position);
    }

    public Vector3 getPosition() {
        return new Vector3(position);
    }

    // Velocity
    public void setVelocity(Vector3 velocity) { 
        this.velocity = new Vector3(velocity);
    }

    public Vector3 getVelocity() { 
        return new Vector3(velocity);
    }

    // Size
    public void setSize(Vector3 size) {
        this.size = new Vector3(size);
    }

    public Vector3 getSize() {
        return new Vector3(size);
    }

    // On Ground
    public void setOnGround(bool onGround) {
        this.onGround = onGround;
    }

    public bool isOnGround() {
        return onGround;
    }

    // Mass
    public void setMass(float mass) {
        this.mass = mass;
    }

    public float getMass() {
        return mass;
    }

    // Gravity
    public float getGravity() {
        return gravity;
    }

    public void setGravityScale(float scale) {
        gravityScale = scale;
    } 

    public float getGravityScale() {
        return gravityScale;
    }

    public void setGravityEnabled(bool enabled) {
        gravityEnabled = enabled;
    }

    public bool isGravityEnabled() {
        return gravityEnabled;
    }

    ///
    /// Update
    /// 
    public void update() {
        float deltaTime = Tick.getDeltaTimeI();
        deltaTime = MathF.Min(deltaTime, 0.1f);
        if(gravityEnabled && !onGround) {
            applyForce(new Vector3(
                0.0f, 
                gravity * mass * gravityScale, 
                0.0f
            ));
        }

        velocity += acceleration * deltaTime;
        velocity *= 1.0f - (drag * deltaTime);
        position += velocity * deltaTime;

        acceleration = Vector3.Zero;
    }
}