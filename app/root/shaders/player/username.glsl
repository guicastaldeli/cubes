/*

    Billboard Shaders for Player's Username
    its not a minecraft copy :P ...

    */

void setBillboardVert() {
    vec3 camRight = vec3(uView[0][0], uView[1][0], uView[2][0]);
    vec3 camUp = vec3(uView[0][1], uView[1][1], uView[2][1]);

    vec3 worldPos = vec3(uModel[3][0], uModel[3][1], uModel[3][2]);
    worldPos += camRight * aPos.x + camUp * aPos.y;

    gl_Position = uProjection * uView * vec4(worldPos, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}