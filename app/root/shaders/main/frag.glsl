#version 330 core

in vec4 vColor;
in vec2 vTexCoord;
in float fragDist;

out vec4 fragColor;

uniform vec4 uColor;
uniform int uHasColors;
uniform int hasTex;
uniform int shaderType;
uniform sampler2D uSampler;
uniform int shaderAddon;

#include "../text/text.frag.glsl"
#include "../ui/ui.frag.glsl"
#include "../player/username.frag.glsl"
#include "flat.frag.glsl"
#include "outline.frag.glsl"
#include "particle.frag.glsl"
#include "../world/weather.frag.glsl"
#include "../world/skybox.frag.glsl"
#include "../world/star.frag.glsl"

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

        Flat

        */
    else if(shaderType == 5) {
        setFlatFrag();
    }
    /**

        Outline

        */
    else if(shaderType == 6) {
        setOutlineFrag();
    }
    /**

        Particle

        */
    else if(shaderType == 7) {
        setParticleFrag();
    }
    /**

        Skybox

        */
    else if(shaderType == 8) {
        setSkyboxFrag();
    }
    else if(shaderType == 9) {
        setStarFrag();
    }
    /**

        Main

        */
    else {
        vec4 baseColor;
        
        if(hasTex == 1) {
            baseColor = texture(uSampler, vTexCoord);
        } else if(uHasColors == 1) {
            baseColor = vColor;
        } else {
            baseColor = uColor;
        }

        fragColor = applyWeatherTemp(baseColor);
    }
}