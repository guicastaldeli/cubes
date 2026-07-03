--[[
    Theme Parser for .th files
    Parses custom theme files with the format:
    {{@theme#NAME}}
        key = value,
        key = ~[ ... ],
        key = "string"
    {{#end}}
]]

local ThemeParser = {}

