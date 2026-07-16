--[[
    
    Calculate Player dynamic movement...

    ]]
local CalculateMovement = {}

-- Convert Value
function convertValue(value)
    if type(value) == "string" then
        if value == "true" then return true end
        if value == "false" then return false end

        local num = tonumber(value)
        if num then return num end
    end

    return value
end

-- To String
function CalculateMovement.toString(movementTable)
    if not movementTable or type(movementTable) ~= "table" then return "" end

    local parts = {}

    local fb = "%s=%s"
    local fn = "%s=%.2f"
    local fs = "%s='%s'"

    for key, value in pairs(movementTable) do
        if type(value) == "boolean" then
            table.insert(parts, string.format(fb, key, tostring(value)))
        elseif type(value) == "number" then
            table.insert(parts, string.format(fn, key, value))
        elseif type(value) == "string" then
            table.insert(parts, string.format(fs, key, value))
        end
    end
    
    return table.concat(parts, ", ")
end

--[[
    Key
]]
-- Has Key
function CalculateMovement.hasKey(movementTable, key)
    if not movementTable or type(movementTable) ~= "table" then 
        return false 
    end
    return movementTable[key] ~= nil
end

-- Keys
function CalculateMovement.keys(movementTable)
    if not movementTable or type(movementTable) ~= "table" then
        return {}
    end

    local keys = {}
    for key, _ in pairs(movementTable) do
        table.insert(keys, key)
    end

    return keys
end

--[[
    Parse
]]
-- Parse
function CalculateMovement.parse(movementData)
    if not movementData then return {} end
    
    if type(movementData) == "string" then return parseMovementString(movementData) end
    if type(movementData) == "table" then return movementData end

    return {}
end

-- Parse Movement String
function parseMovementString(str)
    local p = "^%s*(.-)%s*$"
    str = str:gsub(p, "%1")
    if str == "" then return {} end
    
    local result = {}

    local m1 = "[^,]+" 
    local m2 = "^%s*([%w_]+)%s*=%s*(.-)%s*$"
    for pair in str:gmatch(m1) do
        local key, val = pair:match(m2)
        if key and val then
            result[key] = convertValue(val)
        end
    end
end

--[[
    Merge
]]
function CalculateMovement.merge(...)
    local args = {...}
    local result = {}

    for _, profile in ipairs(args) do
        if profile and type(profile) == "table" then
            for key, value in pairs(profile) do
                result[key] = convertValue(value)
            end
        end
    end

    return result
end

--[[
    Get
]]
function CalculateMovement.get(movementTable, key, default)
    if not movementTable or type(movementTable) ~= table then return default end
    
    local val = movementTable[key]
    if val == nil then return default end
    return val
end
