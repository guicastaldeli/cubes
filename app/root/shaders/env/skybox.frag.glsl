/**

    Skybox fragment shader

    */
in float vWorldY;
in vec3 vWorldPos;

uniform float currentHour;
uniform float time;
uniform float transitionProgress;

uniform vec3 topColor;
uniform vec3 bottomColor;

uniform vec3 prevTopColor;
uniform vec3 prevBottomColor;

uniform int periodType;

float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(
        mix(random(i + vec2(0.0, 0.0)), random(i + vec2(1.0, 0.0)), u.x),
        mix(random(i + vec2(0.0, 1.0)), random(i + vec2(1.0, 1.0)), u.x), u.y
    );
}

float getStarIntensity(float hour) {
    // MIDNIGHT and NIGHT
    if(periodType == 1 || periodType == 2) {
        return 1.0;
    }
    // DUSK
    if(periodType == 3) {
        return smoothstep(17.0, 19.0, hour);
    }
    // DAWN
    if(periodType == 4) {
        return 1.0 - smoothstep(4.0, 6.0, hour);
    }
    return 0.0;
}

void setSkyboxFrag() {
    vec3 currentTop = mix(prevTopColor, topColor, transitionProgress);
    vec3 currentBottom = mix(prevBottomColor, bottomColor, transitionProgress);

    float height = 100.0;
    float gradient = (vWorldY + height / 2.0) / height;
    gradient = clamp(gradient, 0.0, 1.0);

    vec3 finalColor = mix(currentBottom, currentTop, gradient);

    float starIntensity = getStarIntensity(currentHour);
    if(starIntensity > 0.0) {
        vec3 dir = normalize(vWorldPos);
        
        float theta = atan(dir.z, dir.x);
        float phi = asin(dir.y);
        
        vec2 starCoord = vec2(theta / (2.0 * 3.14159), phi / 3.14159 + 0.5);
        
        float stars = 0.0;
        float star1 = noise(starCoord * 50.0);
        stars += smoothstep(0.97, 1.0, star1) * 1.0;
        
        float star2 = noise(starCoord * 30.0 + 0.5);
        stars += smoothstep(0.95, 1.0, star2) * 0.7;
        
        float star3 = noise(starCoord * 20.0 + 1.0);
        stars += smoothstep(0.93, 1.0, star3) * 0.4;
        
        float twinkle = sin(time * 2.0 + starCoord.x * 100.0) * 0.5 + 0.5;
        twinkle = mix(0.8, 1.0, twinkle);
        
        vec3 starColor = vec3(1.0, 0.95, 0.8) * stars * twinkle * starIntensity;
        float heightFade = smoothstep(-0.2, 0.3, dir.y);
        
        finalColor += starColor * heightFade;
    }

    fragColor = vec4(finalColor, 1.0);
}