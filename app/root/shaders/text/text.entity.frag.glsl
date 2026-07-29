void setTextEntityFrag() {
    vec4 texColor = texture(uSampler, vTexCoord);

    float alpha = texColor.a;
    if(alpha < 0.1) discard;

    fragColor = vec4(texColor.rgb, alpha);
}