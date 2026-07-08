--[[

    MeshEntity Class to
    handle general MeshEntity Configuration.

    --]]
dofile("world/entity/SetLoader.lua")

Entities = {
    { id = "cube", l = "d" },
    { id = "rectangle", l = "d" },
    { id = "sphere", l = "d" },
    { id = "triangle", l = "d" },
    { id = "dino", l = "m", tex = "mesh/dino.png", collider = "cube" },
}

Entities = resolved(Entities)