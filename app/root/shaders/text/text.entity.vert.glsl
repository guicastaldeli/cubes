void setTextEntityVert() {
    vec3 camRight = vec3(uView[0][0], uView[1][0], uView[2][0]);
    vec3 worldUp = vec3(0.0, 1.0, 0.0);

    vec3 worldCenter = vec3(uModel[3][0], uModel[3][1], uModel[3][2]);
    float scaleX = length(vec3(uModel[0][0], uModel[0][1], uModel[0][2]));
    float scaleY = length(vec3(uModel[1][0], uModel[1][1], uModel[1][2]));

    vec3 finalPos = worldCenter +
        camRight * aPos.x * scaleX +
        worldUp * aPos.y * scaleY;

    gl_Position = uProjection * uView * vec4(finalPos, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}