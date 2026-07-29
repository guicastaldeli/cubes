--[[

    Loader Setter to help
    load in the correct MeshLoader

    --]]
local map = {
    d = "data",
    m = "model"
}

-- Set
local function set(entity)
    local type = map[entity.l]
    if not type then
        error("Unknown loader type '" .. entity.l .. "' for entity '" .. entity.id .. "'")
    end

    return {
        id = entity.id,
        loader = type,
        tex = entity.tex
    }
end

-- Resolved
local function resolved(entities)
    local res = {}
    
    for _, entity in ipairs(entities) do
        table.insert(res, set(entity))
    end
    
    return res
end

return {
    set = set,
    resolved = resolved
}