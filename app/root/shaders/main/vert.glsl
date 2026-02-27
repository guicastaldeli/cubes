#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec4 aColor;
layout(location = 3) in vec2 aTexCoord;

out vec4 vColor;
out vec2 vTexCoord;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform int uHasColors;
uniform int shaderType;
uniform vec2 screenSize;

#include "../text/text_vert.glsl"

void main() {
    if(shaderType == 1) {
        setTextVert();
    } else {
        gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
        vColor = uHasColors == 1 ? aColor : vec4(1.0); 
        vTexCoord = aTexCoord;
    }
}