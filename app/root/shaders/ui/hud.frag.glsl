/**

    Hud Frag Shader

    */
void setHudFrag() {
    if(hasTex == 1) {
        vec4 texColor = texture(uSampler, vTexCoord); 
        if(texColor.a < 0.1) discard;

        fragColor = texColor;
    } else {
        fragColor = uColor;
    }
}