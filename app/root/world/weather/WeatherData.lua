--[[

    Weather Data to handle
    general weather props...

    --]]
local minHeight = 0.0

-- Weather Helper
local function w(id, f, v)
    return { i = id and id.i or nil, f = f, v = v }
end

-- Set Min Height
function setMinHeight(val)
    minHeight = val
end

local Types = {
    NORMAL = w({ i = 0 }, 0.5, 0.0),
    RAIN = w({ i = 1}, 0.3, 1.0),
    SNOW = w({ i = 2 }, 0.2, 2.0)
}

local Particle = {
    RAIN = function()
        return {
            color = { 0.141, 0.106, 0.871 },
            amount = 150,
            size = 0.05,
            speed = 0.65,
            lifetime = 1.5,
            vel = { 0.0, minHeight, 0.0 }
        }
    end,
    SNOW = function()
        return {
            color = { 0.9, 0.95, 1.0 },
            amount = 80,
            size = 0.08,
            speed = 0.5,
            lifetime = 1.5,
            vel = { 5.0, minHeight, 5.0 }
        }
    end
}

local Temp = {
    -- Rain
    [1] = { r = 0.45, g = 0.52, b = 0.65, strength = 0.35 },
    -- Snow
    [2] = { r = 0.92, g = 0.95, b = 1.00, strength = 0.4  },
}

function getTypes()
    return Types
end

function getParticle(name)
    local f = Particle[name]
    return f and f()
end

function getTemp(addonId)
    return Temp[addonId]
end