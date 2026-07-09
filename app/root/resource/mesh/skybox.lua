meshType = "skybox"

local stacks = 20.0
local slices = 20.0
local radius = 1.0

local verts = {}
local norms = {}
local texs = {}
local idx = {}
local cols = {}

local PI = math.pi

local function randomColor()
    return {
        math.random(),
        math.random(),
        math.random(),
        1.0
    }
end

local function pushVertex(
    vx, vy, vz,
    nx, ny, nz,
    u, v,
    col
)
    table.insert(verts, vx)
    table.insert(verts, vy)
    table.insert(verts, vz)

    table.insert(norms, nx)
    table.insert(norms, ny)
    table.insert(norms, nz)

    table.insert(texs, u)
    table.insert(texs, v)

    table.insert(cols, col[1])
    table.insert(cols, col[2])
    table.insert(cols, col[3])
    table.insert(cols, col[4])
end

local function spherePoint(phi, theta)
    local x = math.sin(phi) * math.cos(theta)
    local y = math.cos(phi)
    local z = math.sin(phi) * math.sin(theta)
    return x, y, z
end

num = 42
math.randomseed(num)

-- Vertices
for i = 0, stacks - 1 do
    for j = 0, slices - 1 do
        local phi1 = PI * i / stacks
        local phi2 = PI * (i+1) / stacks
        local theta1 = 2 * PI * j / slices
        local theta2 = 2 * PI * (j+1) / slices

        local x00, y00, z00 = spherePoint(phi1, theta1)
        local x10, y10, z10 = spherePoint(phi2, theta1)
        local x01, y01, z01 = spherePoint(phi1, theta2)
        local x11, y11, z11 = spherePoint(phi2, theta2)

        local u0 = j / slices
        local u1 = (j+1) / slices
        local v0 = i / stacks
        local v1 = (i+1) / stacks

        local c1 = randomColor()
        pushVertex(radius*x00, radius*y00, radius*z00, x00, y00, z00, u0, v0, c1)
        pushVertex(radius*x10, radius*y10, radius*z10, x10, y10, z10, u0, v1, c1)
        pushVertex(radius*x01, radius*y01, radius*z01, x01, y01, z01, u1, v0, c1)

        local c2 = randomColor()
        pushVertex(radius*x01, radius*y01, radius*z01, x01, y01, z01, u1, v0, c2)
        pushVertex(radius*x10, radius*y10, radius*z10, x10, y10, z10, u0, v1, c2)
        pushVertex(radius*x11, radius*y11, radius*z11, x11, y11, z11, u1, v1, c2)
    end
end

vertices = verts
normals = norms
texCoords = texs
colors = cols

-- Rotation
rotation = {
    axis = "Y",
    speed = 0.0
}

-- Collider
collider = {
    shape = "sphere"
}