namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class BoundaryObject : Collider {
    private BBox bBox;
    private float distance;
    private float thickness = 100.0f;

    private float minHeight;
    private float maxHeight;

    private bool active = true;

    public BoundaryObject(float distance) {
        this.distance = distance;

        this.minHeight = -distance;
        this.maxHeight = distance;
        
        bBox = new BBox(
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
        return bBox;
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
        
        return MathF.Abs(position.X) > distance ||
            MathF.Abs(position.Z) > distance;
    }

    /**
    
        Get Boundary

        */
    public Vector3 getBoundaryNormal(Vector3 position) {
        Vector3 normal = Vector3.Zero;
        if(MathF.Abs(position.X) > distance) {
            normal.X = position.X > 0 ? -1 : 1;
        } else if(MathF.Abs(position.Z) > distance) {
            normal.Z = position.Z > 0 ? -1 : 1;
        }
        return normal;
    }

    public float getBoundaryFar(Vector3 position) {
        float xFar = MathF.Max(0, MathF.Abs(position.X) - distance);
        float zFar = MathF.Max(0, MathF.Abs(position.Z) - distance);
        return MathF.Max(xFar, zFar);
    }

    public float getBoundaryDistance() {
        return distance;
    }
}