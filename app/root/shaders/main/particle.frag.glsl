void setParticleFrag() {
    fragColor = uColor;

    vec2 centeredCoord = texCoord - vec2(0.5);
    float dist = length(centeredCoord);

    float edgeFade = 1.0 - smoothstep(0.3, 0.5, dist);
    fragColor.a *= edgeFade;
}