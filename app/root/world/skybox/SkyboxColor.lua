--[[

    Main implementation to handle 
    general skybox colors and periods 

    ]]
dofile("utils/TimePeriod.lua")

-- Skybox Colors
local Colors = {
    MIDNIGHT = {
        top = "#0a0a2e",
        bottom = "#000011"
    },
    DAWN = {
        top = "#ff7f50",
        bottom = "#4a0080"
    },
    MORNING = {
        top = "#87ceeb",
        bottom = "#f0e68c"
    },
    AFTERNOON = {
        top = "#4169e1",
        bottom = "#87ceeb"
    },
    DUSK = {
        top = "#ff6347",
        bottom = "#2f004f"
    },
    NIGHT = {
        top = "#191970",
        bottom = "#00001a"
    }
}

-- Get Current Color
function getCurrentColor(hour)
    local currentPeriod = getCurrent(hour)
    local periodName = currentPeriod.name
    
    local colorData = Colors[periodName] or Colors.MIDNIGHT
    colorData.name = periodName
    return colorData
end

-- Get top Color
function getTopColor(periodName)
    if Colors[periodName] then
        return Colors[periodName].top
    end
    return Colors.MIDNIGHT.top
end

-- Get Bottom Color
function getBottomColor(periodName)
    if Colors[periodName] then
        return Colors[periodName].bottom
    end
    return Colors.MIDNIGHT.bottom
end