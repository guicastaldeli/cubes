
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

    private const float SLEEP_THRESHOLD = 0.1f;
    private const int SLEEP_FRAMES_REQUIRED = 10;
    private bool isSleeping = false;

    private int stableFrames = 0;
    private int spawnFrames = 15;

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
        if(isSleeping) {
            isSleeping = false;
            stableFrames = 0;
        }
    }

    /**

        Velocity

        */ 
    public Vector3 getVelocity() {
        return velocity;
    }

    public void setVelocity(Vector3 velocity) {
        this.velocity = velocity;
        if(isSleeping &&
            velocity.LengthSquared > MIN_VELOCITY *
            MIN_VELOCITY
        ) {
            isSleeping = false;
            stableFrames = 0;
        }
    }

    /**

        On Surface

        */ 
    public bool isOnSurface() {
        return onSurface;
    }

    public void setOnSurface(bool val) {
        bool wasOnSurface = onSurface;
        if(val && !wasOnSurface) {
            velocity.Y = 0;
        }

        onSurface = val;
        if(!val && wasOnSurface) {
            isSleeping = false;
            stableFrames = 0;
        }
    }

    /**
    
        Sleeping
    
        */
    public bool isSleepingState() {
        return isSleeping;
    }

    public void wake() {
        isSleeping = false;
        stableFrames = 0;
    }

    /**
    
        Apply Gravity
    
        */
    public void applyGravity(float deltaTime) {
        if(isSleeping) return;

        if(spawnFrames > 0) {
            spawnFrames--;
            return;
        }

        if(!onSurface) {
            velocity.Y += GRAVITY * deltaTime;
            stableFrames = 0;
        } else {
            velocity.Y = 0;
        }
    }

    /**
    
        Apply Impulse
    
        */
    public void applyImpulse(Vector3 impulse) {
        velocity += impulse;
        isSleeping = false;
        stableFrames = 0;
    }

    /**
    
        Update
    
        */
    public void update(float deltaTime) {
        if(isSleeping) return;

        velocity *= DAMPING;

        if(MathF.Abs(velocity.X) < MIN_VELOCITY) velocity.X = 0;
        if(MathF.Abs(velocity.Y) < MIN_VELOCITY) velocity.Y = 0;
        if(MathF.Abs(velocity.Z) < MIN_VELOCITY) velocity.Z = 0;
        
        position += velocity * deltaTime;

        if(onSurface && 
            velocity.LengthSquared < SLEEP_THRESHOLD * 
            SLEEP_THRESHOLD
        ) {
            stableFrames++;
            if(stableFrames >= SLEEP_FRAMES_REQUIRED) {
                isSleeping = true;
                velocity = Vector3.Zero;
            }    
        } else {
            stableFrames = 0;
        }
    }
}