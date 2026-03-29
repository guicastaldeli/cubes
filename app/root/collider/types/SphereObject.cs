namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Graphics.ES20;
using OpenTK.Mathematics;

class SphereObject : Collider {
    private Vector3 center;
    private float radius;
    private string type;

    public SphereObject(Vector3 center, float radius, string type) {
        this.center = center;
        this.radius = radius;
        this.type = type;
    }

    public string getType() {
        return type;
    }

    public RigidBody? getRigidBody() {
        return null;
    } 

    public void onCollision(CollisionResult coll) {}

    public BBox getBBox() {
        return new BBox(
            center.X - radius, center.Y - radius, center.Z - radius,
            center.X + radius, center.Y + radius, center.Z + radius
        );
    }

    // Check Collision
    public CollisionResult checkCollision(BBox bBox) {
        float x = MathF.Max(bBox.minX, MathF.Min(center.X, bBox.maxX));
        float y = MathF.Max(bBox.minY, MathF.Min(center.Y, bBox.maxY));
        float z = MathF.Max(bBox.minZ, MathF.Min(center.Z, bBox.maxZ));

        float dist = Vector3.Distance(center, new Vector3(x, y, z));
        if(dist >= radius) return new CollisionResult();

        Vector3 normal = dist > 0.0001f ?
            -Vector3.Normalize(new Vector3(x, y, z) - center) :
            Vector3.UnitY;

        return new CollisionResult(
            true,
            normal,
            radius - dist,
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
        position += collision.normal * (collision.depth + 0.01f);
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