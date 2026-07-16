--[[
    
    General Platform Themes

    ]]
local ResolveFormat = dofile("utils/ResolveFormat.lua")
local Parser = dofile("utils/Parser.lua")
local CalculateMovement = dofile("world/platform/themes/CalculateMovement.lua")

Parser.registerType("theme", "world/theme/", ".th")
Parser.setVariable("top", 0.0)

local Theme = {}
Theme.__index = Theme

-- To Movement Data
local function toMovementData(data)
    local movementData = nil

    if data.movement then
        movementData = CalculateMovement.parse(data.movement)
    else
        movementData = {}
    end

    return movementData
end

-- To Object
local function toObject(parsedTheme)
    if not parsedTheme or not parsedTheme.data then return nil end

    local data = parsedTheme.data

    return {
        id = data.id or 0,
        name = data.name or "Unknown",
        movement = toMovementData(data),
        audio = data.audio or "",
        top = data.top or nil,
        particles = data.particles or "",
        texture = data.texture or ""
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
        local movement = CalculateMovement.parse(params.movement)
        self.movement =  movement
    else 
        self.movement = {}
        print("No movement data in theme")
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
    setMovement(self, params)
    self.audio = params.audio or ""
    self.top = params.top or nil
    self.particles = params.particles or ""
    setTexture(self, params)
    
    return self
end

-- Get Movement
function Theme:getMovement()
    local val = self.movement or {}
    return val
end

-- Get Movement String
function Theme:getMovementString()
    local val = CalculateMovement.toString(self.movement)
    return val
end

-- Get Movement Value
function Theme:getMovementValue(key)
    if self.movement and self.movement ~= nil then
        return self.movement[key]
    end

    return nil
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