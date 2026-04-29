/**

    Skybox stars frag shader

    */
uniform float starTransition;

// Star Intensity
float getStarIntensity(float hour) {
    // MIDNIGHT and NIGHT
    if(periodType == 1 || periodType == 2) {
        return 1.0;
    }
    // DUSK
    if(periodType == 3) {
        return smoothstep(17.0, 19.0, hour);
    }
    // DAWN
    if(periodType == 4) {
        return 1.0 - smoothstep(4.0, 6.0, hour);
    }
    return 0.0;
}

// Set Star Frag
void setStarFrag() {
    float intensity = getStarIntensity(currentHour);
    if(intensity <= 0.0) discard;

    float finalAlpha = intensity * starTransition;
    if(finalAlpha <= 0.01) discard;

    float pixelSize = mix(0.2, 0.1, starTransition);
    vec2 pUv = floor(vTexCoord / pixelSize) * pixelSize;
    vec2 uv = pUv - 0.5;

    float squareSize = mix(0.1, 0.35, starTransition);
    float dist = max(abs(uv.x), abs(uv.y));
    float square = step(dist, squareSize);
    if(square < 0.5) discard;

    fragColor = vec4(vColor.rgb * intensity * starTransition, finalAlpha);
}