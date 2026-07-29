namespace App.Root.Collider;
using App.Root.Player;

interface Collider {
    BBox getBBox();
    RigidBody? getRigidBody();
    string getId(); 
    void onCollision(CollisionResult coll) {}

    void setVisible(bool visible) {}
    bool isVisible() => true;

    void setJumpGravityEnabled(bool enabled) {}
    bool isJumpGravityEnabled() => false;
}