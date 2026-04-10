namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Player;
using OpenTK.Mathematics;

class TriangleObject : Collider {
    private Mesh.Mesh mesh;
    private string id;
    private string type;

    private BBox bbox;
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
        return bbox;
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

        bbox = mesh.getBBox(id);
    }
}