void setBillboardFrag() {
    float alpha = texture(uSampler, vTexCoord).a;
    fragColor = vec4(uColor.rgb, alpha * uColor.a);
    if(alpha < 0.1) discard;
}