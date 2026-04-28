/**

    Skybox vertex shader

    */
void setSkyboxVert() {
    vColor = aColor;
    vTexCoord = aTexCoord;
    fragDist = 0.0;
    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
}