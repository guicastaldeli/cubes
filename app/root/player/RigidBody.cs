namespace App.Root.Player;
using App.Root.Collider;
using OpenTK.Mathematics;

class RigidBody {
    private Vector3 position;
    private Vector3 velocity;
    private Vector3 acceleration;
    private Vector3 size;

    private float mass = 1.0f;
    private bool onSurface = false;
    
    private bool gravityEnabled = true;
    private float gravity = -20.0f;
    private float gravityScale = 3.0f;
    private float drag = 0.1f;
    private float pullDrag = 0.5f;
    private float buoyancy = 0.0f;
    private float maxFallSpeed = -30.0f;
    private float friction = 0.9f;
    private float airControl = 0.8f;

    private float jumpGravity = -15.0f;
    private bool jumpGravityEnabled = false;
    private float jumpGravityScale = 1.0f;
    private bool isJumping = false;
    private float jumpTimer = 0.0f;
    private const float JUMP_DURATION = 0.3f;

    public RigidBody(Vector3 position, Vector3 size) {
        this.position = new Vector3(position);
        this.size = new Vector3(size);
        this.velocity = Vector3.Zero;
        this.acceleration = Vector3.Zero;
    }

    // Get BBox
    public BBox getBBox() {
        BBox val = new BBox(
            position.X - size.X / 2,
            position.Y - size.Y / 2,
            position.Z - size.Z / 2,
            position.X + size.X / 2,
            position.Y + size.Y / 2,
            position.Z + size.Z / 2
        );

        return val;
    }

    // Apply Jump Gravity
    public void applyJumpGravity() {
        if(!jumpGravityEnabled || !isJumping) return;

        if(jumpTimer > 0) {
            float effectiveGravity = jumpGravity * jumpGravityScale;
            float v = effectiveGravity * mass;

            applyForce(new Vector3(0.0f, v, 0.0f));
        } else {
            isJumping = false;
        }
    }

    // Apply Buoyancy
    public void applyBuoyancy() {
        if(buoyancy != 0 && position.Y < 0) {
            float depth = MathF.Abs(position.Y);
            float force = buoyancy * depth * mass;
            applyForce(new Vector3(0.0f, force, 0.0f));
        }
    }

    /**
     * 
     * Apply Force
     *
     */
    public void applyForce(Vector3 force) {
        acceleration += force / mass;
    }

    /**
     * 
     * Position
     *
     */
    public void setPosition(Vector3 position) {
        this.position = new Vector3(position);
    }

    public Vector3 getPosition() {
        return new Vector3(position);
    }

    /**
     * 
     * Velocity
     *
     */
    public void setVelocity(Vector3 velocity) { 
        this.velocity = new Vector3(velocity);
    }

    public Vector3 getVelocity() { 
        return new Vector3(velocity);
    }

    /**
     * 
     * Buoyancy
     *
     */
    public void setBuoyancy(float buoyancy) {
        this.buoyancy = buoyancy;
    }

    public float getBuoyancy() {
        return buoyancy;
    }

    /**
     * 
     * Size
     *
     */
    public void setSize(Vector3 size) {
        this.size = new Vector3(size);
    }

    public Vector3 getSize() {
        return new Vector3(size);
    }

    /**
     * 
     * On Surface
     *
     */
    public void setOnSurface(bool onSurface) {
        this.onSurface = onSurface;
    }

    public bool isOnSurface() {
        return onSurface;
    }

    /**
     * 
     * Mass
     *
     */
    public void setMass(float mass) {
        this.mass = mass;
    }

    public float getMass() {
        return mass;
    }

    /**
     * 
     * Gravity
     *
     */
    public void setGravity(float gravity) {
        this.gravity = gravity;
    }

    public float getGravity() {
        return gravity;
    }

    public void setGravityScale(float gravityScale) {
        this.gravityScale = gravityScale;
    } 

