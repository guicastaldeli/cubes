#version 330 core

in vec4 vColor;
in vec2 vTexCoord;

out vec4 fragColor;

uniform vec4 uColor;
uniform int uHasColors;
uniform int hasTex;
uniform int shaderType;
uniform sampler2D uSampler;

#include "../text/text_frag.glsl"

void main() {
    if(shaderType == 1) {
        setTextFrag();
    } else {
        if(hasTex == 1) {
            fragColor = texture(uSampler, vTexCoord);
        } else if(uHasColors == 1) {
            fragColor = vColor;
        } else {
            fragColor = uColor;
        }
    }
}