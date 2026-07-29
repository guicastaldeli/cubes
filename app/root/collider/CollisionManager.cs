namespace App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Player;
using OpenTK.Mathematics;

[ManagedState]
class CollisionManager {
    public enum CollisionType {
        STATIC_OBJECT,
        BOUNDARY_OBJECT
    }

    private List<Collider> staticColliders = new();
    private List<Collider> interactionColliders = new();
    private HashSet<string> colliderIdSet = new();
    
    public List<string> pendingRemovals = new();
    private List<string> removedIds = new();

    public CollisionManager() {
        StateManager.Register(this);
    }

    // Add Static Collider
    public void addStaticCollider(Collider coll) {
        staticColliders.Add(coll);
        colliderIdSet.Add(coll.getId());
    }

    // Add Interaction Collider
    public void addInteractionCollider(Collider coll) {
        interactionColliders.Add(coll);
        colliderIdSet.Add(coll.getId());
    }

    // Collider Exists
    public bool colliderExists(string id) {
        if(pendingRemovals.Contains(id)) return false;
        if(removedIds.Contains(id)) return false;
        return colliderIdSet.Contains(id);
    }

    // Clear Removed
    public void clearRemoved() {
        removedIds.Clear();
    }

    // Get Pending Removals Count
    public int getPendingRemovalsCount() {
        return pendingRemovals.Count;
    }

    // Set Collider Visibility
    public void setColliderVisibility(string id, bool visible) {
        var collider = getCollider(id);
        if(collider != null) {
            collider.setVisible(visible);
            Console.WriteLine($"[CollisionManager] Set collider {id} visibility to {visible}");
        }
    }

    // Set Collider Jump Gravity
    public void setColliderJumpGravity(string id, bool enabled) {
        var collider = getCollider(id);
        if(collider != null) {
            collider.setJumpGravityEnabled(enabled);
            Console.WriteLine($"[CollisionManager] Set collider {id} jump gravity to {enabled}");
        }
    }

    /**
     * 
     * Get Collider
     *
     */
    // Get Collider
    public Collider? getCollider(string id) {
        Collider? val = staticColliders.FirstOrDefault(c => c.getId() == id);
        return val;
    }

    // Get Colliders
    public List<Collider> getColliders() {
        List<Collider> val = staticColliders.ToList();
        return val;
    }

    // Get Colliders by Prefix
    public List<string> getCollidersByPrefix(string prefix) {
        List<string> val = staticColliders
            .Where(c => c.getId().StartsWith(prefix))
            .Select(c => c.getId())
            .ToList();
        
        return val;
    }

    /**
     * 
     * Remove
     *
     */
    // Remove Collider
    public void removeCollider(Collider coll) {
        pendingRemovals.Add(coll.getId());
    }

    public void removeCollider(string id) {
        pendingRemovals.Add(id);
    }

    // Process Removals
    public void processRemovals() {
        if(pendingRemovals.Count == 0) return;

        var snapshot = pendingRemovals.ToList();
        pendingRemovals.Clear();

        foreach(var id in snapshot) {
            staticColliders.RemoveAll(c => c.getId() == id);
            interactionColliders.RemoveAll(c => c.getId() == id);
            colliderIdSet.Remove(id);
            removedIds.Add(id);
        }
        
        pendingRemovals.Clear();
    }

    /**
     * 
     * Check Collision
     *
     */
    public List<CollisionResult> checkCollision(RigidBody rigidBody) {
        BBox bodyBounds = rigidBody.getBBox();
        List<CollisionResult> results = new();

        var colliders = staticColliders.ToList();

        // Boundary Object
        foreach(var collider in colliders) {
            if(collider is BoundaryObject boundary) {
                Vector3 position = rigidBody.getPosition();
                if(boundary.isOutsideBoundary(position)) {
                    results.Add(new CollisionResult(
                        true,
                        boundary.getBoundaryNormal(position),
                        boundary.getBoundaryFar(position),
                        boundary,
                        CollisionType.STATIC_OBJECT
                    ));
                }
            }
        }
        // Static Object
        foreach(var collider in colliders) {
            if(collider is StaticObject staticObj) {
                BBox? self = staticObj.getBBox();
                if(self == null) continue;
                
                CollisionResult res = staticObj.checkCollision(bodyBounds);
                if(res.collided) {
                    res.otherCollider = staticObj;
                    results.Add(res);
                }
            }
        } 
        // Sphere Object
        foreach(var collider in colliders) {
            if(collider is SphereObject sphereObj) {
                CollisionResult res = sphereObj.checkCollision(bodyBounds);
                if(res.collided) {
                    res.otherCollider = sphereObj;
                    results.Add(res);
                }
            }
        }
        // Triangle Object
        foreach(var collider in colliders) {
            if(collider is TriangleObject triObj) {
                CollisionResult res = triObj.checkCollision(bodyBounds);
                if(res.collided) {
                    res.otherCollider = triObj;
                    results.Add(res);
                }
            }
        }

        return results;
    }