    public float getGravityScale() {
        return gravityScale;
    }

    public void setGravityEnabled(bool gravityEnabled) {
        this.gravityEnabled = gravityEnabled;
    }

    public bool isGravityEnabled() {
        return gravityEnabled;
    }

    /**
     * 
     * Drag
     *
     */
    public void setDrag(float drag) {
        this.drag = drag;
    }

    public float getDrag() {
        return drag;
    }

    public void setPullDrag(float pullDrag) {
        this.pullDrag = pullDrag;
    }

    public float getPullDrag() {
        return pullDrag;
    }

    /**
     * 
     * Jump Gravity
     *
     */
    public void setJumpGravityEnabled(bool jumpGravityEnabled) {
        this.jumpGravityEnabled = jumpGravityEnabled;
    }

    public bool isJumpGravityEnabled() {
        return jumpGravityEnabled;
    }

    public void setJumpGravity(float jumpGravity) {
        this.jumpGravity = jumpGravity;
    }

    public void setJumpGravityScale(float jumpGravityScale) {
        this.jumpGravityScale = jumpGravityScale;
    }

    public void setJumping(bool isJumping) {
        this.isJumping = isJumping;
        if(isJumping) jumpTimer = JUMP_DURATION;
    }

    public float getJumpGravity() {
        return jumpGravity;
    }

    public float getJumpGravityScale() {
        return jumpGravityScale;
    }

    public bool getJumping() {
        return isJumping;
    }

    /**
     * 
     * Max Fall Speed
     *
     */
    public void setMaxFallSpeed(float maxFallSpeed) {
        this.maxFallSpeed = maxFallSpeed;
    }

    public float getMaxFallSpeed() {
        return maxFallSpeed;
    }

    /**
     * 
     * Friction
     *
     */
    public void setFriction(float friction) {
        this.friction = friction;
    }

    public float getFriction() {
        return friction;
    }

    /**
     * 
     * Air Control
     *
     */
    public void setAirControl(float airControl) {
        this.airControl = airControl;
    }

    public float getAirControl() {
        return airControl;
    }

    /**
     * 
     * Update
     *
     */
    public void update() {
        float deltaTime = Tick.getDeltaTimeI();
        deltaTime = MathF.Min(deltaTime, 0.1f);
        
        if(gravityEnabled) {
            float c = gravity * mass * gravityScale;
            float j = jumpGravity * mass * jumpGravityScale;
            if(jumpGravityEnabled && isJumping) c = j;

            applyForce(new Vector3(0.0f, c, 0.0f));
        }

        applyBuoyancy();

        velocity += acceleration * deltaTime;
        if(position.Y < 0 && pullDrag > 0) velocity *= (1.0f - (pullDrag * deltaTime));

        velocity *= 1.0f - (drag * deltaTime);
        if(velocity.Y < maxFallSpeed) velocity.Y = maxFallSpeed;

        position += velocity * deltaTime;
        acceleration = Vector3.Zero;

        if(jumpTimer > 0) {
            jumpTimer -= deltaTime;
            if(jumpTimer <= 0) isJumping = false;
        }
    }

    /**
     * 
     * Reset
     *
     */
    public void reset() {
        position = Vector3.Zero;
        velocity = Vector3.Zero;
        acceleration = Vector3.Zero;
        size = new Vector3(1.0f, 2.0f, 1.0f);
        
        mass = 1.0f;
        onSurface = false;
        gravityEnabled = true;
        gravity = -20.0f;
        gravityScale = 3.0f;
        drag = 0.1f;
        
        jumpGravityEnabled = false;
        jumpGravity = -15.0f;
        jumpGravityScale = 1.0f;
        isJumping = false;
        jumpTimer = 0.0f;
        
        pullDrag = 0.5f;
        buoyancy = 0.0f;
        maxFallSpeed = -30.0f;
        friction = 0.9f;
        airControl = 0.8f;
    }
}