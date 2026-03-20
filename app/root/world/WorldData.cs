using App.Root.Packets;

namespace App.Root.World;

class WorldData {
    public List<WorldObject> worldObjs = new();
    
    ///
    /// Get World Data
    /// 
    public PacketWorldData get() {
        var packet = new PacketWorldData();
        if(worldObjs != null) packet.objs = worldObjs;
        return packet;
    }

    ///
    /// Register World Object
    /// 
    public void registerObj(
        string id,
        string meshType,
        float x,
        float y,
        float z
    ) {
        worldObjs.Add(new WorldObject{
            id = id,
            meshType = meshType,
            x = x, y = y, z = z
        });
    }
}