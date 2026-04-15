uniform sampler2D stencilTexture;
uniform vec2 canvasSize;
uniform vec4 outlineColor;
uniform float outlineSize;

void setOutlineFrag() {
    vec2 texelSize = 1.0 / canvasSize;
    vec2 texCoord = gl_FragCoord.xy / canvasSize;
    vec4 stencilValue = texture(stencilTexture, texCoord);
    if(stencilValue.r > 0.0) discard;

    int outInt = int(ceil(outlineSize));
    float o2 = outlineSize * outlineSize;

    for(int y = -outInt; y <= outInt; y++) {
        for(int x = -outInt; x <= outInt; x++) {
            if(x*x + y*y > o2) continue;

            vec2 offset = vec2(x, y) * texelSize;
            vec4 neighbor = texture(stencilTexture, texCoord + offset);
            if(neighbor.r > 0.0) {
                fragColor = outlineColor;
                return;
            }
        }
    }

    discard;
}