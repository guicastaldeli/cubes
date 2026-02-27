namespace App.Root.Collider;
using App.Root.Player;

interface Collider {
    BBox getBBox();
    RigidBody? getRigidBody();
    void onCollision(CollisionResult coll);
}