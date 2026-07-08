--[[

    MeshEntity Class to
    handle general MeshEntity Configuration...

    --]]
local Parser = dofile("utils/Parser.lua")
Parser.registerType("entity", "world/entity/", ".entity")

dofile("world/entity/SetLoader.lua")

local entityCache = {}

-- To Object
local function toObject(parsedEntity)
    if not parsedEntity or not parsedEntity.data then return nil end

    local data = parsedEntity.data
    return {
        id = data.id or "Unknown",
        l = data.l or "d",
        tex = data.tex or "",
        collider = data.collider or "",
        raw = data
    }
end

--[[
    Load
]]
local function load(name)
    if entityCache[name] then return entityCache[name] end

    local result = Parser.load("entity", name)
    if not result or not result.data then
        print("[Entities] Failed to load entity: " .. name)
        return nil
    end

    local data = toObject(result)
    if data then entityCache[name] = data end

    return entityCache[name]
end

-- Get Entity
local function getEntity(name)
    local key = name:lower()

    local data = entityCache[key]
    if not data then data = load(key) end

    return data
end

-- Get Entity By Id
local function getEntityById(id)
    return getEntity(id)
end

-- Get All Entity
local function getAllEntities()
    local results = {}
    local parsedEntities = Parser.loadAll("entity")

    if not parsedEntities or #parsedEntities == 0 then
        print("[Entities] No entity files found")
        return results
    end

    for _, parsed in ipairs(parsedEntities) do
        if parsed.type == "entity" then
            local name = parsed.name:lower()
            local data = toObject(parsed)
            if data then
                entityCache[name] = data
                table.insert(results, data)
                print("[Entities] Loaded: " .. name)
            end
        end
    end

    print(string.format("[Entities] Loaded %d entities", #results))
    return results
end

-- Get Entities
local function getEntities()
    return getAllEntities()
end

-- Resolved
local function resolved(entities)
    return entities
end

--[[
    Build
]]
local function build()
    local entities = {}
    local all = getAllEntities()

    for _, entity in ipairs(all) do
        local entry = {
            id = entity.id,
            l = entity.l,
            tex = entity.tex or "",
            collider = entity.collider or ""
        }

        table.insert(entities, entry)
    end

    return entities
end

Entities = build()
RawEntities = entityCache

--[[
    Init
]]
local function init()
    print("[Entities] Initializing...")
    local all = getAllEntities()
    print(string.format("[Entities] Initialized with %d entities", #all))
end
init()

return {
    getEntities = getEntities,
    getEntity = getEntity,
    getEntityById = getEntityById,
    getAllEntities = getAllEntities,
    toObject = toObject,
    Entities = Entities,
    resolved = resolved,
    RawEntities = RawEntities
}