#version 450

layout(location = 0) in vec4 frag_Color;
layout(location = 1) in vec2 frag_TexCoord;

layout(location = 0) out vec4 out_Color;

void main() {
    float fillCoverage = min(1.0, frag_TexCoord.y);
    float strokeCoverage = min(1.0, (1.0 - abs(frag_TexCoord.x * 2.0 - 1.0)) * 2.0);
    float aaCoverage = fillCoverage * strokeCoverage;
    out_Color = vec4(frag_Color.rgb, frag_Color.a * aaCoverage);
}