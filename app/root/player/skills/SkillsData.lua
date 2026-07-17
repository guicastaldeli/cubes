--[[
    
    General Skill Data for Player.

    ]]
local Parser = dofile("utils/Parser.lua")
local CalculateMovement = dofile("utils/CalculateMovement.lua")

Parser.registerType("skill", "player/skills/", ".skill")

local Skill = {}
Skill.__index = Skill

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
local function toObject(parsedSkill)
    if not parsedSkill or not parsedSkill.data then return nil end

    local data = parsedSkill.data

    return {
        id = data.id or 0,
        name = data.name or "",
        movement = toMovementData(data),
        audio = data.audio or "",
        particles = data.particles or ""
    }
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

--[[
    Skill
    ]]
function Skill:new(params)
    params = params or {}

    local self = setmetatable({}, Skill)
    self.id = params.id
    self.name = params.name
    setMovement(self, params)
    self.audio = params.audio or ""
    self.particles = params.particles or ""

    return self
end

--[[
    Load
    ]]
function load()
    local skills = {}

    local parsedSkills = Parser.loadAll("skill")
    if not parsedSkills or #parsedSkills == 0 then
        print("No skills found")
        return skills
    end

    for _, parsedSkill in ipairs(parsedSkills) do
        local skillData = toObject(parsedSkill)
        if skillData then
            local skill = Skill:new(skillData)
            table.insert(skills, skill)
        end
    end

    print(string.format("Loaded %d skills from .sk files", #skills))
    return skills
end

Skills = load()

return {
    Skills = Skills,
    Skill = Skill,
    load = load
}