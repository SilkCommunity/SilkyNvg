#ifdef GL_ES
precision highp float;
#endif

in vec2 pass_texCoord;
in vec2 pass_position;

out vec4 outColour;

uniform mat3 scissorMat;
uniform mat3x4 paintMat;
uniform vec4 innerCol;
uniform vec4 outerCol;
uniform vec2 scissorExt;
uniform vec2 scissorScale;
uniform vec2 extent;
uniform float radius;
uniform float feather;
uniform float strokeMult;
uniform float strokeThr;
// texture goes here!
uniform int type;

// texture

float sdroundrect(vec2 pt, vec2 ext, float rad) {
    vec2 ext2 = ext - vec2(rad, rad);
    vec2 d = abs(pt) - ext2;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
}

float scissorMask(vec2 p) {
    vec2 sc = (abs((scissorMat * vec3(p, 1.0)).xy) - scissorExt);
    sc = vec2(0.5, 0.5) - sc * scissorScale;
    return clamp(sc.x, 0.0, 1.0) * clamp(sc.y, 0.0, 1.0);
}

#ifdef EDGE_AA
float strokeMask() {
    return min(1.0, (1.0 - abs(pass_texCoord.x * 2.0 - 1.0)) * strokeMult) * min(1.0, pass_texCoord.y);
}
#endif

void main(void) {
    vec4 result;
    float scissor = scissorMask(pass_position);
#ifdef EDGE_AA
    float strokeAlpha = strokeMask();
    if (strokeAlpha < strokeThr)
        discard;
#else
    float strokeAlpha = 1.0;
#endif
    if (type == 0) { // gradient
        vec2 pt = (paintMat * vec3(pass_position, 1.0)).xy;
        float d = clamp((sdroundrect(pt, extent, radius) + feather * 0.5) / feather, 0.0, 1.0);
        vec4 colour = mix(innerCol, outerCol, d);
        colour *= strokeAlpha * scissor;
        result = colour;
    } else if (type == 1) { // image
        // TODO
    } else if (type == 2) { // stencil fill
        result = vec4(1, 1, 1, 1);
    } else if (type == 3) { // textured tris
        // TODO
    }

    outColour = result;

}