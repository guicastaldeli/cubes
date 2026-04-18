namespace App.Root.Collider;
using App.Root.Collider.Types;
using App.Root.Player;
using OpenTK.Mathematics;

class CollisionManager {
    public enum CollisionType {
        STATIC_OBJECT,
        BOUNDARY_OBJECT
    }

    private List<Collider> staticColliders = new();

    // Add Static Collider
    public void addStaticCollider(Collider coll) {
        staticColliders.Add(coll);
    }

    // Remove Collider
    public void removeCollider(Collider coll) {
        staticColliders.Remove(coll);
    }

    public void removeCollider(string id) {
        staticColliders.RemoveAll(c => c.getId() == id);
    }

    // Get Colliders
    public List<Collider> getColliders() {
        return staticColliders;
    }

    ///
    /// Check Collision
    /// 
    public List<CollisionResult> checkCollision(RigidBody rigidBody) {
        BBox bodyBounds = rigidBody.getBBox();
        List<CollisionResult> results = new();

        // Boundary Object
        foreach(var collider in staticColliders) {
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
        foreach(var collider in staticColliders) {
            if(collider is StaticObject staticObj) {
                CollisionResult res = staticObj.checkCollision(bodyBounds);
                if(res.collided) {
                    res.otherCollider = staticObj;
                    results.Add(res);
                }
            }
        } 
        // Sphere Object
        foreach(var collider in staticColliders) {
            if(collider is SphereObject sphereObj) {
                CollisionResult res = sphereObj.checkCollision(bodyBounds);
                if(res.collided) {
                    res.otherCollider = sphereObj;
                    results.Add(res);
                }
            }
        }
        // Triangle Object
        foreach(var collider in staticColliders) {
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

    ///
    /// Resolve Collision
    /// 
    public void resolveCollision(RigidBody rigidBody, List<CollisionResult> collisions) {
        if(collisions.Count == 0) {
            rigidBody.setOnSurface(false);
            return;
        }
        collisions.Sort((a, b) => b.depth.CompareTo(a.depth));

        bool groundFound = false;

        foreach(var collision in collisions) {
            Vector3 position = rigidBody.getPosition();
            BBox bBox = rigidBody.getBBox();

            // Boundary Object
            if(collision.otherCollider is BoundaryObject boundaryObj) {
                Vector3 newPos = new Vector3(position);
                float dist = boundaryObj.getBoundaryDistance();
                if(MathF.Abs(position.X) > dist) newPos.X = MathF.CopySign(dist, position.X);
                if(MathF.Abs(position.Z) > dist) newPos.Z = MathF.CopySign(dist, position.Z);
                rigidBody.setPosition(newPos);

                Vector3 vel = rigidBody.getVelocity();
                if(collision.normal.X != 0) vel.X = 0;
                if(collision.normal.Z != 0) vel.Z = 0;
                rigidBody.setVelocity(vel);
                continue;
            }
            // Static Object
            if(collision.otherCollider is StaticObject ||
                collision.otherCollider is SphereObject) {
                if(collision.depth > 0.0001f) {
                    rigidBody.setPosition(
                        position + 
                        collision.normal *
                        collision.depth
                    );
                }

                Vector3 vel = rigidBody.getVelocity();
                float dot = Vector3.Dot(vel, collision.normal);
                if(dot < 0) {
                    vel -= collision.normal * dot;
                    rigidBody.setVelocity(vel);
                }

                if(collision.normal.Y > 0.5f) {
                    groundFound = true;
                    Vector3 v = rigidBody.getVelocity();
                    v.Y = 0;
                    rigidBody.setVelocity(v);
                }
            }
            if(collision.otherCollider is TriangleObject) {
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
                        groundFound = true;
                        Vector3 v = rigidBody.getVelocity();
                        v.Y = 0;
                        rigidBody.setVelocity(v);
                    }
                }
            }
        }

        rigidBody.setOnSurface(groundFound);
    } 

    ///
    /// Update 
    /// 
    public void update() {
        foreach(var collider in staticColliders) {
            RigidBody? rigidBody = collider.getRigidBody();
            if(rigidBody != null) {
                rigidBody.update();
                var collision = checkCollision(rigidBody);
                resolveCollision(rigidBody, collision);
            }
        }
    }
}