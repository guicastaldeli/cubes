namespace App.Root.Player;

using System.Runtime.Serialization;
using App.Root.Collider;
using OpenTK.Mathematics;

/**

    Interface of Mesh Shapes
    to help Raycaster detect 
    the collider type...

    */
public enum Shape {
    [EnumMember(Value = "cube")] 
    CUBE,
    [EnumMember(Value = "shape")] 
    SHAPE,
    [EnumMember(Value = "triangle")] 
    TRIANGLE
}

class Raycaster {
    private Camera camera;
    private Mesh.Mesh mesh;

    private float maxDist = 50.0f;

    public Raycaster(Camera camera, Mesh.Mesh mesh) {
        this.camera = camera;
        this.mesh = mesh;
    }

    // Get Ray
    private (Vector3 origin, Vector3 dir) getRay() {
        Vector3 origin = camera.getPosition();
        Vector3 dir = Vector3.Normalize(camera.getFront());
        return (origin, dir);
    }

    // Intersects
    private bool intersects(
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

            BBox box = mesh.getBBox(id);
            
            if(intersects(origin, dir, box, out float dist)) {
                if(dist < maxDist && dist < closestDist) {
                    closestDist = dist;
                    closest = id;
                }
            }
        }

        return closest;
    }

    ///
    /// Update
    /// 
    public void update() {
        string? d = cast();
        if(d != null) {
            Console.WriteLine($"Raycaster hit: {d}");
        }
    }
}