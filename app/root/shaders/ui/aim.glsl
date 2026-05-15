/**

    Aim Custom Shader

    */
uniform sampler2D uScreenTexture; 
uniform int isInv;

vec4 setAim(vec4 texColor) {
    if(isInv == 0) return texColor;

    vec2 screenUv = gl_FragCoord.xy / canvasSize;
    vec4 screenColor = texture(uScreenTexture, screenUv);

    vec4 color = vec4(1.0 - screenColor.rgb, texColor.a); 
    return color;
}