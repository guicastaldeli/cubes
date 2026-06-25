#version 330 core

in vec4 vColor;
in vec2 vTexCoord;
in float fragDist;
in float vWorldY;
in vec3 vWorldPos;
flat in int vInstanceTexId;

out vec4 fragColor;

uniform vec4 uColor;
uniform int uHasColors;
uniform int hasTex;
uniform int isEntity;
uniform vec2 canvasSize;
uniform int shaderType;
uniform sampler2D uSampler;
uniform sampler2DArray uSamplerArray;
uniform int useArrayTexture;
uniform int shaderAddon;
uniform sampler2D uScreenTexture; 
uniform int isInv;
uniform float cameraY;

#include "../text/text.frag.glsl"
#include "../text/text.entity.frag.glsl"
#include "../ui/invert.glsl"
#include "../ui/hud.frag.glsl"
#include "../ui/ui.frag.glsl"
#include "../player/username.frag.glsl"
#include "flat.frag.glsl"
#include "outline.frag.glsl"
#include "particle.frag.glsl"
#include "../world/weather.frag.glsl"
#include "../world/skybox.frag.glsl"
#include "../world/star.frag.glsl"
#include "fog.glsl"

/**
 *
 * Set Entity Color
 *
 */
vec4 setEntityColor(vec4 texColor) {
    if(isEntity == 1) {
        vec4 color = vColor * texColor;     
        return color;
    } else {
        vec4 color = texColor;
        return color;
    }
}

/**
 *
 * Main
 *
 */
void main() {
    // Text
    if(shaderType == 1) {
        setTextFrag();
    }
    // UI
    else if(shaderType == 3) {
        setUIFrag();
    }
    // Username
    else if(shaderType == 4) {
        setUsernameFrag();
    }
    // Flat
    else if(shaderType == 5) {
        setFlatFrag();
    }
    // Outline
    else if(shaderType == 6) {
        setOutlineFrag();
    }
    // Particle
    else if(shaderType == 7) {
        setParticleFrag();
    }
    // Skybox
    else if(shaderType == 8) {
        setSkyboxFrag();
    }
    else if(shaderType == 9) {
        setStarFrag();
    }
    // Hud
    else if(shaderType == 10) {
        setHudFrag();
    }
    // Text Entity
    else if(shaderType == 11) {
        setTextEntityFrag();
    }
    // Main
    else {
        vec4 baseColor;
        
        if(hasTex == 1) {
            vec4 texColor;

            if(useArrayTexture == 1) {
                texColor = texture(uSamplerArray, vec3(vTexCoord, vInstanceTexId));
            } else {
                texColor = texture(uSampler, vTexCoord);
            }

            baseColor = setEntityColor(texColor);
        } else if(uHasColors == 1) {
            baseColor = vColor;
        } else {
            baseColor = uColor;
        }

        vec4 weather = applyWeatherTemp(baseColor);
        vec4 fog = applyFog(weather);
        
        //fragColor = fog;
        fragColor = baseColor;
    }
}