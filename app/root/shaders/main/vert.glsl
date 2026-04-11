#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec4 aColor;
layout(location = 3) in vec2 aTexCoord;
layout(location = 4) in vec3 aInstanceOffset;

out vec4 vColor;
out vec2 vTexCoord;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform int uHasColors;
uniform int shaderType;
uniform int isInstanced;
uniform vec2 screenSize;

#include "../text/text.vert.glsl"
#include "../ui/ui.vert.glsl"
#include "../player/username.vert.glsl"
#include "outline.vert.glsl"

void main() {
    /**

        Text

        */
    if(shaderType == 1) {
        setTextVert();
    }
    /**

        UI

        */
    else if(shaderType == 3) {
        setUIVert();
    }
    /**

        Username

        */
    else if(shaderType == 4) {
        setUsernameVert();
    }
    /**

        Outline

        */
    else if(shaderType == 5) {
        setOutlineVert();
    }
    /**

        Main

        */
    else {
        vec3 worldPos = aPos;
        if(isInstanced == 1) worldPos += aInstanceOffset;

        gl_Position = uProjection * uView * uModel * vec4(worldPos, 1.0);
        vColor = uHasColors == 1 ? aColor : vec4(1.0); 
        vTexCoord = aTexCoord;
    }
}