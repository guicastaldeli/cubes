namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class StaticObject : Collider {
    private BBox bBox;
    private string type;

    public StaticObject(BBox bBox, string type) {
        this.bBox = bBox;
        this.type = type;
    }

    public BBox getBBox() {
        return bBox;
    }

    public RigidBody? getRigidBody() {
        return null;
    }

    public void onCollision(CollisionResult coll) {}

    // Get Type
    public string getType() {
        return type;
    }

    // Check Collision
    public CollisionResult checkCollision(BBox bBox) {
        if(bBox.intersects(this.bBox)) {
            return calcCollision(bBox, this.bBox);
        }
        return new CollisionResult();
    }

    // Calculate Collision
    private CollisionResult calcCollision(BBox a, BBox b) {
        float xOverlap = MathF.Min(a.maxX, b.maxX) - MathF.Max(a.minX, b.minX);
        float yOverlap = MathF.Min(a.maxY, b.maxY) - MathF.Max(a.minY, b.minY);
        float zOverlap = MathF.Min(a.maxZ, b.maxZ) - MathF.Max(a.minZ, b.minZ);

        float minOverlap = MathF.Min(MathF.Min(xOverlap, yOverlap), zOverlap);
        Vector3 normal = Vector3.Zero;
        if(minOverlap == xOverlap) {
            normal = new Vector3(a.maxX > b.maxX ? 1 : -1, 0, 0);
        } else if(minOverlap == yOverlap) {
            normal = new Vector3(0, a.maxY > b.maxY ? 1 : -1, 0);
        } else {
            normal = new Vector3(0, 0, a.maxZ > b.maxZ ? 1 : -1);
        }

        return new CollisionResult(
            true,
            normal,
            minOverlap,
            this,
            CollisionManager.CollisionType.STATIC_OBJECT
        );
    }

    // Resolve Collision
    public static void resolveCollision(
        Vector3 position, 
        BBox bBox, 
        RigidBody rigidBody,
        CollisionResult collision
    ) {
        Vector3 correction = collision.normal * (collision.depth + 0.01f);
        position += correction;
        rigidBody.setPosition(position);

        Vector3 vel = rigidBody.getVelocity();
        float dot = Vector3.Dot(vel, collision.normal);
        if(dot < 0) {
            vel -= collision.normal * dot;
            if(collision.normal.Y > 0.5f) {
                vel.Y = 0;
                rigidBody.setOnGround(true);
            }
        }
        rigidBody.setVelocity(vel);
    }
}