--[[

    Loader Finder to help
    load in the correct MeshLoader

    --]]
global map = {
    d = "data",
    m = "model"
}

-- Find
global function find(entity)
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
    table.insert(resolved, find(entity))
end