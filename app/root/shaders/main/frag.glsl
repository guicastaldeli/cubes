#version 330 core

in vec4 vColor;
out vec4 fragColor;

uniform vec4 uColor;
uniform int uHasColors;

void main() {
    fragColor = uHasColors == 1 ? vColor : uColor;
}