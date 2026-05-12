--[[

    Entity Class to
    handle general Entity Configuration.

    --]]
dofile("world/entity/SetLoader.lua")

Entities = {
    { id = "cube", l = "d" },
    { id = "rectangle", l = "d" },
    { id = "sphere", l = "d" },
    { id = "triangle", l = "d" },
    { id = "dino", l = "m", tex = "mesh/dino.png" },
}

Entities = resolved(Entities)
