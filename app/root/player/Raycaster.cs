namespace App.Root.Player;
using App.Root.Collider;
using App.Root.Mesh;
using OpenTK.Mathematics;

/**

    Helper Shape class of Meshes
    to help Raycaster detect 
    the collider type...

    */
static class Types {
    public const string CUBE = "cube";
    public const string SPHERE = "sphere";
    public const string TRIANGLE = "triangle";
}

class Shape {
    public Mesh mesh = null!;
    public MeshData data = null!;
    public Raycaster raycaster = null!;

    public string id = null!;
    public Vector3 origin;
    public Vector3 dir;
    public float dist;
    public bool hit;

    /**

        Overloaded Constructors
        to handle different initializations

        */

    // Main Variables
    public Shape(
        Mesh mesh, 
        MeshData data,
        Raycaster raycaster
    ) {
        this.mesh = mesh;
        this.data = data;
        this.raycaster = raycaster;
    }

    // Raycaster Props
    public Shape(
        string id,
        Vector3 origin,
        Vector3 dir,
        float dist,
        bool hit
    ) {
        this.id = id;
        this.origin = origin;
        this.dir = dir;
        this.dist = dist;
        this.hit = hit;
    }
    
    // Get Scaled Radius
    private static float getScaledRadius(Mesh mesh, string id) {
        Vector3 size = mesh.getSize(id);
        float baseRadius = Math.Max(size.X, Math.Max(size.Y, size.Z)) / 2.0f;

        var renderer = mesh.getMeshRenderer(id);
        if(renderer != null && renderer.isScaled()) {
            Vector3 scale = renderer.getScale();
            return baseRadius *
                Math.Max(scale.X, Math.Max(scale.Y, scale.Z));
        }

        return baseRadius;
    }

    /**

        Update Detection

        */
    public Shape update() {
        if(data == null) return this;

        switch(data.colliderShape) {
            case Types.SPHERE:
                Vector3 center = mesh.getPosition(id);
                float r = getScaledRadius(mesh, id);
                hit = raycaster.intersectsSphere(origin, dir, center, r, out dist);
                break;
            case Types.TRIANGLE:
                BBox tBox = mesh.getBBox(id);
                if(!raycaster.intersects(origin, dir, tBox, out _)) {
                    hit = false;
                    dist = 0;
                } else {
                    hit = raycaster.intersectsTriangle(origin, dir, id, out dist);
                }
                break;
            case Types.CUBE:
            default:
                BBox box = mesh.getBBox(id);
                hit = raycaster.intersects(origin, dir, box, out dist);
                break;
        }

        return this;
    }
}

/**

    Main Raycaster class.

    */
class Raycaster {
    private Camera camera;
    private Mesh mesh;

    private float maxDist = 50.0f;

    public Raycaster(Camera camera, Mesh mesh) {
        this.camera = camera;
        this.mesh = mesh;
    }

    // Get Ray
    private (Vector3 origin, Vector3 dir) getRay() {
        Vector3 origin = camera.getPosition();
        Vector3 dir = Vector3.Normalize(camera.getFront());
        return (origin, dir);
    }

    ///
    /// Intersects
    /// 
    public bool intersects(
        Vector3 origin,
        Vector3 dir,
        BBox box,
        out float dist
    ) {
        dist = 0;
        float tMin = float.MinValue;
        float tMax = float.MaxValue;

        float[] origins = {
            origin.X,
            origin.Y,
            origin.Z
        };
        float[] dirs = {
            dir.X,
            dir.Y,
            dir.Z
        };

        float[] mins = {
            box.minX,
            box.minY,
            box.minZ
        };
        float[] maxs = {
            box.maxX,
            box.maxY,
            box.maxZ
        };

        int min = 0;
        int max = 3;
        for(int i = min; i < max; i++) {
            float f = 1e-6f;
            if(Math.Abs(dirs[i]) < f) {
                if(
                    origins[i] < mins[i] ||
                    origins[i] > maxs[i]
                ) {
                    return false;
                }
            } else {
                float t1 = (mins[i] - origins[i]) / dirs[i];
                float t2 = (maxs[i] - origins[i]) / dirs[i];
                if(t1 > t2) (t1, t2) = (t2, t1);

                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                if(tMin > tMax) return false;
            }
        }

        if(tMax < 0) return false;
        dist = tMin >= 0 ? tMin : tMax;
        return true;
    }

