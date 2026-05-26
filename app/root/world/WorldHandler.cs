/**

    Main world interface
    handler to general world
    objets, etc...

    */
namespace App.Root.World;
using App.Root.Utils;

[ClassRegistryIgnore]
abstract class WorldHandler {
    public virtual void render() {}
    public virtual void update() {}
}