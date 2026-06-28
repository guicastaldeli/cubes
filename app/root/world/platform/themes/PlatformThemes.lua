--[[
    
    General Platform Themes

    ]]
dofile("CalculateMovement.lua")
dofile("utils/ResolveFormat.lua")

local texturePath = "texture/world/platform_themes/"

-- Theme Payload
local Theme = {}
Theme.__index = Theme

function Theme:new(params)
    params = params or {}

    local self = setmetatable({}, Theme)
    self.id = params.id
    self.name = params.name
    self.movement = params.movement or ""
    self.audio = params.audio or ""
    self.top = params.top or ""
    self.particles = params.particles or ""
    self.texture = texturePath .. parms.texture

    if params.custom then
        for key, value in pairs(params.custom) do
            self[key] = value
        end
    end

    return self
end

-- Themes
local Themes = {
    Theme.new({
        id = -1,
        name = "TEST_1",
        --movement = "",
        --audio = "",
        --top = "",
        --particles = "",
        texture = "test1"
    }),
    Theme.new({
        id = -2,
        name = "TEST_2",
        texture = "test2"
    }),
    Theme.new({
        id = -3,
        name = "TEST_3",
        texture = "test3"
    }),
}