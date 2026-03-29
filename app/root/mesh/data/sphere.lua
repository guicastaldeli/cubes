meshType = "sphere"

local stacks = 20.0
local slices = 20.0
local radius = 1.0

local vets = {}
local normals = {}
local texs = {}
local idx = {}
local cols = {}

local PI = math.pi

-- Vertices
for i = 0, stacks do
    local phi = PI * i / stacks

    for j = 0, slices do
        local theta = 2 * PI * j / slices
        
        local x = math.sin(phi) * math.cos(theta)
        local y = math.cos(phi)
        local z = math.sin(phi) * math.sin(theta)

        -- Vertex
        table.insert(verts, radius * x)
        table.insert(verts, radius * y)
        table.insert(verts, radius * z)

        -- Normals
        table.insert(norms, x)
        table.insert(norms, y)
        table.insert(norms, z)

        -- Tex Coords
        table.insert(texs, j / slices)
        table.insert(texs, i / stacks)

        -- Color
        table.insert(cols, 1.0)
        table.insert(cols, 1.0)
        table.insert(cols, 1.0)
        table.insert(cols, 1.0)
    end
end

-- Indices
for i = 0, stacks - 1 do
    for j = 0, slices - 1 do
        local row1 = i * (slices + 1)
        local row2 =  (i+1) * (slices + 1)

        table.insert(idx, row1 + j)
        table.insert(idx, row2 + j)
        table.insert(idx, row1 + j+1)

        table.insert(idx, row1 + j+1)
        table.insert(idx, row2 + j)
        table.insert(idx, row2 + j+1)
    end
end

vertices = verts
indices = idx
normals = norms
texCoords = texs
colors = cols

rotation = {
    axis = "Y",
    speed = 30.0
}