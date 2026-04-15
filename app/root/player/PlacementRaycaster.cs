namespace App.Root.Player;
using App.Root.Collider;
using App.Root.Mesh;
using OpenTK.Mathematics;

class PlacementRaycaster {
    private Camera camera;
    private Mesh mesh;
    private CollisionManager collisionManager;
    private Raycaster raycaster;

    private float maxDist = 20.0f;

    public PlacementRaycaster(
        Camera camera,
        Mesh mesh,
        CollisionManager collisionManager,
        Raycaster raycaster
    ) {
        this.camera = camera;
        this.mesh = mesh;
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
            float eps = 0.01f;
            Vector3? surfaceHit = null;

            if(hitPoint.Y >= box.maxY - eps) {
                surfaceHit = new Vector3(hitPoint.X, box.maxY + height, hitPoint.Z);
            }
            else if(Math.Abs(hitPoint.Z - box.minZ) < eps || Math.Abs(hitPoint.Z - box.maxZ) < eps) {
                surfaceHit = new Vector3(hitPoint.X, box.maxY + height, hitPoint.Z);
            }
            else if(Math.Abs(hitPoint.X - box.minX) < eps || Math.Abs(hitPoint.X - box.maxX) < eps) {
                surfaceHit = new Vector3(hitPoint.X, box.maxY + height, hitPoint.Z);
            }
        
            if(surfaceHit == null) continue;
            bestHit = surfaceHit;
            bestDist = dist;
        }

        return bestHit;
    }
}