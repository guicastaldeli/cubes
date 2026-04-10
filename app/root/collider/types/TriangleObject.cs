namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class TriangleObject : Collider {
    private Mesh.Mesh mesh;
    private string id;
    private string type;

    private BBox bBox;
    private List<(
        Vector3 a, 
        Vector3 b, 
        Vector3 c
    )> triangles = new();

    public TriangleObject(
        Mesh.Mesh mesh, 
        string id, 
        string type
    ) {
        this.mesh = mesh;
        this.id = id;
        this.type = type;
        build();
    }

    // Get Type
    public string getType() {
        return type;
    }

    // Get Rigid Body
    public RigidBody? getRigidBody() {
        return null;
    }

    // On Collision
    public void onCollision(CollisionResult coll) {}

    // Get BBox
    public BBox getBBox() {
        return bBox;
    }

    // Build
    private void build() {
        var data = mesh.getData(id);
        if(data == null) return;

        float[]? verts = data.getVertices();
        int[]? indices = data.getIndices();
        Vector3 pos = mesh.getPosition(id);
        if(verts == null) return;

        Vector3 getVert(int i) {
            return new Vector3(
                verts[i*3+0] + pos.X,
                verts[i*3+1] + pos.Y,
                verts[i*3+2] + pos.Z
            );
        }   

        triangles.Clear();

        if(indices != null) {
            for(int i = 0; i < indices.Length; i += 3) {
                triangles.Add((
                    getVert(indices[i]),
                    getVert(indices[i+1]),
                    getVert(indices[i+2])
                ));
            }
        } else {
            int count = verts.Length / 3;
            for(int i = 0; i < count; i += 3) {
                triangles.Add((
                    getVert(i),
                    getVert(i+1),
                    getVert(i+2)
                ));
            }
        }

        bBox = mesh.getBBox(id);
    }

    // Check Collision
    public CollisionResult checkCollision(BBox box) {
        if(!box.intersects(bBox)) return new CollisionResult();

        CollisionResult best = new CollisionResult();
        float bestDepth = 0;

        foreach(var (a, b, c) in triangles) {
            var result = checkAABB(box, a, b, c);
            if(result.collided && result.depth > bestDepth) {
                bestDepth = result.depth;
                best = result;
            }
        }

        return best;
    }

    // Check AABB
    private CollisionResult checkAABB(
        BBox box,
        Vector3 a,
        Vector3 b,
        Vector3 c
    ) {
        Vector3 center = new Vector3(
            (box.minX + box.maxX) / 2.0f,
            (box.minY + box.maxY) / 2.0f,
            (box.minZ + box.maxZ) / 2.0f
        );
        Vector3 extents = new Vector3(
            (box.maxX - box.minX) / 2.0f,
            (box.maxY - box.minX) / 2.0f,
            (box.maxZ - box.maxZ) / 2.0f
        );

        Vector3 v0 = a - center;
        Vector3 v1 = b - center;
        Vector3 v2 = c - center;

        Vector3 e0 = v1 - v0;
        Vector3 e1 = v2 - v1;
        Vector3 e2 = v0 - v2;

        Vector3[] axes = {
            new Vector3(0, -e0.Z, e0.Y),
            new Vector3(0, -e1.Z, e1.Y),
            new Vector3(0, -e2.Z, e2.Y),
            new Vector3(e0.Z, 0, -e0.X),
            new Vector3(e1.Z, 0, -e1.X),
            new Vector3(e2.Z, 0, -e2.X),
            new Vector3(-e0.Y, e0.X, 0),
            new Vector3(-e1.Y, e1.X, 0),
            new Vector3(-e2.Y, e2.X, 0),
            Vector3.Cross(e0, e1)
        };

        float minDepth = float.MaxValue;
        Vector3 bestNormal = Vector3.UnitY;

        foreach(var axis in axes) {
            float f = 1e-10f;
            if(axis.LengthSquared < f) continue;
            Vector3 n = Vector3.Normalize(axis);
        }

    }
}