/**

    Skybox stars vertex shader

    */
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

    float size = aInstanceColor.a * 0.3;

    vec3 finalPos =
        (camRight * aPos.x * size) +
        (camUp * aPos.y * size);
    finalPos += aInstanceOffset;

    vColor = vec4(aInstanceColor.rgb, 1.0);
    vTexCoord = aTexCoord;
    fragDist = 0.0;

    gl_Position = uProjection * uView * vec4(finalPos, 1.0);
}