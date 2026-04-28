--[[
    
    Time Period Definitions
    Handles general time period ranges

    ]]
-- Period Helper
local function p(s, e, n)
    return { s = s, e = e, n = n }
end

local Period = {
    MIDNIGHT = p(0, 4, 1),
    DAWN = p(4, 6, 3),
    MORNING = p(6, 12, 5),
    AFTERNOON = p(12, 17, 6),
    DUSK = p(17, 19, 4),
    NIGHT = p(19, 24, 2)
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
    