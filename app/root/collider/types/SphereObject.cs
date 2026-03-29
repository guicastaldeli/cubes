namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class SphereObject : Collider {
    private Mesh.Mesh mesh;
    private string id;
    private float radius = 1.5f;
    private string type;

    public SphereObject(
        Mesh.Mesh mesh, 
        string id, 
        string type
    ) {
        this.mesh = mesh;
        this.id = id;
        this.type = type;
    }

    public string getType() {
        return type;
    }

    public RigidBody? getRigidBody() {
        return null;
    }

    private Vector3 getCenter() {
        return mesh.getPosition(id);
    }

    public void onCollision(CollisionResult coll) {}

    public BBox getBBox() {
        Vector3 center = getCenter();
        
        return new BBox(
            center.X - radius, center.Y - radius, center.Z - radius,
            center.X + radius, center.Y + radius, center.Z + radius
        );
    }

    // Check Collision
    public CollisionResult checkCollision(BBox bBox) {
        Vector3 center = getCenter();

        float x = MathF.Max(bBox.minX, MathF.Min(center.X, bBox.maxX));
        float y = MathF.Max(bBox.minY, MathF.Min(center.Y, bBox.maxY));
        float z = MathF.Max(bBox.minZ, MathF.Min(center.Z, bBox.maxZ));
        
        float dist = Vector3.Distance(center, new Vector3(x, y, z));
        if(dist >= radius) return new CollisionResult();

        Vector3 playerCenter = new Vector3(
            (bBox.minX + bBox.maxX) / 2.0f,
            (bBox.minY + bBox.maxY) / 2.0f,
            (bBox.minZ + bBox.maxZ) / 2.0f
        );

        Vector3 dir = playerCenter - center;
        dir.Y = 0;
        float horizontal = dir.Length;

        Vector3 normal = horizontal > 0.0001f ?
            Vector3.Normalize(dir) :
            Vector3.UnitX;

        float depth = radius - horizontal;
        if(depth <= 0) return new CollisionResult();

        return new CollisionResult(
            true,
            normal,
            depth,
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
        position += collision.normal * collision.depth;
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