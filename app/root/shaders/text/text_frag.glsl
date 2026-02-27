void setTextFrag() {
    float alpha = texture(uSampler, vTexCoord).a;
    fragColor = vec4(vColor.rgb, alpha * vColor.a);
    if(alpha < 0.1) discard;
}