--[[
    
    General Skill Data for Player.

    ]]
local ResolveFormat = dofile("utils/ResolveFormat.lua")
local Parser = dofile("utils/Parser.lua")

Parser.registerType("skill", "player/skill/", ".sk")

local Skill = {}
Skill.__index = Skill

-- To Object
local function toObject(parsedSkill)
    if not parsedSkill or not parsedSkill.data then return nil end

    local data = parsedSkill.data

    return {
        id = data.id or 0,
        name = parsedSkill.name or "Unknown",
        movement = data.movement or "",
        audio = data.audio or "",
        particles = data.particles or ""
    }
end

--[[
    Skill
    ]]
function Skill:new(params)
    params = params or {}

    local self = setmetatable({}, Skill)
    self.id = params.id
    self.name = params.name
    self.movement = params.movement or ""
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