/**

    Skybox stars frag shader

    */
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

    vec2 uv = vTexCoord - 0.5;
    float dist = length(uv);
    float circle = step(dist, 0.35);
    if(circle < 0.5) discard;

    fragColor = vec4(vColor.rgb * intensity, 1.0);
}