namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class SphereObject : Collider {
    private Mesh.Mesh mesh;
    private string id;
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

    // Get Type
    public string getType() {
        return type;
    }

    // Get Id
    public string getId() {
        return id;
    }

    // Get Rigid Body
    public RigidBody? getRigidBody() {
        return null;
    }

    // Get Center
    private Vector3 getCenter() {
        return mesh.getPosition(id);
    }

    // On Collision
    public void onCollision(CollisionResult coll) {
        
    }

    // Get Radius
    public float getRadius() {
        Vector3 size = mesh.getSize(id);
        float baseRadius = Math.Max(size.X, Math.Max(size.Y, size.Z)) / 2.0f;

        var renderer = mesh.getMeshRenderer(id);
        if(renderer != null && renderer.isScaled()) {
            Vector3 scale = renderer.getScale();
            return baseRadius * Math.Max(scale.X, Math.Max(scale.Y, scale.Z)); 
        }

        var data = mesh.getData(id);
        if(data != null && data.hasScale()) {
            float[]? s = data.getScale();
            if(s != null) {
                return baseRadius *
                    Math.Max(s[0], Math.Max(s[1],s[2]));
            }
        }

        return baseRadius;
    }

    // Get BBox
    public BBox getBBox() {
        Vector3 center = getCenter();
        float r = getRadius();
        
        return new BBox(
            center.X - r, center.Y - r, center.Z - r,
            center.X + r, center.Y + r, center.Z + r
        );
    }

    // Check Collision
    public CollisionResult checkCollision(BBox bBox) {
        Vector3 center = getCenter();
        float r = getRadius();

        float x = MathF.Max(bBox.minX, MathF.Min(center.X, bBox.maxX));
        float y = MathF.Max(bBox.minY, MathF.Min(center.Y, bBox.maxY));
        float z = MathF.Max(bBox.minZ, MathF.Min(center.Z, bBox.maxZ));
        
        float dist = Vector3.Distance(center, new Vector3(x, y, z));
        if(dist >= r) return new CollisionResult();

        Vector3 playerCenter = new Vector3(
            (bBox.minX + bBox.maxX) / 2.0f,
            (bBox.minY + bBox.maxY) / 2.0f,
            (bBox.minZ + bBox.maxZ) / 2.0f
        );

        Vector3 dir = playerCenter - center;
        dir.Y = 0;
        float horizontal = dir.Length;

        Vector3 normal = 
            horizontal > 0.0001f ?
            Vector3.Normalize(dir) :
            Vector3.UnitX;

        float depth = r - horizontal;
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