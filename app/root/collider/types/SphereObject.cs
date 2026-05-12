namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class SphereObject : Collider {
    private Mesh.Mesh? mesh = null!;
    private string? type = null!;
    private string id;

    private Func<Vector3>? centerProvider;
    private Func<Vector3>? sizeProvider;
    private bool useProviders = false;

    public SphereObject(
        Mesh.Mesh mesh, 
        string id, 
        string type
    ) {
        this.mesh = mesh;
        this.id = id;
        this.type = type;
        this.useProviders = false;
    }
    public SphereObject(
        Func<Vector3> centerProvider,
        Func<Vector3> sizeProvider,
        string id
    ) {
        this.centerProvider = centerProvider;
        this.sizeProvider = sizeProvider;
        this.id = id;
        this.useProviders = true;
        this.mesh = null!;
    }

    // Get Type
    public string getType() {
        return type!;
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
        if(useProviders && centerProvider != null) {
            return centerProvider();
        }
        return mesh!.getPosition(id);
    }

    // Get Radius
    public float getRadius() {
        if(useProviders && sizeProvider != null) {
            Vector3 s = sizeProvider();
            return Math.Max(s.X, Math.Max(s.Y, s.Z)) / 2.0f;
        }

        Vector3 size = mesh!.getSize(id);
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
    public CollisionResult checkCollision(BBox bbox) {
        Vector3 center = getCenter();
        float r = getRadius();

        float x = MathF.Max(bbox.minX, MathF.Min(center.X, bbox.maxX));
        float y = MathF.Max(bbox.minY, MathF.Min(center.Y, bbox.maxY));
        float z = MathF.Max(bbox.minZ, MathF.Min(center.Z, bbox.maxZ));
        
        float dist = Vector3.Distance(center, new Vector3(x, y, z));
        if(dist >= r) return new CollisionResult();

        Vector3 playerCenter = new Vector3(
            (bbox.minX + bbox.maxX) / 2.0f,
            (bbox.minY + bbox.maxY) / 2.0f,
            (bbox.minZ + bbox.maxZ) / 2.0f
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
        BBox bbox,
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
                rigidBody.setOnSurface(true);
            }
        }
        rigidBody.setVelocity(vel);
    }
}