/**

    Skybox stars vertex shader

    */
uniform float starTransition;

void setStarVert() {
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

    float sizeMultiplier = mix(0.0, 1.0, starTransition);
    float size = aInstanceColor.a * 0.3 * sizeMultiplier;

    float rotationMultiplier = mix(5.0, 1.0, starTransition);
    float rotation = aInstanceRotation.x;
    float cosR = cos(rotation * rotationMultiplier);
    float sinR = sin(rotation * rotationMultiplier);

    vec2 rotatedPos = vec2(
        aPos.x * cosR - aPos.y * sinR,
        aPos.x * sinR + aPos.y * cosR
    );

    vec3 finalPos =
        (camRight * rotatedPos.x * size) +
        (camUp * rotatedPos.y * size);
    finalPos += aInstanceOffset;

    vColor = vec4(aInstanceColor.rgb, 1.0);
    vTexCoord = aTexCoord;
    fragDist = 0.0;

    gl_Position = uProjection * uView * vec4(finalPos, 1.0);
}