    /**
     * 
     * Resolve Collision
     *
     */
    public void resolveCollision(RigidBody rigidBody, List<CollisionResult> collisions) {
        if(collisions.Count == 0) {
            rigidBody.setOnSurface(false);
            return;
        }
        collisions.Sort((a, b) => b.depth.CompareTo(a.depth));

        bool surfaceFound = false;
        bool jumpGravityActive = false;

        foreach(var collision in collisions) {
            Vector3 position = rigidBody.getPosition();
            BBox bbox = rigidBody.getBBox();

            var collider = collision.otherCollider;
            if(collider != null && collider.isJumpGravityEnabled()) jumpGravityActive = true;

            // Boundary Object
            if(collider is BoundaryObject boundaryObj) {
                Vector3 newPos = new Vector3(position);
                float dist = boundaryObj.getBoundaryDistance();
                Vector3 center = boundaryObj.getCenter();

                float localX = position.X - center.X;
                float localZ = position.Z - center.Z;

                if(MathF.Abs(localX) > dist) newPos.X = center.X + MathF.CopySign(dist, localX);
                if(MathF.Abs(localZ) > dist) newPos.Z = center.Z + MathF.CopySign(dist, localZ);
            
                rigidBody.setPosition(newPos);

                Vector3 vel = rigidBody.getVelocity();
                if(collision.normal.X != 0) vel.X = 0;
                if(collision.normal.Z != 0) vel.Z = 0;
                
                rigidBody.setVelocity(vel);

                continue;
            }
            // Static Object
            if(collider is StaticObject || collider is SphereObject) {
                if(collision.depth > 0.0001f) {
                    rigidBody.setPosition(
                        position + 
                        collision.normal *
                        collision.depth
                    );
                }

                if(collision.normal.Y > 0.5f) {
                    surfaceFound = true;

                    Vector3 pos = rigidBody.getPosition();
                    BBox colliderBox = collider.getBBox();
                    BBox playerBox = rigidBody.getBBox();

                    float halfHeight = (playerBox.maxY - playerBox.minY) / 2.0f;
                    pos.Y = colliderBox.maxY + halfHeight;
                    rigidBody.setPosition(pos);
                    
                    Vector3 v = rigidBody.getVelocity();
                    v.Y = 0;
                    rigidBody.setVelocity(v);
                }
            }
            if(collider is TriangleObject) {
                float f = 0.0001f;
                if(collision.depth > f) {
                    Vector3 normal = collision.normal;

                    float ax = MathF.Abs(normal.X);
                    float ay = MathF.Abs(normal.Y);
                    float az = MathF.Abs(normal.Z);
                    if(ay >= ax && ay >= az) {
                        normal = new Vector3(0, MathF.Sign(normal.Y), 0);
                    } else if(ax >= ay && ax >= az) {
                        normal = new Vector3(MathF.Sign(normal.X), 0, 0);
                    } else {
                        normal = new Vector3(0, 0, MathF.Sign(normal.Z));
                    }

                    rigidBody.setPosition(
                        position + normal *
                        collision.depth
                    );

                    Vector3 vel = rigidBody.getVelocity();
                    float dot = Vector3.Dot(vel, normal);
                    if(dot < 0) {
                        vel -= normal * dot;
                        rigidBody.setVelocity(vel);
                    }

                    if(normal.Y > 0.5f) {
                        surfaceFound = true;
                        Vector3 v = rigidBody.getVelocity();
                        v.Y = 0;
                        rigidBody.setVelocity(v);
                    }
                }
            }
        }

        rigidBody.setOnSurface(surfaceFound);

        if(jumpGravityActive) {
            rigidBody.setJumpGravityEnabled(true);
        } else {
            if(!rigidBody.getJumping()) rigidBody.setJumpGravityEnabled(false);
        }
    } 

    /**
     * 
     * Update
     *
     */
    public void update() {
        processRemovals();

        var colliders = staticColliders.ToList();
        
        foreach(var collider in colliders) {
            RigidBody? rigidBody = collider.getRigidBody();
            if(rigidBody != null) {
                rigidBody.update();
                var collision = checkCollision(rigidBody);
                resolveCollision(rigidBody, collision);
            }
        }

        processRemovals();
    }
}