namespace App.Root.Collider.Types;
using App.Root.Collider;
using App.Root.Mesh;
using App.Root.Player;
using App.Root.Utils;
using OpenTK.Mathematics;

class TriangleObject : Collider {
    private Mesh? mesh = null!;
    private string? type = null!;
    private string id;

    private BBox bbox = null!;

    private float[]? verts = null!;
    private Vector3 scale = Vector3.One;
    private List<(Vector3 a, Vector3 b, Vector3 c)> tri = new();

    private Func<Vector3>? positionProvider;
    private Func<float[]?>? verticesProvider;
    private Func<int[]?>? indicesProvider;
    private Func<Vector3>? scaleProvider;
    private Func<BBox>? bboxProvider;

    private bool built = false;
    private bool useProviders = false;

    public TriangleObject(
        Mesh mesh, 
        string id, 
        string type
    ) {
        this.mesh = mesh;
        this.id = id;
        this.type = type;
        this.useProviders = false;

        build();
    }
    public TriangleObject(
        Func<Vector3> positionProvider,
        Func<float[]?> verticesProvider, 
        Func<int[]?> indicesProvider,
        Func<Vector3> scaleProvider,
        Func<BBox> bboxProvider,
        string id
    ) {
        this.positionProvider = positionProvider;
        this.verticesProvider = verticesProvider;
        this.indicesProvider = indicesProvider;
        this.scaleProvider = scaleProvider;
        this.bboxProvider = bboxProvider; 

        this.id = id;
        this.useProviders = true;

        build();
    }

    // Get Type
    public string getType() {
        return type!;
    }

    // Get Id
    public string getId() {
        return id;
    }

    // Get Rigid Body
    public RigidBody? getRigidBody() {
        return null;
    }

    // Get BBox
    public BBox getBBox() {
        if(useProviders && bboxProvider != null) {
            BBox? bb = bboxProvider();
            if(bb != null) return bb;
        }

        bbox = mesh!.getBBox(id);
        return bbox;
    }

    /**
     * 
     * Build
     *
     */
    private void build() {
        if(useProviders) {
            verts = verticesProvider?.Invoke();
            int[]? ind = indicesProvider?.Invoke();
            scale = scaleProvider?.Invoke() ?? Vector3.One;

            if(verts == null) return;
            
            DefTriangle.m(verts, Vector3.Zero, scale);
            tri.Clear();

            if(ind != null) {
                for(int i = 0; i < ind.Length; i += 3) {
                    tri.Add((
                        DefTriangle.getVert(ind[i]), 
                        DefTriangle.getVert(ind[i+1]), 
                        DefTriangle.getVert(ind[i+2])
                    ));
                }
            } else {
                int count = verts.Length / 3;
                for(int i = 0; i < count; i += 3) {
                    tri.Add((
                        DefTriangle.getVert(i), 
                        DefTriangle.getVert(i+1), 
                        DefTriangle.getVert(i+2)
                    ));
                }
            }

            built = true;
            return;
        }

        var data = mesh!.getData(id);
        if(data == null) return;

        verts = data.getVertices()!;
        int[]? indices = data.getIndices();
        if(verts == null) return;

        var renderer = mesh.getMeshRenderer(id);
        Vector3 pos = mesh.getPosition(id);
        if(renderer != null && renderer.isScaled()) {
            scale = renderer.getScale();
        } else if(data.hasScale()) {
            float[]? s = data.getScale();
            if(s != null) scale = new Vector3(s[0], s[1], s[2]);
        }

        DefTriangle.m(verts, pos, scale);
        tri.Clear();

        if(indices != null) {
            for(int i = 0; i < indices.Length; i += 3) {
                tri.Add((
                    DefTriangle.getVert(indices[i]),
                    DefTriangle.getVert(indices[i+1]),
                    DefTriangle.getVert(indices[i+2])
                ));
            }
        } else {
            int count = verts.Length / 3;
            for(int i = 0; i < count; i += 3) {
                tri.Add((
                    DefTriangle.getVert(i),
                    DefTriangle.getVert(i+1),
                    DefTriangle.getVert(i+2)
                ));
            }
        }

        bbox = mesh.getBBox(id);
    }

    /**
     * 
     * Check
     *
     */
    // Check Collision
    public CollisionResult checkCollision(BBox box) {
        if(!built) build();

        BBox bbox = getBBox();
        if(bbox == null || !box.intersects(bbox)) return new CollisionResult();

        Vector3 currentPos;
        if(useProviders && positionProvider != null) {
            currentPos = positionProvider();
        } else {
            currentPos = mesh!.getPosition(id);
        }
        CollisionResult best = new CollisionResult();
        float bestDepth = 0;

        foreach(var (a, b, c) in tri) {
            var result = checkAABB(box, 
                a + currentPos,
                b + currentPos,
                c + currentPos
            );
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
            (box.maxY - box.minY) / 2.0f,
            (box.maxZ - box.minZ) / 2.0f
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

        Vector3 triCenter = (a + b + c) / 3.0f;

        foreach(var axis in axes) {
            float f = 1e-10f;
            if(axis.LengthSquared < f) continue;
            Vector3 n = Vector3.Normalize(axis);

            float p0 = Vector3.Dot(v0, n);
            float p1 = Vector3.Dot(v1, n);
            float p2 = Vector3.Dot(v2, n);
            float triMin = MathF.Min(p0, MathF.Min(p1, p2));
            float triMax = MathF.Max(p0, MathF.Max(p1, p2));

            float r = 
                extents.X * MathF.Abs(n.X) +
                extents.Y * MathF.Abs(n.Y) +
                extents.Z * MathF.Abs(n.Z);

            if(triMin > r || triMax < -r) {
                return new CollisionResult();
            }

            float depth = r - triMin;
            if(depth < minDepth) {
                minDepth = depth;
                Vector3 dir = center - triCenter;
                bestNormal = Vector3.Dot(n, dir) < 0 ? -n : n;
            }
        }


        Vector3[] faceAxes = {
            Vector3.UnitX,
            Vector3.UnitY,
            Vector3.UnitZ
        };
        foreach(var n in faceAxes) {
            float p0 = Vector3.Dot(v0, n);
            float p1 = Vector3.Dot(v1, n);
            float p2 = Vector3.Dot(v2, n);
            float triMin = MathF.Min(p0, MathF.Min(p1, p2));
            float triMax = MathF.Max(p0, MathF.Max(p1, p2));
            float r = Vector3.Dot(extents,
                new Vector3(
                    MathF.Abs(n.X),
                    MathF.Abs(n.Y), 
                    MathF.Abs(n.Z)
                )
            );

            if(triMin > r || triMax < -r) {
                return new CollisionResult();
            }

            float depth = r - triMin;
            if(depth < minDepth) {
                minDepth = depth;
                bestNormal = n;
            }
        }

        return new CollisionResult(
            true,
            bestNormal,
            minDepth,
            this,
            CollisionManager.CollisionType.STATIC_OBJECT
        );
    }
}