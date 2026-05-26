namespace App.Root.Player;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.Utils;
using OpenTK.Mathematics;

/**

    Helper Shape class of Meshes
    to help Raycaster detect 
    the collider type...

    */
static class ShapeType {
    public const string CUBE = "cube";
    public const string SPHERE = "sphere";
    public const string TRIANGLE = "triangle";
}

class Shape {
    public Mesh mesh = null!;
    public MeshData data = null!;
    public Raycaster raycaster = null!;

    public string id = null!;
    public string? meshType = null;
    public Vector3 origin;
    public Vector3 dir;
    public float dist;
    public bool hit;

    public Vector3? instancedPosition = null;
    public Vector3? instancedSize = null;

    public Shape(
        Mesh mesh, 
        MeshData data,
        Raycaster raycaster
    ) {
        this.mesh = mesh;
        this.data = data;
        this.raycaster = raycaster;
    }
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
    
    /**

        Scaled Radius

        */
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

    private static float getScaledRadius(Vector3 size) {
        float val = Math.Max(size.X, Math.Max(size.Y, size.Z)) / 2.0f;
        return val;
    }

    /**

        Update

        */
    public Shape update() {
        if(data == null) return this;

        switch(data.colliderShape) {
            case ShapeType.SPHERE:
                Vector3 center;
                float r;

                if(instancedPosition.HasValue && instancedSize.HasValue) {
                    center = instancedPosition.Value;
                    r = getScaledRadius(instancedSize.Value);
                } else {
                    center = mesh.getPosition(id);
                    r = getScaledRadius(mesh, id);
                }

                hit = raycaster.intersectsSphere(origin, dir, center, r, out dist);
                break;
            case ShapeType.TRIANGLE:
                BBox tbox;
    
                if(instancedPosition.HasValue && instancedSize.HasValue) {
                    tbox = BBox.setFromCenterI(instancedPosition.Value, instancedSize.Value);                
                } else {
                    tbox = mesh.getBBox(id);
                }

                if(!raycaster.intersects(origin, dir, tbox, out _)) {
                    hit = false;
                    dist = 0;
                } else {
                    if(instancedPosition.HasValue && meshType != null) {
                        float scale = instancedSize!.Value.X / MeshCollider.getCachedSize(meshType).X;
                        
                        hit = raycaster.intersectsTriangle(origin, dir, meshType, instancedPosition.Value, scale, out dist);
                    } else {
                        hit = raycaster.intersectsTriangle(origin, dir, id, out dist);
                    }
                }

                break;
            case ShapeType.CUBE:
            default:
                BBox bbox;

                if(instancedPosition.HasValue && instancedSize.HasValue) {
                    bbox = BBox.setFromCenterI(instancedPosition.Value, instancedSize.Value);
                } else {
                    bbox = mesh.getBBox(id);
                }
    
                hit = raycaster.intersects(origin, dir, bbox, out dist);
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

    public Action? onRenderOutline = null;
    private bool isActive = true;

    private const float MAX_DIST = 300.0f;

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

    // Set Active
    public void setActive(bool active) {
        isActive = active;
    }

    /**
    
        Intersects
    
        */
    // Main
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

    // Sphere
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

    // Triangle
    public bool intersectsTriangle(
        Vector3 origin,
        Vector3 dir,
        string id,
        out float dist
    ) {
        float f = 1.0f;
        bool val = intersectsTriangle(origin, dir, id, null, f, out dist); 
        return val;
    }

    public bool intersectsTriangle(
        Vector3 origin,
        Vector3 dir,
        string id,
        Vector3? instancedPos,
        float instancedScale,
        out float dist
    ) {
        dist = 0;

        float[]? verts;
        int[]? indices;
        Vector3 pos;
        Vector3 scale;

        if(instancedPos.HasValue) {
            if(!MeshCollider.cachedVertices.TryGetValue(id, out verts) || verts == null) {
                return false;
            }
            MeshCollider.cachedIndices.TryGetValue(id, out indices);

            pos = instancedPos.Value;
            scale = new Vector3(instancedScale);
        } else {
            MeshData? data = mesh.getData(id);
            if(data == null) return false;

            verts = data.getVertices();
            indices = data.getIndices();

            pos = mesh.getPosition(id);
            scale = Vector3.One;

            var renderer = mesh.getMeshRenderer(id);
            if(renderer != null && renderer.isScaled()) {
                scale = renderer.getScale();
            } else if(data.hasScale()) {
                float[]? s = data.getScale();
                if(s != null) scale = new Vector3(s[0], s[1], s[2]);
            }
        }

        DefTriangle.r(dir, origin);
        DefTriangle.m(verts!, pos, scale);

        if(indices != null) {
            for(int i = 0; i < indices.Length; i+= 3) {
                DefTriangle.t(
                    DefTriangle.getVert(indices[i]),
                    DefTriangle.getVert(indices[i+1]),
                    DefTriangle.getVert(indices[i+2])
                );
            }
        } else {
            int count = verts!.Length / 3;
            for(int i = 0; i < count; i += 3) {
                DefTriangle.t(
                    DefTriangle.getVert(i),
                    DefTriangle.getVert(i+1),
                    DefTriangle.getVert(i+2)
                );
            }
        }

        dist = DefTriangle.bdist;
        return DefTriangle.hit;
    }

    /**
    
        Cast
    
        */
    public string? cast() {
        if(!isActive) return null;

        var (origin, dir) = getRay();
    
        string? closest = null;
        float closestDist = float.MaxValue;

        foreach(var entry in mesh.getMeshRendererMap()) {
            string id = entry.Key;
            var renderer = entry.Value;

            if(!renderer.isInteractive) continue;
            if(renderer.isHud) continue;
            if(renderer.isInstanced) continue;
            if(string.IsNullOrEmpty(id)) continue;

            MeshData? data = mesh.getData(id);
            if(data == null) continue;

            Shape shape = new Shape(id, origin, dir, 0, false) {
                mesh = mesh,
                data = data,
                raycaster = this
            }.update();
            
            if(shape.hit && shape.dist < MAX_DIST && shape.dist < closestDist) {
                closestDist = shape.dist;
                closest = id;
            }
        }
        foreach(var (id, instancedMeshType, position, scale) in MeshCollider.getInstancedColliders()) {
            MeshData? data = mesh.getData(instancedMeshType);
            if(data?.colliderShape == null) continue;

            Vector3 size = MeshCollider.getCachedSize(instancedMeshType) * scale;

            Shape shape = new Shape(id, origin, dir, 0, false) {
                mesh = mesh,
                data = data,
                raycaster = this,
                instancedPosition = position,
                instancedSize = size,
                meshType = instancedMeshType
            }.update();

            if(shape.hit && shape.dist < MAX_DIST && shape.dist < closestDist) {
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
        if(!isActive) return;

        onRenderOutline = null;

        string? d = cast();
        if(d == null) return;

        onRenderOutline = () => {
            var instanced = EventStream.get<Dictionary<string, List<string>>>("stream-id");
            if(instanced != null) {
                foreach(var (id, list) in instanced) {
                    int index = list.IndexOf(d);
                    if(index >= 0) {
                        Vector3 pos = MeshCollider.getInstancedPosition(d);
                        mesh.renderOutlineAll(d, id, pos, index);
                        return;
                    }
                }
            }

            mesh.renderOutlineAll(d);
        };
    }
}