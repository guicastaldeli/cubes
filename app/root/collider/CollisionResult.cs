namespace App.Root.Collider;
using OpenTK.Mathematics;

class CollisionResult {
    public bool collided = false;
    public Vector3 normal = Vector3.Zero;
    public float depth = 0.0f;
    public Collider? otherCollider = null;
    public CollisionManager.CollisionType type = CollisionManager.CollisionType.STATIC_OBJECT;

    public CollisionResult() {}
    public CollisionResult(
        bool collided, 
        Vector3 normal, 
        float depth, 
        Collider? other, 
        CollisionManager.CollisionType type
    ) {
        this.collided = collided;
        this.normal = normal;
        this.depth = depth;
        this.otherCollider = other;
        this.type = type;
    }
}