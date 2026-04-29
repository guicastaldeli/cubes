/**

    Weather Data to handle
    general weather props...

    */
-- Weather Helper
local function w(id, f, s) {
    return { i = id and id.i or nil, f = f, s = s }
}

local Weather {
    NORMAL = w({ id = 0 }, 0.5, 0.0),
    RAIN = w({ id = 1}, 0.3, 1.0),
    SNOW = w({ id = 2 }, 0.2, 2.0)
}