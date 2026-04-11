void setOutlineVert() {
    float thickness = 0.05;
    vec3 worldPos = aPos + aNormal * thickness;
    
    gl_Position = uProjection * uView * uModel * vec4(worldPos, 1.0);
}