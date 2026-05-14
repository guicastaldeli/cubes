namespace App.Root.Player;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.World.Platform;
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
    public static Vector3? set(BBox box, Vector3 hitPoint, Collider collider, float height) {
        var (face, dist) = getClosestFace(box, hitPoint);

        bool isGrid = MeshInteractionRegistry.getInstance().isGrid(collider.getId());
        if(isGrid) {
            if(face != Face.Top) return null;

            float? topY = Platform.topSurfaceY;
            if(!topY.HasValue) return null;

            float t = 0.05f;
            if(Math.Abs(box.maxY - topY.Value) > t) return null;

            return new Vector3(hitPoint.X, box.maxY + height, hitPoint.Z);
        }

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

    public static Vector3? set(BBox box, Vector3 hitPoint, Collider collider, Vector3 halfExtents) {
        float eps = 0.02f;
        var (face, dist) = getClosestFace(box, hitPoint);
        if(dist > eps) return null;

        bool isGrid = MeshInteractionRegistry.getInstance().isGrid(collider.getId());
        if(isGrid) {
            if(face != Face.Top) return null;
            float? topY = Platform.topSurfaceY;
            if(!topY.HasValue) return null;
            float t = 0.05f;
            if(Math.Abs(box.maxY - topY.Value) > t) return null;
            return new Vector3(hitPoint.X, box.maxY + halfExtents.Y, hitPoint.Z);
        }

        return face switch {
            Face.Top    => new Vector3(hitPoint.X, box.maxY + halfExtents.Y, hitPoint.Z),
            Face.Bottom => new Vector3(hitPoint.X, box.minY - halfExtents.Y, hitPoint.Z),
            Face.Front  => new Vector3(hitPoint.X, hitPoint.Y, box.maxZ + halfExtents.Z),
            Face.Back   => new Vector3(hitPoint.X, hitPoint.Y, box.minZ - halfExtents.Z),
            Face.Right  => new Vector3(box.maxX + halfExtents.X, hitPoint.Y, hitPoint.Z),
            Face.Left   => new Vector3(box.minX - halfExtents.X, hitPoint.Y, hitPoint.Z),
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

    // Snap to Face
    private Vector3 snapToFace(BBox box, Vector3 hitPoint) {
        float dTop = Math.Abs(hitPoint.Y - box.maxY);
        float dBottom = Math.Abs(hitPoint.Y - box.minY);
        float dFront = Math.Abs(hitPoint.Z - box.maxZ);
        float dBack = Math.Abs(hitPoint.Z - box.minZ);
        float dRight = Math.Abs(hitPoint.X - box.maxX);
        float dLeft = Math.Abs(hitPoint.X - box.minX);

        float min = Math.Min(dTop, Math.Min(dBottom, Math.Min(dFront, Math.Min(dBack, Math.Min(dRight, dLeft)))));

        if(min == dTop) return new Vector3(hitPoint.X, box.maxY, hitPoint.Z);
        if(min == dBottom) return new Vector3(hitPoint.X, box.minY, hitPoint.Z);
        if(min == dFront) return new Vector3(hitPoint.X, hitPoint.Y, box.maxZ);
        if(min == dBack) return new Vector3(hitPoint.X, hitPoint.Y, box.minZ);
        if(min == dRight) return new Vector3(box.maxX, hitPoint.Y, hitPoint.Z);
        return new Vector3(box.minX, hitPoint.Y, hitPoint.Z);
    }

    // Find Placement Point
    public Vector3? findPlacementPoint(float height) {
        Vector3 origin = camera.getPosition();
        Vector3 dir = Vector3.Normalize(camera.getFront());

        Vector3? bestHit = null;
        float bestDist = float.MaxValue;

        foreach(var collider in collisionManager.getColliders()) {
            BBox box = collider.getBBox();
            if(box == null) continue;

            if(!raycaster.intersects(origin, dir, box, out float dist)) continue;
            if(dist > maxDist || dist >= bestDist) continue;
        
            Vector3 hitPoint = origin + dir * dist;
            hitPoint = snapToFace(box, hitPoint);

            Vector3? surfaceHit = Sides.set(box, hitPoint, collider, height);
            if(surfaceHit == null) continue;

            bestHit = surfaceHit;
            bestDist = dist;
        }

        return bestHit;
    }

    public Vector3? findPlacementPoint(Vector3 halfExtents) {
        Vector3 origin = camera.getPosition();
        Vector3 dir = Vector3.Normalize(camera.getFront());

        Vector3? bestHit = null;
        float bestDist = float.MaxValue;

        foreach(var collider in collisionManager.getColliders()) {
            BBox box = collider.getBBox();
            if(box == null) continue;

            if(!raycaster.intersects(origin, dir, box, out float dist)) continue;
            if(dist > maxDist || dist >= bestDist) continue;
        
            Vector3 hitPoint = origin + dir * dist;
            hitPoint = snapToFace(box, hitPoint);

            Vector3? surfaceHit = Sides.set(box, hitPoint, collider, halfExtents);
            if(surfaceHit == null) continue;

            bestHit = surfaceHit;
            bestDist = dist;
        }

        return bestHit;
    }
}