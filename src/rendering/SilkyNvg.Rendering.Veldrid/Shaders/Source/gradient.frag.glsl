#version 450

layout(set = 0, binding = 1) uniform GradientParams {
    mat4 paintMat;      // 64 bytes
    vec4 innerCol;      // 16 bytes
    vec4 outerCol;      // 16 bytes
    vec2 extent;        //  8 bytes
    float radius;       //  4 bytes
    float feather;      //  4 bytes
};

layout(location = 0) in vec2 frag_WorldPosition;
layout(location = 1) in vec2 frag_TexCoord;

layout(location = 0) out vec4 out_Color;

float sdroundrect(vec2 pt, vec2 ext, float rad) {
    vec2 ext2 = ext - vec2(rad, rad);
    vec2 d = abs(pt) - ext2;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - rad;
}

void main() {
    // Transform world position into paint space
    vec2 pt = (paintMat * vec4(frag_WorldPosition, 1.0, 1.0)).xy;

    // Compute signed distance to rounded rectangle
    float d = clamp((sdroundrect(pt, extent, radius) + feather * 0.5) / feather, 0.0, 1.0);

    // Mix between inner and outer colors, apply AA coverage from frag_TexCoord.y
    vec4 colour = mix(innerCol, outerCol, d);
    out_Color = vec4(colour.rgb, colour.a * frag_TexCoord.y);
}