/**

    Weather Data to handle
    general weather props...

    */
-- Weather Helper
local function w(id, f, s) {
    return { i = id and id.i or nil, f = f, s = s }
}

local Types {
    NORMAL = w({ id = 0 }, 0.5, 0.0),
    RAIN = w({ id = 1}, 0.3, 1.0),
    SNOW = w({ id = 2 }, 0.2, 2.0)
}

local Particle = {
    RAIN = {
        color = { 0.5, 0.6, 0.8 },
        amount = 200,
        size = 0.05,
        speed = 15.0,
        lifetime = 1.5,
        vel = { 0.1, -1.0, 0.05 }
    },
    SNOW = {
        color = { 0.9, 0.95, 1.0 },
        amount = 80,
        size = 0.08,
        speed = 15,
        lifetime = 5.0,
        vel = { 0.2, -0.3, 0.2 }
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