namespace App.Root.Player;
using App.Root.Collider;
using App.Root.Mesh;
using OpenTK.Mathematics;

/**

    Helper to palcement on
    other meshes faces...

    */
enum Face {
    Top,
    Bottom,
    Front,
    Back,
    Right,
    Left
}

class Sides {
    // Get Closest Face
    private static (Face face, float dist) getClosestFace(BBox box, Vector3 hit) {
        var faces = new (Face face, float dist)[] {
            (Face.Top, Math.Abs(hit.Y - box.maxY)),
            (Face.Bottom, Math.Abs(hit.Y - box.minY)),
            (Face.Front, Math.Abs(hit.Z - box.maxZ)),
            (Face.Back, Math.Abs(hit.Z - box.minZ)),
            (Face.Right, Math.Abs(hit.X - box.maxX)),
            (Face.Left, Math.Abs(hit.X - box.minX))
        };
        return faces.MinBy(f => f.dist);
    }

    // Set
    public static Vector3? set(
        BBox box, 
        Vector3 hitPoint, 
        Collider collider, 
        float height
    ) {
        float eps = 0.02f;
        bool isGrid = 
            MeshInteractionRegistry
                .getInstance()
                .isGrid(collider.getId());

        var (face, dist) = getClosestFace(box, hitPoint);
        if(dist > eps) return null;

        return face switch {
            Face.Top => new Vector3(hitPoint.X, box.maxY + height, hitPoint.Z),
            Face.Bottom => isGrid ? null : new Vector3(hitPoint.X, box.minY - height, hitPoint.Z),
            Face.Front => isGrid ? null : new Vector3(hitPoint.X, hitPoint.Y, box.maxZ + height),
            Face.Back => isGrid ? null : new Vector3(hitPoint.X, hitPoint.Y, box.minZ - height),
            Face.Right => isGrid ? null : new Vector3(box.maxX + height, hitPoint.Y, hitPoint.Z),
            Face.Left => isGrid ? null : new Vector3(box.minX - height, hitPoint.Y, hitPoint.Z),
            _ => null
        };
    }
}

/**

    Main Placement Raycaster
    class.

    */
class PlacementRaycaster {
    private Camera camera;
    private CollisionManager collisionManager;
    private Raycaster raycaster;

    private float maxDist = 20.0f;

    public PlacementRaycaster(
        Camera camera,
        CollisionManager collisionManager,
        Raycaster raycaster
    ) {
        this.camera = camera;
        this.collisionManager = collisionManager;
        this.raycaster = raycaster;
    }

    // Find Placement Point
    public Vector3? findPlacementPoint(float height) {
        Vector3 origin = camera.getPosition();
        Vector3 dir = Vector3.Normalize(camera.getFront());

        Vector3? bestHit = null;
        float bestDist = float.MaxValue;

        foreach(var collider in collisionManager.getColliders()) {
            BBox box = collider.getBBox();

            if(!raycaster.intersects(origin, dir, box, out float dist)) continue;
            if(dist > maxDist || dist >= bestDist) continue;
        
            Vector3 hitPoint = origin + dir * dist;

            Vector3? surfaceHit = Sides.set(box, hitPoint, collider, height);
            if(surfaceHit == null) continue;

            bestHit = surfaceHit;
            bestDist = dist;
        }

        return bestHit;
    }
}