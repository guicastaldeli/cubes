
/**

    Physics Body to set
    general physics.

    */
namespace App.Root.Physics;
using OpenTK.Mathematics;

class PhysicsBody {
    private Vector3 position;
    private Vector3 velocity;
    private bool onSurface;

    private const float GRAVITY = -20.0f;
    private const float DAMPING = 0.98f;
    private const float MIN_VELOCITY = 0.01f;

    public PhysicsBody(Vector3 position) {
        this.position = position;
        this.velocity = Vector3.Zero;
        this.onSurface = false;
    }

    /**

        Position

        */ 
    public Vector3 getPosition() {
        return position;
    }

    public void setPosition(Vector3 position) {
        this.position = position;
    }

    /**

        Velocity

        */ 
    public Vector3 getVelocity() {
        return velocity;
    }

    public void setVelocity(Vector3 velocity) {
        this.velocity = velocity;
    }

    /**

        On Surface

        */ 
    public bool isOnSurface() {
        return onSurface;
    }

    public void setOnSurface(bool val) {
        onSurface = val;
    }

    ///
    /// Apply Gravity
    /// 
    public void applyGravity(float deltaTime) {
        if(!onSurface) {
            velocity.Y += GRAVITY * deltaTime;
        }
    }

    ///
    /// Apply Impulse
    /// 
    public void applyImpulse(Vector3 impulse) {
        velocity += impulse;
    }

    ///
    /// Update
    /// 
    public void update(float deltaTime) {
        velocity *= DAMPING;

        if(MathF.Abs(velocity.X) < MIN_VELOCITY) velocity.X = 0;
        if(MathF.Abs(velocity.Y) < MIN_VELOCITY) velocity.Y = 0;
        if(MathF.Abs(velocity.Z) < MIN_VELOCITY) velocity.Z = 0;
        
        position += velocity * deltaTime;
    }
}