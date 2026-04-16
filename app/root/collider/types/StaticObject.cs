namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class StaticObject : Collider {
    private BBox? bBox;
    private Vector3? position;
    private string? type;
    private string? id;
    
    private float half;
    private float halfX;
    private float halfY;
    private float halfZ;
    private Func<BBox>? bBoxProvider;

    public StaticObject(BBox bBox, string id, string type = "") {
        this.bBox = bBox;
        this.id = id;
        this.type = type;
    }
    public StaticObject(Vector3 position, float half, string id, string type = "") {
        this.position = position;
        this.halfX = half;
        this.halfY = half;
        this.halfZ = half;
        this.id = id;
        this.type = type;
    }
    public StaticObject(
        Vector3 position, 
        float halfX,
        float halfY,
        float halfZ, 
        string id,
        string type = ""
    ) {
        this.position = position;
        this.halfX = halfX;
        this.halfY = halfY;
        this.halfZ = halfZ;
        this.id = id;
        this.type = type;
    }
    public StaticObject(Func<BBox> bBoxProvideer, string id, string type = "") {
        this.bBoxProvider = bBoxProvideer;
        this.id = id;
        this.type = type;
    }

    // Get BBox
    public BBox getBBox() {
        if(bBoxProvider != null) return bBoxProvider();
        if(position.HasValue) {
            Vector3 p = position.Value;
            return new BBox(
                p.X - halfX, p.Y - halfY, p.Z - halfZ,
                p.X + halfX, p.Y + halfY, p.Z + halfZ
            );
        }
        return bBox!;
    }

    // Get Rigid Body
    public RigidBody? getRigidBody() {
        return null;
    }

    // On Collision
    public void onCollision(CollisionResult coll) {
        
    }

    // Get Type
    public string getType() {
        string val = type ?? "";
        return val;
    }

    // Get Id
    public string getId() {
        string val = id ?? "";
        return val;
    }

    // Check Collision
    public CollisionResult checkCollision(BBox bBox) {
        BBox self = getBBox();
        if(bBox.intersects(self)) {
            return calcCollision(bBox, self);
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
        Vector3 correction = collision.normal * collision.depth;
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