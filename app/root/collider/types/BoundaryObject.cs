namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class BoundaryObject : Collider {
    private BBox bBox;
    private float distance;
    private float thickness = 100.0f;

    public BoundaryObject(float distance) {
        this.distance = distance;
        bBox = new BBox(
            -distance - thickness, float.MinValue, -distance - thickness,
            distance + thickness, float.MaxValue, distance + thickness
        );
    }

    public BBox getBBox() {
        return bBox;
    }

    public RigidBody? getRigidBody() {
        return null;
    }

    public void onCollision(CollisionResult coll) {}

    public bool isOutsideBoundary(Vector3 position) {
        return MathF.Abs(position.X) > distance ||
            MathF.Abs(position.Z) > distance;
    }

    ///
    /// Get Boundary
    /// 
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