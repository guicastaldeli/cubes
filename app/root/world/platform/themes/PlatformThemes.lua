--[[
    
    General Platform Themes

    ]]
local ResolveFormat = dofile("utils/ResolveFormat.lua")
local Parser = dofile("utils/Parser.lua")
local CalculateMovement = dofile("utils/CalculateMovement.lua")

Parser.registerType("theme", "world/theme/", ".th")
Parser.setVariable("top", 0.0)

local Theme = {}
Theme.__index = Theme

-- To Movement Data
local function toMovementData(data)
    local movementData = nil

    if data.movement then
        movementData = CalculateMovement.parse(data.movement)
        return CalculateMovement.toString(movementData)
    else
        return ""
    end
end

-- To Object
local function toObject(parsedTheme)
    if not parsedTheme or not parsedTheme.data then return nil end

    local data = parsedTheme.data

    return {
        id = data.id or 0,
        name = data.name or "",
        price = data.price or "",
        movement = toMovementData(data),
        audio = data.audio or "",
        top = data.top or nil,
        particles = data.particles or "",
        texture = data.texture or "",
        colliderVisible = data.colliderVisible,
        gravityRegular = data.gravityRegular
    }
end

-- Set Texture
local function setTexture(self, params)
    if params.texture then
        local texturePath, format, fullPath = ResolveFormat.resolveTexture(params.texture)
        if texturePath then
            self.texture = texturePath
            self.textureFormat = format
            self.textureFullPath = fullPath
            print(string.format("Loaded texture '%s' as %s", params.texture, format or "unknown"))
        else 
            print(string.format("Texture '%s' not found", params.texture))
            self.texture = ""
            self.textureFormat = nil
            self.textureFullPath = nil
        end
    else
        self.texture = ""
        self.textureFormat = nil
        self.textureFullPath = nil
    end
end

-- Set Movement
local function setMovement(self, params)
    if params.movement then
        self.movement = params.movement
    else 
        self.movement = {}
        print("No movement data in theme")
    end
end

-- Set Collider
local function setCollider(self, params)
    if params.colliderVisible ~= nil then
        self.colliderVisible = params.colliderVisible
        print(string.format("[Theme] Collider visible: %s", tostring(self.colliderVisible)))
    else
        self.colliderVisible = true
    end
end

-- Set Gravity
local function setGravity(self, params)
    if params.colliderVisible ~= nil then
        self.gravityRegular = params.gravityRegular
        print(string.format("[Theme] Gravity regular: %s", tostring(self.gravityRegular)))
    else
        self.gravityRegular = true
    end
end

--[[
    Theme
    ]]
function Theme:new(params)
    params = params or {}

    local self = setmetatable({}, Theme)
    self.id = params.id
    self.name = params.name
    self.price = params.price
    setMovement(self, params)
    self.audio = params.audio or ""
    self.top = params.top or nil
    self.particles = params.particles or ""
    setTexture(self, params)
    setCollider(self, params)
    setGravity(self, params)
    
    return self
end

--[[
    Load
    ]]
function load()
    local themes = {}

    local parsedThemes = Parser.loadAll("theme")
    if not parsedThemes or #parsedThemes == 0 then
        print("No themes found")
        return themes
    end

    for _, parsedTheme in ipairs(parsedThemes) do
        local themeData = toObject(parsedTheme)
        if themeData then
            local theme = Theme:new(themeData)
            table.insert(themes, theme)
        end
    end

    print(string.format("Loaded %d themes from .th files", #themes))
    return themes
end

Themes = load()

return {
    Themes = Themes,
    Theme = Theme,
    load = load
}