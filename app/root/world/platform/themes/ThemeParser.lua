--[[

    Theme Parser for .th files
    Parses custom theme files with the format:
    {{@theme#NAME}}
        key = value,
        key = ~[ ... ],
        key = "string"
    {{@end}}

]]
local themeCache = {}

local ThemeParser = {}
ThemeParser.config = {
    path = "resource/theme/",
    fileExtension = ".th",
    directives = {
        start = "{{@theme#",
        endt = "{{@end}}",
        codeBlock = "~[",
        codeBlockEnd = "]"
    }
}

-- Get Theme
function ThemeParser.getTheme(name)
    local themes = ThemeParser.loadAllThemes()
    for _, theme in ipairs(themes) do
        if theme.name == name then
            return theme
        end
    end

    return nil
end

-- To Theme Object
function ThemeParser.toThemeObject(parsedTheme)
    local themeData = {
        id = parsedTheme.id or 0,
        name = parsedTheme.name or "Unknown",
        movement = parsedTheme.movement or "",
        audio = parsedTheme.audio or "",
        top = parsedTheme.top or nil,
        particles = parsedTheme.particles or "",
        texture = parsedTheme.texture or ""
    }

    return themeData
end

--[[
    Parse
    ]]
-- Parse File
function ThemeParser.parseFile(filename)
    local fullPath = ThemeParser.config.path .. filename
    
    if themeCache[fullPath] then
        return themeCache[fullPath]
    end

    local file = io.open(fullPath, "r")
    if not file then
        print("Theme file not found" .. fullPath)
        return nil
    end

    local content = file:read("*all")
    file:close()

    local theme = ThemeParser.parseContent(content, filename)

    if theme then
        themeCache[fullPath] = theme
    end

    return theme
end

-- Parse Content
function ThemeParser.parseContent(content, filename)
    local theme = {}
    theme.sourceFile = filename or ""

    local startPattern = "{{@theme#([^}]+)}}"
    local nameMatch = content:match(startPattern)
    if not nameMatch then
        print("Invalid theme format: Missing start directive in " .. (filename or "content"))
        return nil
    end

    theme.name = nameMatch:gsub("%s+", "")
    theme.rawName = nameMatch

    local endPattern = "{{@end}}"
    local endPos = content:find(endPattern)
    if not endPos then
        print("Invalid theme format: Missing end directive in " .. (filename or "content"))
        return nil
    end

    local startTag = "{{@theme#" .. nameMatch .. "}}"
    local themeContent = content:sub(#startTag + 1, endPos - 1)
    ThemeParser.parseProperties(theme, themeContent)

    return theme
end

-- Parse Properties
function ThemeParser.parseProperties(theme, content)
    local regex = "[^\n]*"
    local lines = {}
    for line in content:gmatch(regex) do
        table.insert(lines, line)
    end

    local i = 1
    while i <= #lines do
        local line = lines[i]

        local l1 = "^%s*$"
        local l2 = "^%s*%-%-"

        if line:match(l1) or line:match(l2) then
            i = i+1
            goto continue
        end

        local c = "^(%s*)([%w_]+)%s*=%s*~%[%s*$"
        local codeBlockStart = line:match(c)
        if codeBlockStart then
            local indent = codeBlockStart:match("^(%s*)")
            local key = codeBlockStart:match("%s*([%w_]+)%s*=%s*~%[%s*$")

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
                i = i+1
            end

            if foundEnd then
                local codeBlock = table.concat(codeLines, "\n")
                codeBlock = codeBlock:gsub(",%s*$", "")
                theme[key] = codeBlock
            else 
                print("Warning: Unclosed code block for key: " .. key)

                local f = table.concat(lines, "\n", i - #codeLines - 1)
                theme[key] = f
            end

            goto continue
        end

        local v1 = "^%s*([%w_]+)%s*=%s*(.-)%s*,%s*$"
        local v2 ="^%s*([%w_]+)%s*=%s*(.-)%s*$"
        local key, value = line:match(v1)
        if not key then
            key, value = line:match(v2)
        end

        if key and value then
            value = value:gsub(",%s*$", "")
            value = value:gsub("^%s*(.-)%s*$", "%1")
            theme[key] = ThemeParser.parseValue(value)
        end

        ::continue::
        i = i+1
    end
end

-- Parse Value
function ThemeParser.parseValue(value)
    if not value then return nil end

    value = value:gsub(",%s*$", "")
    value = value:gsub("^%s*(.-)%s*$", "%1")
    if value == "nil" then
        return nil
    end

    if value == "true" then
        return true
    end
    if value == "false" then
        return false
    end

    local n1 = "^%-?%d+$"
    local n2 = "^%-?%d+%.%d+$"
    if value:match(n1) then
        return tonumber(value)
    end
    if value:match(n2) then
        return tonumber(value)
    end

    if value:match('^".-"$') or value:match("^'.-'$") then
        return value:sub(2, -2)
    end

    if value:match("^{") then
        return ThemeParser.parseTable(value)
    end

    return value
end

-- Parse Table
function ThemeParser.parseTable(value)
    local content = value:gsub("^{%s*", ""):gsub("%s*}$", "")
    local result = {}

    local regex = "[^,]+"
    for pair in content:gmatch(regex) do
        local v = "^%s*([%w_]+)%s*=%s*(.-)%s*$"
        local key, val = pair:match(v)
        if key and val then
            result[key] = ThemeParser.parseValue(val)
        else
            local item = ThemeParser.parseValue(pair)
            table.insert(result, item)
        end
    end

    return result
end

--[[
    Load
    ]]
function ThemeParser.loadAllThemes()
    local themes = {}
    local path = ThemeParser.config.path

    local handle
    if package.config:sub(1,1) == "\\" then
        -- Windows
        handle = io.popen('dir /b "' .. path .. '" 2>nul')
    else
        -- Unix/Linux
        handle = io.popen('ls "' .. path .. '" 2>/dev/null')
    end

    if handle then
        for file in handle:lines() do
            local f = "%.th$"
            if file:match(f) then
                local theme = ThemeParser.parseFile(file)
                if theme then
                    table.insert(themes, theme)
                    print("!!! Loaded theme: " .. theme.name .. " from " .. file)
                end
            end
        end

        handle:close()
    else
        print("No .th files found in " .. path)
    end

    return themes
end

--[[
    Clear
    ]]
function ThemeParser.clearCache()
    themeCache = {}
end

return ThemeParser