#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec4 aColor;
layout(location = 3) in vec2 aTexCoord;
layout(location = 4) in vec3 aInstanceOffset;
layout(location = 5) in vec4 aInstanceColor;
layout(location = 6) in vec4 aInstanceRotation;
layout(location = 7) in int aInstanceTexId;

out vec4 vColor;
out vec2 vTexCoord;
out float fragDist;
out float vWorldY;
out vec3 vWorldPos;
flat out int vInstanceTexId;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform int uHasColors;
uniform int shaderType;
uniform int isInstanced;
uniform vec2 screenSize;
uniform int hasInstanceColor;
uniform int isInv;

#include "../text/text.vert.glsl"
#include "../text/text.entity.vert.glsl"
#include "../ui/hud.vert.glsl"
#include "../ui/ui.vert.glsl"
#include "../player/username.vert.glsl"
#include "flat.vert.glsl"
#include "outline.vert.glsl"
#include "particle.vert.glsl"
#include "../world/skybox.vert.glsl"
#include "../world/star.vert.glsl"

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

        Skybox and Stars

        */
    else if(shaderType == 8) {
        setSkyboxVert();
    }
    else if(shaderType == 9) {
        setStarVert();
    }
    /**

        Hud

        */
    else if(shaderType == 10) {
        setHudVert();
    }
    /**

        Text Entity

        */
    else if(shaderType == 11) {
        setTextEntityVert();
    }
    /**
 
        Main

        */
    else {
        vec3 pos = aPos;

        if(isInstanced == 1) {
            float angle = radians(aInstanceRotation.x);
            float cosA = cos(angle);
            float sinA = sin(angle);

            mat3 instanceRotation = mat3(
                cosA, 0.0, sinA,
                0.0, 1.0, 0.0,
                -sinA, 0.0, cosA
            );

            pos = instanceRotation * pos;
            pos += aInstanceOffset;
            
            vWorldPos = pos;
            vWorldY = pos.y;

            vec4 viewPos = uView * vec4(pos, 1.0);
            gl_Position = uProjection * viewPos;

            if(hasInstanceColor == 1) {
                vColor = aInstanceColor;
            } else {
                vColor = uHasColors == 1 ? aColor : vec4(1.0);
            }

            vTexCoord = aTexCoord;
            fragDist = length(viewPos.xyz);
            vInstanceTexId = aInstanceTexId;

            return;
        }

        vec4 worldPos = uModel * vec4(pos, 1.0);
        vWorldPos = worldPos.xyz;
        vWorldY = worldPos.y;
        vec4 viewPos = uView * worldPos;

        gl_Position = uProjection * viewPos;

        vColor = uHasColors == 1 ? aColor : vec4(1.0);
        vTexCoord = aTexCoord;
        fragDist = length(viewPos.xyz);
    }
}