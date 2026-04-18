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

    // Resolve Collisions
    public void resolveCollisins(Entry entry, List<CollisionResult> collisions) {
        if(entry.physicsBody == null) return;

        collisions.Sort((a, b) => b.depth.CompareTo(a.depth));

        bool found = false;

        foreach(var collision in collisions) {
            if(mesh == null) return;

            float f = 0.0001f;
            if(collision.depth < f) continue;

            Vector3 position = entry.physicsBody.getPosition();
            position += collision.normal * collision.depth;
            entry.physicsBody.setPostion(position);
            mesh.setPosition(entry.id, position);

            Vector3 vel = entry.physicsBody.getVelocity();
            float dot = Vector3.Dot(vel, collision.normal);
            if(dot < 0) {
                vel -= collision.normal * dot;
                entry.physicsBody.setVelocity(vel);
            }

            if(collision.normal.Y > 0.5f) {
                found = true;

                Vector3 v = entry.physicsBody.getVelocity();
                v.Y = 0;
                entry.physicsBody.setVelocity(v);
            }
        }

        entry.physicsBody.set(found);
    }

    /**
    
        Receivers
    
        */
    // Get All Receivers
    public List<Entry> getReceivers() {
        return entries.Values
            .Where(e => e.type == Type.RECEIVER)
            .ToList();
    }

    // Check Collision with Receivers
    public List<CollisionResult> checkCollisionWithReceivers(BBox dynamicBBox) {
        List<CollisionResult> results = new();
        foreach(var receiver in getReceivers()) {
            if(receiver.collider != null) {
                var result = receiver.collider.checkCollision(dynamicBBox);
                if(result.collided) results.Add(result);
            }
        }
        return results;
    }

    /**
    
        Dynamic
    
        */
    // Get All Dynamic Objects
    public List<Entry> getDynamicObjects() {
        return entries.Values
            .Where(e => e.type == Type.DYNAMIC)
            .ToList();
    }

    // Check Collision with Dynamic Objects
    public List<CollisionResult> checkCollisionWithDynamic(string excludeId, BBox dynamicBBox) {
        List<CollisionResult> results = new();
        foreach(var other in getDynamicObjects()) {
            if(other.id == excludeId) continue;
            if(other.collider != null) {
                var result = other.collider.checkCollision(dynamicBBox);
                if(result.collided) results.Add(result);
            }
        }
        return results;
    }

    /**
    
        Register
    
        */
    public void register(string id, Type type) {
        if(entries.ContainsKey(id)) {
            Console.WriteLine($"PhysicsRegistry: {id} already registered");
            return;
        }

        Entry entry = new Entry(id, type);
        
        updater = new Updater(type, mesh!, data!, entry, collisionManager!);
        updater.update(id);

        entries[id] = entry;
        Console.WriteLine($"PhysicsRegistry: Registered {id} as {type}");
    }

    /**
    
        Unregister
    
        */
    public void unregister(string id) {
        if(entries.TryGetValue(id, out var entry)) {
            if(collisionManager != null && entry.collider != null) {
                collisionManager.removeCollider(entry.collider);
            }
            entries.Remove(id);
            Console.WriteLine($"PhysicsRegistry: Unregistered {id}");
        }
    }

    ///
    /// Init
    /// 
    public void init(Mesh.Mesh mesh, MeshData data, CollisionManager collisionManager) {
        this.mesh = mesh;
        this.data = data;
        this.collisionManager = collisionManager;
    }

    ///
    /// Update
    /// 
    public void update() {
        float deltaTime = Tick.getDeltaTimeI();
        
        foreach(var entry in getDynamicObjects()) {
            if(mesh == null) return;
            if(entry.physicsBody == null) continue;

            entry.physicsBody.applyGravity(deltaTime);
            entry.physicsBody.update(deltaTime);

            Vector3 newPos = entry.physicsBody.getPosition();
            mesh.setPosition(entry.id, newPos);

            BBox bBox = mesh.getBBox(entry.id);

            var receiverColls = checkCollisionWithReceivers(bBox);
            var dynamicColls = checkCollisionWithDynamic(entry.id, bBox);

            var allColls = new List<CollisionResult>();
            allColls.AddRange(receiverColls);
            allColls.AddRange(dynamicColls);
            if(allColls.Count > 0) {
                receiverColls(entry, allColls);
            } else {
                entry.physicsBody.set(false);
            }
        }
    }

    ///
    /// Cleanup
    /// 
    public void cleanup() {
        foreach(var entry in entries.Values) {
            if(collisionManager != null && entry.collider != null) {
                collisionManager.removeCollider(entry.collider);
            }
        }
        entries.Clear();
    }
}