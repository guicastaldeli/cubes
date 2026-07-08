/**

    Weather frag shader

    */
uniform vec3 weatherTemp;
uniform float weatherStrength;

vec4 applyWeatherTemp(vec4 color) {
    if(shaderAddon == -1 || weatherStrength <= 0.0) return color;
    
    vec3 tinted = mix(color.rgb, weatherTemp, weatherStrength);
    return vec4(tinted, color.a);
}