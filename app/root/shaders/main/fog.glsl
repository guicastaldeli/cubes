/**

    Fog main shader.

    */
vec4 applyFog(vec4 color) {
    vec3 camPos = vec3(0.0, cameraY, 0.0);
    vec3 dir = normalize(vWorldPos - camPos);

    float gradient = clamp(dir.y * 0.5 + 0.5, 0.0, 1.0);

    vec3 currentTop = mix(prevTopColor, topColor, transitionProgress);
    vec3 currentBottom = mix(prevBottomColor, bottomColor, transitionProgress);
    vec3 fogColor = mix(currentBottom, currentTop, gradient);

    float end = 50.0;
    float start = 10.0;
    float fogFactor = clamp((end - fragDist) / (end - start), 0.0, 1.0);

    vec4 val = vec4(mix(fogColor, color.rgb, fogFactor), color.a);
    return val;
}