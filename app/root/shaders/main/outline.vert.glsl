void setOutlineVert() {
    vec3 worldPos = aPos + aNormal * 0.05;
    gl_Position = uProjection * uView * uModel * vec4(worldPos, 1.0);
}