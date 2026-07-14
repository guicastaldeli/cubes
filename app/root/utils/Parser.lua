--[[
    Parses custom files with the format:
    {{@type#NAME}}
        key = value,
        key = ~[ ... ],
        key = "string"
    {{@end}}

]]
local Parser = {}
Parser.config = {
    path = "resource/",
    registeredPaths = {},
    fileExtension = {},
    variables = {},
    directives = {
        start = "{{@",
        endt = "}}",
        blockStart = "{{@",
        blockEnd = "}}",
        codeBlock = "~[",
        codeBlockEnd = "]"
    },
    cacheEnabled = true,
    debug = false
}

local parserCache = {
    files = {},
    parsed = {}
}

-- File Exists
local function fileExists(path)
    local dirCheck = io.open(path, "r")
    if dirCheck then
        dirCheck:close()
        return true
    end
    
    local file = io.open(path, "r")
    if file then
        file:close()
        return true
    end

    return false
end

-- Debug
local function debugPrint(...)
    if Parser.config.debug then
        print("[Parser]", ...)
    end
end

-- Debug path
local function debugPath(path)
    print("=== PATH DEBUG ===")
    print("Path:", path)
    print("Exists:", fileExists(path))
    
    local handle
    if package.config:sub(1,1) == "\\" then
        handle = io.popen('dir "' .. path .. '" 2>nul')
    else
        handle = io.popen('ls -la "' .. path .. '" 2>/dev/null')
    end
    
    if handle then
        print("Directory contents:")
        for line in handle:lines() do
            print("  " .. line)
        end
        handle:close()
    else
        print("Could not list directory")
    end
    print("==================")
end

-- Read File
local function readFile(path)
    if not fileExists(path) then return nil end

    local file = io.open(path, "r")
    if not file then
        return nil
    end

    local content = file:read("*all")
    file:close()
    return content
end

-- List Files
local function listFiles(dir, extension)
    local files = {}
    local handle

    if package.config:sub(1,1) == "\\" then
        handle = io.popen('dir /b "' .. dir .. '" 2>nul')
    else
        handle = io.popen('ls "' .. dir .. '" 2>/dev/null')
    end

    if not handle then
        return files
    end

    for file in handle:lines() do
        if extension and file:match(extension .. "$") then
            table.insert(files, file)
        elseif not extension then
            table.insert(files, file)
        end
    end

    handle:close()
    return files
end

-- Normalize Path
local function normalizePath(path)
    path = path:gsub("\\", "/")
    path = path:gsub("/$", "")
    return path
end

-- Build Path
local function buildPath(base, filename)
    base = normalizePath(base)
    filename = normalizePath(filename)

    if base:sub(-1) ~= "/" then
        base = base .. "/"
    end

    return base .. filename
end

-- Get Type from Directive
local function getTypeFromDirective(content)
    local pattern = "{{@([%w_]+)#[^}]+}}"
    local type = content:match(pattern)
    return type
end

-- Get Name from Directive
local function getNameFromDirective(content)
    local pattern = "{{@[%w_]+#([^}]+)}}"

    local name = content:match(pattern)
    if name then name = name:gsub("%s+", "") end

    return name
end

-- Get Extension for Type
local function getExtensionForType(type)
    if Parser.config.fileExtension[type] then
        return Parser.config.fileExtension[type]
    end

    return "." .. type
end

-- Get Path for Type
local function getPathForType(type)
    if Parser.config.registeredPaths[type] then
        local base = normalizePath(Parser.config.path)
        local registered = normalizePath(Parser.config.registeredPaths[type])
        return base .. "/" .. registered .. "/"
    end

    local path = Parser.config.path .. type .. "/"
    return path
end

-- Set Variable
function Parser.setVariable(key, value)
    Parser.config.variables[key] = value
end

-- Get Variable
function Parser.getVariable(key)
    return Parser.config.variables[key]
end

--[[
    Parse Value
]]
local function parseValue(value)
    if not value then return nil end

    value = value:gsub(",%s*$", "")
    value = value:gsub("^%s*(.-)%s*$", "%1")

    if type(value) == "string" then
        local pattern = "%$([%w_]+)"
        value = value:gsub(pattern, function(varName)
            local varValue = Parser.config.variables[varName]
            if varValue ~= nil then return tostring(varValue) end
            return "$" .. varName
        end)
    end

    if value == "nil" then return nil end
    if value == "true" then return true end
    if value == "false" then return false end

    local p1 = "^%-?%d+$"
    if value:match(p1) then return tonumber(value) end
    
    local p2 = "^%-?%d+%.%d+$"
    if value:match(p2) then return tonumber(value) end

    local p3 = '^".-"$'
    local p4 = "^'.-'$"
    if value:match(p3) or value:match(p4) then return value:sub(2, -2) end

    if value:match("^{") then 
        local content = value:gsub("^{%s*", ""):gsub("%s*}$", "")
        local result = {}
        if content == "" then return result end

        local inString = false
        local current = ""
        local pairs = {}

        for i = 1, #content do
            local char = content:sub(i, i)

            if char == '"' or char == "'" then 
                inString = not inString 
            end
            if char == "," and not inString then
                table.insert(pairs, current)
                current = ""
            else
                current = current .. char
            end
        end

        if current ~= "" then
            table.insert(pairs, current)
        end

        for _, pair in ipairs(pairs) do
            local key, val = pair:match("^%s*([%w_]+)%s*=%s*(.-)%s*$")
            if key and val then
                result[key] = parseValue(val)
            else
                local item = parseValue(pair)
                table.insert(result, item)
            end
        end

        return result
    end

    return value
end

--[[
    Parse Block
]]
local function parseBlock(content)
    local result = {}

    local lines = {}
    local pattern = "[^\n]*"
    for line in content:gmatch(pattern) do
        table.insert(lines, line)
    end

    local i = 1
    while i <= #lines do
        local line = lines[i]

        local l1 = "^%s*$"
        local l2 = "^%s*%-%-"
        if not line:match(l1) and not line:match(l2) then
            local s = "^(%s*)([%w_]+)%s*=%s*~%[%s*$"
            local start = line:match(s)

            if start then
                local key = line:match("^%s*([%w_]+)%s*[=:]%s*~%[")
                if key then
                    local indent = line:match("^(%s*)")
                 
                    local codeLines = {}
                    i = i+1
                    local foundEnd = false

                    while i <= #lines do
                        local currentLine = lines[i]


                        if currentLine:match("^" .. indent .. "%]%s*,%s*$") or 
                        currentLine:match("^" .. indent .. "%]%s*$") or
                        currentLine:match("^%s*%]%s*,%s*$") or
                        currentLine:match("^%s*%]%s*$") then
                            foundEnd = true
                            i = i+1
                            break
                        end

                        local codeLine = currentLine:gsub("^" .. indent, "")
                        table.insert(codeLines, codeLine)
                        i = i + 1
                    end

                    if foundEnd then
                        result[key] = table.concat(codeLines, "\n")
                    else
                        result[key] = table.concat(lines, "\n", i - #codeLines - 1)
                    end

                    i = i + 1
                    goto continue
                end
            end

            local p1 = "^%s*([%w_]+)%s*=%s*(.-)%s*,%s*$"
            local p2 = "^%s*([%w_]+)%s*=%s*(.-)%s*$"
            local key, value = line:match(p1)
            if not key then key, value = line:match(p2) end
            if key and value then result[key] = parseValue(value) end
        end
        
        ::continue::
        i = i+1
    end

    return result
end

--[[
    Parse Content
]]
function Parser.parseContent(content)
    if not content then return nil end

    local type = getTypeFromDirective(content)
    local name = getNameFromDirective(content)
    if not type then print("[Parser] No type found in content") return nil end
    if not name then print("[Parser] No name found in content") return nil end

    local startPattern = "{{@" .. type .. "#" .. name .. "}}" 

    local endPattern = "{{@end}}"
    local endPos = content:find(endPattern)
    if not endPos then
        print("[Parser] Missing end directive")
        return nil
    end

    local blockContent = content:sub(#startPattern+1, endPos-1)
    local data = parseBlock(blockContent)

    return {
        type = type,
        name = name,
        data = data
    }
end

--[[
    Parse File
]]
function Parser.parseFile(filepath)
    if Parser.config.cacheEnabled and parserCache.parsed[filepath] then
        return parserCache.parsed[filepath]
    end

    local content = readFile(filepath)
    if not content then 
        debugPrint("File not found!:", filepath) 
        return nil 
    end

    local result = Parser.parseContent(content)
    if Parser.config.cacheEnabled and result then
        parserCache.parsed[filepath] = result
    end

    return result
end

--[[
    Load
]]
-- Load
function Parser.load(type, name)
    local path = getPathForType(type)
    local extension = getExtensionForType(type)

    local filename = name .. extension
    local fullPath = buildPath(path, filename)

    debugPrint("Loading...:", fullPath)

    return Parser.parseFile(fullPath)
end

-- Load All
function Parser.loadAll(type)
    local results = {}
    local path = getPathForType(type)
    local extension = getExtensionForType(type)

    debugPrint("Loading all...", type, "from", path)

    local files = listFiles(path, extension)
    if #files == 0 then
        print("[Parser] No files found in:", path)
        return results
    end

    for _, file in ipairs(files) do
        local fullPath = buildPath(path, file)
        local parsed = Parser.parseFile(fullPath)
        if parsed then
            table.insert(results, parsed)
            debugPrint("Loaded:", file)
        end
    end

    return results
end

--[[
    Get Value
]]
function Parser.getValue(type, name, key)
    local parsed = Parser.load(type, name)
    if not parsed then return nil end
    return parsed.data[key]
end

--[[
    Register Type
]]
function Parser.registerType(type, path, extension)
    if extension then Parser.config.fileExtension[type] = extension end
    if path then Parser.config.registeredPaths[type] = path end
    debugPrint("Registered type:", type, "path", path, "extension:", extension)
end

--[[
    Clear
]]
-- Clear
function Parser.clear()
    parserCache.parsed = {}
    parserCache.files = {}

    debugPrint("Cleared!")
end

-- Clear Variables
function Parser.clearVariables()
    Parser.config.variables = {}
end

return Parser