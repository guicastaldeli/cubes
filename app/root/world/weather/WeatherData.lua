--[[

    Weather Data to handle
    general weather props...

    --]]
local Parser = dofile("utils/Parser.lua")
Parser.registerType("weather", "world/weather/", ".weather")
Parser.setVariable("minHeight", 0.0) 

local weatherCache = {}
local minHeight = 0.0

local cachedEntries = nil
local cachedTypes = nil

--[[
    Meta
]]
-- Meta
local function meta(i, f, v)
    return { i = i, f = f, v = v }
end

-- Parse Meta String
local function parseMetaString(str)
    if not str or str == "" then return nil end

    str = str:gsub("^%s*", "")
    str = str:gsub("%s*$", "")
    str = str:gsub("^~%[%s*", "")
    str = str:gsub("%s*%]$", "")
    str = str:gsub("^%s*", "")

    local p1 = "meta%(%s*(%d+)%s*,%s*([%d%.]+)%s*,%s*([%d%.]+)%s*%)"
    local i, f, v = str:match(p1)
    if i and f and v then
        return {
            i = tonumber(i) or 0.0,
            f = tonumber(f) or 0.0,
            v = tonumber(v) or 0.0
        }
    end

    local p2 = "meta%(%s*(%-?%d+)%s*,%s*(%-?%d+%.?%d*)%s*,%s*(%-?%d+%.?%d*)%s*%)"
    local i2, f2, v2 = str:match(p2)
    if i2 and f2 and v2 then
        return {
            i = tonumber(i2) or 0.0,
            f = tonumber(f2) or 0.0,
            v = tonumber(v2) or 0.0
        }
    end

    return nil
end

-- To Object
local function toObject(parsedWeather)
    if not parsedWeather or not parsedWeather.data then
        return nil
    end

    local data = parsedWeather.data
    local metaData = {}
    if data.data then
        if type(data.data) == "string" then
            local parsed = parseMetaString(data.data)
            if parsed then metaData = parsed end
        elseif type(data.data) == "table" then
            metaData = data.data
        end
    end

    return {
        id = metaData.i or 0,
        name = parsedWeather.name or "Unknown",
        frequency = metaData.f or 0.5,
        value = metaData.v or 0.0,
        color = data.color or { 1.0, 1.0, 1.0 },
        amount = data.amount or 10,
        size = data.size or 0.1,
        speed = data.speed or 1.0,
        lifetime = data.lifetime or 1.0,
        vel = data.vel or { 0.0, minHeight, 0.0 },
        temp = data.temp or { r = 0.0, g = 0.0, b = 0.0, strength = 0.0 },
        raw = data
    }
end

-- Set Min Height
local function setMinHeight(val)
    minHeight = val
    Parser.setVariable("minHeight", val)
end

--[[
    Load
]]
local function load(name)
    if weatherCache[name] then return weatherCache[name] end

    local result = Parser.load("weather", name)
    if not result or not result.data then
        print("[WeatherData] Failed to load weather: " .. name)
        return nil
    end

    local data = toObject(result)
    if data then
        if data.vel and type(data.vel) == "table" then
            data.vel[2] = minHeight
        end
        weatherCache[name] = data
    end

    return weatherCache[name]
end

-- Get Weather
local function getWeather(name)
    local key = name:lower()
    
    local data = weatherCache[key]
    if not data then data = load(key) end

    return data
end

-- Get Types
local function getTypes()
    if cachedEntries then return cachedEntries end
    
    local res = {}
    local entryCount = 0

    local parsedWeather = Parser.loadAll("weather")
    if not parsedWeather or #parsedWeather == 0 then
        print("[WeatherData] No weather files found")
        cachedEntries = res
        return res
    end
    
    for _, parsed in ipairs(parsedWeather) do
        if parsed.type == "weather" then
            local name = parsed.name:lower()
            local data = toObject(parsed)
            if data then
                weatherCache[name] = data

                res[name] = {
                    i = data.id or 0.0,
                    name = name,
                    f = data.frequency or 0.0,
                    v = data.value or 0.0
                }
                print("[WeatherData] Added entry: " .. name .. " (id=" .. data.id .. ")")
            end
        end
    end

    print(string.format("[WeatherData] Loaded %d weather types", #parsedWeather))
    
    cachedEntries = res
    return res
end

-- Get Particle
local function getParticle(name)
    local weather = getWeather(name)
    if not weather then return nil end

    return {
        color = weather.color or { 1.0, 1.0, 1.0 },
        amount = weather.amount or 10,
        size = weather.size or 0.1,
        speed = weather.speed or 1.0,
        lifetime = weather.lifetime or 1.0,
        vel = weather.vel or { 0.0, minHeight, 0.0 },
        temp = weather.temp or { r = 0.0, g = 0.0, b = 0.0, strength = 0.0 }
    }
end

-- Get Temp
local function getTemp(name)
    local weather = getWeather(name)
    if not weather or not weather.temp then return nil end
    return weather.temp
end

-- Get All Objects
local function getAllObjects()
    local results = {}
    local types = getTypes()

    for name, _ in pairs(types) do
        local weather = getWeather(name)
        if weather then table.insert(results, weather) end
    end

    return results
end

-- Print Weather
local function printAllWeather()
    local types = getTypes()
    print("=== Loaded Weather Types ===")
    for name, entry in pairs(types) do 
        local data = weatherCache[name]
        if data then
            print(string.format("  %s: id=%d, freq=%.2f, value=%.2f, amount=%d, size=%.2f", 
                name,
                data.id or 0,
                data.frequency or 0.5,
                data.value or 0.0,
                data.amount or 0,
                data.size or 0.0
            ))
        end
    end
end

--[[
    Init
]]
local function init()
    print("[WeatherData] Initializing...")
    getTypes()
    --printAllWeather()
    print(string.format("[WeatherData] Initialized with %d weather types", #weatherCache))
end
init()

return {
    getTypes = getTypes,
    getParticle = getParticle,
    getTemp = getTemp,
    setMinHeight = setMinHeight,
    printAllWeather = printAllWeather,
    getWeather = getWeather,
    toWeatherObject = toObject,
    getAllWeatherObjects = getAllObjects
}