--[[

    Weather Data to handle
    general weather props...

    --]]
local minHeight = 0.0

-- Weather Helper
local function meta(i, f, v)
    return { i = i, f = f, v = v }
end

-- Set Min Height
function setMinHeight(val)
    minHeight = val
end

local Data = {
    -- NORMAL
    NORMAL = function()
        local data = meta(0, 0.5, 0.0)
        return data
    end,
    -- RAIN
    RAIN = function()
        local data = meta(1, 0.3, 1.0)
        data.color = { 0.141, 0.106, 0.871 }
        data.amount = 40
        data.size = 0.05
        data.speed = 0.65
        data.lifetime = 1.5
        data.vel = { 0.0, minHeight, 0.0 }
        data.temp = { r = 0.45, g = 0.52, b = 0.65, strength = 0.35 }
        return data
    end,
    -- SNOW
    SNOW = function()
        local data = meta(2, 0.2, 2.0)
        data.color = { 0.9, 0.95, 1.0 }
        data.amount = 20
        data.size = 0.08
        data.speed = 0.5
        data.lifetime = 1.5
        data.vel = { 5.0, minHeight, 5.0 }
        data.temp = { r = 0.92, g = 0.95, b = 1.00, strength = 0.4 }
        return data
    end,
    -- DEBUG TESTING
    DEBUG = function()
        local data = meta(9, 0.0, 2.0)
        data.color = { 0.0, 0.0, 0.0 }
        data.amount = 10
        data.size = 0.1
        data.speed = 0.8
        data.lifetime = 1.5
        data.vel = { 0.0, minHeight, 0.0 }
        data.temp = { r = 0.0, g = 0.0, b = 0.0, strength = 0.0 }
        return data
    end,
}

function getTypes()
    local res = {}
    
    for name, f in pairs(Data) do
        local data = f()
        res[name] = meta(data.i, data.f, data.v)
    end

    return res
end

function getParticle(name)
    local f = Data[name]
    return f and f()
end

function getTemp(name)
    local f = Data[name]
    if not f then return nil end
    return f().temp
end