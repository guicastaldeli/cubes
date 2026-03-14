void setUIFrag() {
    if(hasTex == 1) {
        fragColor = texture(uSampler, vTexCoord);
    } else {
        fragColor = uColor;
    }
}