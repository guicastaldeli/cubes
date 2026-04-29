--[[

    Weather Data to handle
    general weather props...

    --]]
-- Weather Helper
local function w(id, f, v)
    return { i = id and id.i or nil, f = f, v = v }
end

local Types = {
    NORMAL = w({ i = 0 }, 0.0, 0.0),
    RAIN = w({ i = 1}, 1.0, 1.0),
    SNOW = w({ i = 2 }, 0.0, 2.0)
}

local Particle = {
    RAIN = {
        color = { 0.5, 0.6, 0.8 },
        amount = 200,
        size = 0.05,
        speed = 1.0,
        lifetime = 1.5,
        vel = { 0.0, -10.0, 0.0 }
    },
    SNOW = {
        color = { 0.9, 0.95, 1.0 },
        amount = 80,
        size = 0.08,
        speed = 1.0,
        lifetime = 5.0,
        vel = { 0.2, -10.0, 0.2 }
    }
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
    return Particle[name]
end

function getTemp(addonId)
    return Temp[addonId]
end