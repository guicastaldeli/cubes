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
    public CollisionResult checkCollision(RigidBody rigidBody) {
        BBox bodyBounds = rigidBody.getBBox();

        // Boundary Object
        foreach(var collider in staticColliders) {
            if(collider is BoundaryObject boundary) {
                Vector3 position = rigidBody.getPosition();
                if(boundary.isOutsideBoundary(position)) {
                    return new CollisionResult(
                        true,
                        boundary.getBoundaryNormal(position),
                        boundary.getBoundaryFar(position),
                        boundary,
                        CollisionType.STATIC_OBJECT
                    );
                }
            }
        }
        // Static Object
        foreach(var collider in staticColliders) {
            if(collider is StaticObject staticObj) {
                CollisionResult result = staticObj.checkCollision(bodyBounds);
                if(result.collided) {
                    result.otherCollider = staticObj;
                    return result;
                }
            }
        } 

        return new CollisionResult();
    }

    ///
    /// Resolve Collision
    /// 
    public void resolveCollision(RigidBody rigidBody, CollisionResult collision) {
        if(!collision.collided) {
            rigidBody.setOnGround(false);
            return;
        }

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
            return;
        }
        // Static Object
        if(collision.otherCollider is StaticObject staticObj) {
            StaticObject.resolveCollision(position, bBox, rigidBody, collision);
            return;
        }
    } 

    ///
    /// Update 
    /// 
    public void update() {
        foreach(var collider in staticColliders) {
            RigidBody? rigidBody = collider.getRigidBody();
            if(rigidBody != null) {
                rigidBody.update();
                CollisionResult collision = checkCollision(rigidBody);
                resolveCollision(rigidBody, collision);
            }
        }
    }
}