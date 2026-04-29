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

void setSkyboxFrag() {
    vec3 currentTop = mix(prevTopColor,    topColor,    transitionProgress);
    vec3 currentBottom = mix(prevBottomColor, bottomColor, transitionProgress);

    float height = 100.0;
    float gradient = (vWorldY + height / 2.0) / height;
    gradient = clamp(gradient, 0.0, 1.0);

    fragColor = vec4(mix(currentBottom, currentTop, gradient), 1.0);
}