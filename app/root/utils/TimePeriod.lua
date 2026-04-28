--[[
    
    Time Period Definitions
    Handles general time period ranges

    ]]

-- Period Helper
local function p(s, e)
    return { s = s, e = e }
end

local Period = {
    MIDNIGHT = p(0, 4),
    DAWN = p(4, 6),
    MORNING = p(6, 12),
    AFTERNOON = p(12, 17),
    DUSK = p(17, 19),
    NIGHT = p(19, 24)
}

-- Is Active
function isActive(period, hour)
    if period.s < period.e then
        local val = hour >= period.s and hour < period.e
        return val
    end
    return false
end

-- Get Current
function getCurrent(hour)
    for name, period in pairs(Period) do
        if isActive(period, hour) then
            period.name = name
            return period
        end
    end

    return Period.MIDNIGHT
end
    