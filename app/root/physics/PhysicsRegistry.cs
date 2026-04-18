
using App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Mesh;
using OpenTK.Mathematics;

/**

    Physics registry to
    register which meshes will
    have physics.

    */
namespace App.Root.Physics;

/**

    Shape mesh helper class to help
    detect the collider type.

    */
static class MeshType {
    public const string CUBE = "cube";
    public const string SPHERE = "sphere";
    public const string TRIANGLE = "triangle";
}

class Shape {
    private Mesh.Mesh mesh;
    private MeshData data;
    private PhysicsRegistry.Entry entry;
    private CollisionManager collisionManager;

    public Shape(
        Mesh.Mesh mesh, 
        MeshData data, 
        PhysicsRegistry.Entry entry,
        CollisionManager collisionManager
    ) {
        this.mesh = mesh;
        this.data = data;
        this.entry = entry;
        this.collisionManager = collisionManager;
    }

    /**

        Update

        */
    public void update(string id) {
        switch(data.colliderShape) {
            case MeshType.CUBE:
                entry.collider = new StaticObject(() => mesh.getBBox(id), id);
                break;
            case MeshType.SPHERE:
                entry.collider = new SphereObject(mesh, id, id);
                break;
            case MeshType.TRIANGLE:
                entry.collider = new TriangleObject(mesh, id, id);
                break;
        }
    }
}

/**

    Updater helper to help registry
    detect which is the Mesh physics type.

    */
enum Type {
    DYNAMIC,
    RECEIVER
}

class Updater {
    private Type type;
    private Mesh.Mesh mesh;
    private MeshData data;
    private PhysicsRegistry.Entry entry;
    private CollisionManager collisionManager;

    private Shape shape;

    public Updater(
        Type type, 
        Mesh.Mesh mesh,
        MeshData data,
        PhysicsRegistry.Entry entry,
        CollisionManager collisionManager
    ) {
        this.type = type;
        this.mesh = mesh;
        this.data = data;
        this.entry = entry;
        this.collisionManager = collisionManager;

        this.shape = new Shape(mesh, data, entry, collisionManager);
    }
    
    public void update(string id) {
        if(type == Type.DYNAMIC) {
            Vector3 position = mesh.getPosition(id);
            entry.physicsBody = new PhysicsBody(position);
        }  

        shape.update(id);
    }
}

/**

    Main Physics Registry
    class.

    */
class PhysicsRegistry {
    /**

        Physics Entry

        */
    public class Entry {
        public string id;
        public Type type;
        public PhysicsBody? physicsBody;
        public Collider.Collider? collider;

        public Entry(string id, Type type) {
            this.id = id;
            this.type = type;
        }
    }

    private static PhysicsRegistry? instance;

    private Mesh.Mesh? mesh;
    private MeshData? data;
    private CollisionManager? collisionManager;
    private Updater? updater;

    private Dictionary<string, Entry> entries = new();

    private PhysicsRegistry() {
        this.mesh = null;
        this.data = null;
        this.collisionManager = null;
    }

    public static PhysicsRegistry getInstance() {
        if(instance == null) {
            instance = new PhysicsRegistry();
        }
        return instance;
    }

    ///
    /// Init
    /// 
    public void init(Mesh.Mesh mesh, MeshData data, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.collisionManager = collisionManager;
    }

    ///
    /// Register
    /// 
    public void register(string id, Type type) {
        if(entries.ContainsKey(id)) {
            Console.WriteLine($"PhysicsRegistry: {id} already registered");
            return;
        }

        Entry entry = new Entry(id, type);
        
        updater = new Updater(type, mesh, data, entry, collisionManager);
    }
}