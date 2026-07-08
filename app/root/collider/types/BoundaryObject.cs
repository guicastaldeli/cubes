namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class BoundaryObject : Collider {
    private BBox bbox;
    private float distance;
    private float thickness = 1.0f;

    private float minHeight;
    private float maxHeight;

    private Vector3 center = Vector3.Zero;

    private bool active = true;

    public BoundaryObject(float distance) {
        this.distance = distance;

        this.minHeight = -distance;
        this.maxHeight = distance;
        
        bbox = new BBox(
            -distance - thickness, float.MinValue, -distance - thickness,
            distance + thickness, float.MaxValue, distance + thickness
        );
    }

    // Get Id
    public string getId() {
        return "";
    }

    // Get BBox
    public BBox getBBox() {
        return new BBox(
            center.X - distance - thickness, float.MinValue, center.Z - distance - thickness,
            center.X + distance + thickness, float.MaxValue, center.Z + distance + thickness
        );
    }

    // Get Rigid Body
    public RigidBody? getRigidBody() {
        return null;
    }

    // Activate
    public void activate() {
        active = true;
    }

    public bool isActive() {
        return active;
    }

    // Deactivate
    public void deactivate() {
        active = false;
    }

    // Height
    public float getMinHeight() {
        return minHeight;
    }

    public float getMaxHeight() {
        return maxHeight;
    }

    // Is Outside Boundary
    public bool isOutsideBoundary(Vector3 position) {
        if(!active) return false;
        
        return MathF.Abs(position.X - center.X) > distance ||
            MathF.Abs(position.Z - center.Z) > distance;
    }

    // Center
    public void setCenter(Vector3 center) {
        this.center = new Vector3(center.X, 0.0f, center.Z);
    }

    public Vector3 getCenter() {
        return this.center;
    }

    // Distance
    public void setDistance(float distance) {
        if(MathF.Abs(this.distance - distance) < 0.001f) return;

        this.distance = distance;
        this.minHeight = -distance;
        this.maxHeight = distance;
    }

    public float getBoundaryDistance() {
        return distance;
    }

    /**
     * 
     * Get Boundary
     *
     */
    // Get Boundary Normal
    public Vector3 getBoundaryNormal(Vector3 position) {
        Vector3 normal = Vector3.Zero;
        Vector3 local = position - center;
        if(MathF.Abs(local.X) > distance) {
            normal.X = local.X > 0 ? -1 : 1;
        } else if(MathF.Abs(local.Z) > distance) {
            normal.Z = local.Z > 0 ? -1 : 1;
        }
        return normal;
    }

    // Get Boundary Far
    public float getBoundaryFar(Vector3 position) {
        Vector3 local = position - center;
        float xFar = MathF.Max(0, MathF.Abs(local.X) - distance);
        float zFar = MathF.Max(0, MathF.Abs(local.Z) - distance);
        return MathF.Max(xFar, zFar);
    }
}