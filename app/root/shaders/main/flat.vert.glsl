/**

    Flag vertex shader
    for mesh outline

    */
void setFlatVert() {
    if(isInstanced == 1) {
        float angle = radians(aInstanceRotation.x);
        float cosA = cos(angle);
        float sinA = sin(angle);

        mat3 instanceRotation = mat3(
            cosA, 0.0, sinA,
            0.0,  1.0, 0.0,
           -sinA, 0.0, cosA
        );

        vec3 pos = instanceRotation * aPos;
        pos += aInstanceOffset;
        gl_Position = uProjection * uView * vec4(pos, 1.0);
    } else {
        gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
    }
}