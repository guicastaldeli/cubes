void setTextVert() {
    vec2 normalizedPos = vec2(
        (aPos.x * 2.0) / screenSize.x - 1.0,
        1.0 - (aPos.y * 2.0) / screenSize.y
    );

    gl_Position = vec4(normalizedPos, 0.0, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}