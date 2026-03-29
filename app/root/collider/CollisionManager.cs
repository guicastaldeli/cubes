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

    public void addStaticCollider(Collider coll) {
        staticColliders.Add(coll);
    }

    public void removeCollider(Collider coll) {
        staticColliders.Remove(coll);
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

        return results;
    }

    ///
    /// Resolve Collision
    /// 
    public void resolveCollision(RigidBody rigidBody, List<CollisionResult> collisions) {
        if(collisions.Count == 0) {
            rigidBody.setOnGround(false);
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
        }

        rigidBody.setOnGround(groundFound);
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