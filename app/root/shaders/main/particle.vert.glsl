/**

    Particle vertex shader
    for mesh general particles.

    */
void setParticleVert() {
    vec3 camRight = vec3(
        uView[0][0],
        uView[1][0],
        uView[2][0]
    );
    vec3 camUp = vec3(
        uView[0][1],
        uView[1][1],
        uView[2][1]
    );

    vec3 finalPos = aPos;

    vec3 particleCenter = vec3(
        uModel[3][0],
        uModel[3][1],
        uModel[3][2]
    );

    vec3 scaledVert = finalPos * vec3(
        uModel[0][0],
        uModel[1][1],
        uModel[2][2]
    );

    finalPos = 
        (camRight * scaledVert.x) + 
        (cameraUp * scaledVert.x);
    finalPos += particleCenter;

    vec4 worldPos = vec4(finalPos, 1.0);
    worldPos = worldPos.xyz;

    uColor = aColor;
    texCoord = aTexCoord;

    vec4 uViewPos = uView * worldPos;
    fragDist = length(uViewPos.xyz);
    gl_Position = projection * uViewPos;
}