    public bool intersectsSphere(
        Vector3 origin,
        Vector3 dir,
        Vector3 center,
        float radius,
        out float dist
    ) {
        dist = 0;
        Vector3 oc = origin - center;

        float a = Vector3.Dot(dir, dir);
        float b = 2.0f * Vector3.Dot(oc, dir);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;
        if(discriminant < 0) return false;

        float t = (-b - MathF.Sqrt(discriminant)) / (2.0f * a);
        if(t < 0) t = (-b + MathF.Sqrt(discriminant)) / (2.0f * a);
        if(t < 0) return false;

        dist = t;
        return true;
    }

    public bool intersectsTriangle(
        Vector3 origin,
        Vector3 dir,
        string id,
        out float dist
    ) {
        dist = 0;
        float bDist = float.MaxValue;
        bool hit = false;

        MeshData? data = mesh.getData(id);
        if(data == null) return false;

        float[]? verts = data.getVertices();
        int[]? indices = data.getIndices();
        if(verts == null) return false;

        var renderer = mesh.getMeshRenderer(id);

        Vector3 pos = mesh.getPosition(id);
        Vector3 scale = Vector3.One;
        if(renderer != null && renderer.isScaled()) {
            scale = renderer.getScale();
        } else if(data.hasScale()) {
            float[]? s = data.getScale();
            if(s != null) scale = new Vector3(s[0], s[1], s[2]);
        }

        Vector3 getVert(int i) => new Vector3(
            verts[i*3+0] * scale.X + pos.X,
            verts[i*3+1] * scale.Y + pos.Y,
            verts[i*3+2] * scale.Z + pos.Z
        );

        void t(Vector3 a, Vector3 b, Vector3 c) {
            Vector3 edge1 = b - a;
            Vector3 edge2 = c - a;
            Vector3 h = Vector3.Cross(dir, edge2);
            float det = Vector3.Dot(edge1, h);

            float f = 1e-6f;
            if(MathF.Abs(det) < f) return;

            float invDet = 1.0f / det;
            Vector3 s = origin - a;
            float u = invDet * Vector3.Dot(s, h);
            if(u < 0 || u > 1) return;

            Vector3 q = Vector3.Cross(s, edge1);
            float v = invDet * Vector3.Dot(dir, q);
            if(v < 0 || u + v > 1) return;

            float t = invDet * Vector3.Dot(edge2, q);
            if(t >= 0 && t < bDist) {
                bDist = t;
                hit = true;
            }
        }

        if(indices != null) {
            for(int i = 0; i < indices.Length; i+= 3) {
                t(getVert(indices[i]),
                    getVert(indices[i+1]),
                    getVert(indices[i+2])
                );
            }
        } else {
            int count = verts.Length / 3;
            for(int i = 0; i < count; i += 3) {
                t(getVert(i),
                    getVert(i+1),
                    getVert(i+2)
                );
            }
        }

        dist = bDist;
        return hit;
    }

    // Cast
    public string? cast() {
        var (origin, dir) = getRay();

        string? closest = null;
        float closestDist = float.MaxValue;

        foreach(var entry in mesh.getMeshRendererMap()) {
            string id = entry.Key;
            var renderer = entry.Value;

            if(!renderer.isInteractive) continue;
            if(renderer.isHud) continue;
            if(string.IsNullOrEmpty(id)) continue;

            MeshData? data = mesh.getData(id);
            if(data == null) continue;

            Shape shape = new Shape(id, origin, dir, 0, false) {
                mesh = mesh,
                data = data,
                raycaster = this
            }.update();
            
            if(shape.hit && shape.dist < maxDist && shape.dist < closestDist) {
                closestDist = shape.dist;
                closest = id;
            }
        }

        return closest;
    }

    /**
    
        Update
    
        */
    public void update() {
        string? d = cast();
        if(d != null) {
            mesh.renderOutline(new List<string> { d });
        }
    }
}