/**

    Skybox fragment shader

    */
uniform vec3 topColor;
uniform vec3 bottomColor; 

void setSkyboxFrag() {
    float gradient = vTexCoord.y;
    vec3 finalColor = mix(bottomColor, topColor, gradient);
    fragColor = vec4(finalColor, 1.0);
}