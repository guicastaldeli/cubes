namespace App.Root.Player;
using App.Root.Collider;
using OpenTK.Mathematics;

class PlacementRaycaster {
    private Camera camera;
    private Mesh.Mesh mesh;
    private CollisionManager collisionManager;
    private Raycaster raycaster;

    private float maxDist = 20.0f;
    private float? platformY = null;

    public PlacementRaycaster(
        Camera camera,
        Mesh.Mesh mesh,
        CollisionManager collisionManager,
        Raycaster raycaster
    ) {
        this.camera = camera;
        this.mesh = mesh;
        this.collisionManager = collisionManager;
        this.raycaster = raycaster;
    }

    // Set Platform Y
    public void setPlatformY(float y) {
        platformY = y;
    }

    // Find Placement Point
    public Vector3? findPlacementPoint() {
        Vector3 origin = camera.getPosition();
        Vector3 dir = Vector3.Normalize(camera.getFront());

        Vector3? bestHit = null;
        float bestDist = float.MaxValue;

        foreach(var entry in mesh.getMeshRendererMap()) {
            string id = entry.Key;
            var renderer = entry.Value;
            if(renderer.isHud || string.IsNullOrEmpty(id)) continue;

            BBox box = mesh.getBBox(id);
            if(!raycaster.intersects(origin, dir, box, out float dist)) continue;
            if(dist > maxDist || dist >= bestDist) continue;

            Vector3 hitPoint = origin + dir * dist;
            Vector3 size = mesh.getSize(id);
            
            bestHit = new Vector3(
                hitPoint.X, 
                box.maxX + size.Y / 2.0f, 
                hitPoint.Z
            );
            bestDist = dist;
        }

        if(platformY.HasValue) {
            float planeY = platformY.Value;
            float f = 1e-6f;
            
            if(Math.Abs(dir.Y) > f) {
                float t = (planeY - origin.Y) / dir.Y;
                if(t > 0 && t < maxDist && t < bestDist) {
                    Vector3 hitPoint = origin + dir * t;
                    bestHit = hitPoint;
                    bestDist = t;
                }
            }
        }

        return bestHit;
    }
}