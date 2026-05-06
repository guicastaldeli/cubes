meshType = "triangle"

-- Vertices
vertices = {
    -0.5, -0.5, -0.5,
    0.5, -0.5, -0.5,
    0.5, -0.5,  0.5,
    -0.5, -0.5,  0.5,
    0.0,  0.5,  0.0
}

-- Indices
indices = { 
    0, 1, 2,
    0, 2, 3,
    0, 1, 4,
    1, 2, 4,
    2, 3, 4,
    3, 0, 4
}

-- Colors
colors = {
    1.0, 0.0, 0.0, 1.0,
    0.0, 1.0, 0.0, 1.0,
    0.0, 0.0, 1.0, 1.0,
    1.0, 1.0, 0.0, 1.0,
    1.0, 0.0, 1.0, 1.0
}

-- Normals
normals = {
    0.0, -1.0,  0.0,
    0.0, -1.0,  0.0,
    0.0, -1.0,  0.0,
    0.0, -1.0,  0.0,
    0.0,  1.0,  0.0
}

-- Tex Coords
texCoords = {
    0.0, 0.0,
    1.0, 0.0,
    1.0, 1.0,
    0.0, 1.0,
    0.5, 0.5
}

-- Rotation
rotation = {
    axis = "Y",
    speed = 100.0
}

-- Collider
collider = {
    shape = "triangle"
}