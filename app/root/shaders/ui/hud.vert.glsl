/**

    Hud Vertex Shader

    */
void setHudVert() {
    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}