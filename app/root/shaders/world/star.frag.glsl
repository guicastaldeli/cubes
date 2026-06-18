/**

    Skybox stars frag shader

    */
uniform float starTransition;

uniform float periodStart;
uniform float periodEnd;

// Set Star Frag
void setStarFrag() {
    float intensity = smoothstep(periodStart, periodEnd, currentHour);

    float alpha = intensity * starTransition;
    if(alpha <= 0.0) discard;

    float pixelSize = mix(0.2, 0.1, starTransition);
    vec2 pUv = floor(vTexCoord / pixelSize) * pixelSize;
    vec2 uv = pUv - 0.5;

    float squareSize = mix(0.1, 0.35, starTransition);
    float dist = max(abs(uv.x), abs(uv.y));
    float square = step(dist, squareSize);
    if(square < 0.5) discard;

    vec3 color = vColor.rgb;
    fragColor = vec4(color, alpha);
}