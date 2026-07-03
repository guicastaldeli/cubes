--[[
    
    General Platform Themes

    ]]
--dofile("world/platform/themes/CalculateMovement.lua")
--dofile("utils/ResolveFormat.lua")

local ResolveFormat = dofile("utils/ResolveFormat.lua")
local ThemeParser = dofile("app/root/world/platform/themes/ThemeParser.lua")

local Theme = {}
Theme.__index = Theme
Themes = load()

--[[
    Theme
    ]]
function Theme:new(params)
    params = params or {}

    local self = setmetatable({}, Theme)
    self.id = params.id
    self.name = params.name
    self.movement = params.movement or ""
    self.audio = params.audio or ""
    self.top = params.top or nil
    self.particles = params.particles or ""
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
    
    return self
end

--[[
    Load
    ]]
function load()
    local themes = {}

    local parsedThemes = ThemeParser.loadAllThemes()
    if not parsedThemes or #parsedThemes == 0 then
        print("No themes found in .th files")
        return themes
    end

    for _, parsedTheme in ipairs(parsedThemes) do
        local themeData = ThemeParser.toThemeObject(parsedTheme)
        local theme = Theme:new(themeData)
        table.insert(themes, theme)
    end

    print(string.format("Loaded %d themes from .th files", #themes))
    return themes
end