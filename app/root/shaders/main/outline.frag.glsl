uniform sampler2D stencilTexture;
uniform sampler2D stencilDepthTexture;
uniform sampler2D sceneDepthTexture;

uniform vec2 canvasSize;
uniform vec4 outlineColor;
uniform float outlineSize;

/**

    Outline frag shader
    for mesh outline

    */
void setOutlineFrag() {
    vec2 texelSize = 1.0 / canvasSize;
    vec2 texCoord = gl_FragCoord.xy / canvasSize;

    vec4 stencilValue = texture(stencilTexture, texCoord);
    if(stencilValue.r > 0.0) discard;

    float sceneDepth = texture(sceneDepthTexture, texCoord).r;
    float stencilDepth = texture(stencilDepthTexture, texCoord).r;

    int outInt = int(ceil(outlineSize));
    float o2 = outlineSize * outlineSize;

    for(int y = -outInt; y <= outInt; y++) {
        for(int x = -outInt; x <= outInt; x++) {
            if(x*x + y*y > o2) continue;

            vec2 offset = vec2(x, y) * texelSize;
            vec4 neighbor = texture(stencilTexture, texCoord + offset);
            float neighborDepth = texture(stencilDepthTexture, texCoord + offset).r;

            if(neighbor.r > 0.0 &&
                neighborDepth <= sceneDepth + 0.0001) {
                fragColor = outlineColor;
                return;
            }
        }
    }

    discard;
}