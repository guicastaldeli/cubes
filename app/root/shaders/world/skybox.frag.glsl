/**

    Skybox fragment shader

    */
uniform float currentHour;
uniform float time;
uniform float transitionProgress;

uniform vec3 topColor;
uniform vec3 bottomColor;

uniform vec3 prevTopColor;
uniform vec3 prevBottomColor;

uniform int periodType;

uniform float skyboxSize;

void setSkyboxFrag() {
    vec3 currentTop = mix(prevTopColor, topColor, transitionProgress);
    vec3 currentBottom = mix(prevBottomColor, bottomColor, transitionProgress);

    vec3 dir = normalize(vWorldPos);
    float gradient = clamp(dir.y * 0.5 + 0.5, 0.0, 1.0);

    vec4 color = applyWeatherTemp(vec4(mix(currentBottom, currentTop, gradient), 1.0));
    fragColor = color;
}