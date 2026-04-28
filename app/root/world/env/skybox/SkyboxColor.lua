--[[

    Main implementation to handle 
    general skybox colors and periods 

    ]]
dofile("utils/TimePeriod.lua")

-- Skybox Colors
local Colors = {
    MIDNIGHT = {
        upper = "#0a0a2e",
        bottom = "#000011"
    },
    DAWN = {
        upper = "#ff7f50",
        bottom = "#4a0080"
    },
    MORNING = {
        upper = "#87ceeb",
        bottom = "#f0e68c"
    },
    AFTERNOON = {
        upper = "#4169e1",
        bottom = "#87ceeb"
    },
    DUSK = {
        upper = "#ff6347",
        bottom = "#2f004f"
    },
    NIGHT = {
        upper = "#191970",
        bottom = "#00001a"
    }
}

-- Get Skybox Colors
function getSkyboxColors(hour)
    local currentPeriod = getCurrent(hour)
    local periodName = currentPeriod.name

    if colors[periodName] then
        return SkyboxColors[periodName]
    end

    return Colors.MIDNIGHT

-- Get Upper Color
function getUpperColor(periodName)
    if Colors[periodName] then
        return Colors[periodName].upper
    end
    return Colors.MIDNIGHT.upper
end

-- Get Bottom Color
function getBottomColor(periodName)
    if Colors[periodName] then
        return Colors[periodname].bottom
    end
    return Colors.MIDNIGHT.bottom
end