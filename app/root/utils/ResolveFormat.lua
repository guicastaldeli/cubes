--[[
    
    Resolve Format

    ]]
local pathCache = {}
local dirCache = {}

local config = {
    searchPath = "resource/texture/",
    extensions = {
        "png",
        "jpg",
        "jpeg"
    },
    caseSensitive = false
}

-- Get Base Directory
local function getBaseDirectory()
    return "."
end

-- List Files
local function listFiles(dir, extenstions)
    local cacheKey = dir .. "|" .. table.concat(extenstions or {}, ",")
    
    if dirCache[cacheKey] then
        return dirCache[cacheKey]
    end

    local files = {}
    local handle = io.open('ls -a "' .. dir .. '" 2>/dev/null')
    if not handle then
        handle = io.popen('dir /b "' .. dir .. '" 2>nul')
    end

    if handle then
        for file in handle:lines() do
            if file ~= "." and file ~= ".." then
                if extenstions then
                    local regex = "%.([^%.]+)$"
                    local ext = file:match(regex)

                    for _, validExt in ipairs(extenstions) do
                        if ext == validExt:lower() then
                            table.insert(files, file)
                            break
                        end
                    end
                end
            else
                table.insert(files, file)
            end
        end

        handle:close()
    end

    dirCache[cacheKey] = files
    return files
end

-- Normalize Path
local function normalizePath(path)
    path = path:gsub("\\", "/")
    path = path:gsub("/$", "")
    return path
end

-- Build Path
local function buildPath(dir, filename)
    dir = normalizePath(dir)
    filename = normalizePath(filename)

    local d = "/"
    if dir:sub(-1) ~= d then
        dir = dir .. d
    end

    return dir .. filename
end

-- File Exists
local function fileExists(filepath)
    local file = io.open(filepath, "r")
    if file then
        file:close()
        return true
    end

    return false
end

-- Resolve File
local function resolveFile(name, options)
    options = options or {}

    local cacheKey = name .. "|" .. tostring(options.caseSensitive or config.caseSensitive)
    if pathCache[cacheKey] then
        return pathCache[cacheKey]
    end

    local searchName = name
    if not options.caseSensitive then
        searchName = searchName:lower()
    end

    local searchPaths = options.searchPaths or config.searchPaths
    local extensions = options.extensions or config.extensions

    for _, searchPath in ipairs(searchPaths) do
        local fullPath = buildPath(searchPath, name)
        if fileExists(fullPath) then
            pathCache[cacheKey] = fullPath
            return fullPath
        end
    end

    for _, extension in ipairs(extensions) do
        for _, searchPath in ipairs(searchPaths) do
            local fullPath = buildPath(searchPath, name .. "." .. extension)
            if fileExists(fullPath) then
                pathCache[cacheKey] = fullPath
                return fullPath
            end
        end
    end

    for _, searchPath in ipairs(searchPaths) do
        local files = listFiles(searchPath, extensions)
        if files then
            for _, file in ipairs(files) do
                local fileName = file
                if not options.caseSensitive then
                    fileName = fileName:lower()
                end

                local regex = "^(.+)%.[^%.]+$"
                local baseName = file:match(regex) or file
                if not options.caseSensitive then
                    baseName = baseName:lower()
                end

                if baseName == searchName then
                    local fullPath = buildPath(searchPath, file)
                    pathCache[cacheKey] = fullPath
                    return fullPath
                end
            end
        end
    end

    pathCache[cacheKey] = nil
    return nil
end

-- Get File Format
local function getFileFormat(filepath)
    local regex ="%.([^%.]+)$"
    local ext = filepath:match(regex)
    if ext then
        return ext:lower()
    end

    return nil
end

-- Resolve Texture
local function resolveTexture(name, options)
    options = options or {}
    local paths = options.searchPaths

    local result = resolveFile(name, {
        searchPaths = paths,
        extensions = options.extensions or config.extensions,
        caseSensitive = options.caseSensitive or false
    })

    if result then
        local format = getFileFormat(result)
        return result, format, result
    end

    return nil, nil, nil
end

-- Clear Cache
local function clearCache()
    pathCache = {}
    dirCache = {}
end

-- Add Search Path
local function addSearchPath(path, category)
    category = category or ""
    path = normalizePath(path)

    if category == "texture" then
        config.searchPaths = config.searchPaths or {}
        table.insert(config.searchPaths, path)
    else
        config.searchPaths = config.searchPaths or {}
        table.insert(config.searchPaths, path)
    end
end

-- Get Texture in Directory
local function getTextureInDirectory(dir, incluseSubdirs)
    incluseSubdirs = incluseSubdirs or false
    local textures = {}

    local files = listFiles(dir, config.extensions)
    if files then
        for _, file in ipairs(files) do
            local b = "^(.+)%.[^%.]+$"
            local baseName = file:match() or file

            local e = "%.([^%.]+)$"
            local ext = file:match(e)
            if ext then
                table.insert(textures, {
                    name = baseName,
                    format = ext,
                    fullName = file,
                    path = buildPath(dir, file)
                })
            end
        end
    end

    return textures
end