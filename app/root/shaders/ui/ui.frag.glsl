/**

    UI Vertex Shader

    */
void setUIFrag() {
    if(hasTex == 1) {
        vec4 texColor = texture(uSampler, vTexCoord); 
        if(texColor.a < 0.1) discard;

        if(isInv == 1) {
            fragColor = setAim(texColor);
        } else if(isInv == 0) {
            fragColor = texColor;
        }
    } else {
        fragColor = uColor;
    }
}