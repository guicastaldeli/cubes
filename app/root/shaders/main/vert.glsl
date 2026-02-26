#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec4 aColor;

out vec4 vColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;
uniform int uHasColors;

void main() {
    gl_Position = uProj * uView * uModel * vec4(aPos, 1.0);
    vColor = uHasColors == 1 ? aColor : vec4(1.0); 
}