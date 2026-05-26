void setTextEntityFrag() {
    vec4 texColor = texture(uSampler, vTexCoord);
    if(texColor.a < 0.1) discard;
    fragColor = vec4(texColor.rgb, texColor.a);
}