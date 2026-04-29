/**

    Skybox vertex shader

    */
out float vWorldY;
out vec3 vWorldPos;

void setSkyboxVert() {
    vColor = aColor;
    vTexCoord = aTexCoord;
    fragDist = 0.0;
    
    vec4 worldPos = uModel * vec4(aPos, 1.0);
    vWorldY = worldPos.y;
    vWorldPos = worldPos.xyz;

    gl_Position = uProjection * uView * worldPos;
}