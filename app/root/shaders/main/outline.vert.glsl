void setOutlineVert() {
    gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
}