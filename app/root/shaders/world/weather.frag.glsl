/**

    Weather frag shader

    */
uniform vec3 weatherTemp;
uniform float weatherStrength;
uniform int shaderAddon;

vec4 applyWeatherTemp(vec4 color) {
    if(shaderAddon == 0 || weatherStrength <= 0.0) return color;
    
    vec3 tinted = mix(color.rgb, weatherTemp, weatherStrength);
    return vec4(tinted, color.a);
}