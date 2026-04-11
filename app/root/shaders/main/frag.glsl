#version 330 core

in vec4 vColor;
in vec2 vTexCoord;

out vec4 fragColor;

uniform vec4 uColor;
uniform int uHasColors;
uniform int hasTex;
uniform int shaderType;
uniform sampler2D uSampler;

#include "../text/text.frag.glsl"
#include "../ui/ui.frag.glsl"
#include "../player/username.frag.glsl"
#include "outline.frag.glsl"

void main() {
    /**

        Text

        */
    if(shaderType == 1) {
        setTextFrag();
    }
    /**

        UI

        */
    else if(shaderType == 3) {
        setUIFrag();
    }
    /**

        Username

        */
    else if(shaderType == 4) {
        setUsernameFrag();
    }
    /**

        Outline

        */
    else if(shaderType == 5) {
        setOutlineFrag();
    }
    /**

        Main

        */
    else {
        if(hasTex == 1) {
            fragColor = texture(uSampler, vTexCoord);
        } else if(uHasColors == 1) {
            fragColor = vColor;
        } else {
            fragColor = uColor;
        }
    }
}