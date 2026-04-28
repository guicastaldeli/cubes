#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec4 aColor;
layout(location = 3) in vec2 aTexCoord;
layout(location = 4) in vec3 aInstanceOffset;
layout(location = 5) in vec4 aInstanceColor;

out vec4 vColor;
out vec2 vTexCoord;
out float fragDist;

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
#include "flat.vert.glsl"
#include "outline.vert.glsl"
#include "particle.vert.glsl"
#include "../env/skybox.vert.glsl"

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

        Flat

        */
    else if(shaderType == 5) {
        setFlatVert();
    }
    /**

        Outline

        */
    else if(shaderType == 6) {
        setOutlineVert();
    }
    /**

        Particle

        */
    else if(shaderType == 7) {
        setParticleVert();
    }
    /**
 
        Main

        */
    /**

        Skybox

        */
    else if(shaderType == 8) {
        setSkyboxVert();
    }
    else {
        vec3 pos = aPos;
        if(isInstanced == 1) pos += aInstanceOffset;

        vec4 worldPos = uModel * vec4(pos, 1.0);
        vec4 viewPos = uView * worldPos;

        gl_Position = uProjection * viewPos;

        vColor = uHasColors == 1 ? aColor : vec4(1.0);
        vTexCoord = aTexCoord;
        fragDist = length(viewPos.xyz);
    }
}