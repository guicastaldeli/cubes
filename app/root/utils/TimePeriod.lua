--[[
    
    Time Period Definitions
    Handles general time period ranges

    ]]
local function p(s, e, num)
    return { s = s, e = e, n = num and num.n or nil }
end

local function ASSETS(supported, period)
    period.support = supported or false
    return period
end

local Period = {
    MIDNIGHT = ASSETS(true, p(0, 4, { n = 1 })),
    DAWN = ASSETS(true, p(4, 6, { n = 4 })), 
    MORNING = ASSETS(false, p(6, 12, { n= 5 })), 
    AFTERNOON = ASSETS(false, p(12, 17, { n = 6 })), 
    DUSK = ASSETS(true, p(17, 19, { n = 3 })),
    NIGHT = ASSETS(true, p(19, 24, { n = 2 }))
}

-- Is Active
local function isActive(period, hour)
    if period.s < period.e then
        local val = hour >= period.s and hour < period.e
        return val
    end
    return false
end

-- Get Current
local function getCurrent(hour)
    for name, period in pairs(Period) do
        if isActive(period, hour) then
            period.name = name
            return period
        end
    end

    return Period.MIDNIGHT
end

return {
    getCurrent = getCurrent,
    isActive = isActive,
    Period = Period
}