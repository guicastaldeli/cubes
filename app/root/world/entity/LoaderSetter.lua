--[[

    Loader Setter to help
    load in the correct MeshLoader

    --]]
global map = {
    d = "data",
    m = "model"
}

-- Set
global function set(entity)
    local type = map[entity.l]
    if not type then
        error("Unknown loader type '" .. entity.l .. "' for entity '" .. entity.id .. "'")
    end

    return {
        id = entity.id,
        loader = type
    }
end

-- Resolved
global resolved = {}
for _, entity in ipairs(entityes) do
    table.insert(resolved, set(entity))
end